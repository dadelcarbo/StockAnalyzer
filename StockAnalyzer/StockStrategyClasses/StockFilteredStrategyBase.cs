using System.IO;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;

namespace StockAnalyzer.StockStrategyClasses
{
    public class StockFilteredStrategyBase : IStockFilteredStrategy
    {
        #region StockStrategy Properties
        virtual public string Name { get; set; }
        virtual public string Description { get; set; }
        [XmlIgnore]
        public bool IsBuyStrategy
        {
            get { return true; }
        }
        [XmlIgnore]
        public bool IsSellStrategy
        {
            get { return true; }
        }
        [XmlIgnore]
        public bool SupportShortSelling { get; protected set; }

        [XmlIgnore]
        public StockSerie Serie { get; set; }
        [XmlIgnore]
        public StockOrder LastBuyOrder { get; set; }

        #region Filter
        private IStockEvent filterIndicator { get; set; }
        [XmlIgnore]
        public IStockEvent FilterIndicator { get; set; }
        public string FilterName
        {
            get
            {
                if (this.FilterIndicator == null)
                {
                    return string.Empty;
                }
                else
                {
                    if (this.FilterIndicator is IStockDecorator)
                    {
                        IStockDecorator decorator = this.FilterIndicator as IStockDecorator;
                        return decorator.Name + "|" + decorator.DecoratedItem;
                    }
                    else
                    {
                        return (this.FilterIndicator as IStockViewableSeries).Name;
                    }
                }
            }
            set
            {
                string indicatorName = value.Split('(')[0];
                if (StockIndicatorManager.Supports(indicatorName))
                {
                    FilterIndicator = StockIndicatorManager.CreateIndicator(value);
                }
                else
                {
                    if (StockTrailStopManager.Supports(indicatorName))
                    {
                        FilterIndicator = StockTrailStopManager.CreateTrailStop(value);
                    }
                    else
                    {
                        if (StockPaintBarManager.Supports(indicatorName))
                        {
                            FilterIndicator = StockPaintBarManager.CreatePaintBar(value);
                        }
                        else
                        {
                            if (StockDecoratorManager.Supports(indicatorName))
                            {
                                string[] fields = value.Split('|');
                                if (fields.Length == 2)
                                {
                                    FilterIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public string OkToBuyFilterEventName { get; set; }
        public string OkToShortFilterEventName { get; set; }

        protected int OkToBuyFilterEventIndex = -1;
        protected int OkToShortFilterEventIndex = -1;
        #endregion

        #region Entry
        [XmlIgnore]
        public IStockEvent EntryTriggerIndicator { get; set; }

        public string EntryTriggerName
        {
            get
            {
                if (this.EntryTriggerIndicator == null)
                {
                    return string.Empty;
                }
                else
                {
                    if (this.EntryTriggerIndicator is IStockDecorator)
                    {
                        IStockDecorator decorator = this.EntryTriggerIndicator as IStockDecorator;
                        return decorator.Name + "|" + decorator.DecoratedItem;
                    }
                    else
                    {
                        return (this.EntryTriggerIndicator as IStockViewableSeries).Name;
                    }
                }
            }
            set
            {
                string indicatorName = value.Split('(')[0];
                if (StockIndicatorManager.Supports(indicatorName))
                {
                    EntryTriggerIndicator = StockIndicatorManager.CreateIndicator(value);
                }
                else
                {
                    if (StockTrailStopManager.Supports(indicatorName))
                    {
                        EntryTriggerIndicator = StockTrailStopManager.CreateTrailStop(value);
                    }
                    else
                    {
                        if (StockPaintBarManager.Supports(indicatorName))
                        {
                            EntryTriggerIndicator = StockPaintBarManager.CreatePaintBar(value);
                        }
                        else
                        {
                            if (StockDecoratorManager.Supports(indicatorName))
                            {
                                string[] fields = value.Split('|');
                                if (fields.Length == 2)
                                {
                                    EntryTriggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                                }
                            }
                        }
                    }
                }
            }
        }
        public string BuyTriggerEventName { get; set; }
        public string ShortTriggerEventName { get; set; }

        protected int BuyTriggerEventIndex = -1;
        protected int ShortTriggerEventIndex = -1;

        #endregion

        #region Exit
        [XmlIgnore]
        public IStockEvent ExitTriggerIndicator { get; set; }

        public string ExitTriggerName
        {
            get
            {
                if (this.ExitTriggerIndicator == null)
                {
                    return string.Empty;
                }
                else
                {
                    if (this.ExitTriggerIndicator is IStockDecorator)
                    {
                        IStockDecorator decorator = this.ExitTriggerIndicator as IStockDecorator;
                        return decorator.Name + "|" + decorator.DecoratedItem;
                    }
                    else
                    {
                        return (this.ExitTriggerIndicator as IStockViewableSeries).Name;
                    }
                }
            }
            set
            {
                string indicatorName = value.Split('(')[0];
                if (StockIndicatorManager.Supports(indicatorName))
                {
                    ExitTriggerIndicator = StockIndicatorManager.CreateIndicator(value);
                }
                else
                {
                    if (StockTrailStopManager.Supports(indicatorName))
                    {
                        ExitTriggerIndicator = StockTrailStopManager.CreateTrailStop(value);
                    }
                    else
                    {
                        if (StockPaintBarManager.Supports(indicatorName))
                        {
                            ExitTriggerIndicator = StockPaintBarManager.CreatePaintBar(value);
                        }
                        else
                        {
                            if (StockDecoratorManager.Supports(indicatorName))
                            {
                                string[] fields = value.Split('|');
                                if (fields.Length == 2)
                                {
                                    ExitTriggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                                }
                            }
                        }
                    }
                }
            }
        }
        public string SellTriggerEventName { get; set; }
        public string CoverTriggerEventName { get; set; }

        protected int SellTriggerEventIndex = -1;
        protected int CoverTriggerEventIndex = -1;
        #endregion

        #region Stop
        [XmlIgnore]
        public IStockEvent StopTriggerIndicator { get; set; }

        public string StopTriggerName
        {
            get
            {
                if (this.StopTriggerIndicator == null)
                {
                    return string.Empty;
                }
                else
                {
                    if (this.StopTriggerIndicator is IStockDecorator)
                    {
                        IStockDecorator decorator = this.StopTriggerIndicator as IStockDecorator;
                        return decorator.Name + "|" + decorator.DecoratedItem;
                    }
                    else
                    {
                        return (this.StopTriggerIndicator as IStockViewableSeries).Name;
                    }
                }
            }
            set
            {
                string indicatorName = value.Split('(')[0];
                if (StockIndicatorManager.Supports(indicatorName))
                {
                    StopTriggerIndicator = StockIndicatorManager.CreateIndicator(value);
                }
                else
                {
                    if (StockTrailStopManager.Supports(indicatorName))
                    {
                        StopTriggerIndicator = StockTrailStopManager.CreateTrailStop(value);
                    }
                    else
                    {
                        if (StockPaintBarManager.Supports(indicatorName))
                        {
                            StopTriggerIndicator = StockPaintBarManager.CreatePaintBar(value);
                        }
                        else
                        {
                            if (StockDecoratorManager.Supports(indicatorName))
                            {
                                string[] fields = value.Split('|');
                                if (fields.Length == 2)
                                {
                                    StopTriggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                                }
                            }
                        }
                    }
                }
            }
        }
        public string StopLongTriggerEventName { get; set; }
        public string StopShortTriggerEventName { get; set; }

        protected int StopLongTriggerEventIndex = -1;
        protected int StopShortTriggerEventIndex = -1;
        #endregion

        #endregion
        #region StockStrategy Methods

        public StockFilteredStrategyBase(IStockEvent filterEvent, string okToBuyFilterEventName, string okToShortFilterEventName,
            IStockEvent entryTriggerEvent, string buyTriggerEventName, string shortTriggerEventName,
            IStockEvent exitTriggerEvent, string sellTriggerEventName, string coverTriggerEventName,
            IStockEvent stopTriggerEvent, string stopLongTriggerEventName, string stopShortTriggerEventName)
        {
            EntryTriggerIndicator = entryTriggerEvent;
            BuyTriggerEventName = buyTriggerEventName;
            ShortTriggerEventName = shortTriggerEventName;

            ExitTriggerIndicator = exitTriggerEvent;
            SellTriggerEventName = sellTriggerEventName;
            CoverTriggerEventName = coverTriggerEventName;

            StopTriggerIndicator = stopTriggerEvent;
            StopLongTriggerEventName = stopLongTriggerEventName;
            StopShortTriggerEventName = stopShortTriggerEventName;

            FilterIndicator = filterEvent;
            OkToBuyFilterEventName = okToBuyFilterEventName;
            OkToShortFilterEventName = okToShortFilterEventName;
        }

        protected StockFilteredStrategyBase()
        {
        }

        public virtual void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
        {
            this.Serie = stockSerie;
            this.LastBuyOrder = lastBuyOrder;
            this.SupportShortSelling = supportShortSelling;

            // Initialise trigger indicator
            IStockViewableSeries entryTriggerSerie = this.EntryTriggerIndicator as IStockViewableSeries;
            if (entryTriggerSerie != null)
            {
                if (stockSerie.HasVolume || !entryTriggerSerie.RequiresVolumeData)
                {
                    this.EntryTriggerIndicator = stockSerie.GetStockEvents(entryTriggerSerie);
                }
                else
                {
                    throw new System.Exception("This serie has no volume information but is required for " + entryTriggerSerie.Name);
                }
                try
                {
                    BuyTriggerEventIndex = this.EntryTriggerIndicator.EventNames.ToList().IndexOf(BuyTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + entryTriggerSerie.Name + "(" + BuyTriggerEventName + ")");
                }
                try
                {
                    ShortTriggerEventIndex = this.EntryTriggerIndicator.EventNames.ToList().IndexOf(ShortTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + entryTriggerSerie.Name + "(" + ShortTriggerEventName + ")");
                }
            }

            // Initialise trigger indicator
            IStockViewableSeries exitTriggerSerie = this.ExitTriggerIndicator as IStockViewableSeries;
            if (exitTriggerSerie != null)
            {
                if (stockSerie.HasVolume || !exitTriggerSerie.RequiresVolumeData)
                {
                    this.ExitTriggerIndicator = stockSerie.GetStockEvents(exitTriggerSerie);
                }
                else
                {
                    throw new System.Exception("This serie has no volume information but is required for " + exitTriggerSerie.Name);
                }
                try
                {
                    SellTriggerEventIndex = this.ExitTriggerIndicator.EventNames.ToList().IndexOf(SellTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + exitTriggerSerie.Name + "(" + SellTriggerEventName + ")");
                }
                try
                {
                    CoverTriggerEventIndex = this.ExitTriggerIndicator.EventNames.ToList().IndexOf(CoverTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + exitTriggerSerie.Name + "(" + CoverTriggerEventName + ")");
                }
            }

            // Initialise trigger indicator
            IStockViewableSeries stopTriggerSerie = this.StopTriggerIndicator as IStockViewableSeries;
            if (stopTriggerSerie != null)
            {
                if (stockSerie.HasVolume || !stopTriggerSerie.RequiresVolumeData)
                {
                    this.StopTriggerIndicator = stockSerie.GetStockEvents(stopTriggerSerie);
                }
                else
                {
                    throw new System.Exception("This serie has no volume information but is required for " + stopTriggerSerie.Name);
                }
                try
                {
                    StopLongTriggerEventIndex = this.StopTriggerIndicator.EventNames.ToList().IndexOf(StopLongTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + stopTriggerSerie.Name + "(" + StopLongTriggerEventName + ")");
                }
                try
                {
                    StopShortTriggerEventIndex = this.StopTriggerIndicator.EventNames.ToList().IndexOf(StopShortTriggerEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no triggering event" + stopTriggerSerie.Name + "(" + StopShortTriggerEventName + ")");
                }
            }

            // Initialise filter indicator
            IStockViewableSeries filterSerie = this.FilterIndicator as IStockViewableSeries;
            if (filterSerie != null)
            {
                if (stockSerie.HasVolume || !filterSerie.RequiresVolumeData)
                {
                    this.filterIndicator = stockSerie.GetStockEvents(filterSerie);
                }
                else
                {
                    throw new System.Exception("This serie has no volume information but is required for " + filterSerie.Name);
                }

                // Get event indexes
                try
                {
                    OkToBuyFilterEventIndex = this.FilterIndicator.EventNames.ToList().IndexOf(OkToBuyFilterEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no filtering event" + filterSerie.Name + "(" + OkToBuyFilterEventName + ")");
                }
                try
                {

                    OkToShortFilterEventIndex = this.FilterIndicator.EventNames.ToList().IndexOf(OkToShortFilterEventName);
                }
                catch
                {
                    throw new System.Exception("This indicator has no filtering event" + filterSerie.Name + "(" + OkToShortFilterEventName + ")");
                }
            }
        }

        virtual public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            if (this.SupportShortSelling)
            {
                if (this.filterIndicator.Events[this.OkToShortFilterEventIndex][index] && this.EntryTriggerIndicator.Events[this.ShortTriggerEventIndex][index])
                {
                    return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
                }
            }
            if (this.filterIndicator.Events[this.OkToBuyFilterEventIndex][index] && this.EntryTriggerIndicator.Events[this.BuyTriggerEventIndex][index])
            {
                return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }

            return null;
        }
        virtual public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            if (LastBuyOrder.IsShortOrder)
            {
                if (this.ExitTriggerIndicator.Events[this.CoverTriggerEventIndex][index] || 
                    (this.StopTriggerIndicator != null && this.StopTriggerIndicator.Events[this.StopShortTriggerEventIndex][index]))
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                        dailyValue.DATE.AddDays(30), number, true);
                }
            }
            else
            {
                if (this.ExitTriggerIndicator.Events[this.SellTriggerEventIndex][index] ||
                    (this.StopTriggerIndicator != null && this.StopTriggerIndicator.Events[this.StopLongTriggerEventIndex][index]))
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                        dailyValue.DATE.AddDays(30), number, false);
                }
            }

            return null;
        }
        virtual public void AmendBuyOrder(ref StockOrder stockOrder, StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            // stockOrder = TryToBuy(dailyValue, index, amount, ref benchmark);
        }
        virtual public void AmendSellOrder(ref StockOrder stockOrder, StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            // stockOrder = TryToSell(dailyValue, index, ref benchmark);
        }
        #endregion

        public void Save(string rootFolder)
        {
            string folderName = rootFolder + @"\FilteredStrategies";
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
            }
            string fileName = folderName + @"\" + this.Name + ".xml";
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StockFilteredStrategyBase));
                System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(fs, null);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(xmlWriter, this);
            }
        }
        const string ThemeTemplate = @"#ScrollGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line
DATA|CLOSE|1:255:0:0:0:Solid|True
#CloseGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart|1:255:184:134:11:Solid
DATA|CLOSE|1:255:0:0:0:Solid|True
@PriceIndicator
SECONDARY|NONE
#Indicator1Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator1
#Indicator2Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator2
#Indicator3Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator3
#VolumeGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart";

        public string ToTheme()
        {
            indCount = 1;

            string theme = ThemeTemplate;
            IStockViewableSeries indicator = (IStockViewableSeries)this.FilterIndicator;
            theme = AppendThemeLine(indicator, theme);

            indicator = (IStockViewableSeries)this.EntryTriggerIndicator;
            theme = AppendThemeLine(indicator, theme);

            indicator = (IStockViewableSeries)this.ExitTriggerIndicator;
            theme = AppendThemeLine(indicator, theme);

            indicator = (IStockViewableSeries)this.StopTriggerIndicator;
            theme = AppendThemeLine(indicator, theme);

            return theme;
        }

        private static int indCount;
        private static string AppendThemeLine(IStockViewableSeries indicator, string theme)
        {
            int index = -1;
            if (indicator.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
            {
                index = theme.IndexOf("@PriceIndicator");
                theme = theme.Insert(index, indicator.ToThemeString() + System.Environment.NewLine);
            }
            else
            {
                if (indicator is IStockIndicator)
                {
                    IStockIndicator stockIndicator = indicator as IStockIndicator;
                    if (stockIndicator.HorizontalLines != null && stockIndicator.HorizontalLines.Count() > 0)
                    {
                        foreach (HLine hline in stockIndicator.HorizontalLines)
                        {
                            theme = theme.Replace("@Indicator" + indCount,
                               "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) + System.Environment.NewLine + "@Indicator" + indCount);
                        }
                    }
                }
                if (indicator is IStockDecorator)
                {
                    IStockDecorator decorator = indicator as IStockDecorator;
                    IStockIndicator decoratedIndicator = StockIndicatorManager.CreateIndicator(decorator.DecoratedItem);
                    theme = theme.Replace("@Indicator" + indCount, decoratedIndicator.ToThemeString() + System.Environment.NewLine + "@Indicator" + indCount);
                    if (decoratedIndicator.HorizontalLines != null && decoratedIndicator.HorizontalLines.Count() > 0)
                    {
                        foreach (HLine hline in decoratedIndicator.HorizontalLines)
                        {
                            theme = theme.Replace("@Indicator" + indCount,
                               "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) + System.Environment.NewLine + "@Indicator" + indCount);
                        }
                    }
                }
                theme = theme.Replace("@Indicator" + indCount, indicator.ToThemeString());



                indCount++;
            }
            return theme;
        }
    }
}