using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MANSFIELD : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Mansfield Relative Performance indicator" + Environment.NewLine + "Plots the relative strength compared to an index or a stock";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Index" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, "CAC40" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeStockName() };
        public override string[] SerieNames => new string[] { $"MANSFIELD({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            int period = (int)this.parameters[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Get index close serie.
            var indexSerie = StockDictionary.Instance[this.parameters[1] as string];
            if (!indexSerie.Initialise())
            {
                this.series[0] = new StockMath.FloatSerie(stockSerie.Count);
                this.Series[0].Name = this.Name;
                return;
            }

            this.eventSeries[2][this.eventSeries[2].Count - 1] = (stockSerie.BarDuration == BarDuration.Daily && indexSerie.LastValue.DATE != stockSerie.LastValue.DATE);
            var indexCloseSerie = stockSerie.GenerateSecondarySerieFromOtherSerie(indexSerie, stockSerie.BarDuration);
            if (indexCloseSerie == null)
            {
                this.series[0] = new StockMath.FloatSerie(stockSerie.Count);
                this.Series[0].Name = this.Name;
                return;
            }

            var rpSerie = (closeSerie / indexCloseSerie) * 100f;
            var mrpSerie = (rpSerie / rpSerie.CalculateEMA(period) - 1f) * 100f;
            this.series[0] = mrpSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = mrpSerie[i] > 0;
                this.eventSeries[1][i] = mrpSerie[i] < 0;
            }
        }
        static readonly string[] eventNames = new string[] { "Positive", "Negative", "Obsolete" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
