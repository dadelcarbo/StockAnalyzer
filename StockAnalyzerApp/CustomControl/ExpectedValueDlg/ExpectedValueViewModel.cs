using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    public class ExpectedValueViewModel : NotifyPropertyChangedBase
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

        private static List<string> indicatorTypes = new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Decorator", "Cloud" };

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

                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.indicatorType1.ToUpper() + "|" + this.indicator1);
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

                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.indicatorType2.ToUpper() + "|" + this.indicator2);
                    this.Events2 = (viewableSeries as IStockEvent).EventNames;

                    OnPropertyChanged("Events2");
                    OnPropertyChanged("Indicator2");
                }
            }
        }

        public string[] Events1 { get; set; }
        public string[] Events2 { get; set; }

        private string eventName1;
        private int eventIndex1;
        public string Event1
        {
            get { return eventName1; }
            set
            {
                if (value != eventName1)
                {
                    eventName1 = value;
                    eventIndex1 = this.Events1.ToList().IndexOf(eventName1);
                    OnPropertyChanged("Event1");
                }
            }
        }
        private string eventName2;
        private int eventIndex2;
        public string Event2
        {
            get { return eventName2; }
            set
            {
                if (value != eventName2)
                {
                    eventName2 = value;
                    eventIndex2 = this.Events2.ToList().IndexOf(eventName2);
                    OnPropertyChanged("Event2");
                }
            }
        }

        private float stop;
        public float Stop
        {
            get { return stop; }
            set
            {
                if (value != stop)
                {
                    stop = value;
                    OnPropertyChanged("Stop");
                }
            }
        }

        private ObservableCollection<TradeResult> results;
        public ObservableCollection<TradeResult> Results { get { return results; } set { if (value != results) { results = value; OnPropertyChanged("Results"); } } }
        public ObservableCollection<TradeResult> SummaryResults { get; set; }

        public ExpectedValueViewModel(string indicator, string event1Name, string event2Name)
        {
            this.IndicatorType1 = "PaintBar";
            Indicator1 = indicator;
            this.IndicatorType2 = "PaintBar";
            Indicator2 = indicator;
            this.Event1 = event1Name;
            this.Event2 = event2Name;
            this.Results = new ObservableCollection<TradeResult>();
            this.SummaryResults = new ObservableCollection<TradeResult>();
            this.BarDuration = BarDuration.Daily;
            this.Group = StockSerie.Groups.COUNTRY;
        }
        public bool Calculate()
        {
            this.SummaryResults.Clear();
            this.Results.Clear();
            var trades = new List<StockTrade>();

            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (!stockSerie.Initialise())
                    continue;
                stockSerie.BarDuration = this.barDuration;

                var stockEvent1 = StockViewableItemsManager.GetViewableItem(this.indicatorType1.ToUpper() + "|" + this.Indicator1, stockSerie) as IStockEvent;
                var stockEvent2 = StockViewableItemsManager.GetViewableItem(this.indicatorType2.ToUpper() + "|" + this.Indicator2, stockSerie) as IStockEvent;

                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                StockTrade trade = null;
                float stopLoss = 0;
                for (int i = 0; i < stockSerie.Count; i++)
                {
                    if (trade == null) // look for opening a position
                    {
                        if (stockEvent1.Events[eventIndex1][i])
                        {
                            trade = new StockTrade(stockSerie, i + 1);
                            stopLoss = this.stop == 0 ? 0 : trade.EntryValue * (1 - 0.01f * this.Stop);
                        }
                    }
                    else
                    {
                        var stopTouched = stopLoss > 0 && lowSerie[i] < stopLoss;
                        if (stopTouched)
                        {
                            trade.Close(i, stopLoss);
                            trades.Add(trade);
                            trade = null;
                        }
                        else if (stockEvent2.Events[eventIndex2][i])
                        {
                            trade.CloseAtOpen(i + 1);
                            trades.Add(trade);
                            trade = null;
                        }
                    }
                }
            }
            foreach (var group in trades.GroupBy(t => t.Serie))
            {
                var winners = group.Where(t => t.Gain > 0).Select(t => t.Gain).ToList();
                var losers = group.Where(t => t.Gain < 0).Select(t => t.Gain).ToList();
                this.results.Add(new TradeResult
                {
                    Name = group.Key.StockName,
                    NbWin = winners.Count,
                    NbLoss = losers.Count,
                    AvgGain = winners.Average(),
                    AvgLoss = losers.Average(),
                    MaxGain = winners.Max(),
                    MaxLoss = losers.Min(),
                    ExpectedValue = group.Average(t => t.Gain)
                }); ;
            }
            return true;
        }

        public override string ToString()
        {
            return this.Indicator1 + "=>" + this.Event1;
        }
    }
}
