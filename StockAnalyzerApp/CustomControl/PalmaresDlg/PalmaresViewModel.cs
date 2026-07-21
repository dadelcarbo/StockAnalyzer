using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockData;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockScripting;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public enum Operator
    {
        No,
        LT,
        GT
    }

    public class PalmaresViewModel : NotifyPropertyChangedBase
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
                    this.Lines?.Clear();
                    OnPropertyChanged("Lines");
                    OnPropertyChanged("ExportEnabled");
                }
            }
        }

        private BarDuration barDuration;
        public BarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (value != barDuration)
                {
                    barDuration = value; OnPropertyChanged("BarDuration");
                }
            }
        }

        private string indicator1;
        public string Indicator1
        {
            get { return indicator1; }
            set
            {
                if (value != indicator1)
                {
                    indicator1 = value;
                    OnPropertyChanged("Indicator1");
                }
            }
        }

        private float indicator1Min;
        public float Indicator1Min
        {
            get { return indicator1Min; }
            set
            {
                if (value != indicator1Min)
                {
                    indicator1Min = value;
                    OnPropertyChanged("Indicator1Min");
                }
            }
        }

        private Operator indicator1Operator;
        public Operator Indicator1Operator { get => indicator1Operator; set { SetProperty(ref indicator1Operator, value); } }

        private Operator indicator2Operator;
        public Operator Indicator2Operator { get => indicator2Operator; set { SetProperty(ref indicator2Operator, value); } }

        private Operator indicator3Operator;
        public Operator Indicator3Operator { get => indicator3Operator; set { SetProperty(ref indicator3Operator, value); } }


        private string indicator2;
        public string Indicator2
        {
            get { return indicator2; }
            set
            {
                if (value != indicator2)
                {
                    indicator2 = value;
                    OnPropertyChanged("Indicator2");
                }
            }
        }
        private float indicator2Min;
        public float Indicator2Min
        {
            get { return indicator2Min; }
            set
            {
                if (value != indicator2Min)
                {
                    indicator2Min = value;
                    OnPropertyChanged("Indicator2Min");
                }
            }
        }
        private string indicator3;
        public string Indicator3
        {
            get { return indicator3; }
            set
            {
                if (value != indicator3)
                {
                    indicator3 = value;
                    OnPropertyChanged("Indicator3");
                }
            }
        }
        private float indicator3Min;
        public float Indicator3Min
        {
            get { return indicator3Min; }
            set
            {
                if (value != indicator3Min)
                {
                    indicator3Min = value;
                    OnPropertyChanged("Indicator3Min");
                }
            }
        }

        private bool athOnly = false;
        public bool AthOnly
        {
            get { return athOnly; }
            set
            {
                if (value != athOnly)
                {
                    athOnly = value;
                    OnPropertyChanged("AthOnly");
                }
            }
        }



        private int ath1 = 175;
        public int Ath1
        {
            get { return ath1; }
            set
            {
                if (value != ath1)
                {
                    ath1 = value;
                    OnPropertyChanged("Ath1");
                }
            }
        }

        private int ath2 = 35;
        public int Ath2
        {
            get { return ath2; }
            set
            {
                if (value != ath2)
                {
                    ath2 = value;
                    OnPropertyChanged("Ath2");
                }
            }
        }

        private int stok = 35;
        public int Stok
        {
            get { return stok; }
            set
            {
                if (value != stok)
                {
                    stok = value;
                    OnPropertyChanged("Stok");
                }
            }
        }
        private float stokMin = 35;
        public float StokMin
        {
            get { return stokMin; }
            set
            {
                if (value != stokMin)
                {
                    stokMin = value;
                    OnPropertyChanged("StokMin");
                }
            }
        }

        private Operator stokOperator;
        public Operator StokOperator { get => stokOperator; set { SetProperty(ref stokOperator, value); } }

        private bool bullOnly = true;
        public bool BullOnly
        {
            get { return bullOnly; }
            set
            {
                if (value != bullOnly)
                {
                    bullOnly = value;
                    OnPropertyChanged("BullOnly");
                }
            }
        }
        private string stop;
        public string Stop
        {
            get { return stop; }
            set
            {
                if (value != stop)
                {
                    stop = value;
                    OnPropertyChanged("Stop");
                }
            }
        }
        private bool screenerOnly;
        public bool ScreenerOnly
        {
            get { return screenerOnly; }
            set
            {
                if (value != screenerOnly)
                {
                    screenerOnly = value;
                    OnPropertyChanged("ScreenerOnly");
                }
            }
        }

        private StockScript screener;
        public StockScript Screener
        {
            get { return screener; }
            set
            {
                if (value != screener)
                {
                    screener = value;
                    OnPropertyChanged("Screener");
                }
            }
        }

        public IEnumerable<StockScript> Screeners => new List<StockScript>() { null }.Union(StockScriptManager.Instance.StockScripts);

        private float liquidity;
        public float Liquidity { get { return liquidity; } set { if (liquidity != value) { liquidity = value; OnPropertyChanged("Liquidity"); } } }


        private string theme;
        public string Theme
        {
            get { return theme; }
            set
            {
                if (value != theme)
                {
                    theme = value;
                    OnPropertyChanged("Theme");
                }
            }
        }

        static public ObservableCollection<string> Settings { get; } = new ObservableCollection<string>(Directory.EnumerateFiles(Folders.Palmares).Select(s => Path.GetFileNameWithoutExtension(s)).OrderBy(s => s));

        private string setting;
        public string Setting
        {
            get { return setting; }
            set
            {
                if (value != setting)
                {
                    setting = value;
                    OnPropertyChanged("Setting");
                }
            }
        }

        private bool isReportable;
        public bool IsReportable
        {
            get { return isReportable; }
            set
            {
                if (value != isReportable)
                {
                    isReportable = value;
                    OnPropertyChanged("IsReportable");
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

        private string runStatus = "Run";
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

        public bool ExportEnabled => this.Lines != null && this.Lines.Count > 0;

        public bool DownloadIntraday { get; set; }

        public ObservableCollection<PalmaresLine> Lines { get; set; }

        public IEnumerable<string> Themes { get; set; }

        public PalmaresViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            this.Lines = new ObservableCollection<PalmaresLine>();

            this.Setting = Settings.FirstOrDefault();

            this.Themes = StockAnalyzerForm.MainFrame.Themes.Append(string.Empty);
            this.Theme = MainFrameViewModel.Instance.Theme;
            if (this.Theme.Contains("*"))
                this.Theme = this.Themes.FirstOrDefault();

            DownloadIntraday = false;
            ProgressVisibility = Visibility.Collapsed;
        }

        private bool canceled = false;
        public async Task CalculateAsync()
        {
            try
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

                #region Sanity Check
                IStockIndicator viewableSeries1 = null;
                if (!string.IsNullOrEmpty(this.indicator1))
                {
                    try
                    {
                        viewableSeries1 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator1) as IStockIndicator;
                        if (viewableSeries1 == null)
                        {
                            MessageBox.Show($"Indicator1: {this.indicator1} nout found", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                    catch { }
                }
                IStockIndicator viewableSeries2 = null;
                if (!string.IsNullOrEmpty(this.indicator2))
                {
                    try
                    {
                        viewableSeries2 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator2) as IStockIndicator;
                        if (viewableSeries2 == null)
                        {
                            MessageBox.Show($"Indicator2: {this.indicator2} nout found", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                    catch { }
                }
                IStockIndicator viewableSeries3 = null;
                if (!string.IsNullOrEmpty(this.indicator3))
                {
                    try
                    {
                        viewableSeries3 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator3) as IStockIndicator;
                        if (viewableSeries3 == null)
                        {
                            MessageBox.Show($"Indicator3: {this.indicator3} not found !", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                    catch { }
                }
                IStockTrailStop trailStopSerie = null;
                if (!string.IsNullOrEmpty(this.stop))
                {
                    try
                    {
                        trailStopSerie = StockViewableItemsManager.GetViewableItem("TRAILSTOP|" + this.stop) as IStockTrailStop;
                    }
                    catch { }
                }
                IStockFilter screenerSerie = null;
                if (this.screener != null)
                {
                    try
                    {
                        screenerSerie = StockScriptManager.Instance.CreateStockFilterInstance(screener);

                    }
                    catch { }
                }
                #endregion
                if (this.DownloadIntraday)
                {
                    throw new NotImplementedException("Intraday download in Palmares");
                }

                Lines = new ObservableCollection<PalmaresLine>();
                OnPropertyChanged("Lines");
                await Task.Delay(10);

                var instruments = StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(this.group)).ToList();
                if (instruments.Count == 0)
                {
                    MessageBox.Show($"No instruments found in group {this.group}", "Error", MessageBoxButton.OK);
                    return;
                }
                this.Progress = 0;
                this.NbStocks = instruments.Count;
                int count = 0;
                foreach (var instrument in instruments)
                {
                    if (canceled)
                    {
                        break;
                    }
                    count++;
                    if (count % 10 == 0)
                        this.Progress = count;
                    if (this.DownloadIntraday && (this.group == StockAnalyzer.StockData.Groups.TURBO_5M || this.group == StockAnalyzer.StockData.Groups.TURBO))
                    {
                        throw new NotImplementedException("DownloadIntraday for TURBO not implemented yet");
                        //StockDataProviderBase.DownloadSerieData(instrument);
                    }

                    var dataSerie = instrument.GetDataSerie(this.BarDuration);

                    if (dataSerie == null || dataSerie.Count < Math.Max(100, this.Stok))
                    {
                        continue;
                    }

                    var closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
                    var lowSerie = dataSerie.GetSerie(StockDataType.LOW);
                    var highSerie = dataSerie.GetSerie(StockDataType.HIGH);
                    var openSerie = dataSerie.GetSerie(StockDataType.OPEN);

                    var endIndex = dataSerie.LastIndex;
                    var lastBar = dataSerie.LastValue;

                    if (Liquidity >= 0 && Liquidity > lastBar.EXCHANGED / 1000000f)
                    {
                        continue;
                    }

                    #region Calculate ATH
                    if (athOnly)
                    {
                        bool athFound = false;
                        for (int i = 0; !athFound && i <= ath2; i++)
                        {
                            athFound = closeSerie.GetHighestIn(endIndex - i) >= ath1;
                        }
                        if (!athFound)
                            continue;
                    }
                    #endregion

                    #region Calculate Stok
                    float stokValue = 0;
                    if (stok > 0)
                    {
                        float lowest = 0f;
                        float highest = 0f;
                        closeSerie.GetMinMax(endIndex - stok, endIndex, ref lowest, ref highest);

                        stokValue = 100 * (closeSerie[endIndex] - lowest) / (highest - lowest);

                        if (stokOperator != Operator.No)
                        {
                            if (stokOperator == Operator.LT && stokValue > stokMin) continue;
                            if (stokOperator == Operator.GT && stokValue < stokMin) continue;
                        }
                    }
                    #endregion

                    float lastValue = lastBar.CLOSE;
                    var firstValue = closeSerie[endIndex - 1];
                    float barVariation = (lastValue - firstValue) / firstValue;

                    float stopValue = float.NaN;
                    if (trailStopSerie != null)
                    {
                        try
                        {
                            trailStopSerie = dataSerie.GetTrailStop(this.stop);
                            stopValue = trailStopSerie.Series[0][endIndex];
                            if (float.IsNaN(stopValue))
                            {
                                if (bullOnly)
                                    continue;
                                stopValue = trailStopSerie.Series[1][endIndex];
                            }
                            if (!float.IsNaN(stopValue))
                            {
                                stopValue = (lastValue - stopValue) / lastValue;
                            }
                        }
                        catch { }
                    }
                    bool match = true;
                    if (screenerSerie != null)
                    {
                        match = screenerSerie.MatchFilter(dataSerie);
                        if (screenerOnly && !match)
                        {
                            continue;
                        }
                    }

                    int highestIn = lastBar.VARIATION > 0 ? closeSerie.GetHighestIn(endIndex) : 0;

                    #region Calculate Indicators
                    float stockIndicator1 = float.NaN;
                    if (viewableSeries1 != null)
                    {
                        if (Indicator1.StartsWith("RO"))
                            stockIndicator1 = dataSerie.CalculateLastROx(Indicator1, (int)viewableSeries1.Parameters[0]);
                        else
                            try { viewableSeries1.ApplyTo(dataSerie); stockIndicator1 = viewableSeries1.Series[0][endIndex]; } catch { }

                        if (stockIndicator1 != float.NaN && indicator1Operator != Operator.No)
                        {
                            if (indicator1Operator == Operator.LT && stockIndicator1 > indicator1Min) continue;
                            if (indicator1Operator == Operator.GT && stockIndicator1 < indicator1Min) continue;
                        }
                    }
                    float stockIndicator2 = float.NaN;
                    if (viewableSeries2 != null)
                    {
                        if (Indicator2.StartsWith("RO"))
                            stockIndicator2 = dataSerie.CalculateLastROx(Indicator2, (int)viewableSeries2.Parameters[0]);
                        else
                            try { viewableSeries2.ApplyTo(dataSerie); stockIndicator2 = viewableSeries2.Series[0][endIndex]; } catch { }

                        if (stockIndicator2 != float.NaN && indicator2Operator != Operator.No)
                        {
                            if (indicator2Operator == Operator.LT && stockIndicator2 > indicator2Min) continue;
                            if (indicator2Operator == Operator.GT && stockIndicator2 < indicator2Min) continue;
                        }
                    }
                    float stockIndicator3 = float.NaN;
                    if (viewableSeries3 != null)
                    {
                        if (Indicator3.StartsWith("RO"))
                            stockIndicator3 = dataSerie.CalculateLastROx(Indicator3, (int)viewableSeries3.Parameters[0]);
                        else
                            try { viewableSeries3.ApplyTo(dataSerie); stockIndicator3 = viewableSeries3.Series[0][endIndex]; } catch { }

                        if (stockIndicator3 != float.NaN && indicator3Operator != Operator.No)
                        {
                            if (indicator3Operator == Operator.LT && stockIndicator3 > indicator3Min) continue;
                            if (indicator3Operator == Operator.GT && stockIndicator3 < indicator3Min) continue;
                        }
                    }
                    #endregion

                    Lines.Add(new PalmaresLine
                    {
                        Match = match,
                        Instrument = instrument,
                        Group = instrument.Group.ToString(),
                        Symbol = instrument.Symbol,
                        Name = instrument.DisplayName,
                        Value = lastValue,
                        Highest = highestIn,
                        Exchanged = lastBar.EXCHANGED,
                        Indicator1 = stockIndicator1,
                        Indicator2 = stockIndicator2,
                        Indicator3 = stockIndicator3,
                        Stok = stokValue,
                        Stop = stopValue,
                        BarVariation = barVariation,
                        LastDate = lastBar.DATE
                    });
                }

            }
            catch (Exception exception)
            {
                StockLog.Write(exception);
                StockAnalyzerException.MessageBox(exception);
            }
            finally
            {
                OnPropertyChanged("Lines");
                OnPropertyChanged("ExportEnabled");
                await Task.Delay(0);

                ProgressVisibility = Visibility.Collapsed;
                this.RunStatus = "Run";
            }
        }

        public static PalmaresSettings LoadSettings(string palmaresSetting)
        {
            string fileName = Path.Combine(Folders.Palmares, palmaresSetting + ".xml");
            if (File.Exists(fileName))
            {
                using FileStream fs = new FileStream(fileName, FileMode.Open);
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(PalmaresSettings));
                return (PalmaresSettings)serializer.Deserialize(xmlReader);
            }
            return null;
        }

        public PalmaresSettings LoadSettings()
        {
            string fileName = Path.Combine(Folders.Palmares, this.Setting + ".xml");
            if (File.Exists(fileName))
            {
                using FileStream fs = new FileStream(fileName, FileMode.Open);
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(PalmaresSettings));
                var palmaresSettings = (PalmaresSettings)serializer.Deserialize(xmlReader);

                this.Group = palmaresSettings.Group;
                this.BarDuration = palmaresSettings.BarDuration;

                this.Indicator1 = palmaresSettings.Indicator1;
                this.Indicator1Min = palmaresSettings.Indicator1Min;
                this.Indicator1Operator = palmaresSettings.Indicator1Operator;

                this.Indicator2 = palmaresSettings.Indicator2;
                this.Indicator2Min = palmaresSettings.Indicator2Min;
                this.Indicator2Operator = palmaresSettings.Indicator2Operator;

                this.Indicator3 = palmaresSettings.Indicator3;
                this.Indicator3Min = palmaresSettings.Indicator3Min;
                this.Indicator3Operator = palmaresSettings.Indicator3Operator;

                this.AthOnly = palmaresSettings.AthOnly;
                this.Ath1 = palmaresSettings.Ath1;
                this.Ath2 = palmaresSettings.Ath2;

                this.Stok = palmaresSettings.Stok;
                this.StokMin = palmaresSettings.StokMin;
                this.StokOperator = palmaresSettings.StokOperator;

                this.ScreenerOnly = palmaresSettings.ScreenerOnly;
                this.Screener = StockScriptManager.Instance.StockScripts?.FirstOrDefault(s => s.Name == palmaresSettings.Screener);
                this.Stop = palmaresSettings.Stop;
                this.BullOnly = palmaresSettings.BullOnly;
                this.Liquidity = palmaresSettings.Liquidity;

                this.IsReportable = palmaresSettings.IsReportable;

                if (StockAnalyzerForm.MainFrame.Themes.Contains(palmaresSettings.Theme) || string.IsNullOrEmpty(palmaresSettings.Theme))
                    this.Theme = palmaresSettings.Theme;
                else
                {
                    this.Theme = null;
                    MessageBox.Show($"Theme '{palmaresSettings.Theme}' doen't exist !", "Error");
                }

                return palmaresSettings;
            }
            return null;
        }
    }
}
