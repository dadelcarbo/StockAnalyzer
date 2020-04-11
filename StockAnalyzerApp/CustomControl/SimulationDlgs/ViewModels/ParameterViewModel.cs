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
        public string Name { get; set; }
        public object Min { get; set; }
        public object Max { get; set; }

        public static IEnumerable<ParameterViewModel> GetParameters(Type type)
        {
            return StockAgentBase.GetParams(type).Select(p => new ParameterViewModel { Name = p.Key.Name, Min = p.Value.Min, Max = p.Value.Max });
        }
    }
}
