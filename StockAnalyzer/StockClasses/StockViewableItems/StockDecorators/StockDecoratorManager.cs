using System;
using System.Collections.Generic;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecoratorManager
    {
        static private List<string> decoratorList = null;
        static public List<string> GetDecoratorList()
        {
            if (decoratorList == null)
            {
                decoratorList = new List<string>();
                StockDecoratorManager sm = new StockDecoratorManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes())
                {
                    Type st = t.GetInterface("IStockDecorator");
                    if (st!=null)
                    {
                        if (t.Name != "StockDecoratorBase")
                        {
                            decoratorList.Add(t.Name.Replace("StockDecorator_",""));
                        }
                    }
                }
            }
            decoratorList.Sort();
            return decoratorList;
        }
        static public IStockDecorator CreateDecorator(string fullName, string decoratedItem)
        {
            using (MethodLogger ml = new MethodLogger(typeof(StockDecoratorManager)))
            {
                StockDecoratorBase decorator = null;
                if (decoratorList == null)
                {
                    GetDecoratorList();
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
                    if (decoratorList.Contains(name))
                    {
                        StockDecoratorManager sm = new StockDecoratorManager();
                        decorator = (StockDecoratorBase)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockClasses.StockViewableItems.StockDecorators.StockDecorator_" + name);
                        if (decorator != null)
                        {
                            decorator.DecoratedItem = decoratedItem;
                            if (paramLength > 0)
                            {
                                string parameters = fullName.Substring(paramStartIndex, paramLength);
                                decorator.Initialise(parameters.Split(','));
                            }
                        }
                    }
                    else
                    {
                        throw new StockAnalyzerException("Decorator " + name + " doesn't not exist ! ");
                    }
                }
                catch (System.Exception e)
                {
                    if (e is StockAnalyzerException) throw e;
                    decorator = null;
                    StockLog.Write(e);
                }
                return decorator;
            }
        }
    }
}
