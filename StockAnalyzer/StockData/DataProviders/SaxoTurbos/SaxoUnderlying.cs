using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    [DebuggerDisplay("Id={Id}, SaxoName={SaxoName}, InstrumentId={InstrumentId}")]
    public class SaxoUnderlying
    {
        public long Id { get; set; }
        public string SaxoName { get; set; }

        public string InstrumentId { get; set; }

        public static IList<SaxoUnderlying> Load()
        {
            return File.ReadAllLines(SaxoTurboDataProvider.SaxoUnderlyingFile).Select(l =>
            {
                var parts = l.Split(',');
                return new SaxoUnderlying
                {
                    Id = long.Parse(parts[0]),
                    SaxoName = parts[1],
                    InstrumentId = parts.Length > 2 ? parts[2] : null
                };
            }).ToList();
        }

        public static void Save(IEnumerable<SaxoUnderlying> underlyings)
        {
            File.WriteAllLines(SaxoTurboDataProvider.SaxoUnderlyingFile, underlyings.Select(u => $"{u.Id},{u.SaxoName},{u.InstrumentId}"));
        }
    }
}
