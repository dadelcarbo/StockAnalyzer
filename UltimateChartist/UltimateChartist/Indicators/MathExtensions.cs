using System;
using System.Collections.Generic;
using System.Linq;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators;

public static class MathExtensions
{
    #region StockBars extensions 
    public static decimal[] CalculateFastOscillator(this StockSerie stockSerie, int period, bool useBody)

    {
        //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
        //  %D = MA3(%K)
        var closeSerie = stockSerie.CloseValues;
        var lowSerie = useBody ? stockSerie.BodyLowValues : stockSerie.LowValues;
        var highSerie = stockSerie.HighValues;

        int count = closeSerie.Length;
        var fastOscillatorSerie = new decimal[count];

        decimal lowestLow, highestHigh;

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
                fastOscillatorSerie[i] = 100.0m * (closeSerie[i] - lowestLow) / (highestHigh - lowestLow);
            }
        }
        return fastOscillatorSerie;
    }

    public static decimal[] CalculateATR(this List<StockBar> bars)
    {
        var serie = new decimal[bars.Count];
        var previousBar = bars.FirstOrDefault();
        int i = 0;
        foreach (var bar in bars)
        {
            serie[i++] = (Math.Max(previousBar.Close, bar.High) - Math.Min(previousBar.Close, bar.Low));
            previousBar = bar;
        }
        return serie;
    }
    public static decimal[] NATR(this List<StockBar> bars)
    {
        var serie = new decimal[bars.Count];
        var previousBar = bars.FirstOrDefault();
        int i = 0;
        foreach (var bar in bars)
        {
            serie[i++] = (Math.Max(previousBar.Close, bar.High) - Math.Min(previousBar.Close, bar.Low)) / bar.Close;
            previousBar = bar;
        }
        return serie;
    }

    public static void CalculateBandTrailStop(this List<StockBar> bars, decimal[] lowerBand, decimal[] upperBand, out decimal?[] longStopSerie, out decimal?[] shortStopSerie)
    {
        longStopSerie = new decimal?[lowerBand.Length];
        shortStopSerie = new decimal?[lowerBand.Length];
        var previousBar = bars.First();

        bool upTrend = true;
        if (upTrend)
        {
            longStopSerie[0] = previousBar.Low;
            shortStopSerie[0] = null;
        }
        else
        {
            longStopSerie[0] = null;
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
                        longStopSerie[i] = null;
                        shortStopSerie[i] = upperBand[i];
                    }
                    else
                    {
                        // UpTrend still in place
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1].Value, lowerBand[i]);
                        shortStopSerie[i] = null;
                    }
                }
                else
                {
                    if (currentBar.Close > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = lowerBand[i];
                        shortStopSerie[i] = null;
                    }
                    else
                    {
                        // Down trend still in place
                        longStopSerie[i] = null;
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1].Value, upperBand[i]);
                    }
                }
            }
            previousBar = currentBar;
            i++;
        }
    }
    #endregion
    #region decimal[] extensions
    public static decimal[] Mult(this decimal[] s1, decimal a)
    {
        var multSerie = new decimal[s1.Length];
        for (int i = 0; i < s1.Length; i++)
        {
            multSerie[i] = s1[i] * a;
        }
        return multSerie;
    }
    public static decimal[] Add(this decimal[] s1, decimal[] s2)
    {
        var multSerie = new decimal[s1.Length];
        for (int i = 0; i < s1.Length; i++)
        {
            multSerie[i] = s1[i] + s2[i];
        }
        return multSerie;
    }
    public static decimal[] Sub(this decimal[] s1, decimal[] s2)
    {
        var multSerie = new decimal[s1.Length];
        for (int i = 0; i < s1.Length; i++)
        {
            multSerie[i] = s1[i] - s2[i];
        }
        return multSerie;
    }

    public static decimal Min(this decimal[] values, int startIndex, int endIndex)
    {
        decimal min = decimal.MaxValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (min > values[i]) min = values[i];
        }
        return min;
    }
    public static decimal Max(this decimal[] values, int startIndex, int endIndex)
    {
        decimal max = decimal.MinValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (max < values[i]) max = values[i];
        }
        return max;
    }

    public static decimal[] CalculateEMA(this decimal[] values, int period)
    {
        var ema = new decimal[values.Length];
        if (period <= 1)
        {
            for (int i = 0; i < values.Length; i++)
            {
                ema[i] = values[i];
            }
        }
        else
        {
            decimal alpha = 2.0m / (period + 1m);

            ema[0] = values[0];
            for (int i = 1; i < values.Count(); i++)
            {
                ema[i] = ema[i - 1] + alpha * (values[i] - ema[i - 1]);
            }
        }
        return ema;
    }

    public static decimal[] CalculateMA(this decimal[] values, int period)
    {
        var ma = new decimal[values.Length];
        var cumul = 0.0m;
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
