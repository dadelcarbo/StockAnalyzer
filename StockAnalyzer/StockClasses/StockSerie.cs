using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.Bourso;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
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
    public class StockSerie : StockSortedDictionary, IXmlSerializable
    {
        #region Type Definition
        public enum Groups
        {
            NONE = 0,
            PEA,
            PEA_EURONEXT,
            CAC40,
            DAX40,
            SBF120,
            CAC_AT,
            CACALL,
            EURO_A,
            EURO_A_B,
            EURO_B,
            EURO_A_B_C,
            EURO_C,
            SRD,
            SRD_LO,
            ALTERNEXT,
            BELGIUM,
            HOLLAND,
            PORTUGAL,
            EUROPE,
            ITALIA,
            GERMANY,
            SPAIN,
            USA,
            SAXO,
            INDICES,
            INDICATOR,
            SECTORS,
            SECTORS_CAC,
            SECTORS_STOXX,
            CURRENCY,
            COMMODITY,
            FOREX,
            FUND,
            RATIO,
            BREADTH,
            PTF,
            BOND,
            INTRADAY,
            TURBO,
            Portfolio,
            Replay,
            ALL_STOCKS,
            ALL
        }

        #endregion
        #region public properties
        public string StockName { get; private set; }
        public string Symbol { get; private set; }
        public string ABCName { get; set; }

        public StockDataProvider DataProvider { get; private set; }

        public string IsinPrefix => ISIN?.Substring(0, 2);
        public string ISIN { get; set; }
        /// <summary>
        /// Investing.com ticker used for download
        /// </summary>
        public long Ticker { get; set; }
        public long SaxoId { get; set; }
        /// <summary>
        /// SAXO OpenAPI instrument ID
        /// </summary>
        public long Uic { get; set; }

        public bool SRD { get; set; }
        public bool SRD_LO { get; set; }
        public int SectorId { get; set; }

        public string ProductType { get; set; }
        public string Underlying { get; set; }
        public string Url { get; set; }
        public Groups StockGroup { get; private set; }
        public StockAnalysis StockAnalysis { get; set; }

        #region StockAgenda
        private StockAgenda agenda;
        public StockAgenda Agenda
        {
            get
            {
                agenda ??= LoadAgenda();
                return agenda;
            }
            set { agenda = value; }
        }

        private StockDividend dividend;

        public StockDividend Dividend => dividend ??= new StockDividend(this);
        private StockAgenda LoadAgenda()
        {
            StockAgenda stockAgenda = null;
            if (this.BelongsToGroup(Groups.CACALL))
            {
                string path = Folders.AgendaFolder;
                string fileName = path + @"\" + this.Symbol + "_" + this.StockGroup + ".xml";
                if (File.Exists(fileName))
                {
                    using FileStream fs = new FileStream(fileName, FileMode.Open);
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    XmlSerializer serializer = new XmlSerializer(typeof(StockAgenda));
                    stockAgenda = (StockAgenda)serializer.Deserialize(xmlReader);
                }
            }
            return stockAgenda;
        }
        public void SaveAgenda()
        {
            if (this.Agenda == null) return;
            string path = Folders.AgendaFolder;
            string fileName = path + @"\" + this.Symbol + "_" + this.StockGroup + ".xml";
            using FileStream fs = new FileStream(fileName, FileMode.Create);
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
            XmlSerializer serializer = new XmlSerializer(typeof(StockAgenda));
            serializer.Serialize(xmlWriter, this.Agenda);
        }

        #endregion

        public bool IsPortfolioSerie { get; set; }
        public int LastIndex => this.ValueArray.Length - 1;
        public int LastCompleteIndex => this.LastIndex > 0 ? (this.ValueArray.Last().IsComplete ? this.Count - 1 : this.Count - 2) : -1;

        public StockSerie SecondarySerie { get; set; }
        public StockDailyValue LastValue { get; private set; }
        public bool HasVolume { get; private set; }

        /// <summary>
        /// Indicates if a stock has good liquitiy on the last (period) bars by average a exchange in million of Euro.
        /// </summary>
        /// <param name="trigger">0.1 indicates 100K€</param>
        /// <returns></returns>
        public bool HasLiquidity(float trigger, int period)
        {
            float value = GetExchanged(period);
            return value > trigger;
        }
        public float GetExchanged(int period)
        {
            if (this.LastCompleteIndex < period)
                return 0f;

            float value = 0;
            for (int i = this.LastCompleteIndex - period; i <= this.LastCompleteIndex; i++)
            {
                value += this.ValueArray[i].EXCHANGED;
            }
            value /= period * 1000000f;
            return value;
        }
        #endregion
        #region DATA, EVENTS AND INDICATORS SERIES MANAGEMENT
        public new void Add(DateTime date, StockDailyValue dailyValue)
        {
            if (date.Year >= StockDataProviderBase.LOAD_START_YEAR)
            {
                base.Add(date, dailyValue);
            }
        }

        [XmlIgnore]
        public StockDataSource DataSource { get; set; }

        [XmlIgnore]
        public SortedDictionary<string, List<StockDailyValue>> BarSmoothedDictionary { get; private set; }

        BarDuration barDuration = BarDuration.Daily;
        public BarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                using (new StockSerieLocker(this))
                {
                    this.SetBarDuration(value);
                }
            }
        }

        [XmlIgnore]
        public FloatSerie[] ValueSeries { get; set; }
        [XmlIgnore]
        public Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
        [XmlIgnore]
        protected Dictionary<string, IStockCloud> CloudCache { get; set; }
        [XmlIgnore]
        public IStockTrailStop TrailStopCache { get; set; }
        [XmlIgnore]
        public IStockPaintBar PaintBarCache { get; set; }
        [XmlIgnore]
        public IStockAutoDrawing AutoDrawingCache { get; set; }
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
                    this.isInitialised = value;
                }
                if (!value)
                {
                    this.Clear();
                    ResetAllCache();
                }
            }
        }
        #region MANAGE TIMESPAN

        private StockDailyValue[] StockDailyValuesAsArray()
        {
            StockDailyValue[] values = this.Values.ToArray();
            return values;
        }

        private StockDailyValue[] valueArray = null;
        public StockDailyValue[] ValueArray
        {
            get
            {
                valueArray ??= this.StockDailyValuesAsArray();
                return valueArray;
            }
        }
        public List<StockDailyValue> GetValues(BarDuration stockBarDuration)
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
        public List<StockDailyValue> GetSmoothedValues(BarDuration newBarDuration)
        {
            string barSmoothedDuration = newBarDuration.ToString();
            if (this.BarSmoothedDictionary.ContainsKey(barSmoothedDuration))
            {
                return this.BarSmoothedDictionary[barSmoothedDuration];
            }
            else
            {
                List<StockDailyValue> newList = this.GenerateSerieForTimeSpan(this.BarSmoothedDictionary["Daily"], newBarDuration);
                this.BarSmoothedDictionary.Add(barSmoothedDuration, newList);

                return newList;
            }
        }

        private void SetBarDuration(BarDuration newBarDuration)
        {
            if (this.DataProvider == StockDataProvider.Saxo)
            {
                if (newBarDuration != barDuration)
                {
                    barDuration = newBarDuration;
                }
                StockDataProviderBase.LoadSerieData(this);
                this.Initialise();
                if (newBarDuration != barDuration)
                {
                    barDuration = newBarDuration;
                }
                return;
            }
            using MethodLogger ml = new MethodLogger(typeof(StockSerie), false, $"{this.StockName} Previous:{this.barDuration} New:{newBarDuration}");
            if (!this.Initialise() || (newBarDuration == this.barDuration))
            {
                this.barDuration = newBarDuration;
                return;
            }
            this.ResetIndicatorCache();
            this.InitRange(this.GetSmoothedValues(newBarDuration));
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
                    case StockDataType.BODYHIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyHigh).ToArray(), "BODYHIGH");
                        break;
                    case StockDataType.BODYLOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyLow).ToArray(), "BODYLOW");
                        break;
                    case StockDataType.VARIATION:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VARIATION).ToArray(), "VARIATION");
                        break;
                    case StockDataType.VOLUME:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f).ToArray(), "VOLUME");
                        break;
                    case StockDataType.EXCHANGED:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.EXCHANGED).ToArray(), "EXCHANGED");
                        break;
                }
            }

            return ValueSeries[(int)dataType];
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
                    StockLog.Write($"{trailStopName} to {this.StockName} - {this.BarDuration}");
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
                    StockLog.Write($"{indicatorName} to {this.StockName} - {this.BarDuration}", false);
                    indicator.ApplyTo(this);
                    AddIndicatorSerie(indicator, indicatorName);
                    return indicator;
                }
                return null;
            }
        }

        public IStockCloud GetCloud(String cloudName)
        {
            if (this.CloudCache.ContainsKey(cloudName))
            {
                return this.CloudCache[cloudName];
            }
            else
            {
                IStockCloud indicator = StockCloudManager.CreateCloud(cloudName);
                if (indicator != null && (this.HasVolume || !indicator.RequiresVolumeData))
                {
                    StockLog.Write($"{cloudName} to {this.StockName} - {this.BarDuration}");
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
                    StockLog.Write($"{paintBarName} to {this.StockName} - {this.BarDuration}");
                    paintBar.ApplyTo(this);

                    this.PaintBarCache = paintBar;
                    return paintBar;
                }
                return null;
            }
        }
        public IStockAutoDrawing GetAutoDrawing(String autoDrawingName)
        {
            if (this.AutoDrawingCache != null && this.AutoDrawingCache.Name == autoDrawingName)
            {
                return this.AutoDrawingCache;
            }
            else
            {
                IStockAutoDrawing paintBar = StockAutoDrawingManager.CreateAutoDrawing(autoDrawingName);
                if (paintBar != null && (this.HasVolume || !paintBar.RequiresVolumeData))
                {
                    StockLog.Write($"{autoDrawingName} to {this.StockName} - {this.BarDuration}");
                    paintBar.ApplyTo(this);

                    this.AutoDrawingCache = paintBar;
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
                    StockLog.Write($"{decoratorName} to {this.StockName} - {this.BarDuration}");
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
                    StockLog.Write($"{trailName} to {this.StockName} - {this.BarDuration}");
                    trail.ApplyTo(this);
                    this.TrailCache = trail;
                    return trail;
                }
                return null;
            }
        }

        public IStockViewableSeries GetViewableItem(string name)
        {
            string[] nameFields = name.Split('|');
            switch (nameFields[0])
            {
                case "INDICATOR":
                    return this.GetIndicator(nameFields[1]);
                case "CLOUD":
                    return this.GetCloud(nameFields[1]);
                case "TRAILSTOP":
                    return this.GetTrailStop(nameFields[1]);
                case "TRAIL":
                    return this.GetTrail(nameFields[1], nameFields[2]);
                case "PAINTBAR":
                    return this.GetPaintBar(nameFields[1]);
                case "AUTODRAWING":
                    return this.GetAutoDrawing(nameFields[1]);
                case "DECORATOR":
                    return this.GetDecorator(nameFields[1], nameFields[2]);
            }
            throw new ArgumentException("No viewable item matching " + name + " has been found");
        }

        public void AddIndicatorSerie(IStockIndicator indicator, string name)
        {
            if (this.IndicatorCache.ContainsKey(name))
            {
                this.IndicatorCache[name] = indicator;
            }
            else
            {
                this.IndicatorCache.Add(name, indicator);
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
        #region Constructors
        public StockSerie()
        {
            this.DataSource = new StockDataSource
            {
                Duration = StockClasses.BarDuration.Daily
            };
        }
        public StockSerie(string stockName, string symbol, Groups stockGroup, StockDataProvider dataProvider, BarDuration duration)
        {
            this.StockName = stockName;
            this.Symbol = symbol;
            this.StockGroup = stockGroup;
            this.StockAnalysis = new StockAnalysis();
            this.IsPortfolioSerie = false;
            this.barDuration = BarDuration.Daily;
            this.DataProvider = dataProvider;
            this.IsInitialised = false;
            this.DataSource = new StockDataSource { Duration = duration };
            ResetAllCache();
        }
        public StockSerie(string stockName, string symbol, string isin, Groups stockGroup, StockDataProvider dataProvider, BarDuration duration)
        {
            this.StockName = stockName;
            this.Symbol = symbol;
            this.ISIN = isin;
            this.StockGroup = stockGroup;
            this.StockAnalysis = new StockAnalysis();
            this.IsPortfolioSerie = false;
            this.barDuration = BarDuration.Daily;
            this.DataProvider = dataProvider;
            this.IsInitialised = false;
            this.DataSource = new StockDataSource { Duration = duration };
            ResetAllCache();
        }
        public void ResetAllCache()
        {
            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
            this.IndicatorCache = new Dictionary<string, IStockIndicator>();
            this.DecoratorCache = new Dictionary<string, IStockDecorator>();
            this.CloudCache = new Dictionary<string, IStockCloud>();
            this.PaintBarCache = null;
            this.AutoDrawingCache = null;
            this.TrailStopCache = null;
            this.TrailCache = null;
            this.dateArray = null;
            this.valueArray = null;
            this.BarSmoothedDictionary = new SortedDictionary<string, List<StockDailyValue>>();
            this.barDuration = BarDuration.Daily;
        }
        public void ResetIndicatorCache()
        {
            this.IndicatorCache.Clear();
            this.DecoratorCache.Clear();
            this.CloudCache.Clear();
            this.PaintBarCache = null;
            this.AutoDrawingCache = null;
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
                using (new StockSerieLocker(this))
                {
                    if (!this.IsInitialised)
                    {
                        StockLog.Write($"Initialising {StockName}");
                        // Multithread management
                        while (initialisingThread != null && initialisingThread != Thread.CurrentThread)
                            Thread.Sleep(50);
                        this.initialisingThread = Thread.CurrentThread;

                        if (this.Count == 0)
                        {
                            if (!StockDataProviderBase.LoadSerieData(this) || this.Count == 0)
                            {
                                return false;
                            }
                            if (this.BarSmoothedDictionary.ContainsKey("Daily"))
                            {
                                this.BarSmoothedDictionary.Remove("Daily");
                            }
                            this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
                        }
                        else
                        {
                            if (this.barDuration == BarDuration.Daily && !this.BarSmoothedDictionary.ContainsKey("Daily"))
                            {
                                this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
                            }
                        }
                        // Force indicator,data,event and other to null;
                        PreInitialise();

                        // Flag initialisation as completed
                        this.isInitialised = this.Count > 0;
                    }
                    return isInitialised;
                }
            }
            finally
            {
                this.initialisingThread = null;
            }
        }

        public void PreInitialise()
        {
            if (!this.BarSmoothedDictionary.ContainsKey("Daily"))
            {
                this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
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
            this.LastValue = previousValue;

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
            BarDuration currentBarDuration = this.BarDuration;
            try
            {
                if (index < 50 || this.LastIndex < index)
                    return false;
                int eventIndex;
                IStockEvent stockEvent = null;
                IStockViewableSeries indicator;

                switch (stockAlert.Type)
                {
                    case AlertType.Group:
                    case AlertType.Stock:
                        {
                            if (!string.IsNullOrEmpty(stockAlert.FilterFullName))
                            {
                                if (this.BarDuration != stockAlert.FilterDuration)
                                    this.BarDuration = stockAlert.FilterDuration;
                                indicator = StockViewableItemsManager.GetViewableItem(stockAlert.FilterFullName);
                                if (this.HasVolume || !indicator.RequiresVolumeData)
                                {
                                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                                }
                                else
                                {
                                    return false;
                                }
                                eventIndex = Array.IndexOf<string>(stockEvent.EventNames, stockAlert.FilterEventName);
                                if (eventIndex == -1)
                                {
                                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                                    return false;
                                }
                                else
                                {
                                    if (!stockEvent.Events[eventIndex][index])
                                        return false;
                                }
                            }
                            if (!string.IsNullOrEmpty(stockAlert.IndicatorName))
                            {
                                if (this.BarDuration != stockAlert.BarDuration)
                                    this.BarDuration = stockAlert.BarDuration;
                                indicator = StockViewableItemsManager.GetViewableItem(stockAlert.IndicatorFullName);
                                if (this.HasVolume || !indicator.RequiresVolumeData)
                                {
                                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, this);
                                }
                                else
                                {
                                    return false;
                                }
                                eventIndex = Array.IndexOf<string>(stockEvent.EventNames, stockAlert.EventName);
                                if (eventIndex == -1)
                                {
                                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                                    return false;
                                }
                                else
                                {
                                    return stockEvent.Events[eventIndex][index];
                                }
                            }
                        }
                        break;
                    case AlertType.Price:
                        if (stockAlert.PriceTrigger != 0 && index > 1)
                        {
                            var closeSerie = this.GetSerie(StockDataType.CLOSE);
                            if (stockAlert.TriggerBrokenUp)
                            {
                                return closeSerie[index - 1] < stockAlert.PriceTrigger && closeSerie[index] > stockAlert.PriceTrigger;
                            }
                            else
                            {
                                return closeSerie[index - 1] > stockAlert.PriceTrigger && closeSerie[index] < stockAlert.PriceTrigger;
                            }
                        }
                        break;
                }
            }
            finally
            {
                if (this.BarDuration != currentBarDuration)
                    this.BarDuration = currentBarDuration;
            }

            return false;
        }

        public bool MatchEvent(StockAlertDef stockAlert)
        {
            BarDuration currentBarDuration = this.BarDuration;
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

            BarDuration currentBarDuration = this.BarDuration;
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

            BarDuration currentBarDuration = this.BarDuration;
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
        public void CalculateCrossSR(FloatSerie fastSerie, int smoothing, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
        {
            supportSerie = new FloatSerie(this.Count, fastSerie.Name + ".S", float.NaN);
            resistanceSerie = new FloatSerie(this.Count, fastSerie.Name + ".R", float.NaN);

            FloatSerie slowSerie = fastSerie.CalculateEMA(smoothing);

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            supportSerie[0] = float.NaN;
            resistanceSerie[0] = lowSerie[0];
            float latestHigh = highSerie[0];
            float latestLow = lowSerie[0];

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
            resistanceSerie[0] = lowSerie[0];
            float latestHigh = highSerie[0];
            float latestLow = lowSerie[0];

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

        public FloatSerie CalculateRateOfRise(int period, bool bodyLow = true)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = bodyLow ? this.GetSerie(StockDataType.LOW) : this.GetSerie(StockDataType.BODYLOW);

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
            serie.Name = $"ROR_{period}";
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
                serie[i] = -(closeSerie[i] - max) / max;
            }
            for (int i = period; i < this.Count; i++)
            {
                max = highSerie.GetMax(i - period, i);
                serie[i] = -(closeSerie[i] - max) / max;
            }
            serie.Name = $"ROD_{period}";
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
            serie.Name = $"ROC_{period}";
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
        public FloatSerie CalculateFastOscillator(int period)
        {
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            FloatSerie fastOscillatorSerie = new FloatSerie(this.Count);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = this.GetSerie(StockDataType.CLOSE);
            float lowestLow = float.MaxValue;
            float highestHigh = float.MinValue;

            for (int i = 0; i < this.Count; i++)
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
            fastOscillatorSerie.Name = $"FastK({period})";
            return fastOscillatorSerie;
        }
        public FloatSerie CalculateFastBodyOscillator(int period)
        {
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            FloatSerie fastOscillatorSerie = new FloatSerie(this.Count);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            var bodyHighSerie = this.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = this.GetSerie(StockDataType.BODYLOW);
            float lowestLow = float.MaxValue;
            float highestHigh = float.MinValue;

            for (int i = 0; i < this.Count; i++)
            {
                lowestLow = bodyLowSerie.GetMin(Math.Max(0, i - period), i);
                highestHigh = bodyHighSerie.GetMax(Math.Max(0, i - period), i);
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
            fastOscillatorSerie.Name = $"FastK({period})";
            return fastOscillatorSerie;
        }
        public void CalculateEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            float alpha = 2.0f / (float)(period + 1);

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

        public bool NeedIntradayDowload()
        {
            return true;
        }
        public bool IsMarketOpened()
        {
            return true;
            var today = DateTime.Now;
            if (today.DayOfWeek == DayOfWeek.Sunday || today.DayOfWeek == DayOfWeek.Saturday)
                return false;
            if (this.StockName.StartsWith("INT_US_") && today.TimeOfDay > new TimeSpan(14, 30, 0) && today.TimeOfDay < new TimeSpan(22, 0, 0))
                return true;
            if (this.StockName.StartsWith("INT_") && today.TimeOfDay > new TimeSpan(9, 0, 0) && today.TimeOfDay < new TimeSpan(17, 40, 0))
                return true;
            if (this.StockName.StartsWith("FUT_") && today.TimeOfDay > new TimeSpan(8, 0, 0) && today.TimeOfDay < new TimeSpan(22, 05, 0))
                return true;
            if (this.StockName.StartsWith("TURBO") && today.TimeOfDay > new TimeSpan(8, 0, 0) && today.TimeOfDay < new TimeSpan(22, 05, 0))
                return true;
            return false;
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
        public void CalculateBandTrailStop(FloatSerie lowerBand, FloatSerie upperBand, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
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
                            shortStopSerie[i] = upperBand[i];
                        }
                        else
                        {
                            // UpTrend still in place
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBand[i]);
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowerBand[i];
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Down trend still in place
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBand[i]);
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
        public void CalculateTOPEMA(int period, out FloatSerie emaSerieSupport, out FloatSerie emaSerieResistance)
        {
            float alpha = 2.0f / (float)(period + 1);
            bool isUpTrend = false;
            bool isDownTrend = false;
            emaSerieSupport = new FloatSerie(this.Values.Count(), "TOPEMA.Sup");
            emaSerieResistance = new FloatSerie(this.Values.Count(), "TOPEMA.Res");
            float previousLow = float.NaN;
            float previousHigh = float.NaN;
            float previousEMAUp = float.NaN;
            float previousEMADown = float.NaN;
            emaSerieResistance[0] = emaSerieResistance[1] = float.NaN;
            emaSerieSupport[0] = emaSerieSupport[1] = float.NaN;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

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
                        emaSerieSupport[i] = previousLow = previousEMAUp = lowSerie[i - 1];
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
                        emaSerieSupport[i] = previousLow = previousEMAUp = Math.Min(lowSerie[i - 1], lowSerie[i]);
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
                        emaSerieResistance[i] = previousHigh = previousEMADown = highSerie[i - 1];
                    }
                    else
                    {
                        emaSerieResistance[i] = float.NaN;
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
                        accelerationFactor = accelerationFactorInit * (1f + ((previousExtremum - closeSerie[i]) / closeSerie[i] * 100f));
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
                        accelerationFactor = accelerationFactorInit * (1f + ((closeSerie[i] - previousExtremum) / closeSerie[i] * 100f));
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
            catch (Exception e)
            {
                StockLog.Write(e);
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
            catch (Exception e)
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
            catch (Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        #endregion

        public bool BelongsToGroup(Groups group)
        {
            if (StockAnalysis != null && StockAnalysis.Excluded) return false;
            return BelongsToGroupFull(group);
        }
        public bool BelongsToGroupFull(Groups group)
        {
            if (group == Groups.NONE)
                return false;
            if (StockGroup == group || group == Groups.ALL)
                return true;

            if (DataProvider == StockDataProvider.ABC)
                return ABCDataProvider.BelongsToGroup(this, group);

            switch (group)
            {
                case Groups.SAXO:
                    return this.SaxoId > 0;
            }

            return false;
        }
        public bool BelongsToGroup(string groupName)
        {
            return this.BelongsToGroup((Groups)Enum.Parse(typeof(Groups), groupName));
        }

        public bool Intraday => BoursoIntradayDataProvider.ContainsSerie(this);

        #region Advanced Data Access (min/max ...)
        private DateTime[] dateArray = null;

        public int IndexOfFirstGreaterOrEquals(DateTime date)
        {
            if (dateArray == null)
            {
                if (this.Count > 0)
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
                if (this.Count > 0)
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
        //public int IndexOf(DateTime date)
        //{
        //    if (dateArray == null)
        //    {
        //        if (this.Count > 0)
        //        {
        //            dateArray = this.Keys.ToArray();
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    if (date < dateArray[0]) { return -1; }
        //    if (date > dateArray[dateArray.Length - 1]) { return -1; }
        //    return IndexOfRec(date, 0, dateArray.Length - 1);
        //}
        //private int IndexOfRec(DateTime date, int startIndex, int endIndex)
        //{
        //    if (startIndex < endIndex)
        //    {
        //        if (dateArray[startIndex] == date)
        //        {
        //            return startIndex;
        //        }
        //        if (dateArray[endIndex] == date)
        //        {
        //            return endIndex;
        //        }
        //        int midIndex = (startIndex + endIndex) / 2;
        //        int comp = date.CompareTo(dateArray[midIndex]);
        //        if (comp == 0)
        //        {
        //            return midIndex;
        //        }
        //        else if (comp < 0)
        //        {// 
        //            return IndexOfRec(date, startIndex + 1, midIndex - 1);
        //        }
        //        else
        //        {
        //            return IndexOfRec(date, midIndex + 1, endIndex - 1);
        //        }
        //    }
        //    else
        //    {
        //        if (startIndex == endIndex && dateArray[startIndex] == date)
        //        {
        //            return startIndex;
        //        }
        //        return -1;
        //    }
        //}
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
        public StockSerie GenerateLogStockSerie()
        {
            string stockName = this.StockName + "_LOG";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Generated, this.DataSource.Duration);
            stockSerie.IsPortfolioSerie = this.IsPortfolioSerie;

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
        public StockSerie GenerateDivStockSerie()
        {
            string stockName = this.StockName + "_DIV";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Generated, this.DataSource.Duration);
            stockSerie.IsPortfolioSerie = this.IsPortfolioSerie;

            // Calculate ratio foreach values
            var previousDate = DateTime.MaxValue;
            var adjClose = this.Values.First().CLOSE;
            var previousClose = this.Values.First().CLOSE;

            var bars = new List<StockDailyValue>();
            foreach (var bar in this.Values)
            {
                float dividend = 0;
                var entries = this?.Dividend?.Entries.Where(e => e.Date == bar.DATE).ToList();
                if (entries.Count > 0)
                {
                    dividend = entries.Sum(e => e.Dividend) / previousClose;
                }

                var adjVar = 1 + bar.VARIATION + dividend;
                adjClose *= adjVar;
                var newBar = new StockDailyValue(bar.OPEN * adjClose / bar.CLOSE, bar.HIGH * adjClose / bar.CLOSE, bar.LOW * adjClose / bar.CLOSE, adjClose, bar.VOLUME, bar.DATE);
                bars.Add(newBar);

                previousDate = bar.DATE;
                previousClose = bar.CLOSE;
            }
            var ratio = previousClose / adjClose;

            foreach (var bar in bars)
            {
                bar.OPEN *= ratio;
                bar.HIGH *= ratio;
                bar.LOW *= ratio;
                bar.CLOSE *= ratio;
                stockSerie.Add(bar.DATE, bar);
            }

            //foreach (var value in this.Values.Reverse())
            //{
            //    var entries = this?.Dividend?.Entries.Where(e => e.Date <= previousDate && e.Date > value.DATE.Date).ToList();
            //    if (entries.Count > 0)
            //    {
            //        dividend += entries.Sum(e => e.Dividend) * previousClose / previousAdjClose;
            //    }

            //    var newValue = new StockDailyValue(value.OPEN - dividend, value.HIGH - dividend, value.LOW - dividend, value.CLOSE - dividend, value.VOLUME, value.DATE);
            //    bars.Add(newValue);

            //    previousDate = value.DATE;
            //    previousClose = value.CLOSE;
            //    previousAdjClose = newValue.CLOSE;
            //}
            //bars.Reverse();
            if (this.ValueArray.Length != stockSerie.Count)
            {
                StockLog.Write("Here!");
            }
            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }

        public List<StockDailyValue> GenerateSerieForTimeSpan(List<StockDailyValue> dailyValueList, BarDuration timeSpan)
        {
            StockLog.Write((string)("Name:" + this.StockName + " barDuration:" + timeSpan.ToString() + " CurrentBarDuration:" + this.BarDuration));
            List<StockDailyValue> newBarList = null;
            if (dailyValueList.Count == 0) return new List<StockDailyValue>();

            // Load cache if exists
            //StockDataProviderBase.LoadIntradayDurationArchive(StockAnalyzerSettings.Properties.Folders, this, timeSpan);

            switch (timeSpan)
            {
                case StockClasses.BarDuration.Daily:
                    newBarList = dailyValueList;
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
                            var lastDailyValue = dailyValueList.Last();
                            if (lastDailyValue.DATE.DayOfWeek == DayOfWeek.Friday)
                                newValue.IsComplete = lastDailyValue.IsComplete;
                            newBarList.Add(newValue);
                        }
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
                            // Check if bar complete
                            var currentMonth = newValue.DATE.Month;
                            var lastDailyValue = dailyValueList.Last().DATE;
                            if (lastDailyValue.DayOfWeek == DayOfWeek.Friday && lastDailyValue.AddDays(3).Month != currentMonth)
                                newValue.IsComplete = true;
                            newBarList.Add(newValue);
                        }
                    }
                    break;
                default:
                    {
                        int period;
                        string[] timeSpanString = timeSpan.ToString().Split('_');
                        switch (timeSpanString[0].ToUpper())
                        {
                            case "H":
                                if (timeSpanString.Length > 1 && int.TryParse(timeSpanString[1], out period))
                                {
                                    if (!string.IsNullOrEmpty(this.ISIN) && this.DataProvider == StockDataProvider.ABC)
                                    {
                                        var intradaySerie = new StockSerie { StockName = this.StockName, StockGroup = this.StockGroup, Symbol = this.Symbol, ISIN = this.ISIN, DataProvider = StockDataProvider.BoursoIntraday };
                                        intradaySerie.ResetAllCache();
                                        if (StockDataProviderBase.DownloadSerieData(intradaySerie) && intradaySerie.Initialise())
                                        {
                                            newBarList = GenerateHourBar(intradaySerie.Values.ToList(), period);
                                        }
                                    }
                                    else
                                    {
                                        newBarList = GenerateHourBar(dailyValueList, period);
                                    }
                                }
                                break;
                            case "M":
                                if (timeSpanString.Length > 1 && int.TryParse(timeSpanString[1], out period))
                                {
                                    if (!string.IsNullOrEmpty(this.ISIN) && this.DataProvider == StockDataProvider.ABC)
                                    {
                                        var intradaySerie = new StockSerie { StockName = this.StockName, StockGroup = this.StockGroup, Symbol = this.Symbol, ISIN = this.ISIN, DataProvider = StockDataProvider.BoursoIntraday };
                                        intradaySerie.ResetAllCache();
                                        if (StockDataProviderBase.DownloadSerieData(intradaySerie) && intradaySerie.Initialise())
                                        {
                                            newBarList = GenerateMinuteBar(intradaySerie.Values.ToList(), period);
                                        }
                                    }
                                    else
                                    {
                                        newBarList = GenerateMinuteBar(dailyValueList, period);
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
        private List<StockDailyValue> GenerateMinuteBar(List<StockDailyValue> stockDailyValueList, int nbMinutes)
        {
            if (nbMinutes <= 4)
                return stockDailyValueList;
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            DateTime closeDate = DateTime.Now;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null)
                {
                    // New bar
                    int min = dailyValue.DATE.Minute / nbMinutes * nbMinutes;
                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, min, 0);
                    closeDate = openDate.AddMinutes(nbMinutes);
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
                    newValue.IsComplete = false;
                }
                else if (dailyValue.DATE.Date != newValue.DATE.Date || dailyValue.DATE >= closeDate)
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    int min = dailyValue.DATE.Minute / nbMinutes * nbMinutes;
                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, min, 0);
                    closeDate = openDate.AddMinutes(nbMinutes);
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
                    newValue.IsComplete = false;
                }
                else
                {
                    // Need to extend current bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.CLOSE = dailyValue.CLOSE;
                }
            }
            if (newValue != null)
            {
                newBarList.Add(newValue);
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateHourBar(List<StockDailyValue> stockDailyValueList, int nbHours)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            DateTime closeDate = DateTime.Now;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null)
                {
                    // New bar
                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, 0, 0);
                    closeDate = openDate.AddHours(nbHours);
                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
                    newValue.IsComplete = false;
                }
                else if (dailyValue.DATE.Date != newValue.DATE.Date || dailyValue.DATE >= closeDate)
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, 0, 0);
                    closeDate = openDate.AddHours(nbHours);

                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
                    newValue.IsComplete = false;
                }
                else
                {
                    // Need to extend current bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.CLOSE = dailyValue.CLOSE;
                }
            }
            if (newValue != null)
            {
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
                    if (++count == nbDay)
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

        #endregion
        /// <summary>
        /// This function extract a float serie from another serie in order to be compared with the current serie. This function manages inconsistent date issues
        /// </summary>
        /// <param name="otherSerie"></param>
        /// <returns></returns>
        public FloatSerie GenerateSecondarySerieFromOtherSerie(StockSerie otherSerie)
        {
            if (!otherSerie.Initialise())
            {
                return null;
            }
            BarDuration currentBarDuration = this.barDuration;
            otherSerie.BarDuration = BarDuration.Daily;
            this.barDuration = BarDuration.Daily;

            FloatSerie newSerie = new FloatSerie(this.Count);
            newSerie.Name = otherSerie.StockName;

            FloatSerie otherFloatSerie = otherSerie.GetSerie(StockDataType.CLOSE);
            float previousValue = otherFloatSerie[0];
            DateTime startDate = otherSerie.Keys.First();
            DateTime lastDate = otherSerie.LastValue.DATE;

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
                        newSerie[i] = otherSerie[dailyValue.DATE].GetStockData(StockDataType.CLOSE);
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
        public FloatSerie GenerateSecondarySerieFromOtherSerie(StockSerie otherSerie, BarDuration duration)
        {
            if (duration >= BarDuration.M_5 || !otherSerie.Initialise())
            {
                return null;
            }
            BarDuration currentBarDuration = this.barDuration;
            otherSerie.BarDuration = duration;
            this.barDuration = duration;

            FloatSerie newSerie = new FloatSerie(this.Count);
            newSerie.Name = otherSerie.StockName;

            FloatSerie otherCloseSerie = otherSerie.GetSerie(StockDataType.CLOSE);
            float previousValue = otherCloseSerie[0];
            DateTime startDate = otherSerie.Keys.First();
            DateTime lastDate = otherSerie.LastValue.DATE;

            int i = 0;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (dailyValue.DATE > lastDate || dailyValue.DATE < startDate)
                {
                    newSerie[i] = otherCloseSerie[0];
                }
                else
                {
                    if (otherSerie.ContainsKey(dailyValue.DATE))
                    {
                        newSerie[i] = otherSerie[dailyValue.DATE].GetStockData(StockDataType.CLOSE);
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
                        }
                    }
                }
                result = true;
            }
            return result;
        }

        public void SaveToCSV(string fileName, DateTime startDate, bool archive)
        {
            using StreamWriter sw = new StreamWriter(fileName);
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

        public void SaveToCSVFromDateToDate(string fileName, DateTime startDate, DateTime endDate)
        {
            var values = this.Values.Where(v => v.DATE >= startDate && v.DATE <= endDate).ToList();
            if (values.Count() > 0)
            {
                using StreamWriter sw = new StreamWriter(fileName);
                sw.WriteLine(StockDailyValue.StringFormat());
                foreach (StockDailyValue value in values)
                {
                    sw.WriteLine(value.ToString());
                }
            }
        }
        #endregion


        public override string ToString()
        {
            return this.StockName + " " + this.Count;
        }

        #region Multithread Lock/Unlock

        static readonly bool lockLoggingActive = false;

        readonly object __lockObj = new object();
        public void Lock()
        {
            using MethodLogger ml = new MethodLogger(this, lockLoggingActive, this.StockName);
            bool lockTaken = false;
            while (!lockTaken)
            {
                StockLog.Write($"Trying to lock {this.StockName}", lockLoggingActive);
                Monitor.TryEnter(__lockObj, 500, ref lockTaken);
            }
            StockLog.Write("Lock taken", lockLoggingActive);
        }

        public void UnLock()
        {
            using MethodLogger ml = new MethodLogger(this, lockLoggingActive, this.StockName);
            Monitor.Exit(__lockObj);
        }

        #endregion
    }
}