using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class COTDataProvider : StockDataProviderBase
   {
      // IStockDataProvider Implementation
      public override bool SupportsIntradayDownload
      {
         get { return false; }
      }
      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         // Check archive folder
         string COTFolder = rootFolder + COT_ARCHIVE_SUBFOLDER;
         if (!Directory.Exists(COTFolder))
         {
            Directory.CreateDirectory(COTFolder);
         }

         // Read archive first
         bool res = false;
         string archiveFileName = COTFolder + @"\" + stockSerie.StockName + ".csv";
         if (File.Exists(archiveFileName))
         {
            res |= ParseCSVFile(stockSerie, archiveFileName);
         }

         //COTFolder = rootFolder + COT_SUBFOLDER;
         //if (Directory.Exists(COTFolder))
         //{
         //   string[] files = System.IO.Directory.GetFiles(COTFolder, "annual_*.txt");
         //   foreach (string fileName in files)
         //   {
         //      res |= this.ParseCOT(stockSerie, fileName);
         //   }

         //   stockSerie.SaveToCSVFromDateToDate(archiveFileName, stockSerie.Keys.First(), new DateTime(stockSerie.Keys.Last().Year - 1, 12, 31));

         //}
         return res;
      }
      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
         SortedDictionary<string, string> cotIncludeList = new SortedDictionary<string, string>();
         string[] fields;

         string fileName = Settings.Default.RootFolder + @"\COT.cfg";
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               while (!sr.EndOfStream)
               {
                  fields = sr.ReadLine().Split(';');
                  if (!stockDictionary.ContainsKey(fields[0]))
                  {
                     stockDictionary.Add(fields[0], new StockSerie(fields[0], fields[0], StockSerie.Groups.COT, StockDataProvider.COT));
                  }
                  /* @@@@ COT
                                          if (fields.Length > 1 && !string.IsNullOrWhiteSpace(fields[1]))
                                          {
                                              if (stockDictionary.ContainsKey(fields[1]))
                                              {
                                                  stockDictionary[fields[0]].SecondarySerie = stockDictionary[fields[1]];
                                              }
                                              else
                                              {
                                                  MessageBox.Show(string.Format("Invalid COT Mapping for {0} to {1}", fields[0], fields[1] ));
                                              }
                                          }
                  */
               }
            }
         }
      }
      public override bool DownloadDailyData(string rootFolder, StockSerie serie)
      {
         throw new NotImplementedException();
      }

      // Private methods
      static private string COT_ARCHIVE_SUBFOLDER = @"\data\archive\weekly\cot";
      private bool ParseCOT(StockSerie stockSerie, string fileName)
      {
         StockLog.Write("ParseCOT: " + stockSerie.StockName + " from file: " + fileName);
         string line = string.Empty;
         bool res = false;
         try
         {
            int cotLongIndex = 10;
            int cotShortIndex = 11;
            int cotOpenInterestIndex = 6;
            using (StreamReader sr = new StreamReader(fileName))
            {
               StockDailyValue readValue = null;
               int endOfNameIndex = 0;

               string cotSerieName = string.Empty;
               DateTime cotDate;
               float cotValue = 0.0f;
               string[] row;
               sr.ReadLine();   // Skip header line
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine().Replace("\"", "");
                  if (line == string.Empty)
                  {
                     continue;
                  }
                  res = true;

                  endOfNameIndex = line.IndexOf(",");
                  cotSerieName = line.Substring(0, endOfNameIndex);
                  if (cotSerieName.Contains(" - "))
                  {
                     cotSerieName = cotSerieName.Substring(0, cotSerieName.IndexOf(" - "));
                  }

                  cotSerieName += "_COT";
                  if (stockSerie.StockName != cotSerieName)
                  {
                     continue;
                  }
                  row = line.Substring(endOfNameIndex + 2).Split(',');

                  cotDate = DateTime.Parse(row[1], usCulture);
                  if (!stockSerie.Keys.Contains(cotDate))
                  {
                     float cotLong = float.Parse(row[cotLongIndex]);
                     float cotShort = float.Parse(row[cotShortIndex]);
                     float cotOpenInterest = float.Parse(row[cotOpenInterestIndex]);
                     if (cotOpenInterest != 0.0f)
                     {
                        cotValue = 100.0f * (cotLong - cotShort) / cotOpenInterest;
                     }
                     else
                     {
                        cotValue = 0.0f;
                     }
                     readValue = new StockDailyValue(cotSerieName, cotValue, cotValue, cotValue, cotValue, 0, cotDate);
                     stockSerie.Add(readValue.DATE, readValue);
                     readValue.Serie = stockSerie;
                  }
                  else
                  {
                     StockLog.Write("COT for " + cotSerieName + " already added, data for" + row[3] + " exchange ignored");
                  }
               }
            }
         }
         catch (System.Exception e)
         {
            StockLog.Write(e.Message + "Failed to parse COT file" + "\r\r" + line);
         }
         return res;
      }

   }
}
