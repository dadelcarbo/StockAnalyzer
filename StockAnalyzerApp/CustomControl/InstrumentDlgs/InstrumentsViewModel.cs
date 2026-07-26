using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockData.DataProviders.SaxoTurbos;
using StockAnalyzer.StockData.DataProviders.SaxoTurbos.ConfigDialog;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio.Saxo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        static public IEnumerable<Groups> Groups => StockDictionary.GetValidGroups();

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

            InitVariables();
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
            try
            {
                InstrumentDlg.Instance.Cursor = System.Windows.Forms.Cursors.WaitCursor;

                var jsonData = SaxoHttpClient.HttpGetFromSaxo("https://fr-be.structured-products.saxo/page-api/products/BE/activeProducts?locale=fr_BE");

                if (string.IsNullOrEmpty(jsonData))
                {
                    MessageBox.Show("Error retrieving data from Saxo Turbo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result = JsonSerializer.Deserialize<UnderlyingRoot>(jsonData);
                var underlyings = result?.data?.filters?.firstLevel?.underlying?.list?.Values?.ToList();

                if (underlyings == null)
                {
                    MessageBox.Show("Error parsing data from Saxo Turbo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Task.Delay(5000).Wait();

                // Detect Removed Underlyings
                var toBeRemoved = new List<SaxoUnderlyingViewModel>();
                foreach (var underlying in this.SaxoUnderlyings)
                {
                    if (underlyings.Any(u => u.value == underlying.Id))
                        continue;
                    toBeRemoved.Add(underlying);
                }
                foreach (var underlying in toBeRemoved)
                {
                    this.SaxoUnderlyings.Remove(underlying);
                }
                if (toBeRemoved.Count > 0)
                {
                    MessageBox.Show(toBeRemoved.Select(u => $"{u.Id} - {u.SaxoName}").Aggregate((i, j) => i + Environment.NewLine + j), "Removed Underlyings");
                }

                // Detect Added underlyings
                var toBeAdded = new List<SaxoUnderlyingViewModel>();
                foreach (var underlying in underlyings)
                {
                    if (this.SaxoUnderlyings.FirstOrDefault(u => u.Id == underlying.value) == null)
                    {
                        var newUnderlying = new SaxoUnderlyingViewModel(underlying.value, underlying.label);
                        toBeAdded.Add(newUnderlying);
                        this.SaxoUnderlyings.Add(newUnderlying);
                    }
                }

                if (toBeAdded.Count > 0)
                {
                    MessageBox.Show(toBeAdded.Select(u => $"{u.Id} - {u.SaxoName} - {u.InstrumentName}").Aggregate((i, j) => i + Environment.NewLine + j), "New Underlyings");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error parsing data from Saxo Turbo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InstrumentDlg.Instance.Cursor = System.Windows.Forms.Cursors.Default;
            }
        }

        #region Saxo Instruments

        public List<SaxoInstrument> Instruments => InstrumentService.InstrumentCache;

        private CommandBase bindInstrumentsCommand;
        public ICommand BindInstrumentsCommand => bindInstrumentsCommand ??= new CommandBase(BindInstruments);


        private StockInstrument selectedInstrument;
        public StockInstrument SelectedInstrument { get => selectedInstrument; set => SetProperty(ref selectedInstrument, value); }

        private string selectedInstrumentId;
        public string SelectedInstrumentId
        {
            get => selectedInstrumentId;
            set
            {
                if (value != selectedInstrumentId)
                {
                    this.selectedInstrumentId = value;
                    if (selectedInstrumentId != null)
                    {
                        StockDictionary.Instruments.TryGetValue(selectedInstrumentId, out this.selectedInstrument);
                        this.OnPropertyChanged(nameof(SelectedInstrument));
                    }
                    else
                    {
                        this.SelectedInstrument = null;
                    }
                    this.OnPropertyChanged(nameof(SelectedInstrumentId));
                }
            }
        }

        private long selectedSaxoId;
        public long SelectedSaxoId
        {
            get => selectedSaxoId;
            set
            {
                if (this.selectedSaxoId != value)
                {
                    this.selectedSaxoId = value;
                    var instrument = SaxoToInstrumentMapping.GetInstrument(this.selectedSaxoId);

                    this.SelectedInstrumentId = instrument?.Id;

                    this.OnPropertyChanged(nameof(SelectedSaxoId));
                }
            }
        }

        private void BindInstruments()
        {
            if (this.SelectedInstrument == null)
            {
                return;
            }

            if (MessageBox.Show($"Binding Saxo:{SelectedSaxoId} with {SelectedInstrument.DisplayName}", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SaxoToInstrumentMapping.AddMapping(this.SelectedSaxoId, this.SelectedInstrumentId);

                this.OnPropertyChanged(nameof(this.SaxoMappings));
                this.OnPropertyChanged(nameof(this.Instruments));
            }

        }
        #endregion

        #region Instrument Mappings

        InstrumentService instrumentService = new InstrumentService();
        public IEnumerable<SaxoMappingViewModel> SaxoMappings => SaxoToInstrumentMapping.GetSaxoToInstrumentMappings()
            .Select(m => new SaxoMappingViewModel
            {
                SaxoInstrument = instrumentService.GetInstrumentById(m.Key),
                Instrument = StockDictionary.Instruments.ContainsKey(m.Value) ? StockDictionary.Instruments[m.Value] : null
            });

        #endregion

        #region Saxo POSTMAN

        private string serviceUrl = "chart/v3/charts?AssetType=Stock&Horizon=120&Uic=13185";
        public string ServiceUrl
        {
            get => serviceUrl; set
            {
                SetProperty(ref serviceUrl, value);

                // Extract all variables between { and }
                var matches = Regex.Matches(serviceUrl, @"\{([^}]+)\}").Cast<Match>().Select(match => match.Groups[1].Value);

                var transient = Variables.Where(v => v.Persist == false).ToList();
                foreach (var v in transient)
                {
                    if (!matches.Contains(v.Name))
                        this.Variables.Remove(v);
                }

                // Create a list of Variable objects
                foreach (var match in matches.Where(m => !Variables.Any(v => v.Name == m)))
                {
                    Variables.Add(new Variable
                    {
                        Name = match,
                        Value = string.Empty
                    });
                }

            }
        }

        private string httpResult;
        public string HttpResult { get => httpResult; set => SetProperty(ref httpResult, value); }

        public ObservableCollection<Variable> Variables { get; set; } = new ObservableCollection<Variable>();

        private CommandBase httpGetCommand;
        public ICommand HttpGetCommand => httpGetCommand ??= new CommandBase(HttpGet);

        private void HttpGet()
        {
            try
            {
                if (string.IsNullOrEmpty(serviceUrl))
                {
                    this.HttpResult = "Invalid service Url";
                    return;
                }
                var service = serviceUrl;
                foreach (var variable in Variables)
                {
                    service = service.Replace("{" + variable.Name + "}", variable.Value);
                }
                var result = TestSaxoService.HttpGet(service);
                JsonDocument jsonDocument = JsonDocument.Parse(result);

                // Write to a stream with indentation
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                    {
                        jsonDocument.WriteTo(writer);
                    }
                    this.HttpResult = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                this.HttpResult = ex.Message;
            }
        }

        void InitVariables()
        {
            this.Variables.Add(new Variable("AssetTypes", "MutualFund%2CCertificateUncappedCapitalProtection%2CCertificateCappedCapitalProtected%2CCertificateDiscount%2CCertificateCappedOutperformance%2CCertificateCappedBonus%2CCertificateExpress%2CCertificateTracker%2CCertificateUncappedOutperformance%2CCertificateBonus%2CCertificateConstantLeverage%2CStock%2CEtf%2CEtc%2CEtn%2CFund%2CRights%2CMiniFuture%2CWarrantKnockOut%2CWarrantOpenEndKnockOut%2CWarrantDoubleKnockOut%2CIpoOnStock%2CCompanyWarrant%2CStockIndex", true));
            this.Variables.Add(new Variable("ClientKey", TestSaxoService.GetClientKey(), true));
            this.Variables.Add(new Variable("AccountKey", TestSaxoService.GetAccountKey(), true));
        }


        #endregion
    }

    public class Variable
    {
        public Variable()
        {

        }
        public Variable(string name, string value, bool persist)
        {
            Name = name;
            Value = value;
            this.Persist = persist;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public bool Persist { get; set; }
    }
}
