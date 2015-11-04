using System;

namespace StockAnalyzer.StockClasses
{
   public class CotValue
   {
      public enum CotValueType
      {
         LargeSpeculatorPosition,
         SmallSpeculatorPosition,
         CommercialHedgerPosition,
         LargeSpeculatorPositionLong,
         SmallSpeculatorPositionLong,
         CommercialHedgerPositionLong,
         LargeSpeculatorPositionShort,
         LargeSpeculatorPositionSpread,
         SmallSpeculatorPositionShort,
         CommercialHedgerPositionShort,
         OpenInterest
      }

      public DateTime Date { get; private set; }
      public float LargeSpeculatorPositionLong { get; private set; }
      public float LargeSpeculatorPositionShort { get; private set; }
      public float LargeSpeculatorPositionSpread { get; private set; }
      public float LargeSpeculatorPosition { get { return LargeSpeculatorPositionLong - LargeSpeculatorPositionShort; } }
      public float SmallSpeculatorPositionLong { get; private set; }
      public float SmallSpeculatorPositionShort { get; private set; }
      public float SmallSpeculatorPosition { get { return SmallSpeculatorPositionLong - SmallSpeculatorPositionShort; } }
      public float CommercialHedgerPositionLong { get; private set; }
      public float CommercialHedgerPositionShort { get; private set; }
      public float CommercialHedgerPosition { get { return CommercialHedgerPositionLong - CommercialHedgerPositionShort; } }
      public float OpenInterest { get; private set; }

      public CotValue(DateTime date,
          float largeSpeculatorPositionLong, float largeSpeculatorPositionShort, float largeSpeculatorPositionShortSpread,
          float smallSpeculatorPositionLong, float smallSpeculatorPositionShort,
          float commercialHedgerPositionLong, float commercialHedgerPositionShort,
          float openInterest)
      {
         this.Date = date;
         this.LargeSpeculatorPositionLong = largeSpeculatorPositionLong;
         this.SmallSpeculatorPositionLong = smallSpeculatorPositionLong;
         this.CommercialHedgerPositionLong = commercialHedgerPositionLong;
         this.LargeSpeculatorPositionShort = largeSpeculatorPositionShort;
         this.LargeSpeculatorPositionSpread = largeSpeculatorPositionShortSpread;
         this.SmallSpeculatorPositionShort = smallSpeculatorPositionShort;
         this.CommercialHedgerPositionShort = commercialHedgerPositionShort;

         this.OpenInterest = openInterest;
      }

      static public string StringFormat()
      {
         return "Date,LargeSpeculatorPositionLong,LargeSpeculatorPositionShort,LargeSpeculatorPositionSpread,SmallSpeculatorPositionLong,SmallSpeculatorPositionShort,CommercialHedgerPositionLong,CommercialHedgerPositionShort,OpenInterest";
      }
      public override string ToString()
      {
         return Date.ToString("s") + "," + LargeSpeculatorPositionLong.ToString() + "," + LargeSpeculatorPositionShort.ToString() + "," + LargeSpeculatorPositionSpread.ToString() + "," + SmallSpeculatorPositionLong.ToString() + "," + SmallSpeculatorPositionShort.ToString()
             + "," + CommercialHedgerPositionLong.ToString() + "," + CommercialHedgerPositionShort.ToString() + "," + OpenInterest.ToString();
      }
      public static CotValue Parse(string line)
      {
         string[] fields = line.Split(',');
         return new CotValue(DateTime.Parse(fields[0]), float.Parse(fields[1]), float.Parse(fields[2]), float.Parse(fields[3]), float.Parse(fields[4]), float.Parse(fields[5]), float.Parse(fields[6]), float.Parse(fields[7]), float.Parse(fields[8]));
      }
   }
}
