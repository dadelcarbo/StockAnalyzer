﻿using System.Linq;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_ATRBand : IndicatorBase
    {
        public StockIndicator_ATRBand()
        {
            this.Series = new IndicatorBandSeries();
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
            var values = new IndicatorBandValue[stockSerie.Bars.Count];
            var atrSerie = stockSerie.Bars.CalculateATR().CalculateEMA(AtrPeriod);

            double alpha = 2.0 / (Period + 1.0);
            var firstBar = stockSerie.Bars.First();
            values[0] = new IndicatorBandValue() { Date = firstBar.Date, Up = firstBar.Close + upWidth * atrSerie[0], Mid = firstBar.Close, Down = firstBar.Close - downWidth * atrSerie[0] };
            double ema = firstBar.Close;

            int i = 1;
            foreach (var bar in stockSerie.Bars.Skip(1))
            {
                ema += alpha * (bar.Close - ema);
                values[i] = new IndicatorBandValue() { Date = bar.Date, Up = ema + upWidth * atrSerie[i], Mid = ema, Down = ema - downWidth * atrSerie[i] };
                i++;
            }

            this.Series.Values = values;
        }
    }
}