using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
    public class StockTrail_HL : StockTrailBase, IStockTrail
    {
        public StockTrail_HL()
        {
        }
        public override string Name => "HL(" + this.Parameters[0].ToString() + ")";

        public override string Definition => "Draws a trail upon an indicator";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.TrailCurve;

        public override object[] ParameterDefaultValues => new Object[] { 10 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };


        public override string[] SerieNames => new string[] { "Trail" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkRed) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            IStockIndicator indicator = stockSerie.GetIndicator(this.TrailedItem);
            if (indicator != null && indicator.Series[0].Count > 0)
            {
                FloatSerie indicatorSerie = indicator.Series[0];
                FloatSerie trailSerie = indicatorSerie.CalculateHLTrail((int)this.parameters[0]);
                this.Series[0] = trailSerie;

                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                for (int i = (int)this.parameters[0]; i < stockSerie.Count; i++)
                {
                    int j = 0;
                    this.eventSeries[j++][i] = indicatorSerie[i] >= trailSerie[i];
                    this.eventSeries[j++][i] = indicatorSerie[i] < trailSerie[i]; ;
                    this.eventSeries[j++][i] = indicatorSerie[i] >= trailSerie[i] && indicatorSerie[i - 1] < trailSerie[i - 1];
                    this.eventSeries[j++][i] = indicatorSerie[i] < trailSerie[i] && indicatorSerie[i - 1] > trailSerie[i - 1];
                    this.eventSeries[j++][i] = indicatorSerie[i] > indicatorSerie[i - 1];
                    this.eventSeries[j++][i] = indicatorSerie[i] < indicatorSerie[i - 1];
                }

            }
            else
            {
                this.Series[0] = new FloatSerie(0, this.SerieNames[0]);
            }
        }
        static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TrailedUp", "TrailedDown" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}

