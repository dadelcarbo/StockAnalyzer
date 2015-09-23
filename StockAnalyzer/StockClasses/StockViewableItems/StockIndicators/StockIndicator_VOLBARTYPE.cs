using System;
using System.Drawing;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLBARTYPE : StockIndicatorBase
    {
        public StockIndicator_VOLBARTYPE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Definition
        {
            get { return "VOLBARTYPE()"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }


        public override string[] SerieNames { get { return new string[] { }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            List<StockEvent.EventType> eventTypes = new List<StockEvent.EventType>();
            foreach (string eventName in eventNames)
            {
                eventTypes.Add( (StockEvent.EventType) Enum.Parse(typeof(StockEvent.EventType), eventName));
            }
            // 
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int eventNum = 0;
                foreach (StockEvent.EventType eventType in eventTypes)
                {
                    this.Events[eventNum++][i] = stockSerie.DetectEvent(eventType, i);
                }
            }
        }

        static string[] eventNames = new string[] { "VolClimaxUp", "VolClimaxDown" , "VolLowVolume" , "VolChurn" , "VolClimaxChurn"};
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}


