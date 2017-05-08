using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLBB : StockIndicatorBase
    {
        public StockIndicator_VOLBB()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "SignalPeriod", "UpCoef", "DownCoef" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 6, 50, 0.1f, 0.1f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0.010f, 1.0f), new ParamRangeFloat(0.010f, 1.0f) }; }
        }
        public override string[] SerieNames { get { return new string[] { "VOL", "SIGNAL", "UpBand", "DownBand" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed), new Pen(Color.Maroon), new Pen(Color.Maroon) };
                    seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[3].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                FloatSerie volume = stockSerie.GetSerie(StockDataType.VOLUME).Sqrt().CalculateEMA((int)this.parameters[0]);
                FloatSerie signal = volume.CalculateEMA((int)this.parameters[1]);

                FloatSerie upBand = signal * ((float)this.parameters[2] + 1.0f);
                FloatSerie downBand = signal * (-(float)this.parameters[3] + 1.0f);

                this.series[0] = volume;
                this.Series[0].Name = this.SerieNames[0];

                this.series[1] = signal;
                this.Series[1].Name = this.SerieNames[1];

                this.series[2] = upBand;
                this.Series[2].Name = this.SerieNames[2];

                this.series[3] = downBand;
                this.Series[3].Name = this.SerieNames[3];
            }
        }

        static string[] eventNames = new string[] { "Bullish", "Bearish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}