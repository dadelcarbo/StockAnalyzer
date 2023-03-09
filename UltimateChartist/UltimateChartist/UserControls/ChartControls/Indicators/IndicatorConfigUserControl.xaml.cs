using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using UltimateChartist.Indicators;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.UserControls.ChartControls.Indicators;

/// <summary>
/// Interaction logic for IndicatorConfigUserControl.xaml
/// </summary>
public partial class IndicatorConfigUserControl : Window
{
    public IndicatorViewModel IndicatorViewModel { get; }
    public IndicatorConfigUserControl(IndicatorViewModel indicatorViewModel)
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
                case "Boolean":
                    CreateBoolParameter(parameter);
                    break;
                default:
                    throw new NotImplementedException($"Parameter type not implemented {parameter.Parameter.Type.Name} in IndicatorConfigUserControl");
            }
        }

        switch (indicatorViewModel.Indicator.Series.GetType().Name)
        {
            case "IndicatorLineSeries":
                {
                    var curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorLineSeries).Curve;
                    this.curvePanel.Children.Add(curveConfig);
                }
                break;
            case "IndicatorLineSignalSeries":
                {
                    var curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorLineSignalSeries).Curve;
                    this.curvePanel.Children.Add(curveConfig);

                    curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorLineSignalSeries).Signal;
                    this.curvePanel.Children.Add(curveConfig);
                }
                break;
            case "IndicatorRangeSeries":
                {
                    var rangeConfig = new RangeConfigUserControl();
                    rangeConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorRangeSeries).Area;
                    this.curvePanel.Children.Add(rangeConfig);
                }
                break;
            case "IndicatorBandSeries":
                {
                    var rangeConfig = new RangeConfigUserControl();
                    rangeConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorBandSeries).Area;
                    this.curvePanel.Children.Add(rangeConfig);
                    var curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = (indicatorViewModel.Indicator.Series as IndicatorBandSeries).MidLine;
                    this.curvePanel.Children.Add(curveConfig);
                }
                break;
            case "IndicatorTrailSeries":
                {
                    var trailSerie = indicatorViewModel.Indicator.Series as IndicatorTrailSeries;
                    var rangeConfig = new RangeConfigUserControl();
                    rangeConfig.DataContext = trailSerie.Long;
                    this.curvePanel.Children.Add(rangeConfig);

                    rangeConfig = new RangeConfigUserControl();
                    rangeConfig.DataContext = trailSerie.Short;
                    this.curvePanel.Children.Add(rangeConfig);

                    var curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = trailSerie.LongReentry;
                    this.curvePanel.Children.Add(curveConfig);

                    curveConfig = new CurveConfigUserControl();
                    curveConfig.DataContext = trailSerie.ShortReentry;
                    this.curvePanel.Children.Add(curveConfig);
                }
                break;
        }

        IndicatorViewModel = indicatorViewModel;
    }

    private void CreateBoolParameter(IIndicatorParameterViewModel parameter)
    {
        var boolParameter = parameter.Parameter as IndicatorParameterBoolAttribute;
        var label = new System.Windows.Controls.Label() { Content = parameter.Parameter.Name, Width = 80, Margin = new Thickness(2) };
        var upDown = new CheckBox() { VerticalAlignment = VerticalAlignment.Center };

        var binding = new Binding("Indicator." + parameter.PropertyName) { Mode = BindingMode.TwoWay };
        upDown.SetBinding(CheckBox.IsCheckedProperty, binding);

        var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
        stackPanel.Children.Add(label);
        stackPanel.Children.Add(upDown);
        this.parameterPanel.Children.Add(stackPanel);
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
            NumberDecimalDigits= 0
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
        var doubleParameter = parameter.Parameter as IndicatorParameterDecimalAttribute;
        var label = new System.Windows.Controls.Label() { Content = parameter.Parameter.Name, Width = 80, Margin = new Thickness(2) };
        var upDown = new RadNumericUpDown()
        {
            Minimum = (double) doubleParameter.Min,
            Maximum = (double)doubleParameter.Max,
            SmallChange = (double)doubleParameter.Step,
            LargeChange =  (double) doubleParameter.Step * 10,
            Margin = new Thickness(2),
            NumberDecimalDigits = -(int)Math.Round((Math.Log10((double) doubleParameter.Step)))
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
