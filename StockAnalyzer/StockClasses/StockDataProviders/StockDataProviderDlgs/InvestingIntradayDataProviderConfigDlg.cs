using System;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    public partial class InvestingIntradayDataProviderConfigDlg : Form
    {
        public InvestingIntradayDataProviderConfigDlg(StockDictionary stockDico, string fileName)
        {
            InitializeComponent();

            var vm = (this.elementHost1.Child as InvestingConfigControl).ViewModel;

            vm.Initialize(fileName, stockDico);
            if (fileName.ToLower().Contains("intraday"))
            {
                this.Text = "Investing Intraday Data Provider Configuration";
            }
            else
            {
                this.Text = "Investing Data Provider Configuration";
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            (this.elementHost1.Child as InvestingConfigControl).ViewModel.Save();
        }
    }
}
