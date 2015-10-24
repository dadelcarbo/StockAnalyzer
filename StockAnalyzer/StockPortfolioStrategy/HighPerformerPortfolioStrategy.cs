//#define USE_LOGS 

using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;



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

      private static DateTime previousDate;
      protected override void ApplyAtDate(System.DateTime applyDate)
      {
         int nbPositions = 10;

         // Evaluate portofolio value.
         float[] positionValues = new float[this.Series.Count];
         float portfolioValue = this.availableLiquidity;
         int count = 0;
         foreach (StockPosition position in this.Positions)
         {
            StockSerie serie = this.Series.Find(s => s.StockName == position.StockName);
            float value = 0;
            if (serie.ContainsKey(applyDate))
            {
               value = position.Value(serie[applyDate].OPEN);
            }
            else
            {
                value = position.Value(serie[previousDate].OPEN);
            }
            positionValues[count++] = value;
            portfolioValue += value;
         }

#if USE_LOGS
            Console.WriteLine("Before Selling Arbitrage");
            this.Dump(applyDate);
#endif

         // Evaluate performance using RANK indicator
         Dictionary<StockSerie, float> variations = new Dictionary<StockSerie, float>();
         foreach (StockSerie serie in this.Series)
         {
            if (serie.ContainsKey(applyDate))
            {
               //variations.Add(serie, serie.GetIndicator("RANK(12,10)").Series[0][serie.IndexOf(applyDate)-1]);  
               //variations.Add(serie, serie.GetIndicator("ADXDIFF(50,25)").Series[0][serie.IndexOf(applyDate)-1]);
               int index = serie.IndexOf(applyDate) - 1;
               if (index > 0)
               {
                  StockSerie rsSerie = StockDictionary[serie.StockName + "_RS"];
                  IStockIndicator indicator = serie.GetIndicator("OSC(149,150)");
                  if (indicator.Events[4][index])
                  {
                     variations.Add(serie, indicator.Series[0][index]);
                  }
               }
            }
         }

         var sortedList = variations.Where(p => p.Value > 0).OrderByDescending(p => p.Value).Take(nbPositions);

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

            StockOrder order;
            DateTime executionDate = serie.ContainsKey(applyDate) ? applyDate : previousDate;

            order = StockOrder.CreateExecutedOrder(position.StockName, StockOrder.OrderType.SellAtMarketOpen, false,
               applyDate, applyDate, position.Number, serie[executionDate].OPEN, 0.0f);

            this.availableLiquidity += position.Number * serie[executionDate].OPEN;
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
            //if (nbUnit <= 0)
            //   Console.WriteLine("Unit" + nbUnit);
            if (nbUnit > 0)
            {
               Console.WriteLine(" ==> " + applyDate.ToShortDateString() + "Buying " + nbUnit + " " + pair.Key.StockName);
               StockOrder order = StockOrder.CreateExecutedOrder(pair.Key.StockName, StockOrder.OrderType.BuyAtMarketOpen, false,
                   applyDate, applyDate, nbUnit, pair.Key[applyDate].OPEN, 0.0f);

               this.availableLiquidity -= nbUnit * pair.Key[applyDate].OPEN;

               this.Portfolio.OrderList.Add(order);
               this.Positions.Add(new StockPosition(order));
            }
         }

            this.Dump(applyDate);
         previousDate = applyDate;

#if USE_LOGS
            Console.WriteLine("After Arbitrage");
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