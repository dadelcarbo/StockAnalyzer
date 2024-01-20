using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockEntryStopBase : IStockEntryStop
    {
        public StockSerie StockSerie { get; private set; }
        public StockBarDuration Duration { get; private set; }


        static List<string> entryStopNames = null;
        public static List<string> GetEntryStopNames()
        {
            if (entryStopNames != null)
                return entryStopNames;

            entryStopNames = new List<string>();
            foreach (Type t in typeof(IStockEntryStop).Assembly.GetTypes())
            {
                Type st = t.GetInterface("IStockEntryStop");
                if (st != null)
                {
                    if (!t.Name.EndsWith("Base"))
                    {
                        entryStopNames.Add(t.Name.Replace("StockEntryStop_", ""));
                    }
                }
            }
            entryStopNames.Sort();
            return entryStopNames;
        }

        public bool Initialize(StockSerie stockSerie, StockBarDuration duration)
        {
            try
            {
                this.StockSerie = stockSerie;
                this.Duration = duration;

                return Init(stockSerie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EntryStop: {this.GetType()} Exception: {ex.Message}");
                return false;
            }
        }
        protected abstract bool Init(StockSerie stockSerie);

        public abstract string Description { get; }

        public abstract float GetStop(int index);


        static public IStockEntryStop CreateInstance(string shortName)
        {
            Type type = GetType(shortName);
            if (type == null)
                return null;
            return (IStockEntryStop)Activator.CreateInstance(type);
        }

        static public Type GetType(string shortName)
        {
            try
            {
                return typeof(IStockEntryStop).Assembly.GetType($"StockAnalyzer.StockAgent.EntryStops.StockEntryStop_{shortName}");
            }
            catch
            {
                return null;
            }
        }


        public override string ToString()
        {
            string res = string.Empty;

            var parameters = StockEntryStopBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + ' ';
            }
            return res;
        }
        public string ToParamValueString()
        {
            var parameters = StockEntryStopBase.GetParams(this.GetType());
            return parameters.Select(p => p.Key.GetValue(this, null).ToString()).Aggregate((i, j) => i + "\t" + j);
        }
        public string ToLog()
        {
            string res = this.GetType().Name + Environment.NewLine;

            var parameters = StockEntryStopBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + Environment.NewLine;
            }
            return res;
        }

        #region PARAMETER MANAGEMENT
        private static readonly Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>> parameters = new Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>>();
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
                    throw new NotSupportedException("Type " + param.Key.PropertyType + " is not supported as a parameter in EntryStop");
                }
            }
            return res;
        }

        public string GetParameterValues()
        {
            string res = string.Empty;

            var parameters = StockEntryStopBase.GetParams(this.GetType());
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
                    throw new NotSupportedException("Type " + param.Property.PropertyType + " is not supported as a parameter in EntryStop");
                }
            }
        }

        public bool AreSameParams(IStockAgent other)
        {
            if (other.GetType() != this.GetType())
            {
                throw new InvalidOperationException("Can only compare params of same type");
            }

            foreach (var param in StockEntryStopBase.GetParams(this.GetType()))
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
