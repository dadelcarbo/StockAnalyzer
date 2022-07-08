using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_HLBODY : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period"}; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "UpTrend", "DownTrend" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, false};
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
            var drawingHLBody = stockSerie.GetAutoDrawing($"HLBODY({this.parameters[0]})");

            this.Events[0] = drawingHLBody.Events[Array.IndexOf<string>(drawingHLBody.EventNames, "UpTrend")];
            this.Events[1] = drawingHLBody.Events[Array.IndexOf<string>(drawingHLBody.EventNames, "DownTrend")];
        }
    }
}
