using System.Collections.Generic;
using System;
using UltimateChartist.Helpers;

namespace UltimateChartist.Indicators;

public class IndicatorManager
{
    static private List<string> indicatorList = null;
    static public List<string> GetIndicatorList()
    {
        if (indicatorList == null)
        {
            indicatorList = new List<string>();
            var sm = new IndicatorManager();
            foreach (Type t in sm.GetType().Assembly.GetTypes())
            {
                Type st = t.GetInterface("IIndicator");
                if (st != null)
                {
                    if (!(t.Name.EndsWith("Base") || t.Name.Contains("StockTrail")))
                    {
                        indicatorList.Add(t.Name.Replace("Indicator_", ""));
                    }
                }
            }
        }
        indicatorList.Sort();
        return indicatorList;
    }
    //static public IIndicator CreateIndicator(string fullName)
    //{
    //    using (MethodLogger ml = new MethodLogger(typeof(IndicatorManager)))
    //    {
    //        IndicatorBase indicator = null;
    //        if (indicatorList == null)
    //        {
    //            GetIndicatorList();
    //        }

    //        try
    //        {
    //            int paramStartIndex = fullName.IndexOf('(') + 1;
    //            string name = fullName;
    //            int paramLength = 0;
    //            if (paramStartIndex != 0) // Else we are creating an empty indicator for the dianlog window
    //            {
    //                paramLength = fullName.LastIndexOf(')') - paramStartIndex;
    //                name = fullName.Substring(0, paramStartIndex - 1);
    //            }

    //            if (indicatorList.Contains(name))
    //            {
    //                IndicatorManager sm = new IndicatorManager();
    //                indicator = (IndicatorBase)sm.GetType().Assembly.CreateInstance("UltimateChartist.Indicators.StockIndicator_" + name);
    //                if (indicator != null)
    //                {
    //                    if (paramLength > 0)
    //                    {
    //                        string parameters = fullName.Substring(paramStartIndex, paramLength);
    //                        indicator.Initialise(parameters.Split(','));
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                throw new StockAnalyzerException("Indicator " + name + " doesn't not exist ! ");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            //if (e is StockAnalyzerException) throw e;
    //            indicator = null;
    //            StockLog.Write(e);
    //        }
    //        return indicator;
    //    }
    //}

}