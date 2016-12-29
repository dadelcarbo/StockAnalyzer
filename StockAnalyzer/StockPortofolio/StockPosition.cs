using StockAnalyzer.StockPortfolio;
using System;

namespace StockAnalyzer.Portofolio
{
   public class StockPosition2 : StockPositionBase
   {
      public int Number { get; protected set; }
      public float TotalCost { get { return this.UnitCost * this.Number; } }

      public StockPosition2(StockOrder stockOrder)
      {
         if (stockOrder.State != StockOrder.OrderStatus.Executed)
         {
            throw new System.Exception("Cannot create a position from a not executed order");
         }

         this.OpenDate = stockOrder.ExecutionDate;
         this.Number = stockOrder.Number;
         this.StockName = stockOrder.StockName;
         this.UnitCost = stockOrder.UnitCost;
         this.CloseDate = DateTime.MinValue;
         this.IsShortPosition = stockOrder.IsShortOrder;
         this.Status = PositionStatus.Opened;
      }

        public StockPosition2()
        {
        }

        public override float AddedValue(float stockValue)
      {
         if (this.IsShortPosition)
         {
            return Number * (stockValue - UnitCost);
         }
         else
         {
            return Number * (UnitCost - stockValue);
         }
      }
      public override float Value(float stockValue)
      {
         if (this.IsShortPosition)
         {
            return Number * (-stockValue);
         }
         else
         {
            return Number * stockValue;
         }
      }
      public override void Add(StockOrder stockOrder)
      {

         if (stockOrder.State != StockOrder.OrderStatus.Executed)
         {
            throw new System.Exception("Cannot add a not executed order to a position");
         }
         if (stockOrder.IsShortOrder ^ this.IsShortPosition)
         {
            throw new System.Exception("Cannot add a short order to a long position");
         }
         if (stockOrder.IsBuyOrder())
         {
            this.UnitCost = (this.UnitCost * this.Number + stockOrder.UnitCost * stockOrder.Number) / (float)(this.Number + stockOrder.Number);
            this.Number += stockOrder.Number;
         }
         else
         {
            if (this.Number > stockOrder.Number)
            {
               this.Number -= stockOrder.Number;
            }
            else
            {
               if (this.Number == stockOrder.Number)
               {
                  this.Number = 0;
                  this.Status = PositionStatus.Closed;
               }
               else
               {
                  throw new System.Exception("Cannot sell more stock than owned");
               }
            }
         }
      }
   }
}
