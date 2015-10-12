using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl
{
   public partial class StockStrategyScannerDlg : Form
   {
      private StockDictionary stockDictionary;
      private StockSerie.StockBarDuration barDuration;

      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
      public event StockAnalyzerForm.SelectedStockGroupChangedEventHandler SelectStockGroupChanged;
      public event StockAnalyzerForm.SelectedStrategyChangedEventHandler SelectedStrategyChanged;

      public IEnumerable<string> SelectedStocks
      {
         get
         {
            return this.selectedStockListBox.Items.Cast<string>();
         }
      }

      public StockStrategyScannerDlg(StockDictionary stockDictionary, StockSerie.Groups stockGroup, StockSerie.StockBarDuration barDuration, string strategyName)
      {
         InitializeComponent();

         this.stockDictionary = stockDictionary;
         this.barDuration = barDuration;

         // Initialise group combo box
         groupComboBox.Items.AddRange(this.stockDictionary.GetValidGroupNames().ToArray());
         groupComboBox.SelectedItem = stockGroup.ToString();
         groupComboBox.SelectedValueChanged += new EventHandler(groupComboBox_SelectedValueChanged);

         // Initialise Strategy Combo box
         this.strategyComboBox.Items.Clear();
         List<string> strategyList = StrategyManager.GetStrategyList();
         foreach (string name in strategyList)
         {
            this.strategyComboBox.Items.Add(name);
         }
         this.strategyComboBox.SelectedItem = strategyName;
         this.strategyComboBox.SelectedValueChanged += new EventHandler(strategyComboBox_SelectedValueChanged);

         periodComboBox.SelectedIndex = 0;

         OnBarDurationChanged(barDuration);
      }

      void strategyComboBox_SelectedValueChanged(object sender, EventArgs e)
      {
         if (SelectedStrategyChanged != null)
         {
            SelectedStrategyChanged(this.strategyComboBox.SelectedItem.ToString());
         }
      }
      public void OnBarDurationChanged(StockSerie.StockBarDuration barDuration)
      {
         this.barDuration = barDuration;
         selectedStockListBox.Items.Clear();
      }

      public void OnStrategyChanged(string strategyName)
      {
         this.strategyComboBox.SelectedItem = strategyName;
         selectedStockListBox.Items.Clear();
      }

      void groupComboBox_SelectedValueChanged(object sender, EventArgs e)
      {
         if (SelectStockGroupChanged != null)
         {
            SelectStockGroupChanged(groupComboBox.SelectedItem.ToString());
         }
      }
      private void selectButton_Click(object sender, EventArgs e)
      {
         Cursor cursor = Cursor;
         Cursor = Cursors.WaitCursor;

         var stockInGroupList = stockDictionary.Values.Where(s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString()) && !s.IsPortofolioSerie);
         try
         {
            selectedStockListBox.Items.Clear();
            selectedStockListBox.Refresh();

            if (progressBar != null)
            {
               progressBar.Minimum = 0;
               progressBar.Maximum = stockInGroupList.Count();
               progressBar.Value = 0;
            }

            foreach (StockSerie stockSerie in stockInGroupList)
            {
               bool selected = false;

               progressLabel.Text = stockSerie.StockName;
               progressLabel.Refresh();

               if (this.refreshDataCheckBox.Checked)
               {
                  stockSerie.IsInitialised = false;
                  StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, stockSerie);
               }

               if (!stockSerie.Initialise()) { continue; }

               stockSerie.BarDuration = barDuration;

               int lastIndex = stockSerie.LastCompleteIndex;
               int firstIndex = lastIndex + 1 - (int)periodComboBox.SelectedItem;

               // Perform Strategy calculation
               // Create new simulation portofolio

               StockPortofolio currentPortofolio = new StockPortofolio(stockSerie.StockName + "_P");
               currentPortofolio.IsSimulation = true;
               currentPortofolio.TotalDeposit = 10000;

               if (!string.IsNullOrWhiteSpace(this.strategyComboBox.SelectedItem.ToString()))
               {
                  var selectedStrategy = StrategyManager.CreateStrategy(this.strategyComboBox.SelectedItem.ToString(), stockSerie,
                      null, true);

                  if (selectedStrategy != null)
                  {
                     float amount = stockSerie.GetMax(StockDataType.CLOSE) * 100f;

                     currentPortofolio.TotalDeposit = amount;
                     currentPortofolio.Clear();

                     stockSerie.GenerateSimulation(selectedStrategy, Settings.Default.StrategyStartDate,
                        stockSerie.Keys.Last(), amount, false, false, Settings.Default.SupportShortSelling,
                        false, 0.0f, false, 0.0f, 0.0f, 0.0f, currentPortofolio);

                     // Check if orders happened during the time frame
                     for (int i = firstIndex; i < lastIndex; i++)
                     {
                        if (currentPortofolio.OrderList.Any(o => o.ExecutionDate == stockSerie.Keys.ElementAt(i)))
                        {
                           selected = true;
                           break;
                        }
                     }
                  }
               }

               if (selected)
               {
                  selectedStockListBox.Items.Add(stockSerie.StockName);
                  selectedStockListBox.Refresh();
               }
            }
            if (progressBar != null)
            {
               progressBar.Value++;
            }
         }
         catch (Exception exception)
         {
            MessageBox.Show(exception.Message, "Script Error !!!");
         }
         finally
         {
            if (progressBar != null)
            {
               progressBar.Value = 0;
               progressLabel.Text = selectedStockListBox.Items.Count + "/" + stockInGroupList.Count();
            }
         }
         Cursor = cursor;
      }

      void clearButton_Click(object sender, EventArgs e)
      {
         selectedStockListBox.Items.Clear();
      }

      void selectedStockListBox_SelectedValueChanged(object sender, EventArgs e)
      {
         if (SelectedStockChanged != null && selectedStockListBox.SelectedItem != null)
         {
            SelectedStockChanged(selectedStockListBox.SelectedItem.ToString(), true);
         }
         Focus();
      }

      private void eventTreeView_AfterCheck(object sender, TreeViewEventArgs e)
      {
         if (e.Node.Nodes.Count > 0)
         {
            foreach (TreeNode node in e.Node.Nodes)
            {
               node.Checked = e.Node.Checked;
            }
         }
      }

      private void reloadButton_Click(object sender, EventArgs e)
      {
         try
         {
            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            var stockInGroupList = stockDictionary.Values.Where(s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString()) && !s.IsPortofolioSerie);
            foreach (StockSerie stockSerie in stockInGroupList)
            {
               stockSerie.IsInitialised = false;
               StockSplashScreen.ProgressText = "Downloading " + stockSerie.StockGroup + " - " + stockSerie.StockName;
               StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, stockSerie);
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message, "Unable to download selected stock data...", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
            StockSplashScreen.CloseForm(true);
         }
      }
   }
}
