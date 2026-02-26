using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
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

        internal static IEnumerable<ParameterRangeViewModel> GetTrailStopParameters(string trailStopName)
        {
            var parameters = new List<ParameterRangeViewModel>();
            var trailStop = StockTrailStopManager.CreateTrailStop(trailStopName);
            if (trailStop != null)
            {
                for (int i = 0; i < trailStop.Parameters.Length; i++)
                {
                    if (trailStop.ParameterTypes[i] == typeof(float) || trailStop.ParameterTypes[i] == typeof(int))
                    {
                        parameters.Add(new ParameterRangeViewModel
                        {
                            Name = trailStop.ParameterNames[i],
                            Attribute = new StockAgentParamAttribute(float.Parse(trailStop.ParameterRanges[i].MinValue.ToString()), float.Parse(trailStop.ParameterRanges[i].MaxValue.ToString()), 1.0f)
                        });
                    }
                }
            }
            return parameters;
        }
    }
}
