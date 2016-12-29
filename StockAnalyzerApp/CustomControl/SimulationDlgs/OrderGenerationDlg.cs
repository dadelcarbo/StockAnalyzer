using System;
using System.Drawing;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl
{
   public partial class OrderGenerationDlg : Form
   {
      private StockDictionary stockDictionary = null;
      private StockPortofolioList stockPortofolioList = null;

      public delegate void SimulationCompletedEventHandler();

      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
      public event StockAnalyzerForm.SelectedPortofolioChangedEventHandler SelectedPortofolioChanged;
      public event SimulationCompletedEventHandler SimulationCompleted;

      public string SelectedStockName
      {
         get
         {
            if (this.stockComboBox.SelectedItem == null)
            {
               this.stockComboBox.SelectedIndex = 0;
            }
            return this.stockComboBox.SelectedItem.ToString();
         }
         set
         {
            if (value != SelectedPortofolio.Name)
            {
               this.stockComboBox.SelectedIndex = this.stockComboBox.Items.IndexOf(value);
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
      }

      public OrderGenerationDlg(StockDictionary stockDictionary, StockPortofolioList stockPortofolioList)
      {
         InitializeComponent();
         this.stockDictionary = stockDictionary;
         this.stockPortofolioList = stockPortofolioList;
         // Initialize portofolio combo
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
         this.stockComboBox.Enabled = true;
         this.stockComboBox.Items.Clear();
         foreach (StockSerie stockSerie in stockDictionary.Values)
         {
            if (stockSerie.StockAnalysis.FollowUp)
            {
               this.stockComboBox.Items.Add(stockSerie.StockName);
            }
         }
         this.stockComboBox.SelectedItem = this.stockComboBox.Items[0];
      }

      private void OrderGenerationDlg_Load(object sender, EventArgs e)
      {
      }

      private void generateOrderBtn_Click(object sender, EventArgs e)
      {
         // Manage selected Stock and portofolio
         // Get selected Stock
         StockSerie stockSerie = this.stockDictionary[this.SelectedStockName];
         stockSerie.Initialise();

         // Create dedicated portofolio
         StockPortofolio portofolio = this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
         StockPortofolio tmpPortofolio = this.stockPortofolioList.Find(p => p.Name == stockSerie.StockName + "_P");
         if (tmpPortofolio == null)
         {
            tmpPortofolio = new StockPortofolio(stockSerie.StockName + "_P");
            this.portofolioComboBox.Items.Add(tmpPortofolio.Name);
            this.stockPortofolioList.Add(tmpPortofolio);
         }
         this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(tmpPortofolio.Name);
         tmpPortofolio.OrderList = portofolio.OrderList.GetSummaryOrderList(stockSerie.StockName, true);

         StockOrder lastOrder = stockSerie.GenerateOrder(this.SelectedStrategy, simulationParameterControl.StartDate, simulationParameterControl.EndDate.AddHours(18),
            simulationParameterControl.amount, simulationParameterControl.reinvest,
            simulationParameterControl.amendOrders, simulationParameterControl.supportShortSelling,
             this.simulationParameterControl.takeProfit, this.simulationParameterControl.profitRate,
             this.simulationParameterControl.stopLoss, this.simulationParameterControl.stopLossRate,
            simulationParameterControl.fixedFee, simulationParameterControl.taxRate,
            tmpPortofolio);

         //// Do a bit of cleanup @@@@
         //if (lastOrder != null && this.simulationParameterControl.removePendingOrders)
         //{
         //    if (lastOrder.IsBuyOrder())
         //    {
         //        lastOrder = null;
         //    }
         //    else
         //    {
         //        if (lastOrder.UpDownState != StockOrder.OrderStatus.Executed)
         //        {
         //            portofolio.OrderList.Remove(portofolio.OrderList.Last());
         //            lastOrder = null;
         //        }
         //    }
         //}

         if (this.simulationParameterControl.displayPendingOrders && lastOrder != null && lastOrder.State == StockOrder.OrderStatus.Pending)
         {
            OrderEditionDlg orderEditionDlg = new OrderEditionDlg(lastOrder);
            orderEditionDlg.StartPosition = FormStartPosition.Manual;
            orderEditionDlg.Location = new Point(0, 0);
            orderEditionDlg.ShowDialog();
         }
         // Send event
         if (SimulationCompleted != null)
         {
            SimulationCompleted();
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
            SelectedPortofolioChanged(this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString()), false);
         }
      }
   }
}
