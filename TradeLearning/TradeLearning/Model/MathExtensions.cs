using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeLearning.Model
{
    public static class MathExtensions
    {
        public static double[] CalculateEMA(this double[] values, int period)
        {
            if (values == null || values.Length == 0 || period <= 0)
                throw new ArgumentException("Invalid input data or period.");

            double[] ema = new double[values.Length];
            double multiplier = 2.0 / (period + 1);

            ema[0] = values[0]; // Initialize with the first value

            for (int i = 1; i < values.Length; i++)
            {
                ema[i] = ((values[i] - ema[i - 1]) * multiplier) + ema[i - 1];
            }

            return ema;
        }

        public static double CalculateStandardDeviation(this double[] values)
        {
            // Calculate the mean (average) of the values
            double mean = values.Average();

            // Calculate the sum of the squares of the differences between each value and the mean
            double sumOfSquaresOfDifferences = values.Sum(val => Math.Pow(val - mean, 2));

            // Calculate the variance
            double variance = sumOfSquaresOfDifferences / values.Length;

            // The standard deviation is the square root of the variance
            return Math.Sqrt(variance);
        }

        public static double[] CalculateReturns(this double[] values)
        {
            var returns = new double[values.Length];

            for (int i = 1; i < values.Length; i++)
            {
                // Calculate the return as the percentage change from the previous value
                returns[i] = (values[i] - values[i - 1]) / values[i - 1];
            }

            return returns;
        }

        public static double CalculateSharpeRatio(this double[] returns, double riskFreeRate = 0)
        {
            if (returns == null || returns.Length == 0)
            {
                throw new ArgumentException("Array must not be empty.", nameof(returns));
            }

            // Calculate the mean of the returns
            double meanReturn = returns.Average();

            // Calculate the excess return
            double excessReturn = meanReturn - riskFreeRate;

            // Calculate the standard deviation of the returns
            double stdDev = returns.CalculateStandardDeviation();

            // Calculate the Sharpe Ratio
            return stdDev == 0 ? 0 : excessReturn / stdDev;
        }

        public static double CalculateSortinoRatio(this double[] returns, double riskFreeRate = 0)
        {
            if (returns == null || returns.Length < 2)
            {
                throw new ArgumentException("Array must have at least two elements.", nameof(returns));
            }

            // Calculate the mean of the returns
            double meanReturn = returns.Average();

            // Calculate the excess return
            double excessReturn = meanReturn - riskFreeRate;

            // Calculate the downside deviation
            double downsideDeviation = CalculateDownsideDeviation(returns, meanReturn);

            // Calculate the Sortino Ratio
            return downsideDeviation == 0 ? 0 : excessReturn / downsideDeviation;
        }

        public static double CalculateDownsideDeviation(double[] returns, double meanReturn)
        {
            double sumOfSquaresOfNegativeDifferences = returns
                .Where(r => r < meanReturn)
                .Sum(r => Math.Pow(r - meanReturn, 2));

            double numberOfNegativeReturns = returns.Count(r => r < meanReturn);

            if (numberOfNegativeReturns == 0)
            {
                return 0;
            }

            double averageSquareOfNegativeDifferences = sumOfSquaresOfNegativeDifferences / numberOfNegativeReturns;
            return Math.Sqrt(averageSquareOfNegativeDifferences);
        }

        public static double CalculateMaxDrawdown(this double[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Array must not be empty.", nameof(values));
            }

            double maxDrawdown = 0;
            double peak = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > peak)
                {
                    peak = values[i];
                }

                double drawdown = (peak - values[i]) / peak;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }

            return maxDrawdown;
        }
    }

    public static class RandomExtensions
    {
        public static double[] GenerateNormalDistribution(this Random rng, double sigma, int numberOfPoints)
        {
            if (sigma <= 0 || numberOfPoints <= 0)
                throw new ArgumentException("Sigma and number of points must be positive.");

            double[] values = new double[numberOfPoints];

            for (int i = 0; i < numberOfPoints; i += 2)
            {
                // Generate two uniform random numbers between 0 and 1
                double u1 = 1.0 - rng.NextDouble(); // avoid log(0)
                double u2 = 1.0 - rng.NextDouble();

                // Box-Muller transform
                double randStdNormal1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                double randStdNormal2 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                // Scale by sigma (mean = 0)
                values[i] = randStdNormal1 * sigma;
                if (i + 1 < numberOfPoints)
                    values[i + 1] = randStdNormal2 * sigma;
            }

            return values;
        }

        public static double[] GenerateBrownianPath(this Random rng, double level, double sigma, int numberOfPoints, double drift = 0)
        {
            if (sigma <= 0 || numberOfPoints <= 0)
                throw new ArgumentException("Sigma and number of points must be positive.");

            double[] values = new double[numberOfPoints];
            values[0] = level;
            for (int i = 1; i < numberOfPoints; i += 2)
            {
                // Generate two uniform random numbers between 0 and 1
                double u1 = 1.0 - rng.NextDouble(); // avoid log(0)
                double u2 = 1.0 - rng.NextDouble();

                // Box-Muller transform
                double randStdNormal1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                double randStdNormal2 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                // Scale by sigma (mean = 0)
                level *= 1 + drift + randStdNormal1 * sigma;
                values[i] = level;
                if (i + 1 < numberOfPoints)
                {
                    level *= 1 + drift + randStdNormal2 * sigma;
                    values[i + 1] = level;
                }
            }

            return values;
        }
    }
}
