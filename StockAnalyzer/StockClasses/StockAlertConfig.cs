using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using StockAnalyzerSettings;

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

        public static string AlertDefFolder => Path.Combine(Folders.Alert , "AlertDef");

        private StockAlertConfig(StockAlertTimeFrame timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        private static List<StockAlertConfig> configs = null;
        public static List<StockAlertConfig> AlertConfigs
        {
            get
            {
                if (configs == null)
                {
                    configs = TimeFrames.Select(t => new StockAlertConfig(t)).ToList();
                }
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
                    string alertFileName = Path.Combine(AlertDefFolder, "AlertDefUserDefined.xml");
                    // Parse alert lists
                    if (File.Exists(alertFileName))
                    {
                        try
                        {
                            using (var fs = new FileStream(alertFileName, FileMode.Open))
                            {
                                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings
                                {
                                    IgnoreWhitespace = true
                                };
                                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                                var serializer = new XmlSerializer(typeof(List<StockAlertDef>));
                                allAlertDefs = (List<StockAlertDef>)serializer.Deserialize(xmlReader);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + Environment.NewLine + alertFileName, "Alert File Error");
                        }
                    }
                }
                if (allAlertDefs == null)
                {
                    allAlertDefs = new List<StockAlertDef>();
                }
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
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == StockBarDuration.Daily).ToList();
                        break;
                    case StockAlertTimeFrame.Weekly:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == StockBarDuration.Weekly).ToList();
                        break;
                    case StockAlertTimeFrame.Monthly:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration == StockBarDuration.Monthly).ToList();
                        break;
                    case StockAlertTimeFrame.Intraday:
                        alertDefs = AllAlertDefs.Where(a => a.BarDuration.Duration > BarDuration.Monthly).ToList();
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
                string alertFileName = AlertDefFolder + $@"\AlertDefUserDefined.xml";
                // Parse alert lists
                using (var fs = new FileStream(alertFileName, FileMode.Create))
                {

                    var settings = new System.Xml.XmlWriterSettings
                    {
                        Indent = true,
                        NewLineOnAttributes = true
                    };
                    var xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                    var serializer = new XmlSerializer(typeof(List<StockAlertDef>));
                    serializer.Serialize(xmlWriter, allAlertDefs);
                }
            }
        }

        static public void ReloadConfig()
        {
            allAlertDefs = null;
            configs = null;
        }
    }
}
