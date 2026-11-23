using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STDEVE : StockIndicatorBase
    {
        public override string Definition => "Normalized standard deviation based on EMA";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };

        public override string[] SerieNames => new string[] { "STDEV(" + this.Parameters[0].ToString() + ")" };
        public override string[] SerieFormats => serieFormats ??= new string[] { "P2" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { ColorManager.GetPen("Indicator.Main") };

        public override HLine[] HorizontalLines => null;

        public override void ApplyTo(DataSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie emaSerie = closeSerie.CalculateEMA((int)this.Parameters[0]);

            this.series[0] = closeSerie.CalculateStdev(emaSerie, (int)this.Parameters[0]) / emaSerie;
            this.Series[0].Name = this.Name;
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
