using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators;

namespace UltimateChartist.UserControls.ChartControls;

public class ChartViewModel : ViewModelBase
{
    static int count = 1;
    public ChartViewModel()
    {
        name = "Chart" + count++;
        Instrument = MainWindowViewModel.Instance.Instruments.FirstOrDefault();
    }
    private string name;
    public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }

    public ObservableCollection<IndicatorChartViewModel> Indicators { get; } = new ObservableCollection<IndicatorChartViewModel>();

    public void AddIndicator()
    {
        var indicatorChartViewModel = new IndicatorChartViewModel(this, new StockIndicator_STOCK());
        Indicators.Add(indicatorChartViewModel);
    }

    public void RemoveIndicator(IndicatorChartViewModel indicatorViewModel)
    {
        Indicators.Remove(indicatorViewModel);
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
                StockSerie = instrument.GetStockSerie(barDuration);
                Data = StockSerie.Bars;

                Name = instrument.Name;
                foreach (var indicator in PriceIndicators)
                {
                    indicator.Initialize(StockSerie);
                }

                ResetZoom();
                RaisePropertyChanged();
            }
        }
    }

    private int nbBar = 100;
    private void ResetZoom()
    {
        HorizontalZoomRangeEnd = 1;
        HorizontalZoomRangeStart = 1 - (double)nbBar / Data.Count;
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
                StockSerie = instrument.GetStockSerie(barDuration);
                Data = StockSerie.Bars;

                OnPropertyChanged(nameof(AxisLabelTemplate));
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
