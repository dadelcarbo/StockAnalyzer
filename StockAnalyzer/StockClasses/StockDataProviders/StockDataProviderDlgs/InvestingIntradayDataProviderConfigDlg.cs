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

            vm.StockDico = stockDico;
            vm.Initialize(fileName);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            (this.elementHost1.Child as InvestingConfigControl).ViewModel.Save();
        }
    }
}
