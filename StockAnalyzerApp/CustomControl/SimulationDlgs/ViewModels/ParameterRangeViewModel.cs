using StockAnalyzer.StockAgent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels
{
    public class ParameterRangeViewModel
    {
        StockAgentParamAttribute Attribute { get; set; }
        public string Name { get; set; }
        public float Min { get => Attribute.Min; set => Attribute.Min = value; }
        public float Max { get => Attribute.Max; set => Attribute.Max = value; }
        public float Step { get => Attribute.Step; set => Attribute.Step = value; }

        public static IEnumerable<ParameterRangeViewModel> GetParameters(Type type)
        {
            return StockAgentBase.GetParams(type).Select(p => new ParameterRangeViewModel { Name = p.Key.Name, Attribute = p.Value });
        }
    }
}
