using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_KUMODIST : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { 9, 26, 52 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] ParameterNames => new string[] { "TenkanSenPeriod", "KijunSenPeriod", "KumoPeriod" };
        public override string[] SerieNames => new string[] { "KUMODIST(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens => seriePens ?? (seriePens = new Pen[] { new Pen(Color.Black) });
        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ?? (lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) });

        public override void ApplyTo(StockSerie stockSerie)
        {
            int tenkanSenPeriod = (int)parameters[0];
            int kijunSenPeriod = (int)parameters[1];
            int kumoPeriod = (int)parameters[2];

            var ichimoku = stockSerie.GetIndicator("ICHIMOKU(" + tenkanSenPeriod + "," + kijunSenPeriod + "," + kumoPeriod + ")");

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var maxSerie = FloatSerie.MaxSerie(ichimoku.Series[2], ichimoku.Series[3]);
            var minSerie = FloatSerie.MinSerie(ichimoku.Series[2], ichimoku.Series[3]);

            var distSerie = new FloatSerie(stockSerie.Count);
            for (int i = kumoPeriod; i < stockSerie.Count; i++)
            {
                if (closeSerie[i] > maxSerie[i])
                {
                    distSerie[i] = closeSerie[i] - maxSerie[i];
                }
                else if (closeSerie[i] < minSerie[i])
                {
                    distSerie[i] = closeSerie[i] - minSerie[i];
                }
            }

            this.series[0] = distSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (distSerie[i] > 0);
                this.eventSeries[1][i] = (distSerie[i] < 0);
                this.eventSeries[2][i] = eventSeries[0][i] & !eventSeries[0][i - 1];
                this.eventSeries[3][i] = eventSeries[1][i] & !eventSeries[1][i - 1];
            }
        }

        static readonly string[] eventNames = new string[] { "UpTrend", "DownTrend", "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
