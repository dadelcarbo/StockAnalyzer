using System;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentParamAttribute : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Step { get; set; }

        public StockAgentParamAttribute(float min, float max, float step)
        {
            this.Min = min;
            this.Max = max;
            this.Step = step;
        }
    }
}
