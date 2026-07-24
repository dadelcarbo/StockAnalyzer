using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using System;
using System.Diagnostics.Metrics;
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
            var rsSerie = 100.0f * closeSerie / indexCloseSerie;

            this.series[0] = rsSerie;
            this.Series[0].Name = this.Name;

        }
        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
