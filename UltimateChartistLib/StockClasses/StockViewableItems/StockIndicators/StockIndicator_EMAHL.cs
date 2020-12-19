using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMAHL : StockIndicatorBase
    {
        public StockIndicator_EMAHL()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "EMAL(" + this.Parameters[0].ToString() + ")", "EMAH(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.DarkRed, 2) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie lowEMASerie = stockSerie.GetIndicator(this.SerieNames[0]).Series[0];
            FloatSerie highEMASerie = stockSerie.GetIndicator(this.SerieNames[1]).Series[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            this.Series[0] = lowEMASerie;
            this.Series[1] = highEMASerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = (int)this.parameters[0]; i < stockSerie.Count; i++)
            {
                if (lowSerie[i] > highEMASerie[i])
                {
                    this.Events[0][i] = true;
                    if (!this.Events[0][i-1])
                        this.Events[2][i] = true;
                }
                else if (highSerie[i] < lowEMASerie[i])
                {
                    this.Events[1][i] = true;
                    if (!this.Events[1][i - 1])
                        this.Events[3][i] = true;
                }
                else
                {
                    this.Events[4][i] = true;
                }
            }
        }

        static readonly string[] eventNames = new string[] {  "UpTrend", "DownTrend", "FirstBarAbove", "FirstBarBelow", "Ranging"};
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
