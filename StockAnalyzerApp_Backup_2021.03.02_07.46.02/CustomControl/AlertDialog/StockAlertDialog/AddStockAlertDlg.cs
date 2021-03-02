using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
