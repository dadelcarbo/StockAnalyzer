using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.StatisticsDlg
{
    public class StatisticsViewModel : NotifyPropertyChangedBase
    {
        public static List<String> IndicatorTypes
        {
            get { return indicatorTypes; }
        }

        static public Array Groups
        {
            get { return Enum.GetValues(typeof(StockSerie.Groups)); }
        }

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                }
            }
        }
        static public Array BarDurations
        {
            get { return Enum.GetValues(typeof(StockBarDuration)); }
        }

        private StockBarDuration barDuration1;
        public StockBarDuration BarDuration { get { return barDuration1; } set { if (value != barDuration1) { barDuration1 = value; OnPropertyChanged("BarDuration"); } } }

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
                    eventIndex = this.Events.ToList().IndexOf(eventName);
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

        private float s1Percent;
        public float S1Percent { get { return s1Percent; } set { if (value != s1Percent) { s1Percent = value; OnPropertyChanged("S1Percent"); } } }

        private float r1Percent;
        public float R1Percent { get { return r1Percent; } set { if (value != r1Percent) { r1Percent = value; OnPropertyChanged("R1Percent"); } } }

        private ObservableCollection<StatisticsResult> results;
        public ObservableCollection<StatisticsResult> Results { get { return results; } set { if (value != results) { results = value; OnPropertyChanged("Results"); } } }

        private ObservableCollection<StatisticsResult> summary;
        public ObservableCollection<StatisticsResult> Summary { get { return summary; } set { if (value != summary) { summary = value; OnPropertyChanged("Summary"); } } }

        public StatisticsViewModel(string indicator, string eventName, int lookbackPeriod)
        {
            lookback = lookbackPeriod;
            this.IndicatorType = "PaintBar";
            Indicator = indicator;
            this.Event = eventName;
            this.Results = new ObservableCollection<StatisticsResult>();
            this.Summary = new ObservableCollection<StatisticsResult>();
            this.BarDuration = StockBarDuration.Daily;
            this.Group = StockSerie.Groups.CAC40;
            this.S1Percent = 0.15f;
            this.R1Percent = 0.15f;
        }
        public bool CalculateFixedStopProfit(string name)
        {
            S1Count = 0;
            R1Count = 0;
            R2Count = 0;
            TotalReturn = 0;

            float stopRatio = 1-this.s1Percent;
            float targetRatio = 1 + this.r1Percent;
            float stopPercent = -this.s1Percent;
            float targetPercent = this.r1Percent;

            StockSerie stockSerie = StockDictionary.StockDictionarySingleton[name];
            if (!stockSerie.Initialise()) return false;

            stockSerie.BarDuration = this.BarDuration;

            IStockEvent stockEvent = StockViewableItemsManager.GetViewableItem(this.indicatorType.ToUpper() + "|" + this.Indicator, stockSerie) as IStockEvent;

            float buy = 0, S1 = 0, R1 = 0;
            bool inPosition = false;
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            R1Count = 0;
            R2Count = 0;
            S1Count = 0;
            totalReturn = 0f;
            int nbClosedPosition = 0;
            for (int i = 0; !inPosition && i < stockSerie.Count - 50; i++)
            {
                if (stockEvent.Events[eventIndex][i])
                {
                    // Open position whitout condition
                    buy = closeSerie[i];
                    S1 = buy * stopRatio;
                    R1 = buy * targetRatio;
                    inPosition = true;
                }

                // Manage running position
                int j;
                for (j = i + 1; inPosition && j < stockSerie.Count; j++)
                {
                    if (lowSerie[j] <= S1) // Stop loss reached, sell...
                    {
                        S1Count++;
                        inPosition = false;
                        totalReturn += stopPercent;
                        nbClosedPosition++;
                    }
                    else if (highSerie[j] > R1) // Target reached
                    {
                        R1Count++;
                        inPosition = false;
                        totalReturn += targetPercent;
                        nbClosedPosition++;
                    }
                }
                i = j;
            }
            this.Results.Add(new StatisticsResult()
            {
                Name = name,
                R1Count = R1Count,
                R2Count = R2Count,
                S1Count = S1Count,
                TotalReturn = totalReturn
            });
            return true;
        }

        public bool Calculate(string name)
        {
            S1Count = 0;
            R1Count = 0;
            R2Count = 0;
            TotalReturn = 0;

            StockSerie stockSerie = StockDictionary.StockDictionarySingleton[name];
            if (!stockSerie.Initialise()) return false;

            stockSerie.BarDuration = this.BarDuration;

            IStockEvent stockEvent = StockViewableItemsManager.GetViewableItem(this.indicatorType.ToUpper() + "|" + this.Indicator, stockSerie) as IStockEvent;

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
                            //totalReturn -= 0.5f*gapPercent;
                        }
                        else
                        {
                            totalReturn -= gapPercent;
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
                    if (stockEvent.Events[eventIndex][i])
                    {
                        //S1 = trail.Series[0][i];
                        buy = closeSerie[i];
                        S1 = lowSerie.GetMin(i - lookback, i);
                        gap = closeSerie[i] - S1;
                        gapPercent = gap / closeSerie[i];

                        R1 = closeSerie[i] + gap;
                        R2 = closeSerie[i] + 2 * gap;

                        inPosition = true;
                        R1Touched = false;
                    }
                }
            }
            this.Results.Add(new StatisticsResult() { Name = name, R1Count = R1Count, R2Count = R2Count, S1Count = S1Count, TotalReturn = totalReturn });
            return true;
        }

        public override string ToString()
        {
            return this.Indicator + "=>" + this.Event + " S1: " + this.S1Count + "\tR1: " + this.R1Count + "\tR2: " + this.R2Count + "\tReturn: " + this.TotalReturn;
        }
    }
}
