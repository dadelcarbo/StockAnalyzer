﻿using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    public class InvestingConfigViewModel : INotifyPropertyChanged
    {
        public InvestingConfigViewModel()
        {
            this.SearchResults = new List<StockDetails>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<InvestingConfigEntry> Entries { get; set; }
        public IEnumerable<StockDetails> SearchResults { get; private set; }
        StockDetails selectedItem;
        public StockDetails SelectedItem
        {
            get => selectedItem; set
            {
                if (value != selectedItem)
                {
                    selectedItem = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                        this.PropertyChanged(this, new PropertyChangedEventArgs("AddEnabled"));
                    }
                }
            }
        }

        IEnumerable<StockSerie> existing;
        internal void Initialize(string fileName, StockDictionary stockDico)
        {
            this.FileName = fileName;
            this.Entries = new ObservableCollection<InvestingConfigEntry>(InvestingConfigEntry.LoadFromFile(Path.Combine(Folders.PersonalFolder, FileName)));
            this.StockDico = stockDico;
            existing = stockDico.Values.Where(s => s.Ticker != 0 && s.DataProvider == StockDataProvider.Investing);
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value != searchText)
                {
                    searchText = value;
                    if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length > 2)
                    {
                        StockWebHelper wh = new StockWebHelper();
                        this.SearchResults = wh.GetInvestingStockDetails(searchText);

                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchResults"));
                    }
                }
            }
        }

        public bool AddEnabled
        {
            get
            {
                if (this.SelectedItem == null) return false;
                if (this.Entries.Any(e => e.Ticker == this.SelectedItem.Ticker)) return false;
                if (existing.Any(s => s.Ticker == this.SelectedItem.Ticker)) return false;
                return true;
            }
        }


        public StockDictionary StockDico { get; private set; }
        public string FileName { get; private set; }

        public void Save()
        {
            InvestingConfigEntry.SaveToFile(this.Entries, Path.Combine(Folders.PersonalFolder, FileName));

            StockLog.Write(Settings.Default.InvestingUrlRoot);
            Settings.Default.Save();
        }
        private ICommand _addCommand;
        public ICommand AddCommand => _addCommand ??= new CommandBase<InvestingConfigViewModel>(AddEntry, this, t => t.AddEnabled, "AddEnabled");
        public void AddEntry()
        {
            MessageBox.Show("Not Implemented");
        }
    }
}
