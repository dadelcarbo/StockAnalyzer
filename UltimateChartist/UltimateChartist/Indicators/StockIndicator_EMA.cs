using System.Linq;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMA : MovingAverageBase
    {
        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorLineValue[stockSerie.Bars.Count];

            double alpha = 2.0 / (Period + 1.0);
            var firstBar = stockSerie.Bars.First();
            values[0] = new IndicatorLineValue() { Date = firstBar.Date, Value = firstBar.Close };
            double ema = firstBar.Close;

            int i = 1;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                ema += alpha * (bar.Close - ema);
                values[i++] = new IndicatorLineValue() { Date = bar.Date, Value = ema };
            }

            this.Series.Values = values;
        }
    }
}
