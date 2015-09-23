using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_VOLPATTERN: StockPaintBarBase
    {
        public StockPaintBar_VOLPATTERN()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }

        public override string Definition
        {
            get { return "VOLPATTERN()"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] {  }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[8];
                    for (int i = 0; i < 8; i++)
                    {
                        eventNames[i] = ((StockEvent.EventType)StockEvent.EventType.VolPro + i).ToString();
                    }
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { 
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Green), new Pen(Color.Red), 
                        new Pen(Color.Green), new Pen(Color.Red), 
                        new Pen(Color.Green), new Pen(Color.Red)};

                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {            
            for (int i = 0; i < this.SeriesCount; i++)
            {
                this.eventSeries[i] = stockSerie.GetSerie((StockEvent.EventType)StockEvent.EventType.VolPro + i);
            }
        }
    }
}
