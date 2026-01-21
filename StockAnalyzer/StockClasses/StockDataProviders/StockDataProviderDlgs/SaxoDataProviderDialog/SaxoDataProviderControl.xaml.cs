using System.Windows.Controls;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    /// <summary>
    /// Interaction logic for SaxoDataProviderControl.xaml
    /// </summary>
    public partial class SaxoDataProviderControl : UserControl
    {
        private SaxoDataProviderViewModel viewModel;
        private SaxoDataProviderDlg form;

        public SaxoDataProviderControl(SaxoDataProviderDlg saxoDataProviderDlg)
        {
            InitializeComponent();
            this.DataContext = this.viewModel = new SaxoDataProviderViewModel();

            this.form = saxoDataProviderDlg;
        }

        public SaxoDataProviderViewModel ViewModel => this.viewModel;

        private void cancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            form.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            form.Close();
        }
        private void saveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            form.DialogResult = System.Windows.Forms.DialogResult.OK;

            this.viewModel.Save();

            form.Close();
        }
    }
}
