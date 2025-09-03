using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RSN : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Normalized Relative Strength indicator" + Environment.NewLine +
            "Plots the relative strength compared to an index or a stock";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Index", "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { "CAC40", 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeStockName(), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { $"RSN({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Get index close serie.
            var indexSerie = StockDictionary.Instance[this.parameters[0] as string];
            if (!indexSerie.Initialise())
                return;

            var period = (int)this.parameters[1];
            var indexCloseSerie = stockSerie.GenerateSecondarySerieFromOtherSerie(indexSerie, stockSerie.BarDuration);

            var rsSerie = (closeSerie / indexCloseSerie).CalculateZScore(period);

            this.series[0] = rsSerie;
            this.Series[0].Name = this.Name;
        }
        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
