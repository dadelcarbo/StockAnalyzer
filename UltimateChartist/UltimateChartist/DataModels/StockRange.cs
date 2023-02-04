using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimateChartist.DataModels
{
    public class StockRange
    {
        public DateTime Date { get; }
        public double High { get; }
        public double Low { get; }

        public StockRange(DateTime date, double high, double low)
        {
            Date = date;
            High = high;
            Low = low;
        }
    }
}
