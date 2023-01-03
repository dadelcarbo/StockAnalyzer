using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayViewModel : NotifyPropertyChangedBase
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        private StockSerie.Groups selectedGroup;
        private StockBarDuration barDuration;

        public ObservableCollection<MarketReplayPositionViewModel> Positions { get; set; }

        StockTradeSummary tradeSummary;
        public StockTradeSummary TradeSummary
        {
            get => tradeSummary;
            set
            {
                if (tradeSummary != value)
                {
                    tradeSummary = value;
                    this.OnPropertyChanged("TradeSummary");
                }
            }
        }

        private MarketReplayPositionViewModel openPosition = null;
        private StockTrade openTrade = null;

        private float _value;
        public float Value
        {
            get => _value;
            private set
            {
                if (_value != value)
                {
                    _value = value;
                    this.OnPropertyChanged("Value");

                    if (this.openPosition != null)
                    {
                        this.openPosition.SetValue(value);
                    }
                }
            }
        }
        private float stop;
        public float Stop
        {
            get => stop;
            set
            {
                if (stop != value)
                {
                    stop = value;
                    if (this.openPosition != null)
                        this.openPosition.Stop = this.Stop;
                    this.OnPropertyChanged("Stop");
                }
            }
        }

        #region Visibility Properties
        private bool stopEnabled;
        public bool StopEnabled
        {
            get => stopEnabled; set
            {
                if (stopEnabled != value)
                {
                    stopEnabled = value;
                    this.OnPropertyChanged("StopEnabled");
                }
            }
        }
        private bool forwardEnabled;
        public bool ForwardEnabled
        {
            get => forwardEnabled; set
            {
                if (forwardEnabled != value)
                {
                    forwardEnabled = value;
                    this.OnPropertyChanged("ForwardEnabled");
                }
            }
        }
        private bool fastForwardEnabled;
        public bool FastForwardEnabled
        {
            get => fastForwardEnabled; set
            {
                if (fastForwardEnabled != value)
                {
                    fastForwardEnabled = value;
                    this.OnPropertyChanged("FastForwardEnabled");
                }
            }
        }
        private bool buyEnabled;
        public bool BuyEnabled
        {
            get => buyEnabled; set
            {
                if (buyEnabled != value)
                {
                    buyEnabled = value;
                    this.OnPropertyChanged("BuyEnabled");
                }
            }
        }
        private bool sellEnabled;
        public bool SellEnabled
        {
            get => sellEnabled; set
            {
                if (sellEnabled != value)
                {
                    sellEnabled = value;
                    this.OnPropertyChanged("SellEnabled");
                }
            }
        }
        #endregion

        #region INDICATOR

        string indicatorName;
        public string IndicatorName
        {
            get => indicatorName;
            set
            {
                if (indicatorName != value)
                {
                    indicatorName = value;
                    var viewableSeries = StockViewableItemsManager.GetViewableItem(this.indicatorName);

                    this.Events = (viewableSeries as IStockEvent).EventNames;
                    this.Event = this.Events?[0];

                    OnPropertyChanged("Events");
                    OnPropertyChanged("IndicatorName");
                }
            }
        }
        public IList<string> IndicatorNames { get; set; }

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
                    OnPropertyChanged("Event");
                }
            }
        }
        #endregion

        DispatcherTimer replayTimer;
        public MarketReplayViewModel(StockSerie.Groups selectedGroup, StockBarDuration barDuration)
        {
            this.selectedGroup = selectedGroup;
            this.barDuration = barDuration;

            this.stopEnabled = true;
            this.forwardEnabled = true;
            this.fastForwardEnabled = true;
            this.buyEnabled = true;
            this.sellEnabled = false;

            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            StockAnalyzerForm.MainFrame.Portfolio = StockPortfolio.ReplayPortfolio;

            this.Positions = new ObservableCollection<MarketReplayPositionViewModel>();
            Start();

            replayTimer = new DispatcherTimer();
            replayTimer.Tick += fastForwardTimer_Tick;
            replayTimer.Interval = new TimeSpan(0, 0, 0, 0, 25); // 100 ms
            replayTimer.Start();
        }


        private ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new CommandBase(StopReplay));

        private ICommand forwardCommand;
        public ICommand ForwardCommand => forwardCommand ?? (forwardCommand = new CommandBase(Forward));

        private ICommand fastForwardCommand;
        public ICommand FastForwardCommand => fastForwardCommand ?? (fastForwardCommand = new CommandBase(FastForward));

        private ICommand skipForwardCommand;
        public ICommand SkipForwardCommand => skipForwardCommand ?? (skipForwardCommand = new CommandBase(SkipForward));

        private ICommand buyCommand;
        public ICommand BuyCommand => buyCommand ?? (buyCommand = new CommandBase<MarketReplayViewModel>(Buy, this, vm => vm.BuyEnabled, "BuyEnabled"));

        private ICommand sellCommand;
        public ICommand SellCommand => sellCommand ?? (sellCommand = new CommandBase<MarketReplayViewModel>(Sell, this, vm => vm.SellEnabled, "SellEnabled"));

        public void Closing()
        {
            this.isFastForwarding = false;
            this.isSkipForwarding = false;

            // Generate statistics
            if (replayTimer.IsEnabled)
            {
                replayTimer.Stop();
            }
        }

        private void StopReplay()
        {
            this.isFastForwarding = false;
            this.isSkipForwarding = false;
            // Generate statistics
            var log = this.TradeSummary.ToLog(barDuration);
            MessageBox.Show(referenceSerie.StockName + Environment.NewLine + log, "Trade Summary", MessageBoxButton.OK);

            this.Start();
        }
        private void Forward()
        {
            if (++referenceSerieIndex < referenceSerie.Count)
            {
                //Add value in replay serie
                CopyReferenceValues(referenceSerieIndex + 1);

                // Manage stop and target orders
                if (openPosition != null)
                {
                    var lastValue = replaySerie.Values.Last();
                    if (this.stop != 0 && lastValue.LOW < this.stop) // Stop Loss
                    {
                        sell(lastValue, openPosition.Qty, Math.Min(stop, lastValue.OPEN));
                    }
                }
                else
                {
                    var lastValue = replaySerie.Values.Last();
                    if (this.stop != 0 && lastValue.HIGH > this.stop) // Stop Loss
                    {
                        this.BuyEnabled = false;
                        this.SellEnabled = true;

                        var date = replaySerie.LastValue.DATE;
                        int qty = 2;
                        var id = StockPortfolio.ReplayPortfolio.GetNextOperationId();
                        var value = Math.Max(this.Stop, lastValue.OPEN);
                        StockPortfolio.ReplayPortfolio.AddOperation(StockOperation.FromSimu(id, date, replaySerie.StockName, StockOperation.BUY, qty, -value * qty));

                        var position = StockPortfolio.ReplayPortfolio.OpenedPositions.FirstOrDefault();
                        this.Positions.Add(openPosition = new MarketReplayPositionViewModel(position));
                        openPosition.SetValue(lastValue.CLOSE);
                        openTrade = new StockTrade(replaySerie, replaySerie.LastCompleteIndex, value);
                        this.Stop = 0;
                    }
                }
                if (!this.isSkipForwarding)
                    this.SelectedStockChanged(replaySerie.StockName, false);
            }
            else
            {
                StopReplay();
            }
        }


        bool isFastForwarding = false;
        private void FastForward()
        {
            if (isSkipForwarding) return;
            isFastForwarding = !isFastForwarding;
        }

        bool isSkipForwarding = false;
        int count = 0;
        private void SkipForward()
        {
            if (isFastForwarding) return;
            if (string.IsNullOrEmpty(indicatorName) || string.IsNullOrEmpty(eventName))
                return;
            isSkipForwarding = !isSkipForwarding;
        }

        private void fastForwardTimer_Tick(object sender, EventArgs e)
        {
            if (isFastForwarding)
            {
                if (referenceSerieIndex >= referenceSerie.Count)
                {
                    isFastForwarding = false;
                    StopReplay();
                }
                else
                {
                    Forward();
                }
            }
            else if (isSkipForwarding)
            {
                if (referenceSerieIndex >= referenceSerie.Count)
                {
                    isSkipForwarding = false;
                    StopReplay();
                }
                else
                {
                    Forward();
                    var events = replaySerie.GetViewableItem(indicatorName) as IStockEvent;
                    if (events == null)
                        return;
                    var boolSerie = events.Events[Array.IndexOf<string>(events.EventNames, eventName)];

                    if (boolSerie[referenceSerieIndex])
                    {
                        isSkipForwarding = false;
                        this.SelectedStockChanged(replaySerie.StockName, true);
                    }
                    if (++count == 5)
                    {
                        count = 0;
                        this.SelectedStockChanged(replaySerie.StockName, true);
                    }
                }
            }
        }

        private void Buy()
        {
            this.BuyEnabled = false;
            this.SellEnabled = true;

            var date = replaySerie.LastValue.DATE;
            int qty = 2;
            var id = StockPortfolio.ReplayPortfolio.GetNextOperationId();
            StockPortfolio.ReplayPortfolio.AddOperation(StockOperation.FromSimu(id, date, replaySerie.StockName, StockOperation.BUY, qty, -this.Value * qty));

            var position = StockPortfolio.ReplayPortfolio.OpenedPositions.FirstOrDefault();
            this.Positions.Add(openPosition = new MarketReplayPositionViewModel(position));
            openPosition.Stop = this.Stop;
            openTrade = new StockTrade(replaySerie, replaySerie.LastCompleteIndex, this.Value);

            this.Forward();
        }
        private void Sell()
        {
            sell(replaySerie.Values.Last(), openPosition.Qty, this._value);

            this.Forward();
        }

        private void sell(StockDailyValue lastValue, int qty, float value)
        {
            if (openPosition == null) return;

            if (openPosition.Qty == qty)
            {
                // Close full position
                var id = StockPortfolio.ReplayPortfolio.GetNextOperationId();
                StockPortfolio.ReplayPortfolio.AddOperation(StockOperation.FromSimu(id, lastValue.DATE, replaySerie.StockName, StockOperation.SELL, qty, value * qty));
                this.TradeSummary.Trades.Insert(0, openTrade);
                openTrade.Close(replaySerie.LastCompleteIndex, value);
                this.Stop = 0;
                openPosition = null;
                openTrade = null;
                this.BuyEnabled = true;
                this.SellEnabled = false;
                this.Positions.Clear();
            }
        }

        static int replayCount = 1;
        int referenceSerieIndex;

        StockSerie replaySerie;
        StockSerie referenceSerie;

        private void Start()
        {
            this.Positions.Clear();
            this.TradeSummary = new StockTradeSummary();

            var name = "Replay_" + replayCount++;
            this.replaySerie = new StockSerie(name, name, this.selectedGroup, StockDataProvider.Replay, BarDuration.Daily);

            // Select random serie
            var series = StockDictionary.Instance.Values.Where(v => !v.StockAnalysis.Excluded && v.BelongsToGroup(this.selectedGroup)).ToList();
            var rnd = new Random();

            referenceSerie = series[rnd.Next(0, series.Count)];
            referenceSerie.Initialise();
            referenceSerie.BarDuration = this.barDuration;
            referenceSerieIndex = 100;
            if (referenceSerie.Count < referenceSerieIndex)
            {
                MessageBox.Show("Serie too small for simulation: " + referenceSerie.StockName);
                Start();
            }

            CopyReferenceValues(referenceSerieIndex + 1);

            StockDictionary.Instance.Add(name, replaySerie);

            this.SelectedStockChanged(name, true);

            this.Stop = 0;
            openPosition = null;
            openTrade = null;
            this.BuyEnabled = true;
            this.SellEnabled = false;
        }

        void CopyReferenceValues(int nbValues)
        {
            this.replaySerie.IsInitialised = false;
            referenceSerie.BarDuration = this.barDuration;
            DateTime currentDate = DateTime.Today;
            foreach (var dailyValue in referenceSerie.Values.Take(nbValues))
            {
                this.replaySerie.Add(currentDate, new StockDailyValue(currentDate, dailyValue));
                currentDate = currentDate.AddDays(1);
            }
            replaySerie.Initialise();
            this.Value = replaySerie.ValueArray.Last().CLOSE;
        }
    }
}
