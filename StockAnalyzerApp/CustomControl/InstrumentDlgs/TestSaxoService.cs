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

        public static string GetClientKey()
        {
            return accountService.GetAccounts()?.FirstOrDefault()?.ClientKey;
        }
        public static string GetAccountKey()
        {
            return accountService.GetAccounts()?.FirstOrDefault()?.AccountKey;
        }
    }
}
