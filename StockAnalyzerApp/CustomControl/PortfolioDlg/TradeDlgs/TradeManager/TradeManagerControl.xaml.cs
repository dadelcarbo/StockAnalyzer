using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{
    /// <summary>
    /// Interaction logic for TradeManagerControl.xaml
    /// </summary>
    public partial class TradeManagerControl : UserControl
    {
        private TradeManagerViewModel viewModel;

        public TradeManagerControl()
        {
            InitializeComponent();
        }

        public TradeManagerViewModel ViewModel
        {
            get => viewModel;
            set
            {
                if (viewModel != value)
                {
                    this.viewModel = value;
                    this.DataContext = value;
                }
            }
        }

        static int index = 0;
        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, Telerik.Windows.Controls.Data.PropertyGrid.AutoGeneratingPropertyDefinitionEventArgs e)
        {
            var attribute = e.PropertyDefinition.PropertyDescriptor.Attributes[typeof(PropertyAttribute)] as PropertyAttribute;
            if (attribute == null)
                e.Cancel = true;
            else
            {
                e.Cancel = false;
                if (!string.IsNullOrEmpty(attribute.Format))
                {
                    e.PropertyDefinition.Binding.StringFormat = attribute.Format;
                }
                if (!string.IsNullOrEmpty(attribute.Group))
                {
                    e.PropertyDefinition.GroupName = attribute.Group;
                }
                e.PropertyDefinition.OrderIndex = index++;
            }
        }
    }
}
