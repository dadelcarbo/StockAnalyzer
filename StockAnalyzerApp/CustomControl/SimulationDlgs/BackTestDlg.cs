using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public partial class BackTestDlg : Form
    {
        public BackTestDlg()
        {
            InitializeComponent();
            var viewModel = backTestControl.ViewModel;
            this.Closing += (s, e) => { viewModel.Cancel(); };

            viewModel.Completed += Completed;
        }

        private void Completed()
        {
            this.TopMost = true;
            this.TopMost = false;
        }
    }
}
