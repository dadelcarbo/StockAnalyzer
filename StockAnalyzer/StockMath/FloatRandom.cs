using System;

namespace StockAnalyzer.StockMath
{
    public class FloatRandom
    {
        private static readonly Random r = new Random();

        public static float NextUniform(float min, float max)
        {
            return (float)((r.NextDouble() - 0.5f) * (max - min) + (max + min) / 2.0f);
        }

        static bool useSecond = false;
        static float firstValue, secondValue;

        /// <summary>
        ///   Generates normally distributed numbers.
        /// </summary>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static float NextGaussian(float mean, float stdev)
        {
            // check if we can use second value
            if (useSecond)
            {
                // return the second number
                useSecond = false;
                return secondValue;
            }

            double x1, x2, w;

            // generate new numbers
            do
            {
                x1 = r.NextDouble() * 2.0 - 1.0;
                x2 = r.NextDouble() * 2.0 - 1.0;
                w = x1 * x1 + x2 * x2;
            }
            while (w >= 1.0);

            w = Math.Sqrt((-2.0 * Math.Log(w)) / w);

            // get two standard random numbers
            firstValue = (float)((x1 * w) + mean) * stdev;
            secondValue = (float)((x2 * w) + mean) * stdev;

            useSecond = true;

            // return the first number
            return firstValue;
        }
        public static float NextGauchy(float median, float gamma)
        {
            double randNum;
            do
            {
                randNum = r.NextDouble();
            }
            while (randNum < 0.01 || randNum > 0.99);
            return median + gamma * (float)Math.Tan(Math.PI * (randNum - 0.5f));
        }


    }
}
