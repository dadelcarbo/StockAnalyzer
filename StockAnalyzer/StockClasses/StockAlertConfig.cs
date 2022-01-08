using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using StockAnalyzerSettings;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertConfig
    {
        public static List<String> TimeFrames = new List<string> { "UserDefined", "Intraday", "Daily", "Weekly", "Monthly" };

        public static string AlertDefFolder => Folders.Alert + @"\AlertDef";

        private StockAlertConfig(string timeFrame)
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
        public static StockAlertConfig GetConfig(string timeFrame)
        {
            return AlertConfigs.First(c => c.TimeFrame == timeFrame);
        }

        public string TimeFrame { get; set; }

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
                        case "UserDefined":
                            startDate = startDate.AddMonths(-1);
                            break;
                        case "Weekly":
                            startDate = startDate.AddMonths(-1);
                            break;
                        case "Monthly":
                            startDate = startDate.AddMonths(-2);
                            break;
                        case "Intraday":
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

        private List<StockAlertDef> alertDefs = null;
        public List<StockAlertDef> AlertDefs
        {
            get
            {
                if (alertDefs == null)
                {
                    string alertFileName = AlertDefFolder + $@"\AlertDef{TimeFrame}.xml";
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
                                alertDefs = (List<StockAlertDef>)serializer.Deserialize(xmlReader);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + Environment.NewLine + alertFileName, "Alert File Error");
                        }
                    }
                    else
                    {
                        alertDefs = new List<StockAlertDef>();
                    }

                }
                return alertDefs;
            }
        }

        public static void SaveConfig(string timeFrame)
        {
            var alertDef = GetConfig(timeFrame)?.AlertDefs;
            if (alertDef != null)
            {
                string alertFileName = AlertDefFolder + $@"\AlertDef{timeFrame}.xml";
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
                    serializer.Serialize(xmlWriter, alertDef);
                }
            }
        }
    }
}
