using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockWeb;
using StockAnalyzerApp.CustomControl;
using StockAnalyzerApp.CustomControl.AgendaDlg;
using StockAnalyzerApp.CustomControl.AlertDialog;
using StockAnalyzerApp.CustomControl.BinckPortfolioDlg;
using StockAnalyzerApp.CustomControl.ConditionalStatisticsDlg;
using StockAnalyzerApp.CustomControl.ExpectedValueDlg;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.CustomControl.HorseRaceDlgs;
using StockAnalyzerApp.CustomControl.IndicatorDlgs;
using StockAnalyzerApp.CustomControl.MarketReplay;
using StockAnalyzerApp.CustomControl.MultiTimeFrameDlg;
using StockAnalyzerApp.CustomControl.PalmaresControl;
using StockAnalyzerApp.CustomControl.SectorDlg;
using StockAnalyzerApp.CustomControl.SimulationDlgs;
using StockAnalyzerApp.CustomControl.TrendDlgs;
using StockAnalyzerApp.CustomControl.WatchlistDlgs;
using StockAnalyzerApp.Localisation;
using StockAnalyzerApp.StockScripting;
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
using System.Threading;
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
        public delegate void SelectedStockAndDurationChangedEventHandler(string stockName, StockBarDuration barDuration, bool activateMainWindow);
        public delegate void SelectedStockAndDurationAndIndexChangedEventHandler(string stockName, int startIndex, int endIndex, StockBarDuration barDuration, bool activateMainWindow);

        public delegate void SelectedStockGroupChangedEventHandler(string stockgroup);

        public delegate void SelectedStrategyChangedEventHandler(string strategyName);

        public delegate void NotifySelectedThemeChangedEventHandler(Dictionary<string, List<string>> theme);

        public delegate void NotifyBarDurationChangedEventHandler(StockBarDuration barDuration);

        public delegate void NotifyStrategyChangedEventHandler(string newStrategy);

        public delegate void StockWatchListsChangedEventHandler();

        public delegate void AlertDetectedHandler();
        public event AlertDetectedHandler AlertDetected;
        public delegate void AlertDetectionStartedHandler(int nbStock);
        public event AlertDetectionStartedHandler AlertDetectionStarted;
        public delegate void AlertDetectionProgressHandler(string StockName);
        public event AlertDetectionProgressHandler AlertDetectionProgress;

        public delegate void OnStockSerieChangedHandler(StockSerie newSerie, bool ignoreLinkedTheme);

        public delegate void SavePortofolio();

        public static StockAnalyzerForm MainFrame { get; private set; }
        public bool IsClosing { get; set; }

        private const string PEAPerfTemplatePath = @"Resources\PEAPerformanceTemplate.html";
        private const string ReportTemplatePath = @"Resources\ReportTemplate.html";

        public static CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-GB");
        public static CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        public static CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public StockDictionary StockDictionary { get; private set; }
        public SortedDictionary<StockSerie.Groups, StockSerie> GroupReference { get; private set; }

        public List<StockPortfolio> Portfolios => BinckPortfolioDataProvider.Portofolios;

        public ToolStripProgressBar ProgressBar
        {
            get { return this.progressBar; }
        }

        public GraphCloseControl GraphCloseControl
        {
            get { return this.graphCloseControl; }
        }

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

        public StockBarDuration BarDuration
        {
            get
            {
                return new StockBarDuration((BarDuration)this.barDurationComboBox.SelectedItem, (int)this.barSmoothingComboBox.SelectedItem, this.barHeikinAshiCheckBox.CheckBox.Checked, (int)this.barLineBreakComboBox.SelectedItem);
            }
        }

        private StockPortfolio binckPortfolio;
        public StockPortfolio BinckPortfolio
        {
            get => binckPortfolio;
            set
            {
                if (binckPortfolio != value)
                {
                    if (portfolioComboBox.SelectedItem != value)
                    {
                        portfolioComboBox.SelectedIndex = portfolioComboBox.Items.IndexOf(value);
                    }
                    else
                    {
                        binckPortfolio = value;
                    }
                }
            }
        }

        private StockSerie.Groups selectedGroup;
        public StockSerie.Groups Group => selectedGroup;


        private static int NbBars { get; set; }

        private int startIndex = 0;
        private int endIndex = 0;

        private List<GraphControl> graphList = new List<GraphControl>();

        #region CONSTANTS

        private static string WORK_THEME = "__NewTheme*";

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
        }

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
        #endregion

        protected override void OnShown(EventArgs e)
        {
            // Validate preferences and local repository
            while (string.IsNullOrWhiteSpace(Settings.Default.UserId) || !CheckLicense())
            {
                PreferenceDialog prefDlg = new PreferenceDialog();
                DialogResult res = prefDlg.ShowDialog();
                if (res == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
            }

            base.OnActivated(e);

            // Enable timers and multithreading
            busy = false;

            this.FormClosing += StockAnalyzerForm_FormClosing1;
        }

        private void StockAnalyzerForm_FormClosing1(object sender, FormClosingEventArgs e)
        {
            TimerSuspended = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = EnglishCulture;
            Thread.CurrentThread.CurrentCulture = EnglishCulture;

            System.Windows.FrameworkElement.LanguageProperty.OverrideMetadata
                (typeof(System.Windows.FrameworkElement),
                new System.Windows.FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Graphical initialisation
            StockSplashScreen.ProgressText = "Checking license";
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            StockLog.Write("GetFolderPath: " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            // This is the first time the user runs the application.
            Settings.Default.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UltimateChartistRoot";
            string stockRootFolder = Settings.Default.RootFolder;

            // Root folder sanity check
            if (!Directory.Exists(Settings.Default.RootFolder))
            {
                MessageBox.Show(UltimateChartistStrings.SetupCorruptedText, UltimateChartistStrings.SetupCorruptedTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(0);
            }
            if (!Directory.Exists(Settings.Default.RootFolder + StockDividend.DIVIDEND_SUBFOLDER))
            {
                Directory.CreateDirectory(Settings.Default.RootFolder + StockDividend.DIVIDEND_SUBFOLDER);
            }

            // Validate preferences and local repository
            if (string.IsNullOrWhiteSpace(Settings.Default.UserId) || !CheckLicense())
            {
                StockSplashScreen.CloseForm(true);
                return;
            }

            // Parse Yahoo market data
            StockSplashScreen.ProgressText = "Initialize stock dictionary...";
            StockSplashScreen.ProgressVal = 30;

            StockDataProviderBase.InitStockDictionary(this.StockDictionary,
                Settings.Default.DownloadData && NetworkInterface.GetIsNetworkAvailable(),
                new DownloadingStockEventHandler(Notifiy_SplashProgressChanged));

            //
            InitialiseThemeCombo();

            // Deserialize Drawing Items - Read Analysis files
            if (Settings.Default.AnalysisFile == string.Empty)
            {
                Settings.Default.AnalysisFile = Settings.Default.RootFolder + "\\" + "UltimateChartistAnalysis.ulc";
                Settings.Default.Save();
            }
            else
            {
                StockSplashScreen.ProgressText = "Reading Drawing items ...";
                LoadAnalysis(Settings.Default.AnalysisFile);
            }


#if DEBUG
            bool fastStart = false;
#else
            bool fastStart = false;
#endif
            if (!fastStart)
            {
                // Generate breadth 
                if (Settings.Default.GenerateBreadth)
                {
                    foreach (StockSerie stockserie in this.StockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.Breadth))
                    {
                        StockSplashScreen.ProgressText = "Generating breadth data " + stockserie.StockName;
                        stockserie.Initialise();
                    }
                }

                this.GroupReference = new SortedDictionary<StockSerie.Groups, StockSerie>();
                this.GroupReference.Add(StockSerie.Groups.CAC40, this.StockDictionary["CAC40"]);
            }

            // Deserialize saved orders
            StockSplashScreen.ProgressText = "Reading portofolio data...";

            InitialisePortfolioCombo();
            BinckPortfolio = BinckPortfolioDataProvider.Portofolios.First();

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

            this.StockSerieChanged += new OnStockSerieChangedHandler(StockAnalyzerForm_StockSerieChanged);
            this.ThemeChanged += new OnThemeChangedHandler(StockAnalyzerForm_ThemeChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(graphScrollerControl_ZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphCloseControl.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator2Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator3Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator1Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphVolumeControl.OnZoomChanged);
            StockSplashScreen.ProgressText = "Loading " + this.CurrentStockSerie.StockName + " data...";
            this.ForceBarDuration(StockBarDuration.Daily, false);
            SetDurationForStockGroup(this.CurrentStockSerie.StockGroup);
            this.StockAnalyzerForm_StockSerieChanged(this.CurrentStockSerie, false);

            // Initialise event call backs (because of a bug in the designer)
            this.graphCloseControl.MouseClick += new MouseEventHandler(graphCloseControl.GraphControl_MouseClick);
            this.graphScrollerControl.MouseClick += new MouseEventHandler(graphScrollerControl.GraphControl_MouseClick);
            this.graphIndicator2Control.MouseClick += new MouseEventHandler(graphIndicator2Control.GraphControl_MouseClick);
            this.graphIndicator3Control.MouseClick += new MouseEventHandler(graphIndicator3Control.GraphControl_MouseClick);
            this.graphIndicator1Control.MouseClick += new MouseEventHandler(graphIndicator1Control.GraphControl_MouseClick);
            this.graphVolumeControl.MouseClick += new MouseEventHandler(graphVolumeControl.GraphControl_MouseClick);

            // Refresh intraday every 2 minutes.
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Interval = 120 * 1000;
            refreshTimer.Start();

            #region DailyAlerts

            if (!dailyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(dailyAlertConfig);
            }

            if (!weeklyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(weeklyAlertConfig);
            }
            if (!monthlyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(monthlyAlertConfig);
            }

            #endregion

            if (Settings.Default.GenerateDailyReport)
            {
                // Daily report
                string fileName = Settings.Default.RootFolder + @"\CommentReport\Daily\Report.html";
                if (!File.Exists(fileName) || File.GetLastWriteTime(fileName).Date != DateTime.Today)
                {
                    GenerateReport("Daily Report", StockBarDuration.Daily, dailyAlertConfig.AlertDefs);
                }

                fileName = Settings.Default.RootFolder + @"\CommentReport\Weekly\Report.html";
                var lastUpdate = File.GetLastWriteTime(fileName).Date;
                if (!File.Exists(fileName) || lastUpdate != DateTime.Today)
                {
                    if (lastUpdate < DateTime.Today.AddDays(-7) ||
                         (DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastUpdate < DateTime.Today.AddDays(-1)) ||
                         (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastUpdate < DateTime.Today.AddDays(-2)) ||
                         (DateTime.Today.DayOfWeek == DayOfWeek.Monday && lastUpdate < DateTime.Today.AddDays(-3)))
                    {
                        GenerateReport("Weekly Report", StockBarDuration.Weekly, weeklyAlertConfig.AlertDefs);
                    }
                }

                fileName = Settings.Default.RootFolder + @"\CommentReport\Monthly\Report.html";
                lastUpdate = File.GetLastWriteTime(fileName).Date;
                if (!File.Exists(fileName) || lastUpdate.Month != DateTime.Today.Month)
                {
                    GenerateReport("Montly Report", StockBarDuration.Monthly, monthlyAlertConfig.AlertDefs);
                }
            }

            // Checks for alert every x minutes.
            if (Settings.Default.RaiseAlerts)
            {
                int minutes = Settings.Default.AlertsFrequency;
                alertTimer = new System.Windows.Forms.Timer(new Container());
                alertTimer.Tick += new EventHandler(alertTimer_Tick);
                alertTimer.Interval = minutes * 60 * 1000;
                alertTimer.Start();

                string fileName = Path.Combine(StockAlertLog.AlertLogFolder, "AlertLog.xml");
                IEnumerable<string> alertLog = new List<string>();
                bool needDirectAlertCheck = false;
                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName).Date != DateTime.Today)
                    {
                        if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 18)
                        {
                            needDirectAlertCheck = true;
                        }
                    }
                    else if (DateTime.Now - File.GetLastWriteTime(fileName) > new TimeSpan(0, 0, minutes, 0))
                    // Check if older than x Minutes
                    {
                        needDirectAlertCheck = true;
                    }
                }
                else
                {
                    needDirectAlertCheck = true;
                }
                if (needDirectAlertCheck) alertTimer_Tick(null, null);
            }

            AutoCompleteStringCollection allowedTypes = new AutoCompleteStringCollection();
            allowedTypes.AddRange(this.StockDictionary.Where(p => !p.Value.StockAnalysis.Excluded).Select(p => p.Key.ToUpper()).ToArray());
            searchText.AutoCompleteCustomSource = allowedTypes;
            searchText.AutoCompleteMode = AutoCompleteMode.Suggest;
            searchText.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Ready to start
            StockSplashScreen.CloseForm(true);
            this.Focus();
        }

        private void goBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchText.Text)) return;

            var text = searchText.Text.ToUpper();
            if (text == this.currentStockSerie.StockName.ToUpper()) return;

            var serie = this.StockDictionary.Values.FirstOrDefault(s => s.StockName.ToUpper() == text);

            if (serie == null) return;
            searchText.Text = serie.StockName;
            searchText.Select(0, serie.StockName.Length);

            // Update Group
            if (this.selectedGroup != serie.StockGroup)
            {
                this.selectedGroup = serie.StockGroup;

                SetDurationForStockGroup(serie.StockGroup);

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

        private bool CheckLicense()
        {
            return true;
            //StockLicense stockLicense = null;

            //// Check on local disk in license is found
            //string licenseFileName = Settings.Default.RootFolder + @"\license.dat";
            //if (File.Exists(licenseFileName))
            //{
            //    string fileName = licenseFileName;
            //    using (StreamReader sr = new StreamReader(fileName))
            //    {
            //        try
            //        {
            //            stockLicense = new StockLicense(Settings.Default.UserId, sr.ReadLine());
            //        }
            //        catch
            //        {
            //            this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseCorrupted);
            //            return false;
            //        }
            //        if (stockLicense.UserID != Settings.Default.UserId)
            //        {
            //            this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseInvalidUserId);
            //            return false;
            //        }
            //        if (Settings.Default.MachineID == string.Empty)
            //        {
            //            Settings.Default.MachineID = StockToolKit.GetMachineUID();
            //            Settings.Default.Save();
            //        }
            //        if (stockLicense.MachineID != Settings.Default.MachineID)
            //        {
            //            this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseInvalidMachineId);
            //            return false;
            //        }
            //        if (stockLicense.ExpiryDate < DateTime.Today)
            //        {
            //            this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseExpired);
            //            return false;
            //        }
            //        if (Assembly.GetExecutingAssembly().GetName().Version.Major > stockLicense.MajorVerion)
            //        {
            //            this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseWrongVersion);
            //            return false;
            //        }
            //    }
            //}
            //else
            //{
            //    this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseNoFile);
            //}
            //return true;
        }

        private void graphScrollerControl_ZoomChanged(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        #endregion
        #region TIMER MANAGEMENT
        public static bool busy = false;

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (TimerSuspended)
                return;
            if (busy) return;
            busy = true;

            try
            {
                if (this.currentStockSerie != null && this.currentStockSerie.StockGroup == StockSerie.Groups.INTRADAY)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (StockDataProviderBase.DownloadSerieData(this.currentStockSerie))
                    {
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
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
                busy = false;
            }
        }

        private StockAlertConfig userDefinedAlertConfig = StockAlertConfig.GetConfig("UserDefined");
        private StockAlertConfig intradayAlertConfig = StockAlertConfig.GetConfig("Intraday");
        private StockAlertConfig dailyAlertConfig = StockAlertConfig.GetConfig("Daily");
        private StockAlertConfig weeklyAlertConfig = StockAlertConfig.GetConfig("Weekly");
        private StockAlertConfig monthlyAlertConfig = StockAlertConfig.GetConfig("Monthly");

        private void alertTimer_Tick(object sender, EventArgs e)
        {
            if (TimerSuspended)
                return;
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.Hour < 8 || DateTime.Now.Hour > 20) return;
            if (this.intradayAlertConfig.AlertLog.LastRefreshDate > DateTime.Now.AddMinutes(-30))
            {
                if (DateTime.Now.Minute % 30 > Settings.Default.AlertsFrequency)
                    return;
            }

            if (this.intradayAlertConfig != null && this.intradayAlertConfig.AlertDefs.Count > 0)
            {
                var alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateAlert_Thread);
                alertThread.Name = "IntradayAlert";
                alertThread.Start(this.intradayAlertConfig);
            }

            if (this.userDefinedAlertConfig != null && this.userDefinedAlertConfig.AlertDefs.Count > 0)
            {
                var alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateAlert_Thread);
                alertThread.Name = "UserDefinedAlert";
                alertThread.Start(this.userDefinedAlertConfig);
            }
        }
        public void GenerateAlert_Thread(object param)
        {
            try
            {
                var p = (StockAlertConfig)param;
                if (p.TimeFrame == "UserDefined")
                {
                    this.GenerateUserDefinedAlert(p);
                }
                else
                {
                    this.GenerateAlert(p);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        ManualResetEvent alertThreadEvent = new ManualResetEvent(true);
        public void GenerateAlert(StockAlertConfig alertConfig)
        {
            StockLog.Write("Thread: " + Thread.CurrentThread.Name + "Alert: " + alertConfig.TimeFrame);
            alertThreadEvent.WaitOne();
            alertThreadEvent.Reset();
            if (busy || alertConfig == null) return;
            busy = true;

            try
            {
                string alertString = string.Empty;

                List<StockSerie> stockList;
                if (alertConfig.TimeFrame == "Intraday")
                {
                    stockList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded &&
                    !s.StockName.StartsWith("INT_") &&
                    s.BelongsToGroup(StockSerie.Groups.INTRADAY)).ToList();
                }
                else
                {
                    stockList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded &&
                    s.BelongsToGroup(StockSerie.Groups.CACALL)).ToList();
                }
                if (AlertDetectionStarted != null)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(this.AlertDetectionStarted, stockList.Count);
                    }
                    else
                    {
                        this.AlertDetectionStarted(stockList.Count);
                    }
                }

                foreach (var stockSerie in stockList)
                {
                    if (TimerSuspended)
                        return;
                    if (AlertDetectionProgress != null)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(this.AlertDetectionProgress, stockSerie.StockName);
                        }
                        else
                        {
                            this.AlertDetectionProgress(stockSerie.StockName);
                        }
                    }

                    if (stockSerie.StockGroup == StockSerie.Groups.INTRADAY)
                    {
                        StockDataProviderBase.DownloadSerieData(stockSerie);
                    }

                    if (!stockSerie.Initialise()) continue;

                    StockBarDuration previouBarDuration = stockSerie.BarDuration;

                    lock (alertConfig)
                    {
                        foreach (var alertDef in alertConfig.AlertDefs)
                        {
                            stockSerie.BarDuration = alertDef.BarDuration;
                            var values = stockSerie.GetValues(alertDef.BarDuration);

                            int stopIndex = Math.Max(10, stockSerie.LastCompleteIndex - 10);
                            for (int i = stockSerie.LastCompleteIndex; i > stopIndex; i--)
                            {
                                var dailyValue = values.ElementAt(i);
                                if (dailyValue.DATE < alertConfig.AlertLog.StartDate)
                                    break;
                                if (stockSerie.MatchEvent(alertDef, i))
                                {
                                    var date = i == stockSerie.LastIndex ? dailyValue.DATE : values.ElementAt(i + 1).DATE;
                                    var stockAlert = new StockAlert(alertDef,
                                        date,
                                        stockSerie.StockName,
                                        stockSerie.StockGroup.ToString(),
                                        dailyValue.CLOSE,
                                        dailyValue.VOLUME,
                                        stockSerie.GetIndicator("ROR(100)").Series[0][i]);

                                    if (alertConfig.AlertLog.Alerts.All(a => a != stockAlert))
                                    {
                                        alertString += stockAlert.ToString() + Environment.NewLine;
                                        if (this.InvokeRequired)
                                        {
                                            this.Invoke(new Action(() => alertConfig.AlertLog.Alerts.Insert(0, stockAlert)));
                                        }
                                        else
                                        {
                                            alertConfig.AlertLog.Alerts.Insert(0, stockAlert);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    stockSerie.BarDuration = previouBarDuration;
                }
                alertConfig.AlertLog.Save();

                if (!string.IsNullOrWhiteSpace(alertString) && !string.IsNullOrWhiteSpace(Settings.Default.UserSMTP) && !string.IsNullOrWhiteSpace(Settings.Default.UserEMail))
                {
                    StockMail.SendEmail("Ultimate Chartist - " + alertConfig.AlertLog.FileName.Replace("AlertLog", "").Replace(".xml", "") + " Alert", alertString);
                }

                if (this.AlertDetected != null)
                {
                    this.Invoke(this.AlertDetected);
                }

                StockSplashScreen.CloseForm(true);
            }
            finally
            {
                busy = false;
                alertThreadEvent.Set();
            }
        }
        public void GenerateUserDefinedAlert(StockAlertConfig alertConfig)
        {
            StockLog.Write("Thread: " + Thread.CurrentThread.Name + "Alert: " + alertConfig.TimeFrame);
            alertThreadEvent.WaitOne();
            alertThreadEvent.Reset();
            if (busy || alertConfig == null) return;
            busy = true;

            try
            {
                int lookbackPeriod = 10;

                string alertString = string.Empty;

                #region Detect alert from drawing
                var drawingIndicator = StockViewableItemsManager.GetViewableItem("AUTODRAWING|DRAWING()") as IStockPaintBar;
                foreach (var stockSerie in StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.StockAnalysis.DrawingItems.Sum(di => di.Value.Count) != 0))
                {
                    StockBarDuration previouBarDuration = stockSerie.BarDuration;
                    foreach (var drawings in stockSerie.StockAnalysis.DrawingItems.Where(dr => dr.Value.Any(d => d.IsPersistent)))
                    {
                        stockSerie.BarDuration = drawings.Key;
                        var values = stockSerie.GetValues(drawings.Key);
                        drawingIndicator.ApplyTo(stockSerie);
                        int stopIndex = Math.Max(100, stockSerie.LastCompleteIndex - lookbackPeriod);
                        for (int i = stockSerie.LastCompleteIndex; i >= stopIndex; i--)
                        {
                            var dailyValue = values.ElementAt(i);
                            string eventName = null;
                            if (drawingIndicator.Events[0][i])
                                eventName = "AUTODRAWING|DRAWING()=>ResistanceBroken";
                            else if (drawingIndicator.Events[1][i])
                                eventName = "AUTODRAWING|DRAWING()=>SupportBroken";
                            if (eventName != null)
                            {
                                var date = i == stockSerie.LastIndex ? dailyValue.DATE : values.ElementAt(i + 1).DATE;
                                var stockAlert = new StockAlert(eventName,
                                    drawings.Key,
                                    date,
                                    stockSerie.StockName,
                                    stockSerie.StockGroup.ToString(),
                                    dailyValue.CLOSE,
                                    dailyValue.VOLUME,
                                    stockSerie.GetIndicator("ROR(100)").Series[0][i]);

                                if (alertConfig.AlertLog.Alerts.All(a => a != stockAlert))
                                {
                                    alertString += stockAlert.ToString() + Environment.NewLine;
                                    if (this.InvokeRequired)
                                    {
                                        this.Invoke(new Action(() => alertConfig.AlertLog.Alerts.Insert(0, stockAlert)));
                                    }
                                    else
                                    {
                                        alertConfig.AlertLog.Alerts.Insert(0, stockAlert);
                                    }
                                }
                            }
                        }
                    }
                    stockSerie.BarDuration = previouBarDuration;
                }
                #endregion
                #region Detect Alert from Alert Definition
                foreach (var alertDef in alertConfig.AlertDefs.Where(ad => !string.IsNullOrEmpty(ad.StockName)))
                {
                    #region Find Serie for alert
                    if (TimerSuspended)
                        return;

                    var stockSerie = this.StockDictionary.Values.FirstOrDefault(s => !s.StockAnalysis.Excluded && s.StockName == alertDef.StockName);
                    if (stockSerie == null)
                    {
                        StockLog.Write("Serie not found in alert: " + alertDef.StockName);
                        continue;
                    }
                    if (stockSerie.StockGroup == StockSerie.Groups.INTRADAY)
                    {
                        StockDataProviderBase.DownloadSerieData(stockSerie);
                    }
                    if (!stockSerie.Initialise())
                        continue;

                    StockBarDuration previouBarDuration = stockSerie.BarDuration;
                    #endregion

                    stockSerie.BarDuration = alertDef.BarDuration;
                    var values = stockSerie.GetValues(alertDef.BarDuration);
                    int stopIndex = stockSerie.IndexOfFirstLowerOrEquals(alertDef.CreationDate);
                    for (int i = stockSerie.LastCompleteIndex; i >= stopIndex; i--)
                    {
                        var dailyValue = values.ElementAt(i);

                        if (stockSerie.MatchEvent(alertDef, i))
                        {
                            var date = i == stockSerie.LastIndex ? dailyValue.DATE : values.ElementAt(i + 1).DATE;
                            var stockAlert = new StockAlert(alertDef,
                                date,
                                stockSerie.StockName,
                                stockSerie.StockGroup.ToString(),
                                dailyValue.CLOSE,
                                dailyValue.VOLUME,
                                stockSerie.GetIndicator("ROR(100)").Series[0][i]);

                            if (alertConfig.AlertLog.Alerts.All(a => a != stockAlert))
                            {
                                alertString += stockAlert.ToString() + Environment.NewLine;
                                if (this.InvokeRequired)
                                {
                                    this.Invoke(new Action(() => alertConfig.AlertLog.Alerts.Insert(0, stockAlert)));
                                }
                                else
                                {
                                    alertConfig.AlertLog.Alerts.Insert(0, stockAlert);
                                }
                            }
                        }
                    }
                    stockSerie.BarDuration = previouBarDuration;
                }
                #endregion
                alertConfig.AlertLog.Save();

                if (!string.IsNullOrWhiteSpace(alertString) && !string.IsNullOrWhiteSpace(Settings.Default.UserSMTP) && !string.IsNullOrWhiteSpace(Settings.Default.UserEMail))
                {
                    StockMail.SendEmail("Ultimate Chartist - " + alertConfig.AlertLog.FileName.Replace("AlertLog", "").Replace(".xml", "") + " Alert", alertString);
                }

                if (this.AlertDetected != null)
                {
                    this.Invoke(this.AlertDetected);
                }

                StockSplashScreen.CloseForm(true);
            }
            finally
            {
                busy = false;
                alertThreadEvent.Set();
                StockLog.Write("Thread: " + Thread.CurrentThread.Name + "Alert: " + alertConfig.TimeFrame + "alertThreadEvent.Set()");
            }
        }
        #endregion

        public static bool TimerSuspended { get; set; } = false;

        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.Timer alertTimer;


        private void StockDictionary_ReportProgress(string progress)
        {
            StockSplashScreen.ProgressSubText = progress;
        }

        #region ZOOMING

        private void ChangeZoom(int startIndex, int endIndex)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                try
                {
                    this.startIndex = startIndex;
                    this.endIndex = endIndex;
                    this.graphScrollerControl.InitZoom(this.startIndex, this.endIndex);
                }
                catch (InvalidSerieException e)
                {
                    StockLog.Write(e);
                    DeactivateGraphControls(e.Message);
                }
                catch (System.Exception e)
                {
                    StockLog.Write(e);
                    DeactivateGraphControls(e.Message);
                    MessageBox.Show(e.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ResetZoom()
        {
            using (MethodLogger ml = new MethodLogger(this))
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

        private void ZoomIn()
        {
            NbBars = Math.Max(25, (int)(NbBars / 1.75f));
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

        public void OnSelectedStockChanged(string stockName, bool activate)
        {
            using (MethodLogger ml = new MethodLogger(this))
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
                            SetDurationForStockGroup(newGroup);
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

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);

                if (activate)
                {
                    this.Activate();
                }
            }
        }
        public void OnSelectedStockAndDurationChanged(string stockName, StockBarDuration barDuration, bool activate)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.barDurationComboBox.SelectedItem = barDuration.Duration;
                this.barSmoothingComboBox.SelectedItem = barDuration.Smoothing;
                this.barHeikinAshiCheckBox.CheckBox.Checked = barDuration.HeikinAshi;
                this.barLineBreakComboBox.SelectedItem = barDuration.LineBreak;

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
                this.stockNameComboBox.SelectedIndexChanged -= StockNameComboBox_SelectedIndexChanged;
                this.stockNameComboBox.Text = stockName;
                this.stockNameComboBox.SelectedIndexChanged += new EventHandler(StockNameComboBox_SelectedIndexChanged);

                StockAnalyzerForm_StockSerieChanged(this.StockDictionary[stockName], true);

                if (activate)
                {
                    this.Activate();
                }
            }
        }

        public void OnSelectedStockAndDurationAndIndexChanged(string stockName, int startIndex, int endIndex, StockBarDuration barDuration, bool activate)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.barDurationComboBox.SelectedItem = barDuration.Duration;
                this.barSmoothingComboBox.SelectedItem = barDuration.Smoothing;
                this.barHeikinAshiCheckBox.CheckBox.Checked = barDuration.HeikinAshi;
                this.barLineBreakComboBox.SelectedItem = barDuration.LineBreak;

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
            using (MethodLogger ml = new MethodLogger(this))
            {
                //
                if (newSerie == null)
                {
                    DeactivateGraphControls("No data to display");
                    this.Text = "Ultimate Chartist - " + "No stock selected";
                    return;
                }
                if (newSerie.BelongsToGroup(StockSerie.Groups.INTRADAY))
                {
                    this.statusLabel.Text = ("Downloading data...");
                    this.Refresh();
                    this.Cursor = Cursors.WaitCursor;
                    StockDataProviderBase.DownloadSerieData(newSerie);
                }
                if (!newSerie.IsInitialised)
                {
                    this.statusLabel.Text = ("Loading data...");
                    this.Refresh();
                    this.Cursor = Cursors.WaitCursor;
                }

                this.currentStockSerie = newSerie;
                if (!newSerie.Initialise() || newSerie.Count == 0)
                {
                    DeactivateGraphControls("No data to display");
                    this.Text = "Ultimate Chartist - " + "Failure Loading data selected from " + this.CurrentStockSerie.DataProvider;
                    return;
                }
                string id;
                if (CurrentStockSerie.ShortName == CurrentStockSerie.StockName)
                {
                    id = CurrentStockSerie.StockGroup + "-" + CurrentStockSerie.ShortName;
                }
                else
                {
                    id = CurrentStockSerie.StockGroup + "-" + CurrentStockSerie.ShortName + " - " + CurrentStockSerie.StockName;
                }
                if (!string.IsNullOrWhiteSpace(this.CurrentStockSerie.ISIN))
                {
                    id += " - " + this.CurrentStockSerie.ISIN;
                }
                id += " - " + this.CurrentStockSerie.DataProvider;
                this.Text = "Ultimate Chartist - " + Settings.Default.AnalysisFile.Split('\\').Last() + " - " + id;
                if (newSerie.Count < 20)
                {
                    DeactivateGraphControls("Not enough data to display");
                    return;
                }

                var bd = new StockBarDuration((BarDuration)this.barDurationComboBox.SelectedItem, (int)this.barSmoothingComboBox.SelectedItem, this.barHeikinAshiCheckBox.CheckBox.Checked, (int)this.barLineBreakComboBox.SelectedItem);
                this.currentStockSerie.BarDuration = bd;

                if (!ignoreLinkedTheme
                    && newSerie.StockAnalysis != null
                    && !string.IsNullOrEmpty(newSerie.StockAnalysis.Theme)
                    && this.themeComboBox.SelectedText != newSerie.StockAnalysis.Theme
                    && this.themeComboBox.Items.Contains(newSerie.StockAnalysis.Theme))
                {
                    if (this.themeComboBox.SelectedItem.ToString() == newSerie.StockAnalysis.Theme)
                    {
                        ApplyTheme();
                    }
                    else
                    {
                        this.themeComboBox.SelectedItem = newSerie.StockAnalysis.Theme;
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
            this.Cursor = Cursors.Arrow;
            this.statusLabel.Text = ("Loading data...");
        }

        private void StockAnalyzerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.ThemeToolbarLocation = this.themeToolStrip.Location;
            Settings.Default.StockToolbarLocation = this.browseToolStrip.Location;
            Settings.Default.drawingToolbarLocation = this.drawToolStrip.Location;
            Settings.Default.Save();

            this.IsClosing = true;
        }

        public void OnSerieEventProcessed()
        {
            this.progressBar.Value++;
        }

        public List<StockWatchList> WatchLists { get; set; }

        private void LoadWatchList()
        {
            string watchListsFileName = Settings.Default.RootFolder + @"\WatchLists.xml";

            // Parse watch lists
            if (File.Exists(watchListsFileName))
            {
                using (FileStream fs = new FileStream(watchListsFileName, FileMode.Open))
                {
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StockWatchList>));
                    this.WatchLists = (List<StockWatchList>)serializer.Deserialize(xmlReader);
                    this.WatchLists = this.WatchLists.OrderBy(wl => wl.Name).ToList();

                    // Cleanup missing stocks
                    foreach (var watchList in this.WatchLists)
                    {
                        watchList.StockList.RemoveAll(s => !StockDictionary.ContainsKey(s));
                    }
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
                    using (FileStream fs = new FileStream(analysisFileName, FileMode.Open))
                    {
                        System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                        settings.IgnoreWhitespace = true;
                        System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                        StockDictionary.ReadAnalysisFromXml(xmlReader);
                    }
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
            catch (System.Exception exception)
            {
                string message = exception.Message;
                if (exception.InnerException != null)
                {
                    message += "\n\r" + exception.InnerException.Message;
                }
                MessageBox.Show(message, "Error Loading Analysis file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private delegate bool DownloadDataMethod(string destination, ref bool upToDate);

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
            using (MethodLogger ml = new MethodLogger(this))
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

                    if (endIndex == 0 || endIndex > (CurrentStockSerie.Values.Count - 1))
                    {
                        this.ResetZoom();
                    }

                    // Refresh all components
                    RefreshGraph();
                }
            }
        }

        #region STOCK and PORTOFOLIO selection tool bar

        private void StockNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            if (busy) return;
            busy = true;
            if (Control.ModifierKeys == Keys.Control)
            {
                DownloadStockGroup();
            }
            else
            {
                DownloadStock(false);
            }
            busy = false;
        }

        private void ForceDownloadStock(bool showSplash)
        {
            using (MethodLogger ml = new MethodLogger(this))
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
                    if (MessageBox.Show($"Are you sure you want to force downloading the full group {currentStockSerie.StockGroup} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                    StockSplashScreen.FadeInOutSpeed = 0.25;
                    StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + this.currentStockSerie.StockName;

                    var stockSeries =
                       this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.selectedGroup));

                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = stockSeries.Count();
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();

                    foreach (var stockSerie in stockSeries)
                    {
                        StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + stockSerie.StockName;
                        StockDataProviderBase.ForceDownloadSerieData(stockSerie);

                        if (stockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
                        {
                            try
                            {
                                StockSplashScreen.ProgressText = "Downloading Dividend " + stockSerie.StockGroup + " - " + stockSerie.StockName;
                                this.CurrentStockSerie.Dividend.DownloadFromYahoo(stockSerie);
                            }
                            catch (Exception ex)
                            {
                                StockLog.Write(ex);
                            }
                        }

                        StockSplashScreen.ProgressVal++;
                    }

                    this.SaveAnalysis(Settings.Default.AnalysisFile);

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
            using (MethodLogger ml = new MethodLogger(this))
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
                        StockDataProviderBase.DownloadSerieData(stockSerie);
                        StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " + stockSerie.StockName;

                        if (stockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
                        {
                            try
                            {
                                StockSplashScreen.ProgressText = "Downloading Dividend " + stockSerie.StockGroup + " - " + stockSerie.StockName;
                                //this.CurrentStockSerie.Dividend.DownloadFromYahoo(stockSerie);
                            }
                            catch (Exception ex)
                            {
                                StockLog.Write(ex);
                            }
                        }

                        StockSplashScreen.ProgressVal++;
                    }

                    this.SaveAnalysis(Settings.Default.AnalysisFile);

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
            drawAreaStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void drawAreaStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (drawAreaStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddArea;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            drawLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void cupHandleBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (cupHandleBtn.Checked)
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
            drawAreaStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
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
            drawAreaStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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
            drawAreaStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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
            copyLineStripBtn.Checked = false;
            drawAreaStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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
            copyLineStripBtn.Checked = false;
            drawAreaStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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
            copyLineStripBtn.Checked = false;
            drawAreaStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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
            copyLineStripBtn.Checked = false;
            drawAreaStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            cupHandleBtn.Checked = false;
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

            this.SaveAnalysis(Settings.Default.AnalysisFile);

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
                string watchListsFileName = Settings.Default.RootFolder + @"\WatchLists.xml";
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                using (FileStream fs = new FileStream(watchListsFileName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StockWatchList>));
                    System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(fs, null);
                    xmlWriter.Formatting = System.Xml.Formatting.Indented;
                    xmlWriter.WriteStartDocument();
                    serializer.Serialize(xmlWriter, this.WatchLists);
                    xmlWriter.WriteEndDocument();
                }
            }
        }

        public void SaveAnalysis(string analysisFileName)
        {
            if (this.currentStockSerie == null) return;
            string tmpFileName = analysisFileName + ".tmp";
            bool success = true;
            // Save stock analysis to XML
            XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));
            System.Xml.XmlTextWriter xmlWriter;
            try
            {
                // Save analysis file
                using (FileStream fs = new FileStream(tmpFileName, FileMode.Create))
                {
                    xmlWriter = new System.Xml.XmlTextWriter(fs, null);
                    xmlWriter.Formatting = System.Xml.Formatting.Indented;
                    xmlWriter.WriteStartDocument();
                    StockDictionary.WriteAnalysisToXml(xmlWriter);
                    xmlWriter.WriteEndDocument();
                }
            }
            catch (System.Exception exception)
            {
                success = false;
                if (exception.InnerException != null)
                {
                    MessageBox.Show(exception.InnerException.Message, "Application Error", MessageBoxButtons.OK,
                       MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(exception.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void snapshotToolStripButton_Click(object sender, EventArgs e)
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
                Clipboard.SetImage(snapshot);
            }
        }

        private void magnetStripBtn_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.Magnetism = this.magnetStripBtn.Checked;
        }

        #endregion DRAWING TOOLBAR HANDLERS

        #region ANALYSYS TOOLBAR HANDLERS

        private void excludeButton_Click(object sender, EventArgs e)
        {
            // Flag as excluded
            CurrentStockSerie.StockAnalysis.Excluded = true;
            SaveAnalysis(Settings.Default.AnalysisFile);

            // Remove from current combo list.
            int selectedIndex = this.stockNameComboBox.SelectedIndex;
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
        #endregion

        #region REWIND/FAST FORWARD METHODS

        private void rewindBtn_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            int step = 20;
            Rewind(step);
        }

        private void fastForwardBtn_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            int step = 20;
            Forward(step);
        }

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
            if (!Enum.TryParse<StockSerie.Groups>(Settings.Default.SelectedGroup, out this.selectedGroup))
            {
                this.selectedGroup = StockSerie.Groups.INDICES;
                Settings.Default.SelectedGroup = StockSerie.Groups.INDICES.ToString();
                Settings.Default.Save();
            }

            // Clean existing menus
            this.stockFilterMenuItem.DropDownItems.Clear();

            List<ToolStripItem> groupMenuItems = new List<ToolStripItem>();
            ToolStripMenuItem groupSubMenuItem;

            List<string> validGroups = this.StockDictionary.GetValidGroupNames();
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

            this.OnSelectedStockGroupChanged(sender.ToString());
        }

        #region MENU CREATION
        private void CreateSecondarySerieMenuItem()
        {
            // Clean existing menus
            this.secondarySerieMenuItem.DropDownItems.Clear();
            List<string> validGroups = this.StockDictionary.GetValidGroupNames();
            ToolStripMenuItem[] groupMenuItems = new ToolStripMenuItem[validGroups.Count];

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

        #region BAR DURATION MANAGEMENT

        private void InitialiseBarDurationComboBox()
        {
            foreach (var barDuration in Enum.GetValues(typeof(BarDuration)))
            {
                this.barDurationComboBox.Items.Add(barDuration);
            }
            this.barDurationComboBox.SelectedItem = StockBarDuration.Daily;

            foreach (int barSmoothing in new List<int> { 1, 3, 6, 9, 12, 20, 50, 100 })
            {
                this.barSmoothingComboBox.Items.Add(barSmoothing);
            }
            this.barSmoothingComboBox.SelectedItem = this.barSmoothingComboBox.Items.OfType<int>().First();

            foreach (int lineBreak in new List<int> { 0, 1, 2, 3, 4, 5 })
            {
                this.barLineBreakComboBox.Items.Add(lineBreak);
            }
            this.barLineBreakComboBox.SelectedItem = this.barLineBreakComboBox.Items.OfType<int>().First();
        }

        private void BarDurationChanged(object sender, EventArgs e)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.currentStockSerie == null || !this.currentStockSerie.Initialise()) return;

                StockBarDuration barDuration = (BarDuration)barDurationComboBox.SelectedItem;
                barDuration.Smoothing = (int)barSmoothingComboBox.SelectedItem;
                barDuration.HeikinAshi = barHeikinAshiCheckBox.CheckBox.CheckState == CheckState.Checked;
                barDuration.LineBreak = (int)barLineBreakComboBox.SelectedItem;

                if (this.CurrentStockSerie.BarDuration != barDuration)
                {
                    int previousBarCount = this.CurrentStockSerie.Count;
                    this.CurrentStockSerie.BarDuration = barDuration;

                    if (previousBarCount != this.CurrentStockSerie.Count)
                    {
                        NbBars = Settings.Default.DefaultBarNumber;
                    }
                    this.endIndex = this.CurrentStockSerie.Count - 1;
                    this.startIndex = Math.Max(0, this.endIndex - NbBars);
                    if (endIndex - startIndex < 10)
                    {
                        this.DeactivateGraphControls("Not enough data to display...");
                        return;
                    }
                    OnNeedReinitialise(true);
                    this.ApplyTheme();

                    if (NotifyBarDurationChanged != null)
                    {
                        this.NotifyBarDurationChanged(barDuration);
                    }
                }
            }
        }

        public void ForceBarDuration(StockBarDuration barDuration, bool triggerEvent)
        {
            if (!triggerEvent)
            {
                this.barDurationComboBox.SelectedIndexChanged -= new System.EventHandler(this.BarDurationChanged);
            }

            this.barDurationComboBox.SelectedItem = barDuration.Duration;
            this.barSmoothingComboBox.SelectedItem = barDuration.Smoothing;
            this.barHeikinAshiCheckBox.CheckBox.Checked = barDuration.HeikinAshi;
            this.barLineBreakComboBox.SelectedItem = barDuration.LineBreak;

            if (!triggerEvent)
            {
                this.barDurationComboBox.SelectedIndexChanged += new System.EventHandler(this.BarDurationChanged);
            }
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
        private delegate bool ConditionMatched(int i, StockSerie serie, ref string eventName);

        private void logSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            StockSerie newSerie = this.CurrentStockSerie.GenerateLogStockSerie();
            AddNewSerie(newSerie);
        }
        #endregion

        private PalmaresDlg palmaresDlg = null;
        private void palmaresMenuItem_Click(object sender, EventArgs e)
        {
            if (palmaresDlg == null)
            {
                palmaresDlg = new PalmaresDlg();
                palmaresDlg.palmaresControl1.ViewModel.BarDuration = this.BarDuration;
                palmaresDlg.palmaresControl1.ViewModel.Group = this.Group;
                palmaresDlg.FormClosing += new FormClosingEventHandler(palmaresDlg_FormClosing);
                palmaresDlg.palmaresControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
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
        public bool changingGroup = false;
        private void OnSelectedStockGroupChanged(string stockGroup)
        {
            try
            {
                changingGroup = true;
                StockSerie.Groups newGroup = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), stockGroup);
                if (this.selectedGroup != newGroup)
                {
                    this.selectedGroup = newGroup;

                    foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                    {
                        groupSubMenuItem.Checked = groupSubMenuItem.Text == stockGroup;
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
                case StockSerie.Groups.INTRADAY:
                    if (this.logScaleBtn.CheckState == CheckState.Checked)
                    {
                        this.logScaleBtn_Click(null, null);
                    }
                    this.ForceBarDuration(StockBarDuration.Bar_12, true);
                    break;
                default:
                    if (this.barDurationComboBox.SelectedItem != null && this.BarDuration.Duration > StockAnalyzer.StockClasses.BarDuration.Monthly)
                    {
                        this.ForceBarDuration(StockBarDuration.Daily, true);
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

        private void hideIndicatorsStockMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.HideIndicators = this.hideIndicatorsStockMenuItem.Checked;
            this.OnNeedReinitialise(false);
        }

        #endregion VIEW MENU HANDLERS

        #region PORTOFOLIO MENU HANDERS

        BinckPortfolioDlg portfolioDlg = null;
        private void currentPortfolioMenuItem_Click(object sender, EventArgs e)
        {
            if (BinckPortfolio == null)
                return;

            if (portfolioDlg == null)
            {
                portfolioDlg = new BinckPortfolioDlg();
                portfolioDlg.FormClosing += (a, b) => { this.portfolioDlg = null; };
                portfolioDlg.Show();
            }
            else
            {
                portfolioDlg.Activate();
            }
        }

        private void showPortfolioSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.BinckPortfolio == null || this.BinckPortfolio.TradeOperations.Count == 0)
                return;

            this.AddNewSerie(this.StockDictionary.GeneratePortfolioSerie(this.BinckPortfolio));
        }

        private void nameMappingMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new StockAnalyzer.StockBinckPortfolio.NameMappingDlg.NameMappingDlg();
            dlg.Show();
        }
        #endregion

        #region ANALYSIS MENU HANDLERS

        AgentSimulationDlg agentTunningDialog = null;
        private void agentTunningMenuItem_Click(object sender, EventArgs e)
        {
            if (agentTunningDialog == null)
            {
                agentTunningDialog = new AgentSimulationDlg();
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
                portfolioSimulationDialog = new PortfolioSimulationDlg();
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

        private static string commentTitleTemplate = "COMMENT_TITLE_TEMPLATE";
        private static string commentTemplate = "COMMENT_TEMPLATE";
        private static string eventTemplate = "EVENT_TEMPLATE";

        // static private string htmlEventTemplate = "<P style=\"font-size: x-small\">" + eventTemplate + "</P>";
        private static string htmlEventTemplate = "<br />" + eventTemplate;

        private static string htmlAlertTemplate = "\r\n<B><U>" + commentTitleTemplate + "</U></B>" + commentTemplate;

        class RankedSerie
        {
            public int rank;
            public int previousRank;
            public float rankIndicatorValue;
            public float previousRankIndicatorValue;
            public StockSerie stockSerie;
        }

        string CELL_DIR_IMG_TEMPLATE =
           @"<td><img alt=""%DIR%"" src=""../../img/%DIR%.png"" height=""16"" width=""16""/></td>" +
           Environment.NewLine;

        private void GenerateReport(string title, StockBarDuration duration, List<StockAlertDef> alertDefs)
        {
            if (!File.Exists(ReportTemplatePath))
                return;
            var htmlReportTemplate = File.ReadAllText(ReportTemplatePath);

            StockSerie previousStockSerie = this.CurrentStockSerie;
            string previousTheme = this.CurrentTheme;
            StockBarDuration previousBarDuration = previousStockSerie.BarDuration;

            string timeFrame = duration.ToString();
            string folderName = Settings.Default.RootFolder + @"\CommentReport\" + timeFrame;
            string imgFolderName = folderName + @"\Img";
            CleanReportFolder(folderName);

            string fileName = folderName + @"\Report.html";

            string htmlBody = $"<h1 style=\"text-align: center;\">{title} - {DateTime.Today.ToShortDateString()}</h1>";

            #region Report leaders

            this.barDurationComboBox.SelectedItem = StockBarDuration.Daily;

            string rankLeaderIndicatorName = "ROR(100)";
            string rankLoserIndicatorName = "ROD(100)";
            int nbLeaders = 12;
            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ShowSplashScreen();

            string htmlLeaders = GenerateLeaderLoserTable(duration, StockSerie.Groups.CACALL, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders * 2);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_A, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.EURO_A, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_B, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.EURO_B, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_C, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.EURO_C, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.COMMODITY, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.COMMODITY, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.FOREX, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.FOREX, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.COUNTRY, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.COUNTRY, imgFolderName);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.FUND, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GererateReportForAlert(alertDefs, StockSerie.Groups.FUND, imgFolderName);
            htmlBody += htmlLeaders;

            StockSplashScreen.CloseForm(true);
            #endregion


            var htmlReport = htmlReportTemplate.Replace("%HTML_TILE%", title).Replace("%HTML_BODY%", htmlBody);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(htmlReport);
            }

            Process.Start(fileName);

            OnSelectedStockChanged(previousStockSerie.StockName, true);
            this.CurrentTheme = previousTheme;
            this.barDurationComboBox.SelectedItem = previousBarDuration.Duration;
            this.barSmoothingComboBox.SelectedItem = previousBarDuration.Smoothing;
            this.barHeikinAshiCheckBox.CheckBox.Checked = previousBarDuration.HeikinAshi;
            this.barLineBreakComboBox.SelectedItem = previousBarDuration.LineBreak;
        }

        private string GererateReportForAlert(List<StockAlertDef> alertDefs, StockSerie.Groups stockGroup, string imgFolder)
        {
            string htmlBody = string.Empty;

            foreach (StockAlertDef alertDef in alertDefs)
            {
                var commentTitle = alertDef.BarDuration + ": " + alertDef.IndicatorName + " => " + alertDef.EventName;

                var alertMsgs = new List<StockAlert>();
                string html = string.Empty;
                var alertMsg = "\r\n<pre>\r\n";
                foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(stockGroup)))
                {
                    StockSplashScreen.ProgressVal++;
                    StockSplashScreen.ProgressSubText = "Scanning " + stockSerie.StockName;

                    if (!stockSerie.Initialise()) continue;

                    StockBarDuration currentBarDuration = stockSerie.BarDuration;

                    if (stockSerie.Count < 200 || (stockSerie.Last().Value.VOLUME * stockSerie.Last().Value.CLOSE) < 10000) continue;

                    if (stockSerie.MatchEvent(alertDef))
                    {
                        var values = stockSerie.GetValues(alertDef.BarDuration);
                        string alertLine = stockSerie.StockName.PadRight(30) + "\t" + values.ElementAt(values.Count - 1).DATE.ToShortDateString();

                        var dailyValue = stockSerie.GetValues(StockBarDuration.Daily).Last();

                        var alert = new StockAlert(alertDef, dailyValue.DATE, stockSerie.StockName, stockSerie.StockGroup.ToString(), dailyValue.CLOSE, dailyValue.VOLUME, 0f);

                        // Generate Snapshot
                        this.OnSelectedStockAndDurationChanged(stockSerie.StockName, alertDef.BarDuration, false);
                        StockAnalyzerForm.MainFrame.SetThemeFromIndicator(alertDef.IndicatorFullName);

                        var bitmapString = this.graphCloseControl.GetSnapshotAsHTML();
                        alertMsg += AlertLineTemplate.Replace("%MSG%", alert.StockName).Replace("%IMG%", bitmapString) + "\r\n";
                    }
                    stockSerie.BarDuration = currentBarDuration;
                }
                alertMsg += "</pre>";
                htmlBody += htmlAlertTemplate.Replace(commentTitleTemplate, commentTitle).Replace(commentTemplate, alertMsg);
            }
            return htmlBody;
        }

        const string AlertLineTemplate = "<a class=\"tooltip\">%MSG%<span><img src=\"%IMG%\"></a>";

        private string GenerateLeaderLoserTable(StockBarDuration duration, StockSerie.Groups reportGroup, string rankLeaderIndicatorName, string rankLoserIndicatorName, int nbLeaders)
        {
            const string rowTemplate = @"
         <tr>
             <td>%COL1%</td>
             <td>%COL2%</td>
             %RANK_DIR_IMG%
             <td>%COL3%</td>
             %CLOSE_DIR_IMG%
             <td>%COL4%</td>
         </tr>";

            string html = @"
        <table>
            <tr>
            <td>";
            try
            {
                List<RankedSerie> leadersDico = new List<RankedSerie>();
                var stockList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(reportGroup)).ToList();

                StockSplashScreen.ProgressVal = 0;
                StockSplashScreen.ProgressMax = stockList.Count();
                StockSplashScreen.ProgressMin = 0;

                foreach (StockSerie stockSerie in stockList)
                {
                    StockSplashScreen.ProgressVal++;
                    StockSplashScreen.ProgressText = "Initializing " + reportGroup + " - " + stockSerie.StockName;
                    if (stockSerie.Initialise() && stockSerie.Count > 100)
                    {
                        stockSerie.BarDuration = duration;
                        var indicatorSerie = stockSerie.GetIndicator(rankLeaderIndicatorName).Series[0];
                        leadersDico.Add(new RankedSerie()
                        {
                            rankIndicatorValue = indicatorSerie.Last,
                            previousRankIndicatorValue = indicatorSerie[indicatorSerie.Count - 2],
                            stockSerie = stockSerie
                        });
                    }
                }
                // Calculate ranks
                int rank = 1;
                foreach (var rankSerie in leadersDico.OrderBy(rs => rs.rankIndicatorValue))
                {
                    rankSerie.rank = rank++;
                }
                rank = 1;
                foreach (var rankSerie in leadersDico.OrderBy(rs => rs.previousRankIndicatorValue))
                {
                    rankSerie.previousRank = rank++;
                }

                var tableHeader = "Leaders for " + reportGroup;
                html += $@"
            <table  class=""reportTable"">
                <thead>
                <tr>
                    <td rowspan=""1"">&nbsp;</td>
                    <th colspan=""6"" scope =""colgroup""> {tableHeader} </th>
                </tr>
                <tr>
                    <th>Stock Name</th>
                    <th>{rankLeaderIndicatorName}</th>
                    <th>Rank Trend</th>
                    <th>Daily %</th>
                    <th>Daily Trend</th>
                    <th>Value</th>
                </tr>
                </thead>
                <tbody>";

                var leaders = leadersDico.OrderByDescending(l => l.rank).Take(nbLeaders);
                foreach (RankedSerie pair in leaders)
                {
                    var lastValue = pair.stockSerie.ValueArray.Last();
                    html += rowTemplate.
                        Replace("%COL1%", pair.stockSerie.StockName).
                        Replace("%COL2%", (pair.rankIndicatorValue).ToString("P2")).
                        Replace("%COL3%", (lastValue.VARIATION).ToString("P2")).
                        Replace("%COL4%", (lastValue.CLOSE).ToString("#.##"));

                    if (pair.previousRank < pair.rank)
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Up"));
                    }
                    else if (pair.previousRank > pair.rank)
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Down"));
                    }
                    else
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Flat"));
                    }
                    if (lastValue.VARIATION > 0)
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Up"));
                    }
                    else if (lastValue.VARIATION < 0)
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Down"));
                    }
                    else
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Flat"));
                    }
                }

                html += @" 
</tbody>
</table>";

                html += "</td><td width=100></td><td>";

                leadersDico.Clear();

                StockSplashScreen.ProgressVal = 0;
                foreach (StockSerie stockSerie in stockList)
                {
                    StockSplashScreen.ProgressVal++;
                    StockSplashScreen.ProgressText = "Initializing " + reportGroup + " - " + stockSerie.StockName;
                    if (stockSerie.Initialise() && stockSerie.Count > 100)
                    {
                        stockSerie.BarDuration = duration;
                        var indicatorSerie = stockSerie.GetIndicator(rankLoserIndicatorName).Series[0];
                        leadersDico.Add(new RankedSerie()
                        {
                            rankIndicatorValue = indicatorSerie.Last,
                            previousRankIndicatorValue = indicatorSerie[indicatorSerie.Count - 2],
                            stockSerie = stockSerie
                        });
                    }
                }
                // Calculate ranks
                rank = 1;
                foreach (var rankSerie in leadersDico.OrderBy(rs => rs.rankIndicatorValue))
                {
                    rankSerie.rank = rank++;
                }
                rank = 1;
                foreach (var rankSerie in leadersDico.OrderBy(rs => rs.previousRankIndicatorValue))
                {
                    rankSerie.previousRank = rank++;
                }

                tableHeader = "Losers for " + reportGroup;
                html += $@"
            <table  class=""reportTable"">
                <thead>
                <tr>
                    <td rowspan=""1"">&nbsp;</td>
                    <th colspan=""6"" scope =""colgroup""> {tableHeader} </th>
                </tr>
                <tr>
                    <th>Stock Name</th>
                    <th>{rankLoserIndicatorName}</th>
                    <th>Rank Trend</th>
                    <th>Daily %</th>
                    <th>Daily Trend</th>
                    <th>Value</th>
                </tr>
                </thead>
                <tbody>";

                leaders = leadersDico.OrderBy(l => l.rank).Take(nbLeaders);
                foreach (RankedSerie pair in leaders)
                {
                    var lastValue = pair.stockSerie.ValueArray.Last();
                    html += rowTemplate.
                        Replace("%COL1%", pair.stockSerie.StockName).
                        Replace("%COL2%", (pair.rankIndicatorValue).ToString("P2")).
                        Replace("%COL3%", (lastValue.VARIATION).ToString("P2")).
                        Replace("%COL4%", (lastValue.CLOSE).ToString("#.##"));

                    if (pair.previousRank > pair.rank)
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Up"));
                    }
                    else if (pair.previousRank < pair.rank)
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Down"));
                    }
                    else
                    {
                        html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Flat"));
                    }
                    if (lastValue.VARIATION > 0)
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Up"));
                    }
                    else if (lastValue.VARIATION < 0)
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Down"));
                    }
                    else
                    {
                        html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "Flat"));
                    }
                }

                html += " </table>";
                html += "</td></tr></table>";


                return html;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractEventsForReport()
        {
            string eventTypeString = string.Empty;
            #region Extract Event
            foreach (GraphControl gc in this.graphList)
            {
                GraphCurveTypeList curveList = gc.CurveList;
                for (int i = this.CurrentStockSerie.Count - 2; i < this.CurrentStockSerie.Count; i++)
                {
                    foreach (IStockIndicator indicator in curveList.Indicators.Where(indic => indic.Events != null))
                    {
                        foreach (BoolSerie eventSerie in indicator.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (eventSerie[i])
                            {
                                eventTypeString += htmlEventTemplate.Replace(eventTemplate, indicator.Name + " - " + eventSerie.Name);
                            }
                        }
                    }
                    // Trail Stops
                    if (curveList.TrailStop != null && curveList.TrailStop.EventCount > 0)
                    {
                        foreach (BoolSerie eventSerie in curveList.TrailStop.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (eventSerie[i])
                            {
                                eventTypeString += htmlEventTemplate.Replace(eventTemplate, curveList.TrailStop.Name + " - " + eventSerie.Name);
                            }
                        }
                    }
                    // Paint Bars
                    if (curveList.PaintBar != null && curveList.PaintBar.EventCount > 0)
                    {
                        int j = 0;
                        foreach (BoolSerie eventSerie in curveList.PaintBar.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (curveList.PaintBar.SerieVisibility[j] && eventSerie[i])
                            {
                                eventTypeString += htmlEventTemplate.Replace(eventTemplate, curveList.PaintBar.Name + " - " + eventSerie.Name);
                            }
                            j++;
                        }
                    }
                    // Decorator
                    if (curveList.Decorator != null && curveList.Decorator.EventCount > 0)
                    {
                        int j = 0;
                        foreach (BoolSerie eventSerie in curveList.Decorator.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (curveList.Decorator.EventVisibility[j] && eventSerie[i])
                            {
                                eventTypeString += htmlEventTemplate.Replace(eventTemplate, curveList.Decorator.Name + " - " + eventSerie.Name);
                            }
                            j++;
                        }
                    }
                }
            }
            #endregion Extract Events
            return eventTypeString;
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
            Directory.CreateDirectory(folderName + "\\Img");
        }

        void addToReportStripBtn_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = File.AppendText(Settings.Default.RootFolder + @"\Report.cfg"))
            {
                sw.WriteLine(this.CurrentStockSerie.StockName + ";" + this.CurrentTheme + ";" + this.barDurationComboBox.SelectedItem.ToString() + ";" + (this.endIndex - this.startIndex));
            }
        }
        private void generateDailyReportToolStripBtn_Click(object sender, EventArgs e)
        {
            GenerateReport("Daily Report", StockBarDuration.Daily, dailyAlertConfig.AlertDefs);
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

        private void scriptEditorMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            ScriptDlg scriptEditor = new ScriptDlg();
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
            if (searchText.Focused) return false;

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
                    case Keys.P:
                        this.currentPortfolioMenuItem_Click(null, null);
                        break;
                    case Keys.Control | Keys.C:
                        ClearSecondarySerie();
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
                    case Keys.Control | Keys.Shift | Keys.F8: // Generate multi time frame trend view.
                        {
                            MTFDlg mtfDlg = new MTFDlg();
                            mtfDlg.MtfControl.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                            mtfDlg.Show();
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
            MultiTimeFrameChartDlg mtg = new MultiTimeFrameChartDlg();
            mtg.Initialize(this.selectedGroup, this.currentStockSerie);
            mtg.WindowState = FormWindowState.Maximized;
            mtg.ShowDialog();
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
                    if (graphControl.GraphRectangle.Contains(e.Location) && e.Location.X > graphControl.GraphRectangle.X)
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
                catch (System.Exception exception)
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
        private string currentTheme;
        public string CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                if (themeComboBox.SelectedItem.ToString() != value)
                {
                    themeComboBox.SelectedItem = value;
                    currentTheme = value;
                }
                else
                {
                    currentTheme = value;
                    if (this.ThemeChanged != null)
                    {
                        this.ThemeChanged(value);
                    }
                }
            }
        }

        public void SetThemeFromIndicator(string fullName)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.themeDictionary.ContainsKey(this.currentTheme) && this.themeDictionary[this.currentTheme].Values.Any(v => v.Any(vv => vv.Contains(fullName))))
                {
                    return;
                }

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(StockViewableItemsManager.GetTheme(fullName))))
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        this.LoadThemeStream(WORK_THEME, sr);
                    }
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
        BestTrendDlg bestrendDlg = null;
        void bestTrendViewMenuItem_Click(object sender, EventArgs e)
        {
            if (bestrendDlg == null)
            {
                bestrendDlg = new BestTrendDlg(this.selectedGroup.ToString(), this.BarDuration);
                bestrendDlg.Disposed += bestrendDialog_Disposed;
                bestrendDlg.bestTrend1.SelectedStockChanged += OnSelectedStockAndDurationAndIndexChanged;
                bestrendDlg.Show();
            }
            else
            {
                bestrendDlg.Activate();
            }
        }

        void bestrendDialog_Disposed(object sender, EventArgs e)
        {
            this.bestrendDlg = null;
        }
        #endregion
        #region Sector Dialog
        SectorDlg sectorDlg = null;
        void sectorViewMenuItem_Click(object sender, EventArgs e)
        {
            if (sectorDlg == null)
            {
                sectorDlg = new SectorDlg(this.selectedGroup.ToString(), this.BarDuration);
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
        ConditionalStatisticsDlg statisticsDlg = null;
        void statisticsMenuItem_Click(object sender, EventArgs e)
        {
            if (statisticsDlg == null)
            {
                statisticsDlg = new ConditionalStatisticsDlg();
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
                expectedValueDlg = new ExpectedValueDlg();
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
                horseRaceDlg = new HorseRaceDlg(this.selectedGroup.ToString(), this.BarDuration);
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
                marketReplayDlg = new MarketReplayDlg(this.selectedGroup, this.BarDuration);
                marketReplayDlg.Disposed += marketReplayDlg_Disposed;
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
        AlertDlg alertDlg = null;
        void showAlertDialogMenuItem_Click(object sender, EventArgs e)
        {
            if (alertDlg == null)
            {
                alertDlg = new AlertDlg(intradayAlertConfig);
                alertDlg.alertControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                alertDlg.Disposed += delegate
                {
                    alertDlg.alertControl1.SelectedStockChanged -= OnSelectedStockAndDurationChanged;
                    this.alertDlg = null;
                };
                alertDlg.Show();
            }
            else
            {
                alertDlg.Activate();
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
            using (MethodLogger ml = new MethodLogger(this))
            {
                StockIndicatorSelectorDlg indicatorSelectorDialog = new StockIndicatorSelectorDlg(this.themeDictionary[this.CurrentTheme]);
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
        Dictionary<string, Dictionary<string, List<string>>> themeDictionary = new Dictionary<string, Dictionary<string, List<string>>>();

        public Dictionary<string, List<string>> GetCurrentTheme()
        {
            if (!this.themeDictionary.ContainsKey(this.CurrentTheme))
                // LoadTheme
                LoadCurveTheme(currentTheme);
            return this.themeDictionary[this.CurrentTheme];
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
                throw new System.Exception("We don't deal with empty themes in this house");
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
        private void ApplyTheme()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                try
                {
                    StockLog.Write($"Apply theme {this.CurrentStockSerie?.StockName}-{this.BarDuration}-{this.CurrentTheme}");
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
                    // Delete transient drawing created by alert Detection
                    if (this.CurrentStockSerie.StockAnalysis.DeleteTransientDrawings() > 0)
                    {
                        this.CurrentStockSerie.ResetIndicatorCache();
                    }

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
                                    this.graphCloseControl.ShowVariation = Settings.Default.ShowVariation;
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
                                        curveList.Add(new GraphCurveType(
                                            CurrentStockSerie.GetSerie(StockDataType.VOLUME),
                                            Pens.Green, true));
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
                                                IStockIndicator stockIndicator =
                                                    (IStockIndicator)
                                                        StockViewableItemsManager.GetViewableItem(line,
                                                            this.CurrentStockSerie);
                                                if (stockIndicator != null)
                                                {
                                                    if (entry.ToUpper() != "CLOSEGRAPH")
                                                    {
                                                        if (stockIndicator.DisplayTarget ==
                                                            IndicatorDisplayTarget.RangedIndicator)
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
                                                    if (
                                                        !(stockIndicator.RequiresVolumeData &&
                                                          !this.CurrentStockSerie.HasVolume))
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
                                                false));
                                    }
                                    if (curveList.FindIndex(c => c.DataSerie.Name == "HIGH") < 0)
                                    {
                                        curveList.Insert(0,
                                            new GraphCurveType(CurrentStockSerie.GetSerie(StockDataType.HIGH), Pens.Black,
                                                false));
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
                            catch (System.Exception e)
                            {
                                StockAnalyzerException.MessageBox(e);
                                StockLog.Write("Exception loading theme: " + this.currentTheme);
                                foreach (string line in this.themeDictionary[currentTheme][entry])
                                {
                                    StockLog.Write(line);
                                }
                                StockLog.Write(e);
                            }
                        }
                    }

                    // Apply Strategy

                    // Create new simulation portofolio
                    if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.BREADTH) ||
                        this.currentStockSerie.BelongsToGroup(StockSerie.Groups.INDICATOR) ||
                        this.currentStockSerie.BelongsToGroup(StockSerie.Groups.NONE))
                    {
                        if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.BREADTH))
                        {
                            string[] fields = this.currentStockSerie.StockName.Split('.');
                            this.graphCloseControl.SecondaryFloatSerie = this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(this.StockDictionary[fields[1]]);
                        }
                    }

                    // Reinitialise drawing
                    ResetZoom();
                    this.Cursor = Cursors.Arrow;
                }

                catch (Exception ex)
                {
                    StockAnalyzerException.MessageBox(ex);
                }
            }
        }

        void portfolioComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.binckPortfolio != portfolioComboBox.SelectedItem)
            {
                this.binckPortfolio = portfolioComboBox.SelectedItem as StockPortfolio;
                this.graphCloseControl.ForceRefresh();
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
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error loading theme");
            }
        }
        private void InitialisePortfolioCombo()
        {
            // Initialise Combo values
            portfolioComboBox.ComboBox.DataSource = BinckPortfolioDataProvider.Portofolios;
            portfolioComboBox.ComboBox.DisplayMember = "Name";
            portfolioComboBox.ComboBox.ValueMember = "Name";
        }
        private void InitialiseThemeCombo()
        {
            // Initialise Combo values
            themeComboBox.Items.Clear();

            string folderName = Settings.Default.RootFolder + @"\themes";
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
                    using (StreamWriter tw = new StreamWriter(folderName + @"\\" + Localisation.UltimateChartistStrings.ThemeEmpty + ".thm"))
                    {
                        tw.Write(emptyTheme);
                    }
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
            using (StreamWriter sr = new StreamWriter(fileName))
            {
                foreach (string entry in themeDictionary[this.CurrentTheme].Keys)
                {
                    sr.WriteLine("#" + entry);
                    foreach (string line in themeDictionary[this.CurrentTheme][entry])
                    {
                        sr.WriteLine(line);
                    }
                }
            }
        }
        private void LoadCurveTheme(string themeName)
        {
            try
            {
                // Load Curve Theme
                string fileName = Settings.Default.RootFolder + @"\themes\" + themeName + ".thm";
                using (StreamReader sr = new StreamReader(fileName))
                {
                    LoadThemeStream(themeName, sr);
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Application Error loading theme", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            SaveAnalysis(Settings.Default.AnalysisFile);
        }
        void deleteThemeStripButton_Click(object sender, EventArgs e)
        {
            if (this.CurrentTheme == WORK_THEME)
            {
                return;
            }

            // delete theme file
            string fileName = Settings.Default.RootFolder + @"\themes\" + this.CurrentTheme + ".thm";
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
            if (!Enum.TryParse<AgendaEntryType>(Settings.Default.ShowAgenda, out agendaEntryType))
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

            string folderName = Settings.Default.RootFolder;
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

                Settings.Default.AnalysisFile = analysisFileName;
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

            this.SaveAnalysis(Settings.Default.AnalysisFile);
        }
        private void saveAnalysisFileAsMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "ulc";
            saveFileDialog.Filter = "Ultimate Chartist Analysis files (*.ulc)|*.ulc";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.InitialDirectory = Settings.Default.RootFolder;
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string analysisFileName = saveFileDialog.FileName;
                this.SaveAnalysis(analysisFileName);
                Settings.Default.AnalysisFile = analysisFileName;
                Settings.Default.Save();
            }
        }
        private void saveThemeMenuItem_Click(object sender, EventArgs e)
        {
            string folderName = Settings.Default.RootFolder + @"\themes";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            List<string> themeList = new List<string>();
            foreach (object theme in this.themeComboBox.Items)
            {
                themeList.Add(theme.ToString());
            }
            themeList.Sort();

            SaveThemeForm saveThemeForm = new SaveThemeForm(themeList);
            if (saveThemeForm.ShowDialog() == DialogResult.OK)
            {
                SaveCurveTheme(folderName + @"\" + saveThemeForm.Theme + ".thm");
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
        private void folderPrefMenuItem_Click(object sender, EventArgs e)
        {
            PreferenceDialog prefDlg = new PreferenceDialog();
            prefDlg.ShowDialog();

            this.graphCloseControl.ShowVariation = Settings.Default.ShowVariation;

            OnNeedReinitialise(true);
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

        internal void OpenInABCMenu()
        {
            if (string.IsNullOrWhiteSpace(this.currentStockSerie.ABCName))
                return;
            string url = $"https://www.abcbourse.com/graphes/display.aspx?s={this.currentStockSerie.ABCName}";
            Process.Start(url);
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

            return;
            var htmlReport = File.ReadAllText(PEAPerfTemplatePath)
                .Replace("%ISIN%", this.currentStockSerie.ISIN)
                .Replace("%STOCKNAME%", this.currentStockSerie.StockName)
                .Replace("%SYMBOL%", symbol);

            string folderName = Settings.Default.RootFolder + @"\CommentReport\";

            string fileName = folderName + @"\PEAPerfReport.html";
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(htmlReport);
            }
            Process.Start(fileName);
        }
        internal void OpenInZBMenu()
        {
            if (string.IsNullOrWhiteSpace(this.currentStockSerie.ISIN))
                return;
            string url = "https://www.zonebourse.com/recherche/?mots=%ISIN%";
            url = url.Replace("%ISIN%", this.currentStockSerie.ISIN);
            Process.Start(url);
        }
        internal void OpenInSocGenMenu()
        {
            if (!this.currentStockSerie.StockName.StartsWith("TURBO_"))
                return;
            string url = $"https://sgbourse.fr/product-detail?productId={this.currentStockSerie.Ticker}";
            Process.Start(url);
        }
    }
}