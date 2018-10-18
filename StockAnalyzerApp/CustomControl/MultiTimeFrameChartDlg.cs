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
            this.fullGraphUserControl1 = new FullGraphUserControl(StockSerie.StockBarDuration.Weekly);
            this.fullGraphUserControl2 = new FullGraphUserControl(StockSerie.StockBarDuration.TLB);
            this.fullGraphUserControl3 = new FullGraphUserControl(StockSerie.StockBarDuration.Daily);
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
                case StockSerie.Groups.FUTURE:
                    fullGraphUserControl1.SetDuration(StockSerie.StockBarDuration.TLB_9D);
                    fullGraphUserControl2.SetDuration(StockSerie.StockBarDuration.TLB_3D);
                    fullGraphUserControl3.SetDuration(StockSerie.StockBarDuration.TLB);
                    break;
                case StockSerie.Groups.INTRADAY:
                    fullGraphUserControl1.SetDuration(StockSerie.StockBarDuration.TLB_9D);
                    fullGraphUserControl2.SetDuration(StockSerie.StockBarDuration.TLB_3D);
                    fullGraphUserControl3.SetDuration(StockSerie.StockBarDuration.TLB);
                    break;
            }

            InitialiseStockCombo();
        }

        public void ApplyTheme()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.fullGraphUserControl1.CurrentStockSerie = currentStockSerie;
                this.fullGraphUserControl1.ApplyTheme();
                this.fullGraphUserControl2.CurrentStockSerie = currentStockSerie;
                this.fullGraphUserControl2.ApplyTheme();
                this.fullGraphUserControl3.CurrentStockSerie = currentStockSerie;
                this.fullGraphUserControl3.ApplyTheme();
            }
        }

        private void StockNameComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            StockSerie selectedSerie = null;
            if (StockDictionary.StockDictionarySingleton.ContainsKey(stockNameComboBox.SelectedItem.ToString()))
            {
                selectedSerie = StockDictionary.StockDictionarySingleton[stockNameComboBox.SelectedItem.ToString()];
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

            var stocks = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(this.selectedGroup)).Select(s => s.StockName);
            foreach (string stockName in stocks)
            {
                if (StockDictionary.StockDictionarySingleton.Keys.Contains(stockName))
                {
                    StockSerie stockSerie = StockDictionary.StockDictionarySingleton[stockName];
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
