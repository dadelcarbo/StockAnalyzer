using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.ConditionalStatisticsDlg
{
    public class CondStatisticsViewModel : NotifyPropertyChangedBase
    {
        public static List<String> IndicatorTypes => indicatorTypes;

        static public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));

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

        static public IList<StockBarDuration> BarDurations => StockBarDuration.Values;

        private StockBarDuration barDuration1;
        public StockBarDuration BarDuration { get { return barDuration1; } set { if (value != barDuration1) { barDuration1 = value; OnPropertyChanged("BarDuration"); } } }

        private static List<string> indicatorTypes = new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Decorator" };

        private string indicatorType1;
        public string IndicatorType1 { get { return indicatorType1; } set { if (value != indicatorType1) { indicatorType1 = value; OnPropertyChanged("IndicatorType1"); } } }

        private string indicatorType2;
        public string IndicatorType2 { get { return indicatorType2; } set { if (value != indicatorType2) { indicatorType2 = value; OnPropertyChanged("IndicatorType2"); } } }

        private string indicator1;
        public string Indicator1
        {
            get { return indicator1; }
            set
            {
                if (value != indicator1)
                {
                    indicator1 = value;

                    IStockViewableSeries viewableSeries =
                       StockViewableItemsManager.GetViewableItem(this.indicatorType1.ToUpper() + "|" + this.Indicator1);

                    this.Events1 = (viewableSeries as IStockEvent).EventNames;

                    OnPropertyChanged("Events1");
                    OnPropertyChanged("Indicator1");
                }
            }
        }
        private string indicator2;
        public string Indicator2
        {
            get { return indicator2; }
            set
            {
                if (value != indicator2)
                {
                    indicator2 = value;

                    IStockViewableSeries viewableSeries =
                       StockViewableItemsManager.GetViewableItem(this.indicatorType2.ToUpper() + "|" + this.Indicator2);

                    this.Events2 = (viewableSeries as IStockEvent).EventNames;

                    OnPropertyChanged("Events2");
                    OnPropertyChanged("Indicator2");
                }
            }
        }

        public string[] Events1 { get; set; }
        public string[] Events2 { get; set; }

        private string event1Name;
        private int event1Index = 2;

        public string Event1
        {
            get { return event1Name; }
            set
            {
                if (value != event1Name)
                {
                    event1Name = value;
                    event1Index = this.Events1.ToList().IndexOf(event1Name);
                    OnPropertyChanged("Event1");
                }
            }
        }
        private string event2Name;
        private int event2Index = 2;

        public string Event2
        {
            get { return event2Name; }
            set
            {
                if (value != event2Name)
                {
                    event2Name = value;
                    event2Index = this.Events2.ToList().IndexOf(event2Name);
                    OnPropertyChanged("Event2");
                }
            }
        }

        private ObservableCollection<CondStatisticsResult> results;
        public ObservableCollection<CondStatisticsResult> Results { get { return results; } set { if (value != results) { results = value; OnPropertyChanged("Results"); } } }

        private ObservableCollection<CondStatisticsResult> summary;
        public ObservableCollection<CondStatisticsResult> Summary { get { return summary; } set { if (value != summary) { summary = value; OnPropertyChanged("Summary"); } } }

        public CondStatisticsViewModel(string indicatorType1, string indicator1, string event1Name, string indicatorType2, string indicator2, string event2Name)
        {
            this.IndicatorType1 = indicatorType1;
            Indicator1 = indicator1;
            this.Event1 = event1Name;

            this.IndicatorType2 = indicatorType2;
            Indicator2 = indicator2;
            this.Event2 = event2Name;

            this.Results = new ObservableCollection<CondStatisticsResult>();
            this.Summary = new ObservableCollection<CondStatisticsResult>();
            this.BarDuration = StockBarDuration.Daily;
            this.Group = StockSerie.Groups.CAC40;
        }

        public void CalculateCondProb()
        {
            this.Results.Clear();
            this.Summary.Clear();
            foreach (var serie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.Group) && s.Initialise()))
            {
                this.Calculate(serie);
            }
            this.Summary.Add(new CondStatisticsResult()
            {
                Name = "Total",
                Event1Count = this.Results.Sum(r => r.Event1Count),
                Event2Count = this.Results.Sum(r => r.Event2Count),
                Event1n2Count = this.Results.Sum(r => r.Event1n2Count),
                TotalCount = this.Results.Sum(r => r.TotalCount)
            });
        }

        public bool Calculate(StockSerie stockSerie)
        {
            stockSerie.BarDuration = this.BarDuration;

            IStockEvent stockEvent1 = StockViewableItemsManager.GetViewableItem(this.indicatorType1.ToUpper() + "|" + this.Indicator1, stockSerie) as IStockEvent;
            var event1 = stockEvent1.Events[event1Index];
            IStockEvent stockEvent2 = StockViewableItemsManager.GetViewableItem(this.indicatorType2.ToUpper() + "|" + this.Indicator2, stockSerie) as IStockEvent;
            var event2 = stockEvent2.Events[event2Index];

            int e1Count = 0, e2Count = 0, e1n2Count = 0, totalCount = 0;
            for (int i = 50; i < stockSerie.Count; i++)
            {
                bool e1 = event1[i];
                bool e2 = event2[i];
                if (e1) e1Count++;
                if (e2) e2Count++;
                if (e1 && e2) e1n2Count++;
                totalCount++;
            }
            this.Results.Add(new CondStatisticsResult() { Name = stockSerie.StockName, Event1Count = e1Count, Event2Count = e2Count, Event1n2Count = e1n2Count, TotalCount = totalCount });
            return true;
        }
    }
}
