using StockAnalyzer.StockAgent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels
{
    public class ParameterViewModel
    {
        StockAgentParamAttribute Attribute { get; set; }
        public string Name { get; set; }
        public float Min { get => Attribute.Min; set => Attribute.Min = value; }
        public float Max { get => Attribute.Max; set => Attribute.Max = value; }

        public static IEnumerable<ParameterViewModel> GetParameters(Type type)
        {
            return StockAgentBase.GetParams(type).Select(p => new ParameterViewModel { Name = p.Key.Name, Attribute = p.Value });
        }
    }
}
