using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.MultiTimeFrameDlg
{
    public partial class MTFDlg : Form
   {
      public MTFDlg()
      {
         InitializeComponent();
      }

      public MTFUserControl MtfControl
      {
         get { return mtfUserControl1; }
      }
   }
}
