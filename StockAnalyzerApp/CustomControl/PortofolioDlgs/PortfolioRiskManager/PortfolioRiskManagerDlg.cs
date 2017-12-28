using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager
{
    public partial class PortfolioRiskManagerDlg : Form
    {

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public PortfolioRiskManagerDlg()
        {
            InitializeComponent();
            portofolioRiskManagerUserControl1.SelectedStockChanged += PortofolioRiskManagerUserControl1_SelectedStockChanged;
        }

        private void PortofolioRiskManagerUserControl1_SelectedStockChanged(string stockName, bool activateMainWindow)
        {
            this.SelectedStockChanged?.Invoke(stockName, activateMainWindow);
        }
    }
}
