using StockAnalyzer.StockWeb;
using StockAnalyzerSettings.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        bool isIntraday = false;
        internal void Initialize(string fileName, StockDictionary stockDico)
        {
            this.FileName = fileName;
            isIntraday = fileName.ToLower().Contains("intraday");
            this.Entries = new ObservableCollection<InvestingConfigEntry>(InvestingConfigEntry.LoadFromFile(Settings.Default.DataFolder + FileName));
            this.StockDico = stockDico;
            if (isIntraday)
            {
                existing = stockDico.Values.Where(s => s.Ticker != 0 && s.DataProvider == StockDataProvider.InvestingIntraday);
            }
            else
            {
                existing = stockDico.Values.Where(s => s.Ticker != 0 && s.DataProvider == StockDataProvider.Investing);
            }
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


        private ICommand _addCommand;
        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new CommandBase<InvestingConfigViewModel>(AddEntry, this, t => t.AddEnabled, "AddEnabled"));
            }
        }
        public StockDictionary StockDico { get; private set; }
        public string FileName { get; private set; }

        public void Save()
        {
            InvestingConfigEntry.SaveToFile(this.Entries, Settings.Default.DataFolder + FileName);
        }
        public void AddEntry()
        {
            this.Entries.Insert(0, new InvestingConfigEntry(this.SelectedItem.Ticker)
            {
                Group = StockSerie.Groups.INTRADAY.ToString(),
                ShortName = this.SelectedItem.Symbol,
                StockName = this.SelectedItem.FullName
            });

            this.PropertyChanged(this, new PropertyChangedEventArgs("AddEnabled"));
        }
    }
}
