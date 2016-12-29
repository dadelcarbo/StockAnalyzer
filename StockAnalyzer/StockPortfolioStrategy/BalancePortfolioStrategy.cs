// #define USE_LOGS 

using System;
using System.Linq;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzer.Portofolio;



namespace StockAnalyzer.StockPortfolioStrategy
{


   public class BalancePortfolioStrategy : StockPortfolioStrategyBase
   {
      public override string Description
      {
         get
         {
            return "Simple test portfolio strategy making the balance between different assets on monthly basis";
         }
      }

      protected override void ApplyAtDate(System.DateTime applyDate)
      {
         // Verify all serie have data

         // Evaluate portofolio value.
         float[] positionValues = new float[this.Series.Count];
         float[] serieValues = new float[this.Series.Count];
         float portfolioValue = this.availableLiquidity;
         int count = 0;
         foreach (StockPosition position in this.Positions)
         {
            StockSerie serie = this.Series.Find(s => s.StockName == position.StockName);
            serieValues[count] = serie[applyDate].OPEN;
            float value = position.Value(serieValues[count]);
            positionValues[count++] = value;
            portfolioValue += value;
         }

#if USE_LOGS
            StockLog.Write("Before Selling Arbitrage");
            this.Dump(applyDate);
#endif

         // Sell stock outvaluing the ratio by xx%
         const float ratioTrigger = 0.05f;
         float targetPositionValue = portfolioValue / (float)this.Series.Count;
         count = 0;
         foreach (StockPosition position in this.Positions)
         {
            float delta = positionValues[count] - targetPositionValue;
            float deltaRatio = delta / portfolioValue;
            if (deltaRatio > ratioTrigger)
            {
               // Sell to reduce postion
               int nbUnit = (int)Math.Floor(delta / serieValues[count]);
               if (nbUnit <= 0)
                  StockLog.Write("Unit" + nbUnit);
               if (nbUnit > 0)
               {
                  StockLog.Write(" ==> " + applyDate.ToShortDateString() + "Selling " + nbUnit + " " + position.StockName);
                  StockOrder order = StockOrder.CreateExecutedOrder(position.StockName, StockOrder.OrderType.SellAtMarketOpen, false,
                      applyDate, applyDate, nbUnit, serieValues[count], 0.0f);

                  this.availableLiquidity += nbUnit * serieValues[count];

                  this.Portfolio.OrderList.Add(order);
                  position.Add(order);
               }
            }
            count++;
         }


#if USE_LOGS
            StockLog.Write("Before Buying Arbitrage");
            this.Dump(applyDate);
#endif
         // Buy stock undervaluing the ratio by xx%
         count = 0;
         foreach (StockPosition position in this.Positions)
         {
            float delta = positionValues[count] - targetPositionValue;
            float deltaRatio = delta / portfolioValue;
            if (deltaRatio < -ratioTrigger)
            {
               // Buy to increase position
               int nbUnit = (int)Math.Floor(-delta / serieValues[count]);
               if (nbUnit <= 0)
                  StockLog.Write("Unit" + nbUnit);
               if (nbUnit > 0 && this.availableLiquidity > nbUnit * serieValues[count])
               {
                  StockLog.Write(" ==> " + applyDate.ToShortDateString() + "Buying " + nbUnit + " " + position.StockName);
                  StockOrder order = StockOrder.CreateExecutedOrder(position.StockName, StockOrder.OrderType.BuyAtMarketOpen, false,
                      applyDate, applyDate, nbUnit, serieValues[count], 0.0f);

                  this.availableLiquidity -= nbUnit * serieValues[count];

                  this.Portfolio.OrderList.Add(order);
                  position.Add(order);
               }
            }
            count++;
         }


#if USE_LOGS
            StockLog.Write("After Arbitrage");
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
         if (!found)
            return null;

         foreach (StockSerie serie in this.Series)
         {
            float value = serie[nextDate].OPEN;
            StockOrder order = StockOrder.CreateExecutedOrder(serie.StockName, StockOrder.OrderType.BuyAtMarketOpen, false,
                nextDate, nextDate, (int)(amountPerLine / value), value, 0.0f);

            this.Portfolio.OrderList.Add(order);

            this.availableLiquidity -= order.TotalCost;

            this.Positions.Add(new StockPosition(order));
         }

         //StockLog.Write("InitialiseAllocation");
         //this.Dump(nextDate);

         return nextDate;
      }
   }
}