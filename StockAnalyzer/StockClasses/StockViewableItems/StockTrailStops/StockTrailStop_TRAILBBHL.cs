using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBBHL : StockTrailStopBase
    {
        public StockTrailStop_TRAILBBHL()
        {
        }
        public override string Definition
        {
            get { return "TRAILBBHL(int Period, float NbUpDev, float NbDownDev)"; }
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 2.0f, -2.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 20.0f), new ParamRangeFloat(-20.0f, 0.0f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILBB.S", "TRAILBB.R" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie HighSerie = stockSerie.GetSerie(StockDataType.HIGH);

            IStockIndicator bbIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAIL", ""));
            stockSerie.CalculateBBTrailStop(bbIndicator.Series[1], bbIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
