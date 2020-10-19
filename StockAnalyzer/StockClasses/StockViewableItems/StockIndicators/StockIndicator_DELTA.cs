using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DELTA : StockIndicatorBase
    {
        public StockIndicator_DELTA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "DELTA(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }


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
            
           // IStockTrailStop trailStop = stockSerie.GetTrailStop("TRAILEMA(" + period + "," + smoothing + ")");
            FloatSerie slowSerie = stockSerie.GetIndicator("EMA(" + period + ")").Series[0].CalculateDerivative().CalculateEMA(1) * 100f;
            FloatSerie fastSerie = stockSerie.GetIndicator("EMA(" + smoothing + ")").Series[0].CalculateDerivative().CalculateEMA(1) * 100f;
            FloatSerie deltaSerie = fastSerie+slowSerie;
            
            //for (int i = period; i < stockSerie.Count; i++)
            //{
            //    float refVar = (refSerie[i] - refSerie[i - period]) / refSerie[i - period];
            //    float closeVar = (currentSerie[i] - currentSerie[i - period]) / currentSerie[i - period];

            //    betaSerie[i] = closeVar / refVar;
            //}

            this.series[0] = deltaSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (deltaSerie[i - 2] < deltaSerie[i - 1] && deltaSerie[i - 1] > deltaSerie[i]);
                this.eventSeries[1][i] = (deltaSerie[i - 2] > deltaSerie[i - 1] && deltaSerie[i - 1] < deltaSerie[i]);
                this.eventSeries[2][i] = (deltaSerie[i - 1] < 0 && deltaSerie[i] >= 0);
                this.eventSeries[3][i] = (deltaSerie[i - 1] > 0 && deltaSerie[i] <= 0);
            }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
