using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DRAWDOWNADR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "ADR Period", "DD1", "DD2", "DD3", "DD4" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 15, 2f, 4, 6f, 8f };
        public override ParamRange[] ParameterRanges => new ParamRange[] {
            new ParamRangeInt(0, 500),
            new ParamRangeInt(0, 500),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f) };

        public override string[] SerieNames => new string[] { "MAX", "DD1", "DD2", "DD3", "DD4" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.DarkRed), new Pen(Color.DarkRed) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int lookback = (int)this.parameters[0];
            float dd1 = (float)this.parameters[2];
            float dd2 = (float)this.parameters[3];
            float dd3 = (float)this.parameters[4];
            float dd4 = (float)this.parameters[5];

            // Calculate Donchian Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie dd1Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd2Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd3Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd4Line = new FloatSerie(stockSerie.Count);

            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var adrSerie = stockSerie.GetIndicator($"ADR({this.parameters[1]}").Series[0].CalculateWMA(4);

            for (int i = 0; i < Math.Min(lookback, stockSerie.Count); i++)
            {
                upLine[i] = highSerie.GetMax(0, i);
                dd1Line[i] = upLine[i] - adrSerie[i] * dd1;
                dd2Line[i] = upLine[i] - adrSerie[i] * dd2;
                dd3Line[i] = upLine[i] - adrSerie[i] * dd3;
                dd4Line[i] = upLine[i] - adrSerie[i] * dd4;
            }
            for (int i = lookback; i < stockSerie.Count; i++)
            {
                upLine[i] = highSerie.GetMax(i - lookback, i);
                dd1Line[i] = upLine[i] - adrSerie[i] * dd1;
                dd2Line[i] = upLine[i] - adrSerie[i] * dd2;
                dd3Line[i] = upLine[i] - adrSerie[i] * dd3;
                dd4Line[i] = upLine[i] - adrSerie[i] * dd4;
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = dd1Line;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = dd2Line;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = dd3Line;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = dd4Line;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[]
          {

          };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[]
          {
          };
        public override bool[] IsEvent => isEvent;
    }
}
