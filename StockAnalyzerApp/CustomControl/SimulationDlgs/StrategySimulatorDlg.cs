using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl
{
   public partial class StrategySimulatorDlg : Form
   {
      private StockDictionary stockDictionary = null;
      private StockPortofolioList stockPortofolioList = null;

      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
      public event StockAnalyzerForm.SelectedPortofolioChangedEventHandler SelectedPortofolioChanged;

      public string SelectedStockName
      {
         get { return this.stockComboBox.SelectedItem.ToString(); }
         set
         {
            if (value != SelectedPortofolio.Name)
            {
               this.stockComboBox.SelectedIndex = Math.Max(this.stockComboBox.Items.IndexOf(value), 0);
               if (this.portofolioComboBox.Items.Contains(value + "_P"))
               {
                  this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(value + "_P");
               }
            }
         }
      }

      public IStockStrategy SelectedStrategy { get; set; }
      public StockPortofolio SelectedPortofolio
      {
         get { return this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString()); }
         set { this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(value.Name); }
      }

      public delegate void SimulationCompletedEventHandler();
      public event SimulationCompletedEventHandler SimulationCompleted;

      public StrategySimulatorDlg(StockDictionary stockDictionary, StockPortofolioList stockPortofolioList, string stockName)
      {
         InitializeComponent();

         // Initialize portofolio combo
         this.stockDictionary = stockDictionary;
         this.portofolioComboBox.Enabled = true;
         this.portofolioComboBox.Items.Clear();
         if (stockPortofolioList.Count == 0)
         {
            StockPortofolio portofolio = new StockPortofolio("SIMULATION");
            stockPortofolioList.Add(portofolio);
         }
         foreach (string name in stockPortofolioList.GetPortofolioNames())
         {
            this.portofolioComboBox.Items.Add(name);
         }
         this.portofolioComboBox.SelectedItem = this.portofolioComboBox.Items[0];

         // Initialize stock combo
         this.stockPortofolioList = stockPortofolioList;
         this.stockComboBox.Enabled = true;
         this.stockComboBox.Items.Clear();
         foreach (StockSerie stockSerie in stockDictionary.Values)
         {
            this.stockComboBox.Items.Add(stockSerie.StockName);
         }
         this.stockComboBox.SelectedItem = this.stockComboBox.Items[0];
      }

      void simulationParameterControl_SelectedStrategyChanged(string strategyName)
      {
         this.SelectedStrategy = StrategyManager.CreateStrategy(strategyName, this.stockDictionary[this.SelectedStockName], null, simulationParameterControl.supportShortSelling);
         this.parametrizableControl.ViewableItem = this.SelectedStrategy.EntryTriggerIndicator as IStockViewableSeries;
      }

      private void simulateTradingBtn_Click(object sender, EventArgs e)
      {
         this.simulationParameterControl.ValidateInputParameters();

         //
         GenerateSimulation();

         // Send event
         if (SimulationCompleted != null)
         {
            SimulationCompleted();
         }
      }
      private void GenerateSimulation()
      {
         // Manage selected Stock and portofolio
         // Get selected portofolio
         StockPortofolio portofolio = this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
         portofolio.OrderList.Clear();

         // Get selected Stock
         StockSerie stockSerie = this.stockDictionary[this.SelectedStockName];
         if (!stockSerie.Initialise()) { return; }

         if (this.SelectedStrategy == null)
         {
            this.simulationParameterControl_SelectedStrategyChanged(this.simulationParameterControl.SelectedStrategyName);
         }

         StockOrder lastOrder = stockSerie.GenerateSimulation(this.SelectedStrategy,
            simulationParameterControl.StartDate, simulationParameterControl.EndDate.AddHours(18),
            simulationParameterControl.amount, simulationParameterControl.reinvest,
            simulationParameterControl.amendOrders, simulationParameterControl.supportShortSelling,
            simulationParameterControl.takeProfit, simulationParameterControl.profitRate,
            simulationParameterControl.stopLoss, simulationParameterControl.stopLossRate,
            simulationParameterControl.fixedFee, simulationParameterControl.taxRate, portofolio);

         // Do a bit of cleanup
         if (lastOrder != null && this.simulationParameterControl.removePendingOrders)
         {
            if (lastOrder.IsBuyOrder())
            {
               lastOrder = null;
            }
            else
            {
               if (lastOrder.State != StockOrder.OrderStatus.Executed)
               {
                  portofolio.OrderList.Remove(portofolio.OrderList.Last());
                  lastOrder = null;
               }
            }
         }

         if (this.simulationParameterControl.displayPendingOrders && lastOrder != null && lastOrder.State == StockOrder.OrderStatus.Pending)
         {
            OrderEditionDlg orderEditionDlg = new OrderEditionDlg(lastOrder);
            orderEditionDlg.StartPosition = FormStartPosition.Manual;
            orderEditionDlg.Location = new Point(0, 0);
            orderEditionDlg.ShowDialog();
         }
      }
      private void stockComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (SelectedStockChanged != null)
         {
            SelectedStockChanged(SelectedStockName, false);
         }
      }
      void portofolioComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         if (SelectedPortofolioChanged != null)
         {
            StockPortofolio portofolio = this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
            if (portofolio != null)
            {
               SelectedPortofolioChanged(portofolio, false);
            }
         }
      }
   }
}
