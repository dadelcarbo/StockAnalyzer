using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.Models;
using System;
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
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), method);
            try
            {
                return Get<Instruments>(url).Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public Instrument GetInstrumentById(string isin)
        {
            var method = $"ref/v1/instruments/?$top=201&$skip=0&includeNonTradable=true&AssetTypes=Stock%2CEtf%2CEtc%2CEtn%2CFund%2CRights%2CWarrant%2CMiniFuture%2CWarrantSpread%2CWarrantKnockOut%2CWarrantOpenEndKnockOut%2CWarrantDoubleKnockOut%2CCertificateUncappedCapitalProtection%2CCertificateCappedCapitalProtected%2CCertificateDiscount%2CCertificateCappedOutperformance%2CCertificateCappedBonus%2CCertificateExpress%2CCertificateTracker%2CCertificateUncappedOutperformance%2CCertificateBonus%2CCertificateConstantLeverage%2CSrdOnStock%2CSrdOnEtf%2CIpoOnStock%2CCompanyWarrant%2CStockIndex&keywords={isin}&MarketDataProvider=Factset";


            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), method);
            try
            {
                var instruments = Get<Instruments>(url);
                if (instruments.Data.Length == 1)
                    return instruments.Data[0];
                return null;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public Instrument GetInstrumentById(long uic)
        {
            var method = $"ref/v1/instruments/?$top=201&$skip=0&includeNonTradable=true&AssetTypes=Stock%2CEtf%2CEtc%2CEtn%2CFund%2CRights%2CWarrant%2CMiniFuture%2CWarrantSpread%2CWarrantKnockOut%2CWarrantOpenEndKnockOut%2CWarrantDoubleKnockOut%2CCertificateUncappedCapitalProtection%2CCertificateCappedCapitalProtected%2CCertificateDiscount%2CCertificateCappedOutperformance%2CCertificateCappedBonus%2CCertificateExpress%2CCertificateTracker%2CCertificateUncappedOutperformance%2CCertificateBonus%2CCertificateConstantLeverage%2CSrdOnStock%2CSrdOnEtf%2CIpoOnStock%2CCompanyWarrant%2CStockIndex&Uics={uic}&MarketDataProvider=Factset";


            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), method);
            try
            {
                var instruments = Get<Instruments>(url);
                if (instruments.Data.Length == 1)
                    return instruments.Data[0];
                return null;
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
}
