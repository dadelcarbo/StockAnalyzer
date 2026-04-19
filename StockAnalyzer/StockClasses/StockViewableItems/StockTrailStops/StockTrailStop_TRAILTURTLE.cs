using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTURTLE : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "TrailStop based on steroïd turtles as defined by InvestingZen.";

        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 75, 35, 12 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };


        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var indicator = stockSerie.GetIndicator(this.Name.Replace("TRAIL", ""));
            var lowLine = indicator.Series[1];
            var emaSerie = indicator.Series[2];

            bool bull = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (bull)
                {
                    if (lowLine[i - 1] <= emaSerie[i])
                    {
                        longStopSerie[i] = lowLine[i];
                    }
                    else
                    {
                        bull = false;
                    }
                }
                else
                {
                    if (emaSerie[i] > lowLine[i])// BrokenUp
                    {
                        bull = true;
                        longStopSerie[i] = emaSerie[i];
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
