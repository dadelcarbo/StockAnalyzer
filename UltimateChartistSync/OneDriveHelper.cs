using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UltimateChartistSync
{
    public class OneDriveHelper
    {
        private static string syncFileName = ".LastSync.txt";
        private readonly string clientId;
        private readonly string[] scopes = { "Files.ReadWrite", "User.Read" };
        private readonly string cacheFilePath = @"C:\ProgramData\UltimateChartist\OneDriveSync\msal_cache.bin";
        private IPublicClientApplication app;
        private HttpClient httpClient;
        AuthenticationResult authResult;

        public OneDriveHelper(string clientId)
        {
            this.clientId = clientId;
            InitializeApp();
        }

        private void InitializeApp()
        {
            app = PublicClientApplicationBuilder.Create(clientId)
                .WithRedirectUri("http://localhost")
                .WithAuthority(AzureCloudInstance.AzurePublic, "consumers")
                .Build();

            SetupTokenCache(app.UserTokenCache);
            httpClient = new HttpClient();
        }
        private async Task EnsureAuthenticatedAsync()
        {
            try
            {
                if (authResult == null || authResult.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    var accounts = await app.GetAccountsAsync();
                    try
                    {
                        authResult = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                                              .ExecuteAsync();
                    }
                    catch (MsalUiRequiredException)
                    {
                        authResult = await app.AcquireTokenInteractive(scopes)
                                              .WithAccount(accounts.FirstOrDefault())
                                              .ExecuteAsync();
                    }

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                }
            }
            catch (Exception ex)
            {
                throw new OneDriveHelperException("OneDrive authentication failure", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string path)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}";
            var response = await httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> ReadFileAsync(string path)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}:/content";
            return await httpClient.GetStringAsync(url);
        }

        public async Task WriteFileAsync(string path, string content)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}:/content";
            var contentBytes = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
            await httpClient.PutAsync(url, contentBytes);
        }

        public async Task UploadFileAsync(string path, string sourceFilePath)
        {
            await EnsureAuthenticatedAsync();

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Local file not found.", sourceFilePath);

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}:/content";
            using var fileStream = File.OpenRead(sourceFilePath);
            using var content = new StreamContent(fileStream);

            var response = await httpClient.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Upload failed: {response.StatusCode} - {error}");
            }
            else
            {
                // Sync local file with OneDrive date
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("lastModifiedDateTime", out JsonElement item))
                {
                    File.SetLastWriteTime(sourceFilePath, item.GetDateTime().ToUniversalTime());
                }
            }
        }
        public async Task DownloadFileAsync(string path, string destinationFilePath)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}:/content";

            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Download failed: {response.StatusCode} - {error}");
            }

            var directoryPath = Path.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
        }
        public async Task DeleteFileAsync(string path)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{path}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Delete failed: {response.StatusCode} - {error}");
            }

            Logger.Instance.WriteLine($"✅ Deleted OneDrive file: {path}");
        }


        public async Task CreateDirectoryAsync(string folderPath)
        {
            await EnsureAuthenticatedAsync();

            var url = $"https://graph.microsoft.com/v1.0/me/drive/root/children";
            var json = $@"{{ ""name"": ""{folderPath}"", ""folder"": {{}}, ""@microsoft.graph.conflictBehavior"": ""rename"" }}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }

        public async Task<List<string>> EnumerateFilesAsync(string oneDriveFolderPath, bool recursive = false)
        {
            await EnsureAuthenticatedAsync();

            var files = new List<string>();
            var queue = new Queue<(string oneDrivePath, string relativePath)>();
            queue.Enqueue((oneDriveFolderPath, ""));

            while (queue.Count > 0)
            {
                var (currentPath, relativePath) = queue.Dequeue();
                var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{currentPath}:/children";
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("value", out JsonElement items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var name = item.GetProperty("name").GetString()!;
                        var isFolder = item.TryGetProperty("folder", out _);
                        var newRelativePath = string.IsNullOrEmpty(relativePath) ? name : $"{relativePath}/{name}";

                        if (isFolder && recursive)
                        {
                            queue.Enqueue(($"{currentPath}/{name}", newRelativePath));
                        }
                        else if (!isFolder)
                        {
                            files.Add(newRelativePath);
                        }
                    }
                }
            }

            return files;
        }

        public async Task<Dictionary<string, FileMetaData>> EnumerateFileInfosAsync(string oneDriveFolderPath, bool recursive = false)
        {
            await EnsureAuthenticatedAsync();

            var fileInfos = new Dictionary<string, FileMetaData>();
            var queue = new Queue<(string oneDrivePath, string relativePath)>();
            queue.Enqueue((oneDriveFolderPath, ""));

            while (queue.Count > 0)
            {
                var (currentPath, relativePath) = queue.Dequeue();
                var url = $"https://graph.microsoft.com/v1.0/me/drive/root:/{currentPath}:/children";
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("value", out JsonElement items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var name = item.GetProperty("name").GetString()!;
                        var isFolder = item.TryGetProperty("folder", out _);
                        var newRelativePath = string.IsNullOrEmpty(relativePath) ? name : $"{relativePath}/{name}";

                        if (isFolder && recursive)
                        {
                            queue.Enqueue(($"{currentPath}/{name}", newRelativePath));
                        }
                        else if (!isFolder)
                        {
                            var metadata = new FileMetaData
                            {
                                Name = item.GetProperty("name").GetString()!,
                                Id = item.GetProperty("id").GetString()!,
                                Size = item.GetProperty("size").GetInt64(),
                                LastModified = item.GetProperty("lastModifiedDateTime").GetDateTime().ToUniversalTime()
                            };

                            fileInfos[newRelativePath] = metadata;
                        }
                    }
                }
            }

            return fileInfos;
        }

        public async Task SyncFolderAsync(string oneDrivePath, string localPath)
        {
            Logger.Instance.WriteLine($"🔄 Starting sync between local: {localPath} and OneDrive: {oneDrivePath}");

            await EnsureAuthenticatedAsync();

            var oneDriveFiles = await EnumerateFileInfosAsync(oneDrivePath, true);

            // Identify deleted files
            DateTime lastOneDriveSyncDate = DateTime.MinValue;
            if (oneDriveFiles.TryGetValue(syncFileName, out FileMetaData lastSyncOneDriveFile))
            {
                lastOneDriveSyncDate = lastSyncOneDriveFile.LastModified.ToLocalTime();
            }
            var lastSyncPath = Path.Combine(localPath, syncFileName);
            DateTime lastlocalSyncDate = DateTime.MinValue;
            if (File.Exists(lastSyncPath))
            {
                lastlocalSyncDate = File.GetLastWriteTime(lastSyncPath);
            }

            var localFiles = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .ToDictionary(f => Path.GetRelativePath(localPath, f.FullName).Replace("\\", "/"));

            await Parallel.ForEachAsync(localFiles.Keys.Union(oneDriveFiles.Keys), async (relativePath, cancellationToken) =>
            {
                localFiles.TryGetValue(relativePath, out var localFile);
                oneDriveFiles.TryGetValue(relativePath, out var oneDriveFile);

                var oneDriveFullPath = $"{oneDrivePath}/{relativePath}";

                if (localFile != null && oneDriveFile != null)
                {
                    var localTime = localFile.LastWriteTimeUtc;
                    var oneDriveTime = oneDriveFile.LastModified;

                    if (localTime == oneDriveTime)
                    {
                        // Nothing to do.
                        Logger.Instance.WriteLine($"⬆️ Same are in sync: {relativePath}");
                    }
                    else if (localTime > oneDriveTime)
                    {
                        Logger.Instance.WriteLine($"⬆️ Uploading newer local file: {relativePath}");
                        await UploadFileAsync(oneDriveFullPath, localFile.FullName);
                    }
                    else if (oneDriveTime > localTime)
                    {
                        Logger.Instance.WriteLine($"⬇️ Downloading newer OneDrive file: {relativePath}");
                        await DownloadFileAsync(oneDriveFullPath, Path.Combine(localPath, relativePath));
                    }
                }
                else if (localFile != null)
                {
                    // Check if needed to delete local or upload
                    if (localFile.LastWriteTime > lastOneDriveSyncDate)
                    {
                        Logger.Instance.WriteLine($"⬆️ Uploading new local file: {relativePath}");
                        await UploadFileAsync(oneDriveFullPath, localFile.FullName);
                    }
                    else
                    {
                        Logger.Instance.WriteLine($"⬆️ Deleting local file: {relativePath}");
                        File.Delete(localFile.FullName);
                    }
                }
                else if (oneDriveFile != null)
                {
                    // Check if needed to delete the One Drive file
                    if (oneDriveFile.LastModified > lastlocalSyncDate)
                    {
                        Logger.Instance.WriteLine($"⬇️ Downloading new OneDrive file: {relativePath}");
                        var filePath = Path.Combine(localPath, relativePath);

                        await DownloadFileAsync(oneDriveFullPath, filePath);
                        File.SetLastWriteTime(filePath, oneDriveFile.LastModified);
                    }
                    else
                    {
                        Logger.Instance.WriteLine($"⬇️ Deleting OneDrive file: {relativePath}");
                        await DeleteFileAsync(oneDriveFullPath);
                    }
                }
            });

            // UpdateLastSyncFile
            await File.WriteAllTextAsync(lastSyncPath, Environment.MachineName + "=>" + DateTime.UtcNow.ToString("o"));
            await UploadFileAsync($"{oneDrivePath}/LastSync.txt", lastSyncPath);

            Logger.Instance.WriteLine("✅ Sync complete.");
        }


        private void SetupTokenCache(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(args =>
            {
                if (File.Exists(cacheFilePath))
                    args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(cacheFilePath));
            });

            tokenCache.SetAfterAccess(args =>
            {
                if (args.HasStateChanged)
                    File.WriteAllBytes(cacheFilePath, args.TokenCache.SerializeMsalV3());
            });
        }

        public class FileMetaData
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public long Size { get; set; }
            public DateTime LastModified { get; set; }
        }

        public class OneDriveHelperException : Exception
        {
            public OneDriveHelperException() { }

            public OneDriveHelperException(string message)
            : base(message) { }

            public OneDriveHelperException(string message, Exception inner)
            : base(message, inner) { }
        }

    }
}
