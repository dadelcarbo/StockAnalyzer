using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OBV : StockIndicatorBase
    {
        public StockIndicator_OBV()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override bool RequiresVolumeData => true;
        public override string Name => "OBV()";
        public override string Definition => "OBV()";
        public override string[] ParameterNames => new string[] { };

        public override Object[] ParameterDefaultValues => new Object[] { };
        public override ParamRange[] ParameterRanges => new ParamRange[] { };
        public override string[] SerieNames => new string[] { "OBV()" };


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
            this.series[0] = stockSerie.CalculateOnBalanceVolume();
            this.Series[0].Name = this.Name;
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
