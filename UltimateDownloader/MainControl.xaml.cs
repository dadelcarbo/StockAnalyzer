using System.Net.Http;

namespace UltimateDownloader;

/// <summary>
/// Interaction logic for MainControl.xaml
/// </summary>
public partial class MainControl : System.Windows.Controls.UserControl
{
    public MainControl()
    {
        InitializeComponent();
    }

    private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await SendRequest();
    }


    static async Task SendRequest()
    {
        // Create HTTP client
        using (var client = new HttpClient())
        {
            var requestUri = new Uri("https://tvc4.investing.com/3d83b9a90ebadcc8393b85b526d3dd96/1753259866/1/1/8/history?symbol=1175151&resolution=5&from=1752827896&to=1753259956");

            // Add headers
            client.DefaultRequestHeaders.Add("accept", "*/*");
            client.DefaultRequestHeaders.Add("accept-language", "en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7");
            // Content-Type will be set with the request content
            client.DefaultRequestHeaders.Add("origin", "https://tvc-invdn-cf-com.investing.com");
            client.DefaultRequestHeaders.Add("priority", "u=1, i");
            client.DefaultRequestHeaders.Add("referer", "https://tvc-invdn-cf-com.investing.com/");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "Windows");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36 Edg/138.0.0.0");

            // Send the request
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Process the response
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Display the response
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(responseBody);
        }
    }
}
