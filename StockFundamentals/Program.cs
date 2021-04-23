using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockFundamentals
{
    class Program
    {

        const string PARAMS = "{\"filter\":[{\"left\":\"price_book_ratio\",\"operation\":\"nempty\"},{\"left\":\"type\",\"operation\":\"equal\",\"right\":\"stock\"},{ \"left\":\"subtype\",\"operation\":\"equal\",\"right\":\"common\"},{ \"left\":\"SMA50\",\"operation\":\"less\",\"right\":\"close\"}],\"options\":{ \"active_symbols_only\":true,\"lang\":\"en\"},\"symbols\":{ \"query\":{ \"types\":[]},\"tickers\":[]},\"columns\":[\"logoid\",\"name\",\"close\",\"market_cap_basic\",\"price_earnings_ttm\",\"earnings_per_share_basic_ttm\",\"sector\",\"price_book_ratio\",\"dividend_yield_recent\",\"enterprise_value_fq\",\"enterprise_value_ebitda_ttm\",\"gross_margin\",\"description\",\"name\",\"type\",\"subtype\",\"update_mode\",\"pricescale\",\"minmov\",\"fractional\",\"minmove2\"],\"sort\":{ \"sortBy\":\"price_book_ratio\",\"sortOrder\":\"asc\"},\"range\":[0,150]}";

        //     logoid	name	close	market_cap_basic	price_earnings_ttm	earnings_per_share_basic_ttm	sector	price_book_ratio	dividend_yield_recent	enterprise_value_fq	enterprise_value_ebitda_ttm	gross_margin	description	name	type	subtype	update_mode	pricescale	minmov	fractional	minmove2

        [STAThread]
        static void Main(string[] args)
        {

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.boursorama.com");

            var response = client.GetAsync("bourse/action/graph/ws/GetTicksEOD?symbol=1rPCA&length=5&period=-1&guid=").GetAwaiter().GetResult();
            var data = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            client = new HttpClient();
            client.BaseAddress = new Uri("https://scanner.tradingview.com");

            var content = new StringContent(PARAMS);

            var result = client.PostAsync("france/scan", content).GetAwaiter().GetResult();
            if (result.IsSuccessStatusCode)
            {
                var resultString = result.Content.ReadAsStringAsync().Result;
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(resultString);
                string text = string.Empty;
                foreach (var item in myDeserializedClass.data)
                {
                    var line = item.d.Select(d => d?.ToString()).Aggregate((i, j) => i + "\t" + j);
                    text += line + Environment.NewLine;
                }

                Clipboard.SetText(text);
            }
        }

        public class Datum
        {
            public string s { get; set; }
            public List<object> d { get; set; }
        }

        public class Root
        {
            public List<Datum> data { get; set; }
            public int totalCount { get; set; }
        }
    }
}
