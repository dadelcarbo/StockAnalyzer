using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockData;
using StockAnalyzer.StockLogging;
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
            this.group = StockAnalyzer.StockData.Groups.SECTORS_STOXX;
        }

        BarDuration barDuration;
        public BarDuration BarDuration { get => barDuration; set => SetProperty(ref barDuration, value); }

        public List<BarDuration> BarDurations => new List<BarDuration>() { BarDuration.Daily, BarDuration.Weekly };

        Groups group;
        public Groups Group { get => group; set => SetProperty(ref group, value); }

        public List<Groups> Groups => StockDictionary.GetValidGroups();

        public void Perform(string param)
        {
            try
            {
                var sectorSeries = StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(Group)).Take(20);
                var refSerie = sectorSeries.FirstOrDefault();
                #region Sanity checks
                if (refSerie == null)
                {
                    MessageBox.Show("No sector series found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var refDataSerie = refSerie.GetDefaultDataSerie();
                if (refDataSerie == null || refDataSerie.Count == 0)
                {
                    MessageBox.Show($"Error initialising {refSerie.DisplayName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                #endregion

                DateTime startDate = DateTime.MinValue;
                DateTime endDate = refDataSerie.LastValue.DATE;
                switch (param)
                {
                    case "1W": startDate = endDate.AddDays(-7); break;
                    case "MTD": startDate = new DateTime(endDate.Year, endDate.Month, 1); break;
                    case "1M": startDate = endDate.AddMonths(-1); break;
                    case "2M": startDate = endDate.AddMonths(-2); break;
                    case "3M": startDate = endDate.AddMonths(-3); break;
                    case "6M": startDate = endDate.AddMonths(-6); break;
                    case "YTD": startDate = new DateTime(endDate.Year, 1, 1); break;
                    case "1Y": startDate = endDate.AddYears(-1); break;
                    case "2Y": startDate = endDate.AddYears(-2); break;
                    case "Max": startDate = refDataSerie.Values[0].DATE; break;
                    default:
                        MessageBox.Show($"Invalid period parameter {param}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                var sectorDataList = new List<SectorData>();

                // Calculate conversion ratio
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (var sectorSerie in sectorSeries)
                {
                    var sectorDataSerie = sectorSerie.GetDataSerie(this.BarDuration);
                    if (sectorDataSerie == null || sectorDataSerie.Count == 0)
                    {
                        StockLog.Write($"{sectorSerie.DisplayName}: Initialisation failed !");
                        continue;
                    }

                    var sectorData = new SectorData() { Name = sectorSerie.DisplayName, ShortName = sectorSerie.DisplayName.Replace("STOXX EUROPE 600 ", "") };

                    var startIndex = sectorDataSerie.IndexOfFirstGreaterOrEquals(startDate);
                    if (startIndex < 0)
                        continue;
                    var closeSerie = sectorDataSerie.GetSerie(StockDataType.CLOSE);
                    float ratio = closeSerie[startIndex] / 100f;
                    SectorPoint[] points = new SectorPoint[sectorDataSerie.Count - startIndex];
                    points[0] = new SectorPoint { X = 0, Y = 100f };
                    for (int i = startIndex; i <= sectorDataSerie.LastIndex; i++)
                    {
                        var val = closeSerie[i] / ratio;
                        points[i - startIndex] = new SectorPoint { X = i - startIndex, Y = val, Date = sectorDataSerie.Values[i].DATE };
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

        private ParamCommandBase<string> performCommand;
        public ICommand PerformCommand => performCommand ??= new ParamCommandBase<string>(Perform);

    }

    public class SectorData
    {
        public string Name { get; set; }
        public string ShortName { get; set; }

        public string LegendName => $"{LastValue:F2} - {ShortName}";

        public float LastValue => Points == null || Points.Length == 0 ? 0 : Points.Last().Y;
        public SectorPoint[] Points { get; set; }
    }

    public class SectorPoint
    {
        public DateTime Date { get; set; }
        public int X { get; set; }
        public float Y { get; set; }
    }
}