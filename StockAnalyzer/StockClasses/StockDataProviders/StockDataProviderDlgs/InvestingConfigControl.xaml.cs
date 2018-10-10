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

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    /// <summary>
    /// Interaction logic for InvestingConfigControl.xaml
    /// </summary>
    public partial class InvestingConfigControl : UserControl
    {
        public InvestingConfigViewModel ViewModel { get; set; }

        public InvestingConfigControl()
        {
            InitializeComponent();

            this.ViewModel = new InvestingConfigViewModel();
            this.DataContext = this.ViewModel;
        }
    }
}
