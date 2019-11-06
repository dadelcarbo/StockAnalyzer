using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertConfig
    {
        public static List<String> TimeFrames = new List<string> { "Intraday", "Daily", "Weekly", "Monthly" };

        private StockAlertConfig(string timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        private static IEnumerable<StockAlertConfig> configs = null;
        public static IEnumerable<StockAlertConfig> GetConfigs()
        {
            if (configs == null)
            {
                configs = TimeFrames.Select(t => new StockAlertConfig(t));
            }
            return configs;
        }
        public static StockAlertConfig GetConfig(string timeFrame)
        {
            return GetConfigs().First(c=>c.TimeFrame == timeFrame);
        }

        public string TimeFrame { get; set; }

        private StockAlertLog alertLog = null;
        public StockAlertLog AlertLog
        {
            get
            {
                if (alertLog == null)
                {
                    alertLog =StockAlertLog.Load($"AlertLog{TimeFrame}.xml", DateTime.Today.AddDays(-5));
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
                    string alertFileName = Settings.Default.RootFolder + $@"\Alert{TimeFrame}.xml";
                    // Parse alert lists
                    if (File.Exists(alertFileName))
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
                    else
                    {
                        alertDefs = new List<StockAlertDef>();
                    }
                }
                return alertDefs;
            }
        }
    }
}
