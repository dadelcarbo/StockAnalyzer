using StockAnalyzer.StockClasses;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public partial class BestTrendDlg : Form
    {
        public BestTrendDlg(string group, BarDuration barDuration)
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
