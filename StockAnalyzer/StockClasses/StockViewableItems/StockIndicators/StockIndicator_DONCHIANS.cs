using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DONCHIANS : StockIndicatorBase
    {
        public override string Definition => "Display the highest and lowest lines for the specified period of a EMA.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 20, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Highest", "Lowest", "EMA" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                    new Pen(Color.DarkGreen) {Width = 2},
                    new Pen(Color.DarkRed)   {Width = 2},
                    new Pen(Color.Black)     {Width = 2} };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            if (period > stockSerie.Count)
            {
                this.CreateEventSeries(stockSerie.Count);
                return;
            }

            // Calculate MDH Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie emaSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[1]);

            upLine[0] = emaSerie[0];
            downLine[0] = emaSerie[0];

            for (int i = 1; i <= period; i++)
            {
                upLine[i] = emaSerie.GetMax(0, i);
                downLine[i] = emaSerie.GetMin(0, i);
            }
            for (int i = period + 1; i < stockSerie.Count; i++)
            {
                upLine[i] = emaSerie.GetMax(i - period - 1, i);
                downLine[i] = emaSerie.GetMin(i - period - 1, i);
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = emaSerie;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = false;
            for (int i = period; i < stockSerie.Count; i++)
            {
                count = 0;
                if (upTrend)
                {
                    upTrend = !(emaSerie[i] <= downLine[i]);
                }
                else
                {
                    upTrend = emaSerie[i] >= upLine[i];
                }

                this.Events[count++][i] = upTrend;
                this.Events[count++][i] = !upTrend;
                this.Events[count++][i] = (!this.Events[0][i - 1]) && (this.Events[0][i]);
                this.Events[count++][i] = (this.Events[0][i - 1]) && (!this.Events[0][i]);
            }
        }

        static string[] eventNames = new string[]
          {
            "Uptrend", "DownTrend", "BrokenUp","BrokenDown"
          };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true
          };
        public override bool[] IsEvent => isEvent;
    }
}
