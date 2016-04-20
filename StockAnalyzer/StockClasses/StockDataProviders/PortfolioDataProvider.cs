using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   class PortfolioDataProvider : StockDataProviderBase
   {
      public override bool SupportsIntradayDownload
      {
         get { return false; }
      }
      
      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
      }

      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         return true;
      }

      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         StockPortofolio portfolio = StockPortofolioList.Instance.FirstOrDefault(p => p.Name == stockSerie.StockName);
         if (portfolio == null) return false;

         portfolio.ToSerie(stockSerie);

         return true;
      }
   }
}
