using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BARSINEVENT : StockUpDownIndicatorBase
    {
        public override string Definition => "Calculates the numbers of bars an evant has been true";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Type", "Name", "Event" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { "TRAILSTOP", "TRAILBB(50_1.75_-1.75_EMA)", "Bullish" }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeString(), new ParamRangeString(), new ParamRangeString() }; }
        }

        public override string[] SerieNames { get { return new string[] { "DAYS" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black, 1) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            string indicatorParam = this.Parameters[0].ToString() + "|" + this.Parameters[1].ToString().Replace("_", ",");
            var viewableItem = stockSerie.GetViewableItem(indicatorParam);
            if (viewableItem == null)
                throw new InvalidOperationException($"Cannot calculate {indicatorParam}");

            string eventParam = this.Parameters[2].ToString();
            var eventItem = viewableItem as IStockEvent;
            int index = Array.IndexOf<string>(eventItem.EventNames, eventParam);
            if (index == -1)
                throw new InvalidOperationException($"Event {eventParam} not found in {indicatorParam}");

            var eventSerie = eventItem.Events[index];
            var indexSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = indexSerie;
            this.Series[0].Name = this.Name;

            int count = 0;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (eventSerie[i])
                {
                    indexSerie[i] = ++count;
                }
                else
                {
                    count = 0;
                }
            }
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
