using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EXP : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Draws an exponential trail stop that starts";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;
        public override string[] ParameterNames => new string[] { "Period", "Rate1" };
        public override Object[] ParameterDefaultValues => new Object[] { 20, 0.1f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0001f, 1.0f) };

        public override string[] SerieNames { get { return new string[] { "EXP1", "EXP2", "EXP3" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Gray, 2), new Pen(Color.Gray, 1), new Pen(Color.Gray, 1) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            float rate1 = 1f + (float)this.parameters[1] * 0.01f;
            float rate2 = 1f + (float)this.parameters[1] * 0.02f;
            float rate3 = 1f + (float)this.parameters[1] * 0.04f;

            var expSerie1 = new FloatSerie(stockSerie.Count, "EXP1", float.NaN);
            var expSerie2 = new FloatSerie(stockSerie.Count, "EXP2", float.NaN);
            var expSerie3 = new FloatSerie(stockSerie.Count, "EXP3", float.NaN);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            bool upTrend = false;
            float trailExp1 = float.NaN;
            float trailExp2 = float.NaN;
            float trailExp3 = float.NaN;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    trailExp1 *= rate1;
                    trailExp2 *= rate2;
                    trailExp3 *= rate3;
                    if (closeSerie[i] > trailExp1) // trend continues
                    {
                        expSerie1[i] = trailExp1;
                        expSerie2[i] = trailExp2;
                        expSerie3[i] = trailExp3;
                    }
                    else
                    {
                        upTrend = false;
                        expSerie1[i] = float.NaN;
                        expSerie2[i] = float.NaN;
                        expSerie3[i] = float.NaN;
                        i += period - 1;
                    }
                }
                else
                {
                    int firstBottomIndex = lowSerie.FindMinIndex(i - period, i);
                    if (firstBottomIndex == i - period)
                    {
                        // Bottom found
                        trailExp1 = trailExp2 = trailExp3 = lowSerie[firstBottomIndex];
                        //var secondBottomIndex = lowSerie.FindMinIndex(i - period + 2, i);
                        //var duration = secondBottomIndex - firstBottomIndex;
                        //alpha = (float)(Math.Pow(lowSerie[secondBottomIndex] / trailExp, 1f / duration));
                        expSerie1[firstBottomIndex] = trailExp1;
                        expSerie2[firstBottomIndex] = trailExp2;
                        expSerie3[firstBottomIndex] = trailExp3;
                        for (int j = firstBottomIndex + 1; j <= i; j++)
                        {
                            trailExp1 *= rate1;
                            trailExp2 *= rate2;
                            trailExp3 *= rate3;
                            expSerie1[j] = trailExp1;
                            expSerie2[j] = trailExp2;
                            expSerie3[j] = trailExp3;
                        }
                        upTrend = true;
                    }
                    else
                    {
                        expSerie1[i] = float.NaN;
                        expSerie2[i] = float.NaN;
                        expSerie3[i] = float.NaN;
                    }
                }
            }
            this.Series[0] = expSerie1;
            this.Series[1] = expSerie2;
            this.Series[2] = expSerie3;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

        }

        static string[] eventNames = new string[] { };
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
