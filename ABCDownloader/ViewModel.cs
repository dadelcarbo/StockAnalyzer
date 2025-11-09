using ABCDownloader.AbcDataProvider;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ABCDownloader;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

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

    private string _isin;
    public string Isin
    {
        get => _isin;
        set
        {
            if (_isin != value)
            {
                _isin = value.Trim();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Isin)));
            }
        }
    }

    public ICommand OpenAbcCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand DownloadDataCommand { get; }
    public ICommand DownloadLabelCommand { get; }
    public ICommand DownloadIsinCommand { get; }

    private readonly HttpClient _httpClient;

    //List<string> markets = new List<string> { "indicesmkp", "indicessecp", "eurolistap", "spainm" };

    List<string> markets = new List<string> {
            "indicesmkp", "indicessecp", "eurolistap", "eurolistbp", "eurolistcp", "eurogp", "euroap",
            "germanyf", "usau", "uke", "belg", "torontot", "spainm", "holln", "italiai", "lisboal",
            "switzs", "devp", "mpp", "cryptou", "trackp" };

    public ViewModel()
    {
        _httpClient = new HttpClient();
        OpenAbcCommand = new AsyncRelayCommand(OpenAbcAsync);
        OpenFolderCommand = new AsyncRelayCommand(OpenFolderAsync);
        PreviousMonthCommand = new AsyncRelayCommand(PreviousMonthAsync);
        NextMonthCommand = new AsyncRelayCommand(NextMonthAsync);
        DownloadDataCommand = new AsyncRelayCommand(DownloadDataAsync);
        DownloadLabelCommand = new AsyncRelayCommand(DownloadLabelAsync);
        DownloadIsinCommand = new AsyncRelayCommand(DownloadIsinAsync);
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
    private async Task NextMonthAsync()
    {
        this.FromDate = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(1);
        this.ToDate = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(1).AddDays(-1);
    }
    private async Task DownloadLabelAsync()
    {
        try
        {
            foreach (var market in markets)
            {
                this.Data = $"Downloading market: {market}";

                var content = await AbcClient.DownloadLabelAsync(market);

                // Save the string to the file
                await File.WriteAllTextAsync(Path.Combine(folder, $"{market}_Label.csv"), content);

                this.Data += Environment.NewLine + $"Labels for market {market} downloaded successfully" + Environment.NewLine;

                await Task.Delay(20); // To avoid overwhelming the server

            }
            this.Data = $"Downloading From: {fromDate.ToString("yyyy/MM/dd")} To: {fromDate.ToString("yyyy/MM/dd")}" + Environment.NewLine;
            this.Data += $"Completed !!!!";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            Data += Environment.NewLine + "Error downloading data. Please check the URL and try again.";
        }
    }
    private async Task DownloadIsinAsync()
    {
        try
        {
            this.Data = $"Downloading market: {this.Isin}";

            if (string.IsNullOrEmpty(_isin))
                return;

            var content = await AbcClient.DownloadIsinAsync(this.FromDate, this.ToDate, this.Isin);

            this.Data += Environment.NewLine + content + Environment.NewLine;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            Data += Environment.NewLine + "Error downloading data. Please check the URL and try again.";
        }
    }
    private async Task DownloadDataAsync()
    {
        try
        {
            foreach (var market in markets)
            {
                this.Data = $"Downloading market: {market}";

                var content = await AbcClient.DownloadDataAsync(this.FromDate, this.ToDate, market);

                // Save the string to the file
                await File.WriteAllTextAsync(Path.Combine(folder, $"{market}_{FromDate.ToString("yyy_MM")}.csv"), content);

                this.Data += Environment.NewLine + $"Data for market {market} downloaded successfully" + Environment.NewLine;

                await Task.Delay(20); // To avoid overwhelming the server

            }
            this.Data = $"Downloading From: {fromDate.ToString("yyyy/MM/dd")} To: {fromDate.ToString("yyyy/MM/dd")}" + Environment.NewLine;
            this.Data += $"Completed !!!!";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            Data += Environment.NewLine + "Error downloading data. Please check the URL and try again.";
        }
    }

    private Dictionary<string, string> ExtractCookiesFromCurl(string curl)
    {
        var result = new Dictionary<string, string>();
        var lines = curl.Replace("'", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim());

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
    private Dictionary<string, string> ExtractSecretsFromCurl(string curl)
    {
        var result = new Dictionary<string, string>();
        var lines = curl.Replace("'", "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim());
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

    private string folder = @"C:\ProgramData\UltimateChartist\data\daily\ABC\WebCache";
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
