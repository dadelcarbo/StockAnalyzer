using StockAnalyzer.StockClasses;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockAnalyzerApp.CustomControl.FinancialDlg
{
    /// <summary>
    /// Interaction logic for StockFinancialUserControl.xaml
    /// </summary>
    public partial class StockFinancialUserControl : UserControl
    {
        public StockFinancialUserControl()
        {
            InitializeComponent();

            foreach (var prop in (typeof(StockFinancial)).GetProperties().Where(p=>p.GetCustomAttributes(false).Any(a=> a is StockGeneralAttibute)))
            {
                if (prop.PropertyType == typeof(DataTable) || prop.PropertyType.Name.StartsWith("List"))
                    continue;
                StackPanel propertyRow = new StackPanel() { Orientation = Orientation.Horizontal };

                // Create financials control
                Label labelTitle = new Label();
                labelTitle.Content = prop.Name;
                labelTitle.Width = 100;

                TextBlock labelValue = new TextBlock();
                labelValue.Margin = new Thickness(0, 5, 0, 0);
                if (prop.PropertyType == typeof(long))
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name) { StringFormat = "#,##0,K" });
                }
                else if (prop.GetCustomAttributes(false).Any(a=> a is StockPercentAttribute))
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name) { StringFormat = "P2" });
                }
                else
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name));
                }
                propertyRow.Children.Add(labelTitle);
                propertyRow.Children.Add(labelValue);

                this.generalPropertyPanel1.Children.Add(propertyRow);
            }
            foreach (var prop in (typeof(StockFinancial)).GetProperties().Where(p=>p.GetCustomAttributes(false).Any(a=> a is StockFinancialAttibute)))
            {
                if (prop.PropertyType == typeof(DataTable) || prop.PropertyType.Name.StartsWith("List"))
                    continue;
                StackPanel propertyRow = new StackPanel() { Orientation = Orientation.Horizontal };

                // Create financials control
                Label labelTitle = new Label();
                labelTitle.Content = prop.Name;
                labelTitle.Width = 160;

                TextBlock labelValue = new TextBlock();
                labelValue.Margin = new Thickness(0, 5, 0, 0);
                if (prop.PropertyType == typeof(long))
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name) { StringFormat = "#,##0,K" });
                }
                else if (prop.GetCustomAttributes(false).Any(a=> a is StockPercentAttribute))
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name) { StringFormat = "P2" });
                }
                else
                {
                    labelValue.SetBinding(TextBlock.TextProperty, new Binding(prop.Name));
                }
                propertyRow.Children.Add(labelTitle);
                propertyRow.Children.Add(labelValue);

                this.generalPropertyPanel2.Children.Add(propertyRow);
            }
        }
    }
}
