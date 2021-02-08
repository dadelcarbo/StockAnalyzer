using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Web.UI.WebControls.WebParts;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MANSFIELD : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Mansfield Relative Performance indicator" + Environment.NewLine + "Plots the relative strength compared to an index or a stock";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Index" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, "CAC40" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeStockName() };
        public override string[] SerieNames => new string[] { $"MANSFIELD({this.Parameters[0]},{this.Parameters[0]})" };

        public override System.Drawing.Pen[] SeriePens => seriePens ?? (seriePens = new Pen[] { new Pen(Color.Black) });

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ?? (lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) });

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie mansfieldSerie = new FloatSerie(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Get index close serie.
            var indexSerie = StockDictionary.Instance[this.parameters[1] as string];
            var indexCloseSerie = stockSerie.GenerateSecondarySerieFromOtherSerie(indexSerie, stockSerie.BarDuration);

            var rpSerie = (closeSerie / indexCloseSerie) * 100f;
            var mrpSerie = (rpSerie / rpSerie.CalculateEMA(period) - 1f) * 100f;

            this.series[0] = mrpSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = mansfieldSerie[i] > 0;
                this.eventSeries[1][i] = mansfieldSerie[i] < 0;
            }
        }
        static string[] eventNames = new string[] { "Positive", "Negative" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
