using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockStrategyClasses.StockMoneyManagement;
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
    public class MomentumSerie
    {
        [XmlIgnore]
        public float MomentumSlow { get; set; }
        public float MomentumFast { get; set; }

        public StockSerie StockSerie { get; set; }
        public float PositionSlow { get; set; }
        public float PositionFast { get; set; }

        //public float Position { get { return (PositionSlow - PositionFast) / 2.0f; } }
        public float Position { get { return PositionSlow; } }

        public MomentumSerie()
        {

        }
    }

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
            FUTURE,
            FOREX,
            FUND,
            RATIO,
            BREADTH,
            BOND,
            COT,
            TICK,
            RANGE,
            INTRADAY,
            USER1,
            USER2,
            USER3,
            ShortInterest,
            Portfolio,
            ALL,
            TURBO,

        }
        public enum StockBarDuration
        {
            Daily,
            Daily_EMA3,
            Daily_EMA6,
            Daily_EMA9,
            Daily_EMA12,
            Daily_EMA20,
            Weekly,
            Weekly_EMA3,
            Weekly_EMA6,
            Weekly_EMA9,
            Weekly_EMA12,
            Weekly_EMA20,
            Monthly,
            Bar_2,
            Bar_3,
            Bar_6,
            Bar_9,
            Bar_12,
            Bar_24,
            Bar_27,
            Bar_48,
            MY,
            HA,
            HA_3D,
            //HLBreak,
            //HLBreak3,
            //HLBreak6,
            //HeikinAshi2B,
            //HeikinAshi2B_3D,
            //HeikinAshi2B_6D,
            //HeikinAshi2B_9D,
            //HeikinAshi2B_27D,
            //TLB_BIS,
            //TLB_TER,
            //TLB_TER_6D,
            //TLB_TER_9D,
            //TLB_TER_27D,
            MIN_5,
            MIN_15,
            MIN_60,
            MIN_120,
            TLB,
            TLB_3D,
            TLB_6D,
            TLB_9D,
            TLB_27D,
            TLB_EMA3,
            TLB_3D_EMA3,
            TLB_6D_EMA3,
            TLB_9D_EMA3,
            TLB_27D_EMA3,
            TLB_EMA6,
            TLB_3D_EMA6,
            TLB_6D_EMA6,
            TLB_9D_EMA6,
            TLB_27D_EMA6,
            TLB_EMA12,
            TLB_3D_EMA12,
            TLB_6D_EMA12,
            TLB_9D_EMA12,
            TLB_27D_EMA12,
            TLB_EMA20,
            TLB_3D_EMA20,
            TLB_6D_EMA20,
            TLB_9D_EMA20,
            TLB_27D_EMA20,
            ThreeLineBreak,
            ThreeLineBreak_BIS,
            ThreeLineBreak_TER,
            SixLineBreak,
            TLB_Weekly,
            RENKO_1,
            RENKO_2,
            RENKO_5,
            RENKO_10,
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
        public int LastIndex { get { return this.Values.Count - 1; } }
        public int LastCompleteIndex { get { return this.Values.Last().IsComplete ? this.Values.Count - 1 : this.Values.Count - 2; } }
        public CotSerie CotSerie { get; set; }
        public StockSerie SecondarySerie { get; set; }
        public bool HasVolume { get; private set; }
        public bool HasShortInterest { get; set; }
        #endregion

        #region DATA, EVENTS AND INDICATORS SERIES MANAGEMENT
        [XmlIgnore]
        public SortedDictionary<StockBarDuration, List<StockDailyValue>> BarSerieDictionary { get; private set; }

        StockBarDuration barDuration = StockBarDuration.Daily;
        public StockBarDuration BarDuration { get { return barDuration; } set { this.SetBarDuration(value); } }
        [XmlIgnore]
        public FloatSerie[] ValueSeries { get; set; }
        [XmlIgnore]
        public FloatSerie[] IndicatorSeries { get; set; }
        [XmlIgnore]
        public BoolSerie[] EventSeries { get; set; }
        [XmlIgnore]
        protected Dictionary<string, FloatSerie> FloatSerieCache { get; set; }
        [XmlIgnore]
        protected Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
        [XmlIgnore]
        protected IStockTrailStop TrailStopCache { get; set; }
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
                if (valueArray == null) valueArray = this.StockDailyValuesAsArray();
                return valueArray;
            }
        }
        public List<StockDailyValue> GetValues(StockBarDuration stockBarDuration)
        {
            if (this.BarSerieDictionary.ContainsKey(stockBarDuration))
            {
                return this.BarSerieDictionary[stockBarDuration];
            }
            else
            {
                return null;
                //List<StockDailyValue> newList = this.GenerateSerieForTimeSpanFromDaily(stockBarDuration);
                //this.BarSerieDictionary.Add(stockBarDuration, newList);
                //return newList;
            }
        }
        public List<StockDailyValue> GetExactValues()
        {
            if (ExactDataDurationMapping == null)
            {
                ExactDataDurationMapping = new SortedDictionary<StockBarDuration, StockBarDuration>();
                //ExactDataDurationMapping.Add(StockBarDuration.Bar_1_EMA3, StockBarDuration.Daily);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi, StockBarDuration.Daily);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi2B, StockBarDuration.TLB);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi2B_3D, StockBarDuration.TwoLineBreaks_3D);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi2B_6D, StockBarDuration.TwoLineBreaks_6D);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi2B_9D, StockBarDuration.TwoLineBreaks_9D);
                //ExactDataDurationMapping.Add(StockBarDuration.HeikinAshi2B_27D, StockBarDuration.TwoLineBreaks_27D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_EMA3, StockBarDuration.TLB);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_3D_EMA3, StockBarDuration.TwoLineBreaks_3D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_6D_EMA3, StockBarDuration.TwoLineBreaks_6D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_9D_EMA3, StockBarDuration.TwoLineBreaks_9D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_27D_EMA3, StockBarDuration.TwoLineBreaks_27D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_EMA6, StockBarDuration.TLB);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_3D_EMA6, StockBarDuration.TwoLineBreaks_3D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_6D_EMA6, StockBarDuration.TwoLineBreaks_6D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_9D_EMA6, StockBarDuration.TwoLineBreaks_9D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_27D_EMA6, StockBarDuration.TwoLineBreaks_27D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_EMA12, StockBarDuration.TLB);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_3D_EMA12, StockBarDuration.TwoLineBreaks_3D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_6D_EMA12, StockBarDuration.TwoLineBreaks_6D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_9D_EMA12, StockBarDuration.TwoLineBreaks_9D);
                //ExactDataDurationMapping.Add(StockBarDuration.TLB_27D_EMA12, StockBarDuration.TwoLineBreaks_27D);
            }
            if (ExactDataDurationMapping.ContainsKey(this.barDuration))
            {
                return GetValues(ExactDataDurationMapping[this.barDuration]);
            }
            else
            {
                return GetValues(StockSerie.StockBarDuration.Daily);
            }
        }
        protected void SetBarDuration(StockBarDuration newBarDuration)
        {
            //if (newBarDuration != this.BarDuration) StockLog.Write("SetBarDuration Name:" + this.StockName + " newDuration:" + newBarDuration + " CurrentDuration:" + this.BarDuration);
            if (!this.Initialise() || newBarDuration == this.BarDuration)
            {
                if (!this.BarSerieDictionary.ContainsKey(StockBarDuration.Daily))
                {
                    if (this.BarDuration == StockBarDuration.Daily && this.Values.Count != 0)
                    {
                        this.BarSerieDictionary.Add(StockBarDuration.Daily, this.Values.ToList());
                    }
                    this.barDuration = newBarDuration;
                }
                return;
            }

            if (!this.BarSerieDictionary.ContainsKey(StockBarDuration.Daily))
            {
                if (this.BarDuration == StockBarDuration.Daily)
                {
                    this.BarSerieDictionary.Add(StockBarDuration.Daily, this.Values.ToList());
                }
                else
                {
                    if (this.BarSerieDictionary.Count == 0)
                    {
                        // Reinitialise the serie.
                        this.IsInitialised = false;
                        this.barDuration = StockBarDuration.Daily;
                        this.SetBarDuration(newBarDuration);
                    }
                    return;
                }
            }
            if (this.BarSerieDictionary.ContainsKey(newBarDuration))
            {
                this.IsInitialised = false;
                foreach (StockDailyValue dailyValue in this.BarSerieDictionary[newBarDuration])
                {
                    this.Add(dailyValue.DATE, dailyValue);
                }
                this.Initialise();
            }
            else
            {
                this.IsInitialised = false;
                List<StockDailyValue> newList =
                   this.GenerateSerieForTimeSpanFromDaily(newBarDuration);

                foreach (StockDailyValue dailyValue in newList)
                {
                    this.Add(dailyValue.DATE, dailyValue);
                }
                this.Initialise();
            }
            this.barDuration = newBarDuration;
            valueArray = StockDailyValuesAsArray();
            return;
        }
        public void ClearBarDurationCache()
        {
            this.BarSerieDictionary.Clear();
        }
        #endregion

        public float GetValue(StockDataType dataType, int index)
        {
            return GetSerie(dataType).Values.ElementAt(index);
        }
        public float GetValue(StockIndicatorType indicatorType, int index)
        {
            return GetSerie(indicatorType).Values.ElementAt(index);
        }
        public bool GetValue(StockEvent.EventType eventType, int index)
        {
            return GetSerie(eventType)[index];
        }
        public FloatSerie GetSerie(StockDataType dataType)
        {
            return ValueSeries[(int)dataType];
        }
        public FloatSerie GetSerie(StockIndicatorType indicatorType)
        {
            if (IndicatorSeries[(int)indicatorType] == null)
            {
                this.Initialise(indicatorType);
            }
            return IndicatorSeries[(int)indicatorType];
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

        public BoolSerie GetSerie(StockEvent.EventType eventType)
        {
            if (EventSeries[(int)eventType] == null)
            {
                this.Initialise(eventType);
            }
            return EventSeries[(int)eventType];
        }
        public void AddSerie(StockDataType dataType, FloatSerie serie)
        {
            if (serie != null)
            {
                serie.Name = dataType.ToString();
            }
            this.ValueSeries[(int)dataType] = serie;
        }
        public void AddSerie(StockIndicatorType indicatorType, FloatSerie serie)
        {
            if (serie != null)
            {
                serie.Name = indicatorType.ToString();
            }
            this.IndicatorSeries[(int)indicatorType] = serie;
        }
        public void AddSerie(StockEvent.EventType eventType, BoolSerie serie)
        {
            this.EventSeries[(int)eventType] = serie;
        }
        public void AddSerie(string serieName, FloatSerie serie)
        {
            if (serie != null)
            {
                serie.Name = serieName;
            }
            if (this.FloatSerieCache.ContainsKey(serieName))
            {
                this.FloatSerieCache[serieName] = serie;
            }
            else
            {
                this.FloatSerieCache.Add(serieName, serie);
            }
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
        public StockSerie(StockBarSerie barSerie, Groups stockGroup)
        {
            this.StockName = barSerie.Name;
            this.ShortName = barSerie.ShortName;
            this.StockGroup = stockGroup;
            this.lastDate = DateTime.MinValue;
            this.StockAnalysis = new StockAnalysis();
            this.IsPortofolioSerie = false;
            this.barDuration = StockBarDuration.Daily;

            this.IsInitialised = false;

            System.TimeSpan minSpan = System.TimeSpan.FromSeconds(1);
            DateTime date;
            foreach (StockBar bar in barSerie.StockBars)
            {
                StockDailyValue dailyValue = new StockDailyValue(barSerie.Name, (float)bar.OPEN, (float)bar.HIGH, (float)bar.LOW, (float)bar.CLOSE, bar.VOLUME, bar.DATE);
                date = bar.DATE;
                while (this.ContainsKey(date))
                {
                    date += minSpan;
                }
                this.Add(date, dailyValue);
            }
            ResetAllCache();
        }
        private void ResetAllCache()
        {
            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
            this.IndicatorSeries = new FloatSerie[Enum.GetValues(typeof(StockIndicatorType)).Length];
            this.EventSeries = new BoolSerie[Enum.GetValues(typeof(StockEvent.EventType)).Length];
            this.FloatSerieCache = new Dictionary<string, FloatSerie>();
            this.IndicatorCache = new Dictionary<string, IStockIndicator>();
            this.DecoratorCache = new Dictionary<string, IStockDecorator>();
            this.PaintBarCache = null;
            this.TrailStopCache = null;
            this.TrailCache = null;
            this.dateArray = null;
            this.valueArray = null;

            // This initialisation is here a this method is called in all constructors.
            if (this.BarSerieDictionary == null)
            {
                this.BarSerieDictionary = new SortedDictionary<StockBarDuration, List<StockDailyValue>>();
            }
            // Do not clear bar cache here, ust indicators are concerned.
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
                        if (
                            !LoadData(StockBar.StockBarType.Daily,
                                StockAnalyzerSettings.Properties.Settings.Default.RootFolder))
                        {
                            return false;
                        }
                    }

                    // Force indicator,data,event and other to null;
                    PreInitialise();

                    if (this.barDuration == StockBarDuration.Daily &&
                        !this.BarSerieDictionary.ContainsKey(StockBarDuration.Daily))
                    {
                        this.BarSerieDictionary.Add(StockBarDuration.Daily, this.Values.ToList());
                    }

                    // Flag initialisation as completed
                    this.isInitialised = true;
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
            ResetAllCache();

            float[] openSerie = new float[Values.Count];
            float[] lowSerie = new float[Values.Count];
            float[] highSerie = new float[Values.Count];
            float[] closeSerie = new float[Values.Count];
            float[] avgSerie = new float[Values.Count];
            float[] atrSerie = new float[Values.Count];
            float[] variationSerie = new float[Values.Count];
            float[] volumeSerie = new float[Values.Count];
            float[] upVolumeSerie = new float[Values.Count];
            float[] downVolumeSerie = new float[Values.Count];
            float[] positionSerie = new float[Values.Count];

            int i = 0;
            StockDailyValue previousValue = null;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (previousValue != null)
                {
                    dailyValue.PreviousClose = previousValue.CLOSE;
                    dailyValue.ATR =
                       atrSerie[i] =
                          Math.Max(dailyValue.HIGH - dailyValue.LOW,
                             Math.Max(Math.Abs(dailyValue.HIGH - previousValue.LOW),
                                Math.Abs(previousValue.HIGH - dailyValue.LOW)));

                    if (dailyValue.POSITION == 0.0f)
                        dailyValue.POSITION = previousValue.POSITION; // Fix glitches in POSITION calculation
                }
                else
                {
                    dailyValue.PreviousClose = dailyValue.CLOSE;
                    dailyValue.ATR = atrSerie[i] = dailyValue.HIGH - dailyValue.LOW;
                }
                variationSerie[i] = (dailyValue.CLOSE - dailyValue.PreviousClose) / dailyValue.PreviousClose;
                openSerie[i] = dailyValue.OPEN;
                lowSerie[i] = dailyValue.LOW;
                highSerie[i] = dailyValue.HIGH;
                closeSerie[i] = dailyValue.CLOSE;
                avgSerie[i] = dailyValue.AVG;
                volumeSerie[i] = dailyValue.VOLUME;
                dailyValue.CalculateUpVolume();
                upVolumeSerie[i] = dailyValue.UPVOLUME;
                downVolumeSerie[i] = dailyValue.DOWNVOLUME;
                positionSerie[i] = dailyValue.POSITION;
                i++;
                previousValue = dailyValue;
            }

            StockDailyValue yesterValue = null;
            foreach (StockDailyValue currentValue in this.Values)
            {
                // Calculate variation
                if (yesterValue == null)
                {
                    currentValue.VARIATION = (currentValue.CLOSE - currentValue.OPEN) / currentValue.OPEN;
                }
                else
                {
                    currentValue.VARIATION = (currentValue.CLOSE - yesterValue.CLOSE) / yesterValue.CLOSE;
                }
                currentValue.AMPLITUDE = (currentValue.HIGH - currentValue.LOW) / currentValue.LOW;
                yesterValue = currentValue;
            }

            // Check if has volume on the last 10 bars, othewise, disable it
            this.HasVolume = false;
            for (i = Math.Max(0, this.Values.Count - 10); i < this.Values.Count; i++)
            {
                if (volumeSerie[i] != 0)
                {
                    HasVolume = true;
                    break;
                }
            }

            this.AddSerie(StockDataType.OPEN, new FloatSerie(openSerie, "OPEN"));
            this.AddSerie(StockDataType.LOW, new FloatSerie(lowSerie, "LOW"));
            this.AddSerie(StockDataType.HIGH, new FloatSerie(highSerie, "HIGH"));
            this.AddSerie(StockDataType.CLOSE, new FloatSerie(closeSerie, "CLOSE"));
            this.AddSerie(StockDataType.AVG, new FloatSerie(avgSerie, "AVG"));
            this.AddSerie(StockDataType.ATR, new FloatSerie(atrSerie, "ATR"));
            this.AddSerie(StockDataType.VARIATION, new FloatSerie(variationSerie, "VARIATION"));
            this.AddSerie(StockDataType.VOLUME, new FloatSerie(volumeSerie, "VOLUME"));
            this.AddSerie(StockDataType.UPVOLUME, new FloatSerie(upVolumeSerie, "UPVOLUME"));
            this.AddSerie(StockDataType.DOWNVOLUME, new FloatSerie(downVolumeSerie, "DOWNVOLUME"));
            this.AddSerie(StockDataType.POSITION, new FloatSerie(positionSerie, "POSITION"));
        }

        public void Initialise(StockIndicatorType indicatorType)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            int i = 0;
            switch (indicatorType)
            {
                case StockIndicatorType.NONE:
                    break;
                case StockIndicatorType.VARIATION_REL:
                    {
                        FloatSerie variationSerie = new FloatSerie(closeSerie.Values.Count());
                        i = 0;
                        foreach (StockDailyValue dailyValue in this.Values)
                        {
                            variationSerie.Values[i++] = dailyValue.VARIATION;
                        }
                        this.AddSerie(indicatorType, variationSerie);
                    }
                    break;
                case StockIndicatorType.VARIATION_ABS:
                    {
                        FloatSerie variationSerie = new FloatSerie(closeSerie.Values.Count());
                        variationSerie[0] = 0.0f;
                        for (i = 1; i < closeSerie.Values.Count(); i++)
                        {
                            variationSerie.Values[i] = closeSerie[i] - closeSerie[i - 1];
                        }
                        this.AddSerie(indicatorType, variationSerie);
                    }
                    break;
                case StockIndicatorType.VOLUME_CHURN:
                    {
                        FloatSerie volumeChurnSerie = new FloatSerie(closeSerie.Values.Count());
                        FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                        FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                        FloatSerie volSerie = this.GetSerie(StockDataType.VOLUME);
                        volumeChurnSerie[0] = 0.0f;
                        float range = 0.0f;
                        for (i = 1; i < closeSerie.Values.Count(); i++)
                        {
                            range = Math.Max(closeSerie[i - 1], highSerie[i]) - lowSerie[i] / lowSerie[i];
                            if (range == 0.0f)
                            {
                                volumeChurnSerie[i] = volSerie[i] / 0.1f;
                            }
                            else
                            {
                                volumeChurnSerie[i] = volSerie[i] / range;
                            }
                        }
                        this.AddSerie(indicatorType, volumeChurnSerie);
                    }
                    break;
                #region Pul/Call ratio indicators
                case StockIndicatorType.VOLATILITY_STDEV:
                    {
                        this.AddSerie(indicatorType, this.CalculateVolatilityStdev(30));
                    }
                    break;
                case StockIndicatorType.VOLATILITY:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockDataType.CLOSE).CalculateVolatility(12));
                    }
                    break;
                case StockIndicatorType.VOLATILITY_BBLOW:
                case StockIndicatorType.VOLATILITY_BBUP:
                    {
                        FloatSerie volatilityFloatSerie = this.GetSerie(StockIndicatorType.VOLATILITY);

                        FloatSerie volatilityBBUp = new FloatSerie(volatilityFloatSerie.Count);
                        FloatSerie volatilityBBLow = new FloatSerie(volatilityFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.VOLATILITY).CalculateBB(volatilityFloatSerie.CalculateMA(20), 20, 2, -2, ref volatilityBBUp, ref volatilityBBLow);
                        this.AddSerie(StockIndicatorType.VOLATILITY_BBLOW, volatilityBBLow);
                        this.AddSerie(StockIndicatorType.VOLATILITY_BBUP, volatilityBBUp);
                    }
                    break;
                #region VIX INDICATORS
                case StockIndicatorType.VIX:
                    {
                        FloatSerie vixFloatSerie = null;
                        if (this.StockName == "VIX")
                        {
                            vixFloatSerie = this.GetSerie(StockDataType.CLOSE);
                        }
                        else
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("VIX"))
                            {
                                StockSerie vixSerie = StockDictionary.StockDictionarySingleton["VIX"];
                                vixFloatSerie = this.GenerateSecondarySerieFromOtherSerie(vixSerie, StockDataType.CLOSE);
                            }
                        }
                        this.AddSerie(indicatorType, vixFloatSerie);
                    }
                    break;
                case StockIndicatorType.VIX_BBLOW:
                case StockIndicatorType.VIX_BBUP:
                    {
                        // Use the indicator to instead of the StockSerie "VIX" to prevent date issues
                        FloatSerie vixFloatSerie = this.GetSerie(StockIndicatorType.VIX);

                        FloatSerie vixBBUp = new FloatSerie(vixFloatSerie.Count);
                        FloatSerie vixBBLow = new FloatSerie(vixFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.VIX).CalculateBB(vixFloatSerie.CalculateMA(20), 20, 2, -2, ref vixBBUp, ref vixBBLow);
                        this.AddSerie(StockIndicatorType.VIX_BBLOW, vixBBLow);
                        this.AddSerie(StockIndicatorType.VIX_BBUP, vixBBUp);
                    }
                    break;
                case StockIndicatorType.VIX_MA5:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.VIX).CalculateMA(5));
                    }
                    break;
                #endregion
                #region GVZ INDICATORS
                case StockIndicatorType.GVZ:
                    {
                        FloatSerie gvzFloatSerie = null;
                        if (this.StockName == "GVZ")
                        {
                            gvzFloatSerie = this.GetSerie(StockDataType.CLOSE);
                        }
                        else
                        {
                            StockSerie gvzSerie = StockDictionary.StockDictionarySingleton["GVZ"];
                            gvzFloatSerie = this.GenerateSecondarySerieFromOtherSerie(gvzSerie, StockDataType.CLOSE);
                        }
                        this.AddSerie(indicatorType, gvzFloatSerie);
                    }
                    break;
                case StockIndicatorType.GVZ_BBLOW:
                case StockIndicatorType.GVZ_BBUP:
                    {
                        // Use the indicator to instead of the StockSerie "GVZ" to prevent date issues
                        FloatSerie gvzFloatSerie = this.GetSerie(StockIndicatorType.GVZ);

                        FloatSerie gvzBBUp = new FloatSerie(gvzFloatSerie.Count);
                        FloatSerie gvzBBLow = new FloatSerie(gvzFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.GVZ).CalculateBB(gvzFloatSerie.CalculateMA(20), 20, 2, -2, ref gvzBBUp, ref gvzBBLow);
                        this.AddSerie(StockIndicatorType.GVZ_BBLOW, gvzBBLow);
                        this.AddSerie(StockIndicatorType.GVZ_BBUP, gvzBBUp);
                    }
                    break;
                case StockIndicatorType.GVZ_MA5:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.GVZ).CalculateMA(5));
                    }
                    break;
                #endregion
                #region EVZ INDICATORS
                case StockIndicatorType.EVZ:
                    {
                        FloatSerie evzFloatSerie = null;
                        if (this.StockName == "EVZ")
                        {
                            evzFloatSerie = this.GetSerie(StockDataType.CLOSE);
                        }
                        else
                        {
                            StockSerie evzSerie = StockDictionary.StockDictionarySingleton["EVZ"];
                            evzFloatSerie = this.GenerateSecondarySerieFromOtherSerie(evzSerie, StockDataType.CLOSE);
                        }
                        this.AddSerie(indicatorType, evzFloatSerie);
                    }
                    break;
                case StockIndicatorType.EVZ_BBLOW:
                case StockIndicatorType.EVZ_BBUP:
                    {
                        // Use the indicator to instead of the StockSerie "EVZ" to prevent date issues
                        FloatSerie evzFloatSerie = this.GetSerie(StockIndicatorType.EVZ);

                        FloatSerie evzBBUp = new FloatSerie(evzFloatSerie.Count);
                        FloatSerie evzBBLow = new FloatSerie(evzFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.EVZ).CalculateBB(evzFloatSerie.CalculateMA(20), 20, 2, -2, ref evzBBUp, ref evzBBLow);
                        this.AddSerie(StockIndicatorType.EVZ_BBLOW, evzBBLow);
                        this.AddSerie(StockIndicatorType.EVZ_BBUP, evzBBUp);
                    }
                    break;
                case StockIndicatorType.EVZ_MA5:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.EVZ).CalculateMA(5));
                    }
                    break;
                #endregion
                #region OVX INDICATORS
                case StockIndicatorType.OVX:
                    {
                        FloatSerie OVXFloatSerie = null;
                        if (this.StockName == "OVX")
                        {
                            OVXFloatSerie = this.GetSerie(StockDataType.CLOSE);
                        }
                        else
                        {
                            StockSerie OVXSerie = StockDictionary.StockDictionarySingleton["OVX"];
                            OVXFloatSerie = this.GenerateSecondarySerieFromOtherSerie(OVXSerie, StockDataType.CLOSE);
                        }
                        this.AddSerie(indicatorType, OVXFloatSerie);
                    }
                    break;
                case StockIndicatorType.OVX_BBLOW:
                case StockIndicatorType.OVX_BBUP:
                    {
                        // Use the indicator to instead of the StockSerie "OVX" to prevent date issues
                        FloatSerie OVXFloatSerie = this.GetSerie(StockIndicatorType.OVX);

                        FloatSerie OVXBBUp = new FloatSerie(OVXFloatSerie.Count);
                        FloatSerie OVXBBLow = new FloatSerie(OVXFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.OVX).CalculateBB(OVXFloatSerie.CalculateMA(20), 20, 2, -2, ref OVXBBUp, ref OVXBBLow);
                        this.AddSerie(StockIndicatorType.OVX_BBLOW, OVXBBLow);
                        this.AddSerie(StockIndicatorType.OVX_BBUP, OVXBBUp);
                    }
                    break;
                case StockIndicatorType.OVX_MA5:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.OVX).CalculateMA(5));
                    }
                    break;
                #endregion
                #region CHAIKIN MONEY FLOW
                case StockIndicatorType.CMF:
                    {
                        this.AddSerie(indicatorType, this.CalculateChaikinMoneyFlow(21));
                    }
                    break;

                case StockIndicatorType.CMF_BBLOW:
                case StockIndicatorType.CMF_BBUP:
                    {
                        // Use the indicator to instead of the StockSerie "CMF" to prevent date issues
                        FloatSerie cmfFloatSerie = this.GetSerie(StockIndicatorType.CMF);

                        FloatSerie cmfBBUp = new FloatSerie(cmfFloatSerie.Count);
                        FloatSerie cmfBBLow = new FloatSerie(cmfFloatSerie.Count);
                        this.GetSerie(StockIndicatorType.CMF).CalculateBB(cmfFloatSerie.CalculateMA(20), 20, 2, -2, ref cmfBBUp, ref cmfBBLow);
                        this.AddSerie(StockIndicatorType.CMF_BBLOW, cmfBBLow);
                        this.AddSerie(StockIndicatorType.CMF_BBUP, cmfBBUp);
                    }
                    break;
                case StockIndicatorType.CMF_MA5:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.CMF).CalculateMA(5));
                    }
                    break;
                #endregion
                #endregion
                #region AMPLITUDE INDICATORS
                case StockIndicatorType.AMPLITUDE:
                    FloatSerie amplitudeSerie = new FloatSerie(this.Values.Count);
                    i = 0;
                    foreach (StockDailyValue currentValue in this.Values)
                    {
                        // Calculate amplitude
                        amplitudeSerie.Values[i++] = currentValue.AMPLITUDE;
                    }
                    this.AddSerie(StockIndicatorType.AMPLITUDE, amplitudeSerie);
                    break;
                case StockIndicatorType.AMPLITUDE_EMA6:
                    this.AddSerie(StockIndicatorType.AMPLITUDE_EMA6, this.GetSerie(StockIndicatorType.AMPLITUDE).CalculateEMA(6));
                    break;
                #endregion
                #region DIRECTIONAL MOVEMENT
                case StockIndicatorType.DI_UP:
                case StockIndicatorType.DI_DOWN:
                case StockIndicatorType.DI_UP_EMA3:
                case StockIndicatorType.DI_DOWN_EMA3:
                case StockIndicatorType.ADX:
                    {
                        FloatSerie DIUpSerie = new FloatSerie(this.Values.Count);
                        FloatSerie DIDownSerie = new FloatSerie(this.Values.Count);
                        FloatSerie ADXSerie = new FloatSerie(this.Values.Count);
                        int period = 10;
                        int previousIndex = 0;

                        StockDailyValue dailyValue = null;
                        StockDailyValue previousValue = this.Values.First();
                        for (i = 0; i < this.Values.Count; i++)
                        {
                            DIUpSerie.Values[i] = 0.0f;
                            DIDownSerie.Values[i] = 0.0f;
                            if (i <= period)
                            {
                                previousValue = this.Values.First();
                                for (int j = 1; j <= i; j++)
                                {
                                    dailyValue = this.ValueArray[j];
                                    if (dailyValue.VARIATION > 0)
                                    {
                                        DIUpSerie.Values[i] += dailyValue.CLOSE - previousValue.CLOSE;
                                    }
                                    else
                                    {
                                        DIDownSerie.Values[i] += previousValue.CLOSE - dailyValue.CLOSE;
                                    }
                                    previousValue = dailyValue;
                                }
                                if (DIUpSerie.Values[i] + DIDownSerie.Values[i] != 0.0f)
                                {
                                    ADXSerie.Values[i] = 100.0f * Math.Abs(DIUpSerie.Values[i] - DIDownSerie.Values[i]) / (DIUpSerie.Values[i] + DIDownSerie.Values[i]);
                                }
                                else
                                {
                                    ADXSerie.Values[i] = 50.0f;
                                }
                            }
                            else
                            {
                                previousIndex = i - period;
                                DIUpSerie.Values[i] = DIUpSerie.Values[i - 1];
                                DIDownSerie.Values[i] = DIDownSerie.Values[i - 1];

                                previousValue = this.ValueArray[previousIndex - 1];
                                dailyValue = this.ValueArray[previousIndex];
                                if (dailyValue.VARIATION > 0)
                                {
                                    DIUpSerie.Values[i] -= dailyValue.CLOSE - previousValue.CLOSE;
                                }
                                else
                                {
                                    DIDownSerie.Values[i] -= previousValue.CLOSE - dailyValue.CLOSE;
                                }

                                previousValue = this.ValueArray[i - 1];
                                dailyValue = this.ValueArray[i];
                                if (dailyValue.VARIATION > 0)
                                {
                                    DIUpSerie.Values[i] += dailyValue.CLOSE - previousValue.CLOSE;
                                }
                                else
                                {
                                    DIDownSerie.Values[i] += previousValue.CLOSE - dailyValue.CLOSE;
                                }

                                ADXSerie.Values[i] = 100.0f * Math.Abs(DIUpSerie.Values[i] - DIDownSerie.Values[i]) / (DIUpSerie.Values[i] + DIDownSerie.Values[i]);
                            }
                        }
                        this.AddSerie(StockIndicatorType.DI_UP, DIUpSerie);
                        this.AddSerie(StockIndicatorType.DI_DOWN, DIDownSerie);
                        this.AddSerie(StockIndicatorType.DI_UP_EMA3, DIUpSerie.CalculateEMA(3));
                        this.AddSerie(StockIndicatorType.DI_DOWN_EMA3, DIDownSerie.CalculateEMA(3));
                        this.AddSerie(StockIndicatorType.ADX, ADXSerie.CalculateEMA(period));
                    }
                    break;
                case StockIndicatorType.DI_HIGH_UP:
                case StockIndicatorType.DI_LOW_DOWN:
                    {
                        FloatSerie DIUpSerie = new FloatSerie(this.Values.Count);
                        FloatSerie DIDownSerie = new FloatSerie(this.Values.Count);
                        int period = 10;

                        StockDailyValue dailyValue = null;
                        StockDailyValue previousValue = this.Values.First();
                        int previousIndex = 0;
                        for (i = 0; i < this.Values.Count; i++)
                        {
                            DIUpSerie.Values[i] = 0.0f;
                            DIDownSerie.Values[i] = 0.0f;
                            if (i <= period)
                            {
                                previousValue = this.Values.First();
                                for (int j = 1; j <= i; j++)
                                {
                                    dailyValue = this.ValueArray[j];
                                    if (dailyValue.HIGH > previousValue.HIGH)
                                    {
                                        DIUpSerie.Values[i] += dailyValue.HIGH - previousValue.HIGH;
                                    }
                                    if (dailyValue.LOW < previousValue.LOW)
                                    {
                                        DIDownSerie.Values[i] += previousValue.LOW - dailyValue.LOW;
                                    }
                                    previousValue = dailyValue;
                                }
                            }
                            else
                            {
                                previousIndex = i - period;
                                DIUpSerie.Values[i] = DIUpSerie.Values[i - 1];
                                DIDownSerie.Values[i] = DIDownSerie.Values[i - 1];

                                previousValue = this.ValueArray[previousIndex - 1];
                                dailyValue = this.ValueArray[previousIndex];
                                if (dailyValue.HIGH > previousValue.HIGH)
                                {
                                    DIUpSerie.Values[i] -= dailyValue.HIGH - previousValue.HIGH;
                                }
                                if (dailyValue.LOW < previousValue.LOW)
                                {
                                    DIDownSerie.Values[i] -= previousValue.LOW - dailyValue.LOW;
                                }

                                previousValue = this.ValueArray[i - 1];
                                dailyValue = this.ValueArray[i];
                                if (dailyValue.HIGH > previousValue.HIGH)
                                {
                                    DIUpSerie.Values[i] += dailyValue.HIGH - previousValue.HIGH;
                                }
                                if (dailyValue.LOW < previousValue.LOW)
                                {
                                    DIDownSerie.Values[i] += previousValue.LOW - dailyValue.LOW;
                                }
                            }
                        }
                        this.AddSerie(StockIndicatorType.DI_HIGH_UP, DIUpSerie);
                        this.AddSerie(StockIndicatorType.DI_LOW_DOWN, DIDownSerie);
                        this.AddSerie(StockIndicatorType.DI_HIGH_UP_EMA3, DIUpSerie.CalculateEMA(3));
                        this.AddSerie(StockIndicatorType.DI_LOW_DOWN_EMA3, DIDownSerie.CalculateEMA(3));
                    }
                    break;
                #endregion
                #region STOCHASTICS - OSCILLATOR
                case StockIndicatorType.FAST_OSCILLATOR_14:
                    FloatSerie fastOscillator = this.CalculateFastOscillator(14);
                    this.AddSerie(StockIndicatorType.FAST_OSCILLATOR_14, fastOscillator);
                    this.AddSerie(StockIndicatorType.SLOW_OSCILLATOR_14_EMA3, fastOscillator.CalculateEMA(5));
                    this.AddSerie(StockIndicatorType.SLOW_OSCILLATOR_14_MA5, fastOscillator.CalculateMA(10));
                    break;
                case StockIndicatorType.FAST_OSCILLATOR_14_EX:
                    // We've got to consider the RSI range:
                    // [0..25]      => Oversold
                    // [25..75]     => Low meaning
                    // [75..100]    => Overbought
                    // We apply a sin2 smoothing to applify value over 75 or below 25.
                    this.AddSerie(indicatorType,
                        this.GetSerie(StockIndicatorType.FAST_OSCILLATOR_14).Normalise(0.0f, 100.0f, -1.0f, 1.0f).
                        ApplySmoothing(StockMathToolkit.SmoothingType.Sigmoid3, 2.0f, 1.0f));
                    break;
                case StockIndicatorType.SLOW_OSCILLATOR_14_EMA3:
                    this.Initialise(StockIndicatorType.FAST_OSCILLATOR_14);
                    break;
                case StockIndicatorType.SLOW_OSCILLATOR_14_MA5:
                    this.Initialise(StockIndicatorType.FAST_OSCILLATOR_14);
                    break;
                #endregion
                #region RSI
                case StockIndicatorType.RSI:
                    if (closeSerie.Min <= 0.0f)
                    {
                        this.AddSerie(indicatorType, closeSerie.CalculateRSI(RSITimePeriod, false));
                    }
                    else
                    {
                        this.AddSerie(indicatorType, closeSerie.CalculateRSI(RSITimePeriod, true));
                    }
                    break;
                case StockIndicatorType.RSI_EX:
                    // We've got to consider the RSI range:
                    // [0..25]      => Oversold
                    // [25..75]     => Low meaning
                    // [75..100]    => Overbought
                    // We apply a sin2 smoothing to applify value over 75 or below 25.
                    this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.RSI).Normalise(-1.0f, 1.0f).
                        ApplySmoothing(StockMathToolkit.SmoothingType.Sigmoid3, 2.0f, 1.0f));
                    break;
                case StockIndicatorType.RSI_EMA5:
                    this.AddSerie(StockIndicatorType.RSI_EMA5, this.GetSerie(StockIndicatorType.RSI).CalculateEMA(5));
                    break;
                case StockIndicatorType.RSI_TREND:
                    this.AddSerie(StockIndicatorType.RSI_TREND, this.GetSerie(StockIndicatorType.RSI).CalculateRelativeTrend());
                    break;
                #endregion
                #region EXTREMUM INDICATORS
                case StockIndicatorType.HIGHEST_SINCE_DAYS:
                    {
                        FloatSerie highestSinceDays = new FloatSerie(closeSerie.Values.Count());
                        i = 0;
                        int j = 0;
                        foreach (StockDailyValue dailyValue in this.Values)
                        {
                            if (i == 0)
                            {
                                highestSinceDays.Values[0] = 0;
                            }
                            else
                            {
                                for (j = i - 1; j >= 0; j--)
                                {
                                    if (dailyValue.HIGH < this.ValueArray[j].HIGH)
                                    {
                                        break;
                                    }
                                }
                                highestSinceDays.Values[i] = i - j + 1;
                            }
                            i++;
                        }
                        this.AddSerie(StockIndicatorType.HIGHEST_SINCE_DAYS, highestSinceDays);
                    }
                    break;
                case StockIndicatorType.LOWEST_SINCE_DAYS:
                    {
                        FloatSerie lowestSinceDays = new FloatSerie(closeSerie.Values.Count());
                        i = 0;
                        int j = 0;
                        foreach (StockDailyValue dailyValue in this.Values)
                        {
                            if (i == 0)
                            {
                                lowestSinceDays.Values[0] = 0;
                            }
                            else
                            {
                                for (j = i - 1; j >= 0; j--)
                                {
                                    if (dailyValue.LOW > this.ValueArray[j].LOW)
                                    {
                                        break;
                                    }
                                }
                                lowestSinceDays.Values[i] = i - j + 1;
                            }
                            i++;
                        }
                        this.AddSerie(StockIndicatorType.LOWEST_SINCE_DAYS, lowestSinceDays);
                    }
                    break;
                #endregion
                #region probability
                case StockIndicatorType.VARIATION_MA20:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockIndicatorType.VARIATION_ABS).CalculateMA(12));
                    }
                    break;
                case StockIndicatorType.VOLUME_VAR_RATIO:
                    {
                        this.AddSerie(indicatorType, this.GetSerie(StockDataType.VOLUME) / this.GetSerie(StockIndicatorType.AMPLITUDE));
                    }
                    break;
                #endregion
                #region COT INDICATORS
                case StockIndicatorType.COT_COMMERCIAL:
                    if (this.CotSerie != null)
                    {
                        this.AddSerie(indicatorType, this.GetCotSerie(CotValue.CotValueType.CommercialHedgerPosition));
                    }
                    break;
                case StockIndicatorType.COT_LARGE_SPECULATOR:
                    if (this.CotSerie != null)
                    {
                        this.AddSerie(indicatorType, this.GetCotSerie(CotValue.CotValueType.LargeSpeculatorPosition));
                    }
                    break;
                case StockIndicatorType.COT_SMALL_SPECULATOR:
                    if (this.CotSerie != null)
                    {
                        this.AddSerie(indicatorType, this.GetCotSerie(CotValue.CotValueType.SmallSpeculatorPosition));
                    }
                    break;
                #endregion
                default:
                    break;
            }
        }
        public void Initialise(StockEvent.EventType eventType)
        {
            int index = 0;
            BoolSerie eventSerie = new BoolSerie(this.Count, eventType.ToString());
            StockDailyValue previousValue = null;
            foreach (StockDailyValue currentValue in this.Values)
            {
                eventSerie[index] = DetectEvent(eventType, index, currentValue, previousValue);
                previousValue = currentValue;
                index++;
            }
            this.AddSerie(eventType, eventSerie);
        }
        private bool DetectEvent(StockEvent.EventType eventType, int index, StockDailyValue currentValue, StockDailyValue previousValue)
        {
            this.Initialise();
            if (index > 0)
            {
                switch (eventType)
                {
                    #region Basic events
                    //case StockEvent.EventType.BBOverrun:
                    //    {
                    //        // Detect BB Events
                    //        float currentUpperBB = this.GetSerie(StockDataType.UPPERBB).Values.ElementAt(index);
                    //        if (currentValue.HIGH > currentUpperBB)
                    //        {
                    //            return true;
                    //        }
                    //    }
                    //    break;
                    //case StockEvent.EventType.BBUnderrun:
                    //    {
                    //        // Detect BB Events
                    //        float currentLowerBB = this.GetSerie(StockDataType.LOWERBB).Values.ElementAt(index);

                    //        if (currentValue.LOW < currentLowerBB)
                    //        {
                    //            return true;
                    //        }
                    //    }
                    //    break;
                    #region VIX Events
                    case StockEvent.EventType.VIXOverrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("VIX"))
                            {
                                StockSerie vixSerie = StockDictionary.StockDictionarySingleton["VIX"];
                                if (vixSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int vixIndex = vixSerie.IndexOf(this.Keys.ElementAt(index));
                                    return vixSerie.DetectEvent(StockEvent.EventType.BBOverrun, vixIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.VIXUnderrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("VIX"))
                            {
                                StockSerie vixSerie = StockDictionary.StockDictionarySingleton["VIX"];
                                if (vixSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int vixIndex = vixSerie.IndexOf(this.Keys.ElementAt(index));
                                    return vixSerie.DetectEvent(StockEvent.EventType.BBUnderrun, vixIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.GVZOverrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("GVZ"))
                            {
                                StockSerie gvzSerie = StockDictionary.StockDictionarySingleton["GVZ"];
                                if (gvzSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int gvzIndex = gvzSerie.IndexOf(this.Keys.ElementAt(index));
                                    return gvzSerie.DetectEvent(StockEvent.EventType.BBOverrun, gvzIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.GVZUnderrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("GVZ"))
                            {
                                StockSerie gvzSerie = StockDictionary.StockDictionarySingleton["GVZ"];
                                if (gvzSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int gvzIndex = gvzSerie.IndexOf(this.Keys.ElementAt(index));
                                    return gvzSerie.DetectEvent(StockEvent.EventType.BBUnderrun, gvzIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.OVXOverrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("OVX"))
                            {
                                StockSerie ovxSerie = StockDictionary.StockDictionarySingleton["OVX"];
                                if (ovxSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int ovxIndex = ovxSerie.IndexOf(this.Keys.ElementAt(index));
                                    return ovxSerie.DetectEvent(StockEvent.EventType.BBOverrun, ovxIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.OVXUnderrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("OVX"))
                            {
                                StockSerie ovxSerie = StockDictionary.StockDictionarySingleton["OVX"];
                                if (ovxSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int ovxIndex = ovxSerie.IndexOf(this.Keys.ElementAt(index));
                                    return ovxSerie.DetectEvent(StockEvent.EventType.BBUnderrun, ovxIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.EVZOverrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("EVZ"))
                            {
                                StockSerie evzSerie = StockDictionary.StockDictionarySingleton["EVZ"];
                                if (evzSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int evzIndex = evzSerie.IndexOf(this.Keys.ElementAt(index));
                                    return evzSerie.DetectEvent(StockEvent.EventType.BBOverrun, evzIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.EVZUnderrun:
                        {
                            if (StockDictionary.StockDictionarySingleton.ContainsKey("EVZ"))
                            {
                                StockSerie evzSerie = StockDictionary.StockDictionarySingleton["EVZ"];
                                if (evzSerie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int evzIndex = evzSerie.IndexOf(this.Keys.ElementAt(index));
                                    return evzSerie.DetectEvent(StockEvent.EventType.BBUnderrun, evzIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.EPCROverrun:
                        {
                            string stockName = "PCR.EQUITY";
                            if (StockDictionary.StockDictionarySingleton.ContainsKey(stockName))
                            {
                                StockSerie serie = StockDictionary.StockDictionarySingleton[stockName];
                                if (serie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int evzIndex = serie.IndexOf(this.Keys.ElementAt(index));
                                    return serie.DetectEvent(StockEvent.EventType.BBOverrun, evzIndex);
                                }
                            }
                        }
                        break;
                    case StockEvent.EventType.EPCRUnderrun:
                        {
                            string stockName = "PCR.EQUITY";
                            if (StockDictionary.StockDictionarySingleton.ContainsKey(stockName))
                            {
                                StockSerie serie = StockDictionary.StockDictionarySingleton[stockName];
                                if (serie.Keys.Contains(this.Keys.ElementAt(index)))
                                {
                                    int evzIndex = serie.IndexOf(this.Keys.ElementAt(index));
                                    return serie.DetectEvent(StockEvent.EventType.BBUnderrun, evzIndex);
                                }
                            }
                        }
                        break;
                    #endregion
                    #endregion
                    case StockEvent.EventType.GapUp:
                        if (currentValue.LOW > previousValue.HIGH)
                        {
                            return true;
                        }
                        break;
                    case StockEvent.EventType.GapDown:
                        if (currentValue.HIGH < previousValue.LOW)
                        {
                            return true;
                        }
                        break;
                    case StockEvent.EventType.OopsUp:
                        {
                            if (currentValue.CLOSE > currentValue.OPEN && currentValue.OPEN < previousValue.LOW && currentValue.CLOSE > previousValue.HIGH)
                            {
                                return true;
                            }
                        }
                        break;
                    case StockEvent.EventType.OopsDown:
                        {
                            if (currentValue.CLOSE < currentValue.OPEN && currentValue.OPEN > previousValue.HIGH && currentValue.CLOSE < previousValue.LOW)
                            {
                                return true;
                            }
                        }
                        break;
                    #region VOLUME PATTERNS
                    case StockEvent.EventType.VolPro:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolAm:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolNoDemand:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0 || index < 3)
                            {
                                return false;
                            }
                            else
                            {
                                return (currentValue.CLOSE <= previousValue.CLOSE || currentValue.CLOSE <= currentValue.OPEN) && currentValue.VOLUME < previousValue.VOLUME && currentValue.HIGH > previousValue.HIGH && currentValue.Range < previousValue.Range;
                            }
                        }
                    case StockEvent.EventType.VolNoSupply:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0 || index < 3)
                            {
                                return false;
                            }
                            else
                            {
                                return (currentValue.CLOSE >= previousValue.CLOSE || currentValue.CLOSE >= currentValue.OPEN) && currentValue.VOLUME < previousValue.VOLUME && currentValue.LOW < previousValue.LOW && currentValue.Range < previousValue.Range;
                            }
                        }
                    case StockEvent.EventType.VolStoppingUp:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            else
                            {
                                return currentValue.VOLUME > previousValue.VOLUME && currentValue.HIGH > previousValue.HIGH && currentValue.Range < previousValue.Range;
                            }
                        }
                    case StockEvent.EventType.VolStoppingDown:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            else
                            {
                                return currentValue.VOLUME > previousValue.VOLUME && currentValue.LOW < previousValue.LOW && currentValue.Range < previousValue.Range;
                            }
                        }
                    case StockEvent.EventType.VolProfitTakingUp:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0 || index < 3)
                            {
                                return false;
                            }
                            else
                            {
                                return (currentValue.CLOSE <= previousValue.CLOSE || currentValue.CLOSE <= currentValue.OPEN) && currentValue.VOLUME > previousValue.VOLUME && currentValue.HIGH > previousValue.HIGH && currentValue.Range > previousValue.Range;
                            }
                        }
                    case StockEvent.EventType.VolProfitTakingDown:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0 || index < 3)
                            {
                                return false;
                            }
                            else
                            {
                                return (currentValue.CLOSE >= previousValue.CLOSE || currentValue.CLOSE >= currentValue.OPEN) && currentValue.VOLUME > previousValue.VOLUME && currentValue.LOW < previousValue.LOW && currentValue.Range < previousValue.Range;
                            }
                        }
                    #endregion
                    #region Volume Patterns
                    case StockEvent.EventType.VolLowVolume:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            int lookback = 20;
                            bool use2Bars = true;
                            InitVolumeCacheSeries();

                            FloatSerie Value3 = this.GetSerie("Value3");
                            if (Value3[index] == Value3.GetMin(Math.Max(0, index - lookback), index)) return true;

                            if (use2Bars)
                            {
                                FloatSerie Value13 = this.GetSerie("Value13");
                                if (Value13[index] == Value13.GetMin(Math.Max(0, index - lookback), index)) return true;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolClimaxUp:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            int lookback = 20;
                            bool use2Bars = true;
                            InitVolumeCacheSeries();

                            FloatSerie Value4 = this.GetSerie("Value4");
                            if (currentValue.CLOSE > currentValue.OPEN && Value4[index] == Value4.GetMax(Math.Max(0, index - lookback), index)) return true;
                            FloatSerie Value5 = this.GetSerie("Value5");
                            if (currentValue.CLOSE > currentValue.OPEN && Value5[index] == Value5.GetMax(Math.Max(0, index - lookback), index)) return true;
                            FloatSerie Value10 = this.GetSerie("Value10");
                            if (currentValue.CLOSE > currentValue.OPEN && Value10[index] == Value10.GetMin(Math.Max(0, index - lookback), index)) return true;
                            FloatSerie Value11 = this.GetSerie("Value11");
                            if (currentValue.CLOSE > currentValue.OPEN && Value11[index] == Value11.GetMin(Math.Max(0, index - lookback), index)) return true;
                            if (use2Bars)
                            {
                                FloatSerie Value14 = this.GetSerie("Value14");
                                if (currentValue.CLOSE > currentValue.OPEN && previousValue.CLOSE > previousValue.OPEN && Value14[index] == Value14.GetMax(Math.Max(0, index - lookback), index)) return true;
                                FloatSerie Value15 = this.GetSerie("Value15");
                                if (currentValue.CLOSE > currentValue.OPEN && previousValue.CLOSE > previousValue.OPEN && Value15[index] == Value15.GetMax(Math.Max(0, index - lookback), index)) return true;
                                FloatSerie Value20 = this.GetSerie("Value20");
                                if (currentValue.CLOSE > currentValue.OPEN && previousValue.CLOSE > previousValue.OPEN && Value20[index] == Value20.GetMin(Math.Max(0, index - lookback), index)) return true;
                                FloatSerie Value21 = this.GetSerie("Value21");
                                if (currentValue.CLOSE > currentValue.OPEN && previousValue.CLOSE > previousValue.OPEN && Value21[index] == Value21.GetMin(Math.Max(0, index - lookback), index)) return true;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolClimaxDown:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            int lookback = 20;
                            bool use2Bars = true;
                            InitVolumeCacheSeries();
                            FloatSerie Value6 = this.GetSerie("Value6");
                            FloatSerie Value7 = this.GetSerie("Value7");
                            FloatSerie Value8 = this.GetSerie("Value8");
                            FloatSerie Value9 = this.GetSerie("Value9");

                            if (currentValue.CLOSE < currentValue.OPEN && Value6[index] == Value6.GetMax(Math.Max(0, index - lookback), index)) return true;
                            if (currentValue.CLOSE < currentValue.OPEN && Value7[index] == Value7.GetMax(Math.Max(0, index - lookback), index)) return true;
                            if (currentValue.CLOSE < currentValue.OPEN && Value8[index] == Value8.GetMin(Math.Max(0, index - lookback), index)) return true;
                            if (currentValue.CLOSE < currentValue.OPEN && Value9[index] == Value9.GetMin(Math.Max(0, index - lookback), index)) return true;
                            if (use2Bars)
                            {
                                FloatSerie Value16 = this.GetSerie("Value16");
                                FloatSerie Value17 = this.GetSerie("Value17");
                                FloatSerie Value18 = this.GetSerie("Value18");
                                FloatSerie Value19 = this.GetSerie("Value19");
                                if (currentValue.CLOSE < currentValue.OPEN && previousValue.CLOSE < previousValue.OPEN && Value16[index] == Value16.GetMax(Math.Max(0, index - lookback), index)) return true;
                                if (currentValue.CLOSE < currentValue.OPEN && previousValue.CLOSE < previousValue.OPEN && Value17[index] == Value17.GetMax(Math.Max(0, index - lookback), index)) return true;
                                if (currentValue.CLOSE < currentValue.OPEN && previousValue.CLOSE < previousValue.OPEN && Value18[index] == Value18.GetMin(Math.Max(0, index - lookback), index)) return true;
                                if (currentValue.CLOSE < currentValue.OPEN && previousValue.CLOSE < previousValue.OPEN && Value19[index] == Value19.GetMin(Math.Max(0, index - lookback), index)) return true;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolChurn:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            int lookback = 20;
                            bool use2Bars = true;
                            InitVolumeCacheSeries();
                            FloatSerie Value12 = this.GetSerie("Value12");

                            if (Value12[index] == Value12.GetMax(Math.Max(0, index - lookback), index)) return true;
                            if (use2Bars)
                            {
                                FloatSerie Value22 = this.GetSerie("Value22");
                                if (Value22[index] == Value22.GetMax(Math.Max(0, index - lookback), index)) return true;
                            }
                        }
                        break;
                    case StockEvent.EventType.VolClimaxChurn:
                        {
                            if (!this.HasVolume || currentValue.VOLUME == 0)
                            {
                                return false;
                            }
                            InitVolumeCacheSeries();

                            if (DetectEvent(StockEvent.EventType.VolChurn, index) &&
                                (DetectEvent(StockEvent.EventType.VolClimaxUp, index) || DetectEvent(StockEvent.EventType.VolClimaxDown, index)))
                                return true;
                        }
                        break;
                    #endregion
                    default:
                        throw new System.ArgumentException(eventType + " detection is not implemented !!!");
                }
            }
            return false;
        }
        private void InitVolumeCacheSeries()
        {
            FloatSerie Value3 = this.GetSerie("Value3");
            if ((Value3 = this.GetSerie("Value3")) != null)
            {
                return;
            }
            Value3 = new FloatSerie(this.Count);
            FloatSerie Value4 = new FloatSerie(this.Count);
            FloatSerie Value5 = new FloatSerie(this.Count);
            FloatSerie Value6 = new FloatSerie(this.Count);
            FloatSerie Value7 = new FloatSerie(this.Count);
            FloatSerie Value8 = new FloatSerie(this.Count);
            FloatSerie Value9 = new FloatSerie(this.Count);
            FloatSerie Value10 = new FloatSerie(this.Count);
            FloatSerie Value11 = new FloatSerie(this.Count);
            FloatSerie Value12 = new FloatSerie(this.Count);
            FloatSerie Value13 = new FloatSerie(this.Count);
            FloatSerie Value14 = new FloatSerie(this.Count);
            FloatSerie Value15 = new FloatSerie(this.Count);
            FloatSerie Value16 = new FloatSerie(this.Count);
            FloatSerie Value17 = new FloatSerie(this.Count);
            FloatSerie Value18 = new FloatSerie(this.Count);
            FloatSerie Value19 = new FloatSerie(this.Count);
            FloatSerie Value20 = new FloatSerie(this.Count);
            FloatSerie Value21 = new FloatSerie(this.Count);
            FloatSerie Value22 = new FloatSerie(this.Count);

            int i = 0;
            StockDailyValue previousValue = null;
            float highest2, lowest2, range2, upVol2, downVol2; // Highest && lowest on 2 bars
            foreach (StockDailyValue dailyValue in this.Values)
            {
                Value3[i] = Math.Abs(dailyValue.UPVOLUME + dailyValue.DOWNVOLUME);
                Value4[i] = dailyValue.UPVOLUME * dailyValue.Range;
                Value5[i] = (dailyValue.UPVOLUME - dailyValue.DOWNVOLUME) * dailyValue.Range;
                Value6[i] = dailyValue.DOWNVOLUME * dailyValue.Range;
                Value7[i] = (dailyValue.DOWNVOLUME - dailyValue.UPVOLUME) * dailyValue.Range;
                if (dailyValue.Range != 0)
                {
                    Value8[i] = dailyValue.UPVOLUME / dailyValue.Range;
                    Value9[i] = (dailyValue.UPVOLUME - dailyValue.DOWNVOLUME) / dailyValue.Range;
                    Value10[i] = dailyValue.DOWNVOLUME / dailyValue.Range;
                    Value11[i] = (dailyValue.DOWNVOLUME - dailyValue.UPVOLUME) / dailyValue.Range;
                    Value12[i] = Value3[i] / dailyValue.Range;
                }
                if (previousValue != null)
                {
                    highest2 = Math.Max(dailyValue.HIGH, previousValue.HIGH);
                    lowest2 = Math.Min(dailyValue.LOW, previousValue.LOW);
                    range2 = highest2 - lowest2;
                    upVol2 = dailyValue.UPVOLUME + previousValue.UPVOLUME;
                    downVol2 = dailyValue.DOWNVOLUME + previousValue.DOWNVOLUME;
                    Value13[i] = Value3[i] + Value3[i - 1];
                    Value14[i] = upVol2 * range2;
                    Value15[i] = (upVol2 - downVol2) * range2;
                    Value16[i] = downVol2 * range2;
                    Value17[i] = (downVol2 - upVol2) * range2;
                    if (highest2 != lowest2)
                    {
                        Value18[i] = upVol2 / range2;
                        Value19[i] = (upVol2 - downVol2) / range2;
                        Value20[i] = downVol2 / range2;
                        Value21[i] = (downVol2 - upVol2) / range2;
                        Value22[i] = Value13[i] / range2;
                    }
                }
                i++;
                previousValue = dailyValue;
            }
            this.AddSerie("Value3", Value3);
            this.AddSerie("Value4", Value4);
            this.AddSerie("Value5", Value5);
            this.AddSerie("Value6", Value6);
            this.AddSerie("Value7", Value7);
            this.AddSerie("Value8", Value8);
            this.AddSerie("Value9", Value9);
            this.AddSerie("Value10", Value10);
            this.AddSerie("Value11", Value11);
            this.AddSerie("Value12", Value12);
            this.AddSerie("Value13", Value13);
            this.AddSerie("Value14", Value14);
            this.AddSerie("Value15", Value15);
            this.AddSerie("Value16", Value16);
            this.AddSerie("Value17", Value17);
            this.AddSerie("Value18", Value18);
            this.AddSerie("Value19", Value19);
            this.AddSerie("Value20", Value20);
            this.AddSerie("Value21", Value21);
            this.AddSerie("Value22", Value22);
        }
        public bool DetectEvent(StockEvent.EventType eventType, int index)
        {
            return this.GetSerie(eventType)[index];
        }
        public StockEvent.EventType[] DetectEvents(int index, StockEvent.EventType[] eventsToDetect, bool detectOnlyOne)
        {
            List<StockEvent.EventType> eventTypeList = new List<StockEvent.EventType>();
            foreach (StockEvent.EventType eventType in eventsToDetect)
            {
                if (this.DetectEvent(eventType, index))
                {
                    eventTypeList.Add(eventType);
                    if (detectOnlyOne)
                    {
                        break;
                    }
                }
            }
            return eventTypeList.ToArray();
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
                }
                if (stockEvent.Events[eventIndex][index]) return true;
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
        public bool MatchEvents(int index, string eventMask, bool allEvents)
        {
            foreach (StockEvent.EventType eventType in StockEvent.EventTypesFromString(eventMask))
            {
                if (this.DetectEvent(eventType, index))
                {
                    if (!allEvents)
                    {
                        return true;
                    }
                }
                else
                {
                    if (allEvents)
                    {
                        return false;
                    }
                }
            }
            return allEvents;
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

            FloatSerie volume = this.GetSerie(StockDataType.VOLUME);
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
        //public FloatSerie CalculateRateOfChange(int period)
        //{
        //    FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
        //    FloatSerie serie = new FloatSerie(Values.Count());
        //    for (int i = 0; i < period; i++)
        //    {
        //        serie[i] = (float)Math.Log(closeSerie[i] / closeSerie[0]);
        //    }
        //    for (int i = period; i < this.Count; i++)
        //    {
        //        serie[i] = (float)Math.Log(closeSerie[i] / closeSerie[i - period]);
        //    }
        //    serie.Name = "ROC_" + period.ToString();
        //    return serie;
        //}

        public FloatSerie CalculateRateOfRise(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie serie = new FloatSerie(Values.Count());
            float min;

            for (int i = 1; i < Math.Min(period, this.Count); i++)
            {
                min = closeSerie.GetMin(0, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            for (int i = period; i < this.Count; i++)
            {
                min = closeSerie.GetMin(i - period, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            serie.Name = "ROR_" + period.ToString();
            return serie;
        }

        public FloatSerie CalculateRateOfDecline(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie serie = new FloatSerie(Values.Count());
            float max;

            for (int i = 1; i < Math.Min(period, this.Count); i++)
            {
                max = closeSerie.GetMax(0, i);
                serie[i] = (closeSerie[i] - max) / max;
            }
            for (int i = period; i < this.Count; i++)
            {
                max = closeSerie.GetMax(i - period, i);
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

        public FloatSerie CalculateBuySellMomemtum(int period, bool useHMA)
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "BUYMOM");
            }

            FloatSerie momentum = new FloatSerie(this.Count);
            FloatSerie upVol = this.GetSerie(StockDataType.UPVOLUME);
            FloatSerie downVol = this.GetSerie(StockDataType.DOWNVOLUME);
            FloatSerie volume = this.GetSerie(StockDataType.VOLUME);

            if (!useHMA)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    momentum[i] = upVol[i] - downVol[i];
                    if (momentum[i] >= 1)
                    {
                        momentum[i] = (float)Math.Log10(momentum[i]);
                    }
                    else if (momentum[i] <= -1)
                    {
                        momentum[i] = -(float)Math.Log10(-momentum[i]);
                    }
                    else
                    {
                        momentum[i] = 0;
                    }
                }
                momentum.Values = momentum.CalculateHMA(period).CalculateEMA(period / 2).Values;
            }
            else
            {
                FloatSerie cumul = new FloatSerie(this.Count);
                for (int i = 1; i < this.Count; i++)
                {
                    float vol = upVol[i] - downVol[i];
                    float var = this.ValueArray[i].VARIATION;
                    if (var >= 0f)
                    {
                        vol = (var + 1.0f) * volume[i];
                    }
                    else
                    {
                        vol = (var - 1.0f) * volume[i];
                    }

                    //if (vol >= 1)
                    //{
                    //    vol = (float)Math.Log10(vol);
                    //}
                    //else if (vol <= -1)
                    //{
                    //    vol = -(float)Math.Log10(-vol);
                    //}
                    //else
                    //{
                    //    vol = 0;
                    //}
                    cumul[i] = cumul[i - 1] + vol;
                }
                momentum.Values = (cumul.CalculateHMA(period) - cumul.CalculateEMA(period / 2)).CalculateEMA(period / 2).Values;
            }
            momentum.Name = "BUYMOM_" + period;
            return momentum;
        }

        public FloatSerie CalculateBuySellMomemtum2(int period, bool newMethod)
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "BUYMOM");
            }

            FloatSerie momentum = null;
            FloatSerie upVol = this.GetSerie(StockDataType.UPVOLUME);
            FloatSerie downVol = this.GetSerie(StockDataType.DOWNVOLUME);

            bool useLog = true;
            if (!newMethod)
            {
                momentum = new FloatSerie(this.Count);
                for (int i = 0; i < this.Count; i++)
                {
                    momentum[i] = upVol[i] - downVol[i];
                    if (useLog)
                    {
                        if (momentum[i] >= 1)
                        {
                            momentum[i] = (float)Math.Log10(momentum[i]);
                        }
                        else if (momentum[i] <= -1)
                        {
                            momentum[i] = -(float)Math.Log10(-momentum[i]);
                        }
                        else
                        {
                            momentum[i] = 0;
                        }
                    }
                }
                momentum.Values = momentum.CalculateEMA(period).CalculateEMA(period / 2).Values;
            }
            else
            {
                FloatSerie volCumul = new FloatSerie(this.Count);
                volCumul[0] = (float)(Math.Sqrt(upVol[0]) - Math.Sqrt(downVol[0]));
                for (int i = 1; i < this.Count; i++)
                {
                    volCumul[i] = volCumul[i - 1] + (float)(Math.Sqrt(upVol[i]) - Math.Sqrt(downVol[i]));
                }
                momentum = ((volCumul.CalculateEMA(period / 2) - volCumul.CalculateEMA(period)).CalculateRSI(period, false) - 50f).CalculateHMA(period / 2);
            }
            momentum.Name = "BUYMOM_" + period;
            return momentum;
        }
        public FloatSerie CalculateOnBalanceVolume()
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "OBV");
            }

            FloatSerie OBV = new FloatSerie(this.Count, "OBV");
            FloatSerie vol = this.GetSerie(StockDataType.VOLUME);
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
        public FloatSerie CalculateOnBalanceVolumeEx()
        {
            if (!this.HasVolume)
            {
                return new FloatSerie(0, "OBVEX");
            }
            FloatSerie OBVEX = new FloatSerie(this.Count, "OBVEX");
            FloatSerie vol = this.GetSerie(StockDataType.VOLUME);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            float previousClose = closeSerie[0];
            float var = 0.0f;

            for (int i = 1; i < this.Count; i++)
            {
                float close = closeSerie[i];
                var = (close - previousClose) / previousClose;
                OBVEX[i] = OBVEX[i - 1] + vol[i] * var;
                previousClose = close;
            }
            return OBVEX;

            //FloatSerie OBVEX = new FloatSerie(this.Count);
            //FloatSerie upVol = this.GetSerie(StockDataType.UPVOLUME);
            //FloatSerie downVol = this.GetSerie(StockDataType.DOWNVOLUME);
            //FloatSerie openSerie = this.GetSerie(StockDataType.OPEN);
            //for (int i = 1; i < this.Count; i++)
            //{
            //    OBVEX[i] = OBVEX[i - 1] + upVol[i] - downVol[i];
            //}
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
                        new StockDailyValue(seasonalSerie.StockName, previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
                    previousClose = close;
                }
                else
                {
                    close = previousClose * (1.0f + (rebasedSerie[day] - rebasedSerie[previousDay]) / rebasedSerie[previousDay]);
                    seasonalSerie.Add(date,
                        new StockDailyValue(seasonalSerie.StockName, previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
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
                    new StockDailyValue(seasonalSerie.StockName, previousClose, Math.Max(previousClose, close), Math.Min(previousClose, close), close, occurences[day], date));
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
                        new StockDailyValue(overnightSerie.StockName, currentValue, currentValue, currentValue, currentValue, 0, dailyValue.DATE));

                previousClose = dailyValue.CLOSE;
            }
            return overnightSerie;
        }
        //public void CalculateBuySellRateWithNN2(ref NeuralNetwork network, bool forceTeaching)
        //{
        //    List<FloatSerie> floatSerieList = new List<FloatSerie>();
        //    float error = 0.0f;

        //    FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
        //    FloatSerie variationSerie = this.GetSerie(StockIndicatorType.VARIATION_REL).Normalise(-1.0f, 1.0f);
        //    float max = closeSerie.Max;
        //    float min = closeSerie.Min;
        //    closeSerie = closeSerie.Normalise(min, max, 0.0f, 1.0f);
        //    int windowPeriod = 20;

        //    floatSerieList.Add(closeSerie.Normalise(min, max, 0.0f, 1.0f));
        //    floatSerieList.Add(this.GetSerie(StockDataType.EMA3).Normalise(min, max, 0.0f, 1.0f));

        //    //floatSerieList.Add(this.GetSerie(StockIndicatorType.EMA12_EMA20_DIST_TREND).Normalise(0.0f, 100.0f, -1.0f, 1.0f));
        //    //floatSerieList.Add(this.GetSerie(StockIndicatorType.RSI).Normalise(0.0f, 100.0f, -1.0f, 1.0f).ApplySmoothing(StockMathToolkit.SmoothingType.Sigmoid, 1.0f));

        //    int i, j;

        //    // Check if network already exist for this stock
        //    // string fileName = StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork\" + this.StockName + ".nntwk";
        //    string fileName = StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork\ALL_STOCKS.nntwk";
        //    if (File.Exists(fileName))
        //    {
        //        network = NeuralNetwork.load(fileName);
        //        if (network.N_Inputs != floatSerieList.Count)
        //        {
        //            network = null;
        //        }
        //    }
        //    if (network == null)
        //    {
        //        int[] nbNeuronsInLayer = new int[] { floatSerieList.Count, 2 };

        //        network = new NeuralNetwork(floatSerieList.Count, nbNeuronsInLayer, new SigmoidActivationFunction());
        //        network.r&&omizeAll();

        //        forceTeaching = true;
        //    }

        //    if (forceTeaching)
        //    {
        //        // Learning phase
        //        float[][] inputs = new float[this.Values.Count - windowPeriod - 2][];
        //        float[][] outputs = new float[this.Values.Count - windowPeriod - 2][];

        //        StockDailyValue dailyValue = Values.ElementAt(windowPeriod);

        //        // Create input && expected outputs
        //        for (i = 0; i < this.Values.Count - windowPeriod - 2; i++)
        //        {
        //            // Build inputs
        //            inputs[i] = new float[floatSerieList.Count];
        //            j = 0;
        //            foreach (FloatSerie floatSerie in floatSerieList)
        //            {
        //                inputs[i][j++] = floatSerie.Values[i + windowPeriod];
        //            }

        //            // Build output
        //            outputs[i] = new float[] { variationSerie.Values[i + windowPeriod + 1], closeSerie.Values[i + windowPeriod + 1] };
        //        }

        //        // Teach the network
        //        error = network.LearningAlg.Learn(inputs, outputs);
        //    }

        //    // Create or overwrite the series
        //    FloatSerie buySellRatesSerie = new FloatSerie(this.Values.Count);
        //    FloatSerie forecastSerie = new FloatSerie(closeSerie.Count);
        //    i = 0;
        //    float[] input = new float[floatSerieList.Count];

        //    float[] output = null;
        //    foreach (StockDailyValue currentValue in this.Values)
        //    {
        //        if (i != 0)
        //        {
        //            j = 0;
        //            foreach (FloatSerie floatSerie in floatSerieList)
        //            {
        //                input[j++] = floatSerie.Values[i - 1];
        //            }

        //            // Calculate Buy/Sell rate
        //            output = network.Output(input);
        //            buySellRatesSerie[i] = output[0];
        //            forecastSerie[i] = output[1];
        //        }
        //        i++;
        //    }
        //    // No forecast for first value, just use the next data
        //    buySellRatesSerie[0] = buySellRatesSerie[1];
        //    forecastSerie[0] = forecastSerie[1];

        //    if (!File.Exists(fileName))
        //    {
        //        if (!Directory.Exists(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork"))
        //        {
        //            Directory.CreateDirectory(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork");
        //        }
        //        //network.save(fileName);
        //    }

        //    // Store the calculated series.
        //    this.AddSerie(StockIndicatorType.NN_BUY_SELL_RATE, buySellRatesSerie.Normalise(-1.0f, 1.0f));
        //    this.AddSerie(StockDataType.NN_FORECAST, forecastSerie.Normalise(min, max));
        //}
        //private void CalculateBuySellRateWithNN()
        //{
        //    List<FloatSerie> floatSerieList = new List<FloatSerie>();
        //    float error = 0.0f;

        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.EMA3_TREND).Normalise(-1.0f, 1.0f));
        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.EMA6_TREND).Normalise(-1.0f, 1.0f));
        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.EMA12_TREND).Normalise(-1.0f, 1.0f));
        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.DIST_SAREX_FOLLOWER).Normalise(1.0f, -1.0f));
        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.RSI).Normalise(0.0f, 100.0f, -1.0f, 1.0f));
        //    floatSerieList.Add(this.GetSerie(StockIndicatorType.SLOW_OSCILLATOR_14_MA5).Normalise(0.0f, 100.0f, -1.0f, 1.0f));

        //    int i, j;

        //    // Check if network already exist for this stock
        //    string fileName = StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork\" + this.StockName + ".nntwk";
        //    // string fileName = StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork\ALL_STOCKS.nntwk";
        //    NeuralNetwork network = null;

        //    if (File.Exists(fileName))
        //    {
        //        network = NeuralNetwork.load(fileName);
        //        if (network.N_Inputs != floatSerieList.Count)
        //        {
        //            network = null;
        //        }
        //    }
        //    if (network == null)
        //    {
        //        // Use as input EMA3_TREND, EMA6_TREND, EMA12_TREND, EMA20_TREND,
        //        int[] nbNeuronsInLayer = new int[] { floatSerieList.Count, floatSerieList.Count, 1 };

        //        network = new NeuralNetwork(floatSerieList.Count, nbNeuronsInLayer, new SigmoidActivationFunction());
        //        network.r&&omizeAll();

        //        // Learning phase
        //        float[][] inputs = new float[this.Values.Count - 22][];
        //        float[][] outputs = new float[this.Values.Count - 22][];

        //        StockDailyValue dailyValue = Values.ElementAt(20);

        //        // Create input && expected outputs
        //        float var1;
        //        for (i = 0; i < this.Values.Count - 22; i++)
        //        {
        //            // Build inputs
        //            inputs[i] = new float[floatSerieList.Count];
        //            j = 0;
        //            foreach (FloatSerie floatSerie in floatSerieList)
        //            {
        //                inputs[i][j++] = floatSerie.Values[i + 20];
        //            }

        //            // Build output
        //            var1 = Values.ElementAt(i + 21).VARIATION;
        //            outputs[i] = new float[1] { var1 };
        //        }

        //        // Teach the network
        //        error = network.LearningAlg.Learn(inputs, outputs);
        //    }

        //    // Create or overwrite the series
        //    FloatSerie buySellRatesSerie = new FloatSerie(this.Values.Count);
        //    this.IndicatorSeries[(int)StockIndicatorType.NN_BUY_SELL_RATE] = buySellRatesSerie;

        //    i = 0;
        //    float[] input = new float[floatSerieList.Count];

        //    foreach (StockDailyValue currentValue in this.Values)
        //    {
        //        j = 0;
        //        foreach (FloatSerie floatSerie in floatSerieList)
        //        {
        //            input[j++] = floatSerie.Values[i];
        //        }

        //        // Calculate Buy/Sell rate
        //        buySellRatesSerie.Values[i] = network.Output(input)[0];
        //        i++;
        //    }
        //    if (!File.Exists(fileName))
        //    {
        //        if (!Directory.Exists(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork"))
        //        {
        //            Directory.CreateDirectory(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork");
        //        }
        //        network.save(fileName);
        //    }
        //}


        // $$$$ Prototype
        public FloatSerie CalculateSmoothedOscillator(int period, int smootingPeriod)
        {
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            FloatSerie fastOscillatorSerie = new FloatSerie(this.Values.Count);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateHMA(smootingPeriod);
            FloatSerie lowSerie = closeSerie;
            FloatSerie highSerie = closeSerie;
            float lowestLow = float.MaxValue;
            float highestHigh = float.MinValue;

            for (int i = 0; i < this.Values.Count; i++)
            {
                lowestLow = lowSerie.GetMin(Math.Max(0, i - period), i);
                highestHigh = highSerie.GetMax(Math.Max(0, i - period), i);
                if (highestHigh == lowestLow)
                {
                    fastOscillatorSerie[i] = 100.0f;
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
                        longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
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
                        shortStopSerie[i] = shortStopSerie[i - 1] + alpha * (highEMASerie[i] - shortStopSerie[i - 1]);
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
                else if (i > 0)
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

        public void CalculateVolumeTrailStop(bool trailGaps, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            if (this.HasVolume)
            {
                longStopSerie = new FloatSerie(this.Count, "TRAILVOL.S");
                shortStopSerie = new FloatSerie(this.Count, "TRAILVOL.R");
            }
            else
            {
                longStopSerie = new FloatSerie(0, "TRAILVOL.S");
                shortStopSerie = new FloatSerie(0, "TRAILVOL.R");
            }
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            BoolSerie climaxDownVolSerie = this.GetSerie(StockEvent.EventType.VolClimaxDown);
            BoolSerie climaxUpVolSerie = this.GetSerie(StockEvent.EventType.VolClimaxUp);
            BoolSerie climaxChurnVolSerie = this.GetSerie(StockEvent.EventType.VolClimaxChurn);
            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
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
                if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Max(highSerie[i - 1], highSerie[i]);
                        }
                        else
                        {
                            if (climaxUpVolSerie[i] || climaxChurnVolSerie[i])
                            {
                                if (trailGaps && highSerie[i - 1] < lowSerie[i])
                                { // in case of gap trail up to previous close
                                    longStopSerie[i] = closeSerie[i - 1];
                                    shortStopSerie[i] = float.NaN;
                                }
                                else
                                {
                                    // Trail the stop up
                                    longStopSerie[i] = Math.Min(lowSerie[i - 1], lowSerie[i]);
                                    shortStopSerie[i] = float.NaN;
                                }
                            }
                            else
                            { // Nothing has changed
                                longStopSerie[i] = longStopSerie[i - 1];
                                shortStopSerie[i] = float.NaN;
                            }
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = Math.Min(lowSerie[i - 1], lowSerie[i]);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            if (climaxDownVolSerie[i] || climaxChurnVolSerie[i])
                            {
                                if (trailGaps && lowSerie[i - 1] > highSerie[i])
                                { // in case of gap trail down to previous close
                                    longStopSerie[i] = float.NaN;
                                    shortStopSerie[i] = closeSerie[i - 1];
                                }
                                else
                                {
                                    // Trail the stop down 
                                    longStopSerie[i] = float.NaN;
                                    shortStopSerie[i] = Math.Max(highSerie[i - 1], highSerie[i]);
                                }
                            }
                            else
                            { // Nothing has changed
                                longStopSerie[i] = float.NaN;
                                shortStopSerie[i] = shortStopSerie[i - 1];
                            }
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
            float currentSarex = this.Values.First().AVG;
            sarexFollowerResistance.Values[0] = currentSarex;
            float previousExtremum = this.Values.First().AVG;
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
                    previousExtremum = this.ValueArray[i].AVG;
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
                        newLine = new Segment2D(latestLine.Point2.X, latestLine.Point2.Y, j, lowSerie[j]);

                        points.Add(newLine.Point2);

                        latestLine = newLine;
                    }
                    if (resistanceDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestLine.Point2.X && highSerie[j] != resistanceSerie[i]; j--) ;
                        newLine = new Segment2D(latestLine.Point2.X, latestLine.Point2.Y, j, highSerie[j]);

                        points.Add(newLine.Point2);

                        latestLine = newLine;
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

                Segment2D latestResistanceLine = new Segment2D(startIndex - 1, highSerie[startIndex], startIndex,
                    highSerie[startIndex]);
                Segment2D latestSupportLine = new Segment2D(startIndex - 1, lowSerie[startIndex], startIndex,
                    lowSerie[startIndex]);

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
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                            newLine = new Segment2D(latestSupportLine.Point2.X, latestSupportLine.Point2.Y,
                                j, lowSerie[j]));
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
                            for (j = i;
                                j > latestResistanceLine.Point2.X && highSerie[j] != resistanceSerie[i];
                                j--) ;
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                                newLine =
                                    new Segment2D(latestResistanceLine.Point2.X, latestResistanceLine.Point2.Y,
                                        j, highSerie[j]));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(
                                bullet = new Bullet2D(newLine.Point2, 3));

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
                case Groups.ShortInterest:
                    return this.HasShortInterest;
                case Groups.COT:
                    return this.CotSerie != null;
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
        #region Strategy Simulation
        public StockOrder GenerateSimulation(IStockStrategy strategy, System.DateTime startDate, System.DateTime endDate, float amount, bool reinvest,
            bool amendOrders, bool supportShortSelling,
           bool takeProfit, float profitTarget,
           bool stopLoss, float stopLossTarget,
           float fixedFee, float taxRate, StockPortofolio portfolio)
        {
            if (!this.Initialise()) { return null; }
            strategy.Initialise(this, null, supportShortSelling);

            // Working variables
            StockOrder lastBuyOrder = null;
            float remainingCash = 0.0f;
            bool lookingForBuying = true;
            float benchmark = float.NaN;
            // Loop on series values
            StockDailyValue previousValue = this.Values.First();
            StockOrder stockOrder = null;

            if (this.GetSerie(StockDataType.HIGH).Max >= amount)
            {
                if (MessageBox.Show("There are not enough fund to purchase " + this.StockName + ", do you want to increase it accordingly ?", "Not enough fund", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    amount = this.GetSerie(StockDataType.HIGH).Max * 2;
                }
                else
                    return null;
            }
            portfolio.TotalDeposit = amount;

            // Run the order processing loop
            RunOrderProcessingLoop(strategy, startDate, endDate, amount, reinvest, amendOrders, supportShortSelling,
               takeProfit, profitTarget,
               stopLoss, stopLossTarget,
               fixedFee, taxRate, portfolio, ref stockOrder, lastBuyOrder, lookingForBuying, remainingCash, benchmark);

            return stockOrder;
        }

        public StockOrder GenerateOrder(IStockStrategy strategy, System.DateTime startDate, System.DateTime endDate, float amount, bool reinvest,
    bool amendOrders, bool supportShortSelling,
           bool takeProfit, float profitTarget,
           bool stopLoss, float stopLossTarget,
              float fixedFee, float taxRate, StockPortofolio portfolio)
        {
            // Get the exiting order list
            StockOrderList orderList = portfolio.OrderList.GetOrderListSortedByDate(this.StockName);
            StockOrder stockOrder = null;
            StockOrder lastBuyOrder = null;
            bool lookingForBuying = true;
            float remainingCash = 0.0f;
            float benchmark = float.NaN;

            #region Initialise from last order
            if (orderList.Count != 0)
            {
                stockOrder = orderList.Last();
                startDate = stockOrder.CreationDate;
                switch (stockOrder.State)
                {
                    case StockOrder.OrderStatus.Executed:
                        if (stockOrder.IsBuyOrder())
                        {
                            startDate = stockOrder.ExecutionDate;
                            lastBuyOrder = stockOrder;
                            lookingForBuying = false;
                            stockOrder = null;
                            remainingCash = amount - lastBuyOrder.TotalCost;
                        }
                        else
                        {
                            startDate = stockOrder.ExecutionDate;
                            lookingForBuying = true;
                            lastBuyOrder = null;
                            stockOrder = null;
                        }
                        break;
                    case StockOrder.OrderStatus.Pending:
                        if (stockOrder.IsBuyOrder())
                        {
                            benchmark = this[stockOrder.CreationDate].LOW;
                            lastBuyOrder = stockOrder;
                            lookingForBuying = true;
                        }
                        else
                        {
                            benchmark = this[stockOrder.CreationDate].HIGH;
                            lastBuyOrder = orderList.ElementAt(orderList.Count - 2); // Assume that if there is a pending sell order, it was a previous buy order.
                            lookingForBuying = false;
                        }
                        break;
                    case StockOrder.OrderStatus.Expired:
                        if (stockOrder.IsBuyOrder())
                        {
                            lastBuyOrder = null;
                            lookingForBuying = true;
                        }
                        else
                        {
                            lastBuyOrder = orderList.ElementAt(orderList.Count - 2); // Assume that if there is a pending sell order, it was a previous buy order.
                            lookingForBuying = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion

            strategy.Initialise(this, lastBuyOrder, supportShortSelling);

            // Run the order processing loop
            RunOrderProcessingLoop(strategy, startDate, endDate, amount, reinvest, amendOrders, supportShortSelling,
               takeProfit, profitTarget,
               stopLoss, stopLossTarget,
                fixedFee, taxRate, portfolio, ref stockOrder, lastBuyOrder, lookingForBuying, remainingCash, benchmark);

            return stockOrder;
        }

        private bool printOrderLog = false;

        private void RunOrderProcessingLoop(IStockStrategy strategy, System.DateTime startDate, System.DateTime endDate,
           float amount, bool reinvest, bool amendOrders, bool supportShortSelling,
           bool takeProfit, float profitTarget,
           bool stopLoss, float stopLossTarget,
           float fixedFee, float taxRate, StockPortofolio portofolio, ref StockOrder stockOrder, StockOrder lastBuyOrder, bool lookingForBuying, float remainingCash, float benchmark)
        {
            //printOrderLog = Debugger.IsAttached;

            // Loop on series values
            StockDailyValue previousValue = this.Values.First();
            int currentIndex = 0;
            int nbOpenPosition = 0;

            var dailyValues = this.GetExactValues();
            DateTime firstDaily = dailyValues.First().DATE;

            // Need to start a minimun on the start of the intraday daily cache.
            startDate = startDate > firstDaily ? startDate : firstDaily;
            endDate = endDate > dailyValues.Last().DATE ? dailyValues.Last().DATE : startDate;

            StockOrder takeProfitOrder = null;
            StockOrder stopLossOrder = null;

            IStockMoneyManagement moneyManagement = null;
            if (stopLoss) moneyManagement = MoneyManagementManager.CreateMoneyManagement("StockPreviousLowRiskFree", this);

            StockOrder.FixedFee = fixedFee;
            StockOrder.TaxRate = taxRate;
            foreach (StockDailyValue barValue in this.Values)
            {
                if ((barValue.DATE >= startDate) && currentIndex > 0 && (barValue.DATE <= endDate) && amount > barValue.CLOSE)
                {
                    #region Process Pending Orders
                    if (stockOrder != null)
                    {
                        if (StockOrder.OrderStatus.Executed == ProcessPendingOrder(ref amount, reinvest, takeProfit, profitTarget, stopLoss, stopLossTarget, fixedFee, portofolio, stockOrder, ref lookingForBuying, ref remainingCash, ref benchmark, ref nbOpenPosition, dailyValues, ref takeProfitOrder, ref stopLossOrder, barValue))
                        {
                            if (printOrderLog) StockLog.Write("Main executed: " + stockOrder.ToString());
                            if (supportShortSelling && !stockOrder.IsBuyOrder()) // Closing position
                            { // Try to reverse 
                                stockOrder = strategy.TryToBuy(barValue, currentIndex - 1, amount, ref benchmark);
                                strategy.LastBuyOrder = stockOrder;
                                if (stockOrder != null)
                                {
                                    if (StockOrder.OrderStatus.Executed == ProcessPendingOrder(ref amount, reinvest, takeProfit, profitTarget, stopLoss, stopLossTarget, fixedFee, portofolio, stockOrder, ref lookingForBuying, ref remainingCash, ref benchmark, ref nbOpenPosition, dailyValues, ref takeProfitOrder, ref stopLossOrder, barValue))
                                    {
                                        stockOrder = null;
                                    }
                                }
                            }
                            else
                            {
                                stockOrder = null;
                            }
                        }
                    }
                    else
                    {
                        if (takeProfit && takeProfitOrder != null)
                        {
                            #region Process take profit

                            StockDailyValue dailyValue = dailyValues.First(v => v.DATE >= barValue.DATE);
                            takeProfitOrder.ProcessOrder(dailyValue);
                            switch (takeProfitOrder.State)
                            {
                                case StockOrder.OrderStatus.Executed:
                                    if (printOrderLog) StockLog.Write("Target executed: " + takeProfitOrder.ToString());

                                    if (takeProfitOrder.IsBuyOrder())
                                    {
                                        remainingCash = amount - takeProfitOrder.TotalCost;
                                    }
                                    else
                                    {
                                        if (reinvest)
                                        {
                                            // Calculate the new amount to invest
                                            amount = remainingCash + takeProfitOrder.TotalCost;
                                            if (amount < fixedFee)
                                            {
                                                MessageBox.Show("You lost everything", "Game Over");
                                                break; // Exit the loop
                                            }
                                        }
                                    }
                                    if (!portofolio.OrderList.Contains(takeProfitOrder))
                                    {
                                        portofolio.OrderList.Add(takeProfitOrder);
                                    }
                                    nbOpenPosition -= takeProfitOrder.Number;
                                    if (nbOpenPosition == 0)
                                    {
                                        //StockLog.Write("nothing left");
                                        stopLossOrder = null;
                                        lookingForBuying = true;
                                    }
                                    else
                                    {
                                        if (stopLossOrder != null)
                                        {
                                            stopLossOrder.Number -= takeProfitOrder.Number;
                                        }
                                    }
                                    takeProfitOrder = null;
                                    break;
                                case StockOrder.OrderStatus.Expired:
                                    takeProfitOrder = null;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                        if (stopLoss && stopLossOrder != null)
                        {
                            #region Process stop loss

                            StockDailyValue dailyValue = dailyValues.First(v => v.DATE >= barValue.DATE);
                            stopLossOrder.ProcessOrder(dailyValue);
                            switch (stopLossOrder.State)
                            {
                                case StockOrder.OrderStatus.Executed:
                                    if (printOrderLog) StockLog.Write("Stop executed: " + stopLossOrder.ToString());
                                    if (stopLossOrder.IsBuyOrder())
                                    {
                                        remainingCash = amount - stopLossOrder.TotalCost;
                                    }
                                    else
                                    {
                                        if (reinvest)
                                        {
                                            // Calculate the new amount to invest
                                            amount = remainingCash + stopLossOrder.TotalCost;
                                            if (amount < fixedFee)
                                            {
                                                MessageBox.Show("You lost everything", "Game Over");
                                                break; // Exit the loop
                                            }
                                        }
                                    }
                                    if (!portofolio.OrderList.Contains(stopLossOrder))
                                    {
                                        portofolio.OrderList.Add(stopLossOrder);
                                    }
                                    nbOpenPosition -= stopLossOrder.Number;
                                    if (nbOpenPosition == 0)
                                    {
                                        //StockLog.Write("nothing left");
                                        takeProfitOrder = null;
                                        lookingForBuying = true;
                                    }
                                    stopLossOrder = null;
                                    break;
                                case StockOrder.OrderStatus.Expired:
                                    stopLossOrder = null;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }

                    #endregion
                    #region Buy Order
                    if (lookingForBuying)
                    {
                        if (stockOrder == null)
                        {
                            stockOrder = strategy.TryToBuy(barValue, currentIndex, amount, ref benchmark);
                            strategy.LastBuyOrder = stockOrder;
                            if (stockOrder != null && stockOrder.Type == StockOrder.OrderType.BuyAtMarketClose)
                            {
                                stockOrder.ProcessOrder(barValue);
                            }
                        }
                        else
                        {
                            if (amendOrders)
                            {
                                strategy.AmendBuyOrder(ref stockOrder, barValue, currentIndex, amount, ref benchmark);
                                strategy.LastBuyOrder = stockOrder;
                                if (stockOrder != null && stockOrder.Type == StockOrder.OrderType.BuyAtMarketClose)
                                {
                                    stockOrder.ProcessOrder(barValue);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Sell Order
                    else
                    {
                        // Create or update existing order
                        if (stockOrder == null)
                        {
                            // Create new sell trailing order
                            stockOrder = strategy.TryToSell(barValue, currentIndex, nbOpenPosition, ref benchmark);
                            if (stockOrder != null && stockOrder.Type == StockOrder.OrderType.SellAtMarketClose)
                            {
                                stockOrder.ProcessOrder(barValue);
                            }
                        }
                        else
                        {
                            if (amendOrders)
                            {
                                strategy.AmendSellOrder(ref stockOrder, barValue, currentIndex, nbOpenPosition, ref benchmark);
                                if (stockOrder != null && stockOrder.Type == StockOrder.OrderType.SellAtMarketClose)
                                {
                                    stockOrder.ProcessOrder(barValue);
                                }
                            }
                        }
                    }
                    #endregion
                }
                previousValue = barValue;
                currentIndex++;
            }
        }

        private static StockOrder.OrderStatus ProcessPendingOrder(ref float amount, bool reinvest,
           bool takeProfit, float profitTarget,
           bool stopLoss, float stopLossTarget,
           float fixedFee, StockPortofolio portofolio, StockOrder stockOrder,
           ref bool lookingForBuying, ref float remainingCash, ref float benchmark,
           ref int nbOpenPosition, List<StockDailyValue> dailyValues,
           ref StockOrder takeProfitOrder, ref StockOrder stopLossOrder, StockDailyValue barValue)
        {
            // Get bar from daily values
            StockDailyValue dailyValue = dailyValues.First(v => v.DATE >= barValue.DATE);
            stockOrder.ProcessOrder(dailyValue);
            switch (stockOrder.State)
            {
                case StockOrder.OrderStatus.Executed:
                    if (stockOrder.IsBuyOrder())
                    {
                        remainingCash = amount - stockOrder.TotalCost;
                        if (takeProfit)
                        {
                            // Create take profit order
                            takeProfitOrder = StockOrder.CreateSellAtLimitStockOrder(stockOrder.StockName,
                               dailyValue.DATE, DateTime.MaxValue, stockOrder.Number / 2, stockOrder.Value * profitTarget,
                               dailyValue, false);
                        }
                        if (stopLoss)
                        {
                            stopLossOrder = StockOrder.CreateSellAtThresholdStockOrder(stockOrder.StockName,
                               dailyValue.DATE, DateTime.MaxValue, stockOrder.Number, stockOrder.Value * (1 - stopLossTarget),
                               dailyValue, false);
                        }
                        nbOpenPosition = stockOrder.Number;
                    }
                    else
                    {
                        if (reinvest)
                        {
                            // Calculate the new amount to invest
                            amount = remainingCash + stockOrder.TotalCost;
                            if (amount < fixedFee)
                            {
                                MessageBox.Show("You lost everything", "Game Over");
                                break;  // Exit the loop
                            }
                        }
                        takeProfitOrder = null;
                        stopLossOrder = null;
                        nbOpenPosition -= stockOrder.Number;
                        if (nbOpenPosition != 0)
                        {
                            StockLog.Write("Error is position calculation");
                        }
                    }
                    lookingForBuying = !lookingForBuying;
                    if (!portofolio.OrderList.Contains(stockOrder))
                    {
                        portofolio.OrderList.Add(stockOrder);
                    }
                    benchmark = float.NaN;
                    break;
                case StockOrder.OrderStatus.Pending:
                    if (stockOrder.IsBuyOrder())
                    {
                        benchmark = Math.Min(barValue.LOW, benchmark);
                    }
                    else
                    {
                        benchmark = Math.Max(barValue.HIGH, benchmark);
                    }
                    break;
                case StockOrder.OrderStatus.Expired:
                    benchmark = float.NaN;
                    throw (new Exception("We don't deal with Expired orders"));
                default:
                    break;
            }
            return stockOrder.State;
        }

        #endregion
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
        public float GetMin(StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.Min;
        }
        public float GetMin(StockIndicatorType indicatorType)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            return floatSerie.Min;
        }
        public float GetMin(GraphCurveType curveType)
        {
            return curveType.DataSerie.Min;
        }
        public float GetMin(List<GraphCurveType> curveList)
        {
            float minValue = float.MaxValue;
            float tmpMin = float.MaxValue;
            foreach (GraphCurveType currentCurveType in curveList)
            {
                tmpMin = GetMin(currentCurveType);
                minValue = Math.Min(minValue, tmpMin);
            }
            return minValue;
        }
        public float GetMin(int startIndex, int endIndex, StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.GetMin(startIndex, endIndex);
        }
        public float GetMin(int startIndex, int endIndex, StockIndicatorType indicatorType)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            return floatSerie.GetMin(startIndex, endIndex);
        }
        public float GetMin(int startIndex, int endIndex, GraphCurveType curveType)
        {
            return curveType.DataSerie.GetMin(startIndex, endIndex);
        }
        public float GetMin(int startIndex, int endIndex, List<GraphCurveType> curveList)
        {
            float minValue = float.MaxValue;
            float tmpMin = float.MaxValue;
            foreach (GraphCurveType currentCurveType in curveList)
            {
                tmpMin = GetMin(currentCurveType);
                minValue = Math.Min(minValue, tmpMin);
            }
            return minValue;
        }


        public float GetMax(StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.Max;
        }
        public float GetMax(StockIndicatorType indicatorType)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            return floatSerie.Max;
        }
        public float GetMax(GraphCurveType curveType)
        {
            return curveType.DataSerie.Max;
        }
        public float GetMax(List<GraphCurveType> curveList)
        {
            float maxValue = float.MinValue;
            float tmpMax = float.MinValue;
            foreach (GraphCurveType currentCurveType in curveList)
            {
                tmpMax = currentCurveType.DataSerie.Max;
                maxValue = Math.Max(maxValue, tmpMax);
            }
            return maxValue;
        }
        public float GetMax(int startIndex, int endIndex, StockDataType dataType)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            return floatSerie.GetMax(startIndex, endIndex);
        }
        public float GetMax(int startIndex, int endIndex, StockIndicatorType indicatorType)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            return floatSerie.GetMax(startIndex, endIndex);
        }
        public float GetMax(int startIndex, int endIndex, GraphCurveType curveType)
        {
            return curveType.DataSerie.GetMax(startIndex, endIndex);
        }
        public float GetMax(int startIndex, int endIndex, List<GraphCurveType> curveList)
        {
            float maxValue = float.MinValue;
            float tmpMax = float.MinValue;
            foreach (GraphCurveType currentCurveType in curveList)
            {
                tmpMax = GetMax(startIndex, endIndex, currentCurveType);
                maxValue = Math.Max(maxValue, tmpMax);
            }
            return maxValue;
        }

        public void GetMinMax(StockDataType dataType, ref float minValue, ref float maxValue)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            floatSerie.GetMinMax(ref minValue, ref maxValue);
        }
        public void GetMinMax(StockIndicatorType indicatorType, ref float minValue, ref float maxValue)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            floatSerie.GetMinMax(ref minValue, ref maxValue);
        }
        public void GetMinMax(GraphCurveType curveType, ref float minValue, ref float maxValue)
        {
            curveType.DataSerie.GetMinMax(ref minValue, ref maxValue);
        }
        public void GetMinMax(List<GraphCurveType> curveList, ref float minValue, ref float maxValue)
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            float tmpMin = float.MaxValue, tmpMax = float.MinValue;
            foreach (GraphCurveType currentCurveType in curveList)
            {
                currentCurveType.DataSerie.GetMinMax(ref minValue, ref maxValue);
                minValue = Math.Min(minValue, tmpMin);
                maxValue = Math.Max(maxValue, tmpMax);
            }
        }
        public void GetMinMax(int startIndex, int endIndex, StockDataType dataType, ref float minValue, ref float maxValue)
        {
            FloatSerie floatSerie = this.GetSerie(dataType);
            floatSerie.GetMinMax(startIndex, endIndex, ref minValue, ref maxValue);
        }
        public void GetMinMax(int startIndex, int endIndex, StockIndicatorType indicatorType, ref float minValue, ref float maxValue)
        {
            FloatSerie floatSerie = this.GetSerie(indicatorType);
            floatSerie.GetMinMax(startIndex, endIndex, ref minValue, ref maxValue);
        }
        //public void GetMinMax(int startIndex, int endIndex, GraphCurveType curveType, ref float minValue, ref float maxValue)
        //{
        //    FloatSerie floatSerie = null;
        //    if (curveType.IsIndicator)
        //    {
        //        floatSerie = this.GetSerie(curveType.CurveIndicatorType);
        //    }
        //    else
        //    {
        //        floatSerie = this.GetSerie(curveType.CurveDataType);
        //        if (curveType.CurveDataType == StockDataType.HILBERT_SR)
        //        {
        //            floatSerie = floatSerie.Abs();
        //        }
        //    }
        //    if (floatSerie != null)
        //    {
        //        floatSerie.CalculateMinMax(startIndex, endIndex, ref minValue, ref maxValue);
        //    }
        //}
        //public void GetMinMax(int startIndex, int endIndex, List<GraphCurveType> curveList, ref float minValue, ref float maxValue)
        //{
        //    minValue = float.MaxValue;
        //    maxValue = float.MinValue;
        //    float tmpMin = float.MaxValue, tmpMax = float.MinValue;
        //    foreach (GraphCurveType currentCurveType in curveList)
        //    {
        //        GetMinMax(startIndex, endIndex, currentCurveType, ref tmpMin, ref tmpMax);
        //        minValue = Math.Min(minValue, tmpMin);
        //        maxValue = Math.Max(maxValue, tmpMax);
        //    }
        //}

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
        public StockSerie GenerateRandomSerie()
        {
            string stockName = "RANDOM UNIFORM";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Yahoo);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;

            float previousClose = 1000.0f;
            float open, tmp1, tmp2, close, high, low;
            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            float volatity = 15;

            int period1 = 19;
            int period2 = 85;


            DateTime startDate = this.Values.Last().DATE.AddDays(-this.Values.Count * 10);

            for (int i = 0; i < this.Values.Count * 2; i++)
            {
                open = previousClose;
                close = open +
                    (float)Math.Sin(2 * Math.PI * (i % period1) / (double)period1) +
                    1.5f * (float)Math.Sin(2 * Math.PI * (i % period2) / (double)period2);

                high = Math.Max(open, close);
                low = Math.Min(open, close);

                newValue = new StockDailyValue(stockName, open, high, low, close, 1000, startDate);
                stockSerie.Add(startDate, newValue);

                previousClose = close;
                startDate = startDate.AddDays(1);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }

        public StockSerie GenerateRandomSerie2()
        {
            string stockName = "RANDOM UNIFORM";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Yahoo);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;
            Random r = new Random();

            float previousClose = 1000.0f;
            float open, tmp1, tmp2, close, high, low;
            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            float volatity = 15;


            DateTime startDate = this.Values.Last().DATE.AddDays(-this.Values.Count * 10);

            for (int i = 0; i < this.Values.Count * 2; i++)
            {
                open = previousClose + FloatRandom.NextUniform(-volatity, volatity);
                tmp1 = open + FloatRandom.NextUniform(-volatity, volatity);
                tmp2 = open + FloatRandom.NextUniform(-volatity, volatity);
                close = open + FloatRandom.NextUniform(-volatity, volatity);

                high = Math.Max(open, Math.Max(tmp1, Math.Max(tmp2, close)));
                low = Math.Min(open, Math.Min(tmp1, Math.Min(tmp2, close)));

                newValue = new StockDailyValue(stockName, open, high, low, close, 1000, startDate);
                stockSerie.Add(startDate, newValue);

                previousClose = close;
                startDate = startDate.AddDays(1);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public StockSerie GenerateNormalRandomSerie()
        {
            string stockName = "RANDOM NORMAL";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Test);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;
            Random r = new Random();

            FloatSerie variationSerie = this.GetSerie(StockIndicatorType.VARIATION_REL);
            float mean = variationSerie.Mean;
            float stdev = variationSerie.Stdev;

            float previousClose = 1000.0f;
            float open, tmp1, tmp2, close, high, low;
            // Calculate ratio foreach values
            StockDailyValue newValue = null;

            DateTime startDate = this.Values.Last().DATE.AddDays(-this.Values.Count * 10);

            for (int i = 0; i < this.Values.Count * 2; i++)
            {
                open = previousClose * (1 + FloatRandom.NextNormal(mean, stdev));
                tmp1 = open * (1 + FloatRandom.NextNormal(mean, stdev));
                tmp2 = open * (1 + FloatRandom.NextNormal(mean, stdev));
                close = open * (1 + FloatRandom.NextNormal(mean, stdev));

                high = Math.Max(open, Math.Max(tmp1, Math.Max(tmp2, close)));
                low = Math.Min(open, Math.Min(tmp1, Math.Min(tmp2, close)));

                newValue = new StockDailyValue(stockName, open, high, low, close, 1000, startDate);
                stockSerie.Add(startDate, newValue);

                previousClose = close;
                startDate = startDate.AddDays(1);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public StockSerie GenerateGauchyRandomSerie()
        {
            string stockName = "RANDOM GAUCHY";
            StockSerie stockSerie = new StockSerie(stockName, stockName, this.StockGroup, StockDataProvider.Test);
            stockSerie.IsPortofolioSerie = this.IsPortofolioSerie;
            Random r = new Random();

            FloatSerie variationSerie = this.GetSerie(StockIndicatorType.VARIATION_REL);
            float median = 0;
            float gamma = variationSerie.Gamma / 4.0f;

            float previousClose = 1000.0f;
            float open, tmp1, tmp2, close, high, low;
            // Calculate ratio foreach values
            StockDailyValue newValue = null;

            DateTime startDate = this.Values.Last().DATE.AddDays(-this.Values.Count * 10);

            for (int i = 0; i < this.Values.Count * 2; i++)
            {
                open = previousClose * (1 + FloatRandom.NextGauchy(median, gamma));
                tmp1 = open * (1 + FloatRandom.NextGauchy(median, gamma));
                tmp2 = open * (1 + FloatRandom.NextGauchy(median, gamma));
                close = open * (1 + FloatRandom.NextGauchy(median, gamma));

                high = Math.Max(open, Math.Max(tmp1, Math.Max(tmp2, close)));
                low = Math.Min(open, Math.Min(tmp1, Math.Min(tmp2, close)));

                newValue = new StockDailyValue(stockName, open, high, low, close, 1000, startDate);
                stockSerie.Add(startDate, newValue);

                previousClose = close;
                startDate = startDate.AddDays(1);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public StockSerie GenerateCashStockSerie()
        {
            if (!this.Initialise())
            {
                return null;
            }
            const string stockName = "CASH";
            StockSerie stockSerie = new StockSerie(stockName, stockName, Groups.FUND, StockDataProvider.Generated);
            stockSerie.IsPortofolioSerie = false;

            // Calculate ratio foreach values
            DateTime lastDate = this.Keys.Last();
            for (DateTime date = this.Keys.First(); date <= lastDate; date = date.AddDays(1))
            {
                stockSerie.Add(date, new StockDailyValue(stockName, 1.0f, 1.0f, 1.0f, 1.0f, 0, date));
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }
        public bool GenerateRelativeStrenthStockSerie(StockSerie baseSerie, StockSerie referenceSerie)
        {
            if (!baseSerie.Initialise() || !referenceSerie.Initialise())
            {
                return false;
            }

            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            StockDailyValue value2 = null;
            float ratio = float.NaN;
            foreach (StockDailyValue value1 in baseSerie.Values)
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

                    newValue = new StockDailyValue(this.StockName, ratio * value1.OPEN / value2.OPEN, ratio * value1.HIGH / value2.HIGH, ratio * value1.LOW / value2.LOW, ratio * value1.CLOSE / value2.CLOSE, value1.VOLUME + value2.VOLUME, value1.DATE);
                    this.Add(value1.DATE, newValue);
                }
            }

            // Initialise the serie
            return true;
        }

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

                    newValue = new StockDailyValue(stockName, ratio * value1.OPEN / value2.OPEN, ratio * value1.HIGH / value2.HIGH, ratio * value1.LOW / value2.LOW, ratio * value1.CLOSE / value2.CLOSE, value1.VOLUME + value2.VOLUME, value1.DATE);
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
            if (this.GetSerie(StockDataType.LOW).Min < 1.0f)
            {
                scaleFactor = 0.5f / this.GetSerie(StockDataType.LOW).Min;
            }

            // Calculate ratio foreach values
            StockDailyValue newValue = null;
            foreach (StockDailyValue value1 in this.Values)
            {
                newValue = new StockDailyValue(stockName, (float)Math.Log(value1.OPEN * scaleFactor, Math.E), (float)Math.Log(value1.HIGH * scaleFactor, Math.E), (float)Math.Log(value1.LOW * scaleFactor, Math.E), (float)Math.Log(value1.CLOSE * scaleFactor, Math.E), value1.VOLUME, value1.DATE);
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

            float scale = (float)Math.Pow(10, Math.Log10(this.GetSerie(StockDataType.HIGH).Max) + 1);

            StockDailyValue destValue;
            foreach (StockDailyValue invStockValue in this.Values)
            {
                destValue = new StockDailyValue(stockName, scale / invStockValue.OPEN, scale / invStockValue.LOW, scale / invStockValue.HIGH, scale / invStockValue.CLOSE, invStockValue.VOLUME, invStockValue.DATE);
                destValue.Serie = stockSerie;
                stockSerie.Add(destValue.DATE, destValue);
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }

        public List<StockDailyValue> GenerateSerieForTimeSpanFromDaily(StockBarDuration barDuration)
        {
            if (this.BarSerieDictionary.ContainsKey(barDuration))
            {
                StockLog.Write("GenerateSerieForTimeSpanFromDaily Already in cache Name:" + this.StockName + " barDuration:" + barDuration.ToString());
                return this.BarSerieDictionary[barDuration];
            }
            StockLog.Write("GenerateSerieForTimeSpanFromDaily Name:" + this.StockName + " barDuration:" + barDuration.ToString());

            List<StockDailyValue> newStockValues = null;
            List<StockDailyValue> cachedStockValues = null;


            List<StockDailyValue> dailyValueList = this.BarSerieDictionary[StockBarDuration.Daily];

            // Check if has saved cache
            DateTime cacheEndDate = DateTime.Now;
            if (StockDataProviderBase.LoadIntradayDurationArchive(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, this, barDuration))
            {
                cachedStockValues = this.BarSerieDictionary[barDuration];
                this.BarSerieDictionary.Remove(barDuration);

                cacheEndDate = cachedStockValues.Last().DATE;
                DateTime cacheEndDate2 = cacheEndDate.AddDays(-1);

                StockLog.Write("Has file cache from " + cachedStockValues.First().DATE + " to " + cacheEndDate);

                dailyValueList = dailyValueList.Where(v => v.DATE >= cacheEndDate2).ToList();
            }

            // Managed smoothed durations
            string barDurationString = barDuration.ToString();
            int index = barDurationString.IndexOf("_EMA");
            if (index != -1)
            {
                StockBarDuration duration = (StockBarDuration)Enum.Parse(typeof(StockBarDuration), barDurationString.Substring(0, index));
                newStockValues = GenerateSerieForTimeSpanFromDaily(duration);

                int smoothing = int.Parse(barDurationString.Substring(index + 4));

                newStockValues = GenerateSmoothedBars(newStockValues, smoothing);
                this.BarSerieDictionary.Add(barDuration, newStockValues);
                return newStockValues;
            }

            int period;
            string[] timeSpanString = barDurationString.Split('_');
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
                            newStockValues = GenerateMultipleBar(dailyValueList, period);
                        }
                    }
                    break;
                case "RENKO":
                    if (barDuration == StockBarDuration.RENKO_1)
                    {
                        newStockValues = GenerateRenkoBarFromDaily(dailyValueList, 0.01f);
                    }
                    if (barDuration == StockBarDuration.RENKO_2)
                    {
                        newStockValues = GenerateRenkoBarFromDaily(dailyValueList, 0.02f);
                    }
                    if (barDuration == StockBarDuration.RENKO_5)
                    {
                        newStockValues = GenerateRenkoBarFromDaily(dailyValueList, 0.05f);
                    }
                    if (barDuration == StockBarDuration.RENKO_10)
                    {
                        newStockValues = GenerateRenkoBarFromDaily(dailyValueList, 0.1f);
                    }
                    break;
                case "HA":
                    //HA_3D,
                    if (barDuration == StockBarDuration.HA)
                    {
                        newStockValues = GenerateHeikinAshiBarFromDaily(dailyValueList);
                    }
                    else
                    {
                        if (timeSpanString[1].EndsWith("D"))
                        {
                            newStockValues =
                               GenerateSerieForTimeSpanFromDaily(
                                  (StockBarDuration)
                                     Enum.Parse(typeof(StockBarDuration), "Bar_" + timeSpanString[1].Replace("D", "")));
                            newStockValues = GenerateHeikinAshiBarFromDaily(newStockValues);
                        }
                    }
                    break;
                case "MY":
                    //HA_3D,
                    if (barDuration == StockBarDuration.MY)
                    {
                        newStockValues = GenerateMyBarFromDaily(dailyValueList);
                    }
                    else
                    {
                        if (timeSpanString[1].EndsWith("D"))
                        {
                            newStockValues =
                               GenerateSerieForTimeSpanFromDaily(
                                  (StockBarDuration)
                                     Enum.Parse(typeof(StockBarDuration), "Bar_" + timeSpanString[1].Replace("D", "")));
                            newStockValues = GenerateHeikinAshiBarFromDaily(newStockValues);
                        }
                    }
                    break;
                case "MIN":
                    newStockValues = GenerateMinuteBarsFromIntraday(dailyValueList, int.Parse(timeSpanString[1]));
                    break;
                case "TLB":
                    //TLB_3D,
                    //TLB_EMA3,
                    //TLB_3D_EMA3,
                    if (barDuration == StockBarDuration.TLB)
                    {
                        newStockValues = GenerateNbLineBreakBarFromDaily(dailyValueList, 2);
                    }
                    else
                    {
                        if (timeSpanString[1].EndsWith("D"))
                        {
                            newStockValues =
                               GenerateSerieForTimeSpanFromDaily(
                                  (StockBarDuration)
                                     Enum.Parse(typeof(StockBarDuration), "Bar_" + timeSpanString[1].Replace("D", "")));
                            newStockValues = GenerateNbLineBreakBarFromDaily(newStockValues, 2);
                        }
                        else if (timeSpanString[1] == "Weekly")
                        {
                            newStockValues =
                                  GenerateSerieForTimeSpanFromDaily(StockBarDuration.Weekly);
                            newStockValues = GenerateNbLineBreakBarFromDaily(newStockValues, 2);
                        }
                    }
                    break;
                default:
                    newStockValues = GenerateSerieForTimeSpan(dailyValueList, barDuration);
                    break;
            }

            if (newStockValues == null)
            {
                throw new StockAnalyzerException("BarDuration not supported: " + barDuration);
            }
            if (cachedStockValues != null)
            {
                // Merge with cache values
                foreach (StockDailyValue dailyValue in newStockValues.Where(b => b.DATE > cacheEndDate))
                {
                    cachedStockValues.Add(dailyValue);
                }
                newStockValues = cachedStockValues;
            }
            this.BarSerieDictionary.Add(barDuration, newStockValues);
            return newStockValues;
        }

        public List<StockDailyValue> GenerateSerieForTimeSpan(List<StockDailyValue> dailyValueList, StockBarDuration timeSpan)
        {
            StockLog.Write("GenerateSerieForTimeSpan Name:" + this.StockName + " barDuration:" + timeSpan.ToString() +
                              " CurrentBarDuration:" + this.BarDuration);
            List<StockDailyValue> newBarList = null;
            if (dailyValueList.Count == 0) return new List<StockDailyValue>();

            // Load cache if exists
            StockDataProviderBase.LoadIntradayDurationArchive(
               StockAnalyzerSettings.Properties.Settings.Default.RootFolder, this, timeSpan);

            switch (timeSpan)
            {
                case StockBarDuration.Daily:
                    break;
                //case StockBarDuration.HLBreak:
                //   newBarList = GenerateHighLowBreakBarFromDaily(dailyValueList);
                //   break;
                //case StockBarDuration.HLBreak3:
                //   newBarList = GenerateHighLowBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3));
                //   break;
                //case StockBarDuration.HLBreak6:
                //   newBarList =
                //      GenerateHighLowBreakBarFromDaily(
                //         GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 6), 2));
                //   break;

                case StockBarDuration.TLB:
                    newBarList = GenerateNbLineBreakBarFromDaily(dailyValueList, 2);
                    break;
                //case StockBarDuration.TLB_BIS:
                //   newBarList = GenerateNbLineBreakBarFromDaily(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB), 2);
                //   break;
                //case StockBarDuration.TLB_TER:
                //   newBarList = GenerateNbLineBreakBarFromDaily(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_BIS), 2);
                //   break;
                //case StockBarDuration.TLB_TER_6D:
                //   newBarList = GenerateSerieForTimeSpan(GenerateMultipleBar(dailyValueList, 6), StockBarDuration.TwoWeekBreaks_TER);
                //   break;
                //case StockBarDuration.TLB_TER_9D:
                //   newBarList = GenerateSerieForTimeSpan(GenerateMultipleBar(dailyValueList, 9), StockBarDuration.TwoWeekBreaks_TER);
                //   break;
                //case StockBarDuration.TLB_TER_27D:
                //   newBarList = GenerateSerieForTimeSpan(GenerateMultipleBar(dailyValueList, 27), StockBarDuration.TwoWeekBreaks_TER);
                //   break;
                case StockBarDuration.TLB_3D:
                    newBarList = GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2);
                    break;
                case StockBarDuration.TLB_6D:
                    newBarList = GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 6), 2);
                    break;
                case StockBarDuration.TLB_9D:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateMultipleBar(GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2), 3),
                          2);
                    break;
                case StockBarDuration.TLB_27D:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateNbLineBreakBarFromDaily(
                             GenerateMultipleBar(GenerateNbLineBreakBarFromDaily(GenerateMultipleBar(dailyValueList, 3), 2),
                                3), 2), 3);
                    break;
                case StockBarDuration.TLB_EMA3:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB), 3);
                    break;
                case StockBarDuration.TLB_3D_EMA3:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_3D), 3);
                    break;
                case StockBarDuration.TLB_6D_EMA3:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_6D), 3);
                    break;
                case StockBarDuration.TLB_9D_EMA3:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_9D), 3);
                    break;
                case StockBarDuration.TLB_27D_EMA3:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_27D), 3);
                    break;
                case StockBarDuration.TLB_EMA6:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB), 6);
                    break;
                case StockBarDuration.TLB_3D_EMA6:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_3D), 6);
                    break;
                case StockBarDuration.TLB_6D_EMA6:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_6D), 6);
                    break;
                case StockBarDuration.TLB_9D_EMA6:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_9D), 6);
                    break;
                case StockBarDuration.TLB_27D_EMA6:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_27D), 6);
                    break;
                case StockBarDuration.TLB_EMA12:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB), 12);
                    break;
                case StockBarDuration.TLB_3D_EMA12:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_3D), 12);
                    break;
                case StockBarDuration.TLB_6D_EMA12:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_6D), 12);
                    break;
                case StockBarDuration.TLB_9D_EMA12:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_9D), 12);
                    break;
                case StockBarDuration.TLB_27D_EMA12:
                    newBarList = GenerateSmoothedBars(GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.TLB_27D), 12);
                    break;
                case StockBarDuration.ThreeLineBreak:
                    newBarList = GenerateNbLineBreakBarFromDaily(dailyValueList, 3);
                    break;
                case StockBarDuration.ThreeLineBreak_BIS:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.ThreeLineBreak), 3);
                    break;
                case StockBarDuration.ThreeLineBreak_TER:
                    newBarList =
                       GenerateNbLineBreakBarFromDaily(
                          GenerateSerieForTimeSpan(dailyValueList, StockBarDuration.ThreeLineBreak_BIS), 3);
                    break;
                case StockBarDuration.SixLineBreak:
                    newBarList = GenerateNbLineBreakBarFromDaily(dailyValueList, 6);
                    break;
                case StockBarDuration.Weekly:
                    {
                        StockDailyValue newValue = null;
                        DayOfWeek previousDayOfWeek = DayOfWeek.Sunday;
                        DateTime beginDate = dailyValueList.First().DATE;
                        newBarList = new List<StockDailyValue>();

                        foreach (StockDailyValue dailyValue in dailyValueList)
                        {
                            if (newValue == null)
                            {
                                newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
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
                                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                                       dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
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
                case StockBarDuration.Monthly:
                    {
                        StockDailyValue newValue = null;
                        int previousMonth = dailyValueList.First().DATE.Month;
                        DateTime beginDate = dailyValueList.First().DATE;
                        newBarList = new List<StockDailyValue>();

                        foreach (StockDailyValue dailyValue in dailyValueList)
                        {
                            if (newValue == null)
                            {
                                newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
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
                                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
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
                default:
                    throw new System.NotImplementedException("Bar duration: " + timeSpan.ToString() + " is not implemented");
            }
            return newBarList;
        }

        private List<StockDailyValue> GenerateRangeBar(List<StockDailyValue> stockDailyValueList, float variation)
        {
            // Generate tickList
            StockTick[] ticks = new StockTick[stockDailyValueList.Count];
            int i = 0;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                ticks[i++] = new StockTick(dailyValue.DATE, i, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.VARIATION > 0);
            }

            // Generate range serie
            StockBarSerie barSerie = StockBarSerie.CreateRangeBarSerie(this.StockName, this.ShortName, stockDailyValueList.First().CLOSE * variation, ticks);

            List<StockDailyValue> newBarList = new StockSerie(barSerie, this.StockGroup).Values.ToList();
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
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                       dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    newValue.POSITION = dailyValue.POSITION;
                    newValue.IsComplete = false;
                }
                else if (isIntraday && dailyValue.DATE >= newValue.DATE.AddMinutes(nbMinutes))
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                }
                else
                {
                    // Next bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.CLOSE = dailyValue.CLOSE;
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.UPVOLUME += dailyValue.UPVOLUME;
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
            bool isIntraday = this.StockName.StartsWith("INT_");
            int count = 0;
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                if (newValue == null)
                {
                    // New bar
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
                       dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    newValue.POSITION = dailyValue.POSITION;
                    newValue.IsComplete = false;
                    count = 1;
                }
                else if (isIntraday && dailyValue.DATE.Date != newValue.DATE.Date)
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);

                    // New bar
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    count = 1;
                }
                else
                {
                    // Next bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.CLOSE = dailyValue.CLOSE;
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.UPVOLUME += dailyValue.UPVOLUME;
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
            FloatSerie positionSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.POSITION).ToArray()).CalculateEMA(nbDay);

            StockDailyValue previousValue = stockDailyValueList[0];
            for (int i = 0; i < stockDailyValueList.Count; i++)
            {
                StockDailyValue dailyValue = stockDailyValueList[i];

                float open = openSerie[i];
                float high = highSerie[i];
                float low = lowSerie[i];
                float close = closeSerie[i];

                // New bar
                StockDailyValue newValue = new StockDailyValue(this.StockName, open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
                newValue.POSITION = positionSerie[i];
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
            StockDailyValue newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(newValue);

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];

                float open = (newValue.OPEN + newValue.CLOSE) / 2f; // (HA-Open(-1) + HA-Close(-1)) / 2 
                float high = Math.Max(Math.Max(dailyValue.HIGH, newValue.OPEN), newValue.CLOSE); // Maximum of the High(0), HA-Open(0) or HA-Close(0) 
                float low = Math.Min(Math.Min(dailyValue.LOW, newValue.OPEN), newValue.CLOSE); // Minimum of the Low(0), HA-Open(0) or HA-Close(0) 
                float close = (dailyValue.OPEN + dailyValue.HIGH + dailyValue.LOW + dailyValue.CLOSE) / 4f; // (Open(0) + High(0) + Low(0) + Close(0)) / 4

                // New bar
                newValue = new StockDailyValue(this.StockName, open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
                newValue.POSITION = dailyValue.POSITION;
                newValue.IsComplete = dailyValue.IsComplete;
                newBarList.Add(newValue);
            }
            return newBarList;
        }
        public List<StockDailyValue> GenerateMyBarFromDaily(List<StockDailyValue> stockDailyValueList)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            StockDailyValue previousValue = stockDailyValueList[0];
            StockDailyValue newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(newValue);

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];

                float open = dailyValue.OPEN;
                float high = Math.Max(dailyValue.HIGH, previousValue.HIGH);
                float low = Math.Min(dailyValue.LOW, previousValue.LOW);
                float close = dailyValue.CLOSE;

                // New bar
                newValue = new StockDailyValue(this.StockName, open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
                newValue.POSITION = dailyValue.POSITION;
                newValue.IsComplete = dailyValue.IsComplete;
                newBarList.Add(newValue);

                previousValue = dailyValue;
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateHighLowBreakBarFromDaily(List<StockDailyValue> stockDailyValueList)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            StockDailyValue lastNewValue = null;
            StockDailyValue previousNewValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
            newBarList.Add(previousNewValue);

            for (int i = 1; i < stockDailyValueList.Count; i++)
            {
                dailyValue = stockDailyValueList[i];
                if (lastNewValue == null) // A new bar was completed in previous iteration
                {
                    // New bar
                    lastNewValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
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
                    lastNewValue.UPVOLUME += dailyValue.UPVOLUME;
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

        /// <summary>
        /// Calculate Renko using the variation method. Remains a bug in volume allocation for multiple renko bar generated for single daily (gaps...)
        /// </summary>
        /// <param name="stockDailyValueList"></param>
        /// <param name="variation"></param>
        /// <returns></returns>
        private List<StockDailyValue> GenerateRenkoBarFromDaily(List<StockDailyValue> stockDailyValueList, float variation)
        {
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue dailyValue = stockDailyValueList[0];
            float upVar = 1f + variation;
            float downVar = 1 - variation;

            StockDailyValue newValue = dailyValue.OPEN <= dailyValue.CLOSE
               ? new StockDailyValue(this.StockName, dailyValue.CLOSE * downVar, dailyValue.CLOSE, dailyValue.CLOSE * downVar,
                  dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE)
               : new StockDailyValue(this.StockName, dailyValue.CLOSE * upVar, dailyValue.CLOSE * upVar, dailyValue.CLOSE,
                  dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
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
                        newValue = new StockDailyValue(this.StockName, previousHigh, newHigh, previousHigh, newHigh,
                           dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE + uniqueTimeSpan);
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
                        newValue = new StockDailyValue(this.StockName, previousLow, previousLow, newLow, newLow,
                           dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE + uniqueTimeSpan);
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
                    newValue.UPVOLUME += dailyValue.UPVOLUME;
                }
            }
            return newBarList;
        }

        #region OLD CODE
        //private List<StockDailyValue> GenerateHighLowBreakBarFromDaily3(List<StockDailyValue> stockDailyValueList)
        //{
        //    List<StockDailyValue> newBarList = new List<StockDailyValue>();
        //    StockDailyValue dailyValue = stockDailyValueList[0];
        //    StockDailyValue newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //    StockDailyValue previousValue = newValue;
        //    newBarList.Add(previousValue);

        //    for (int i = 1; i < stockDailyValueList.Count; i++)
        //    {
        //        dailyValue = stockDailyValueList[i];
        //        if (dailyValue.DATE.Date >= new DateTime(2012, 12, 19))
        //        {
        //            dailyValue = stockDailyValueList[i];
        //        }
        //        if (dailyValue.CLOSE > newValue.HIGH || dailyValue.CLOSE < newValue.LOW)
        //        {
        //            // New bar
        //            previousValue = newValue;
        //            newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //            newBarList.Add(newValue);
        //        }
        //        else
        //        {
        //            newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
        //            newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
        //            newValue.VOLUME += dailyValue.VOLUME;
        //            newValue.CLOSE = dailyValue.CLOSE;
        //        }
        //    }
        //    return newBarList;
        //}
        //private List<StockDailyValue> GenerateHighLowBreakBarFromDaily2(List<StockDailyValue> stockDailyValueList)
        //{
        //    List<StockDailyValue> newBarList = new List<StockDailyValue>();
        //    StockDailyValue dailyValue = stockDailyValueList[0];
        //    StockDailyValue newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //    for (int i = 1; i < stockDailyValueList.Count; i++)
        //    {
        //        dailyValue = stockDailyValueList[i];
        //        if (dailyValue.CLOSE > newValue.HIGH || dailyValue.CLOSE < newValue.LOW)
        //        {
        //            // New bar
        //            newBarList.Add(newValue);
        //            newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //        }
        //        else
        //        {
        //            newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
        //            newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
        //            newValue.VOLUME += dailyValue.VOLUME;
        //            newValue.CLOSE = dailyValue.CLOSE;
        //        }
        //    }
        //    newBarList.Add(newValue);
        //    return newBarList;
        //}
        //private List<StockDailyValue> GenerateNbLineBreakBarFromDaily2(List<StockDailyValue> stockDailyValueList, int nbDay)
        //{
        //    Queue<StockDailyValue> previousValues = new Queue<StockDailyValue>(nbDay);
        //    List<StockDailyValue> newBarList = new List<StockDailyValue>();
        //    StockDailyValue dailyValue = stockDailyValueList[0];
        //    StockDailyValue newValue = null;
        //    if (dailyValue.CLOSE > dailyValue.OPEN)
        //    {
        //        newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.CLOSE, dailyValue.OPEN, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //    }
        //    else
        //    {
        //        newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.OPEN, dailyValue.CLOSE, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //    }
        //    previousValues.Enqueue(newValue);

        //    for (int i = 1; i < stockDailyValueList.Count; i++)
        //    {
        //        dailyValue = stockDailyValueList[i];

        //        // Check if price is exceeding Higher
        //        bool isExtending = false;
        //        bool isUp = false;
        //        float extremum = previousValues.Max(v => v.HIGH);
        //        if (dailyValue.CLOSE > extremum)
        //        {
        //            isExtending = true;
        //            isUp = true;
        //        }
        //        else
        //        {
        //            extremum = previousValues.Min(v => v.LOW);
        //            if (dailyValue.CLOSE < extremum)
        //            {
        //                isExtending = true;
        //                isUp = false;
        //            }
        //        }

        //        if (isExtending)
        //        {
        //            // Manage previous values
        //            previousValues.Enqueue(newValue);
        //            if (previousValues.Count > nbDay)
        //            {
        //                previousValues.Dequeue();
        //            }

        //            // New bar
        //            newBarList.Add(newValue);
        //            if (isUp)
        //            {
        //                newValue = new StockDailyValue(this.StockName, extremum, dailyValue.CLOSE, extremum, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //            }
        //            else
        //            {
        //                newValue = new StockDailyValue(this.StockName, extremum, extremum, dailyValue.CLOSE, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
        //            }
        //        }
        //        else
        //        {
        //            newValue.VOLUME += dailyValue.VOLUME;
        //            newValue.CLOSE = dailyValue.CLOSE;
        //            //newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
        //            //newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
        //        }
        //    }
        //    newBarList.Add(newValue);
        //    return newBarList;
        //}

        #endregion

        public List<StockDailyValue> GenerateDailyFromIntraday()
        {
            if (!this.StockName.StartsWith("INT_")) throw new InvalidOperationException("Cannot generate daily value from non intraday data");

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
                    newBarList.Add(newValue = new StockDailyValue(this.StockName, open, high, low, close, volume, currentDay));

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
            newBarList.Add(newValue = new StockDailyValue(this.StockName, open, high, low, close, volume, newDay));
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
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    newValue.IsComplete = false;
                    newValue.POSITION = dailyValue.POSITION;
                    newBarList.Add(newValue);
                }
                else
                {
                    // Need to extend current bar
                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                    newValue.VOLUME += dailyValue.VOLUME;
                    newValue.UPVOLUME += dailyValue.UPVOLUME;
                    newValue.CLOSE = dailyValue.CLOSE;
                    newValue.POSITION = (newValue.POSITION + dailyValue.POSITION) / 2f;
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
        /// <summary>
        /// Create TLB but creates two bar the same day, which causes strategies, to buy before the second bar is actually created.
        /// </summary>
        /// <param name="stockDailyValueList"></param>
        /// <param name="nbBar"></param>
        /// <returns></returns>
        private List<StockDailyValue> GenerateNbLineBreakBarFromDaily_Test(List<StockDailyValue> stockDailyValueList, int nbBar)
        {
            bool isIntraday = this.StockName.StartsWith("INT_");
            Queue<StockDailyValue> previousValues = new Queue<StockDailyValue>(nbBar);
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            StockDailyValue firstValue = stockDailyValueList[0];
            newValue = new StockDailyValue(this.StockName, firstValue.OPEN, firstValue.CLOSE, firstValue.OPEN, firstValue.CLOSE, 0, firstValue.DATE);

            newValue.POSITION = firstValue.POSITION;

            previousValues.Enqueue(firstValue);

            int i = 0;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                float highest = previousValues.Max(v => v.HIGH);
                if ((dailyValue.CLOSE > highest && dailyValue.IsComplete))
                {
                    if (newValue != null)
                    {
                        newValue.IsComplete = true;
                        newBarList.Add(newValue);
                    }
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    newValue.IsComplete = true;
                    newValue.POSITION = dailyValue.POSITION;
                    newBarList.Add(newValue);
                    newValue = null;
                }
                else
                {
                    float lowest = previousValues.Min(v => v.LOW);
                    if (dailyValue.CLOSE < lowest && dailyValue.IsComplete)
                    {
                        if (newValue != null)
                        {
                            newValue.IsComplete = true;
                            newBarList.Add(newValue);
                        }
                        newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                        newValue.IsComplete = true;
                        newValue.POSITION = dailyValue.POSITION;
                        newBarList.Add(newValue);
                        newValue = null;
                    }
                    else
                    {
                        if (newValue == null)
                        {
                            newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                            newValue.IsComplete = true;
                        }
                        else
                        {
                            // Extend current bar
                            newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                            newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                            newValue.VOLUME += dailyValue.VOLUME;
                            newValue.UPVOLUME += dailyValue.UPVOLUME;
                            newValue.CLOSE = dailyValue.CLOSE;
                        }
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
            if (newBarList.Last() != newValue && newValue != null)
            {
                //float highest = previousValues.Max(v => v.HIGH);
                //if (newValue.CLOSE > highest)
                //{
                //    newValue.IsComplete = true;
                //}
                //else
                //{
                //    float lowest = previousValues.Min(v => v.LOW);
                //    if (newValue.CLOSE < lowest)
                //    {
                //        newValue.IsComplete = true;
                //    }
                //}
                newBarList.Add(newValue);
            }
            return newBarList;
        }
        private List<StockDailyValue> GenerateNbLineBreakBarFromDaily_anticipate(List<StockDailyValue> stockDailyValueList, int nbBar)
        {
            bool isIntraday = this.StockName.StartsWith("INT_");
            Queue<StockDailyValue> previousValues = new Queue<StockDailyValue>(nbBar);
            List<StockDailyValue> newBarList = new List<StockDailyValue>();
            if (isIntraday)
            {

            }
            StockDailyValue newValue = null;
            StockDailyValue firstValue = stockDailyValueList[0];
            newValue = new StockDailyValue(this.StockName, firstValue.OPEN, firstValue.CLOSE, firstValue.OPEN, firstValue.CLOSE, 0, firstValue.DATE);

            newValue.POSITION = firstValue.POSITION;

            previousValues.Enqueue(firstValue);

            int i = 0;
            foreach (StockDailyValue dailyValue in stockDailyValueList)
            {
                float highest = previousValues.Max(v => v.HIGH);
                if ((dailyValue.CLOSE > highest && dailyValue.IsComplete) || (isIntraday && dailyValue.DATE.Date != newValue.DATE.Date))
                {
                    newValue.IsComplete = true;
                    newBarList.Add(newValue);
                    newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                    newValue.IsComplete = false;
                    newValue.POSITION = dailyValue.POSITION;
                }
                else
                {
                    float lowest = previousValues.Min(v => v.LOW);
                    if (dailyValue.CLOSE < lowest && dailyValue.IsComplete || (isIntraday && dailyValue.DATE.Date != newValue.DATE.Date))
                    {
                        newValue.IsComplete = true;
                        newBarList.Add(newValue);
                        newValue = new StockDailyValue(this.StockName, dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.UPVOLUME, 0, 0, dailyValue.DATE);
                        newValue.IsComplete = false;
                        newValue.POSITION = dailyValue.POSITION;
                    }
                    else
                    {
                        // Extend current bar
                        newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
                        newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
                        newValue.VOLUME += dailyValue.VOLUME;
                        newValue.UPVOLUME += dailyValue.UPVOLUME;
                        newValue.CLOSE = dailyValue.CLOSE;
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
            if (newBarList.Last() != newValue)
            {
                //float highest = previousValues.Max(v => v.HIGH);
                //if (newValue.CLOSE > highest)
                //{
                //    newValue.IsComplete = true;
                //}
                //else
                //{
                //    float lowest = previousValues.Min(v => v.LOW);
                //    if (newValue.CLOSE < lowest)
                //    {
                //        newValue.IsComplete = true;
                //    }
                //}
                newBarList.Add(newValue);
            }
            return newBarList;
        }

        #endregion
        internal SortedDictionary<StockEvent.EventType, StockStatistics> GenerateStatistics(SortedDictionary<StockEvent.EventType, StockStatistics> statDico)
        {
            int i = 0;
            StockEvent.EventType[] stockEventTypes = null;
            if (statDico == null)
            {
                statDico = new SortedDictionary<StockEvent.EventType, StockStatistics>();
                foreach (StockEvent.EventType eventType in Enum.GetValues(typeof(StockEvent.EventType)))
                {
                    statDico.Add(eventType, new StockStatistics());
                }
            }

            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (i >= 20 && i < this.Values.Count - 20)
                {
                    stockEventTypes = this.DetectEvents(i, StockEvent.AllEvents(), false);
                    foreach (StockEvent.EventType stockEventType in stockEventTypes)
                    {
                        statDico[stockEventType].AddStock(i, this);
                    }
                }
                i++;
            }

            return statDico;
        }
        internal SortedDictionary<string, StockStatistics> GenerateAdvancedStatistics(SortedDictionary<string, StockStatistics> statDico)
        {
            int i = 0;
            StockEvent.EventType[] stockEventTypes = null;
            if (statDico == null)
            {
                statDico = new SortedDictionary<string, StockStatistics>();
            }
            string eventMask = string.Empty;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (i >= 20 && i < this.Values.Count - 20)
                {
                    stockEventTypes = this.DetectEvents(i, StockEvent.AllEvents(), false);

                    eventMask = StockEvent.EventTypesToString(stockEventTypes);
                    if (!statDico.ContainsKey(eventMask))
                    {
                        statDico.Add(eventMask, new StockStatistics());
                    }
                    statDico[eventMask].AddStock(i, this);
                }
                i++;
            }

            return statDico;
        }
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
        public FloatSerie GetCotSerie(CotValue.CotValueType cotType)
        {
            if (this.CotSerie == null)
            {
                return null;
            }

            // Look for the first date of the serie in the COT serie.
            DateTime firstSerieDate = this.Keys.First();
            int cotIndex = 0;
            FloatSerie cotSerie = this.CotSerie.GetSerie(cotType);
            float previousCotValue = 0;
            while (this.CotSerie.Keys.ElementAt(cotIndex) <= firstSerieDate)
            {
                previousCotValue = cotSerie[cotIndex++];
            }
            // Fill the new cotSerie2 with value from CotSerie
            FloatSerie cotSerie2 = new FloatSerie(this.Count);
            int serieIndex = 0;
            foreach (DateTime serieDate in this.Keys)
            {
                if (cotIndex < (this.CotSerie.Count - 1) && this.CotSerie.Keys.ElementAt(cotIndex) <= serieDate)
                {
                    previousCotValue = cotSerie[++cotIndex];
                }
                cotSerie2[serieIndex++] = previousCotValue;
            }
            return cotSerie2;
        }
        #region CSV file IO
        public bool LoadData(StockBar.StockBarType barType, string rootFolder)
        {
            bool result = false;
            switch (barType)
            {
                case StockBar.StockBarType.Daily:
                    return StockDataProviderBase.LoadSerieData(rootFolder, this);
                case StockBar.StockBarType.Intraday:
                    break;
                case StockBar.StockBarType.Range:
                    break;
                case StockBar.StockBarType.Tick:
                    break;
                default:
                    break;
            }
            return result;
        }
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

        public bool ReadFromCSVFile(string fileName, StockSerie.StockBarDuration duration)
        {
            bool result = false;
            if (File.Exists(fileName))
            {
                List<StockDailyValue> bars;
                DateTime lastDateTime = DateTime.MinValue;
                if (this.BarSerieDictionary.ContainsKey(duration))
                {
                    bars = this.BarSerieDictionary[duration];
                    lastDateTime = bars.Last().DATE;
                }
                else
                {
                    bars = new List<StockDailyValue>();
                    this.BarSerieDictionary.Add(duration, bars);
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