using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerTest.BarDurationTest
{
    [TestClass]
    public class BarDurationTest
    {
        [TestMethod]
        public void EqualityTest()
        {
            var daily1 = StockBarDuration.Daily;
            var daily2 = new StockBarDuration(BarDuration.Daily);

            Assert.IsFalse(object.ReferenceEquals(daily1, daily2));
            Assert.AreNotSame(daily1, daily2);
            Assert.IsTrue(daily1==daily2);
            Assert.AreEqual(daily1, daily2);

            StockBarDuration daily3 = null;
            Assert.AreNotEqual(daily1, daily3);
            Assert.AreNotEqual(daily3, daily1);

            Assert.IsFalse(daily1 == daily3);
            Assert.IsFalse(daily3 == daily1);

            StockBarDuration daily4 = BarDuration.Daily;
            Assert.AreEqual(daily1, daily4);

            SortedList<StockBarDuration, string> sortedList = new SortedList<StockBarDuration, string>();

            sortedList.Add(daily1, "daily1");
            Assert.IsTrue(sortedList.ContainsKey(daily1));
            Assert.IsTrue(sortedList.ContainsKey(daily2));
            Assert.IsTrue(sortedList.ContainsKey(daily4));
        }

        [TestMethod]
        public void SerializeTest()
        {
            var duration = new StockBarDuration(BarDuration.Daily);

            XmlSerializer s = new XmlSerializer(typeof(StockBarDuration));
            StringBuilder sb = new StringBuilder();

            using (var stream = new StringWriter(sb))
            {
                s.Serialize(stream, duration);
            }
            string xml = sb.ToString();
        }
    }
}
