using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   class NASDACQShortInterestDataProvider : StockDataProviderBase
   {
      public override bool SupportsIntradayDownload
      {
         get { return false; }
      }

      static private string SHORTINTEREST_FOLDER = @"data\daily\ShortInterest\";
      static private string SI_ARCHIVE_SUBFOLDER = @"\data\archive\daily\ShortInterest";

      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         string url = @"http://www.nasdaq.com/symbol/%TICKER/short-interest";
         url = url.Replace("%TICKER", stockSerie.ShortName);

         ShortInterestSerie siSerie = StockDictionary.StockDictionarySingleton.ShortInterestDictionary[stockSerie.StockName];
         siSerie.Initialise();

         if (siSerie.Count>0 && siSerie.Keys.Last() > (DateTime.Today.AddDays(15))) return false;

         StockWebHelper swh = new StockWebHelper();
         if (swh.DownloadFile(Settings.Default.RootFolder + SHORTINTEREST_FOLDER, stockSerie.ShortName + "_SI.html", url))
         {
            string html = string.Empty;
            using (StreamReader rw = new StreamReader(Settings.Default.RootFolder + SHORTINTEREST_FOLDER + stockSerie.ShortName + "_SI.html"))
            {
               html = rw.ReadToEnd();
               html = html.Remove(0, html.IndexOf("Settlement Date"));
               html = html.Remove(0, html.IndexOf("<tr>"));
               html = html.Remove(html.IndexOf("</tbody>"));

               html = html.Replace("</tr>","");
               html = html.Replace("<tr>","");

               html = html.Replace("</td><td>", ";");

               html = html.Replace("<td>", "");
               html = html.Replace("</td>", "");
               html = html.Replace("\t", "");
               html = html.Replace("\r\n\r\n", "\r\n");
               int index = html.IndexOf("<tr>");
            }
            using (Stream stringStream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
            {
               using (StreamReader rw = new StreamReader(stringStream))
               {
                  string line = rw.ReadLine();
                  while (!rw.EndOfStream)
                  {
                     string[] fields = rw.ReadLine().Split(';');
                     ShortInterestValue siValue = new ShortInterestValue(DateTime.Parse(fields[0], usCulture), float.Parse(fields[1], usCulture), float.Parse(fields[2], usCulture), float.Parse(fields[3], usCulture));

                     if (!siSerie.ContainsKey(siValue.Date))
                     {
                        siSerie.Add(siValue.Date, siValue);
                     }
                  }
               }
            }

            siSerie.SaveToFile();

            File.Delete(Settings.Default.RootFolder + SHORTINTEREST_FOLDER + stockSerie.ShortName + "_SI.html");
         }
         return false;
      }

      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
         string dirName = Settings.Default.RootFolder + SI_ARCHIVE_SUBFOLDER;
         if (!Directory.Exists(dirName))
         {
            Directory.CreateDirectory(dirName);
         }
         dirName = Settings.Default.RootFolder + SHORTINTEREST_FOLDER;
         if (!Directory.Exists(dirName))
         {
            Directory.CreateDirectory(dirName);
         }
         string fileName = Settings.Default.RootFolder + @"\ShortInterest.cfg";
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               while (!sr.EndOfStream)
               {
                  string[] fields = sr.ReadLine().Split(';');
                  if (stockDictionary.ContainsKey(fields[0]))
                  {
                     NotifyProgress("Short Interest for " + fields[0]);
                     stockDictionary[fields[0]].HasShortInterest = true;

                     ShortInterestSerie siSerie = new ShortInterestSerie(fields[0]);
                     stockDictionary.ShortInterestDictionary.Add(fields[0], siSerie);

                     if (download)
                     {
                        this.DownloadDailyData(Settings.Default.RootFolder, stockDictionary[fields[0]]);
                     }
                  }
                  else
                  {
                     StockLog.Write("Short interest for not supported stock: " + fields[0]);
                  }
               }
            }
         }
      }
   }
}
