using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public class StockScreener_Amsterdam : StockScreenerBase
    {
        public override string Definition => "Detect opportunities for Amsterdam strategy from Investir Zen";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType" };
        public override object[] ParameterDefaultValues => new object[] { 20, 10, 3.0f, -3.0f, "EMA" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var atr = stockSerie.GetIndicator("ATR(50)").Series[0];
            var EMA200 = stockSerie.GetIndicator("EMA(200)").Series[0];
            var EMA50 = stockSerie.GetIndicator("EMA(50)").Series[0];
            var EMA21 = stockSerie.GetIndicator("EMA(21)").Series[0];
            var EMA12 = stockSerie.GetIndicator("EMA(12)").Series[0];

            // Detecting events
            CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = 1; i < stockSerie.Count; i++)
            {
                var max = Math.Max(EMA200[i], Math.Max(EMA50[i], Math.Max(EMA21[i], EMA12[i])));

                Match[i] = closeSerie[i] > max - atr[i];
            }
        }

    }
}
