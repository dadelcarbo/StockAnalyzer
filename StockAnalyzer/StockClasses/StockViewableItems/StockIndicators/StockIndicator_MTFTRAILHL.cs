using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MTFTRAILHL : StockIndicatorBase, IRange
    {
        public StockIndicator_MTFTRAILHL()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 10.2f; }
        }

        public float Min
        {
            get { return -10.2f; }
        }

        public override string Name
        {
            get { return "MTFTRAILHL(" + this.parameters[0] + ")"; }
        }
        public override string Definition
        {
            get { return "MTFTRAILHL(int period)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                return new string[]
          {
              "TRAILHL(" + this.parameters[0] + ")"
              //,
              //"TRAILHL(" + (int)this.parameters[0]*3 + ")",
              //"TRAILHL(" +  (int)this.parameters[0]*9 + ")",
          };
            }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black, 1) };
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
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie trail1Serie = new FloatSerie(stockSerie.Count);
            //FloatSerie trail2Serie = new FloatSerie(stockSerie.Count);
            //FloatSerie trail3Serie = new FloatSerie(stockSerie.Count);

            int period = (int)this.parameters[0];
            BoolSerie event1 = stockSerie.GetIndicator("TRAILHLSR(" + period + ")").Events[8];
            BoolSerie event2 = stockSerie.GetIndicator("TRAILHLSR(" + period * 3 + ")").Events[8];
            BoolSerie event3 = stockSerie.GetIndicator("TRAILHLSR(" + period * 9 + ")").Events[8];
            BoolSerie event4 = stockSerie.GetIndicator("TRAILHLSR(" + period * 27 + ")").Events[8];

            for (int i = period * 9; i < stockSerie.Count; i++)
            {
                trail1Serie[i] = (event1[i] ? 1f : -1f) + (event2[i] ? 2f : -2f) + (event3[i] ? 3f : -3f) + (event4[i] ? 4f : -4f);
                //trail2Serie[i] = event2[i] ? 2f : -2f;
                //trail3Serie[i] = event3[i] ? 3f : -3f;
            }

            this.series[0] = trail1Serie;
            //this.series[1] = trail2Serie;
            //this.series[2] = trail3Serie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 10; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = event1[i] && event2[i] && event3[i];
                this.eventSeries[1][i] = !(event1[i] || event2[i] || event3[i]);
                this.eventSeries[2][i] = this.eventSeries[0][i] && !this.eventSeries[0][i - 1];
                this.eventSeries[3][i] = this.eventSeries[1][i] && !this.eventSeries[1][i - 1];
            }
        }

        static string[] eventNames = new string[] { "UpSwing", "DownSwing", "NewUpSwing", "NewDownSwing", };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
