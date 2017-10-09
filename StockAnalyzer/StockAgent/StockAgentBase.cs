using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockAgentBase : IStockAgent
    {
        protected StockContext context;

        protected FloatSerie closeSerie;

        protected StockAgentBase(StockContext context)
        {
            this.context = context;
            this.context.OnSerieChanged += context_OnSerieChanged;
            context_OnSerieChanged();
        }

        void context_OnSerieChanged()
        {
            if (this.context.Serie != null)
            {
                this.closeSerie = context.Serie.GetSerie(StockDataType.CLOSE);
            }
        }

        public TradeAction Decide()
        {
            if (context.Trade == null)
            {
                return this.TryToOpenPosition();
            }
            else
            {
                return this.TryToClosePosition();
            }
        }

        protected abstract TradeAction TryToClosePosition();

        protected abstract TradeAction TryToOpenPosition();

        static private Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>> parameters = new Dictionary<Type, Dictionary<PropertyInfo, StockAgentParamAttribute>>();
        static private Dictionary<PropertyInfo, StockAgentParamAttribute> GetParams(Type type)
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

        static Random rnd = new Random();
        public void Randomize()
        {
            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                float newValue = (float)rnd.NextDouble() * (param.Value.Max - param.Value.Min) + param.Value.Min;
                param.Key.SetValue(this, newValue, null);
            }
        }

        public IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren)
        {
            List<IStockAgent> children = new List<IStockAgent>();

            var parameters = StockAgentBase.GetParams(this.GetType());
            for (int i = 0; i < nbChildren; i++)
            {
                IStockAgent agent = this.CreateInstance(context);
                foreach (var param in parameters)
                {
                    var property = param.Key;

                    float val1 = (float)property.GetValue(this, null);
                    float val2 = (float)property.GetValue(partner, null);

                    float mean = (val1 + val2) / 2.0f;
                    float stdev = Math.Abs(val1 - val2) / 4.0f;

                    float newValue = FloatRandom.NextGaussian(mean, stdev);
                    newValue = Math.Min(newValue, param.Value.Max);
                    newValue = Math.Max(newValue, param.Value.Min);
                    param.Key.SetValue(agent, newValue, null);
                }
                children.Add(agent);
            }
            return children;
        }

        protected abstract IStockAgent CreateInstance(StockContext context);

        public string ToLog()
        {
            string res = string.Empty;

            var parameters = StockAgentBase.GetParams(this.GetType());
            foreach (var param in parameters)
            {
                res += param.Key.Name + ": " + param.Key.GetValue(this, null) + Environment.NewLine;
            }
            return res;
        }
    }
}
