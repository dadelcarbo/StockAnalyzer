using System;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class ClientService : BaseService
    {
        /// <summary>
        /// Get connected client info
        /// </summary>
        public Client GetClient()
        {
            try
            {
                return Get<Client>("port/v1/clients/me");
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }


    public class Client
    {
        public string AllowedTradingSessions { get; set; }
        public string ClientId { get; set; }
        public string ClientKey { get; set; }
        public string ClientType { get; set; }
        public int CurrencyDecimals { get; set; }
        public string DefaultAccountId { get; set; }
        public string DefaultAccountKey { get; set; }
        public string DefaultCurrency { get; set; }
        public bool ForceOpenDefaultValue { get; set; }
        public bool IsMarginTradingAllowed { get; set; }
        public bool IsVariationMarginEligible { get; set; }
        public string[] LegalAssetTypes { get; set; }
        public bool LegalAssetTypesAreIndicative { get; set; }
        public string MarginCalculationMethod { get; set; }
        public string Name { get; set; }
        public string PositionNettingMethod { get; set; }
        public string PositionNettingMode { get; set; }
        public string PositionNettingProfile { get; set; }
        public bool ReduceExposureOnly { get; set; }
        public bool SupportsAccountValueProtectionLimit { get; set; }
    }

}
