using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class MultiTimeFrameChartDlg : Form
    {
        private StockSerie.Groups selectedGroup;
        public MultiTimeFrameChartDlg()
        {
            InitializeComponent();
        }

        public void Initialize(StockSerie.Groups group, StockSerie stockSerie)
        {
            this.selectedGroup = group;
            InitialiseStockCombo();

            this.CurrentStockSerie = stockSerie;
        }

        public void ApplyTheme()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.fullGraphUserControl1.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Bar_6);
                this.fullGraphUserControl2.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Bar_3);
                this.fullGraphUserControl3.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Daily);
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
