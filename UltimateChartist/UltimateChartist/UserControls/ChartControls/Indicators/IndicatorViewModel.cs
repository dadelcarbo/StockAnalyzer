using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators;

namespace UltimateChartist.UserControls.ChartControls.Indicators;

public interface IIndicatorParameterViewModel
{
    IndicatorParameterAttribute Parameter { get; }
    string PropertyName { get; }
}
public class IndicatorParameterViewModel<T> : ViewModelBase, IIndicatorParameterViewModel
{
    public IndicatorParameterViewModel(string propertyName)
    {
        PropertyName = propertyName;
    }
    public T Value { get; set; }

    public string PropertyName { get; }

    public IndicatorParameterAttribute Parameter { get; set; }
}

public class IndicatorViewModel : ViewModelBase
{
    private StockSerie stockSerie;
    public IndicatorViewModel(IIndicator indicator, StockSerie stockSerie)
    {
        Indicator = indicator;
        this.stockSerie = stockSerie;
        indicator.Initialize(stockSerie);

        // Get Parameters from instrospection
        foreach (PropertyInfo prop in indicator.GetType().GetProperties())
        {
            var attribute = prop.GetCustomAttributes(typeof(IndicatorParameterAttribute), true).FirstOrDefault() as IndicatorParameterAttribute;
            if (attribute == null)
                continue;
            switch (attribute.Type.Name)
            {
                case "Double":
                    Parameters.Add(new IndicatorParameterViewModel<double>(prop.Name)
                    {
                        Value = (double)prop.GetValue(indicator),
                        Parameter = attribute
                    });
                    break;
                case "Int32":
                    Parameters.Add(new IndicatorParameterViewModel<int>(prop.Name)
                    {
                        Value = (int)prop.GetValue(indicator),
                        Parameter = attribute
                    });
                    break;
                case "Boolean":
                    Parameters.Add(new IndicatorParameterViewModel<bool>(prop.Name)
                    {
                        Value = (bool)prop.GetValue(indicator),
                        Parameter = attribute
                    });
                    break;
                default:
                    throw new NotImplementedException($"Attribute type not implemented {attribute.Type.Name} in IndicatorViewModel");
            }
        }

        // Create GraphSeries from instrospection
        var indicatorSeries = indicator.GetType().GetProperty("Series")?.GetValue(indicator);
        if (indicatorSeries != null)
        {
            switch (indicatorSeries.GetType().Name)
            {
                case "IndicatorLineSeries":
                    {
                        var lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("Value")
                        };
                        var binding = new Binding($"Series.Curve.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.Curve.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);
                    }
                    break;
                case "IndicatorLineSignalSeries":
                    {
                        var lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("Value")
                        };
                        var binding = new Binding($"Series.Curve.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.Curve.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);

                        lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("Signal")
                        };
                        binding = new Binding($"Series.Signal.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.Signal.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);
                    }
                    break;
                case "IndicatorRangeSeries":
                    {
                        var rangeSeries = new RangeSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            HighBinding = new PropertyNameDataPointBinding("High"),
                            LowBinding = new PropertyNameDataPointBinding("Low"),
                            StrokeMode = RangeSeriesStrokeMode.LowAndHighPoints,
                        };

                        var binding = new Binding($"Series.Area.Fill");
                        rangeSeries.SetBinding(RangeSeries.FillProperty, binding);
                        binding = new Binding($"Series.Area.Thickness");
                        rangeSeries.SetBinding(RangeSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Area.Stroke");
                        rangeSeries.SetBinding(RangeSeries.StrokeProperty, binding);

                        binding = new Binding($"Series.Values");
                        rangeSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        rangeSeries.DataContext = indicator;

                        CartesianSeries.Add(rangeSeries);
                    }
                    break;
                case "IndicatorBandSeries":
                    {
                        var rangeSeries = new RangeSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            HighBinding = new PropertyNameDataPointBinding("High"),
                            LowBinding = new PropertyNameDataPointBinding("Low"),
                            StrokeMode = RangeSeriesStrokeMode.LowAndHighPoints,
                        };

                        var binding = new Binding($"Series.Area.Fill");
                        rangeSeries.SetBinding(RangeSeries.FillProperty, binding);
                        binding = new Binding($"Series.Area.Thickness");
                        rangeSeries.SetBinding(RangeSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Area.Stroke");
                        rangeSeries.SetBinding(RangeSeries.StrokeProperty, binding);

                        binding = new Binding($"Series.Values");
                        rangeSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        rangeSeries.DataContext = indicator;

                        CartesianSeries.Add(rangeSeries);

                        var lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("Mid")
                        };
                        binding = new Binding($"Series.MidLine.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.MidLine.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);
                    }
                    break;
                case "IndicatorTrailSeries":
                    {
                        var rangeSeries = new RangeSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            HighBinding = new PropertyNameDataPointBinding("High"),
                            LowBinding = new PropertyNameDataPointBinding("Long"),
                            StrokeMode = RangeSeriesStrokeMode.LowPoints
                        };

                        var binding = new Binding($"Series.Long.Fill");
                        rangeSeries.SetBinding(RangeSeries.FillProperty, binding);
                        binding = new Binding($"Series.Long.Thickness");
                        rangeSeries.SetBinding(RangeSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Long.Stroke");
                        rangeSeries.SetBinding(RangeSeries.StrokeProperty, binding);

                        binding = new Binding($"Series.Values");
                        rangeSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        rangeSeries.DataContext = indicator;

                        CartesianSeries.Add(rangeSeries);

                        rangeSeries = new RangeSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            HighBinding = new PropertyNameDataPointBinding("Short"),
                            LowBinding = new PropertyNameDataPointBinding("Low"),
                            StrokeMode = RangeSeriesStrokeMode.HighPoints
                        };

                        binding = new Binding($"Series.Short.Fill");
                        rangeSeries.SetBinding(RangeSeries.FillProperty, binding);
                        binding = new Binding($"Series.Short.Thickness");
                        rangeSeries.SetBinding(RangeSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Short.Stroke");
                        rangeSeries.SetBinding(RangeSeries.StrokeProperty, binding);

                        binding = new Binding($"Series.Values");
                        rangeSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        rangeSeries.DataContext = indicator;

                        CartesianSeries.Add(rangeSeries);

                        var lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("LongReentry")
                        };
                        binding = new Binding($"Series.LongReentry.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.LongReentry.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);

                        lineSeries = new LineSeries()
                        {
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("ShortReentry")
                        };
                        binding = new Binding($"Series.ShortReentry.Stroke");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeProperty, binding);
                        binding = new Binding($"Series.ShortReentry.Thickness");
                        lineSeries.SetBinding(CategoricalStrokedSeries.StrokeThicknessProperty, binding);
                        binding = new Binding($"Series.Values");
                        lineSeries.SetBinding(ChartSeries.ItemsSourceProperty, binding);
                        lineSeries.DataContext = indicator;

                        CartesianSeries.Add(lineSeries);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Series type not implemented {indicatorSeries.GetType().Name} in IndicatorViewModel");
            }
        }

        indicator.ParameterChanged += Indicator_ParameterChanged;
    }

    public IIndicator Indicator { get; }

    private void Indicator_ParameterChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var indicator = sender as UltimateChartist.Indicators.IndicatorBase;
        if (indicator != null)
        {
            indicator.Initialize(stockSerie);
        }
    }

    public List<IIndicatorParameterViewModel> Parameters { get; } = new List<IIndicatorParameterViewModel>();
    public List<CartesianSeries> CartesianSeries { get; } = new List<CartesianSeries>();
}
