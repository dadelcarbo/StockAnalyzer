﻿using Newtonsoft.Json;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Saxo.OpenAPI.TradingServices
{
    public class InstrumentService : BaseService
    {
        static InstrumentService()
        {
            if (File.Exists(Folders.SaxoInstruments))
            {
                InstrumentCache = JsonConvert.DeserializeObject<List<Instrument>>(File.ReadAllText(Folders.SaxoInstruments));
            }
            else
            {
                InstrumentCache = new List<Instrument>();
            }
        }
        private static readonly List<Instrument> InstrumentCache;

        //static readonly string ASSET_TYPES = "Stock%2CMiniFuture%2CWarrantOpenEndKnockOut%2CEtf%2CCertificateConstantLeverage";

        static readonly string ASSET_TYPES = "MutualFund%2CCertificateUncappedCapitalProtection%2CCertificateCappedCapitalProtected%2CCertificateDiscount%2CCertificateCappedOutperformance%2CCertificateCappedBonus%2CCertificateExpress%2CCertificateTracker%2CCertificateUncappedOutperformance%2CCertificateBonus%2CCertificateConstantLeverage%2CStock%2CEtf%2CEtc%2CEtn%2CFund%2CRights%2CMiniFuture%2CWarrantKnockOut%2CWarrantOpenEndKnockOut%2CWarrantDoubleKnockOut%2CIpoOnStock%2CCompanyWarrant%2CStockIndex"; // %2CSrdOnStock%2CSrdOnEtf

        public Instrument GetInstrumentByIsin(string isin)
        {
            try
            {
                var instrument = InstrumentCache.FirstOrDefault(i => i.Isin == isin);
                if (instrument != null)
                {
                    return instrument;
                }
                else
                {
                    var method = $"ref/v1/instruments/?keywords={isin}&AssetTypes={ASSET_TYPES}&includeNonTradable=true";
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length == 0)
                    {
                        return null;
                    }
                    else if (instruments.Data.Length > 1)
                    {
                        instrument = instruments.Data.FirstOrDefault(i => i.ExchangeId.StartsWith("PAR"));
                        instrument ??= instruments.Data.FirstOrDefault(i => i.AssetType == "Stock");
                        instrument ??= instruments.Data[0];
                        InstrumentCache.Add(instrument);
                    }
                    else if (instruments.Data.Length == 1)
                    {
                        instrument = instruments.Data.First();

                        var cacheInstrument = InstrumentCache.FirstOrDefault(i => i.Identifier == instrument.Identifier);
                        if (cacheInstrument == null)
                        {
                            InstrumentCache.Add(instrument);
                        }
                        else
                        {
                            instrument = cacheInstrument;
                            instrument.Isin = isin;
                        }
                    }

                    File.WriteAllText(Folders.SaxoInstruments, JsonConvert.SerializeObject(InstrumentCache, Formatting.Indented));
                }
                return instrument;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public Instrument GetInstrumentById(long uic)
        {
            var method = $"ref/v1/instruments/?Uics={uic}&AssetTypes={ASSET_TYPES}";
            try
            {
                var instrument = InstrumentCache.FirstOrDefault(i => i.Identifier == uic);
                if (instrument != null)
                {
                    return instrument;
                }
                else
                {
                    var instruments = Get<Instruments>(method);
                    if (instruments.Data.Length > 0)
                    {
                        instrument = instruments.Data.First();
                    }
                    else
                    {
                        instrument = new Instrument
                        {
                            Identifier = uic,
                            Symbol = uic.ToString(),
                            Description = $"Not found: {uic}"
                        };
                    }
                    InstrumentCache.Add(instrument);
                    // Save Cache
                    File.WriteAllText(Folders.SaxoInstruments, JsonConvert.SerializeObject(InstrumentCache, Formatting.Indented));
                }
                return instrument;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        private static readonly SortedDictionary<long, InstrumentDetails> InstrumentDetailsCache = new SortedDictionary<long, InstrumentDetails>();
        public InstrumentDetails GetInstrumentDetailsById(long uic, string assetType, Account account)
        {
            try
            {
                if (!InstrumentDetailsCache.ContainsKey(uic))
                {
                    var method = $"ref/v1/instruments/details/{uic}/{assetType}/?AccountKey={account.AccountKey}";
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

        public PriceInfo GetInstrumentPrice(Instrument instrument, Account account)
        {
            try
            {
                var method = $"trade/v1/infoprices?AccountKey={account.AccountKey}&AssetType={instrument.AssetType}&Uic={instrument.Identifier}";
                return Get<PriceInfo>(method);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public async Task<PriceInfo> GetInstrumentPriceAsync(Instrument instrument, Account account)
        {
            try
            {
                var method = $"trade/v1/infoprices?AccountKey={account.AccountKey}&AssetType={instrument.AssetType}&Uic={instrument.Identifier}";
                return await GetAsync<PriceInfo>(method);
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

    public class PriceInfo
    {
        public Quote Quote { get; set; }
        public int Uic { get; set; }
        public string AssetType { get; set; }
        public object Commissions { get; set; }
        public DateTime LastUpdated { get; set; }
        public object DisplayAndFormat { get; set; }
        public object PriceInfoDetails { get; set; }
        public string PriceSource { get; set; }
    }

    public class Quote
    {
        public int Amount { get; set; }
        public float Bid { get; set; }
        public string PriceTypeBid { get; set; }
        public float Ask { get; set; }
        public string PriceTypeAsk { get; set; }
        public float Mid { get; set; }
        public int DelayedByMinutes { get; set; }
        public string ErrorCode { get; set; }
        public string PriceSource { get; set; }
        public string PriceSourceType { get; set; }
        public float BidSize { get; set; }
        public float AskSize { get; set; }
        public string MarketState { get; set; }
    }


}
