using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayViewModel : NotifyPropertyChangedBase
    {
        private StockSerie.Groups selectedGroup;
        private StockBarDuration barDuration;

        public ObservableCollection<MarketReplayPositionViewModel> Positions { get; set; }
        ObservableCollection<StockPosition> positionHistory;
        public ObservableCollection<StockPosition> PositionHistory
        {
            get => positionHistory;
            set
            {
                if (positionHistory != value)
                {
                    positionHistory = value;
                    this.OnPropertyChanged("PositionHistory");
                }
            }
        }

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
                    this.OnPropertyChanged("Stop");
                }
            }
        }
        private float target1;
        public float Target1
        {
            get => target1;
            set
            {
                if (target1 != value)
                {
                    target1 = value;
                    this.OnPropertyChanged("Target1");
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

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public MarketReplayViewModel(StockSerie.Groups selectedGroup, StockBarDuration barDuration)
        {
            this.selectedGroup = selectedGroup;
            this.barDuration = barDuration;
            this.Positions = new ObservableCollection<MarketReplayPositionViewModel>();
            this.PositionHistory = new ObservableCollection<StockPosition>();

            this.stopEnabled = true;
            this.forwardEnabled = true;
            this.fastForwardEnabled = true;
            this.buyEnabled = true;
            this.sellEnabled = false;

            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            StockAnalyzerForm.MainFrame.BinckPortfolio = StockPortfolio.ReplayPortfolio;

            Start();
        }


        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                return stopCommand ?? (stopCommand = new CommandBase(StopReplay));


            }
        }
        private ICommand forwardCommand;
        public ICommand ForwardCommand
        {
            get
            {
                return forwardCommand ?? (forwardCommand = new CommandBase(Forward));
            }
        }
        private ICommand fastForwardCommand;
        public ICommand FastForwardCommand
        {
            get
            {
                return fastForwardCommand ?? (fastForwardCommand = new CommandBase(FastForward));
            }
        }
        private ICommand buyCommand;
        public ICommand BuyCommand
        {
            get
            {
                return buyCommand ?? (buyCommand = new CommandBase<MarketReplayViewModel>(Buy, this, vm=>vm.BuyEnabled, "BuyEnabled"));
            }
        }
        private ICommand sellCommand;
        public ICommand SellCommand
        {
            get
            {
                return sellCommand ?? (sellCommand = new CommandBase<MarketReplayViewModel>(Sell, this, vm => vm.SellEnabled, "SellEnabled"));
            }
        }

        private void StopReplay()
        {
            // Generate statistics

            // Clear and restart
            this.Positions.Clear();
            this.PositionHistory.Clear();

            Start();
        }
        private void Forward()
        {
            if (++referenceSerieIndex < referenceSerie.Count())
            {
                CopyReferenceValues(referenceSerieIndex + 1);

                this.SelectedStockChanged(replaySerie.StockName, true);
            }
            else
            {
                StopReplay();
            }
        }
        private void FastForward()
        {
            referenceSerieIndex += 5;
            if (referenceSerieIndex < referenceSerie.Count())
            {
                CopyReferenceValues(referenceSerieIndex + 1);

                this.SelectedStockChanged(replaySerie.StockName, true);
            }
            else
            {
                StopReplay();
                this.BuyEnabled = false;
                this.SellEnabled = false;
            }

        }
        private void Buy()
        {
            this.BuyEnabled = false;
            this.SellEnabled = true;

            var date = replaySerie.Keys.Last();
            int qty = 2;
            StockPortfolio.ReplayPortfolio.AddOperation(StockOperation.FromSimu(date, replaySerie.StockName, StockOperation.BUY, qty, -this.Value * qty));

            this.Positions.Add(openPosition = new MarketReplayPositionViewModel() { Entry = this.Value, Stop = this.Stop, Target1 = this.Target1 });
            this.Forward();
        }
        private void Sell()
        {
            var date = replaySerie.Keys.Last();
            int qty = 2;

            StockPortfolio.ReplayPortfolio.AddOperation(StockOperation.FromSimu(date, replaySerie.StockName, StockOperation.SELL, qty, this.Value * qty));
            PositionHistory = new ObservableCollection<StockPosition>(StockPortfolio.ReplayPortfolio.Positions.Where(p => p.IsClosed));
            openPosition = null;
            this.BuyEnabled = true;
            this.SellEnabled = false;
            this.Positions.Clear();
            this.Forward();
        }

        static int replayCount = 1;
        int referenceSerieIndex;

        StockSerie replaySerie;
        StockSerie referenceSerie;
        private MarketReplayPositionViewModel openPosition = null;

        private void Start()
        {
            var name = "Replay_" + replayCount++;
            this.replaySerie = new StockSerie(name, name, StockSerie.Groups.Replay, StockDataProvider.Replay);

            // Select random serie
            var series = StockDictionary.StockDictionarySingleton.Values.Where(v => !v.StockAnalysis.Excluded && v.BelongsToGroup(this.selectedGroup)).ToList();
            var rnd = new Random();

            referenceSerie = series[rnd.Next(0, series.Count)];
            referenceSerie.Initialise();
            referenceSerie.BarDuration = this.barDuration;
            referenceSerieIndex = 199;

            CopyReferenceValues(referenceSerieIndex + 1);

            StockDictionary.StockDictionarySingleton.Add(name, replaySerie);

            this.SelectedStockChanged(name, true);
        }

        void CopyReferenceValues(int nbValues)
        {
            this.replaySerie.IsInitialised = false;
            referenceSerie.BarDuration = this.barDuration;
            DateTime currentDate = DateTime.Today;
            foreach (var dailyValue in referenceSerie.Take(nbValues))
            {
                this.replaySerie.Add(currentDate, dailyValue.Value);
                currentDate = currentDate.AddDays(1);
            }
            this.Value = replaySerie.ValueArray.Last().CLOSE;
        }
    }
}
