using System;

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
