using StockAnalyzer.StockAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels
{
    public class ParameterViewModel
    {
        private StockAgentParamAttribute Attribute { get; set; }
        private PropertyInfo Property { get; set; }
        public string Name => Property.Name;
        public float Min => Attribute.Min;
        public float Max => Attribute.Max;
        public float Value { get; set; }

        public StockAgentParamAttribute GetAttribute() => Attribute;
        public PropertyInfo GetProperty() => Property;

        public static IEnumerable<ParameterViewModel> GetParameters(Type type)
        {
            return StockAgentBase.GetParams(type).Select(p => new ParameterViewModel { Property = p.Key, Attribute = p.Value, Value = p.Value.Min });
        }
    }
}
