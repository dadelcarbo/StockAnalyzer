using StockAnalyzer.StockClasses;
using StockAnalyzerSettings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public class TradeAgentDef
    {
        public int Id { get; set; }

        public bool Draft { get; set; }

        public bool AutoStart { get; set; }

        public string PortfolioName { get; set; }

        public string StockName { get; set; }

        public BarDuration BarDuration { get; set; }

        public string StrategyName { get; set; }

        public string Theme { get; set; }

        public override string ToString()
        {
            return $"{Id}-{StrategyName}-{BarDuration}-{StockName}";
        }

        static ObservableCollection<TradeAgentDef> agentDefs;
        public static ObservableCollection<TradeAgentDef> AgentDefs => agentDefs ??= Load();

        const string fileName = "AgentDefs.txt";
        private static ObservableCollection<TradeAgentDef> Load()
        {
            try
            {
                var filePath = Path.Combine(Folders.AutoTrade, fileName);
                if (File.Exists(filePath))
                {
                    // Save result for analysis purpose
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                    };
                    return JsonSerializer.Deserialize<ObservableCollection<TradeAgentDef>>(File.ReadAllText(filePath), options);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Agent Load error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new ObservableCollection<TradeAgentDef>();
        }

        public static void Save()
        {
            if (agentDefs == null || agentDefs.Count == 0)
                return;

            try
            {
                // Save result for analysis purpose
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    Converters = { new JsonStringEnumConverter() },
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var filePath = Path.Combine(Folders.AutoTrade, fileName);


                File.WriteAllText(filePath, JsonSerializer.Serialize(agentDefs, options));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Agent Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
