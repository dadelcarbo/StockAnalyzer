using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UltimateChartist.Indicators
{
    public interface IRangeReentrySerie
    {
        double[] High { get; }
        double[] Low { get; }
        double[] Reentry { get; }
    }
    public interface IRangeSerie
    {
        double[] High { get; }
        double[] Low { get; }
    }
    public class ValueSerie
    {
        public string Name { get; set; }
        public double[] Values { get; set; }
        public Brush Brush { get; set; }
    }
}
