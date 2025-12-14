using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider
{
    public class AbcClientException : Exception
    {
        public AbcClientException(string msg) : base($"Too many requests sent to ABC Bourse: {Environment.NewLine}{msg}")
        {
        }
    }
    public static class AbcClient
    {
        static HttpClient httpClient { get; set; }
        static Dictionary<string, string> secrets { get; set; }
        static Dictionary<string, string> cookies { get; set; }

        static bool asyncResult = false;

        public static string CacheFolder { get; set; }

        public static bool DownloadLabel(string fileName, string market)
        {
            StockLog.Write($"market: {market}");
            asyncResult = false;
            Task.Run(async () =>
            {
                var data = await DownloadLabelAsync(market);
                if (string.IsNullOrEmpty(data) || data.StartsWith(" <!DOCTYPE"))
                    return;
                File.WriteAllText(fileName, data);
                asyncResult = true;
            }).Wait();

            return asyncResult;
        }
        public static async Task<string> DownloadLabelAsync(string market)
        {
            if (!await InitClientAsync())
                return null;

            try
            {
                using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.abcbourse.com/download/libelles");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                request.Headers.TryAddWithoutValidation("origin", "https://www.abcbourse.com");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("referer", "https://www.abcbourse.com/download/libelles");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");

                var cookieString = cookies.Select(c => $"{c.Key}={c.Value}").Aggregate((i, j) => $"{i};{j}");
                request.Headers.TryAddWithoutValidation("Cookie", cookieString);

                var requestVerificationToken = secrets.ContainsKey("__RequestVerificationToken") ? secrets["__RequestVerificationToken"] : "";

                request.Content = new StringContent($"cbox={market}&cbPlace=true&__RequestVerificationToken={requestVerificationToken}&cbPlace=false");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == (HttpStatusCode)429)
                    throw new AbcClientException($"DownloadLabel: {market}");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
            catch (AbcClientException ex)
            {
                StockLog.Write(ex);
                throw;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
                return null;
            }
        }

        public static bool DownloadData(string fileName, DateTime dateFrom, DateTime dateTo, string market, bool useCache)
        {
            StockLog.Write($"Market: {market} From:{dateFrom:dd/MM/yy} To:{dateTo:dd/MM/yy}");
            asyncResult = false;
            Task.Run(async () =>
            {
                var data = await DownloadDataAsync(dateFrom, dateTo, market, useCache);
                if (string.IsNullOrEmpty(data) || data.StartsWith(" <!DOCTYPE"))
                    return;
                File.WriteAllText(fileName, data);
                asyncResult = true;
            }).Wait();

            return asyncResult;
        }
        public static async Task<string> DownloadDataAsync(DateTime dateFrom, DateTime dateTo, string market, bool useCache)
        {
            if (useCache && !string.IsNullOrEmpty(CacheFolder))
            {
                string fileName = Path.Combine(CacheFolder, market + "_" + dateFrom.Year + "_" + dateFrom.Month.ToString("0#") + ".csv");
                if (File.Exists(fileName))
                    return File.ReadAllText(fileName);
            }

            if (!await InitClientAsync())
                return null;

            if ((DateTime.Today - dateFrom).TotalDays > 100)
            {
                var delay = new Random().Next(1, 5) * 1000;
                await Task.Delay(delay);
            }

            try
            {
                using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.abcbourse.com/download/historiques");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7");
                request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                request.Headers.TryAddWithoutValidation("origin", "https://www.abcbourse.com");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("referer", "https://www.abcbourse.com/download/historiques");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");

                var cookieString = cookies.Select(c => $"{c.Key}={c.Value}").Aggregate((i, j) => $"{i};{j}");
                request.Headers.TryAddWithoutValidation("Cookie", cookieString);

                var requestVerificationToken = secrets.ContainsKey("__RequestVerificationToken") ? secrets["__RequestVerificationToken"] : "";

                var data = $"dateFrom={dateFrom.ToString("yyyy-MM-dd")}&__Invariant=dateFrom&dateTo={dateTo.ToString("yyyy-MM-dd")}&__Invariant=dateTo&txtOneSico=&cbox={market}&sFormat=ab&typeData=isin&cbYes=true&__RequestVerificationToken={requestVerificationToken}&cbYes=false";
                request.Content = new StringContent(data);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == (HttpStatusCode)429)
                    throw new AbcClientException($"DownloadData: {market}");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Return the response content as a string
                var content = await response.Content.ReadAsStringAsync();

                if (useCache && !string.IsNullOrEmpty(CacheFolder))
                {
                    string fileName = Path.Combine(CacheFolder, market + "_" + dateFrom.Year + "_" + dateFrom.Month.ToString("0#") + ".csv");
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    File.WriteAllText(fileName, content);
                }

                return content;
            }
            catch (AbcClientException ex)
            {
                StockLog.Write(ex);
                throw;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
                return null;
            }
        }

        public static bool DownloadIsin(string fileName, DateTime dateFrom, DateTime dateTo, string isin)
        {
            StockLog.Write($"Isin: {isin} From:{dateFrom:dd/MM/yy} To:{dateTo:dd/MM/yy}");
            asyncResult = false;
            Task.Run(async () =>
            {
                var data = await DownloadIsinAsync(dateFrom, dateTo, isin);
                if (string.IsNullOrEmpty(data) || data.StartsWith(" <!DOCTYPE"))
                    return;
                File.WriteAllText(fileName, data);
                asyncResult = true;
            }).Wait();

            return asyncResult;
        }

        public static bool DownloadIsinYear(string fileName, int year, string isin)
        {
            StockLog.Write($"Isin: {isin} for year: {year}");
            asyncResult = false;

            if (!string.IsNullOrEmpty(CacheFolder))
            {
                string cacheFileName = Path.Combine(CacheFolder, isin + "_" + year + ".csv");
                if (File.Exists(cacheFileName))
                {
                    File.Copy(cacheFileName, fileName, true);
                    return true;
                }
            }

            Task.Run(async () =>
            {
                var dateFrom = new DateTime(year, 1, 1);
                var dateTo = new DateTime(year, 12, 31);
                var data = await DownloadIsinAsync(dateFrom, dateTo, isin);
                if (string.IsNullOrEmpty(data) || data.StartsWith(" <!DOCTYPE"))
                    return;
                string cacheFileName = Path.Combine(CacheFolder, isin + "_" + year + ".csv");
                File.WriteAllText(cacheFileName, data);
                File.Copy(cacheFileName, fileName, true);
                asyncResult = true;
            }).Wait();

            return asyncResult;
        }

        public static async Task<string> DownloadIsinAsync(DateTime dateFrom, DateTime dateTo, string isin)
        {
            if (!await InitClientAsync())
                return null;

            try
            {
                using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.abcbourse.com/download/historiques");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7");
                request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                request.Headers.TryAddWithoutValidation("origin", "https://www.abcbourse.com");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("referer", "https://www.abcbourse.com/download/historiques");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");

                var cookieString = cookies.Select(c => $"{c.Key}={c.Value}").Aggregate((i, j) => $"{i};{j}");
                request.Headers.TryAddWithoutValidation("Cookie", cookieString);

                var requestVerificationToken = secrets.ContainsKey("__RequestVerificationToken") ? secrets["__RequestVerificationToken"] : "";

                var data = $"dateFrom={dateFrom.ToString("yyyy-MM-dd")}&__Invariant=dateFrom&dateTo={dateTo.ToString("yyyy-MM-dd")}&__Invariant=dateTo&cbox=oneSico&txtOneSico={isin}&sFormat=x&typeData=isin&__RequestVerificationToken={requestVerificationToken}&cbYes=false";

                request.Content = new StringContent(data);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == (HttpStatusCode)429)
                    throw new AbcClientException($"DownloadIsin: {isin}");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
            catch (AbcClientException ex)
            {
                StockLog.Write(ex);
                throw;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
                return null;
            }
        }


        public static AbcAgendaItem[] DownloadAgenda(string abcId)
        {
            StockLog.Write($"Isin: {abcId}");
            asyncResult = false;
            AbcAgendaItem[] agenda = null;
            Task.Run(async () =>
            {
                var data = await DownloadAgendaAsync(abcId);
                if (string.IsNullOrEmpty(data) || data.StartsWith(" <!DOCTYPE"))
                    return;

                agenda = Newtonsoft.Json.JsonConvert.DeserializeObject<AbcAgendaItem[]>(data);

                asyncResult = true;
            }).Wait();

            return agenda;
        }
        public static async Task<string> DownloadAgendaAsync(string abcId)
        {
            if (!await InitClientAsync())
                return null;

            try
            {
                using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://www.abcbourse.com/api/general/GetEventsFiltered?typeEv=all&symbolid={abcId}");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7");
                request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                request.Headers.TryAddWithoutValidation("origin", "https://www.abcbourse.com");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("referer", "https://www.abcbourse.com");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");

                var cookieString = cookies.Select(c => $"{c.Key}={c.Value}").Aggregate((i, j) => $"{i};{j}");
                request.Headers.TryAddWithoutValidation("Cookie", cookieString);

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == (HttpStatusCode)429)
                    throw new AbcClientException($"DownloadAgenda: {abcId}");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
            catch (AbcClientException ex)
            {
                StockLog.Write(ex);
                throw;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
                return null;
            }
        }


        private static List<DateTime> requestTimestamps = new List<DateTime>();

        static bool forbidden = false;
        private static async Task<bool> InitClientAsync()
        {
            try
            {
                if (forbidden)
                    return false;
                if (httpClient != null)
                {
                    var now = DateTime.Now;

                    requestTimestamps.RemoveAll(t => (now - t) > TimeSpan.FromSeconds(20));
                    if (requestTimestamps.Count < 5)
                    {
                        StockLog.Write($"ABC Bourse Short Delay");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        StockLog.Write($"ABC Bourse Long Delay");
                        await Task.Delay(2000);
                    }
                    requestTimestamps.Add(now);

                    return true;
                }

                var handler = new HttpClientHandler();
                handler.UseCookies = false;

                // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
                // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/

                var cookieContainer = new CookieContainer();
                httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer });
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                httpClient.BaseAddress = new Uri("https://www.abcbourse.com");

                using var request = new HttpRequestMessage(new HttpMethod("GET"), "download/historiques");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
                request.Headers.TryAddWithoutValidation("Cookie", "__eoi=ID=ead3dff89a2100c4:T=1762438747:RT=1762438747:S=AA-AfjbTKMaIkfAf3GQjhr2sogKO");

                var resp = await httpClient.SendAsync(request);

                if (!resp.IsSuccessStatusCode)
                {
                    forbidden = resp.StatusCode == HttpStatusCode.Forbidden;

                    Debug.WriteLine("Failed initializing ABC Provider HttpClient: " + resp.StatusCode);
                    Debug.WriteLine(resp.Content.ReadAsStringAsync().Result);
                    httpClient.Dispose();
                    httpClient = null;
                    return false;
                }

                var verifToken = FindToken("RequestVerificationToken", resp.Content.ReadAsStringAsync().Result);
                secrets = new Dictionary<string, string>();
                secrets["__RequestVerificationToken"] = verifToken;

                cookies = new Dictionary<string, string>();
                foreach (var cookie in cookieContainer.GetCookies(httpClient.BaseAddress).Cast<Cookie>())
                {
                    cookies[cookie.Name] = cookie.Value;
                }
            }
            catch (Exception)
            {
                httpClient?.Dispose();
                httpClient = null;
                return false;
            }

            return httpClient != null;
        }

        static string FindToken(string pattern, string body)
        {
            int index = body.IndexOf(pattern);
            body = body.Substring(index);
            index = body.IndexOf("value=") + 7;
            body = body.Substring(index);
            index = body.IndexOf('"');
            body = body.Remove(index);
            return body;
        }
    }


    public class AbcAgenda
    {
        public AbcAgendaItem[] Items { get; set; }
    }

    public class AbcAgendaItem
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public string Item3 { get; set; }
    }
}