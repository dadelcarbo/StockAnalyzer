
using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILVAR : StockTrailStopBase
    {
        public StockTrailStop_TRAILVAR()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Indicator", "IndicatorMax", "IndicatorCenter", "IndicatorWeight" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 2, "ER(20_1_1)", 1f, 0f, 1f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeIndicator(), new ParamRangeFloat(0f, float.MaxValue), new ParamRangeFloat(0f, float.MaxValue), new ParamRangeFloat() }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILVAR.LS", "TRAILVAR.SS" }; } }

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

            float indicatorMax = (float)this.Parameters[2];
            float indicatorCenter = (float)this.Parameters[3];
            float indicatorWeight = (float)this.Parameters[4];


            stockSerie.CalculateVarTrailStop((int)this.Parameters[0], ((string)this.Parameters[1]).Replace('_', ','), indicatorMax, indicatorCenter, indicatorWeight, out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}