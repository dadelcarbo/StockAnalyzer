using Saxo.OpenAPI.AuthenticationServices;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.Bourso;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockPortfolio.AutoTrade;
using StockAnalyzer.StockWeb;
using StockAnalyzerApp.CustomControl;
using StockAnalyzerApp.CustomControl.AgendaDlg;
using StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog;
using StockAnalyzerApp.CustomControl.AutoTradeDlg;
using StockAnalyzerApp.CustomControl.DrawingDlg;
using StockAnalyzerApp.CustomControl.ExpectedValueDlg;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.CustomControl.HorseRaceDlgs;
using StockAnalyzerApp.CustomControl.IndicatorDlgs;
using StockAnalyzerApp.CustomControl.InstrumentDlgs;
using StockAnalyzerApp.CustomControl.MarketReplay;
using StockAnalyzerApp.CustomControl.PalmaresControl;
using StockAnalyzerApp.CustomControl.PortfolioDlg;
using StockAnalyzerApp.CustomControl.PortfolioDlg.SaxoPortfolioDlg;
using StockAnalyzerApp.CustomControl.SectorDlg;
using StockAnalyzerApp.CustomControl.SimulationDlgs;
using StockAnalyzerApp.CustomControl.SplitDlg;
using StockAnalyzerApp.CustomControl.TrendDlgs;
using StockAnalyzerApp.CustomControl.TweetDlg;
using StockAnalyzerApp.CustomControl.WatchlistDlgs;
using StockAnalyzerApp.StockScripting;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
using Telerik.Windows.Data;

namespace StockAnalyzerApp
{
    public partial class StockAnalyzerForm : Form
    {
        public delegate void SelectedStockSerieChangedEventHandler(object sender, SelectedStockSerieChangedEventArgs args);

        public delegate void SelectedStockChangedEventHandler(string stockName, bool activateMainWindow);
        public delegate void SelectedStockAndDurationChangedEventHandler(string stockName, BarDuration barDuration, bool activateMainWindow);
        public delegate void SelectedStockAndDurationAndThemeChangedEventHandler(string stockName, BarDuration barDuration, string theme, bool activateMainWindow);
        public delegate void SelectedStockAndDurationAndIndexChangedEventHandler(string stockName, int startIndex, int endIndex, BarDuration barDuration, bool activateMainWindow);

        public delegate void SelectedStockGroupChangedEventHandler(StockSerie.Groups stockgroup);

        public delegate void SelectedStrategyChangedEventHandler(string strategyName);

        public delegate void NotifySelectedThemeChangedEventHandler(Dictionary<string, List<string>> theme);

        public delegate void NotifyBarDurationChangedEventHandler(BarDuration barDuration);

        public delegate void NotifyStrategyChangedEventHandler(string newStrategy);

        public delegate void StockWatchListsChangedEventHandler();

        public delegate void AlertDetectedHandler();
        public event AlertDetectedHandler AlertDetected;

        public delegate void AlertDetectionStartedHandler(int nbStock, string alertTitle);
        public event AlertDetectionStartedHandler AlertDetectionStarted;

        public delegate void AlertDetectionProgressHandler(string StockName);
        public event AlertDetectionProgressHandler AlertDetectionProgress;

        public delegate void OnStockSerieChangedHandler(StockSerie newSerie, bool ignoreLinkedTheme);


        public static StockAnalyzerForm MainFrame { get; private set; }
        public MainFrameViewModel ViewModel { get; private set; }
        public bool IsClosing { get; set; }

        private const string PEAPerfTemplatePath = @"Resources\PEAPerformanceTemplate.html";
        private const string ReportTemplatePath = @"Resources\ReportTemplate.html";

        public static CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-GB");
        public static CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        public static CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public StockDictionary StockDictionary { get; private set; }

        public List<StockPortfolio> Portfolios => PortfolioDataProvider.Portfolios;

        public ToolStripProgressBar ProgressBar => this.progressBar;

        public GraphCloseControl GraphCloseControl => this.graphCloseControl;

        private StockSerie currentStockSerie = null;

        public StockSerie CurrentStockSerie
        {
            get { return currentStockSerie; }
            set
            {
                if (this.StockSerieChanged != null)
                {
                    this.StockSerieChanged(value, false);
                }
                else
                {
                    currentStockSerie = value;
                }
            }
        }

        public event OnStockSerieChangedHandler StockSerieChanged;


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

        private StockSerie.Groups selectedGroup;
        public StockSerie.Groups Group => selectedGroup;


        private static int NbBars { get; set; }

        private int startIndex = 0;
        private int endIndex = 0;

        private readonly List<GraphControl> graphList = new List<GraphControl>();

        #region CONSTANTS

        private static readonly string WORK_THEME = "__NewTheme*";

        private static readonly string EMPTY_THEME = "_Empty";

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

            this.ViewModel = new MainFrameViewModel();
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.IsClosing = false;

            // Add indicator1Name into the indicators controls layout panel
            int nbControl = 0;
            this.indicatorLayoutPanel.Controls.Add(this.graphScrollerControl, nbControl++, 0);
            this.indicatorLayoutPanel.Controls.Add(this.graphCloseControl, nbControl++, 0);
            this.indicatorLayoutPanel.Controls.Add(this.graphIndicator1Control, nbControl++, 0);
            this.indicatorLayoutPanel.Controls.Add(this.graphIndicator2Control, nbControl++, 0);
            this.indicatorLayoutPanel.Controls.Add(this.graphIndicator3Control, nbControl++, 0);
            this.indicatorLayoutPanel.Controls.Add(this.graphVolumeControl, nbControl++, 0);

            // Fill the control list
            this.graphList.Add(this.graphCloseControl);
            this.graphList.Add(this.graphScrollerControl);
            this.graphList.Add(this.graphIndicator1Control);
            this.graphList.Add(this.graphIndicator2Control);
            this.graphList.Add(this.graphIndicator3Control);
            this.graphList.Add(this.graphVolumeControl);

            foreach (GraphControl graphControl in this.graphList)
            {
                graphControl.DrawingPen = GraphCurveType.PenFromString(Settings.Default.DrawingPen);
            }

            this.graphCloseControl.HideIndicators = false;
            this.FormClosing += new FormClosingEventHandler(StockAnalyzerForm_FormClosing);

            this.StockDictionary = new StockDictionary(new DateTime(DateTime.Now.Year, 01, 01));
            this.StockDictionary.ReportProgress += new StockDictionary.ReportProgressHandler(StockDictionary_ReportProgress);

            NbBars = Settings.Default.DefaultBarNumber;

            Settings.Default.PropertyChanged += (sender, args) => Settings.Default.Save();

            previousState = this.WindowState;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BarDuration":
                    OnBarDurationChanged();
                    break;
            }
        }
        #region BAR DURATION MANAGEMENT

        private bool barDurationChangeFromUI = false;

        private void InitialiseBarDurationComboBox()
        {
            this.barDurationComboBox.Items.Clear();
            this.barDurationComboBox.Items.AddRange(StockBarDuration.BarDurationArray);
            this.barDurationComboBox.SelectedIndex = 0;
            this.ViewModel.BarDuration = BarDuration.Daily;
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
                if (this.currentStockSerie == null || !this.currentStockSerie.Initialise()) return;

                this.barDurationChangeFromUI = true;
                ViewModel.SetBarDuration((BarDuration)barDurationComboBox.SelectedItem, true);
                this.barDurationChangeFromUI = false;
            }
        }

        private void OnBarDurationChanged()
        {
            if (!barDurationChangeFromUI)
            {
                this.barDurationComboBox.SelectedItem = this.ViewModel.BarDuration;
            }
            if (this.currentStockSerie == null || !this.currentStockSerie.Initialise())
                return;
            if (this.CurrentStockSerie.BarDuration != this.ViewModel.BarDuration)
            {
                int previousBarCount = this.CurrentStockSerie.Count;
                this.CurrentStockSerie.BarDuration = this.ViewModel.BarDuration;

                if (previousBarCount != this.CurrentStockSerie.Count)
                {
                    NbBars = Settings.Default.DefaultBarNumber;
                }
                this.endIndex = this.CurrentStockSerie.Count - 1;
                this.startIndex = Math.Max(0, this.endIndex - NbBars);
                if (endIndex - startIndex < MIN_BAR_DISPLAY)
                {
                    this.DeactivateGraphControls("Not enough data to display...");
                    return;
                }
                if (!repaintSuspended)
                {
                    this.ApplyTheme();
                }

                if (NotifyBarDurationChanged != null) // @@@@ Need to remove this event ==> Subscribe directly to the view model.
                {
                    this.NotifyBarDurationChanged(this.ViewModel.BarDuration);
                }
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

        private void Form1_Load(object sender, EventArgs e)
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


            OneDriveSync();

            string folderName = Folders.DividendFolder;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            folderName = Folders.Palmares;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            folderName = Folders.Tweets;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                //Directory.Delete(folderName, true);
            }
            folderName = Folders.Saxo;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            StockSplashScreen.ProgressText = "Initialize stock dictionary...";
            StockSplashScreen.ProgressVal = 30;

            var download = Settings.Default.DownloadData && NetworkInterface.GetIsNetworkAvailable();
            StockDataProviderBase.InitStockDictionary(this.StockDictionary, download, new DownloadingStockEventHandler(Notifiy_SplashProgressChanged));

            //
            InitialiseThemeCombo();

            // Deserialize Drawing Items - Read Analysis files
            if (string.IsNullOrEmpty(this.ViewModel.AnalysisFile))
            {
                this.ViewModel.AnalysisFile = Path.Combine(Folders.PersonalFolder, "UltimateChartist.ulc");
                Settings.Default.Save();
            }
            else
            {
                StockSplashScreen.ProgressText = "Reading Drawing items ...";
                LoadAnalysis(this.ViewModel.AnalysisFile);
            }

            var cac40 = this.StockDictionary["CAC40"];
            cac40.Initialise();

            // Generate breadth 
            if (Settings.Default.GenerateBreadth)
            {

                foreach (StockSerie stockserie in this.StockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.Breadth))
                {
                    StockSplashScreen.ProgressText = "Generating breadth data " + stockserie.StockName;
                    stockserie.Initialise();
                    if (!BreadthDataProvider.NeedGenerate)
                        break;
                }
            }

            // Deserialize saved orders
            StockSplashScreen.ProgressText = "Reading portfolio data...";

            var portfolioDataProvider = PortfolioDataProvider.GetDataProvider(StockDataProvider.Portfolio);
            portfolioDataProvider.InitDictionary(StockDictionary, false);
            InitialisePortfolioCombo();
            Portfolio = PortfolioDataProvider.Portfolios.First();

            // Initialise dico
            StockSplashScreen.ProgressText = "Initialising menu items...";

            // Create Groups menu items
            CreateGroupMenuItem();

            CreateAgendaMenuItem();

            // Update dynamic menu
            InitialiseBarDurationComboBox();
            CreateSecondarySerieMenuItem();

            // Update dynamic menu
            InitDataProviderMenuItem();

            // Watchlist menu item
            this.LoadWatchList();

            // 
            InitialiseStockCombo(true);

            InitialiseWatchListComboBox();

            this.Show();
            this.progressBar.Value = 0;
            this.showShowStatusBarMenuItem.Checked = Settings.Default.ShowStatusBar;
            this.statusStrip1.Visible = Settings.Default.ShowStatusBar;
            this.showDrawingsMenuItem.Checked = Settings.Default.ShowDrawings;
            this.showEventMarqueeMenuItem.Checked = Settings.Default.ShowEventMarquee;
            this.showIndicatorDivMenuItem.Checked = Settings.Default.ShowIndicatorDiv;
            this.showIndicatorTextMenuItem.Checked = Settings.Default.ShowIndicatorText;
            this.showVariationBtn.CheckState = Settings.Default.ShowVariation ? CheckState.Checked : CheckState.Unchecked;

            this.StockSerieChanged += new OnStockSerieChangedHandler(StockAnalyzerForm_StockSerieChanged);
            this.ThemeChanged += new OnThemeChangedHandler(StockAnalyzerForm_ThemeChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(graphScrollerControl_ZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphCloseControl.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator2Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator3Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator1Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphVolumeControl.OnZoomChanged);
            StockSplashScreen.ProgressText = "Loading " + this.CurrentStockSerie.StockName + " data...";

            SetDurationForStockGroup(this.CurrentStockSerie.StockGroup);
            this.StockAnalyzerForm_StockSerieChanged(this.CurrentStockSerie, false);

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

            if (Settings.Default.GenerateDailyReport)
            {
                var folder = Folders.Report;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                // Daily report
                var fileName = Path.Combine(Folders.Report, "LastGeneration.txt");
                DateTime reportDate = DateTime.MinValue;
                if (File.Exists(fileName))
                {
                    reportDate = DateTime.Parse(File.ReadAllText(fileName), CultureInfo.InvariantCulture);
                }
                cac40.BarDuration = BarDuration.Daily;
                if (reportDate < cac40.LastValue.DATE)
                {
                    showAlertDefDialogMenuItem_Click(this, null);
                    stockAlertManagerViewModel.RunFullAlert();
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

            searchCombo.Items.AddRange(this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded).Select(p => p.Key).ToArray());

            // Ready to start
            StockSplashScreen.CloseForm(true);
            this.Focus();
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
                    this.searchCombo.Items.AddRange(this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded).Select(p => p.Key).ToArray());
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
                    match = this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded && (p.Key.ToUpper().Contains(name) || p.Value.ISIN == name)).Select(p => p.Key).ToArray();
                else if (name.Length <= 3)
                    match = this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded && (p.Key.ToUpper().Contains(name) || (p.Value.Symbol != null && p.Value.Symbol.Contains(name)))).Select(p => p.Key).ToArray();
                else
                    match = this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded && p.Key.ToUpper().Contains(name)).Select(p => p.Key).ToArray();

                if (match.Length == 1)
                {
                    Debug.WriteLine("Cond3");
                    searchCombo.Text = name;
                    this.searchCombo.SelectionStart = this.searchCombo.Text.Length;
                    this.SetCurrentStock(match.First().ToUpper());
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

        private readonly bool showTimerDebug = false;

        private void goToStock(object sender, EventArgs e)
        {
            if (searchCombo.SelectedItem == null)
                return;

            var stockName = searchCombo.SelectedItem.ToString().ToUpper();
            SetCurrentStock(stockName);
        }

        private void SetCurrentStock(string stockName)
        {
            if (stockName == this.currentStockSerie.StockName.ToUpper()) return;

            var serie = this.StockDictionary.Values.FirstOrDefault(s => s.StockName.ToUpper() == stockName);

            if (serie == null) return;

            // Update Group
            if (this.selectedGroup != serie.StockGroup)
            {
                this.selectedGroup = serie.StockGroup;
                repaintSuspended = true;
                SetDurationForStockGroup(serie.StockGroup);
                repaintSuspended = false;

                foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                {
                    groupSubMenuItem.Checked = groupSubMenuItem.Text == selectedGroup.ToString();
                }

                InitialiseStockCombo(false);
            }

            // Update Stock
            if (this.currentStockSerie != serie)
            {
                //StockAnalyzerForm_StockSerieChanged(serie, false);
                this.stockNameComboBox.SelectedItem = serie.StockName;
            }
        }

        private void InitialiseWatchListComboBox()
        {
            if (this.WatchLists != null)
            {
                // 
                ToolStripItem[] watchListMenuItems = new ToolStripItem[this.WatchLists.Count()];
                ToolStripItem[] addToWatchListMenuItems = new ToolStripItem[this.WatchLists.Count()];
                ToolStripMenuItem addToWatchListSubMenuItem;

                int i = 0;
                foreach (StockWatchList watchList in WatchLists)
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

            LoginService.RefreshSessions();

            if (refreshing)
                return;
            refreshing = true;
            return; // §§§§

            using (new MethodLogger(this, showTimerDebug))
            {
                using (new StockSerieLocker(this.currentStockSerie))
                {

                    // Download INTRADAY current serie
                    try
                    {
                        if (Settings.Default.SupportIntraday)
                        {
                            (StockDataProviderBase.GetDataProvider(StockDataProvider.ABC) as ABCDataProvider).DownloadAllGroupsIntraday();
                        }
                        if (this.currentStockSerie != null)
                        {
                            var lastValue = this.CurrentStockSerie.LastValue;
                            if (StockDataProviderBase.DownloadSerieData(this.currentStockSerie))
                            {
                                if (this.currentStockSerie.Initialise())
                                {
                                    if (lastValue != this.currentStockSerie.LastValue)
                                    {
                                        this.BeginInvoke(new Action(() => this.ApplyTheme()));
                                    }
                                }
                                else
                                {
                                    this.DeactivateGraphControls("Unable to download selected stock data...");
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        StockLog.Write(exception);

                        StockAnalyzerException.MessageBox(exception);
                    }
                    finally
                    {
                        refreshing = false;
                    }
                }
            }
        }

        public void GenerateIntradayReport(List<BarDuration> barDurations)
        {
            Action action = () => GenerateIntradayReportDispatch(barDurations);
            this.Invoke(action);
        }

        bool IsReportingIntraday = false;
        public void GenerateIntradayReportDispatch(List<BarDuration> barDurations)
        {
            using var ml = new MethodLogger(this);

            lock (ml)
            {
                if (IsReportingIntraday)
                    return;
            }

            try
            {
                IsReportingIntraday = true;

                var alertDefs = StockAlertDef.AlertDefs.Where(a => a.InReport && a.InAlert && barDurations.Contains(a.BarDuration)).ToList();
                if (alertDefs.Count == 0)
                    return;
                var sw = Stopwatch.StartNew();
                var groups = alertDefs.Select(a => a.Group).Distinct();

                var turboList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.StockGroup == StockSerie.Groups.TURBO);
                var downloadTasks = turboList.Select(s => Task.Run(() => StockDataProviderBase.DownloadSerieData(s)));

                if (alertDefs.Any(a => a.Group != StockSerie.Groups.TURBO))
                {
                    var peaIntradayList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.Intraday == true && s.BelongsToGroup(StockSerie.Groups.PEA));
                    var dp = StockDataProviderBase.GetDataProvider(StockDataProvider.BoursoIntraday);
                    downloadTasks = downloadTasks.Union(peaIntradayList.Select(s => Task.Run(() => dp.DownloadDailyData(s))));
                }

                Task.WaitAll(downloadTasks.ToArray());

                sw.Stop();

                foreach (var duration in barDurations)
                {
                    GenerateReport(duration);
                }
            }
            finally { IsReportingIntraday = false; }
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
                if (this.CurrentStockSerie == null || this.CurrentStockSerie.Count == 0 ||
                    this.CurrentStockSerie.IsInitialised == false)
                {
                    startIndex = 0;
                    endIndex = 0;
                }
                else
                {
                    int nbBars = NbBars;
                    if (CurrentStockSerie.Count > 1 && CurrentStockSerie.Count - 1 - nbBars < 0) // Previous serie was longer
                    {
                        nbBars = CurrentStockSerie.Count - 1;
                    }
                    ChangeZoom(Math.Max(0, CurrentStockSerie.Count - 1 - nbBars), CurrentStockSerie.Count - 1);
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

        private void divScaleBtn_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

            if (this.currentStockSerie.StockName.EndsWith("_DIV"))
            {
                this.OnSelectedStockChanged(this.currentStockSerie.StockName.Replace("_DIV", ""), false);
                return;
            }
            if (StockDictionary.ContainsKey(this.currentStockSerie.StockName + "_DIV"))
            {
                this.OnSelectedStockChanged(this.currentStockSerie.StockName + "_DIV", false);
                return;
            }
            if (!this.CurrentStockSerie.Dividend.DownloadFromYahoo(this.CurrentStockSerie, true) || this.CurrentStockSerie.Dividend.Entries.Count == 0)
            {
                return;
            }

            StockSerie newSerie = this.CurrentStockSerie.GenerateDivStockSerie();
            AddNewSerie(newSerie);
        }
        #endregion

        public void OnSelectedStockChanged(string stockName, bool activate)
        {
            using (new MethodLogger(this))
            {
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    if (this.StockDictionary.ContainsKey(stockName))
                    {
                        var stockSerie = this.StockDictionary[stockName];

                        StockSerie.Groups newGroup = stockSerie.StockGroup;
                        if (this.selectedGroup != newGroup)
                        {
                            this.selectedGroup = newGroup;


                            foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                            {
                                groupSubMenuItem.Checked = groupSubMenuItem.Text == selectedGroup.ToString();
                            }

                            InitialiseStockCombo(false);
                            SetDurationForStockGroup(newGroup);
                        }
                    }
                    else
                    {
                        this.stockNameComboBox.Items.Add(stockName);
                    }
                }
                this.repaintSuspended = true;
                this.stockNameComboBox.SelectedIndexChanged -= StockNameComboBox_SelectedIndexChanged;
                this.stockNameComboBox.Text = stockName;
                this.stockNameComboBox.SelectedIndexChanged += new EventHandler(StockNameComboBox_SelectedIndexChanged);
                this.repaintSuspended = false;

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);

                if (activate)
                {
                    this.Activate();
                }
            }
        }
        public void OnSelectedStockAndDurationAndThemeChanged(string stockName, BarDuration barDuration, string theme, bool activate)
        {
            using (new MethodLogger(this))
            {
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    if (this.StockDictionary.ContainsKey(stockName))
                    {
                        var stockSerie = this.StockDictionary[stockName];

                        StockSerie.Groups newGroup = stockSerie.StockGroup;
                        if (!stockSerie.BelongsToGroup(this.selectedGroup))
                        {
                            this.selectedGroup = newGroup;

                            foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                            {
                                groupSubMenuItem.Checked = groupSubMenuItem.Text == selectedGroup.ToString();
                            }

                            InitialiseStockCombo(false);
                        }
                    }
                    else
                    {
                        this.stockNameComboBox.Items.Add(stockName);
                    }
                }
                this.stockNameComboBox.SelectedIndexChanged -= StockNameComboBox_SelectedIndexChanged;
                this.stockNameComboBox.Text = stockName;
                this.stockNameComboBox.SelectedIndexChanged += new EventHandler(StockNameComboBox_SelectedIndexChanged);

                this.repaintSuspended = true;
                this.ViewModel.BarDuration = barDuration;
                this.repaintSuspended = false;

                if (!string.IsNullOrEmpty(theme))
                {
                    this.themeComboBox.SelectedIndexChanged -= themeComboBox_SelectedIndexChanged;
                    this.currentTheme = theme;
                    this.themeComboBox.SelectedItem = theme;
                    this.themeComboBox.SelectedIndexChanged += themeComboBox_SelectedIndexChanged;
                }
                else
                {
                    this.themeComboBox.SelectedIndexChanged -= themeComboBox_SelectedIndexChanged;
                    this.currentTheme = EMPTY_THEME;
                    this.themeComboBox.SelectedItem = EMPTY_THEME;
                    this.themeComboBox.SelectedIndexChanged += themeComboBox_SelectedIndexChanged;
                }

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);

                if (activate)
                {
                    this.Activate();
                }
            }
        }
        public void OnSelectedStockAndDurationChanged(string stockName, BarDuration barDuration, bool activate)
        {
            using (new MethodLogger(this))
            {
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    if (this.StockDictionary.ContainsKey(stockName))
                    {
                        var stockSerie = this.StockDictionary[stockName];

                        StockSerie.Groups newGroup = stockSerie.StockGroup;
                        if (!stockSerie.BelongsToGroup(this.selectedGroup))
                        {
                            this.selectedGroup = newGroup;

                            foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                            {
                                groupSubMenuItem.Checked = groupSubMenuItem.Text == selectedGroup.ToString();
                            }

                            InitialiseStockCombo(false);
                        }
                    }
                    else
                    {
                        this.stockNameComboBox.Items.Add(stockName);
                    }
                }
                this.stockNameComboBox.SelectedIndexChanged -= StockNameComboBox_SelectedIndexChanged;
                this.stockNameComboBox.Text = stockName;
                this.stockNameComboBox.SelectedIndexChanged += new EventHandler(StockNameComboBox_SelectedIndexChanged);

                this.ViewModel.BarDuration = barDuration;

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        public void OnSelectedStockAndDurationAndIndexChanged(string stockName, int startIndex, int endIndex, BarDuration barDuration, bool activate)
        {
            using (new MethodLogger(this))
            {
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    if (this.StockDictionary.ContainsKey(stockName))
                    {
                        var stockSerie = this.StockDictionary[stockName];

                        StockSerie.Groups newGroup = stockSerie.StockGroup;
                        if (this.selectedGroup != newGroup)
                        {
                            this.selectedGroup = newGroup;

                            foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                            {
                                groupSubMenuItem.Checked = groupSubMenuItem.Text == selectedGroup.ToString();
                            }

                            InitialiseStockCombo(true);
                        }
                    }
                    else
                    {
                        this.stockNameComboBox.Items.Add(stockName);
                    }
                }

                this.repaintSuspended = true;
                this.ViewModel.BarDuration = barDuration;
                this.repaintSuspended = false;

                this.stockNameComboBox.SelectedIndexChanged -= StockNameComboBox_SelectedIndexChanged;
                this.stockNameComboBox.Text = stockName;
                this.stockNameComboBox.SelectedIndexChanged += new EventHandler(StockNameComboBox_SelectedIndexChanged);

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);
                this.ChangeZoom(startIndex, endIndex);

                if (activate)
                {
                    this.Activate();
                }
            }
        }
        private void StockAnalyzerForm_StockSerieChanged(StockSerie newSerie, bool ignoreLinkedTheme)
        {
            using (new MethodLogger(this))
            {
                //
                if (newSerie == null)
                {
                    DeactivateGraphControls("No data to display");
                    this.Text = "Ultimate Chartist - " + "No stock selected";
                    return;
                }
                this.currentStockSerie = newSerie;

                if (BoursoIntradayDataProvider.ContainsSerie(newSerie))
                {
                    intradayButton.CheckState = CheckState.Checked;
                }
                else
                {
                    intradayButton.CheckState = CheckState.Unchecked;
                }

                #region Set Window Title
                string id;
                if (CurrentStockSerie.Symbol == CurrentStockSerie.StockName)
                {
                    id = CurrentStockSerie.StockGroup + "-" + CurrentStockSerie.Symbol;
                }
                else
                {
                    id = CurrentStockSerie.StockGroup + "-" + CurrentStockSerie.Symbol + " - " + CurrentStockSerie.StockName;
                }
                if (!string.IsNullOrWhiteSpace(this.CurrentStockSerie.ISIN))
                {
                    id += " - " + this.CurrentStockSerie.ISIN;
                }
                id += " - " + this.CurrentStockSerie.DataProvider;
                this.Text = "Ultimate Chartist - " + this.ViewModel.AnalysisFile.Split('\\').Last() + " - " + id;
                #endregion

                if (!this.IsReportingIntraday && (currentStockSerie.BelongsToGroup(StockSerie.Groups.TURBO) || currentStockSerie.BelongsToGroup(StockSerie.Groups.TURBO_5M)))
                {
                    this.statusLabel.Text = ("Downloading data...");
                    this.Refresh();
                    this.Cursor = Cursors.WaitCursor;
                    StockDataProviderBase.DownloadSerieData(currentStockSerie);
                }
                if (!currentStockSerie.IsInitialised)
                {
                    this.statusLabel.Text = ("Loading data...");
                    this.Cursor = Cursors.WaitCursor;
                }
                if (!currentStockSerie.Initialise() || currentStockSerie.Count == 0)
                {
                    DeactivateGraphControls("No data to display");
                    return;
                }
                this.currentStockSerie.BarDuration = this.ViewModel.BarDuration;
                if (currentStockSerie.Count < MIN_BAR_DISPLAY)
                {
                    DeactivateGraphControls("Not enough data to display");
                    return;
                }
                if (!ignoreLinkedTheme
                    && currentStockSerie.StockAnalysis != null
                    && !string.IsNullOrEmpty(currentStockSerie.StockAnalysis.Theme)
                    && this.themeComboBox.SelectedText != currentStockSerie.StockAnalysis.Theme
                    && this.themeComboBox.Items.Contains(currentStockSerie.StockAnalysis.Theme))
                {
                    if (this.themeComboBox.SelectedItem.ToString() == currentStockSerie.StockAnalysis.Theme)
                    {
                        ApplyTheme();
                    }
                    else
                    {
                        this.themeComboBox.SelectedItem = currentStockSerie.StockAnalysis.Theme;
                    }
                }
                else
                {
                    ApplyTheme();
                }

                // Set the default theme checkstate
                if (this.currentStockSerie.StockAnalysis != null && this.currentStockSerie.StockAnalysis.Theme == currentTheme)
                {
                    this.defaultThemeStripButton.CheckState = CheckState.Checked;
                }
                else
                {
                    this.defaultThemeStripButton.CheckState = CheckState.Unchecked;
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
            OneDriveSync();
        }

        public void OnSerieEventProcessed()
        {
            this.progressBar.Value++;
        }

        public List<StockWatchList> WatchLists { get; set; }

        private void LoadWatchList()
        {
            string watchListsFileName = Path.Combine(Folders.PersonalFolder, "WatchLists.xml");

            // Parse watch lists
            if (File.Exists(watchListsFileName))
            {
                using FileStream fs = new FileStream(watchListsFileName, FileMode.Open);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(List<StockWatchList>));
                this.WatchLists = (List<StockWatchList>)serializer.Deserialize(xmlReader);
                this.WatchLists = this.WatchLists.OrderBy(wl => wl.Name).ToList();

                // Cleanup missing stocks
                foreach (var watchList in this.WatchLists)
                {
                    watchList.StockList.RemoveAll(s => !StockDictionary.ContainsKey(s));
                }
            }
            else
            {
                this.WatchLists = new List<StockWatchList>();
            }
            if (this.WatchLists.Count == 0)
            {
                // Create new empty watchlist
                this.WatchLists.Add(new StockWatchList("Empty"));
            }
        }

        private void LoadAnalysis(string analysisFileName)
        {
            // Clear existing analysis
            foreach (StockSerie stockSerie in this.StockDictionary.Values)
            {
                stockSerie.StockAnalysis.Clear();
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
                foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.StockAnalysis.Theme != string.Empty))
                {
                    if (!this.themeComboBox.Items.Contains(stockSerie.StockAnalysis.Theme))
                    {
                        stockSerie.StockAnalysis.Theme = string.Empty;
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

        private void InitialiseStockCombo(bool setCurrentStock)
        {
            // Initialise Combo values
            stockNameComboBox.Items.Clear();
            stockNameComboBox.SelectedItem = string.Empty;

            var stocks = StockDictionary.Values.Where(s => s.BelongsToGroup(this.selectedGroup)).Select(s => s.StockName);
            foreach (string stockName in stocks)
            {
                if (StockDictionary.Keys.Contains(stockName))
                {
                    StockSerie stockSerie = StockDictionary[stockName];
                    stockNameComboBox.Items.Add(stockName);
                }
            }
            // 
            if (setCurrentStock && stockNameComboBox.Items.Count != 0)
            {
                stockNameComboBox.SelectedIndex = 0;
                if (!string.IsNullOrEmpty(stockNameComboBox.Items[0].ToString()))
                {
                    this.CurrentStockSerie = this.StockDictionary[stockNameComboBox.SelectedItem.ToString()];
                }
            }
        }

        public void OnNeedReinitialise(bool resetDrawingButtons)
        {
            using (new MethodLogger(this))
            {
                if (this.currentStockSerie == null) return;
                if (resetDrawingButtons)
                {
                    ResetDrawingButtons();
                }
                if (stockNameComboBox.SelectedItem != null && !string.IsNullOrEmpty(stockNameComboBox.SelectedItem.ToString()))
                {
                    // Set the new selected serie
                    if (CurrentStockSerie == null || CurrentStockSerie.StockName != stockNameComboBox.SelectedItem.ToString())
                    {
                        CurrentStockSerie = StockDictionary[stockNameComboBox.SelectedItem.ToString()];
                    }

                    if (endIndex == 0 || endIndex > (CurrentStockSerie.Count - 1))
                    {
                        this.ResetZoom();
                    }

                    // Refresh all components
                    RefreshGraph();
                }
            }
        }

        #region STOCK and PORTFOLIO selection tool bar

        private void StockNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.searchCombo.Text) && !stockNameComboBox.SelectedItem.ToString().ToUpper().Contains(this.searchCombo.Text.ToUpper()))
                this.searchCombo.Text = "";

            StockSerie selectedSerie = null;
            if (this.StockDictionary.ContainsKey(stockNameComboBox.SelectedItem.ToString()))
            {
                selectedSerie = StockDictionary[stockNameComboBox.SelectedItem.ToString()];
            }
            else
            {
                throw new ApplicationException("Data for " + stockNameComboBox.SelectedItem.ToString() + "does not exist");
            }
            // Set the new selected serie
            CurrentStockSerie = selectedSerie;
        }

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
                if (this.currentStockSerie != null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (showSplash)
                    {
                        StockSplashScreen.FadeInOutSpeed = 0.25;
                        StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + this.currentStockSerie.StockName;
                        StockSplashScreen.ProgressVal = 0;
                        StockSplashScreen.ProgressMax = 100;
                        StockSplashScreen.ProgressMin = 0;
                        StockSplashScreen.ShowSplashScreen();
                    }

                    if (StockDataProviderBase.ForceDownloadSerieData(this.currentStockSerie))
                    {
                        if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
                        {
                            try
                            {
                                ABCDataProvider.DownloadAgenda(this.currentStockSerie);
                            }
                            catch (Exception ex)
                            {
                                StockLog.Write(ex);
                            }
                        }

                        if (this.currentStockSerie.Initialise())
                        {
                            this.ApplyTheme();
                        }
                        else
                        {
                            this.DeactivateGraphControls("Unable to download selected stock data...");
                        }
                    }

                    if (showSplash)
                    {
                        StockSplashScreen.CloseForm(true);
                    }
                    this.Cursor = Cursors.Arrow;
                }
            }
        }
        private void ForceDownloadStockGroup()
        {
            if (this.currentStockSerie != null)
            {
                try
                {
                    if (MessageBox.Show($"Are you sure you want to force downloading the full group {this.selectedGroup} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                    StockSplashScreen.FadeInOutSpeed = 0.25;
                    StockSplashScreen.ProgressText = "Downloading " + this.selectedGroup + " - " + this.currentStockSerie.StockName;

                    var stockSeries =
                       this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.selectedGroup));

                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = stockSeries.Count();
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();

                    foreach (var stockSerie in stockSeries)
                    {
                        StockSplashScreen.ProgressText = "Downloading " + this.selectedGroup + " - " + stockSerie.StockName;
                        StockDataProviderBase.ForceDownloadSerieData(stockSerie);

                        //try
                        //{
                        //StockSplashScreen.ProgressText = "Downloading Dividend " + selectedGroup + " - " + stockSerie.StockName;
                        //this.CurrentStockSerie.Dividend.DownloadFromYahoo(stockSerie, true);
                        //}
                        //catch (Exception ex)
                        //{
                        //    StockLog.Write(ex);
                        //}
                        StockSplashScreen.ProgressVal++;
                    }

                    this.SaveAnalysis(this.ViewModel.AnalysisFile);

                    if (this.currentStockSerie.Initialise())
                    {
                        this.ApplyTheme();
                    }
                    else
                    {
                        this.DeactivateGraphControls("Unable to download selected stock data...");
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }

                StockSplashScreen.CloseForm(true);
            }
        }
        private void DownloadStock(bool showSplash)
        {
            using (new MethodLogger(this))
            {
                if (this.currentStockSerie != null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (showSplash)
                    {
                        StockSplashScreen.FadeInOutSpeed = 0.25;
                        StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + this.currentStockSerie.StockName;
                        StockSplashScreen.ProgressVal = 0;
                        StockSplashScreen.ProgressMax = 100;
                        StockSplashScreen.ProgressMin = 0;
                        StockSplashScreen.ShowSplashScreen();
                    }

                    using (new StockSerieLocker(currentStockSerie))
                    {
                        if (StockDataProviderBase.DownloadSerieData(this.currentStockSerie))
                        {
                            this.CurrentStockSerie.Dividend.DownloadFromYahoo(this.CurrentStockSerie);
                            if (this.currentStockSerie.Initialise())
                            {
                                this.ApplyTheme();
                            }
                            else
                            {
                                this.DeactivateGraphControls("Unable to download selected stock data...");
                            }
                        }
                    }

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
            if (this.currentStockSerie != null)
            {
                try
                {
                    StockSplashScreen.FadeInOutSpeed = 0.25;
                    StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " +
                                                     this.currentStockSerie.StockName;

                    var stockSeries =
                       this.StockDictionary.Values.Where(
                          s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.selectedGroup));

                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = stockSeries.Count();
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();

                    foreach (var stockSerie in stockSeries)
                    {
                        StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + stockSerie.StockName;
                        using (new StockSerieLocker(stockSerie))
                        {
                            StockDataProviderBase.DownloadSerieData(stockSerie);
                        }
                        try
                        {
                            StockSplashScreen.ProgressText = "Downloading Dividend " + stockSerie.StockGroup + " - " + stockSerie.StockName;
                            this.CurrentStockSerie.Dividend.DownloadFromYahoo(stockSerie);
                        }
                        catch (Exception ex)
                        {
                            StockLog.Write(ex);
                        }

                        StockSplashScreen.ProgressVal++;
                    }

                    this.SaveAnalysis(this.ViewModel.AnalysisFile);

                    if (this.currentStockSerie.Initialise())
                    {
                        this.ApplyTheme();
                    }
                    else
                    {
                        this.DeactivateGraphControls("Unable to download selected stock data...");
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }

                StockSplashScreen.CloseForm(true);
            }
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
            StockWatchList watchList = this.WatchLists.Find(wl => wl.Name == sender.ToString());
            if (!watchList.StockList.Contains(this.stockNameComboBox.SelectedItem.ToString()))
            {
                watchList.StockList.Add(this.stockNameComboBox.SelectedItem.ToString());
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
                foreach (GraphControl graphControl in this.graphList)
                {
                    graphControl.DrawingPen = drawingStyleForm.Pen;
                }
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
            if (this.currentStockSerie == null) return;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.SaveAnalysis(this.ViewModel.AnalysisFile);

            this.Cursor = currentCursor;

        }

        private void SaveWatchList()
        {
            // Sort all the watchlists
            if (this.WatchLists != null)
            {
                foreach (StockWatchList watchList in this.WatchLists)
                {
                    watchList.StockList.RemoveAll(s => !StockDictionary.ContainsKey(s));
                    watchList.StockList.Sort();
                }

                // Save watch list file
                string watchListsFileName = Path.Combine(Folders.PersonalFolder, "WatchLists.xml");
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                using FileStream fs = new FileStream(watchListsFileName, FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(List<StockWatchList>));
                XmlTextWriter xmlWriter = new XmlTextWriter(fs, null);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                xmlWriter.WriteStartDocument();
                serializer.Serialize(xmlWriter, this.WatchLists);
                xmlWriter.WriteEndDocument();
            }
        }

        public void SaveAnalysis(string analysisFileName)
        {
            if (this.currentStockSerie == null) return;
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
                using (Brush bb = new SolidBrush(this.graphCloseControl.BackgroundColor))
                {
                    g.FillRectangle(bb, g.VisibleClipBounds);
                }

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

        public string GetStockSnapshotAsHtml(StockSerie stockSerie, string theme, int nbBars = 0)
        {
            this.CurrentStockSerie = stockSerie;
            if (!string.IsNullOrEmpty(theme) && this.themeComboBox.Items.Contains(theme))
            {
                this.CurrentTheme = theme;
            }
            else
            {
                this.CurrentTheme = EMPTY_THEME;
            }
            if (nbBars > 0)
            {
                this.ChangeZoom(stockSerie.LastIndex - nbBars, stockSerie.LastIndex);
            }

            return SnapshotAsHtml();
        }

        private string SnapshotAsHtml()
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
            if (bitmaps.Count > 0)
            {
                Bitmap snapshot = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(snapshot);
                using (Brush bb = new SolidBrush(this.graphCloseControl.BackgroundColor))
                {
                    g.FillRectangle(bb, g.VisibleClipBounds);
                }

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
            var dp = StockDataProviderBase.GetDataProvider(CurrentStockSerie.DataProvider);
            var handled = dp.RemoveEntry(CurrentStockSerie);
            // Flag as excluded
            CurrentStockSerie.StockAnalysis.Excluded = true;
            if (!handled)
            {
                SaveAnalysis(this.ViewModel.AnalysisFile);
            }

            // Remove from current combo list.
            int selectedIndex = this.stockNameComboBox.SelectedIndex;
            if (selectedIndex != -1)
            {
                this.stockNameComboBox.Items.RemoveAt(selectedIndex);
                if (selectedIndex < this.stockNameComboBox.Items.Count - 1)
                {
                    this.stockNameComboBox.SelectedIndex = selectedIndex;
                }
                else
                {
                    this.stockNameComboBox.SelectedIndex = this.stockNameComboBox.Items.Count - 1;
                }
            }
        }


        private void intradayButton_Click(object sender, EventArgs e)
        {
            if (this.intradayButton.CheckState == CheckState.Checked)
            {
                BoursoIntradayDataProvider.RemoveSerie(this.CurrentStockSerie);
                this.intradayButton.CheckState = CheckState.Unchecked;
            }
            else
            {
                BoursoIntradayDataProvider.AddSerie(this.CurrentStockSerie);
                this.intradayButton.CheckState = CheckState.Checked;
            }
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
            if (this.CurrentStockSerie == null)
                return;
            if (!string.IsNullOrEmpty(this.currentStockSerie.ISIN))
            {
                Clipboard.SetText(this.currentStockSerie.ISIN);
                return;
            }
            if (!string.IsNullOrEmpty(this.currentStockSerie.StockName))
            {
                Clipboard.SetText(this.currentStockSerie.StockName);
                return;
            }
        }

        #endregion

        #region REWIND/FAST FORWARD METHODS

        private void Rewind(int step)
        {
            if (this.currentStockSerie == null) return;
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
            if (this.currentStockSerie == null) return;
            int max = CurrentStockSerie.Count - 1;
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

        private void CreateGroupMenuItem()
        {
            if (!Enum.TryParse(Settings.Default.SelectedGroup, out this.selectedGroup))
            {
                this.selectedGroup = StockSerie.Groups.INDICES;
                Settings.Default.SelectedGroup = StockSerie.Groups.INDICES.ToString();
                Settings.Default.Save();
            }

            // Clean existing menus
            this.stockFilterMenuItem.DropDownItems.Clear();

            List<ToolStripItem> groupMenuItems = new List<ToolStripItem>();
            ToolStripMenuItem groupSubMenuItem;

            var validGroups = this.StockDictionary.GetValidGroups().Select(g => g.ToString());
            bool selectedGroupFound = false;
            foreach (string groupName in validGroups)
            {
                // Create group menu items
                groupSubMenuItem = new ToolStripMenuItem(groupName);
                groupSubMenuItem.Click += new EventHandler(groupSubMenuItem_Click);
                if (groupName == this.selectedGroup.ToString())
                {
                    groupSubMenuItem.Checked = true;
                    selectedGroupFound = true;
                }
                else
                {
                    groupSubMenuItem.Checked = false;
                }
                groupMenuItems.Add(groupSubMenuItem);
            }
            if (!selectedGroupFound)
            {
                // Set default group
                ((ToolStripMenuItem)groupMenuItems[0]).Checked = true;
                this.selectedGroup =
                   (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), groupMenuItems[0].ToString());
                Settings.Default.SelectedGroup = this.selectedGroup.ToString();
                Settings.Default.Save();
            }

            this.stockFilterMenuItem.DropDownItems.AddRange(groupMenuItems.ToArray());
        }

        private void groupSubMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.SelectedGroup = sender.ToString();
            Settings.Default.Save();

            this.OnSelectedStockGroupChanged((StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), sender.ToString()));
        }

        #region MENU CREATION
        private void CreateSecondarySerieMenuItem()
        {
            // Clean existing menus
            this.secondarySerieMenuItem.DropDownItems.Clear();
            var validGroups = this.StockDictionary.GetValidGroups().Select(g => g.ToString());
            ToolStripMenuItem[] groupMenuItems = new ToolStripMenuItem[validGroups.Count()];

            int i = 0;
            foreach (string group in validGroups)
            {
                groupMenuItems[i] = new ToolStripMenuItem(group);

                // 
                var groupSeries = StockDictionary.Values.Where(s => s.StockGroup.ToString() == group && !s.StockAnalysis.Excluded);
                if (groupSeries.Count() != 0)
                {
                    ToolStripMenuItem[] secondarySerieMenuItems = new ToolStripMenuItem[groupSeries.Count()];
                    ToolStripMenuItem secondarySerieSubMenuItem;

                    int n = 0;
                    foreach (StockSerie stockSerie in groupSeries)
                    {
                        secondarySerieSubMenuItem = new ToolStripMenuItem(stockSerie.StockName);
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


        #region generate new series

        private void AddNewSerie(StockSerie newSerie)
        {
            if (StockDictionary.ContainsKey(newSerie.StockName))
            {
                StockDictionary.Remove(newSerie.StockName);
            }
            StockDictionary.Add(newSerie.StockName, newSerie);

            if (!stockNameComboBox.Items.Contains(newSerie.StockName))
            {
                stockNameComboBox.Items.Insert(stockNameComboBox.Items.IndexOf(this.CurrentStockSerie.StockName) + 1,
                   newSerie.StockName);
            }
            stockNameComboBox.SelectedIndex = stockNameComboBox.Items.IndexOf(newSerie.StockName);

            this.StockAnalyzerForm_StockSerieChanged(newSerie, false);
        }

        private void logSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            StockSerie newSerie = this.CurrentStockSerie.GenerateLogStockSerie();
            AddNewSerie(newSerie);
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
                palmaresDlg.palmaresControl1.ViewModel.Group = this.Group;

                palmaresDlg.FormClosing += new FormClosingEventHandler(palmaresDlg_FormClosing);
                palmaresDlg.palmaresControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                palmaresDlg.palmaresControl1.SelectedStockAndThemeChanged += OnSelectedStockAndDurationAndThemeChanged;
                palmaresDlg.Show();
            }
            else
            {
                palmaresDlg.Activate();
            }
        }
        private void palmaresDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            palmaresDlg.palmaresControl1.SelectedStockChanged -= OnSelectedStockAndDurationChanged;
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

                instrumentsDlg.instrumentsControl1.ViewModel.Group = this.Group;

                instrumentsDlg.FormClosing += new FormClosingEventHandler(instrumentsDlg_FormClosing);
                instrumentsDlg.instrumentsControl1.SelectedStockChanged += OnSelectedStockChanged;
                instrumentsDlg.Show();
            }
            else
            {
                instrumentsDlg.Activate();
            }
        }
        private void instrumentsDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            instrumentsDlg.instrumentsControl1.SelectedStockChanged -= OnSelectedStockChanged;
            this.instrumentsDlg = null;
        }
        #endregion

        public bool changingGroup = false;
        private void OnSelectedStockGroupChanged(StockSerie.Groups stockGroup)
        {
            try
            {
                changingGroup = true;
                StockSerie.Groups newGroup = stockGroup;
                if (this.selectedGroup != newGroup)
                {
                    this.selectedGroup = newGroup;

                    foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                    {
                        groupSubMenuItem.Checked = groupSubMenuItem.Text == stockGroup.ToString();
                        break;
                    }
                    InitialiseStockCombo(true);
                    SetDurationForStockGroup(newGroup);

                    changingGroup = false;
                    ApplyTheme();
                }
            }
            finally
            {
                changingGroup = false;
            }
        }

        private void SetDurationForStockGroup(StockSerie.Groups newGroup)
        {
            // In order to speed the intraday display.
            switch (newGroup)
            {
                case StockSerie.Groups.TURBO_5M:
                case StockSerie.Groups.TURBO:
                    if (this.logScaleBtn.CheckState == CheckState.Checked)
                    {
                        this.logScaleBtn_Click(null, null);
                    }
                    this.ViewModel.BarDuration = BarDuration.H_1;
                    break;
                default:
                    if (this.ViewModel.BarDuration > BarDuration.Monthly)
                    {
                        this.ViewModel.BarDuration = BarDuration.Daily;
                    }
                    break;
            }
        }

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

        private void showCommentMarqueeMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowCommentMarquee = this.showCommentMarqueeMenuItem.Checked;
            Settings.Default.Save();
            // Refresh the graphs
            OnNeedReinitialise(false);
        }
        private void showDividendMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowDividend = this.showDividendMenuItem.Checked;
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
                autoTradeDlg.autoTradeControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                autoTradeDlg.autoTradeControl1.SelectedStockAndThemeChanged += OnSelectedStockAndDurationAndThemeChanged;
                autoTradeDlg.FormClosing += (a, b) =>
                {
                    autoTradeDlg.autoTradeControl1.SelectedStockChanged -= OnSelectedStockAndDurationChanged;
                    autoTradeDlg.autoTradeControl1.SelectedStockAndThemeChanged -= OnSelectedStockAndDurationAndThemeChanged;
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
                agentTunningDialog.agentSimulationControl.SelectedStockChanged += OnSelectedStockAndDurationAndIndexChanged;
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

        PortfolioSimulationDlg portfolioSimulationDialog = null;
        private void portfolioSimulationMenuItem_Click(object sender, EventArgs e)
        {
            if (portfolioSimulationDialog == null)
            {
                portfolioSimulationDialog = new PortfolioSimulationDlg() { StartPosition = FormStartPosition.CenterScreen };
                portfolioSimulationDialog.portfolioSimulationControl1.SelectedStockChanged += OnSelectedStockAndDurationAndIndexChanged;

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
                    <th style=""font-size:20px;"" colspan=""8"" scope =""colgroup"">Value: {portfolio.TotalValue}€<br>Risk Free: {portfolio.RiskFreeValue}€<br>Cash:{portfolio.Balance}€<br>DrawDown: {portfolio.DrawDown.ToString("P2")}</th>
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

            var previousTheme = StockAnalyzerForm.MainFrame.CurrentTheme;

            string reportBody = html;
            foreach (var position in positions)
            {
                StockSerie stockSerie = portfolio.GetStockSerieFromUic(position.Uic);
                if (stockSerie != null && stockSerie.Initialise() && stockSerie.Values.Count() > 50)
                {
                    barDurationChangeFromUI = true;
                    this.ViewModel.BarDuration = position.BarDuration;
                    barDurationChangeFromUI = false;

                    var bitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(stockSerie, position.Theme);

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

            var portfolioSerie = StockDictionary[portfolio.Name];
            this.ViewModel.BarDuration = BarDuration.Daily;
            var portfolioSerieBitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(portfolioSerie, "_Portfolio2", 350);
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
                    barDurationChangeFromUI = true;
                    this.ViewModel.BarDuration = order.BarDuration;
                    barDurationChangeFromUI = false;

                    var bitmapString = StockAnalyzerForm.MainFrame.GetStockSnapshotAsHtml(stockSerie, order.Theme, 350);

                    var stockNameHtml = stockNamePortfolioTemplate.Replace("%STOCKNAME%", stockSerie.StockName) + "\r\n";
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
            StockAnalyzerForm.MainFrame.CurrentTheme = previousTheme;

            return reportBody;
        }

        public void GeneratePortfolioReportFile(StockPortfolio portfolio)
        {
            if (portfolio.SaxoSilentLogin())
            {
                portfolio.Refresh();
            }

            this.Portfolio = portfolio;
            StockSerie previousStockSerie = this.CurrentStockSerie;
            string previousTheme = this.CurrentTheme;
            BarDuration previousBarDuration = previousStockSerie.BarDuration;

            this.ViewModel.IsHistoryActive = false;
            string reportTemplate = File.ReadAllText(@"Resources\PortfolioTemplate.html").Replace("%HTML_TILE%", portfolio.Name + "Report " + DateTime.Today.ToShortDateString());

            string positionHtml = portfolio.GeneratePositionHtml();

            var report = GeneratePortfolioReportHtml(portfolio);
            if (!string.IsNullOrEmpty(report))
            {
                var htmlReport = reportTemplate.Replace("%HTML_BODY%", report);
                string fileName = Path.Combine(Folders.Portfolio, $@"Report\{portfolio.Name}.html");
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.Write(htmlReport);
                }
                Process.Start(fileName);
            }
            this.ViewModel.IsHistoryActive = true;
            OnSelectedStockChanged(previousStockSerie.StockName, true);
            this.CurrentTheme = previousTheme;
            this.ViewModel.BarDuration = previousBarDuration;
            this.ViewModel.IsHistoryActive = true;
        }
        public void GenerateReport(BarDuration duration, List<StockAlertDef> alertDefs = null)
        {
            alertDefs ??= StockAlertDef.AlertDefs.Where(a => a.BarDuration == duration && a.InReport).OrderBy(a => a.Rank).ToList();
            if (alertDefs.Count == 0)
                return;
            this.ViewModel.IsHistoryActive = false;
            string timeFrame = duration.ToString();
            string folderName = Path.Combine(Folders.Report, timeFrame);

            CleanReportFolder(folderName);

            if (!File.Exists(ReportTemplatePath) || alertDefs.Count(a => a.InReport && a.Type == AlertType.Group) == 0)
                return;
            var htmlReportTemplate = File.ReadAllText(ReportTemplatePath);

            StockSerie previousStockSerie = this.CurrentStockSerie;
            string previousTheme = this.CurrentTheme;
            BarDuration previousBarDuration = previousStockSerie.BarDuration;

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
            if (StockDictionary.ContainsKey("McClellanSum.EURO_A"))
            {
                this.Size = new Size(950, 800);

                this.ViewModel.BarDuration = BarDuration.Daily;

                var bitmapString = this.GetStockSnapshotAsHtml(StockDictionary["McClellanSum.EURO_A"], "EUROA_SUM", 770);
                htmlReportTemplate = htmlReportTemplate.Replace("%EURO_A_IMG%", bitmapString);
            }

            // Find Pattern

            string pattern = @"%%.*?%%";

            // Instantiate the regular expression object.
            Regex regex = new Regex(pattern);

            // Match the regular expression pattern against the input string.
            MatchCollection matches = regex.Matches(htmlReportTemplate);

            foreach (Match match in matches)
            {
                var fields = match.Value.Replace("%%", "").Split('|');
                var stockName = fields[0];
                //var duration = fields[1];
                var theme = fields[2];
                var nbBars = int.Parse(fields[3]);
                if (!StockDictionary.ContainsKey(stockName))
                    continue;
                var bitmapString = this.GetStockSnapshotAsHtml(StockDictionary[stockName], theme, nbBars);
                string data = $"\r\n    <h2>{stockName}</h2>\r\n    <a>\r\n        <img src=\"{bitmapString}\">\r\n    </a>";

                htmlReportTemplate = htmlReportTemplate.Replace(match.Value, data);
            }

            StockSplashScreen.CloseForm(true);
            #endregion

            this.Size = previousSize;
            this.WindowState = previousState;

            // Download Market Harmonics pictures
            string url = "http://www.market-harmonics.com/images/tech/sentiment/nu.png";
            string destFile = Path.Combine(folderName, $"NU_{DateTime.Now.Ticks}.png");
            if (StockWebHelper.DownloadFile(destFile, url))
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NU_IMG%", "file://" + destFile);
            }
            else
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NU_IMG%", url);
            }
            url = "http://www.market-harmonics.com/images/tech/sentiment/nulong.png";
            destFile = Path.Combine(folderName, $"NUL_{DateTime.Now.Ticks}.png");
            if (StockWebHelper.DownloadFile(destFile, url))
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NUL_IMG%", "file://" + destFile);
            }
            else
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NUL_IMG%", url);
            }
            url = "http://www.market-harmonics.com/images/tech/sentiment/ndsi.png";
            destFile = Path.Combine(folderName, $"NSDI_{DateTime.Now.Ticks}.png");
            if (StockWebHelper.DownloadFile(destFile, url))
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NSDI_IMG%", "file://" + destFile);
            }
            else
            {
                htmlReportTemplate = htmlReportTemplate.Replace("%MH_NSDI_IMG%", url);
            }

            var htmlReport = htmlReportTemplate.Replace("%HTML_TILE%", $"{duration} Report").Replace("%HTML_BODY%", htmlBody);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(htmlReport);
            }

            Process.Start(fileName);

            OnSelectedStockChanged(previousStockSerie.StockName, true);
            this.CurrentTheme = previousTheme;
            this.ViewModel.BarDuration = previousBarDuration;
            this.ViewModel.IsHistoryActive = true;
        }

        const string stockNameTemplate = "<a class=\"tooltip\">%MSG%<span><img src=\"%IMG%\"></a>";
        const string stockNamePortfolioTemplate = "<a href=\"#%STOCKNAME%\">%STOCKNAME%</a>";
        const string stockPictureTemplate = "<br/><h2 id=\"%STOCKNAME%\"><a href=\"#PAGE_TOP\">%STOCKNAME% - %DURATION%</a></h2><img alt=\"%STOCKNAME% - %DURATION% - Chart missing\" src=\"%IMG%\"/>";
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
                    <th style=""font-size:20px;"" colspan=""2"" >{alertDef.Group}</th>
                    <th style=""font-size:20px;"" colspan=""7"" scope =""colgroup""> {tableHeader} </th>
                </tr>
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

                this.CurrentTheme = alertDef.Theme;
                foreach (var alertValue in alertValues.OrderByDescending(l => l.Speed).Take(nbStocks))
                {
                    // Generate Snapshot
                    this.OnSelectedStockAndDurationChanged(alertValue.StockSerie.StockName, (BarDuration)alertDef.BarDuration, false);
                    // StockAnalyzerForm.MainFrame.SetThemeFromIndicator($"TRAILSTOP|{trailStopIndicatorName}");

                    var bitmapString = this.SnapshotAsHtml();

                    var stockName = stockNameTemplate.Replace("%MSG%", alertValue.StockSerie.StockName).Replace("%IMG%", bitmapString) + "\r\n";
                    var stokValue = alertValue.StockSerie.CalculateLastFastOscillator(stokPeriod, InputType.Close);
                    if (float.IsNaN(alertValue.Stop))
                    {
                        html += rowTemplate.
                            Replace("%GROUP%", alertValue.StockSerie.StockGroup.ToString()).
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
                            Replace("%GROUP%", alertValue.StockSerie.StockGroup.ToString()).
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
            if (this.currentStockSerie == null || this.WatchLists == null) return;

            if (watchlistDlg == null)
            {
                watchlistDlg = new WatchListDlg(this.WatchLists);
                watchlistDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
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

        #region Stock Scanner Dlg
        private StockScannerDlg stockScannerDlg = null;
        private void stockScannerMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            if (stockScannerDlg == null)
            {
                stockScannerDlg = new StockScannerDlg(StockDictionary, this.selectedGroup,
                    this.CurrentStockSerie.BarDuration,
                    this.themeDictionary[this.currentTheme]);
                stockScannerDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                stockScannerDlg.SelectStockGroupChanged += new SelectedStockGroupChangedEventHandler(this.OnSelectedStockGroupChanged);
                stockScannerDlg.FormClosing += new FormClosingEventHandler(delegate
                {
                    this.NotifyThemeChanged -= stockScannerDlg.OnThemeChanged;
                    this.NotifyBarDurationChanged -= stockScannerDlg.OnBarDurationChanged;
                    this.stockScannerDlg = null;
                });
                stockScannerDlg.Show();
            }
            else
            {
                stockScannerDlg.Activate();
            }
            this.NotifyThemeChanged += stockScannerDlg.OnThemeChanged;
            this.NotifyBarDurationChanged += stockScannerDlg.OnBarDurationChanged;

        }
        #endregion
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
            if (this.currentStockSerie == null) return;
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
            if (searchCombo.Focused) return false;

            if (this.currentStockSerie == null) return false;

            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if (this.stockNameComboBox.Focused || this.themeComboBox.Focused || this.barDurationComboBox.Focused)
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
                    case Keys.Control | Keys.M:
                        this.showEventMarqueeMenuItem.Checked = !this.showEventMarqueeMenuItem.Checked;
                        showEventMarqueeMenuItem_Click(null, null);
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
            mtg.Initialize(this.selectedGroup, this.currentStockSerie);
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
            if (this.CurrentTheme == WORK_THEME)
            {
                dico = this.themeDictionary[WORK_THEME];
            }
            else
            {
                // Create a dico copy
                dico = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in this.themeDictionary[this.CurrentTheme])
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
            if (this.StockDictionary.Keys.Contains(stockName))
            {
                StockSerie secondarySerie = this.StockDictionary[stockName];

                foreach (ToolStripMenuItem groupMenuItem in this.secondarySerieMenuItem.DropDownItems)
                {
                    if (groupMenuItem.Text == secondarySerie.StockGroup.ToString())
                    {
                        groupMenuItem.Checked = true;
                    }
                    else
                    {
                        groupMenuItem.Checked = false;
                    }
                    foreach (ToolStripMenuItem subMenuItem in groupMenuItem.DropDownItems)
                    {
                        if (subMenuItem.Text == secondarySerie.StockName)
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
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            if (this.currentStockSerie == null)
            {
                menuItem.Checked = false;
                ((ToolStripMenuItem)menuItem.OwnerItem).Checked = menuItem.Checked;
                return;
            }

            menuItem.Checked = !menuItem.Checked;
            FloatSerie secondarySerie = this.currentStockSerie.GenerateSecondarySerieFromOtherSerie(this.StockDictionary[sender.ToString()]);
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
            if (this.currentTheme != WORK_THEME)
            {
                // Create a dico copy
                Dictionary<string, List<string>> dico = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in this.themeDictionary[this.CurrentTheme])
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
                this.CurrentTheme = WORK_THEME;
                this.ApplyTheme();
            }
        }
        #endregion
        #region THEME MANAGEMENT

        public IEnumerable<string> Themes => themeComboBox.Items.OfType<string>().Where(t => !t.Contains("*"));
        private string currentTheme;
        public string CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                if (themeComboBox.SelectedItem.ToString() != value || value == WORK_THEME)
                {
                    if (themeComboBox.Items.Contains(value))
                    {
                        themeComboBox.SelectedItem = value;
                        currentTheme = value;
                        if (this.ThemeChanged != null)
                        {
                            this.ThemeChanged(value);
                        }
                    }
                }
                else
                {
                    if (currentTheme != value)
                    {
                        currentTheme = value;
                        if (this.ThemeChanged != null)
                        {
                            this.ThemeChanged(value);
                        }
                    }
                }
            }
        }
        public void SetThemeFromIndicator(string fullName)
        {
            using (new MethodLogger(this))
            {
                if (this.themeDictionary.ContainsKey(this.currentTheme) && this.themeDictionary[this.currentTheme].Values.Any(v => v.Any(vv => vv.Contains(fullName))))
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
        #region TWEET
        TweetDlg2 tweetDlg = null;
        static int tweetCount = 0;
        void tweetMenuItem_Click(object sender, EventArgs e)
        {
            if (tweetDlg == null)
            {
                string fileName = Path.Combine(Folders.Tweets, $"tweet{++tweetCount}.png");
                var bitmap = this.graphCloseControl.GetSnapshot();
                bitmap?.Save(fileName, ImageFormat.Png);

                tweetDlg = new TweetDlg2();
                tweetDlg.Disposed += tweetDialog_Disposed;
                tweetDlg.Show();

                tweetDlg.ViewModel.Text = $"${this.currentStockSerie.Symbol}" + Environment.NewLine;
                tweetDlg.ViewModel.FileName = fileName;
            }
            else
            {
                tweetDlg.Activate();
            }
        }

        void tweetDialog_Disposed(object sender, EventArgs e)
        {
            this.tweetDlg = null;
        }
        #endregion
        #region BEST TRENDS
        BestTrendDlg bestTrendDlg = null;
        void bestTrendViewMenuItem_Click(object sender, EventArgs e)
        {
            if (bestTrendDlg == null)
            {
                bestTrendDlg = new BestTrendDlg(this.selectedGroup.ToString(), this.ViewModel.BarDuration);
                bestTrendDlg.Disposed += bestrendDialog_Disposed;
                bestTrendDlg.bestTrend1.SelectedStockChanged += OnSelectedStockAndDurationAndIndexChanged;
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
                sectorDlg = new SectorDlg(this.selectedGroup.ToString(), this.ViewModel.BarDuration);
                sectorDlg.Disposed += sectorDialog_Disposed;
                sectorDlg.bestTrend1.SelectedStockChanged += OnSelectedStockAndDurationAndIndexChanged;
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
        #region Conditional Statistics
        StatisticsDlg statisticsDlg = null;
        void statisticsMenuItem_Click(object sender, EventArgs e)
        {
            if (statisticsDlg == null)
            {
                statisticsDlg = new StatisticsDlg() { StartPosition = FormStartPosition.CenterScreen };
                statisticsDlg.Disposed += statisticsDlg_Disposed;
                statisticsDlg.Show();
            }
            else
            {
                statisticsDlg.Activate();
            }
        }

        void statisticsDlg_Disposed(object sender, EventArgs e)
        {
            this.statisticsDlg = null;
        }
        #endregion
        #region EXPECTED VALUE
        ExpectedValueDlg expectedValueDlg = null;
        void expectedValueMenuItem_Click(object sender, EventArgs e)
        {
            if (expectedValueDlg == null)
            {
                expectedValueDlg = new ExpectedValueDlg() { StartPosition = FormStartPosition.CenterScreen };
                expectedValueDlg.Disposed += expectedValueDlg_Disposed;
                expectedValueDlg.Show();
            }
            else
            {
                expectedValueDlg.Activate();
            }
        }

        void expectedValueDlg_Disposed(object sender, EventArgs e)
        {
            this.expectedValueDlg = null;
        }
        #endregion
        #region HORSE RACE DIALOG
        HorseRaceDlg horseRaceDlg = null;
        void showHorseRaceViewMenuItem_Click(object sender, EventArgs e)
        {
            if (horseRaceDlg == null)
            {
                horseRaceDlg = new HorseRaceDlg(this.selectedGroup.ToString(), this.ViewModel.BarDuration);
                horseRaceDlg.Disposed += horseRaceDlg_Disposed;
                horseRaceDlg.Show();
            }
            else
            {
                horseRaceDlg.Activate();
            }
        }

        void horseRaceDlg_Disposed(object sender, EventArgs e)
        {
            this.horseRaceDlg = null;
        }
        #endregion
        #region MARKET REPLAY
        MarketReplayDlg marketReplayDlg = null;
        void marketReplayViewMenuItem_Click(object sender, EventArgs e)
        {
            if (marketReplayDlg == null)
            {
                marketReplayDlg = new MarketReplayDlg(this.selectedGroup, this.ViewModel.BarDuration);
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
                    StockName = this.CurrentStockSerie.StockName,
                    Group = StockAnalyzerForm.MainFrame.Group,
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
                drawingDlg.drawingControl1.SelectedStockAndDurationChanged += OnSelectedStockAndDurationChanged;
                drawingDlg.Disposed += delegate
                {
                    drawingDlg.drawingControl1.SelectedStockAndDurationChanged -= OnSelectedStockAndDurationChanged;
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
            this.GraphCloseControl.ChartMode = GraphChartMode.Line;
            this.graphCloseControl.ForceRefresh();
        }
        private void selectDisplayedIndicatorMenuItem_Click(object sender, EventArgs e)
        {
            using (new MethodLogger(this))
            {
                StockIndicatorSelectorDlg indicatorSelectorDialog = new StockIndicatorSelectorDlg(this.themeDictionary[this.CurrentTheme]) { StartPosition = FormStartPosition.CenterScreen };
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
                    this.CurrentTheme = this.themeComboBox.SelectedItem.ToString();
                }
                //
                OnNeedReinitialise(false);
            }
        }
        void indicatorSelectorDialog_ThemeEdited(Dictionary<string, List<string>> themeDico)
        {
            // Apply new working theme
            this.themeDictionary[WORK_THEME] = themeDico;
            this.CurrentTheme = WORK_THEME;
        }
        public delegate void OnStrategyChangedHandler(string currentStrategy);
        public delegate void OnThemeChangedHandler(string currentTheme);
        public delegate void OnThemeEditedHandler(Dictionary<string, List<string>> themeDico);
        readonly Dictionary<string, Dictionary<string, List<string>>> themeDictionary = new Dictionary<string, Dictionary<string, List<string>>>();

        public Dictionary<string, List<string>> GetCurrentTheme()
        {
            if (!this.themeDictionary.ContainsKey(this.CurrentTheme))
                // LoadTheme
                if (!LoadCurveTheme(currentTheme))
                    return null;
            return this.themeDictionary[this.CurrentTheme];
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


        public event NotifySelectedThemeChangedEventHandler NotifyThemeChanged;
        public event NotifyBarDurationChangedEventHandler NotifyBarDurationChanged;

        event OnThemeChangedHandler ThemeChanged;

        void StockAnalyzerForm_ThemeChanged(string currentTheme)
        {
            if (string.IsNullOrEmpty(currentTheme))
            {
                // Add error management here
                throw new Exception("We don't deal with empty themes in this house");
            }
            else
            {
                if (this.currentStockSerie.StockAnalysis != null && this.currentStockSerie.StockAnalysis.Theme == currentTheme)
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

        private bool repaintSuspended = false;
        public void ApplyTheme()
        {
            using (new MethodLogger(this, showTimerDebug))
            {
                using (new StockSerieLocker(this.currentStockSerie))
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        StockLog.Write($"Apply theme {this.CurrentStockSerie?.StockName}-{this.ViewModel.BarDuration}-{this.CurrentTheme}");
                        if (changingGroup) return;
                        if (this.CurrentTheme == null || this.CurrentStockSerie == null) return;
                        if (!this.CurrentStockSerie.IsInitialised)
                        {
                            this.statusLabel.Text = ("Loading data...");
                            this.Refresh();
                        }
                        if (!this.CurrentStockSerie.Initialise() || this.CurrentStockSerie.Count == 0)
                        {
                            this.DeactivateGraphControls("Data for " + this.CurrentStockSerie.StockName + " cannot be initialised");
                            return;
                        }

                        // Set bar duration
                        this.CurrentStockSerie.BarDuration = this.ViewModel.BarDuration;
                        // Delete transient drawing created by alert Detection
                        if (this.CurrentStockSerie.StockAnalysis.DeleteTransientDrawings() > 0)
                        {
                            this.CurrentStockSerie.ResetIndicatorCache();
                        }

                        if (this.CurrentStockSerie.Count < MIN_BAR_DISPLAY)
                        {
                            this.DeactivateGraphControls("Not enough data to display...");
                            return;
                        }

                        // Add to browsing history
                        this.ViewModel.AddHistory(this.CurrentStockSerie.StockName, this.CurrentTheme);

                        // Build curve list from definition
                        if (!this.themeDictionary.ContainsKey(currentTheme))
                        {
                            // LoadTheme
                            LoadCurveTheme(currentTheme);
                        }

                        // Force resetting the secondary serie.
                        if (themeDictionary[currentTheme]["CloseGraph"].FindIndex(s => s.StartsWith("SECONDARY")) == -1)
                        {
                            if (this.graphCloseControl.SecondaryFloatSerie != null)
                            {
                                themeDictionary[currentTheme]["CloseGraph"].Add("SECONDARY|" + this.graphCloseControl.SecondaryFloatSerie.Name);
                            }
                            else
                            {
                                themeDictionary[currentTheme]["CloseGraph"].Add("SECONDARY|NONE");
                            }
                        }

                        DateTime[] dateSerie = CurrentStockSerie.Keys.ToArray();
                        GraphCurveTypeList curveList;
                        bool skipEntry = false;
                        foreach (string entry in themeDictionary[currentTheme].Keys)
                        {
                            if (entry.ToUpper().EndsWith("GRAPH"))
                            {
                                GraphControl graphControl = null;
                                curveList = new GraphCurveTypeList();
                                switch (entry.ToUpper())
                                {
                                    case "CLOSEGRAPH":
                                        graphControl = this.graphCloseControl;
                                        this.graphCloseControl.Agenda = this.CurrentStockSerie.Agenda;
                                        this.graphCloseControl.Dividends = this.CurrentStockSerie.Dividend;
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
                                        if (this.CurrentStockSerie.HasVolume)
                                        {
                                            graphControl = this.graphVolumeControl;
                                            curveList.Add(new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.EXCHANGED), Pens.Green, true));
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

                                    foreach (string line in this.themeDictionary[currentTheme][entry])
                                    {
                                        string[] fields = line.Split('|');
                                        switch (fields[0].ToUpper())
                                        {
                                            case "GRAPH":
                                                string[] colorItem = fields[1].Split(':');
                                                graphControl.BackgroundColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                                colorItem = fields[2].Split(':');
                                                graphControl.TextBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                                graphControl.ShowGrid = bool.Parse(fields[3]);
                                                colorItem = fields[4].Split(':');
                                                graphControl.GridColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));

                                                if (entry.ToUpper() == "CLOSEGRAPH")
                                                {
                                                    if (fields.Length >= 7)
                                                    {
                                                        this.graphCloseControl.SecondaryPen = GraphCurveType.PenFromString(fields[6]);
                                                    }
                                                    else
                                                    {
                                                        this.graphCloseControl.SecondaryPen = new Pen(Color.DarkGoldenrod, 1);
                                                    }
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
                                                if (this.currentStockSerie.SecondarySerie != null)
                                                {
                                                    CheckSecondarySerieMenu(fields[1]);
                                                    this.graphCloseControl.SecondaryFloatSerie = this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(this.currentStockSerie.SecondarySerie);
                                                }
                                                else
                                                {
                                                    if (fields[1].ToUpper() == "NONE" ||
                                                        !this.StockDictionary.ContainsKey(fields[1]))
                                                    {
                                                        ClearSecondarySerieMenu();
                                                        this.graphCloseControl.SecondaryFloatSerie = null;
                                                    }
                                                    else
                                                    {
                                                        if (this.StockDictionary.ContainsKey(fields[1]))
                                                        {
                                                            CheckSecondarySerieMenu(fields[1]);
                                                            this.graphCloseControl.SecondaryFloatSerie = this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(this.StockDictionary[fields[1]]);
                                                        }
                                                    }
                                                }
                                                break;
                                            case "DATA":
                                                curveList.Add(
                                                    new GraphCurveType(
                                                        CurrentStockSerie.GetSerie(
                                                            (StockDataType)Enum.Parse(typeof(StockDataType), fields[1])),
                                                 fields[2], bool.Parse(fields[3])));
                                                break;
                                            case "TRAIL":
                                            case "INDICATOR":
                                                {
                                                    IStockIndicator stockIndicator = (IStockIndicator)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
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
                                                        if (!(stockIndicator.RequiresVolumeData && !this.CurrentStockSerie.HasVolume))
                                                        {
                                                            curveList.Indicators.Add(stockIndicator);
                                                        }
                                                    }
                                                }
                                                break;
                                            case "CLOUD":
                                                {
                                                    var stockCloud = (IStockCloud)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
                                                    if (stockCloud != null)
                                                    {
                                                        curveList.Cloud = stockCloud;
                                                    }
                                                }
                                                break;
                                            case "PAINTBAR":
                                                {
                                                    IStockPaintBar paintBar = (IStockPaintBar)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
                                                    curveList.PaintBar = paintBar;
                                                }
                                                break;
                                            case "AUTODRAWING":
                                                {
                                                    IStockAutoDrawing autoDrawing = (IStockAutoDrawing)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
                                                    curveList.AutoDrawing = autoDrawing;
                                                }
                                                break;
                                            case "DECORATOR":
                                                {
                                                    IStockDecorator decorator =
                                                        (IStockDecorator)
                                                            StockViewableItemsManager.GetViewableItem(line,
                                                                this.CurrentStockSerie);
                                                    curveList.Decorator = decorator;
                                                    this.GraphCloseControl.CurveList.ShowMes.Add(decorator);
                                                }
                                                break;
                                            case "TRAILSTOP":
                                                {
                                                    IStockTrailStop trailStop =
                                                        (IStockTrailStop)
                                                            StockViewableItemsManager.GetViewableItem(line,
                                                                this.CurrentStockSerie);
                                                    curveList.TrailStop = trailStop;
                                                }
                                                break;
                                            case "LINE":
                                                horizontalLines.Add(new HLine(float.Parse(fields[1]),
                                                    GraphCurveType.PenFromString(fields[2])));
                                                break;
                                            default:
                                                continue;
                                        }
                                    }
                                    if (curveList.FindIndex(c => c.DataSerie.Name == "CLOSE") < 0)
                                    {
                                        curveList.Insert(0,
                                            new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.CLOSE), Pens.Black,
                                                false));
                                    }
                                    if (graphControl == this.graphCloseControl)
                                    {
                                        if (curveList.FindIndex(c => c.DataSerie.Name == "LOW") < 0)
                                        {
                                            curveList.Insert(0,
                                                new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.LOW), Pens.Black,
                                                    true));
                                        }
                                        if (curveList.FindIndex(c => c.DataSerie.Name == "HIGH") < 0)
                                        {
                                            curveList.Insert(0,
                                                new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.HIGH), Pens.Black,
                                                    true));
                                        }
                                        if (curveList.FindIndex(c => c.DataSerie.Name == "OPEN") < 0)
                                        {
                                            curveList.Insert(0,
                                                new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.OPEN), Pens.Black,
                                                    false));
                                        }
                                    }
                                    if (!this.CurrentStockSerie.StockAnalysis.DrawingItems.ContainsKey(this.CurrentStockSerie.BarDuration))
                                    {
                                        this.CurrentStockSerie.StockAnalysis.DrawingItems.Add(this.CurrentStockSerie.BarDuration, new StockDrawingItems());
                                    }
                                    graphControl.Initialize(curveList, horizontalLines, dateSerie,
                                        CurrentStockSerie,
                                        CurrentStockSerie.StockAnalysis.DrawingItems[this.CurrentStockSerie.BarDuration],
                                        startIndex, endIndex);
                                }
                                catch (Exception exception)
                                {
                                    StockAnalyzerException.MessageBox(exception);
                                    StockLog.Write("Exception loading theme: " + this.currentTheme);
                                    foreach (string line in this.themeDictionary[currentTheme][entry])
                                    {
                                        StockLog.Write(line);
                                    }
                                    StockLog.Write(exception);
                                }
                            }
                        }

                        if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.BREADTH))
                        {
                            string[] fields = this.currentStockSerie.StockName.Split('.');
                            if (fields.Length > 1 && this.StockDictionary.ContainsKey(fields[1]))
                            {
                                this.graphCloseControl.SecondaryFloatSerie = this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(this.StockDictionary[fields[1]]);
                            }
                        }

                        // Reinitialise zoom
                        ResetZoom();
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
                if (this.CurrentTheme != themeComboBox.SelectedItem.ToString())
                {
                    this.CurrentTheme = themeComboBox.SelectedItem.ToString();

                    if (this.CurrentTheme != WORK_THEME)
                    {
                        Settings.Default.SelectedTheme = themeComboBox.SelectedItem.ToString();
                        Settings.Default.Save();
                    }

                    this.NotifyThemeChanged?.Invoke(this.themeDictionary[this.currentTheme]);
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
            }

            while (themeComboBox.Items.Count == 0)
            {
                foreach (string themeName in Directory.EnumerateFiles(folderName, "*.thm"))
                {
                    themeComboBox.Items.Add(themeName.Split('\\').Last().Replace(".thm", ""));
                }
                if (themeComboBox.Items.Count == 0)
                {
                    // Create a default empty theme
                    string emptyTheme = "#ScrollGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line\r\nDATA|CLOSE|1:255:0:0:0:Solid|True\r\n#CloseGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line\r\nDATA|CLOSE|1:255:0:0:0:Solid|True\r\nSECONDARY|NONE\r\n#Indicator1Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#Indicator2Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#Indicator3Graph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart\r\n#VolumeGraph\r\nGRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart";
                    using StreamWriter tw = new StreamWriter(folderName + @"\\" + Localisation.UltimateChartistStrings.ThemeEmpty + ".thm");
                    tw.Write(emptyTheme);
                }
            }

            //
            if (this.CurrentStockSerie != null && !string.IsNullOrEmpty(this.CurrentStockSerie.StockAnalysis.Theme))
            {
                if (this.themeComboBox.Items.Contains(CurrentStockSerie.StockAnalysis.Theme))
                {
                    this.themeComboBox.SelectedItem = CurrentStockSerie.StockAnalysis.Theme;
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
            // Load current theme
            LoadCurveTheme(currentTheme);

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
            foreach (string entry in themeDictionary[this.CurrentTheme].Keys)
            {
                sr.WriteLine("#" + entry);
                foreach (string line in themeDictionary[this.CurrentTheme][entry])
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
            if (this.CurrentTheme == WORK_THEME)
            {
                this.saveThemeMenuItem_Click(sender, e);
                if (this.CurrentTheme == WORK_THEME)
                {
                    return;
                }
                this.currentStockSerie.StockAnalysis.Theme = this.currentTheme;
                this.defaultThemeStripButton.CheckState = CheckState.Checked;
            }
            else if (this.defaultThemeStripButton.CheckState == CheckState.Checked)
            {
                this.currentStockSerie.StockAnalysis.Theme = string.Empty;
                this.defaultThemeStripButton.CheckState = CheckState.Unchecked;
            }
            else
            {
                this.currentStockSerie.StockAnalysis.Theme = this.currentTheme;
                this.defaultThemeStripButton.CheckState = CheckState.Checked;
            }
            SaveAnalysis(this.ViewModel.AnalysisFile);
        }
        void deleteThemeStripButton_Click(object sender, EventArgs e)
        {
            if (this.CurrentTheme == WORK_THEME)
            {
                return;
            }

            // delete theme file
            string fileName = Path.Combine(Folders.Theme, CurrentTheme + ".thm");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            foreach (StockSerie stockSerie in this.StockDictionary.Values)
            {
                if (stockSerie.StockAnalysis.Theme == this.CurrentTheme)
                {
                    stockSerie.StockAnalysis.Theme = string.Empty;
                }
            }

            this.themeComboBox.Items.Remove(this.CurrentTheme);
            if (this.CurrentStockSerie.StockAnalysis.Theme != string.Empty)
            {
                this.themeComboBox.SelectedItem = this.CurrentStockSerie.StockAnalysis.Theme;
            }
            else
            {
                this.themeComboBox.SelectedItem = this.themeComboBox.Items[0].ToString();
            }
        }
        #endregion
        #region SHOW AGENDA MENU HANDLERS

        private void CreateAgendaMenuItem()
        {
            AgendaEntryType agendaEntryType = AgendaEntryType.No;
            if (!Enum.TryParse(Settings.Default.ShowAgenda, out agendaEntryType))
            {
                Settings.Default.ShowAgenda = AgendaEntryType.No.ToString();
            }

            // Clean existing menus
            this.showAgendaMenuItem.DropDownItems.Clear();

            List<ToolStripItem> agendaMenuItems = new List<ToolStripItem>();
            ToolStripMenuItem agendaSubMenuItem;

            foreach (AgendaEntryType agendaType in Enum.GetValues(typeof(AgendaEntryType)))
            {
                // Create group menu items
                agendaSubMenuItem = new ToolStripMenuItem(agendaType.ToString());
                agendaSubMenuItem.Click += new EventHandler(agendaSubMenuItem_Click);
                agendaSubMenuItem.Checked = (agendaType == agendaEntryType);

                agendaMenuItems.Add(agendaSubMenuItem);
            }
            this.showAgendaMenuItem.DropDownItems.AddRange(agendaMenuItems.ToArray());
        }

        private void agendaSubMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowAgenda = sender.ToString();
            Settings.Default.Save();

            foreach (ToolStripMenuItem agendaSubMenuItem in this.showAgendaMenuItem.DropDownItems)
            {
                agendaSubMenuItem.Checked = agendaSubMenuItem.Text == Settings.Default.ShowAgenda;
            }
            this.OnNeedReinitialise(false);
        }

        #endregion

        void aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
        }

        #region FILE MENU HANDLERS
        private void newAnalysisMenuItem_Click(object sender, EventArgs e)
        {
            foreach (StockSerie stockSerie in this.StockDictionary.Values)
            {
                stockSerie.StockAnalysis.Clear();
            }
            this.saveAnalysisFileAsMenuItem_Click(sender, e);

            OnNeedReinitialise(true);
        }
        private void loadAnalysisFileMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

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
                Settings.Default.Save();

                // Apply the them of the loaded analysis file if any
                if (this.currentStockSerie != null && this.currentStockSerie.StockAnalysis.Theme != string.Empty)
                {
                    this.CurrentTheme = this.currentStockSerie.StockAnalysis.Theme;
                }
            }
        }
        private void saveAnalysisFileMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

            this.SaveAnalysis(this.ViewModel.AnalysisFile);
        }
        private void saveAnalysisFileAsMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

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
            if (configDialog.ShowDialog(this.StockDictionary) == DialogResult.OK)
            {
                var dataProvider = (IStockDataProvider)configDialog;
                dataProvider.InitDictionary(this.StockDictionary, true);
                this.CreateGroupMenuItem();
                this.CreateSecondarySerieMenuItem();
                this.InitialiseStockCombo(true);
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
            foreach (var serie in this.StockDictionary.Values)
            {
                serie.StockAnalysis.DrawingItems.Clear();
            }
            OnNeedReinitialise(true);
        }
        #endregion
        public void ShowAgenda()
        {
            if (this.currentStockSerie != null)
            {
                if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
                {
                    ABCDataProvider.DownloadAgenda(this.currentStockSerie);
                }
                if (this.currentStockSerie.Agenda != null)
                {
                    StockAgendaForm agendaForm = new StockAgendaForm(this.currentStockSerie);
                    agendaForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No agenda information for this stock", "Error");
                }
            }
        }

        internal void OpenInPEAPerf()
        {
            if (string.IsNullOrWhiteSpace(this.currentStockSerie.ISIN))
                return;

            if (!File.Exists(PEAPerfTemplatePath))
                return;

            // Find name from PEA Performance
            StockWebHelper wh = new StockWebHelper();
            var suggestXML = wh.DownloadHtml("https://www.pea-performance.fr/wp-content/plugins/pea-performance/autocomplete/autocomplete_ajax.php?search=" + this.currentStockSerie.ISIN, null);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(suggestXML);
            XmlNodeList parentNode = xmlDoc.GetElementsByTagName("suggest");
            if (parentNode.Count != 1)
                return;
            var symbol = parentNode.Item(0).InnerText.Split('|')[1];

            string url = $"https://www.pea-performance.fr/fiches-societes/{symbol}/";
            Process.Start(url);
        }
        internal void OpenInZBMenu()
        {
            if (string.IsNullOrWhiteSpace(this.currentStockSerie.ISIN))
                return;
            string url = "https://www.zonebourse.com/recherche/?q=%ISIN%";
            url = url.Replace("%ISIN%", this.currentStockSerie.ISIN);
            Process.Start(url);
        }
        internal void openInTradingViewMenu()
        {
            string url = $"https://www.tradingview.com/";
            if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.PEA_EURONEXT))
            {
                url = $"https://www.tradingview.com/symbols/EURONEXT-{this.currentStockSerie.Symbol}/financials-statistics-and-ratios/";
            }
            else
            {
                Clipboard.SetText(currentStockSerie.StockName);
            }
            Process.Start(url);
        }

        internal void OpenInDataProvider()
        {
            IStockDataProvider dataProvider = StockDataProviderBase.GetDataProvider(this.CurrentStockSerie.DataProvider);
            if (dataProvider == null)
            {
                return;
            }
            dataProvider.OpenInDataProvider(this.CurrentStockSerie);
        }
        internal void OpenInYahoo()
        {
            if (string.IsNullOrWhiteSpace(this.currentStockSerie.ISIN))
                return;

            YahooSearchResult searchResult = YahooDataProvider.SearchFromYahoo(this.currentStockSerie.ISIN);
            if (searchResult?.quotes != null && searchResult.quotes.Count > 0)
            {
                string url = $"https://finance.yahoo.com/quote/{searchResult.quotes[0].symbol}/";
                Process.Start(url);
            }
            else
            {
                string url = $"https://finance.yahoo.com/lookup/?s={this.currentStockSerie.StockName}";
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
                dataProvider.InitDictionary(this.StockDictionary, true);
                this.CreateGroupMenuItem();
                this.CreateSecondarySerieMenuItem();
                this.InitialiseStockCombo(true);
            }
        }

        private static string syncFileName = ".LastSync.txt";
        private static void OneDriveSync()
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
                p.WaitForExit(60000);
            }
        }
    }
}