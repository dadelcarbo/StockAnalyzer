using System.Diagnostics;

namespace UltimateChartistSync;

class Program
{
    static string clientId = "a648dde3-63e8-4257-a717-85dda0b520ba";

    static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Logger.Instance.WriteLine("Usage: OneDriveSyncApp <OneDriveFolderPath> <LocalFolderPath>");
            return 1;
        }

        string oneDrivePath = args[0];
        string localPath = args[1];

        try
        {
            // Network connection sanity check
            var sw = Stopwatch.StartNew();
            bool hasConnection = await HasNetworkConnectionAsync();

            while (!hasConnection && sw.Elapsed.TotalMinutes < 10)
            {
                await Task.Delay(30000); // 30s
                hasConnection = await HasNetworkConnectionAsync();
            }
            if (hasConnection)
            {
                var helper = new OneDriveHelper(clientId);
                await helper.SyncFolderAsync(oneDrivePath, localPath);
            }
            else
            {
                Logger.Instance.WriteLine("❌ Sync failed: No network connection");
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.WriteLine($"❌ Sync failed: {ex.Message}");
            return 1;
        }

        return 0;
    }

    public static async Task<bool> HasNetworkConnectionAsync()
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var response = await httpClient.GetAsync("https://www.microsoft.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

}
