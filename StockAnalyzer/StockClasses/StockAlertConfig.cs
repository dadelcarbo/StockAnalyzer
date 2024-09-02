using Saxo.OpenAPI.TradingServices;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace StockAnalyzer.StockClasses
{
    public enum StockAlertTimeFrame
    {
        Daily,
        Weekly,
        Monthly,
        Intraday
    };
    public class StockAlertConfig
    {
        static public IEnumerable<StockAlertTimeFrame> TimeFrames = Enum.GetValues(typeof(StockAlertTimeFrame)).Cast<StockAlertTimeFrame>();

        public static string AlertDefFolder => Folders.AlertDef;

        private StockAlertConfig(StockAlertTimeFrame timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        private static List<StockAlertConfig> configs = null;
        public static List<StockAlertConfig> AlertConfigs
        {
            get
            {
                configs ??= TimeFrames.Select(t => new StockAlertConfig(t)).ToList();
                return configs;
            }
        }
        public static StockAlertConfig GetConfig(StockAlertTimeFrame timeFrame)
        {
            return AlertConfigs.First(c => c.TimeFrame == timeFrame);
        }

        public StockAlertTimeFrame TimeFrame { get; set; }

        private StockAlertLog alertLog = null;
        public StockAlertLog AlertLog
        {
            get
            {
                if (alertLog == null)
                {
                    var startDate = DateTime.Today;
                    switch (TimeFrame)
                    {
                        case StockAlertTimeFrame.Weekly:
                            startDate = startDate.AddMonths(-1);
                            break;
                        case StockAlertTimeFrame.Monthly:
                            startDate = startDate.AddMonths(-2);
                            break;
                        case StockAlertTimeFrame.Intraday:
                            startDate = startDate.AddDays(-5);
                            break;
                        default:
                            startDate = startDate.AddDays(-5);
                            break;
                    }
                    alertLog = StockAlertLog.Load($"AlertLog{TimeFrame}.xml", startDate);
                }
                return alertLog;
            }
        }

        static private List<StockAlertDef> allAlertDefs = null;
        static public List<StockAlertDef> AllAlertDefs
        {
            get
            {
                if (allAlertDefs == null)
                {
                    string alertFileName = Path.Combine(AlertDefFolder, "AlertDefUserDefined.json");
                    // Parse alert lists
                    if (File.Exists(alertFileName))
                    {
                        try
                        {
                            allAlertDefs = JsonSerializer.Deserialize<List<StockAlertDef>>(File.ReadAllText(alertFileName),
                                new JsonSerializerOptions
                                {
                                    Converters = { new JsonStringEnumConverter() }
                                });
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + Environment.NewLine + alertFileName, "Alert File Error");
                        }
                    }
                }
                allAlertDefs ??= new List<StockAlertDef>();
                return allAlertDefs;
            }
        }

        private List<StockAlertDef> alertDefs = null;
        public List<StockAlertDef> AlertDefs
        {
            get
            {
                if (alertDefs != null)
                    return alertDefs;
                switch (TimeFrame)
                {
                    case StockAlertTimeFrame.Daily:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == BarDuration.Daily).ToList();
                        break;
                    case StockAlertTimeFrame.Weekly:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == BarDuration.Weekly).ToList();
                        break;
                    case StockAlertTimeFrame.Monthly:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == BarDuration.Monthly).ToList();
                        break;
                    case StockAlertTimeFrame.Intraday:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration > BarDuration.Monthly).ToList();
                        break;
                    default:
                        alertDefs = new List<StockAlertDef>();
                        break;
                }
                return alertDefs;
            }
        }

        static public void SaveConfig()
        {
            if (allAlertDefs != null)
            {
                foreach (var alertGroup in allAlertDefs.GroupBy(a => a.BarDuration))
                {
                    var rank = 10;
                    foreach (var alertDef in alertGroup.OrderBy(a => a.Rank))
                    {
                        alertDef.Rank = rank;
                        rank += 10;
                    }
                }

                string alertFileName = Path.Combine(AlertDefFolder, "AlertDefUserDefined.json");
                File.WriteAllText(alertFileName, JsonSerializer.Serialize(allAlertDefs.OrderBy(a => a.BarDuration).ThenBy(a => a.Rank).ToList(),
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() }
                    }));
            }
        }

        static public void ReloadConfig()
        {
            allAlertDefs = null;
            configs = null;
        }
    }
}
