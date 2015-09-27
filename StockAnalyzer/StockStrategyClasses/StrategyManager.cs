using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockStrategyClasses
{
   public class StrategyManager
   {
      private static List<string> strategyList = null;

      private StrategyManager()
      {
      }
      public static void ResetStrategyList()
      {
         strategyList = null;
      }
      public static List<string> GetStrategyList()
      {
         if (strategyList == null)
         {
            strategyList = new List<string>();
            StrategyManager sm = new StrategyManager();
            foreach (Type t in sm.GetType().Assembly.GetTypes().Where(t => t.GetInterface("IStockStrategy") != null && !t.IsInterface && !t.IsAbstract))
            {
               strategyList.Add(t.Name);
            }
            strategyList.AddRange(GetFilteredStrategyList(true));
         }

         strategyList.Sort();
         return strategyList;
      }

      public static IStockStrategy CreateStrategy(string name, StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         IStockStrategy strategy = null;
         try
         {
            if (name.StartsWith("@"))
               return CreateFilteredStrategy(name, stockSerie, lastBuyOrder, supportShortSelling);
            if (strategyList == null)
            {
               GetStrategyList();
            }
            if (strategyList.Contains(name))
            {
               StrategyManager sm = new StrategyManager();
               strategy =
                           (IStockStrategy)
                               sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockStrategyClasses." + name);
               strategy.Initialise(stockSerie, lastBuyOrder, supportShortSelling);
            }
         }
         catch (Exception ex)
         {
            throw new StockAnalyzerException("Failed to create strategy " + name, ex);
         }
         return strategy;
      }


      private static List<string> filteredStrategyList = null;

      public static List<string> GetFilteredStrategyList(bool forceReload)
      {
         if (forceReload) filteredStrategyList = null;
         if (filteredStrategyList == null)
         {
            filteredStrategyList = new List<string>();

            string folderName = Settings.Default.RootFolder + @"\FilteredStrategies";
            if (!System.IO.Directory.Exists(folderName))
            {
               System.IO.Directory.CreateDirectory(folderName);
               return filteredStrategyList;
            }
            foreach (string fileName in Directory.EnumerateFiles(folderName, "*.xml"))
            {
               filteredStrategyList.Add("@" + Path.GetFileNameWithoutExtension(fileName));
            }
         }
         filteredStrategyList.Sort();
         return filteredStrategyList;
      }

      public static StockFilteredStrategyBase CreateFilteredStrategy(string name)
      {
         StockFilteredStrategyBase strategy = null;
         if (filteredStrategyList == null)
         {
            GetFilteredStrategyList(false);
         }
         if (filteredStrategyList.Contains(name))
         {
            // Deserialize strategy file
            string fileName = Settings.Default.RootFolder + @"\FilteredStrategies\" + name.Replace("@", "") + ".xml";
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
               System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
               settings.IgnoreWhitespace = true;
               System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
               XmlSerializer serializer = new XmlSerializer(typeof(StockFilteredStrategyBase));
               strategy = (StockFilteredStrategyBase)serializer.Deserialize(xmlReader);
            }
         }
         return strategy;
      }
      public static StockFilteredStrategyBase CreateFilteredStrategy(string name, StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         StockFilteredStrategyBase strategy = null;
         if (filteredStrategyList == null)
         {
            GetFilteredStrategyList(false);
         }
         if (filteredStrategyList.Contains(name))
         {
            // Deserialize strategy file
            string fileName = Settings.Default.RootFolder + @"\FilteredStrategies\" + name.Replace("@", "") + ".xml";
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
               System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
               settings.IgnoreWhitespace = true;
               System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
               XmlSerializer serializer = new XmlSerializer(typeof(StockFilteredStrategyBase));
               strategy = (StockFilteredStrategyBase)serializer.Deserialize(xmlReader);
               strategy.Initialise(stockSerie, lastBuyOrder, supportShortSelling);
            }
         }
         return strategy;
      }
   }
}
