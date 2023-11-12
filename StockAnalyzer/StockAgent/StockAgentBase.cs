using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockAgentBase : IStockAgent
    {
        public StockAgentBase()
        {
            this.TradeSummary = new StockTradeSummary();
        }

        public virtual string DisplayIndicator => string.Empty;

        protected FloatSerie closeSerie;
        protected FloatSerie openSerie;
        protected FloatSerie lowSerie;
        protected FloatSerie highSerie;
        protected FloatSerie atrSerie;
        protected FloatSerie volumeSerie;
        protected FloatSerie volumeEuroSerie;  // Exchanged volume in M€

        public StockTrade Trade { get; set; }
        public StockSerie StockSerie { get; private set; }
        static List<string> agentNames = null;
        public static List<string> GetAgentNames()
        {
            if (agentNames != null)
                return agentNames;

            agentNames = new List<string>();
            foreach (Type t in typeof(IStockAgent).Assembly.GetTypes())
            {
                Type st = t.GetInterface("IStockAgent");
                if (st != null)
                {
                    if (!t.Name.EndsWith("Base"))
                    {
                        agentNames.Add(t.Name.Replace("Agent", ""));
                    }
                }
            }
            agentNames.Sort();
            return agentNames;
        }

        protected float EntryStopValue { get; private set; }
        protected IStockEntryStop EntryStopAgent { get; private set; }

        protected float EntryTargetValue { get; private set; }
        protected IStockEntryTarget EntryTargetAgent { get; private set; }

        public bool Initialize(StockSerie stockSerie, StockBarDuration duration, IStockEntryStop entryStopAgent, IStockEntryTarget entryTargetAgent)
        {
            try
            {
                this.StockSerie = stockSerie;
                stockSerie.ResetIndicatorCache();

                stockSerie.BarDuration = duration;
                closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                openSerie = stockSerie.GetSerie(StockDataType.OPEN);
                lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
                volumeEuroSerie = stockSerie.GetSerie(StockDataType.VOLUME).CalculateEMA(10);
                this.Trade = null;
                this.EntryStopAgent = entryStopAgent;
                this.EntryTargetAgent = entryTargetAgent;

                return Init(stockSerie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Agent: {this.GetType()} Exception: {ex.Message}");
                return false;
            }
        }
        protected abstract bool Init(StockSerie stockSerie);

        public virtual TradeAction Decide(int index)
        {
            if (this.Trade == null)
            {
                if (volumeEuroSerie[index] < 0.5f)
                    return TradeAction.Nothing;
                var action = this.TryToOpenPosition(index);
                if (action == TradeAction.Buy && this.EntryStopAgent != null)
                {
                    this.EntryStopValue = this.EntryStopAgent.GetStop(index);
                }
                if (action == TradeAction.Buy && this.EntryTargetAgent != null)
                {
                    this.EntryTargetValue = this.EntryTargetAgent.GetTarget(index);
                }
                return action;
            }
            else
            {
                if (lowSerie[index] < this.EntryStopValue)
                {
                    this.Trade.Close(index, Math.Min(this.EntryStopValue, this.openSerie[index]), true);
                    this.Trade = null;

                    return TradeAction.Nothing;
                }
                if (highSerie[index] > this.EntryTargetValue)
                {
                    this.Trade.Close(index, Math.Max(this.EntryTargetValue, this.openSerie[index]), true);
                    this.Trade = null;

                    return TradeAction.Nothing;
                }
                return this.TryToClosePosition(index);
            }
        }

        public bool CanOpen(int index)
        {
            return TryToOpenPosition(index) == TradeAction.Buy;
        }
        public bool CanClose(int index)
        {
            return TryToClosePosition(index) == TradeAction.Sell;
        }

        public void OpenTrade(StockSerie serie, int entryIndex, int qty = 1, bool isLong = true)
        {
            if (entryIndex >= serie.Count) return;
            if (openSerie[entryIndex] * 0.99 <= this.EntryStopValue) // Do not buy if lower than stop level or not more than 1% upper the stop)
                return;

            this.Trade = new StockTrade(serie, entryIndex, qty, this.EntryStopValue, isLong);
            this.TradeSummary.Trades.Add(this.Trade);
        }

        public void CloseTrade(int exitIndex)
        {
            if (this.Trade == null)
                throw new InvalidOperationException("Cannot close the trade as it's not opened");
            this.Trade.CloseAtOpen(exitIndex);
            this.Trade = null;
        }

        protected abstract TradeAction TryToClosePosition(int index);

        protected abstract TradeAction TryToOpenPosition(int index);
        public void EvaluateOpenedPositions()
        {
            foreach (var trade in this.TradeSummary.Trades.Where(t => !t.IsClosed))
            {
                trade.Evaluate();
            }
        }

        public abstract string Description { get; }

        public StockTradeSummary TradeSummary { get; }

        static Random rnd = new Random();
        public void Randomize()
        {
            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                float newValue = (float)rnd.NextDouble() * (param.Value.Max - param.Value.Min) + param.Value.Min;
                if (param.Key.PropertyType == typeof(int))
                {
                    param.Key.SetValue(this, (int)Math.Round(newValue), null);
                }
                else if (param.Key.PropertyType == typeof(float))
                {
                    param.Key.SetValue(this, newValue, null);
                }
                else
                {
                    throw new NotSupportedException("Type " + param.Key.PropertyType + " is not supported as a parameter in Agent");
                }
            }
        }

        public IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren)
        {
            List<IStockAgent> children = new List<IStockAgent>();

            var parameters = StockAgentBase.GetParams(this.GetType());
            for (int i = 0; i < nbChildren; i++)
            {
                IStockAgent agent = (IStockAgent)Activator.CreateInstance(this.GetType());
                foreach (var param in parameters)
                {
                    var property = param.Key;

                    if (param.Key.PropertyType == typeof(int))
                    {
                        int val1 = (int)property.GetValue(this, null);
                        int val2 = (int)property.GetValue(partner, null);

                        float mean = (val1 + val2) / 2.0f;
                        float stdev = Math.Abs(val1 - val2) / 4.0f;

                        float newValue = FloatRandom.NextGaussian(mean, stdev);
                        newValue = Math.Min(newValue, param.Value.Max);
                        newValue = Math.Max(newValue, param.Value.Min);
                        param.Key.SetValue(agent, (int)Math.Round(newValue), null);
                    }
                    else if (param.Key.PropertyType == typeof(float))
                    {
                        float val1 = (float)property.GetValue(this, null);
                        float val2 = (float)property.GetValue(partner, null);

                        float mean = (val1 + val2) / 2.0f;
                        float stdev = Math.Abs(val1 - val2) / 4.0f;

                        float newValue = FloatRandom.NextGaussian(mean, stdev);
                        newValue = Math.Min(newValue, param.Value.Max);
                        newValue = Math.Max(newValue, param.Value.Min);
                        param.Key.SetValue(agent, newValue, null);
                    }
                    else
                    {
                        throw new NotSupportedException("Type " + param.Key.PropertyType + " is not supported as a parameter in Agent");
                    }
                }
                if (!this.AreSameParams(agent))
                {
                    children.Add(agent);
                }
            }
            return children;
        }

        static public IStockAgent CreateInstance(string shortName)
        {
            Type type = typeof(IStockAgent).Assembly.GetType($"StockAnalyzer.StockAgent.Agents.{shortName}Agent");
            return (IStockAgent)Activator.CreateInstance(type);
        }

        public override string ToString()
        {
            string res = string.Empty;

            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + ' ';
            }
            return res;
        }
        public string ToParamValueString()
        {
            var parameters = StockAgentBase.GetParams(this.GetType());
            return parameters.Select(p => p.Key.GetValue(this, null).ToString()).Aggregate((i, j) => i + "\t" + j);
        }
        public string ToLog()
        {
            string res = this.GetType().Name + Environment.NewLine;

            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + Environment.NewLine;
            }
            return res;
        }

        #region PARAMETER MANAGEMENT
        static private Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>> parameters = new Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>>();
        static public Dictionary<PropertyInfo, StockAgentParamAttribute> GetParams(Type type)
        {
            if (!parameters.ContainsKey(type))
            {
                Dictionary<PropertyInfo, StockAgentParamAttribute> _dict = new Dictionary<PropertyInfo, StockAgentParamAttribute>();

                PropertyInfo[] props = type.GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    var attribute = prop.GetCustomAttributes(typeof(StockAgentParamAttribute), true).FirstOrDefault();
                    if (attribute != null)
                    {
                        _dict.Add(prop, attribute as StockAgentParamAttribute);
                    }
                }
                parameters.Add(type, _dict);
            }
            return parameters[type];
        }
        static public Dictionary<PropertyInfo, List<object>> GetParamRanges(Type type)
        {
            var res = new Dictionary<PropertyInfo, List<object>>();
            var parameters = GetParams(type);
            foreach (var param in parameters)
            {
                if (param.Key.PropertyType == typeof(int))
                {
                    var values = new List<object>();
                    for (int i = (int)param.Value.Min; i <= (int)param.Value.Max; i += (int)param.Value.Step)
                    {
                        values.Add(i);
                    }
                    if ((int)values.Last() != (int)param.Value.Max)
                    {
                        values.Add((int)param.Value.Max);
                    }
                    res.Add(param.Key, values);
                }
                else
                if (param.Key.PropertyType == typeof(float))
                {
                    float min = (float)param.Value.Min;
                    float max = (float)param.Value.Max;
                    if (min == max)
                    {
                        res.Add(param.Key, new List<object>() { min });
                    }
                    else
                    {
                        var values = new List<object>();
                        for (float val = min; val <= max; val += param.Value.Step)
                        {
                            values.Add(val);
                        }
                        if ((float)values.Last() != (float)param.Value.Max)
                        {
                            values.Add((float)param.Value.Max);
                        }
                        res.Add(param.Key, values);
                    }
                }
                else
                {
                    throw new NotSupportedException("Type " + param.Key.PropertyType + " is not supported as a parameter in Agent");
                }
            }
            return res;
        }

        public string GetParameterValues()
        {
            string res = string.Empty;

            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + "\t";
            }
            return res;
        }
        public void SetParams(IEnumerable<StockAgentParam> paramList)
        {
            foreach (var param in paramList)
            {
                if (param.Property.PropertyType == typeof(int))
                {
                    param.Property.SetValue(this, (int)Math.Round(param.Value), null);
                }
                else if (param.Property.PropertyType == typeof(float))
                {
                    param.Property.SetValue(this, param.Value, null);
                }
                else
                {
                    throw new NotSupportedException("Type " + param.Property.PropertyType + " is not supported as a parameter in Agent");
                }
            }
        }

        public bool AreSameParams(IStockAgent other)
        {
            if (other.GetType() != this.GetType())
            {
                throw new InvalidOperationException("Can only compare params of same type");
            }

            foreach (var param in StockAgentBase.GetParams(this.GetType()))
            {
                var val1 = param.Key.GetValue(this, null);
                var val2 = param.Key.GetValue(other, null);
                if (val1.ToString() != val2.ToString()) return false;
            }

            return true;
        }
        #endregion
    }
}
