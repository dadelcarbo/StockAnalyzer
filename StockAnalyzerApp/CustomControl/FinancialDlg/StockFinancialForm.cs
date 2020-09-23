using StockAnalyzer.StockClasses;
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
