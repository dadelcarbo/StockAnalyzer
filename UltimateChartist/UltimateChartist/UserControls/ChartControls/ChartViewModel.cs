using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators;

namespace UltimateChartist.UserControls.ChartControls;

public class ChartViewModel : ViewModelBase
{
    const double ZOOM_MARGIN = 0.025; // %
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
                if (!instrument.SupportedBarDurations.Contains(barDuration))
                {
                    this.BarDuration = instrument.DataProvider.DefaultBarDuration;
                }
                StockSerie = instrument.GetStockSerie(barDuration);
                Data = StockSerie.Bars;

                RaisePropertyChanged();
            }
        }
    }

    private int nbBar = 100;
    private void ResetZoom()
    {
        horizontalZoomRangeEnd = 1;
        HorizontalZoomRangeStart = Math.Max(0, 1 - (double)nbBar / Data.Count);
    }

    private Size maxZoom = new Size(100, 100);

    public Size MaxZoom
    {
        get { return maxZoom; }
        set { if (value != maxZoom) { maxZoom = value; RaisePropertyChanged(); } }
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
                Data = StockSerie?.Bars;

                OnPropertyChanged(nameof(AxisLabelTemplate));
                RaisePropertyChanged();
            }
        }
    }

    #region Chart Data

    public StockSerie StockSerie { get; set; }

    private List<StockBar> data;
    public List<StockBar> Data
    {
        get => data;
        set
        {
            if (data != value)
            {
                data = value;
                if (data == null)
                {
                    RaisePropertyChanged();
                    return;
                }
                //MaxZoom = new Size(Math.Max(1, data.Count / nbBar), 100);

                var max = 0m;
                if (data != null && data.Count > 0)
                {
                    max = data.Max(d => d.High);

                    Name = instrument.Name;
                    foreach (var indicator in PriceIndicators)
                    {
                        indicator.Initialize(StockSerie);
                        max = Math.Max(max, indicator.Max);
                    }
                }
                this.Maximum = (1 + ZOOM_MARGIN) * (double)max;

                ResetZoom();

                RaisePropertyChanged();
            }
        }
    }

    private double maximum;
    public double Maximum { get { return maximum; } set { if (maximum != value) { maximum = value; RaisePropertyChanged(); } } }

    private double horizontalZoomRangeStart;
    public double HorizontalZoomRangeStart
    {
        get => horizontalZoomRangeStart;
        set
        {
            if (horizontalZoomRangeStart != value)
            {
                horizontalZoomRangeStart = value;
                CalculateVerticalZoom();
                RaisePropertyChanged();
            }
        }
    }

    private double horizontalZoomRangeEnd;
    public double HorizontalZoomRangeEnd
    {
        get => horizontalZoomRangeEnd;
        set
        {
            if (horizontalZoomRangeEnd != value)
            {
                horizontalZoomRangeEnd = value;
                CalculateVerticalZoom();
                RaisePropertyChanged();
            }
        }
    }

    private void CalculateVerticalZoom()
    {
        int startIndex = (int)Math.Floor(horizontalZoomRangeStart * Data.Count);
        int endIndex = (int)Math.Ceiling(horizontalZoomRangeEnd * Data.Count) - 1;
        if (startIndex == endIndex)
            return;
        var visibleData = Data.Skip(startIndex).Take(endIndex - startIndex).ToList();
        var min = (double)visibleData.Min(f => f.Low);
        var max = (double)visibleData.Max(f => f.High);
        var margin = ZOOM_MARGIN * (max - min);
        this.VerticalZoomRangeStart = (min - margin) / Maximum;
        this.VerticalZoomRangeEnd = (max + margin) / Maximum;
    }

    private double verticalZoomRangeStart;
    public double VerticalZoomRangeStart { get => verticalZoomRangeStart; set { if (verticalZoomRangeStart != value) { verticalZoomRangeStart = value; RaisePropertyChanged(); } } }

    private double verticalZoomRangeEnd;
    public double VerticalZoomRangeEnd { get => verticalZoomRangeEnd; set { if (verticalZoomRangeEnd != value) { verticalZoomRangeEnd = value; RaisePropertyChanged(); } } }

    private SeriesType seriesType;
    public SeriesType SeriesType { get => seriesType; set { if (seriesType != value) { seriesType = value; RaisePropertyChanged(); } } }

    public DataTemplate AxisLabelTemplate => BarDuration switch
    {
        BarDuration.Daily => App.AppInstance.FindResource($"axisDailyLabelTemplate") as DataTemplate,
        BarDuration.Weekly => App.AppInstance.FindResource($"axisDailyLabelTemplate") as DataTemplate,
        BarDuration.Monthly => App.AppInstance.FindResource($"axisDailyLabelTemplate") as DataTemplate,
        _ => App.AppInstance.FindResource($"axisIntradayLabelTemplate") as DataTemplate
    };

    public ObservableCollection<IIndicator> PriceIndicators { get; set; } = new ObservableCollection<IIndicator>();
    #endregion
}
