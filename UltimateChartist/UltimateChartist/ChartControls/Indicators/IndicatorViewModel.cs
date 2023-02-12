using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls.Indicators
{
    public interface IIndicatorParameterViewModel
    {
        IndicatorParameterAttribute Parameter { get; }
        string PropertyName { get; }
    }
    public class IndicatorParameterViewModel<T> : ViewModelBase, IIndicatorParameterViewModel
    {
        public IndicatorParameterViewModel(string propertyName)
        {
            this.PropertyName = propertyName;
        }
        public T Value { get; set; }

        public string PropertyName { get; }

        public IndicatorParameterAttribute Parameter { get; set; }
    }

    public class IndicatorViewModel : ViewModelBase
    {
        public IndicatorViewModel(IIndicator indicator, ChartViewModel chartViewModel, int index = 0)
        {
            Indicator = indicator;
            ChartViewModel = chartViewModel;
            indicator.Initialize(ChartViewModel.StockSerie);

            // Get Parameters from instrospection
            foreach (PropertyInfo prop in indicator.GetType().GetProperties())
            {
                var attribute = prop.GetCustomAttributes(typeof(IndicatorParameterAttribute), true).FirstOrDefault() as IndicatorParameterAttribute;
                if (attribute == null)
                    continue;
                switch (attribute.Type.Name)
                {
                    case "Double":
                        this.Parameters.Add(new IndicatorParameterViewModel<double>(prop.Name)
                        {
                            Value = (double)prop.GetValue(indicator),
                            Parameter = attribute
                        });
                        break;
                    case "Int32":
                        this.Parameters.Add(new IndicatorParameterViewModel<int>(prop.Name)
                        {
                            Value = (int)prop.GetValue(indicator),
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
                            var series = (IndicatorLineSeries)indicatorSeries;
                            var lineSeries = new LineSeries()
                            {
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                ValueBinding = new PropertyNameDataPointBinding("Value")
                            };
                            var binding = new Binding($"PriceIndicators[{index}].Series.Curve.Brush");
                            lineSeries.SetBinding(LineSeries.StrokeProperty, binding);
                            binding = new Binding($"PriceIndicators[{index}].Series.Curve.Thickness");
                            lineSeries.SetBinding(LineSeries.StrokeThicknessProperty, binding);
                            binding = new Binding($"PriceIndicators[{index}].Series.Values");
                            lineSeries.SetBinding(LineSeries.ItemsSourceProperty, binding);

                            this.CartesianSeries.Add(lineSeries);
                        }
                        break;
                    case "IndicatorRangeSeries":
                        break;
                    case "IndicatorBandSeries":
                        {
                            var series = (IndicatorBandSeries)indicatorSeries;

                            var rangeSeries = new RangeSeries()
                            {
                                StrokeMode = Telerik.Charting.RangeSeriesStrokeMode.LowAndHighPoints,
                                Stroke = series.Stroke,
                                StrokeThickness = 1,
                                Fill = series.Fill,
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                HighBinding = new PropertyNameDataPointBinding("Up"),
                                LowBinding = new PropertyNameDataPointBinding("Down")
                            };
                            var binding = new Binding($"PriceIndicators[{index}].Series.Values");
                            rangeSeries.SetBinding(RangeSeries.ItemsSourceProperty, binding);
                            this.CartesianSeries.Add(rangeSeries);

                            var lineSeries = new LineSeries()
                            {
                                Stroke = series.MidBrush,
                                StrokeThickness = series.MidThickness,
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                ValueBinding = new PropertyNameDataPointBinding("Mid")
                            };
                            binding = new Binding($"PriceIndicators[{index}].Series.Values");
                            lineSeries.SetBinding(LineSeries.ItemsSourceProperty, binding);
                            this.CartesianSeries.Add(lineSeries);
                        }
                        break;

                    case "IndicatorTrailSeries":
                        {
                            var series = (IndicatorTrailSeries)indicatorSeries;

                            var rangeSeries = new RangeSeries()
                            {
                                StrokeMode = Telerik.Charting.RangeSeriesStrokeMode.LowPoints,
                                Stroke = series.LongStroke,
                                StrokeThickness = 1,
                                Fill = series.LongFill,
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                HighBinding = new PropertyNameDataPointBinding("High"),
                                LowBinding = new PropertyNameDataPointBinding("Long")
                            };
                            var binding = new Binding($"PriceIndicators[{index}].Series.Values");
                            rangeSeries.SetBinding(RangeSeries.ItemsSourceProperty, binding);
                            this.CartesianSeries.Add(rangeSeries);

                            rangeSeries = new RangeSeries()
                            {
                                StrokeMode = Telerik.Charting.RangeSeriesStrokeMode.LowPoints,
                                Stroke = series.ShortStroke,
                                StrokeThickness = 1,
                                Fill = series.ShortFill,
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                HighBinding = new PropertyNameDataPointBinding("Short"),
                                LowBinding = new PropertyNameDataPointBinding("Low")
                            };
                            binding = new Binding($"PriceIndicators[{index}].Series.Values");
                            rangeSeries.SetBinding(RangeSeries.ItemsSourceProperty, binding);
                            this.CartesianSeries.Add(rangeSeries);
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Series type not implemented {indicatorSeries.GetType().Name} in IndicatorViewModel");
                }
            }

            indicator.ParameterChanged += Indicator_ParameterChanged;
        }

        public IIndicator Indicator { get; }
        public ChartViewModel ChartViewModel { get; }

        private void Indicator_ParameterChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var indicator = sender as UltimateChartist.Indicators.IndicatorBase;
            if (indicator != null)
            {
                indicator.Initialize(ChartViewModel.StockSerie);
            }
        }

        public List<IIndicatorParameterViewModel> Parameters { get; } = new List<IIndicatorParameterViewModel>();
        public List<CartesianSeries> CartesianSeries { get; } = new List<CartesianSeries>();
    }
}
