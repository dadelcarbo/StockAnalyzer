using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.BulletGraph;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls.ChartView;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls.Indicators
{
    /// <summary>
    /// Interaction logic for IndicatorConfigWindow.xaml
    /// </summary>
    public partial class IndicatorConfigWindow : Window
    {
        public IndicatorConfigWindow(IIndicator indicator)
        {
            InitializeComponent();

            this.DataContext = indicator;

            foreach (PropertyInfo prop in indicator.GetType().GetProperties())
            {
                if (prop.GetCustomAttributes(typeof(IndicatorParameterAttribute), true).Any())
                {
                    var label = new System.Windows.Controls.Label() { Content = prop.Name };
                    var upDown = new RadNumericUpDown() { Minimum = 1, Maximum = 500 };

                    var binding = new Binding(prop.Name) { Mode = BindingMode.TwoWay };
                    upDown.SetBinding(RadNumericUpDown.ValueProperty, binding);

                    var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(label);
                    stackPanel.Children.Add(upDown);
                    this.ParameterPanel.Children.Add(stackPanel);
                }
                //else if (prop.GetCustomAttributes(typeof(IndicatorDisplayAttribute), true).Any())
                //{
                //    var label = new System.Windows.Controls.Label() { Content = prop.Name };
                //    var colorPicker = new RadColorPicker();

                //    //var binding = new Binding(prop.Name + ".Brush") { Mode = BindingMode.TwoWay, Converter = new ColorToBrushConverter() };
                //    //colorPicker.SetBinding(RadColorPicker.SelectedColorProperty, binding);

                //    var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                //    stackPanel.Children.Add(label);
                //    stackPanel.Children.Add(colorPicker);
                //    this.DisplayPanel.Children.Add(stackPanel);
                //}
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
