using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerSettings
{
    public static class Folders
    {
        public static string Theme => Path.Combine(Settings.Default.PersonalFolder, "Themes");
        public static string Portfolio => Path.Combine(Settings.Default.PersonalFolder, "Portfolio");
        public static string Palmares => Path.Combine(Settings.Default.PersonalFolder, "Palmares");
        public static string Tweets => Path.Combine(Settings.Default.PersonalFolder, "Tweets");
        public static string Report => Path.Combine(Settings.Default.PersonalFolder, "Report");
        public static string Alert => Path.Combine(Settings.Default.PersonalFolder, "Alert");
        public static string Log => Path.Combine(Settings.Default.PersonalFolder, "Log");

        public static string DataFolder { get { return Settings.Default.DataFolder; } set { if (value != Settings.Default.DataFolder) Settings.Default.DataFolder = value; } }
        public static string PersonalFolder { get { return Settings.Default.PersonalFolder; } set { if (value != Settings.Default.PersonalFolder) Settings.Default.PersonalFolder = value; } }

        public static string DividendFolder => Path.Combine (DataFolder, "dividend");
        public static string AgendaFolder => Path.Combine (DataFolder, "agenda");

    }
}
