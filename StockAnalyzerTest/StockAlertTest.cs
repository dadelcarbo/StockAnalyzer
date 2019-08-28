using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockAlertTest
    {
        [TestMethod]
        public void AlertSerializeTest()
        {
            List<StockAlertDef> expectedAlertDefs = new List<StockAlertDef>()
            {
                new StockAlertDef() {BarDuration = StockBarDuration.TLB_3D, EventName="Event1", IndicatorName="Indic1", IndicatorType="IndicType1"},
                new StockAlertDef() {BarDuration = new StockBarDuration(BarDuration.TLB_6D, 3), EventName="Event1", IndicatorName="Indic1", IndicatorType="IndicType1"}
            };

            var serializer = new XmlSerializer(typeof(List<StockAlertDef>));

            var dailyAlertFileName = Directory.GetCurrentDirectory() + "AlertDailyReport.xml";
            using (var fs = new FileStream(dailyAlertFileName, FileMode.Create))
            {
                serializer.Serialize(fs, expectedAlertDefs);
            }

            using (var fs = new FileStream(dailyAlertFileName, FileMode.Open))
            {
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings
                {
                    IgnoreWhitespace = true
                };
                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);

                var actualAlertDefs = (List<StockAlertDef>)serializer.Deserialize(xmlReader);

                Assert.AreEqual(expectedAlertDefs.Count, actualAlertDefs.Count);
                for (int i = 0; i < expectedAlertDefs.Count; i++)
                {
                    Assert.AreEqual(expectedAlertDefs[i].ToString(), actualAlertDefs[i].ToString());
                }
            }
        }

        [TestMethod]
        public void EqualityTest()
        {
            var alert1 = new StockAlert() { BarDuration = StockBarDuration.TLB_3D, Alert = "t1", AlertClose = 12, Date = DateTime.Today, ExchangedMoney = 1000, StockGroup = "CAC", StockName = "EDF" };
            var alert2 = new StockAlert() { BarDuration = StockBarDuration.TLB_3D, Alert = "t1", AlertClose = 12, Date = DateTime.Today, ExchangedMoney = 1000, StockGroup = "CAC", StockName = "EDF" };
            Assert.AreEqual(alert1, alert2);
        }
    }
}
