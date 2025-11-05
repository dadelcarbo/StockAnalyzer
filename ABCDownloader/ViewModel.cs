using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Input;

namespace ABCDownloader;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    // Example property
    private string _curl;
    public string Curl
    {
        get => _curl;
        set
        {
            if (_curl != value)
            {
                _curl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Curl)));
                DownloadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _data;
    public string Data
    {
        get => _data;
        set
        {
            if (_data != value)
            {
                _data = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
            }
        }
    }

    public AsyncRelayCommand DownloadCommand { get; }

    public ICommand OpenAbcCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand PreviousMonthCommand { get; }

    private readonly HttpClient _httpClient;

    public ViewModel()
    {
        _httpClient = new HttpClient();
        DownloadCommand = new AsyncRelayCommand(DownloadAsync, () => !string.IsNullOrEmpty(Curl));
        OpenAbcCommand = new AsyncRelayCommand(OpenAbcAsync);
        OpenFolderCommand = new AsyncRelayCommand(OpenFolderAsync);
        PreviousMonthCommand = new AsyncRelayCommand(PreviousMonthAsync);
    }

    private async Task OpenAbcAsync()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.abcbourse.com/download/historiques",
            UseShellExecute = true
        });
    }

    private async Task OpenFolderAsync()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Folder,
            UseShellExecute = true
        });
    }

    private async Task PreviousMonthAsync()
    {
        this.FromDate = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(-1);
        this.ToDate = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(1).AddDays(-1);
    }
    private async Task DownloadAsync()
    {
        try
        {
            this.Data = $"Downloading From: {fromDate.ToString("yyyy/MM/dd")} To: {fromDate.ToString("yyyy/MM/dd")}" + Environment.NewLine;

            var cookies = ExtractCookiesFromCurl();
            var secrets = ExtractSecretsFromCurl();

            var markets = new List<string> { "indicesmkp", "indicessecp", "eurolistap", "eurolistbp", "eurolistcp", "eurogp", "euroap",
                                             "germanyf", "usau", "uke", "belg", "torontot", "spainm", "holln", "italiai", "lisboal",
                                             "switzs", "devp", "mpp", "cryptou" };

            foreach (var market in markets)
            {
                this.Data += $"Downloading market: {market}";

                var content = await DownloadDataAsync(this.FromDate, this.ToDate, cookies, secrets, market);

                // Save the string to the file
                await File.WriteAllTextAsync(Path.Combine(folder, $"{market}_{FromDate.ToString("yyy_MM")}.csv"), content);

                this.Data += Environment.NewLine + $"Data for market {market} downloaded successfully" + Environment.NewLine;

                await Task.Delay(200); // To avoid overwhelming the server

            }
            this.Data += Environment.NewLine + "Completed !!!!";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            Data = "Error downloading data. Please check the URL and try again.";
        }
    }

    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<string> DownloadDataAsync(DateTime dateFrom, DateTime dateTo, Dictionary<string, string> cookies, Dictionary<string, string> secrets, string market)
    {
        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.abcbourse.com/download/historiques"))
        {
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

            var data = $"dateFrom={dateFrom.ToString("yyyy-MM-dd")}&__Invariant=dateFrom&dateTo={dateTo.ToString("yyyy-MM-dd")}&__Invariant=dateTo&txtOneSico=&cbox={market}&sFormat=ab&typeData=isin&__RequestVerificationToken={requestVerificationToken}&cbYes=false";
            request.Content = new StringContent(data);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await httpClient.SendAsync(request);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Return the response content as a string
            return await response.Content.ReadAsStringAsync();
        }
    }

    private Dictionary<string, string> ExtractCookiesFromCurl()
    {
        var result = new Dictionary<string, string>();
        var lines = Curl.Replace("'", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim());

        var cookies = lines.FirstOrDefault(l => l.StartsWith("-b"))?.Replace("-b ", "").Replace(" \\", "").Split(';');
        if (cookies == null)
            return result;

        foreach (var cookie in cookies)
        {
            var parts = cookie.Split('=', 2);
            if (parts.Length == 2)
            {
                result.TryAdd(parts[0].Trim(), parts[1].Trim());
            }
        }

        return result;
    }
    private Dictionary<string, string> ExtractSecretsFromCurl()
    {
        var result = new Dictionary<string, string>();
        var lines = Curl.Replace("'", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim());
        var rawData = lines.FirstOrDefault(l => l.StartsWith("--data-raw"))?.Replace("--data-raw ", "").Split('&');

        foreach (var data in rawData)
        {
            var parts = data.Split('=', 2);
            if (parts.Length == 2)
            {
                result.TryAdd(parts[0].Trim(), parts[1].Trim());
            }
        }

        return result;
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }

    private DateTime fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime FromDate { get => fromDate; set => SetProperty(ref fromDate, value); }

    private DateTime toDate = DateTime.Today;
    public DateTime ToDate { get => toDate; set => SetProperty(ref toDate, value); }

    private string folder = @"C:\ProgramData\UltimateChartistDev\data\daily\ABC\WebCache";
    public string Folder { get => folder; set => SetProperty(ref folder, value); }
}

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return (_canExecute?.Invoke() ?? true) && !_isExecuting;
    }

    public async void Execute(object parameter)
    {
        _isExecuting = true;
        RaiseCanExecuteChanged();
        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
