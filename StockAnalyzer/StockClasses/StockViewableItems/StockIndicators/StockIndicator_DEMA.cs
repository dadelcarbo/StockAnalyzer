using StockAnalyzer.StockMath;
using StockAnalyzer.StockData;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DEMA : StockIndicatorMovingAvgBase
    {
        public override string Definition => "Exponential Moving Average on a smoothed series";
        public override object[] ParameterDefaultValues => new Object[] { 35, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "Smoothing" };
        public override string[] SerieNames => new string[] { $"{this.ShortName}({this.Parameters[0]},{this.Parameters[1]})" };

        public override void ApplyTo(DataSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            this.Series[0] = closeSerie.CalculateEMA((int)this.parameters[1]).CalculateEMA((int)this.parameters[0]);

            this.CalculateEvents(stockSerie);
        }
    }
}
