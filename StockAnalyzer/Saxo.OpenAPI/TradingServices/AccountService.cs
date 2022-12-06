using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using static Saxo.OpenAPI.TradingServices.AccountService;

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
            try
            {
                return Get<Accounts>("port/v1/accounts/me").Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public Position[] GetPositions()
        {
            try
            {
                return Get<Positions>("port/v1/positions/me").Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public Position[] GetPositions(Account account)
        {
            try
            {
                return Get<Positions>($"port/v1/positions/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}").Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public Position GetPositionById(Account account, long positionId)
        {
            try
            {
                return Get<Position>($"port/v1/positions/{positionId}/?ClientKey={account.ClientKey}");
            }
            catch (Exception)
            {
                return null;
            }
        }
        public ClosedPositions GetClosedPositions(Account account, DateTime fromDate)
        {
            try
            {
                if (fromDate.Year == 1)
                    return null;
                return Get<ClosedPositions>($"cs/v1/reports/closedPositions/{account.ClientKey}/{fromDate.ToString("yyyy-MM-dd")}/{DateTime.Today.ToString("yyyy-MM-dd")}/?&AccountGroupKey={account.AccountGroupKey}&AccountKey={account.AccountKey}");
            }
            catch (Exception)
            {
                return null;
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


    public class ClosedPositions
    {
        public int __count { get; set; }
        public Datum[] Data { get; set; }
    }

    public class Datum
    {
        public string AccountCurrency { get; set; }
        public int AccountCurrencyDecimals { get; set; }
        public string AccountId { get; set; }
        public float Amount { get; set; }
        public float AmountClose { get; set; }
        public float AmountOpen { get; set; }
        public string AssetType { get; set; }
        public string ClientCurrency { get; set; }
        public string ClosePositionId { get; set; }
        public float ClosePrice { get; set; }
        public string CloseType { get; set; }
        public string ExchangeDescription { get; set; }
        public string InstrumentCurrency { get; set; }
        public string InstrumentDescription { get; set; }
        public string InstrumentSymbol { get; set; }
        public string OpenPositionId { get; set; }
        public float OpenPrice { get; set; }
        public float PnLAccountCurrency { get; set; }
        public float PnLClientCurrency { get; set; }
        public float PnLUSD { get; set; }
        public float TotalBookedOnClosingLegAccountCurrency { get; set; }
        public float TotalBookedOnClosingLegClientCurrency { get; set; }
        public float TotalBookedOnClosingLegUSD { get; set; }
        public float TotalBookedOnOpeningLegAccountCurrency { get; set; }
        public float TotalBookedOnOpeningLegClientCurrency { get; set; }
        public float TotalBookedOnOpeningLegUSD { get; set; }
        public string TradeDate { get; set; }
        public string TradeDateClose { get; set; }
        public string TradeDateOpen { get; set; }
        public string UnderlyingInstrumentDescription { get; set; }
        public string UnderlyingInstrumentSymbol { get; set; }
    }


}

