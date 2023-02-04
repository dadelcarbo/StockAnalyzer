using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using UltimateChartist.DataModels;
using UltimateChartist.DataModels.DataProviders;

namespace UltimateChartist.ChartControls
{
    public class ChartViewModel : ViewModelBase
    {
        static int count = 1;
        public ChartViewModel()
        {
            this.name = "Chart" + count++;

            this.Zoom = new Size(3, 1);
            this.PanOffset = new Point();

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
                    RaisePropertyChanged();
                }
            }
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

        private Point panOffset;
        public Point PanOffset { get => panOffset; set { if (panOffset != value) { panOffset = value; RaisePropertyChanged(); } } }

        private Size zoom;
        public Size Zoom { get => zoom; set { if (zoom != value) { zoom = value; RaisePropertyChanged(); } } }

        private SeriesType seriesType;
        public SeriesType SeriesType { get => seriesType; set { if (seriesType != value) { seriesType = value; RaisePropertyChanged(); } } }

        public DataTemplate AxisLabelTemplate => App.AppInstance.FindResource($"axis{BarDuration}LabelTemplate") as DataTemplate;
        #endregion
    }
}
