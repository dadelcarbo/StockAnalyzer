using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;

namespace StockAnalyzerApp.CustomControl.StatisticsDlg
{
   public class StatisticsViewModel : NotifyPropertyChanged
   {
      public static List<String> IndicatorTypes {
         get { return indicatorTypes; }
      }
         
      private static List<string> indicatorTypes = new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Decorator" };

      private string indicatorType;
      public string IndicatorType { get { return indicatorType; } set { if (value != indicatorType) { indicatorType = value; OnPropertyChanged("IndicatorType"); } } }

      private string indicator;
      public string Indicator
      {
         get { return indicator; }
         set
         {
            if (value != indicator)
            {
               indicator = value;

               IStockViewableSeries viewableSeries =
                  StockViewableItemsManager.GetViewableItem(this.indicatorType.ToUpper() + "|" + this.Indicator);


               this.Events = (viewableSeries as IStockEvent).EventNames;
               
               OnPropertyChanged("Events");
               OnPropertyChanged("Indicator");
            }
         }
      }

      public string[] Events { get; set; }

      private string eventName;

      public string Event
      {
         get { return eventName; }
         set
         {
            if (value != eventName)
            {
               eventName = value;
               eventIndex = this.Event.IndexOf(eventName);
               OnPropertyChanged("Event");
            }
         }
      }

      private int lookback;
      public int Lookback { get { return lookback; } set { if (value != lookback) { lookback = value; OnPropertyChanged("Lookback"); } } }

      private int s1Count;
      public int S1Count { get { return s1Count; } set { if (value != s1Count) { s1Count = value; OnPropertyChanged("S1Count"); } } }

      private int r1Count;
      public int R1Count { get { return r1Count; } set { if (value != r1Count) { r1Count = value; OnPropertyChanged("R1Count"); } } }

      private int r2Count;
      public int R2Count { get { return r2Count; } set { if (value != r2Count) { r2Count = value; OnPropertyChanged("R2Count"); } } }

      private float totalReturn;
      public float TotalReturn { get { return totalReturn; } set { if (value != totalReturn) { totalReturn = value; OnPropertyChanged("TotalReturn"); } } }

      private int eventIndex = 2;

      private ObservableCollection<StatisticsResult> results;
      public ObservableCollection<StatisticsResult> Results { get { return results; } set { if (value != results) { results = value; OnPropertyChanged("Results"); } } } 

      public StatisticsViewModel(string indicator, string eventName, int lookbackPeriod)
      {
         lookback = lookbackPeriod;
         this.IndicatorType = "TrailStop";
         Indicator = indicator;
         this.Event = eventName;
         this.Results = new ObservableCollection<StatisticsResult>();
      }

      public void Calculate(string name)
      {
         S1Count = 0;
         R1Count = 0;
         R2Count = 0;
         TotalReturn = 0;

         StockSerie stockSerie = StockDictionary.StockDictionarySingleton[name];
         if (!stockSerie.Initialise()) return;

         IStockTrailStop trail = stockSerie.GetTrailStop(Indicator);

         bool inPosition = false;
         bool R1Touched = false;
         float gap = 0, gapPercent = 0, buy = 0, S1 = 0, R1 = 0, R2 = 0;
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
         R1Count = 0;
         R2Count = 0;
         S1Count = 0;
         for (int i = 50; i < stockSerie.Count; i++)
         {
            if (inPosition)
            {
               if (lowSerie[i] < S1)
               {
                  Console.WriteLine("S1");
                  S1Count++;
                  inPosition = false;
                  if (R1Touched)
                  {
                     totalReturn -= 0.5f*gapPercent;
                  }
                  else
                  {
                     //totalReturn -= gapPercent;
                  }
               }
               else
               {
                  float high = highSerie[i];
                  if (!R1Touched && high > R1)
                  {
                     Console.WriteLine("R1");
                     R1Touched = true;
                     R1Count++;
                     totalReturn += 0.5f * gapPercent;
                     S1 = buy;
                  }
                  if (high > R2)
                  {
                     Console.WriteLine("R2");
                     R2Count++;
                     inPosition = false;
                     totalReturn += gapPercent;
                  }
               }
            }
            else
            {
               if (trail.Events[eventIndex][i])
               {
                  //S1 = trail.Series[0][i];
                  buy = closeSerie[i];
                  S1 = lowSerie.GetMin(i - lookback, i);
                  gap = closeSerie[i] - S1;
                  gapPercent = gap/closeSerie[i];

                  R1 = closeSerie[i] + gap;
                  R2 = closeSerie[i] + 2 * gap;

                  inPosition = true;
                  R1Touched = false;
               }
            }
         }
         this.Results.Add(new StatisticsResult(){Name = name, R1Count = R1Count, R2Count = R2Count, S1Count = S1Count, TotalReturn = totalReturn});
      }

      public override string ToString()
      {
         return this.Indicator + "=>" + this.Event + " S1: " + this.S1Count + "\tR1: " + this.R1Count + "\tR2: " + this.R2Count + "\tReturn: " + this.TotalReturn;
      }
   }
}
