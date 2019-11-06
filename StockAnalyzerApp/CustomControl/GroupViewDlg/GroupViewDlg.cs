using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    public partial class GroupViewDlg : Form
    {
        public GroupViewDlg()
        {
            InitializeComponent();

            var control = this.elementHost1.Child as GroupUserViewControl;
            control.DataContext = new GroupViewModel(StockAnalyzer.StockClasses.StockSerie.Groups.SECTORS);
        }
    }
}
