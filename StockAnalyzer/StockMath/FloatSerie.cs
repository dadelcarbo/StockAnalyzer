using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockMath
{
    public class FloatSerie : IEnumerable<float>
    {
        public string Name { get; set; }

        #region STATISTICAL FUNCTIONS
        public float CalculateAVG(int startIndex, int endIndex)
        {
            float sum = 0.0f;
            for (int i = startIndex; i <= endIndex; i++)
            {
                sum += this[i];
            }
            return sum / (endIndex - startIndex);
        }

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
                for (int j = i - period + 1; j <= i; j++)
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
                    double diff2 = serie[j] - ma2[i];
                    sum1 += diff1 * diff2;
                    sum2 += diff1 * diff1;
                    sum3 += diff2 * diff2;
                }
                correl[i] = (float)(sum1 / (Math.Sqrt(sum2) * Math.Sqrt(sum3)));
            }

            return correl;
        }

        /// <summary>
        /// Self pearson correlation applied with a shift
        /// https://en.wikipedia.org/wiki/Pearson_correlation_coefficient
        /// <summary>   
        /// <remarks><img src="https://wikimedia.org/api/rest_v1/media/math/render/svg/bd1ccc2979b0fd1c1aec96e386f686ae874f9ec0"/></remarks>
        /// </summary>
        /// </summary>
        /// <param name="period"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public FloatSerie CalculateAutoCorrelation(int period, int shift)
        {
            var emaSerie = this.CalculateMA(period);

            FloatSerie correlSerie = new FloatSerie(this.Count, "AUTO_CORREL");

            float diff1, diff2;

            for (int i = period + shift; i < this.Count; i++)
            {
                float ssum1 = 0, sum1 = 0, ssum2 = 0;

                float avg1 = 0, avg2 = 0;
                for (int j = 0; j < period; j++)
                {
                    int index1 = i - j;
                    int index2 = index1 - shift;
                    avg1 += this.Values[index1];
                    avg2 += this.Values[index2];
                }
                avg1 /= (float)period;
                avg2 /= (float)period;

                for (int j = 0; j < period; j++)
                {
                    int index1 = i - j;
                    int index2 = index1 - shift;

                    diff1 = this.Values[index1] - avg1;
                    diff2 = this.Values[index2] - avg2;

                    sum1 += diff1 * diff2;

                    ssum1 += diff1 * diff1;
                    ssum2 += diff2 * diff2;
                }
                correlSerie[i] = sum1 / (float)(Math.Sqrt(ssum1) * Math.Sqrt(ssum2));
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
        public FloatSerie(int size, string name, float val)
        {
            this.Values = new float[size];
            this.Name = name;
            for (int i = 0; i < size; i++)
            {
                this.Values[i] = val;
            }
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
        public FloatSerie(IEnumerable<float> values)
        {
            this.Values = values.ToArray();
        }
        #endregion
        public void Reset(float value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i] = value;
            }
        }

        public FloatSerie CalculateDerivative()
        {
            var derivative = new FloatSerie(this.Count);
            for (int i = 1; i < this.Count; i++)
            {
                derivative[i] = (this[i] - this[i - 1]) / this[i - 1];
            }
            return derivative;
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
                if (volatility == 0) { erSerie[i] = 0; }
                else
                {
                    direction = this[i] - this[i - period];
                    erSerie[i] = direction / volatility;
                }
            }

            return erSerie;
        }
        public FloatSerie ShiftForward(int length)
        {
            FloatSerie newSerie = new FloatSerie(this.Count);

            for (int i = 0; i < length; i++)
            {
                newSerie[i] = this[0];
            }
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
                if ((RSI_EMA_U + RSI_EMA_D) != 0)
                {
                    serie[i] = 100.0f * RSI_EMA_U / (RSI_EMA_U + RSI_EMA_D);
                }
                else
                {
                    serie[i] = serie[i - 1];
                }

                // Clamp in case of first values
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

            return new FloatSerie(serie, "STOK_" + period);
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
        public void CalculateBBEX(FloatSerie referenceAverage, int bbTimePeriod, float BBUpCoef, float BBDownCoef, ref FloatSerie bbUpSerie, ref FloatSerie bbDownSerie)
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

                    float tt = tmp / referenceAverageVal;

                    upBB = ((referenceAverageVal * (1.0f + tt)) - referenceAverageVal) * BBUpCoef;
                    downBB = -((referenceAverageVal / (1.0f + tt)) - referenceAverageVal) * BBDownCoef;
                }
                if (bbUpSerie != null) { bbUpSerie.Values[i] = referenceAverageVal + upBB; }
                if (bbDownSerie != null) { bbDownSerie.Values[i] = referenceAverageVal + downBB; }
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
        public FloatSerie Sign()
        {
            float[] signSerie = new float[this.Values.Count()];

            for (int i = 0; i < this.Values.Count(); i++)
            {
                signSerie[i] = this[i] >= 0 ? 1.0f : -1.0f;
            }
            return new FloatSerie(signSerie, this.Name + "_Sign");
        }
        /// <summary>
        /// Calculate Power of 2
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Calculate Power of 2 but keep the original sign
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Calculate the Power series
        /// </summary>
        /// <returns></returns>
        public FloatSerie Pow(double power)
        {
            float[] sqrtSerie = new float[this.Values.Count()];

            for (int i = 0; i < this.Values.Count(); i++)
            {
                sqrtSerie[i] = (float)Math.Pow(this[i], power);
            }
            return new FloatSerie(sqrtSerie);
        }
        /// <summary>
        /// Calculate the square root series
        /// </summary>
        /// <returns></returns>
        public FloatSerie Sqrt()
        {
            float[] sqrtSerie = new float[this.Values.Count()];

            for (int i = 0; i < this.Values.Count(); i++)
            {
                sqrtSerie[i] = (float)Math.Sqrt(this[i]);
            }
            return new FloatSerie(sqrtSerie);
        }
        /// <summary>
        /// Calculate the square root series
        /// </summary>
        /// <returns></returns>
        public FloatSerie SqrtSigned()
        {
            float[] sqrtSerie = new float[this.Values.Count()];

            for (int i = 0; i < this.Values.Count(); i++)
            {
                sqrtSerie[i] = this[i] >= 0 ? (float)Math.Sqrt(this[i]) : (float)Math.Sqrt(-this[i]);
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
        public FloatSerie Log10()
        {
            float[] serie = new float[this.Values.Count()];

            for (int i = 0; i < this.Values.Count(); i++)
            {
                serie[i] = (float)(Math.Log10(Values[i]));
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

        public static FloatSerie MaxSerie(FloatSerie serie1, FloatSerie serie2)
        {
            if (serie1.Count != serie2.Count) throw new InvalidOperationException("Cannot get maximun serie as size in different");

            float[] serie = new float[serie1.Count];

            for (int i = 0; i < serie1.Count; i++)
            {
                serie[i] = Math.Max(serie1[i], serie2[i]);
            }
            return new FloatSerie(serie);
        }
        public static FloatSerie MinSerie(FloatSerie serie1, FloatSerie serie2)
        {
            if (serie1.Count != serie2.Count) throw new InvalidOperationException("Cannot get minimun serie as size in different");

            float[] serie = new float[serie1.Count];

            for (int i = 0; i < serie1.Count; i++)
            {
                serie[i] = Math.Min(serie1[i], serie2[i]);
            }
            return new FloatSerie(serie);
        }
        #endregion

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

        /// <summary>
        /// Return the highest from the n highest bars looking backward, if a bar is lower it's ignored
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        public float GetCountBackHigh(int startIndex, int lookback)
        {
            float highest = this[startIndex];
            int count = 1;
            for (int i = startIndex - 1; count < lookback && i >= 0; i--)
            {
                if (this[i] > highest)
                {
                    count++;
                    highest = this[i];
                }
            }
            return highest;
        }
        /// <summary>
        /// Return the lowest from the n lowest bars looking backward, if a bar is lower it's ignored
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        public float GetCountBackLow(int startIndex, int lookback)
        {
            float lowest = this[startIndex];
            int count = 1;
            for (int i = startIndex - 1; count < lookback && i >= 0; i--)
            {
                if (this[i] < lowest)
                {
                    count++;
                    lowest = this[i];
                }
            }
            return lowest;
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
        public FloatSerie CalculateEMATrailStop(int period, int inputSmoothing)
        {
            float alpha = 2.0f / (float)(period + 1);

            // shortStopSerie[i] = shortStopSerie[i - 1] + alpha * (closeEMASerie[i] - shortStopSerie[i - 1]);
            // longStopSerie[i] = longStopSerie[i - 1] + alpha * (closeEMASerie[i] - longStopSerie[i - 1]);
            FloatSerie trailSerie = new FloatSerie(this.Count, "TRAILEMA");

            FloatSerie EMASerie = this.CalculateEMA(inputSmoothing);

            bool upTrend = EMASerie[1] > EMASerie[0];
            int i = 1;
            float extremum = EMASerie[0];
            trailSerie[0] = EMASerie[0];
            extremum = EMASerie[0];

            foreach (float currentValue in this.Values.Skip(1))
            {
                if (i > inputSmoothing)
                {
                    if (upTrend)
                    {
                        if (EMASerie[i] < trailSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            trailSerie[i] = extremum;
                            extremum = EMASerie[i];
                        }
                        else
                        {
                            // Trail the stop  
                            trailSerie[i] = trailSerie[i - 1] + alpha * (EMASerie[i] - trailSerie[i - 1]);
                            extremum = Math.Max(extremum, EMASerie[i]);
                        }
                    }
                    else
                    {
                        if (EMASerie[i] > trailSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            trailSerie[i] = extremum;
                            extremum = EMASerie[i];
                        }
                        else
                        {
                            // Trail the stop  
                            trailSerie[i] = trailSerie[i - 1] + alpha * (EMASerie[i] - trailSerie[i - 1]);
                            extremum = Math.Min(extremum, EMASerie[i]);
                        }
                    }
                }
                i++;
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

        public IEnumerator<float> GetEnumerator()
        {
            return ((IEnumerable<float>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}
