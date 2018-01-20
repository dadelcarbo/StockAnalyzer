using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockDrawing;
using System.Drawing;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockHarmonicTest
    {
        [TestMethod]
        public void TestXABCD()
        {
            XABCD harmonic = new XABCD
            {
                X = new PointF(0, 100),
                A = new PointF(10, 50),
                B = new PointF(20, 75),
                C = new PointF(30, 50),
                D = new PointF(40, 100)
            };

            Assert.AreEqual(0.5, harmonic.XB);
            Assert.AreEqual(1, harmonic.AC);
            Assert.AreEqual(2, harmonic.BD);
            Assert.AreEqual(1, harmonic.XD);

        }
    }
}
