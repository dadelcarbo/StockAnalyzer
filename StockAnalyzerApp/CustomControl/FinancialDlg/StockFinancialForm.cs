using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.FinancialDlg
{
    public partial class StockFinancialForm : Form
    {
        private StockAnalyzer.StockClasses.StockFinancial stockFinancial;

        public StockFinancialForm()
        {
            InitializeComponent();
        }

        public StockFinancialForm(StockSerie stockSerie)
        {
            InitializeComponent();

            this.Text = "Financials for " + stockSerie.ShortName + " - " + stockSerie.StockName;

            this.stockFinancial = stockSerie.StockAnalysis.Financial;
            this.stockFinancialUserControl1.DataContext = stockFinancial;
        }
    }
}
