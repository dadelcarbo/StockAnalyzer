using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public partial class StockSerie : SortedDictionary<DateTime, StockDailyValue>, IXmlSerializable
    {
        #region Type Definition
        public enum Groups
        {
            NONE = 0,
            COUNTRY,
            CAC40,
            CACALL,
            SRD,
            SRD_LO,
            EURO_A,
            EURO_A_B,
            EURO_B,
            EURO_A_B_C,
            EURO_C,
            ALTERNEXT,
            EURONEXT,
            DAX30,
            FTSE100,
            SP500,
            INDICES,
            INDICES_CALC,
            INDICATOR,
            SECTORS,
            SECTORS_CAC,
            CURRENCY,
            COMMODITY,
            FOREX,
            FUND,
            RATIO,
            BREADTH,
            BOND,
            TICK,
            RANGE,
            INTRADAY,
            USER1,
            USER2,
            USER3,
            ShortInterest,
            Portfolio,
            Replay,
            ALL
        }
        public enum Trend
        {
            NoTrend,
            UpTrend,
            DownTrend
        };


        public static SortedDictionary<StockBarDuration, StockBarDuration> ExactDataDurationMapping;


        #endregion
        #region public properties
        public string StockName { get; private set; }
        public string ShortName { get; private set; }
        public string Exchange { get; set; }
        public StockDataProvider DataProvider { get; private set; }
        public string ISIN { get; set; }
        /// <summary>
        /// Investing.com ticker used for download
        /// </summary>
        public long Ticker { get; set; }
        public string SectorID { get; set; }
        public Groups StockGroup { get; private set; }
        public StockAnalysis StockAnalysis { get; set; }

        #region StockFinancial
        private StockFinancial financial;
        public StockFinancial Financial
        {
            get
            {
                if (financial == null)
                {
                    financial = LoadFinancial();
                }
                return financial;
            }
            set { financial = value; }
        }

        private static string FINANCIAL_SUBFOLDER = @"\data\financial";
        private StockFinancial LoadFinancial()
        {
            StockFinancial stockFinancial = null;
            if (this.BelongsToGroup(Groups.CACALL))
            {
                string path = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + FINANCIAL_SUBFOLDER;
                string fileName = path + @"\" + this.ShortName + "_" + this.StockGroup + ".xml";
                if (File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                        settings.IgnoreWhitespace = true;
                        System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                        XmlSerializer serializer = new XmlSerializer(typeof(StockFinancial));
                        stockFinancial = (StockFinancial)serializer.Deserialize(xmlReader);
                    }
                }
            }
            return stockFinancial;
        }
        public void SaveFinancial()
        {
            if (this.Financial == null) return;
            string path = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + FINANCIAL_SUBFOLDER;
            string fileName = path + @"\" + this.ShortName + "_" + this.StockGroup + ".xml";
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                settings.Indent = true;
                System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(StockFinancial));
                serializer.Serialize(xmlWriter, this.Financial);
            }
        }

        #endregion

        #region StockAgenda
        private StockAgenda agenda;
        public StockAgenda Agenda
        {
            get
            {
                if (agenda == null)
                {
                    agenda = LoadAgenda();
                }
                return agenda;
            }
            set { agenda = value; }
        }

        private StockDividend dividend;

        public StockDividend Dividend => dividend ?? (dividend = new StockDividend(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, this));

        private static string AGENDA_SUBFOLDER = @"\data\agenda";
        private StockAgenda LoadAgenda()
        {
            StockAgenda stockAgenda = null;
            if (this.BelongsToGroup(Groups.CACALL))
            {
                string path = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + AGENDA_SUBFOLDER;
                string fileName = path + @"\" + this.ShortName + "_" + this.StockGroup + ".xml";
                if (File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                        settings.IgnoreWhitespace = true;
                        System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                        XmlSerializer serializer = new XmlSerializer(typeof(StockAgenda));
                        stockAgenda = (StockAgenda)serializer.Deserialize(xmlReader);
                    }
                }
            }
            return stockAgenda;
        }
        public void SaveAgenda()
        {
            if (this.Agenda == null) return;
            string path = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + AGENDA_SUBFOLDER;
            string fileName = path + @"\" + this.ShortName + "_" + this.StockGroup + ".xml";
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                settings.Indent = true;
                System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(StockAgenda));
                serializer.Serialize(xmlWriter, this.Agenda);
            }
        }

        #endregion


        public bool IsPortofolioSerie { get; set; }
        public int LastIndex { get { return this.ValueArray.Length - 1; } }
        public int LastCompleteIndex { get { return this.ValueArray.Last().IsComplete ? this.Values.Count - 1 : this.Values.Count - 2; } }

        public StockSerie SecondarySerie { get; set; }
        public bool HasVolume { get; private set; }
        #endregion

        #region DATA, EVENTS AND INDICATORS SERIES MANAGEMENT
        [XmlIgnore]
        public SortedDictionary<string, List<StockDailyValue>> BarSmoothedDictionary { get; private set; }

        StockBarDuration barDuration = StockBarDuration.Daily;
        public StockBarDuration BarDuration { get { return barDuration; } set { this.SetBarDuration(value); } }

        [XmlIgnore]
        public FloatSerie[] ValueSeries { get; set; }
        [XmlIgnore]
        protected Dictionary<string, FloatSerie> FloatSerieCache { get; set; }
        [XmlIgnore]
        public Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
        [XmlIgnore]
        protected Dictionary<string, IStockCloud> CloudCache { get; set; }
        [XmlIgnore]
        public IStockTrailStop TrailStopCache { get; set; }
        [XmlIgnore]
        public IStockPaintBar PaintBarCache { get; set; }
        [XmlIgnore]
        protected Dictionary<string, IStockDecorator> DecoratorCache { get; set; }
        [XmlIgnore]
        protected IStockTrail TrailCache { get; set; }

        protected bool isInitialised = false;
        [XmlIgnore]
        public bool IsInitialised
        {
            get { return this.isInitialised; }
            set
            {
                if (isInitialised != value)
                {
                    if (!value)
                    {
                        this.Clear();
                        ResetAllCache();
                    }
                    this.isInitialised = value;
                }
            }
        }
        #region MANAGE TIMESPAN

        private StockDailyValue[] StockDailyValuesAsArray()
        {
            StockDailyValue[] values = new StockDailyValue[this.Count];
            this.Values.CopyTo(values, 0);
            return values;
        }

        private StockDailyValue[] valueArray = null;
        public StockDailyValue[] ValueArray
        {
            get
            {
                if (valueArray == null) 
                    valueArray = this.StockDailyValuesAsArray();
                return valueArray;
            }
        }
        public List<StockDailyValue> GetValues(StockBarDuration stockBarDuration)
        {
            if (this.BarSmoothedDictionary.ContainsKey(stockBarDuration.ToString()))
            {
                return this.BarSmoothedDictionary[stockBarDuration.ToString()];
            }
            else
            {
                return null;
            }
        }
        public List<StockDailyValue> GetSmoothedValues(StockBarDuration newBarDuration)
        {
            string barSmoothedDuration = newBarDuration.ToString();
            if (this.BarSmoothedDictionary.ContainsKey(barSmoothedDuration))
            {
                return this.BarSmoothedDictionary[barSmoothedDuration];
            }
            else
            {
                List<StockDailyValue> newList = this.GenerateSerieForTimeSpan(this.BarSmoothedDictionary[StockBarDuration.Daily.ToString()], newBarDuration);
                if (newBarDuration.Smoothing > 1)
                {
                    newList = this.GenerateSmoothedBars(newList, newBarDuration.Smoothing);
                }
                if (newBarDuration.LineBreak > 0)
                {
                    newList = this.GenerateNbLineBreakBar(newList, newBarDuration.LineBreak);
                }
                if (newBarDuration.HeikinAshi)
                {
                    newList = this.GenerateHeikinAshiBarFromDaily(newList);
                }
                this.BarSmoothedDictionary.Add(barSmoothedDuration, newList);

                return newList;
            }
        }

        public List<StockDailyValue> GetExactValues()
        {
            if (ExactDataDurationMapping == null)
            {
                ExactDataDurationMapping = new SortedDictionary<StockBarDuration, StockBarDuration>();
            }
            if (ExactDataDurationMapping.ContainsKey(this.barDuration))
            {
                return GetValues(ExactDataDurationMapping[this.barDuration]);
            }
            else
            {
                return GetValues(StockBarDuration.Daily);
            }
        }
        private void SetBarDuration(StockBarDuration newBarDuration)
        {
            if (!this.Initialise() || (newBarDuration == this.barDuration))
            {
                this.barDuration = newBarDuration;
                return;
            }
            this.Clear();
            this.ResetIndicatorCache();
            foreach (StockDailyValue dailyValue in this.GetSmoothedValues(newBarDuration))
            {
                this.Add(dailyValue.DATE, dailyValue);
            }
            this.barDuration = newBarDuration;
            this.PreInitialise();
            valueArray = StockDailyValuesAsArray();
            dateArray = null;
            return;
        }
        public void ClearBarDurationCache()
        {
            this.BarSmoothedDictionary.Clear();
        }
        #endregion

        public float GetValue(StockDataType dataType, int index)
        {
            return GetSerie(dataType).Values.ElementAt(index);
        }
        public FloatSerie GetSerie(StockDataType dataType)
        {
            if (ValueSeries[(int)dataType] == null)
            {
                switch (dataType)
                {
                    case StockDataType.CLOSE:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.CLOSE).ToArray(), "CLOSE");
                        break;
                    case StockDataType.OPEN:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.OPEN).ToArray(), "OPEN");
                        break;
                    case StockDataType.HIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.HIGH).ToArray(), "HIGH");
                        break;
                    case StockDataType.LOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.LOW).ToArray(), "LOW");
                        break;
                    case StockDataType.VARIATION:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VARIATION).ToArray(), "VARIATION");
                        break;
                    case StockDataType.VOLUME:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f).ToArray(), "VOLUME");
                        break;
                }
            }

            return ValueSeries[(int)dataType];
        }
        public FloatSerie GetSerie(String serieName)
        {
            if (this.FloatSerieCache.ContainsKey(serieName))
            {
                return this.FloatSerieCache[serieName];
            }
            return null;
        }
        public IStockTrailStop GetTrailStop(String trailStopName)
        {
            if (this.TrailStopCache != null && this.TrailStopCache.Name == trailStopName)
            {
                return this.TrailStopCache;
            }
            else
            {
                IStockTrailStop trailStop = StockTrailStopManager.CreateTrailStop(trailStopName);
                if (trailStop != null && (this.HasVolume || !trailStop.RequiresVolumeData))
                {
                    trailStop.ApplyTo(this);
                    this.TrailStopCache = trailStop;
                    return trailStop;
                }
                return null;
            }
        }
        public IStockIndicator GetIndicator(String indicatorName)
        {
            if (this.IndicatorCache.ContainsKey(indicatorName))
            {
                return this.IndicatorCache[indicatorName];
            }
            else
            {
                IStockIndicator indicator = StockIndicatorManager.CreateIndicator(indicatorName);
                if (indicator != null && (this.HasVolume || !indicator.RequiresVolumeData))
                {
                    indicator.ApplyTo(this);
                    AddIndicatorSerie(indicator);
                    return indicator;
                }
                return null;
            }
        }

        public IStockCloud GetCloud(String indicatorName)
        {
            if (this.CloudCache.ContainsKey(indicatorName))
            {
                return this.CloudCache[indicatorName];
            }
            else
            {
                IStockCloud indicator = StockCloudManager.CreateCloud(indicatorName);
                if (indicator != null && (this.HasVolume || !indicator.RequiresVolumeData))
                {
                    indicator.ApplyTo(this);
                    AddCloudSerie(indicator);
                    return indicator;
                }
                return null;
            }
        }
        public IStockPaintBar GetPaintBar(String paintBarName)
        {
            if (this.PaintBarCache != null && this.PaintBarCache.Name == paintBarName)
            {
                return this.PaintBarCache;
            }
            else
            {
                IStockPaintBar paintBar = StockPaintBarManager.CreatePaintBar(paintBarName);
                if (paintBar != null && (this.HasVolume || !paintBar.RequiresVolumeData))
                {
                    paintBar.ApplyTo(this);

                    this.PaintBarCache = paintBar;
                    return paintBar;
                }
                return null;
            }
        }
        public IStockDecorator GetDecorator(string decoratorName, string decoratedItem)
        {
            string fullDecoratorName = decoratorName + "|" + decoratedItem;
            if (this.DecoratorCache.ContainsKey(fullDecoratorName))
            {
                return this.DecoratorCache[fullDecoratorName];
            }
            else
            {
                IStockDecorator decorator = StockDecoratorManager.CreateDecorator(decoratorName, decoratedItem);
                if (decorator != null && (this.HasVolume || !decorator.RequiresVolumeData))
                {
                    decorator.ApplyTo(this);
                    this.DecoratorCache.Add(fullDecoratorName, decorator);
                    return decorator;
                }
                return null;
            }
        }
        public IStockTrail GetTrail(string trailName, string trailedItem)
        {
            if (this.TrailCache != null && this.TrailCache.Name == trailName && this.TrailCache.TrailedItem == trailedItem)
            {
                return this.TrailCache;
            }
            else
            {
                IStockTrail trail = StockTrailManager.CreateTrail(trailName, trailedItem);
                if (trail != null && (this.HasVolume || !trail.RequiresVolumeData))
                {
                    trail.ApplyTo(this);
                    this.TrailCache = trail;
                    return trail;
                }
                return null;
            }
        }

        public IStockEvent GetStockEvents(IStockViewableSeries stockViewableSerie)
        {
            if (stockViewableSerie is IStockIndicator)
            {
                return this.GetIndicator(stockViewableSerie.Name);
            }
            if (stockViewableSerie is IStockPaintBar)
            {
                return this.GetPaintBar(stockViewableSerie.Name);
            }
            if (stockViewableSerie is IStockTrail)
            {
                return this.GetTrail(stockViewableSerie.Name, (stockViewableSerie as IStockTrail).TrailedItem);
            }
            if (stockViewableSerie is IStockTrailStop)
            {
                return this.GetTrailStop(stockViewableSerie.Name);
            }
            if (stockViewableSerie is IStockDecorator)
            {
                return this.GetDecorator(stockViewableSerie.Name, (stockViewableSerie as IStockDecorator).DecoratedItem);
            }
            throw new StockAnalyzerException("Type not supported, cannot apply to serie the viewable item: " + stockViewableSerie.Name);
        }

        public IStockViewableSeries GetViewableItem(string name)
        {
            string[] nameFields = name.Split('|');
            switch (nameFields[0])
            {
                case "INDICATOR":
                    return this.GetIndicator(nameFields[1]);
                case "TRAILSTOP":
                    return this.GetTrailStop(nameFields[1]);
                case "TRAIL":
                    return this.GetTrail(nameFields[1], nameFields[2]);
                case "PAINTBAR":
                    return this.GetPaintBar(nameFields[1]);
                case "DECORATOR":
                    return this.GetDecorator(nameFields[1], nameFields[2]);
            }
            throw new ArgumentException("No viewable item matching " + name + " has been found");
        }

        public void AddIndicatorSerie(IStockIndicator indicator)
        {
            if (this.IndicatorCache.ContainsKey(indicator.Name))
            {
                this.IndicatorCache[indicator.Name] = indicator;
            }
            else
            {
                this.IndicatorCache.Add(indicator.Name, indicator);
            }
        }
        public void AddCloudSerie(IStockCloud indicator)
        {
            if (this.CloudCache.ContainsKey(indicator.Name))
            {
                this.CloudCache[indicator.Name] = indicator;
            }
            else
            {
                this.CloudCache.Add(indicator.Name, indicator);
            }
        }
        #endregion
        #region Private members
        private System.DateTime lastDate;
        #endregion
        #region Calculation constants

        // RSI Relative Strength index
        static private int RSITimePeriod = 14;
        static private float alphaEMA_RSI = 2.0f / (float)(RSITimePeriod + 1);

        public static System.TimeSpan[] DateRangeDate = {
                                           System.TimeSpan.FromDays(31),
                                           System.TimeSpan.FromDays(92),
                                           System.TimeSpan.FromDays(184),
                                           System.TimeSpan.FromDays(365),
                                           System.TimeSpan.FromDays(630),
                                           System.TimeSpan.MaxValue};
        #endregion
        #region Constructors
        public StockSerie(string stockName, string shortName, Groups stockGroup, StockDataProvider dataProvider)
        {
            this.StockName = stockName;
            this.ShortName = shortName;
            this.StockGroup = stockGroup;
            this.lastDate = DateTime.MinValue;
            this.StockAnalysis = new StockAnalysis();
            this.IsPortofolioSerie = false;
            this.barDuration = StockBarDuration.Daily;
            this.DataProvider = dataProvider;
            this.IsInitialised = false;
            ResetAllCache();
        }
        public StockSerie(string stockName, string shortName, string isin, Groups stockGroup, StockDataProvider dataProvider)
        {
            this.StockName = stockName;
            this.ShortName = shortName;
            this.ISIN = isin;
            this.StockGroup = stockGroup;
            this.lastDate = DateTime.MinValue;
            this.StockAnalysis = new StockAnalysis();
            this.IsPortofolioSerie = false;
            this.barDuration = StockBarDuration.Daily;
            this.DataProvider = dataProvider;
            this.IsInitialised = false;
            ResetAllCache();
        }
        private void ResetAllCache()
        {
            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
            this.FloatSerieCache = new Dictionary<string, FloatSerie>();
            this.IndicatorCache = new Dictionary<string, IStockIndicator>();
            this.DecoratorCache = new Dictionary<string, IStockDecorator>();
            this.CloudCache = new Dictionary<string, IStockCloud>();
            this.PaintBarCache = null;
            this.TrailStopCache = null;
            this.TrailCache = null;
            this.dateArray = null;
            this.valueArray = null;
            this.BarSmoothedDictionary = new SortedDictionary<string, List<StockDailyValue>>();
        }
        public void ResetIndicatorCache()
        {
            this.IndicatorCache.Clear();
            this.DecoratorCache.Clear();
            this.CloudCache.Clear();
            this.PaintBarCache = null;
            this.TrailStopCache = null;
            this.TrailCache = null;
        }

        #endregion
        #region Initialisation methods (indicator, data && events calculation)

        private Thread initialisingThread = null;
        public bool Initialise()
        {
            try
            {
                if (!this.IsInitialised)
                {
                    // Multithread management
                    while (initialisingThread != null && initialisingThread != Thread.CurrentThread)
                        Thread.Sleep(50);
                    this.initialisingThread = Thread.CurrentThread;

                    if (this.Count == 0)
                    {
                        if (!StockDataProviderBase.LoadSerieData(StockDataProviderBase.RootFolder, this) || this.Count == 0)
                        {
                            return false;
                        }
                        this.BarSmoothedDictionary.Add(StockBarDuration.Daily.ToString(), this.Values.ToList());
                    }
                    else
                    {
                        if (this.barDuration == StockBarDuration.Daily && !this.BarSmoothedDictionary.ContainsKey(StockBarDuration.Daily.ToString()))
                        {
                            this.BarSmoothedDictionary.Add(StockBarDuration.Daily.ToString(), this.Values.ToList());
                        }
                    }
                    // Force indicator,data,event and other to null;
                    PreInitialise();

                    // Flag initialisation as completed
                    this.isInitialised = this.Count > 0;
                }
                return isInitialised;
            }
            finally
            {
                this.initialisingThread = null;
            }
        }

        public void PreInitialise()
        {
            if (!this.BarSmoothedDictionary.ContainsKey(StockBarDuration.Daily.ToString()))
            {
                this.BarSmoothedDictionary.Add(StockBarDuration.Daily.ToString(), this.Values.ToList());
            }
            StockDailyValue previousValue = null;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (previousValue != null)
                {
                    dailyValue.VARIATION = (dailyValue.CLOSE - previousValue.CLOSE) / previousValue.CLOSE;
                }
                else
                {
                    dailyValue.VARIATION = (dailyValue.CLOSE - dailyValue.OPEN) / dailyValue.OPEN;
                }
                previousValue = dailyValue;
            }

            // Check if has volume on the last 10 bars, othewise, disable it
            this.HasVolume = this.Values.Any(d => d.VOLUME > 0);

            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
        }

        public struct EventMatch
        {
            public IStockViewableSeries ViewableSerie;
            public int EventIndex;
        }
        public bool MatchEvent(StockAlertDef stockAlert, int index)
        {
            StockBarDuration currentBarDuration = this.BarDuration;
            try
            {
                this.BarDuration = stockAlert.BarDuration;
                IStockEvent stockEvent = null;
                IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(stockAlert.IndicatorFullName);
                if (this.HasVolume || !indicator.RequiresVolumeData)
                {
                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                }
                else
                {
                    return false;
                }
                int eventIndex = Array.IndexOf<string>(stockEvent.EventNames, stockAlert.EventName);
                if (eventIndex == -1)
                {
                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                    return false;
                }
                else
                {
                    if (stockEvent.Events[eventIndex][index]) return true;
                }
            }
            finally
            {
                this.BarDuration = currentBarDuration;
            }

            return false;
        }

        public bool MatchEvent(StockAlertDef stockAlert)
        {
            StockBarDuration currentBarDuration = this.BarDuration;
            try
            {
                this.BarDuration = stockAlert.BarDuration;
                IStockEvent stockEvent = null;
                IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(stockAlert.IndicatorFullName);
                if (this.HasVolume || !indicator.RequiresVolumeData)
                {
                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                }
                else
                {
                    return false;
                }

                int index = LastCompleteIndex;

                int eventIndex = Array.IndexOf<string>(stockEvent.EventNames, stockAlert.EventName);
                if (eventIndex == -1)
                {
                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                }
                if (stockEvent.Events[eventIndex][index]) return true;
            }
            finally
            {
                this.BarDuration = currentBarDuration;
            }

            return false;
        }

        public bool MatchEventsAnd(List<StockAlertDef> indicators)
        {
            bool match = true;

            StockBarDuration currentBarDuration = this.BarDuration;
            try
            {
                foreach (StockAlertDef alertDef in indicators)
                {
                    this.BarDuration = alertDef.BarDuration;
                    IStockEvent stockEvent = null;
                    IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(alertDef.IndicatorFullName);
                    if (this.HasVolume || !indicator.RequiresVolumeData)
                    {
                        stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                    }
                    else
                    {
                        continue;
                    }

                    int index = LastCompleteIndex;

                    int eventIndex = Array.IndexOf<string>(stockEvent.EventNames, alertDef.EventName);
                    if (!stockEvent.Events[eventIndex][index]) return false;
                }
            }
            finally
            {
                this.BarDuration = currentBarDuration;
            }

            return match;
        }
        public bool MatchEventsOr(List<StockAlertDef> indicators)
        {
            bool match = false;

            StockBarDuration currentBarDuration = this.BarDuration;
            try
            {
                foreach (StockAlertDef alertDef in indicators)
                {
                    this.BarDuration = alertDef.BarDuration;
                    IStockEvent stockEvent = null;
                    IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(alertDef.IndicatorFullName);
                    if (this.HasVolume || !indicator.RequiresVolumeData)
                    {
                        stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                    }
                    else
                    {
                        continue;
                    }

                    int index = LastCompleteIndex;

                    int eventIndex = Array.IndexOf<string>(stockEvent.EventNames, alertDef.EventName);
                    if (stockEvent.Events[eventIndex][index]) return true;
                }
            }
            finally
            {
                this.BarDuration = currentBarDuration;
            }

            return match;
        }

        public bool MatchEventsOr(int index, List<EventMatch> viewableSeries)
        {
            bool match = false;
            IStockEvent stockEvent = null;
            foreach (EventMatch eventMatch in viewableSeries)
            {
                if (this.HasVolume || !eventMatch.ViewableSerie.RequiresVolumeData)
                {
                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(eventMatch.ViewableSerie, this);
                }
                else
                {
                    continue;
                }

                if (stockEvent is IStockDecorator)
                {
                    if (((IStockDecorator)stockEvent).Events[eventMatch.EventIndex][index]) return true;
                }
                else
                {
                    if (stockEvent.Events[eventMatch.EventIndex][index]) return true;
                }
            }
            return match;
        }
        public bool MatchEventsAnd(int index, List<EventMatch> viewableSeries)
        {
            bool match = true;
            IStockEvent stockEvent = null;
            foreach (EventMatch eventMatch in viewableSeries)
            {
                if (this.HasVolume || !eventMatch.ViewableSerie.RequiresVolumeData)
                {
                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(eventMatch.ViewableSerie, this);
                }
                else
                {
                    continue;
                }
                if (stockEvent is IStockDecorator)
                {
                    if (!((IStockDecorator)stockEvent).Events[eventMatch.EventIndex][index]) return false;
                }
                else
                {
                    if (!stockEvent.Events[eventMatch.EventIndex][index]) return false;
                }
            }
            return match;
        }
        #endregion
        #region Hilbert Sine Wave Methods
        public void CalculateHilbertSineWave(FloatSerie inputDataSerie, ref FloatSerie hilbertSine, ref FloatSerie hilbertSineLead)
        {
            double[] phase = new double[this.Count];
            double[] in_phase = new double[this.Count];
            double[] quadrature = new double[this.Count];
            double[] delta_phase = new double[this.Count];
            double[] inst_prd = new double[this.Count];
            double[] back_wtd = new double[this.Count];
            double[] diff = new double[this.Count];

            hilbertSine = new FloatSerie(this.Count);
            hilbertSineLead = new FloatSerie(this.Count);
            hilbertSine.Name = "HILBERT";
            hilbertSineLead.Name = "HILBERTL";

            double radToDegree = 180.0f / Math.PI;

            for (int currentBar = 0; currentBar < this.Count; currentBar++)
            {

                if (currentBar < 6)
                {
                    hilbertSineLead[currentBar] = 0.0f;
                    hilbertSine[currentBar] = 0.0f;
                    continue;
                }

                diff[currentBar] = (inputDataSerie[currentBar] - inputDataSerie[currentBar - 6]);

                double mid = diff[currentBar - 3];
                double wtd = 0.75 * (diff[currentBar] - diff[currentBar - 6]) + 0.25 * (diff[currentBar - 2] - diff[currentBar - 4]);

                in_phase[currentBar] = (0.33 * mid + 0.67 * in_phase[currentBar - 1]);
                quadrature[currentBar] = (0.2 * wtd + 0.8 * quadrature[currentBar - 1]);

                if (Math.Abs(in_phase[currentBar] + in_phase[currentBar - 1]) > 0)
                    phase[currentBar] = (Math.Atan(Math.Abs((quadrature[currentBar] + quadrature[currentBar - 1]) / (in_phase[currentBar] + in_phase[currentBar - 1]))) * radToDegree);

                if (in_phase[currentBar] < 0 && quadrature[currentBar] > 0)
                    phase[currentBar] = (180 - phase[currentBar]);
                else if (in_phase[currentBar] < 0 && quadrature[currentBar] < 0)
                    phase[currentBar] = (180 + phase[currentBar]);
                else if (in_phase[currentBar] > 0 && quadrature[currentBar] < 0)
                    phase[currentBar] = (360 - phase[currentBar]);

                delta_phase[currentBar] = (phase[currentBar - 1] - phase[currentBar]);

                if (phase[currentBar - 1] < 90 && phase[currentBar] > 270)
                    delta_phase[currentBar] = 360 + phase[currentBar - 1] - phase[currentBar];
                else if (delta_phase[currentBar] < 1)
                    delta_phase[currentBar] = (1);
                else if (delta_phase[currentBar] > 60)
                    delta_phase[currentBar] = (60);

                double delta_phase_sum = 0;
                inst_prd[currentBar] = (0);

                for (int i = 0; i <= Math.Min(40, currentBar); i++)
                {
                    delta_phase_sum = delta_phase_sum + delta_phase[currentBar - i];
                    if (delta_phase_sum > 360 && inst_prd[currentBar] == 0)
                        inst_prd[currentBar] = (i);
                }

                if (inst_prd[currentBar] == 0)
                    inst_prd[currentBar] = (inst_prd[currentBar - 1]);

                back_wtd[currentBar] = (0.25 * inst_prd[currentBar] + 0.75 * back_wtd[currentBar - 1]);

                double period = Math.Truncate(back_wtd[currentBar]);
                double real_part = 0;
                double imag_part = 0;
                double dc_phase = 0;

                double tmp;
                for (int i = 0; i <= period - 1; i++)
                {
                    tmp = (2 * i / period) * Math.PI;
                    real_part = real_part + Math.Sin(tmp) * inputDataSerie[currentBar - i];
                    imag_part = imag_part + Math.Cos(tmp) * inputDataSerie[currentBar - i];
                }

                if (Math.Abs(imag_part) > 0.001)
                    dc_phase = Math.Atan(real_part / imag_part) * radToDegree;
                else if (Math.Abs(imag_part) <= 0.001)
                    dc_phase = 90 * Math.Sign(real_part);

                dc_phase = dc_phase + 90;

                if (imag_part < 0)
                    dc_phase = dc_phase + 180;
                else if (dc_phase > 315)
                    dc_phase = dc_phase - 360;

                hilbertSine[currentBar] = (float)(Math.Sin(dc_phase / radToDegree));
                hilbertSineLead[currentBar] = (float)(Math.Sin((dc_phase + 55) / radToDegree));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slowFastIndicator"></param>
        /// <param name="nbLatencyBars"></param>
        /// <param name="supportSerie"></param>
        /// <param name="resistanceSerie"></param>
        public void CalculateSR(IStockIndicator slowFastIndicator, int nbLatencyBars, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, slowFastIndicator.Name.Split('(')[0] + ".S");
            resistanceSerie = new FloatSerie(this.Count, slowFastIndicator.Name.Split('(')[0] + ".R");

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie ema5Serie = this.GetIndicator("EMA(5)").Series[0];

            bool inSwing = true; // Indicate a turning point was found and waiting for a Cyclical turn
            bool lookForBottom = true;
            float previousExtremum = this.First().Value.LOW;
            float previousValue = -this.First().Value.LOW;

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = this.First().Value.LOW;

            int cyclicalTurnIndex = 0;

            FloatSerie fastSerie = slowFastIndicator.Series[0];
            FloatSerie slowSerie = slowFastIndicator.Series[1];

            BoolSerie bullishCrossingSerie = slowFastIndicator.Events[2];
            BoolSerie bearishCrossingSerie = slowFastIndicator.Events[3];

            bool isUpCycle;
            bool previousIsUpCycle = false;
            for (int i = 1; i < this.Count; i++)
            {
                isUpCycle = slowSerie[i] < fastSerie[i];

                if (inSwing)
                {
                    if (previousIsUpCycle && !isUpCycle)
                    {
                        previousExtremum = Math.Max(highSerie[i], highSerie[i - 1]);
                        lookForBottom = false;
                        cyclicalTurnIndex = i;
                        inSwing = false;
                    }
                    else if (!previousIsUpCycle && isUpCycle)
                    {
                        previousExtremum = Math.Min(lowSerie[i], lowSerie[i - 1]);
                        lookForBottom = true;
                        cyclicalTurnIndex = i;
                        inSwing = false;
                    }
                }
                else
                {
                    // Looking for a new top or bottom.
                    if (lookForBottom)
                    {
                        if ((i - cyclicalTurnIndex) > nbLatencyBars || ema5Serie[i] < closeSerie[i])
                        {
                            // Bottom has been found
                            previousValue = -previousExtremum;
                            inSwing = true;
                        }
                        else
                        {
                            previousExtremum = Math.Min(previousExtremum, lowSerie[i]);
                        }
                    }
                    else
                    {
                        if ((i - cyclicalTurnIndex) > nbLatencyBars || ema5Serie[i] > closeSerie[i])
                        {
                            // Top has been found
                            previousValue = previousExtremum;
                            inSwing = true;
                        }
                        else
                        {
                            previousExtremum = Math.Max(previousExtremum, highSerie[i]);
                        }
                    }
                }
                if (previousValue < 0)
                {
                    resistanceSerie[i] = float.NaN;
                    supportSerie[i] = -previousValue;
                }
                else
                {
                    resistanceSerie[i] = previousValue;
                    supportSerie[i] = float.NaN;
                }
                previousIsUpCycle = isUpCycle;
            }
        }

        public void CalculateHilbertSR(IStockIndicator hilbertIndicator, int nbLatencyBars, out FloatSerie hilbertS, out FloatSerie hilbertR, out FloatSerie secondarySupport, out FloatSerie secondaryResistance)
        {
            hilbertS = new FloatSerie(this.Count, "HILBERT.S");
            hilbertR = new FloatSerie(this.Count, "HILBERT.R");
            secondarySupport = new FloatSerie(this.Count, "Secondary.S");
            secondaryResistance = new FloatSerie(this.Count, "Secondary.R");

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie ema5Serie = this.GetIndicator("EMA(5)").Series[0];
            FloatSerie ema8Serie = this.GetIndicator("EMA(8)").Series[0];

            bool inSwing = true; // Indicate a turning point was found and waiting for a Cyclical turn
            bool lookForBottom = true;
            float previousExtremum = this.First().Value.LOW;
            float previousValue = -this.First().Value.LOW;

            hilbertS[0] = float.NaN;
            hilbertR[0] = this.First().Value.LOW;
            secondaryResistance[0] = secondarySupport[0] = float.NaN;

            float previousSecondarySupport = float.NaN;
            float previousSecondaryResistance = float.NaN;

            int cyclicalTurnIndex = 0;

            FloatSerie hilbertSine = hilbertIndicator.Series[0];
            FloatSerie hilbertSineLead = hilbertIndicator.Series[1];

            BoolSerie bullishCrossingSerie = hilbertIndicator.Events[2];
            BoolSerie bearishCrossingSerie = hilbertIndicator.Events[3];

            bool isUpCycle;
            bool previousIsUpCycle = false;
            for (int i = 1; i < this.Count; i++)
            {
                isUpCycle = hilbertSineLead[i] > hilbertSine[i];

                // Manage when a cross happened before the S or R has been found
                //if (!inSwing && isUpCycle != previousIsUpCycle)
                //{
                //    if (lookForBottom)
                //    {
                //        hilbertR[i-1] = float.NaN;
                //        hilbertS[i-1] = previousExtremum;
                //    }
                //    else
                //    {
                //        hilbertR[i - 1] = previousExtremum; 
                //        hilbertS[i - 1]= float.NaN;
                //    }
                //    previousSecondarySupport = float.NaN;
                //    previousSecondaryResistance = float.NaN;
                //    inSwing = true;
                //    lookForBottom = !lookForBottom;
                //}

                if (inSwing)
                {
                    if (previousIsUpCycle && !isUpCycle)
                    {
                        previousExtremum = Math.Max(highSerie[i], highSerie[i - 1]);
                        lookForBottom = false;
                        cyclicalTurnIndex = i;
                        inSwing = false;
                    }
                    else if (!previousIsUpCycle && isUpCycle)
                    {
                        previousExtremum = Math.Min(lowSerie[i], lowSerie[i - 1]);
                        lookForBottom = true;
                        cyclicalTurnIndex = i;
                        inSwing = false;
                    }
                }
                else
                {
                    // Looking for a new top or bottom.
                    if (lookForBottom)
                    {
                        if ((i - cyclicalTurnIndex) > nbLatencyBars || ema5Serie[i] < closeSerie[i])
                        {
                            // Bottom has been found
                            previousValue = -previousExtremum;
                            previousSecondarySupport = float.NaN;
                            previousSecondaryResistance = float.NaN;
                            inSwing = true;
                        }
                        else
                        {
                            previousExtremum = Math.Min(previousExtremum, lowSerie[i]);
                        }
                    }
                    else
                    {
                        if ((i - cyclicalTurnIndex) > nbLatencyBars || ema5Serie[i] > closeSerie[i])
                        {
                            // Top has been found
                            previousValue = previousExtremum;
                            previousSecondarySupport = float.NaN;
                            previousSecondaryResistance = float.NaN;
                            inSwing = true;
                        }
                        else
                        {
                            previousExtremum = Math.Max(previousExtremum, highSerie[i]);
                        }
                    }
                }
                if (previousValue < 0)
                {
                    hilbertR[i] = float.NaN;
                    hilbertS[i] = -previousValue;
                }
                else
                {
                    hilbertR[i] = previousValue;
                    hilbertS[i] = float.NaN;
                }
                previousIsUpCycle = isUpCycle;

                // Secondary support/resistance management
                if (previousValue < 0 && closeSerie[i] < -previousValue) // looking for secondary resistance
                {
                    if (float.IsNaN(previousSecondaryResistance))
                    {
                        if (closeSerie[i] > ema8Serie[i])
                        {
                            previousSecondaryResistance = lowSerie.GetMin(Math.Max(0, i - nbLatencyBars), i);
                        }
                    }
                    else
                    {
                        if (closeSerie[i] < previousSecondaryResistance) // Secondary resistance has been broken
                        {
                            previousSecondaryResistance = float.NaN;
                        }
                    }
                }
                else if (previousValue > 0 && closeSerie[i] > previousValue) // looking for secondary support
                {
                    if (float.IsNaN(previousSecondarySupport))
                    {
                        if (closeSerie[i] < ema8Serie[i])
                        {
                            previousSecondarySupport = highSerie.GetMax(Math.Max(0, i - nbLatencyBars), i);
                        }
                    }
                    else
                    {
                        if (closeSerie[i] > previousSecondarySupport) // Secondary support has been broken
                        {
                            previousSecondarySupport = float.NaN;
                        }
                    }
                }
                secondarySupport[i] = previousSecondarySupport;
                secondaryResistance[i] = previousSecondaryResistance;
            }
        }
        public void CalculateSlowFastSR(FloatSerie fastSerie, FloatSerie slowSerie, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, fastSerie.Name + ".S");
            resistanceSerie = new FloatSerie(this.Count, fastSerie.Name + ".R");

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = this.First().Value.LOW;
            float latestHigh = this.First().Value.HIGH;
            float latestLow = this.First().Value.LOW;

            bool isUpCycle;
            bool previousIsUpCycle = false;
            for (int i = 1; i < this.Count; i++)
            {
                isUpCycle = fastSerie[i] > slowSerie[i];

                latestHigh = Math.Max(highSerie[i], latestHigh);
                latestLow = Math.Min(lowSerie[i], latestLow);

                if (isUpCycle)
                {
                    if (!previousIsUpCycle)
                    { // Support detected
                        supportSerie[i] = latestLow;
                        latestHigh = float.MinValue;
                    }
                    else
                    {

                        supportSerie[i] = supportSerie[i - 1];
                    }
                    resistanceSerie[i] = float.NaN;
                }
                else
                {
                    if (previousIsUpCycle)
                    { // Support detected
                        resistanceSerie[i] = latestHigh;
                        latestLow = float.MaxValue;
                    }
                    else
                    {

                        resistanceSerie[i] = supportSerie[i - 1];
                    }
                    supportSerie[i] = float.NaN;
                }

                previousIsUpCycle = isUpCycle;
            }
        }
        public void CalculateSlowFastSR(FloatSerie fastSerie, FloatSerie slowSerie, float overbought, float oversold, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, fastSerie.Name + ".S");
            resistanceSerie = new FloatSerie(this.Count, fastSerie.Name + ".R");

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = this.First().Value.LOW;
            float latestHigh = this.First().Value.HIGH;
            float latestLow = this.First().Value.LOW;

            bool isUpCycle = false;
            bool previousIsUpCycle = false;
            for (int i = 1; i < this.Count; i++)
            {
                if (fastSerie[i - 1] > slowSerie[i - 1] || (previousIsUpCycle && fastSerie[i - 1] > overbought))
                {
                    isUpCycle = true;
                }
                else
                {
                    if (fastSerie[i - 1] < slowSerie[i - 1] || (!previousIsUpCycle && fastSerie[i - 1] < oversold))
                    {
                        isUpCycle = false;
                    }
                }

                latestHigh = Math.Max(highSerie[i], Math.Max(highSerie[i - 1], latestHigh));
                latestLow = Math.Min(lowSerie[i], Math.Min(lowSerie[i - 1], latestLow));

                if (isUpCycle)
                {
                    if (!previousIsUpCycle)
                    { // Support detected
                        supportSerie[i] = latestLow;
                        latestHigh = float.MinValue;
                    }
                    else
                    {

                        supportSerie[i] = supportSerie[i - 1];
                    }
                    resistanceSerie[i] = float.NaN;
                }
                else
                {
                    if (previousIsUpCycle)
                    { // Resistance detected
                        resistanceSerie[i] = latestHigh;
                        latestLow = float.MaxValue;
                    }
                    else
                    {

                        resistanceSerie[i] = resistanceSerie[i - 1];
                    }
                    supportSerie[i] = float.NaN;
                }

                previousIsUpCycle = isUpCycle;
            }
        }
        public void CalculateCrossSR(FloatSerie fastSerie, int smoothing, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, fastSerie.Name + ".S", float.NaN);
            resistanceSerie = new FloatSerie(this.Count, fastSerie.Name + ".R", float.NaN);

            FloatSerie slowSerie = fastSerie.CalculateEMA(smoothing);

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = this.First().Value.LOW;
            float latestHigh = this.First().Value.HIGH;
            float latestLow = this.First().Value.LOW;

            bool isUp = true;
            for (int i = 1; i < this.Count; i++)
            {
                if (isUp)
                {
                    if (fastSerie[i] < slowSerie[i])
                    {
                        // Resistance detected
                        resistanceSerie[i] = latestHigh;
                        supportSerie[i] = float.NaN;
                        isUp = false;
                        latestLow = lowSerie[i];
                    }
                    else
                    {
                        resistanceSerie[i] = resistanceSerie[i - 1];
                        supportSerie[i] = supportSerie[i - 1];
                    }
                }
                else
                {
                    if (fastSerie[i] > slowSerie[i])
                    {
                        // Support detected
                        supportSerie[i] = latestLow;
                        resistanceSerie[i] = float.NaN;
                        isUp = true;
                        latestHigh = highSerie[i];
                    }
                    else
                    {
                        resistanceSerie[i] = resistanceSerie[i - 1];
                        supportSerie[i] = supportSerie[i - 1];
                    }
                }

                latestHigh = Math.Max(highSerie[i], Math.Max(highSerie[i - 1], latestHigh));
                latestLow = Math.Min(lowSerie[i], Math.Min(lowSerie[i - 1], latestLow));
            }
        }

        public void CalculateOverboughtSR(FloatSerie serie, float overbought, float oversold, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, serie.Name + ".S", float.NaN);
            resistanceSerie = new FloatSerie(this.Count, serie.Name + ".R", float.NaN);

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = this.First().Value.LOW;
            float latestHigh = this.First().Value.HIGH;
            float latestLow = this.First().Value.LOW;

            bool isOverbought = false;
            bool isOversold = false;
            for (int i = 1; i < this.Count; i++)
            {
                if (isOverbought)
                {
                    if (serie[i] < overbought)
                    {
                        // Resistance detected
                        resistanceSerie[i] = latestHigh;
                        supportSerie[i] = float.NaN;
                        isOverbought = false;
                    }
                    else
                    {
                        resistanceSerie[i] = resistanceSerie[i - 1];
                        supportSerie[i] = supportSerie[i - 1];
                    }
                }
                else if (isOversold)
                {
                    if (serie[i] > oversold)
                    {
                        // Support detected
                        supportSerie[i] = latestLow;
                        resistanceSerie[i] = float.NaN;
                        isOversold = false;
                    }
                    else
                    {
                        resistanceSerie[i] = resistanceSerie[i - 1];
                        supportSerie[i] = supportSerie[i - 1];
                    }
                }
                else
                {
                    resistanceSerie[i] = resistanceSerie[i - 1];
                    supportSerie[i] = supportSerie[i - 1];
                    if (serie[i] > overbought)
                    {
                        latestHigh = highSerie[i];
                        isOverbought = true;
                    }
                    if (serie[i] < oversold)
                    {
                        latestLow = lowSerie[i];
                        isOversold = true;
                    }
                }

                latestHigh = Math.Max(highSerie[i], Math.Max(highSerie[i - 1], latestHigh));
                latestLow = Math.Min(lowSerie[i], Math.Min(lowSerie[i - 1], latestLow));
            }
        }
        #endregion
        #region Indicators calculation

        public FloatSerie CalculateWilliamAccumulationDistribution()
        {
            FloatSerie wadSerie = new FloatSerie(this.Count, "WAD");

            FloatSerie volume = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f));
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);

            float wad = 0;
            for (int i = 1; i < this.Count; i++)
            {
                float trueRangeHigh = Math.Max(highSerie[i], closeSerie[i - 1]);
                float trueRangeLow = Math.Min(lowSerie[i], closeSerie[i - 1]);
                float priceMove = 0;
                float vol = (float)Math.Sqrt(volume[i]);

                if (closeSerie[i] > closeSerie[i - 1])
                {
                    priceMove = closeSerie[i] - trueRangeLow;
                }
                else if (closeSerie[i] < closeSerie[i - 1])
                {
                    priceMove = closeSerie[i] - trueRangeHigh;
                }
                wad += priceMove * vol;
                wadSerie[i] = wad;
            }
            return wadSerie;

        }
        public FloatSerie CalculateChaikinMoneyFlow(int period)
        {
            FloatSerie cmfSerie = new FloatSerie(this.Count);

            float volumeCumul = 0;
            float valueCumul = 0;
            StockDailyValue dailyValue = null;
            StockDailyValue previousValue = null;
            float low, high;
            for (int i = 0; i < this.Count; i++)
            {
                if (i < period)
                {
                    cmfSerie[i] = 0;
                }
                else
                {
                    volumeCumul = 0;
                    valueCumul = 0;
                    for (int j = i - period + 1; j <= i; j++)
                    {
                        dailyValue = this.ValueArray[j];
                        previousValue = this.ValueArray[j - 1];
                        low = Math.Min(dailyValue.LOW, previousValue.HIGH);
                        high = Math.Max(dailyValue.HIGH, previousValue.LOW);
                        if (high != low)
                        {
                            valueCumul += dailyValue.VOLUME * ((dailyValue.CLOSE - low) - (high - dailyValue.CLOSE)) / (high - low);
                        }
                        volumeCumul += dailyValue.VOLUME;
                    }
                    if (volumeCumul == 0.0f)
                    {
                        cmfSerie[i] = 0.0f;
                    }
                    else
                    {
                        cmfSerie[i] = valueCumul / volumeCumul;
                    }
                }
            }
            return cmfSerie;
        }
        public FloatSerie CalculateDistanceToSerie(FloatSerie dataSerie)
        {
            float dailyData = .0f;
            FloatSerie distanceSerie = new FloatSerie(this.Count);
            int i = 0;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                dailyData = dataSerie.Values[i];
                if (dailyValue.HIGH < dailyData)
                {
                    distanceSerie.Values[i] = dailyValue.HIGH - dailyData;
                }
                else if (dailyData < dailyValue.LOW)
                {
                    distanceSerie.Values[i] = dailyValue.LOW - dailyData;
                }
                else
                {
                    distanceSerie.Values[i] = 0.0f;
                }
                i++;
            }
            return distanceSerie;
        }

        public FloatSerie CalculateRateOfRise(int period, bool bodyLow = true)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = bodyLow ? this.GetSerie(StockDataType.LOW) : new FloatSerie(this.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)));

            FloatSerie serie = new FloatSerie(Values.Count());
            float min;

            for (int i = 1; i < Math.Min(period, this.Count); i++)
            {
                min = lowSerie.GetMin(0, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            for (int i = period; i < this.Count; i++)
            {
                min = lowSerie.GetMin(i - period, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            serie.Name = "ROR_" + period.ToString();
            return serie;
        }

        public FloatSerie CalculateRateOfDecline(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie serie = new FloatSerie(Values.Count());
            float max;

            for (int i = 1; i < Math.Min(period, this.Count); i++)
            {
                max = highSerie.GetMax(0, i);
                serie[i] = (closeSerie[i] - max) / max;
            }
            for (int i = period; i < this.Count; i++)
            {
                max = highSerie.GetMax(i - period, i);
                serie[i] = (closeSerie[i] - max) / max;
            }
            serie.Name = "ROD_" + period.ToString();
            return serie;
        }
        public FloatSerie CalculateRateOfChange(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie serie = new FloatSerie(Values.Count());
            for (int i = 1; i < Math.Min(period, this.Count); i++)
            {
                serie[i] = (closeSerie[i] - closeSerie[0]) / closeSerie[0];
            }
            for (int i = period; i < this.Count; i++)
            {
                serie[i] = (closeSerie[i] - closeSerie[i - period]) / closeSerie[i - period];
            }
            serie.Name = "ROC_" + period.ToString();
            return serie;
        }
        public FloatSerie CalculateOnBalanceVolume()
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "OBV");
            }

            FloatSerie OBV = new FloatSerie(this.Count, "OBV");
            FloatSerie vol = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f));
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            float previousClose = closeSerie[0];
            for (int i = 1; i < this.Count; i++)
            {
                if (closeSerie[i] > previousClose)
                {
                    OBV[i] = OBV[i - 1] + vol[i];
                }
                else if (closeSerie[i] < previousClose)
                {
                    OBV[i] = OBV[i - 1] - vol[i];
                }
                else
                {
                    OBV[i] = OBV[i - 1];
                }
                previousClose = closeSerie[i];
            }
            return OBV;
        }
        public FloatSerie CalculateOnBalanceVolumeEx(int period)
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "OBVEX");
            }
            var volume = this.Values.Select(v => (float)(v.VARIATION >= 0 ? v.VOLUME : -v.VOLUME)).ToArray();

            FloatSerie volumeSerie = new FloatSerie(volume);
            var OBVEX = volumeSerie.CalculateEMA(period);
            OBVEX.Name = $"OBVEX({period}";

            return OBVEX;
        }
        public FloatSerie CalculateVolatility(int period)
        {
            FloatSerie volatilitySerie = new FloatSerie(this.Values.Count, "VLTY");
            FloatSerie variationSerie = this.GetSerie(StockDataType.VARIATION);
            int i = 0;
            for (i = 1; i <= period; i++)
            {
                volatilitySerie[i] = volatilitySerie[i - 1] + Math.Abs(variationSerie[i]);
            }
            for (i = period + 1; i < this.Count; i++)
            {
                volatilitySerie[i] = volatilitySerie[i - 1] + Math.Abs(variationSerie[i]) - Math.Abs(variationSerie[i - period]);
            }

            return volatilitySerie;
        }
        public FloatSerie CalculateER(int period, int inputSmoothing)
        {
            return this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing).CalculateER(period);
        }
        public FloatSerie CalculateVolatility2(int period)
        {
            FloatSerie volatilitySerie = new FloatSerie(this.Values.Count, "VLTY2");
            FloatSerie volatilitySerie2 = new FloatSerie(this.Values.Count, "VLTY2");
            int i = 0;
            StockDailyValue previousValue = null;
            foreach (StockDailyValue val in this.Values)
            {
                if (i > period && previousValue != null && previousValue.LOW != 0.0f && val.HIGH != 0.0f && previousValue.HIGH != 0.0f && val.LOW != 0.0f)
                {
                    volatilitySerie[i] = (float)Math.Log((previousValue.LOW / val.HIGH + previousValue.HIGH / val.LOW) / 2.0f);
                }
                i++;
                previousValue = val;
            }

            for (i = period; i < this.Count; i++)
            {
                for (int j = 0; j < period; j++)
                {
                    volatilitySerie2[i] += volatilitySerie[i - j];
                }
            }

            return volatilitySerie2;
        }
        public FloatSerie CalculateVolatilityStdev(int period)
        {
            FloatSerie volatilitySerie = new FloatSerie(this.Values.Count, "VLTY");
            float K = (float)Math.Sqrt(252) * 100.0f;
            FloatSerie logReturnSerie = new FloatSerie(this.Values.Count);
            logReturnSerie[0] = 0.00f;
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            for (int i = 1; i < logReturnSerie.Count; i++)
            {
                logReturnSerie[i] = (float)Math.Log(Math.Max(highSerie[i], highSerie[i - 1]) / Math.Min(lowSerie[i], lowSerie[i - 1]));
                //logReturnSerie[i] = (float)Math.Log(this.Values.ElementAt(i).CLOSE / this.Values.ElementAt(i - 1).CLOSE);
                if (i < period)
                {
                    volatilitySerie[i] = K * logReturnSerie.CalculateStdev(0, i);
                }
                else
                {
                    volatilitySerie[i] = K * logReturnSerie.CalculateStdev(i - period + 1, i);
                }
            }
            volatilitySerie[0] = volatilitySerie[1];
            return volatilitySerie;
        }
        public StockSerie CalculateSeasonality()
        {
            this.barDuration = StockBarDuration.Daily;
            // Calculate average daily returns
            StockSerie seasonalSerie = new StockSerie(this.StockName + ".SEAS", this.ShortName + ".SEAS", this.StockGroup, StockDataProvider.Generated);
            float[] dailyVariation = new float[366];
            int[] occurences = new int[366];
            long[] volume = new long[366];
            int dayOfYear;
            double exponent = 1.25;
            double reverseExponent = 1.0 / exponent;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                dayOfYear = dailyValue.DATE.DayOfYear - 1;
                occurences[dayOfYear]++;
                if (dailyValue.VARIATION >= 0f)
                {
                    dailyVariation[dayOfYear] += (float)Math.Pow(dailyValue.VARIATION, exponent);
                }
                else
                {
                    dailyVariation[dayOfYear] -= (float)Math.Pow(-dailyValue.VARIATION, exponent);
                }
                volume[dayOfYear] += dailyValue.VOLUME;
            }
            for (int i = 0; i < 366; i++)
            {
                if (dailyVariation[i] >= 0)
                {
                    dailyVariation[i] = (float)Math.Pow(dailyVariation[i], reverseExponent);
                }
                else
                {
                    dailyVariation[i] = -(float)Math.Pow(-dailyVariation[i], reverseExponent);
                }
            }

            // Calculate rebased serie over a year
            float[] rebasedSerie = new float[366];
            float previousClose = 100;
            float close = 100;
            for (int i = 0; i < 366; i++)
            {
                if (occurences[i] != 0)
                {
                    close = previousClose * (1 + dailyVariation[i] / (float)occurences[i]);
                    rebasedSerie[i] = close;
                    previousClose = close;
                }
                else
                {
                    rebasedSerie[i] = close;
                }
            }
            float reference = 1000f;
            previousClose = reference;
            close = reference;
            int day = 0;
            int previousDay = 0;
            foreach (DateTime date in this.Keys)
            {
                day = date.DayOfYear - 1;
                if (occurences[day] == 0) { throw new System.Exception("WTF"); }
                if (day < previousDay)
                {  // Happy new year !!!
                    close = previousClose * (1.0f + (rebasedSerie[day] - 100f) / 100f);
                    seasonalSerie.Add(date,
                        new StockDailyValue(previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
                    previousClose = close;
                }
                else
                {
                    close = previousClose * (1.0f + (rebasedSerie[day] - rebasedSerie[previousDay]) / rebasedSerie[previousDay]);
                    seasonalSerie.Add(date,
                        new StockDailyValue(previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
                    previousClose = close;
                }
                previousDay = day;
            }
            // Continue filling the serie until the end of the year
            DateTime startDate = seasonalSerie.Keys.Last().Date.AddDays(1);
            DateTime endDate = new DateTime(startDate.Year, 12, 31);
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                day = date.DayOfYear - 1;
                if (occurences[day] == 0) { continue; }
                close = previousClose * (1.0f + (rebasedSerie[day] - rebasedSerie[previousDay]) / rebasedSerie[previousDay]);
                seasonalSerie.Add(date,
                    new StockDailyValue(previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
                previousClose = close;
                previousDay = day;
            }

            //DateTime date = new DateTime(DateTime.Today.Year, 1, 1);
            //for (int i = 0; i < 366; i++)
            //{
            //    if (occurences[i] != 0)
            //    {
            //        close = previousClose * (1 + dailyVariation[i] / (float)occurences[i]);
            //        seasonalSerie.Add(date,
            //            new StockDailyValue(seasonalSerie.StockName, previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[i], date));
            //        previousClose = close;
            //    }
            //    date = date.AddDays(1);
            //}
            seasonalSerie.Initialise();
            return seasonalSerie;
        }

        public StockSerie GenerateOvernightStockSerie()
        {
            this.barDuration = StockBarDuration.Daily;
            // Calculate average daily returns
            StockSerie overnightSerie = new StockSerie(this.StockName + ".OVN", this.ShortName + ".OVN", this.StockGroup, StockDataProvider.Generated);
            float currentValue = this.Values.First().OPEN;
            float previousClose = currentValue;

            foreach (StockDailyValue dailyValue in this.Values)
            {
                currentValue *= 1f + (dailyValue.OPEN - previousClose) / previousClose;

                overnightSerie.Add(dailyValue.DATE,
                        new StockDailyValue(currentValue, currentValue, currentValue, currentValue, 0, dailyValue.DATE));

                previousClose = dailyValue.CLOSE;
            }
            return overnightSerie;
        }

        public FloatSerie CalculateFastOscillator(int period)
        {
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            FloatSerie fastOscillatorSerie = new FloatSerie(this.Values.Count);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = this.GetSerie(StockDataType.CLOSE);
            float lowestLow = float.MaxValue;
            float highestHigh = float.MinValue;

            for (int i = 0; i < this.Values.Count; i++)
            {
                lowestLow = lowSerie.GetMin(Math.Max(0, i - period), i);
                highestHigh = highSerie.GetMax(Math.Max(0, i - period), i);
                if (highestHigh == lowestLow)
                {
                    fastOscillatorSerie[i] = 50.0f;
                }
                else
                {
                    fastOscillatorSerie[i] = 100.0f * (closeSerie.Values[i] - lowestLow) / (highestHigh - lowestLow);
                }
                if (i < period)
                {
                    fastOscillatorSerie[i] = Math.Max(30.0f, fastOscillatorSerie[i]);
                    fastOscillatorSerie[i] = Math.Min(70.0f, fastOscillatorSerie[i]);
                }
            }
            fastOscillatorSerie.Name = "FastK(" + period.ToString() + ")";
            return fastOscillatorSerie;
        }
        public void CalculateHighLowTrailSR(int period, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, "TRAILHL.S");
            resistanceSerie = new FloatSerie(this.Count, "TRAILHL.R");

            FloatSerie longStopSerie = new FloatSerie(this.Count, "TRAILHL.Long");
            FloatSerie shortStopSerie = new FloatSerie(this.Count, "TRAILHL.Sort");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 0;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;

                supportSerie[0] = previousValue.LOW;
                resistanceSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;

                supportSerie[0] = float.NaN;
                resistanceSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > 0)
                {
                    if (i > period)
                    {
                        if (upTrend)
                        {
                            if (currentValue.CLOSE < longStopSerie[i - 1])
                            { // Trailing stop has been broken => reverse trend
                                upTrend = false;
                                longStopSerie[i] = float.NaN;
                                shortStopSerie[i] = highSerie.GetMax(i - period, i);

                                supportSerie[i] = float.NaN;
                                resistanceSerie[i] = highSerie.GetMax(i - period - 1, i);
                            }
                            else
                            {
                                // Trail the stop  
                                longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                                shortStopSerie[i] = float.NaN;

                                supportSerie[i] = supportSerie[i - 1];
                                resistanceSerie[i] = float.NaN;
                            }
                        }
                        else
                        {
                            if (currentValue.CLOSE > shortStopSerie[i - 1])
                            {  // Trailing stop has been broken => reverse trend
                                upTrend = true;
                                longStopSerie[i] = lowSerie.GetMin(i - period, i);
                                shortStopSerie[i] = float.NaN;

                                supportSerie[i] = lowSerie.GetMin(i - period - 1, i);
                                resistanceSerie[i] = float.NaN;
                            }
                            else
                            {
                                // Trail the stop  
                                longStopSerie[i] = float.NaN;
                                shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));

                                supportSerie[i] = float.NaN;
                                resistanceSerie[i] = resistanceSerie[i - 1];
                            }
                        }
                    }
                    else
                    {
                        if (upTrend)
                        {
                            if (currentValue.CLOSE < longStopSerie[i - 1])
                            { // Trailing stop has been broken => reverse trend
                                upTrend = false;
                                longStopSerie[i] = float.NaN;
                                shortStopSerie[i] = highSerie.GetMax(0, i);
                            }
                            else
                            {
                                // Trail the stop  
                                longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                                shortStopSerie[i] = float.NaN;
                            }
                        }
                        else
                        {
                            if (currentValue.CLOSE > shortStopSerie[i - 1])
                            {  // Trailing stop has been broken => reverse trend
                                upTrend = true;
                                longStopSerie[i] = lowSerie.GetMin(0, i);
                                shortStopSerie[i] = float.NaN;
                            }
                            else
                            {
                                // Trail the stop  
                                longStopSerie[i] = float.NaN;
                                shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                            }
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        /// <summary>
        /// Trail HighLow stop with adaptative period
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHighLowTrailStop2(int intialPeriod, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            int period = intialPeriod;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 0;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
                            period = intialPeriod;
                        }
                        else
                        {
                            if (lowSerie[i - 2] > lowSerie[i - 1] && lowSerie[i - 1] < lowSerie[i])
                            {
                                period = Math.Max(3, period - 1);
                            }
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            shortStopSerie[i] = float.NaN;
                            period = intialPeriod;
                        }
                        else
                        {
                            if (lowSerie[i - 2] > lowSerie[i - 1] && lowSerie[i - 1] < lowSerie[i])
                            {
                                period = Math.Max(3, period - 1);
                            }
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculateDecayTrailStop(int period, float decayPercent, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            float alpha = 2.0f / (float)(period + 1);
            float decay = 1.0f + decayPercent;

            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowEMASerie[i];
                        alpha = 2.0f / (float)(period + 1);
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + alpha * (lowEMASerie[i] - longStop));
                        alpha *= decay;
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                        alpha = 2.0f / (float)(period + 1);
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + alpha * (highEMASerie[i] - shortStop));
                        alpha *= decay;
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculateEMABBTrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILEMABB.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMABB.SS");

            FloatSerie lowSmoothSerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highSmoothSerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeSmoothSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(period);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeSmoothSerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowSmoothSerie[i];
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + closeEMASerie[i] - closeEMASerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highSmoothSerie[i]);
                    }
                }
                else
                {
                    if (closeSmoothSerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highSmoothSerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + closeEMASerie[i] - closeEMASerie[i - 1]);
                        extremum = Math.Min(extremum, lowSmoothSerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        public void CalculateEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            float alpha = 2.0f / (float)(period + 1);

            // shortStopSerie[i] = shortStopSerie[i - 1] + alpha * (closeEMASerie[i] - shortStopSerie[i - 1]);
            // longStopSerie[i] = longStopSerie[i - 1] + alpha * (closeEMASerie[i] - longStopSerie[i - 1]);

            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + alpha * (lowEMASerie[i] - longStop));
                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + alpha * (highEMASerie[i] - shortStop));
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculatePEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie EMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(period);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        var step = EMASerie[i] - EMASerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + step);
                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        var step = EMASerie[i] - EMASerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + step);
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculateHMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {

            //FloatSerie EMASerie1 = this.CalculateEMA(period / 2);
            //FloatSerie EMASerie2 = this.CalculateEMA(period);

            //return ((EMASerie1 * 2.0f) - EMASerie2).CalculateEMA((int)Math.Sqrt(period));

            float alpha1 = 2.0f / (float)(period + 1);
            float alpha2 = 2.0f / (float)(period * 2 + 1);
            float alpha3 = 2.0f / (float)(Math.Sqrt(period) + 1);
            float ema1, ema2, ema3;

            // shortStopSerie[i] = shortStopSerie[i - 1] + alpha * (closeEMASerie[i] - shortStopSerie[i - 1]);
            // longStopSerie[i] = longStopSerie[i - 1] + alpha * (closeEMASerie[i] - longStopSerie[i - 1]);

            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
                ema1 = ema2 = previousValue.LOW;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
                ema1 = ema2 = previousValue.HIGH;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        ema1 = extremum;
                        ema2 = extremum;
                        extremum = lowEMASerie[i];

                    }
                    else
                    {
                        // Trail the stop  
                        ema1 = ema1 + alpha1 * (closeEMASerie[i] - ema1);
                        ema2 = ema2 + alpha2 * (closeEMASerie[i] - ema2);
                        ema3 = (ema1 * 2.0f) - ema2;
                        longStopSerie[i] = longStopSerie[i - 1] + alpha3 * (ema3 - longStopSerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        ema1 = extremum;
                        ema2 = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        ema1 = ema1 + alpha1 * (closeEMASerie[i] - ema1);
                        ema2 = ema2 + alpha2 * (closeEMASerie[i] - ema2);
                        ema3 = (ema1 * 2.0f) - ema2;
                        shortStopSerie[i] = shortStopSerie[i - 1] + alpha3 * (ema3 - shortStopSerie[i - 1]);
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fadePeriod"></param>
        /// <param name="inputSmoothing"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHLAVGTrailStop(int fadePeriod, int inputSmoothing, out FloatSerie longStopSerie,
           out FloatSerie shortStopSerie)
        {
            float ratio = 1.0f;
            float fadeOut = 1f - 1f / fadePeriod;

            float min, max;

            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                min = lowSerie.GetMin(Math.Max(0, i - fadePeriod), i);
                max = highSerie.GetMax(Math.Max(0, i - fadePeriod), i);

                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowSerie[i];
                        ratio = 1.0f;
                    }
                    else
                    {
                        // Trail the stop
                        ratio *= fadeOut;
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], min * ratio + max * (1f - ratio));
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highSerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highSerie[i];
                        ratio = 1.0f;
                    }
                    else
                    {
                        // Trail the stop  
                        ratio *= fadeOut;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], min * (1f - ratio) + max * ratio);
                        extremum = Math.Min(extremum, lowSerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        public void CalculatePercentTrailStop(float percent, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;

            float upPercent = 1.0f + percent;
            float downPercent = 1.0f - percent;

            if (upTrend)
            {
                longStopSerie[0] = previousValue.HIGH * downPercent;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.LOW * upPercent;
            }
            int i = 1;
            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (currentValue.CLOSE < longStopSerie[i - 1])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = lowSerie[i] * upPercent;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], highSerie[i] * downPercent);
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (currentValue.CLOSE > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = highSerie[i] * downPercent;
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], lowSerie[i] * upPercent);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        public void CalculateHighLowSmoothedTrailStop(int period, int smoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW).CalculateEMA(smoothing);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(smoothing);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 0;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                        }
                    }
                }
                else if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        /// <summary>
        /// Calculate trail stop trailing using the daily variation (adding daily variation only when positive for uptrend)
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateVarTrailStop(int period, string indicatorName, float indicatorMax, float indicatorCenter, float indicatorWeight, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            FloatSerie ERSerie = this.GetIndicator(indicatorName).Series[0].Abs() - indicatorCenter;
            ERSerie = ERSerie * (indicatorWeight / (indicatorMax - indicatorCenter));

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            int i = 1;
            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
                        }
                        else
                        {
                            // Trail the stop 
                            longStopSerie[i] = currentValue.VARIATION > 0 ? longStopSerie[i - 1] * (1f + currentValue.VARIATION * ERSerie[i]) : longStopSerie[i - 1];
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = currentValue.VARIATION < 0 ? shortStopSerie[i - 1] * (1f + currentValue.VARIATION * ERSerie[i]) : shortStopSerie[i - 1];
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = currentValue.VARIATION > 0 ? longStopSerie[i - 1] * (1f + currentValue.VARIATION * ERSerie[i]) : longStopSerie[i - 1];
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = currentValue.VARIATION < 0 ? shortStopSerie[i - 1] * (1f + currentValue.VARIATION * ERSerie[i]) : shortStopSerie[i - 1];
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateCountbackLineTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILCBL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILCBL.SS");

            if (this.ValueArray.Length < period) return;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            bool upTrend = closeSerie[1] > closeSerie[0];
            if (upTrend)
            {
                longStopSerie[0] = Math.Min(lowSerie[0], lowSerie[1]);
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = Math.Max(highSerie[0], highSerie[1]);
            }
            for (int i = 1; i < this.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    {// Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = highSerie.GetCountBackHigh(i, period);
                    }
                    else
                    {// Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetCountBackLow(i, period));
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = lowSerie.GetCountBackLow(i, period);
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        if (lowSerie[i] < lowSerie[i - 1])
                        {
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetCountBackHigh(i, period));
                        }
                        else
                        {
                            shortStopSerie[i] = shortStopSerie[i - 1];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHighLowTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            if (this.ValueArray.Length < period) return;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 1;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period - 1, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period - 1, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHighLowBodyTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            if (this.ValueArray.Length < period) return;

            var bodyHighSerie = new FloatSerie(this.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
            var bodyLowSerie = new FloatSerie(this.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 1;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = bodyHighSerie.GetMax(i - period - 1, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], bodyLowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = bodyLowSerie.GetMin(i - period - 1, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], bodyHighSerie.GetMax(i - period, i));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = bodyHighSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], bodyLowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = bodyLowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], bodyHighSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHighLowFollowTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            if (this.ValueArray.Length < period) return;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
            int i = 0;
            float R1Percent;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                R1Percent = (this.ValueArray[1].HIGH - previousValue.LOW) / this.ValueArray[1].HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                R1Percent = (previousValue.HIGH - this.ValueArray[1].LOW) / this.ValueArray[1].LOW;
            }
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
                            R1Percent = (shortStopSerie[i] - currentValue.LOW) / currentValue.LOW;
                        }
                        else
                        {
                            // Trail the stop
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], currentValue.HIGH * (1.0f - R1Percent));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            shortStopSerie[i] = float.NaN;
                            R1Percent = (currentValue.HIGH - longStopSerie[i]) / currentValue.HIGH;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], currentValue.LOW * (1.0f + R1Percent));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                            R1Percent = (shortStopSerie[i] - currentValue.LOW) / currentValue.LOW;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], currentValue.HIGH * (1.0f - R1Percent));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                            R1Percent = (currentValue.HIGH - longStopSerie[i]) / currentValue.HIGH;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], currentValue.LOW * (1.0f + R1Percent));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        public void CalculateBBTrailStop(FloatSerie lowerBB, FloatSerie upperBB, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILBB.S");
            shortStopSerie = new FloatSerie(this.Count, "TRAILBB.R");
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            int i = 0;
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = upperBB[i];
                        }
                        else
                        {
                            // UpTrend still in place
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBB[i]);
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowerBB[i];
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Down trend still in place
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBB[i]);
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculateTOPSAR(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            float accelerationFactorUp = accelerationFactorInit;
            float accelerationFactorDown = accelerationFactorInit;
            bool isUpTrend = false;
            bool isDownTrend = false;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "TOPSAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "TOPSAR.R");
            float previousLow = float.NaN;
            float previousHigh = float.NaN;
            float previousSARUp = float.NaN;
            float previousSARDown = float.NaN;
            sarSerieResistance[0] = sarSerieResistance[1] = float.NaN;
            sarSerieSupport[0] = sarSerieSupport[1] = float.NaN;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 2; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    float nextSAR = Math.Max(previousSARUp, previousSARUp + accelerationFactorUp * (lowSerie[i] - previousSARUp));
                    if (nextSAR >= closeSerie[i]) // UpTrendBroken
                    {
                        isUpTrend = false;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else // Up Trend continues
                    {
                        previousSARUp = nextSAR;
                        sarSerieSupport[i] = previousSARUp;
                        accelerationFactorUp = Math.Min(accelerationFactorMax, accelerationFactorUp + accelerationFactorStep);
                    }
                }
                else
                {
                    if (lowSerie.IsBottom(i - 1)) // Uptrend starts
                    {
                        isUpTrend = true;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = previousLow = previousSARUp = lowSerie[i - 1];
                    }
                    else
                    {
                        sarSerieSupport[i] = float.NaN;
                    }
                }
                if (isDownTrend)
                {
                    float nextSAR = Math.Min(previousSARDown, previousSARDown + accelerationFactorDown * (highSerie[i] - previousSARDown));
                    if (nextSAR <= closeSerie[i]) // DownTrendBroken
                    {
                        isDownTrend = false;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else // Down Trend continues
                    {
                        previousSARDown = nextSAR;
                        sarSerieResistance[i] = previousSARDown;
                        accelerationFactorDown = Math.Min(accelerationFactorMax, accelerationFactorDown + accelerationFactorStep);
                    }
                }
                else
                {
                    if (highSerie.IsTop(i - 1)) // Downtrend starts
                    {
                        isDownTrend = true;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = previousHigh = previousSARDown = highSerie[i - 1];
                    }
                    else
                    {
                        sarSerieResistance[i] = float.NaN;
                    }
                }
            }
        }
        public void CalculateTOPEMA(int period, float initGap, out FloatSerie emaSerieSupport, out FloatSerie emaSerieResistance, int inputSmoothing)
        {
            float alpha = 2.0f / (float)(period + 1);
            bool isUpTrend = false;
            bool isDownTrend = false;
            emaSerieSupport = new FloatSerie(this.Values.Count(), "TOPEMA.S");
            emaSerieResistance = new FloatSerie(this.Values.Count(), "TOPEMA.R");
            float previousLow = float.NaN;
            float previousHigh = float.NaN;
            float previousEMAUp = float.NaN;
            float previousEMADown = float.NaN;
            emaSerieResistance[0] = emaSerieResistance[1] = float.NaN;
            emaSerieSupport[0] = emaSerieSupport[1] = float.NaN;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);

            for (int i = 2; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    float nextEMA = Math.Max(previousEMAUp, previousEMAUp + alpha * (lowSerie[i] - previousEMAUp));
                    if (nextEMA >= closeSerie[i]) // UpTrendBroken
                    {
                        isUpTrend = false;
                        emaSerieSupport[i] = float.NaN;
                    }
                    else // Up Trend continues
                    {
                        previousEMAUp = nextEMA;
                        emaSerieSupport[i] = previousEMAUp;
                    }
                }
                else
                {
                    if (lowSerie.IsBottom(i - 1)) // Uptrend starts
                    {
                        isUpTrend = true;
                        emaSerieSupport[i] = previousLow = previousEMAUp = lowSerie[i - 1] * (1.0f - initGap);
                    }
                    else
                    {
                        emaSerieSupport[i] = float.NaN;
                    }
                }
                if (isDownTrend)
                {
                    float nextEMA = Math.Min(previousEMADown, previousEMADown + alpha * (highSerie[i] - previousEMADown));
                    if (nextEMA <= closeSerie[i]) // DownTrendBroken
                    {
                        isDownTrend = false;
                        emaSerieResistance[i] = float.NaN;
                    }
                    else // Down Trend continues
                    {
                        previousEMADown = nextEMA;
                        emaSerieResistance[i] = previousEMADown;
                    }
                }
                else
                {
                    if (highSerie.IsTop(i - 1)) // Downtrend starts
                    {
                        isDownTrend = true;
                        emaSerieResistance[i] = previousHigh = previousEMADown = highSerie[i - 1] * (1.0f + initGap);
                    }
                    else
                    {
                        emaSerieResistance[i] = float.NaN;
                    }
                }
            }
        }
        public void CalculateHLSAR(float accelerationFactorStep, float margin, float accelerationFactorMax, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int hlPeriod)
        {
            float accelerationFactorInit = 0;
            float accelerationFactorUp = accelerationFactorInit;
            float accelerationFactorDown = accelerationFactorInit;
            bool isUpTrend = false;
            bool isDownTrend = false;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "TOPSAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "TOPSAR.R");
            float previousLow = float.NaN;
            float previousHigh = float.NaN;
            float previousSARUp = float.NaN;
            float previousSARDown = float.NaN;
            sarSerieResistance[0] = sarSerieResistance[1] = float.NaN;
            sarSerieSupport[0] = sarSerieSupport[1] = float.NaN;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            IStockIndicator hlTrailSR = this.GetIndicator("TRAILHLSR(" + hlPeriod + ")");
            FloatSerie supportSerie = hlTrailSR.Series[0];
            FloatSerie resistanceSerie = hlTrailSR.Series[1];

            for (int i = 2; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    float nextSAR = Math.Max(previousSARUp, previousSARUp + accelerationFactorUp * (lowSerie[i] - previousSARUp));
                    if (nextSAR >= closeSerie[i]) // UpTrendBroken
                    {
                        isUpTrend = false;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else // Up Trend continues
                    {
                        previousSARUp = nextSAR;
                        sarSerieSupport[i] = previousSARUp;
                        accelerationFactorUp = Math.Min(accelerationFactorMax, accelerationFactorUp + accelerationFactorStep);
                    }
                }
                else
                {
                    if (float.IsNaN(supportSerie[i - 1]) && !float.IsNaN(supportSerie[i])) // Uptrend starts
                    {
                        float low = lowSerie[i - 1];
                        int index = i - 1;
                        while (--index > 0 && float.IsNaN(supportSerie[index]))
                        {
                            if (lowSerie[index] < low)
                            {
                                low = lowSerie[index];
                            }
                        }
                        isUpTrend = true;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = previousLow = previousSARUp = low * (1.0f - margin);
                    }
                    else
                    {
                        sarSerieSupport[i] = float.NaN;
                    }
                }
                if (isDownTrend)
                {
                    float nextSAR = Math.Min(previousSARDown, previousSARDown + accelerationFactorDown * (highSerie[i] - previousSARDown));
                    if (nextSAR <= closeSerie[i]) // DownTrendBroken
                    {
                        isDownTrend = false;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else // Down Trend continues
                    {
                        previousSARDown = nextSAR;
                        sarSerieResistance[i] = previousSARDown;
                        accelerationFactorDown = Math.Min(accelerationFactorMax, accelerationFactorDown + accelerationFactorStep);
                    }
                }
                else
                {
                    if (float.IsNaN(resistanceSerie[i - 1]) && !float.IsNaN(resistanceSerie[i])) // Down  trend starts
                    {
                        float high = highSerie[i - 1];
                        int index = i - 1;
                        while (--index > 0 && float.IsNaN(resistanceSerie[index]))
                        {
                            if (highSerie[index] > high)
                            {
                                high = highSerie[index];
                            }
                        }
                        isDownTrend = true;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = previousHigh = previousSARDown = high * (1.0f + margin);
                    }
                    else
                    {
                        sarSerieResistance[i] = float.NaN;
                    }
                }
            }
        }
        public void CalculateSAR(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum;
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }
        public void CalculateSSAR(float accelerationFactor, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SSAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SSAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    var nextSAR = previousSAR * (1f + accelerationFactor);
                    if (nextSAR >= closeSerie[i]) // Up trend Broken
                    {
                        isUpTrend = false;
                        previousSAR = previousExtremum;
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            previousExtremum = highSerie[i];
                        }
                        previousSAR = nextSAR;
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    var nextSAR = previousSAR * (1f - accelerationFactor);
                    if (nextSAR <= closeSerie[i]) // Down  trend Broken
                    {
                        isUpTrend = true;
                        previousSAR = previousExtremum;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR = nextSAR;
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }

        public void CalculateSARHL(int HLPeriod, float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, float margin, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance)
        {
            IStockEvent trailHRSR = this.GetTrailStop("TRAILHL(" + HLPeriod + ")");
            BoolSerie upBreakSerie = trailHRSR.Events[2];
            BoolSerie downBreakSerie = trailHRSR.Events[3];

            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit * (1f + (((previousExtremum - closeSerie[i]) / closeSerie[i]) * 100f));
                        previousSAR = previousExtremum * (1.0f + margin);
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        // if (upBreakSerie[i]) accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor *2f);
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit * (1f + (((closeSerie[i] - previousExtremum) / closeSerie[i]) * 100f));
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum * (1.0f - margin); ;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        // if (downBreakSerie[i]) accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor * 2f);
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }

        public void CalculateSAR2(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, FloatSerie sarSerieSupport1, FloatSerie sarSerieResistance1, out FloatSerie sarSerieSupport2, out FloatSerie sarSerieResistance2)
        {
            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport2 = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance2 = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance2[0] = float.NaN;
            sarSerieSupport2[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit;
                        if (float.IsNaN(sarSerieResistance1[i]))
                        {
                            previousSAR = previousExtremum;
                        }
                        else
                        {
                            previousSAR = sarSerieResistance1[i];
                        }
                        sarSerieResistance2[i] = previousSAR;
                        sarSerieSupport2[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance2[i] = float.NaN;
                        sarSerieSupport2[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit;

                        if (float.IsNaN(sarSerieSupport1[i]))
                        {
                            previousSAR = previousExtremum;
                        }
                        else
                        {
                            previousSAR = sarSerieSupport1[i];
                        }
                        sarSerieSupport2[i] = previousSAR;
                        sarSerieResistance2[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport2[i] = float.NaN;
                        sarSerieResistance2[i] = previousSAR;
                    }
                }
            }
        }
        public void CalculateSAREX(float accelerationFactorInit, float accelerationFactorStep, out FloatSerie sarexFollowerSupport, out FloatSerie sarexFollowerResistance)
        {
            sarexFollowerSupport = new FloatSerie(this.Values.Count, "SAREX.S");
            sarexFollowerResistance = new FloatSerie(this.Values.Count, "SAREX.R");
            int i = 0;
            float currentSarex = this.Values.First().CLOSE;
            sarexFollowerResistance.Values[0] = currentSarex;
            float previousExtremum = this.Values.First().CLOSE;
            bool upTrend = true;

            float accelerationFactor = accelerationFactorInit;

            FloatSerie referenceEMASerie = this.GetIndicator("EMA(6)").Series[0];
            FloatSerie referenceEMATrendSerie = referenceEMASerie.CalculateRelativeTrend().CalculateEMA(3);
            float sarexToEMAGap = 0.0f;
            float fastSwingAccelerationFactor = 0.0f;
            float fastSwingFactor = accelerationFactorStep;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            StockDailyValue yesterValue;
            StockDailyValue yesterYesterValue = null;
            while (++i <= this.Values.Count)
            {
                // Check if new extremum is reached
                yesterValue = this.ValueArray[i - 1];
                if (currentSarex >= yesterValue.HIGH)
                {
                    if (upTrend == true)
                    {
                        accelerationFactor = accelerationFactorInit;
                        upTrend = false;
                    }
                    if (yesterValue.LOW < previousExtremum)
                    {
                        accelerationFactor = accelerationFactor + accelerationFactorStep;
                        previousExtremum = yesterValue.LOW;
                    }
                }
                if (currentSarex <= yesterValue.LOW)
                {
                    if (upTrend == false)
                    {
                        accelerationFactor = accelerationFactorInit;
                        upTrend = true;
                    }
                    if (yesterValue.HIGH > previousExtremum)
                    {
                        accelerationFactor = accelerationFactor + accelerationFactorStep;
                        previousExtremum = yesterValue.HIGH;
                    }
                }

                // Gap management
                if (yesterYesterValue != null)
                {
                    if (upTrend)
                    {
                        if (yesterYesterValue.HIGH < yesterValue.LOW)
                        {
                            currentSarex += yesterValue.LOW - yesterYesterValue.HIGH;
                        }
                    }
                    else
                    {
                        if (yesterYesterValue.LOW > yesterValue.HIGH)
                        {
                            currentSarex -= yesterYesterValue.LOW - yesterValue.HIGH;
                        }
                    }
                }

                // Fast swing management
                sarexToEMAGap = referenceEMASerie.Values[i - 1] - currentSarex;
                fastSwingAccelerationFactor = Math.Max(Math.Abs(referenceEMATrendSerie.Values[i - 1] * fastSwingFactor), fastSwingAccelerationFactor);
                currentSarex += accelerationFactor * (previousExtremum - currentSarex) + fastSwingAccelerationFactor * sarexToEMAGap;

                if (currentSarex >= yesterValue.CLOSE)
                {
                    sarexFollowerResistance[i - 1] = currentSarex;
                    sarexFollowerSupport[i - 1] = float.NaN;
                }
                else
                {
                    sarexFollowerResistance[i - 1] = float.NaN;
                    sarexFollowerSupport[i - 1] = currentSarex;
                }

                // Check if the sarex has broken
                if (i < this.Values.Count && (currentSarex < highSerie[i] && currentSarex > lowSerie[i]))
                {   // The SAREX has broken
                    previousExtremum = this.ValueArray[i].CLOSE;
                }
                yesterYesterValue = yesterValue;
            }
        }

        public FloatSerie CalculateCCI(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie maSerie = this.GetIndicator("MA(" + period + ")").Series[0];

            FloatSerie diff = closeSerie - maSerie;

            FloatSerie meanDeviation = new FloatSerie(closeSerie.Count);
            FloatSerie cciSerie = new FloatSerie(closeSerie.Count, "CCI");

            float sum = 0;
            for (int i = 1; i < period && i < closeSerie.Count; i++)
            {
                sum += Math.Abs(diff[i]);
                cciSerie[i] = diff[i] / (0.015f * sum / i);
            }
            float K = 0.015f / period;
            for (int i = period; i < closeSerie.Count; i++)
            {
                sum += Math.Abs(diff[i]) - Math.Abs(diff[i - period]);
                cciSerie[i] = diff[i] / (K * sum);
            }

            return cciSerie;
        }
        /// <summary>
        /// Generates automated trends lines and fill the event array.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="leftStrength"></param>
        /// <param name="rightStrength"></param>
        /// <param name="nbPivots"></param>
        /// <param name="events"> 
        /// [0] = SupportDetected
        /// [1] = SupportBroken
        /// [2] = ResistanceDetected
        /// [3] = ResistanceBroken
        /// [4] = UpTrend => No resistance above
        /// [5] = DownTrend => no support below
        /// </param>
        /// 
        public enum TLEvent
        {
            SupportDetected = 0,
            SupportBroken,
            ResistanceDetected,
            ResistanceBroken,
            UpTrend,
            DownTrend
        }
        public void generateAutomaticTrendLines(int startIndex, int endIndex, int leftStrength, int rightStrength, int nbPivots, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockPaintBar pivots = this.GetPaintBar("PIVOT(" + leftStrength + "," + rightStrength + ")");
                BoolSerie highPivots = pivots.Events[0];
                BoolSerie lowPivots = pivots.Events[1];

                Queue<int> highPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<int> lowPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<float> highPivotValueQueue = new Queue<float>(nbPivots);
                Queue<float> lowPivotValueQueue = new Queue<float>(nbPivots);

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                float latestHighPivotValue;
                float latestLowPivotValue;

                Line2DBase latestResistanceLine = null;
                Line2DBase latestSupportLine = null;

                List<Line2DBase> supportList = new List<Line2DBase>();
                List<Line2DBase> resistanceList = new List<Line2DBase>();

                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                int j, pivotIndex;
                bool brokenResistance = false;
                bool brokenSupport = false;
                for (int i = startIndex + leftStrength + rightStrength; i <= endIndex; i++)
                {
                    // Check for broken lines
                    if (latestResistanceLine != null)
                    {
                        if (closeSerie[i] > latestResistanceLine.ValueAtX(i))
                        {
                            // Down trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                            resistanceList.Remove(latestResistanceLine);
                            latestResistanceLine = null;
                            events[(int)TLEvent.ResistanceBroken][i] = true;
                            brokenResistance = true;
                            brokenSupport = false;
                        }
                        else
                        {
                            brokenResistance = false;
                        }
                    }
                    if (latestSupportLine != null)
                    {
                        if (closeSerie[i] < latestSupportLine.ValueAtX(i))
                        {
                            // Up trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(i, true));
                            supportList.Remove(latestSupportLine);
                            latestSupportLine = null;
                            events[(int)TLEvent.SupportBroken][i] = true;
                            brokenSupport = true;
                            brokenResistance = false;
                        }
                        else
                        {
                            brokenSupport = false;
                        }
                    }

                    pivotIndex = i - rightStrength;
                    if (highPivots[pivotIndex])
                    {
                        latestHighPivotValue = highSerie[pivotIndex];

                        if (highPivotIndexQueue.Count >= nbPivots)
                        {
                            highPivotIndexQueue.Dequeue();
                            highPivotValueQueue.Dequeue();
                        }
                        highPivotIndexQueue.Enqueue(pivotIndex);
                        highPivotValueQueue.Enqueue(latestHighPivotValue);

                        bool highestPivotFound = false;
                        for (j = highPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestHighPivotValue < highPivotValueQueue.ElementAt(j))
                            {
                                highestPivotFound = true;
                                break;
                            }
                        }
                        if (highestPivotFound)
                        {
                            if (latestResistanceLine != null)
                            {
                                // New line has to be drawn
                                this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                                resistanceList.Remove(latestResistanceLine);
                            }

                            latestResistanceLine =
                                new HalfLine2D(
                                    new PointF(highPivotIndexQueue.ElementAt(j), highPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestHighPivotValue),
                                    Pens.Green);
                            resistanceList.Add(latestResistanceLine);

                            events[(int)TLEvent.ResistanceDetected][i] = true;
                            brokenResistance = false;
                        }
                    }
                    else if (lowPivots[pivotIndex])
                    {
                        latestLowPivotValue = lowSerie[pivotIndex];

                        if (lowPivotIndexQueue.Count >= nbPivots)
                        {
                            lowPivotIndexQueue.Dequeue();
                            lowPivotValueQueue.Dequeue();
                        }
                        lowPivotIndexQueue.Enqueue(pivotIndex);
                        lowPivotValueQueue.Enqueue(lowSerie[pivotIndex]);

                        bool lowestPivotFound = false;
                        for (j = lowPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestLowPivotValue > lowPivotValueQueue.ElementAt(j))
                            {
                                lowestPivotFound = true;
                                break;
                            }
                        }
                        if (lowestPivotFound)
                        {
                            if (latestSupportLine != null)
                            {
                                // New line has to be drawn
                                this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(pivotIndex, true));
                                supportList.Remove(latestSupportLine);
                            }

                            latestSupportLine =
                                new HalfLine2D(
                                    new PointF(lowPivotIndexQueue.ElementAt(j), lowPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestLowPivotValue),
                                    Pens.Red);
                            supportList.Add(latestSupportLine);

                            events[(int)TLEvent.SupportDetected][i] = true;

                            brokenSupport = false;
                        }
                    }

                    // Detecting upTrend events
                    bool upTrend = resistanceList.Count == 0 && supportList.Count != 0;
                    foreach (Line2DBase line in resistanceList)
                    {
                        if (closeSerie[i] > line.ValueAtX(i))
                        {
                            upTrend = false;
                            break;
                        }
                    }

                    // Detecting downTrend events
                    bool downTrend = supportList.Count == 0 && resistanceList.Count != 0;
                    foreach (Line2DBase line in supportList)
                    {
                        if (closeSerie[i] < line.ValueAtX(i))
                        {
                            downTrend = false;
                            break;
                        }
                    }
                    if (!(downTrend || upTrend))
                    {
                        events[(int)TLEvent.UpTrend][i] = brokenResistance;
                        events[(int)TLEvent.DownTrend][i] = brokenSupport;
                    }
                    else
                    {
                        events[(int)TLEvent.UpTrend][i] = upTrend;
                        events[(int)TLEvent.DownTrend][i] = downTrend;
                    }
                }
                if (latestSupportLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine);
                }
                if (latestResistanceLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine);
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
        public void generateAutomaticHL2TrendLines(int startIndex, int endIndex, int period, int nbPivots, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockTrailStop pivots = this.GetTrailStop("TRAILHL(" + period + ")");
                BoolSerie brokenDown = pivots.Events[3];
                BoolSerie brokenUp = pivots.Events[2];

                Queue<int> highPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<int> lowPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<float> highPivotValueQueue = new Queue<float>(nbPivots);
                Queue<float> lowPivotValueQueue = new Queue<float>(nbPivots);

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                float latestHighPivotValue = 0f;
                float highPivotValue;
                float lowPivotValue;
                float latestLowPivotValue = 0f;

                Line2DBase latestResistanceLine = null;
                Line2DBase latestSupportLine = null;

                List<Line2DBase> supportList = new List<Line2DBase>();
                List<Line2DBase> resistanceList = new List<Line2DBase>();

                SortedDictionary<int, float> highs = new SortedDictionary<int, float>();
                SortedDictionary<int, float> lows = new SortedDictionary<int, float>();

                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                int pivotIndex;
                int lastBreakDownIndex = -1;
                int lastBreakUpIndex = -1;
                bool brokenResistance = false;
                bool brokenSupport = false;
                Pen greenLargePen = new Pen(Color.Green, 2);
                Pen redLargePen = new Pen(Color.Red, 2);
                Pen greenThinPen = new Pen(Color.Green, 1);
                Pen redThinPen = new Pen(Color.Red, 1);

                for (int i = startIndex + period; i <= endIndex; i++)
                {
                    if (brokenDown[i])
                    {
                        pivotIndex = highSerie.FindMaxIndex(i - period - 2, i - 1);
                        highPivotValue = highSerie[pivotIndex];

                        List<int> indexToRemove = new List<int>();
                        float maxHigh = float.MinValue;
                        foreach (var pair in highs)
                        {
                            maxHigh = Math.Max(maxHigh, highPivotValue);
                            if (pair.Value < highPivotValue) indexToRemove.Add(i);
                        }
                        indexToRemove.ForEach(ind => highs.Remove(ind));


                        if (lastBreakDownIndex == -1 || highPivotValue >= latestHighPivotValue) // Need a new line
                        {
                            lastBreakDownIndex = pivotIndex;
                            latestHighPivotValue = highPivotValue;
                        }
                        else
                        {
                            latestResistanceLine = new HalfLine2D(
                                   new PointF(lastBreakDownIndex, latestHighPivotValue),
                                   new PointF(pivotIndex, highPivotValue), greenLargePen);

                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(latestResistanceLine.Point1, 3));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(latestResistanceLine.Point2, 3));

                            resistanceList.Add(latestResistanceLine);

                            lastBreakDownIndex = pivotIndex;
                            latestHighPivotValue = highPivotValue;

                            events[(int)TLEvent.ResistanceDetected][i] = true;
                            brokenResistance = false;
                        }
                    }
                    else
                    {
                        events[(int)TLEvent.UpTrend][i] = resistanceList.Count == 0;

                        var closePoint = new PointF(i, closeSerie[i]);
                        var brokenLines = resistanceList.Where(r => !r.IsAbovePoint(closePoint)).ToList();
                        events[(int)TLEvent.ResistanceBroken][i] = brokenLines.Count > 0;
                        foreach (var line in brokenLines)
                        {
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(line.Cut(i, true));
                            resistanceList.Remove(line);
                        }
                    }

                    if (brokenUp[i])
                    {
                        pivotIndex = lowSerie.FindMinIndex(i - period - 2, i - 1);
                        lowPivotValue = lowSerie[pivotIndex];

                        if (lastBreakUpIndex == -1 || lowPivotValue <= latestLowPivotValue) // Need a new line
                        {
                            lastBreakUpIndex = pivotIndex;
                            latestLowPivotValue = lowPivotValue;
                        }
                        else
                        {
                            latestSupportLine = new HalfLine2D(
                                   new PointF(lastBreakUpIndex, latestLowPivotValue),
                                   new PointF(pivotIndex, lowPivotValue), redLargePen);

                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(latestSupportLine.Point1, 3));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(latestSupportLine.Point2, 3));

                            supportList.Add(latestSupportLine);

                            lastBreakUpIndex = pivotIndex;
                            latestLowPivotValue = lowPivotValue;

                            events[(int)TLEvent.SupportDetected][i] = true;
                            brokenSupport = false;
                        }
                    }
                    else
                    {
                        events[(int)TLEvent.DownTrend][i] = supportList.Count == 0;

                        var closePoint = new PointF(i, closeSerie[i]);
                        var brokenLines = supportList.Where(r => r.IsAbovePoint(closePoint)).ToList();
                        events[(int)TLEvent.SupportBroken][i] = brokenLines.Count > 0;
                        foreach (var line in brokenLines)
                        {
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(line.Cut(i, true));
                            supportList.Remove(line);
                        }
                    }

                    // Detecting upTrend events
                    bool upTrend = resistanceList.Count == 0 && supportList.Count != 0;
                    foreach (Line2DBase line in resistanceList)
                    {
                        if (closeSerie[i] > line.ValueAtX(i))
                        {
                            upTrend = false;
                            break;
                        }
                    }

                    // Detecting downTrend events
                    bool downTrend = supportList.Count == 0 && resistanceList.Count != 0;
                    foreach (Line2DBase line in supportList)
                    {
                        if (closeSerie[i] < line.ValueAtX(i))
                        {
                            downTrend = false;
                            break;
                        }
                    }
                    if (!(downTrend || upTrend))
                    {
                        events[(int)TLEvent.UpTrend][i] = brokenResistance;
                        events[(int)TLEvent.DownTrend][i] = brokenSupport;
                    }
                    else
                    {
                        events[(int)TLEvent.UpTrend][i] = upTrend;
                        events[(int)TLEvent.DownTrend][i] = downTrend;
                    }
                }
                foreach (var line in resistanceList)
                {
                    line.Pen = greenThinPen;
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(line);
                }
                foreach (var line in supportList)
                {
                    line.Pen = redThinPen;
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(line);
                }
                //if (latestSupportLine != null)
                //{
                //    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine);
                //}
                //if (latestResistanceLine != null)
                //{
                //    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine);
                //}
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        public void generateAutomaticHLTrendLines(int startIndex, int endIndex, int period, int nbPivots, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockTrailStop pivots = this.GetTrailStop("TRAILHL(" + period + ")");
                BoolSerie brokenDown = pivots.Events[3];
                BoolSerie brokenUp = pivots.Events[2];

                Queue<int> highPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<int> lowPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<float> highPivotValueQueue = new Queue<float>(nbPivots);
                Queue<float> lowPivotValueQueue = new Queue<float>(nbPivots);

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                float latestHighPivotValue;
                float latestLowPivotValue;

                Line2DBase latestResistanceLine = null;
                Line2DBase latestSupportLine = null;

                List<Line2DBase> supportList = new List<Line2DBase>();
                List<Line2DBase> resistanceList = new List<Line2DBase>();

                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                int j, pivotIndex;
                int lastBreakIndex = 0;
                bool brokenResistance = false;
                bool brokenSupport = false;
                for (int i = startIndex + period; i <= endIndex; i++)
                {
                    #region Check for broken lines
                    if (latestResistanceLine != null)
                    {
                        if (closeSerie[i] > latestResistanceLine.ValueAtX(i))
                        {
                            // Down trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                            resistanceList.Remove(latestResistanceLine);
                            latestResistanceLine = null;
                            events[(int)TLEvent.ResistanceBroken][i] = true;
                            brokenResistance = true;
                            brokenSupport = false;
                        }
                        else
                        {
                            brokenResistance = false;
                        }
                    }
                    if (latestSupportLine != null)
                    {
                        if (closeSerie[i] < latestSupportLine.ValueAtX(i))
                        {
                            // Up trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(i, true));
                            supportList.Remove(latestSupportLine);
                            latestSupportLine = null;
                            events[(int)TLEvent.SupportBroken][i] = true;
                            brokenSupport = true;
                            brokenResistance = false;
                        }
                        else
                        {
                            brokenSupport = false;
                        }
                    }
                    #endregion
                    if (brokenDown[i])
                    {
                        pivotIndex = highSerie.FindMaxIndex(lastBreakIndex, i - 1);
                        latestHighPivotValue = highSerie[pivotIndex];
                        lastBreakIndex = i;

                        if (highPivotIndexQueue.Count >= nbPivots)
                        {
                            highPivotIndexQueue.Dequeue();
                            highPivotValueQueue.Dequeue();
                        }
                        highPivotIndexQueue.Enqueue(pivotIndex);
                        highPivotValueQueue.Enqueue(latestHighPivotValue);

                        bool highestPivotFound = false;
                        for (j = highPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestHighPivotValue < highPivotValueQueue.ElementAt(j))
                            {
                                highestPivotFound = true;
                                break;
                            }
                        }
                        if (highestPivotFound)
                        {
                            if (latestResistanceLine != null)
                            {
                                // New line has to be drawn
                                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                                //resistanceList.Remove(latestResistanceLine);
                            }

                            latestResistanceLine =
                                new HalfLine2D(
                                    new PointF(highPivotIndexQueue.ElementAt(j), highPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestHighPivotValue),
                                    Pens.Green);
                            resistanceList.Add(latestResistanceLine);

                            events[(int)TLEvent.ResistanceDetected][i] = true;
                            brokenResistance = false;
                        }
                    }
                    else if (brokenUp[i])
                    {
                        pivotIndex = lowSerie.FindMinIndex(lastBreakIndex, i - 1);
                        latestLowPivotValue = lowSerie[pivotIndex];
                        lastBreakIndex = i;

                        if (lowPivotIndexQueue.Count >= nbPivots)
                        {
                            lowPivotIndexQueue.Dequeue();
                            lowPivotValueQueue.Dequeue();
                        }
                        lowPivotIndexQueue.Enqueue(pivotIndex);
                        lowPivotValueQueue.Enqueue(lowSerie[pivotIndex]);

                        bool lowestPivotFound = false;
                        for (j = lowPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestLowPivotValue > lowPivotValueQueue.ElementAt(j))
                            {
                                lowestPivotFound = true;
                                break;
                            }
                        }
                        if (lowestPivotFound)
                        {
                            if (latestSupportLine != null)
                            {
                                // New line has to be drawn
                                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(pivotIndex, true));
                                //supportList.Remove(latestSupportLine);
                            }

                            latestSupportLine =
                                new HalfLine2D(
                                    new PointF(lowPivotIndexQueue.ElementAt(j), lowPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestLowPivotValue),
                                    Pens.Red);
                            supportList.Add(latestSupportLine);

                            events[(int)TLEvent.SupportDetected][i] = true;

                            brokenSupport = false;
                        }
                    }

                    // Detecting upTrend events
                    bool upTrend = resistanceList.Count == 0 && supportList.Count != 0;
                    foreach (Line2DBase line in resistanceList)
                    {
                        if (closeSerie[i] > line.ValueAtX(i))
                        {
                            upTrend = false;
                            break;
                        }
                    }

                    // Detecting downTrend events
                    bool downTrend = supportList.Count == 0 && resistanceList.Count != 0;
                    foreach (Line2DBase line in supportList)
                    {
                        if (closeSerie[i] < line.ValueAtX(i))
                        {
                            downTrend = false;
                            break;
                        }
                    }
                    if (!(downTrend || upTrend))
                    {
                        events[(int)TLEvent.UpTrend][i] = brokenResistance;
                        events[(int)TLEvent.DownTrend][i] = brokenSupport;
                    }
                    else
                    {
                        events[(int)TLEvent.UpTrend][i] = upTrend;
                        events[(int)TLEvent.DownTrend][i] = downTrend;
                    }
                }
                if (latestSupportLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine);
                }
                if (latestResistanceLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine);
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        public enum DowEvent
        {
            UpTrend,
            DownTrend,
            Ranging,
            //StartUpTrend, // @@@@ Not Implemented yet.
            //StartDownTrend,
            //StartRanging

        }
        public List<PointF> generateZigzagPoints(int startIndex, int endIndex, int hlPeriod)
        {
            var points = new List<PointF>();
            try
            {
                IStockIndicator hlTrailSR = this.GetIndicator("TRAILHLSR(" + hlPeriod + ")");

                BoolSerie supportDetected = hlTrailSR.Events[0];
                BoolSerie resistanceDetected = hlTrailSR.Events[1];

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                FloatSerie supportSerie = hlTrailSR.Series[0];
                FloatSerie resistanceSerie = hlTrailSR.Series[1];

                Segment2D latestLine = new Segment2D(startIndex - 1, highSerie[startIndex], startIndex, highSerie[startIndex]);

                Segment2D newLine;

                // Start Creating lines
                int j;
                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (supportDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestLine.Point2.X && lowSerie[j] != supportSerie[i]; j--) ;
                        if (latestLine.Point2.X != j)
                        {
                            newLine = new Segment2D(latestLine.Point2.X, latestLine.Point2.Y, j, lowSerie[j]);

                            points.Add(newLine.Point2);

                            latestLine = newLine;
                        }
                    }
                    if (resistanceDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestLine.Point2.X && highSerie[j] != resistanceSerie[i]; j--) ;
                        if (latestLine.Point2.X != j)
                        {
                            newLine = new Segment2D(latestLine.Point2.X, latestLine.Point2.Y, j, highSerie[j]);

                            points.Add(newLine.Point2);

                            latestLine = newLine;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
            }
            return points;
        }

        public void generateZigzagLines(int startIndex, int endIndex, int hlPeriod, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockIndicator hlTrailSR = this.GetIndicator("TRAILHLSR(" + hlPeriod + ")");

                BoolSerie upTrendSerie = events[0];
                BoolSerie downTrendSerie = events[1];
                BoolSerie rangingSerie = events[2];

                BoolSerie supportDetected = hlTrailSR.Events[0];
                BoolSerie resistanceDetected = hlTrailSR.Events[1];

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                FloatSerie supportSerie = hlTrailSR.Series[0];
                FloatSerie resistanceSerie = hlTrailSR.Series[1];

                Segment2D latestLine = new Segment2D(startIndex - 1, highSerie[startIndex], startIndex,
                    highSerie[startIndex]);

                Segment2D newLine;
                Bullet2D bullet;

                DowEvent trendStatus = DowEvent.Ranging;

                upTrendSerie[0] = trendStatus == DowEvent.UpTrend;
                downTrendSerie[0] = trendStatus == DowEvent.DownTrend;
                rangingSerie[0] = trendStatus == DowEvent.Ranging;

                // Do drawing Items cleanup
                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                // Start Creating lines
                int j;
                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (supportDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestLine.Point2.X && lowSerie[j] != supportSerie[i]; j--) ;
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                            newLine = new Segment2D(latestLine.Point2.X, latestLine.Point2.Y,
                                j, lowSerie[j]));
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(bullet = new Bullet2D(newLine.Point2, 3));

                        latestLine = newLine;

                        // Set trend Status
                        if (latestLine.Point2.Y >= latestLine.Point1.Y) // Support Higher Low
                        {
                            trendStatus = latestLine.Point2.Y >= latestLine.Point1.Y
                                ? DowEvent.UpTrend
                                : DowEvent.Ranging;
                        }
                        else // Support Lower Low
                        {
                            trendStatus = latestLine.Point2.Y >= latestLine.Point1.Y
                                ? DowEvent.Ranging
                                : DowEvent.DownTrend;
                        }

                        // Set line color
                        if (newLine.Point2.Y >= newLine.Point1.Y)
                        {
                            newLine.Pen = Pens.Green;
                            bullet.Pen = Pens.Green;
                        }
                        else
                        {
                            newLine.Pen = Pens.Red;
                            bullet.Pen = Pens.Red;
                        }
                    }
                    else
                    {
                        if (resistanceDetected[i])
                        {
                            // Find previous Low value
                            for (j = i;
                                j > latestLine.Point2.X && highSerie[j] != resistanceSerie[i];
                                j--) ;
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                                newLine =
                                    new Segment2D(latestLine.Point2.X, latestLine.Point2.Y,
                                        j, highSerie[j]));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                                bullet = new Bullet2D(newLine.Point2, 3));

                            latestLine = newLine;
                            // Set trend Status
                            if (latestLine.Point2.Y >= latestLine.Point1.Y) // Support Higher Low
                            {
                                trendStatus = latestLine.Point2.Y >= latestLine.Point1.Y
                                    ? DowEvent.UpTrend
                                    : DowEvent.Ranging;
                            }
                            else // Support Lower Low
                            {
                                trendStatus = latestLine.Point2.Y >= latestLine.Point1.Y
                                    ? DowEvent.Ranging
                                    : DowEvent.DownTrend;
                            }

                            // Set line color
                            if (newLine.Point2.Y >= newLine.Point1.Y)
                            {
                                newLine.Pen = Pens.Green;
                                bullet.Pen = Pens.Green;
                            }
                            else
                            {
                                newLine.Pen = Pens.Red;
                                bullet.Pen = Pens.Red;
                            }
                        }
                        else
                        {
                            if (closeSerie[i] < latestLine.Point2.Y) // Broken latest support
                            {
                                trendStatus = DowEvent.DownTrend;
                            }
                            else if (closeSerie[i] > latestLine.Point2.Y) // Broken latest resistance
                            {
                                trendStatus = DowEvent.UpTrend;
                            }
                        }
                    }
                    upTrendSerie[i] = trendStatus == DowEvent.UpTrend;
                    downTrendSerie[i] = trendStatus == DowEvent.DownTrend;
                    rangingSerie[i] = trendStatus == DowEvent.Ranging;
                }

                // Add end lines
                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(new HalfLine2D(latestResistanceLine.Point2, 1, 0));
                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(new HalfLine2D(latestSupportLine.Point2, 1, 0));

            }
            catch (System.Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        public void generateDowTrendLines(int startIndex, int endIndex, int hlPeriod, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockIndicator hlTrailSR = this.GetIndicator("TRAILHLSR(" + hlPeriod + ")");

                BoolSerie upTrendSerie = events[0];
                BoolSerie downTrendSerie = events[1];
                BoolSerie rangingSerie = events[2];

                BoolSerie supportDetected = hlTrailSR.Events[0];
                BoolSerie resistanceDetected = hlTrailSR.Events[1];

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                FloatSerie supportSerie = hlTrailSR.Series[0];
                FloatSerie resistanceSerie = hlTrailSR.Series[1];

                Segment2D latestResistanceLine = new Segment2D(startIndex - 1, highSerie[startIndex], startIndex, highSerie[startIndex]);
                Segment2D latestSupportLine = new Segment2D(startIndex - 1, lowSerie[startIndex], startIndex, lowSerie[startIndex]);

                Segment2D newLine;
                Bullet2D bullet;

                DowEvent trendStatus = DowEvent.Ranging;

                upTrendSerie[0] = trendStatus == DowEvent.UpTrend;
                downTrendSerie[0] = trendStatus == DowEvent.DownTrend;
                rangingSerie[0] = trendStatus == DowEvent.Ranging;

                // Do drawing Items cleanup
                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                // Start Creating lines
                int j;
                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (supportDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestSupportLine.Point2.X && lowSerie[j] != supportSerie[i]; j--) ;
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(newLine = new Segment2D(latestSupportLine.Point2.X, latestSupportLine.Point2.Y, j, lowSerie[j]));
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(bullet = new Bullet2D(newLine.Point2, 3));

                        latestSupportLine = newLine;

                        // Set trend Status
                        if (latestSupportLine.Point2.Y >= latestSupportLine.Point1.Y) // Support Higher Low
                        {
                            trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                ? DowEvent.UpTrend
                                : DowEvent.Ranging;
                        }
                        else // Support Lower Low
                        {
                            trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                ? ((closeSerie[i] > latestSupportLine.Point1.Y) ? DowEvent.Ranging : trendStatus) // Don't change trend in case of PB to END of trend.
                                : DowEvent.DownTrend;
                        }

                        // Set line color
                        if (newLine.Point2.Y >= newLine.Point1.Y)
                        {
                            newLine.Pen = Pens.Green;
                            bullet.Pen = Pens.Green;
                        }
                        else
                        {
                            newLine.Pen = Pens.Red;
                            bullet.Pen = Pens.Red;
                        }
                    }
                    else
                    {
                        if (resistanceDetected[i])
                        {
                            // Find previous Low value
                            for (j = i; j > latestResistanceLine.Point2.X && highSerie[j] != resistanceSerie[i]; j--) ;
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(newLine = new Segment2D(latestResistanceLine.Point2.X, latestResistanceLine.Point2.Y, j, highSerie[j]));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(bullet = new Bullet2D(newLine.Point2, 3));

                            latestResistanceLine = newLine;
                            // Set trend Status
                            if (latestSupportLine.Point2.Y >= latestSupportLine.Point1.Y) // Support Higher Low
                            {
                                trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                    ? DowEvent.UpTrend
                                    : DowEvent.Ranging;
                            }
                            else // Support Lower Low
                            {
                                trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                    ? ((closeSerie[i] < latestResistanceLine.Point1.Y) ? DowEvent.Ranging : trendStatus) // Don't change trend in case of PB to END of trend.
                                    : DowEvent.DownTrend;
                            }

                            // Set line color
                            if (newLine.Point2.Y >= newLine.Point1.Y)
                            {
                                newLine.Pen = Pens.Green;
                                bullet.Pen = Pens.Green;
                            }
                            else
                            {
                                newLine.Pen = Pens.Red;
                                bullet.Pen = Pens.Red;
                            }
                        }
                        else
                        {
                            if (closeSerie[i] < latestSupportLine.Point2.Y) // Broken latest support
                            {
                                trendStatus = DowEvent.DownTrend;
                            }
                            else if (closeSerie[i] > latestResistanceLine.Point2.Y) // Broken latest resistance
                            {
                                trendStatus = DowEvent.UpTrend;
                            }
                        }
                    }
                    upTrendSerie[i] = trendStatus == DowEvent.UpTrend;
                    downTrendSerie[i] = trendStatus == DowEvent.DownTrend;
                    rangingSerie[i] = trendStatus == DowEvent.Ranging;
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        #endregion

        public StockSerie.Trend[] GenerateMultiTimeFrameTrendSummary(List<string> indicators, List<StockBarDuration> durations)
        {
            StockBarDuration currentDuration = this.BarDuration;

            List<Trend> resultTrends = new List<Trend>();

            foreach (string indicatorName in indicators)
            {
                foreach (StockBarDuration duration in durations)
                {
                    this.BarDuration = duration;
                    if (this.IsInitialised)
                    {
                        IStockUpDownState stockState = this.GetViewableItem(indicatorName) as IStockUpDownState;
                        if (stockState != null)
                        {
                            resultTrends.Add(stockState.UpDownState[this.LastCompleteIndex]);
                        }
                        else
                        {
                            resultTrends.Add(Trend.NoTrend);
                        }
                    }
                    else
                    {
                        foreach (string eventName in indicators)
                        {
                            resultTrends.Add(Trend.NoTrend);
                        }
                    }
                }
            }

            currentDuration = this.BarDuration = currentDuration;

            return resultTrends.ToArray();
        }

        public bool BelongsToGroup(Groups group)
        {
            if (this.StockAnalysis.Excluded) return false;
            switch (group)
            {
                case Groups.ALL:
                    return true;
                case Groups.CAC40:
                    return this.DataProvider == StockDataProvider.ABC && ABCDataProvider.BelongsToCAC40(this);
                case Groups.SRD:
                    return this.DataProvider == StockDataProvider.ABC && ABCDataProvider.BelongsToSRD(this);
                case Groups.SRD_LO:
                    return this.DataProvider == StockDataProvider.ABC && ABCDataProvider.BelongsToSRD_LO(this);
                case Groups.EURO_A:
                    return (this.StockGroup == Groups.EURO_A);
                case Groups.EURO_B:
                    return (this.StockGroup == Groups.EURO_B);
                case Groups.EURO_A_B:
                    return (this.StockGroup == Groups.EURO_A) || (this.StockGroup == Groups.EURO_B);
                case Groups.EURO_C:
                    return (this.StockGroup == Groups.EURO_C);
                case Groups.EURO_A_B_C:
                    return (this.StockGroup == Groups.EURO_A) || (this.StockGroup == Groups.EURO_B) || (this.StockGroup == Groups.EURO_C);
                case Groups.ALTERNEXT:
                    return (this.StockGroup == Groups.ALTERNEXT);
                case Groups.CACALL:
                    return (this.StockGroup == Groups.EURO_A) || (this.StockGroup == Groups.EURO_B) || (this.StockGroup == Groups.EURO_C) || (this.StockGroup == Groups.ALTERNEXT);
                default:
                    return this.StockGroup == group;
            }
        }
        public bool BelongsToGroup(string groupName)
        {
            return this.BelongsToGroup((Groups)Enum.Parse(typeof(Groups), groupName));
        }

        #region Advanced Data Access (min/max ...)
        private DateTime[] dateArray = null;

        public int IndexOfFirstGreaterOrEquals(DateTime date)
        {
            if (dateArray == null)
            {
                if (this.Keys.Count > 0)
                {
                    dateArray = this.Keys.ToArray();
                }
                else
                {
                    return -1;
                }
            }
            if (date > dateArray[dateArray.Length - 1]) { return -1; }
            int index;
            for (index = dateArray.Length - 1; index > 0; index--)
            {
                if (dateArray[index].Date <= date.Date) break;
            }
            return index;
        }
        public int IndexOfFirstLowerOrEquals(DateTime date)
        {
            if (dateArray == null)
            {
                if (this.Keys.Count > 0)
                {
                    dateArray = this.Keys.ToArray();
                }
                else
                {
                    return -1;
                }
            }
            if (date < dateArray[0]) { return -1; }
            int index;
            for (index = dateArray.Length - 1; index > 0; index--)
            {
                if (dateArray[index].Date <= date.Date) break;
            }
            return index;
        }
        public IList<float[]> GetMTFVariation(List<StockBarDuration> durations, int period, DateTime endDate)
        {
            var barDuration = this.BarDuration;
            var res = new List<float[]>();
            foreach (var duration in durations)
            {
                this.BarDuration = duration;
                var index = this.IndexOfFirstLowerOrEquals(endDate);
                var array = new float[period];
                res.Add(array);
                for (int i = 0; i < period; i++)
                {
                    array[i] = this.ElementAt(index - i).Value.VARIATION;
                }
            }
            return res;
        }
        public int IndexOf(DateTime date)
        {
            if (dateArray == null)
            {
                if (this.Keys.Count > 0)
                {
                    dateArray = this.Keys.ToArray();
                }
                else
                {
                    return -1;
                }
            }
            if (date < dateArray[0]) { return -1; }
            if (date > dateArray[dateArray.Length - 1]) { return -1; }
            return IndexOfRec(date, 0, dateArray.Length - 1);
        }
        private int IndexOfRec(DateTime date, int startIndex, int endIndex)
        {
            if (startIndex < endIndex)
            {
                if (dateArray[startIndex] == date)
                {
                    return startIndex;
                }
                if (dateArray[endIndex] == date)
                {
                    return endIndex;
                }
                int midIndex = (startIndex + endIndex) / 2;
                int comp = date.CompareTo(dateArray[midIndex]);
                if (comp == 0)
                {
                    return midIndex;
                }
                else if (comp < 0)
                {// 
                    return IndexOfRec(date, startIndex + 1, midIndex - 1);
                }
                else
                {
                    return IndexOfRec(date, midIndex + 1, endIndex - 1);
                }
            }
            else
            {
                if (startIndex == endIndex && dateArray[startIndex] == date)
                {
                    return startIndex;
                }
                return -1;
            }
        }
        #region MIN_MAX_FUNCTIONS
        public float GetMin(int startIndex, int endIndex, StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.GetMin(startIndex, endIndex);
        }
        public float GetMax(int startIndex, int endIndex, StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.GetMax(startIndex, endIndex);
        }
        #endregion MIN MAX FUNCTION
        #endregion
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Deserialize Flat Attributes
            this.StockName = reader.GetAttribute("StockName");
            this.StockGroup = (Groups)Enum.Parse(typeof(Groups), reader.GetAttribute("StockGroup"));

            // Deserialize Daily Value
            XmlSerializer serializer = new XmlSerializer(typeof(StockDailyValue));
            while (reader.Name == "StockDailyValue")
            {
                StockDailyValue stockDailyValue = (StockDailyValue)serializer.Deserialize(reader);
                this.Add(stockDailyValue.DATE, stockDailyValue);
            }
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Serialize Flat Attributes
            writer.WriteStartElement(typeof(StockSerie).ToString());
            writer.WriteAttributeString("StockName", this.StockName);
            writer.WriteAttributeString("StockGroup", this.StockGroup.ToString());

            // Serialize Daily Value
            XmlSerializer serializer = new XmlSerializer(typeof(StockDailyValue));
            foreach (StockDailyValue stockDailyValue in Values)
            {
                serializer.Serialize(writer, stockDailyValue);
            }
            writer.WriteEndElement();
        }
        #endregion
        #region Analysis Serialisation Members
        public void ReadAnalysisFromXml(System.Xml.XmlReader reader)
        {
            try
            {
                // Deserialize StockAnalysis
                reader.ReadStartElement(); // Start StockAnalysisItem
                XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));

                this.StockAnalysis = (StockAnalysis)serializer.Deserialize(reader);

                reader.ReadEndElement(); // End StockAnalysisItem
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error parsing analysis file");
            }
        }
        public void WriteAnalysisToXml(System.Xml.XmlWriter writer)
        {
            if (!this.StockAnalysis.IsEmpty())
            {
                // Serialize StockAnalysis
                XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));
                serializer.Serialize(writer, this.StockAnalysis);
            }
        }
        #endregion
        #region Generate related series
        public StockSerie GenerateRelativeStrenthStockSerie(StockSerie referenceSerie)
        {
            if (!this.Initialise() || !referenceSerie.Initialise())
            {
                return null;
            }
            string stockName = this.StockName + "_" + referenceSerie.StockName;
            StockSerie stockSerie = new StockSerie(stockName, stockName, referenceSerie.StockGroup, StockDataProvider.Generated);
            stockSerie.IsPortofolioSerie = false;

            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            StockDailyValue value2 = null;
            float ratio = float.NaN;
            foreach (StockDailyValue value1 in this.Values)
            {
                if (referenceSerie.ContainsKey(value1.DATE))
                {
                    value2 = referenceSerie[value1.DATE];
                    if (float.IsNaN(ratio))
                    {
                        if (value1.OPEN == 0 || value2.OPEN == 0)
                        {
                            continue;
                        }
                        ratio = 100 * value2.OPEN / value1.OPEN;
                    }

                    newValue = new StockDailyValue(ratio * value1.OPEN / value2.OPEN, ratio * value1.HIGH / value2.HIGH, ratio * value1.LOW / value2.LOW, ratio * value1.CLOSE / value2.CLOSE, value1.VOLUME + value2.VOLUME, value1.DATE);
                    stockSerie.Add(value1.DATE, newValue);
                }
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public StockSerie GenerateLogStockSerie()
        {
            string stockName = this.StockName + "_LOG";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Generated);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;

            float scaleFactor = 1.0f;
            if (this.GetSerie(StockDataType.LOW).Min() < 1.0f)
            {
                scaleFactor = 0.5f / this.GetSerie(StockDataType.LOW).Min();
            }

            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            foreach (StockDailyValue value1 in this.Values)
            {
                newValue = new StockDailyValue((float)Math.Log(value1.OPEN * scaleFactor, Math.E), (float)Math.Log(value1.HIGH * scaleFactor, Math.E), (float)Math.Log(value1.LOW * scaleFactor, Math.E), (float)Math.Log(value1.CLOSE * scaleFactor, Math.E), value1.VOLUME, value1.DATE);
                stockSerie.Add(value1.DATE, newValue);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public StockSerie GenerateInverseStockSerie()
        {
            string stockName = this.StockName + "_INV";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Generated);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;

            float scale = (float)Math.Pow(10, Math.Log10(this.GetSerie(StockDataType.HIGH).Max()) + 1);

            var duration = this.BarDuration;
            this.BarDuration = StockBarDuration.Daily;

            StockDailyValue destValue;
            foreach (StockDailyValue invStockValue in this.Values)
            {
                destValue = new StockDailyValue(scale / invStockValue.OPEN, scale / invStockValue.LOW, scale / invStockValue.HIGH, scale / invStockValue.CLOSE, invStockValue.VOLUME, invStockValue.DATE);
                destValue.Serie = stockSerie;
                stockSerie.Add(destValue.DATE, destValue);
            }
            this.BarDuration = duration;

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public List<StockDailyValue> GenerateSerieForTimeSpan(List<StockDailyValue> dailyValueList, StockBarDuration timeSpan)
        {
            StockLog.Write("GenerateSerieForTimeSpan Name:" + this.StockName + " barDuration:" + timeSpan.ToString() + " CurrentBarDuration:" + this.BarDuration);
            List<StockDailyValue> newBarList = null;
            if (dailyValueList.Count == 0) return new List<StockDailyValue>();

            // Load cache if exists
            //StockDataProviderBase.LoadIntradayDurationArchive(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, this, timeSpan);

            switch (timeSpan.Duration)
            {
                case StockClasses.BarDuration.Daily:
                    newBarList = dailyValueList;
                    break;
                case StockClasses.BarDuration.TLB:
                    newBarList = GenerateNbLineBreakBarFromDaily(dailyValueList, 2);
                    break;
                case StockClasses.BarDuration.TLB_3D:
                    newBarList = GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2);
                    break;
                case StockClasses.BarDuration.TLB_6D:
                    newBarList = GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 6), 2);
                    break;
                case StockClasses.BarDuration.TLB_9D:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateMultipleBar(GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2), 3),
                          2);
                    break;
                case StockClasses.BarDuration.TLB_27D:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateNbLineBreakBarFromDaily(
                             GenerateMultipleBar(GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2),
                                3), 2), 3);
                    break;
                case StockClasses.BarDuration.Weekly:
                    {
                        StockDailyValue newValue = null;
                        DayOfWeek previousDayOfWeek = DayOfWeek.Sunday;
                        DateTime beginDate = dailyValueList.First().DATE;
                        newBarList = new List<StockDailyValue>();

                        foreach (StockDailyValue dailyValue in dailyValueList)
                        {
                            if (newValue == null)
                            {
                                newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                                   dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                                beginDate = dailyValue.DATE;
                                previousDayOfWeek = dailyValue.DATE.DayOfWeek;
                                newValue.IsComplete = false;
                            }
                            else
                            {
                                if (previousDayOfWeek < dailyValue.DATE.DayOfWeek)
                                {
                                    // We are in the week
                                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                                    newValue.CLOSE = dailyValue.CLOSE;
                                    newValue.VOLUME += dailyValue.VOLUME;
                                    previousDayOfWeek = dailyValue.DATE.DayOfWeek;
                                }
                                else
                                {
                                    // We switched to next week
                                    newValue.IsComplete = true;
                                    newBarList.Add(newValue);
                                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                                    beginDate = dailyValue.DATE;
                                    previousDayOfWeek = dailyValue.DATE.DayOfWeek;
                                    newValue.IsComplete = false;
                                }
                            }
                        }
                        if (newValue != null)
                        {
                            if (previousDayOfWeek == DayOfWeek.Friday) newValue.IsComplete = true;
                            newBarList.Add(newValue);
                        }
                    }
                    break;
                case StockClasses.BarDuration.BiWeekly:
                    {
                        var weeklyValueList = GetSmoothedValues(StockClasses.BarDuration.Weekly);
                        newBarList = GenerateMultipleBar(weeklyValueList, 2);
                    }
                    break;
                case StockClasses.BarDuration.Monthly:
                    {
                        StockDailyValue newValue = null;
                        int previousMonth = dailyValueList.First().DATE.Month;
                        DateTime beginDate = dailyValueList.First().DATE;
                        newBarList = new List<StockDailyValue>();

                        foreach (StockDailyValue dailyValue in dailyValueList)
                        {
                            if (newValue == null)
                            {
                                newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                                   dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                                beginDate = dailyValue.DATE;
                                previousMonth = dailyValue.DATE.Month;
                                newValue.IsComplete = false;
                            }
                            else
                            {
                                if (previousMonth == dailyValue.DATE.Month)
                                {
                                    // We are in the month
                                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                                    newValue.VOLUME += dailyValue.VOLUME;
                                    newValue.CLOSE = dailyValue.CLOSE;
                                    previousMonth = dailyValue.DATE.Month;
                                }
                                else
                                {
                                    // We switched to next month
                                    newValue.IsComplete = true;
                                    newBarList.Add(newValue);
                                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                                       dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                                    beginDate = dailyValue.DATE;
                                    previousMonth = dailyValue.DATE.Month;
                                    newValue.IsComplete = false;
                                }
                            }
                        }
                        if (newValue != null)
                        {
                            newBarList.Add(newValue);
                        }
                    }
                    break;
                case StockClasses.BarDuration.RENKO_2:
                    newBarList = GenerateRenkoBarFromDaily(dailyValueList, 0.02f);
                    break;
                default:
                    {
                        int period;
                        string[] timeSpanString = timeSpan.Duration.ToString().Split('_');
                        switch (timeSpanString[0].ToUpper())
                        {
                            case "BAR":
                                if (timeSpanString.Length > 1 && int.TryParse(timeSpanString[1], out period))
                                {
                                    if (period == 1)
                                    {
                                        return dailyValueList;
                                    }
                                    else
                                    {
                                        newBarList = GenerateMultipleBar(dailyValueList, period);
                                    }
                                }
                                break;
                            default:
                                throw new System.NotImplementedException("Bar duration: " + timeSpan.ToString() + " is not implemented");
                        }
                    }
                    break;
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateMinuteBarsFromIntraday(List<StockDailyValue> stockDailyValueList, int nbMinutes)
        {
            bool isIntraday = this.StockName.StartsWith("INT_");
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null)
                {
                    // New bar
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                    newValue.IsComplete = false;
                }
                else if (isIntraday && dailyValue.DATE >= newValue.DATE.AddMinutes(nbMinutes))
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                }
                else
                {
                    // Next bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.CLOSE = dailyValue.CLOSE;
                    newValue.VOLUME += dailyValue.VOLUME;
                }
            }
            if (newValue != null)
            {
                newBarList.Add(newValue);
            }
            return newBarList;
        }

        /// <summary>
        /// Real 3 line break - GenerateNbLineBreakBar
        /// </summary>
        /// <param name="stockDailyValueList"></param>
        /// <param name="nbBar"></param>
        /// <returns></returns>
        private List<StockDailyValue> GenerateNbLineBreakBar(List<StockDailyValue> stockDailyValueList, int nbBar)
        {
            bool isIntraday = this.StockGroup == Groups.INTRADAY;
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            List<StockDailyValue> tmpBarList = new List<StockDailyValue>();
            var firstValue = stockDailyValueList.First();
            StockDailyValue newValue = new StockDailyValue(firstValue.OPEN, firstValue.HIGH, firstValue.LOW, firstValue.CLOSE, firstValue.VOLUME, firstValue.DATE);
            float periodLow = Math.Max(firstValue.CLOSE, firstValue.OPEN), periodHigh = Math.Max(firstValue.CLOSE, firstValue.OPEN);
            tmpBarList.Add(newValue);
            newBarList.Add(newValue);
            long volume = 0;
            float low = float.MaxValue, high = float.MinValue;
            DateTime date = DateTime.MaxValue;
            foreach (StockDailyValue dailyValue in stockDailyValueList.Skip(1))
            {
                if (dailyValue.CLOSE > periodHigh || dailyValue.CLOSE < periodLow)
                {
                    // New up or down bar
                    float open = tmpBarList.Last().CLOSE;
                    newValue = new StockDailyValue(open, Math.Max(high, dailyValue.HIGH), Math.Min(low, dailyValue.LOW), dailyValue.CLOSE, dailyValue.VOLUME + volume, date == DateTime.MaxValue ? dailyValue.DATE : date);
                    newValue.IsComplete = dailyValue.IsComplete;
                    volume = 0; low = float.MaxValue; high = float.MinValue; date = DateTime.MaxValue;

                    if (tmpBarList.Count == nbBar) tmpBarList.RemoveAt(0);
                    tmpBarList.Add(newValue);
                    newBarList.Add(newValue);
                    periodHigh = tmpBarList.Max(v => Math.Max(v.CLOSE, v.OPEN));
                    periodLow = tmpBarList.Min(v => Math.Min(v.CLOSE, v.OPEN));
                }
                else
                {
                    if (date == DateTime.MaxValue) date = dailyValue.DATE;
                    volume += dailyValue.VOLUME;
                    low = Math.Min(low, dailyValue.LOW);
                    high = Math.Max(high, dailyValue.HIGH);
                }
            }
            if (date != DateTime.MaxValue)
            {
                var lastValue = stockDailyValueList.Last();
                float open = tmpBarList.Last().CLOSE;
                newValue = new StockDailyValue(open, Math.Max(high, lastValue.HIGH), Math.Min(low, lastValue.LOW), lastValue.CLOSE, lastValue.VOLUME + volume, date == DateTime.MaxValue ? lastValue.DATE : date);
                newValue.IsComplete = false;
                newBarList.Add(newValue);
            }
            return newBarList;
        }

        private List<StockDailyValue> GenerateMultipleBar(List<StockDailyValue> stockDailyValueList, int nbDay)
        {
            bool isIntraday = this.StockGroup == Groups.INTRADAY;
            int count = 0;
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null)
                {
                    // New bar
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                    newValue.IsComplete = false;
                    count = 1;
                }
                else if (isIntraday && dailyValue.DATE.Date != newValue.DATE.Date)
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                    newValue.IsComplete = false;
                    count = 1;
                }
                else
                {
                    // Next bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.CLOSE = dailyValue.CLOSE;
                    newValue.VOLUME += dailyValue.VOLUME;
                    if ((++count == nbDay))
                    {
                        // Final bar set to comlete only is last bar is complete
                        newValue.IsComplete = dailyValue.IsComplete;
                        newBarList.Add(newValue);
                        newValue = null;
                    }
                }
            }
            if (newValue != null)
            {
                newBarList.Add(newValue);
            }
            return newBarList;
        }
        public List<StockDailyValue> GenerateSmoothedBars(List<StockDailyValue> stockDailyValueList, int nbDay)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();

            FloatSerie closeSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.CLOSE).ToArray()).CalculateEMA(nbDay);
            FloatSerie highSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.HIGH).ToArray()).CalculateEMA(nbDay);
            FloatSerie lowSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.LOW).ToArray()).CalculateEMA(nbDay);
            FloatSerie openSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.OPEN).ToArray()).CalculateEMA(nbDay);

            StockDailyValue previousValue = stockDailyValueList[0];
            for (int i = 0; i < stockDailyValueList.Count; i++)
            {
                StockDailyValue dailyValue = stockDailyValueList[i];

                float open = openSerie[i];
                float high = highSerie[i];
                float low = lowSerie[i];
                float close = closeSerie[i];

                // New bar
                StockDailyValue newValue = new StockDailyValue(open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
                newValue.IsComplete = dailyValue.IsComplete;
                newBarList.Add(newValue);

                previousValue = dailyValue;
            }
            return newBarList;
        }
        public List<StockDailyValue> GenerateHeikinAshiBarFromDaily(List<StockDailyValue> stockDailyValueList)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            StockDailyValue newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(newValue);

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];

                float open = (newValue.OPEN + newValue.CLOSE) / 2f; // (HA-Open(-1) + HA-Close(-1)) / 2 
                float high = Math.Max(Math.Max(dailyValue.HIGH, newValue.OPEN), newValue.CLOSE); // Maximum of the High(0), HA-Open(0) or HA-Close(0) 
                float low = Math.Min(Math.Min(dailyValue.LOW, newValue.OPEN), newValue.CLOSE); // Minimum of the Low(0), HA-Open(0) or HA-Close(0) 
                float close = (dailyValue.OPEN + dailyValue.HIGH + dailyValue.LOW + dailyValue.CLOSE) / 4f; // (Open(0) + High(0) + Low(0) + Close(0)) / 4

                // New bar
                newValue = new StockDailyValue(open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
                newValue.IsComplete = dailyValue.IsComplete;
                newBarList.Add(newValue);
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateHighLowBreakBarFromDaily(List<StockDailyValue> stockDailyValueList)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            StockDailyValue lastNewValue = null;
            StockDailyValue previousNewValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(previousNewValue);

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];
                if (lastNewValue == null) // A new bar was completed in previous iteration
                {
                    // New bar
                    lastNewValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                    lastNewValue.IsComplete = false;

                    newBarList.Add(lastNewValue);

                    if (dailyValue.CLOSE > previousNewValue.HIGH || dailyValue.CLOSE < previousNewValue.LOW) // Out of previous bar range ==> Create new bar
                    {
                        previousNewValue = lastNewValue;
                        lastNewValue = null;
                    }
                }
                else
                {
                    lastNewValue.HIGH = Math.Max(lastNewValue.HIGH, dailyValue.HIGH);
                    lastNewValue.LOW = Math.Min(lastNewValue.LOW, dailyValue.LOW);
                    lastNewValue.VOLUME += dailyValue.VOLUME;
                    lastNewValue.CLOSE = dailyValue.CLOSE;
                    lastNewValue.DATE = dailyValue.DATE;

                    if (dailyValue.CLOSE > previousNewValue.HIGH || dailyValue.CLOSE < previousNewValue.LOW) // Out of previous bar range ==> Create new bar
                    {
                        // Mark bar as completed
                        previousNewValue = lastNewValue;
                        lastNewValue.IsComplete = true;
                        lastNewValue = null;
                    }
                }
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateRenkoBarFromDaily(List<StockDailyValue> stockDailyValueList, float variation)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            float upVar = 1f + variation;
            float downVar = 1 - variation;

            StockDailyValue newValue = dailyValue.OPEN <= dailyValue.CLOSE
               ? new StockDailyValue(dailyValue.CLOSE * downVar, dailyValue.CLOSE, dailyValue.CLOSE * downVar, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE)
               : new StockDailyValue(dailyValue.CLOSE * upVar, dailyValue.CLOSE * upVar, dailyValue.CLOSE, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(newValue);
            newValue.IsComplete = false;

            StockDailyValue previousBar = newValue;

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];

                if (dailyValue.CLOSE > newValue.HIGH && dailyValue.CLOSE > previousBar.HIGH)
                {
                    // New upbars
                    newValue.IsComplete = true;
                    float previousHigh = newValue.HIGH;
                    float newHigh = newValue.HIGH * upVar;
                    TimeSpan uniqueTimeSpan = new TimeSpan();
                    do
                    {
                        previousBar = newValue;
                        newValue.IsComplete = true;
                        newValue = new StockDailyValue(previousHigh, newHigh, previousHigh, newHigh, dailyValue.VOLUME, dailyValue.DATE + uniqueTimeSpan);
                        newValue.IsComplete = false;

                        newBarList.Add(newValue);

                        previousHigh = newValue.HIGH;
                        newHigh = newValue.HIGH * upVar;

                        uniqueTimeSpan += TimeSpan.FromTicks(1);

                    } while (dailyValue.CLOSE > newValue.HIGH);
                }
                else if (dailyValue.CLOSE < newValue.LOW && dailyValue.CLOSE < previousBar.LOW)
                {
                    // new downbars
                    newValue.IsComplete = true;
                    float previousLow = newValue.LOW;
                    float newLow = newValue.LOW * downVar;
                    TimeSpan uniqueTimeSpan = new TimeSpan();
                    do
                    {
                        previousBar = newValue;
                        newValue.IsComplete = true;
                        newValue = new StockDailyValue(previousLow, previousLow, newLow, newLow, dailyValue.VOLUME, dailyValue.DATE + uniqueTimeSpan);
                        newValue.IsComplete = false;

                        newBarList.Add(newValue);

                        previousLow = newValue.LOW;
                        newLow = newValue.LOW * downVar;
                        uniqueTimeSpan += TimeSpan.FromTicks(1);

                    } while (dailyValue.CLOSE < newValue.LOW);
                }
                else
                {
                    // Stay in same bar
                    newValue.VOLUME += dailyValue.VOLUME;
                }
            }
            return newBarList;
        }

        public List<StockDailyValue> GenerateDailyFromIntraday()
        {
            if (!this.BelongsToGroup(Groups.INTRADAY))
                throw new InvalidOperationException("Cannot generate daily value from non intraday data");

            DateTime currentDay = this.Values.First().DATE.Date;
            DateTime newDay = currentDay;

            float low = this.Values.First().LOW;
            float high = this.Values.First().HIGH;
            float close = this.Values.First().CLOSE;
            float open = this.Values.First().OPEN;
            long volume = 0;

            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            foreach (StockDailyValue value in this.Values)
            {
                newDay = value.DATE.Date;
                if (currentDay != newDay)
                {
                    // Create new bar
                    newBarList.Add(newValue = new StockDailyValue(open, high, low, close, volume, currentDay));

                    currentDay = newDay;
                    volume = 0;
                    open = value.OPEN;
                    low = value.LOW;
                    high = value.HIGH;
                }
                low = Math.Min(low, value.LOW);
                high = Math.Max(high, value.HIGH);
                close = value.CLOSE;
                volume += value.VOLUME;
            }
            // Add last bar
            newBarList.Add(newValue = new StockDailyValue(open, high, low, close, volume, newDay));
            newValue.IsComplete = false;

            return newBarList;
        }

        private List<StockDailyValue> GenerateNbLineBreakBarFromDaily(List<StockDailyValue> stockDailyValueList, int nbBar)
        {
            bool isIntraday = this.StockName.StartsWith("INT_");
            Queue<StockDailyValue> previousValues = new Queue<StockDailyValue>(nbBar);
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;

            if (stockDailyValueList.Count == 0) return newBarList;

            previousValues.Enqueue(stockDailyValueList[0]);

            int i = 0;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null || (isIntraday && dailyValue.DATE.Date != newValue.DATE.Date))
                {
                    // Need to create a new bar
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
                    newValue.IsComplete = false;
                    newBarList.Add(newValue);
                }
                else
                {
                    // Need to extend current bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.CLOSE = dailyValue.CLOSE;
                }

                // Check if current Bar is complete or not
                float highest = previousValues.Max(v => v.HIGH);
                if ((dailyValue.CLOSE > highest && dailyValue.IsComplete))
                {
                    newValue.IsComplete = true;
                    newValue = null;
                }
                else
                {
                    float lowest = previousValues.Min(v => v.LOW);
                    if (dailyValue.CLOSE < lowest && dailyValue.IsComplete)
                    {
                        newValue.IsComplete = true;
                        newValue = null;
                    }
                }

                if (i < nbBar)
                {
                    previousValues.Enqueue(dailyValue);
                    i++;
                    if (i == nbBar) previousValues.Dequeue();
                }
                else
                {
                    previousValues.Dequeue();
                    previousValues.Enqueue(dailyValue);
                }
            }
            return newBarList;
        }

        #endregion
        /// <summary>
        /// This function extract a float serie from another serie in order to be compared with the current serie. This function manages inconsistent date issues
        /// </summary>
        /// <param name="otherSerie"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public FloatSerie GenerateSecondarySerieFromOtherSerie(StockSerie otherSerie, StockDataType dataType)
        {
            if (!otherSerie.Initialise())
            {
                return null;
            }
            StockBarDuration currentBarDuration = this.barDuration;
            otherSerie.BarDuration = StockBarDuration.Daily;
            this.barDuration = StockBarDuration.Daily;

            FloatSerie newSerie = new FloatSerie(this.Count);
            newSerie.Name = otherSerie.StockName;

            FloatSerie otherFloatSerie = otherSerie.GetSerie(dataType);
            float previousValue = otherFloatSerie[0];
            DateTime startDate = otherSerie.Keys.First();
            DateTime lastDate = otherSerie.Keys.Last();

            int i = 0;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (dailyValue.DATE > lastDate || dailyValue.DATE < startDate)
                {
                    newSerie[i] = float.NaN;
                }
                else
                {
                    if (otherSerie.ContainsKey(dailyValue.DATE))
                    {
                        newSerie[i] = otherSerie[dailyValue.DATE].GetStockData(dataType);
                        previousValue = newSerie[i];
                    }
                    else
                    {
                        newSerie[i] = previousValue;
                    }
                }
                i++;
            }

            otherSerie.BarDuration = currentBarDuration;
            this.barDuration = currentBarDuration;

            return newSerie;
        }
        #region CSV file IO
        public bool ReadFromCSVFile(string fileName)
        {
            bool result = false;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    sr.ReadLine();  // Skip the first line
                    StockDailyValue readValue = null;
                    while (!sr.EndOfStream)
                    {
                        readValue = StockDailyValue.ReadMarketDataFromCSVStream(sr, this.StockName, true);
                        if (readValue != null && !this.ContainsKey(readValue.DATE))
                        {
                            this.Add(readValue.DATE, readValue);
                            readValue.Serie = this;
                        }
                    }
                }
                result = true;
            }
            return result;
        }

        public bool ReadFromCSVFile(string fileName, StockBarDuration duration)
        {
            bool result = false;
            if (File.Exists(fileName))
            {
                List<StockDailyValue> bars;
                DateTime lastDateTime = DateTime.MinValue;
                if (this.BarSmoothedDictionary.ContainsKey(duration.ToString()))
                {
                    bars = this.BarSmoothedDictionary[duration.ToString()];
                    lastDateTime = bars.Last().DATE;
                }
                else
                {
                    bars = new List<StockDailyValue>();
                    this.BarSmoothedDictionary.Add(duration.ToString(), bars);
                }
                using (StreamReader sr = new StreamReader(fileName))
                {
                    sr.ReadLine(); // Skip the first line
                    StockDailyValue readValue = null;
                    while (!sr.EndOfStream)
                    {
                        readValue = StockDailyValue.ReadMarketDataFromCSVStream(sr, this.StockName, true);
                        if (readValue != null && readValue.DATE > lastDateTime) //!bars.Any(b => b.DATE == readValue.DATE))
                        {
                            bars.Add(readValue);
                            readValue.Serie = this;
                        }
                    }
                }
                result = true;
            }
            return result;
        }

        public void SaveToCSV(string fileName, DateTime startDate, bool archive)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                DateTime lastDate = this.Keys.Last();
                sw.WriteLine(StockDailyValue.StringFormat());
                foreach (StockDailyValue value in this.Values)
                {
                    if (value.DATE >= startDate && !archive && value.DATE <= DateTime.Today)
                    {
                        sw.WriteLine(value.ToString());
                    }
                    else if (value.DATE <= startDate && archive)
                    {
                        sw.WriteLine(value.ToString());
                    }
                }
            }
        }

        public void SaveToCSVFromDateToDate(string fileName, DateTime startDate, DateTime endDate)
        {
            var values = this.Values.Where(v => v.DATE >= startDate && v.DATE <= endDate);
            if (values.Count() > 0)
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    DateTime lastDate = this.Keys.Last();
                    sw.WriteLine(StockDailyValue.StringFormat());
                    foreach (StockDailyValue value in values)
                    {
                        sw.WriteLine(value.ToString());
                    }
                }
            }
        }

        public void SaveToCSVFromDate(string fileName, DateTime startDate)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                DateTime lastDate = this.Keys.Last();
                sw.WriteLine(StockDailyValue.StringFormat());
                foreach (StockDailyValue value in this.Values.Where(v => v.DATE >= startDate))
                {
                    sw.WriteLine(value.ToString());
                }
            }
        }
        public void SaveToCSV(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                DateTime lastDate = this.Keys.Last();
                sw.WriteLine(StockDailyValue.StringFormat());
                foreach (StockDailyValue value in this.Values)
                {
                    sw.WriteLine(value.ToString());
                }
            }
        }
        #endregion

        public void CalculateSARStop(int index, out int sarStartIndex, out FloatSerie sarSerie, bool isSupport, float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, int lookbackPeriod)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            float accelerationFactor = accelerationFactorInit;

            float previousSAR, previousExtremum;
            sarStartIndex = index;
            if (isSupport)
            {
                // Find previous Min within the last 20 bars and index number
                previousExtremum = float.MaxValue;
                for (int i = Math.Max(0, index - lookbackPeriod); i <= index; i++)
                {
                    if (lowSerie[i] <= previousExtremum)
                    {
                        previousExtremum = lowSerie[i];
                        sarStartIndex = i;
                    }
                }
            }
            else
            {
                // Find previous Min within the last 20 bars and index number
                previousExtremum = float.MinValue;
                for (int i = Math.Max(0, index - lookbackPeriod); i <= index; i++)
                {
                    if (highSerie[i] >= previousExtremum)
                    {
                        previousExtremum = highSerie[i];
                        sarStartIndex = i;
                    }
                }
            }
            previousSAR = previousExtremum;
            List<float> sarList = new List<float>();
            sarList.Add(previousSAR);

            for (int i = sarStartIndex + 1; i < this.Values.Count(); i++)
            {
                if (isSupport)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        break;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarList.Add(previousSAR);
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        break;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarList.Add(previousSAR);
                    }
                }
            }

            sarSerie = new FloatSerie(sarList.ToArray());
        }


        internal float GetVariationSince(DateTime endDate, DateTime startDate)
        {
            int index = this.IndexOf(endDate);
            if (index == -1) throw new ArgumentOutOfRangeException("Serie does not contains start date");
            if (index == 0) return this.Values.First().VARIATION;
            int i = index;
            while (--i > 0)
            {
                StockDailyValue value = this.ValueArray[i];
                if (value.DATE <= startDate)
                {
                    break;
                }
            }
            float startValue = this.ValueArray[i].CLOSE;
            float endValue = this[endDate].CLOSE;
            return (endValue - startValue) / startValue;
        }

        public override string ToString()
        {
            return this.StockName + " " + this.Count;
        }
    }
}