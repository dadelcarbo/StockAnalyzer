﻿using Newtonsoft.Json;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace StockAnalyzer.StockPortfolio.StockStrategy
{
    public class StockStrategyEvent
    {
        private static List<string> indicatorTypes = new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Decorator", "Cloud", "AutoDrawing" };

        public string IndicatorType { get; set; }
        public string Indicator { get; set; }
        public string Event { get; set; }
    }

    public class StockStrategy
    {
        public string Name { get; set; }
        public string StockName { get; set; }
        public long Uic { get; set; }
        public string Portfolio { get; set; }
        public BarDuration BarDuration { get; set; }

        public bool Active { get; set; }
        public string Theme { get; set; }

        public StockStrategyEvent EntryEvent { get; set; }
        public StockStrategyEvent ExitEvent { get; set; }

        public string EntryStop { get; set; }

        #region PERSISTENCY

        private const string STRATEGY_FILE_EXT = ".stgy";

        public void Serialize()
        {
            string filepath = Path.Combine(Folders.Strategy, this.Name + STRATEGY_FILE_EXT);
            File.WriteAllText(filepath, JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings));
        }
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { DateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ" };


        public static StockStrategy Deserialize(string filepath)
        {
            return JsonConvert.DeserializeObject<StockStrategy>(File.ReadAllText(filepath), jsonSerializerSettings);
        }

        private static List<StockStrategy> strategies;
        public static List<StockStrategy> Strategies => strategies ??= strategies = LoadStrategies();


        private static List<StockStrategy> LoadStrategies()
        {
            try
            {
                // Load saved portfolio
                var strategies = new List<StockStrategy>();
                foreach (var strategy in Directory.EnumerateFiles(Folders.Strategy, "*" + STRATEGY_FILE_EXT).OrderBy(s => s).Select(s => StockStrategy.Deserialize(s)))
                {
                    strategies.Add(strategy);
                }
                return strategies;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading strategy file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        #endregion 
    }
}