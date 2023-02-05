using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.FieldList;

namespace UltimateChartist.Indicators
{
    public static class MathExtensions
    {
        public static double[] CalculateEMA(this double[] values, int emaPeriod)
        {
            var ema = new double[values.Length];
            if (emaPeriod <= 1)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    ema[i] = values[i];
                }
            }
            else
            {
                float alpha = 2.0f / (float)(emaPeriod + 1);

                ema[0] = values[0];
                for (int i = 1; i < values.Count(); i++)
                {
                    ema[i] = ema[i - 1] + alpha * (values[i] - ema[i - 1]);
                }
            }
            return ema;
        }
    }
}
