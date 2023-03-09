using System;

namespace UltimateChartist.Indicators;

public abstract class IndicatorParameterAttribute : Attribute, IIndicatorParameter
{
    public IndicatorParameterAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
    public abstract Type Type { get; }
}

public class IndicatorParameterBoolAttribute : IndicatorParameterAttribute
{
    public override Type Type => typeof(bool);

    public IndicatorParameterBoolAttribute(string name) : base(name)
    {
    }
}

public class IndicatorParameterIntAttribute : IndicatorParameterAttribute
{
    public override Type Type => typeof(int);

    public IndicatorParameterIntAttribute(string name, int min, int max) : base(name)
    {
        Min = min;
        Max = max;
    }
    public int Min { get; }
    public int Max { get; }
}

public class IndicatorParameterDecimalAttribute : IndicatorParameterAttribute
{
    public override Type Type => typeof(decimal);
    public IndicatorParameterDecimalAttribute(string name, double min, double max, double step, string format) : base(name)
    {
        Min = (decimal)min;
        Max = (decimal)max;
        Step = (decimal)step;
        Format = format;
    }
    public decimal Min { get; }
    public decimal Max { get; }
    public decimal Step { get; }
    public string Format { get; }
}

public interface IIndicatorParameter
{
    public string Name { get; }
    public Type Type { get; }
}