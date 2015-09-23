using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_PIVOT: StockPaintBarBase
    {
        public StockPaintBar_PIVOT()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        
        public override string Definition
        {
            get { return "PIVOT(int LeftStrength, int RightStrength)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "LeftStrength", "RightStrength" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 50), new ParamRangeInt(1, 50) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string []{"TopPivot", "BottomPivot"};
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
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
            this.Events[0] = stockSerie.GetSerie(StockDataType.HIGH).GetHighPivotSerie((int)this.Parameters[0], (int)this.Parameters[1]);
            this.Events[1] = stockSerie.GetSerie(StockDataType.LOW).GetLowPivotSerie((int)this.Parameters[0], (int)this.Parameters[1]);
        }
    }
}
