using StockAnalyzer.StockClasses;
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

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager
{
    /// <summary>
    /// Interaction logic for PortofolioRiskManagerUserControl.xaml
    /// </summary>
    public partial class PortofolioRiskManagerUserControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public PortofolioRiskManagerUserControl()
        {
            InitializeComponent();

            this.PositionDataGrid.MouseDoubleClick += PositionDataGrid_MouseDoubleClick;
        }

        private void PositionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var data = PositionDataGrid.CurrentItem as PositionViewModel;
            if (data == null || !StockDictionary.StockDictionarySingleton.ContainsKey(data.StockName)) return;
            this.SelectedStockChanged?.Invoke(data.StockName, false);
        }
    }
}
