using Saxo.OpenAPI.AuthenticationServices;
using System;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class ClientService : BaseService
    {

        /// <summary>
        /// Get client info
        /// </summary>
        /// <param name="openApiBaseUrl"></param>
        /// <param name="accessToken"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public dynamic GetClient()
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), "port/v1/clients/me");
            try
            {
                return Get<dynamic>(url);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }
}
