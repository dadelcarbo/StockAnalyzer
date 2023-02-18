using System;
using System.Collections.Generic;
using System.Linq;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators;

public static class MathExtensions
{
    #region StockBars extensions 
    public static double[] CalculateFastOscillator(this StockSerie stockSerie, int period, bool useBody)

    {
        //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
        //  %D = MA3(%K)
        var closeSerie = stockSerie.CloseValues;
        var lowSerie = useBody ? stockSerie.BodyLowValues : stockSerie.LowValues;
        var highSerie = stockSerie.HighValues;

        int count = closeSerie.Length;
        var fastOscillatorSerie = new double[count];

        double lowestLow, highestHigh;

        int i = 0;
        for (; i < period; i++)
        {
            fastOscillatorSerie[i] = 50;
        }
        for (i = period; i < count; i++)
        {
            lowestLow = lowSerie.Min(i - period, i);
            highestHigh = highSerie.Max(i - period, i);
            if (highestHigh == lowestLow)
            {
                fastOscillatorSerie[i] = 50;
            }
            else
            {
                fastOscillatorSerie[i] = 100.0 * (closeSerie[i] - lowestLow) / (highestHigh - lowestLow);
            }
        }
        return fastOscillatorSerie;
    }

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
            shortStopSerie[0] = double.NaN;
        }
        else
        {
            longStopSerie[0] = double.NaN;
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
                        longStopSerie[i] = double.NaN;
                        shortStopSerie[i] = upperBand[i];
                    }
                    else
                    {
                        // UpTrend still in place
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBand[i]);
                        shortStopSerie[i] = double.NaN;
                    }
                }
                else
                {
                    if (currentBar.Close > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = lowerBand[i];
                        shortStopSerie[i] = double.NaN;
                    }
                    else
                    {
                        // Down trend still in place
                        longStopSerie[i] = double.NaN;
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBand[i]);
                    }
                }
            }
            previousBar = currentBar;
            i++;
        }
    }
    #endregion
    #region double[] extensions
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

    public static double Min(this double[] values, int startIndex, int endIndex)
    {
        double min = double.MaxValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (min > values[i]) min = values[i];
        }
        return min;
    }
    public static double Max(this double[] values, int startIndex, int endIndex)
    {
        double max = double.MinValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (max < values[i]) max = values[i];
        }
        return max;
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
            double alpha = 2.0f / (double)(period + 1);

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
