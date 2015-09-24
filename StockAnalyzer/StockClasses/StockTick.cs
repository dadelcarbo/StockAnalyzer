using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalyzer.StockClasses
{
   public class StockTick
   {
      public DateTime Date { get; private set; }
      public int TradeNumber { get; private set; }
      public double Value { get; private set; }
      public long Volume { get; private set; }
      public bool UpTick { get; private set; }

      public StockTick(DateTime dateTime, int tradeNumber, double value, long volume, bool upTtick)
      {
         this.Date = dateTime;
         this.TradeNumber = tradeNumber;
         this.Value = value;
         this.Volume = volume;
         this.UpTick = upTtick;
      }

      public override string ToString()
      {
         return this.Date.ToShortDateString() + " " + this.Date.ToShortTimeString() + " Id:" + this.TradeNumber + " Val" + this.Value + " Vol" + this.Volume;
      }

      #region FILE IO MANAGEMENT
      /// <summary>
      /// Parse a Euronext file and return a tick array.
      /// </summary>
      /// <param name="fileName">Name of the ruonext file to be parsed</param>
      /// <param name="lastClose">Value of previous session close, if unknown set is to any value <=0 </param>
      /// <returns></returns>
      static public StockTick[] ParseEuronextFile(string fileName, double lastClose)
      {
         StreamReader reader = new StreamReader(fileName);
         reader.ReadLine();
         reader.ReadLine();
         string line = reader.ReadLine();
         string[] fieldNames = line.Split('\t');
         line = reader.ReadLine();
         string[] fields = line.Split('\t');
         string date1 = fields[Array.IndexOf(fieldNames, "Date")].Remove(9);
         System.DateTime dataDate;
         if (!DateTime.TryParse(date1, out dataDate))
         {
            throw new System.Exception("Invalid Euronext file, date not found");
         }
         line = reader.ReadLine();
         line = reader.ReadLine();

         // 
         List<string[]> tickList = new List<string[]>();
         while (!reader.EndOfStream)
         {
            line = reader.ReadLine();
            tickList.Add(line.Split('\t'));
         }
         reader.Close();
         tickList.Reverse();

         // Create the array
         StockTick[] ticks = new StockTick[tickList.Count];
         int i = 0;
         bool upTick = true;
         double value, previousValue = lastClose;
         foreach (string[] tickFields in tickList)
         {
            value = double.Parse(tickFields[2]);
            if (value < previousValue) // Check the up or down ticks at the open using the last close value
            {
               upTick = false;
            }
            else if (value > previousValue)
            {
               upTick = true;
            }
            long volume = 0;
            long.TryParse(tickFields[3], out volume);
            ticks[i++] = new StockTick(dataDate.Add(TimeSpan.Parse(tickFields[0])), int.Parse(tickFields[1]), value
                , volume, upTick);
            previousValue = value;
         }
         return ticks;
      }
      #endregion
   }
}
