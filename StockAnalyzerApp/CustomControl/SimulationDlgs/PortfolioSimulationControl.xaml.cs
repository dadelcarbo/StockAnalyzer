using System.Windows;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    /// <summary>
    /// Interaction logic for PortfolioSimulationControl.xaml
    /// </summary>
    public partial class PortfolioSimulationControl : System.Windows.Controls.UserControl
    {
        Form parent;

        public PortfolioSimulationViewModel ViewModel { get; set; }
 
        public PortfolioSimulationControl(Form parentForm)
        {
            this.parent = parentForm;
            InitializeComponent();

            this.ViewModel = (PortfolioSimulationViewModel)this.Resources["ViewModel"];
        }

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Perform();
        }
    }
}
