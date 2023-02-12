using System;
using System.Collections.Generic;
using System.Linq;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public static class MathExtensions
    {
        #region StockBars extensions
        public static double[] CalculateATR(this List<StockBar> bars)
        {
            var serie = new double[bars.Count];
            var previousBar = bars.FirstOrDefault();
            int i = 0;
            foreach (var bar in bars)
            {
                serie[i++] = (Math.Max(previousBar.Close, bar.High) - Math.Min(previousBar.Close, bar.Low));
                previousBar = bar;
            }
            return serie;
        }
        public static double[] NATR(this List<StockBar> bars)
        {
            var serie = new double[bars.Count];
            var previousBar = bars.FirstOrDefault();
            int i = 0;
            foreach (var bar in bars)
            {
                serie[i++] = (Math.Max(previousBar.Close, bar.High) - Math.Min(previousBar.Close, bar.Low)) / bar.Close;
                previousBar = bar;
            }
            return serie;
        }

        public static void CalculateBandTrailStop(this List<StockBar> bars, double[] lowerBand, double[] upperBand, out double[] longStopSerie, out double[] shortStopSerie)
        {
            longStopSerie = new double[lowerBand.Length];
            shortStopSerie = new double[lowerBand.Length];
            var previousBar = bars.First();

            bool upTrend = true;
            if (upTrend)
            {
                longStopSerie[0] = previousBar.Low;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousBar.High;
            }
            int i = 0;
            foreach (var currentBar in bars)
            {
                if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentBar.Close < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = upperBand[i];
                        }
                        else
                        {
                            // UpTrend still in place
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBand[i]);
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentBar.Close > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowerBand[i];
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Down trend still in place
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBand[i]);
                        }
                    }
                }
                previousBar = currentBar;
                i++;
            }
        }
        #endregion
        #region Double[] extensions
        public static double[] Mult(this double[] s1, double a)
        {
            var multSerie = new double[s1.Length];
            for (int i = 0; i < s1.Length; i++)
            {
                multSerie[i] = s1[i] * a;
            }
            return multSerie;
        }
        public static double[] Add(this double[] s1, double[] s2)
        {
            var multSerie = new double[s1.Length];
            for (int i = 0; i < s1.Length; i++)
            {
                multSerie[i] = s1[i] + s2[i];
            }
            return multSerie;
        }
        public static double[] Sub(this double[] s1, double[] s2)
        {
            var multSerie = new double[s1.Length];
            for (int i = 0; i < s1.Length; i++)
            {
                multSerie[i] = s1[i] - s2[i];
            }
            return multSerie;
        }

        public static double[] CalculateEMA(this double[] values, int period)
        {
            var ema = new double[values.Length];
            if (period <= 1)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    ema[i] = values[i];
                }
            }
            else
            {
                float alpha = 2.0f / (float)(period + 1);

                ema[0] = values[0];
                for (int i = 1; i < values.Count(); i++)
                {
                    ema[i] = ema[i - 1] + alpha * (values[i] - ema[i - 1]);
                }
            }
            return ema;
        }

        public static double[] CalculateMA(this double[] values, int period)
        {
            var ma = new double[values.Length];
            var cumul = 0.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (i < period)
                {
                    cumul += values[i];
                    ma[i] = cumul / (i + 1);
                }
                else
                {
                    cumul += values[i] - values[i - period];
                    ma[i] = cumul / period;
                }
            }
            return ma;
        }
        #endregion
    }
}
