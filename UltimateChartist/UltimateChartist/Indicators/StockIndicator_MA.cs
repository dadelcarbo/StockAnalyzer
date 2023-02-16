using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.FieldList;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_MA : MovingAverageBase
    {
        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorLineValue[stockSerie.Bars.Count];
            var closeValues = stockSerie.CloseValues;
            var maValues = closeValues.CalculateMA(Period);

            int i = 0;
            foreach (var bar in stockSerie.Bars)
            {
                values[i] = new IndicatorLineValue()
                {
                    Date = bar.Date,
                    Value = maValues[i]
                };
                i++;
            }

            this.Series.Values = values;
        }
    }
}
