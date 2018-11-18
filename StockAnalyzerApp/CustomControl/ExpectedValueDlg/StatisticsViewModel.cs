using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
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
            get { return Enum.GetValues(typeof(BarDuration)); }
        }

        private BarDuration barDuration;
        public BarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

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

                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.indicatorType.ToUpper() + "|" + this.Indicator);

                    this.Events = (viewableSeries as IStockEvent).EventNames;

                    OnPropertyChanged("Events");
                    OnPropertyChanged("Indicator");
                }
            }
        }

        public string[] Events { get; set; }

        private string eventName;

        private int eventIndex;
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
        private int eventCount;
        public int EventCount { get { return eventCount; } set { if (value != eventCount) { eventCount = value; OnPropertyChanged("EventCount"); } } }

        private int nbBars;
        public int NbBars { get { return nbBars; } set { if (value != nbBars) { nbBars = value; OnPropertyChanged("NbBars"); } } }

        private int smoothing;
        public int Smoothing { get { return smoothing; } set { if (value != smoothing) { smoothing = value; OnPropertyChanged("Smoothing"); } } }

        private ObservableCollection<StatisticsResult> results;
        public ObservableCollection<StatisticsResult> Results { get { return results; } set { if (value != results) { results = value; OnPropertyChanged("Results"); } } }
        public ObservableCollection<StatisticsResult> SummaryResults { get; set; }

        public StatisticsViewModel(string indicator, string eventName, int lookbackPeriod)
        {
            this.IndicatorType = "PaintBar";
            Indicator = indicator;
            this.Event = eventName;
            this.Results = new ObservableCollection<StatisticsResult>();
            this.SummaryResults = new ObservableCollection<StatisticsResult>();
            this.BarDuration = BarDuration.Daily;
            this.smoothing = 1;
            this.nbBars = 20;
            this.Group = StockSerie.Groups.CAC40;
        }
        public bool Calculate()
        {
            this.SummaryResults.Clear();
            this.Results.Clear();

            foreach (var stockSerie in StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (!stockSerie.Initialise()) return false;
                var results = new List<float[]>();

                stockSerie.BarDuration = new StockBarDuration(this.barDuration, this.smoothing);

                IStockEvent stockEvent = StockViewableItemsManager.GetViewableItem(this.indicatorType.ToUpper() + "|" + this.Indicator, stockSerie) as IStockEvent;

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                for (int i = 0; i < stockSerie.Count - NbBars; i++)
                {
                    if (stockEvent.Events[eventIndex][i])
                    {
                        // Event detected
                        var eventValue = closeSerie[i];
                        var eventStat = new float[this.nbBars];
                        for (int j = 0; j < NbBars; j++)
                        {
                            eventStat[j] = (closeSerie[i + j + 1] - eventValue) / eventValue;
                        }
                        results.Add(eventStat);
                    }
                }
                if (results.Count == 0)
                    continue;
                for (int i = 0; i < nbBars; i++)
                {
                    float avg = 0;
                    float max = float.MinValue;
                    float min = float.MaxValue;
                    for (int j = 0; j < results.Count; j++)
                    {
                        var val = results[j][i];
                        avg += val;
                        max = Math.Max(max, val);
                        min = Math.Min(min, val);
                    }
                    this.Results.Add(new StatisticsResult()
                    {
                        Name = stockSerie.StockName,
                        NbEvents = results.Count,
                        Index = i,
                        ExpectedValue = avg / results.Count,
                        MaxReturnValue = max,
                        MinReturnValue = min
                    });
                }
            }
            int nbEvents = this.Results.Where(r => r.Index == 0).Sum(r => r.NbEvents);
            if (nbEvents > 0)
            {
                for (int i = 0; i < nbBars; i++)
                {
                    float avg = 0;
                    float max = float.MinValue;
                    float min = float.MaxValue;

                    var indexResults = this.Results.Where(r => r.Index == i).ToList();

                    this.SummaryResults.Add(new StatisticsResult()
                    {
                        Name = "Summary",
                        NbEvents = nbEvents,
                        Index = i + 1,
                        ExpectedValue = indexResults.Sum(r => r.ExpectedValue * r.NbEvents) / nbEvents,
                        MaxReturnValue = indexResults.Max(r => r.MaxReturnValue),
                        MinReturnValue = indexResults.Min(r => r.MinReturnValue)
                    });
                }
            }
            return true;
        }

        public override string ToString()
        {
            return this.Indicator + "=>" + this.Event;
        }
    }
}
