using Saxo.OpenAPI.TradingServices;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockData;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockData.DataProviders.SaxoTurbos;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class LineViewModel
    {
        public StockInstrument Instrument { get; set; }
        public StockDailyValue LastValue { get; set; }
        public StockDailyValue FirstValue { get; set; }
    }

    public class InstrumentViewModel : NotifyPropertyChangedBase
    {
        static public IEnumerable<Groups> Groups => StockDictionary.Instance.GetValidGroups();

        private Groups group;
        public Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                    this.Lines = new ObservableCollection<LineViewModel>(GetLines());

                    OnPropertyChanged("Lines");
                }
            }
        }

        static public Array DataProviders => Enum.GetValues(typeof(DataProvider));

        private DataProvider dataProvider = DataProvider.ABC;
        public DataProvider DataProvider
        {
            get { return dataProvider; }
            set
            {
                if (value != dataProvider)
                {
                    dataProvider = value;
                    OnPropertyChanged("DataProvider");
                    this.Lines = new ObservableCollection<LineViewModel>(GetLines());

                    OnPropertyChanged("Lines");
                }
            }
        }

        private int nbStocks;
        public int NbStocks
        {
            get { return nbStocks; }
            set
            {
                if (value != nbStocks)
                {
                    nbStocks = value;
                    OnPropertyChanged("NbStocks");
                }
            }
        }

        private string runStatus = "Load";
        public string RunStatus
        {
            get { return runStatus; }
            set
            {
                if (value != runStatus)
                {
                    runStatus = value;
                    OnPropertyChanged("RunStatus");
                }
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private Visibility progressVisibility;
        public Visibility ProgressVisibility
        {
            get { return progressVisibility; }
            set
            {
                if (value != progressVisibility)
                {
                    progressVisibility = value;
                    OnPropertyChanged("ProgressVisibility");
                }
            }
        }
        public ObservableCollection<LineViewModel> Lines { get; set; }

        public InstrumentViewModel()
        {
            this.Lines = new ObservableCollection<LineViewModel>();

            this.SaxoUnderlyings = new ObservableCollection<SaxoUnderlyingViewModel>(SaxoUnderlying.Load().Select(s => new SaxoUnderlyingViewModel(s)));

            ProgressVisibility = Visibility.Collapsed;
        }

        private IEnumerable<LineViewModel> GetLines()
        {
            return dataProvider == DataProvider.All ?
                StockDictionary.Instruments.Values.Where(s => s.BelongsToGroupFull(this.group)).Select(s => new LineViewModel() { Instrument = s }) :
                StockDictionary.Instruments.Values.Where(s => s.Provider == dataProvider && s.BelongsToGroupFull(this.group)).Select(s => new LineViewModel() { Instrument = s });
        }

        private bool canceled = false;
        public async Task CalculateAsync()
        {
            if (ProgressVisibility == Visibility.Visible)
            {
                canceled = true;
                return;
            }
            else
            {
                this.RunStatus = "Cancel";
                canceled = false;
            }
            ProgressVisibility = Visibility.Visible;
            this.Progress = 0;

            Lines.Clear();
            OnPropertyChanged("Lines");
            await Task.Delay(10);

            try
            {
                var lines = GetLines().ToList();
                this.Progress = 0;
                this.NbStocks = lines.Count;
                int count = 0;
                int step = Math.Max(1, this.NbStocks / 100);
                foreach (var line in lines)
                {
                    if (canceled)
                    {
                        break;
                    }
                    count++;
                    if (step == 1 || count % step == 0)
                    {
                        this.Progress = count;

                        await Task.Delay(5);
                    }

                    var dataSerie = line.Instrument.GetDefaultDataSerie();
                    if (dataSerie != null && dataSerie.Count > 0)
                    {
                        line.LastValue = dataSerie.LastValue;
                        line.FirstValue = dataSerie.Values[0];
                    }
                    Lines.Add(line);
                }
            }
            catch (Exception exception)
            {
                StockLog.Write(exception);
                StockAnalyzerException.MessageBox(exception);
            }

            OnPropertyChanged("Lines");
            await Task.Delay(0);

            ProgressVisibility = Visibility.Collapsed;
            this.RunStatus = "Load";
        }

        public ObservableCollection<SaxoUnderlyingViewModel> SaxoUnderlyings { get; set; }

        private CommandBase saveCommand;
        public ICommand SaveCommand => saveCommand ??= new CommandBase(Save);

        private void Save()
        {
            SaxoUnderlying.Save(this.SaxoUnderlyings.Select(s => new SaxoUnderlying
            {
                Id = s.Id,
                SaxoName = s.SaxoName,
                InstrumentId = s.InstrumentId,
            }));
        }

        private CommandBase refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new CommandBase(Refresh);

        private void Refresh()
        {
            var jsonData = SaxoHttpClient.HttpGetFromSaxo("https://fr-be.structured-products.saxo/page-api/products/BE/activeProducts?locale=fr_BE");

            if (string.IsNullOrEmpty(jsonData))
            {
                MessageBox.Show("Error retrieving data from Saxo Turbo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var result = JsonSerializer.Deserialize<UnderlyingRoot>(jsonData);
                var underlyings = result?.data?.filters?.firstLevel?.underlying?.list?.Values?.ToList();

                foreach (var underlying in underlyings)
                {
                    if (this.SaxoUnderlyings.FirstOrDefault(u => u.Id == underlying.value) == null)
                    {
                        this.SaxoUnderlyings.Add(new SaxoUnderlyingViewModel(underlying.value, underlying.label));
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error parsing data from Saxo Turbo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        #region Instruments

        public List<SaxoInstrument> Instruments => InstrumentService.InstrumentCache;

        private CommandBase saveInstrumentsCommand;
        public ICommand SaveInstrumentsCommand => saveInstrumentsCommand ??= new CommandBase(SaveInstruments);

        private void SaveInstruments()
        {
            InstrumentService.SaveCache();
        }
        #endregion
    }
}
