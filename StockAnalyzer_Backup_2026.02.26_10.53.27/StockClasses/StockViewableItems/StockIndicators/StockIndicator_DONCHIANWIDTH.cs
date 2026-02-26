using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DONCHIANWIDTH : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string Definition => "DONCHIANWIDTH(int Period, int Smoothing)";
        public override string[] ParameterNames => new string[] { "Period", "Smoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 60, 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "DONCHIANWIDTH" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            int smoothing = (int)this.parameters[1];
            IStockIndicator donchianIndicator = stockSerie.GetIndicator("DONCHIAN(" + period + ")");

            // Calculate Donchian Channel
            FloatSerie upLine = donchianIndicator.Series[0];
            FloatSerie downLine = donchianIndicator.Series[4];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            this.series[0] = ((upLine - downLine) / downLine).CalculateEMA(smoothing);
            this.Series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
