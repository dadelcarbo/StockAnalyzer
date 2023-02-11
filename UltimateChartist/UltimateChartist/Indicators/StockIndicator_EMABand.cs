using System.Linq;
using System.Net.WebSockets;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.FixedDocumentViewersUI;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMABand : IndicatorBase
    {
        public StockIndicator_EMABand()
        {
            this.Series = new IndicatorBandSeries();
        }
        public override DisplayType DisplayType => DisplayType.Price;

        public override string DisplayName => $"{ShortName}({Period},{Percent})";


        private int period = 20;
        [IndicatorParameterInt("Period", 1, 500)]
        public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }

        private double percent = 0.05;
        [IndicatorParameterDouble("Percent", 0, 1, 0.001, "P2")]
        public double Percent { get => percent; set { if (percent != value) { percent = value; RaiseParameterChanged(); } } }

        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorBandValue[stockSerie.Bars.Count];

            double alpha = 2.0 / (Period + 1.0);
            var firstBar = stockSerie.Bars.First();
            values[0] = new IndicatorBandValue() { Date = firstBar.Date, Up = firstBar.Close, Mid = firstBar.Close, Down = firstBar.Close };
            double ema = firstBar.Close;

            int i = 1;
            double upRatio = 1 + Percent;
            double downRatio = 1 - Percent;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                ema += alpha * (bar.Close - ema);
                values[i++] = new IndicatorBandValue() { Date = bar.Date, Mid = ema, Up = ema * upRatio, Down = ema * downRatio };
            }

            this.Series.Values = values;
        }
    }
}
