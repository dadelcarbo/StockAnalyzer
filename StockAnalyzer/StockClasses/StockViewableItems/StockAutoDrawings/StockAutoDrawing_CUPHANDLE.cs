using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_CUPHANDLE : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns";

        public override string[] ParameterNames => new string[] { "Period", "Right HL" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool() };

        public override string[] SerieNames => new string[] { };
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = period * 2; i < stockSerie.Count; i++)
            {
                var cupHandle = closeSerie.DetectCupHandle(i, period, rightHigherLow);
                if (cupHandle != null)
                {
                    brokenUpEvents[i] = true;
                    this.DrawingItems.Insert(0, cupHandle);
                }
            }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "BrokenUp" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;
    }
}