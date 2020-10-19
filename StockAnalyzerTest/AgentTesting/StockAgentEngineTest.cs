using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockAgent;

namespace StockAnalyzerTest.AgentTesting
{
    [TestClass]
    public class StockAgentEngineTest
    {
        [TestMethod]
        public void CalculateIndexes()
        {
            var sizes = new int[] { 4, 3, 5, 3 };
            var indexes = new int[sizes.Length];

            int nbSteps = sizes.Aggregate(1, (i, j) => i * j);

            for (int i = 0; i < nbSteps; i++)
            {
                StockAgentEngine.CalculateIndexes(sizes.Length, sizes, indexes, i);

                // Calculate Step from Index
                int step = indexes[0];
                int size = sizes[0];
                for (int j = 1; j < sizes.Length; j++)
                {
                    step += size * indexes[j];
                    size *= sizes[j];
                }
                Assert.AreEqual(i, step);
            }
        }
    }
}
