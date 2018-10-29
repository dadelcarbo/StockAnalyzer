using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockAnalysisTest
    {
        [TestMethod]
        public void StockAnalysisSerializeTest()
        {
            StockAnalysis2 analysis = new StockAnalysis2();
            analysis.Comments.Add(new StockComment { Date = DateTime.Today, Comment = "This is today's comment" });
            analysis.Comments.Add(new StockComment { Date = DateTime.Today.AddDays(1), Comment = "This is tomorrow's comment" });

            var di = new StockDrawingItems2(StockBarDuration.Daily);
            di.DrawingItems.Add(new Line2D(new System.Drawing.PointF(0, 0), 0f, 1f, Pens.Red));
            analysis.DrawingItems.Add(di);

            di = new StockDrawingItems2(StockBarDuration.TLB_3D);
            di.DrawingItems.Add(new Segment2D(0f, 1f, 1f, 0f));
            analysis.DrawingItems.Add(di);

            di = new StockDrawingItems2(StockBarDuration.TLB);
            di.DrawingItems.Add(new HalfLine2D(new System.Drawing.PointF(0, 0), 1f, 0f, Pens.Blue));
            analysis.DrawingItems.Add(di);

            analysis.Excluded = true;
            analysis.Theme = "ThemeTest";

            XmlSerializer s = new XmlSerializer(typeof(StockAnalysis2));
            StringBuilder sb = new StringBuilder();

            using (var stream = new StringWriter(sb))
            {
                s.Serialize(stream, analysis);
            }
            string xml = sb.ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));

            var a = (StockAnalysis2)s.Deserialize(new StringReader(xml));

            Assert.AreEqual(3, a.DrawingItems.Count);
        }
    }
}
