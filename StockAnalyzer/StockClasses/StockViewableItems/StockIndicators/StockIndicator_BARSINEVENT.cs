using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BARSINEVENT : StockIndicatorBase
    {
        public override string Definition => "Calculates the numbers of bars an evant has been true";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "Indicator", "Event" };

        public override Object[] ParameterDefaultValues => new Object[] { "TRAILSTOP>TRAILBB(50_1.75_-1.75_EMA)", "Bullish" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeViewableItem(), new ParamRangeString() };

        public override string[] SerieNames => new string[] { "BARS" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black, 1) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            string indicatorParam = this.Parameters[0].ToString().Replace("_", ",").Replace(">", "|");
            var viewableItem = stockSerie.GetViewableItem(indicatorParam);
            if (viewableItem == null)
                throw new InvalidOperationException($"Cannot calculate {indicatorParam}");

            string eventParam = this.Parameters[1].ToString();
            var eventItem = viewableItem as IStockEvent;
            int index = Array.IndexOf<string>(eventItem.EventNames, eventParam);
            if (index == -1)
                throw new InvalidOperationException($"Event {eventParam} not found in {indicatorParam}");

            var eventSerie = eventItem.Events[index];
            var barInEventSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = barInEventSerie;
            this.Series[0].Name = this.Name;

            int count = 0;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (eventSerie[i])
                {
                    barInEventSerie[i] = ++count;
                }
                else
                {
                    count = 0;
                }
            }
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
