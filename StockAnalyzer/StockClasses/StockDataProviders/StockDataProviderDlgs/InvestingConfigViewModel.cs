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

        internal void Initialize(string fileName)
        {
            this.FileName = fileName;
            this.Entries = new ObservableCollection<InvestingConfigEntry>(InvestingConfigEntry.LoadFromFile(Settings.Default.RootFolder + FileName));
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
                return this.SelectedItem != null
                       && !this.Entries.Any(e => e.Ticker == this.SelectedItem.Ticker || StockDico.Values.Any(s => s.Ticker == e.Ticker));
            }
        }


        private ICommand _addCommand;
        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new CommandBase(AddEntry));
            }
        }
        public StockDictionary StockDico { get; set; }
        public string FileName { get; private set; }

        public InvestingConfigViewModel()
        {
            this.SearchResults = new List<StockDetails>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Save()
        {
            InvestingConfigEntry.SaveToFile(this.Entries, Settings.Default.RootFolder + FileName);
        }
        public void AddEntry()
        {
            this.Entries.Insert(0, new InvestingConfigEntry(this.SelectedItem.Ticker)
            {
                Group = "FUTURE",
                ShortName = this.SelectedItem.Symbol,
                StockName = this.SelectedItem.FullName
            });

            this.PropertyChanged(this, new PropertyChangedEventArgs("AddEnabled"));
        }

    }
}
