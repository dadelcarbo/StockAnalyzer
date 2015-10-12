using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockStrategyClasses;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
   public partial class FilteredSimulationParameterControl : UserControl
   {
      public delegate void SelectedStrategyHandler(string strategyName);
      public event SelectedStrategyHandler SelectedStrategyChanged;

      #region Public members
      // Input Parameters
      public System.DateTime StartDate { get; set; }
      public System.DateTime EndDate { get; set; }

      public float amount;
      public bool reinvest = true;
      public bool amendOrders = false;
      public bool supportShortSelling = false;
      public bool displayPendingOrders = false;
      public bool removePendingOrders = false;
      
      public bool takeProfit = false;
      public bool stopLoss = false;
      public float profitRate = 5;
      public float stopLossRate = 5;

      public string SelectedStrategyName
      {
         get
         {
            if (this.strategyComboBox.SelectedItem == null)
               return null;
            else
               return this.strategyComboBox.SelectedItem.ToString();
         }
         set { this.strategyComboBox.SelectedItem = value; }
      }


      public float fixedFee;
      public float taxRate;
      #endregion

      public FilteredSimulationParameterControl()
      {
         InitializeComponent();

         // Initialise date pickers
         this.startDatePicker.Value = new System.DateTime(2009, 1, 1);
         this.endDatePicker.Value = System.DateTime.Today;

         // Initialize strategy combo
         LoadStrategies(string.Empty);
      }

      public void LoadStrategies(string selected)
      {
         this.strategyComboBox.Enabled = true;
         this.strategyComboBox.Items.Clear();
         List<string> strategyList = StrategyManager.GetFilteredStrategyList(true);
         strategyList.Insert(0, string.Empty);
         foreach (string name in strategyList)
         {
            this.strategyComboBox.Items.Add(name);
         }
         this.strategyComboBox.SelectedItem = selected;
      }

      void strategyComboBox_SelectedValueChanged(object sender, System.EventArgs e)
      {
         if (this.SelectedStrategyChanged != null)
         {
            this.SelectedStrategyChanged(this.strategyComboBox.SelectedItem.ToString());
         }
      }

      public void ValidateInputParameters()
      {
         reinvest = this.reinvestCheckBox.Checked;
         amount = float.Parse(this.amountTextBox.Text, StockAnalyzerForm.EnglishCulture);

         if (string.IsNullOrWhiteSpace(this.fixedFeeTextBox.Text))
         {
            this.fixedFeeTextBox.Text = "0";
         }
         fixedFee = float.Parse(this.fixedFeeTextBox.Text, StockAnalyzerForm.EnglishCulture);

         if (string.IsNullOrWhiteSpace(this.taxRateTextBox.Text))
         {
            this.taxRateTextBox.Text = "0";
         }
         taxRate = float.Parse(this.taxRateTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;

         if (string.IsNullOrWhiteSpace(this.taxRateTextBox.Text))
         {
            this.taxRateTextBox.Text = "0";
         }
         taxRate = float.Parse(this.taxRateTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;

         takeProfit = this.takeProfitCheckBox.Checked;
         if (string.IsNullOrWhiteSpace(this.profitRateTextBox.Text))
         {
            this.profitRateTextBox.Text = "0";
         }
         profitRate = 1f + float.Parse(this.profitRateTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;

         stopLoss = this.stopLossCheckBox.Checked;
         if (string.IsNullOrWhiteSpace(this.stopLossTextBox.Text))
         {
            this.stopLossTextBox.Text = "0";
         }
         stopLossRate = 1f - float.Parse(this.stopLossTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;

         amendOrders = this.amendOrdersCheckBox.Checked;
         supportShortSelling = this.shortSellingCheckBox.Checked;
         displayPendingOrders = this.displayPendingOrdersCheckBox.Checked;
         removePendingOrders = this.removePendingOrdersCheckBox.Checked;
         StartDate = this.startDatePicker.Value;
         EndDate = this.endDatePicker.Value;
      }

      public void GenerateReportHeader(string fileName, bool append)
      {
         // Generate report
         if (!System.IO.Directory.Exists(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + "\\Report"))
         {
            System.IO.Directory.CreateDirectory(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + "\\Report");
         }
         using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + "\\Report\\" + fileName, append))
         {
            sr.WriteLine("StockName;AddedValue(%);Strategy;StartDate;EndDate;Amount;Reinvest;AmendOrders;FixedFee;TaxRate");
         }
      }
      public void GenerateReportLine(string fileName, StockSerie stockSerie, StockPortofolio portofolio)
      {
         using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + "\\Report\\" + fileName, true))
         {
            sr.WriteLine(stockSerie.StockName + ";" + portofolio.TotalAddedValue.ToString() + ";" + this.SelectedStrategyName + ";" +
                this.StartDate.ToShortDateString() + ";" + this.EndDate.ToShortDateString() + ";" + this.amount.ToString() +
                ";" + this.reinvest.ToString() + ";" + this.amendOrders.ToString() + ";" + this.fixedFee.ToString() + ";" + this.taxRate.ToString());
         }
      }
   }
}
