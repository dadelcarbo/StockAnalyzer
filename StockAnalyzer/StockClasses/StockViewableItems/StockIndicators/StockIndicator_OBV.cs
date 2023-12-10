using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OBV : StockIndicatorBase
    {
        public StockIndicator_OBV()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }
        public override string Name
        {
            get { return "OBV()"; }
        }
        public override string Definition
        {
            get { return "OBV()"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }
        public override string[] SerieNames { get { return new string[] { "OBV()" }; } }


        public override System.Drawing.Pen[] SeriePens
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

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
