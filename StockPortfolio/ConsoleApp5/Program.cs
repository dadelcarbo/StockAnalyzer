using System;
using System.Linq;

namespace ConsoleApp5
{
    partial class Program
    {
        static void Main(string[] args)
        {
            StockPortfolio.PriceProvider = new PriceProvider();

            //var ptfs = StockPortfolio.LoadPortofolios(@"C:\Users\David\AppData\Roaming\UltimateChartistRoot\Portfolio");

            StockPortfolio portfolio = new StockPortfolio("TextFile2.txt");

            var date = new DateTime(2019, 12, 1);
            foreach (var i in Enumerable.Range(0, 10))
            {
                Console.WriteLine($"Evaluation at {date}: {portfolio.EvaluateAt(date)}");

                date = date.AddDays(1);
            }
        }
    }

    public class PriceProvider : IStockPriceProvider
    {
        public float GetClosingPrice(string stockName, DateTime date)
        {
            return 10.0f;
        }
    }
}
