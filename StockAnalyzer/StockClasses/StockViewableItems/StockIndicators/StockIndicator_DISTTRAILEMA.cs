using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DISTTRAILEMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smooting" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 6 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "DISTTRAILEMA(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }

        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            int smoothing = (int)this.parameters[1];

            IStockTrailStop trail = stockSerie.GetTrailStop("TRAILEMA(" + period + "," + smoothing + ")");
            FloatSerie longStop = trail.Series[0];
            FloatSerie shortStop = trail.Series[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(smoothing);

            FloatSerie distSerie = new FloatSerie(stockSerie.Count);
            for (int i = period + smoothing; i < stockSerie.Count; i++)
            {
                distSerie[i] = float.IsNaN(longStop[i]) ? (closeSerie[i] - shortStop[i]) / closeSerie[i] : (closeSerie[i] - longStop[i]) / closeSerie[i];
            }

            this.series[0] = distSerie;
            this.series[0].Name = this.Name;
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }

        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
