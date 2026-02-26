using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DRAWDOWNLINE : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "DD1", "DD2", "DD3", "DD4" };
        public override Object[] ParameterDefaultValues => new Object[] { 5.0f, 10.0f, 15.0f, 20.0f };
        public override ParamRange[] ParameterRanges => new ParamRange[] {
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
            float dd1 = (100.0f - (float)this.parameters[0]) / 100.0f;
            float dd2 = (100.0f - (float)this.parameters[1]) / 100.0f;
            float dd3 = (100.0f - (float)this.parameters[2]) / 100.0f;
            float dd4 = (100.0f - (float)this.parameters[3]) / 100.0f;

            // Calculate Donchian Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie dd1Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd2Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd3Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd4Line = new FloatSerie(stockSerie.Count);

            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            upLine[0] = highSerie[0];
            dd1Line[0] = highSerie[0] * dd1;
            dd2Line[0] = highSerie[0] * dd2;
            dd3Line[0] = highSerie[0] * dd3;
            dd4Line[0] = highSerie[0] * dd4;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine[i] = Math.Max(highSerie[i], upLine[i - 1]);
                dd1Line[i] = upLine[i] * dd1;
                dd2Line[i] = upLine[i] * dd2;
                dd3Line[i] = upLine[i] * dd3;
                dd4Line[i] = upLine[i] * dd4;
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
