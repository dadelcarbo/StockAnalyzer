using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.SectorDlg
{
    public class SectorViewModel : NotifyPropertyChangedBase
    {
        public SectorViewModel()
        {
            this.BarDuration = BarDuration.Daily;
            this.Period = 100;
        }

        static public IList<BarDuration> BarDurations => StockBarDuration.BarDurations;
        private BarDuration barDuration;
        public BarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (barDuration != value)
                {
                    barDuration = value;
                    this.OnPropertyChanged("BarDuration");
                }
            }
        }

        private int period;
        public int Period
        {
            get { return period; }
            set
            {
                if (period != value)
                {
                    period = value;
                    this.OnPropertyChanged("Period");
                    this.Perform();
                }
            }
        }

        public void Perform()
        {
            try
            {
                var sectorSeries = StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.SECTORS_STOXX));
                var sectorDataList = new List<SectorData>();

                // Calculate conversion ratio
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (var sectorSerie in sectorSeries)
                {
                    var sectorData = new SectorData() { Name = sectorSerie.StockName, ShortName = sectorSerie.StockName.Replace("STOXX EUROPE 600 ", "") };

                    sectorSerie.BarDuration = this.BarDuration;
                    var closeSerie = sectorSerie.GetSerie(StockDataType.CLOSE);
                    float ratio = closeSerie[closeSerie.LastIndex - period] / 100f;
                    SectorPoint[] points = new SectorPoint[period];
                    points[0] = new SectorPoint { X = 0, Y = 100f };
                    for (int i = 1; i < period; i++)
                    {
                        var val = closeSerie[closeSerie.LastIndex - period + i] / ratio;
                        points[i] = new SectorPoint { X = i, Y = val };
                        if (val < min) min = val;
                        if (val > max) max = val;
                    }
                    sectorData.Points = points;
                    sectorDataList.Add(sectorData);
                }

                this.SectorData = sectorDataList.OrderByDescending(d => d.LastValue).ToArray();

                this.MinVal = ((int)min / 10) * 10f;
                this.MaxVal = (1 + ((int)max / 10)) * 10f;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        SectorData[] sectorData;
        public SectorData[] SectorData { get => sectorData; set => SetProperty(ref sectorData, value); }

        private float minVal;
        public float MinVal
        {
            get { return minVal; }
            set
            {
                if (minVal != value)
                {
                    minVal = value;
                    this.OnPropertyChanged("MinVal");
                }
            }
        }
        private float maxVal;
        public float MaxVal
        {
            get { return maxVal; }
            set
            {
                if (maxVal != value)
                {
                    maxVal = value;
                    this.OnPropertyChanged("MaxVal");
                }
            }
        }

        private CommandBase performCommand;
        public ICommand PerformCommand => performCommand ??= new CommandBase(Perform);

    }

    public class SectorData
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public float LastValue => Points == null || Points.Length == 0 ? 0 : Points.Last().Y;
        public SectorPoint[] Points { get; set; }
    }
    public class SectorPoint
    {
        public int X { get; set; }
        public float Y { get; set; }
    }
}