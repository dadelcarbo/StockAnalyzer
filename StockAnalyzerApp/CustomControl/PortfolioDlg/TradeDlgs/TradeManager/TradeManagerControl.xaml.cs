using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

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
