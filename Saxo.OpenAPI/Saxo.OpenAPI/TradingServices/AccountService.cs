using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class AccountService : BaseService
    {
        /// <summary>
        /// Get client info
        /// </summary>
        /// <param name="openApiBaseUrl"></param>
        /// <param name="accessToken"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public Account[] GetAccounts()
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), "port/v1/accounts/me");
            try
            {
                return Get<Accounts>(url).Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public Position[] GetPositions()
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), $"port/v1/positions/me");
            try
            {
                return Get<Positions>(url).Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public Position[] GetPositions(Account account)
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), $"port/v1/positions/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}");
            try
            {
                return Get<Positions>(url).Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }


    public class Accounts
    {
        public Account[] Data { get; set; }
    }

    public class Account
    {
        public string AccountGroupKey { get; set; }
        public string AccountId { get; set; }
        public string AccountKey { get; set; }
        public string AccountSubType { get; set; }
        public string AccountType { get; set; }
        public bool Active { get; set; }
        public bool CanUseCashPositionsAsMarginCollateral { get; set; }
        public bool CfdBorrowingCostsActive { get; set; }
        public string ClientId { get; set; }
        public string ClientKey { get; set; }
        public DateTime CreationDate { get; set; }
        public string Currency { get; set; }
        public int CurrencyDecimals { get; set; }
        public bool DirectMarketAccess { get; set; }
        public bool FractionalOrderEnabled { get; set; }
        public object[] FractionalOrderEnabledAssetTypes { get; set; }
        public bool IndividualMargining { get; set; }
        public bool IsCurrencyConversionAtSettlementTime { get; set; }
        public bool IsMarginTradingAllowed { get; set; }
        public bool IsShareable { get; set; }
        public bool IsTrialAccount { get; set; }
        public string[] LegalAssetTypes { get; set; }
        public string ManagementType { get; set; }
        public string MarginCalculationMethod { get; set; }
        public string MarginLendingEnabled { get; set; }
        public bool PortfolioBasedMarginEnabled { get; set; }
        public string[] Sharing { get; set; }
        public bool SupportsAccountValueProtectionLimit { get; set; }
        public bool UseCashPositionsAsMarginCollateral { get; set; }
    }


    public class Positions
    {
        public int __count { get; set; }
        public Position[] Data { get; set; }
    }

    public class Position
    {
        public Displayandformat DisplayAndFormat { get; set; }
        public string NetPositionId { get; set; }
        public Positionbase PositionBase { get; set; }
        public string PositionId { get; set; }
        public Positionview PositionView { get; set; }
    }

    public class Displayandformat
    {
        public string Currency { get; set; }
        public int Decimals { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public string Symbol { get; set; }
    }

    public class Positionbase
    {
        public string AccountId { get; set; }
        public string AccountKey { get; set; }
        public float Amount { get; set; }
        public string AssetType { get; set; }
        public bool CanBeClosed { get; set; }
        public string ClientId { get; set; }
        public bool CloseConversionRateSettled { get; set; }
        public string CorrelationKey { get; set; }
        public DateTime ExecutionTimeOpen { get; set; }
        public bool IsForceOpen { get; set; }
        public bool IsMarketOpen { get; set; }
        public bool LockedByBackOffice { get; set; }
        public float OpenPrice { get; set; }
        public float OpenPriceIncludingCosts { get; set; }
        public object[] RelatedOpenOrders { get; set; }
        public string SourceOrderId { get; set; }
        public string SpotDate { get; set; }
        public string Status { get; set; }
        public long Uic { get; set; }
        public DateTime ValueDate { get; set; }
    }

    public class Positionview
    {
        public float Ask { get; set; }
        public float Bid { get; set; }
        public string CalculationReliability { get; set; }
        public float ConversionRateCurrent { get; set; }
        public float ConversionRateOpen { get; set; }
        public float CurrentPrice { get; set; }
        public int CurrentPriceDelayMinutes { get; set; }
        public string CurrentPriceType { get; set; }
        public float Exposure { get; set; }
        public string ExposureCurrency { get; set; }
        public float ExposureInBaseCurrency { get; set; }
        public float InstrumentPriceDayPercentChange { get; set; }
        public string MarketState { get; set; }
        public float MarketValue { get; set; }
        public float MarketValueInBaseCurrency { get; set; }
        public float ProfitLossOnTrade { get; set; }
        public float ProfitLossOnTradeInBaseCurrency { get; set; }
        public float TradeCostsTotal { get; set; }
        public float TradeCostsTotalInBaseCurrency { get; set; }
    }


}
