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
        public StockFinancialForm()
        {
            InitializeComponent();
        }

        public StockFinancialForm(StockSerie stockSerie)
        {
            InitializeComponent();

            this.Text = "Financials for " + stockSerie.ShortName + " - " + stockSerie.StockName;
            stockSerie.Financial.CalculateRatios();
            this.stockFinancialUserControl1.DataContext = stockSerie.Financial;
        }
    }
}
