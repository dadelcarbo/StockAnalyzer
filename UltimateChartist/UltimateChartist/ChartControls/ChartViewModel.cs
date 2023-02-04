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

        public ObservableCollection<IndicatorViewModel> Indicators { get; } = new ObservableCollection<IndicatorViewModel>();


        static int indicatorCount = 1;
        public void AddIndicator()
        {
            this.Indicators.Add(new IndicatorViewModel(this) { Name = "Indicator" + indicatorCount++ });
        }

        public void RemoveIndicator(IndicatorViewModel indicatorViewModel)
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

                    var closeArray = this.Data.Select(b => b.Close).ToArray();
                    var fastMA = closeArray.CalculateEMA(10);
                    var slowMA = closeArray.CalculateEMA(20);

                    this.BullRange = new List<StockRange>();
                    this.BearRange = new List<StockRange>();
                    for (int i = 0; i < closeArray.Length; i++)
                    {
                        if (fastMA[i] > slowMA[i])
                        {
                            this.BullRange.Add(new StockRange(Data.ElementAt(i).Date, fastMA[i], slowMA[i]));
                            this.BearRange.Add(new StockRange(Data.ElementAt(i).Date, double.NaN, double.NaN));
                        }
                        else
                        {
                            this.BullRange.Add(new StockRange(Data.ElementAt(i).Date, double.NaN, double.NaN));
                            this.BearRange.Add(new StockRange(Data.ElementAt(i).Date, slowMA[i], fastMA[i]));
                        }
                    }
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
        private List<StockBar> data;
        public List<StockBar> Data { get => data; set { if (data != value) { data = value; RaisePropertyChanged(); } } }

        private List<StockRange> bearRange;
        public List<StockRange> BearRange { get => bearRange; set { if (bearRange != value) { bearRange = value; RaisePropertyChanged(); } } }

        private List<StockRange> bullRange;
        public List<StockRange> BullRange { get => bullRange; set { if (bullRange != value) { bullRange = value; RaisePropertyChanged(); } } }

        private double horizontalZoomRangeStart;
        public double HorizontalZoomRangeStart { get => horizontalZoomRangeStart; set { if (horizontalZoomRangeStart != value) { horizontalZoomRangeStart = value; RaisePropertyChanged(); } } }

        private double horizontalZoomRangeEnd;
        public double HorizontalZoomRangeEnd { get => horizontalZoomRangeEnd; set { if (horizontalZoomRangeEnd != value) { horizontalZoomRangeEnd = value; RaisePropertyChanged(); } } }

        private SeriesType seriesType;
        public SeriesType SeriesType { get => seriesType; set { if (seriesType != value) { seriesType = value; RaisePropertyChanged(); } } }

        public DataTemplate AxisLabelTemplate => App.AppInstance.FindResource($"axis{BarDuration}LabelTemplate") as DataTemplate;
        #endregion
    }
}
