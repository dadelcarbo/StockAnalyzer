using System.Windows;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    /// <summary>
    /// Interaction logic for AgentSimulationControl.xaml
    /// </summary>
    public partial class AgentSimulationControl : System.Windows.Controls.UserControl
    {
        Form parent;

        public AgentSimulationViewModel ViewModel { get; set; }
        public AgentSimulationControl(Form parentForm)
        {
            this.parent = parentForm;
            InitializeComponent();

            this.ViewModel = (AgentSimulationViewModel)this.Resources["ViewModel"];
        }

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Perform();
        }
    }
}
