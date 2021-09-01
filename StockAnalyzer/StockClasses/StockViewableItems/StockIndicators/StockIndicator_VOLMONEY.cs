using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLMONEY : StockIndicatorBase
    {
        public override string Definition => "Calculate the excahnged volume money, Price*Volume in K€";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override bool RequiresVolumeData { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "SmoothPeriod", "Trigger" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, int.MaxValue) }; }
        }
        public override string[] SerieNames { get { return new string[] { "VOLMONEY", "SIGNAL(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen, 2), new Pen(Color.DarkRed, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie volume = stockSerie.GetSerie(StockDataType.VOLUME) * stockSerie.GetSerie(StockDataType.CLOSE) / 1000.0f;
            FloatSerie slowSerie = volume.CalculateEMA(((int)parameters[0]));

            this.Series[0] = volume;
            this.Series[0].Name = SerieNames[0];
            this.Series[1] = slowSerie;
            this.Series[1].Name = SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var trigger = (int)parameters[0];
            for (int i = 20; i < stockSerie.Count; i++)
            {
                if (slowSerie[i] > trigger)
                {
                    this.Events[0][i] = true;
                }
                else
                {
                    this.Events[1][i] = true;
                }
            }
        }

        static readonly string[] eventNames = new string[] { "GoodLiquitity", "BadLiquitity" };
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
