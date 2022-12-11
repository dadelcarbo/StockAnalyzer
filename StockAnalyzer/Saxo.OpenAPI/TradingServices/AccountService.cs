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

        public Balance GetBalance(Account account)
        {
            try
            {
                return Get<Balance>($"port/v1/balances/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}&FieldGroups=CalculateCashForTrading");
            }
            catch (Exception ex)
            {
                return null;
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
        public HistoricalClosedPositions GetHistoricalClosedPositions(Account account, DateTime fromDate)
        {
            try
            {
                if (fromDate.Year == 1)
                    return null;
                return Get<HistoricalClosedPositions>($"cs/v1/reports/closedPositions/{account.ClientKey}/{fromDate.ToString("yyyy-MM-dd")}/{DateTime.Today.ToString("yyyy-MM-dd")}/?&AccountGroupKey={account.AccountGroupKey}&AccountKey={account.AccountKey}");
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DailyClosedPositions GetDailyClosedPositions(Account account)
        {
            try
            {
                return Get<DailyClosedPositions>($"port/v1/closedpositions/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}");
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

    public class Balance
    {
        public string CalculationReliability { get; set; }
        public float CashAvailableForTrading { get; set; }
        public float CashBalance { get; set; }
        public float CashBlocked { get; set; }
        public bool ChangesScheduled { get; set; }
        public int ClosedPositionsCount { get; set; }
        public float CollateralAvailable { get; set; }
        public float CorporateActionUnrealizedAmounts { get; set; }
        public float CostToClosePositions { get; set; }
        public string Currency { get; set; }
        public int CurrencyDecimals { get; set; }
        public Initialmargin InitialMargin { get; set; }
        public float IntradayMarginDiscount { get; set; }
        public bool IsPortfolioMarginModelSimple { get; set; }
        public float MarginAndCollateralUtilizationPct { get; set; }
        public float MarginAvailableForTrading { get; set; }
        public float MarginCollateralNotAvailable { get; set; }
        public float MarginExposureCoveragePct { get; set; }
        public float MarginNetExposure { get; set; }
        public float MarginUsedByCurrentPositions { get; set; }
        public float MarginUtilizationPct { get; set; }
        public float NetEquityForMargin { get; set; }
        public int NetPositionsCount { get; set; }
        public float NonMarginPositionsValue { get; set; }
        public int OpenIpoOrdersCount { get; set; }
        public int OpenPositionsCount { get; set; }
        public float OptionPremiumsMarketValue { get; set; }
        public int OrdersCount { get; set; }
        public float OtherCollateral { get; set; }
        public float SettlementValue { get; set; }
        public float ShareSpendingPower { get; set; }
        public float SpendingPower { get; set; }
        public Spendingpowerdetail SpendingPowerDetail { get; set; }
        public float SrdSpendingPower { get; set; }
        public float TotalValue { get; set; }
        public float TransactionsNotBooked { get; set; }
        public int TriggerOrdersCount { get; set; }
        public float UnrealizedMarginClosedProfitLoss { get; set; }
        public float UnrealizedMarginOpenProfitLoss { get; set; }
        public float UnrealizedMarginProfitLoss { get; set; }
        public float UnrealizedPositionsValue { get; set; }
    }

    public class Initialmargin
    {
        public float CollateralAvailable { get; set; }
        public float MarginAvailable { get; set; }
        public float MarginCollateralNotAvailable { get; set; }
        public float MarginUsedByCurrentPositions { get; set; }
        public float MarginUtilizationPct { get; set; }
        public float NetEquityForMargin { get; set; }
        public float OtherCollateralDeduction { get; set; }
    }

    public class Spendingpowerdetail
    {
        public float Current { get; set; }
        public float Maximum { get; set; }
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


    public class HistoricalClosedPositions
    {
        public int __count { get; set; }
        public ClosedPositions[] Data { get; set; }
    }

    public class ClosedPositions
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
        public DateTime TradeDate { get; set; }
        public DateTime TradeDateClose { get; set; }
        public DateTime TradeDateOpen { get; set; }
        public string UnderlyingInstrumentDescription { get; set; }
        public string UnderlyingInstrumentSymbol { get; set; }
    }


    public class DailyClosedPositions
    {
        public int __count { get; set; }
        public DailyClosedPosition[] Data { get; set; }
    }

    public class DailyClosedPosition
    {
        public Closedposition ClosedPosition { get; set; }
        public string ClosedPositionUniqueId { get; set; }
        public string NetPositionId { get; set; }
    }

    public class Closedposition
    {
        public string AccountId { get; set; }
        public float Amount { get; set; }
        public string AssetType { get; set; }
        public string BuyOrSell { get; set; }
        public string ClientId { get; set; }
        public float ClosedProfitLoss { get; set; }
        public float ClosedProfitLossInBaseCurrency { get; set; }
        public float ClosingMarketValue { get; set; }
        public float ClosingMarketValueInBaseCurrency { get; set; }
        public string ClosingMethod { get; set; }
        public string ClosingPositionId { get; set; }
        public float ClosingPrice { get; set; }
        public bool ConversionRateInstrumentToBaseSettledClosing { get; set; }
        public bool ConversionRateInstrumentToBaseSettledOpening { get; set; }
        public DateTime ExecutionTimeClose { get; set; }
        public DateTime ExecutionTimeOpen { get; set; }
        public string OpeningPositionId { get; set; }
        public float OpenPrice { get; set; }
        public float ProfitLossCurrencyConversion { get; set; }
        public float ProfitLossOnTrade { get; set; }
        public float ProfitLossOnTradeInBaseCurrency { get; set; }
        public int Uic { get; set; }
    }

}

