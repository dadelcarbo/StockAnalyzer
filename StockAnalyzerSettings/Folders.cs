using StockAnalyzerSettings.Properties;
using System;
using System.IO;
using System.Reflection;

namespace StockAnalyzerSettings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCreateAttribute : Attribute
    {
        public AutoCreateAttribute() { }
    }

    public static class Folders
    {
        [AutoCreate]
        public static string Theme => Path.Combine(Settings.Default.PersonalFolder, "Themes");
        [AutoCreate]
        public static string Portfolio => Path.Combine(Settings.Default.PersonalFolder, "Portfolio");
        [AutoCreate]
        public static string Strategy => Path.Combine(Settings.Default.PersonalFolder, "Strategy");
        [AutoCreate]
        public static string Palmares => Path.Combine(Settings.Default.PersonalFolder, "Palmares");
        [AutoCreate]
        public static string Tweets => Path.Combine(Settings.Default.PersonalFolder, "Tweets");
        [AutoCreate]
        public static string AlertDef => Path.Combine(Settings.Default.PersonalFolder, "Alert");
        [AutoCreate]
        public static string AutoTrade => Path.Combine(Settings.Default.PersonalFolder, "AutoTrade");
        [AutoCreate]
        public static string ReportTemplates => Path.Combine(Settings.Default.PersonalFolder, "ReportTemplates");
        [AutoCreate]
        public static string Report => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Report");
        [AutoCreate]
        public static string PortfolioReport => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Report\Portfolio");
        [AutoCreate]
        public static string AlertLog => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Alerts");
        [AutoCreate]
        public static string Log => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Log");
        [AutoCreate]
        public static string Saxo => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Saxo");

        public static string SaxoInstruments => Path.Combine(Portfolio, "InstrumentCache.json");

        public static string DataFolder { get { return Settings.Default.DataFolder; } set { if (value != Settings.Default.DataFolder) Settings.Default.DataFolder = value; } }
        public static string PersonalFolder { get { return Settings.Default.PersonalFolder; } set { if (value != Settings.Default.PersonalFolder) Settings.Default.PersonalFolder = value; } }

        [AutoCreate]
        public static string DividendFolder => Path.Combine(DataFolder, "dividend");
        [AutoCreate]
        public static string AgendaFolder => Path.Combine(DataFolder, "agenda");

        public static string WatchlistReportTemplate => Path.Combine(Report, "WatchlistReportTemplate.html");
        public static string WatchlistItemTemplate => Path.Combine(Report, "WatchlistItemTemplate.html");

        public static string ReportTemplate = Path.Combine(Report, "ReportTemplate.html");

        public static void CreateDirectories()
        {
            Type type = typeof(Folders);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(AutoCreateAttribute)))
                {
                    string path = (string)property.GetValue(null);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }
    }
}
