using SharpDX;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using Telerik.Windows.Persistence.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace InvestingFundamentals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var fundamentals = await DownloadAsync();

            this.grid.ItemsSource = fundamentals.hits;
        }

        private async Task<Fundamentals> DownloadAsync()
        {
            var handler = new HttpClientHandler();

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.investing.com/stock-screener/Service/SearchStocks"))
                {
                    request.Headers.TryAddWithoutValidation("authority", "www.investing.com");
                    request.Headers.TryAddWithoutValidation("accept", "application/json, text/javascript, */*; q=0.01");
                    request.Headers.TryAddWithoutValidation("accept-language", "fr-FR,fr;q=0.9,en-GB;q=0.8,en;q=0.7,en-US;q=0.6");
                    request.Headers.TryAddWithoutValidation("origin", "https://www.investing.com");
                    request.Headers.TryAddWithoutValidation("referer", "https://www.investing.com/stock-screener/?sp=country::34^|sector::a^|industry::a^|equityType::ORD^%^3Ceq_market_cap;1");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");


                    request.Content = new StringContent("country%5B%5D=22&sector=36%2C25%2C27%2C28%2C24%2C29%2C35%2C30%2C26%2C34%2C33%2C31%2C32&industry=182%2C190%2C204%2C199%2C212%2C177%2C172%2C207%2C214%2C217%2C179%2C184%2C203%2C181%2C185%2C197%2C222%2C215%2C220%2C202%2C200%2C187%2C229%2C209%2C210%2C192%2C195%2C193%2C228%2C206%2C218%2C205%2C208%2C194%2C183%2C196%2C178%2C230%2C225%2C223%2C216%2C173%2C174%2C180%2C188%2C201%2C211%2C232%2C186%2C226%2C175%2C227%2C231%2C213%2C219%2C198%2C221%2C191%2C189%2C176%2C224&equityType=ORD&pn=1&order%5Bcol%5D=eq_market_cap&order%5Bdir%5D=d");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                    var tt = await response.Content.ReadAsStringAsync();

                    //var index = tt.IndexOf(",\"pageNumber");
                    //tt = tt.Substring(0,index) + "}";

                    return JsonSerializer.Deserialize<Fundamentals>(tt);
                }
            }
        }

        private void grid_AutoGeneratingColumn(object sender, GridViewAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString().EndsWith("_eu") || e.Column.Header.ToString().EndsWith("_us"))
                e.Cancel = true;
        }
    }
}
