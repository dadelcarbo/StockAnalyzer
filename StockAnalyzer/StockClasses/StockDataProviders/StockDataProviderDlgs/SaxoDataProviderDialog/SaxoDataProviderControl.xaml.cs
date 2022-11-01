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

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    /// <summary>
    /// Interaction logic for SaxoDataProviderControl.xaml
    /// </summary>
    public partial class SaxoDataProviderControl : UserControl
    {
        public SaxoDataProviderControl(StockDictionary stockDico, string cfgFile)
        {
            InitializeComponent();
            this.DataContext = new SaxoDataProviderViewModel(stockDico, cfgFile);
        }

        private void RadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                (this.DataContext as SaxoDataProviderViewModel).UnderlyingChanged((Entry)e.AddedItems[0]);
            }
        }
    }
}
