using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_XEMA : StockIndicatorMovingAvgBase
    {
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
                var atrSerie = stockSerie.GetIndicator("ATR(14)").Series[0]*2.0f;
                maSerie[0] = closeSerie[0];
                float previousMa = closeSerie[0];
                for (int i = 1; i < closeSerie.Count; i++)
                {
                    if (Math.Abs(closeSerie[i] - previousMa) > atrSerie[i])
                    {
                        previousMa = maSerie[i] = previousMa + alpha * (closeSerie[i] - previousMa);
                    }
                    else
                    {
                        maSerie[i] = previousMa;
                    }
                }
                this.CalculateEvents(stockSerie);
            }

        }
    }
}
