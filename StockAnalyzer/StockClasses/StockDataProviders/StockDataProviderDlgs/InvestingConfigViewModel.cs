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
                return this.SelectedItem != null && !this.Entries.Any(e => e.Ticker == this.SelectedItem.Ticker);
            }
        }


        private ICommand _clickCommand;
        public ICommand AddCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new CommandBase(AddEntry));
            }
        }
        public void AddEntry()
        {
            this.Entries.Insert(0, new InvestingConfigEntry() {
                Group = "FUTURE",
                ShortName = this.SelectedItem.Symbol,
                StockName = this.SelectedItem.FullName,
                Ticker = this.SelectedItem.Ticker
            });
        }
        public StockDictionary StockDico { get; set; }

        public InvestingConfigViewModel()
        {
            this.Entries = new ObservableCollection<InvestingConfigEntry>(InvestingConfigEntry.LoadFromFile(Settings.Default.RootFolder + new InvestingIntradayDataProvider().UserConfigFileName));
            this.SearchResults = new List<StockDetails>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
