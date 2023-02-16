using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Xml.Linq;

namespace UltimateChartist.Indicators.Display
{
    public abstract class IndicatorValueBase
    {
        public DateTime Date { get; set; }
    }

    public interface IIndicatorSeries
    {
        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }

}
