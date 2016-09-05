using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockStrategyClasses.StockMoneyManagement
{
    public class MoneyManagementManager
    {
        private static List<string> moneyManagementList = null;

        private MoneyManagementManager()
        {
        }
        public static void ResetMoneyManagementList()
        {
            moneyManagementList = null;
        }
        public static List<string> GetMoneyManagementList()
        {
            if (moneyManagementList == null)
            {
                moneyManagementList = new List<string>();
                MoneyManagementManager sm = new MoneyManagementManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes().Where(t => t.GetInterface("IStockMoneyManagement") != null && !t.IsInterface && !t.IsAbstract))
                {
                    moneyManagementList.Add(t.Name);
                }
            }

            moneyManagementList.Sort();
            return moneyManagementList;
        }

        public static IStockMoneyManagement CreateMoneyManagement(string name, StockSerie stockSerie)
        {
            IStockMoneyManagement moneyManagement = null;
            try
            {
                if (moneyManagementList == null)
                {
                    GetMoneyManagementList();
                }
                if (moneyManagementList.Contains(name))
                {
                    MoneyManagementManager sm = new MoneyManagementManager();
                    moneyManagement =
                                (IStockMoneyManagement)
                                    sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockStrategyClasses.StockMoneyManagement." + name);
                    moneyManagement.Initialise(stockSerie);
                }
            }
            catch (Exception ex)
            {
                throw new StockAnalyzerException("Failed to create money management " + name, ex);
            }
            return moneyManagement;
        }
    }
}
