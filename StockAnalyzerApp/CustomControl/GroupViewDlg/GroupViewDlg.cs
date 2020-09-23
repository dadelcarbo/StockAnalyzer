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
