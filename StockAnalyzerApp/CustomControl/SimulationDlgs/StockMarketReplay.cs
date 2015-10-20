using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
   public partial class StockMarketReplay : Form, INotifyPropertyChanged
   {
      private enum Action
      {
         None,
         Buy,
         Sell
      }


      public StockMarketReplay()
      {
         InitializeComponent();
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
      private StockSerie refSerie = null;
      private int index = 0;

      private bool started = false;

      private void nextButton_Click(object sender, EventArgs e)
      {
         DateTime currentDate = DateTime.Today;

         index++;

         if (index < refSerie.Count)
         {
            replaySerie.IsInitialised = false;
            for (int i = 0; i < index; i++)
            {
               replaySerie.Add(currentDate, refSerie.ValueArray[i]);
               currentDate = currentDate.AddDays(1);
            }

            StockAnalyzerForm.MainFrame.CurrentStockSerie = replaySerie;

            this.CurrentValue = replaySerie.Values.Last().CLOSE;

         }
         else
         {
            MessageBox.Show("Replay finished !!!");
         }
      }

      private void startButton_Click(object sender, EventArgs e)
      {
         if (started)
         {
            replaySerie = null;
            started = false;
            startButton.Text = "Start";
            startButton.Focus();
            nextButton.Enabled = false;

            this.Position = 0;
            this.OpenValue = 0;
            this.CurrentValue = 0;
            this.totalValue = 0;

            this.buyButton.Enabled = false;
            this.sellButton.Enabled = false;
            this.shortButton.Enabled = false;
            this.coverButton.Enabled = false;
         }
         else
         {
            replaySerie = new StockSerie("Replay", "Replay", StockSerie.Groups.ALL, StockDataProvider.Replay);

            // Random pick

            Random rand = new Random(DateTime.Now.Millisecond);
            var series = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.EURO_A)).Select(s => s.StockName);

            string stockName = series.ElementAt(rand.Next(0, series.Count()));

            refSerie = StockDictionary.StockDictionarySingleton[stockName];
            refSerie.Initialise();

            DateTime currentDate = DateTime.Today;
            int nbInitBars = rand.Next(200, refSerie.Count - 200);

            for (index = 0; index < nbInitBars; index++)
            {
               replaySerie.Add(currentDate, refSerie.ValueArray[index]);
               currentDate = currentDate.AddDays(1);
            }

            replaySerie.IsInitialised = false;

            StockAnalyzerForm.MainFrame.CurrentStockSerie = replaySerie;

            startButton.Text = "Stop";
            nextButton.Enabled = true;
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

            this.OpenValue = replaySerie.Values.Last().CLOSE;
            this.Position = 1;

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

            this.OpenValue = replaySerie.Values.Last().CLOSE;
            this.Position = -1;

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

            this.totalValue += this.AddedValue;
            OnPropertyChanged("TotalValue");
            this.Position = 0;
            this.OpenValue = 0;

            nextButton.Focus();
         }
      }
   }
}
