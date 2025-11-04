using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup.Localizer;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    class ABCDownloadGroup
    {
        public Groups Group { get; set; }

        public string AbcCode { get; set; }

        public string RefSerie { get; set; }

        public bool LabelOnly { get; set; }

        public string[] Prefixes { get; set; }

        /// <summary>
        /// Date of the last download attempt
        /// </summary>
        public DateTime LastDownload { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Date of the last downloaded date for the refSerie
        /// </summary>
        public DateTime LastDownloaded { get; set; } = DateTime.MinValue;
        public bool SkipDownload { get; set; } = true;
    }


    public class ABCDataProvider : StockDataProviderBase, IConfigDialog
    {
        private static readonly string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
        private static readonly string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
        private static readonly string ABC_DAILY_CFG_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl";
        private static readonly string ABC_DAILY_CFG_GROUP_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\group";
        private static readonly string ABC_DAILY_CFG_SECTOR_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\sector";
        private static readonly string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
        private static readonly string CONFIG_FILE = "EuronextDownload.cfg";
        private static readonly string ABC_TMP_FOLDER = ABC_DAILY_FOLDER + @"\TMP";
        private static readonly string ABC_WEB_CACHE_FOLDER = ABC_DAILY_FOLDER + @"\WebCache";
        private static readonly string ABC_TMP_CACHE_FOLDER = ABC_DAILY_FOLDER + @"\TmpCache";

        #region ABC DOWNLOAD HELPER

        private CookieContainer cookieContainer = null;
        private HttpClient httpClient = null;
        public string verifToken = null;

        public bool forbidden = false;

        private bool Initialize()
        {
            return false;
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return false;
            if (this.httpClient == null && !forbidden)
            {
                try
                {
                    cookieContainer = new CookieContainer();
                    var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer });
                    httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                    httpClient.BaseAddress = new Uri("https://www.abcbourse.com/");

                    var resp = httpClient.GetAsync("download/historiques").GetAwaiter().GetResult();
                    if (!resp.IsSuccessStatusCode)
                    {
                        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            forbidden = true;
                        }
                        StockLog.Write("Failed initializing ABC Provider HttpClient: " + resp.Content.ReadAsStringAsync().Result);
                        return false;
                    }

                    verifToken = FindToken("RequestVerificationToken", resp.Content.ReadAsStringAsync().Result);
                    this.httpClient = httpClient;

                    //{
                    //    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://www.abcbourse.com/api/palmares/GetPalmares?sVar=h&sDate=yesterday&sMarket=eurolistBp"))
                    //    {
                    //        request.Headers.TryAddWithoutValidation("authority", "www.abcbourse.com");
                    //        request.Headers.TryAddWithoutValidation("accept", "*/*");
                    //        request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr;q=0.8");
                    //        request.Headers.TryAddWithoutValidation("cookie", "_ga=GA1.2.464449205.1632342487; _lr_env_src_ats=false; __gads=ID=976880403fdbddc8:T=1632428829:S=ALNI_MaBr79UeSpSeSqMxoN1lLLjyu4cow; trc_cookie_storage=taboola^%^2520global^%^253Auser-id^%^3Dada4ba88-668c-4676-ae3e-bde92c328c2a-tuct8454c22; _cc_id=56e047fe9db05b649ec837d55b875b99; cto_bundle=BtDmjl8wb3haeWdkUXZMTmlIMThZajVnYnZuSTVOazRtSmZkbjlYNUxacGpzekYzUHdLTFBwNnByVUlSYUNVbnNORkRteUhhdG1rb2xDSHNNN3c3VWo1Q2lRcGVpUFFiYW1lSHM0QmZ4Vm1aRnNrOUZmTiUyRlRTYUZTSlROZlA1UkRQWWpIakdEYnl4VSUyQk1FSWRCeFAwMEtpeEtBJTNEJTNE; cto_bidid=DnArOV9reXNHJTJCU1lDcDA1SSUyRjJWU1R2cGRFWVlnN2xzR1ZRWDZOeGlpT0ZNeDY0cnJFYW1Bdnl0Tm9BYmNXNU5FTiUyQmhoUDVhYzQwczJoeVRtWWs5MVEwTEREdXJ5V3ZjS1A2dXMyNnFuVjMlMkJ6MzFjJTNE; _sharedID=cff2808b-85cd-4eee-b7c5-4fffb1b1c95a; st_audience_clientId=e9972dd0-17e6-4908-8b46-945f66a66b93; euconsent-v2=CPeG8YAPeG8YABcAIEENCdCgAP_AAH_AAAqIIDkZ_C9NQWNjcX59E_s0eYxHxgCWoWQADACJgygBCAPA8IQEwGAYIAVAAogKAAAAoiJBAAAhCAlAAAEAQAAAACGMAEAAAAAAIKIAgEARAgEICAhBkQAQEIIQAABBABQAgAAEQhoAQAAAAAAgAEAAAAAgAICBAAQAAAAAAQAAAAAAAIggOACYKkxAA2JQYE2QaRQogBBGABQAIAAIAIGCKAAIAADghABQYAgAAAAAiAgAAACiBgEAAAEACEAAAABAAEAAAAgAAAAAAAAAIgAAQAEAAAAACACBABAAAAAAAEEABACAAAAAEABAAAAAACAAAAIAAAAAgMAAAAAAAAABAAAAAAAAAAAA.x7xHCHw.4YpAQgBZAC6AGwASoAxAB-gEQAKMAVIA4oB6wElgPVAiIBEkCUQEsQJagTOAwsBzIDpwJDIUFhQYChEFOoKzwWZgtHBb6C50F5IMUgAhACgACADQAIQAhAEgACAAUACAAKgAaACYAGA; uid_dm=1e044079-e36f-0cff-fe1e-0c440d91a916; ABCBourse=CfDJ8MljebP1noVHmnIJ9WWUbjS_F7bpGT7FiQ4UDwwtFw1tVtBRixr5N8LbAIH0tGV5LndBCUYDa8Oykl0LR_LsLvc05iK4zcC1RXGfkpSNadQmZ7veFbN75vKqfYXZgbep0SOd9KLtR4ST7eDNuqRoDk9LJoKerMV2a2Y0zp3iMP4VF9J5hFHDC0VqsWA2RHJQTdTmvSHPRTT_F1UEoC09R_IFo-jy8qsyT9UE0frCthidPufQh4Nlcep7gPOemBW-hZZ_MLCIJ3cFyESGz9q-oeUNeEBZusmxgQH84Gcz2ly_huSmHW4oMJTJqzVN_WntueWZ7G5dppKjPXQaaZJmEC5WOhv-Wy8q7TNezY_TnBGbm92t8-sdmzIaOA2BOBk6R7Rn6WlbrRSli861HjKIOWpwsqH47VWESywjgFNwvOyHRLAei8EvHXqzrOEYIEMr0qPS2HggRf_8Wd6WAGE4v-094K6Y2K5nSEODs_fV99EhxpkFHbjs6pZb36NqG8JDNYbruH_CcfaPD_ozFxDEIXObcxF8Y6t-enZfwaT0-Zw3M8Myo4bsdYv1h_cbJZ8biiwpq0-SEmDXHm3oAsRTXE8; __gpi=UID=00000ab92d108069:T=1662555905:RT=1664536430:S=ALNI_MYfNfUGYztjsp4dm20XpALFN26Tkw; panoramaId_expiry=1665150040558; panoramaId=d68c3887a8e3ed9102825e0b0b6f4945a702fd1fff99f1051c96b1a6627c2a52; _gid=GA1.2.1226607729.1665126201; _pbjs_userid_consent_data=3998499341092627; pbjs-id5id=^%^7B^%^22created_at^%^22^%^3A^%^222021-09-23T20^%^3A14^%^3A46Z^%^22^%^2C^%^22id5_consent^%^22^%^3Atrue^%^2C^%^22original_uid^%^22^%^3A^%^22ID5*ZPJ4_59biN2x3L0NXIOD0UbpNXdnUrVS0OnMlYG0HaMmHoQ-JiQzaG_7O6cLE7n3JiAbQLciZE11bxHTiHp-IiYhDOoFwJEObHZrOZJVXNMmIhD8cCuN4rnmyWFPEEbpJiR2Scg71__dxl9QkfZfGCYv20BtzmLskhB61K76VaYmMWjG_pou3lPXvn4JIlt4JjIP_9aWS7-Eolf2niuI8yY3MkLC1iZrJAdD78F5qPomORqaK9jk7X2lhRTzPpU2JjxLUA17CtFYdwIdoYqcRw^%^22^%^2C^%^22universal_uid^%^22^%^3A^%^22ID5*WUHcz3q2P6fmXBU6dbiiQBsIcVuuiuALH74xdGjW3qwmHstExugHo7McT68weLCKJiAUFZrKSqcAKE8MapnVHSYhsXl2gGZ95BU5idZXsdImIjsM0DLfp2ZMsOL6IvhsJiQM0eTKl81TKjP61j8vMCYvnu1N1RoyxnJbu-2EUq4mMdp2Z-KCbEGs_YOni1VkJjI-G2Yo3fwydJSGMGyNyiY3ejh4S4iCz-RDL-pvYAYmOePZexov8zG0J9XT5BHoJjxOnGx52VPhUiefRQiI4A^%^22^%^2C^%^22signature^%^22^%^3A^%^22ID5_AeqFG1kejV_-mV80Yp-ZKoqTvAqPPtcslA-IVQ59ucTH5vI79Wxu56Sh8okW5jzb82Z1Jj1EyAzDRTaTLkQG1q4^%^22^%^2C^%^22link_type^%^22^%^3A2^%^2C^%^22cascade_needed^%^22^%^3Afalse^%^2C^%^22privacy^%^22^%^3A^%^7B^%^22jurisdiction^%^22^%^3A^%^22gdpr^%^22^%^2C^%^22id5_consent^%^22^%^3Atrue^%^7D^%^7D; pbjs-id5id_last=Fri^%^2C^%^2007^%^20Oct^%^202022^%^2007^%^3A03^%^3A35^%^20GMT; .AspNetCore.Antiforgery.fDmU841VioE=CfDJ8MljebP1noVHmnIJ9WWUbjR6lv7HEM5W6Tr0VZ0Sr1fuXEU2vI62yiJxtSXwEMg5AO3qgEbWVZZWB6W8_vnAc8EgOHKtQW1d_xc8XKcmSFswopOOrP4VetJzh2xte1I7OhZcHsqTAtwkrIaYC0_MVzI; _lr_retry_request=true; cto_bidid=uqBTY19reXNHJTJCU1lDcDA1SSUyRjJWU1R2cGRFWVlnN2xzR1ZRWDZOeGlpT0ZNeDY0cnJFYW1Bdnl0Tm9BYmNXNU5FTiUyQmhoUDVhYzQwczJoeVRtWWs5MVEwTEREcWtOdmw1VE5JOUN6eXBhdkIlMkJyWWJFJTNE; cto_bidid=uqBTY19reXNHJTJCU1lDcDA1SSUyRjJWU1R2cGRFWVlnN2xzR1ZRWDZOeGlpT0ZNeDY0cnJFYW1Bdnl0Tm9BYmNXNU5FTiUyQmhoUDVhYzQwczJoeVRtWWs5MVEwTEREcWtOdmw1VE5JOUN6eXBhdkIlMkJyWWJFJTNE; cto_bundle=Pud6Vl8wb3haeWdkUXZMTmlIMThZajVnYnZ1eHo5YjFVUXVoQjRRc0RyWmxudWdNa0NCYmNXTFFkcG1pWEliOFExRmhOWlhma3RBaDVqMTYlMkZHakVJTTdwYzFTVVB4Y2VadFpkVUpoNThGcG50JTJCMlU2RjRKbDZVNEolMkZucWwlMkJvdHRzdFdIcDk3Q2xCUGcyb2RjbnVDR3JKalRJZyUzRCUzRA; cto_bundle=Pud6Vl8wb3haeWdkUXZMTmlIMThZajVnYnZ1eHo5YjFVUXVoQjRRc0RyWmxudWdNa0NCYmNXTFFkcG1pWEliOFExRmhOWlhma3RBaDVqMTYlMkZHakVJTTdwYzFTVVB4Y2VadFpkVUpoNThGcG50JTJCMlU2RjRKbDZVNEolMkZucWwlMkJvdHRzdFdIcDk3Q2xCUGcyb2RjbnVDR3JKalRJZyUzRCUzRA");
                    //        request.Headers.TryAddWithoutValidation("referer", "https://www.abcbourse.com/palmares/palmares");
                    //        request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    //        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    //        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    //        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                    //        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                    //        request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    //        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");

                    //        var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                    //        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                    return false;
                }
            }
            return httpClient != null;
        }

        private string FindToken(string pattern, string body)
        {
            int index = body.IndexOf(pattern);
            body = body.Substring(index);
            index = body.IndexOf("value=") + 7;
            body = body.Substring(index);
            index = body.IndexOf('"');
            body = body.Remove(index);
            return body;
        }

        const string DOWNLOAD_ISIN_BODY =
            "dateFrom=$START_DATE&__Invariant=dateFrom&" +
            "dateTo=$END_DATE&__Invariant=dateTo&" +
            "cbox=oneSico&" +
            "txtOneSico=$ISIN&" +
            "sFormat=x&" +
            "typeData=isin&" +
            "__RequestVerificationToken=$TOKEN&" +
            "cbYes=false";

        public bool DownloadISIN(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
        {
            if (startDate > endDate)
                return true;
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_ISIN_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$ISIN", ISIN);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/historiques", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType.Contains("html"))
                    return false;
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using var fileStream = File.Create(Path.Combine(destFolder, fileName));
                respStream.CopyTo(fileStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_GROUP_BODY =
            "dateFrom=$START_DATE&" +
            "dateTo=$END_DATE&" +
            "cbox=$GROUP&" +
            "txtOneSico=&" +
            "sFormat=w&" +
            "typeData=isin&" +
            "__RequestVerificationToken=$TOKEN&"
            + "cbYes=false";

        static DateTime cacheEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public bool DownloadGroup(string fileName, DateTime startDate, DateTime endDate, string group)
        {
            if (!this.Initialize())
                return false;

            try
            {
                string cacheFileName = fileName.Replace("\\TMP\\", "\\TMPCache\\");
                if (File.Exists(cacheFileName))
                {
                    File.Copy(cacheFileName, fileName);
                    return true;
                }

                var postData = DOWNLOAD_GROUP_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                // StockLog.Write("Downloading group: " + postData);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/historiques", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType.Contains("html"))
                {
                    StockLog.Write("Failed downloading group: " + resp.Content.ReadAsStringAsync().Result.Substring(0, 200));
                    return false;
                }
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using (var fileStream = File.Create(fileName))
                {
                    respStream.CopyTo(fileStream);
                }
                if (endDate < cacheEndDate)
                {
                    File.Copy(fileName, cacheFileName);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                return false;
            }
            return true;
        }

        const string DOWNLOAD_INTRADAY_GROUP_BODY =
            "splace=$GROUP&" +
            "sformat=w&" +
            "__RequestVerificationToken=$TOKEN";
        public bool DownloadIntradayGroup(string destFolder, string fileName, string group)
        {
            StockLog.Write(group);
            if (!this.Initialize())
                return false;

            var now = DateTime.Now.TimeOfDay;
            if (now < TimeSpan.FromHours(9) || now > TimeSpan.FromHours(17.5))
                return false;

            try
            {
                var postData = DOWNLOAD_INTRADAY_GROUP_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/telechargement_intraday", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType.Contains("html"))
                    return false;
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using var fileStream = File.Create(Path.Combine(destFolder, fileName));
                respStream.CopyTo(fileStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_LABEL_BODY =
            "cbox=$GROUP&" +
            "__RequestVerificationToken=$TOKEN&" +
            "cbPlace=false";
        public bool DownloadLabels(string destFolder, string fileName, string group)
        {
            if (!this.Initialize())
                return false;

            try
            {
                var postData = DOWNLOAD_LABEL_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/libelles", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                    return false;
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using var fileStream = File.Create(Path.Combine(destFolder, fileName));
                respStream.CopyTo(fileStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_INTRADAY_BODY =
            "sformat=ex&" +
            "splace=$GROUP&" +
            "__RequestVerificationToken=$TOKEN";

        public bool DownloadGroupIntraday(string destFolder, string fileName, string group)
        {
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_INTRADAY_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/telechargement_intraday", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                    return false;
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using var fileStream = File.Create(Path.Combine(destFolder, fileName));
                respStream.CopyTo(fileStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion

        public string UserConfigFileName => CONFIG_FILE;

        private static StockDictionary stockDictionary = null;

        public override bool SupportsIntradayDownload => Settings.Default.SupportIntraday;

        static readonly string defaultConfigFileContent = "ISIN;NOM;SICOVAM;TICKER;GROUP" + Environment.NewLine + "FR0003500008;CAC40;;CAC40;INDICES";

        static string configPath => Path.Combine(Folders.PersonalFolder, "AbcDownloadConfig.txt");

        static List<ABCDownloadGroup> downloadGroups = null;

        public override void InitDictionary(StockDictionary dictionary, bool download)
        {
            download = false;
            stockDictionary = dictionary;
            CreateDirectories();

            #region init group config file
            if (!File.Exists(configPath))
            {
                MessageBox.Show("The default ABC configuration file is missing.", "ABC Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            downloadGroups = JsonSerializer.Deserialize<List<ABCDownloadGroup>>(File.ReadAllText(configPath), new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
            #endregion


            // Intialize Groups
            foreach (var config in downloadGroups)
            {
                if (InitAbcGroup(config, download))
                {
                    config.LastDownload = DateTime.Now;
                }
            }

            // Load Config files
            string fileName = Path.Combine(Folders.PersonalFolder, UserConfigFileName);
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, defaultConfigFileContent);
            }
            InitFromFile(download, fileName);

            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadPath = System.IO.Path.Combine(downloadPath, "Downloads");
            var dataFile = Directory.EnumerateFiles(downloadPath, "Cotations*.csv").OrderBy(f => File.GetCreationTime(f));

            foreach (var file in dataFile)
            {
                LoadDataFromCotations(file);
            }

            LoadDataFromSeance();

            // Save download Config
            if (!download)
                return;

            var json = JsonSerializer.Serialize(downloadGroups, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = { new JsonStringEnumConverter() }
            });

            File.WriteAllText(configPath, json);

            // For analysis only
            foreach (var g in dictionary.Values.Where(s => s.DataProvider == StockDataProvider.ABC).GroupBy(s => s.StockGroup))
            {
                StockLog.Write($"Group: {g.Key} prefix: {g.Select(s => s.ISIN.Substring(0, 2)).Distinct().Aggregate((i, j) => i + " " + j)}");
            }
        }

        private void LoadDataFromWebCache()
        {
            string downloadPath = DataFolder + ABC_TMP_CACHE_FOLDER;
            var dataFiles = Directory.EnumerateFiles(downloadPath, "*.csv").OrderByDescending(f => File.GetCreationTime(f)).OrderBy(f => f).GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('_')[0]);

            foreach (var fileGroup in dataFiles)
            {
                NotifyProgress($"Processing {Path.GetFileNameWithoutExtension(fileGroup.Key)}");
                var lines = fileGroup.SelectMany(f => File.ReadAllLines(f));
                LoadDataFromWebCache(lines);
            }
        }
        private void LoadDataFromWebCache(IEnumerable<string> lines)
        {
            var historyFileName = Path.Combine(DataFolder + ABC_DAILY_CFG_FOLDER, "DownloadHistory.txt");
            if (downloadHistory == null)
            {
                downloadHistory = AbcDownloadHistory.Load(historyFileName);
            }

            var splitLines = lines.Select(l => l.Split(';'));
            foreach (var serieData in splitLines.GroupBy(l => l[0]))
            {
                var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == serieData.Key);
                if (stockSerie == null)
                    continue;

                var history = downloadHistory.FirstOrDefault(h => h.Id == stockSerie.ISIN);
                if (history == null)
                {
                    this.LoadFromCSV(stockSerie, false);

                    history = new AbcDownloadHistory(stockSerie.ISIN, stockSerie.Count > 0 ? stockSerie.Values.Last().DATE : DateTime.MinValue, stockSerie.StockName, stockSerie.StockGroup.ToString());
                    downloadHistory.Add(history);
                }

                var dailyValues = serieData.Where(row => DateTime.Parse(row[1], frenchCulture) > history.LastDate).Select(row => new StockDailyValue(
                    float.Parse(row[2], CultureInfo.InvariantCulture),
                    float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture),
                    float.Parse(row[5], CultureInfo.InvariantCulture),
                    long.Parse(row[6], frenchCulture),
                    DateTime.Parse(row[1], frenchCulture))).OrderBy(d => d.DATE).ToList();

                if (dailyValues.Count > 0)
                {
                    if (stockSerie.Count == 0)
                        this.LoadFromCSV(stockSerie, false);

                    foreach (var dailyValue in dailyValues)
                    {
                        stockSerie.Add(dailyValue.DATE, dailyValue);
                        history.LastDate = dailyValue.DATE;
                    }
                    this.SaveToCSV(stockSerie, false); // Not true need to detect if need to save archive
                }
                stockSerie.IsInitialised = false;
            }

            AbcDownloadHistory.Save(historyFileName, downloadHistory);
        }


        private void LoadDataFromCotations()
        {
            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadPath = System.IO.Path.Combine(downloadPath, "Downloads");
            var dataFile = Directory.EnumerateFiles(downloadPath, "Cotations*.csv").OrderByDescending(f => File.GetCreationTime(f)).FirstOrDefault();

            if (string.IsNullOrEmpty(null) || !File.Exists(dataFile))
                return;

            LoadDataFromCotations(dataFile);
        }
        List<AbcDownloadHistory> downloadHistory;
        private void LoadDataFromCotations(string dataFile)
        {
            var historyFileName = Path.Combine(DataFolder + ABC_DAILY_CFG_FOLDER, "DownloadHistory.txt");
            if (downloadHistory == null)
            {
                downloadHistory = AbcDownloadHistory.Load(historyFileName);
            }

            var lines = File.ReadAllLines(dataFile).Select(l => l.Split(';'));
            foreach (var serieData in lines.GroupBy(l => l[0]))
            {
                var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == serieData.Key);
                if (stockSerie == null)
                    continue;

                var history = downloadHistory.FirstOrDefault(h => h.Id == stockSerie.ISIN);
                if (history == null)
                {
                    this.LoadFromCSV(stockSerie, false);

                    history = new AbcDownloadHistory(stockSerie.ISIN, stockSerie.Count > 0 ? stockSerie.Values.Last().DATE : DateTime.MinValue, stockSerie.StockName, stockSerie.StockGroup.ToString());
                    downloadHistory.Add(history);
                }

                var dailyValues = serieData.Where(row => DateTime.Parse(row[1], frenchCulture) > history.LastDate).Select(row => new StockDailyValue(
                    float.Parse(row[2], CultureInfo.InvariantCulture),
                    float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture),
                    float.Parse(row[5], CultureInfo.InvariantCulture),
                    long.Parse(row[6], frenchCulture),
                    DateTime.Parse(row[1], frenchCulture))).OrderBy(d => d.DATE).ToList();

                if (dailyValues.Count > 0)
                {
                    if (stockSerie.Count == 0)
                        this.LoadFromCSV(stockSerie, false);

                    foreach (var dailyValue in dailyValues)
                    {
                        stockSerie.Add(dailyValue.DATE, dailyValue);
                        history.LastDate = dailyValue.DATE;
                    }
                    this.SaveToCSV(stockSerie, false);
                }
                stockSerie.IsInitialised = false;
            }

            File.Delete(dataFile);

            AbcDownloadHistory.Save(historyFileName, downloadHistory);
        }

        private void LoadDataFromSeance()
        {
            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadPath = System.IO.Path.Combine(downloadPath, "Downloads");
            var dataFiles = Directory.EnumerateFiles(downloadPath, "Seance*.csv").OrderByDescending(f => File.GetCreationTime(f));

            foreach (var dataFile in dataFiles)
            {
                var date = File.GetLastWriteTime(dataFile);
                if (date.Date == DateTime.Today)
                {
                    var lines = File.ReadAllLines(dataFile).Select(l => l.Split(';'));
                    foreach (var row in lines)
                    {
                        var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == row[0]);
                        if (stockSerie == null)
                            continue;

                        if (stockSerie.Count > 0)
                            stockSerie.IsInitialised = false;

                        this.LoadFromCSV(stockSerie);
                        if (stockSerie.Count == 0)
                            continue;

                        var dailyValue = new StockDailyValue(
                        float.Parse(row[2], frenchCulture),
                        float.Parse(row[3], frenchCulture),
                        float.Parse(row[4], frenchCulture),
                        float.Parse(row[5], frenchCulture),
                        long.Parse(row[6], frenchCulture),
                        date);

                        stockSerie.Add(date, dailyValue);
                    }
                }
                File.Delete(dataFile);
            }
        }

        private bool InitAbcGroup(ABCDownloadGroup config, bool download)
        {
            var destFolder = config.LabelOnly ? DataFolder + ABC_DAILY_CFG_GROUP_FOLDER : DataFolder + ABC_DAILY_CFG_FOLDER;
            string fileName = Path.Combine(destFolder, config.Group.ToString() + ".txt");

            if (!config.LabelOnly)
            {
                InitFromLibelleFile(config, fileName);

                if (download)
                {
                    return config.LastDownloaded < lastDownloadedCAC40Date ? DownloadGroupFromAbc(config) : false;
                }
                else
                    return false;
            }
            else
            {
                groupSeries.Add(config.Group, null);
            }
            if (config.Group == StockSerie.Groups.SRD || config.Group == StockSerie.Groups.SRD_LO)
            {
                InitSRDFromLibelleFile(fileName, config.Group);
            }

            return true;
        }

        private bool DownloadGroupFromAbc(ABCDownloadGroup config)
        {
            var startDate = config.LastDownloaded.Year < ARCHIVE_START_YEAR ? new DateTime(ARCHIVE_START_YEAR, 5, 1) : config.LastDownloaded.AddDays(1);

            if (stockDictionary.ContainsKey(config.RefSerie))
            {
                var refSerie = stockDictionary[config.RefSerie];

                DownloadMonthlyFileFromABC(DataFolder + ABC_TMP_FOLDER, startDate, DateTime.Today, config);

                refSerie.Initialise();
                if (config.LastDownloaded < refSerie.LastValue.DATE)
                {
                    // Force Report Generation
                    var fileName = Path.Combine(Folders.Report, "LastGeneration.txt");
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    config.LastDownloaded = refSerie.LastValue.DATE;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"Reference serie:{config.RefSerie} not found for group {config.Group}", "Download configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void InitFromSector(SectorCode sector)
        {
            var longName = "_" + sector.Sector;
            if (!stockDictionary.ContainsKey(longName))
            {
                // Set SectorId to stock
                string fileName = DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER + @"\" + sector.Code.ToString() + ".txt";
                if (File.Exists(fileName))
                {
                    using StreamReader sr = new StreamReader(fileName, true);
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] fields = line.Split(';');
                            var stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == fields[0]);
                            if (stockSerie != null)
                            {
                                stockSerie.SectorId = sector.Code;
                            }
                        }
                    }
                }
            }
        }

        public static void CreateDirectories()
        {
            if (!Directory.Exists(Folders.AgendaFolder))
            {
                Directory.CreateDirectory(Folders.AgendaFolder);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_INTRADAY_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_GROUP_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_GROUP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(DataFolder + ABC_INTRADAY_FOLDER))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
            if (!Directory.Exists(DataFolder + ABC_WEB_CACHE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_WEB_CACHE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_TMP_CACHE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_TMP_CACHE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_TMP_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_TMP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
        }
        private void InitFromLibelleFile(ABCDownloadGroup config, string fileName)
        {
            if (File.Exists(fileName))
            {
                Groups group = (Groups)Enum.Parse(typeof(Groups), Path.GetFileNameWithoutExtension(fileName));
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line) && IsinMatchGroup(config, line))
                    {
                        string[] row = line.Split(';');
                        string stockName = row[1].ToUpper().Replace(",", " "); // .Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");
                        if (!stockDictionary.ContainsKey(stockName))
                        {
                            var existingInstrument = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0]);
                            if (existingInstrument != null)
                            {
                                StockLog.Write($"Duplicate ISIN {row[0]}:{group}:{stockName} already listed from {existingInstrument.StockGroup}:{existingInstrument.StockName}");
                                continue;
                            }
                            StockSerie stockSerie = new StockSerie(stockName, row[2], row[0], group, StockDataProvider.ABC, BarDuration.Daily);

                            var abcSuffix = group switch
                            {
                                StockSerie.Groups.EURO_A => "p",
                                StockSerie.Groups.EURO_B => "p",
                                StockSerie.Groups.EURO_C => "p",
                                StockSerie.Groups.ALTERNEXT => "p",
                                StockSerie.Groups.FOREX => "p",

                                StockSerie.Groups.BELGIUM => "g",
                                StockSerie.Groups.HOLLAND => "n",
                                StockSerie.Groups.PORTUGAL => "l",
                                StockSerie.Groups.ITALIA => "i",
                                StockSerie.Groups.GERMANY => "f",
                                StockSerie.Groups.SPAIN => "m",
                                StockSerie.Groups.USA => "u",
                                _ => string.Empty
                            };

                            stockSerie.ABCName = stockSerie.Symbol + abcSuffix;
                            stockDictionary.Add(stockName, stockSerie);
                        }
                        else
                        {
                            StockLog.Write("Duplicate " + group + ";" + line + " already in group " + stockDictionary[stockName].StockGroup);
                        }
                    }
                }
            }
            else
            {
                StockLog.Write("File does not exist");
            }
        }
        private void InitSRDFromLibelleFile(string fileName, Groups group)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] row = line.Split(';');
                        string stockName = row[1].ToUpper().Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");
                        if (stockDictionary.ContainsKey(stockName))
                        {
                            if (group == StockSerie.Groups.SRD)
                            {
                                stockDictionary[stockName].SRD = true;
                            }
                            if (group == StockSerie.Groups.SRD_LO)
                            {
                                stockDictionary[stockName].SRD_LO = true;
                            }
                        }
                    }
                }
            }
            else
            {
                StockLog.Write("File does not exist");
            }
        }

        private bool IsinMatchGroup(ABCDownloadGroup group, string line)
        {
            if (group.Prefixes != null && group.Prefixes.Length > 0)
            {
                return group.Prefixes.Any(prefix => line.StartsWith(prefix));
            }
            return true;
        }

        private void InitFromFile(bool download, string fileName)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] row = line.Split(';');
                        StockSerie stockSerie = new StockSerie(row[1], string.IsNullOrEmpty(row[2]) ? row[3] : row[2], row[0], (Groups)Enum.Parse(typeof(Groups), row[4]), StockDataProvider.ABC, BarDuration.Daily);
                        stockSerie.ABCName = stockSerie.Symbol + stockSerie.ISIN?.Substring(0, 2) switch
                        {
                            null => string.Empty,
                            "FR" => "p",
                            "QS" => "p",
                            "BE" => "g",
                            "NL" => "n",
                            "DE" => "f",
                            "IT" => "i",
                            "ES" => "m",
                            "PT" => "I",
                            _ => string.Empty
                        };
                        if (!stockDictionary.ContainsKey(row[1]))
                        {
                            stockDictionary.Add(row[1], stockSerie);
                        }
                        else
                        {
                            StockLog.Write("ABC Entry: " + row[1] + " already in stockDictionary");
                        }
                        if (download)
                        {
                            NotifyProgress($"Downloading {stockSerie.StockName}");
                            this.DownloadABCData(stockSerie);
                        }
                    }
                }
            }
        }

        static string loadingGroup = null;

        public override bool LoadData(StockSerie stockSerie)
        {
            bool res = false;
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName + " - " + stockSerie.Count);

            // Read from CSV files Archive + current year.
            if (stockSerie.Count == 0)
            {
                res = this.LoadFromCSV(stockSerie);
            }

            // Load data that just has been downloaded
            string abcGroup = GetABCGroup(stockSerie.StockGroup);
            if (abcGroup != null)
            {
                // Group data is available only after download
                LoadGroupData(abcGroup, stockSerie.StockGroup);
            }
            else
            {
                // Daily value is available only after download
                string fileName = stockSerie.ISIN + "_" + stockSerie.Symbol + "_" + stockSerie.StockGroup.ToString() + ".csv";
                res |= ParseCSVFile(stockSerie, Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName));
            }

            this.ApplySplit(stockSerie);

            return res;
        }
        private void LoadGroupData(string abcGroup, Groups stockGroup)
        {
            StockLog.Write("Group: " + abcGroup);
            try
            {
                #region Multi thread synchro
                if (loadingGroup == null)
                {
                    loadingGroup = abcGroup;
                }
                else
                {
                    StockLog.Write("Already busy loading group: " + loadingGroup);
                    if (loadingGroup == abcGroup)
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (loadingGroup == abcGroup);
                        return;
                    }
                    else
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (loadingGroup == abcGroup);
                    }
                }
                #endregion

                string filePattern = abcGroup + "_*.csv";
                bool dataLoaded = false;
                foreach (string currentFile in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER, filePattern).OrderBy(s => s))
                {
                    NotifyProgress($"Loading {Path.GetFileNameWithoutExtension(currentFile)}");
                    ParseABCGroupCSVFile(currentFile, stockGroup);
                    File.Delete(currentFile);
                    dataLoaded = true;
                }
                var fileName = Path.Combine(DataFolder + ABC_INTRADAY_FOLDER, abcGroup + ".csv");
                if (File.Exists(fileName))
                {
                    ParseABCGroupCSVFile(fileName, stockGroup, true);
                    File.Delete(fileName);
                    dataLoaded = true;
                }
                // Save to CSV file
                if (dataLoaded)
                {
                    NotifyProgress($"Saving files for {stockGroup}");
                    foreach (var serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockGroup) && s.Count > 0))
                    {
                        this.SaveToCSV(serie, false);
                        this.ApplySplit(serie);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            finally
            {
                loadingGroup = null;
            }
        }
        private static string GetABCGroup(Groups stockGroup)
        {
            var groupConfig = downloadGroups.FirstOrDefault(g => g.Group == stockGroup);
            //if (groupConfig == null)
            //{
            //    MessageBox.Show($"Group {stockGroup} not defined in configuration file", "Download configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return null;
            //}

            return groupConfig?.AbcCode;


            //string abcGroup = null;
            //switch (stockGroup)
            //{
            //    case StockSerie.Groups.EURO_A:
            //        abcGroup = "eurolistap";
            //        break;
            //    case StockSerie.Groups.EURO_B:
            //        abcGroup = "eurolistbp";
            //        break;
            //    case StockSerie.Groups.EURO_C:
            //        abcGroup = "eurolistcp";
            //        break;
            //    case StockSerie.Groups.ALTERNEXT:
            //        abcGroup = "eurogp";
            //        break;
            //    case StockSerie.Groups.SECTORS_CAC:
            //        abcGroup = "indicessecp";
            //        break;
            //    case StockSerie.Groups.BELGIUM:
            //        abcGroup = "belg";
            //        break;
            //    case StockSerie.Groups.HOLLAND:
            //        abcGroup = "holln";
            //        break;
            //    case StockSerie.Groups.PORTUGAL:
            //        abcGroup = "lisboal";
            //        break;
            //    case StockSerie.Groups.CAC40:
            //        abcGroup = "xcac40p";
            //        break;
            //    case StockSerie.Groups.SBF120:
            //        abcGroup = "xsbf120p";
            //        break;
            //    case StockSerie.Groups.CAC_AT:
            //        abcGroup = "xcacatp";
            //        break;
            //    case StockSerie.Groups.USA:
            //        abcGroup = "usau";
            //        break;
            //    case StockSerie.Groups.SPAIN:
            //        abcGroup = "spainm";
            //        break;
            //    case StockSerie.Groups.ITALIA:
            //        abcGroup = "italiai";
            //        break;
            //    case StockSerie.Groups.GERMANY:
            //        abcGroup = "germanyf";
            //        break;
            //    //case StockSerie.Groups.NASDAQ:
            //    //    abcGroup = "nasu";
            //    //    break;
            //    case StockSerie.Groups.SRD:
            //        abcGroup = "srdp";
            //        break;
            //    case StockSerie.Groups.SRD_LO:
            //        abcGroup = "srdlop";
            //        break;
            //    default:
            //        StockLog.Write($"StockGroup {stockGroup} is not supported in ABC Bourse");
            //        break;
            //}
            //return abcGroup;
        }
        private bool ParseABCGroupCSVFile(string fileName, Groups group, bool intraday = false)
        {
            if (!File.Exists(fileName)) return false;
            StockSerie stockSerie = null;
            using StreamReader sr = new StreamReader(fileName, true);
            string previousISIN = string.Empty;
            DateTime date = File.GetLastWriteTime(fileName);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Replace(",", ".");
                try
                {
                    string[] row = line.Split(';');
                    if (previousISIN != row[0])
                    {
                        stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0] && s.StockGroup == group);
                        if (stockSerie == null)
                            continue;
                        previousISIN = row[0];
                        if (stockSerie.Count == 0)
                        {
                            this.LoadFromCSV(stockSerie);
                        }
                    }
                    if (stockSerie != null)
                    {
                        if (intraday)
                        {
                            if (DateTime.Parse(row[1]) != DateTime.Today)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            date = DateTime.Parse(row[1]);
                        }
                        if (date.Year >= LOAD_START_YEAR)
                        {
                            if (!stockSerie.ContainsKey(date))
                            {
                                StockDailyValue dailyValue = new StockDailyValue(
                                  float.Parse(row[2]),
                                  float.Parse(row[3]),
                                  float.Parse(row[4]),
                                  float.Parse(row[5]),
                                  long.Parse(row[6]),
                                  date);
                                dailyValue.IsComplete = !intraday;
                                stockSerie.Add(dailyValue.DATE, dailyValue);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    StockLog.Write(line);
                    StockLog.Write(e);
                    return false;
                }
            }
            return true;
        }
        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            string filePattern = stockSerie.ISIN + "_" + stockSerie.Symbol + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
            string fileName;
            StockLog.Write(stockSerie.StockName + " " + stockSerie.ISIN);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                StockLog.Write("Network is Available");
                stockSerie.IsInitialised = false;
                int nbFile = 0;
                var isin = stockSerie.ISIN;
                if (stockSerie.StockGroup == StockSerie.Groups.USA)
                    isin += "u";
                for (int i = DateTime.Today.Year - 1; i >= ARCHIVE_START_YEAR; i--)
                {
                    fileName = filePattern.Replace("*", i.ToString());
                    if (!this.DownloadISIN(DataFolder + ABC_TMP_FOLDER, fileName, new DateTime(i, 1, 1), new DateTime(i, 12, 31), isin))
                    {
                        break;
                    }
                    nbFile++;
                }
                int year = DateTime.Today.Year;
                fileName = filePattern.Replace("*", year.ToString());
                if (this.DownloadISIN(DataFolder + ABC_TMP_FOLDER, fileName, new DateTime(year, 1, 1), DateTime.Today, isin))
                {
                    nbFile++;
                }
                if (nbFile == 0)
                    return false;

                // Parse loaded files
                foreach (var csvFileName in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER, filePattern).OrderBy(f => f))
                {
                    using (StreamReader sr = new StreamReader(csvFileName))
                    {
                        StockDailyValue readValue = null;
                        while (!sr.EndOfStream)
                        {
                            readValue = this.ReadMarketDataFromABCCSVStream(sr, stockSerie.StockName, true);
                            if (readValue != null)
                            {
                                stockSerie.Add(readValue.DATE, readValue);
                            }
                        }
                    }
                    File.Delete(csvFileName);
                }

                this.SaveToCSV(stockSerie, true);
                return true;
            }
            else
            {
                StockLog.Write("Network is not Available");
                return false;
            }
        }

        DateTime lastDownloadedCAC40Date = DateTime.MaxValue;
        bool happyNewMonth = false;
        public bool DownloadABCData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadABCData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return false;

            this.LoadData(stockSerie);
            if (stockSerie.Count == 0)
            {
                return ForceDownloadData(stockSerie);
            }

            // Check if up to date
            var lastLoadedDate = stockSerie.Keys.Last();
            if (stockSerie.StockName != "CAC40" && lastLoadedDate >= lastDownloadedCAC40Date)
                return true;

            var fileName = stockSerie.ISIN + "_" + stockSerie.Symbol + "_" + stockSerie.StockGroup.ToString() + ".csv";
            if (this.DownloadISIN(DataFolder + ABC_TMP_FOLDER, fileName, lastLoadedDate.AddDays(1), DateTime.Today, stockSerie.ISIN))
            {
                stockSerie.IsInitialised = false;

                this.LoadData(stockSerie);

                if (stockSerie.StockName == "CAC40")
                {
                    lastDownloadedCAC40Date = stockSerie.Keys.Last();
                    happyNewMonth = lastDownloadedCAC40Date.Month != DateTime.Today.Month || lastDownloadedCAC40Date.Month != lastLoadedDate.Month;
                }

                this.SaveToCSV(stockSerie, happyNewMonth);
                File.Delete(Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName));
            }
            else // Failed loading data, could be because data is up to date.
            {
                if (stockSerie.StockName == "CAC40")
                {
                    lastDownloadedCAC40Date = stockSerie.Keys.Last();
                    happyNewMonth = false;
                }
            }

            return true;
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                this.LoadData(stockSerie);
                if (stockSerie.Count == 0)
                {
                    return ForceDownloadData(stockSerie);
                }
            }

            return stockSerie.Initialise();
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadIntradayData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());

                if (!stockSerie.Initialise() || stockSerie.Count == 0)
                {
                    return false;
                }

                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday || stockSerie.Keys.Last().Date == DateTime.Today)
                {
                    return false;
                }

                string abcGroup = GetABCGroup(stockSerie.StockGroup);
                if (abcGroup != null)
                {
                    var destFolder = DataFolder + ABC_INTRADAY_FOLDER;
                    string fileName = abcGroup + ".csv";
                    if (this.DownloadIntradayGroup(destFolder, fileName, abcGroup))
                    {
                        // Deinitialise all the stocks belonging to group
                        foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup)))
                        {
                            serie.IsInitialised = false;
                            serie.ClearBarDurationCache();
                        }
                    }
                }
            }
            return true;
        }

        private String downloadingGroups = String.Empty;
        TimeSpan nextDownload = new TimeSpan(9, 1, 0);
        public void DownloadAllGroupsIntraday()
        {
            StockLog.Write(string.Empty);
            try
            {
                if (IntradayDownloadSuspended)
                    return;
                var now = DateTime.Now.TimeOfDay;
                if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday || now.Hours > 18 || now.Hours < 9)
                    return;
                while (downloadingGroups != String.Empty)
                {
                    Thread.Sleep(500);
                }
                lock (downloadingGroups)
                {
                    // Download ABC intraday data
                    if (now < nextDownload)
                    {
                        StockLog.Write("Up To Date");
                        return;
                    }
                    int minutes = ((int)now.TotalMinutes / 5) * 5;
                    nextDownload = TimeSpan.FromMinutes(minutes + 5);

                    downloadingGroups = "True";
                    var groups = new Groups[] {
                        StockSerie.Groups.BELGIUM, StockSerie.Groups.HOLLAND, StockSerie.Groups.PORTUGAL,
                        StockSerie.Groups.ITALIA, StockSerie.Groups.GERMANY, StockSerie.Groups.SPAIN, StockSerie.Groups.USA,
                        StockSerie.Groups.EURO_A, StockSerie.Groups.EURO_B, StockSerie.Groups.EURO_C, StockSerie.Groups.ALTERNEXT };

                    foreach (var group in groups)
                    {
                        string abcGroup = GetABCGroup(group);
                        if (abcGroup != null)
                        {
                            var destFolder = DataFolder + ABC_INTRADAY_FOLDER;
                            string fileName = abcGroup + ".csv";
                            if (this.DownloadIntradayGroup(destFolder, fileName, abcGroup))
                            {
                                // Deinitialise all the stocks belonging to group
                                foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(group)))
                                {
                                    using (new StockSerieLocker(serie))
                                    {
                                        serie.IsInitialised = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
            }
            finally
            {
                downloadingGroups = String.Empty;
            }
        }

        /// <summary>
        /// Parse ABC file
        /// </summary>
        /// <param name="stockSerie"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected override bool ParseCSVFile(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName);
                StockDailyValue readValue = null;
                while (!sr.EndOfStream)
                {
                    readValue = this.ReadMarketDataFromABCCSVStream(sr, stockSerie.StockName, true);
                    if (readValue != null && readValue.DATE.Year >= LOAD_START_YEAR)
                    {
                        if (!stockSerie.ContainsKey(readValue.DATE))
                        {
                            stockSerie.Add(readValue.DATE, readValue);
                        }
                        else
                        { // 
                            StockLog.Write("The dailyValue already exist in the serie");
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        protected StockDailyValue ReadMarketDataFromABCCSVStream(StreamReader sr, string stockName, bool useAdjusted)
        {
            StockDailyValue stockValue = null;
            try
            {
                // File format
                // ISIN,Date,Open,High,Low,Close,Volume
                // FR0000120404;02/01/12;19.735;20.03;19.45;19.94;418165
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("<") || line.StartsWith("/")) return null;
                string[] row = line.Split(';');
                if (row.Length < 7)
                    return null;
                stockValue = new StockDailyValue(
                float.Parse(row[2], frenchCulture),
                float.Parse(row[3], frenchCulture),
                float.Parse(row[4], frenchCulture),
                float.Parse(row[5], frenchCulture),
                long.Parse(row[6], frenchCulture),
                DateTime.Parse(row[1], frenchCulture));
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
            }
            return stockValue;
        }




        public static List<SectorCode> SectorCodes = new List<SectorCode>
        {
                new SectorCode(45,"Biens de consommation"),
                new SectorCode(50,"Industries"),
                new SectorCode(35,"Immobilier"),
                new SectorCode(55,"Materiaux de base"),
                new SectorCode(60,"Petrole et gaz"),
                new SectorCode(20,"Sante"),
                new SectorCode(40,"Services aux consommateurs"),
                new SectorCode(65,"Services aux collectivites"),
                new SectorCode(30,"Societes financieres"),
                new SectorCode(15,"Telecommunications"),
                new SectorCode(10,"Technologie")
        };
        private bool DownloadSectorFromABC(string destFolder, int sector)
        {
            bool success = true;

            string fileName = destFolder + @"\" + sector + ".txt";
            if (File.Exists(fileName))
            {
                if (File.GetLastWriteTime(fileName) > DateTime.Now.AddDays(-7)) // File has been updated during the last 7 days
                    return true;
            }
            if (!this.Initialize())
                return false;

            try
            {
                // Send POST request
                string url = $"/api/General/DownloadSector?sectorCode={sector}";
                var resp = httpClient.GetAsync(url).Result;
                if (!resp.IsSuccessStatusCode)
                    return false;
                using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                using var fileStream = File.Create(Path.Combine(destFolder, fileName));
                respStream.CopyTo(fileStream);
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed loading sectors");
                success = false;
            }
            return success;
        }

        private bool DownloadMonthlyFileFromABC(string destFolder, DateTime startDate, DateTime endDate, ABCDownloadGroup group, bool loadData = true)
        {
            bool success = true;

            NotifyProgress($"Downloading data for {group.Group} from {startDate.ToShortDateString()}");
            try
            {
                while (endDate - startDate >= new TimeSpan(31, 0, 0, 0))
                {
                    var endOfMonth = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
                    DownloadMonthlyFileFromABC(destFolder, startDate, endOfMonth, group, false);
                    startDate = endOfMonth.AddDays(1);
                }

                string fileName = destFolder + @"\" + group.AbcCode + "_" + endDate.Year + "_" + endDate.Month.ToString("0#") + ".csv";
                if (this.DownloadGroup(fileName, startDate, endDate, group.AbcCode))
                {
                    if (loadData)
                    {
                        this.LoadGroupData(group.AbcCode, group.Group);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                MessageBox.Show(ex.Message, "Connection failed");
                success = false;
            }
            return success;
        }
        private bool SaveResponseToFile(string fileName, HttpWebRequest req)
        {
            bool success = true;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                // Get the stream containing content returned by the server.
                using Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                using StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                if (responseFromServer.Length != 0 && !responseFromServer.StartsWith("<", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Save content to file
                    using StreamWriter writer = new StreamWriter(fileName);
                    writer.Write(responseFromServer);
                    //StockLog.Write("Download succeeded: " + fileName);
                }
                else
                {
                    success = false;
                }
            }
            return success;
        }

        #region IConfigDialog Implementation
        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            EuronextDataProviderConfigDlg configDlg = new EuronextDataProviderConfigDlg(stockDico) { StartPosition = FormStartPosition.CenterScreen };
            return configDlg.ShowDialog();
        }

        public override string DisplayName => "ABCBourse";
        #endregion

        static SortedDictionary<Groups, List<string>> groupSeries = new SortedDictionary<Groups, List<string>>();

        public static bool BelongsToGroup(StockSerie stockSerie, Groups group)
        {
            if (group == stockSerie.StockGroup || group == StockSerie.Groups.ALL_STOCKS)
                return true;

            switch (group)
            {
                case Groups.EURO_A_B:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B;
                case Groups.EURO_A_B_C:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C;
                case Groups.CACALL:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT;
                case Groups.PEA_EURONEXT:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT
                        || stockSerie.StockGroup == Groups.BELGIUM || stockSerie.StockGroup == Groups.HOLLAND || stockSerie.StockGroup == Groups.PORTUGAL;
                case Groups.PEA:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT
                        || stockSerie.StockGroup == Groups.BELGIUM || stockSerie.StockGroup == Groups.HOLLAND || stockSerie.StockGroup == Groups.PORTUGAL
                        || stockSerie.StockGroup == Groups.ITALIA || stockSerie.StockGroup == Groups.GERMANY || stockSerie.StockGroup == Groups.SPAIN;
            }

            if (!groupSeries.ContainsKey(group))
                return false;

            var groupList = groupSeries[group];
            if (groupList == null)
            {
                groupList = new List<string>();

                // parse group definition
                string fileName = DataFolder + @"\" + ABC_DAILY_CFG_GROUP_FOLDER + $@"\{group}.txt";
                if (File.Exists(fileName))
                {
                    using StreamReader sr = new StreamReader(fileName, true);
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(';');
                            groupList.Add(row[1].ToUpper());
                        }
                    }
                }
                else
                {
                    MessageBox.Show("ABD DataProvider Group error", $"Group definition file not found for Group: {group}");
                }
            }

            return groupList != null && groupList.Contains(stockSerie.StockName);
        }

        public static void DownloadAgenda(StockSerie stockSerie)
        {
            if (!stockSerie.BelongsToGroup(StockSerie.Groups.CACALL)) return;
            if (stockSerie.Agenda == null)
            {
                stockSerie.Agenda = new StockAgenda();
            }
            else
            {
                if (stockSerie.Agenda.DownloadDate.AddMonths(1) > DateTime.Today) return;
            }

            string url = $"http://www.abcbourse.com/marches/events.aspx?s={stockSerie.ABCName}";

            StockWebHelper swh = new StockWebHelper();
            string html = swh.DownloadHtml(url, Encoding.UTF8);

            WebBrowser browser = new WebBrowser();
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentText = html;
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();

            HtmlDocument doc = browser.Document;

            HtmlElementCollection tables = doc.GetElementsByTagName("table");
            List<List<string>> data = new List<List<string>>();

            foreach (HtmlElement tbl in tables)
            {
                if (tbl.InnerText.StartsWith("Date"))
                {
                    data = getTableData(tbl).Skip(1).ToList();
                    break;
                }
            }
            //
            foreach (var row in data)
            {
                if (row[0].StartsWith("du")) row[0] = row[0].Substring(row[0].IndexOf("au ") + 3);
                DateTime date = DateTime.Parse(row[0]);

                if (!stockSerie.Agenda.ContainsKey(date))
                {
                    stockSerie.Agenda.Add(date, row[1], row[2]);
                }
            }
            stockSerie.Agenda.DownloadDate = DateTime.Today;
            stockSerie.Agenda.SortDescending();
            stockSerie.SaveAgenda();
        }

        static private List<List<string>> getTableData(HtmlElement tbl)
        {
            List<List<string>> data = new List<List<string>>();

            HtmlElementCollection rows = tbl.GetElementsByTagName("tr");
            HtmlElementCollection cols; // = rows.GetElementsByTagName("th");
            foreach (HtmlElement tr in rows)
            {
                List<string> row = new List<string>();
                cols = tr.GetElementsByTagName("th");
                foreach (HtmlElement td in cols)
                {
                    row.Add(WebUtility.HtmlDecode(td.InnerText));
                }
                cols = tr.GetElementsByTagName("td");
                foreach (HtmlElement td in cols)
                {
                    row.Add(WebUtility.HtmlDecode(td.InnerText));
                }
                if (row.Count > 0) data.Add(row);
            }

            return data;
        }

        #region Persistency
        const string DATEFORMAT = "dd/MM/yyyy";
        public void SaveToCSV(StockSerie stockSerie, bool forceArchive = true)
        {
            if (stockSerie.Values.Count() == 0)
                return;
            string fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            var lastDate = stockSerie.Keys.Last();
            var pivotDate = new DateTime(lastDate.Year, lastDate.Month, 1).AddDays(-1);
            if (forceArchive || !File.Exists(fileName))
            {
                if (stockSerie.Values.Any(v => v.DATE <= pivotDate))
                {
                    using StreamWriter sw = new StreamWriter(fileName);
                    foreach (var value in stockSerie.Values.Where(v => v.DATE <= pivotDate))
                    {
                        sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                    }
                }
            }

            fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (var value in stockSerie.Values.Where(v => v.DATE > pivotDate && v.IsComplete))
                {
                    sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                }
            }
        }

        public bool LoadFromCSV(StockSerie stockSerie, bool loadArchive = true)
        {
            StockLog.Write($"Serie: {stockSerie.StockName}");
            bool result = false;
            string fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            var lastArchiveDate = DateTime.MinValue;
            if (loadArchive && File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        DateTime date = DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                        var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), date);
                        stockSerie.Add(value.DATE, value);
                    }
                }
                result = true;
                if (stockSerie.Count > 0)
                    lastArchiveDate = stockSerie.Keys.Last();
            }
            fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                DateTime date = DateTime.MinValue;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        date = DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                        if (date > lastArchiveDate)
                        {
                            var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), date);
                            stockSerie.Add(value.DATE, value);
                        }
                    }
                }

                if (loadArchive && lastArchiveDate < new DateTime(date.Year, date.Month, 1).AddMonths(-1))
                {
                    SaveToCSV(stockSerie, true);
                }
                result = true;
            }
            return result;
        }
        #endregion

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            if (stockSerie.ABCName != null)
            {
                string url = $"https://www.abcbourse.com/graphes/display.aspx?s={stockSerie.ABCName}";
                Process.Start(url);
            }
        }

        public override void AddSplit(StockSerie stockSerie, DateTime date, float before, float after)
        {
            if (!stockSerie.Initialise())
                return;

            var split = new StockSplit() { StockName = stockSerie.StockName, Date = date, Before = before, After = after };
            StockSplit.Splits.Add(split);
            StockSplit.Save();

            var barDuration = stockSerie.BarDuration;
            stockSerie.BarDuration = BarDuration.Daily;

            stockSerie.IsInitialised = false;
            stockSerie.BarDuration = barDuration;
        }

        private void ApplySplit(StockSerie stockSerie)
        {
            if (stockSerie.Count == 0)
                return;

            foreach (var split in StockSplit.Splits.Where(s => s.StockName == stockSerie.StockName).OrderBy(s => s.Date))
            {
                float ratio = split.Before / split.After;
                foreach (var value in stockSerie.Values.Where(v => v.DATE < split.Date))
                {
                    value.ApplyRatio(ratio);
                }
            }
        }
        public virtual void ApplyTrimAfter(DateTime endDate)
        {
            // Clean Data
            foreach (var stockSerie in stockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.ABC))
            {
                // Delete non archive file
                var fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                if (!stockSerie.Initialise())
                    continue;

                stockSerie.BarDuration = BarDuration.Daily;
                if (stockSerie.LastValue.DATE < endDate)
                    continue;

                // Trim archive
                fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
                using StreamWriter sw = new StreamWriter(fileName);
                foreach (var value in stockSerie.Values.Where(v => v.DATE < endDate))
                {
                    sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                }

                stockSerie.IsInitialised = false;
            }

            // Update download config file
            foreach (var config in downloadGroups.Where(c => !c.LabelOnly))
            {
                config.LastDownloaded = endDate.AddDays(-1);
                config.LastDownload = endDate.AddDays(-1);
            }

            var json = JsonSerializer.Serialize(downloadGroups, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = { new JsonStringEnumConverter() }
            });

            File.WriteAllText(configPath, json);
        }

        public override void ApplyTrimBefore(StockSerie stockSerie, DateTime date)
        {
            // Delete non archive file
            var fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            if (!stockSerie.Initialise())
                return;

            stockSerie.BarDuration = BarDuration.Daily;
            if (stockSerie.LastValue.DATE < date)
                return;

            // Trim archive
            fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            using StreamWriter sw = new StreamWriter(fileName);
            foreach (var value in stockSerie.Values.Where(v => v.DATE > date))
            {
                sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
            }

            stockSerie.IsInitialised = false;
        }
    }
}