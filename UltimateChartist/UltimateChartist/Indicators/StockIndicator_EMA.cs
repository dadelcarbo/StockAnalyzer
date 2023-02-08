using System.Linq;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMA : MovingAverageBase
    {
        public override DisplayType DisplayType => DisplayType.Price;
        public override void Initialize(StockSerie stockSerie)
        {
            this.Series.Name = this.DisplayName;
            this.Series.Values = new IndicatorLineValue[stockSerie.Bars.Count];

            double alpha = 2.0 / (Period + 1.0);
            var firstBar = stockSerie.Bars.First();
            this.Series.Values[0] = new IndicatorLineValue() { Date = firstBar.Date, Value = firstBar.Close };
            double ema = firstBar.Close;

            int i = 1;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                ema += alpha * (bar.Close - ema);
                this.Series.Values[i++] = new IndicatorLineValue() { Date = bar.Date, Value = ema };
            }
        }
    }
}
