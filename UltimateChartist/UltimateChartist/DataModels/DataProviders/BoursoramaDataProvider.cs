using System;
using System.Collections.Generic;
using System.IO;
using UltimateChartist.Helpers;

namespace UltimateChartist.DataModels.DataProviders
{
    public class BoursoramaDataProvider : StockDataProviderBase
    {
        static private string Boursorama_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\Boursorama";
        static private string Boursorama_DAILY_FOLDER = DAILY_SUBFOLDER + @"\Boursorama";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\Boursorama";
        static private string CONFIG_FILE = "boursorama.cfg";
        static private string CONFIG_FILE_USER = "boursorama.user.cfg";
        static private string Boursorama_TMP_FOLDER = Boursorama_DAILY_FOLDER + @"\TMP";
        public override BarDuration[] BarDurations { get; } = { BarDuration.M_1, BarDuration.M_2, BarDuration.M_5, BarDuration.M_15, BarDuration.M_30, BarDuration.H_1, BarDuration.H_2, BarDuration.H_4, BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };

        public override string Name => "Boursorama";
        public override string DisplayName => "Boursorama";

        public override void InitDictionary()
        {
            CreateDirectories();
        }

        public static void CreateDirectories()
        {
            if (!Directory.Exists(Folders.AgendaFolder))
            {
                Directory.CreateDirectory(Folders.AgendaFolder);
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, Boursorama_DAILY_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, Boursorama_DAILY_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder + ARCHIVE_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder + ARCHIVE_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, Boursorama_INTRADAY_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, Boursorama_INTRADAY_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, Boursorama_TMP_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, Boursorama_TMP_FOLDER));
            }
            else
            {
                foreach (string file in Directory.GetFiles(Path.Combine(Folders.DataFolder, Boursorama_TMP_FOLDER)))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
        }

        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        public override List<StockBar> LoadData(Instrument instrument, BarDuration duration)
        {
            throw new NotImplementedException("BoursoramaDataProvider.LoadData");
        }
        public override List<StockBar> DownloadData(Instrument instrument, BarDuration duration)
        {
            throw new NotImplementedException("BoursoramaDataProvider.DownloadData");
        }
    }
}
