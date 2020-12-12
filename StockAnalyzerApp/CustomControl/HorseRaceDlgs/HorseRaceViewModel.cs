using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
    public class HorseRaceViewModel : NotifyPropertyChangedBase
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
                    this.OnPropertyChanged("MaxIndex");
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
                    this.OnPropertyChanged("MinIndex");
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
                    this.OnPropertyChanged("Index");
                }
            }
        }

        private string group = string.Empty;
        public string Group
        {
            get { return group; }
            set
            {
                if (group != value)
                {
                    group = value;
                    this.InitPositions();
                    this.OnPropertyChanged("Group");
                }
            }
        }

        private StockBarDuration barDuration = StockBarDuration.Daily;
        public StockBarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (barDuration != value)
                {
                    barDuration = value;
                    this.InitPositions();
                    this.OnPropertyChanged("BarDuration");
                }
            }
        }

        static List<string> groups = StockDictionary.Instance.GetValidGroupNames();
        public List<string> Groups { get { return HorseRaceViewModel.groups; } }

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
                    this.OnPropertyChanged("Indicator1Name");
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
                    this.OnPropertyChanged("Indicator2Name");
                }
            }
        }

        static List<int> ranges = new List<int>() { -1, -5, -20, -100, -200 };
        public List<int> Ranges { get { return HorseRaceViewModel.ranges; } }

        public HorseRaceViewModel()
        {
            minIndex = -200;
            maxIndex = 0;
            index = 0;

            this.indicator1Name = "STOKF(100,20,75,25)";
            this.indicator2Name = "ROR(100,1,20)";

            this.stockPositions = new ObservableCollection<StockPosition>();
        }

        private void CalculatePositions()
        {
            float min1 = float.MaxValue;
            float min2 = float.MaxValue;
            float max1 = float.MinValue;
            float max2 = float.MinValue;

            foreach (StockPosition stockPosition in this.StockPositions)
            {
                if (!stockPosition.StockSerie.Initialise()) continue;

                float startClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + minIndex - 1)).CLOSE;
                float endClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + index - 1)).CLOSE;
                stockPosition.Close = endClose;
                stockPosition.Variation = 100f * (endClose - startClose) / startClose;
                int currentIndex = Math.Max(0, stockPosition.StockSerie.Count + index - 1);
                int previousIndex = Math.Max(0, stockPosition.StockSerie.Count + index - 2);

                IStockIndicator indicator1 = stockPosition.StockSerie.GetIndicator(this.indicator1Name);
                stockPosition.Indicator1 = indicator1.Series[0][currentIndex];
                stockPosition.Indicator1Up = indicator1.Series[0][previousIndex] <= stockPosition.Indicator1;
                min1 = Math.Min(min1, stockPosition.Indicator1);
                max1 = Math.Max(max1, stockPosition.Indicator1);

                IStockIndicator indicator2 = stockPosition.StockSerie.GetIndicator(this.indicator2Name);
                stockPosition.Indicator2 = indicator2.Series[0][currentIndex];
                stockPosition.Indicator2Up = indicator2.Series[0][previousIndex] <= stockPosition.Indicator2;
                min2 = Math.Min(min2, stockPosition.Indicator2);
                max2 = Math.Max(max2, stockPosition.Indicator2);
            }

            IStockIndicator indicator = StockIndicatorManager.CreateIndicator(this.Indicator1Name);
            if (indicator is IRange)
            {
                IRange range = indicator as IRange;
                foreach (var position in this.stockPositions)
                {
                    position.MinIndicator1 = range.Min;
                    position.MaxIndicator1 = range.Max;
                }
            }
            else
            {
                foreach (var position in this.stockPositions)
                {
                    position.MinIndicator1 = min1;
                    position.MaxIndicator1 = max1;
                }
            }

            indicator = StockIndicatorManager.CreateIndicator(this.Indicator2Name);
            if (indicator is IRange)
            {
                IRange range = indicator as IRange;
                foreach (var position in this.stockPositions)
                {
                    position.MinIndicator2 = range.Min;
                    position.MaxIndicator2 = range.Max;
                }
            }
            else
            {
                foreach (var position in this.stockPositions)
                {
                    position.MinIndicator2 = min2;
                    position.MaxIndicator2 = max2;
                }
            }

            this.stockPositions = StockPositions.OrderByDescending(p => p.Indicator2);

            this.OnPropertyChanged("StockPositions");
        }

        private void InitPositions()
        {
            if (string.IsNullOrEmpty(this.group))
            {
                return;
            }

            List<StockPosition> positions = new List<StockPosition>();

            var series = StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group));

            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ProgressMax = series.Count();
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ShowSplashScreen();
            StockSplashScreen.ProgressText = "Intializing Horse Race";
            foreach (StockSerie stockSerie in series)
            {
                StockSplashScreen.ProgressSubText = "Intializing " + stockSerie.StockName;
                if (stockSerie.Initialise())
                {
                    stockSerie.BarDuration = this.barDuration;
                    positions.Add(new StockPosition() { Variation = stockSerie.Values.Last().VARIATION * 100f, StockSerie = stockSerie });
                }
            }
            this.StockPositions = positions.OrderByDescending(p => p.Indicator2);

            StockSplashScreen.CloseForm(true);
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
                    this.OnPropertyChanged("StockList");
                }
            }
        }

        private IEnumerable<StockPosition> stockPositions;

        public IEnumerable<StockPosition> StockPositions
        {
            get { return stockPositions; }
            set
            {
                if (stockPositions != value)
                {
                    stockPositions = value;
                    this.CalculatePositions();
                }
            }
        }
    }

    public class StockPosition : INotifyPropertyChanged
    {
        public string Name
        {
            get { return stockSerie != null ? stockSerie.StockName : "null"; }
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

        private float minIndicator1 = 0;
        public float MinIndicator1
        {
            get { return minIndicator1; }
            set
            {
                if (minIndicator1 != value)
                {
                    minIndicator1 = value;
                    OnPropertyChanged("MinIndicator1");
                }
            }
        }
        private float maxIndicator1 = 0;
        public float MaxIndicator1
        {
            get { return maxIndicator1; }
            set
            {
                if (maxIndicator1 != value)
                {
                    maxIndicator1 = value;
                    OnPropertyChanged("MaxIndicator1");
                }
            }
        }

        private float minIndicator2 = 0;
        public float MinIndicator2
        {
            get { return minIndicator2; }
            set
            {
                if (minIndicator2 != value)
                {
                    minIndicator2 = value;
                    OnPropertyChanged("MinIndicator2");
                }
            }
        }
        private float maxIndicator2 = 0;
        public float MaxIndicator2
        {
            get { return maxIndicator2; }
            set
            {
                if (maxIndicator2 != value)
                {
                    maxIndicator2 = value;
                    OnPropertyChanged("MaxIndicator2");
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

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}