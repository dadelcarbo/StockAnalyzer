using System;

namespace UltimateChartist.Indicators
{

    public class IndicatorParameterAttribute : Attribute
    {
        public IndicatorParameterAttribute()
        {
        }
    }
    public class IndicatorDisplayAttribute : Attribute
    {
        public IndicatorDisplayAttribute()
        {
        }
    }

    public class IndicatorParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public IParamRange Range { get; set; }
    }
}