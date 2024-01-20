using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.Models;
using StockAnalyzer.StockLogging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace StockAnalyzer.Saxo.OpenAPI.TradingServices
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
        /// Send out PUT request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected T Put<T>(string method, object data)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    //Version = new Version(2, 0)  // Make sure HTTP/2 is used, once available
                    Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
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
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = new Version(1, 1)  // Make sure HTTP/2 is used, once available
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
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
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }
        protected string Get(string method)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = new Version(1, 1)  // Make sure HTTP/2 is used, once available
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
                // Disable Expect: 100 Continue according to https://www.developer.saxo/openapi/learn/openapi-request-response
                // In our experience the same two-step process has been difficult to get to work reliable, especially as we support clients world wide, 
                // who connect to us through a multitude of network gateways and proxies.We also find that the actual bandwidth savings for the majority of API requests are limited, 
                // since most requests are quite small.
                // We therefore strongly recommend against using the Expect:100 - Continue header, and expect you to make sure your client library does not rely on this mechanism.
                httpClient.DefaultRequestHeaders.ExpectContinue = false;

                var res = httpClient.SendAsync(request).Result;
                content = res.Content.ReadAsStringAsync().Result;
                res.EnsureSuccessStatusCode();
                return content;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }


        protected static readonly JsonSerializerSettings jsonSerializerSettingsSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };
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
                var stringData = JsonConvert.SerializeObject(data, jsonSerializerSettingsSettings);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(stringData, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                var res = httpClient.SendAsync(request).Result;
                content = res.Content.ReadAsStringAsync().Result;
                res.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }

        /// <summary>
        /// Send out POST request
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected T Patch<T>(string method, object data)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    //Version = new Version(2, 0)  // Make sure HTTP/2 is used, once available
                    Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
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
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }


        protected string Delete(string method)
        {
            string content = string.Empty;
            try
            {
                var url = new Uri(LoginService.App.OpenApiBaseUrl + method);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Version = new Version(1, 1)  // Make sure HTTP/2 is used, once available
                };
                request.Headers.Authorization = GetAuthorizationHeader(LoginService.Token);

                using var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false });
                // Disable Expect: 100 Continue according to https://www.developer.saxo/openapi/learn/openapi-request-response
                // In our experience the same two-step process has been difficult to get to work reliable, especially as we support clients world wide, 
                // who connect to us through a multitude of network gateways and proxies.We also find that the actual bandwidth savings for the majority of API requests are limited, 
                // since most requests are quite small.
                // We therefore strongly recommend against using the Expect:100 - Continue header, and expect you to make sure your client library does not rely on this mechanism.
                httpClient.DefaultRequestHeaders.ExpectContinue = false;

                var res = httpClient.SendAsync(request).Result;
                content = res.Content.ReadAsStringAsync().Result;
                res.EnsureSuccessStatusCode();
                return content;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("ErrorInfo"))
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<SaxoErrorInfo>(content)?.ErrorInfo };
                    }
                    else
                    {
                        throw new SaxoApiException() { ErrorInfo = JsonConvert.DeserializeObject<ErrorInfo>(content) };
                    }
                }
                StockLog.Write($"Exception: {ex.Message}\r\n${content}");
                throw new HttpRequestException(ex.Message + Environment.NewLine + content, ex);
            }
        }
    }
}
