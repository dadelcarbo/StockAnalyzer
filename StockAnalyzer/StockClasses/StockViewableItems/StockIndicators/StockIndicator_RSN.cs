using System;
using StockAnalyzer.StockData;
using System.Drawing;
using StockAnalyzer.StockMath;

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

        public override void ApplyTo(DataSerie dataSerie)
        {
            this.CreateEventSeries(dataSerie.Count);
            var closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie indexCloseSerie;

            var period = (int)this.parameters[1];

            var instrument = StockDictionary.GetInstrumentByName(this.parameters[0] as string);
            if (instrument != null)
            {
                indexCloseSerie = dataSerie.GenerateSecondarySerieFromOtherSerie(instrument, dataSerie.BarDuration);
            }
            else
            {
                indexCloseSerie = closeSerie;
            }

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
