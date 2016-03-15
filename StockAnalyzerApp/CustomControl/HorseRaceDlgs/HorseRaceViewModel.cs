using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
   public class HorseRaceViewModel : INotifyPropertyChanged
   {
      private int maxIndex = 0;

      public int MaxIndex
      {
         get { return maxIndex; }
         set
         {
            if (maxIndex != value)
            {
               maxIndex = value;
               OnPropertyChanged("MaxIndex");
            }
         }
      }

      private int minIndex = 0;

      public int MinIndex
      {
         get { return minIndex; }
         set
         {
            if (minIndex != value)
            {
               minIndex = value;
               OnPropertyChanged("MinIndex");
            }
         }
      }

      private int index = 0;
      public int Index
      {
         get { return index; }
         set
         {
            if (index != value)
            {
               index = value;
               this.CalculatePositions();
               OnPropertyChanged("Index");
            }
         }
      }

      private string group = "CAC40";
      public string Group
      {
         get { return group; }
         set
         {
            if (group != value)
            {
               group = value;
               this.InitPositions();
               OnPropertyChanged("Group");
            }
         }
      } 
      
      static List<string> groups = StockDictionary.StockDictionarySingleton.GetValidGroupNames();
      public List<string> Groups { get { return groups; } }

      private string indicator1Name;

      public string Indicator1Name
      {
         get { return indicator1Name; }
         set
         {
            if (indicator1Name != value)
            {
               indicator1Name = value;
               this.CalculatePositions();
               OnPropertyChanged("Indicator1Name");
            }
         }
      }

      private string indicator2Name;

      public string Indicator2Name
      {
         get { return indicator2Name; }
         set
         {
            if (indicator2Name != value)
            {
               indicator2Name = value;
               this.CalculatePositions();
               OnPropertyChanged("Indicator2Name");
            }
         }
      }

      static List<int> ranges = new List<int>(){-1,-5,-20,-100,-200}; 
      public List<int> Ranges { get { return ranges; } }

      public HorseRaceViewModel()
      {
         minIndex = -200;
         maxIndex = 0;
         index = 0;

         this.StockPositions = new ObservableCollection<StockPosition>();

         this.indicator1Name = "STOKF(100,20,75,25)";
         this.indicator2Name = "STOKF(50,20,75,25)";
      }
      
      private void CalculatePositions()
      {
         foreach (StockPosition stockPosition in this.StockPositions)
         {
            float startClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + minIndex - 1)).CLOSE;
            float endClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + index - 1)).CLOSE;
            stockPosition.Close = endClose;
            stockPosition.Variation = 100f * (endClose - startClose) / startClose;
            int currentIndex = Math.Max(0, stockPosition.StockSerie.Count + index - 1);
            int previousIndex = Math.Max(0, stockPosition.StockSerie.Count + index - 2);

            IStockIndicator indicator1 = stockPosition.StockSerie.GetIndicator(this.indicator1Name);
            stockPosition.Indicator1 = indicator1.Series[0][currentIndex];
            stockPosition.Indicator1Up = indicator1.Series[0][previousIndex] <= stockPosition.Indicator1 ;
            
            IStockIndicator indicator2 = stockPosition.StockSerie.GetIndicator(this.indicator2Name);
            stockPosition.Indicator2 = indicator2.Series[0][currentIndex];
            stockPosition.Indicator2Up = indicator2.Series[0][previousIndex] <= stockPosition.Indicator2;
         }
      }

      private void InitPositions()
      {
         this.StockPositions.Clear();
         List<StockPosition> positions = new List<StockPosition>();

         StockSplashScreen.ShowSplashScreen();
         StockSplashScreen.ProgressText = "Intializing Horse Race";
         foreach (StockSerie stockSerie in StockDictionary.StockDictionarySingleton.Values.Where(s=>s.BelongsToGroup(this.group)))
         {
            StockSplashScreen.ProgressSubText= "Intializing " + stockSerie.StockName;
            if (stockSerie.Initialise())
            {
               positions.Add(new StockPosition() {Variation = stockSerie.Values.Last().VARIATION * 100f, StockSerie = stockSerie});
            }
         }
         this.StockPositions = new ObservableCollection<StockPosition>(positions);
         this.CalculatePositions();

         StockSplashScreen.CloseForm(true);
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      private List<StockSerie> stockList;

      public List<StockSerie> StockList
      {
         get { return stockList; }
         set
         {
            if (stockList != value)
            {
               stockList = value;
               this.InitPositions();
               OnPropertyChanged("StockList");
            }
         }
      }

      private ObservableCollection<StockPosition> stockPositions;

      public ObservableCollection<StockPosition> StockPositions
      {
         get { return stockPositions; }
         set
         {
            if (stockPositions != value)
            {
               stockPositions = value;
               this.CalculatePositions();
               OnPropertyChanged("StockPositions");
            }
         }
      }
   }

   public class StockPosition : INotifyPropertyChanged
   {
      public string Name
      {
         get { return stockSerie!=null?stockSerie.StockName:"null"; }
      }

      private float close = 0;
      public float Close
      {
         get { return close; }
         set
         {
            if (close != value)
            {
               close = value;
               OnPropertyChanged("Close");
            }
         }
      }

      
      private float variation = 0;
      public float Variation
      {
         get { return variation; }
         set
         {
            if (variation != value)
            {
               variation = value;
               OnPropertyChanged("Variation");
            }
         }
      }
      private float indicator1 = 0;
      public float Indicator1
      {
         get { return indicator1; }
         set
         {
            if (indicator1 != value)
            {
               indicator1 = value;
               OnPropertyChanged("Indicator1");
            }
         }
      }

      private bool indicator1up = false;
      public bool Indicator1Up
      {
         get { return indicator1up; }
         set
         {
            if (indicator1up != value)
            {
               indicator1up = value;
               OnPropertyChanged("Indicator1Up");
            }
         }
      }

      private float indicator2 = 0;
      public float Indicator2
      {
         get { return indicator2; }
         set
         {
            if (indicator2 != value)
            {
               indicator2 = value;
               OnPropertyChanged("Indicator2");
            }
         }
      }

      private bool indicator2up = false;
      public bool Indicator2Up
      {
         get { return indicator2up; }
         set
         {
            if (indicator2up != value)
            {
               indicator2up = value;
               OnPropertyChanged("Indicator2Up");
            }
         }
      }

      private StockSerie stockSerie;
      public StockSerie StockSerie
      {
         get { return stockSerie; }
         set
         {
            if (stockSerie != value)
            {
               stockSerie = value;
               OnPropertyChanged("Name");
            }
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }
   }
}