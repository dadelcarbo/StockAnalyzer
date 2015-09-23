using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OSC : StockIndicatorBase
    {
        public StockIndicator_OSC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Name
        {
            get { return "OSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "OSC(int Period1, int Period2)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "Period2" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500)}; }
        }
        public override string[] SerieNames { get { return new string[] { "OSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

        
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray))};
                }
                return lines; }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            IStockIndicator fastSerie = stockSerie.GetIndicator("EMA(" + this.parameters[0] + ")");
            IStockIndicator slowSerie = stockSerie.GetIndicator("EMA(" + this.parameters[1] + ")");
            FloatSerie oscSerie = fastSerie.Series[0].Sub(slowSerie.Series[0]); 
            this.series[0] = oscSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (oscSerie[i - 2] < oscSerie[i - 1] && oscSerie[i - 1] > oscSerie[i]);
                this.eventSeries[1][i] = (oscSerie[i - 2] > oscSerie[i - 1] && oscSerie[i - 1] < oscSerie[i]);
                this.eventSeries[2][i] = (oscSerie[i - 1] < 0 && oscSerie[i] >= 0);
                this.eventSeries[3][i] = (oscSerie[i - 1] > 0 && oscSerie[i] <= 0);
            }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
