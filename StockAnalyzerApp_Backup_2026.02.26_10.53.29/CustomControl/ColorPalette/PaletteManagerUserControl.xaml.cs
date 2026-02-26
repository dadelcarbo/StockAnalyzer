using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace StockAnalyzerApp.CustomControl.ColorPalette
{
    /// <summary>
    /// Interaction logic for PaletteManagerUserControl.xaml
    /// </summary>
    public partial class PaletteManagerUserControl : UserControl
    {
        ColorPaletteViewModel viewModel;
        public PaletteManagerUserControl()
        {
            InitializeComponent();

            viewModel = this.Resources["ViewModel"] as ColorPaletteViewModel;
        }

        private void DarkColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var paletteEntry = ((Rectangle)sender).DataContext as ColorPaletteItemModel;

            viewModel.CurrentItem = paletteEntry;
            viewModel.UseDark = true;
        }

        private void LightColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var paletteEntry = ((Rectangle)sender).DataContext as ColorPaletteItemModel;

            viewModel.CurrentItem = paletteEntry;
            viewModel.UseDark = false;
        }
    }
}
