using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public partial class AddStockAlertDlg : Form
    {
        public AddStockAlertDlg(AddStockAlertViewModel viewModel)
        {
            InitializeComponent();
            this.addStockAlert1.DataContext = viewModel;
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
