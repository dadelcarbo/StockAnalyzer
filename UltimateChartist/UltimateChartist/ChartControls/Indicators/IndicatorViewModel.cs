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
    }
    public class IndicatorParameterViewModel<T> : ViewModelBase, IIndicatorParameterViewModel
    {
        public T Value { get; set; }

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
                        this.Parameters.Add(new IndicatorParameterViewModel<double>()
                        {
                            Value = (double)prop.GetValue(indicator),
                            Parameter = attribute
                        });
                        break;
                    case "Int32":
                        this.Parameters.Add(new IndicatorParameterViewModel<int>()
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
                            var binding = new Binding($"PriceIndicators[{index}].Series.Brush");
                            lineSeries.SetBinding(LineSeries.StrokeProperty, binding);
                            binding = new Binding($"PriceIndicators[{index}].Series.Thickness");
                            lineSeries.SetBinding(LineSeries.StrokeThicknessProperty, binding);

                            this.CartesianSeries.Add(lineSeries);
                        }
                        break;
                    case "IndicatorRangeSeries":
                        break;
                    case "IndicatorBandSeries":
                        {
                            var series = (IndicatorBandSeries)indicatorSeries;
                            //var lineSeries = new LineSeries()
                            //{
                            //    Stroke = series.DownBrush,
                            //    StrokeThickness = series.DownThickness,
                            //    CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            //    ValueBinding = new PropertyNameDataPointBinding("Down")
                            //};
                            //this.CartesianSeries.Add(lineSeries);
                            //lineSeries = new LineSeries()
                            //{
                            //    Stroke = series.UpBrush,
                            //    StrokeThickness = series.UpThickness,
                            //    CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            //    ValueBinding = new PropertyNameDataPointBinding("Up")
                            //};
                            //this.CartesianSeries.Add(lineSeries);

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
                            this.CartesianSeries.Add(rangeSeries);

                            var lineSeries = new LineSeries()
                            {
                                Stroke = series.MidBrush,
                                StrokeThickness = series.MidThickness,
                                CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                                ValueBinding = new PropertyNameDataPointBinding("Mid")
                            };
                            this.CartesianSeries.Add(lineSeries);
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Series type not implemented {indicatorSeries.GetType().Name} in IndicatorViewModel");
                }
            }

            this.CartesianSeries.ForEach(cs => cs.ItemsSource = indicator.Series.Values);
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
                this.CartesianSeries.ForEach(cs => cs.ItemsSource = indicator.Series.Values);
            }
        }

        public List<IIndicatorParameterViewModel> Parameters { get; } = new List<IIndicatorParameterViewModel>();
        public List<CartesianSeries> CartesianSeries { get; } = new List<CartesianSeries>();
    }
}
