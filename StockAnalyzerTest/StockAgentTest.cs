using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockAgent.Agents;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerTest.Utility;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockAgentTest
    {
        [TestMethod]
        public void TestPartialSellEngine()
        {
            var serie = StockTestUtility.StockSerieLoad("COCOA", "CC", StockSerie.Groups.COMMODITY, StockDataProvider.Investing);

            var engine = new StockAgentEngine(typeof(TrailEMA2Agent));
            engine.Agent = new TrailEMA2Agent() { Period = 6 };
            engine.Perform(new List<StockSerie>() { serie }, 20, StockBarDuration.Daily, StockPortfolio.SimulationPortfolio);
        }
    }
}
