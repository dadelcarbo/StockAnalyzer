using System;
using System.Collections.Generic;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
   public class StockTrailManager
   {
      static private List<string> trailList = null;
      static public List<string> GetTrailList()
      {
         if (trailList == null)
         {
            trailList = new List<string>();
            StockTrailManager sm = new StockTrailManager();
            foreach (Type t in sm.GetType().Assembly.GetTypes())
            {
               Type st = t.GetInterface("IStockTrail");
               if (st != null)
               {
                  if (t.Name != "StockTrailBase")
                  {
                     trailList.Add(t.Name.Replace("StockTrail_", ""));
                  }
               }
            }
         }
         trailList.Sort();
         return trailList;
      }
      static public IStockTrail CreateTrail(string fullName, string trailedItem)
      {
         using (MethodLogger ml = new MethodLogger(typeof(StockTrailManager)))
         {
            StockTrailBase trail = null;
            if (trailList == null)
            {
               GetTrailList();
            }

            try
            {
               int paramStartIndex = fullName.IndexOf('(') + 1;
               string name = fullName;
               int paramLength = 0;
               if (paramStartIndex != 0) // Else we are creating an empty indicator for the dianlog window
               {
                  paramLength = fullName.LastIndexOf(')') - paramStartIndex;
                  name = fullName.Substring(0, paramStartIndex - 1);
               }
               if (trailList.Contains(name))
               {
                  StockTrailManager sm = new StockTrailManager();
                  trail = (StockTrailBase)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockTrails.StockTrail_" + name);
                  if (trail != null)
                  {
                     trail.TrailedItem = trailedItem;
                     if (paramLength > 0)
                     {
                        string parameters = fullName.Substring(paramStartIndex, paramLength);
                        trail.Initialise(parameters.Split(','));
                     }
                  }
               }
               else
               {
                  throw new StockAnalyzerException("Trail " + name + " doesn't not exist ! ");
               }
            }
            catch (System.Exception e)
            {
               if (e is StockAnalyzerException) throw e;
               trail = null;
               StockLog.Write(e);
            }
            return trail;
         }
      }

      public static bool Supports(string fullName)
      {
         if (trailList == null)
         {
            GetTrailList();
         }

         return trailList.Contains(fullName.Split('(')[0]);
      }
   }
}
