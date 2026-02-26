using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.ColorPalette
{
    public partial class PaletteManagerDlg : Form
    {
        public PaletteManagerDlg()
        {
            InitializeComponent();
        }

        public ColorPaletteViewModel ViewModel => paletteManagerUserControl1.Resources["ViewModel"] as ColorPaletteViewModel;
    }
}
