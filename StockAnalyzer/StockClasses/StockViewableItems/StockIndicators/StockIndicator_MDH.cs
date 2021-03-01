using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MDH : StockIndicatorBase
    {
        public override string Definition => "Display the highest, lowest and mid lines for the specified period.";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 60 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "Highest", "Mid", "Lowest" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.DarkRed) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            // Calculate MDH Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            FloatSerie bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
            FloatSerie bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

            upLine[0] = bodyHighSerie[0];
            downLine[0] = bodyLowSerie[0];
            midLine[0] = closeSerie[0];

            for (int i = 1; i <= period; i++)
            {
                upLine[i] = bodyHighSerie.GetMax(0, i - 1);
                downLine[i] = bodyLowSerie.GetMin(0, i - 1);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }
            for (int i = period + 1; i < stockSerie.Count; i++)
            {
                upLine[i] = bodyHighSerie.GetMax(i - period - 1, i - 1);
                downLine[i] = bodyLowSerie.GetMin(i - period - 1, i - 1);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = true;

            for (int i = period; i < stockSerie.Count; i++)
            {
                count = 0;

                if (upTrend)
                {
                    upTrend = bodyHighSerie[i] > midLine[i];
                }
                else
                {
                    upTrend = bodyLowSerie[i] > midLine[i];
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
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true
          };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
