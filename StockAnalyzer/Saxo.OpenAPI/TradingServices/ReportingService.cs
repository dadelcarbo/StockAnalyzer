using Newtonsoft.Json;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using System;

namespace Saxo.OpenAPI.TradingServices
{
    public class ReportingService : BaseService
    {
        public ClosedPositions GetClosedPositions(Account account)
        {
            try
            {
                var fromDate = DateTime.Today.AddDays(-5);
                var toDate = DateTime.Today;
                var url = $"cs/v1/reports/closedPositions/{account.ClientKey}/{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}?&AccountKey={account.AccountKey}";
                var content = Get(url);
                if (content == "[]")
                    return null;
                return JsonConvert.DeserializeObject<ClosedPositions>(content);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }


    public class ClosedPositions
    {
        public string __next { get; set; }
        public ClosedPosition[] Data { get; set; }
    }

    public class ClosedPosition
    {
        public string AccountCurrency { get; set; }
        public int AccountCurrencyDecimals { get; set; }
        public string AccountId { get; set; }
        public int Amount { get; set; }
        public int AmountClose { get; set; }
        public int AmountOpen { get; set; }
        public string AssetType { get; set; }
        public string ClosePositionId { get; set; }
        public float ClosePrice { get; set; }
        public string ExchangeDescription { get; set; }
        public string InstrumentDescription { get; set; }
        public string InstrumentSectorName { get; set; }
        public int InstrumentSectorTypeId { get; set; }
        public string InstrumentSymbol { get; set; }
        public string OpenPositionId { get; set; }
        public float OpenPrice { get; set; }
        public float PnLAccountCurrency { get; set; }
        public float PnLClientCurrency { get; set; }
        public float PnLUSD { get; set; }
        public string RootInstrumentSectorName { get; set; }
        public int RootInstrumentSectorTypeId { get; set; }
        public float TotalBookedOnClosingLegAccountCurrency { get; set; }
        public float TotalBookedOnClosingLegClientCurrency { get; set; }
        public float TotalBookedOnClosingLegUSD { get; set; }
        public float TotalBookedOnOpeningLegAccountCurrency { get; set; }
        public float TotalBookedOnOpeningLegClientCurrency { get; set; }
        public float TotalBookedOnOpeningLegUSD { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime TradeDateClose { get; set; }
        public DateTime TradeDateOpen { get; set; }
        public string UnderlyingInstrumentDescription { get; set; }
        public string UnderlyingInstrumentSymbol { get; set; }
    }


}

