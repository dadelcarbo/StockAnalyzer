using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;

namespace StockAnalyzer.StockPortfolioStrategy
{
   public class LongTermSmoothed : StockPortfolioStrategyBase
   {
      public override string Description
      {
         get { return "Long term strategy based on smoothed data"; }
      }
      List<StockDailyValue> cacDailyValues;
      protected override DateTime? InitialiseAllocation(DateTime startDate)
      {
         cacDailyValues = this.StockDictionary["CAC40"].GetValues(StockSerie.StockBarDuration.Daily);

         DateTime nextDate = startDate;
         DateTime lastDate = cacDailyValues.Last().DATE;
         bool found = false;

         foreach (StockDailyValue dailyValue in cacDailyValues)
         {
            if (dailyValue.DATE >= nextDate)
            {
               nextDate = dailyValue.DATE;
               found = true;
               break;
            }
         }
         if (!found)
            return null;

         return nextDate;
      }

      int nbPosition = 5;

      protected override void ApplyAtDate(DateTime applyDate)
      {
         string trailHL = "TRAILHL(3)";
         // Look if need to close positions
         if (this.Positions.Count > 0)
         {
            List<StockPosition> trashList = new List<StockPosition>();
            foreach (var position in this.Positions)
            {
               StockSerie serie = StockDictionary[position.StockName];
               int index = serie.IndexOf(applyDate);
               if (index != -1)
               {
                  if (serie.GetTrailStop(trailHL).Events[3][index])
                  {
                     DateTime nextDay = serie.Keys.ElementAt(index + 1);
                     float open = serie.GetValues(StockSerie.StockBarDuration.Daily).First(s => s.DATE == nextDay).OPEN;
                     // Close position
                     Console.WriteLine(" ==> " + applyDate.ToShortDateString() + "Selling " + position.Number + " " +
                                       position.StockName);
                     StockOrder order = StockOrder.CreateExecutedOrder(position.StockName,
                         StockOrder.OrderType.SellAtMarketOpen,
                         nextDay, nextDay, position.Number, open, 0.0f);

                     this.availableLiquidity += position.Number * open;

                     this.Portfolio.OrderList.Add(order);
                     trashList.Add(position);
                  }
               }
            }
            foreach (var p in trashList)
            {
               this.Positions.Remove(p);
            }
         }

         // try to open new position
         if (this.Positions.Count < this.nbPosition)
         {
            foreach (var serie in this.Series)
            {
               if (!this.Positions.Any(p => p.StockName == serie.StockName))
               {
                  int index = serie.IndexOf(applyDate);
                  if (index != -1)
                  {
                     if (serie.GetTrailStop(trailHL).Events[2][index])
                     {
                        // Look for Puke exhaustion sell within the last 5 bars
                        IStockDecorator decorator = serie.GetDecorator("DIV(1)", "PUKE(50,6,0,10)");
                        int nbBar = 5;
                        bool found = false;
                        for (int i = 0; i < nbBar && !found; i++)
                        {
                           found = decorator.Events[1][index - i];
                        }
                        if (found)
                        {
                           DateTime nextDay = serie.Keys.ElementAt(index + 1);
                           float open = serie.GetExactValues().First(s => s.DATE == nextDay).OPEN;

                           // Open position
                           int nbUnit = (int)Math.Floor(this.availableLiquidity / open / (nbPosition - this.Positions.Count));
                           if (nbUnit <= 0)
                              Console.WriteLine("Unit" + nbUnit);
                           if (nbUnit > 0)
                           {
                              Console.WriteLine(" ==> " + applyDate.ToShortDateString() + "Buying " + nbUnit + " " + serie.StockName);
                              StockOrder order = StockOrder.CreateExecutedOrder(serie.StockName, StockOrder.OrderType.BuyAtMarketOpen,
                                  nextDay, nextDay, nbUnit, open, 0.0f);

                              this.availableLiquidity -= nbUnit * open;

                              this.Portfolio.OrderList.Add(order);
                              this.Positions.Add(new StockPosition(order));
                           }

                           if (this.Positions.Count == this.nbPosition)
                           {
                              break;
                           }
                        }
                     }
                  }
               }
            }
         }
      }
   }
}
