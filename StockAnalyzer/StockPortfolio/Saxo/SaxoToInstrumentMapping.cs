using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using StockAnalyzerSettings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockPortfolio.Saxo
{
    public class SaxoToInstrumentMapping
    {
        static SaxoToInstrumentMapping()
        {
            Load();
        }

        static public void AddMapping(long saxoId, string instrumentId)
        {
            saxoToInstrumentMappings.Add(saxoId, instrumentId);
            instrumentToSaxoMappings.Add(instrumentId, saxoId);

            Save();
        }

        static public IEnumerable<KeyValuePair<long, string>> GetSaxoToInstrumentMappings() => saxoToInstrumentMappings;

        static SortedDictionary<long, string> saxoToInstrumentMappings;
        static SortedDictionary<string, long> instrumentToSaxoMappings;
        public static long GetSaxoId(string instrumentId)
        {
            if (instrumentToSaxoMappings.TryGetValue(instrumentId, out var saxoId))
            {
                return saxoId;
            }
            return -1;
        }

        public static string GetInstrumentId(long saxoId)
        {
            if (saxoToInstrumentMappings.TryGetValue(saxoId, out var instrumentId))
            {
                return instrumentId;
            }
            return null;
        }

        public static StockInstrument GetInstrument(long saxoId)
        {
            if (saxoToInstrumentMappings.TryGetValue(saxoId, out var instrumentId))
            {
                if (StockDictionary.Instruments.TryGetValue(instrumentId, out var instrument))
                {
                    return instrument;
                }
            }
            return null;
        }

        private static void Load()
        {
            saxoToInstrumentMappings = new SortedDictionary<long, string>();
            instrumentToSaxoMappings = new SortedDictionary<string, long>();
            if (!File.Exists(Folders.SaxoToInstrumentMapFile))
                return;

            foreach (var line in File.ReadAllLines(Folders.SaxoToInstrumentMapFile))
            {
                var parts = line.Split(',');
                saxoToInstrumentMappings.Add(long.Parse(parts[0]), parts[1]);
                instrumentToSaxoMappings.Add(parts[1], long.Parse(parts[0]));
            }
        }

        private static void Save()
        {
            File.WriteAllLines(Folders.SaxoToInstrumentMapFile, saxoToInstrumentMappings.Select(u => $"{u.Key},{u.Value}"));
        }
    }
}
