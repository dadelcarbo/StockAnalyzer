using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public partial class BestTrendDlg : Form
    {
        public BestTrendDlg(Groups group, BarDuration barDuration)
        {
            InitializeComponent();

            (this.elementHost1.Child as BestTrendUserControl).ViewModel.BarDuration = barDuration;
            (this.elementHost1.Child as BestTrendUserControl).ViewModel.Group = group;

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
