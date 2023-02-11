using System;

namespace UltimateChartist.Indicators
{
    public abstract class IndicatorParameterAttribute : Attribute, IIndicatorParameter
    {
        public IndicatorParameterAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public abstract Type Type { get; }
    }

    public class IndicatorParameterIntAttribute : IndicatorParameterAttribute
    {
        public override Type Type => typeof(int);

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
        public override Type Type => typeof(double);
        public IndicatorParameterDoubleAttribute(string name, double min, double max, double step, string format) : base(name)
        {
            Name = name;
            Min = min;
            Max = max;
            Step = step;
            Format = format;
        }
        public double Min { get; }
        public double Max { get; }
        public double Step { get; }
        public string Format { get; }
    }

    public interface IIndicatorParameter
    {
        public string Name { get; }
        public Type Type { get; }
    }
}