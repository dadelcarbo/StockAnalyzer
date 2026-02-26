using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace StockAnalyzer.StockClasses
{
    public class StockSplit
    {
        public string StockName { get; set; }
        public DateTime Date { get; set; }
        public float Before { get; set; }
        public float After { get; set; }


        #region Persistency

        static string fileName => Path.Combine(Folders.PersonalFolder, "StockSplits.json");
        static private List<StockSplit> splits = null;
        static public List<StockSplit> Splits
        {
            get
            {
                if (splits == null)
                {
                    // Parse alert lists
                    if (File.Exists(fileName))
                    {
                        try
                        {
                            splits = JsonSerializer.Deserialize<List<StockSplit>>(File.ReadAllText(fileName),
                                new JsonSerializerOptions
                                {
                                    Converters = { new JsonStringEnumConverter() }
                                });
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + Environment.NewLine + fileName, "Alert File Error");
                        }
                    }
                }
                splits ??= new List<StockSplit>();
                return splits;
            }
        }

        static public void Save()
        {
            if (splits != null)
            {
                File.WriteAllText(fileName, JsonSerializer.Serialize(splits,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() }
                    }));
            }
        }
        #endregion
    }
}
