using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    public class SaxoUnderlying
    {
        public long Id { get; set; }
        public string SaxoName { get; set; }
        public string SerieName { get; set; }

        public static IEnumerable<SaxoUnderlying> Load()
        {
            return File.ReadAllLines(SaxoIntradayDataProvider.SaxoUnderlyingFile).Select(l =>
            {
                var parts = l.Split(',');
                return new SaxoUnderlying
                {
                    Id = long.Parse(parts[0]),
                    SaxoName = parts[1],
                    SerieName = parts.Length > 2 ? parts[2] : string.Empty
                };
            });
        }


        public static void Save(IEnumerable<SaxoUnderlying> underlyings)
        {
            File.WriteAllLines(SaxoIntradayDataProvider.SaxoUnderlyingFile, underlyings.Select(u => $"{u.Id},{u.SaxoName},{u.SerieName}"));
        }
    }
}
