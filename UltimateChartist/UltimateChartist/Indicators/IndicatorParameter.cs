using System;

namespace UltimateChartist.Indicators
{
    public abstract class IndicatorParameterAttribute : Attribute
    {
        public IndicatorParameterAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }

    public class IndicatorParameterIntAttribute : IndicatorParameterAttribute
    {
        public IndicatorParameterIntAttribute(string name, int min, int max) : base(name)
        {
            Name = name;
            Min = min;
            Max = max;
        }
        public int Min { get; }
        public int Max { get; }
    }

    public class IndicatorParameterDoubleAttribute : IndicatorParameterAttribute
    {
        public IndicatorParameterDoubleAttribute(string name, double min, double max, double step) : base(name)
        {
            Name = name;
            Min = min;
            Max = max;
            Step = step;
        }
        public double Min { get; }
        public double Max { get; }
        public double Step { get; }
    }

    public class IndicatorParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public IParamRange Range { get; set; }
    }
}