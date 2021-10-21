using System;
using System.Linq;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RS : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Relative Strength indicator" + Environment.NewLine +
            "Plots the relative strength compared to an index or a stock";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Index" };
        public override Object[] ParameterDefaultValues => new Object[] { "CAC40" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeStockName() };
        public override string[] SerieNames => new string[] { $"RS({this.Parameters[0]})" };

        public override System.Drawing.Pen[] SeriePens => seriePens ?? (seriePens = new Pen[] { new Pen(Color.Black) });

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ?? (lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) });

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Get index close serie.
            var indexSerie = StockDictionary.Instance[this.parameters[0] as string];
            if (!indexSerie.Initialise())
                return;

            var indexCloseSerie = stockSerie.GenerateSecondarySerieFromOtherSerie(indexSerie, stockSerie.BarDuration);

            var rsSerie = 100.0f * closeSerie / indexCloseSerie;

            this.series[0] = rsSerie;
            this.Series[0].Name = this.Name;

        }
        static string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
