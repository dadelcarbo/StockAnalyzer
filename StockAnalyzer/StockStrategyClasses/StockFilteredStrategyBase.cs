using System.IO;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

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

      [XmlIgnore]
      public IStockEvent TriggerIndicator { get; set; }

      public string TriggerName
      {
         get { return this.TriggerIndicator == null ? string.Empty : (this.TriggerIndicator as IStockViewableSeries).Name; }
         set
         {
            if (StockIndicatorManager.Supports(value.Split('(')[0]))
            {
               this.TriggerIndicator = StockIndicatorManager.CreateIndicator(value);
            }
            if (this.TriggerIndicator == null) this.TriggerIndicator = StockTrailStopManager.CreateTrailStop(value);
         }
      }

      [XmlIgnore]
      public IStockEvent FilterIndicator { get; set; }
      public string FilterName
      {
         get { return this.FilterIndicator == null ? string.Empty : (this.FilterIndicator as IStockViewableSeries).Name; }
         set
         {
            if (StockIndicatorManager.Supports(value.Split('(')[0]))
            {
               this.FilterIndicator = StockIndicatorManager.CreateIndicator(value);
            }
            if (this.FilterIndicator == null) this.FilterIndicator = StockTrailStopManager.CreateTrailStop(value);
         }
      }


      public string BuyTriggerEventName { get; set; }
      public string ShortTriggerEventName { get; set; }

      protected int BuyTriggerEventIndex = -1;
      protected int ShortTriggerEventIndex = -1;

      public string OkToBuyFilterEventName { get; set; }
      public string OkToShortFilterEventName { get; set; }

      protected int OkToBuyFilterEventIndex = -1;
      protected int OkToShortFilterEventIndex = -1;

      #endregion
      #region StockStrategy Methods

      public StockFilteredStrategyBase(IStockEvent filterEvent, IStockEvent triggerEvent, string buyTriggerEventName, string shortTriggerEventName, string okToBuyFilterEventName, string okToShortFilterEventName)
      {
         TriggerIndicator = triggerEvent;
         FilterIndicator = filterEvent;
         BuyTriggerEventName = buyTriggerEventName;
         ShortTriggerEventName = shortTriggerEventName;
         OkToBuyFilterEventName = okToBuyFilterEventName;
         OkToShortFilterEventName = okToShortFilterEventName;
      }

      protected StockFilteredStrategyBase()
      {
      }

      virtual public void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         this.Serie = stockSerie;
         this.LastBuyOrder = lastBuyOrder;
         this.SupportShortSelling = supportShortSelling;

         // Initialise trigger indicator
         IStockViewableSeries triggerSerie = this.TriggerIndicator as IStockViewableSeries;
         if (triggerSerie != null)
         {

            if (stockSerie.HasVolume || !triggerSerie.RequiresVolumeData)
            {
               triggerSerie.ApplyTo(stockSerie);
            }
            else
            {
               throw new System.Exception("This serie has no volume information but is required for " + triggerSerie.Name);
            }
            try
            {
               BuyTriggerEventIndex = this.TriggerIndicator.EventNames.ToList().IndexOf(BuyTriggerEventName);
            }
            catch
            {
               throw new System.Exception("This indicator has no triggering event" + triggerSerie.Name + "(" + OkToBuyFilterEventName + ")");
            }
            try
            {
               ShortTriggerEventIndex = this.TriggerIndicator.EventNames.ToList().IndexOf(ShortTriggerEventName);
            }
            catch
            {
               throw new System.Exception("This indicator has no triggering event" + triggerSerie.Name + "(" + OkToShortFilterEventName + ")");
            }
         }

         // Initialise filter indicator
         IStockViewableSeries filterSerie = this.FilterIndicator as IStockViewableSeries;
         if (filterSerie != null)
         {
            if (stockSerie.HasVolume || !filterSerie.RequiresVolumeData)
            {
               filterSerie.ApplyTo(stockSerie);
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
            if (this.FilterIndicator.Events[this.OkToShortFilterEventIndex][index] && this.TriggerIndicator.Events[this.ShortTriggerEventIndex][index])
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
            }
         }
         if (this.FilterIndicator.Events[this.OkToBuyFilterEventIndex][index] && this.TriggerIndicator.Events[this.BuyTriggerEventIndex][index])
         {
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }

         return null;
      }
      virtual public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         if (LastBuyOrder.IsShortOrder)
         {
            if (this.TriggerIndicator.Events[this.BuyTriggerEventIndex][index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                   dailyValue.DATE.AddDays(30), number, true);
            }
         }
         else
         {
            if (this.TriggerIndicator.Events[this.ShortTriggerEventIndex][index])
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

         indicator = (IStockViewableSeries)this.TriggerIndicator;
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
            theme = theme.Replace("@Indicator" + indCount, indicator.ToThemeString());
            indCount++;
         }
         return theme;
      }
   }
}