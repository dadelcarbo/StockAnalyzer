using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.CustomControl.GraphControls;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class MultiTimeFrameChartDlg : Form
    {
        private StockSerie.Groups selectedGroup;
        public MultiTimeFrameChartDlg()
        {
            this.fullGraphUserControl1 = new FullGraphUserControl(BarDuration.Monthly);
            this.fullGraphUserControl2 = new FullGraphUserControl(BarDuration.Weekly);
            this.fullGraphUserControl3 = new FullGraphUserControl(BarDuration.Daily);
            InitializeComponent();

            this.fullGraphUserControl1.OnMouseDateChanged += fullGraphUserControl2.MouseDateChanged;
            this.fullGraphUserControl1.OnMouseDateChanged += fullGraphUserControl3.MouseDateChanged;

            this.fullGraphUserControl2.OnMouseDateChanged += fullGraphUserControl1.MouseDateChanged;
            this.fullGraphUserControl2.OnMouseDateChanged += fullGraphUserControl3.MouseDateChanged;

            this.fullGraphUserControl3.OnMouseDateChanged += fullGraphUserControl1.MouseDateChanged;
            this.fullGraphUserControl3.OnMouseDateChanged += fullGraphUserControl2.MouseDateChanged;
        }

        public void Initialize(StockSerie.Groups group, StockSerie stockSerie)
        {
            this.selectedGroup = group;
            this.CurrentStockSerie = stockSerie;

            switch (this.selectedGroup)
            {
                case StockSerie.Groups.TURBO:
                    fullGraphUserControl3.SetDuration(BarDuration.Daily);
                    break;
            }

            InitialiseStockCombo();
        }

        public void ApplyTheme()
        {
            using MethodLogger ml = new MethodLogger(this);
            this.fullGraphUserControl1.CurrentStockSerie = currentStockSerie;
            this.fullGraphUserControl1.ApplyTheme();
            this.fullGraphUserControl2.CurrentStockSerie = currentStockSerie;
            this.fullGraphUserControl2.ApplyTheme();
            this.fullGraphUserControl3.CurrentStockSerie = currentStockSerie;
            this.fullGraphUserControl3.ApplyTheme();
        }

        private void StockNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            StockSerie selectedSerie = null;
            if (StockDictionary.Instance.ContainsKey(stockNameComboBox.SelectedItem.ToString()))
            {
                selectedSerie = StockDictionary.Instance[stockNameComboBox.SelectedItem.ToString()];
            }
            else
            {
                throw new ApplicationException("Data for " + stockNameComboBox.SelectedItem.ToString() + "does not exist");
            }
            // Set the new selected serie
            CurrentStockSerie = selectedSerie;
        }

        private void InitialiseStockCombo()
        {
            // Initialise Combo values
            stockNameComboBox.Items.Clear();
            stockNameComboBox.SelectedItem = string.Empty;

            var stocks = StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.selectedGroup)).Select(s => s.StockName);
            foreach (string stockName in stocks)
            {
                if (StockDictionary.Instance.Keys.Contains(stockName))
                {
                    StockSerie stockSerie = StockDictionary.Instance[stockName];
                    stockNameComboBox.Items.Add(stockName);
                }
            }
            stockNameComboBox.SelectedItem = this.currentStockSerie.StockName;
        }

        private StockSerie currentStockSerie;

        public StockSerie CurrentStockSerie
        {
            get { return currentStockSerie; }
            set
            {
                if (currentStockSerie != value)
                {
                    currentStockSerie = value;
                    this.ApplyTheme();
                }
            }
        }
    }
}
