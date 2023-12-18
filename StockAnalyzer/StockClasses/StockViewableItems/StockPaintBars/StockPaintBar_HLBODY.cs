using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_HLBODY : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "UpTrend", "DownTrend" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent => isEvent;

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
