using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
   public class StockBarSerie
   {
      public string Name { get; set; }
      public string ShortName { get; set; }

      public StockBar.StockBarType BarType { get; set; }
      public StockBar[] StockBars { get; private set; }

      public double Range { get; set; }
      public int Duration { get; set; } // in minutes
      public int TickNumber { get; set; }

      private StockBarSerie(string name, string shortName, StockBar.StockBarType barType)
      {
         this.Name = name;
         this.ShortName = shortName;
         this.BarType = barType;
         this.Range = double.MinValue;
         this.Duration = int.MinValue;
         this.TickNumber = int.MinValue;
         this.StockBars = null;
      }

      #region BAR SERIE FACTORY
      static public StockBarSerie CreateRangeBarSerie(string name, string shortName, double range, StockTick[] ticks)
      {
         // Arguments check
         if (ticks == null || ticks.Count() == 0)
         {
            throw new ArgumentNullException("ticks", "ticks must contain at least one tick");
         }
         if (range < (ticks[0].Value / 10000))
         {
            throw new ArgumentOutOfRangeException("range", range, "range must be greater or equal to 0,01% of the value");
         }
         StockBarSerie barSerie = new StockBarSerie(name, shortName, StockBar.StockBarType.Range);
         barSerie.Range = range;
         barSerie.AppendRangeBars(ticks);
         return barSerie;
      }
      static public StockBarSerie CreateTickBarSerie(string name, string shortName, int tickNumber, StockTick[] ticks)
      {
         // Arguments check
         if (tickNumber < 1)
         {
            throw new ArgumentOutOfRangeException("tickNumber", tickNumber, "TickNumber must be greater or equal to one");
         }
         if (ticks == null || ticks.Count() == 0)
         {
            throw new ArgumentNullException("ticks", "ticks must contain at least one tick");
         }

         StockBarSerie barSerie = new StockBarSerie(name, shortName, StockBar.StockBarType.Tick);
         barSerie.TickNumber = tickNumber;
         barSerie.AppendTickBars(ticks);
         return barSerie;
      }

      static public StockBarSerie CreateIntradayBarSerie(string name, string shortName, int duration, StockTick[] ticks)
      {
         // Arguments check
         if (duration < 1)
         {
            throw new ArgumentOutOfRangeException("tickNumber", duration, "duration must be greater or equal to one minute");
         }
         if (ticks == null || ticks.Count() == 0)
         {
            throw new ArgumentNullException("ticks", "ticks must contain at least one tick");
         }

         StockBarSerie barSerie = new StockBarSerie(name, shortName, StockBar.StockBarType.Intraday);
         barSerie.Duration = duration;
         barSerie.AppendIntradayBars(ticks);
         return barSerie;
      }
      static public StockBarSerie CreateDailyBarSerie(string name, string shortName, StockTick[] ticks)
      {
         // Arguments check
         if (ticks == null || ticks.Count() == 0)
         {
            throw new ArgumentNullException("ticks", "ticks must contain at least one tick");
         }

         StockBarSerie barSerie = new StockBarSerie(name, shortName, StockBar.StockBarType.Daily);
         barSerie.AppendDailyBars(ticks);
         return barSerie;
      }
      #endregion
      #region APPEND FUNCTIONS
      public void Append(StockTick[] ticks)
      {
         switch (this.BarType)
         {
            case StockBar.StockBarType.Intraday:
               this.AppendIntradayBars(ticks);
               break;
            case StockBar.StockBarType.Daily:
               this.AppendDailyBars(ticks);
               break;
            case StockBar.StockBarType.Range:
               this.AppendRangeBars(ticks);
               break;
            case StockBar.StockBarType.Tick:
               this.AppendTickBars(ticks);
               break;
            default:
               throw new System.NotSupportedException("Unknown bar type");
         }
      }
      private void AppendIntradayBars(StockTick[] ticks)
      {
         int tickCountInBar = 0;
         double low = double.MaxValue;
         double high = double.MinValue;
         double open = 0, close = 0;
         long volume = 0, upVolume = 0;
         int upTick = 0;
         TimeSpan durationSpan = TimeSpan.FromMinutes(this.Duration);
         StockSession session = new StockSession(new TimeSpan(9, 0, 0), new TimeSpan(17, 40, 0));
         TimeSpan tickTime = session.OpenTime;

         StockTick tick = null;

         DateTime day = ticks[0].Date.Date;
         TimeSpan currentTime = session.OpenTime;
         int i = 0;

         List<StockBar> bars = null;
         if (this.StockBars != null)
         {
            bars = this.StockBars.ToList();
         }
         else
         {
            bars = new List<StockBar>();
         }
         while (currentTime <= session.CloseTime && i < ticks.Length)
         {
            if (tickTime >= currentTime + durationSpan)
            {   // Next tick is after the current bar close
               if (tickCountInBar == 0)
               {
                  // No tick in bar
                  bars.Add(new StockBar(close, close, close, close, 0, 0, 0, 0, day + currentTime));
                  currentTime += durationSpan;
               }
               else
               {
                  // Create new bar
                  bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCountInBar, upTick, day + currentTime));
                  currentTime += durationSpan;
                  tickCountInBar = 0;
                  volume = 0;
                  upVolume = 0;
                  upTick = 0;
                  low = double.MaxValue;
                  high = double.MinValue;
               }
            }
            else
            { // Current tick is in the current bar
               tick = ticks[i];
               if (tickCountInBar == 0)
               {
                  open = tick.Value;
               }
               tickCountInBar++;
               low = Math.Min(low, tick.Value);
               high = Math.Max(high, tick.Value);
               close = tick.Value;
               volume += tick.Volume;
               if (tick.UpTick)
               {
                  upTick++;
                  upVolume += tick.Volume;
               }
               if (++i < ticks.Length)
               {
                  tick = ticks[i];
                  tickTime = tick.Date.TimeOfDay;
               }
               else
               {
                  tick = null;
               }
            }
         }
         if (tick == null)
         {
            // We reached the end of the ticks, let create the last bar
            bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCountInBar, upTick, day + currentTime));
         }
         this.StockBars = bars.ToArray();
      }
      private void AppendDailyBars(StockTick[] ticks)
      {
         double low = double.MaxValue;
         double high = double.MinValue;
         double open = ticks.First().Value, close = 0;
         long volume = 0, upVolume = 0;
         int upTick = 0;

         List<StockBar> bars = null;
         if (this.StockBars != null)
         {
            bars = this.StockBars.ToList();
         }
         else
         {
            bars = new List<StockBar>();
         }
         foreach (StockTick tick in ticks)
         {
            low = Math.Min(low, tick.Value);
            high = Math.Max(high, tick.Value);
            close = tick.Value;
            volume += tick.Volume;
            if (tick.UpTick)
            {
               upTick++;
               upVolume += tick.Volume;
            }
         }
         close = ticks.Last().Value;
         bars.Add(new StockBar(open, high, low, close, volume, upVolume, ticks.Length, upTick, ticks.First().Date.Date));
         this.StockBars = bars.ToArray();
      }
      private void AppendRangeBars(StockTick[] ticks)
      {
         int tickCountInBar = 0;
         double low = double.MaxValue;
         double high = double.MinValue;
         double open = 0, close = 0;
         long volume = 0, upVolume = 0;
         int upTick = 0;
         List<StockBar> bars = null;
         DateTime openDate = new DateTime();

         if (this.StockBars != null)
         {
            StockBar lastBar = this.StockBars[this.StockBars.Length - 1];
            bars = this.StockBars.ToList();
            if (!lastBar.Complete)
            {
               tickCountInBar = lastBar.TICK;
               open = lastBar.OPEN;
               high = lastBar.HIGH;
               low = lastBar.LOW;
               close = lastBar.CLOSE;
               volume = lastBar.VOLUME;
               upVolume = lastBar.UPVOLUME;
               upTick = lastBar.UPTICK;
               openDate = lastBar.DATE;
               bars.RemoveAt(bars.Count - 1);
            }
         }
         else
         {
            bars = new List<StockBar>();
         }

         System.TimeSpan minSpan = System.TimeSpan.FromSeconds(1);

         StockTick tick = null;
         for (int i = 0; i < ticks.Length; i++)
         {
            tick = ticks[i];
            if (tickCountInBar > 0)
            {
               // Check if current tick is in range
               if (tick.Value - low >= Range)
               {   // overshooting the upside
                  close = low + Range;
                  high = close;
                  bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCountInBar, upTick, openDate));
                  TimeSpan timeSpan = minSpan;
                  while (close + Range <= tick.Value)
                  {
                     open = low = close;
                     close = high = close + Range;
                     bars.Add(new StockBar(open, high, low, close, 0, 0, 0, 0, tick.Date + timeSpan));
                     timeSpan = timeSpan.Add(minSpan);
                  }
                  tickCountInBar = 1;
                  open = close;
                  low = close;
                  high = close;
                  volume = tick.Volume;
                  openDate = tick.Date;
                  if (tick.UpTick)
                  {
                     upTick = 1;
                     upVolume = tick.Volume;
                  }
                  else
                  {
                     upTick = 0;
                     upVolume = 0;
                  }
               }
               else if (high - tick.Value >= Range)
               {   // Overshooting the downside
                  close = high - Range;
                  low = close;
                  bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCountInBar, upTick, openDate));
                  TimeSpan timeSpan = minSpan;
                  while (close - Range >= tick.Value)
                  {
                     open = high = close;
                     close = low = close - Range;
                     bars.Add(new StockBar(open, high, low, close, 0, 0, 0, 0, tick.Date + timeSpan));
                     timeSpan = timeSpan.Add(minSpan);
                  }
                  tickCountInBar = 1;
                  open = close;
                  low = close;
                  high = close;
                  volume = tick.Volume;
                  openDate = tick.Date;
                  if (tick.UpTick)
                  {
                     upTick = 1;
                     upVolume = tick.Volume;
                  }
                  else
                  {
                     upTick = 0;
                     upVolume = 0;
                  }
               }
               else
               { // We are in the range
                  tickCountInBar++;
                  low = Math.Min(low, tick.Value);
                  high = Math.Max(high, tick.Value);
                  close = tick.Value;
                  volume += tick.Volume;
                  if (tick.UpTick)
                  {
                     upTick++;
                     upVolume += tick.Volume;
                  }
               }
            }
            else
            {   // First tick in bar
               if (bars.Count == 0)
               {   // This is the first tick ever
                  tickCountInBar = 1;
                  close = low = high = open = tick.Value;
                  volume = tick.Volume;
                  openDate = tick.Date;
                  if (tick.UpTick)
                  {
                     upTick = 1;
                     upVolume = volume;
                  }
                  else
                  {
                     upTick = 0;
                     upVolume = 0;
                  }
               }
            }
         }
         bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCountInBar, upTick, openDate));
         if ((high - low) < this.Range)
         {
            bars.Last().Complete = false;
         }
         this.StockBars = bars.ToArray();
      }
      private void AppendTickBars(StockTick[] ticks)
      {
         int tickCount = 0;
         double low = double.MaxValue;
         double high = double.MinValue;
         double open = 0, close = 0;
         long volume = 0, upVolume = 0;
         int upTick = 0;
         List<StockBar> bars = null;
         DateTime openDate = new DateTime();

         if (this.StockBars != null)
         {
            StockBar lastBar = this.StockBars[this.StockBars.Length - 1];
            bars = this.StockBars.ToList();
            if (!lastBar.Complete)
            {
               tickCount = lastBar.TICK;
               open = lastBar.OPEN;
               high = lastBar.HIGH;
               low = lastBar.LOW;
               close = lastBar.CLOSE;
               volume = lastBar.VOLUME;
               upVolume = lastBar.UPVOLUME;
               upTick = lastBar.UPTICK;
               openDate = lastBar.DATE;
               bars.RemoveAt(bars.Count - 1);
            }
         }
         else
         {
            bars = new List<StockBar>();
         }
         foreach (StockTick tick in ticks)
         {
            if (tickCount == this.TickNumber)
            {
               bars.Add(new StockBar(open, high, low, close, volume, upVolume, this.TickNumber, upTick, openDate));
               tickCount = 0;
            }
            if (tickCount == 0)
            { // This is the first tick of a new bar
               tickCount = 1;
               close = low = high = open = tick.Value;
               volume = tick.Volume;
               openDate = tick.Date;
               if (tick.UpTick)
               {
                  upTick = 1;
                  upVolume = volume;
               }
               else
               {
                  upTick = 0;
                  upVolume = 0;
               }
            }
            else
            {
               tickCount++;
               low = Math.Min(low, tick.Value);
               high = Math.Max(high, tick.Value);
               close = tick.Value;
               volume += tick.Volume;
               if (tick.UpTick)
               {
                  upTick++;
                  upVolume += tick.Volume;
               }
            }
         }
         bars.Add(new StockBar(open, high, low, close, volume, upVolume, tickCount, upTick, openDate));
         if (tickCount < this.TickNumber)
         {
            bars.Last().Complete = false;
         }
         this.StockBars = bars.ToArray();
      }
      #endregion
      #region FILE IO
      public void SaveTofile(string fileName)
      {
         using (StreamWriter sr = new StreamWriter(fileName, false))
         {
            sr.WriteLine(StockBar.StringFormat());
            foreach (StockBar bar in StockBars)
            {
               sr.WriteLine(bar.ToString());
            }
         }
      }
      public void LoadFromFile(string fileName)
      {
         using (StreamReader sr = new StreamReader(fileName))
         {
            sr.ReadLine(); // Skip file header
            string line = string.Empty;
            List<StockBar> barList = new List<StockBar>();
            while (!sr.EndOfStream)
            {
               barList.Add(new StockBar(sr.ReadLine()));
            }
         }
      }
      #endregion
   }
}
