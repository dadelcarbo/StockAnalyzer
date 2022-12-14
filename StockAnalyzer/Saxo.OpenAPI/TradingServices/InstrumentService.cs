using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class InstrumentService : BaseService
    {
        /// <summary>
        /// Get Instrument by keywords
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public Instrument[] GetInstruments(string keywords)
        {
            var method = $"ref/v1/instruments/?$top=201&$skip=0&includeNonTradable=true&AssetTypes=Stock%2CEtf%2CEtc%2CEtn%2CFund%2CRights%2CWarrant%2CMiniFuture%2CWarrantSpread%2CWarrantKnockOut%2CWarrantOpenEndKnockOut%2CWarrantDoubleKnockOut%2CCertificateUncappedCapitalProtection%2CCertificateCappedCapitalProtected%2CCertificateDiscount%2CCertificateCappedOutperformance%2CCertificateCappedBonus%2CCertificateExpress%2CCertificateTracker%2CCertificateUncappedOutperformance%2CCertificateBonus%2CCertificateConstantLeverage%2CSrdOnStock%2CSrdOnEtf%2CIpoOnStock%2CCompanyWarrant%2CStockIndex&keywords={keywords}&MarketDataProvider=Factset";
            try
            {
                return Get<Instruments>(method).Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public Instrument GetInstrumentByIsin(string isin)
        {
            var method = $"ref/v1/instruments/?keywords={isin}&AssetTypes=Stock%2CMiniFuture%2CWarrantOpenEndKnockOut";
            try
            {
                var instruments = Get<Instruments>(method);
                if (instruments.Data.Length > 0)
                    return instruments.Data.First();
                return null;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        private static SortedDictionary<long, Instrument> InstrumentCache = new SortedDictionary<long, Instrument>();
        public Instrument GetInstrumentById(long uic)
        {
            var method = $"ref/v1/instruments/?Uics={uic}&AssetTypes=Stock%2CMiniFuture%2CWarrantOpenEndKnockOut%2CFxSpot";
            try
            {
                if (!InstrumentCache.ContainsKey(uic))
                {
                    Instrument instrument = null;
                    //Task.Delay(2000).Wait();
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length > 0)
                    {
                        instrument = instruments.Data.First();
                    }
                    InstrumentCache.Add(uic, instrument);
                }
                return InstrumentCache[uic];
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public InstrumentDetails GetInstrumentDetailsById(long uic, string assetType, Account account)
        {
            var method = $"ref/v1/instruments/details/{uic}/{assetType}/?AccountKey={account.AccountKey}";
            try
            {
                return Get<InstrumentDetails>(method);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }

    public class Instruments
    {
        public Instrument[] Data { get; set; }
    }

    public class Instrument
    {
        public string AssetType { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
        public string ExchangeId { get; set; }
        public long GroupId { get; set; }
        public long Identifier { get; set; }
        public string SummaryType { get; set; }
        public string Symbol { get; set; }
        public string[] TradableAs { get; set; }
    }

    public class InstrumentDetails
    {
        public bool AffiliateInfoRequired { get; set; }
        public int AmountDecimals { get; set; }
        public string AssetType { get; set; }
        public string CurrencyCode { get; set; }
        public float DefaultAmount { get; set; }
        public float DefaultSlippage { get; set; }
        public string DefaultSlippageType { get; set; }
        public string Description { get; set; }
        public Exchange Exchange { get; set; }
        public Format Format { get; set; }
        public int GroupId { get; set; }
        public float IncrementSize { get; set; }
        public Ipodetails IpoDetails { get; set; }
        public bool IsBarrierEqualsStrike { get; set; }
        public bool IsComplex { get; set; }
        public bool IsExtendedTradingHoursEnabled { get; set; }
        public bool IsPEAEligible { get; set; }
        public bool IsPEASMEEligible { get; set; }
        public bool IsRedemptionByAmounts { get; set; }
        public bool IsSwitchBySameCurrency { get; set; }
        public bool IsTradable { get; set; }
        public string LotSizeType { get; set; }
        public float MinimumLotSize { get; set; }
        public float MinimumTradeSize { get; set; }
        public string NonTradableReason { get; set; }
        public Orderdistances OrderDistances { get; set; }
        public string PriceCurrency { get; set; }
        public float PriceToContractFactor { get; set; }
        public int PrimaryListing { get; set; }
        public object[] RelatedInstruments { get; set; }
        public float[] StandardAmounts { get; set; }
        public string[] SupportedOrderTriggerPriceTypes { get; set; }
        public string[] SupportedOrderTypes { get; set; }
        public string[] SupportedStrategies { get; set; }
        public string Symbol { get; set; }
        public Ticksizescheme TickSizeScheme { get; set; }
        public string[] TradableAs { get; set; }
        public string[] TradableOn { get; set; }
        public string TradingSignals { get; set; }
        public string TradingStatus { get; set; }
        public int Uic { get; set; }
    }

    public class Format
    {
        public int Decimals { get; set; }
        public int OrderDecimals { get; set; }
    }

    public class Ipodetails
    {
        public int MaxLeveragePct { get; set; }
        public int MaxLotSize { get; set; }
    }

    public class Orderdistances
    {
        public float EntryDefaultDistance { get; set; }
        public string EntryDefaultDistanceType { get; set; }
        public float LimitDefaultDistance { get; set; }
        public string LimitDefaultDistanceType { get; set; }
        public float StopLimitDefaultDistance { get; set; }
        public string StopLimitDefaultDistanceType { get; set; }
        public float StopLossDefaultDistance { get; set; }
        public string StopLossDefaultDistanceType { get; set; }
        public bool StopLossDefaultEnabled { get; set; }
        public string StopLossDefaultOrderType { get; set; }
        public float TakeProfitDefaultDistance { get; set; }
        public string TakeProfitDefaultDistanceType { get; set; }
        public bool TakeProfitDefaultEnabled { get; set; }
    }

    public class Ticksizescheme
    {
        public float DefaultTickSize { get; set; }
        public TickSizeElement[] Elements { get; set; }
    }

    public class TickSizeElement
    {
        public float HighPrice { get; set; }
        public decimal TickSize { get; set; }
    }

}
