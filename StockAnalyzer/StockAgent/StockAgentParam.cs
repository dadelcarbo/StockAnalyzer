using System;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentParam
    {
        public PropertyInfo Property { get; set; }
        public float Value { get; set; }

        public StockAgentParam(PropertyInfo property, float value)
        {
            this.Property = property;
            this.Value = value;
        }
    }
}
