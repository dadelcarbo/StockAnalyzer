using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockAgentBase : IStockAgent
    {
        public StockAgentBase()
        {
            this.TradeSummary = new StockTradeSummary();
        }
        protected FloatSerie closeSerie;

        public StockTrade Trade { get; set; }

        public static List<string> GetAgentNames()
        {
            var agentList = new List<string>();
            foreach (Type t in typeof(IStockAgent).Assembly.GetTypes())
            {
                Type st = t.GetInterface("IStockAgent");
                if (st != null)
                {
                    if (!t.Name.EndsWith("Base"))
                    {
                        agentList.Add(t.Name.Replace("Agent", ""));
                    }
                }
            }
            agentList.Sort();
            return agentList;
        }

        public void Initialize(StockSerie stockSerie, StockBarDuration duration)
        {
            stockSerie.ResetIndicatorCache();

            stockSerie.BarDuration = duration;
            closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            this.Trade = null;

            Init(stockSerie);
        }
        protected abstract void Init(StockSerie stockSerie);

        public virtual TradeAction Decide(int index)
        {
            if (this.Trade == null)
            {
                return this.TryToOpenPosition(index);
            }
            else
            {
                return this.TryToClosePosition(index);
            }
        }

        public void OpenTrade(StockSerie serie, int entryIndex, bool isLong = true)
        {
            if (entryIndex >= serie.Count) return;

            this.Trade = new StockTrade(serie, entryIndex, isLong);
            this.TradeSummary.Trades.Add(this.Trade);
        }

        public void CloseTrade(int exitIndex)
        {
            if (this.Trade == null)
                throw new InvalidOperationException("Cannot close the trade as it's not opened");
            this.Trade.CloseAtOpen(exitIndex);
            this.Trade = null;
        }
        public void PartlyCloseTrade(int exitIndex)
        {
            if (this.Trade == null)
                throw new InvalidOperationException("Cannot close the trade as it's not opened");
            if (this.Trade.IsPartlyClosed)
                throw new InvalidOperationException("Cannot partly close the trade as it's already partly closed opened");

            this.Trade.PartialClose(exitIndex);
        }

        protected abstract TradeAction TryToClosePosition(int index);

        protected abstract TradeAction TryToOpenPosition(int index);
        public void EvaluateOpenedPositions()
        {
            foreach (var trade in this.TradeSummary.Trades.Where(t=>!t.IsClosed))
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
                IStockAgent agent = StockAgentBase.CreateInstance(this.GetType());
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

        static public IStockAgent CreateInstance(Type agentType)
        {
            return (IStockAgent)Activator.CreateInstance(agentType);
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
        static public Dictionary<PropertyInfo, List<object>> GetParamRanges(Type type, int nbVal)
        {
            var res = new Dictionary<PropertyInfo, List<object>>();
            var parameters = GetParams(type);
            foreach (var param in parameters)
            {
                if (param.Key.PropertyType == typeof(int))
                {
                    var values = new List<object>();
                    for (int i = (int)param.Value.Min; i <= (int)param.Value.Max; i++)
                    {
                        values.Add(i);
                    }
                    res.Add(param.Key, values);
                }
                else
                if (param.Key.PropertyType == typeof(float))
                {
                    float val = (float)param.Value.Min;
                    float step = ((float)param.Value.Max - val) / (float)nbVal;
                    var values = new List<object>();
                    for (int i = 0; i <= nbVal; i++)
                    {
                        values.Add(val);
                        val += step;
                    }
                    res.Add(param.Key, values);
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
        public void SetParam(PropertyInfo property, StockAgentParamAttribute attribute, float newValue)
        {
            if (property.PropertyType == typeof(int))
            {
                property.SetValue(this, (int)Math.Round(newValue), null);
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(this, newValue, null);
            }
            else
            {
                throw new NotSupportedException("Type " + property.PropertyType + " is not supported as a parameter in Agent");
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
