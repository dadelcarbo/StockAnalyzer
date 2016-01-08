using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockMath
{
   public class FloatSerie
   {
      public string Name { get; set; }
      public string Script { get; protected set; }

      private float max = float.MinValue;
      private float min = float.MaxValue;
      private float stdev = float.MinValue;
      private float variance = float.MinValue;
      private float mean = float.MinValue;
      private float median = float.MinValue;
      private float gamma = float.MinValue;

      #region STATISTICS PROPERTIES
      public void ResetStatistics()
      {
         max = float.MinValue;
         min = float.MaxValue;
         stdev = float.MinValue;
         mean = float.MinValue;
         median = float.MinValue;
         gamma = float.MinValue;
      }
      public float Max
      {
         get
         {
            if (max == float.MinValue)
            {
               max = this.GetMax();
            }
            return max;
         }
      }
      public float Min
      {
         get
         {
            if (min == float.MaxValue)
            {
               min = this.GetMin();
            }
            return min;
         }
      }
      public float Stdev
      {
         get
         {
            if (stdev == float.MinValue)
            {
               this.CalculateStdev();
            }
            return stdev;
         }
      }
      public float Variance
      {
         get
         {
            if (variance == float.MinValue)
            {
               this.CalculateStdev();
            }
            return variance;
         }
      }
      public float CalculateAVG()
      {
         float sum = 0.0f;
         foreach (float value in Values)
         {
            sum += value;
         }
         return sum / (float)Values.Count();
      }
      public float CalculateAVG(int startIndex, int endIndex)
      {
         float sum = 0.0f;
         for (int i = startIndex; i <= endIndex; i++)
         {
            sum += this[i];
         }
         return sum / (endIndex - startIndex);
      }

      private void CalculateStdev()
      {
         float avg = this.Mean;
         float sum = 0.0f;
         float spread = 0.0f;
         foreach (float value in Values)
         {
            spread = value - avg;
            sum += spread * spread;
         }
         variance = sum / (float)Values.Count();
         stdev = (float)Math.Sqrt(variance);
      }
      #region STATISTICAL FUNCTIONS
      public FloatSerie CalculateVariance(int period)
      {
         FloatSerie ema = this.CalculateMA(period);
         FloatSerie variance = new FloatSerie(this.Count, "VAR");
         float avg;
         float sum;
         float spread;
         int count;
         for (int i = period; i < this.Count; i++)
         {
            count = 0;
            sum = 0.0f;
            avg = ema[i];
            for (int j = i - period; j <= i; j++)
            {
               count++;
               spread = this.Values[j] - avg;
               sum += spread * spread;
            }
            variance[i] = sum / (float)count;
         }
         return variance;
      }
      public FloatSerie CalculateCovariance(int period, FloatSerie serie)
      {
         if (this.Count != serie.Count) throw new ArgumentOutOfRangeException("Cannot Covariance on series with different size");

         FloatSerie ma1 = this.CalculateMA(period);
         FloatSerie ma2 = serie.CalculateMA(period);
         FloatSerie variance = new FloatSerie(this.Count, "COVAR");
         float avg1, avg2;
         float sum;
         float spread1, spread2;
         int count;
         for (int i = period; i < this.Count; i++)
         {
            count = 0;
            sum = 0.0f;
            avg1 = ma1[i];
            avg2 = ma2[i];
            for (int j = i - period; j <= i; j++)
            {
               count++;
               spread1 = this.Values[j] - avg1;
               spread2 = serie.Values[j] - avg2;
               sum += spread1 * spread2;
            }
            variance[i] = sum / (float)count;
         }
         return variance;
      }

      public FloatSerie CalculateCorrelation(int period, FloatSerie serie)
      {
         if (this.Count != serie.Count) throw new ArgumentOutOfRangeException("Cannot Covariance on series with different size");

         FloatSerie ma1 = this.CalculateMA(period);
         FloatSerie ma2 = serie.CalculateMA(period);

         // Formula is there http://www.socscistatistics.com/tests/pearson/

         FloatSerie correl = new FloatSerie(this.Count, "CORREL");

         for (int i = period; i < this.Count; i++)
         {
            double sum1 = 0.0;
            double sum2 = 0.0;
            double sum3 = 0.0;
            for (int j = i - period; j <= i; j++)
            {
               double diff1 = this[j] - ma1[i];
               double diff2 = this[j] - ma2[i];
               sum1 += diff1 * diff2;
               sum2 += diff1 * diff1;
               sum3 += diff2 * diff2;
            }
            correl[i] = (float)(sum1 / (Math.Sqrt(sum2) * Math.Sqrt(sum3)));
         }

         return correl;
      }

      public FloatSerie CalculateAutoCorrelation(int period)
      {
         FloatSerie correlSerie = new FloatSerie(this.Count, "AUTO_CORREL");
         period = Math.Min(period, this.Count);
         for (int i = period; i < this.Count; i++)
         {
            for (int j = 0; j < period; j++)
            {
               correlSerie[i] += this[i - j] * this[i - j];
            }
         }
         return correlSerie;
      }
      public FloatSerie CalculateMyAutoCovariance(int period, int lag)
      {
         FloatSerie ma1 = this.CalculateMA(period);
         FloatSerie variance = new FloatSerie(this.Count, "AUTOCOVAR");
         float avg1, avg2;
         float sum;
         float spread1, spread2;
         int count;
         int step;
         for (int i = period + lag; i < this.Count; i++)
         {
            count = 0;
            step = 0;
            sum = 0.0f;
            avg1 = ma1[i];
            avg2 = ma1[i - lag];
            for (int j = i - period; j <= i; j++)
            {
               count += ++step;
               spread1 = this.Values[j] * step;
               spread2 = this.Values[j - lag] * step;
               sum += spread1 * spread2;
            }
            variance[i] = sum / (float)count;
         }
         return variance;
      }
      public FloatSerie CalculateAutoCovariance(int period, int lag)
      {
         FloatSerie ma1 = this.CalculateMA(period);
         FloatSerie variance = new FloatSerie(this.Count, "AUTOCOVAR");
         float avg1, avg2;
         float sum;
         float spread1, spread2;
         int count;
         for (int i = period + lag; i < this.Count; i++)
         {
            count = 0;
            sum = 0.0f;
            avg1 = ma1[i];
            avg2 = ma1[i - lag];
            for (int j = i - period; j <= i; j++)
            {
               count++;
               spread1 = this.Values[j] - avg1;
               spread2 = this.Values[j - lag] - avg2;
               sum += spread1 * spread2;
            }
            variance[i] = sum / (float)count;
         }
         return variance;
      }
      public FloatSerie CalculateStdev(int period)
      {
         FloatSerie ema = this.CalculateMA(period);
         FloatSerie stdev = new FloatSerie(this.Count, "STDEV");
         double avg;
         double sum;
         double spread;
         int count;
         for (int i = period; i < this.Count; i++)
         {
            count = 0;
            sum = 0.0f;
            avg = ema[i];
            for (int j = i - period; j <= i; j++)
            {
               count++;
               spread = this.Values[j] - avg;
               sum += spread * spread;
            }
            stdev[i] = (float)Math.Sqrt(sum / (double)count);
         }
         return stdev;
      }
      public float CalculateVariance(int startIndex, int endIndex)
      {
         float avg = this.CalculateAVG(startIndex, endIndex);
         float sum = 0.0f;
         float spread = 0.0f;
         for (int i = startIndex; i <= endIndex; i++)
         {
            spread = this[i] - avg;
            sum += spread * spread;
         }
         return sum / (endIndex - startIndex);
      }
      public float CalculateStdev(int startIndex, int endIndex)
      {
         float avg = this.CalculateAVG(startIndex, endIndex);
         float sum = 0.0f;
         float spread = 0.0f;
         for (int i = startIndex; i <= endIndex; i++)
         {
            spread = this[i] - avg;
            sum += spread * spread;
         }
         return (float)Math.Sqrt(sum / (endIndex - startIndex));
      }
      private void CalculateMedian()
      {
         List<float> list = this.Values.ToList();
         list.Sort();
         int mediumIndex = list.Count / 2;
         int q1Index = list.Count / 4;
         int q3Index = list.Count * 3 / 4;
         if (list.Count % 2 == 0)
         {
            this.median = (list[mediumIndex - 1] + list[mediumIndex]) / 2.0f;
         }
         else
         {
            this.median = list[mediumIndex];
         }
         if (list.Count % 4 == 0)
         {
            this.gamma = (list[q3Index - 1] + list[q3Index] - list[q1Index - 1] - list[q1Index]) / 4.0f;
         }
         else
         {
            this.gamma = (list[q3Index] - list[q1Index]) / 2.0f;
         }
      }
      public float Mean
      {
         get
         {
            if (mean == float.MinValue)
            {
               mean = this.CalculateAVG();
            }
            return mean;
         }
      }
      public float Median
      {
         get
         {
            if (median == float.MinValue)
            {
               this.CalculateMedian();
            }
            return median;
         }
      }
      public float Gamma
      {
         get
         {
            if (gamma == float.MinValue)
            {
               this.CalculateMedian();
            }
            return gamma;
         }
      }
      #endregion
      #endregion

      public float[] Values { get; set; }
      public float this[int index]
      {
         get { return this.Values[index]; }
         set { this.Values[index] = value; }
      }
      public int Count
      {
         get { return this.Values.Count(); }
      }

      #region Constructors
      public FloatSerie(int size)
      {
         this.Values = new float[size];
      }
      public FloatSerie(int size, string name)
      {
         this.Values = new float[size];
         this.Name = name;
      }
      public FloatSerie(float[] values, string name)
      {
         this.Values = values;
         this.Name = name;
      }
      public FloatSerie(float[] values)
      {
         this.Values = values;
      }
      #endregion
      public void Reset(float value)
      {
         for (int i = 0; i < this.Count; i++)
         {
            this[i] = value;
         }
      }
      private void CalculateRSI_U_D(float yesterValue, float currentValue, out float U, out float D, bool useLog)
      {
         if (yesterValue < currentValue)
         {   // Up day
            if (useLog)
            {
               U = (float)(Math.Log10(currentValue / yesterValue));
            }
            else
            {
               U = currentValue - yesterValue;
            }
            D = 0.0f;
         }
         else
         {   // Down day
            U = 0.0f;
            if (useLog)
            {
               D = (float)(Math.Log10(yesterValue / currentValue)); ;
            }
            else
            {
               D = yesterValue - currentValue;
            }
         }
      }
      public FloatSerie CalculateWMA(int maPeriod, FloatSerie weightSerie)
      {
         float[] serie = new float[Values.Count()];
         float cumul = 0.0f;
         float weightCumul = 0.0f;
         for (int i = 0; i < Values.Count(); i++)
         {
            if (i < maPeriod)
            {
               cumul += Values[i] * weightSerie[i];
               weightCumul += weightSerie[i];
               serie[i] = cumul / weightCumul;
            }
            else
            {
               cumul += Values[i] * weightSerie[i] - Values[i - maPeriod] * weightSerie[i - maPeriod];
               weightCumul += weightSerie[i] - weightSerie[i - maPeriod];
               serie[i] = cumul / weightCumul;
            }
            if (weightCumul == 0.0f)
            {
               return null;
            }
         }
         return new FloatSerie(serie, "WMA_" + maPeriod);
      }
      public FloatSerie CalculateMA(int maPeriod)
      {
         float[] serie = new float[Values.Count()];
         float cumul = 0.0f;
         for (int i = 0; i < Values.Count(); i++)
         {
            if (i < maPeriod)
            {
               cumul += Values[i];
               serie[i] = cumul / (i + 1);
            }
            else
            {
               cumul += Values[i] - Values[i - maPeriod];
               serie[i] = cumul / maPeriod;
            }
         }
         return new FloatSerie(serie, "MA_" + maPeriod);
      }
      public FloatSerie CalculateEMA(int emaPeriod)
      {
         FloatSerie serie = new FloatSerie(Values.Count());
         if (emaPeriod <= 1)
         {
            for (int i = 0; i < this.Count; i++)
            {
               serie[i] = this[i];
            }
         }
         else
         {
            float alpha = 2.0f / (float)(emaPeriod + 1);

            serie[0] = Values[0];
            for (int i = 1; i < Values.Count(); i++)
            {
               serie[i] = serie[i - 1] + alpha * (Values[i] - serie[i - 1]);
            }
         }
         serie.Name = "EMA_" + emaPeriod.ToString();
         return serie;
      }
      public FloatSerie CalculateKEMA(int fastPeriod, int slowPeriod)
      {
         FloatSerie serie = new FloatSerie(Values.Count());
         FloatSerie erSerie = CalculateER((fastPeriod + slowPeriod)/2);
         erSerie = erSerie.Abs(); // Calculate square

         serie[0] = Values[0];
         for (int i = 1; i < Values.Count(); i++)
         {
            int period = (int)((erSerie[i]) * (slowPeriod - fastPeriod)) + fastPeriod;

            float alpha = 2.0f / (float)(period + 1);
            serie[i] = serie[i - 1] + alpha * (Values[i] - serie[i - 1]);
         }
         serie.Name = "KEMA_" + slowPeriod + "_" + fastPeriod;
         return serie;
      }
      public FloatSerie CalculateER(int period)
      {
         float volatility = 0;
         float direction;
         FloatSerie erSerie = new FloatSerie(this.Count, "ER");

         int i = 0;
         for (i = 1; i <= period; i++)
         {
            volatility += Math.Abs(this[i] - this[i - 1]);
         }
         for (i = period + 1; i < this.Count; i++)
         {
            volatility += Math.Abs(this[i] - this[i - 1]) - Math.Abs(this[i - period] - this[i - period - 1]);
            direction = this[i] - this[i - period];
            erSerie[i] = direction / volatility;
         }

         return erSerie;
      }
      public FloatSerie ShiftForward(int length)
      {
         FloatSerie newSerie = new FloatSerie(this.Count);

         for (int i = length; i < this.Count; i++)
         {
            newSerie[i] = this[i - length];
         }

         return newSerie;
      }
      public FloatSerie CalculateRSI(int rsiPeriod, bool useLog)
      {
         float[] serie = new float[Values.Count()];
         float alphaEMA_RSI = 2.0f / (float)(rsiPeriod + 1);
         float RSI_EMA_U = 0.0f;
         float RSI_EMA_D = 0.0f;
         float U, D;

         int maxIndex = Math.Min(rsiPeriod, Values.Count() - 1);
         serie[0] = 50.0f;

         for (int i = 1; i < Values.Count(); i++)
         {
            CalculateRSI_U_D(Values[i - 1], Values[i], out U, out D, useLog);

            // Calculate EMA for U and D 
            RSI_EMA_U = RSI_EMA_U + alphaEMA_RSI * (U - RSI_EMA_U);
            RSI_EMA_D = RSI_EMA_D + alphaEMA_RSI * (D - RSI_EMA_D);

            // Calculate RSI
            if (RSI_EMA_U == 0.0f)
            {
               serie[i] = 50.0f;
            }
            else
            {
               if ((RSI_EMA_U + RSI_EMA_D) != 0)
               {
                  serie[i] = 100.0f * RSI_EMA_U / (RSI_EMA_U + RSI_EMA_D);
               }
               else
               {
                  serie[i] = serie[i - 1];
               }
            }

            // Camp in case of first values
            if (i < rsiPeriod)
            {
               serie[i] = Math.Max(30, serie[i]);
               serie[i] = Math.Min(70, serie[i]);
            }
         }
         return new FloatSerie(serie, "RSI_" + rsiPeriod);
      }
      public FloatSerie CalculateStochastik(int period, int smoothing)
      {
         float[] serie = new float[Values.Count()];
         float alpha = 2.0f / (float)(smoothing + 1);

         float min = 0, max = 0, stock = 0;
         for (int i = 1; i < this.Count; i++)
         {
            if (i < period)
            {
               this.GetMinMax(0, i, ref min, ref max);
            }
            else
            {
               this.GetMinMax(i - period, i, ref min, ref max);
            }

            if (max != min)
            {
               stock = 2.0f * ((this[i] - min) / (max - min) - 0.5f);
            }
            else
            {
               stock = 0;
            }

            serie[i] = serie[i - 1] + alpha * (stock - serie[i - 1]);
         }

         return new FloatSerie(serie, "STOC_" + period);
      }
      public FloatSerie CalculateFisher(int smoothing)
      {
         float[] serie = new float[Values.Count()];
         float alpha = 2.0f / (float)(smoothing + 1);

         double fisher = 0, val;
         for (int i = 1; i < this.Count; i++)
         {
            val = Math.Min(0.9999, Math.Max(-0.9999, this[i]));

            fisher = Math.Log((1.0f + val) / (1.0f - val));

            serie[i] = serie[i - 1] + alpha * ((float)fisher - serie[i - 1]);
         }

         return new FloatSerie(serie, "FISHER_" + smoothing);
      }
      public FloatSerie CalculateFisherInv(int smoothing)
      {
         FloatSerie normalisedSerie = this.Normalise(-1.0f, 1.0f);
         float[] serie = new float[Values.Count()];
         float alpha = 2.0f / (float)(smoothing + 1);

         double fisherInv = 0;
         for (int i = 1; i < this.Count; i++)
         {

            fisherInv = (Math.Exp(2.0 * normalisedSerie[i]) - 1.0) / (Math.Exp(2.0 * normalisedSerie[i]) + 1.0);

            serie[i] = serie[i - 1] + alpha * ((float)fisherInv - serie[i - 1]);
         }

         return new FloatSerie(serie, "FISHER_" + smoothing);
      }

      public FloatSerie CalculateSigmoid(float max, float lambda)
      {
         float[] serie = new float[Values.Count()];

         float sigmoid = 0;
         for (int i = 1; i < this.Count; i++)
         {
            sigmoid = (float)(-max + 2 * max / (1.0 + Math.Exp(-lambda * this[i])));

            serie[i] = sigmoid;
         }

         return new FloatSerie(serie, "SIGMOID");
      }

      public FloatSerie CalculateCorrelation(FloatSerie otherSerie, int period)
      {
         if (this.Count != otherSerie.Count)
         {
            return null;
         }
         FloatSerie correlationSerie = new FloatSerie(this.Count, "CORREL");
         float[] x = this.Values;
         float[] y = otherSerie.Values;
         float[] mx = this.CalculateEMA(period).Values;
         float[] my = otherSerie.CalculateEMA(period).Values;

         double sum1 = 0, sum2 = 0, sum3 = 0, xmx = 0, ymy = 0;
         correlationSerie[0] = 0;

         for (int i = 1; i < this.Count; i++)
         {
            sum1 = 0; sum2 = 0; sum3 = 0;
            for (int j = Math.Max(0, i - period); j <= i; j++)
            {
               xmx = x[j] - mx[j];
               ymy = y[j] - my[j];
               sum1 += xmx * ymy;
               sum2 += xmx * xmx;
               sum3 += ymy * ymy;
            }

            correlationSerie[i] = (float)(sum1 / (Math.Sqrt(sum2) * Math.Sqrt(sum3)));
         }
         return correlationSerie;
      }
      public FloatSerie CalculateRelativeTrend()
      {
         float[] trend = new float[this.Values.Count()];
         trend[0] = 0.0f;
         for (int i = 1; i < this.Values.Count(); i++)
         {
            if (Math.Abs(this.Values[i]) <= 0.00001f)
            { trend[i] = 0.0f; }
            else
            { trend[i] = (this.Values[i] - this.Values[i - 1]) / this.Values[i]; }
            if (float.IsNaN(trend[i]))
            {
               StockLog.Write("NaN");
            }
         }
         return new FloatSerie(trend, "TREND");
      }
      public FloatSerie CalculateAbsoluteTrend()
      {
         float[] trend = new float[this.Values.Count()];
         trend[0] = 0.0f;
         for (int i = 1; i < this.Values.Count(); i++)
         {
            if (Math.Abs(this.Values[i]) <= 0.00001f)
            { trend[i] = 0.0f; }
            else
            { trend[i] = (this.Values[i] - this.Values[i - 1]); }
            if (float.IsNaN(trend[i]))
            {
               StockLog.Write("NaN");
            }
         }
         return new FloatSerie(trend, "TREND");
      }

      public void CalculateBB(FloatSerie referenceAverage, int bbTimePeriod, float BBUpCoef, float BBDownCoef, ref FloatSerie bbUpSerie, ref FloatSerie bbDownSerie)
      {
         float squareSum = 0.0f;
         float tmp = 0.0f;
         float upBB = 0.0f;
         float downBB = 0.0f;
         float referenceAverageVal = 0.0f;

         bbUpSerie = new FloatSerie(this.Values.Count());
         bbUpSerie.Name = "BBUp";
         bbDownSerie = new FloatSerie(this.Values.Count());
         bbDownSerie.Name = "BBDown";

         for (int i = 0; i < this.Values.Count(); i++)
         {
            referenceAverageVal = referenceAverage.Values[i];
            if (i < bbTimePeriod)
            {
               // Calculate BB
               if (i == 0)
               {
                  upBB = 0.0f;
                  downBB = 0.0f;
               }
               else
               {
                  squareSum = 0.0f;
                  for (int j = 0; j <= i; j++)
                  {
                     tmp = this.Values[j] - referenceAverageVal;
                     squareSum += tmp * tmp;
                  }
                  tmp = (float)Math.Sqrt(squareSum / (double)(i + 1));
                  upBB = BBUpCoef * tmp;
                  downBB = BBDownCoef * tmp;
               }
            }
            else
            {
               squareSum = 0.0f;
               for (int j = i - bbTimePeriod + 1; j <= i; j++)
               {
                  tmp = this.Values[j] - referenceAverageVal;
                  squareSum += tmp * tmp;
               }
               tmp = (float)Math.Sqrt(squareSum / (double)bbTimePeriod);
               upBB = BBUpCoef * tmp;
               downBB = BBDownCoef * tmp;
            }
            if (bbUpSerie != null) { bbUpSerie.Values[i] = referenceAverageVal + upBB; }
            if (bbDownSerie != null) { bbDownSerie.Values[i] = referenceAverageVal + downBB; }
         }
      }
      public FloatSerie CalculateEC(int emaPeriod, float gain)
      {
         FloatSerie serie = new FloatSerie(Values.Count(), "EMA_" + emaPeriod.ToString());
         if (emaPeriod <= 1)
         {
            for (int i = 0; i < this.Count; i++)
            {
               serie[i] = this[i];
            }
         }
         else
         {
            float alpha = 2.0f / (float)(emaPeriod + 1);

            // Tradestation code: EC = a*(Price + gain*(Price – EC[1])) + (1 – a)*EC[1];

            serie[0] = Values[0];
            for (int i = 1; i < Values.Count(); i++)
            {
               serie[i] = alpha * (Values[i] + gain * (Values[i] - serie[i - 1])) + (1 - alpha) * serie[i - 1];
            }
         }
         return serie;
      }
      static public float CalculateNextEMA(int emaPeriod, float previousEMA, float currentValue)
      {
         float alpha = 2.0f / (float)(emaPeriod + 1);
         return previousEMA + alpha * (currentValue - previousEMA);
      }
      static public float CalculateValueNextEMA(int emaPeriod, float currentEMA, float requiredNextEMA)
      {
         float alpha = 2.0f / (float)(emaPeriod + 1);
         return (requiredNextEMA - currentEMA) / alpha + currentEMA;
      }
      public float CalculateValueNextMA(int maPeriod, int index, float previousMA, float requiredNextMA)
      {
         if (index <= maPeriod)
         {
            return Values.ElementAt(index);
         }
         else
         {
            float requiredMA = maPeriod * (requiredNextMA - previousMA) + this.Values[index - maPeriod - 1];
            return requiredMA;
         }
      }
      #region ARITHMETIC CALCULUS
      public static FloatSerie operator +(FloatSerie s1, FloatSerie s2)
      {
         return s1.Add(s2);
      }
      public static FloatSerie operator +(FloatSerie s1, float a)
      {
         return s1.Add(a);
      }
      public static FloatSerie operator -(FloatSerie s1, FloatSerie s2)
      {
         return s1.Sub(s2);
      }
      public static FloatSerie operator -(FloatSerie s1, float a)
      {
         return s1.Sub(a);
      }
      public static FloatSerie operator *(FloatSerie s1, FloatSerie s2)
      {
         return s1.Mult(s2);
      }
      public static FloatSerie operator *(FloatSerie s1, float a)
      {
         return s1.Mult(a);
      }
      public static FloatSerie operator *(float a, FloatSerie s1)
      {
         return s1.Mult(a);
      }
      public static FloatSerie operator /(FloatSerie s1, FloatSerie s2)
      {
         return s1.Div(s2);
      }
      public static FloatSerie operator /(FloatSerie s1, float a)
      {
         return s1.Div(a);
      }
      public FloatSerie Abs()
      {
         float[] absSerie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            absSerie[i] = Math.Abs(this[i]);
         }
         return new FloatSerie(absSerie, this.Name);
      }
      public FloatSerie Square()
      {
         float[] squareSerie = new float[this.Count];
         float value;
         for (int i = 0; i < this.Values.Count(); i++)
         {
            value = this[i];
            squareSerie[i] = value * value;
         }
         return new FloatSerie(squareSerie);
      }
      public FloatSerie SquareSigned()
      {
         float[] squareSerie = new float[this.Count];
         float value;
         for (int i = 0; i < this.Values.Count(); i++)
         {
            value = this[i];

            squareSerie[i] = value >= 0 ? value * value : -value * value;
         }
         return new FloatSerie(squareSerie);
      }
      public FloatSerie Sqrt()
      {
         float[] sqrtSerie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            sqrtSerie[i] = (float)Math.Sqrt(this[i]);
         }
         return new FloatSerie(sqrtSerie);
      }
      public FloatSerie Sub(FloatSerie sub)
      {
         float[] serie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = Values[i] - sub.Values[i];
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Add(FloatSerie add)
      {
         float[] serie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = Values[i] + add.Values[i];
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Add(float add)
      {
         float[] serie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = Values[i] + add;
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Sub(float sub)
      {
         float[] serie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = Values[i] - sub;
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Mult(FloatSerie serie)
      {
         float[] multSerie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            multSerie[i] = Values[i] * serie.Values[i];
         }
         return new FloatSerie(multSerie);
      }
      public FloatSerie Mult(float mult)
      {
         float[] serie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = Values[i] * mult;
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Div(FloatSerie serie)
      {
         float[] divSerie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            if (serie.Values[i] == 0.0f)
            {
               divSerie[i] = Values[i] / 0.0001f;
            }
            else
            {
               divSerie[i] = Values[i] / serie.Values[i];
            }
         }
         return new FloatSerie(divSerie);
      }
      public FloatSerie Div(float div)
      {
         if (div == 0.0f)
         {
            throw new DivideByZeroException();
         }
         float[] divSerie = new float[this.Values.Count()];

         for (int i = 0; i < this.Values.Count(); i++)
         {
            divSerie[i] = Values[i] / div;
         }
         return new FloatSerie(divSerie);
      }
      public FloatSerie Cumul()
      {
         float[] serie = new float[this.Values.Count()];
         float cumul = 0.0f;
         int i = 0;
         foreach (float value in Values)
         {
            cumul += value;
            serie[i++] = cumul;
         }
         return new FloatSerie(serie);
      }
      public FloatSerie Cumul(int period)
      {
         float[] serie = new float[this.Values.Count()];
         float cumul = 0.0f;

         serie[0] = this[0];
         for (int i = 1; i < period; i++)
         {
            cumul = 0.0f;
            for (int j = 0; j <= i; j++)
            {
               cumul += this[j];
            }
            serie[i] = cumul;
         }

         for (int i = period; i < this.Count; i++)
         {
            cumul = 0.0f;
            for (int j = i - period; j <= i; j++)
            {
               cumul += this[j];
            }
            serie[i] = cumul;
         }

         return new FloatSerie(serie);
      }
      public FloatSerie Normalise(float min, float max)
      {
         float[] serie = new float[this.Values.Count()];
         float minValue = float.MaxValue;
         float maxValue = float.MinValue;
         this.GetMinMax(ref minValue, ref maxValue);

         float coef = (max - min) / (maxValue - minValue);

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = min + (Values[i] - minValue) * coef;
         }

         return new FloatSerie(serie);
      }
      public FloatSerie Normalise(float minValue, float maxValue, float min, float max)
      {
         float[] serie = new float[this.Values.Count()];

         float coef = (max - min) / (maxValue - minValue);

         for (int i = 0; i < this.Values.Count(); i++)
         {
            serie[i] = min + (Values[i] - minValue) * coef;
         }

         return new FloatSerie(serie);
      }
      #endregion

      public FloatSerie ApplySmoothing(StockMathToolkit.SmoothingType smoothingType, float scale)
      {
         FloatSerie serie = new FloatSerie(this.Count);
         float width = this.Max - this.Min;
         StockMathToolkit.SmoothingFunction smoothingFunction = StockMathToolkit.GetSmoothingFunction(smoothingType);
         int i = 0;
         foreach (float value in this.Values)
         {
            serie[i++] = smoothingFunction(value, width, scale);
         }
         return serie;
      }
      public FloatSerie ApplySmoothing(StockMathToolkit.SmoothingType smoothingType, float inputWidth, float scale)
      {
         FloatSerie serie = new FloatSerie(this.Count);
         StockMathToolkit.SmoothingFunction smoothingFunction = StockMathToolkit.GetSmoothingFunction(smoothingType);
         int i = 0;
         foreach (float value in this.Values)
         {
            serie[i++] = smoothingFunction(value, inputWidth, scale);
         }
         return serie;
      }
      public FloatSerie Clamp(float min, float max)
      {
         float[] serie = new float[this.Values.Count()];
         for (int i = 0; i < this.Values.Count(); i++)
         {
            if (this.Values[i] < min)
            {
               serie[i] = min;
            }
            else if (this.Values[i] > max)
            {
               serie[i] = max;
            }
            else
            {
               serie[i] = this.Values[i];
            }
         }

         return new FloatSerie(serie);
      }

      public float Last { get { return this.Values.Last(); } }
      public float LastNonNaN
      {
         get
         {
            for (int i = this.Values.Count() - 1; i >= 1; i--)
            {
               if (!float.IsNaN(this[i])) return this[i];
            }
            return float.NaN;
         }
      }
      public int LastIndex { get { return this.Values.Count() - 1; } }


      #region MIN_MAX Functions
      private float GetMin()
      {
         float minValue = float.MaxValue;

         foreach (float currentValue in Values)
         {
            if (minValue > currentValue) minValue = currentValue;
         }
         return minValue;
      }
      public float GetMin(int startIndex, int endIndex)
      {
         float minValue = float.MaxValue;

         for (int i = startIndex; i <= endIndex; i++)
         {
            if (float.IsNaN(Values[i]))
            {
               continue;
            }
            if (minValue > Values[i]) minValue = Values[i];
         }
         return minValue;
      }
      private float GetMax()
      {
         float maxValue = float.MinValue;

         foreach (float currentValue in Values)
         {
            if (maxValue < currentValue) maxValue = currentValue;
         }
         return maxValue;
      }
      public float GetMax(int startIndex, int endIndex)
      {
         float maxValue = float.MinValue;

         for (int i = startIndex; i <= endIndex; i++)
         {
            if (float.IsNaN(Values[i]))
            {
               continue;
            }
            if (maxValue < Values[i]) maxValue = Values[i];
         }
         return maxValue;
      }
      public void GetMinMax(ref float minValue, ref float maxValue)
      {
         minValue = float.MaxValue;
         maxValue = float.MinValue;

         foreach (float currentValue in Values)
         {
            if (!float.IsNaN(currentValue))
            {
               if (minValue > currentValue) minValue = currentValue;
               if (maxValue < currentValue) maxValue = currentValue;
            }
         }
      }
      public void GetMinMax(int startIndex, int endIndex, ref float minValue, ref float maxValue)
      {
         minValue = float.MaxValue;
         maxValue = float.MinValue;

         float currentValue = 0.0f;
         for (int i = startIndex; i <= endIndex; i++)
         {
            currentValue = Values[i];
            if (!float.IsNaN(currentValue))
            {
               if (minValue > currentValue) minValue = currentValue;
               if (maxValue < currentValue) maxValue = currentValue;
            }
         }
      }
      #endregion
      #region TOP AND BOTTOMS
      public bool IsTop(int index)
      {
         if (index > 0 && index < this.Count - 1)
         {
            float value = this[index];
            return this[index - 1] <= value && value > this[index + 1];
         }
         return false;
      }
      public bool IsBottom(int index)
      {
         if (index > 0 && index < this.Count - 1)
         {
            float value = this[index];
            return this[index - 1] >= value && value < this[index + 1];
         }
         return false;
      }
      public bool IsTopIsh(int index)
      {
         if (index > 0 && index < this.Count - 1)
         {
            float value = this[index];
            if (this[index - 1] < value && value > this[index + 1]) return true;
            if (this[index - 1] == value && value > this[index + 1])
            {
               int j = index - 2;
               while (j >= 0)
               {
                  if (this[j] < value) return true;
                  if (this[j] > value) return false;
                  j--;
               }
            }
         }
         return false;
      }
      public bool IsBottomIsh(int index)
      {
         if (index > 0 && index < this.Count - 1)
         {
            float value = this[index];
            if (this[index - 1] > value && value < this[index + 1]) return true;
            if (this[index - 1] == value && value < this[index + 1])
            {
               int j = index - 2;
               while (j >= 0)
               {
                  if (this[j] > value) return true;
                  if (this[j] < value) return false;
                  j--;
               }
            }
         }
         return false;
      }
      public int FindMinIndex(int startIndex, int endIndex)
      {
         int minIndex = startIndex;
         float min = this[startIndex];
         for (int i = startIndex + 1; i <= endIndex; i++)
         {
            if (this[i] < min)
            {
               minIndex = i;
               min = this[i];
            }
         }
         return minIndex;
      }
      public int FindMaxIndex(int startIndex, int endIndex)
      {
         int maxIndex = startIndex;
         float max = this[startIndex];
         for (int i = startIndex + 1; i <= endIndex; i++)
         {
            if (this[i] > max)
            {
               maxIndex = i;
               max = this[i];
            }
         }
         return maxIndex;
      }
      public int FindMinIndex(int startIndex, int endIndex, float limit)
      {
         int minIndex = -1;
         float min = limit;
         for (int i = startIndex + 1; i <= endIndex; i++)
         {
            if (this[i] < min)
            {
               minIndex = i;
               min = this[i];
            }
         }
         return minIndex;
      }
      public int FindMaxIndex(int startIndex, int endIndex, float limit)
      {
         int maxIndex = -1;
         float max = limit;
         for (int i = startIndex + 1; i <= endIndex; i++)
         {
            if (this[i] > max)
            {
               maxIndex = i;
               max = this[i];
            }
         }
         return maxIndex;
      }
      public bool IsHighestInDays(int index, int days)
      {
         if (index == 0) { return true; }
         bool result = true;
         int stopIndex = Math.Max(0, index - days);
         for (int i = index - 1; i >= stopIndex; i--)
         {
            if (this[i] > this[index])
            {
               result = false;
               break;
            }
         }
         return result;
      }
      public bool IsLowestInDays(int index, int days)
      {
         if (index == 0) { return true; }
         bool result = true;
         int stopIndex = Math.Max(0, index - days);
         for (int i = index - 1; i >= stopIndex; i--)
         {
            if (this[i] < this[index])
            {
               result = false;
               break;
            }
         }
         return result;
      }
      public BoolSerie GetHighPivotSerie(int leftStrength, int rightStrength)
      {
         BoolSerie pivotSerie = new BoolSerie(this.Count, "TopPivot");
         float pivotValue;
         bool isPivotCandidate;
         for (int i = leftStrength; i < (this.Count - rightStrength); i++)
         {
            isPivotCandidate = IsHighestInDays(i, leftStrength);
            if (isPivotCandidate)
            {
               pivotValue = this[i];
               for (int j = i + 1; j <= i + rightStrength; j++)
               {
                  if (this[j] > pivotValue)
                  {
                     isPivotCandidate = false;
                     break;
                  }
               }
            }
            pivotSerie[i] = isPivotCandidate;
         }
         return pivotSerie;
      }
      public BoolSerie GetLowPivotSerie(int leftStrength, int rightStrength)
      {
         BoolSerie pivotSerie = new BoolSerie(this.Count, "BottomPivot");
         float pivotValue;
         bool isPivotCandidate;
         for (int i = leftStrength; i < (this.Count - rightStrength); i++)
         {
            isPivotCandidate = IsLowestInDays(i, leftStrength);
            if (isPivotCandidate)
            {
               pivotValue = this[i];
               for (int j = i + 1; j <= i + rightStrength; j++)
               {
                  if (this[j] < pivotValue)
                  {
                     isPivotCandidate = false;
                     break;
                  }
               }
            }
            pivotSerie[i] = isPivotCandidate;
         }
         return pivotSerie;
      }
      #endregion
      #region statistic function
      #endregion

      public FloatSerie CalculateVolatility(int period)
      {
         FloatSerie volatilitySerie = new FloatSerie(this.Count, "VLTY");
         int periodOffset = period - 1;
         for (int i = periodOffset; i < this.Count; i++)
         {
            volatilitySerie[i] = this.CalculateStdev(i - periodOffset, i);
         }
         for (int i = 0; i < periodOffset; i++)
         {
            volatilitySerie[i] = volatilitySerie[periodOffset];
         }

         return volatilitySerie;
      }

      public FloatSerie CalculateHMA(int period)
      {
         FloatSerie EMASerie1 = this.CalculateEMA(period / 2);
         FloatSerie EMASerie2 = this.CalculateEMA(period);

         return ((EMASerie1 * 2.0f) - EMASerie2).CalculateEMA((int)Math.Sqrt(period));
      }

      public FloatSerie CalculateEA(int period)
      {
         FloatSerie serie = new FloatSerie(Values.Count());
         if (period <= 1)
         {
            for (int i = 0; i < this.Count; i++)
            {
               serie[i] = this[i];
            }
         }
         else
         {
            float α = 2.0f / (float)(period + 1);
            float α2 = α * α;
            float c1 = α - (α / 2) * (α / 2);
            float c2 = α2 / 2;
            float c3 = α - 3 * α2 / 4;
            float c4 = 2 * (1 - α);
            float c5 = (1 - α) * (1 - α);

            serie[0] = Values[0];
            serie[1] = (Values[1] + Values[0]) / 2.0f;

            // InstTrend = (α - (α/2)2) * Price + (α2/2) * Price[1] - (α - 3α2/4) * Price[2]) 
            //           + 2 * (1 - α) * InstTrend[1] - (1 - α)2 * InstTrend[2];

            for (int i = 2; i < Values.Count(); i++)
            {
               serie[i] = c1 * Values[i] + c2 * Values[i - 1] - c3 * Values[i - 2] + c4 * serie[i - 1] - c5 * serie[i - 2];
            }
         }
         serie.Name = "EA_" + period.ToString();
         return serie;
      }


      internal FloatSerie CalculateSARTrail(float accelerationFactorInit, float accelerationFactorStep)
      {
         FloatSerie trailSerie = new FloatSerie(this.Count, "TRAILHL");

         float accelerationFactorMax = float.MaxValue;

         float accelerationFactor = accelerationFactorInit;
         bool isUpTrend = true;
         float previousExtremum = this[0];
         float previousSAR = previousExtremum * 0.99f;
         trailSerie[0] = previousSAR;

         for (int i = 1; i < this.Values.Count(); i++)
         {
            if (isUpTrend)
            {
               if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= this[i])
               {
                  isUpTrend = false;
                  accelerationFactor = accelerationFactorInit;
                  previousSAR = previousExtremum;
                  trailSerie[i] = previousSAR;
               }
               else
               {
                  if (this[i] > previousExtremum)
                  {
                     accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                     previousExtremum = this[i];
                  }
                  previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                  trailSerie[i] = previousSAR;
               }
            }
            else
            {
               if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= this[i])
               {
                  isUpTrend = true;
                  accelerationFactor = accelerationFactorInit;
                  previousSAR = previousExtremum;
                  trailSerie[i] = previousSAR;
               }
               else
               {
                  if (this[i] < previousExtremum)
                  {
                     accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                     previousExtremum = this[i];
                  }
                  previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                  trailSerie[i] = previousSAR;
               }
            }
         }
         return trailSerie;
      }
      public FloatSerie CalculateHLTrail(int period)
      {
         FloatSerie trailSerie = new FloatSerie(this.Count, "TRAILHL");

         trailSerie[0] = this[0];

         // Initialise until period is reached
         bool upTrend = this[period - 1] > this[0];
         float min = float.MaxValue, max = float.MinValue;
         this.GetMinMax(0, period - 1, ref min, ref max);

         for (int i = 1; i < period; i++)
         {
            trailSerie[i] = upTrend ? min : max;
         }

         for (int i = period; i < this.Count; i++)
         {
            if (upTrend)
            {
               if (this[i] < trailSerie[i - 1]) // upTrend broken
               {
                  upTrend = false;
                  trailSerie[i] = this.GetMax(i - period, i - 1);
               }
               else // Uptrend continues
               {
                  if (this[i] > this[i - 1])
                  {
                     trailSerie[i] = Math.Max(trailSerie[i - 1], this.GetMin(i - period, i - 1));
                  }
                  else
                  {
                     trailSerie[i] = trailSerie[i - 1];
                  }
               }
            }
            else
            {
               if (this[i] > trailSerie[i - 1]) // downTrend broken
               {
                  upTrend = true;
                  trailSerie[i] = this.GetMin(i - period, i - 1);
               }
               else // downTrend continues
               {
                  if (this[i] < this[i - 1])
                  {
                     trailSerie[i] = Math.Min(trailSerie[i - 1], this.GetMax(i - period, i - 1));
                  }
                  else
                  {
                     trailSerie[i] = trailSerie[i - 1];
                  }
               }
            }
         }
         return trailSerie;
      }
      public FloatSerie CalculateHLEXTrail(int period)
      {
         FloatSerie trailSerie = new FloatSerie(this.Count, "TRAIL");

         float weight = period - 1;
         float div = period;

         trailSerie[0] = this[0];

         // Initialise until period is reached
         bool upTrend = this[period - 1] > this[0];
         float min = float.MaxValue, max = float.MinValue;
         this.GetMinMax(0, period - 1, ref min, ref max);

         for (int i = 1; i < period; i++)
         {
            trailSerie[i] = upTrend ? min : max;
         }

         for (int i = period; i < this.Count; i++)
         {
            if (upTrend)
            {
               if (this[i] < trailSerie[i - 1]) // upTrend broken
               {
                  upTrend = false;
                  trailSerie[i] = this.GetMax(i - period, i - 1);
               }
               else // Uptrend continues
               {
                  if (this[i] > this[i - 1])
                  {
                     trailSerie[i] = (weight * trailSerie[i - 1] + this.GetMax(i - period, i - 1)) / div;
                  }
                  else
                  {
                     trailSerie[i] = (weight * trailSerie[i - 1] + this.GetMin(i - period, i - 1)) / div;
                  }
               }
            }
            else
            {
               if (this[i] > trailSerie[i - 1]) // downTrend broken
               {
                  upTrend = true;
                  trailSerie[i] = this.GetMin(i - period, i - 1);
               }
               else // downTrend continues
               {
                  if (this[i] < this[i - 1])
                  {
                     trailSerie[i] = (weight * trailSerie[i - 1] + this.GetMin(i - period, i - 1)) / div;
                  }
                  else
                  {
                     trailSerie[i] = (weight * trailSerie[i - 1] + this.GetMax(i - period, i - 1)) / div;
                  }
               }
            }
         }
         return trailSerie;
      }
   }
}
