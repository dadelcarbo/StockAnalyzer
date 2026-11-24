using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBBSIGNAL : StockTrailStopBase
    {
        override public string Definition => "Calculate Trail Stop based on Bollinger Bands.\r\n Starts when signal line crosses above upper line (Fast).\n\r Stops when signal line crosses below lower band";
        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev", "MAType", "Signal" };


        public override Object[] ParameterDefaultValues => new Object[] { 35, 0.5f, -0.5f, "EMA", "3" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA(),
                new ParamRangeInt(1, 500),
                };

        public override void ApplyTo(DataSerie dataSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            IStockIndicator bbIndicator = dataSerie.GetIndicator($"BB({this.Parameters[0]},{this.Parameters[1]},{this.Parameters[2]},{this.Parameters[3]},{this.Parameters[4]})");
            var upperBand = bbIndicator.Series[0];
            var lowerBand = bbIndicator.Series[1];
            var signal = bbIndicator.Series[3];

            longStopSerie = new FloatSerie(dataSerie.Count, "TRAIL.S", float.NaN);
            shortStopSerie = new FloatSerie(dataSerie.Count, "TRAIL.R", float.NaN);
            bool upTrend = false;

            for (int i = 1; i < dataSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (signal[i] < lowerBand[i])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                    }
                    else
                    {
                        // UpTrend still in place
                        longStopSerie[i] = lowerBand[i];
                    }
                }
                else
                {
                    if (signal[i] > upperBand[i])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = lowerBand[i];
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(dataSerie, longStopSerie, shortStopSerie);
        }

    }
}
