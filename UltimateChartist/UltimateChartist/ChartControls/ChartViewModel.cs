using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using UltimateChartist.DataModels;
using UltimateChartist.DataModels.DataProviders;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls
{
    public class ChartViewModel : ViewModelBase
    {
        static int count = 1;
        public ChartViewModel()
        {
            this.name = "Chart" + count++;
            this.Instrument = MainWindowViewModel.Instance.Instruments.FirstOrDefault();
        }
        private string name;
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }

        public ObservableCollection<IndicatorChartViewModel> Indicators { get; } = new ObservableCollection<IndicatorChartViewModel>();

        public void AddIndicator()
        {
            this.Indicators.Add(new IndicatorChartViewModel(this, new StockIndicator_EMACD()));
        }

        public void RemoveIndicator(IndicatorChartViewModel indicatorViewModel)
        {
            this.Indicators.Remove(indicatorViewModel);
        }

        private Instrument instrument;
        public Instrument Instrument
        {
            get => instrument;
            set
            {
                if (value != null && instrument != value)
                {
                    instrument = value;
                    this.Name = instrument.Name;
                    this.Data = DataProviderHelper.LoadData(instrument, BarDuration.Daily);
                    this.StockSerie = new StockSerie (value, BarDuration.Daily, this.Data);

                    ResetZoom();
                    RaisePropertyChanged();
                }
            }
        }

        private int nbBar = 100;
        private void ResetZoom()
        {
            this.HorizontalZoomRangeEnd = 1;
            this.HorizontalZoomRangeStart = 1 - ((double)nbBar / Data.Count);
        }

        private BarDuration barDuration = BarDuration.Daily;

        public BarDuration BarDuration
        {
            get => barDuration;
            set
            {
                if (barDuration != value)
                {
                    barDuration = value;
                    this.OnPropertyChanged(nameof(AxisLabelTemplate));
                    RaisePropertyChanged();
                }
            }
        }

        #region Chart Data

        public StockSerie StockSerie { get; set; }

        private List<StockBar> data;
        public List<StockBar> Data { get => data; set { if (data != value) { data = value; RaisePropertyChanged(); } } }

        private double horizontalZoomRangeStart;
        public double HorizontalZoomRangeStart { get => horizontalZoomRangeStart; set { if (horizontalZoomRangeStart != value) { horizontalZoomRangeStart = value; RaisePropertyChanged(); } } }

        private double horizontalZoomRangeEnd;
        public double HorizontalZoomRangeEnd { get => horizontalZoomRangeEnd; set { if (horizontalZoomRangeEnd != value) { horizontalZoomRangeEnd = value; RaisePropertyChanged(); } } }

        private SeriesType seriesType;
        public SeriesType SeriesType { get => seriesType; set { if (seriesType != value) { seriesType = value; RaisePropertyChanged(); } } }

        public DataTemplate AxisLabelTemplate => App.AppInstance.FindResource($"axis{BarDuration}LabelTemplate") as DataTemplate;

        public ObservableCollection<IIndicator> PriceIndicators { get; set; } = new ObservableCollection<IIndicator>();
        #endregion
    }
}
