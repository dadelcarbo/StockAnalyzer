using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloudManager
    {
        static private List<string> cloudList = null;
        static public List<string> GetList()
        {
            if (cloudList == null)
            {
                cloudList = new List<string>();
                StockCloudManager sm = new StockCloudManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockCloud");
                    if (st != null)
                    {
                        if (!t.Name.EndsWith("CloudBase"))
                        {
                            cloudList.Add(t.Name.Replace("StockCloud_", ""));
                        }
                    }
                }
            }
            cloudList.Sort();
            return cloudList;
        }
        static public bool Supports(string fullName)
        {
            if (cloudList == null)
            {
                GetList();
            }

            return cloudList.Contains(fullName.Split('(')[0]);
        }

        static public IStockCloud CreateCloud(string fullName)
        {
            using (MethodLogger ml = new MethodLogger(typeof(StockCloudManager)))
            {
                StockCloudBase indicator = null;
                if (cloudList == null)
                {
                    GetList();
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

                    if (cloudList.Contains(name))
                    {
                        StockCloudManager sm = new StockCloudManager();
                        indicator = (StockCloudBase)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockClouds.StockCloud_" + name);
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
                        throw new StockAnalyzerException("Cloud " + name + " doesn't not exist ! ");
                    }
                }
                catch (Exception e)
                {
                    if (e is StockAnalyzerException) throw e;
                    indicator = null;
                    StockLog.Write(e);
                }
                return indicator;
            }
        }
    }
}