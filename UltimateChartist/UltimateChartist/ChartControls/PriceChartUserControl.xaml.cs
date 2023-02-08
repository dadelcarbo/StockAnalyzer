using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls.ChartView;
using UltimateChartist.ChartControls.Indicators;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls
{
    public enum SeriesType
    {
        Candle,
        BarChart,
        Line
    }
    /// <summary>
    /// Interaction logic for PriceChartUserControl.xaml
    /// </summary>
    public partial class PriceChartUserControl : UserControl
    {

        private ChartViewModel viewModel;
        public PriceChartUserControl(ChartViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.Indicators.CollectionChanged += Indicators_CollectionChanged;
            viewModel.PriceIndicators.CollectionChanged += PriceIndicators_CollectionChanged;
            this.OnSeriesTypeChanged();
        }

        private void Indicators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        if (this.viewModel.Indicators.Count > 3)
                            return;
                        IndicatorChartViewModel indicatorChartViewModel = e.NewItems[0] as IndicatorChartViewModel;

                        this.ChartGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        var splitter = new GridSplitter() { Height = 4, SnapsToDevicePixels = true, HorizontalAlignment = HorizontalAlignment.Stretch };
                        Grid.SetRow(splitter, this.ChartGrid.RowDefinitions.Count - 1);
                        this.ChartGrid.Children.Add(splitter);

                        var indicatorPriceChartUserControl = new IndicatorChartUserControl(indicatorChartViewModel);
                        this.ChartGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.5, GridUnitType.Star), MinHeight = 100 });
                        Grid.SetRow(indicatorPriceChartUserControl, this.ChartGrid.RowDefinitions.Count - 1);
                        this.ChartGrid.Children.Add(indicatorPriceChartUserControl);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        IndicatorChartViewModel indicatorChartViewModel = e.OldItems[0] as IndicatorChartViewModel;
                        for (int i = 3; i < this.ChartGrid.Children.Count; i++)
                        {
                            var element = this.ChartGrid.Children[i] as FrameworkElement;
                            if (element.DataContext as IndicatorChartViewModel == indicatorChartViewModel)
                            {
                                this.ChartGrid.RowDefinitions.RemoveRange(i - 1, 2);
                                this.ChartGrid.Children.RemoveRange(i - 1, 2);

                                for (i = i - 1; i < this.ChartGrid.Children.Count; i++)
                                {
                                    element = this.ChartGrid.Children[i] as FrameworkElement;
                                    Grid.SetRow(element, i);
                                }
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("ChartViews_CollectionChanged: " + e.Action + " Not Yet Implement");
            }
        }
        private void PriceIndicators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var indicator = new StockIndicator_EMA() { Period = 69 };

                        var indicatorSeries = new LineSeries()
                        {
                            Stroke = indicator.Series.Brush,
                            StrokeThickness = indicator.Series.Thickness,
                            CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Date" },
                            ValueBinding = new PropertyNameDataPointBinding("Value")
                        };

                        Binding sourceBinding = new Binding($"PriceIndicators[0].Series.Values");
                        sourceBinding.Mode = BindingMode.OneWay;
                        indicatorSeries.SetBinding(ChartSeries.ItemsSourceProperty, sourceBinding);
                        this.Chart.Series.Add(indicatorSeries);

                        indicator.LineSeries = indicatorSeries;
                        indicator.Initialize(this.viewModel.StockSerie);
                        indicator.PropertyChanged += Indicator_PropertyChanged;

                        var dlg = new IndicatorConfigWindow(indicator);
                        dlg.ShowDialog();
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                    }
                    break;
                default:
                    throw new NotImplementedException("ChartViews_CollectionChanged: " + e.Action + " Not Yet Implement");
            }
        }

        private void Indicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var indicator = sender as IIndicator; 
            if (indicator != null)
            {
                indicator.Initialize(this.viewModel.StockSerie);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SeriesType":
                    this.OnSeriesTypeChanged();
                    break;
                case "BarDuration":
                    this.OnBarDurationChanged();
                    break;
            }
        }

        private void OnBarDurationChanged()
        {
        }


        private void ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var closestDataPoint = e.Context.ClosestDataPoint;
            if (closestDataPoint?.DataPoint?.DataItem is StockBar)
            {
                StockBar data = closestDataPoint.DataPoint.DataItem as StockBar;
                this.volume.Text = data.Volume.ToString("##,#");
                this.date.Text = data.Date.ToString("MMM dd, yyyy");
                this.price.Text = data.Close.ToString("0,0.00");
            }
        }

        #region Series Type 
        private void OnSeriesTypeChanged()
        {
            CategoricalSeriesBase series = null;
            switch (this.viewModel.SeriesType)
            {
                case SeriesType.Candle:
                    series = new CandlestickSeries();
                    break;
                case SeriesType.BarChart:
                    series = new OhlcSeries();
                    break;
                case SeriesType.Line:
                    series = new LineSeries();
                    break;
                default:
                    return;
            }
            this.Chart.Series.Clear();
            SetBindings(series);
            SetSourceBinding(series);
            SetTrackBallInfoTemplate(series);
            this.Chart.Series.Add(series);
        }
        private static void SetTrackBallInfoTemplate(CategoricalSeriesBase series)
        {
            //ResourceDictionary exampleResources = new ResourceDictionary();
            //exampleResources.Source = new Uri("/ChartView;component/Financial/Resources.xaml", UriKind.RelativeOrAbsolute);
            //series.TrackBallInfoTemplate = exampleResources["trackBallInfoTemplate"] as DataTemplate;
        }

        private static void SetBindings(CategoricalSeriesBase series)
        {
            series.CategoryBinding = new PropertyNameDataPointBinding();
            (series.CategoryBinding as PropertyNameDataPointBinding).PropertyName = "Date";
            if (series is OhlcSeriesBase)
            {
                var ohlcSeries = (OhlcSeriesBase)series;
                ohlcSeries.OpenBinding = new PropertyNameDataPointBinding("Open");
                ohlcSeries.HighBinding = new PropertyNameDataPointBinding("High");
                ohlcSeries.LowBinding = new PropertyNameDataPointBinding("Low");
                ohlcSeries.CloseBinding = new PropertyNameDataPointBinding("Close");
            }
            else if (series is RangeSeriesBase)
            {
                var rangeSeries = (RangeSeriesBase)series;
                rangeSeries.HighBinding = new PropertyNameDataPointBinding("High");
                rangeSeries.LowBinding = new PropertyNameDataPointBinding("Low");
            }
            else if (series is CategoricalStrokedSeries)
            {
                var strokedSeries = (CategoricalStrokedSeries)series;
                strokedSeries.ValueBinding = new PropertyNameDataPointBinding("Close");
            }
        }

        private static void SetSourceBinding(ChartSeries series)
        {
            Binding sourceBinding = new Binding("Data");
            sourceBinding.Mode = BindingMode.OneWay;
            series.SetBinding(ChartSeries.ItemsSourceProperty, sourceBinding);
        }
        #endregion
    }
}
