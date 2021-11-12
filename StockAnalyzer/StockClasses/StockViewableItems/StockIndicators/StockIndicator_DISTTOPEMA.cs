using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DISTTOPEMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "DISTTRAILEMA(" + this.Parameters[0].ToString() + ")" }; } }

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

            var topEMA = stockSerie.GetIndicator("TOPEMA(" + period + ")");
            FloatSerie longStop = topEMA.Series[0];
            FloatSerie shortStop = topEMA.Series[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            FloatSerie distSerie = new FloatSerie(stockSerie.Count);
            for (int i = period; i < stockSerie.Count; i++)
            {
                distSerie[i] = float.IsNaN(shortStop[i]) ? (closeSerie[i] - longStop[i]) / closeSerie[i] : (closeSerie[i] - shortStop[i]) / closeSerie[i];
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
