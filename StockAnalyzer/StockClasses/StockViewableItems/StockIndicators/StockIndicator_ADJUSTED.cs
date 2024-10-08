﻿using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_Adjusted : StockIndicatorBase
    {
        public override string Name => "Adjusted(" + ((DateTime)this.Parameters[0]).ToShortDateString() + ")";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string Definition => "Adjust serie values reinvesting dividends, this update the series itself";
        public override object[] ParameterDefaultValues => new Object[] { new DateTime(2000, 01, 01) };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeDateTime(new DateTime(2000, 01, 01), DateTime.Today.AddDays(1)) };
        public override string[] ParameterNames => new string[] { "StartDate" };
        public override string[] SerieNames => new string[] { };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {

            //foreach (var value in stockSerie.Values.Reverse())
            //{
            //    if (value.DATE.Date >= startDate)
            //    {
            //        var entries = stockSerie?.Dividend?.Entries.Where(e => e.Date > previousBarDate && e.Date <= value.DATE.Date).ToList();
            //        if (entries.Count > 0)
            //        {
            //            dividend += entries.Sum(e => e.Dividend) * grSerie[i-1] / value.CLOSE;
            //        }
            //    }
            //    grSerie[i++] = value.CLOSE + dividend;
            //    previousBarDate = value.DATE;
            //}

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
