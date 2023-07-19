using Newtonsoft.Json;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Saxo.OpenAPI.TradingServices
{
    public class InstrumentService : BaseService
    {
        static InstrumentService()
        {
            if (File.Exists(Folders.SaxoInstruments))
            {
                InstrumentCache = JsonConvert.DeserializeObject<List<Instrument>>(File.ReadAllText(Folders.SaxoInstruments));
                foreach (var instrumenent in InstrumentCache.Where(i => !String.IsNullOrEmpty(i.Isin)))
                {
                    InstrumentIsinCache.Add(instrumenent.Isin, instrumenent);
                    InstrumentUicCache.Add(instrumenent.Identifier, instrumenent);
                }
            }
            else
            {
                InstrumentCache = new List<Instrument>();
            }
        }
        private static List<Instrument> InstrumentCache;

        static string ASSET_TYPES = "Stock%2CMiniFuture%2CWarrantOpenEndKnockOut%2CEtf%2CCertificateConstantLeverage";

        private static SortedDictionary<string, Instrument> InstrumentIsinCache = new SortedDictionary<string, Instrument>();
        public Instrument GetInstrumentByIsin(string isin)
        {
            var method = $"ref/v1/instruments/?keywords={isin}&AssetTypes={ASSET_TYPES}";
            try
            {
                if (!InstrumentIsinCache.ContainsKey(isin))
                {
                    Instrument instrument = null;
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length > 0)
                    {
                        instrument = instruments.Data.First();
                        instrument.Isin = isin;
                    }

                    InstrumentIsinCache.Add(isin, instrument);

                    if (instrument != null)
                    {
                        InstrumentCache.Add(instrument);
                        // Save Cache
                        File.WriteAllText(Folders.SaxoInstruments, JsonConvert.SerializeObject(InstrumentCache, Formatting.Indented));
                    }
                }
                return InstrumentIsinCache[isin];
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        private static SortedDictionary<long, Instrument> InstrumentUicCache = new SortedDictionary<long, Instrument>();
        public Instrument GetInstrumentById(long uic)
        {
            var method = $"ref/v1/instruments/?Uics={uic}&AssetTypes={ASSET_TYPES}";
            try
            {
                if (!InstrumentUicCache.ContainsKey(uic))
                {
                    Instrument instrument = null;
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length > 0)
                    {
                        instrument = instruments.Data.First();
                    }
                    InstrumentUicCache.Add(uic, instrument);
                }
                return InstrumentUicCache[uic];
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
        public string Isin { get; set; }
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
        public long Uic { get; set; }
        public decimal RoundToTickSize(float value)
        {
            var tickSize = this.GetTickSize(value);
            return decimal.Round((decimal)value / tickSize) * tickSize;
        }
        public decimal GetTickSize(float value)
        {
            var tickSize = this.TickSize;
            if (this.TickSizeScheme != null)
            {
                tickSize = this.TickSizeScheme.Elements[0].TickSize;
                int i = 1;
                while (i < this.TickSizeScheme.Elements.Length && value > this.TickSizeScheme.Elements[i - 1].HighPrice)
                {
                    tickSize = this.TickSizeScheme.Elements[i].TickSize;
                    i++;
                }
                if (i == 1 && this.TickSizeScheme.Elements.Length == 1)
                {
                    tickSize = this.TickSizeScheme.DefaultTickSize;
                }
            }
            return tickSize;
        }
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
        public decimal DefaultTickSize { get; set; }
        public TickSizeElement[] Elements { get; set; }
    }

    public class TickSizeElement
    {
        public float HighPrice { get; set; }
        public decimal TickSize { get; set; }
    }

}
