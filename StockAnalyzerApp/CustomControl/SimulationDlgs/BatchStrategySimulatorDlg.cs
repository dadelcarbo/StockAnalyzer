using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockLogging;

namespace StockAnalyzerApp.CustomControl
{
   public partial class BatchStrategySimulatorDlg : Form
   {
      private StockDictionary stockDictionary = null;
      private StockPortofolioList stockPortofolioList = null;
      private ToolStripProgressBar progressBar = null;
      private StockSerie.Groups group;
      public StockSerie.StockBarDuration BarDuration { get; set; }

      public DateTime StartDate { get { return this.simulationParameterControl.StartDate; } set { this.simulationParameterControl.StartDate = value; } }
      public DateTime EndDate { get { return this.simulationParameterControl.EndDate; } set { this.simulationParameterControl.EndDate = value; } }

      public IStockStrategy SelectedStrategy { get; set; }

      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

      public event StockAnalyzerForm.SimulationCompletedEventHandler SimulationCompleted;

      public BatchStrategySimulatorDlg(StockDictionary stockDictionary, StockPortofolioList stockPortofolioList, StockSerie.Groups group, StockSerie.StockBarDuration barDuration, ToolStripProgressBar progressBar)
      {
         InitializeComponent();

         this.stockPortofolioList = stockPortofolioList;
         this.stockDictionary = stockDictionary;

         this.progressBar = progressBar;

         this.group = group;
         this.BarDuration = barDuration;


      }

      public void OnBarDurationChanged(StockSerie.StockBarDuration barDuration)
      {
         this.BarDuration = barDuration;
      }

      private void simulateTradingBtn_Click(object sender, EventArgs e)
      {
         this.simulationParameterControl.ValidateInputParameters();

         stockPortofolioList.RemoveAll(p => p.IsSimulation);

         if (this.generateReportCheckBox.Checked)
         {
            this.simulationParameterControl.GenerateReportHeader("BatchReport_" + SelectedStrategy +  "_" + this.BarDuration + ".csv", false);
         }

         // Count the stock to simulate to initialise the progress bar
         int stockNumber = 0;
         List<StockSerie> tmpList = new List<StockSerie>();
         foreach (StockSerie stockSerie in this.stockDictionary.Values.Where(s => s.BelongsToGroup(this.group)))
         {
            if (!stockSerie.StockAnalysis.Excluded && !stockSerie.IsPortofolioSerie && stockSerie.Initialise() && stockSerie.Count > 50)
            {
               stockNumber++;
               tmpList.Add(stockSerie);
            }
         }

         this.progressBar.Maximum = stockNumber;
         this.progressBar.Minimum = 0;
         this.progressBar.Value = 0;
         foreach (StockSerie stockSerie in tmpList)
         {
            StockLog.Write("Processing: " + stockSerie.StockName);
            stockSerie.BarDuration = this.BarDuration;
            GenerateSimulation(stockSerie);
            // 
            this.progressBar.Value++;
         }
         this.progressBar.Value = 0;

         if (this.SimulationCompleted != null)
         {
            this.SimulationCompleted(this.simulationParameterControl);
         }

         float totalValue = 0f;
         foreach (StockSerie stockSerie in tmpList.Where(s => s.Values.Count > 0))
         {
            StockSerie serie = this.stockDictionary[stockSerie.StockName + "_P"];
            totalValue += serie.Values.Last().CLOSE;
         }
         totalValue = totalValue / tmpList.Count;
         float totalPercentGain = (totalValue - this.simulationParameterControl.amount) / this.simulationParameterControl.amount;
         StockLog.Write(totalPercentGain.ToString("P"));
      }

      private void GenerateSimulation(StockSerie stockSerie)
      {
         stockSerie.Initialise();

         // Manage selected Stock and portofolio
         StockPortofolio portofolio = new StockPortofolio(stockSerie.StockName + "_P");
         portofolio.TotalDeposit = this.simulationParameterControl.amount;
         stockPortofolioList.Add(portofolio);

         this.SelectedStrategy = StrategyManager.CreateStrategy(this.simulationParameterControl.SelectedStrategyName, stockSerie, null, simulationParameterControl.supportShortSelling);

         // intialise the serie
         stockSerie.Initialise();

         StockOrder lastOrder = stockSerie.GenerateSimulation(SelectedStrategy, this.simulationParameterControl.StartDate, this.simulationParameterControl.EndDate.AddHours(18), this.simulationParameterControl.amount, this.simulationParameterControl.reinvest,
             this.simulationParameterControl.amendOrders, this.simulationParameterControl.supportShortSelling,
             this.simulationParameterControl.takeProfit, this.simulationParameterControl.profitRate,
             this.simulationParameterControl.stopLoss, this.simulationParameterControl.stopLossRate,
             this.simulationParameterControl.fixedFee, this.simulationParameterControl.taxRate, portofolio);

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

         // Display pending order
         if (this.simulationParameterControl.displayPendingOrders && lastOrder != null && lastOrder.State == StockOrder.OrderStatus.Pending)
         {
            if (SelectedStockChanged != null)
            {
               SelectedStockChanged(lastOrder.StockName, true);
            }
            OrderEditionDlg orderEditionDlg = new OrderEditionDlg(lastOrder);
            orderEditionDlg.StartPosition = FormStartPosition.Manual;
            orderEditionDlg.Location = new Point(0, 0);
            orderEditionDlg.ShowDialog();
         }

         // Create Portofoglio serie
         portofolio.Initialize(stockDictionary);
         if (stockDictionary.Keys.Contains(portofolio.Name))
         {
            stockDictionary.Remove(portofolio.Name);
         }
         stockDictionary.Add(portofolio.Name, portofolio.GeneratePortfolioStockSerie(portofolio.Name, stockSerie, stockSerie.StockGroup));

         // Generate report
         if (this.generateReportCheckBox.Checked)
         {
            this.simulationParameterControl.GenerateReportLine("BatchReport_" + SelectedStrategy + ".csv", stockSerie, portofolio);
         }
      }
   }
}
