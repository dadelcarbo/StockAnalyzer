using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawingManager
    {
        static private List<string> autoDrawingList = null;
        static public List<string> GetAutoDrawingList()
        {
            if (autoDrawingList == null)
            {
                autoDrawingList = new List<string>();
                StockAutoDrawingManager sm = new StockAutoDrawingManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockAutoDrawing");
                    if (st != null)
                    {
                        if (t.Name != "StockAutoDrawingBase")
                        {
                            autoDrawingList.Add(t.Name.Replace("StockAutoDrawing_", ""));
                        }
                    }
                }
            }
            autoDrawingList.Sort();
            return autoDrawingList;
        }

        static public bool Supports(string fullName)
        {
            if (autoDrawingList == null)
            {
                GetAutoDrawingList();
            }

            return autoDrawingList.Contains(fullName.Split('(')[0]);
        }
        static public IStockAutoDrawing CreateAutoDrawing(string fullName)
        {
            using MethodLogger ml = new MethodLogger(typeof(StockAutoDrawingManager));
            IStockAutoDrawing autoDraw = null;
            if (autoDrawingList == null)
            {
                GetAutoDrawingList();
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

                if (autoDrawingList.Contains(name))
                {
                    StockAutoDrawingManager sm = new StockAutoDrawingManager();
                    autoDraw = (IStockAutoDrawing)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings.StockAutoDrawing_" + name);
                    if (autoDraw != null)
                    {
                        if (paramLength > 0)
                        {
                            string parameters = fullName.Substring(paramStartIndex, paramLength);
                            autoDraw.Initialise(parameters.Split(','));
                        }
                    }
                }
                else
                {
                    throw new StockAnalyzerException("AutoDrawing " + name + " doesn't not exist ! ");
                }
            }
            catch (Exception e)
            {
                if (e is StockAnalyzerException) throw e;
                autoDraw = null;
                StockLog.Write(e);
            }
            return autoDraw;
        }
    }
}
