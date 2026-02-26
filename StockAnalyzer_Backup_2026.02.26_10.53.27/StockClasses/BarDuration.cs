using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public enum BarDuration
    {
        Daily,
        Weekly,
        Monthly,
        M_5,
        M_10,
        M_15,
        M_30,
        H_1,
        H_2,
        H_3,
        H_4
    }

    public static class StockBarDuration
    {
        public static bool IsIntraday(BarDuration duration) => duration >= BarDuration.M_5;

        public static IList<BarDuration> barDurations;
        public static IList<BarDuration> BarDurations => barDurations ??= Enum.GetValues(typeof(BarDuration)).Cast<BarDuration>().ToList();

        static object[] barDurationArray;
        public static object[] BarDurationArray => barDurationArray ??= Enum.GetValues(typeof(BarDuration)).Cast<object>().ToArray();

    }
}