using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicatorManager
    {
        static private List<string> indicatorList = null;
        static public List<string> GetIndicatorList()
        {
            if (indicatorList == null)
            {
                indicatorList = new List<string>();
                StockIndicatorManager sm = new StockIndicatorManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockIndicator");
                    if (st != null)
                    {
                        if (!(t.Name.EndsWith("Base") || t.Name.Contains("StockTrail")))
                        {
                            indicatorList.Add(t.Name.Replace("StockIndicator_", ""));
                        }
                    }
                }
            }
            indicatorList.Sort();
            return indicatorList;
        }
        static public List<string> GetIndicatorList(bool priceIndicator)
        {
            StockIndicatorBase indicator = null;
            if (indicatorList == null)
            {
                GetIndicatorList();
            }
            List<string> list = new List<string>();
            foreach (string indicatorName in indicatorList)
            {
                StockIndicatorManager sm = new StockIndicatorManager();
                indicator = (StockIndicatorBase)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockIndicators.StockIndicator_" + indicatorName);
                if (indicator != null && (priceIndicator && indicator.DisplayTarget == IndicatorDisplayTarget.PriceIndicator) || (!priceIndicator && indicator.DisplayTarget != IndicatorDisplayTarget.PriceIndicator))
                {
                    list.Add(indicatorName);
                }
            }
            return list;
        }
        static public bool Supports(string fullName)
        {
            if (indicatorList == null)
            {
                GetIndicatorList();
            }

            return indicatorList.Contains(fullName.Split('(')[0]);
        }

        static public IStockIndicator CreateIndicator(string fullName)
        {
            using MethodLogger ml = new MethodLogger(typeof(StockIndicatorManager));
            StockIndicatorBase indicator = null;
            if (indicatorList == null)
            {
                GetIndicatorList();
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

                if (indicatorList.Contains(name))
                {
                    StockIndicatorManager sm = new StockIndicatorManager();
                    indicator = (StockIndicatorBase)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockIndicators.StockIndicator_" + name);
                    if (indicator != null)
                    {
                        if (paramLength > 0)
                        {
                            string parameters = fullName.Substring(paramStartIndex, paramLength);
                            indicator.Initialise(parameters.Split(','));
                        }
                    }
                }
                else
                {
                    throw new StockAnalyzerException("Indicator " + name + " doesn't not exist ! ");
                }
            }
            catch (Exception e)
            {
                //if (e is StockAnalyzerException) throw e;
                indicator = null;
                StockLog.Write(e);
            }
            return indicator;
        }
    }
}