using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ZMA : StockIndicatorMovingAvgBase
    {
        public override string Definition => "ZMA: Z-Score based EMA. It adjusts ema coefficient based on ZScore to avoid whipsaw when score is low";
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var emaPeriod = (int)this.parameters[0];
            var maSerie = new FloatSerie(stockSerie.Count);
            this.Series[0] = maSerie;
            maSerie.Name = this.SerieNames[0];
            float alpha = 2.0f / (float)(emaPeriod + 1);

            if (emaPeriod <= 1)
            {
                for (int i = 0; i < stockSerie.Count; i++)
                {
                    maSerie[i] = closeSerie[i];
                }
            }
            else
            {
                maSerie[0] = closeSerie[0];
                float previousMa = closeSerie[0];
                for (int i = 1; i < emaPeriod; i++)
                {
                    previousMa = maSerie[i] = previousMa + alpha * (closeSerie[i] - previousMa);
                }
                for (int i = emaPeriod; i < closeSerie.Count; i++)
                {
                    // Calculate the standard deviation over the EMA period
                    var zScore = Math.Abs(closeSerie.CalculateZScore(maSerie[i - 1], i, emaPeriod)) ;
                    var coef = (float)Math.Tanh(zScore * zScore * zScore / 10f);
                    previousMa = maSerie[i] = previousMa + alpha * coef * (closeSerie[i] - previousMa);
                }
                this.CalculateEvents(stockSerie);
            }
        }
    }
}
