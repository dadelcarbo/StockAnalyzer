using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses
{
   public class ShortInterestValue
   {
      public enum ShortInterestValueType
      {
         ShortInterest,
         Volume,
         DaysToCover
      }
      public DateTime Date { get; set; }
      public float ShortInterest { get; set; }
      public float Volume { get; set; }
      public float DaysToCover { get; set; }

      public ShortInterestValue(DateTime date, float shortInterest, float volume, float daysToCover)
      {
         this.Date = date;
         this.ShortInterest = shortInterest;
         this.Volume = volume;
         this.DaysToCover = daysToCover;
      }
      static public string StringFormat()
      {
         return "Date,ShortInterest,Volume,DaysToCover";
      }
      public override string ToString()
      {
         return Date.ToString("s") + "," + ShortInterest.ToString() + "," + Volume.ToString() + "," + DaysToCover.ToString();
      }
      public static ShortInterestValue Parse(string line)
      {
         string[] fields = line.Split(',');
         return new ShortInterestValue(DateTime.Parse(fields[0]), float.Parse(fields[1]), float.Parse(fields[2]), float.Parse(fields[3]));
      }

   }
   public class ShortInterestSerie : SortedDictionary<DateTime, ShortInterestValue>
   {
      public string ShortInterestSerieName { get; set; }
      public FloatSerie[] ValueSeries { get; set; }
      public bool IsInitialised { get; set; }

      public ShortInterestSerie(string shortInterestSerieName)
      {
         this.ShortInterestSerieName = shortInterestSerieName;
         this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(ShortInterestValue.ShortInterestValueType)).Length];
         this.IsInitialised = false;
      }

      public FloatSerie GetSerie(ShortInterestValue.ShortInterestValueType cotType)
      {
         if (ValueSeries[(int)cotType] == null)
         {
            this.Initialise();
         }
         return ValueSeries[(int)cotType];
      }


      static private string SI_ARCHIVE_SUBFOLDER = @"\data\archive\daily\ShortInterest";
      public bool Initialise()
      {
         if (this.IsInitialised)
         {
            return true;
         }
         try
         {
            if (this.Count == 0)
            {
               this.LoadFromFile(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + SI_ARCHIVE_SUBFOLDER + @"\" + this.ShortInterestSerieName + ".csv");
            }
            foreach (ShortInterestValue.ShortInterestValueType cotType in Enum.GetValues(typeof(ShortInterestValue.ShortInterestValueType)))
            {
               System.Reflection.PropertyInfo pi = typeof(ShortInterestValue).GetProperty(cotType.ToString());
               FloatSerie valueSerie = new FloatSerie(this.Values.Count);
               int i = 0;
               foreach (ShortInterestValue cotValue in this.Values)
               {
                  valueSerie[i] = (float)pi.GetValue(cotValue, null);
                  i++;
               }
               valueSerie.Name = cotType.ToString();
               ValueSeries[(int)cotType] = valueSerie;
            }
            this.IsInitialised = true;
         }
         catch (System.Exception e)
         {
            StockLog.Write("Exception occured initialising the ShortInterestSerie: " + this.ShortInterestSerieName + " " + e.Message);
         }
         return this.IsInitialised;
      }
      public bool SaveToFile()
      {
         bool res = false;
         try
         {
            string fileName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + SI_ARCHIVE_SUBFOLDER + @"\" + this.ShortInterestSerieName + ".csv";
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
               sw.WriteLine(ShortInterestValue.StringFormat());
               foreach (ShortInterestValue value in this.Values)
               {
                  sw.WriteLine(value.ToString());
               }
               res = true;
            }
         }
         catch (System.Exception e)
         {
            StockLog.Write("Exception occured saving the ShortInterestSerie: " + this.ShortInterestSerieName + " " + e.Message);
         }
         return res;
      }

      public static ShortInterestSerie ReadFromFile(string fileName, string serieName)
      {
         ShortInterestSerie shortInterestSerie = new ShortInterestSerie(serieName);
         shortInterestSerie.LoadFromFile(fileName);
         return shortInterestSerie;
      }

      private void LoadFromFile(string fileName)
      {
         try
         {
            ShortInterestValue value;
            using (StreamReader sr = new StreamReader(fileName, false))
            {
               sr.ReadLine(); // Skip header line
               while (!sr.EndOfStream)
               {
                  value = ShortInterestValue.Parse(sr.ReadLine());
                  this.Add(value.Date, value);
               }
            }
         }
         catch (System.Exception e)
         {
            StockLog.Write("Exception occured parsing the ShortInterestSerie: " + this.ShortInterestSerieName + " " + e.Message);
         }
      }
      public FloatSerie GetSerie(ShortInterestValue.ShortInterestValueType shortInterestValueType, DateTime[] dateList)
      {         
         System.Reflection.PropertyInfo pi = typeof(ShortInterestValue).GetProperty(shortInterestValueType.ToString());

         this.Initialise();
         List<float> serie = new List<float>();
         int i = 0, j = 0;
         DateTime [] siDates = this.Keys.ToArray();
         while(i < dateList.Length && dateList[i] < this.Keys.First())
         {
            serie.Add(0f);
            i++;
         }
         while (j < siDates.Length && siDates[j] < dateList[0])
         {
            j++;
         }
         float previousSI = j > 0 ? (float)pi.GetValue(this.Values.ElementAt(j - 1), null) : (float)pi.GetValue(this.Values.First(), null);
         for (; i < dateList.Length; i++)
         {
            if (dateList[i] >= siDates[j])
            {
               ShortInterestValue siValue = this[siDates[j]];

               serie.Add(previousSI = (float)pi.GetValue(siValue, null));
               if (j<siDates.Length-1) j++;
            }
            else
            {
               serie.Add(previousSI);
            }
         }
         return new FloatSerie(serie.ToArray(), shortInterestValueType.ToString());
      }
   }
}
