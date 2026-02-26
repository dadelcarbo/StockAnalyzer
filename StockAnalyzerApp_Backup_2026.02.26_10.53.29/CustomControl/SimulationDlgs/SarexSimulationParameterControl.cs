using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using System.IO;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockStrategyClasses;
using System.Globalization;

namespace StockAnalyzerApp.CustomControl
{
    public partial class SarexSimulationParameterControl : UserControl
    {
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

        private StockPersonality stockPersonality = null;

        public StockPersonality Personality
        {
            get
            {
                ValidateInputParameters();
                return stockPersonality;
            }
            set
            {
                if (value == null)
                {
                    value = StockPersonality.CreateDefaultPersonality();
                }
                this.stockPersonality = value;
                this.buyGrowLimitTextBox.Text = (value.BuyMargin * 100.0f).ToString(StockAnalyzerForm.EnglishCulture);
                this.sellFallLimitTextBox.Text = (value.SellMargin * 100.0f).ToString(StockAnalyzerForm.EnglishCulture);
                this.accelerationStepTextBox.Text = value.IndicatorSARAccelerationStep.ToString(StockAnalyzerForm.EnglishCulture);
                this.fastSwingFactorTextBox.Text = value.IndicatorSARFastSwingFactor.ToString(StockAnalyzerForm.EnglishCulture);
            }
        }
        public string SelectedStrategy
        {
            get { return this.strategyComboBox.SelectedItem.ToString(); }
            set { this.strategyComboBox.SelectedIndex = this.strategyComboBox.Items.IndexOf(value); }
        }


        public float fixedFee;
        public float taxRate;
        #endregion

        public SarexSimulationParameterControl()
        {
            InitializeComponent();

            // Initialise date pickers
            this.startDatePicker.Value = new System.DateTime(2009, 1, 1);
            this.endDatePicker.Value = System.DateTime.Today;

            // Initialize strategy combo
            this.strategyComboBox.Enabled = true;
            this.strategyComboBox.Items.Clear();
            List<string> strategyList = StrategyManager.GetStrategyList();
            foreach (string name in strategyList)
            {
                this.strategyComboBox.Items.Add(name);
            }
            this.strategyComboBox.SelectedIndex = 0;
        }

        public void ValidateInputParameters()
        {
            reinvest = this.reinvestCheckBox.Checked;
            amount = float.Parse(this.amountTextBox.Text, StockAnalyzerForm.EnglishCulture);
            fixedFee = float.Parse(this.fixedFeeTextBox.Text, StockAnalyzerForm.EnglishCulture);
            taxRate = float.Parse(this.taxRateTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;

            stockPersonality.BuyMargin = float.Parse(this.buyGrowLimitTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;
            stockPersonality.SellMargin = float.Parse(this.sellFallLimitTextBox.Text, StockAnalyzerForm.EnglishCulture) / 100.0f;
            stockPersonality.IndicatorSARAccelerationStep = float.Parse(this.accelerationStepTextBox.Text, StockAnalyzerForm.EnglishCulture);
            stockPersonality.IndicatorSARFastSwingFactor = float.Parse(this.fastSwingFactorTextBox.Text, StockAnalyzerForm.EnglishCulture);

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
            if (!System.IO.Directory.Exists(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report"))
            {
                System.IO.Directory.CreateDirectory(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report");
            }
            using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + fileName, append))
            {
                sr.WriteLine("StockName;AddedValue(%);Strategy;StartDate;EndDate;Amount;Reinvest;AmendOrders;" +
                    this.Personality.ReportHeaderString() + ";" +
                    "FixedFee;TaxRate");
            }
        }
        public void GenerateReportLine(string fileName, StockSerie stockSerie, StockPortofolio portofolio)
        {
            using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + fileName, true))
            {
                sr.WriteLine(stockSerie.StockName + ";" + portofolio.TotalAddedValue.ToString() + ";" + this.SelectedStrategy + ";" +
                    this.StartDate.ToShortDateString() + ";" + this.EndDate.ToShortDateString() + ";" + this.amount.ToString() +
                    ";" + this.reinvest.ToString() + ";" + this.amendOrders.ToString() + ";" +
                    stockPersonality.ToString() + ";" + this.fixedFee.ToString() + ";" + this.taxRate.ToString());
            }
        }

        private void copyBtn_Click(object sender, EventArgs e)
        {
            if (this.Personality != null)
            {
                StockPersonality.StockPersonalityClipboard = this.Personality.Clone();
            }
        }
        private void pasteBtn_Click(object sender, EventArgs e)
        {
            if (StockPersonality.StockPersonalityClipboard != null)
            {
                this.Personality = StockPersonality.StockPersonalityClipboard;
            }
        }
    }
}