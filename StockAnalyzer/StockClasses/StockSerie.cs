//using StockAnalyzer.StockClasses.StockViewableItems;
//using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
//using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
//using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
//using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
//using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
//using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
//using StockAnalyzer.StockData;
//using StockAnalyzer.StockLogging;
//using StockAnalyzer.StockMath;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Xml.Serialization;

//namespace StockAnalyzer.StockClasses
//{
//    #region Type Definition
//    #endregion
//    public class StockSerie : StockSortedDictionary, IXmlSerializable
//    {
//        #region public properties
//        public string StockName { get; private set; }
//        public string Symbol { get; private set; }
//        public string ABCName { get; set; }

//        public char MarketPlace => AbcId?.Length == 13 ? AbcId[0] : 'p';

//        public string IsinPrefix => ISIN?.Substring(0, 2);
//        public string AbcId { get; set; }
//        public string ISIN { get; set; }

//        /// <summary>
//        /// Investing.com ticker used for download
//        /// </summary>
//        public long Ticker { get; set; }
//        public long SaxoId { get; set; }
//        /// <summary>
//        /// SAXO OpenAPI instrument ID
//        /// </summary>
//        public long Uic { get; set; }

//        public bool SRD { get; set; }
//        public bool SRD_LO { get; set; }
//        public int SectorId { get; set; }

//        public string Underlying { get; set; }
//        public string Url { get; set; }
//        public Groups StockGroup { get; private set; }
//        public StockAnalysis StockAnalysis { get; set; }


//        public bool IsPortfolioSerie { get; set; }
//        public int LastIndex => this.ValueArray.Length - 1;
//        public int LastCompleteIndex => this.LastIndex > 0 ? (this.ValueArray.Last().IsComplete ? this.Count - 1 : this.Count - 2) : -1;

//        public StockDailyValue LastValue { get; private set; }
//        public StockDailyValue FirstValue => Values?.FirstOrDefault();

//        public bool HasVolume { get; private set; }

//        /// <summary>
//        /// Indicates if a stock has good liquitiy on the last (period) bars by average a exchange in million of Euro.
//        /// </summary>
//        /// <param name="trigger">0.1 indicates 100K€</param>
//        /// <returns></returns>
//        public bool HasLiquidity(float trigger, int period)
//        {
//            float value = GetExchanged(period);
//            return value > trigger;
//        }
//        public float GetExchanged(int period)
//        {
//            if (this.LastCompleteIndex < period)
//                return 0f;

//            float value = 0;
//            for (int i = this.LastCompleteIndex - period; i <= this.LastCompleteIndex; i++)
//            {
//                value += this.ValueArray[i].EXCHANGED;
//            }
//            value /= period * 1000000f;
//            return value;
//        }
//        public float Exchanged => GetExchanged(5);

//        #endregion
//        #region DATA, EVENTS AND INDICATORS SERIES MANAGEMENT
//        public new void Add(DateTime date, StockDailyValue dailyValue)
//        {
//        }

//        [XmlIgnore]
//        public StockDataSource DataSource { get; set; }

//        [XmlIgnore]
//        public SortedDictionary<string, List<StockDailyValue>> BarSmoothedDictionary { get; private set; }

//        BarDuration barDuration = BarDuration.Daily;
//        public BarDuration BarDuration
//        {
//            get { return barDuration; }
//            set
//            {
//                using (new StockSerieLocker(this))
//                {
//                    this.SetBarDuration(value);
//                }
//            }
//        }

//        [XmlIgnore]
//        public FloatSerie[] ValueSeries { get; set; }
//        [XmlIgnore]
//        public Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
//        [XmlIgnore]
//        protected Dictionary<string, IStockCloud> CloudCache { get; set; }
//        [XmlIgnore]
//        public IStockTrailStop TrailStopCache { get; set; }
//        [XmlIgnore]
//        public IStockAutoDrawing AutoDrawingCache { get; set; }
//        [XmlIgnore]

//        protected Dictionary<string, IStockDecorator> DecoratorCache { get; set; }
//        [XmlIgnore]
//        protected IStockTrail TrailCache { get; set; }

//        protected bool isInitialised = false;
//        [XmlIgnore]
//        public bool IsInitialised
//        {
//            get { return this.isInitialised; }
//            set
//            {
//                if (isInitialised != value)
//                {
//                    this.isInitialised = value;
//                }
//                if (!value)
//                {
//                    this.Clear();
//                    ResetAllCache();
//                }
//            }
//        }
//        #region MANAGE TIMESPAN

//        private StockDailyValue[] StockDailyValuesAsArray()
//        {
//            StockDailyValue[] values = this.Values.ToArray();
//            return values;
//        }

//        private StockDailyValue[] valueArray = null;
//        public StockDailyValue[] ValueArray
//        {
//            get
//            {
//                valueArray ??= this.StockDailyValuesAsArray();
//                return valueArray;
//            }
//        }
//        public List<StockDailyValue> GetValues(BarDuration stockBarDuration)
//        {
//            if (this.BarSmoothedDictionary.ContainsKey(stockBarDuration.ToString()))
//            {
//                return this.BarSmoothedDictionary[stockBarDuration.ToString()];
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public List<StockDailyValue> GetSmoothedValues(BarDuration newBarDuration)
//        {
//            string barSmoothedDuration = newBarDuration.ToString();
//            if (this.BarSmoothedDictionary.ContainsKey(barSmoothedDuration))
//            {
//                return this.BarSmoothedDictionary[barSmoothedDuration];
//            }
//            else
//            {
//                List<StockDailyValue> newList = this.GenerateSerieForTimeSpan(this.BarSmoothedDictionary["Daily"], newBarDuration);
//                this.BarSmoothedDictionary.Add(barSmoothedDuration, newList);

//                return newList;
//            }
//        }

//        private void SetBarDuration(BarDuration newBarDuration)
//        {
//        }
//        public void ClearBarDurationCache()
//        {
//            this.BarSmoothedDictionary.Clear();
//        }
//        #endregion

//        public FloatSerie GetSerie(StockDataType dataType)
//        {
//            if (ValueSeries[(int)dataType] == null)
//            {
//                switch (dataType)
//                {
//                    case StockDataType.CLOSE:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.CLOSE).ToArray(), "CLOSE");
//                        break;
//                    case StockDataType.OPEN:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.OPEN).ToArray(), "OPEN");
//                        break;
//                    case StockDataType.HIGH:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.HIGH).ToArray(), "HIGH");
//                        break;
//                    case StockDataType.LOW:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.LOW).ToArray(), "LOW");
//                        break;
//                    case StockDataType.BODYHIGH:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyHigh).ToArray(), "BODYHIGH");
//                        break;
//                    case StockDataType.BODYLOW:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyLow).ToArray(), "BODYLOW");
//                        break;
//                    case StockDataType.VARIATION:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VARIATION).ToArray(), "VARIATION");
//                        break;
//                    case StockDataType.ADR:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.ADR).ToArray(), "ADR");
//                        break;
//                    case StockDataType.VOLUME:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f).ToArray(), "VOLUME");
//                        break;
//                    case StockDataType.EXCHANGED:
//                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.EXCHANGED).ToArray(), "EXCHANGED");
//                        break;
//                }
//            }

//            return ValueSeries[(int)dataType];
//        }

//        #endregion

//        #region Constructors
//        public StockSerie()
//        {
//            this.DataSource = new StockDataSource
//            {
//                Duration = StockClasses.BarDuration.Daily
//            };
//        }
//        public void ResetAllCache()
//        {
//            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
//            this.IndicatorCache = new Dictionary<string, IStockIndicator>();
//            this.DecoratorCache = new Dictionary<string, IStockDecorator>();
//            this.CloudCache = new Dictionary<string, IStockCloud>();
//            this.AutoDrawingCache = null;
//            this.TrailStopCache = null;
//            this.TrailCache = null;
//            this.dateArray = null;
//            this.valueArray = null;
//            this.BarSmoothedDictionary = new SortedDictionary<string, List<StockDailyValue>>();
//            this.barDuration = BarDuration.Daily;
//        }
//        public void ResetIndicatorCache()
//        {
//            this.IndicatorCache.Clear();
//            this.DecoratorCache.Clear();
//            this.CloudCache.Clear();
//            this.AutoDrawingCache = null;
//            this.TrailStopCache = null;
//            this.TrailCache = null;
//        }

//        #endregion
//        #region Initialisation methods (indicator, data && events calculation)

//        private Thread initialisingThread = null;
//        public bool Initialise()
//        {
//            try
//            {
//                using (new StockSerieLocker(this))
//                {
//                    if (!this.IsInitialised)
//                    {
//                        StockLog.Write($"Initialising {StockName}");
//                        // Multithread management
//                        while (initialisingThread != null && initialisingThread != Thread.CurrentThread)
//                            Thread.Sleep(50);
//                        this.initialisingThread = Thread.CurrentThread;

//                        if (this.Count == 0)
//                        {
//                            if (!StockDataProviderBase.LoadSerieData(this) || this.Count == 0)
//                            {
//                                return false;
//                            }
//                            if (this.BarSmoothedDictionary.ContainsKey("Daily"))
//                            {
//                                this.BarSmoothedDictionary.Remove("Daily");
//                            }
//                            this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
//                        }
//                        else
//                        {
//                            if (this.barDuration == BarDuration.Daily && !this.BarSmoothedDictionary.ContainsKey("Daily"))
//                            {
//                                this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
//                            }
//                        }
//                        // Force indicator,data,event and other to null;
//                        PreInitialise();

//                        // Flag initialisation as completed
//                        this.isInitialised = this.Count > 0;
//                    }
//                    return isInitialised;
//                }
//            }
//            finally
//            {
//                this.initialisingThread = null;
//            }
//        }

//        public void PreInitialise()
//        {
//            if (this.Values == null || this.Values.Count() == 0)
//                return;

//            if (!this.BarSmoothedDictionary.ContainsKey("Daily"))
//            {
//                this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());
//            }

//            StockDailyValue previousValue = this.Values.FirstOrDefault();
//            previousValue.VARIATION = (previousValue.CLOSE - previousValue.OPEN) / previousValue.OPEN;

//            foreach (StockDailyValue dailyValue in this.Values.Skip(1))
//            {
//                dailyValue.VARIATION = (dailyValue.CLOSE - previousValue.CLOSE) / previousValue.CLOSE;
//                previousValue = dailyValue;
//            }
//            this.LastValue = previousValue;

//            // Check if has volume on the last 10 bars, othewise, disable it
//            this.HasVolume = this.Values.Any(d => d.VOLUME > 0);

//            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
//            this.valueArray = null;
//        }

//        #endregion
//        #region Hilbert Sine Wave Methods
//        public void CalculateCrossSR(FloatSerie fastSerie, int smoothing, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
//        {
//            supportSerie = new FloatSerie(this.Count, fastSerie.Name + ".S", float.NaN);
//            resistanceSerie = new FloatSerie(this.Count, fastSerie.Name + ".R", float.NaN);

//            FloatSerie slowSerie = fastSerie.CalculateEMA(smoothing);

//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
//            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

//            supportSerie[0] = float.NaN;
//            resistanceSerie[0] = lowSerie[0];
//            float latestHigh = highSerie[0];
//            float latestLow = lowSerie[0];

//            bool isUp = true;
//            for (int i = 1; i < this.Count; i++)
//            {
//                if (isUp)
//                {
//                    if (fastSerie[i] < slowSerie[i])
//                    {
//                        // Resistance detected
//                        resistanceSerie[i] = latestHigh;
//                        supportSerie[i] = float.NaN;
//                        isUp = false;
//                        latestLow = lowSerie[i];
//                    }
//                    else
//                    {
//                        resistanceSerie[i] = resistanceSerie[i - 1];
//                        supportSerie[i] = supportSerie[i - 1];
//                    }
//                }
//                else
//                {
//                    if (fastSerie[i] > slowSerie[i])
//                    {
//                        // Support detected
//                        supportSerie[i] = latestLow;
//                        resistanceSerie[i] = float.NaN;
//                        isUp = true;
//                        latestHigh = highSerie[i];
//                    }
//                    else
//                    {
//                        resistanceSerie[i] = resistanceSerie[i - 1];
//                        supportSerie[i] = supportSerie[i - 1];
//                    }
//                }

//                latestHigh = Math.Max(highSerie[i], Math.Max(highSerie[i - 1], latestHigh));
//                latestLow = Math.Min(lowSerie[i], Math.Min(lowSerie[i - 1], latestLow));
//            }
//        }
//        public void CalculateOverboughtSR(FloatSerie serie, float overbought, float oversold, out FloatSerie supportSerie, out FloatSerie resistanceSerie)
//        {
//            supportSerie = new FloatSerie(this.Count, serie.Name + ".S", float.NaN);
//            resistanceSerie = new FloatSerie(this.Count, serie.Name + ".R", float.NaN);

//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
//            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

//            supportSerie[0] = float.NaN;
//            resistanceSerie[0] = lowSerie[0];
//            float latestHigh = highSerie[0];
//            float latestLow = lowSerie[0];

//            bool isOverbought = false;
//            bool isOversold = false;
//            for (int i = 1; i < this.Count; i++)
//            {
//                if (isOverbought)
//                {
//                    if (serie[i] < overbought)
//                    {
//                        // Resistance detected
//                        resistanceSerie[i] = latestHigh;
//                        supportSerie[i] = float.NaN;
//                        isOverbought = false;
//                    }
//                    else
//                    {
//                        resistanceSerie[i] = resistanceSerie[i - 1];
//                        supportSerie[i] = supportSerie[i - 1];
//                    }
//                }
//                else if (isOversold)
//                {
//                    if (serie[i] > oversold)
//                    {
//                        // Support detected
//                        supportSerie[i] = latestLow;
//                        resistanceSerie[i] = float.NaN;
//                        isOversold = false;
//                    }
//                    else
//                    {
//                        resistanceSerie[i] = resistanceSerie[i - 1];
//                        supportSerie[i] = supportSerie[i - 1];
//                    }
//                }
//                else
//                {
//                    resistanceSerie[i] = resistanceSerie[i - 1];
//                    supportSerie[i] = supportSerie[i - 1];
//                    if (serie[i] > overbought)
//                    {
//                        latestHigh = highSerie[i];
//                        isOverbought = true;
//                    }
//                    if (serie[i] < oversold)
//                    {
//                        latestLow = lowSerie[i];
//                        isOversold = true;
//                    }
//                }

//                latestHigh = Math.Max(highSerie[i], Math.Max(highSerie[i - 1], latestHigh));
//                latestLow = Math.Min(lowSerie[i], Math.Min(lowSerie[i - 1], latestLow));
//            }
//        }
//        #endregion
//        #region Indicators calculation


//        public float CalculateLastROx(string rox, int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            switch (rox.Substring(0, 3))
//            {
//                case "ROR":
//                    return CalculateLastROR(period, inputType, smoothingPeriod);
//                case "ROD":
//                    return CalculateLastROD(period, inputType, smoothingPeriod);
//                case "ROC":
//                    return CalculateLastROC(period);
//                default:
//                    break;
//            }
//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

//            var min = lowSerie.GetMin(this.LastIndex - period, this.LastIndex);
//            return (this.LastValue.CLOSE - min) / min;
//        }

//        public float CalculateLastROR(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

//            var min = lowSerie.GetMin(this.LastIndex - period, this.LastIndex);
//            return (this.LastValue.CLOSE - min) / min;
//        }
//        public float CalculateLastROC(int period)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            var @ref = closeSerie[this.LastIndex - period];
//            return (this.LastValue.CLOSE - @ref) / @ref;
//        }
//        public FloatSerie CalculateRateOfRise(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

//            FloatSerie serie = new FloatSerie(Values.Count());
//            float min;

//            for (int i = 0; i < Math.Min(period, this.Count); i++)
//            {
//                min = lowSerie.GetMin(0, i);
//                serie[i] = (closeSerie[i] - min) / min;
//            }
//            for (int i = period; i < this.Count; i++)
//            {
//                min = lowSerie.GetMin(i - period, i);
//                serie[i] = (closeSerie[i] - min) / min;
//            }
//            serie.Name = $"ROR_{period}";
//            return serie;
//        }
//        public float CalculateLastROD(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

//            var max = highSerie.GetMax(this.LastIndex - period, this.LastIndex);
//            return (this.LastValue.CLOSE - max) / max;
//        }

//        public FloatSerie CalculateRateOfDecline(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

//            FloatSerie serie = new FloatSerie(Values.Count());
//            float min;

//            for (int i = 0; i < Math.Min(period, this.Count); i++)
//            {
//                min = highSerie.GetMax(0, i);
//                serie[i] = -(closeSerie[i] - min) / min;
//            }
//            for (int i = period; i < this.Count; i++)
//            {
//                min = highSerie.GetMax(i - period, i);
//                serie[i] = -(closeSerie[i] - min) / min;
//            }
//            serie.Name = $"ROD_{period}";
//            return serie;
//        }

//        /// <summary>
//        /// Calulcate drawdown in value (euro), not %
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="inputType"></param>
//        /// <param name="smoothingPeriod"></param>
//        /// <returns></returns>
//        public FloatSerie CalculateDrawdownValue(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

//            FloatSerie serie = new FloatSerie(Values.Count());
//            float max;

//            for (int i = 0; i < Math.Min(period, this.Count); i++)
//            {
//                max = highSerie.GetMax(0, i);
//                serie[i] = max - closeSerie[i];
//            }
//            for (int i = period; i < this.Count; i++)
//            {
//                max = highSerie.GetMax(i - period, i);
//                serie[i] = max - closeSerie[i];
//            }
//            serie.Name = $"DD_{period}";
//            return serie;
//        }

//        public FloatSerie CalculateRateOfChange(int period, int smoothingPeriod = 0)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            if (smoothingPeriod > 1)
//                closeSerie = closeSerie.CalculateEMA(smoothingPeriod);

//            FloatSerie serie = new FloatSerie(Values.Count());
//            if (period == 0)
//            {
//                FloatSerie openSerie = this.GetSerie(StockDataType.OPEN);
//                if (smoothingPeriod > 1)
//                    openSerie = openSerie.CalculateEMA(smoothingPeriod);
//                for (int i = 0; i < this.Count; i++)
//                {
//                    serie[i] = (closeSerie[i] - openSerie[i]) / openSerie[i];
//                }
//            }
//            else
//            {
//                for (int i = 1; i < Math.Min(period, this.Count); i++)
//                {
//                    serie[i] = (closeSerie[i] - closeSerie[0]) / closeSerie[0];
//                }
//                for (int i = period; i < this.Count; i++)
//                {
//                    serie[i] = (closeSerie[i] - closeSerie[i - period]) / closeSerie[i - period];
//                }
//            }
//            serie.Name = $"ROC_{period}";
//            return serie;
//        }

//        /// <summary>
//        /// 
//        ///  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
//        ///  %D = MA3(%K)
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="inputType"></param>
//        /// <returns></returns>
//        public FloatSerie CalculateFastOscillator(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            FloatSerie fastOscillatorSerie = new FloatSerie(this.Count);
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);


//            for (int i = 0; i < Math.Min(period, this.Count); i++)
//            {
//                fastOscillatorSerie[i] = 50.0f;
//            }
//            for (int i = period; i < this.Count; i++)
//            {
//                var lowestLow = lowSerie.GetMin(i - period, i);
//                var highestHigh = highSerie.GetMax(i - period, i);
//                if (highestHigh == lowestLow)
//                {
//                    fastOscillatorSerie[i] = 50.0f;
//                }
//                else
//                {
//                    var close = closeSerie.Values[i];
//                    if (close == highestHigh)
//                        fastOscillatorSerie[i] = 100.0f;
//                    else if (close == lowestLow)
//                        fastOscillatorSerie[i] = 0.0f;
//                    else
//                        fastOscillatorSerie[i] = 100.0f * (close - lowestLow) / (highestHigh - lowestLow);
//                }
//            }
//            fastOscillatorSerie.Name = $"FastK({period})";
//            return fastOscillatorSerie;
//        }

//        /// <summary>
//        /// 
//        ///  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
//        ///  %D = MA3(%K)
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="inputType"></param>
//        /// <returns></returns>
//        public float CalculateLastFastOscillator(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            float fastOscillator = 50.0f;

//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);

//            int lastIndex = LastIndex;

//            var lowestLow = lowSerie.GetMin(lastIndex - period, lastIndex);
//            var highestHigh = highSerie.GetMax(lastIndex - period, lastIndex);
//            if (highestHigh == lowestLow)
//            {
//                fastOscillator = 50.0f;
//            }
//            else
//            {
//                var close = closeSerie.Last;
//                if (close == highestHigh)
//                    fastOscillator = 100.0f;
//                else if (close == lowestLow)
//                    fastOscillator = 0.0f;
//                else
//                    fastOscillator = 100.0f * (close - lowestLow) / (highestHigh - lowestLow);
//            }
//            return fastOscillator;
//        }

//        /// <summary>
//        ///  Get the price range over the period. Higuest in period - Lowest in period.
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="inputType"></param>
//        /// <returns></returns>
//        public float CalculateLastRange(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
//        {
//            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);

//            var lowestLow = lowSerie.GetMin(this.Count - period, this.Count);
//            var highestHigh = highSerie.GetMax(this.Count - period, this.Count);

//            return highestHigh - lowestLow;
//        }

//        public void GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, InputType inputType, int smoothingPeriod = -1)
//        {
//            switch (inputType)
//            {
//                case InputType.HighLow:
//                    lowSerie = this.GetSerie(StockDataType.LOW);
//                    highSerie = this.GetSerie(StockDataType.HIGH);
//                    break;
//                case InputType.Body:
//                    lowSerie = this.GetSerie(StockDataType.BODYLOW);
//                    highSerie = this.GetSerie(StockDataType.BODYHIGH);
//                    break;
//                case InputType.Close:
//                    highSerie = lowSerie = this.GetSerie(StockDataType.CLOSE);
//                    break;
//                case InputType.CloseEMA:
//                    if (smoothingPeriod > 1)
//                    {
//                        highSerie = lowSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(smoothingPeriod);
//                    }
//                    else
//                        throw new ArgumentOutOfRangeException(nameof(smoothingPeriod), "smoothingPeriod shall be greater than 1");
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, "Unexpected enum value in GetHighLowSeries");
//            }
//        }

//        public void CalculateEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            float alpha = 2.0f / (float)(period + 1);

//            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
//            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

//            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
//            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
//            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
//            int i = 1;
//            float extremum;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//                extremum = previousValue.HIGH;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//                extremum = previousValue.LOW;
//            }

//            foreach (StockDailyValue currentValue in this.Values.Skip(1))
//            {
//                if (upTrend)
//                {
//                    if (closeEMASerie[i] < longStopSerie[i - 1])
//                    {
//                        // Trailing stop has been broken => reverse trend
//                        upTrend = false;
//                        longStopSerie[i] = float.NaN;
//                        shortStopSerie[i] = extremum;
//                        extremum = lowEMASerie[i];
//                    }
//                    else
//                    {
//                        // Trail the stop
//                        float longStop = longStopSerie[i - 1];
//                        longStopSerie[i] = Math.Max(longStop, longStop + alpha * (lowEMASerie[i] - longStop));
//                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
//                        shortStopSerie[i] = float.NaN;
//                        extremum = Math.Max(extremum, highEMASerie[i]);
//                    }
//                }
//                else
//                {
//                    if (closeEMASerie[i] > shortStopSerie[i - 1])
//                    {
//                        // Trailing stop has been broken => reverse trend
//                        upTrend = true;
//                        longStopSerie[i] = extremum;
//                        shortStopSerie[i] = float.NaN;
//                        extremum = highEMASerie[i];
//                    }
//                    else
//                    {
//                        // Trail the stop  
//                        longStopSerie[i] = float.NaN;
//                        float shortStop = shortStopSerie[i - 1];
//                        shortStopSerie[i] = Math.Min(shortStop, shortStop + alpha * (highEMASerie[i] - shortStop));
//                        extremum = Math.Min(extremum, lowEMASerie[i]);
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }
//        public void CalculatePEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
//            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

//            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
//            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
//            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
//            FloatSerie EMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(period);

//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
//            int i = 1;
//            float extremum;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//                extremum = previousValue.HIGH;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//                extremum = previousValue.LOW;
//            }

//            foreach (StockDailyValue currentValue in this.Values.Skip(1))
//            {
//                if (upTrend)
//                {
//                    if (closeEMASerie[i] < longStopSerie[i - 1])
//                    {
//                        // Trailing stop has been broken => reverse trend
//                        upTrend = false;
//                        longStopSerie[i] = float.NaN;
//                        shortStopSerie[i] = extremum;
//                        extremum = lowEMASerie[i];
//                    }
//                    else
//                    {
//                        // Trail the stop
//                        float longStop = longStopSerie[i - 1];
//                        var step = EMASerie[i] - EMASerie[i - 1];
//                        longStopSerie[i] = Math.Max(longStop, longStop + step);
//                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
//                        shortStopSerie[i] = float.NaN;
//                        extremum = Math.Max(extremum, highEMASerie[i]);
//                    }
//                }
//                else
//                {
//                    if (closeEMASerie[i] > shortStopSerie[i - 1])
//                    {
//                        // Trailing stop has been broken => reverse trend
//                        upTrend = true;
//                        longStopSerie[i] = extremum;
//                        shortStopSerie[i] = float.NaN;
//                        extremum = highEMASerie[i];
//                    }
//                    else
//                    {
//                        // Trail the stop  
//                        longStopSerie[i] = float.NaN;
//                        float shortStop = shortStopSerie[i - 1];
//                        var step = EMASerie[i] - EMASerie[i - 1];
//                        shortStopSerie[i] = Math.Min(shortStop, shortStop + step);
//                        extremum = Math.Min(extremum, lowEMASerie[i]);
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }

//        public void CalculateHighLowSmoothedTrailStop(int period, int smoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
//            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

//            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW).CalculateEMA(smoothing);
//            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(smoothing);
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
//            int i = 0;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//            }
//            foreach (StockDailyValue currentValue in this.Values)
//            {
//                if (i > period)
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
//                        }
//                    }
//                }
//                else if (i > 0)
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = highSerie.GetMax(0, i);
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowSerie.GetMin(0, i);
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
//                        }
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }

//        /// <summary>
//        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="longStopSerie"></param>
//        /// <param name="shortStopSerie"></param>
//        public void CalculateCountbackLineTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAILCBL.LS");
//            shortStopSerie = new FloatSerie(this.Count, "TRAILCBL.SS");

//            if (this.ValueArray.Length < period) return;

//            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
//            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            bool upTrend = closeSerie[1] > closeSerie[0];
//            if (upTrend)
//            {
//                longStopSerie[0] = Math.Min(lowSerie[0], lowSerie[1]);
//                shortStopSerie[0] = float.NaN;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = Math.Max(highSerie[0], highSerie[1]);
//            }
//            for (int i = 1; i < this.Count; i++)
//            {
//                if (upTrend)
//                {
//                    if (closeSerie[i] < longStopSerie[i - 1])
//                    {// Trailing stop has been broken => reverse trend
//                        upTrend = false;
//                        longStopSerie[i] = float.NaN;
//                        shortStopSerie[i] = highSerie.GetCountBackHigh(i, period);
//                    }
//                    else
//                    {// Trail the stop  
//                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetCountBackLow(i, period));
//                        shortStopSerie[i] = float.NaN;
//                    }
//                }
//                else
//                {
//                    if (closeSerie[i] > shortStopSerie[i - 1])
//                    {  // Trailing stop has been broken => reverse trend
//                        upTrend = true;
//                        longStopSerie[i] = lowSerie.GetCountBackLow(i, period);
//                        shortStopSerie[i] = float.NaN;
//                    }
//                    else
//                    {
//                        // Trail the stop  
//                        longStopSerie[i] = float.NaN;
//                        if (lowSerie[i] < lowSerie[i - 1])
//                        {
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetCountBackHigh(i, period));
//                        }
//                        else
//                        {
//                            shortStopSerie[i] = shortStopSerie[i - 1];
//                        }
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
//        /// </summary>
//        /// <param name="period"></param>
//        /// <param name="longStopSerie"></param>
//        /// <param name="shortStopSerie"></param>
//        public void CalculateHighLowTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
//            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

//            if (this.ValueArray.Length < period) return;

//            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
//            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE > this.ValueArray[1].CLOSE;
//            int i = 1;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//            }
//            foreach (StockDailyValue currentValue in this.Values.Skip(1))
//            {
//                if (i > period)
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = highSerie.GetMax(i - period - 1, i);
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowSerie.GetMin(i - period - 1, i);
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
//                        }
//                    }
//                }
//                else
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = highSerie.GetMax(0, i);
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowSerie.GetMin(0, i);
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Trail the stop  
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
//                        }
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }


//        public void CalculateBandTrailStop(FloatSerie lowerBand, FloatSerie upperBand, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAIL.S");
//            shortStopSerie = new FloatSerie(this.Count, "TRAIL.R");
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//            }
//            int i = 0;
//            foreach (StockDailyValue currentValue in this.Values)
//            {
//                if (i > 0)
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = upperBand[i];
//                        }
//                        else
//                        {
//                            // UpTrend still in place
//                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBand[i]);
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowerBand[i];
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Down trend still in place
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBand[i]);
//                        }
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }

//        /// <summary>
//        /// Calculate a band trail based on upper band overshoting, without upper band to go up.
//        /// </summary>
//        /// <param name="lowerBand"></param>
//        /// <param name="upperBand"></param>
//        /// <param name="longStopSerie"></param>
//        /// <param name="shortStopSerie"></param>
//        public void CalculateBandTrailStop2(FloatSerie lowerBand, FloatSerie upperBand, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
//        {
//            longStopSerie = new FloatSerie(this.Count, "TRAIL.S");
//            shortStopSerie = new FloatSerie(this.Count, "TRAIL.R");
//            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
//            StockDailyValue previousValue = this.Values.First();
//            bool upTrend = previousValue.CLOSE < this.ValueArray[1].CLOSE;
//            if (upTrend)
//            {
//                longStopSerie[0] = previousValue.LOW;
//                shortStopSerie[0] = float.NaN;
//            }
//            else
//            {
//                longStopSerie[0] = float.NaN;
//                shortStopSerie[0] = previousValue.HIGH;
//            }
//            int i = 0;
//            foreach (StockDailyValue currentValue in this.Values)
//            {
//                if (i > 0)
//                {
//                    if (upTrend)
//                    {
//                        if (currentValue.CLOSE < longStopSerie[i - 1])
//                        { // Trailing stop has been broken => reverse trend
//                            upTrend = false;
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = upperBand[i];
//                        }
//                        else
//                        {
//                            // UpTrend still in place
//                            longStopSerie[i] = lowerBand[i];
//                            shortStopSerie[i] = float.NaN;
//                        }
//                    }
//                    else
//                    {
//                        if (currentValue.CLOSE > shortStopSerie[i - 1])
//                        {  // Trailing stop has been broken => reverse trend
//                            upTrend = true;
//                            longStopSerie[i] = lowerBand[i];
//                            shortStopSerie[i] = float.NaN;
//                        }
//                        else
//                        {
//                            // Down trend still in place
//                            longStopSerie[i] = float.NaN;
//                            shortStopSerie[i] = upperBand[i];
//                        }
//                    }
//                }
//                previousValue = currentValue;
//                i++;
//            }
//        }

//        #endregion

//        public bool BelongsToGroup(Groups group)
//        {
//            return BelongsToGroupFull(group);
//        }
//        public bool BelongsToGroupFull(Groups group)
//        {
//            if (group == Groups.NONE)
//                return false;
//            if (StockGroup == group || group == Groups.ALL)
//                return true;

//            switch (group)
//            {
//                case Groups.SAXO:
//                    return this.SaxoId > 0;
//            }

//            //if (DataProvider == StockDataProvider.ABC)
//            //    return ABCDataProvider.BelongsToGroup(this, group);

//            return false;
//        }
//        public bool BelongsToGroup(string groupName)
//        {
//            return this.BelongsToGroup((Groups)Enum.Parse(typeof(Groups), groupName));
//        }

//        #region Advanced Data Access (min/max ...)
//        private DateTime[] dateArray = null;

//        public int IndexOfFirstGreaterOrEquals(DateTime date)
//        {
//            if (dateArray == null)
//            {
//                if (this.Count > 0)
//                {
//                    dateArray = this.Keys.ToArray();
//                }
//                else
//                {
//                    return -1;
//                }
//            }
//            if (date > dateArray[dateArray.Length - 1]) { return -1; }
//            int index;
//            for (index = dateArray.Length - 1; index > 0; index--)
//            {
//                if (dateArray[index].Date <= date.Date) break;
//            }
//            return index;
//        }
//        public int IndexOfFirstLowerOrEquals(DateTime date)
//        {
//            if (dateArray == null)
//            {
//                if (this.Count > 0)
//                {
//                    dateArray = this.Keys.ToArray();
//                }
//                else
//                {
//                    return -1;
//                }
//            }
//            if (date < dateArray[0]) { return -1; }
//            int index;
//            for (index = dateArray.Length - 1; index > 0; index--)
//            {
//                if (dateArray[index].Date <= date.Date) break;
//            }
//            return index;
//        }
//        #endregion
//        #region IXmlSerializable Members
//        public System.Xml.Schema.XmlSchema GetSchema()
//        {
//            return null;
//        }
//        public void ReadXml(System.Xml.XmlReader reader)
//        {
//            // Deserialize Flat Attributes
//            this.StockName = reader.GetAttribute("StockName");
//            this.StockGroup = (Groups)Enum.Parse(typeof(Groups), reader.GetAttribute("StockGroup"));

//            // Deserialize Daily Value
//            XmlSerializer serializer = new XmlSerializer(typeof(StockDailyValue));
//            while (reader.Name == "StockDailyValue")
//            {
//                StockDailyValue stockDailyValue = (StockDailyValue)serializer.Deserialize(reader);
//                this.Add(stockDailyValue.DATE, stockDailyValue);
//            }
//        }
//        public void WriteXml(System.Xml.XmlWriter writer)
//        {
//            // Serialize Flat Attributes
//            writer.WriteStartElement(typeof(StockSerie).ToString());
//            writer.WriteAttributeString("StockName", this.StockName);
//            writer.WriteAttributeString("StockGroup", this.StockGroup.ToString());

//            // Serialize Daily Value
//            XmlSerializer serializer = new XmlSerializer(typeof(StockDailyValue));
//            foreach (StockDailyValue stockDailyValue in Values)
//            {
//                serializer.Serialize(writer, stockDailyValue);
//            }
//            writer.WriteEndElement();
//        }
//        #endregion
//        #region Generate related series

//        public List<StockDailyValue> GenerateSerieForTimeSpan(List<StockDailyValue> dailyValueList, BarDuration timeSpan)
//        {
//            StockLog.Write((string)("Name:" + this.StockName + " barDuration:" + timeSpan.ToString() + " CurrentBarDuration:" + this.BarDuration));
//            List<StockDailyValue> newBarList = null;
//            if (dailyValueList.Count == 0) return new List<StockDailyValue>();

//            // Load cache if exists
//            //StockDataProviderBase.LoadIntradayDurationArchive(StockAnalyzerSettings.Properties.Folders, this, timeSpan);

//            switch (timeSpan)
//            {
//                case StockClasses.BarDuration.Weekly:
//                    {
//                        StockDailyValue newValue = null;
//                        DayOfWeek previousDayOfWeek = DayOfWeek.Sunday;
//                        DateTime beginDate = dailyValueList.First().DATE;
//                        newBarList = new List<StockDailyValue>();

//                        foreach (StockDailyValue dailyValue in dailyValueList)
//                        {
//                            if (newValue == null)
//                            {
//                                newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
//                                   dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
//                                beginDate = dailyValue.DATE;
//                                previousDayOfWeek = dailyValue.DATE.DayOfWeek;
//                                newValue.IsComplete = false;
//                            }
//                            else
//                            {
//                                if (previousDayOfWeek < dailyValue.DATE.DayOfWeek)
//                                {
//                                    // We are in the week
//                                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
//                                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
//                                    newValue.CLOSE = dailyValue.CLOSE;
//                                    newValue.VOLUME += dailyValue.VOLUME;
//                                    previousDayOfWeek = dailyValue.DATE.DayOfWeek;
//                                }
//                                else
//                                {
//                                    // We switched to next week
//                                    newValue.IsComplete = true;
//                                    newBarList.Add(newValue);
//                                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
//                                    beginDate = dailyValue.DATE;
//                                    previousDayOfWeek = dailyValue.DATE.DayOfWeek;
//                                    newValue.IsComplete = false;
//                                }
//                            }
//                        }
//                        if (newValue != null)
//                        {
//                            var lastDailyValue = dailyValueList.Last();
//                            if (lastDailyValue.DATE.DayOfWeek == DayOfWeek.Friday)
//                                newValue.IsComplete = lastDailyValue.IsComplete;
//                            newBarList.Add(newValue);
//                        }
//                    }
//                    break;
//                case StockClasses.BarDuration.Monthly:
//                    {
//                        StockDailyValue newValue = null;
//                        int previousMonth = dailyValueList.First().DATE.Month;
//                        DateTime beginDate = dailyValueList.First().DATE;
//                        newBarList = new List<StockDailyValue>();

//                        foreach (StockDailyValue dailyValue in dailyValueList)
//                        {
//                            if (newValue == null)
//                            {
//                                newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
//                                   dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
//                                beginDate = dailyValue.DATE;
//                                previousMonth = dailyValue.DATE.Month;
//                                newValue.IsComplete = false;
//                            }
//                            else
//                            {
//                                if (previousMonth == dailyValue.DATE.Month)
//                                {
//                                    // We are in the month
//                                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
//                                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
//                                    newValue.VOLUME += dailyValue.VOLUME;
//                                    newValue.CLOSE = dailyValue.CLOSE;
//                                    previousMonth = dailyValue.DATE.Month;
//                                }
//                                else
//                                {
//                                    // We switched to next month
//                                    newValue.IsComplete = true;
//                                    newBarList.Add(newValue);
//                                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW,
//                                       dailyValue.CLOSE, dailyValue.VOLUME, dailyValue.DATE);
//                                    beginDate = dailyValue.DATE;
//                                    previousMonth = dailyValue.DATE.Month;
//                                    newValue.IsComplete = false;
//                                }
//                            }
//                        }
//                        if (newValue != null)
//                        {
//                            // Check if bar complete
//                            var currentMonth = newValue.DATE.Month;
//                            var lastDailyValue = dailyValueList.Last().DATE;
//                            if (lastDailyValue.DayOfWeek == DayOfWeek.Friday && lastDailyValue.AddDays(3).Month != currentMonth)
//                                newValue.IsComplete = true;
//                            newBarList.Add(newValue);
//                        }
//                    }
//                    break;
//                default:
//                    {
//                        int period;
//                        string[] timeSpanString = timeSpan.ToString().Split('_');
//                        switch (timeSpanString[0].ToUpper())
//                        {
//                            case "H":
//                                if (timeSpanString.Length > 1 && int.TryParse(timeSpanString[1], out period))
//                                {
//                                    newBarList = GenerateHourBar(dailyValueList, period);
//                                }
//                                break;
//                            case "M":
//                                if (timeSpanString.Length > 1 && int.TryParse(timeSpanString[1], out period))
//                                {
//                                    newBarList = GenerateMinuteBar(dailyValueList, period);
//                                }
//                                break;
//                            default:
//                                throw new NotImplementedException("Bar duration: " + timeSpan.ToString() + " is not implemented");
//                        }
//                    }
//                    break;
//            }
//            return newBarList;
//        }
//        private List<StockDailyValue> GenerateMinuteBar(List<StockDailyValue> stockDailyValueList, int nbMinutes)
//        {
//            if (nbMinutes <= 4)
//                return stockDailyValueList;
//            List<StockDailyValue> newBarList = new List<StockDailyValue>();
//            StockDailyValue newValue = null;
//            DateTime closeDate = DateTime.Now;
//            foreach (StockDailyValue dailyValue in stockDailyValueList)
//            {
//                if (newValue == null)
//                {
//                    // New bar
//                    int min = dailyValue.DATE.Minute / nbMinutes * nbMinutes;
//                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, min, 0);
//                    closeDate = openDate.AddMinutes(nbMinutes);
//                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
//                    newValue.IsComplete = false;
//                }
//                else if (dailyValue.DATE.Date != newValue.DATE.Date || dailyValue.DATE >= closeDate)
//                {
//                    // Force bar end at the end of a day
//                    newValue.IsComplete = true;
//                    newBarList.Add(newValue);

//                    // New bar
//                    int min = dailyValue.DATE.Minute / nbMinutes * nbMinutes;
//                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, min, 0);
//                    closeDate = openDate.AddMinutes(nbMinutes);
//                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
//                    newValue.IsComplete = false;
//                }
//                else
//                {
//                    // Need to extend current bar
//                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
//                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
//                    newValue.VOLUME += dailyValue.VOLUME;
//                    newValue.CLOSE = dailyValue.CLOSE;
//                }
//            }
//            if (newValue != null)
//            {
//                newBarList.Add(newValue);
//            }
//            return newBarList;
//        }
//        private List<StockDailyValue> GenerateHourBar(List<StockDailyValue> stockDailyValueList, int nbHours)
//        {
//            List<StockDailyValue> newBarList = new List<StockDailyValue>();
//            StockDailyValue newValue = null;
//            DateTime closeDate = DateTime.Now;
//            foreach (StockDailyValue dailyValue in stockDailyValueList)
//            {
//                if (newValue == null)
//                {
//                    // New bar
//                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, 0, 0);
//                    closeDate = openDate.AddHours(nbHours);
//                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
//                    newValue.IsComplete = false;
//                }
//                else if (dailyValue.DATE.Date != newValue.DATE.Date || dailyValue.DATE >= closeDate)
//                {
//                    // Force bar end at the end of a day
//                    newValue.IsComplete = true;
//                    newBarList.Add(newValue);

//                    // New bar
//                    var openDate = new DateTime(dailyValue.DATE.Year, dailyValue.DATE.Month, dailyValue.DATE.Day, dailyValue.DATE.Hour, 0, 0);
//                    closeDate = openDate.AddHours(nbHours);

//                    newValue = new StockDailyValue(dailyValue.OPEN, dailyValue.HIGH, dailyValue.LOW, dailyValue.CLOSE, dailyValue.VOLUME, openDate);
//                    newValue.IsComplete = false;
//                }
//                else
//                {
//                    // Need to extend current bar
//                    newValue.HIGH = Math.Max(newValue.HIGH, dailyValue.HIGH);
//                    newValue.LOW = Math.Min(newValue.LOW, dailyValue.LOW);
//                    newValue.VOLUME += dailyValue.VOLUME;
//                    newValue.CLOSE = dailyValue.CLOSE;
//                }
//            }
//            if (newValue != null)
//            {
//                newBarList.Add(newValue);
//            }
//            return newBarList;
//        }
//        public List<StockDailyValue> GenerateSmoothedBars(List<StockDailyValue> stockDailyValueList, int nbDay)
//        {
//            List<StockDailyValue> newBarList = new List<StockDailyValue>();

//            FloatSerie closeSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.CLOSE).ToArray()).CalculateEMA(nbDay);
//            FloatSerie highSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.HIGH).ToArray()).CalculateEMA(nbDay);
//            FloatSerie lowSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.LOW).ToArray()).CalculateEMA(nbDay);
//            FloatSerie openSerie = new FloatSerie(stockDailyValueList.Select(dv => dv.OPEN).ToArray()).CalculateEMA(nbDay);

//            StockDailyValue previousValue = stockDailyValueList[0];
//            for (int i = 0; i < stockDailyValueList.Count; i++)
//            {
//                StockDailyValue dailyValue = stockDailyValueList[i];

//                float open = openSerie[i];
//                float high = highSerie[i];
//                float low = lowSerie[i];
//                float close = closeSerie[i];

//                // New bar
//                StockDailyValue newValue = new StockDailyValue(open, high, low, close, dailyValue.VOLUME, dailyValue.DATE);
//                newValue.IsComplete = dailyValue.IsComplete;
//                newBarList.Add(newValue);

//                previousValue = dailyValue;
//            }
//            return newBarList;
//        }

//        #endregion



//        #region CSV file IO
//        public bool ReadFromCSVFile(string fileName)
//        {
//            bool result = false;
//            if (File.Exists(fileName))
//            {
//                using (StreamReader sr = new StreamReader(fileName))
//                {
//                    sr.ReadLine();  // Skip the first line
//                    StockDailyValue readValue = null;
//                    while (!sr.EndOfStream)
//                    {
//                        readValue = StockDailyValue.ReadMarketDataFromCSVStream(sr, this.StockName, true);
//                        if (readValue != null && !this.ContainsKey(readValue.DATE))
//                        {
//                            this.Add(readValue.DATE, readValue);
//                        }
//                    }
//                }
//                result = true;
//            }
//            return result;
//        }

//        public void SaveToCSV(string fileName, DateTime startDate, bool archive)
//        {
//            using StreamWriter sw = new StreamWriter(fileName);
//            sw.WriteLine(StockDailyValue.StringFormat());
//            foreach (StockDailyValue value in this.Values)
//            {
//                if (value.DATE >= startDate && !archive && value.DATE <= DateTime.Today)
//                {
//                    sw.WriteLine(value.ToString());
//                }
//                else if (value.DATE <= startDate && archive)
//                {
//                    sw.WriteLine(value.ToString());
//                }
//            }
//        }

//        public void SaveToCSVFromDateToDate(string fileName, DateTime startDate, DateTime endDate)
//        {
//            var values = this.Values.Where(v => v.DATE >= startDate && v.DATE <= endDate).ToList();
//            if (values.Count() > 0)
//            {
//                using StreamWriter sw = new StreamWriter(fileName);
//                sw.WriteLine(StockDailyValue.StringFormat());
//                foreach (StockDailyValue value in values)
//                {
//                    sw.WriteLine(value.ToString());
//                }
//            }
//            else if (File.Exists(fileName))
//            {
//                File.Delete(fileName);
//            }
//        }
//        #endregion


//        public override string ToString()
//        {
//            return this.StockName + " " + this.Count;
//        }

//        #region Multithread Lock/Unlock

//        static readonly bool lockLoggingActive = false;

//        readonly object __lockObj = new object();
//        public void Lock()
//        {
//            using MethodLogger ml = new MethodLogger(this, lockLoggingActive, this.StockName);
//            bool lockTaken = false;
//            while (!lockTaken)
//            {
//                StockLog.Write($"Trying to lock {this.StockName}", lockLoggingActive);
//                Monitor.TryEnter(__lockObj, 500, ref lockTaken);
//            }
//            StockLog.Write("Lock taken", lockLoggingActive);
//        }

//        public void UnLock()
//        {
//            using MethodLogger ml = new MethodLogger(this, lockLoggingActive, this.StockName);
//            Monitor.Exit(__lockObj);
//        }

//        internal void AddIntradayValue(StockDailyValue dailyValue)
//        {
//            if (!this.Initialise())
//                return;

//            var barDuration = this.BarDuration;
//            this.BarDuration = BarDuration.Daily;

//            this.IsInitialised = false;
//            if (!StockDataProviderBase.LoadSerieData(this) || this.Count == 0)
//            {
//                return;
//            }

//            if (this.Values.Last().DATE < dailyValue.DATE.Date)
//            {
//                this.Add(dailyValue.DATE, dailyValue);

//                if (this.BarSmoothedDictionary.ContainsKey("Daily"))
//                {
//                    this.BarSmoothedDictionary.Remove("Daily");
//                }
//                this.BarSmoothedDictionary.Add("Daily", this.Values.ToList());

//                var instrument = StockDictionary.Instruments[this.StockName];
//                instrument.ClearCache();
//            }

//            this.Initialise();
//            this.BarDuration = barDuration;
//        }

//        #endregion
//    }
//}