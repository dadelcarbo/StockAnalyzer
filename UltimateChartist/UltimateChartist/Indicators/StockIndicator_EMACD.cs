using System.Linq;
using System.Net.WebSockets;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.FixedDocumentViewersUI;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMACD : IndicatorBase
    {
        public StockIndicator_EMACD()
        {
            this.Series = new IndicatorLineSeries();
        }
        public override DisplayType DisplayType => DisplayType.NonRanged;

        public override string DisplayName => $"{ShortName}({SlowPeriod},{FastPeriod})";


        private int fastPeriod = 36;
        [IndicatorParameterInt("Period", 1, 500)]
        public int FastPeriod { get => fastPeriod; set { if (fastPeriod != value) { fastPeriod = value; RaiseParameterChanged(); } } }

        private int slowPeriod = 12;
        [IndicatorParameterInt("Period", 1, 500)]
        public int SlowPeriod { get => slowPeriod; set { if (slowPeriod != value) { slowPeriod = value; RaiseParameterChanged(); } } }

        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorLineValue[stockSerie.Bars.Count];

            double fastAlpha = 2.0 / (FastPeriod + 1.0);
            double slowAlpha = 2.0 / (SlowPeriod + 1.0);
            var firstBar = stockSerie.Bars.First();
            values[0] = new IndicatorLineValue() { Date = firstBar.Date, Value = 0 };
            double fastEma = firstBar.Close, slowEma = firstBar.Close;

            int i = 1;
            double upRatio = 1 + SlowPeriod;
            double downRatio = 1 - SlowPeriod;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                fastEma += fastAlpha * (bar.Close - fastEma);
                slowEma += slowAlpha * (bar.Close - slowEma);
                values[i++] = new IndicatorLineValue() { Date = bar.Date, Value = fastEma - slowEma };
            }

            this.Series.Values = values;
        }
    }
}
