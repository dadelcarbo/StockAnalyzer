using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataProvider : DataProviderBase
    {
        public SaxoTurboDataProvider()
        {
            this.dataClient = new SaxoTurboDataClient();
        }

        public override string DisplayName => "Saxo Turbos";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SaxoTurbo;

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');

            return new StockInstrument()
            {
                Id = row[1],
                Name = row[2],
                Isin = row[1],
                Symbol = string.Empty,
                Group = Groups.TURBO,
                Provider = DataProvider.SaxoTurbo,
                Market = Market.TURBO
            };
        }

        TimeSpan closeTime = new TimeSpan(22, 00, 0);
        TimeSpan openTime = new TimeSpan(08, 0, 0);
        TimeSpan longDelay = new TimeSpan(2, 0, 0);

        public override bool NeedDownload(StockInstrument instrument)
        {
            var history = GetDownloadHistory(instrument);
            if (history.LastDate == DateTime.MinValue)
                return true;
            if (history.DownloadDate.Add(longDelay) > DateTime.Now)
                return false;

            var now = DateTime.Now;
            var isLate = now.TimeOfDay > closeTime;
            var isEarly = now.TimeOfDay < openTime;

            if ((now.DayOfWeek == DayOfWeek.Friday && isLate) || // Check if week-end
                now.DayOfWeek == DayOfWeek.Saturday ||
                now.DayOfWeek == DayOfWeek.Sunday ||
                (now.DayOfWeek == DayOfWeek.Monday && isEarly))
            {
                if ((now.Date - history.LastDate.Date).TotalDays > 3)
                    return true;
                if (history.LastDate.DayOfWeek == DayOfWeek.Friday && history.LastDate.AddHours(1).TimeOfDay == closeTime)
                {
                    return false;
                }
            }
            else if (isEarly) // 
            {
                return history.LastDate.AddHours(1) != now.Date.AddDays(-1).Add(closeTime);
            }
            else if (isLate)
            {
                return history.LastDate.AddHours(1) != now.Date.Add(closeTime);
            }
            return true;
        }
    }
}
