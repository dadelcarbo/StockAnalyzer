//#define USE_LOGS 

using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;



namespace StockAnalyzer.StockPortfolioStrategy
{
   public class _HighPerformerPortfolioStrategy : StockPortfolioStrategyBase
   {
      public override string Description
      {
         get
         {
            return "Simple test portfolio strategy making buying the best performers from the list";
         }
      }

      protected override void ApplyAtDate(System.DateTime applyDate)
      {
         int nbPositions = 5;

         // Evaluate portofolio value.
         float[] positionValues = new float[this.Series.Count];
         float portfolioValue = this.availableLiquidity;
         int count = 0;
         foreach (StockPosition position in this.Positions)
         {
            StockSerie serie = this.Series.Find(s => s.StockName == position.StockName);
            float value = position.Value(serie[applyDate].OPEN);
            positionValues[count++] = value;
            portfolioValue += value;
         }

#if USE_LOGS
            Console.WriteLine("Before Selling Arbitrage");
            this.Dump(applyDate);
#endif

         // Evaluate performance since 6 months
         Dictionary<StockSerie, float> variations = new Dictionary<StockSerie, float>();
         DateTime startDate = applyDate.AddMonths(-3);
         foreach (StockSerie serie in this.Series)
         {
            variations.Add(serie, serie.GetVariationSince(applyDate, startDate));
         }

         var sortedList = variations.Where(p => p.Value > 0).OrderByDescending(p => p.Value);

         // Sell stock not listed as best performers
         float targetPositionValue = portfolioValue / (float)this.Series.Count;

         List<StockPosition> trashList = new List<StockPosition>();
         foreach (StockPosition position in this.Positions)
         {
            if (sortedList.Any(p => p.Key.StockName == position.StockName)) continue; // Keep position in portofolio

            StockSerie serie = this.Series.First(s => s.StockName == position.StockName);

            // Sell position
            Console.WriteLine(" ==> " + applyDate.ToShortDateString() + "Selling " + position.Number + " " +
                              position.StockName);
            StockOrder order = StockOrder.CreateExecutedOrder(position.StockName,
                StockOrder.OrderType.SellAtMarketOpen,
                applyDate, applyDate, position.Number, serie[applyDate].OPEN, 0.0f);

            this.availableLiquidity += position.Number * serie[applyDate].OPEN;

            this.Portfolio.OrderList.Add(order);
            trashList.Add(position);
         }
         foreach (var p in trashList)
         {
            this.Positions.Remove(p);
         }

#if USE_LOGS
            Console.WriteLine("Before Buying Arbitrage");
            this.Dump(applyDate);
#endif
         // Buy stock best perfoming stocks

         foreach (var pair in sortedList)
         {
            if (this.Positions.Count >= nbPositions) break;
            if (this.Positions.Any(p => p.StockName == pair.Key.StockName)) continue; // already in portfolio

            // Buy
            int nbUnit = (int)Math.Floor(this.availableLiquidity / pair.Key[applyDate].OPEN / (nbPositions - this.Positions.Count));
            if (nbUnit <= 0)
               Console.WriteLine("Unit" + nbUnit);
            if (nbUnit > 0)
            {
               Console.WriteLine(" ==> " + applyDate.ToShortDateString() + "Buying " + nbUnit + " " + pair.Key.StockName);
               StockOrder order = StockOrder.CreateExecutedOrder(pair.Key.StockName, StockOrder.OrderType.BuyAtMarketOpen,
                   applyDate, applyDate, nbUnit, pair.Key[applyDate].OPEN, 0.0f);

               this.availableLiquidity -= nbUnit * pair.Key[applyDate].OPEN;

               this.Portfolio.OrderList.Add(order);
               this.Positions.Add(new StockPosition(order));
            }
         }


#if USE_LOGS
            Console.WriteLine("After Arbitrage");
            this.Dump(applyDate);
#endif
      }

      protected override DateTime? InitialiseAllocation(System.DateTime startDate)
      {
         this.availableLiquidity = this.Portfolio.AvailableLiquitidity;

         float amountPerLine = availableLiquidity / (float)this.Series.Count;

         DateTime nextDate = startDate;
         DateTime lastDate = this.Series[0].Keys.Last();
         bool found = false;
         while (!found && nextDate < lastDate)
         {
            foreach (StockSerie serie in this.Series)
            {
               if (serie.IndexOf(nextDate) == -1)
               {
                  found = false;
                  nextDate = nextDate.AddDays(1);
                  break;
               }
               else
               {
                  found = true;
               }
            }
         }
         if (!found) return null;

         return nextDate;
      }
   }
}