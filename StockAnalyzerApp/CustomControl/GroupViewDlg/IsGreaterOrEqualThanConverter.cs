﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    public class IsGreaterOrEqualThanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsGreaterOrEqualThanConverter();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IComparable v = value as IComparable;
            IComparable p = parameter as IComparable;

            if (v == null || p == null)
                throw new FormatException("to use this converter, value and parameter shall inherit from IComparable");

            return (v.CompareTo(p) >= 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
