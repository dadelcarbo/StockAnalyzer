using System.Linq;
using System.Net.WebSockets;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.FixedDocumentViewersUI;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_MACD : IndicatorBase
    {
        public StockIndicator_MACD()
        {
            this.Series = new IndicatorLineSignalSeries();
        }
        public override DisplayType DisplayType => DisplayType.NonRanged;

        public override string DisplayName => $"{ShortName}({SlowPeriod},{FastPeriod})";


        private int fastPeriod = 12;
        [IndicatorParameterInt("Fast", 1, 500)]
        public int FastPeriod { get => fastPeriod; set { if (fastPeriod != value) { fastPeriod = value; RaiseParameterChanged(); } } }

        private int slowPeriod = 16;
        [IndicatorParameterInt("Slow", 1, 500)]
        public int SlowPeriod { get => slowPeriod; set { if (slowPeriod != value) { slowPeriod = value; RaiseParameterChanged(); } } }

        private int signalPeriod = 9;
        [IndicatorParameterInt("Signal", 1, 500)]
        public int SignalPeriod { get => signalPeriod; set { if (signalPeriod != value) { signalPeriod = value; RaiseParameterChanged(); } } }

        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorLineSignalValue[stockSerie.Bars.Count];

            double fastAlpha = 2.0 / (FastPeriod + 1.0);
            double slowAlpha = 2.0 / (SlowPeriod + 1.0);
            double signalAlpha = 2.0 / (SignalPeriod + 1.0);
            var firstBar = stockSerie.Bars.First();
            values[0] = new IndicatorLineSignalValue() { Date = firstBar.Date, Value = 0, Signal = 0 };
            double fastEma = firstBar.Close, slowEma = firstBar.Close;

            int i = 1;
            double signal = 0, macd;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                fastEma += fastAlpha * (bar.Close - fastEma);
                slowEma += slowAlpha * (bar.Close - slowEma);
                macd = fastEma - slowEma;
                signal += signalAlpha * (macd - signal);
                values[i++] = new IndicatorLineSignalValue() { Date = bar.Date, Value = macd, Signal = signal };
            }

            this.Series.Values = values;
        }
    }
}
