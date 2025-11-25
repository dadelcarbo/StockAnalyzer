using System.Diagnostics;

namespace UltimateChartistSync;

class Program
{
    static string clientId = "a648dde3-63e8-4257-a717-85dda0b520ba";

    static async Task<int> Main(string[] args)
    {
        using var logger = Logger.Instance;

        //#if DEBUG
        //        if (!Debugger.IsAttached)
        //        {
        //            Debugger.Launch(); // Prompts to attach a debugger
        //        }
        //#endif

        if (args.Length < 2)
        {
            logger.WriteLine("Usage: OneDriveSyncApp <OneDriveFolderPath> <LocalFolderPath>");
            return 1;
        }

        string oneDrivePath = args[0];
        string localPath = args[1];

        try
        {
            var processes = Process.GetProcessesByName("UltimateChartistSync");
            if (Process.GetProcessesByName("UltimateChartistSync").Count() > 1)
            {
                logger.WriteLine("❌ Sync already running");
                return 1;
            }

            // Network connection sanity check
            var sw = Stopwatch.StartNew();
            bool hasConnection = await HasNetworkConnectionAsync();
            int count = 1;
            while (!hasConnection && sw.Elapsed.TotalMinutes < 10)
            {
                logger.WriteLine($"❌ Sync failed: No network connection attempt: {count}");
                await Task.Delay(30000); // 30s
                hasConnection = await HasNetworkConnectionAsync();
                count++;
            }
            if (hasConnection)
            {
                logger.WriteLine($"🔄 Network connection Validated");
                var helper = new OneDriveHelper(clientId);
                await helper.SyncFolderAsync(oneDrivePath, localPath);
            }
            else
            {
                logger.WriteLine("❌ Sync failed: No network connection, aborting");
            }
        }
        catch (Exception ex)
        {
            logger.WriteLine($"❌ Sync failed: {ex.Message}");
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
