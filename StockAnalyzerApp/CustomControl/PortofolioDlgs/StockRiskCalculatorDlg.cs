using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings.Properties;
using System.Reflection;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
    public partial class StockRiskCalculatorDlg : Form
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        private RiskCalculatorViewModel viewModel;
        public StockRiskCalculatorDlg()
        {
            InitializeComponent();

            viewModel = new RiskCalculatorViewModel();

            this.riskCalculatorBindingSource.DataSource = viewModel;

            StockAnalyzerForm.MainFrame.StockSerieChanged += MainFrame_StockSerieChanged;
        }

        void MainFrame_StockSerieChanged(StockSerie newSerie, bool ignoreLinkedTheme)
        {
            this.StockSerie = newSerie;
        }

        private StockSerie stockSerie;
        public StockSerie StockSerie
        {
            private get { return stockSerie; }
            set
            {
                stockSerie = value;

                this.viewModel.StockSerie = stockSerie;
            }
        }
    }
}
