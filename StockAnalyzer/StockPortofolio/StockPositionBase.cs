using StockAnalyzer.StockPortfolio;
using System;

namespace StockAnalyzer.Portofolio
{
   public abstract class StockPositionBase
   {
      public enum PositionStatus
      {
         Opened,
         Closed
      }

      public bool IsShortPosition { get; protected set; }
      public abstract float AddedValue(float stockValue);
      public abstract float Value(float stockValue);
      public float UnitCost { get; protected set; }

      public DateTime OpenDate { get; protected set; }
      public DateTime CloseDate { get; protected set; }
      public string StockName { get; protected set; }
      public PositionStatus Status { get; protected set; }

      #region Position Factory
      public static StockPosition CreatePosition(StockOrder stockOrder)
      {
         return new StockPosition(stockOrder);
      }
      #endregion

      public abstract void Add(StockOrder stockOrder);
   }
}
