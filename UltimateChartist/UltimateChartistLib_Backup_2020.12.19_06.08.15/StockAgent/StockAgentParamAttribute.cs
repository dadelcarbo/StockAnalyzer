using System;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentParamAttribute : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public StockAgentParamAttribute(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}
