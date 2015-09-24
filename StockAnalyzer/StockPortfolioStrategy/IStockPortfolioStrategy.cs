using System;
using System.Collections.Generic;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolioStrategy
{
   public enum UpdatePeriod
   {
      Daily,
      Weekly,
      Monthly
   }

   public interface IStockPortfolioStrategy
   {
      string Description { get; }

      List<StockSerie> Series { get; }

      StockPortofolio Portfolio { get; }

      void Initialise(List<StockSerie> stockSeries, StockPortofolio portfolio, StockDictionary stockDictionary);

      void Apply(DateTime startDate, DateTime endDate, UpdatePeriod updatePeriod);
   }
}
