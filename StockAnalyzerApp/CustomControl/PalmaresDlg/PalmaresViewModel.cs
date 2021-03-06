﻿using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups
        {
            get { return Enum.GetValues(typeof(StockSerie.Groups)); }
        }

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                    this.Lines?.Clear();
                    OnPropertyChanged("Lines");
                    OnPropertyChanged("ExportEnabled");
                }
            }
        }

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

        private string indicator1;
        public string Indicator1
        {
            get { return indicator1; }
            set
            {
                if (value != indicator1)
                {
                    indicator1 = value;
                    OnPropertyChanged("Indicator1");
                }
            }
        }
        private string indicator2;
        public string Indicator2
        {
            get { return indicator2; }
            set
            {
                if (value != indicator2)
                {
                    indicator2 = value;
                    OnPropertyChanged("Indicator2");
                }
            }
        }
        private string indicator3;
        public string Indicator3
        {
            get { return indicator3; }
            set
            {
                if (value != indicator3)
                {
                    indicator3 = value;
                    OnPropertyChanged("Indicator3");
                }
            }
        }
        private string stop;
        public string Stop
        {
            get { return stop; }
            set
            {
                if (value != stop)
                {
                    stop = value;
                    OnPropertyChanged("Stop");
                }
            }
        }

        private DateTime fromDate;
        public DateTime FromDate
        {
            get { return fromDate; }
            set
            {
                if (value != fromDate)
                {
                    fromDate = value;
                    OnPropertyChanged("FromDate");
                }
            }
        }

        private DateTime toDate;
        public DateTime ToDate
        {
            get { return toDate; }
            set
            {
                if (value != toDate)
                {
                    toDate = value;
                    OnPropertyChanged("ToDate");
                }
            }
        }

        public bool ExportEnabled => this.Lines != null && this.Lines.Count > 0;

        public List<PalmaresLine> Lines { get; set; }

        public PalmaresViewModel()
        {
            this.Indicator1 = "MANSFIELD(100,CAC40)";
            this.Indicator2 = "HIGHEST(20)";
            this.Indicator3 = "STOKFBODY(20)";
            this.Stop = "TRAILHIGHEST(70,12)";
            this.Group = StockSerie.Groups.COUNTRY;
            this.Lines = new List<PalmaresLine>();
            this.ToDate = DateTime.Now;
            this.FromDate = new DateTime(this.ToDate.Year, 1, 1);
        }
        public bool Calculate()
        {
            #region Sanity Check
            IStockIndicator viewableSeries1 = null;
            if (!string.IsNullOrEmpty(this.indicator1))
            {
                try
                {
                    viewableSeries1 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator1) as IStockIndicator;
                }
                catch { }
            }
            IStockIndicator viewableSeries2 = null;
            if (!string.IsNullOrEmpty(this.indicator2))
            {
                try
                {
                    viewableSeries2 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator2) as IStockIndicator;
                }
                catch { }
            }
            IStockIndicator viewableSeries3 = null;
            if (!string.IsNullOrEmpty(this.indicator3))
            {
                try
                {
                    viewableSeries3 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator3) as IStockIndicator;
                }
                catch { }
            }
            IStockTrailStop trailStopSerie = null;
            if (!string.IsNullOrEmpty(this.stop))
            {
                try
                {
                    trailStopSerie = StockViewableItemsManager.GetViewableItem("TRAILSTOP|" + this.stop) as IStockTrailStop;
                }
                catch { }
            }
            #endregion

            Lines = new List<PalmaresLine>();
            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (!stockSerie.Initialise())
                    continue;
                var previousDuration = stockSerie.BarDuration;
                stockSerie.BarDuration = this.barDuration;

                #region Calculate variation
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                var startIndex = stockSerie.IndexOfFirstGreaterOrEquals(this.FromDate);
                if (startIndex == -1)
                {
                    continue;
                }
                var endIndex = stockSerie.IndexOfFirstLowerOrEquals(this.ToDate);
                if (endIndex == -1)
                {
                    continue;
                }

                float lastValue = closeSerie[endIndex];
                float firstValue = closeSerie[startIndex];
                float variation = (lastValue - firstValue) / firstValue;

                #endregion
                #region Calculate Indicators
                float stockIndicator1 = float.NaN;
                if (viewableSeries1 != null)
                {
                    try { viewableSeries1.ApplyTo(stockSerie); stockIndicator1 = viewableSeries1.Series[0][endIndex]; } catch { }
                }
                float stockIndicator2 = float.NaN;
                if (viewableSeries2 != null)
                {
                    try { viewableSeries2.ApplyTo(stockSerie); stockIndicator2 = viewableSeries2.Series[0][endIndex]; } catch { }
                }
                float stockIndicator3 = float.NaN;
                if (viewableSeries3 != null)
                {
                    try { viewableSeries3.ApplyTo(stockSerie); stockIndicator3 = viewableSeries3.Series[0][endIndex]; } catch { }
                }
                float stopValue = float.NaN;
                if (trailStopSerie != null)
                {
                    try
                    {
                        trailStopSerie.ApplyTo(stockSerie);
                        stopValue = trailStopSerie.Series[0][endIndex];
                        if (float.IsNaN(stopValue))
                            stopValue = trailStopSerie.Series[1][endIndex];
                        stopValue = (lastValue - stopValue) / lastValue;
                    }
                    catch { }
                }
                #endregion

                Lines.Add(new PalmaresLine
                {
                    Sector = stockSerie.SectorId == 0 ? null : ABCDataProvider.SectorCodes.FirstOrDefault(s => s.Code == stockSerie.SectorId).Sector,
                    Group = stockSerie.StockGroup.ToString(),
                    ShortName = stockSerie.ShortName,
                    //ShortName = "=HYPERLINK(\"https://www.abcbourse.com/graphes/eod/" + stockSerie.ShortName + "p\";\"" + stockSerie.StockName + "\")",
                    Name = stockSerie.StockName,
                    Value = lastValue,
                    Indicator1 = stockIndicator1,
                    Indicator2 = stockIndicator2,
                    Indicator3 = stockIndicator3,
                    Stop = stopValue,
                    Variation = variation
                    // Link = stockSerie.DataProvider == StockAnalyzer.StockClasses.StockDataProviders.StockDataProvider.ABC ? $"https://www.abcbourse.com/graphes/eod/{stockSerie.ShortName}p" : null
                });

                stockSerie.BarDuration = previousDuration;
            }

            OnPropertyChanged("Lines");
            OnPropertyChanged("ExportEnabled");
            return true;
        }
    }
}
