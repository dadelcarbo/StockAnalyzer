using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockStrategyClasses.StockMoneyManagement
{
   public class IStockMoneyManagement
   {
      public string Description { get; protected set; }
      public string Name { get; protected set; }
   }
}
