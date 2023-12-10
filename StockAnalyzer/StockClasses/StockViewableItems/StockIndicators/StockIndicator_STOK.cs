using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{

    public class StockIndicator_STOK : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 100.0f; }
        }
        public float Min
        {
            get { return 0.0f; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { $"STOK({this.Parameters[0]})" }; } }


        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }

        HLine[] lines;
        public override HLine[] HorizontalLines => lines ??= new HLine[] {
            new HLine(50, new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(25f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(75f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
        };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastK = stockSerie.CalculateFastOscillator((int)this.parameters[0]);
            this.series[0] = fastK;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
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
