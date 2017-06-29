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
            get { return new string[] { "Period", "UpCoef", "DownCoef" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 2f, -2f }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                    new ParamRangeInt(1, 500),
                    new ParamRangeFloat(0f, 20.0f),
                    new ParamRangeFloat(-20.0f, 0.0f)
                };
            }
        }

        public override string[] SerieNames { get { return new string[] { "VOLUME", "SIGNAL", "UpBand", "DownBand" }; } }

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
                // Calculate Bollinger Bands
                FloatSerie upperBB = null;
                FloatSerie lowerBB = null;
                FloatSerie volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
                FloatSerie volumeEMASerie = volumeSerie.CalculateEMA((int) this.parameters[0]);

                volumeSerie.CalculateBBEX(volumeEMASerie, (int) this.parameters[0], (float) this.parameters[1],
                    (float) this.parameters[2], ref upperBB, ref lowerBB);

                this.series[0] = volumeSerie;
                this.Series[0].Name = this.SerieNames[0];

                this.series[1] = volumeEMASerie;
                this.Series[1].Name = this.SerieNames[1];

                this.series[2] = upperBB;
                this.Series[2].Name = this.SerieNames[2];

                this.series[3] = lowerBB;
                this.Series[3].Name = this.SerieNames[3];

                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                for (int i = (int) this.parameters[0]; i < stockSerie.Count; i++)
                {
                    var volume = volumeSerie[i];
                    if (volume > upperBB[i])
                    {
                        this.eventSeries[0][i] = true; // Pro

                    }
                    else if (volume < lowerBB[i])
                    {
                        this.eventSeries[1][i] = true; //Am
                    }
                }
            }
        }

        static string[] eventNames = new string[] { "Pro", "Am" };
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