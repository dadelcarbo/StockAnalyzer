using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings.Properties;
using StockAnalyzer.StockWeb;
using System.Windows.Forms;

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
                  if (!stockDictionary.CotDictionary.ContainsKey(fields[0]))
                  {
                     stockDictionary.CotDictionary.Add(fields[0], new CotSerie(fields[0]));
                     if (fields.Length > 1)
                     {
                        cotIncludeList.Add(fields[0], fields[1]);
                        if (stockDictionary.ContainsKey(fields[1]))
                        {
                           stockDictionary[fields[1]].CotSerie = stockDictionary.CotDictionary[fields[0]];
                        }
                     }
                  }
               }
            }
            // Check if download Needed
            CotSerie SP500CotSerie = stockDictionary["SP500"].CotSerie;
            SP500CotSerie.Initialise();
            DateTime lastDate = SP500CotSerie.Keys.Last();
            if ((DateTime.Today - lastDate) > new TimeSpan(9, 0, 0, 0, 0))
            {
               // Need to download new COT
               StockWebHelper swh = new StockWebHelper();
               bool upToDate = false;

               NotifyProgress("Downloding commitment of traders data...");

               swh.DownloadCOT(Settings.Default.RootFolder, ref upToDate);

               NotifyProgress("Parsing commitment of traders data...");
               ParseFullCotSeries(cotIncludeList, stockDictionary);
            }
         }
      }
      private void ParseFullCotSeries(SortedDictionary<string, string> cotIncludeList, StockDictionary stockDictionary)
      {
         string line = string.Empty;

         string cotCfgFileName = Settings.Default.RootFolder + @"\COT.cfg";
         StreamWriter sw = null;

         if (!File.Exists(cotCfgFileName))
         {
            sw = new StreamWriter(cotCfgFileName, false);
         }
         try
         {
            // Shall be downloaded from http://www.cftc.gov/MarketReports/files/dea/history/fut_disagg_txt_2010.zip    
            // Read new downloaded values
            string cotFolder = Settings.Default.RootFolder + COT_SUBFOLDER;
            string cotArchiveFolder = Settings.Default.RootFolder + COT_ARCHIVE_SUBFOLDER;
            string[] files = System.IO.Directory.GetFiles(cotFolder, "annual_*.txt");

            int cotLargeSpeculatorPositionLongIndex = 8;
            int cotLargeSpeculatorPositionShortIndex = 9;
            int cotLargeSpeculatorPositionSpreadIndex = 10;
            int cotCommercialHedgerPositionLongIndex = 11;
            int cotCommercialHedgerPositionShortIndex = 12;
            int cotSmallSpeculatorPositionLongIndex = 13;
            int cotSmallSpeculatorPositionShortIndex = 14;
            int cotOpenInterestIndex = 7;

            DateTime cotDate;
            float cotLargeSpeculatorPositionLong;
            float cotLargeSpeculatorPositionShort;
            float cotLargeSpeculatorPositionSpread;
            float cotCommercialHedgerPositionLong;
            float cotCommercialHedgerPositionShort;
            float cotSmallSpeculatorPositionLong;
            float cotSmallSpeculatorPositionShort;
            float cotOpenInterest;

            foreach (string fileName in files)
            {
               StreamReader sr = new StreamReader(fileName);
               CotValue readCotValue = null;
               CotSerie cotSerie = null;
               int endOfNameIndex = 0;

               string cotSerieName = string.Empty;

               string[] row;
               sr.ReadLine();   // Skip header line
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  if (line == string.Empty)
                  {
                     continue;
                  }

                  string[] fields = ParseCOTLine(line);
                  cotSerieName = fields[0];

                  int index = cotSerieName.IndexOf(" - ");
                  if (index == -1)
                  {
                     continue;
                  }
                  cotSerieName = cotSerieName.Substring(0, index) + "_COT";

                  if (!cotIncludeList.Keys.Contains(cotSerieName))
                  {
                     continue;
                  }

                  row = fields;

                  cotLargeSpeculatorPositionLong = float.Parse(row[cotLargeSpeculatorPositionLongIndex]);
                  cotLargeSpeculatorPositionShort = float.Parse(row[cotLargeSpeculatorPositionShortIndex]);
                  cotLargeSpeculatorPositionSpread = float.Parse(row[cotLargeSpeculatorPositionSpreadIndex]);
                  cotCommercialHedgerPositionLong = float.Parse(row[cotCommercialHedgerPositionLongIndex]);
                  cotCommercialHedgerPositionShort = float.Parse(row[cotCommercialHedgerPositionShortIndex]);
                  cotSmallSpeculatorPositionLong = float.Parse(row[cotSmallSpeculatorPositionLongIndex]);
                  cotSmallSpeculatorPositionShort = float.Parse(row[cotSmallSpeculatorPositionShortIndex]);
                  cotOpenInterest = float.Parse(row[cotOpenInterestIndex]);

                  cotDate = DateTime.Parse(row[2], usCulture);

                  readCotValue = new CotValue(cotDate, cotLargeSpeculatorPositionLong, cotLargeSpeculatorPositionShort, cotLargeSpeculatorPositionSpread,
                      cotSmallSpeculatorPositionLong, cotSmallSpeculatorPositionShort,
                      cotCommercialHedgerPositionLong, cotCommercialHedgerPositionShort, cotOpenInterest);
                  if (stockDictionary.CotDictionary.ContainsKey(cotSerieName))
                  {
                     cotSerie = stockDictionary.CotDictionary[cotSerieName];
                     if (!cotSerie.ContainsKey(readCotValue.Date))
                     {
                        cotSerie.Add(readCotValue.Date, readCotValue);

                        // flag as not initialised as values have to be calculated
                        cotSerie.IsInitialised = false;
                     }
                  }
                  else
                  {
                     cotSerie = new CotSerie(cotSerieName);
                     stockDictionary.CotDictionary.Add(cotSerieName, cotSerie);
                     cotSerie.Add(readCotValue.Date, readCotValue);

                     // Create first COT cfg file, only if not existing.
                     if (sw != null) sw.WriteLine(cotSerieName + ";");

                     if (!string.IsNullOrWhiteSpace(cotIncludeList[cotSerieName]))
                     {
                        if (stockDictionary.ContainsKey(cotIncludeList[cotSerieName]))
                        {
                           stockDictionary[cotIncludeList[cotSerieName]].CotSerie = cotSerie;
                        }
                     }
                  }
               }
               sr.Close();
            }
            foreach (CotSerie cotSerie in stockDictionary.CotDictionary.Values)
            {
               string archiveFileName = cotArchiveFolder + @"\" + cotSerie.CotSerieName + ".csv";

               cotSerie.SaveToFile(archiveFileName);
            }
         }
         catch (System.Exception e)
         {
            MessageBox.Show(e.Message + "\r\r" + line, "Failed to parse COT file");
         }
         finally
         {
            if (sw != null) sw.Dispose();
         }
      }
      private string[] ParseCOTLine(string line)
      {
         string field = string.Empty;
         bool quoteFound = false;
         List<string> fields = new List<string>();
         for (int i = 0; i < line.Length; i++)
         {
            if (line[i] == '\"')
            {
               if (quoteFound)
               {
                  quoteFound = false;
               }
               else
               {
                  quoteFound = true;
               }
            }
            else
            {
               if (quoteFound)
               {
                  field += line[i];
               }
               else
               {
                  if (line[i] == ',')
                  {
                     fields.Add(field.Trim());
                     field = string.Empty;
                  }
                  else
                  {
                     field += line[i];
                  }
               }
            }
         }
         return fields.ToArray();
      }


      public override bool DownloadDailyData(string rootFolder, StockSerie serie)
      {
         throw new NotImplementedException();
      }

      // Private methods
      static private string COT_ARCHIVE_SUBFOLDER = @"\data\archive\weekly\cot";
      static private string COT_SUBFOLDER = @"\data\weekly\cot";
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
