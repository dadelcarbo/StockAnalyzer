using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_POSC : StockIndicatorBase
    {
        public StockIndicator_POSC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Name
        {
            get { return "POSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "POSC(int Period1, int Period2)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "Period2" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 19, 39 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500)}; }
        }
        public override string[] SerieNames { get { return new string[] { "POSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

        
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
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie POSCSerie = fastSerie.Series[0].Sub(slowSerie.Series[0])/closeSerie; 
            this.series[0] = POSCSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (POSCSerie[i - 2] < POSCSerie[i - 1] && POSCSerie[i - 1] > POSCSerie[i]);
                this.eventSeries[1][i] = (POSCSerie[i - 2] > POSCSerie[i - 1] && POSCSerie[i - 1] < POSCSerie[i]);
                this.eventSeries[2][i] = (POSCSerie[i - 1] < 0 && POSCSerie[i] >= 0);
                this.eventSeries[3][i] = (POSCSerie[i - 1] > 0 && POSCSerie[i] <= 0);
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
