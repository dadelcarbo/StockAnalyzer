using Saxo.OpenAPI.AuthenticationServices;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockPortfolio.AutoTrade;
using StockAnalyzerApp.CustomControl;
using StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog;
using StockAnalyzerApp.CustomControl.AutoTradeDlg;
using StockAnalyzerApp.CustomControl.ColorPalette;
using StockAnalyzerApp.CustomControl.DrawingDlg;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.CustomControl.IndicatorDlgs;
using StockAnalyzerApp.CustomControl.InstrumentDlgs;
using StockAnalyzerApp.CustomControl.MarketReplay;
using StockAnalyzerApp.CustomControl.PalmaresControl;
using StockAnalyzerApp.CustomControl.PalmaresDlg;
using StockAnalyzerApp.CustomControl.PortfolioDlg;
using StockAnalyzerApp.CustomControl.PortfolioDlg.SaxoPortfolioDlg;
using StockAnalyzerApp.CustomControl.SectorDlg;
using StockAnalyzerApp.CustomControl.SimulationDlgs;
using StockAnalyzerApp.CustomControl.SplitDlg;
using StockAnalyzerApp.CustomControl.TrendDlgs;
using StockAnalyzerApp.CustomControl.WatchlistDlgs;
using StockAnalyzer.StockData;
using StockAnalyzerApp.StockScripting;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Serialization;
using Telerik.Charting;
using Telerik.Windows.Data;
using Match = System.Text.RegularExpressions.Match;

namespace StockAnalyzerApp
{
    public partial class StockAnalyzerForm : Form
    {
        public delegate void SelectedInstrumentChangedEventHandler(StockInstrument instrument, bool activateMainWindow);

        public delegate void SelectedInstrumentAndDurationChangedEventHandler(StockInstrument instrument, BarDuration barDuration, bool activateMainWindow);

        public delegate void SelectedInstrumentAndDurationAndThemeChangedEventHandler(StockInstrument instrument, BarDuration barDuration, string theme, bool activateMainWindow);

        public delegate void SelectedInstrumentAndDurationAndIndexChangedEventHandler(StockInstrument instrument, int startIndex, int endIndex, BarDuration barDuration, bool activateMainWindow);

        public delegate void SelectedStockGroupChangedEventHandler(Groups stockgroup);

        public delegate void SelectedStrategyChangedEventHandler(string strategyName);

        public delegate void NotifySelectedThemeChangedEventHandler(Dictionary<string, List<string>> theme);

        public delegate void NotifyStrategyChangedEventHandler(string newStrategy);

        public delegate void StockWatchListsChangedEventHandler();

        public delegate void AlertDetectedHandler();
        public event AlertDetectedHandler AlertDetected;

        public delegate void AlertDetectionStartedHandler(int nbStock, string alertTitle);
        public event AlertDetectionStartedHandler AlertDetectionStarted;

        public delegate void AlertDetectionProgressHandler(string StockName);
        public event AlertDetectionProgressHandler AlertDetectionProgress;

        public static StockAnalyzerForm MainFrame { get; private set; }
        public MainFrameViewModel ViewModel { get; private set; }
        public bool IsClosing { get; set; }

        private const string HTML_RESOURCES_FOLDER = @"Resources\Html";

        public static CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-GB");
        public static CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        public static CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public List<StockPortfolio> Portfolios => PortfolioDataProvider.Portfolios;

        public ToolStripProgressBar ProgressBar => this.progressBar;

        public GraphCloseControl GraphCloseControl => this.graphCloseControl;

        private StockPortfolio portfolio;
        public StockPortfolio Portfolio
        {
            get => portfolio;
            set
            {
                if (portfolio != value)
                {
                    if (portfolioComboBox.SelectedItem != value)
                    {
                        portfolioComboBox.SelectedIndex = portfolioComboBox.Items.IndexOf(value);
                    }
                    else
                    {
                        portfolio = value;
                        this.graphCloseControl.ForceRefresh();
                    }
                    if (portfolio == null)
                        return;

                    // Update Connectivity Status
                    if (portfolio.SaxoSilentLogin())
                    {
                        portfolio.Refresh();
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.GreenIcon;
                        this.portfolioStatusLbl.ToolTipText = "Connected";
                    }
                    else
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.RedIcon;
                        this.portfolioStatusLbl.ToolTipText = "Not Connected";
                    }
                }
                else
                {
                    this.graphCloseControl.ForceRefresh();
                }
            }
        }

        private static int NbBars { get; set; }

        private int startIndex = 0;
        private int endIndex = 0;

        private readonly List<GraphControl> graphList = new List<GraphControl>();

        #region CONSTANTS

        private static readonly string WORK_THEME = "__NewTheme*";

        private static readonly string EMPTY_THEME = "_Empty";
        private static readonly string REPORT_THEME = "_Report";

        #endregion
        #region STARTUP methods

        public StockAnalyzerForm()
        {
            InitializeComponent();

            this.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.Controls.Clear();

            if (Settings.Default.StockToolbarLocation == new Point(-1, -1))
            {
                // No location registered, initialisae default one.
                this.browseToolStrip.Location = new Point(3, 0);
                this.drawToolStrip.Location = new Point(3, 25);
                this.themeToolStrip.Location = new Point(3 + this.drawToolStrip.Bounds.Width, 25);
            }
            else
            {
                this.browseToolStrip.Location = Settings.Default.StockToolbarLocation;
                this.drawToolStrip.Location = Settings.Default.drawingToolbarLocation;
                this.themeToolStrip.Location = Settings.Default.ThemeToolbarLocation;
            }

            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.browseToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.drawToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.themeToolStrip);

            //this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.ResumeLayout();
            this.PerformLayout();

            MainFrame = this;

            this.ViewModel = MainFrameViewModel.Instance;
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.IsClosing = false;

            // Add indicator1Name into the indicators controls layout panel
            this.graphScrollerControl.IsCollapsed = false;
            this.graphScrollerControl.SizeRatio = 40;
            this.graphScrollerControl.RatioType = RatioType.Absolute;

            this.graphCloseControl.IsCollapsed = false;
            this.graphCloseControl.SizeRatio = 5;
            this.graphCloseControl.RatioType = RatioType.Ratio;

            this.graphIndicator1Control.IsCollapsed = false;
            this.graphIndicator1Control.SizeRatio = 60;
            this.graphIndicator1Control.RatioType = RatioType.Absolute;

            this.graphIndicator2Control.IsCollapsed = false;
            this.graphIndicator2Control.SizeRatio = 60;
            this.graphIndicator2Control.RatioType = RatioType.Absolute;

            this.graphIndicator3Control.IsCollapsed = false;
            this.graphIndicator3Control.SizeRatio = 60;
            this.graphIndicator3Control.RatioType = RatioType.Absolute;

            this.graphVolumeControl.IsCollapsed = false;
            this.graphVolumeControl.SizeRatio = 60;
            this.graphVolumeControl.RatioType = RatioType.Absolute;

            // Fill the control list
            this.graphList.Add(this.graphScrollerControl);
            this.graphList.Add(this.graphCloseControl);
            this.graphList.Add(this.graphIndicator1Control);
            this.graphList.Add(this.graphIndicator2Control);
            this.graphList.Add(this.graphIndicator3Control);
            this.graphList.Add(this.graphVolumeControl);

            indicatorLayoutPanel.SetRows(this.graphList);

            GraphControl.DrawingPen = GraphCurveType.PenFromString(Settings.Default.DrawingPen);
            CupHandle2D.PivotBrush = new SolidBrush(GraphControl.DrawingPen.Color);

            this.graphCloseControl.HideIndicators = false;
            this.FormClosing += new FormClosingEventHandler(StockAnalyzerForm_FormClosing);

            StockDictionary.Initialize(new DateTime(DateTime.Now.Year, 01, 01));

            StockDictionary.Instance.ReportProgress += new StockDictionary.ReportProgressHandler(StockDictionary_ReportProgress);

            NbBars = Settings.Default.DefaultBarNumber;

            Settings.Default.PropertyChanged += (sender, args) => Settings.Default.Save();

            previousState = this.WindowState;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BarDuration":
                    this.ApplyTheme();
                    break;
                case "Instrument":
                    OnInstrumentChanged();
                    break;
                case "Theme":
                    StockAnalyzerForm_ThemeChanged();
                    break;
                default:
                    StockLog.Write("Unhandled property changed: " + e.PropertyName);
                    break;
            }
        }

        #region INSTRUMENT MANAGEMENT
        private void OnInstrumentChanged()
        {
            StockLog.Write($"Instrument changed to {this.ViewModel.Instrument?.DisplayName}");
            using (new MethodLogger(this))
            {
                //
                if (this.ViewModel.Instrument?.DisplayName == null)
                {
                    DeactivateGraphControls("No data to display");
                    this.Text = "Ultimate Chartist - " + "No stock selected";
                    return;
                }

                #region Set Window Title
                string id;
                if (ViewModel.Instrument.Symbol == ViewModel.Instrument.DisplayName)
                {
                    id = ViewModel.Instrument.DisplayName + "-" + ViewModel.Instrument.DisplayName;
                }
                else
                {
                    id = ViewModel.Instrument.Group + "-" + ViewModel.Instrument.Symbol + " - " + ViewModel.Instrument.DisplayName;
                }
                if (!string.IsNullOrWhiteSpace(this.ViewModel.Instrument.Isin))
                {
                    id += " - " + this.ViewModel.Instrument.Isin;
                }
                id += " - " + ViewModel.Instrument.Provider;
                this.Text = "Ultimate Chartist - " + this.ViewModel.AnalysisFile.Split('\\').Last() + " - " + id;
                #endregion

                if (ViewModel.Instrument.BelongsToGroup(Groups.TURBO) || ViewModel.Instrument.BelongsToGroup(Groups.TURBO_5M))
                {
                    this.statusLabel.Text = ("Downloading data not implemented...");
                    this.Refresh();
                    this.Cursor = Cursors.WaitCursor;
                }

                this.InitialiseBarDurationComboBox();

                var dataSerie = ViewModel.Instrument.GetDataSerie(this.ViewModel.BarDuration);
                if (dataSerie == null || dataSerie.Count == 0)
                {
                    DeactivateGraphControls("No data to display");
                    return;
                }
                if (dataSerie.Count < MIN_BAR_DISPLAY)
                {
                    DeactivateGraphControls("Not enough data to display");
                    return;
                }

                // this.currentStockSerie = this.ViewModel.Instrument.StockSerie;


                ApplyTheme();
            }

            Settings.Default.LastInstrument = this.ViewModel.Instrument.Id;
        }
        #endregion

        #region BAR DURATION MANAGEMENT

        private void InitialiseBarDurationComboBox()
        {
            object[] durations = this.ViewModel?.Instrument == null ? StockBarDuration.BarDurationArray : this.ViewModel.Instrument.SupportedDurations.Cast<object>().ToArray();

            bool needUpdate = false;
            if (durations.Length == this.barDurationComboBox.Items.Count)
            {
                for (int i = 0; i < durations.Length; i++)
                {
                    if ((BarDuration)durations[i] != (BarDuration)this.barDurationComboBox.Items[i])
                    {
                        needUpdate = true;
                        break;
                    }
                }
            }
            else
            {
                needUpdate = true;
            }

            if (needUpdate)
            {
                this.barDurationComboBox.Items.Clear();
                this.barDurationComboBox.Items.AddRange(durations);
                SetBarDurationCombo((BarDuration)durations[0]);
            }

            if (!durations.Contains(this.ViewModel.BarDuration))
            {
                SetBarDurationCombo((BarDuration)durations[0]);
                this.ViewModel.SetBarDuration((BarDuration)durations[0], false);
            }
        }

        /// <summary>
        /// Event received when User change duration from Main UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarDurationChanged(object sender, EventArgs e)
        {
            using (new MethodLogger(this))
            {
                this.ViewModel.BarDuration = (BarDuration)barDurationComboBox.SelectedItem;
            }
        }

        #endregion

        #region Activate Function
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_RESTORE = 0xF120;
        public new void Activate()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                SendMessage(this.Handle, WM_SYSCOMMAND, SC_RESTORE, IntPtr.Zero);
                this.ResetZoom();
            }
            base.Activate();
        }
        private FormWindowState previousState;
        protected override void OnClientSizeChanged(EventArgs e)
        {
            if (this.WindowState != previousState)
            {
                previousState = this.WindowState;
                OnWindowStateChanged(e);
            }
            base.OnClientSizeChanged(e);
        }
        protected void OnWindowStateChanged(EventArgs e)
        {
            using MethodLogger ml = new MethodLogger(this, false, $"{this.WindowState}");
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.ApplyTheme();
            }
        }

        #endregion

        protected override void OnShown(EventArgs e)
        {
            base.OnActivated(e);
            this.FormClosing += StockAnalyzerForm_FormClosing1;

            TradeEngine tradeEngine = TradeEngine.Instance;
        }

        private void StockAnalyzerForm_FormClosing1(object sender, FormClosingEventArgs e)
        {
            StockTimer.TimerSuspended = true;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = EnglishCulture;
            Thread.CurrentThread.CurrentCulture = EnglishCulture;

            System.Windows.FrameworkElement.LanguageProperty.OverrideMetadata
                (typeof(System.Windows.FrameworkElement),
                new System.Windows.FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Graphical initialisation
            StockSplashScreen.ProgressText = "Initialising folders";
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            // This is the first time the user runs the application.
            while (string.IsNullOrEmpty(Folders.DataFolder) || !Directory.Exists(Folders.DataFolder) || string.IsNullOrEmpty(Folders.PersonalFolder) || !Directory.Exists(Folders.PersonalFolder))
            {
                if (string.IsNullOrEmpty(Folders.DataFolder))
                {
                    Folders.DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"UltimateChartist\data");
                }
                if (string.IsNullOrEmpty(Folders.PersonalFolder))
                {
                    Folders.PersonalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UltimateChartist");
                }

                Settings.Default.UserId = Environment.UserName;
                PreferenceDialog prefDlg = new PreferenceDialog() { StartPosition = FormStartPosition.CenterScreen };
                DialogResult res = prefDlg.ShowDialog();
                if (res == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
            }

            Folders.CreateDirectories();

            // Copy Html Resources
            foreach (string file in Directory.GetFiles(HTML_RESOURCES_FOLDER))
            {
                string destFile = Path.Combine(Folders.Report, Path.GetFileName(file));
                if (!File.Exists(destFile) || File.GetLastWriteTime(destFile) < File.GetLastWriteTime(file))
                {
                    File.Copy(file, destFile, true); // Overwrite if exists
                }
            }

            StockSplashScreen.ProgressText = "Synchronizing One Drive...";
            StockSplashScreen.ProgressVal = 20;
#if !DEBUG
            OneDriveSync(true);
#endif

            StockSplashScreen.ProgressText = "Initialize stock dictionary...";
            StockSplashScreen.ProgressVal = 30;

            var download = Settings.Default.DownloadData && NetworkInterface.GetIsNetworkAvailable();

            DataProviderBase.DownloadStarted += Notifiy_SplashProgressChanged;
            DataProviderBase.Initialize(download);

            //StockDataProviderBase.InitStockDictionary(StockDictionary.Instance, download, new DownloadingStockEventHandler(Notifiy_SplashProgressChanged));

            //
            InitialiseThemeCombo();

            // Deserialize Drawing Items - Read Analysis files
            this.ViewModel.AnalysisFile = Path.Combine(Folders.PersonalFolder, "UltimateChartist.ulc");

            StockSplashScreen.ProgressText = "Reading Drawing items ...";
            LoadAnalysis(this.ViewModel.AnalysisFile);

            ABCDataProvider.AddToExcludedList(StockDictionary.Instruments.Values.Where(s => s.Provider == DataProvider.ABC && s.StockAnalysis.Excluded).Select(s => s.Isin));

            var cac40DataSerie = StockDictionary.GetInstrumentByName("CAC40").GetDefaultDataSerie();

            // Generate breadth 
            if (Settings.Default.GenerateBreadth)
            {
                throw new NotImplementedException("Breadth data generation not implemented yet");
                //foreach (var instrument in StockDictionary.Instruments.Values.Where(s => s.Provider == DataProvider.Breadth))
                //{
                //    StockSplashScreen.ProgressText = "Generating breadth data " + instrument.DisplayName;
                //    var dataSerie = instrument.GetDefaultDataSerie();
                //    if (!BreadthDataProvider.NeedGenerate)
                //        break;
                //}
            }

            // Deserialize saved orders
            StockSplashScreen.ProgressText = "Reading portfolio data...";

            var portfolioDataProvider = PortfolioDataProvider.GetDataProvider(StockDataProvider.Portfolio);
            portfolioDataProvider.InitDictionary(StockDictionary.Instance, false);
            InitialisePortfolioCombo();
            Portfolio = PortfolioDataProvider.Portfolios.First();

            // Initialise dico
            StockSplashScreen.ProgressText = "Initialising menu items...";

            // Update dynamic menu
            InitialiseBarDurationComboBox();
            CreateSecondarySerieMenuItem();

            // Update dynamic menu
            InitDataProviderMenuItem();

            // Watchlist menu item
            this.LoadWatchList();

            InitialiseWatchListComboBox();

            this.darkModeStripButton.Checked = Settings.Default.DarkMode;

            this.Show();
            this.progressBar.Value = 0;
            this.showShowStatusBarMenuItem.Checked = Settings.Default.ShowStatusBar;
            this.statusStrip1.Visible = Settings.Default.ShowStatusBar;
            this.showDrawingsMenuItem.Checked = Settings.Default.ShowDrawings;
            this.showEventMarqueeMenuItem.Checked = Settings.Default.ShowEventMarquee;
            this.showGridMenuItem.Checked = Settings.Default.ShowGrid;
            this.showIndicatorDivMenuItem.Checked = Settings.Default.ShowIndicatorDiv;
            this.showIndicatorTextMenuItem.Checked = Settings.Default.ShowIndicatorText;
            this.showVariationBtn.CheckState = Settings.Default.ShowVariation ? CheckState.Checked : CheckState.Unchecked;
            this.showOrdersMenuItem.Checked = Settings.Default.ShowOrders;
            this.showPositionsMenuItem.Checked = Settings.Default.ShowPositions;

            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(graphScrollerControl_ZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphCloseControl.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator2Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator3Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator1Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphVolumeControl.OnZoomChanged);

            GraphControl.IsStarted = true;

            // Initialise event call backs (because of a bug in the designer)
            this.graphCloseControl.MouseClick += new MouseEventHandler(graphCloseControl.GraphControl_MouseClick);
            this.graphScrollerControl.MouseClick += new MouseEventHandler(graphScrollerControl.GraphControl_MouseClick);
            this.graphIndicator2Control.MouseClick += new MouseEventHandler(graphIndicator2Control.GraphControl_MouseClick);
            this.graphIndicator3Control.MouseClick += new MouseEventHandler(graphIndicator3Control.GraphControl_MouseClick);
            this.graphIndicator1Control.MouseClick += new MouseEventHandler(graphIndicator1Control.GraphControl_MouseClick);
            this.graphVolumeControl.MouseClick += new MouseEventHandler(graphVolumeControl.GraphControl_MouseClick);

            foreach (var portfolio in this.Portfolios)
            {
                var logingStatus = portfolio.SaxoSilentLogin();
                if (this.portfolio == portfolio)
                {
                    if (logingStatus)
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.GreenIcon;
                        this.portfolioStatusLbl.ToolTipText = "Connected";
                    }
                    else
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.RedIcon;
                        this.portfolioStatusLbl.ToolTipText = "Not Connected";
                    }
                }
            }

            this.ViewModel.SetBarDuration(BarDuration.Daily, false);
            if (StockDictionary.Instruments.TryGetValue(Settings.Default.LastInstrument, out var lastInstrument))
            {
                this.ViewModel.Instrument = lastInstrument;
            }
            else
            {
                this.ViewModel.Instrument = cac40DataSerie.Instrument;
            }

            if (Settings.Default.GenerateDailyReport)
            {
                // Generate Template and watchlist reports
                await this.GenerateReports();

                // Generate report for alerts
                var fileName = Path.Combine(Folders.Report, "LastGeneration.txt");
                DateTime reportDate = DateTime.MinValue;
                if (File.Exists(fileName))
                {
                    reportDate = DateTime.Parse(File.ReadAllText(fileName), CultureInfo.InvariantCulture);
                }

                if (reportDate < cac40DataSerie.LastValue.DATE)
                {
                    showAlertDefDialogMenuItem_Click(this, null);
                    stockAlertManagerViewModel.RunFullAlert();

                    //GenerateReport(BarDuration.Daily);
                    //GenerateReport(BarDuration.Weekly);

                    File.WriteAllText(fileName, cac40DataSerie.LastValue.DATE.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Refresh intraday every 5 minutes.
            if (DateTime.Today.DayOfWeek != DayOfWeek.Sunday && DateTime.Today.DayOfWeek != DayOfWeek.Saturday)
            {
                var startTime = new TimeSpan(7, 55, 0);
                var endTime = new TimeSpan(22, 05, 0);

                // Checks for alert every x minutes according to bar duration.
                if (Settings.Default.RaiseAlerts)
                {
                    var timer = StockTimer.CreateAlertTimer(startTime, endTime, GenerateIntradayReport);
                }
                StockTimer.CreateRefreshTimer(startTime, endTime, new TimeSpan(0, 1, 0), RefreshTimer_Tick);
            }

            searchCombo.Items.AddRange(StockDictionary.Instruments.Where(p => !p.Value.StockAnalysis.Excluded).Select(p => p.Value.DisplayName).ToArray());

            // Ready to start
            StockSplashScreen.CloseForm(true);
            this.Focus();
        }

        private async void generateReportMenuItem_Click(object sender, EventArgs e)
        {
            await GenerateReports(true);
        }

        private async Task GenerateReports(bool force = false)
        {
            var currentSize = this.Size;
            var currentState = this.WindowState;
            GraphControl.GeneratingReport = true;

            bool showOrders = Settings.Default.ShowOrders;
            bool showPositions = Settings.Default.ShowPositions;

            Settings.Default.ShowOrders = false;
            Settings.Default.ShowPositions = false;

            using var cs = new MainViewModelContextPersister();

            try
            {
                if (currentState == FormWindowState.Maximized || currentState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                this.Size = new Size(500, 400);

                foreach (var reportTemplate in Directory.EnumerateFiles(Folders.ReportTemplates, "*.html"))
                {
                    GenerateReportFromTemplate(reportTemplate, force);
                }

                #region PALMARES REPORT

                this.Size = new Size(500, 600);

                var reportFileName = Path.Combine(Folders.Report, "Palmares.html");
                var reportDate = File.Exists(reportFileName) ? File.GetLastWriteTime(reportFileName) : DateTime.MinValue;
                if (force || reportDate.Date != DateTime.Today ||
                    reportDate <= File.GetLastWriteTime(Folders.PalmaresReportTemplate) ||
                    reportDate <= File.GetLastWriteTime(Folders.PalmaresItemTemplate))
                {
                    string palmaresItems = string.Empty;

                    foreach (var palmares in PalmaresViewModel.Settings)
                    {
                        var palmaresSettings = PalmaresViewModel.LoadSettings(palmares);
                        if (palmaresSettings != null && palmaresSettings.IsReportable)
                            palmaresItems += await GenerateReportFromPalmares(palmares);
                    }

                    var htmlReport = File.ReadAllText(Folders.PalmaresReportTemplate);
                    htmlReport = htmlReport.Replace("%%Title%%", $"Palmares {DateTime.Now}");

                    htmlReport = htmlReport.Replace("%%PALMARES_ITEMS%%", palmaresItems);
                    File.WriteAllText(reportFileName, htmlReport);

                    Process.Start(reportFileName);
                }
                #endregion

                #region WATCHLIST REPORT
                reportFileName = Path.Combine(Folders.Report, "Watchlist.html");
                reportDate = File.Exists(reportFileName) ? File.GetLastWriteTime(reportFileName) : DateTime.MinValue;
                if (force || reportDate.Date != DateTime.Today ||
                    reportDate <= File.GetLastWriteTime(Folders.WatchlistReportTemplate) ||
                    reportDate <= File.GetLastWriteTime(Folders.WatchlistItemTemplate))
                {

                    string watchlistItems = string.Empty;
                    foreach (var watchlist in StockWatchList.WatchLists.Where(w => w.Report && w.StockList.Count > 0))
                    {
                        StockSplashScreen.ProgressText = $"Generating report - {watchlist.Name}";

                        watchlistItems += GenerateReportFromWatchList(watchlist);
                    }

                    var htmlReport = File.ReadAllText(Folders.WatchlistReportTemplate);
                    htmlReport = htmlReport.Replace("%%Title%%", $"Watchlists {DateTime.Now}");

                    htmlReport = htmlReport.Replace("%%WATCHLIST_ITEMS%%", watchlistItems);
                    File.WriteAllText(reportFileName, htmlReport);

                    Process.Start(reportFileName);
                }
                #endregion


                foreach (var duration in new[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly })
                {
                    // GenerateAlertReport(duration);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            finally
            {
                this.WindowState = currentState;
                if (currentState == FormWindowState.Normal)
                    this.Size = currentSize;

                this.Cursor = Cursors.Arrow;
                Cursor.Show();

                GraphControl.GeneratingReport = false;
                Settings.Default.ShowOrders = showOrders;
                Settings.Default.ShowPositions = showPositions;
            }
        }

        private void GenerateReportFromTemplate(string templateFile, bool force = false)
        {
            StockSplashScreen.ProgressText = $"Generating report - {Path.GetFileNameWithoutExtension(templateFile)}";

            var reportFileName = Path.Combine(Folders.Report, Path.GetFileName(templateFile));
            var reportDate = File.Exists(reportFileName) ? File.GetLastWriteTime(reportFileName) : DateTime.MinValue;
            if (!force && reportDate.Date == DateTime.Today && reportDate > File.GetLastWriteTime(templateFile))
                return;

            var htmlReportTemplate = File.ReadAllText(templateFile);
            // Find Pattern
            string pattern = @"§§.*?§§";

            // Instantiate the regular expression object.
            Regex regex = new Regex(pattern);

            // Match the regular expression pattern against the input string.
            MatchCollection matches = regex.Matches(htmlReportTemplate);

            foreach (Match match in matches)
            {
                var fields = match.Value.Replace("§§", "").Split('|');
                var stockName = fields[0];
                var duration = (BarDuration)Enum.Parse(typeof(BarDuration), fields[1]);
                var theme = fields[2];
                var nbBars = int.Parse(fields[3]);

                var instrument = StockDictionary.GetInstrumentByName(stockName);
                if (instrument != null)
                {
                    var bitmapString = this.GetStockSnapshotAsHtml(instrument, theme, true, duration, nbBars);
                    string data = $"\r\n    <h3>{stockName} - {duration}</h3>\r\n    <a>\r\n        <img src=\"{bitmapString}\">\r\n    </a>";

                    htmlReportTemplate = htmlReportTemplate.Replace(match.Value, data);
                }
                else
                {
                    htmlReportTemplate = htmlReportTemplate.Replace(match.Value, $"<h3>{stockName} - {duration}</h3><p>Instrument not found</p>");
                }
            }
            htmlReportTemplate = htmlReportTemplate.Replace("%%Title%%", $"Report - {Path.GetFileNameWithoutExtension(templateFile)}");
            htmlReportTemplate = htmlReportTemplate.Replace("%%Date%%", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            File.WriteAllText(reportFileName, htmlReportTemplate);

            Process.Start(reportFileName);
        }

        const string TABLE_ROW_TEMPLATE_WATCHLIST = @"<tr>
            <td>%%Weekly%%</td>
            <td>%%Daily%%</td>
        </tr>";
        private string GenerateReportFromWatchList(StockWatchList watchlist)
        {
            StockSplashScreen.ProgressText = $"Generating report - {watchlist.Name}";

            var htmlReport = File.ReadAllText(Folders.WatchlistItemTemplate);

            var tableRows = string.Empty;

            foreach (string stockName in watchlist.StockList)
            {
                var instrument = StockDictionary.GetInstrumentByName(stockName);
                if (instrument != null)
                {
                    var duration = BarDuration.Daily;
                    var theme = REPORT_THEME;
                    var nbBars = 75;
                    var bitmapString = this.GetStockSnapshotAsHtml(instrument, theme, true, duration, nbBars);
                    string data = $"\r\n    <h3>{stockName} - {duration}</h3>\r\n    <a>\r\n        <img src=\"{bitmapString}\">\r\n    </a>";

                    var row = TABLE_ROW_TEMPLATE_WATCHLIST.Replace("%%Daily%%", data);

                    duration = BarDuration.Weekly;
                    theme = REPORT_THEME;
                    nbBars = 75;
                    bitmapString = this.GetStockSnapshotAsHtml(instrument, theme, true, duration, nbBars);
                    data = $"\r\n    <h3>{stockName} - {duration}</h3>\r\n    <a>\r\n        <img src=\"{bitmapString}\">\r\n    </a>";

                    row = row.Replace("%%Weekly%%", data);

                    tableRows += row;
                }
            }
            htmlReport = htmlReport.Replace("%%Title%%", $"{watchlist.Name}");
            htmlReport = htmlReport.Replace("%%TABLE_ROWS%%", tableRows);

            return htmlReport;
        }

        private async Task<string> GenerateReportFromPalmares(string palmares, int nbLines = 50)
        {
            StockSplashScreen.ProgressText = $"Generating palmares report - {palmares}";

            var htmlReport = File.ReadAllText(Folders.PalmaresItemTemplate);

            var tableRows = string.Empty;

            PalmaresViewModel palmaresViewModel = new PalmaresViewModel
            {
                Setting = palmares
            };
            var palmaresSettings = palmaresViewModel.LoadSettings();
            await palmaresViewModel.CalculateAsync();

            htmlReport = htmlReport.Replace("%%INDICATOR1%%", $"{palmaresSettings.Indicator1}");
            htmlReport = htmlReport.Replace("%%INDICATOR2%%", $"{palmaresSettings.Indicator2}");
            htmlReport = htmlReport.Replace("%%INDICATOR3%%", $"{palmaresSettings.Indicator3}");
            htmlReport = htmlReport.Replace("%%STOK%%", $"{palmaresSettings.Stok}");

            foreach (var line in palmaresViewModel.Lines.OrderByDescending(l => l.Indicator1).Take(nbLines))
            {
                var duration = palmaresViewModel.BarDuration;
                var theme = palmaresViewModel.Theme;
                var nbBars = 75;
                var bitmapString = this.GetStockSnapshotAsHtml(line.Instrument, theme, true, duration, nbBars);

                string row = $"<tr><td style=\"font-size:11px;\"><a class=\"tooltip\">{line.Name}<span><img src=\"{bitmapString}\"></span></a></td> <td style=\"font-size:11px;\">{line.Group}</td> <td>{line.Value}</td> <td>{ToNaNString(line.Indicator1)}</td> <td>{ToNaNString(line.Indicator2)}</td> <td>{ToNaNString(line.Indicator3)}</td> <td>{line.Stok}</td> <td>{line.Highest}</td> <td>{line.LastDate.ToShortDateString()}</td> </tr>";

                tableRows += row;
            }

            htmlReport = htmlReport.Replace("%%Title%%", $"{palmares} - {palmaresViewModel.Group} - {palmaresViewModel.BarDuration}");
            htmlReport = htmlReport.Replace("%%TABLE_ROWS%%", tableRows);

            return htmlReport;
        }

        string ToNaNString(float value)
        {
            return float.IsNaN(value) ? "" : value.ToString();
        }

        string typedSearch = null;
        private void SearchCombo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (searchCombo.Text == typedSearch)
                {
                    return;
                }
                if (string.IsNullOrEmpty(searchCombo.Text) && !string.IsNullOrEmpty(typedSearch))
                {
                    Debug.WriteLine("Cond0");
                    this.searchCombo.Items.Clear();
                    this.searchCombo.Items.AddRange(StockDictionary.Instruments.Where(p => !p.Value.StockAnalysis.Excluded).Select(p => p.Value.DisplayName).ToArray());
                    this.searchCombo.SelectionStart = this.searchCombo.Text.Length;
                    typedSearch = null;
                    return;
                }
                typedSearch = searchCombo.Text.ToUpper();
                if (this.searchCombo.Items.Count == 1 && this.searchCombo.Items[0].ToString() == searchCombo.Text)
                {
                    Debug.WriteLine("Cond2");
                    return; // Prevent infinite loop
                }

                var name = searchCombo.Text.ToUpper();
                string[] match;
                if (name.Length == 12)
                    match = StockDictionary.Instruments.Where(p => !p.Value.StockAnalysis.Excluded && (p.Value.DisplayName.ToUpper().Contains(name) || p.Value.Isin == name)).Select(p => p.Value.DisplayName).ToArray();
                else if (name.Length <= 3)
                    match = StockDictionary.Instruments.Where(p => !p.Value.StockAnalysis.Excluded && (p.Value.DisplayName.ToUpper().Contains(name) || (p.Value.Symbol != null && p.Value.Symbol.Contains(name)))).Select(p => p.Value.DisplayName).ToArray();
                else
                    match = StockDictionary.Instruments.Where(p => !p.Value.StockAnalysis.Excluded && p.Value.DisplayName.ToUpper().Contains(name)).Select(p => p.Value.DisplayName).ToArray();

                if (match.Length == 1)
                {
                    Debug.WriteLine("Cond3");
                    searchCombo.Text = name;
                    this.searchCombo.SelectionStart = this.searchCombo.Text.Length;
                    this.SetCurrentInstrument(match.First());
                }
                else
                {
                    Debug.WriteLine("Cond4");
                    this.searchCombo.Items.Clear();
                    this.searchCombo.Items.AddRange(match);
                    this.searchCombo.DroppedDown = true;
                    this.searchCombo.SelectedIndex = -1;
                    searchCombo.Text = name;
                    this.searchCombo.SelectionStart = this.searchCombo.Text.Length;
                    Cursor = Cursors.Default;
                    // Automatically pop up drop-down
                }

                Debug.WriteLine("Cond5");
            }
            catch (Exception exception)
            {
                StockLog.Write(exception);
            }
        }

        private readonly bool showTimerDebug = true;

        private void goToStock(object sender, EventArgs e)
        {
            if (searchCombo.SelectedItem == null)
                return;

            SetCurrentInstrument(searchCombo.SelectedItem.ToString());
        }

        private void SetCurrentInstrument(string instrumentName)
        {
            var instrument = StockDictionary.GetInstrumentByName(instrumentName);
            if (instrument != null)
            {
                this.ViewModel.Instrument = instrument;
            }
        }

        private void InitialiseWatchListComboBox()
        {
            if (StockWatchList.WatchLists != null)
            {
                // 
                ToolStripItem[] watchListMenuItems = new ToolStripItem[StockWatchList.WatchLists.Count()];
                ToolStripItem[] addToWatchListMenuItems = new ToolStripItem[StockWatchList.WatchLists.Count()];
                ToolStripMenuItem addToWatchListSubMenuItem;

                int i = 0;
                foreach (StockWatchList watchList in StockWatchList.WatchLists)
                {
                    // Create add to wath list menu items
                    addToWatchListSubMenuItem = new ToolStripMenuItem(watchList.Name);
                    addToWatchListSubMenuItem.Click += new EventHandler(addToWatchListSubMenuItem_Click);
                    addToWatchListMenuItems[i++] = addToWatchListSubMenuItem;
                }
                this.AddToWatchListToolStripDropDownButton.DropDownItems.Clear();
                this.AddToWatchListToolStripDropDownButton.DropDownItems.AddRange(addToWatchListMenuItems);
            }
        }

        private void graphScrollerControl_ZoomChanged(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        #endregion
        #region TIMER MANAGEMENT

        static bool refreshing = false;
        private void RefreshTimer_Tick()
        {
            using var ml = new MethodLogger(this, showTimerDebug);

            refreshing = true;

            LoginService.RefreshSessions();

            refreshing = false;

            return;
        }

        public void GenerateIntradayReport(List<BarDuration> barDurations)
        {
            Action action = () => GenerateIntradayReportDispatch(barDurations);
            this.Invoke(action);
        }

        public void GenerateIntradayReportDispatch(List<BarDuration> barDurations)
        {
            using var ml = new MethodLogger(this);

            try
            {
                var alertDefs = StockAlertDef.AlertDefs.Where(a => a.InReport && a.InAlert && barDurations.Contains(a.BarDuration)).ToList();
                if (alertDefs.Count == 0)
                    return;
                var sw = Stopwatch.StartNew();
                var groups = alertDefs.Select(a => a.Group).Distinct();

                throw new NotImplementedException("GenerateIntradayReport not implemented yet");

                //var turboList = StockDictionary.Instruments.Values.Where(s => !s.StockAnalysis.Excluded && s.Group == Groups.TURBO);
                //var downloadTasks = turboList.Select(s => Task.Run(() => StockDataProviderBase.DownloadSerieData(s)));

                //Task.WaitAll(downloadTasks.ToArray());

                sw.Stop();

                foreach (var duration in barDurations)
                {
                    GenerateAlertReport(duration);
                }
            }
            finally
            {
            }
        }
        #endregion


        private void StockDictionary_ReportProgress(string progress)
        {
            StockSplashScreen.ProgressSubText = progress;
        }

        #region ZOOMING

        private void ChangeZoom(int startIndex, int endIndex)
        {
            using (new MethodLogger(this))
            {
                try
                {
                    this.startIndex = Math.Max(0, startIndex);
                    this.endIndex = endIndex;
                    this.graphScrollerControl.InitZoom(this.startIndex, this.endIndex);
                }
                catch (InvalidSerieException e)
                {
                    StockLog.Write(e);
                    DeactivateGraphControls(e.Message);
                }
                catch (Exception exception)
                {
                    StockLog.Write(exception);
                    DeactivateGraphControls(exception.Message);
                    StockAnalyzerException.MessageBox(exception);
                }
            }
        }
        public void ResetZoom()
        {
            using (new MethodLogger(this))
            {
                var dataSerie = ViewModel.Instrument.GetDataSerie(ViewModel.BarDuration);
                if (dataSerie == null || dataSerie.Count == 0)
                {
                    startIndex = -1;
                    endIndex = -1;
                }
                else
                {
                    int nbBars = NbBars;
                    if (dataSerie.Count > 1 && dataSerie.Count - 1 - nbBars < 0) // Previous serie was longer
                    {
                        nbBars = dataSerie.Count - 1;
                    }
                    ChangeZoom(Math.Max(0, dataSerie.Count - 1 - nbBars), dataSerie.Count - 1);
                }
            }
        }

        const int MIN_BAR_DISPLAY = 25;
        private void ZoomIn()
        {
            NbBars = Math.Max(MIN_BAR_DISPLAY, (int)(NbBars / 1.75f));
            int newIndex = Math.Max(0, endIndex - NbBars);
            if (newIndex != this.startIndex)
            {
                this.ChangeZoom(newIndex, endIndex);
            }
        }

        private void ZoomOut()
        {
            NbBars = Math.Min(this.endIndex, (int)(NbBars * 1.75f));
            int newIndex = endIndex - NbBars;
            if (newIndex != this.startIndex)
            {
                this.ChangeZoom(newIndex, endIndex);
            }
        }

        private void ZoomOutBtn_Click(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void ZoomInBtn_Click(object sender, EventArgs e)
        {
            ZoomIn();
        }
        private void showVariationBtn_Click(object sender, EventArgs e)
        {
            if (this.showVariationBtn.CheckState == CheckState.Checked)
            {
                this.showVariationBtn.CheckState = CheckState.Unchecked;
                Settings.Default.ShowVariation = false;
            }
            else
            {
                this.showVariationBtn.CheckState = CheckState.Checked;
                Settings.Default.ShowVariation = true;
            }
            ChangeZoom(this.startIndex, this.endIndex);
        }

        private void logScaleBtn_Click(object sender, EventArgs e)
        {
            if (this.logScaleBtn.CheckState == CheckState.Checked)
            {
                this.logScaleBtn.CheckState = CheckState.Unchecked;
                this.graphCloseControl.IsLogScale = false;
                this.graphScrollerControl.IsLogScale = false;
            }
            else
            {
                this.logScaleBtn.CheckState = CheckState.Checked;
                this.graphCloseControl.IsLogScale = true;
                this.graphScrollerControl.IsLogScale = true;
            }
            ChangeZoom(this.startIndex, this.endIndex);
        }

        #endregion

        private void SetThemeCombo(string theme)
        {
            this.themeComboBox.SelectedIndexChanged -= themeComboBox_SelectedIndexChanged;
            this.themeComboBox.SelectedItem = theme;
            this.themeComboBox.SelectedIndexChanged += themeComboBox_SelectedIndexChanged;
        }

        private void SetBarDurationCombo(BarDuration barDuration)
        {
            this.barDurationComboBox.SelectedIndexChanged -= this.BarDurationChanged;
            this.barDurationComboBox.SelectedItem = barDuration;
            this.barDurationComboBox.SelectedIndexChanged += this.BarDurationChanged;
        }

        public void OnSelectedInstrumentChanged(StockInstrument instrument, bool activate)
        {
            using (new MethodLogger(this))
            {
                this.ViewModel.SetInstrument(instrument, false);
                this.OnInstrumentChanged();

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        public void OnSelectedInstrumentAndDurationAndThemeChanged(StockInstrument instrument, BarDuration barDuration, string theme, bool activate)
        {
            using (new MethodLogger(this))
            {
                this.ViewModel.SetBarDuration(barDuration, false);
                this.SetBarDurationCombo(barDuration);

                if (string.IsNullOrEmpty(theme) || !themeDictionary.ContainsKey(theme))
                {
                    theme = EMPTY_THEME;
                }
                this.SetThemeCombo(theme);
                this.ViewModel.SetTheme(theme, false);

                this.ViewModel.SetInstrument(instrument, false);
                this.OnInstrumentChanged();

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        public void OnSelectedInstrumentAndDurationChanged(StockInstrument instrument, BarDuration barDuration, bool activate)
        {
            using (new MethodLogger(this))
            {
                this.ViewModel.SetBarDuration(barDuration, false);
                this.SetBarDurationCombo(barDuration);

                this.ViewModel.SetInstrument(instrument, false);
                this.OnInstrumentChanged();

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        public void OnSelectedInstrumentAndDurationAndIndexChanged(StockInstrument instrument, int startIndex, int endIndex, BarDuration barDuration, bool activate)
        {
            using (new MethodLogger(this))
            {
                this.ViewModel.SetBarDuration(barDuration, false);
                this.SetBarDurationCombo(barDuration);

                this.ViewModel.SetInstrument(instrument, false);
                this.OnInstrumentChanged();

                this.ChangeZoom(startIndex, endIndex);

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        private void DeactivateGraphControls(string msg)
        {
            this.graphCloseControl.Deactivate(msg, false);
            this.graphScrollerControl.Deactivate(msg, false);
            this.graphIndicator1Control.Deactivate(msg, false);
            this.graphIndicator2Control.Deactivate(msg, false);
            this.graphIndicator3Control.Deactivate(msg, false);
            this.graphVolumeControl.Deactivate(msg, false);
            this.Invoke(new Action(() => this.Cursor = Cursors.Arrow));
            this.statusLabel.Text = ("Loading data...");
        }

        private void StockAnalyzerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.ThemeToolbarLocation = this.themeToolStrip.Location;
            Settings.Default.StockToolbarLocation = this.browseToolStrip.Location;
            Settings.Default.drawingToolbarLocation = this.drawToolStrip.Location;
            Settings.Default.Save();

            this.IsClosing = true;
            OneDriveSync(false);
        }

        public void OnSerieEventProcessed()
        {
            this.progressBar.Value++;
        }

        private void LoadWatchList()
        {
            string watchListsFileName = Path.Combine(Folders.PersonalFolder, "WatchLists.json");
            StockWatchList.Load(watchListsFileName);

            if (StockWatchList.WatchLists.Count == 0)
            {
                // Create new empty watchlist
                StockWatchList.WatchLists.Add(new StockWatchList("New"));
                StockWatchList.Save(watchListsFileName);
            }
            else
            {
                // Cleanup missing stocks
                foreach (var watchList in StockWatchList.WatchLists)
                {
                    watchList.StockList.RemoveAll(s => StockDictionary.GetInstrumentByName(s) == null);
                }
            }
        }

        private void LoadAnalysis(string analysisFileName)
        {
            // Clear existing analysis
            foreach (var instrument in StockDictionary.Instruments.Values)
            {
                instrument.StockAnalysis.Clear();
            }

            // Read Stock Values from XML
            try
            {
                // Parse existing drawing items
                if (File.Exists(analysisFileName))
                {
                    using FileStream fs = new FileStream(analysisFileName, FileMode.Open);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    StockDictionary.ReadAnalysisFromXml(xmlReader);
                }
                bool dirty = false;
                foreach (var instrument in StockDictionary.Instruments.Values.Where(s => s.StockAnalysis.Theme != string.Empty))
                {
                    if (!this.themeComboBox.Items.Contains(instrument.StockAnalysis.Theme))
                    {
                        instrument.StockAnalysis.Theme = string.Empty;
                        dirty = true;
                    }
                }
                if (dirty)
                {
                    this.SaveAnalysis(analysisFileName);
                }
            }
            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
        }

        private void Notifiy_SplashProgressChanged(string text)
        {
            StockSplashScreen.ProgressText = text;
        }

        public void OnNeedReinitialise(bool resetDrawingButtons)
        {
            using (new MethodLogger(this))
            {

                // Refresh all components
                RefreshGraph();
                if (resetDrawingButtons)
                {
                    ResetDrawingButtons();
                }
            }
        }

        #region STOCK and PORTFOLIO selection tool bar

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    DownloadStockGroup();
                }
                else
                {
                    DownloadStock(false);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        private void ForceDownloadStock(bool showSplash)
        {
            using (new MethodLogger(this))
            {
                this.Cursor = Cursors.WaitCursor;
                if (showSplash)
                {
                    StockSplashScreen.FadeInOutSpeed = 0.25;
                    StockSplashScreen.ProgressText = "Downloading " + this.ViewModel.Instrument.Group + " - " + this.ViewModel.Instrument.DisplayName;
                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = 100;
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();
                }

                if (!StockDataProviderBase.ForceDownloadSerieData(this.ViewModel.Instrument.StockSerie))
                {
                    this.DeactivateGraphControls("Unable to download selected stock data...");
                }
                else
                {
                    this.ViewModel.Instrument.ClearCache();
                }

                if (showSplash)
                {
                    StockSplashScreen.CloseForm(true);
                }
                this.Cursor = Cursors.Arrow;
            }
        }
        private void ForceDownloadStockGroup()
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to force downloading the full group {this.ViewModel.Instrument.Group} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                StockSplashScreen.FadeInOutSpeed = 0.25;
                StockSplashScreen.ProgressText = "Downloading " + this.ViewModel.Instrument.Group + " - " + this.ViewModel.Instrument.DisplayName;

                var stockSeries =
                   StockDictionary.Instance.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.ViewModel.Instrument.Group));

                StockSplashScreen.ProgressVal = 0;
                StockSplashScreen.ProgressMax = stockSeries.Count();
                StockSplashScreen.ProgressMin = 0;
                StockSplashScreen.ShowSplashScreen();

                foreach (var stockSerie in stockSeries)
                {
                    StockSplashScreen.ProgressText = "Downloading " + this.ViewModel.Instrument.Group + " - " + stockSerie.StockName;
                    StockDataProviderBase.ForceDownloadSerieData(stockSerie);

                    StockSplashScreen.ProgressVal++;
                }

                this.SaveAnalysis(this.ViewModel.AnalysisFile);
            }
            catch (Exception ex)
            {
                this.DeactivateGraphControls("Unable to download selected stock data...");
                StockLog.Write(ex);
            }

            StockSplashScreen.CloseForm(true);
        }
        private void DownloadStock(bool showSplash)
        {
            using (new MethodLogger(this))
            {
                if (this.ViewModel.Instrument != null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (showSplash)
                    {
                        StockSplashScreen.FadeInOutSpeed = 0.25;
                        StockSplashScreen.ProgressText = "Downloading " + this.ViewModel.Instrument.StockSerie.StockGroup + " - " + this.ViewModel.Instrument.DisplayName;
                        StockSplashScreen.ProgressVal = 0;
                        StockSplashScreen.ProgressMax = 100;
                        StockSplashScreen.ProgressMin = 0;
                        StockSplashScreen.ShowSplashScreen();
                    }

                    var dataProvider = DataProviderBase.GetDataProvider(this.ViewModel.Instrument.Provider);
                    if (dataProvider != null && dataProvider.NeedDownload(this.ViewModel.Instrument))
                        dataProvider.DownloadData(this.ViewModel.Instrument);

                    if (showSplash)
                    {
                        StockSplashScreen.CloseForm(true);
                    }
                    this.Cursor = Cursors.Arrow;
                }
            }
        }
        private void DownloadStockGroup()
        {
            if (this.ViewModel.Instrument != null)
            {
                try
                {
                    StockSplashScreen.FadeInOutSpeed = 0.25;
                    StockSplashScreen.ProgressText = "Downloading " + this.ViewModel.Instrument.Group + " - " + this.ViewModel.Instrument.DisplayName;

                    var instruments = StockDictionary.Instruments.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.ViewModel.Instrument.Group));

                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = instruments.Count();
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();

                    foreach (var instrument in instruments)
                    {
                        StockSplashScreen.ProgressText = "Downloading " + instrument.Group + " - " + instrument.DisplayName;

                        throw new NotImplementedException("DownloadStockGroup not implemented yet");

                        // StockDataProviderBase.DownloadSerieData(instrument);
                        instrument.ClearCache();

                        StockSplashScreen.ProgressVal++;
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }
            }
            else
            {
                this.DeactivateGraphControls("Unable to download selected stock data...");
            }
            StockSplashScreen.CloseForm(true);
        }

        #endregion

        #region PREFERENCES MENU ITEM HANDLER

        private void RefreshGraph()
        {
            // Refresh all components
            this.graphCloseControl.ForceRefresh();
            this.graphScrollerControl.ForceRefresh();
            this.graphIndicator1Control.ForceRefresh();
            this.graphIndicator2Control.ForceRefresh();
            this.graphIndicator3Control.ForceRefresh();
            this.graphVolumeControl.ForceRefresh();
        }

        private void addToWatchListSubMenuItem_Click(object sender, EventArgs e)
        {
            StockWatchList watchList = StockWatchList.WatchLists.Find(wl => wl.Name == sender.ToString());
            if (!watchList.StockList.Contains(this.ViewModel.Instrument.DisplayName))
            {
                watchList.StockList.Add(this.ViewModel.Instrument.DisplayName);
                this.SaveWatchList();
            }
        }

        #endregion

        #region DRAWING TOOLBAR HANDLERS
        private void drawLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (drawLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddLine;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void drawBoxStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (drawBoxStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddBox;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void drawWinRatioStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (drawWinRatioStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddWinRatio;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void cupHandleBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (drawCupHandleStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddCupHandle;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void copyLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (copyLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.CopyLine;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void deleteLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (deleteLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.DeleteItem;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void drawingStyleStripBtn_Click(object sender, EventArgs e)
        {
            Pen pen = GraphCurveType.PenFromString(Settings.Default.DrawingPen);
            DrawingStyleForm drawingStyleForm = new DrawingStyleForm(pen);
            if (drawingStyleForm.ShowDialog() == DialogResult.OK)
            {
                GraphControl.DrawingPen = drawingStyleForm.Pen;
                CupHandle2D.PivotBrush = new SolidBrush(drawingStyleForm.Pen.Color);

                Settings.Default.DrawingPen = GraphCurveType.PenToString(drawingStyleForm.Pen);
                Settings.Default.Save();
            }
        }

        private void addHalfLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (addHalfLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddHalfLine;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void addSegmentStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (addSegmentStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddSegment;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void cutLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (cutLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.CutLine;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
        }

        private void ResetDrawingButtons()
        {
            // Interupt current drawings
            foreach (GraphControl graphControl in this.graphList)
            {
                this.graphCloseControl.ResetDrawingMode();
            }

            // Reset drawing buttons 
            drawWinRatioStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            drawCupHandleStripBtn.Checked = false;
            drawBoxStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void saveAnalysisToolStripButton_Click(object sender, EventArgs e)
        {
            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.SaveAnalysis(this.ViewModel.AnalysisFile);

            this.Cursor = currentCursor;

        }

        private void SaveWatchList()
        {
            string watchListsFileName = Path.Combine(Folders.PersonalFolder, "WatchLists.json");

            foreach (StockWatchList watchList in StockWatchList.WatchLists)
            {
                watchList.StockList.RemoveAll(s => !StockDictionary.Instruments.ContainsKey(s));
                watchList.StockList.Sort();
            }

            StockWatchList.Save(watchListsFileName);
        }

        public void SaveAnalysis(string analysisFileName)
        {
            string tmpFileName = analysisFileName + ".tmp";
            bool success = true;
            // Save stock analysis to XML
            XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));
            XmlTextWriter xmlWriter;
            try
            {
                // Save analysis file
                using FileStream fs = new FileStream(tmpFileName, FileMode.Create);
                xmlWriter = new XmlTextWriter(fs, null);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                xmlWriter.WriteStartDocument();
                StockDictionary.WriteAnalysisToXml(xmlWriter);
                xmlWriter.WriteEndDocument();
            }
            catch (Exception exception)
            {
                success = false;
                StockAnalyzerException.MessageBox(exception);
            }
            if (success)
            {
                if (File.Exists(analysisFileName))
                {
                    File.Delete(analysisFileName);
                }
                File.Move(tmpFileName, analysisFileName);
            }
        }
        private Bitmap Snapshot()
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            int width = 0;
            int height = -2;
            foreach (GraphControl graphCtrl in this.graphList.Where(g => !(g is GraphScrollerControl)))
            {
                Bitmap bmp = graphCtrl.GetSnapshot();
                if (bmp != null)
                {
                    bitmaps.Add(bmp);
                    width = bmp.Width;
                    height += bmp.Height + 2;
                }
            }
            Bitmap snapshot = null;
            if (bitmaps.Count > 0)
            {
                snapshot = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(snapshot);

                var bb = ColorManager.GetBrush("Graph.Background");
                g.FillRectangle(bb, g.VisibleClipBounds);

                height = 0;
                foreach (Bitmap bmp in bitmaps)
                {
                    g.DrawImage(bmp, 0, height);
                    height += bmp.Height + 2;
                    bmp.Dispose();
                }
                g.Flush();
            }
            return snapshot;
        }

        public string GetStockSnapshotAsHtml(StockInstrument instrument, string theme, bool mainGraphOnly, BarDuration duration, int nbBars = 0)
        {

            if (!string.IsNullOrEmpty(theme) && this.themeComboBox.Items.Contains(theme))
            {
                this.ViewModel.SetTheme(theme, false);
            }
            else
            {
                this.ViewModel.SetTheme(EMPTY_THEME, false);
            }
            this.ViewModel.SetBarDuration(duration, false);
            this.ViewModel.SetInstrument(instrument, false);

            this.OnInstrumentChanged();

            if (nbBars > 0)
            {
                var dataSerie = instrument.GetDataSerie(duration);
                if (dataSerie != null)
                {
                    this.ChangeZoom(dataSerie.LastIndex - nbBars, dataSerie.LastIndex);
                }
            }

            return SnapshotAsHtml(mainGraphOnly);
        }

        private string SnapshotAsHtml(bool mainGraphOnly)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            int width = 0;
            int height = -2;
            foreach (GraphControl graphCtrl in this.graphList.Where(g => !(g is GraphScrollerControl)))
            {
                if (mainGraphOnly && !(graphCtrl is GraphCloseControl))
                    continue;

                Bitmap bmp = graphCtrl.GetSnapshot();
                if (bmp != null)
                {
                    bitmaps.Add(bmp);
                    width = bmp.Width;
                    height += bmp.Height + 2;
                }
            }
            if (bitmaps.Count > 0)
            {
                Bitmap snapshot = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(snapshot);

                var bb = ColorManager.GetBrush("Graph.Background");
                g.FillRectangle(bb, g.VisibleClipBounds);

                height = 0;
                foreach (Bitmap bmp in bitmaps)
                {
                    g.DrawImage(bmp, 0, height);
                    height += bmp.Height + 2;
                    bmp.Dispose();
                }
                g.Flush();

                using var stream = new MemoryStream();
                snapshot.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
            }
            return string.Empty;
        }

        private void snapshotToolStripButton_Click(object sender, EventArgs e)
        {
            var snapshot = this.Snapshot();
            if (snapshot != null)
                Clipboard.SetImage(snapshot);
        }

        private void magnetStripBtn_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.Magnetism = this.magnetStripBtn.Checked;
        }

        #endregion DRAWING TOOLBAR HANDLERS

        #region ANALYSYS TOOLBAR HANDLERS
        private void excludeButton_Click(object sender, EventArgs e)
        {
            var dp = DataProviderBase.GetDataProvider(this.ViewModel.Instrument.Provider);
            var handled = dp.RemoveEntry(this.ViewModel.Instrument);
            // Flag as excluded
            this.ViewModel.Instrument.StockAnalysis.Excluded = true;
            if (!handled)
            {
                SaveAnalysis(this.ViewModel.AnalysisFile);
            }
        }

        private void saxoTurboButton_Click(object sender, EventArgs e)
        {
            if (this.ViewModel.Instrument.SaxoId <= 0)
                return;

            OpenSaxoIntradyConfigDlg(this.ViewModel.Instrument.SaxoId);
        }
        #endregion

        #region History Browsing 
        private void rewindBtn_Click(object sender, EventArgs e)
        {
            this.ViewModel.BrowseBack();
        }

        private void fastForwardBtn_Click(object sender, EventArgs e)
        {
            this.ViewModel.BrowseNext();
        }

        private void copyIsinBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.ViewModel.Instrument?.Isin))
            {
                Clipboard.SetText(this.ViewModel.Instrument?.Isin);
                return;
            }
            if (!string.IsNullOrEmpty(this.ViewModel.Instrument.DisplayName))
            {
                Clipboard.SetText(this.ViewModel.Instrument.DisplayName);
                return;
            }
        }

        #endregion

        #region REWIND/FAST FORWARD METHODS

        private void Rewind(int step)
        {
            if (startIndex > step)
            {
                ChangeZoom(startIndex - step, endIndex - step);
                OnNeedReinitialise(false);
            }
            else if (startIndex > 0)
            {
                ChangeZoom(0, endIndex - startIndex);
                OnNeedReinitialise(false);
            }

        }

        private void Forward(int step)
        {
            int max = this.ViewModel.Instrument.GetDataSerie(this.ViewModel.BarDuration).LastIndex;
            if (endIndex != max)
            {
                if (endIndex + step > max)
                {
                    ChangeZoom(startIndex + max - endIndex, max);
                    OnNeedReinitialise(false);
                }
                else
                {
                    ChangeZoom(startIndex + step, endIndex + step);
                    OnNeedReinitialise(false);
                }
            }
        }

        #endregion

        #region VIEW MENU HANDLERS

        #region MENU CREATION
        private void CreateSecondarySerieMenuItem()
        {
            // Clean existing menus
            this.secondarySerieMenuItem.DropDownItems.Clear();
            var validGroups = StockDictionary.Instance.GetValidGroups().Select(g => g.ToString());
            ToolStripMenuItem[] groupMenuItems = new ToolStripMenuItem[validGroups.Count()];

            int i = 0;
            foreach (string group in validGroups)
            {
                groupMenuItems[i] = new ToolStripMenuItem(group);

                // 
                var groupInstruments = StockDictionary.Instruments.Values.Where(s => s.Group.ToString() == group && !s.StockAnalysis.Excluded);
                if (groupInstruments.Count() != 0)
                {
                    ToolStripMenuItem[] secondarySerieMenuItems = new ToolStripMenuItem[groupInstruments.Count()];
                    ToolStripMenuItem secondarySerieSubMenuItem;

                    int n = 0;
                    foreach (var instruments in groupInstruments)
                    {
                        secondarySerieSubMenuItem = new ToolStripMenuItem(instruments.DisplayName);
                        secondarySerieSubMenuItem.Click += new EventHandler(secondarySerieMenuItem_Click);
                        secondarySerieMenuItems[n++] = secondarySerieSubMenuItem;
                    }
                    groupMenuItems[i].DropDownItems.AddRange(secondarySerieMenuItems);
                }

                i++;
            }

            this.secondarySerieMenuItem.DropDownItems.AddRange(groupMenuItems);
        }

        #endregion

        #region Palmares dialog
        private PalmaresDlg palmaresDlg = null;
        private void palmaresMenuItem_Click(object sender, EventArgs e)
        {
            if (palmaresDlg == null)
            {
                palmaresDlg = new PalmaresDlg() { StartPosition = FormStartPosition.CenterScreen };
                palmaresDlg.palmaresControl1.ViewModel.BarDuration = BarDuration.Daily;
                palmaresDlg.palmaresControl1.ViewModel.Group = this.ViewModel.Instrument.Group;

                palmaresDlg.FormClosing += new FormClosingEventHandler(palmaresDlg_FormClosing);
                palmaresDlg.palmaresControl1.SelectedInstrumentChanged += OnSelectedInstrumentAndDurationChanged;
                palmaresDlg.palmaresControl1.SelectedInstrumentAndThemeChanged += OnSelectedInstrumentAndDurationAndThemeChanged;
                palmaresDlg.Show();
            }
            else
            {
                palmaresDlg.Activate();
            }
        }

        private void palmaresDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            palmaresDlg.palmaresControl1.SelectedInstrumentChanged -= OnSelectedInstrumentAndDurationChanged;
            palmaresDlg.palmaresControl1.SelectedInstrumentAndThemeChanged -= OnSelectedInstrumentAndDurationAndThemeChanged;
            this.palmaresDlg = null;
        }
        #endregion

        #region Instruments dialog
        private InstrumentDlg instrumentsDlg = null;
        private void instrumentsMenuItem_Click(object sender, EventArgs e)
        {
            if (instrumentsDlg == null)
            {
                instrumentsDlg = new InstrumentDlg() { StartPosition = FormStartPosition.CenterScreen };

                instrumentsDlg.instrumentsControl1.ViewModel.Group = this.ViewModel.Instrument.Group;

                instrumentsDlg.FormClosing += new FormClosingEventHandler(instrumentsDlg_FormClosing);
                instrumentsDlg.instrumentsControl1.SelectedInstrumentChanged += OnSelectedInstrumentChanged;
                instrumentsDlg.Show();
            }
            else
            {
                instrumentsDlg.Activate();
            }
        }
        private void instrumentsDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            instrumentsDlg.instrumentsControl1.SelectedInstrumentChanged -= OnSelectedInstrumentChanged;
            this.instrumentsDlg = null;
        }
        #endregion

        private void showShowStatusBarMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowStatusBar = this.showShowStatusBarMenuItem.Checked;
            Settings.Default.Save();
            this.statusStrip1.Visible = Settings.Default.ShowStatusBar;
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void showDrawingsMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowDrawings = this.showDrawingsMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void showEventMarqueeMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowEventMarquee = this.showEventMarqueeMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void showGridMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowGrid = this.showGridMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void showIndicatorDivMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowIndicatorDiv = this.showIndicatorDivMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }
        private void showIndicatorTextMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowIndicatorText = this.showIndicatorTextMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void showOrdersMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowOrders = this.showOrdersMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }
        private void showPositionsMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowPositions = this.showPositionsMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }

        private void hideIndicatorsStockMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.HideIndicators = this.hideIndicatorsStockMenuItem.Checked;
            this.OnNeedReinitialise(false);
        }

        #endregion VIEW MENU HANDLERS

        #region PORTFOLIO MENU HANDERS

        PortfolioDlg portfolioDlg = null;
        private void currentPortfolioMenuItem_Click(object sender, EventArgs e)
        {
            if (Portfolio == null)
                return;

            if (portfolioDlg == null)
            {
                portfolioDlg = new PortfolioDlg() { StartPosition = FormStartPosition.CenterScreen };
                portfolioDlg.FormClosing += (a, b) => { this.portfolioDlg = null; };
                portfolioDlg.Show();
            }
            else
            {
                portfolioDlg.Activate();
            }
        }
        SaxoPortfolioDlg saxoPortfolioDlg = null;
        private void saxoPortfolioMenuItem_Click(object sender, EventArgs e)
        {
            if (Portfolio == null)
                return;

            if (saxoPortfolioDlg == null)
            {
                saxoPortfolioDlg = new SaxoPortfolioDlg() { StartPosition = FormStartPosition.CenterScreen };
                saxoPortfolioDlg.FormClosing += (a, b) => { this.saxoPortfolioDlg = null; };
                saxoPortfolioDlg.Show();
            }
            else
            {
                saxoPortfolioDlg.Activate();
            }
        }
        private void portfolioReportMenuItem_Click(object sender, EventArgs e)
        {
            if (Settings.Default.PortfolioOnline)
            {
                foreach (var p in this.Portfolios.Where(p => !string.IsNullOrEmpty(p.SaxoAccountId) && !p.IsSaxoSimu))
                {
                    if (!p.SaxoLogin())
                    {
                        var diagResult = MessageBox.Show($"Portfolio: {p.Name} login failed !!! {Environment.NewLine}Do you want to continue ?", "Login Error", MessageBoxButtons.YesNo);
                        if (diagResult == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
            }
            foreach (var p in this.Portfolios.Where(p => !string.IsNullOrEmpty(p.SaxoAccountId) && !p.IsSaxoSimu))
            {
                this.GeneratePortfolioReportFile(p);
            }
        }

        #endregion

        #region Auto Trading

        AutoTradeDlg autoTradeDlg = null;
        private void autoTradeMenuItem_Click(object sender, EventArgs e)
        {
            if (autoTradeDlg == null)
            {
                autoTradeDlg = new AutoTradeDlg() { StartPosition = FormStartPosition.CenterScreen };
                autoTradeDlg.autoTradeControl1.SelectedInstrumentChanged += OnSelectedInstrumentAndDurationChanged;
                autoTradeDlg.autoTradeControl1.SelectedInstrumentAndThemeChanged += OnSelectedInstrumentAndDurationAndThemeChanged;
                autoTradeDlg.FormClosing += (a, b) =>
                {
                    autoTradeDlg.autoTradeControl1.SelectedInstrumentChanged -= OnSelectedInstrumentAndDurationChanged;
                    autoTradeDlg.autoTradeControl1.SelectedInstrumentAndThemeChanged -= OnSelectedInstrumentAndDurationAndThemeChanged;
                    this.autoTradeDlg = null;
                };
                autoTradeDlg.Show();
            }
            else
            {
                autoTradeDlg.Activate();
            }
        }
        #endregion

        #region ANALYSIS MENU HANDLERS

        AgentSimulationDlg agentTunningDialog = null;
        private void agentTunningMenuItem_Click(object sender, EventArgs e)
        {
            if (agentTunningDialog == null)
            {
                agentTunningDialog = new AgentSimulationDlg() { StartPosition = FormStartPosition.CenterScreen };
                agentTunningDialog.agentSimulationControl.SelectedInstrumentChanged += OnSelectedInstrumentAndDurationAndIndexChanged;
                agentTunningDialog.FormClosed += (a, b) =>
                {
                    agentTunningDialog = null;
                };
                agentTunningDialog.Show();
            }
            else
            {
                agentTunningDialog.Activate();
            }
        }


        BackTestDlg backTestDialog = null;
        private void backTestMenuItem_Click(object sender, EventArgs e)
        {
            if (backTestDialog == null)
            {
                backTestDialog = new BackTestDlg() { StartPosition = FormStartPosition.CenterScreen };
                backTestDialog.backTestControl.SelectedStockChanged += OnSelectedInstrumentAndDurationAndIndexChanged;
                backTestDialog.FormClosed += (a, b) =>
                {
                    backTestDialog = null;
                };
                backTestDialog.Show();
            }
            else
            {
                backTestDialog.Activate();
            }
        }

        PortfolioSimulationDlg portfolioSimulationDialog = null;
        private void portfolioSimulationMenuItem_Click(object sender, EventArgs e)
        {
            if (portfolioSimulationDialog == null)
            {
                portfolioSimulationDialog = new PortfolioSimulationDlg() { StartPosition = FormStartPosition.CenterScreen };
                portfolioSimulationDialog.portfolioSimulationControl1.SelectedStockChanged += OnSelectedInstrumentAndDurationAndIndexChanged;

                portfolioSimulationDialog.FormClosed += (a, b) =>
                {
                    portfolioSimulationDialog = null;
                };
                portfolioSimulationDialog.Show();
            }
            else
            {
                portfolioSimulationDialog.Activate();
            }
        }
        #endregion

        #region REPORTING

        public string GeneratePortfolioReportHtml(StockPortfolio portfolio)
        {
            const string rowTemplate = @"
         <tr>
             <td>%COL1%</td>
             <td>%COL2%</td>
             <td>%COL3.1%</td>
             <td>%COL3.2%</td>
             <td>%COL4%</td>
             <td>%COL5%</td>
             <td>%COL6%</td>
             <td>%COL7%</td>
             <td>%COL8%</td>
         </tr>";
            string html = $@"
            <table  class=""reportTable"" id=""PAGE_TOP"">
                <thead>
                <tr>
                    <th style=""font-size:20px;"" rowspan=""1"">{portfolio.Name}<br>{DateTime.Now.ToString()}</th>
                    <th style=""font-size:20px;"" colspan=""8"" scope =""colgroup"">Value: {portfolio.TotalValue}€<br>Risk Free: {portfolio.RiskFreeValue}€<br>Cash:{portfolio.Balance}€<br>DrawDown: {portfolio.DrawDown.ToString("P2")}<br>CAGR: {portfolio.CAGR.ToString("P2")}</th>
                </tr>
                <tr>
                    <th>Stock Name</th>
                    <th>Duration</th>
                    <th>Stop</th>
                    <th>TrailStop</th>
                    <th>Risk %</th>
                    <th>Portfolio Risk %</th>
                    <th>Return %</th>
                    <th>Portfolio Return %</th>
                    <th>Risk/Reward Ratio</th>
                </tr>
                </thead>
                <tbody>";

            string picturehtml = string.Empty;

            var positions = portfolio?.Positions.OrderBy(p => p.StockName).ToList();
            if (positions == null)
            {
                return string.Empty;
            }


            var previousState = this.WindowState;
            var previousSize = this.Size;
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(600, 600);

            var previousTheme = this.ViewModel.Theme;

            string reportBody = html;
            foreach (var position in positions)
            {
                StockSerie stockSerie = portfolio.GetStockSerieFromUic(position.Uic);

                if (stockSerie != null && stockSerie.Initialise() && stockSerie.Values.Count() > 50)
                {
                    var bitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(StockDictionary.Instruments[stockSerie.StockName], position.Theme, false, position.BarDuration);

                    var stockNameHtml = stockNamePortfolioTemplate.Replace("%STOCKNAME%", stockSerie.StockName) + "\r\n";
                    var lastValue = stockSerie.ValueArray.Last();
                    var risk = (position.Stop - position.EntryValue) / position.EntryValue;
                    var portfolioRisk = (position.Stop - position.EntryValue) * position.EntryQty / portfolio.TotalValue;
                    var positionReturn = (lastValue.CLOSE - position.EntryValue) / position.EntryValue;
                    var portfolioReturn = (lastValue.CLOSE - position.EntryValue) * position.EntryQty / portfolio.TotalValue;
                    var riskReward = (lastValue.CLOSE - position.EntryValue) / (position.EntryValue - position.Stop);
                    reportBody += rowTemplate.
                        Replace("%COL1%", stockNameHtml).
                        Replace("%COL2%", position.BarDuration.ToString()).
                        Replace("%COL3.1%", position.Stop.ToString("#.##")).
                        Replace("%COL3.2%", position.TrailStop.ToString("#.##")).
                        Replace("%COL4%", risk.ToString("P2")).
                        Replace("%COL5%", portfolioRisk.ToString("P2")).
                        Replace("%COL6%", positionReturn.ToString("P2")).
                        Replace("%COL7%", portfolioReturn.ToString("P2")).
                        Replace("%COL8%", riskReward.ToString("0.##"));
                    picturehtml += stockPictureTemplate.Replace("%STOCKNAME%", stockSerie.StockName).Replace("%DURATION%", position.BarDuration.ToString()).Replace("%IMG%", bitmapString) + "\r\n";
                }
                else
                {
                    var stockNameHtml = stockNamePortfolioTemplate.Replace("%STOCKNAME%", position.StockName) + "\r\n";
                    var risk = (position.Stop - position.EntryValue) / position.EntryValue;
                    var portfolioRisk = (position.Stop - position.EntryValue) * position.EntryQty / portfolio.TotalValue;

                    reportBody += rowTemplate.
                        Replace("%COL1%", stockNameHtml).
                        Replace("%COL2%", position.BarDuration.ToString()).
                        Replace("%COL3.1%", position.Stop.ToString("#.##")).
                        Replace("%COL3.2%", position.TrailStop.ToString("#.##")).
                        Replace("%COL4%", risk.ToString("P2")).
                        Replace("%COL5%", portfolioRisk.ToString("P2")).
                        Replace("%COL6%", " - ").
                        Replace("%COL7%", " - ").
                        Replace("%COL8%", " - ");
                }
            }

            var portfolioSerie = StockDictionary.Instruments[portfolio.Name];

            var portfolioSerieBitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(portfolioSerie, "_Portfolio2", false, BarDuration.Daily, 350);
            picturehtml = stockPictureTemplate.Replace("%STOCKNAME%", portfolio.Name).Replace(" - %DURATION%", "").Replace("%IMG%", portfolioSerieBitmapString) + "\r\n" + picturehtml;

            reportBody += @" 
</tbody>
</table>

";

            string orderHtml = $@"<br/>
    <table  class=""reportTable"" id=""PAGE_TOP"">
        <thead>
        <tr>
            <th style=""font-size:20px;"" rowspan=""1"">{portfolio.Name}    </th>   
            <th style=""font-size:20px;"" colspan=""4"" scope =""colgroup"">Open orders</th>
        </tr>
        <tr>
            <th>Stock Name</th>
            <th>Duration</th>
            <th>Stop</th>
            <th>Risk %</th>
            <th>Portfolio Risk %</th>
        </tr>
        </thead>
        <tbody>
            %ORDER_BODY%
        </tbody>
    </table>
";
            const string orderRowTemplate = @"
         <tr>
             <td>%COL1%</td>
             <td>%COL2%</td>
             <td>%COL3%</td>
             <td>%COL4%</td>
             <td>%COL5%</td>
         </tr>";

            string orderBody = string.Empty;
            string orderPictureHtml = string.Empty;

            foreach (var order in portfolio.SaxoOpenOrders.Where(o => o.BuySell == "Buy"))
            {
                StockSerie stockSerie = portfolio.GetStockSerieFromUic(order.Uic);
                if (stockSerie != null)
                {
                    var bitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(StockDictionary.Instruments[stockSerie.StockName], order.Theme, false, order.BarDuration, 350);

                    var stockNameHtml = stockNamePortfolioTemplate.Replace("%STOCKNAME%", order.StockName) + "\r\n";
                    var lastValue = stockSerie.LastValue;
                    var risk = (order.Stop - order.Price.Value) / order.Price.Value;
                    var portfolioRisk = (order.Stop - order.Price.Value) * order.Qty / portfolio.TotalValue;
                    var positionReturn = (lastValue.CLOSE - order.Price.Value) / order.Price.Value;
                    var portfolioReturn = (lastValue.CLOSE - order.Price.Value) * order.Qty / portfolio.TotalValue;
                    var riskReward = (lastValue.CLOSE - order.Price.Value) / (order.Price.Value - order.Stop);
                    orderBody += orderRowTemplate.
                    Replace("%COL1%", stockNameHtml).
                    Replace("%COL2%", order.BarDuration.ToString()).
                    Replace("%COL3%", order.Stop.ToString("#.##")).
                    Replace("%COL4%", risk.ToString("P2")).
                    Replace("%COL5%", portfolioRisk.ToString("P2"));
                    orderPictureHtml += stockPictureTemplate.Replace("%STOCKNAME%", stockSerie.StockName).Replace("%DURATION%", order.BarDuration.ToString()).Replace("%IMG%", bitmapString) + "\r\n";
                }
                else
                {
                    var stockNameHtml = stockNamePortfolioTemplate.Replace("%STOCKNAME%", order.StockName) + "\r\n";
                    var risk = (order.Stop - order.Price.Value) / order.Price.Value;
                    var portfolioRisk = (order.Stop - order.Price.Value) * order.Qty / portfolio.TotalValue;
                    orderBody += orderRowTemplate.
                    Replace("%COL1%", stockNameHtml).
                    Replace("%COL2%", order.BarDuration.ToString()).
                    Replace("%COL3%", order.Stop.ToString("#.##")).
                    Replace("%COL4%", risk.ToString("P2")).
                    Replace("%COL5%", portfolioRisk.ToString("P2"));
                }
            }

            if (!string.IsNullOrEmpty(orderBody))
            {
                reportBody += orderHtml.Replace("%ORDER_BODY%", orderBody);
            }
            reportBody += picturehtml;
            reportBody += orderPictureHtml;

            StockAnalyzerForm.MainFrame.WindowState = previousState;
            StockAnalyzerForm.MainFrame.Size = previousSize;
            this.ViewModel.Theme = previousTheme;

            return reportBody;
        }

        public void GeneratePortfolioReportFile(StockPortfolio portfolio)
        {
            if (portfolio.SaxoSilentLogin())
            {
                portfolio.Refresh();
            }

            this.Portfolio = portfolio;

            using var p = new MainViewModelContextPersister();

            string reportTemplate = File.ReadAllText(@"Resources\PortfolioTemplate.html").Replace("%HTML_TILE%", portfolio.Name + "Report " + DateTime.Today.ToShortDateString());

            string positionHtml = portfolio.GeneratePositionHtml();
            positionHtml = string.Empty;
            var htmlReport = reportTemplate.Replace("%POSITION%", positionHtml);

            var report = GeneratePortfolioReportHtml(portfolio);
            if (!string.IsNullOrEmpty(report))
            {
                htmlReport = htmlReport.Replace("%HTML_BODY%", report);
                string fileName = Path.Combine(Folders.PortfolioReport, $@"{portfolio.Name}.html");
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.Write(htmlReport);
                }
                Process.Start(fileName);
            }
        }

        public void GenerateAlertReport(BarDuration duration, List<StockAlertDef> alertDefs = null)
        {
            StockSplashScreen.ProgressText = $"Generating alert report - {duration}";

            alertDefs ??= StockAlertDef.AlertDefs.Where(a => a.BarDuration == duration && a.InReport).OrderBy(a => a.Rank).ToList();
            if (alertDefs.Count == 0)
                return;

            string timeFrame = duration.ToString();
            string folderName = Path.Combine(Folders.Report, timeFrame);

            CleanReportFolder(folderName);

            if (!File.Exists(Folders.ReportTemplate) || alertDefs.Count(a => a.InReport && a.Type == AlertType.Group) == 0)
                return;
            var htmlReportTemplate = File.ReadAllText(Folders.ReportTemplate);

            using var cp = new MainViewModelContextPersister();
            try
            {
                string fileName = Path.Combine(folderName, $"Report_{DateTime.Today.ToString("yyyy_MM_dd")}.html");
                string htmlBody = $"<h1 style=\"text-align: center;\">{duration} Report - {DateTime.Today.ToShortDateString()} {DateTime.Now.ToShortTimeString()}</h1>";

                #region Report Alerts

                var previousState = this.WindowState;
                var previousSize = this.Size;
                this.Size = new Size(600, 600);
                this.WindowState = FormWindowState.Normal;

                int nbLeaders = 40;
                StockSplashScreen.FadeInOutSpeed = 0.25;
                StockSplashScreen.ProgressVal = 0;
                StockSplashScreen.ShowSplashScreen();

                string htmlAlerts = string.Empty;
                foreach (var alertDef in alertDefs.Where(a => a.InReport && a.Type == AlertType.Group && a.Title != "Watchlist"))
                {
                    htmlAlerts += GenerateAlertTable(alertDef, nbLeaders);
                }
                htmlBody += htmlAlerts;

                // Generate EURO_A Summation Index
                if (StockDictionary.Instruments.ContainsKey("McClellanSum.EURO_A"))
                {
                    this.Size = new Size(950, 800);

                    var bitmapString = this.GetStockSnapshotAsHtml(StockDictionary.Instruments["McClellanSum.EURO_A"], "EUROA_SUM", false, BarDuration.Daily, 770);
                    htmlReportTemplate = htmlReportTemplate.Replace("%EURO_A_IMG%", bitmapString);
                }

                #region Report embedded definitions

                // Find Pattern
                string pattern = @"§§.*?§§";

                // Instantiate the regular expression object.
                Regex regex = new Regex(pattern);

                // Match the regular expression pattern against the input string.
                MatchCollection matches = regex.Matches(htmlReportTemplate);

                foreach (Match match in matches)
                {
                    var fields = match.Value.Replace("§§", "").Split('|');
                    var stockName = fields[0];
                    var barDuration = (BarDuration)Enum.Parse(typeof(BarDuration), fields[1]);
                    var theme = fields[2];
                    var nbBars = int.Parse(fields[3]);

                    var stockInstrument = StockDictionary.GetInstrumentByName(stockName);
                    if (stockInstrument == null)
                        continue;
                    var bitmapString = this.GetStockSnapshotAsHtml(stockInstrument, theme, true, barDuration, nbBars);
                    string data = $"\r\n    <h3>{stockName}</h3>\r\n    <a>\r\n        <img src=\"{bitmapString}\">\r\n    </a>";

                    htmlReportTemplate = htmlReportTemplate.Replace(match.Value, data);
                }
                #endregion

                StockSplashScreen.CloseForm(true);
                #endregion

                this.Size = previousSize;
                this.WindowState = previousState;

                var htmlReport = htmlReportTemplate.Replace("%HTML_TILE%", $"{duration} Report").Replace("%HTML_BODY%", htmlBody);
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.Write(htmlReport);
                }

                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
        }

        const string stockNameTemplate = "<a class=\"tooltip\">%MSG%<span><img src=\"%IMG%\"></a>";
        const string stockNamePortfolioTemplate = "<a href=\"#%STOCKNAME%\">%STOCKNAME%</a>";
        const string stockPictureTemplate = "<br/><h2 id=\"%STOCKNAME%\"><a href=\"#PAGE_TOP\">%STOCKNAME% - %DURATION%</a></h3><img alt=\"%STOCKNAME% - %DURATION% - Chart missing\" src=\"%IMG%\"/>";

        private string GenerateAlertTable(StockAlertDef alertDef, int nbStocks)
        {
            return GenerateReportTable(alertDef, nbStocks);
        }

        private string GenerateReportTable(StockAlertDef alertDef, int nbStocks)
        {
            const string rowTemplate = @"
         <tr>
             <td style=""font-size:11px;"">%COL1%</td>
             <td style=""font-size:11px;"">%GROUP%</td>
             <td>%COL2.1%</td>
             <td>%COL2.2%</td>
             <td>%COL3%</td>
             <td>%COL4%</td>
             <td>%COL5%</td>
             <td>%COL6%</td>
             <td>%COL7%</td>
         </tr>";
            string html = @"
<br/>
";
            try
            {
                StockSplashScreen.ProgressText = alertDef.Title + " " + alertDef.BarDuration + " for " + alertDef.Group;

                var rankIndicator = string.IsNullOrEmpty(alertDef.Speed) ? "ROR(35)" : alertDef.Speed;

                var stokPeriod = alertDef.Stok == 0 ? 35 : alertDef.Stok;

                var alerts = StockDictionary.Instance.MatchAlert(alertDef);

                StockSplashScreen.ProgressVal = 0;
                StockSplashScreen.ProgressMax = alerts.Count();
                StockSplashScreen.ProgressMin = 0;

                var alertValues = new List<StockAlertValue>();
                foreach (var alert in alerts)
                {
                    StockSplashScreen.ProgressVal++;
                    alertValues.Add(alert.GetAlertValue());
                }

                var tableHeader = $"{alertDef.Title}<br/>Stop: {alertDef.Stop}";
                html += $@"
            <table  class=""reportTable"">
                <thead>
                    <tr>
                        <th style=""font-size:20px;"" >{alertDef.Group}</th>
                        <th style=""font-size:20px;"" > {tableHeader} </th>
                    </tr>
                </thead>
            </table>
            <table  class=""reportTable sortable"">
                <thead>
                    <tr>
                        <th>Stock Name</th>
                        <th>Group</th>
                        <th>{rankIndicator}</th>
                        <th>STOK({stokPeriod})</th>
                        <th>Trail Stop %</th>
                        <th>Trail Stop</th>
                        <th>{alertDef.BarDuration} %</th>
                        <th>Value</th>
                        <th>Highest</th>
                    </tr>
                </thead>
                <tbody>";

                this.ViewModel.Theme = alertDef.Theme;
                foreach (var alertValue in alertValues.OrderByDescending(l => l.Speed).Take(nbStocks))
                {
                    var bitmapString = this.GetStockSnapshotAsHtml(alertValue.Instrument, alertValue.AlertDef.Theme, false, alertValue.AlertDef.BarDuration, 100);

                    var stockName = stockNameTemplate.Replace("%MSG%", alertValue.Instrument.DisplayName).Replace("%IMG%", bitmapString) + "\r\n";
                    var stokValue = alertValue.Stok;
                    if (float.IsNaN(alertValue.Stop))
                    {
                        html += rowTemplate.
                            Replace("%GROUP%", alertValue.Instrument.Group.ToString()).
                            Replace("%COL1%", stockName).
                            Replace("%COL2.1%", alertValue.Speed.ToString(alertValue.SpeedFormat)).
                            Replace("%COL2.2%", stokValue.ToString("#.##")).
                            Replace("%COL3%", "").
                            Replace("%COL4%", "").
                            Replace("%COL5%", alertValue.Variation.ToString("P2")).
                            Replace("%COL6%", alertValue.Value.ToString("#.##")).
                            Replace("%COL7%", alertValue.Highest.ToString());
                    }
                    else
                    {
                        html += rowTemplate.
                            Replace("%GROUP%", alertValue.Instrument.Group.ToString()).
                            Replace("%COL1%", stockName).
                            Replace("%COL2.1%", alertValue.Speed.ToString(alertValue.SpeedFormat)).
                            Replace("%COL2.2%", stokValue.ToString("#.##")).
                            Replace("%COL3%", ((alertValue.Value - alertValue.Stop) / alertValue.Value).ToString("P2")).
                            Replace("%COL4%", alertValue.Stop.ToString("#.##")).
                            Replace("%COL5%", alertValue.Variation.ToString("P2")).
                            Replace("%COL6%", alertValue.Value.ToString("#.##")).
                            Replace("%COL7%", alertValue.Highest.ToString());
                    }
                }
                html += @"
                    </tbody>
                    </table>";
            }
            catch (Exception exception)
            {
                html = string.Empty;
                StockLog.Write(exception);
            }
            return html;
        }

        private static void CleanReportFolder(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                foreach (string directory in (Directory.EnumerateDirectories(folderName)))
                {
                    Directory.Delete(directory, true);
                }
                foreach (string file in (Directory.EnumerateFiles(folderName)))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(folderName);
            }
        }
        #endregion
        WatchListDlg watchlistDlg = null;
        private void manageWatchlistsMenuItem_Click(object sender, EventArgs e)
        {
            if (StockWatchList.WatchLists == null)
                return;

            if (watchlistDlg == null)
            {
                watchlistDlg = new WatchListDlg(StockWatchList.WatchLists);
                watchlistDlg.SelectedInstrumentChanged += new SelectedInstrumentChangedEventHandler(OnSelectedInstrumentChanged);
                watchlistDlg.FormClosed += (a, b) =>
                {
                    if (watchlistDlg.DialogResult == DialogResult.OK)
                    {
                        this.SaveWatchList();
                    }
                    else
                    {
                        this.LoadWatchList();
                    }
                    watchlistDlg = null;
                };
                watchlistDlg.Show();
            }
            else
            {
                watchlistDlg.Activate();
            }
        }

        #region Stock Split Dlg
        private StockSplitDlg stockSplitDlg = null;
        private void stockSplitMenuItem_Click(object sender, EventArgs e)
        {
            var stockSplitDlg = new StockSplitDlg() { StartPosition = FormStartPosition.CenterScreen };
            var res = stockSplitDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                this.ApplyTheme();
            }
        }
        #endregion

        private void scriptEditorMenuItem_Click(object sender, EventArgs e)
        {
            ScriptDlg scriptEditor = new ScriptDlg() { StartPosition = FormStartPosition.CenterScreen };
            scriptEditor.Show();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                ZoomOut();
            }
            else
            {
                ZoomIn();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (searchCombo.Focused)
                return false;

            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if (this.themeComboBox.Focused || this.barDurationComboBox.Focused)
            { return false; }

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        // Interupt current drawings
                        this.ResetDrawingButtons();
                        this.Refresh();
                        break;
                    case Keys.Control | Keys.H:
                        this.graphCloseControl.Focus();
                        this.hideIndicatorsStockMenuItem.Checked = !this.hideIndicatorsStockMenuItem.Checked;
                        this.hideIndicatorsStockMenuItem_Click(null, null);
                        break;
                    //case Keys.Control | Keys.P:
                    //    this.graphCloseControl.Focus();
                    //    this.showPositionsMenuItem.Checked = !this.showPositionsMenuItem.Checked;
                    //    this.showPositionsMenuItem_Click(null, null);
                    //    break;
                    case Keys.Control | Keys.I:
                        selectDisplayedIndicatorMenuItem_Click(null, null);
                        break;
                    case Keys.Control | Keys.D:
                        this.showDrawingsMenuItem.Checked = !this.showDrawingsMenuItem.Checked;
                        showDrawingsMenuItem_Click(null, null);
                        break;
                    case Keys.M:
                        this.magnetStripBtn.Checked = !this.magnetStripBtn.Checked;
                        magnetStripBtn_Click(null, null);
                        break;
                    case Keys.X:
                        this.excludeButton_Click(null, null);
                        break;
                    case Keys.Control | Keys.C:
                        ClearSecondarySerie();
                        break;
                    case Keys.Control | Keys.V:
                        showVariationBtn_Click(null, null);
                        break;
                    case Keys.Control | Keys.L:
                        logScaleBtn_Click(null, null);
                        break;
                    case Keys.Control | Keys.Left:
                        {
                            Rewind((this.endIndex - this.startIndex) / 4);
                        }
                        break;
                    case Keys.Left:
                        {
                            Rewind(1);
                        }
                        break;
                    case Keys.Control | Keys.Right:
                        {
                            Forward((this.endIndex - this.startIndex) / 4);
                        }
                        break;
                    case Keys.Right:
                        {
                            Forward(1);
                        }
                        break;
                    case Keys.Down:
                        {
                            ZoomOut();
                        }
                        break;
                    case Keys.Up:
                        {
                            ZoomIn();
                        }
                        break;
                    case Keys.F5:
                        {
                            this.DownloadStock(false);
                        }
                        break;
                    case Keys.Control | Keys.F5:
                        {
                            this.DownloadStockGroup();
                        }
                        break;
                    case Keys.Shift | Keys.F5:
                        {
                            this.ForceDownloadStock(true);
                        }
                        break;
                    case Keys.Control | Keys.Shift | Keys.F5:
                        {
                            this.ForceDownloadStockGroup();
                        }
                        break;
                    case Keys.Control | Keys.X:
                        {
                            excludeButton_Click(null, null);
                        }
                        break;
                    default:
                        return base.ProcessCmdKey(ref msg, keyData);
                }
            }
            return true;
        }


        #region MULTI TIME FRAME VIEW
        void multipleTimeFrameViewMenuItem_Click(object sender, EventArgs e)
        {
            MultiTimeFrameChartDlg mtg = new MultiTimeFrameChartDlg() { StartPosition = FormStartPosition.CenterScreen };
            mtg.Initialize(this.ViewModel.Instrument);
            mtg.WindowState = FormWindowState.Maximized;
            mtg.Show();
        }
        #endregion

        private Point lastMouseLocation = Point.Empty;
        void MouseMoveOverGraphControl(object sender, MouseEventArgs e)
        {
            if (lastMouseLocation != e.Location)
            {
                try
                {
                    GraphControl graphControl = (GraphControl)sender;
                    if (!graphControl.GraphRectangle.Contains(e.Location) && graphControl != this.graphScrollerControl)
                    {
                        graphControl.DrawMouseCursor(e.Location);
                        return;
                    }
                    //if (graphControl.GraphRectangle.Contains(e.Location) && e.Location.X > graphControl.GraphRectangle.X)
                    if (e.Location.X > graphControl.GraphRectangle.X)
                    {
                        if (graphControl == this.graphScrollerControl && graphControl.IsInitialized)
                        {
                            graphControl.MouseMoveOverControl(e, Control.ModifierKeys, true);
                        }
                        else
                        {
                            foreach (GraphControl graph in graphList)
                            {
                                if (graph.IsInitialized)
                                {
                                    graph.MouseMoveOverControl(e, Control.ModifierKeys, graph == sender);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (graphControl.GraphRectangle.Contains(lastMouseLocation))
                        {
                            foreach (GraphControl graph in graphList)
                            {
                                if (graph.IsInitialized)
                                {
                                    graph.PaintForeground();
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    StockLog.Write(exception);
                }
                lastMouseLocation = e.Location;
            }
        }
        #region SECONDARY SERIE MENU
        private void ClearSecondarySerie()
        {
            ClearSecondarySerieMenu();

            Dictionary<string, List<string>> dico;
            if (this.ViewModel.Theme == WORK_THEME)
            {
                dico = this.themeDictionary[WORK_THEME];
            }
            else
            {
                // Create a dico copy
                dico = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in this.themeDictionary[this.ViewModel.Theme])
                {
                    dico.Add(entry.Key, entry.Value.Select(item => item).ToList());
                }
                this.themeDictionary[WORK_THEME] = dico;
                this.themeComboBox.SelectedItem = WORK_THEME;
            }

            dico["CloseGraph"].RemoveAll(s => s.StartsWith("SECONDARY"));
            dico["CloseGraph"].Add("SECONDARY|NONE");

            this.graphCloseControl.SecondaryFloatSerie = null;
            this.OnNeedReinitialise(false);
        }
        private void CheckSecondarySerieMenu(string stockName)
        {
            var instrument = StockDictionary.GetInstrumentByName(stockName);
            if (instrument != null)
            {
                foreach (ToolStripMenuItem groupMenuItem in this.secondarySerieMenuItem.DropDownItems)
                {
                    if (groupMenuItem.Text == instrument.Group.ToString())
                    {
                        groupMenuItem.Checked = true;
                    }
                    else
                    {
                        groupMenuItem.Checked = false;
                    }
                    foreach (ToolStripMenuItem subMenuItem in groupMenuItem.DropDownItems)
                    {
                        if (subMenuItem.Text == instrument.DisplayName)
                        {
                            subMenuItem.Checked = true;
                        }
                        else
                        {
                            subMenuItem.Checked = false;
                        }
                    }
                }
            }
            else
            {
                ClearSecondarySerieMenu();
            }
        }
        private void ClearSecondarySerieMenu()
        {
            this.secondarySerieMenuItem.Checked = false;
            foreach (ToolStripMenuItem otherMenuItem in this.secondarySerieMenuItem.DropDownItems)
            {
                foreach (ToolStripMenuItem subMenuItem in otherMenuItem.DropDownItems)
                {
                    subMenuItem.Checked = false;
                }
                otherMenuItem.Checked = false;
            }
        }
        private void secondarySerieMenuItem_Click(object sender, EventArgs e)
        {
            var dataSerie = this.ViewModel.Instrument?.GetDataSerie(this.ViewModel.BarDuration);
            if (dataSerie == null)
            {
                return;
            }
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            menuItem.Checked = !menuItem.Checked;

            FloatSerie secondarySerie = dataSerie.GenerateSecondarySerieFromOtherSerie(sender.ToString(), this.ViewModel.BarDuration);
            if (menuItem.Checked && secondarySerie == null)
            {
                menuItem.Checked = false;
            }
            ((ToolStripMenuItem)menuItem.OwnerItem).Checked = menuItem.Checked;
            foreach (ToolStripMenuItem otherMenuItem in this.secondarySerieMenuItem.DropDownItems)
            {
                foreach (ToolStripMenuItem subMenuItem in otherMenuItem.DropDownItems)
                {
                    if (subMenuItem != sender)
                    {
                        subMenuItem.Checked = false;
                    }
                }
                if (otherMenuItem != menuItem.OwnerItem)
                {
                    otherMenuItem.Checked = false;
                }
            }

            // Switch to working theme
            if (this.ViewModel.Theme != WORK_THEME)
            {
                // Create a dico copy
                Dictionary<string, List<string>> dico = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in this.themeDictionary[this.ViewModel.Theme])
                {
                    dico.Add(entry.Key, entry.Value.Select(item => item).ToList());
                }
                this.themeDictionary[WORK_THEME] = dico;
            }
            // Add/remove secondary from working theme
            this.themeDictionary[WORK_THEME]["CloseGraph"].RemoveAll(s => s.StartsWith("SECONDARY"));
            if (menuItem.Checked)
            {
                this.graphCloseControl.SecondaryFloatSerie = secondarySerie;
                this.themeDictionary[WORK_THEME]["CloseGraph"].Add("SECONDARY|" + sender.ToString());
            }
            else
            {
                this.graphCloseControl.SecondaryFloatSerie = null;
                this.themeDictionary[WORK_THEME]["CloseGraph"].Add("SECONDARY|NONE");
            }
            if (this.themeComboBox.SelectedItem.ToString() != WORK_THEME)
            {
                this.themeComboBox.SelectedItem = WORK_THEME;
            }
            else
            {
                this.ViewModel.Theme = WORK_THEME;
                this.ApplyTheme();
            }
        }
        #endregion
        #region THEME MANAGEMENT

        public IEnumerable<string> Themes => themeComboBox.Items.OfType<string>().Where(t => !t.Contains("*"));


        public void SetThemeFromIndicator(string fullName)
        {
            using (new MethodLogger(this))
            {
                if (this.themeDictionary.ContainsKey(this.ViewModel.Theme) && this.themeDictionary[this.ViewModel.Theme].Values.Any(v => v.Any(vv => vv.Contains(fullName))))
                {
                    return;
                }

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(StockViewableItemsManager.GetTheme(fullName))))
                {
                    using StreamReader sr = new StreamReader(ms);
                    this.LoadThemeStream(WORK_THEME, sr);
                }
                if (this.themeComboBox.SelectedItem.ToString() == WORK_THEME)
                {
                    this.ApplyTheme();
                }
                else
                {
                    this.themeComboBox.SelectedItem = WORK_THEME;
                }
            }
        }

        #region BEST TRENDS
        BestTrendDlg bestTrendDlg = null;
        void bestTrendViewMenuItem_Click(object sender, EventArgs e)
        {
            if (bestTrendDlg == null)
            {
                bestTrendDlg = new BestTrendDlg(this.ViewModel.Instrument.Group.ToString(), this.ViewModel.BarDuration);
                bestTrendDlg.Disposed += bestrendDialog_Disposed;
                bestTrendDlg.bestTrend1.SelectedInstrumentChanged += OnSelectedInstrumentAndDurationAndIndexChanged;
                bestTrendDlg.Show();
            }
            else
            {
                bestTrendDlg.Activate();
            }
        }
        void bestrendDialog_Disposed(object sender, EventArgs e)
        {
            this.bestTrendDlg = null;
        }
        #endregion
        #region Sector Dialog
        SectorDlg sectorDlg = null;
        void sectorViewMenuItem_Click(object sender, EventArgs e)
        {
            if (sectorDlg == null)
            {
                sectorDlg = new SectorDlg();
                sectorDlg.Disposed += sectorDialog_Disposed;
                sectorDlg.Show();
            }
            else
            {
                sectorDlg.Activate();
            }
        }

        void sectorDialog_Disposed(object sender, EventArgs e)
        {
            this.sectorDlg = null;
        }
        #endregion

        #region MARKET REPLAY
        MarketReplayDlg marketReplayDlg = null;
        void marketReplayViewMenuItem_Click(object sender, EventArgs e)
        {
            if (marketReplayDlg == null)
            {
                marketReplayDlg = new MarketReplayDlg(this.ViewModel.Instrument.Group, this.ViewModel.BarDuration);
                marketReplayDlg.Disposed += marketReplayDlg_Disposed;
                this.graphCloseControl.StopChanged += marketReplayDlg.OnStopValueChanged;
                marketReplayDlg.Show();
            }
            else
            {
                marketReplayDlg.Activate();
            }
        }

        void marketReplayDlg_Disposed(object sender, EventArgs e)
        {
            this.marketReplayDlg = null;
        }
        #endregion
        #region ALERT DIALOG
        StockAlertManagerDlg alertDefDlg = null;
        StockAlertManagerViewModel stockAlertManagerViewModel = null;
        public void showAlertDefDialogMenuItem_Click(object sender, EventArgs e)
        {
            if (alertDefDlg == null)
            {
                stockAlertManagerViewModel = new StockAlertManagerViewModel()
                {
                    StockName = this.ViewModel.Instrument?.DisplayName,
                    Group = this.ViewModel.Instrument.Group,
                    BarDuration = StockAnalyzerForm.MainFrame.ViewModel.BarDuration,
                    IndicatorNames = StockAnalyzerForm.MainFrame.GetIndicatorsFromCurrentTheme().Append(string.Empty)
                };
                stockAlertManagerViewModel.TriggerName = stockAlertManagerViewModel.IndicatorNames?.FirstOrDefault();
                stockAlertManagerViewModel.Stop = stockAlertManagerViewModel.StopNames?.FirstOrDefault();

                alertDefDlg = new StockAlertManagerDlg(stockAlertManagerViewModel);
                alertDefDlg.Disposed += delegate
                {
                    this.alertDefDlg = null;
                };
                alertDefDlg.Show();
            }
            else
            {
                alertDefDlg.WindowState = FormWindowState.Normal;
                alertDefDlg.Activate();
                alertDefDlg.TopMost = true;
                alertDefDlg.TopMost = false;
            }
        }
        #endregion
        #region DRAWING DIALOG
        DrawingDlg drawingDlg = null;
        void drawingDialogMenuItem_Click(object sender, EventArgs e)
        {
            if (drawingDlg == null)
            {
                drawingDlg = new DrawingDlg() { StartPosition = FormStartPosition.CenterScreen };
                drawingDlg.drawingControl1.SelectedInstrumentAndDurationChanged += OnSelectedInstrumentAndDurationChanged;
                drawingDlg.Disposed += delegate
                {
                    drawingDlg.drawingControl1.SelectedInstrumentAndDurationChanged -= OnSelectedInstrumentAndDurationChanged;
                    this.drawingDlg = null;
                };
                drawingDlg.Show();
            }
            else
            {
                drawingDlg.Activate();
            }
        }
        #endregion

        private void CandleStripButton_Click(object sender, EventArgs e)
        {
            this.barchartStripButton.Checked = !this.candleStripButton.Checked;
            this.linechartStripButton.Checked = !this.candleStripButton.Checked;
            this.GraphCloseControl.ChartMode = GraphChartMode.CandleStick;
            this.graphCloseControl.ForceRefresh();
        }
        private void BarchartStripButton_Click(object sender, EventArgs e)
        {
            this.candleStripButton.Checked = !this.barchartStripButton.Checked;
            this.linechartStripButton.Checked = !this.barchartStripButton.Checked;
            this.GraphCloseControl.ChartMode = GraphChartMode.BarChart;
            this.graphCloseControl.ForceRefresh();
        }
        private void LinechartStripButton_Click(object sender, EventArgs e)
        {
            this.candleStripButton.Checked = !this.linechartStripButton.Checked;
            this.barchartStripButton.Checked = !this.linechartStripButton.Checked;
            this.GraphCloseControl.ChartMode = GraphChartMode.LineCross;
            this.graphCloseControl.ForceRefresh();
        }

        private void darkModeStripButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Settings.Default.DarkMode = darkModeStripButton.Checked;

            DrawingItem.DefaultPen = ColorManager.GetPen("Graph.Drawing", 1);

            this.ApplyTheme();
        }

        private void selectDisplayedIndicatorMenuItem_Click(object sender, EventArgs e)
        {
            using (new MethodLogger(this))
            {
                StockIndicatorSelectorDlg indicatorSelectorDialog = new StockIndicatorSelectorDlg(this.themeDictionary[this.ViewModel.Theme]) { StartPosition = FormStartPosition.CenterScreen };
                indicatorSelectorDialog.ThemeEdited += new OnThemeEditedHandler(indicatorSelectorDialog_ThemeEdited);

                if (indicatorSelectorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Apply new indicator1Name configuration
                    this.themeDictionary[WORK_THEME] = indicatorSelectorDialog.GetTheme();
                    if (this.themeComboBox.SelectedItem.ToString() == WORK_THEME)
                    {
                        this.ApplyTheme();
                    }
                    else
                    {
                        this.themeComboBox.SelectedItem = WORK_THEME;
                    }
                }
                else
                {
                    // Revert to selected theme
                    this.ViewModel.Theme = this.themeComboBox.SelectedItem.ToString();
                }
                //
                OnNeedReinitialise(false);
            }
        }
        void indicatorSelectorDialog_ThemeEdited(Dictionary<string, List<string>> themeDico)
        {
            // Apply new working theme
            this.themeDictionary[WORK_THEME] = themeDico;
            this.ViewModel.Theme = WORK_THEME;
        }
        public delegate void OnStrategyChangedHandler(string currentStrategy);
        public delegate void OnThemeChangedHandler(string currentTheme);
        public delegate void OnThemeEditedHandler(Dictionary<string, List<string>> themeDico);
        readonly Dictionary<string, Dictionary<string, List<string>>> themeDictionary = new Dictionary<string, Dictionary<string, List<string>>>();

        public Dictionary<string, List<string>> GetCurrentTheme()
        {
            if (!this.themeDictionary.ContainsKey(this.ViewModel.Theme))
                // LoadTheme
                if (!LoadCurveTheme(this.ViewModel.Theme))
                    return null;
            return this.themeDictionary[this.ViewModel.Theme];
        }
        public Dictionary<string, List<string>> GetTheme(string themeName)
        {
            if (!this.themeDictionary.ContainsKey(themeName))
                // LoadTheme
                if (!LoadCurveTheme(themeName))
                    return null;
            return this.themeDictionary[themeName];
        }
        public List<string> GetIndicatorsFromCurrentTheme()
        {
            var indicatorNames = new List<string>();
            foreach (var section in this.GetCurrentTheme())
            {
                foreach (var line in section.Value.Where(l => l.StartsWith("INDICATOR") || l.StartsWith("CLOUD") || l.StartsWith("PAINTBAR") || l.StartsWith("AUTODRAWING") || l.StartsWith("TRAILSTOP") || l.StartsWith("DECORATOR") || l.StartsWith("TRAIL")))
                {
                    var fields = line.Split('|');
                    indicatorNames.Add($"{fields[0]}|{fields[1]}");
                }
            }
            return indicatorNames;
        }

        void StockAnalyzerForm_ThemeChanged()
        {
            if (string.IsNullOrEmpty(this.ViewModel.Theme))
            {
                // Add error management here
                throw new Exception("We don't deal with empty themes in this house");
            }
            else
            {
                if (this.ViewModel.Instrument?.StockAnalysis?.Theme == this.ViewModel.Theme)
                {
                    this.defaultThemeStripButton.CheckState = CheckState.Checked;
                }
                else
                {
                    this.defaultThemeStripButton.CheckState = CheckState.Unchecked;
                }
                ApplyTheme();
            }
        }

        public void ApplyTheme()
        {
            if (this.ViewModel.Instrument == null || !GraphControl.IsStarted || this.ViewModel.Theme == null)
                return;

            using var ml = new MethodLogger(this, showTimerDebug);
            try
            {
                this.Cursor = Cursors.WaitCursor;

                StockLog.Write($"Apply theme {this.ViewModel.Instrument.DisplayName}-{this.ViewModel.BarDuration}-{this.ViewModel.Theme}");

                // Get DataSerie for bar duration
                var dataSerie = ViewModel.Instrument.GetDataSerie(this.ViewModel.BarDuration);
                if (dataSerie == null || dataSerie.Count == 0)
                {
                    this.DeactivateGraphControls("Data for " + this.ViewModel.Instrument.DisplayName + " cannot be initialised");
                    return;
                }

                // Delete transient drawing created by alert Detection
                if (this.ViewModel.Instrument.StockAnalysis.DeleteTransientDrawings() > 0)
                {
                    this.ViewModel.Instrument.StockSerie.ResetIndicatorCache();
                }

                if (dataSerie.Count < MIN_BAR_DISPLAY)
                {
                    this.DeactivateGraphControls("Not enough data to display...");
                    return;
                }

                // Add to browsing history
                this.ViewModel.AddHistory(this.ViewModel.Instrument, this.ViewModel.Theme);

                // Force resetting the secondary serie.
                if (themeDictionary[this.ViewModel.Theme]["CloseGraph"].FindIndex(s => s.StartsWith("SECONDARY")) == -1)
                {
                    if (this.graphCloseControl.SecondaryFloatSerie != null)
                    {
                        themeDictionary[this.ViewModel.Theme]["CloseGraph"].Add("SECONDARY|" + this.graphCloseControl.SecondaryFloatSerie.Name);
                    }
                    else
                    {
                        themeDictionary[this.ViewModel.Theme]["CloseGraph"].Add("SECONDARY|NONE");
                    }
                }

                GraphCurveTypeList curveList;
                bool skipEntry = false;
                foreach (string entry in themeDictionary[this.ViewModel.Theme].Keys)
                {
                    if (entry.ToUpper().EndsWith("GRAPH"))
                    {
                        GraphControl graphControl = null;
                        curveList = new GraphCurveTypeList();
                        switch (entry.ToUpper())
                        {
                            case "CLOSEGRAPH":
                                graphControl = this.graphCloseControl;
                                break;
                            case "SCROLLGRAPH":
                                graphControl = this.graphScrollerControl;
                                break;
                            case "INDICATOR1GRAPH":
                                graphControl = this.graphIndicator1Control;
                                break;
                            case "INDICATOR2GRAPH":
                                graphControl = this.graphIndicator2Control;
                                break;
                            case "INDICATOR3GRAPH":
                                graphControl = this.graphIndicator3Control;
                                break;
                            case "VOLUMEGRAPH":
                                if (dataSerie.HasVolume)
                                {
                                    graphControl = this.graphVolumeControl;
                                    curveList.Add(new GraphCurveType(dataSerie.GetSerie(StockDataType.EXCHANGED), Pens.Green, true));
                                }
                                else
                                {
                                    this.graphVolumeControl.Deactivate("This serie has no volume data", false);
                                    skipEntry = true;
                                }
                                break;
                            default:
                                continue;
                        }

                        if (skipEntry)
                        {
                            skipEntry = false;
                            continue;
                        }
                        try
                        {
                            List<HLine> horizontalLines = new List<HLine>();

                            foreach (string line in this.themeDictionary[this.ViewModel.Theme][entry])
                            {
                                string[] fields = line.Split('|');
                                switch (fields[0].ToUpper())
                                {
                                    case "GRAPH":
                                        //string[] colorItem = fields[1].Split(':');
                                        //graphControl.BackgroundColor = ColorManager.GetColor("Graph.Background");
                                        //colorItem = fields[2].Split(':');
                                        ////graphControl.TextBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                        //// graphControl.ShowGrid = bool.Parse(fields[3]);
                                        //colorItem = fields[4].Split(':');

                                        if (entry.ToUpper() == "CLOSEGRAPH")
                                        {
                                            graphControl.ChartMode = (GraphChartMode)Enum.Parse(typeof(GraphChartMode), fields[5]);
                                            // Set buttons
                                            switch (graphControl.ChartMode)
                                            {
                                                case GraphChartMode.Line:
                                                    this.barchartStripButton.Checked = false;
                                                    this.candleStripButton.Checked = false;
                                                    this.linechartStripButton.Checked = true;
                                                    break;
                                                case GraphChartMode.BarChart:
                                                    this.barchartStripButton.Checked = true;
                                                    this.candleStripButton.Checked = false;
                                                    this.linechartStripButton.Checked = false;
                                                    break;
                                                case GraphChartMode.CandleStick:
                                                    this.barchartStripButton.Checked = false;
                                                    this.candleStripButton.Checked = true;
                                                    this.linechartStripButton.Checked = false;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        break;
                                    case "SECONDARY":
                                        if (fields[1].ToUpper() == "NONE" || !StockDictionary.Instruments.ContainsKey(fields[1]))
                                        {
                                            ClearSecondarySerieMenu();
                                            this.graphCloseControl.SecondaryFloatSerie = null;
                                        }
                                        else
                                        {
                                            if (StockDictionary.Instruments.ContainsKey(fields[1]))
                                            {
                                                CheckSecondarySerieMenu(fields[1]);
                                                this.graphCloseControl.SecondaryFloatSerie = dataSerie.GenerateSecondarySerieFromOtherSerie(fields[1], this.ViewModel.BarDuration);
                                            }
                                        }
                                        break;
                                    case "DATA":
                                        curveList.Add(
                                            new GraphCurveType(
                                                dataSerie.GetSerie(
                                                    (StockDataType)Enum.Parse(typeof(StockDataType), fields[1])),
                                         fields[2], bool.Parse(fields[3])));
                                        break;
                                    case "TRAIL":
                                    case "INDICATOR":
                                        {
                                            IStockIndicator stockIndicator = (IStockIndicator)StockViewableItemsManager.GetViewableItem(line, dataSerie);
                                            if (stockIndicator != null)
                                            {
                                                if (entry.ToUpper() != "CLOSEGRAPH")
                                                {
                                                    if (stockIndicator.DisplayTarget == IndicatorDisplayTarget.RangedIndicator)
                                                    {
                                                        IRange range = (IRange)stockIndicator;
                                                        ((GraphRangedControl)graphControl).RangeMin = range.Min;
                                                        ((GraphRangedControl)graphControl).RangeMax = range.Max;
                                                    }
                                                    else
                                                    {
                                                        ((GraphRangedControl)graphControl).RangeMin = float.NaN;
                                                        ((GraphRangedControl)graphControl).RangeMax = float.NaN;
                                                    }
                                                }
                                                if (!(stockIndicator.RequiresVolumeData && !this.ViewModel.Instrument.StockSerie.HasVolume))
                                                {
                                                    curveList.Indicators.Add(stockIndicator);
                                                }
                                            }
                                        }
                                        break;
                                    case "CLOUD":
                                        {
                                            var stockCloud = (IStockCloud)StockViewableItemsManager.GetViewableItem(line, dataSerie);
                                            if (stockCloud != null)
                                            {
                                                curveList.Cloud = stockCloud;
                                            }
                                        }
                                        break;
                                    case "AUTODRAWING":
                                        {
                                            IStockAutoDrawing autoDrawing = (IStockAutoDrawing)StockViewableItemsManager.GetViewableItem(line, dataSerie);
                                            curveList.AutoDrawing = autoDrawing;
                                        }
                                        break;
                                    case "DECORATOR":
                                        {
                                            IStockDecorator decorator = (IStockDecorator)StockViewableItemsManager.GetViewableItem(line, dataSerie);
                                            curveList.Decorator = decorator;
                                            this.GraphCloseControl.CurveList.ShowMes.Add(decorator);
                                        }
                                        break;
                                    case "TRAILSTOP":
                                        {
                                            IStockTrailStop trailStop = (IStockTrailStop)StockViewableItemsManager.GetViewableItem(line, dataSerie);
                                            curveList.TrailStop = trailStop;
                                        }
                                        break;
                                    case "LINE":
                                        horizontalLines.Add(new HLine(float.Parse(fields[1]), GraphCurveType.PenFromString(fields[2])));
                                        break;
                                    default:
                                        continue;
                                }
                            }
                            if (curveList.FindIndex(c => c.DataSerie.Name == "CLOSE") < 0)
                            {
                                curveList.Insert(0,
                                    new GraphCurveType(dataSerie.GetSerie(StockDataType.CLOSE), Pens.Black,
                                        false));
                            }
                            if (graphControl == this.graphCloseControl)
                            {
                                if (curveList.FindIndex(c => c.DataSerie.Name == "LOW") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(dataSerie.GetSerie(StockDataType.LOW), Pens.Black,
                                            true));
                                }
                                if (curveList.FindIndex(c => c.DataSerie.Name == "HIGH") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(dataSerie.GetSerie(StockDataType.HIGH), Pens.Black,
                                            true));
                                }
                                if (curveList.FindIndex(c => c.DataSerie.Name == "OPEN") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(dataSerie.GetSerie(StockDataType.OPEN), Pens.Black,
                                            false));
                                }
                            }
                            if (!this.ViewModel.Instrument.StockAnalysis.DrawingItems.ContainsKey(this.ViewModel.BarDuration))
                            {
                                this.ViewModel.Instrument.StockAnalysis.DrawingItems.Add(this.ViewModel.BarDuration, new StockDrawingItems());
                            }
                            graphControl.Initialize(curveList, horizontalLines, dataSerie,
                                this.ViewModel.Instrument.StockAnalysis.DrawingItems[this.ViewModel.BarDuration],
                                startIndex, endIndex);
                        }
                        catch (Exception exception)
                        {
                            StockAnalyzerException.MessageBox(exception);
                            StockLog.Write("Exception loading theme: " + this.ViewModel.Theme);
                            foreach (string line in this.themeDictionary[this.ViewModel.Theme][entry])
                            {
                                StockLog.Write(line);
                            }
                            StockLog.Write(exception);
                        }
                    }
                }

                if (ViewModel.Instrument.BelongsToGroup(Groups.BREADTH))
                {
                    string[] fields = this.ViewModel.Instrument.DisplayName.Split('.');
                    if (fields.Length > 1 && StockDictionary.Instance.ContainsKey(fields[1]))
                    {
                        this.graphCloseControl.SecondaryFloatSerie = dataSerie.GenerateSecondarySerieFromOtherSerie(fields[1], this.ViewModel.BarDuration);
                    }
                }

                // Reinitialise zoom and hide useless indcators panels
                var collapsedState1 = this.graphList.Select(g => g.IsCollapsed).ToList();

                ResetZoom();

                var collapsedState2 = this.graphList.Select(g => g.IsCollapsed).ToList();
                var needCollapseReset = !collapsedState1.SequenceEqual(collapsedState2);
                if (needCollapseReset)
                    indicatorLayoutPanel.SetRows(this.graphList);


                indicatorLayoutPanel.BackColor = ColorManager.GetColor("Graph.Background");
            }

            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        void portfolioComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.portfolio != portfolioComboBox.SelectedItem)
            {
                this.Portfolio = portfolioComboBox.SelectedItem as StockPortfolio;
                this.graphCloseControl.ForceRefresh();
            }
        }
        void refreshPortfolioBtn_Click(object sender, EventArgs e)
        {
            var refreshAll = Control.ModifierKeys == Keys.Control;
            if (refreshAll)
            {
                foreach (var p in this.Portfolios.Where(p => !string.IsNullOrEmpty(p.SaxoAccountId) && !p.IsSaxoSimu))
                {
                    p.Refresh();
                    if (p.SaxoSilentLogin())
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.GreenIcon;
                        this.portfolioStatusLbl.ToolTipText = "Connected";
                    }
                    else
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.RedIcon;
                        this.portfolioStatusLbl.ToolTipText = "Not Connected";
                    }
                }
            }
            else
            {
                if (this.portfolio != null)
                {
                    this.portfolio.Refresh();
                    if (portfolio.SaxoSilentLogin())
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.GreenIcon;
                        this.portfolioStatusLbl.ToolTipText = "Connected";
                    }
                    else
                    {
                        this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.RedIcon;
                        this.portfolioStatusLbl.ToolTipText = "Not Connected";
                    }
                    this.graphCloseControl.ForceRefresh();
                }
            }
        }

        void themeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.ViewModel.Theme != themeComboBox.SelectedItem.ToString())
                {
                    this.ViewModel.Theme = themeComboBox.SelectedItem.ToString();

                    if (this.ViewModel.Theme != WORK_THEME)
                    {
                        Settings.Default.SelectedTheme = themeComboBox.SelectedItem.ToString();
                        Settings.Default.Save();
                    }

                    this.ViewModel.Theme = this.ViewModel.Theme;
                }
            }
            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
        }
        private void InitialisePortfolioCombo()
        {
            // Initialise Combo values
            portfolioComboBox.ComboBox.DataSource = PortfolioDataProvider.Portfolios;
            portfolioComboBox.ComboBox.DisplayMember = "Name";
            portfolioComboBox.ComboBox.ValueMember = "Name";
        }
        private void InitialiseThemeCombo()
        {
            // Initialise Combo values
            themeComboBox.Items.Clear();

            string folderName = Folders.Theme;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

                var themeFiles = Directory.EnumerateFiles(folderName, "*.thm");
                if (themeFiles.Count() == 0)
                {
                    // Create a default empty theme
                    string emptyTheme = "#ScrollGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line\r\nDATA|CLOSE|1:255:0:0:0:Solid|True\r\n#CloseGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line\r\nDATA|CLOSE|1:255:0:0:0:Solid|True\r\nSECONDARY|NONE\r\n#Indicator1Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#Indicator2Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#Indicator3Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#VolumeGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart";
                    using StreamWriter tw = new StreamWriter(folderName + @"\\" + Localisation.UltimateChartistStrings.ThemeEmpty + ".thm");
                    tw.Write(emptyTheme);
                }
            }

            foreach (string themeFileName in Directory.EnumerateFiles(folderName, "*.thm"))
            {
                var themeName = Path.GetFileNameWithoutExtension(themeFileName);
                themeComboBox.Items.Add(themeName);

                LoadCurveTheme(themeName);
            }

            //
            if (!string.IsNullOrEmpty(this.ViewModel.Instrument?.StockAnalysis?.Theme))
            {
                if (this.themeComboBox.Items.Contains(this.ViewModel.Instrument.StockAnalysis.Theme))
                {
                    this.themeComboBox.SelectedItem = this.ViewModel.Instrument.StockAnalysis.Theme;
                }
                else if (themeComboBox.Items.Contains(Settings.Default.SelectedTheme))
                {
                    themeComboBox.SelectedItem = Settings.Default.SelectedTheme;
                }
                else
                {
                    themeComboBox.SelectedItem = themeComboBox.Items[0];
                }
            }
            else
            {
                if (themeComboBox.Items.Contains(Settings.Default.SelectedTheme))
                {
                    themeComboBox.SelectedItem = Settings.Default.SelectedTheme;
                }
                else
                {
                    themeComboBox.SelectedItem = themeComboBox.Items[0];
                }
            }

            themeComboBox.Items.Add(WORK_THEME);
            // Create working theme from selected theme
            Dictionary<string, List<string>> dico = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> entry in this.themeDictionary[themeComboBox.SelectedItem.ToString()])
            {
                dico.Add(entry.Key, entry.Value.Select(item => item).ToList());
            }
            this.themeDictionary.Add(WORK_THEME, dico);
        }
        private void SaveCurveTheme(string fileName)
        {
            using StreamWriter sr = new StreamWriter(fileName);
            foreach (string entry in themeDictionary[this.ViewModel.Theme].Keys)
            {
                sr.WriteLine("#" + entry);
                foreach (string line in themeDictionary[this.ViewModel.Theme][entry])
                {
                    sr.WriteLine(line);
                }
            }
        }
        private bool LoadCurveTheme(string themeName)
        {
            try
            {
                // Load Curve Theme
                string fileName = Path.Combine(Folders.Theme, themeName + ".thm");
                if (File.Exists(fileName))
                {
                    using StreamReader sr = new StreamReader(fileName);
                    LoadThemeStream(themeName, sr);
                    return true;
                }
            }
            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
            return false;
        }

        private void LoadThemeStream(string themeName, StreamReader sr)
        {
            Dictionary<string, List<string>> dico;
            if (this.themeDictionary.ContainsKey(themeName))
            {
                this.themeDictionary[themeName].Clear();
                dico = this.themeDictionary[themeName];
            }
            else
            {
                dico = new Dictionary<string, List<string>>();
                this.themeDictionary.Add(themeName, dico);
            }

            string entry = string.Empty;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("#"))
                {
                    entry = line.Replace("#", "");
                    dico.Add(entry, new List<string>());
                }
                else
                {
                    dico[entry].Add(line);
                }
            }
        }

        void defaultThemeStripButton_Click(object sender, EventArgs e)
        {
            if (this.ViewModel.Instrument?.StockAnalysis == null)
                return;

            if (this.ViewModel.Theme == WORK_THEME)
            {
                this.saveThemeMenuItem_Click(sender, e);
                if (this.ViewModel.Theme == WORK_THEME)
                {
                    return;
                }
                this.ViewModel.Instrument.StockAnalysis.Theme = this.ViewModel.Theme;
                this.defaultThemeStripButton.CheckState = CheckState.Checked;
            }
            else if (this.defaultThemeStripButton.CheckState == CheckState.Checked)
            {
                this.ViewModel.Instrument.StockAnalysis.Theme = string.Empty;
                this.defaultThemeStripButton.CheckState = CheckState.Unchecked;
            }
            else
            {
                this.ViewModel.Instrument.StockAnalysis.Theme = this.ViewModel.Theme;
                this.defaultThemeStripButton.CheckState = CheckState.Checked;
            }
            SaveAnalysis(this.ViewModel.AnalysisFile);
        }
        void deleteThemeStripButton_Click(object sender, EventArgs e)
        {
            if (this.ViewModel.Theme == WORK_THEME)
            {
                return;
            }

            // delete theme file
            string fileName = Path.Combine(Folders.Theme, this.ViewModel.Theme + ".thm");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            foreach (var instrument in StockDictionary.Instruments.Values)
            {
                if (instrument.StockAnalysis.Theme == this.ViewModel.Theme)
                {
                    instrument.StockAnalysis.Theme = string.Empty;
                }
            }

            this.themeComboBox.SelectedItem = EMPTY_THEME;
            this.themeComboBox.Items.Remove(this.ViewModel.Theme);
        }
        #endregion


        PaletteManagerDlg paletteManagerDlg;
        void aboutMenuItem_Click(object sender, EventArgs e)
        {
            if (paletteManagerDlg == null)
            {
                paletteManagerDlg = new PaletteManagerDlg() { StartPosition = FormStartPosition.CenterScreen };
                paletteManagerDlg.FormClosed += PaletteManagerDlg_FormClosed;
                paletteManagerDlg.Show(this);

                paletteManagerDlg.ViewModel.ColorPaletteChanged += (s, e) =>
                {
                    OnNeedReinitialise(false);
                };
            }
            else
            {
                paletteManagerDlg.BringToFront();
            }
            return;

            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
        }

        private void PaletteManagerDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            paletteManagerDlg = null;
        }

        #region FILE MENU HANDLERS
        private void newAnalysisMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var instrument in StockDictionary.Instruments.Values)
            {
                instrument.StockAnalysis.Clear();
            }
            this.saveAnalysisFileAsMenuItem_Click(sender, e);

            OnNeedReinitialise(true);
        }
        private void loadAnalysisFileMenuItem_Click(object sender, EventArgs e)
        {
            string folderName = Folders.PersonalFolder;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "ulc";
            openFileDialog.Filter = "Ultimate Chartist Analysis files (*.ulc)|*.ulc";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.InitialDirectory = folderName;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string analysisFileName = openFileDialog.FileName;
                this.LoadAnalysis(analysisFileName);

                this.ViewModel.AnalysisFile = analysisFileName;

                // Apply the theme of the loaded analysis file if any
                if (!string.IsNullOrEmpty(this.ViewModel.Instrument?.StockAnalysis?.Theme))
                {
                    this.ViewModel.Theme = this.ViewModel.Instrument?.StockAnalysis?.Theme;
                }
            }
        }
        private void saveAnalysisFileMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveAnalysis(this.ViewModel.AnalysisFile);
        }
        private void saveAnalysisFileAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "ulc";
            saveFileDialog.Filter = "Ultimate Chartist Analysis files (*.ulc)|*.ulc";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.InitialDirectory = Folders.PersonalFolder;
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string analysisFileName = saveFileDialog.FileName;
                this.SaveAnalysis(analysisFileName);
                this.ViewModel.AnalysisFile = analysisFileName;
                Settings.Default.Save();
            }
        }
        private void saveThemeMenuItem_Click(object sender, EventArgs e)
        {
            List<string> themeList = new List<string>();
            foreach (object theme in this.themeComboBox.Items)
            {
                themeList.Add(theme.ToString());
            }
            themeList.Sort();

            SaveThemeForm saveThemeForm = new SaveThemeForm(themeList);
            if (saveThemeForm.ShowDialog() == DialogResult.OK)
            {
                SaveCurveTheme(Path.Combine(Folders.Theme, saveThemeForm.Theme + ".thm"));
                if (!this.themeComboBox.Items.Contains(saveThemeForm.Theme))
                {
                    this.themeComboBox.Items.Add(saveThemeForm.Theme);
                }
                else
                {
                    this.LoadCurveTheme(saveThemeForm.Theme);
                }
                this.themeComboBox.SelectedItem = saveThemeForm.Theme;
            }
        }

        PreferenceDialog prefDlg;
        private void folderPrefMenuItem_Click(object sender, EventArgs e)
        {
            if (prefDlg == null)
            {
                prefDlg = new PreferenceDialog() { StartPosition = FormStartPosition.CenterScreen };
                prefDlg.FormClosed += PrefDlg_FormClosed;
            }
            prefDlg.Show();
            OnNeedReinitialise(true);
        }

        private void PrefDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.prefDlg = null;
        }

        private void InitDataProviderMenuItem()
        {
            // Clean existing menus
            this.configDataProviderMenuItem.DropDownItems.Clear();
            List<ToolStripMenuItem> dataProviderSubMenuItems = new List<ToolStripMenuItem>();
            ToolStripMenuItem dataProviderSubMenuItem;

            // Create group menu items
            foreach (IConfigDialog configDlg in StockDataProviderBase.GetConfigDialogs())
            {
                dataProviderSubMenuItem = new ToolStripMenuItem(configDlg.DisplayName);
                dataProviderSubMenuItem.Click += new EventHandler(configDataProviderMenuItem_Click);
                dataProviderSubMenuItem.Tag = configDlg;
                dataProviderSubMenuItems.Add(dataProviderSubMenuItem);
            }
            this.configDataProviderMenuItem.DropDownItems.AddRange(dataProviderSubMenuItems.ToArray());
        }
        private void configDataProviderMenuItem_Click(object sender, EventArgs e)
        {
            var configDialog = ((IConfigDialog)((ToolStripMenuItem)sender).Tag);
            if (configDialog.ShowDialog(StockDictionary.Instance) == DialogResult.OK)
            {
                var dataProvider = (IStockDataProvider)configDialog;
                dataProvider.InitDictionary(StockDictionary.Instance, true);
                this.CreateSecondarySerieMenuItem();
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
        #region EDIT MENU ITEMS
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.Undo();
            OnNeedReinitialise(true);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.Redo();
            OnNeedReinitialise(true);
        }

        private void eraseDrawingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.EraseAllDrawingItems();
            OnNeedReinitialise(true);
        }
        private void eraseAllDrawingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want erase all drawings on all stocks ?\r\nChanges will apply when saving analysis file.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }
            foreach (var serie in StockDictionary.Instruments.Values)
            {
                serie.StockAnalysis.DrawingItems.Clear();
            }
            OnNeedReinitialise(true);
        }
        #endregion

        internal void OpenInZBMenu()
        {
            if (string.IsNullOrWhiteSpace(this.ViewModel.Instrument?.Isin))
                return;
            string url = "https://www.zonebourse.com/recherche/?q=%ISIN%";
            url = url.Replace("%ISIN%", this.ViewModel.Instrument.Isin);
            Process.Start(url);
        }
        internal void openInTradingViewMenu()
        {
            string url = $"https://www.tradingview.com/";
            if (this.ViewModel.Instrument.BelongsToGroup(Groups.PEA_EURONEXT))
            {
                url = $"https://www.tradingview.com/symbols/EURONEXT-{this.ViewModel.Instrument.Symbol}/financials-statistics-and-ratios/";
            }
            else
            {
                Clipboard.SetText(ViewModel.Instrument.DisplayName);
            }
            Process.Start(url);
        }

        internal void OpenInDataProvider()
        {
            var dataProvider = DataProviderBase.GetDataProvider(this.ViewModel.Instrument.Provider);
            if (dataProvider == null)
            {
                return;
            }
            dataProvider.OpenInDataProvider(this.ViewModel.Instrument);
        }
        internal void OpenInYahoo()
        {
            if (string.IsNullOrWhiteSpace(this.ViewModel.Instrument.Isin))
                return;

            YahooSearchResult searchResult = YahooDataProvider.SearchFromYahoo(this.ViewModel.Instrument.Isin);
            if (searchResult?.quotes != null && searchResult.quotes.Count > 0)
            {
                string url = $"https://finance.yahoo.com/quote/{searchResult.quotes[0].symbol}/";
                Process.Start(url);
            }
            else
            {
                string url = $"https://finance.yahoo.com/lookup/?s={this.ViewModel.Instrument.DisplayName}";
                Process.Start(url);
            }
        }
        internal void OpenSaxoIntradyConfigDlg(long saxoId)
        {
            SaxoIntradayDataProvider dataProvider = StockDataProviderBase.GetDataProvider(StockDataProvider.SaxoIntraday) as SaxoIntradayDataProvider;
            if (dataProvider == null)
            {
                return;
            }
            if (dataProvider.ShowDialog(saxoId) == DialogResult.OK)
            {
                dataProvider.InitDictionary(StockDictionary.Instance, true);
                this.CreateSecondarySerieMenuItem();
            }
        }

        private static string syncFileName = ".LastSync.txt";
        private static void OneDriveSync(bool wait)
        {
            if (Environment.MachineName == "DADELCARBO")
            {
                var lastSyncPath = Path.Combine(Folders.PersonalFolder, syncFileName);
                File.WriteAllText(lastSyncPath, Environment.MachineName + " => " + DateTime.UtcNow.ToString("o"));
                return;
            }


            var oneDriveSyncFolder = @"C:\ProgramData\UltimateChartist\OneDriveSync";
            var oneDriveSyncExe = Path.Combine(oneDriveSyncFolder, "UltimateChartistSync.exe");
            if (File.Exists(oneDriveSyncExe))
            {
                var oneDriveFolder = new DirectoryInfo(Folders.PersonalFolder).Name;

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = $"{oneDriveFolder} {Folders.PersonalFolder}",
                    WorkingDirectory = oneDriveSyncFolder,
                    FileName = oneDriveSyncExe,
                };

                var p = Process.Start(startInfo);
                if (wait)
                    p.WaitForExit(60000);
            }
        }
    }
}