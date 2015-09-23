using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HOSC : StockIndicatorBase
    {
        public StockIndicator_HOSC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Name
        {
            get { return "HOSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "HOSC(int HMAPeriod, int EMAPeriod)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "HMAPeriod", "EMAPeriod" }; }
        }


        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500)}; }
        }
        public override string[] SerieNames { get { return new string[] { "HOSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

        
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
            IStockIndicator fastSerie = stockSerie.GetIndicator("HMA(" + this.parameters[0] + ")");
            IStockIndicator slowSerie = stockSerie.GetIndicator("EMA(" + this.parameters[1] + ")");
            FloatSerie HOSCSerie = fastSerie.Series[0].Sub(slowSerie.Series[0]); 
            this.series[0] = HOSCSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (HOSCSerie[i - 2] < HOSCSerie[i - 1] && HOSCSerie[i - 1] > HOSCSerie[i]);
                this.eventSeries[1][i] = (HOSCSerie[i - 2] > HOSCSerie[i - 1] && HOSCSerie[i - 1] < HOSCSerie[i]);
                this.eventSeries[2][i] = (HOSCSerie[i - 1] < 0 && HOSCSerie[i] >= 0);
                this.eventSeries[3][i] = (HOSCSerie[i - 1] > 0 && HOSCSerie[i] <= 0);
                this.eventSeries[4][i] = HOSCSerie[i] >= 0;
                this.eventSeries[5][i] = HOSCSerie[i] < 0;
            }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative", "Positive", "Negative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true,  true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
