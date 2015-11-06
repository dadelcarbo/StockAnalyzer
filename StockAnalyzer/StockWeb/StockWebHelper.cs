using System;
using System.Data.Linq.Mapping;
using System.IO;
using System.Net;
using System.Text;
using Ionic.Zip;

namespace StockAnalyzer.StockWeb
{
   public class StockWebHelperException : Exception
   {
      public StockWebHelperException(string msg, Exception e)
         : base(msg, e)
      {
      }
   }
   public class StockWebHelper
   {
      public delegate void DownloadingStockEventHandler(string text);
      public event DownloadingStockEventHandler DownloadStarted;


      static protected string DAILY_SUBFOLDER = @"\data\daily";
      static protected string WEEKLY_SUBFOLDER = @"\data\weekly";
      static protected string DAILY_ARCHIVE_SUBFOLDER = @"\data\archive\daily";

      static private string COT_SUBFOLDER = WEEKLY_SUBFOLDER + @"\cot";
      static private string HARPEX_SUBFOLDER = WEEKLY_SUBFOLDER + @"\harpex";
      static private string YAHOO_SUBFOLDER = DAILY_SUBFOLDER + @"\Yahoo";
      static private string RYDEX_SUBFOLDER = DAILY_SUBFOLDER + @"\Rydex";

      public StockWebHelper()
      {
      }
      #region Download Entry Points

      public bool DownloadHarpex(string destRootFolder, ref bool upToDate)
      {
         upToDate = false;

         string folder = destRootFolder + @"\" + HARPEX_SUBFOLDER;
         if (!Directory.Exists(folder))
         {
            Directory.CreateDirectory(folder);
         }
         return DownloadFile(folder, "Harpex.csv", @"http://www.harperpetersen.com/harpex/download.csv");
      }
      public bool DownloadCOT(string destRootFolder, ref bool upToDate)
      {
         upToDate = true;

         // 
         DownloadCOTArchive(destRootFolder, ref upToDate);

         string folder = destRootFolder + COT_SUBFOLDER;
         if (!Directory.Exists(folder))
         {
            Directory.CreateDirectory(folder);
         }
         // Check last download date
         string fileName = folder + @"\annual_" + DateTime.Now.Year + ".txt";
         if (File.GetLastWriteTime(fileName) >= DateTime.Now.AddDays(-1))
         {
            upToDate = true;
            return true;
         }
         upToDate = false;

         if (DownloadStarted != null)
         {
            this.DownloadStarted("Downloading Commitment of Traders...");
         }
         string url = @"http://www.cftc.gov/files/dea/history/deacot$YEAR.zip".Replace("$YEAR", DateTime.Now.Year.ToString());
         using (WebClient wc = new WebClient())
         {
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
            // Download file
            try
            {
               Stream stream = new MemoryStream(wc.DownloadData(url));

               string unzipName = folder + @"\annual.txt";

               // Unzip the file
               using (ZipFile zip = ZipFile.Read(stream))
               {
                  zip.ExtractSelectedEntries("annual.txt", "", folder, ExtractExistingFileAction.OverwriteSilently);
                  if (File.Exists(unzipName))
                  {
                     if (File.Exists(fileName))
                     {
                        File.Delete(fileName);
                     }
                     File.Move(unzipName, fileName);
                     File.SetLastWriteTime(fileName, DateTime.Now);
                  }
               }
            }
            catch
            {   // #### Crap error management
               return false;
            }
         }
         return true;
      }

      public bool DownloadCOTArchive(string destRootFolder, ref bool upToDate)
      {
         upToDate = false;

         string folder = destRootFolder + COT_SUBFOLDER;
         if (!Directory.Exists(folder))
         {
            Directory.CreateDirectory(folder);
         }
         // Check last download date
         for (int i = 1991; i <= DateTime.Now.Year-1; i++)
         {
            string fileName = folder + @"\annual_" + i + ".txt";
            if (File.Exists(fileName)) continue;

            if (DownloadStarted != null)
            {
               this.DownloadStarted("Downloading Commitment of Traders...");
            }
            string url = @"http://www.cftc.gov/files/dea/history/deacot$YEAR.zip".Replace("$YEAR", i.ToString());
            using (WebClient wc = new WebClient())
            {
               wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
               // Download file
               try
               {
                  Stream stream = new MemoryStream(wc.DownloadData(url));

                  string unzipName = folder + @"\annual.txt";

                  // Unzip the file
                  using (ZipFile zip = ZipFile.Read(stream))
                  {
                     zip.ExtractSelectedEntries("annual.txt", "", folder,
                         ExtractExistingFileAction.OverwriteSilently);
                     if (File.Exists(unzipName))
                     {
                        if (File.Exists(fileName))
                        {
                           File.Delete(fileName);
                        }
                        File.Move(unzipName, fileName);
                        File.SetLastWriteTime(fileName, DateTime.Now);
                     }
                  }
               }
               catch
               {
                  // #### Crap error management
                  return false;
               }
            }
         }
         return true;
      }

      #endregion


      public bool DownloadFileFromYahoo(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
      {
         string url = @"http://ichart.finance.yahoo.com/table.csv?s=$NAME&a=$START_MONTH&b=$START_DAY&c=$START_YEAR&d=$END_MONTH&e=$END_DAY&f=$END_YEAR&g=d&ignore=.csv";

         // Build URL
         url = url.Replace("$NAME", stockName);
         url = url.Replace("$START_DAY", startDate.Day.ToString());
         url = url.Replace("$START_MONTH", (startDate.Month - 1).ToString());
         url = url.Replace("$START_YEAR", startDate.Year.ToString());
         url = url.Replace("$END_DAY", endDate.Day.ToString());
         url = url.Replace("$END_MONTH", (endDate.Month - 1).ToString());
         url = url.Replace("$END_YEAR", endDate.Year.ToString());

         return DownloadFile(destFolder, fileName, url);
      }
      private bool DownloadFileFromABC(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
      {
         if (File.GetLastWriteTime(destFolder + @"\" + fileName) > DateTime.Now.AddHours(-6))
         {
            return true;
         }
         bool success = true;
         try
         {
            // Build post data
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData;

            // Send POST request
            string url = "http://www.abcbourse.com/download/historiques.aspx";
            postData = "ctl00_BodyABC_ToolkitScriptManager1_HiddenField=%3B%3BAjaxControlToolkit%2C+Version%3D3.0.20229.20843%2C+Culture%3Dneutral%2C+PublicKeyToken%3D28f01b0e84b6d53e%3Afr-FR%3A3b7d1b28-161f-426a-ab77-b345f2c428f5%3A865923e8%3A9b7907bc%3A411fea1c%3Ae7c87f07%3A91bd373d%3Abbfda34c%3A30a78ec5%3A9349f837%3Ad4245214%3A8e72a662%3Aacd642d2%3A596d588c%3A77c58d20%3A14b56adc%3A269a19ae&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUJMjUzMjM0NTI3ZBgBBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WJwUWY3RsMDAkQm9keUFCQyRldXJvbGlzdAUcY3RsMDAkQm9keUFCQyRhY3Rpb25zaW5kaWNlcwUaY3RsMDAkQm9keUFCQyRhY3Rpb25zaW5kdXMFFWN0bDAwJEJvZHlBQkMkY29tcGxldAUbY3RsMDAkQm9keUFCQyRjb21wbGV0bm93YXJyBRJjdGwwMCRCb2R5QUJDJHNyZHAFGGN0bDAwJEJvZHlBQkMkaW5kaWNlc0ZScAUYY3RsMDAkQm9keUFCQyRldXJvbGlzdGFwBRhjdGwwMCRCb2R5QUJDJGV1cm9saXN0YnAFGGN0bDAwJEJvZHlBQkMkZXVyb2xpc3RjcAUZY3RsMDAkQm9keUFCQyRldXJvbGlzdHplcAUUY3RsMDAkQm9keUFCQyRhbHRlcnAFEGN0bDAwJEJvZHlBQkMkbWwFFGN0bDAwJEJvZHlBQkMkdHJhY2twBRFjdGwwMCRCb2R5QUJDJGJzcAUTY3RsMDAkQm9keUFCQyRvYmwycAUSY3RsMDAkQm9keUFCQyRvYmxwBRZjdGwwMCRCb2R5QUJDJHdhcnJhbnRzBRJjdGwwMCRCb2R5QUJDJGZjcHAFFWN0bDAwJEJvZHlBQkMkeGNhYzQwcAUWY3RsMDAkQm9keUFCQyR4c2JmMTIwcAUVY3RsMDAkQm9keUFCQyR4Y2FjYXRwBRZjdGwwMCRCb2R5QUJDJHhjYWNuMjBwBRhjdGwwMCRCb2R5QUJDJHhjYWNzbWFsbHAFFWN0bDAwJEJvZHlBQkMkeGNhYzYwcAUWY3RsMDAkQm9keUFCQyR4Y2FjbDYwcAUVY3RsMDAkQm9keUFCQyR4Y2FjbXNwBRVjdGwwMCRCb2R5QUJDJHhiZWwyMGcFFWN0bDAwJEJvZHlBQkMkeGFleDI1bgURY3RsMDAkQm9keUFCQyRkanUFEmN0bDAwJEJvZHlBQkMkbmFzdQUUY3RsMDAkQm9keUFCQyRzcDUwMHUFEmN0bDAwJEJvZHlBQkMkdXNhdQUSY3RsMDAkQm9keUFCQyRiZWxnBRNjdGwwMCRCb2R5QUJDJGhvbGxuBRVjdGwwMCRCb2R5QUJDJGxpc2JvYWwFEmN0bDAwJEJvZHlBQkMkZGV2cAUVY3RsMDAkQm9keUFCQyRvbmVTaWNvBRNjdGwwMCRCb2R5QUJDJGNiWWVz%2BsS2JGgdTPMzT67Kk4j41fKYkb0%3D&__EVENTVALIDATION=%2FwEWPgLwo8iJDAKPgp47AuX3t%2B0NAoLSprgMAr7jqP8KAu7P4%2F0BAqKE4sMEAovUvJkOAoyS8LkNArPy16cOAs7D2KsOAre767oNAonG%2FcEHAriKpIIJAtPzwZcDAufBpr8OApeMiP0GAoSYiOsEAqajl%2FkIAs%2Ft68QPAouLt8YNAsLoj7MCAvTZqtILAuPI9KICApeY0uUNArGJ%2BZcLAqDEhNQNAvXt%2B%2BwMApma3OYLAs3qjZACAr%2Bb8NUFAuSyzpoDAvrGrrwMAvPGhsMEAp6bvJgIAo7%2F6cIPAoiY%2F34ClfG83AIC6%2B3nrgwC%2FPC9%2BgcCnOOzhwECn5vYgwgCmJXlpQ0C64nUxAoCvKXGgggCusuh2g8Cusuh2g8Czcuh2g8CyMuh2g8Cq8uh2g8CyMuh2g8Cusuh2g8CsMuh2g8CvMuh2g8CsMuh2g8Cusuh2g8Cusuh2g8CqtSqawKW8NTsBQKe5dnRCwKN37s5AvfF2O4MZz3S6Hh5%2B8oV06LsCyVsEdDTkuI%3D&ctl00%24txtAutoComplete=&ctl00%24BodyABC%24strDateDeb=$START_DAY%2F$START_MONTH%2F$START_YEAR&ctl00%24BodyABC%24strDateFin=$END_DAY%2F$END_MONTH%2F$END_YEAR&ctl00%24BodyABC%24oneSico=on&ctl00%24BodyABC%24txtOneSico=$ISIN&ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger&ctl00%24BodyABC%24dlFormat=w&ctl00%24BodyABC%24listFormat=isin";
            postData = postData.Replace("$ISIN", ISIN);

            postData = postData.Replace("$START_DAY", startDate.Day.ToString());
            postData = postData.Replace("$START_MONTH", startDate.Month.ToString());
            postData = postData.Replace("$START_YEAR", startDate.Year.ToString());
            postData = postData.Replace("$END_DAY", endDate.Day.ToString());
            postData = postData.Replace("$END_MONTH", endDate.Month.ToString());
            postData = postData.Replace("$END_YEAR", endDate.Year.ToString());
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Method = "POST";
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.Referer = url;

            Stream newStream = req.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            success = SaveResponseToFile(destFolder + @"\" + fileName, req);
         }
         catch (System.Exception ex)
         {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
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
            using (Stream dataStream = response.GetResponseStream())
            {
               // Open the stream using a StreamReader for easy access.
               using (StreamReader reader = new StreamReader(dataStream))
               {
                  // Read the content.
                  string responseFromServer = reader.ReadToEnd();

                  if (responseFromServer.Length != 0)
                  {

                     // Save content to file
                     using (StreamWriter writer = new StreamWriter(fileName))
                     {
                        writer.Write(responseFromServer);
                     }
                  }
                  else
                  {
                     success = false;
                  }
               }
            }
         }
         return success;
      }
      private bool DownloadFile(string destFolder, string fileName, string url)
      {
         try
         {
            if (!System.IO.Directory.Exists(destFolder))
            {
               System.IO.Directory.CreateDirectory(destFolder);
            }

            if (!DownloadFile(destFolder + @"\" + fileName, url))
            {
               return false;
            }
         }
         catch (Exception e)
         {
            throw new StockWebHelperException("Exception downloading file: " + url, e);
         }
         return true;
      }
      private bool DownloadFile(string destFile, string url)
      {
         if (File.GetLastWriteTime(destFile) > DateTime.Now.AddHours(-6))
         {
            return true;
         }
         bool success = true;
         HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

         HttpWebResponse response = (HttpWebResponse)req.GetResponse();
         // Get the stream containing content returned by the server.
         Stream dataStream = response.GetResponseStream();
         // Open the stream using a StreamReader for easy access.
         StreamReader reader = new StreamReader(dataStream);
         // Read the content.
         string responseFromServer = reader.ReadToEnd();

         if (responseFromServer.Length != 0)
         {
            // Save content to file
            StreamWriter writer = new StreamWriter(destFile);
            writer.Write(responseFromServer);
            writer.Close();
         }
         else
         {
            success = false;
         }

         // Cleanup the streams
         reader.Close();
         dataStream.Close();
         // Close the reponse
         response.Close();

         return success;
      }
      private string DownloadFile(string url)
      {
         HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

         HttpWebResponse response = (HttpWebResponse)req.GetResponse();
         // Get the stream containing content returned by the server.
         Stream dataStream = response.GetResponseStream();
         // Open the stream using a StreamReader for easy access.
         StreamReader reader = new StreamReader(dataStream);
         // Read the content.
         string responseFromServer = reader.ReadToEnd();

         // Cleanup the streams
         reader.Close();
         dataStream.Close();
         // Close the reponse
         response.Close();

         return responseFromServer;
      }
      private bool DownloadFileFromCBOE(string destFolder, string fileName, string indexName)
      {
         string url = string.Empty;

         if (indexName.ToUpper().CompareTo("VXV") == 0)
         {
            url = "http://www.cboe.com/publish/scheduledtask/mktdata/datahouse/vxvdailyprices.csv";
         }
         else
         {
            url = @"http://www.cboe.com/publish/Scheduledtask/mktdata/datahouse/$INDEX_NAMEhistory.csv";

            // Build URL
            url = url.Replace("$INDEX_NAME", indexName);
         }

         return DownloadFile(destFolder, fileName, url);
      }
   }
}