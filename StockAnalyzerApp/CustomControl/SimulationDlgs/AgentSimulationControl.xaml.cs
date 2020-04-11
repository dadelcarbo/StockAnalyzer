using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
