using System.Linq;
using System.Net.WebSockets;
using System.Windows.Media;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.FixedDocumentViewersUI;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_TrailATR : IndicatorBase
    {
        public StockIndicator_TrailATR()
        {
            this.Series = new IndicatorTrailSeries();
        }
        public override DisplayType DisplayType => DisplayType.Price;

        public override string DisplayName => $"{ShortName}({Period},{AtrPeriod},{UpWidth},{downWidth})";


        private int period = 20;
        [IndicatorParameterInt("Period", 1, 500)]
        public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }

        private int atrPeriod = 20;
        [IndicatorParameterInt("ATR Period", 1, 500)]
        public int AtrPeriod { get => atrPeriod; set { if (atrPeriod != value) { atrPeriod = value; RaiseParameterChanged(); } } }

        private double upWidth = 1;
        [IndicatorParameterDouble("Up Width", 0, 50, 0.1, "{0:F2}")]
        public double UpWidth { get => upWidth; set { if (upWidth != value) { upWidth = value; RaiseParameterChanged(); } } }

        private double downWidth = 1;
        [IndicatorParameterDouble("Down Width", 0, 50, 0.1, "{0:F2}")]
        public double DownWidth { get => downWidth; set { if (downWidth != value) { downWidth = value; RaiseParameterChanged(); } } }

        public override void Initialize(StockSerie stockSerie)
        {
            var values = new IndicatorTrailValue[stockSerie.Bars.Count];
            var atrSerie = stockSerie.Bars.CalculateATR().CalculateEMA(AtrPeriod).Mult(upWidth);
            var emaSerie = stockSerie.CloseValues.CalculateEMA(Period);
            var lowerBand = emaSerie.Sub(atrSerie.Mult(downWidth));
            var upperBand = emaSerie.Add(atrSerie.Mult(upWidth));

            stockSerie.Bars.CalculateBandTrailStop(lowerBand, upperBand, out double[] longStop, out double[] shortStop);

            int i = 0;
            foreach (var bar in stockSerie.Bars)
            {
                values[i] = new IndicatorTrailValue()
                {
                    Date = bar.Date,
                    High = double.IsNaN(longStop[i]) ? float.NaN: bar.Close,
                    Low = double.IsNaN(shortStop[i]) ? float.NaN : bar.Close,
                    Long = longStop[i],
                    Short = shortStop[i],
                    LongReentry = float.NaN,
                    ShortReentry = float.NaN
                };
                i++;
            }

            this.Series.Values = values;
        }
    }
}
