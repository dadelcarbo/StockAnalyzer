using System.Linq;
using Telerik.Windows.Controls.FieldList;

namespace UltimateChartist.Indicators
{
    public static class MathExtensions
    {
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
    }
}
