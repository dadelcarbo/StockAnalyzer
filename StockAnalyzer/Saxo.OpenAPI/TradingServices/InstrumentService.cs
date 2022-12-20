using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class InstrumentService : BaseService
    {
        private static SortedDictionary<string, Instrument> InstrumentIsinCache = new SortedDictionary<string, Instrument>();
        public Instrument GetInstrumentByIsin(string isin)
        {
            var method = $"ref/v1/instruments/?keywords={isin}&AssetTypes=Stock%2CMiniFuture%2CWarrantOpenEndKnockOut%2CEtf";
            try
            {
                if (!InstrumentIsinCache.ContainsKey(isin))
                {
                    Instrument instrument = null;
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length > 0)
                    {
                        instrument = instruments.Data.First();
                    }
                    InstrumentIsinCache.Add(isin, instrument);
                }
                return InstrumentIsinCache[isin];
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        private static SortedDictionary<long, Instrument> InstrumentCache = new SortedDictionary<long, Instrument>();
        public Instrument GetInstrumentById(long uic)
        {
            var method = $"ref/v1/instruments/?Uics={uic}&AssetTypes=Stock%2CMiniFuture%2CWarrantOpenEndKnockOut%2CEtf";
            try
            {
                if (!InstrumentCache.ContainsKey(uic))
                {
                    Instrument instrument = null;
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

        private static SortedDictionary<long, InstrumentDetails> InstrumentDetailsCache = new SortedDictionary<long, InstrumentDetails>();
        public InstrumentDetails GetInstrumentDetailsById(long uic, string assetType, Account account)
        {
            var method = $"ref/v1/instruments/details/{uic}/{assetType}/?AccountKey={account.AccountKey}";
            try
            {
                if (!InstrumentDetailsCache.ContainsKey(uic))
                {
                    InstrumentDetails instrumentDetail = Get<InstrumentDetails>(method);

                    InstrumentDetailsCache.Add(uic, instrumentDetail);
                }
                return InstrumentDetailsCache[uic];
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
        public decimal TickSize { get; set; }
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
