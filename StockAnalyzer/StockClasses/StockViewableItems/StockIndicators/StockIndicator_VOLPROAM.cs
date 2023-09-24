using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLPROAM : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override bool RequiresVolumeData { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "SmoothPeriod", "BBWidth" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 1.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.001f, 10f) }; }
        }
        public override string[] SerieNames { get { return new string[] { "VOLPROAM", "MA", "BBUp", "BBDown" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] {
                        new Pen(Color.DarkGreen, 2) { DashStyle = DashStyle.Custom },
                        new Pen(Color.DarkBlue, 1),
                        new Pen(Color.DarkBlue, 1),
                        new Pen(Color.DarkBlue, 1) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie volume = stockSerie.GetSerie(StockDataType.VOLUME) * 1.0f; // * stockSerie.GetSerie(StockDataType.CLOSE) / 1000.0f;
            FloatSerie upperBB = null;
            FloatSerie lowerBB = null;

            var period = (int)parameters[0];
            FloatSerie emaSerie = volume.CalculateMA(period);

            var bbWidth = (float)this.parameters[1];
            volume.CalculateBB(emaSerie, period, bbWidth, -bbWidth, ref upperBB, ref lowerBB);

            this.Series[0] = volume;
            this.Series[0].Name = SerieNames[0];
            this.Series[1] = emaSerie;
            this.Series[1].Name = SerieNames[1];
            this.Series[2] = lowerBB;
            this.Series[2].Name = SerieNames[2];
            this.Series[3] = upperBB;
            this.Series[3].Name = SerieNames[3];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (volume[i] > upperBB[i])
                    this.Events[0][i] = true;
                if (volume[i] < lowerBB[i])
                    this.Events[1][i] = true;
            }
        }

        static readonly string[] eventNames = new string[] { "Pro", "Am" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
