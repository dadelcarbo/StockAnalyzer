using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RND : StockIndicatorBase, IRange
    {
        public override string Definition => base.Definition + Environment.NewLine + "RandomNumber between 0 and 1";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Trigger" };
        public override Object[] ParameterDefaultValues => new Object[] { 0.5f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0f, 1f) };
        public override string[] SerieNames => new string[] { "RND(" + this.Parameters[0].ToString() + ")" };

        public float Max => 1.0f;
        public float Min => 0.0f;

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => new HLine[] { new HLine((float)this.parameters[0], new Pen(Color.LightGray)) };

        static readonly Random rnd = new Random();
        public override void ApplyTo(StockSerie stockSerie)
        {
            var rndSerie = new FloatSerie(stockSerie.Keys.Select(k => (float)rnd.NextDouble()).ToArray(), "RND");
            this.series[0] = rndSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            float trigger = (float)this.parameters[0];
            for (int i = 0; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = rndSerie[i] < trigger;
                this.eventSeries[1][i] = rndSerie[i] > trigger;
            }
        }
        static readonly string[] eventNames = new string[] { "IsTrue", "IsFalse" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
