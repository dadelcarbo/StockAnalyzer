using System;
using System.Collections.Generic;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
   public class StockTrailStopManager
   {
      static private List<string> trailStopList = null;
      static public List<string> GetTrailStopList()
      {
         if (trailStopList == null)
         {
            trailStopList = new List<string>();
            StockTrailStopManager sm = new StockTrailStopManager();
            foreach (Type t in sm.GetType().Assembly.GetTypes())
            {
               Type st = t.GetInterface("IStockTrailStop");
               if (st != null)
               {
                  if (t.Name != "StockTrailStopBase")
                  {
                     trailStopList.Add(t.Name.Replace("StockTrailStop_", ""));
                  }
               }
            }
         }
         trailStopList.Sort();
         return trailStopList;
      }

      static public bool Supports(string fullName)
      {
         if (trailStopList == null)
         {
            GetTrailStopList();
         }

         return trailStopList.Contains(fullName.Split('(')[0]);
      }
      static public IStockTrailStop CreateTrailStop(string fullName)
      {
         using (MethodLogger ml = new MethodLogger(typeof(StockTrailStopManager)))
         {
            IStockTrailStop trailStop = null;
            if (trailStopList == null)
            {
               GetTrailStopList();
            }
            try
            {
               int paramStartIndex = fullName.IndexOf('(') + 1;
               string name = fullName;
               int paramLength = 0;
               if (paramStartIndex != 0) // Else we are creating an empty indicator for the dialog window
               {
                  paramLength = fullName.LastIndexOf(')') - paramStartIndex;
                  name = fullName.Substring(0, paramStartIndex - 1);
               }

               if (trailStopList.Contains(name))
               {
                  StockTrailStopManager sm = new StockTrailStopManager();
                  trailStop = (IStockTrailStop)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops.StockTrailStop_" + name);
                  if (trailStop != null)
                  {
                     if (paramLength > 0)
                     {
                        string parameters = fullName.Substring(paramStartIndex, paramLength);
                        trailStop.Initialise(parameters.Split(','));
                     }
                  }
               }
               else
               {
                  throw new StockAnalyzerException("TrailStop " + name + " doesn't not exist ! ");
               }
            }
            catch (System.Exception e)
            {
               if (e is StockAnalyzerException) throw e;
               trailStop = null;
               StockLog.Write(e);
            }
            return trailStop;
         }
      }
   }
}
