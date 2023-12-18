using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
    public class StockTrail_SAR : StockTrailBase, IStockTrail
    {
        public override string Definition => "Draws a trail upon an indicator using SAR method";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.TrailCurve;

        public override object[] ParameterDefaultValues => new Object[] { 0.01f, 0.01f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.0f, 0.1f), new ParamRangeFloat(0.0f, 0.1f) };
        public override string[] ParameterNames => new string[] { "Start", "Step" };


        public override string[] SerieNames => new string[] { "TrailSAR" };

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
                FloatSerie trailSerie = indicatorSerie.CalculateSARTrail((float)this.parameters[0], (float)this.parameters[1]);
                this.Series[0] = trailSerie;

                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                for (int i = 10; i < stockSerie.Count; i++)
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

