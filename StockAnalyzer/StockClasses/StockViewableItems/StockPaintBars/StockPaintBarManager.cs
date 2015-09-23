using System;
using System.Collections.Generic;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBarManager
    {
        static private List<string> paintBarList = null;
        static public List<string> GetPaintBarList()
        {
            if (paintBarList == null)
            {
                paintBarList = new List<string>();
                StockPaintBarManager sm = new StockPaintBarManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockPaintBar");
                    if (st != null)
                    {
                        if (t.Name.StartsWith("StockPaintBar_"))
                        {
                            paintBarList.Add(t.Name.Replace("StockPaintBar_", ""));
                        }
                    }
                }
            }
            paintBarList.Sort();
            return paintBarList;
        }
        static public bool Supports(string fullName)
        {
           if (paintBarList == null)
           {
              GetPaintBarList();
           }

           return paintBarList.Contains(fullName.Split('(')[0]);
        }
        static public IStockPaintBar CreatePaintBar(string fullName)
        {
            using (MethodLogger ml = new MethodLogger(typeof(StockPaintBarManager)))
            {
                IStockPaintBar paintBar = null;
                if (paintBarList == null)
                {
                    GetPaintBarList();
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

                    if (paintBarList.Contains(name))
                    {
                        StockPaintBarManager sm = new StockPaintBarManager();
                        paintBar =
                            (IStockPaintBar)
                                sm.GetType()
                                    .Assembly.CreateInstance(
                                        "StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars.StockPaintBar_" +
                                        name);
                        if (paintBar != null)
                        {
                            if (paramLength > 0)
                            {
                                string parameters = fullName.Substring(paramStartIndex, paramLength);
                                paintBar.Initialise(parameters.Split(','));
                            }
                        }
                    }
                    else
                    {
                        throw new StockAnalyzerException("PaintBar " + name + " doesn't not exist ! ");
                    }
                }
                catch (System.Exception e)
                {
                    if (e is StockAnalyzerException) throw e;
                    paintBar = null;
                    StockLog.Write(e);
                }
                return paintBar;
            }
        }
    }
}