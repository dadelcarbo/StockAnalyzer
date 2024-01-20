using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Telerik.Windows.Controls.ChartView;
using static StockAnalyzer.StockMath.FloatSerie;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    public class StatisticsViewModel : NotifyPropertyChangedBase
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
        static public Array BarDurations => Enum.GetValues(typeof(BarDuration));

        private BarDuration barDuration;
        public BarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

        private static readonly List<string> indicatorTypes = new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Decorator", "Cloud", "AutoDrawing" };

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
        public List<HistogramBucket> Histogram { get; private set; }
        public List<HistogramBucket> Histogram2 { get; private set; }

        public StatisticsViewModel(string indicator, string eventName)
        {
            this.IndicatorType = "Indicator";
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

            var variations = new List<float>();

            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (!stockSerie.Initialise())
                    return false;
                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                variations.AddRange(stockSerie.GetSerie(StockDataType.VARIATION));
            }

            var varSerie = new FloatSerie(variations);
            this.Histogram = varSerie.Histogram(0.001f);
            var mean = varSerie.Average();
            var stdev = varSerie.CalculateStdev();

            var k = 1 / (stdev * Math.Sqrt(2 * Math.PI));
            var scale = (float)k / this.Histogram.Max(h => h.Count);
            var max = this.Histogram.Max(h => h.Count);

            foreach (var item in Histogram)
            {
                item.Y = item.Count * scale;
            }
            this.Histogram2 = this.Histogram.Select(h => new HistogramBucket { Y = (float)(k * Math.Exp(-0.5 * (Math.Pow((h.Value - mean) / stdev, 2)))), Value = h.Value }).ToList();

            OnPropertyChanged("Histogram");
            OnPropertyChanged("Histogram2");

            return true;
        }

        public override string ToString()
        {
            return this.Indicator + "=>" + this.Event;
        }
    }
}
