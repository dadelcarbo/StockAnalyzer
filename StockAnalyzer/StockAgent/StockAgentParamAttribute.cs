using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentParamAttribute : Attribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }
        public float Range { get { return Max / Min; } }

        public StockAgentParamAttribute(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}
