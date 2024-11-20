using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public partial class StockAlertManagerDlg : Form
    {
        public StockAlertManagerDlg(StockAlertManagerViewModel viewModel)
        {
            InitializeComponent(viewModel);
            this.stockAlertManagerCtrl.DataContext = viewModel;
        }
        internal void Ok()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        internal void Cancel()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
