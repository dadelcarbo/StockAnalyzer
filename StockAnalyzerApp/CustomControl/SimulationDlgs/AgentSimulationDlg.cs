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
    public partial class AgentSimulationDlg : Form
    {
        public AgentSimulationDlg()
        {
            InitializeComponent();
            this.Closing += (s, e) => { (this.elementHost1.Child as AgentSimulationControl).ViewModel.Cancel(); };
        }
    }
}
