using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzer.StockPortfolio.NameMappingDlg
{
    /// <summary>
    /// Interaction logic for NameMappingUserControl.xaml
    /// </summary>
    public partial class NameMappingUserControl : UserControl
    {
        public NameMappingDlg ParentDialog{ get; private set; }
        public ObservableCollection<StockNameMapping> NameMappings { get; }
        public NameMappingUserControl(NameMappingDlg dlg)
        {
            InitializeComponent();

            this.ParentDialog = dlg;
            this.NameMappings = new ObservableCollection<StockNameMapping>(StockPortfolio.Mappings);
            this.DataContext = this;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            StockPortfolio.SaveMappings(this.NameMappings.ToList());
        }

        public void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            StockPortfolio.ResetMappings();
            this.ParentDialog.Close();
        }

        private void saveAndCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            StockPortfolio.SaveMappings(this.NameMappings.ToList());
            this.ParentDialog.Close();
        }
    }
}
