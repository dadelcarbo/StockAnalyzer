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

        public StockFinancialForm(StockAnalyzer.StockClasses.StockFinancial stockFinancial)
        {
            InitializeComponent();

            this.stockFinancial = stockFinancial;
            this.stockFinancialUserControl1.DataContext = stockFinancial;
        }
    }
}
