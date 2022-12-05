using Newtonsoft.Json;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Saxo.OpenAPI.TradingServices
{
    public abstract class BaseService
    {
        /// <summary>
        /// Get Authorization header
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected AuthenticationHeaderValue GetAuthorizationHeader(Token token)
        {
            return new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
        }

        /// <summary>
        /// Send out GET request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected T Get<T>(string method)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                Console.WriteLine($"Get<{typeof(T).Name}>(${url})");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = new Version(1, 1)  // Make sure HTTP/2 is used, once available
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using (var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false }))
                {
                    // Disable Expect: 100 Continue according to https://www.developer.saxo/openapi/learn/openapi-request-response
                    // In our experience the same two-step process has been difficult to get to work reliable, especially as we support clients world wide, 
                    // who connect to us through a multitude of network gateways and proxies.We also find that the actual bandwidth savings for the majority of API requests are limited, 
                    // since most requests are quite small.
                    // We therefore strongly recommend against using the Expect:100 - Continue header, and expect you to make sure your client library does not rely on this mechanism.
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;

                    var res = httpClient.SendAsync(request).Result;
                    content = res.Content.ReadAsStringAsync().Result;
                    res.EnsureSuccessStatusCode();

                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }
        /// <summary>
        /// Send out POST request
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected T Post<T>(string method, object data)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                Console.WriteLine($"Get<{typeof(T).Name}>(${url})");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    //Version = new Version(2, 0)  // Make sure HTTP/2 is used, once available
                    Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using (var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false }))
                {
                    // Disable Expect: 100 Continue according to https://www.developer.saxo/openapi/learn/openapi-request-response
                    // In our experience the same two-step process has been difficult to get to work reliable, especially as we support clients world wide, 
                    // who connect to us through a multitude of network gateways and proxies.We also find that the actual bandwidth savings for the majority of API requests are limited, 
                    // since most requests are quite small.
                    // We therefore strongly recommend against using the Expect:100 - Continue header, and expect you to make sure your client library does not rely on this mechanism.
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;
                    var res = httpClient.SendAsync(request).Result;
                    content = res.Content.ReadAsStringAsync().Result;
                    res.EnsureSuccessStatusCode();

                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }
    }
}
