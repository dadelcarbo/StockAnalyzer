using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public class StockScreenerManager
    {
        static private List<string> screenerList = null;
        static public List<string> GetScreenerList()
        {
            if (screenerList == null)
            {
                screenerList = new List<string>();
                StockScreenerManager sm = new StockScreenerManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockScreener");
                    if (st != null)
                    {
                        if (t.Name.StartsWith("StockScreener_"))
                        {
                            screenerList.Add(t.Name.Replace("StockScreener_", ""));
                        }
                    }
                }
            }
            screenerList.Sort();
            return screenerList;
        }
        static public bool Supports(string fullName)
        {
            if (screenerList == null)
            {
                GetScreenerList();
            }

            return screenerList.Contains(fullName.Split('(')[0]);
        }
        static public IStockScreener CreateScreener(string fullName)
        {
            using MethodLogger ml = new MethodLogger(typeof(StockScreenerManager));
            IStockScreener screener = null;
            if (screenerList == null)
            {
                GetScreenerList();
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

                if (screenerList.Contains(name))
                {
                    StockScreenerManager sm = new StockScreenerManager();
                    screener =
                        (IStockScreener)
                            sm.GetType()
                                .Assembly.CreateInstance(
                                    "StockAnalyzer.StockClasses.StockViewableItems.StockScreeners.StockScreener_" +
                                    name);
                    if (screener != null)
                    {
                        if (paramLength > 0)
                        {
                            string parameters = fullName.Substring(paramStartIndex, paramLength);
                            screener.Initialise(parameters.Split(','));
                        }
                    }
                }
                else
                {
                    throw new StockAnalyzerException("Screener " + name + " doesn't not exist ! ");
                }
            }
            catch (Exception e)
            {
                if (e is StockAnalyzerException) throw e;
                screener = null;
                StockLog.Write(e);
            }
            return screener;
        }
    }
}