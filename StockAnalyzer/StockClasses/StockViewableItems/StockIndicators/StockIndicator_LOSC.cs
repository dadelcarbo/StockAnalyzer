using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_LOSC : StockIndicatorBase
    {
        public StockIndicator_LOSC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "LOSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "LOSC(int Period1, int period2)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 5, 20, 2.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(-50f, 50f) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "Period2", "Factor" }; }
        }

        public override string[] SerieNames { get { return new string[] { this.Name }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie emaSerie1 = closeSerie.CalculateEMA((int)this.parameters[0]);
            FloatSerie emaSerie2 = closeSerie.CalculateEMA((int)this.parameters[1]);
            FloatSerie oscSerie = emaSerie1 - emaSerie2;

            FloatSerie loscSerie = emaSerie2 + oscSerie.Mult((float)this.parameters[2]);
            this.series[0] = loscSerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < loscSerie.Count; i++)
            {
                this.eventSeries[0][i] = oscSerie[i] < 0 && closeSerie[i] < loscSerie[i];
                this.eventSeries[1][i] = oscSerie[i] > 0 && closeSerie[i] > loscSerie[i];
            }
        }

        static string[] eventNames = new string[] { "Oversold", "Overbought" };
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
