using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ADR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };

        public override string[] SerieNames => new string[] { "ADR(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { ColorManager.GetPen("Indicator.Main") };

        public override HLine[] HorizontalLines => null;

        public override void ApplyTo(DataSerie stockSerie)
        {
            this.series[0] = new FloatSerie(stockSerie.Values.Select(v => v.ADR)).CalculateEMA((int)this.Parameters[0]);
            this.Series[0].Name = this.Name;
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
