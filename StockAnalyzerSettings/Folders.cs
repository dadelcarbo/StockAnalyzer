﻿using StockAnalyzerSettings.Properties;
using System;
using System.IO;

namespace StockAnalyzerSettings
{
    public static class Folders
    {
        public static string Theme => Path.Combine(Settings.Default.PersonalFolder, "Themes");
        public static string Portfolio => Path.Combine(Settings.Default.PersonalFolder, "Portfolio");
        public static string Strategy => Path.Combine(Settings.Default.PersonalFolder, "Strategy");
        public static string Palmares => Path.Combine(Settings.Default.PersonalFolder, "Palmares");
        public static string Tweets => Path.Combine(Settings.Default.PersonalFolder, "Tweets");
        public static string AlertDef => Path.Combine(Settings.Default.PersonalFolder, "Alert");
        public static string AutoTrade => Path.Combine(Settings.Default.PersonalFolder, "AutoTrade");
        public static string Report => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Report");
        public static string AlertLog => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Alerts");
        public static string Log => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Log");
        public static string Saxo => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\Saxo");
        public static string SaxoInstruments => Path.Combine(Portfolio, "InstrumentCache.json");

        public static string DataFolder { get { return Settings.Default.DataFolder; } set { if (value != Settings.Default.DataFolder) Settings.Default.DataFolder = value; } }
        public static string PersonalFolder { get { return Settings.Default.PersonalFolder; } set { if (value != Settings.Default.PersonalFolder) Settings.Default.PersonalFolder = value; } }

        public static string DividendFolder => Path.Combine(DataFolder, "dividend");
        public static string AgendaFolder => Path.Combine(DataFolder, "agenda");
    }
}
