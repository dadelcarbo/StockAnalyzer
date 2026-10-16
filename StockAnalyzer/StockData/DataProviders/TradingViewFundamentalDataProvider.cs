using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace StockAnalyzer.StockData.DataProviders
{
    public class TradingViewFundamentalDataProvider
    {
        // Fields source https://shner-elmo.github.io/TradingView-Screener/fields/stocks.html
        public static readonly Dictionary<string, string> DcfPerShare = new Dictionary<string, string>()
        {
            ["free_cash_flow_per_share_fy"] = "Free cash flow per share",
            ["total_debt_per_share_fy"] = "Total debt per share",
            ["cash_per_share_fy"] = "Cash per share"
        };

        static readonly Dictionary<string, string> marketMappings = new Dictionary<string, string>
        {
            ["EURONEXT"] = "EURONEXT",
            ["NYSE"] = "NASDAQ",
        };

        public static string GetTradingViewSymbol(StockInstrument instrument)
        {
            if (marketMappings.TryGetValue(instrument.Market.ToString(), out var tradingViewMarket))
            {
                return $"{tradingViewMarket}:{instrument.Symbol}";
            }
            return null;
        }

        static readonly Dictionary<string, string> countryMappings = new Dictionary<string, string>
        {
            ["FR"] = "france",
            ["US"] = "america",
        };
        public static string GetTradingViewCountry(StockInstrument instrument)
        {
            if (countryMappings.TryGetValue(instrument.Isin.Substring(0, 2), out var tradingViewCountry))
            {
                return tradingViewCountry;
            }
            return null;
        }


        public static async Task<SortedDictionary<string, float>> GetFundamentalsAsync(StockInstrument instrument, Dictionary<string, string> requestFields)
        {
            var http = GetClient();

            // 2. Call the scanner endpoint the statistics page uses
            var symbol = GetTradingViewSymbol(instrument);
            if (symbol == null)
            {
                throw new Exception($"No TradingView symbol mapping found for instrument {instrument.Symbol} in market {instrument.Market}");
            }
            var country = GetTradingViewCountry(instrument);
            if(country == null)
            {
                throw new Exception($"No TradingView country mapping found for instrument {instrument.Symbol} with ISIN {instrument.Isin}");
            }
            var request = new
            {
                symbols = new Dictionary<string, object>
                {
                    ["tickers"] = new[] { symbol }
                },
                columns = requestFields.Keys.ToArray()
            };

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            var url = $"https://scanner.tradingview.com/{country}/scan";
            using var resp = await http.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();

            var responseString = await resp.Content.ReadAsStringAsync();

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());

            var results = new SortedDictionary<string, float>();
            var totalCount = doc.RootElement.GetProperty("totalCount").GetInt32();
            if (totalCount == 0)
            {
                throw new Exception($"No data found for symbol {symbol}");
            }

            var data = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("d");

            int i = 0;
            foreach (var field in requestFields.Keys)
            {
                var raw = data[i].ValueKind == JsonValueKind.Number
                ? data[i].GetDouble()
                : double.NaN;

                results.Add(field, (float)raw);

                i++;
            }

            return results;
        }

        private static HttpClient httpClient;
        private static HttpClient GetClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }
            return httpClient;
        }

    }
}
