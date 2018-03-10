using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHLS : StockTrailStopBase
    {
        public StockTrailStop_TRAILHLS()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 2, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILHLS.LS", "TRAILHLS.SS" }; } }

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
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            stockSerie.CalculateHighLowSmoothedTrailStop((int)this.Parameters[0], (int)this.Parameters[1], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}