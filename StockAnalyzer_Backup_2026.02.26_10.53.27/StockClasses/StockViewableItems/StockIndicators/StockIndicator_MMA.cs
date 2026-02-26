using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MMA : StockIndicatorMovingAvgBase
    {
        public override string Definition => "Mutiple Moving Average, calulcate fast MA first then calulcate Slow MA";
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 6, 36 };
        public override ParamRange[] ParameterRanges => new ParamRange[] {
            new ParamRangeInt(1, 500),
            new ParamRangeInt(1, 500)
        };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var fastEma = closeSerie.CalculateEMA((int)this.parameters[0]);
            var slowEma = fastEma.CalculateEMA((int)this.parameters[1]);

            this.Series[0] = slowEma;
            slowEma.Name = this.SerieNames[0];

            this.CalculateEvents(stockSerie);
        }
    }
}
