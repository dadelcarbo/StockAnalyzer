using SharpDX.Direct2D1.Effects;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls.Indicators
{
    /// <summary>
    /// Interaction logic for IndicatorConfigWindow.xaml
    /// </summary>
    public partial class IndicatorConfigWindow : Window
    {
        public IndicatorViewModel IndicatorViewModel { get; }
        public IndicatorConfigWindow(IndicatorViewModel indicatorViewModel)
        {
            InitializeComponent();

            this.DataContext = indicatorViewModel;
            foreach (var parameter in indicatorViewModel.Parameters)
            {
                switch (parameter.Parameter.Type.Name)
                {
                    case "Double":
                        CreateDoubleParameter(parameter);
                        break;
                    case "Int32":
                        CreateIntParameter(parameter);
                        break;
                    default:
                        throw new NotImplementedException($"Parameter type not implemented {parameter.Parameter.Type.Name} in IndicatorConfigWindow");
                }
            }

            switch (indicatorViewModel.Indicator.Series.GetType().Name)
            {
                case "IndicatorLineSeries":
                    var curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorLineSeries).Curve;
                    this.curvePanel.Children.Add(curveConfig);
                    break;
            }

            IndicatorViewModel = indicatorViewModel;
        }

        private void CreateIntParameter(IIndicatorParameterViewModel parameter)
        {
            var intParameter = parameter.Parameter as IndicatorParameterIntAttribute;
            var label = new System.Windows.Controls.Label() { Content = parameter.Parameter.Name, Width = 80, Margin = new Thickness(2) };
            var upDown = new RadNumericUpDown()
            {
                Minimum = intParameter.Min,
                Maximum = intParameter.Max,
                Margin = new Thickness(2),
                NumberDecimalDigits = 0
            };

            var binding = new Binding("Indicator." + parameter.PropertyName) { Mode = BindingMode.TwoWay };
            upDown.SetBinding(RadNumericUpDown.ValueProperty, binding);

            var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(upDown);
            this.parameterPanel.Children.Add(stackPanel);
        }
        private void CreateDoubleParameter(IIndicatorParameterViewModel parameter)
        {
            var doubleParameter = parameter.Parameter as IndicatorParameterDoubleAttribute;
            var label = new System.Windows.Controls.Label() { Content = parameter.Parameter.Name, Width = 80, Margin = new Thickness(2) };
            var upDown = new RadNumericUpDown()
            {
                Minimum = doubleParameter.Min,
                Maximum = doubleParameter.Max,
                SmallChange = doubleParameter.Step,
                LargeChange = doubleParameter.Step * 10,
                Margin = new Thickness(2),
                NumberDecimalDigits = -(int)Math.Round((Math.Log10(doubleParameter.Step)))
            };

            var binding = new Binding("Indicator." + parameter.PropertyName) { Mode = BindingMode.TwoWay, StringFormat = doubleParameter.Format };
            upDown.SetBinding(RadNumericUpDown.ValueProperty, binding);

            var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(upDown);
            this.parameterPanel.Children.Add(stackPanel);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
