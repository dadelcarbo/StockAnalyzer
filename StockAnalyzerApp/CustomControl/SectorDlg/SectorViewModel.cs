using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.SectorDlg
{
    public class SectorViewModel : NotifyPropertyChangedBase
    {
        public SectorViewModel()
        {
            this.BarDuration = StockBarDuration.Daily;
            this.Sector = ABCDataProvider.SectorCodes.First();
            this.Period = 100;
        }

        SectorCode sector;
        public SectorCode Sector
        {
            get { return sector; }
            set
            {
                if (sector != value)
                {
                    sector = value;
                    this.OnPropertyChanged("Sector");
                    this.OnPropertyChanged("SectorComponents");
                }
            }
        }

        static public IList<StockBarDuration> BarDurations
        {
            get { return StockBarDuration.Values; }
        }
        private StockBarDuration barDuration;
        public StockBarDuration BarDuration
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

        public IEnumerable<StockSerie> SectorComponents => StockDictionary.Instance.Where(s => s.Value.SectorId == this.Sector.Code).Select(s => s.Value);

        public void Perform()
        {
            try
            {
                var sectorSeries = ABCDataProvider.SectorCodes.Select(sc => StockDictionary.Instance.First(s => s.Key == "_" + sc.Sector).Value).ToList();
                var sectorValues = new List<GraphValue[]>();
                // Calculate conversion ratio
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (var sectorSerie in sectorSeries)
                {
                    sectorSerie.BarDuration = this.BarDuration;
                    var closeSerie = sectorSerie.GetSerie(StockDataType.CLOSE);
                    float ratio = closeSerie[closeSerie.LastIndex - period] / 100f;
                    GraphValue[] values = new GraphValue[period];
                    values[0] = new GraphValue { X = 0, Y = 100f };
                    for (int i = 1; i < period; i++)
                    {
                        var val = closeSerie[closeSerie.LastIndex - period + i] / ratio;
                        values[i] = new GraphValue { X = i, Y = val };
                        if (val < min) min = val;
                        if (val > max) max = val;
                    }
                    sectorValues.Add(values);
                }
                this.SectorValues = sectorValues.OrderByDescending(v => v.Last().Y).ToList();
                this.MinVal = ((int)min / 10) * 10f;
                this.MaxVal = (1 + ((int)max / 10)) * 10f;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        List<GraphValue[]> sectorValues;
        public List<GraphValue[]> SectorValues
        {
            get { return sectorValues; }
            set
            {
                if (sectorValues != value)
                {
                    sectorValues = value;
                    this.OnPropertyChanged("SectorValues");
                }
            }
        }
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
    }

    public class GraphValue
    {
        public int X { get; set; }
        public float Y { get; set; }
    }
}