using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLMONEY : StockIndicatorBase
    {
        public override string Definition => "Calculate the exchanged volume money, Price*Volume in M€.";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override bool RequiresVolumeData { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Threshold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 10, 0.5f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 100), new ParamRangeFloat(0.0f, 1000f) }; }
        }
        public override string[] SerieNames { get { return new string[] { "Exchanged M€", "Exchange MA" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black, 1), new Pen(Color.Black, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie volume = stockSerie.GetSerie(StockDataType.EXCHANGED) / 1000000.0f;
            FloatSerie volumeAvg = volume.CalculateMA((int)parameters[0]);

            this.Series[0] = volume;
            this.Series[0].Name = SerieNames[0];

            this.Series[1] = volumeAvg;
            this.Series[1].Name = SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var threshold = (float)this.parameters[1];
            for (int i = 1; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = volumeAvg[i] > threshold;
            }
        }

        static readonly string[] eventNames = new string[] { "HasLiquidity" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
