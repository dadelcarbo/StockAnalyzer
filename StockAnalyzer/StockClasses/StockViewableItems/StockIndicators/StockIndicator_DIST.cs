using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DIST : StockIndicatorBase
    {
        public StockIndicator_DIST()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Definition
        {
            get { return "DIST(int Period, float Overbought, float Oversold)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 50}; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "DIST(" + this.Parameters[0].ToString() + ")"}; } }

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
        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            IStockIndicator momentum = stockSerie.GetIndicator("MOMENTUM(" + period + ",1,10)");
            FloatSerie momentumSerie = momentum.Series[0];

            FloatSerie atrSerie = stockSerie.GetSerie(StockDataType.ATR);
            FloatSerie distSerie = momentumSerie.Div(atrSerie.Cumul(period));

            

            //cciSerie = cciSerie.CalculateSigmoid(100f, 0.02f).CalculateEMA((int)Math.Sqrt();
            //FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            //for (int i = 10; i < cciSerie.Count; i++)
            //{
            //    if (cciSerie[i] > overbought && cciSerie[i] <= cciSerie[i - 1] && closeSerie[i] >= closeSerie[i-1])
            //    {
            //        cciSerie[i] = cciSerie[i - 1] + (100 - cciSerie[i - 1]) / 4f;
            //    }
            //    else if (cciSerie[i] < oversold && cciSerie[i] >= cciSerie[i - 1] && closeSerie[i] <= closeSerie[i-1])
            //    {
            //        cciSerie[i] = cciSerie[i - 1] *0.75f;
            //    }
            //}

            this.series[0] = distSerie;
            this.series[0].Name = this.Name;

        }

        static string[] eventNames = new string[] {  };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

    }
}
