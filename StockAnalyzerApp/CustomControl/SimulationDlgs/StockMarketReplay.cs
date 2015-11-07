using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.Portofolio;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
   public partial class StockMarketReplay : Form, INotifyPropertyChanged
   {
      public StockMarketReplay()
      {
         InitializeComponent();

         portfolio = new StockPortofolio("Replay_P");
         portfolio.TotalDeposit = 1000;

         this.TopMost = true;
      }

      private int position;

      public int Position
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

      private float openValue;

      public float OpenValue
      {
         get { return openValue; }
         set
         {
            if (value != openValue)
            {
               this.openValue = value;
               OnPropertyChanged("OpenValue");
               OnPropertyChanged("AddedValue");
               OnPropertyChanged("AddedValuePercent");
            }
         }
      }

      private float currentValue;

      public float CurrentValue
      {
         get { return currentValue; }
         set
         {
            if (value != currentValue)
            {
               this.currentValue = value;
               OnPropertyChanged("CurrentValue");
               OnPropertyChanged("AddedValue");
               OnPropertyChanged("AddedValuePercent");
            }
         }
      }

      public float AddedValue
      {
         get { return (CurrentValue - OpenValue)*Position; }
      }
      public float AddedValuePercent
      {
         get { return openValue == 0f ? 0.0f : (CurrentValue - OpenValue)*Position / OpenValue; }
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

            this.Position = 0;
            this.OpenValue = 0;
            this.CurrentValue = 0;
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
                  StockDictionary.StockDictionarySingleton.Values.Where(s => !s.IsPortofolioSerie && s.BelongsToGroup(StockSerie.Groups.EURO_A))
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
               foreach(StockDailyValue dailyValue in serie.Values)
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

               startDate = serie.Keys.ElementAt(index);
               
               startButton.Text = "Stop";
               nextButton.Enabled = true;
               moveButton.Enabled = true;
               nextButton.Focus();

               this.buyButton.Enabled = true;
               this.sellButton.Enabled = false;
               this.shortButton.Enabled = true;
               this.coverButton.Enabled = false;

               this.Position = 0;
               this.OpenValue = 0;
               this.CurrentValue = replaySerie.Values.Last().CLOSE;
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
         NextStep(1);
      }
      private void moveButton_Click(object sender, EventArgs e)
      {
         NextStep(5);
      }
      private void NextStep(int step)
      {
         index += step;
         DateTime currentDate = DateTime.Today;
         refSerie.BarDuration = StockSerie.StockBarDuration.Daily;
         if (index < refSerie.Count)
         {
            replaySerie.IsInitialised = false;
            replaySerie.ClearBarDurationCache();
            replaySerie.BarDuration = StockSerie.StockBarDuration.Daily;
            for (int i = 0; i < index; i++)
            {
               StockDailyValue dailyValue = refSerie.ValueArray[i];
               replaySerie.Add(currentDate, dailyValue);
               currentDate = currentDate.AddDays(1);
            }
            replaySerie.Initialise();

            this.CurrentValue = replaySerie.GetValues(StockSerie.StockBarDuration.Daily).Last().CLOSE;

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
            this.OpenValue = dailyValue.CLOSE;
            this.Position = 1;

            this.portfolio.OrderList.Add(StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.BuyAtMarketClose, false, dailyValue.DATE, dailyValue.DATE, 1, dailyValue.CLOSE, 0));

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
            this.OpenValue = dailyValue.CLOSE;
            this.Position = -1;

            this.portfolio.OrderList.Add(StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.BuyAtMarketClose, true, dailyValue.DATE, dailyValue.DATE, 1, dailyValue.CLOSE, 0));

            nextButton.Focus();
         }
      }

      private void sellButton_Click(object sender, EventArgs e)
      {
         if (started)
         {
            this.buyButton.Enabled = true;
            this.sellButton.Enabled = false;
            this.shortButton.Enabled = true;
            this.coverButton.Enabled = false;

            // Statistics
            nbTrade++;
            if (AddedValue > 0)
            {
               nbWinTrade++;
            }
            else
            {
               nbLostTrade++;
            }
            tradeGains.Add(AddedValuePercent);

            this.totalValue += 1000f * this.AddedValuePercent;
            OnPropertyChanged("TotalValue");

            StockDailyValue dailyValue = replaySerie.GetValues(StockSerie.StockBarDuration.Daily).Last();
            this.portfolio.OrderList.Add(StockOrder.CreateExecutedOrder(replaySerie.StockName, StockOrder.OrderType.SellAtMarketClose, this.position < 0, dailyValue.DATE, dailyValue.DATE, 1, dailyValue.CLOSE, 0));


            this.Position = 0;
            this.OpenValue = 0;

            nextButton.Focus();
         }
      }
   }
}
