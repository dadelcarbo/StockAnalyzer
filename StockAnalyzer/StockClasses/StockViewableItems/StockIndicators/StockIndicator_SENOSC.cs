using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SENOSC : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 9, 26 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "TenkanSenPeriod", "KijunSenPeriod" }; }
        }
        public override string[] SerieNames { get { return new string[] { "SENOSC(" + this.Parameters[0].ToString() + ")" }; } }

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
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int tenkanSenPeriod = (int)parameters[0];
            int kijunSenPeriod = (int)parameters[1];

            var ichimoku = stockSerie.GetIndicator("ICHIMOKU(" + tenkanSenPeriod + "," + kijunSenPeriod + "," + kijunSenPeriod * 2 + ")");

            var oscSerie = ichimoku.Series[0] - ichimoku.Series[1];

            this.series[0] = oscSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (oscSerie[i] > 0);
                this.eventSeries[1][i] = (oscSerie[i] < 0);
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
