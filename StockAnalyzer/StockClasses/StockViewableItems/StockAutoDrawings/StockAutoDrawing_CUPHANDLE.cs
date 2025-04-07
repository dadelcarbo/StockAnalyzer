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

        public override string[] ParameterNames => new string[] { "Period", "Right HL", "Last Only" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, true };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeBool() };

        public override string[] SerieNames => new string[] { };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];
            var lastOnly = (bool)this.parameters[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenUp")];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            if (lastOnly)
            {
                for (int i = stockSerie.Count - 1; i > period; i--)
                {
                    var cupHandle = closeSerie.DetectCupHandle(i, period, rightHigherLow);
                    if (cupHandle != null)
                    {
                        brokenUpEvents[i] = true;
                        this.DrawingItems.Insert(0, cupHandle);
                        return;
                    }
                }
            }
            else
            {
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