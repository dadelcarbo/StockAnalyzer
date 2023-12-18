using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public class StockScreener_ATRBAND : StockScreenerBase
    {
        public override string Definition => "Detect opportunities in stock higher than ATR BAND";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType" };
        public override object[] ParameterDefaultValues => new object[] { 20, 10, 3.0f, -3.0f, "EMA" };
        static List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
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
            // "TRAILATR(175,60,4,1,EMA)"
            // Calculate ATR Bands
            var period = (int)parameters[0];
            var atrPeriod = (int)parameters[1];
            var emaIndicator = stockSerie.GetIndicator(parameters[4] + "(" + period + ")").Series[0];

            var upDev = (float)parameters[2];
            var downDev = (float)parameters[3];

            var atr = stockSerie.GetIndicator("ATR(" + atrPeriod + ")").Series[0];
            var upperBB = emaIndicator + upDev * atr;

            // Detecting events
            CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            for (int i = period; i < upperBB.Count; i++)
            {
                Match[i] = closeSerie[i] > upperBB[i];
            }
        }

    }
}
