using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class TestSaxoService : BaseService
    {
        private TestSaxoService()
        {

        }

        static AccountService accountService = new AccountService();
        static TestSaxoService instance = new TestSaxoService();
        static public string HttpGet(string method)
        {
            return instance.Get(method);
        }

        public static string GetClientKey(string saxoAccountId)
        {
            try
            {
                return accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == saxoAccountId)?.ClientKey;
            }
            catch
            {
                return null;
            }
        }
        public static string GetAccountKey(string saxoAccountId)
        {
            try
            {
                return accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == saxoAccountId)?.AccountKey;
            }
            catch
            {
                return null;
            }
        }
    }
}
