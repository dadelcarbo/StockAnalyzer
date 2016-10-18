using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.Portofolio;
using System.Collections.ObjectModel;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public partial class StockMarketReplay : Form, INotifyPropertyChanged
    {
        public ObservableCollection<PositionViewModel> Positions { get; set; }

        private StockDailyValue dailyValue;

        public StockMarketReplay()
        {
            InitializeComponent();

            portfolio = new StockPortofolio("Replay_P");
            portfolio.TotalDeposit = 1000;

            this.TopMost = true;

            this.Position = new PositionViewModel();
            this.position.OnPositionClosed += OnPositionClosed;
            this.position.OnStopTouched += Position_OnStopTouched;
            this.position.OnTargetTouched += Position_OnTargetTouched;
            this.Positions = new ObservableCollection<PositionViewModel>();
            this.Positions.Add(this.Position);
            this.stockPositionUserControl1.DataContext = this;
        }

        private void Position_OnTargetTouched(OrderViewModel order)
        {
            StockOrder stockOrder = StockOrder.CreateExecutedOrder(replaySerie.StockName,
                StockOrder.OrderType.SellAtLimit,
                order.Type == OrderType.Short,
                dailyValue.DATE, dailyValue.DATE, 1,
                order.Value, 0);
            this.portfolio.OrderList.Add(stockOrder);
        }

        private void Position_OnStopTouched(OrderViewModel order)
        {
            StockOrder stockOrder = StockOrder.CreateExecutedOrder(replaySerie.StockName,
                StockOrder.OrderType.SellAtLimit, order.Type == OrderType.Short,
                dailyValue.DATE, dailyValue.DATE,
                1, order.Value, 0);
            this.portfolio.OrderList.Add(stockOrder);
        }

        private PositionViewModel position;
        public PositionViewModel Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    this.position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        private bool halfPosition;
        public bool HalfPosition
        {
            get { return halfPosition; }
            set
            {
                if (value != halfPosition)
                {
                    halfPosition = value;
                    if (halfPosition)
                    {
                        this.Stop = this.Position.CurrentValue * 0.9f;
                        this.Target = this.Position.CurrentValue * 1.1f;
                    }
                    else
                    {
                        this.Stop = 0;
                        this.Target = 0;
                    }
                    OnPropertyChanged("HalfPosition");
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
                    this.stop = value;
                    OnPropertyChanged("Stop");
                }
            }
        }

        private float target;
        public float Target
        {
            get { return target; }
            set
            {
                if (value != target)
                {
                    this.target = value;
                    OnPropertyChanged("Target");
                }
            }
        }

        private float variation;
        public float Variation
        {
            get { return variation; }
            set
            {
                if (value != variation)
                {
                    this.variation = value;
                    OnPropertyChanged("Variation");
                }
            }
        }

        private float totalValue;
        public float TotalValue
        {
            get { return totalValue; }
        }

        private StockSerie replaySerie = null;
        private DateTime startDate;
        private StockSerie refSerie = null;
        private int index = 0;

        private bool started = false;

        private int nbTrade = 0;
        private int nbWinTrade = 0;
        private int nbLostTrade = 0;

        List<float> tradeGains = new List<float>();

        StockPortofolio portfolio { get; set; }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (started)
            {
                replaySerie = null;
                started = false;
                startButton.Text = "Start";
                startButton.Focus();
                nextButton.Enabled = false;
                moveButton.Enabled = false;

                this.Position.Number = 0;
                this.Position.OpenValue = 0;
                this.totalValue = 0;

                this.buyButton.Enabled = false;
                this.sellButton.Enabled = false;
                this.shortButton.Enabled = false;
                this.coverButton.Enabled = false;

                string msg = "Replay serie was:\t" + refSerie.StockName + Environment.NewLine +
                             "Start date:\t\t" + startDate.ToShortDateString() + Environment.NewLine +
                             "NbTrades:\t\t\t" + nbTrade + Environment.NewLine +
                             "NbWinTrades:\t\t" + nbWinTrade + Environment.NewLine +
                             "NbLostTrades:\t\t" + nbLostTrade + Environment.NewLine +
                             "AvgGain:\t\t\t" + (tradeGains.Sum() / nbTrade).ToString("P2");

                MessageBox.Show(msg);
            }
            else
            {
                Cursor cursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // Initialise stats
                    nbTrade = 0;
                    nbWinTrade = 0;
                    nbLostTrade = 0;
                    tradeGains.Clear();

                    // Create Portfolio
                    StockAnalyzerForm.MainFrame.CurrentPortofolio = portfolio;

                    portfolio.Clear();
                    replaySerie = new StockSerie("Replay", "Replay", StockSerie.Groups.ALL, StockDataProvider.Replay);

                    // Random pick

                    Random rand = new Random(DateTime.Now.Millisecond);
                    var series =
                       StockDictionary.StockDictionarySingleton.Values.Where(s => !s.IsPortofolioSerie && s.BelongsToGroup(StockSerie.Groups.COUNTRY))
                          .Select(s => s.StockName);

                    StockSerie serie = null;
                    do
                    {
                        string stockName = series.ElementAt(rand.Next(0, series.Count()));

                        serie = StockDictionary.StockDictionarySingleton[stockName];
                        serie.Initialise();
                        serie.BarDuration = StockSerie.StockBarDuration.Daily;
                    }
                    while (serie.Count < 400);

                    DateTime currentDate = DateTime.Today;
                    refSerie = new StockSerie(serie.StockName, serie.ShortName, serie.StockGroup, StockDataProvider.Replay);
                    foreach (StockDailyValue dailyValue in serie.Values)
                    {
                        StockDailyValue newValue = new StockDailyValue(serie.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, currentDate);
                        refSerie.Add(currentDate, newValue);
                        currentDate = currentDate.AddDays(1);
                    }

                    currentDate = DateTime.Today;
                    int nbInitBars = rand.Next(200, refSerie.Count - 200);

                    for (index = 0; index < nbInitBars; index++)
                    {
                        replaySerie.Add(currentDate, refSerie.ValueArray[index]);
                        currentDate = currentDate.AddDays(1);
                    }

                    dailyValue = serie.Values.Last();
                    startDate = dailyValue.DATE;

                    startButton.Text = "Stop";
                    nextButton.Enabled = true;
                    moveButton.Enabled = true;
                    nextButton.Focus();

                    OnPositionClosed();

                    this.Position.Number = 0;
                    this.Position.OpenValue = 0;
                    this.Position.CurrentValue = replaySerie.Values.Last().CLOSE;
                    this.totalValue = 0;

                    started = true;

                    refSerie.BarDuration = StockAnalyzerForm.MainFrame.BarDuration;
                    replaySerie.BarDuration = StockAnalyzerForm.MainFrame.BarDuration;
                    StockAnalyzerForm.MainFrame.CurrentStockSerie = replaySerie;

                    StockAnalyzerForm.MainFrame.Activate();
                }
                catch
                {
                }
                finally
                {
                    this.Cursor = cursor;
                }
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            NextStep();
        }
        private void moveButton_Click(object sender, EventArgs e)
        {
            for (int i = 1; i <= 5; i++)
            {
                NextStep();
            }
        }
        private void NextStep()
        {
            index++;
            DateTime currentDate = DateTime.Today;
            refSerie.BarDuration = StockSerie.StockBarDuration.Daily;
            if (index < refSerie.Count)
            {
                replaySerie.IsInitialised = false;
                replaySerie.ClearBarDurationCache();
                replaySerie.BarDuration = StockSerie.StockBarDuration.Daily;
                StockDailyValue dailyVal = null;
                for (int i = 0; i < index; i++)
                {
                    dailyVal = refSerie.ValueArray[i];
                    replaySerie.Add(currentDate, dailyVal);
                    currentDate = currentDate.AddDays(1);
                }
                replaySerie.Initialise();
                dailyValue = dailyVal;

                this.Position.CurrentValue = dailyValue.CLOSE;
                this.Variation = dailyValue.VARIATION;

                this.Position.ValidatePosition();

                replaySerie.BarDuration = StockAnalyzerForm.MainFrame.BarDuration;
                StockAnalyzerForm.MainFrame.CurrentStockSerie = replaySerie;
            }
            else
            {
                MessageBox.Show("Replay finished !!!");
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

        private void buyButton_Click(object sender, EventArgs e)
        {
            if (started)
            {
                this.buyButton.Enabled = false;
                this.sellButton.Enabled = true;
                this.shortButton.Enabled = false;
                this.coverButton.Enabled = false;

                StockDailyValue dailyValue = replaySerie.GetValues(StockSerie.StockBarDuration.Daily).Last();
                this.Position.OpenValue = dailyValue.CLOSE;
                this.Position.Number = 2;

                StockOrder stockOrder = StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.BuyAtMarketClose, false, dailyValue.DATE, dailyValue.DATE, 1, dailyValue.CLOSE, 0);
                this.portfolio.OrderList.Add(stockOrder);

                if (this.stop != 0)
                {
                    this.Position.AddStop(this.stop);
                }
                if (this.target != 0)
                {
                    this.Position.AddTarget(this.target, this.halfPosition ? 1 : 2);
                }

                nextButton.Focus();
            }
        }
        private void shortButton_Click(object sender, EventArgs e)
        {
            if (started)
            {
                this.buyButton.Enabled = false;
                this.sellButton.Enabled = false;
                this.shortButton.Enabled = false;
                this.coverButton.Enabled = true;

                StockDailyValue dailyValue = replaySerie.GetValues(StockSerie.StockBarDuration.Daily).Last();
                this.Position.OpenValue = dailyValue.CLOSE;
                this.Position.Number = -2;

                StockOrder order = StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.BuyAtMarketClose, true, dailyValue.DATE, dailyValue.DATE, 2, dailyValue.CLOSE, 0);
                this.portfolio.OrderList.Add(order);

                if (this.stop != 0)
                {
                    this.Position.AddTarget(this.stop, this.halfPosition ? 1 : 2);
                }

                if (this.target != 0)
                {
                    this.Position.AddStop(this.target);
                }

                nextButton.Focus();
            }
        }
        private void sellButton_Click(object sender, EventArgs e)
        {
            if (started)
            {
                StockDailyValue dailyValue = replaySerie.GetValues(StockSerie.StockBarDuration.Daily).Last();

                PerformSell(dailyValue, this.Position.Number, dailyValue.CLOSE);

                StockAnalyzerForm.MainFrame.CurrentStockSerie = replaySerie;
            }
        }
        private void OnPositionClosed()
        {
            this.buyButton.Enabled = true;
            this.sellButton.Enabled = false;
            this.shortButton.Enabled = true;
            this.coverButton.Enabled = false;
        }
        private void PerformSell(StockDailyValue dailyValue, int qty, float value)
        {
            this.Position.Close();

            // Statistics
            //nbTrade++;
            //if (AddedValue > 0)
            //{
            //    nbWinTrade++;
            //}
            //else
            //{
            //    nbLostTrade++;
            //}
            //tradeGains.Add(this.CalculateAddedValuePercent(value));

            //this.totalValue += 1000f * this.CalculateAddedValuePercent(value);
            //OnPropertyChanged("TotalValue");


            StockOrder stockOrder = StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.SellAtMarketClose, (bool)(this.Position.Number < 0), dailyValue.DATE, dailyValue.DATE, 1, value, 0);
            this.portfolio.OrderList.Add(stockOrder);

            nextButton.Focus();

            this.HalfPosition = false;
            this.Stop = 0;
            this.Target = 0;
        }
    }
}
