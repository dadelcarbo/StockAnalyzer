using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    /// <summary>
    /// Cummulative RSI with slow signal
    /// </summary>
    public class StockIndicator_CRSI : StockIndicatorBase
    {
        public StockIndicator_CRSI()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Definition
        {
            get { return "CRSI(int Period, int Smoothing)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smooting" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "CRSI(" + this.Parameters[0].ToString() + ")", "SlowCRSI(" + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen),new Pen(Color.DarkRed) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie crsiSerie;
            if (closeSerie.Min <= 0.0f)
            {
                crsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], false);
            }
            else
            {
                crsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], true);
            }
            crsiSerie = (crsiSerie - 50f).Cumul();
            this.series[0] = crsiSerie;
            this.series[0].Name = this.Name;

            FloatSerie signal = crsiSerie.CalculateEMA((int)this.parameters[1]);
            this.series[1] = signal;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = crsiSerie[i] >= signal[i];
                this.eventSeries[1][i] = crsiSerie[i] < signal[i];
                this.eventSeries[2][i] = (crsiSerie[i - 1] < signal[i - 1] && crsiSerie[i] >= signal[i]);
                this.eventSeries[3][i] = (crsiSerie[i - 1] >= signal[i - 1] && crsiSerie[i] < signal[i]);
            }
        }


        static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
