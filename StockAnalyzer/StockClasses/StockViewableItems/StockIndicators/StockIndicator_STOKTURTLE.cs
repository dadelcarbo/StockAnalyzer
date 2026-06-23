using StockAnalyzer.StockMath;
using StockAnalyzer.StockData;
using StockAnalyzerSettings;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKTURTLE : StockIndicatorBase, IRange
    {
        public override string Definition => "Calulcate a stochastics indicator based on turtle indicator, i.e. based on high/low and close based on ema";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public float Max => 100.0f;
        public float Min => 0.0f;

        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 35, 6 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { $"STO({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { ColorManager.GetPen("Indicator.Default") };

        public override void ApplyTo(DataSerie stockSerie)
        {
            var turtle = stockSerie.GetIndicator($"TURTLE({this.Parameters[0]},{this.Parameters[1]},{this.Parameters[2]})");

            var lowSerie = turtle.Series[2];
            var highSerie = turtle.Series[1];
            var emaSerie = turtle.Series[0];

            var stoSerie = new FloatSerie(stockSerie.Count, 50.0f);
            this.CreateEventSeries(stockSerie.Count);

            bool bull = false;
            for (int i = Math.Max((int)this.Parameters[0], (int)this.Parameters[1]); i < stockSerie.Count; i++)
            {
                if (highSerie[i] != lowSerie[i])
                    stoSerie[i] = (float)Math.Round(100.0 * (emaSerie[i] - lowSerie[i]) / (highSerie[i] - lowSerie[i]),2);

                if (bull)
                {
                    if (emaSerie[i] <= lowSerie[i])
                    {
                        bull = false;
                        this.Events[1][i] = true;
                    }
                }
                else
                {
                    if (emaSerie[i] >= highSerie[i])
                    {
                        bull = true;
                        this.Events[0][i] = true;
                    }
                }

                this.Events[2][i] = bull;
                this.Events[3][i] = !bull;
            }

            this.series[0] = stoSerie;
            this.series[0].Name = this.SerieNames[0];
        }

        static readonly string[] eventNames = new string[] { "BullStart", "BearStart", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
