using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DDLINE : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Lookback", "ATRPeriod", "DD1", "DD2", "DD3", "DD4" };
        public override Object[] ParameterDefaultValues => new Object[] { 175, 35, 1.0f, 2.0f, 3.0f, 4.0f };
        public override ParamRange[] ParameterRanges => new ParamRange[] {
            new ParamRangeInt(0,1000),
            new ParamRangeInt(0,500),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f),
            new ParamRangeFloat(0f, 100f) };

        public override string[] SerieNames => new string[] { "MAX", "DD1", "DD2", "DD3", "DD4" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.DarkRed), new Pen(Color.DarkRed) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            float dd1 = (float)this.parameters[2];
            float dd2 = (float)this.parameters[3];
            float dd3 = (float)this.parameters[4];
            float dd4 = (float)this.parameters[5];

            // Calculate Donchian Channel
            FloatSerie dd1Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd2Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd3Line = new FloatSerie(stockSerie.Count);
            FloatSerie dd4Line = new FloatSerie(stockSerie.Count);

            var upLine = stockSerie.GetSerie(StockDataType.HIGH).MaxSerie((int)this.parameters[0]);
            var atrSerie = stockSerie.GetIndicator($"ATR({(int)this.parameters[1]})").Series[0];

            dd1Line[0] = upLine[0] - dd1 * atrSerie[0];
            dd2Line[0] = upLine[0] - dd2 * atrSerie[0];
            dd3Line[0] = upLine[0] - dd3 * atrSerie[0];
            dd4Line[0] = upLine[0] - dd4 * atrSerie[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                dd1Line[i] = upLine[i] - dd1 * atrSerie[i];
                dd2Line[i] = upLine[i] - dd2 * atrSerie[i];
                dd3Line[i] = upLine[i] - dd3 * atrSerie[i];
                dd4Line[i] = upLine[i] - dd4 * atrSerie[i];
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
