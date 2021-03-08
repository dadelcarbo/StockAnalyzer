using System;
using System.Collections.Generic;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBBTF : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 2.0f, -2.0f, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA", "MID" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeMA()
                };
            }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILBBTF.S", "TRAILBBTF.R" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);

            IStockIndicator bbIndicator = stockSerie.GetIndicator($"BB({parameters[0]},{parameters[1]},{parameters[2]},{parameters[3]})");
            var upBand = bbIndicator.Series[0];
            var downBand = bbIndicator.Series[1];

            bool upTrend = false;
            for (int i = (int)this.parameters[0]; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < downBand[i])
                    {
                        upTrend = false;
                        shortStopSerie[i] = upBand[i];
                    }
                    else
                    {
                        longStopSerie[i] = downBand[i];
                    }
                }
                else
                {

                    if (closeSerie[i] > upBand[i])
                    {
                        upTrend = true;
                        longStopSerie[i] = downBand[i];
                    }
                    else
                    {
                        shortStopSerie[i] = upBand[i];
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
