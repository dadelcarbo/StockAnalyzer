using StockAnalyzer;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockStatistic;
using StockAnalyzer.StockClasses.StockStatistic.MatchPatterns;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockSecurity;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockWeb;
using StockAnalyzerApp.CustomControl;
using StockAnalyzerApp.CustomControl.AgendaDlg;
using StockAnalyzerApp.CustomControl.AlertDialog;
using StockAnalyzerApp.CustomControl.ExpectedValueDlg;
using StockAnalyzerApp.CustomControl.FinancialDlg;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.CustomControl.GroupViewDlg;
using StockAnalyzerApp.CustomControl.HorseRaceDlgs;
using StockAnalyzerApp.CustomControl.IndicatorDlgs;
using StockAnalyzerApp.CustomControl.MultiTimeFrameDlg;
using StockAnalyzerApp.CustomControl.PortofolioDlgs;
using StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager;
using StockAnalyzerApp.CustomControl.SimulationDlgs;
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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace StockAnalyzerApp
{
    public partial class StockAnalyzerForm : Form
    {
        public delegate void SelectedStockSerieChangedEventHandler(object sender, SelectedStockSerieChangedEventArgs args);
        public event SelectedStockSerieChangedEventHandler OnSelectStockSerieChange;

        public delegate void SelectedStockChangedEventHandler(string stockName, bool activateMainWindow);
        public delegate void SelectedStockAndDurationChangedEventHandler(string stockName, StockBarDuration barDuration, bool activateMainWindow);

        public delegate void SelectedStockGroupChangedEventHandler(string stockgroup);

        public delegate void SelectedStrategyChangedEventHandler(string strategyName);

        public delegate void NotifySelectedThemeChangedEventHandler(Dictionary<string, List<string>> theme);

        public delegate void NotifyBarDurationChangedEventHandler(StockBarDuration barDuration);

        public delegate void NotifyStrategyChangedEventHandler(string newStrategy);

        public delegate void SelectedPortofolioChangedEventHandler(StockPortofolio portofolio, bool activateMainWindow);

        public delegate void SelectedPortofolioNameChangedEventHandler(string portofolioName, bool activateMainWindow);

        public delegate void SimulationCompletedEventHandler(SimulationParameterControl simulationParameterControl);

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

        public static CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-GB");
        public static CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        public static CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public StockDictionary StockDictionary { get; private set; }
        public SortedDictionary<StockSerie.Groups, StockSerie> GroupReference { get; private set; }

        public StockPortofolioList StockPortofolioList
        {
            get
            {
                return PortfolioDataProvider.StockPortofolioList;
            }
        }

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
            get { return new StockBarDuration((BarDuration)this.barDurationComboBox.SelectedItem); }
        }

        public StockPortofolio currentPortofolio;

        public StockPortofolio CurrentPortofolio
        {
            get { return currentPortofolio; }
            set
            {
                if (this.currentPortofolio != value)
                {
                    this.currentPortofolio = value;
                    this.graphCloseControl.Portofolio = currentPortofolio;
                    if (currentPortofolio != null) OnNeedReinitialise(false);
                }
            }
        }

        private const int MARGIN_SIZE = 20;
        private const int MOUSE_MARQUEE_SIZE = 4;
        private StockSerie.Groups selectedGroup;

        private PalmaresDlg palmaresDlg = null;
        private PortofolioDlg portofolioDlg = null;
        private OrderListDlg orderListDlg = null;
        private StrategySimulatorDlg strategySimulatorDlg = null;
        private FilteredStrategySimulatorDlg filteredStrategySimulatorDlg = null;
        private OrderGenerationDlg orderGenerationDlg = null;
        private BatchStrategySimulatorDlg batchStrategySimulatorDlg = null;

        private PortfolioSimulatorDlg portfolioSimulatorDlg = null;

        private static int NbBars { get; set; }

        private int startIndex = 0;
        private int endIndex = 0;

        private List<GraphControl> graphList = new List<GraphControl>();

        #region CONSTANTS

        private static string WORK_THEME = "NewTheme*";
        //static private string COT_SUBFOLDER = @"\data\weekly\cot";
        //static private string COT_ARCHIVE_SUBFOLDER = @"\data\archive\weekly\cot";
        private static string INTRADAY_SUBFOLDER = @"\data\intraday";
        private static string DAILY_SUBFOLDER = @"\data\daily";
        private static string ABC_SUBFOLDER = DAILY_SUBFOLDER + @"\ABC";
        private static string YAHOO_SUBFOLDER = DAILY_SUBFOLDER + @"\Yahoo";
        private static string CBOE_SUBFOLDER = DAILY_SUBFOLDER + @"\CBOE";
        private static string GENERATED_SUBFOLDER = DAILY_SUBFOLDER + @"\Generated";
        private static string BREADTH_SUBFOLDER = DAILY_SUBFOLDER + @"\Breadth";
        private static string POSITION_SUBFOLDER = DAILY_SUBFOLDER + @"\Variation";
        private static string ARCHIVE_DAILY_SUBFOLDER = @"\data\archive\daily";
        private static string ARCHIVE_GENERATED_SUBFOLDER = ARCHIVE_DAILY_SUBFOLDER + @"\Generated";
        private static string ARCHIVE_BREADTH_SUBFOLDER = ARCHIVE_DAILY_SUBFOLDER + @"\Breadth";
        private static string ARCHIVE_POSITION_SUBFOLDER = ARCHIVE_DAILY_SUBFOLDER + @"\Variation";

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

            //this.SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

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

        protected override void OnShown(EventArgs e)
        {
            // Validate preferences and local repository
            while (string.IsNullOrWhiteSpace(Settings.Default.UserId) || !CheckLicense())
            {
                PreferenceDialog prefDlg = new PreferenceDialog();
                System.Windows.Forms.DialogResult res = prefDlg.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
            }

            base.OnActivated(e);

            // Enable timers and multithreading
            busy = false;
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

            // Validate preferences and local repository
            if (string.IsNullOrWhiteSpace(Settings.Default.UserId) || !CheckLicense())
            {
                StockSplashScreen.CloseForm(true);
                return;
            }

            // Parse Yahoo market data
            StockSplashScreen.ProgressText = "Initialize stock dictionary...";
            StockSplashScreen.ProgressVal = 30;

            StockDataProviderBase.InitStockDictionary(stockRootFolder, this.StockDictionary,
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
                StockSplashScreen.ProgressText = "Reading Drawing items...";
                LoadAnalysis(Settings.Default.AnalysisFile);
            }

            //if (this.StockDictionary.ContainsKey("SP500"))
            //{
            //    StockSerie cashSerie = this.StockDictionary["SP500"].GenerateCashStockSerie();
            //    this.StockDictionary.Add(cashSerie.StockName, cashSerie);
            //}

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
                    foreach (
                        StockSerie stockserie in
                        this.StockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.Breadth))
                    {
                        StockSplashScreen.ProgressText = "Generating breadth data " + stockserie.StockName;
                        stockserie.Initialise();
                    }
                }

                // Generate Vix Premium
                //StockSplashScreen.ProgressText = "Generating VIX Premium data...";
                //GenerateVixPremium();

                this.GroupReference = new SortedDictionary<StockSerie.Groups, StockSerie>();
                this.GroupReference.Add(StockSerie.Groups.CAC40, this.StockDictionary["CAC40"]);

                //GeneratePosition(new List<StockSerie.Groups> { StockSerie.Groups.CAC40 });

                #region Test Automatic Indices

                //GenerateCACEqualWeight();

                //GenerateIndexNoDay("CAC40", DayOfWeek.Monday);
                //GenerateIndexNoDay("CAC40", DayOfWeek.Tuesday);
                //GenerateIndexNoDay("CAC40", DayOfWeek.Wednesday);
                //GenerateIndexNoDay("CAC40", DayOfWeek.Thursday);
                //GenerateIndexNoDay("CAC40", DayOfWeek.Friday);
                //GenerateIndexNoDay("EUR-USD", DayOfWeek.Monday);
                //GenerateIndexNoDay("EUR-USD", DayOfWeek.Tuesday);
                //GenerateIndexNoDay("EUR-USD", DayOfWeek.Wednesday);
                //GenerateIndexNoDay("EUR-USD", DayOfWeek.Thursday);
                //GenerateIndexNoDay("EUR-USD", DayOfWeek.Friday);

                //GenerateIndexOnlyDay("CAC40", DayOfWeek.Monday);
                //GenerateIndexOnlyDay("CAC40", DayOfWeek.Tuesday);
                //GenerateIndexOnlyDay("CAC40", DayOfWeek.Wednesday);
                //GenerateIndexOnlyDay("CAC40", DayOfWeek.Thursday);
                //GenerateIndexOnlyDay("CAC40", DayOfWeek.Friday);

                //GenerateCACEqualWeightNoUpDay(DayOfWeek.Monday);
                //GenerateCACEqualWeightNoUpDay(DayOfWeek.Tuesday);
                //GenerateCACEqualWeightNoUpDay(DayOfWeek.Wednesday);
                //GenerateCACEqualWeightNoUpDay(DayOfWeek.Thursday);
                //GenerateCACEqualWeightNoUpDay(DayOfWeek.Friday);

                //GenerateCACEqualWeightNoUpDay();

                //GenerateCACEqualWeightNoDownDay(DayOfWeek.Monday);
                //GenerateCACEqualWeightNoDownDay(DayOfWeek.Tuesday);
                //GenerateCACEqualWeightNoDownDay(DayOfWeek.Wednesday);
                //GenerateCACEqualWeightNoDownDay(DayOfWeek.Thursday);
                //GenerateCACEqualWeightNoDownDay(DayOfWeek.Friday);

                //GenerateCACEqualWeightNoDownDay();

                //GenerateCACEqualWeightOnlyDay(DayOfWeek.Monday);
                //GenerateCACEqualWeightOnlyDay(DayOfWeek.Tuesday);
                //GenerateCACEqualWeightOnlyDay(DayOfWeek.Wednesday);
                //GenerateCACEqualWeightOnlyDay(DayOfWeek.Thursday);
                //GenerateCACEqualWeightOnlyDay(DayOfWeek.Friday);

                for (int i = 2; i < 40; i += 1)
                {
                    //GenerateIndex_Event("CAC40", "EMA_", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("CAC40", "EMA_INV", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("CAC40", "HLAVG_", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("CAC40", "HLAVG_INV", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("CAC40", "EMA_", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("CAC40", "EMA_INV", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("CAC40", "HLAVG_", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("CAC40", "HLAVG_INV", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");

                    //GenerateIndex_Event("SP500", "EMA_", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("SP500", "EMA_INV", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("SP500", "HLAVG_", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("SP500", "HLAVG_INV", StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("SP500", "EMA_", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("SP500", "EMA_INV", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
                    //GenerateIndex_Event("SP500", "HLAVG_", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
                    //GenerateIndex_Event("SP500", "HLAVG_INV", StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
                }
                StockSplashScreen.ProgressText = "Generating CAC SAR...";
                for (float i = 0.001f; i <= 0.1f; i += 0.002f)
                {
                    StockSplashScreen.ProgressText = "Generating CAC TOPSAR_" + i + " Daily...";
                    //GenerateCAC_Event("CAC40", StockBarDuration.Daily, i/15f, "INDICATOR|HLSAR(0,%PERIOD%,5,6)", "Bullish", false);
                }

                StockSplashScreen.ProgressText = "Generating CAC Random...";
                //GenerateCAC_Random();

                StockSplashScreen.ProgressText = "Generating CAC RANK_" + 3 + " Daily...";
                //GenerateCAC_Event("CAC_RANK_", StockBarDuration.Daily, 3, "INDICATOR|RANK(%PERIOD%,10,20,0)", "Overbought");

                for (int i = 10; i <= 500; i += 5)
                {
                    StockSplashScreen.ProgressText = "Generating CAC CCIEX_" + i + " Daily...";
                    //GenerateCAC_Event("CAC40", StockBarDuration.Daily, i, "INDICATOR|STOKF(%PERIOD%,1,61.8,38.2)", "Overbought",false);
                }
                for (int i = 15; i <= 40; i += 1)
                {
                    for (int j = 40; j <= 60; j++)
                    {
                        StockSplashScreen.ProgressText = "Generating SRD STOCK" + i + " Daily...";
                        //  GenerateIndex_Event("CAC40", "STOCK", StockBarDuration.Daily, i, j, "INDICATOR|STOKF(%PERIOD1%,1,%PERIOD2%,38.2)", "Overbought");
                    }
                }
                for (int i = 2; i <= 50; i += 1)
                {
                    //StockSplashScreen.ProgressText = "Generating CAC EMA_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_TRAILHMA_UP_", StockBarDuration.Daily, i, "TRAILSTOP|TRAILHMA(%PERIOD%,1)", "UpTrend", false);
                    //StockSplashScreen.ProgressText = "Generating CAC EMA_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_TRAILHMA_DOWN_", StockBarDuration.Daily, i, "TRAILSTOP|TRAILHMA(%PERIOD%,1)", "DownTrend", false);
                }
                int period = 11;
                int smoothing = 7;
                for (int i = 1; i <= period; i++)
                {
                    for (int j = 1; j <= smoothing; j++, j++)
                    {
                        //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily...";
                        //GenerateCAC_Event("CAC_TRAILHLS_UP_", StockBarDuration.Daily, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                        //GenerateCAC_Event("CAC_TRAILHLS_DOWN_", StockBarDuration.Daily, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "DownTrend", false);
                        //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_3...";
                        //GenerateCAC_Event("CAC_TRAILHLS_", StockBarDuration.Daily_EMA3, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                        //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_6...";
                        //GenerateCAC_Event("CAC_TRAILHLS_", StockBarDuration.Daily_EMA6, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                        //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_9...";
                        //GenerateCAC_Event("CAC_TRAILHLS_", StockBarDuration.Daily_EMA9, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                        //StockSplashScreen.ProgressText = "Generating CAC TRAILEMA_" + i + "," + j + " TLB...";
                        //GenerateCAC_Event("CAC_TRAILHLS_", StockBarDuration.TLB, i, j,
                        //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                    }
                }
                //for (int i = 90; i <= 100; i += 10)
                //{
                //   StockSplashScreen.ProgressText = "Generating CAC TRAILEMA_" + i + " Daily...";
                //   GenerateCAC2_Event("CAC_TRAILEMA_TLB_", StockBarDuration.Daily, i, "TRAILSTOP|TRAILEMA(%PERIOD%,%PERIOD%)", "UpTrend", true);
                //} 
                //for (int i = 10; i <= 500; i += 5)
                //{
                //   StockSplashScreen.ProgressText = "Generating CAC HMA_" + i + " Daily...";
                //   GenerateCAC_Event("CAC_HMA_", StockBarDuration.Daily, i, "INDICATOR|HMA(%PERIOD%)", "PriceAbove");
                //   GenerateCAC_Event("CAC_HMA_", StockBarDuration.Bar_1_EMA3, i, "INDICATOR|HMA(%PERIOD%)", "PriceAbove");
                //}
                for (int i = 1; i <= 400; i++)
                {
                    //StockSplashScreen.ProgressText = "Generating CAC PUKE_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Daily, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB, i, "INDICATOR|PUKE(%PERIOD%,3,20,3)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                }
                for (int i = 30; i <= 230; i += 5)
                {
                    for (int j = 1; j < 20; j += 2)
                    {
                        //StockSplashScreen.ProgressText = "Generating CAC RSI_" + i + "_" + j + " Daily...";
                        //GenerateCAC_Event("CAC_RSI", StockBarDuration.Daily, i, j, "INDICATOR|RSI(%PERIOD1%,50,50,%PERIOD2%)", "Overbought", true);
                    }
                }
                for (int i = 10; i <= 60; i += 10)
                {
                    for (int j = 2; j < 6; j += 1)
                    {
                        //StockSplashScreen.ProgressText = "Generating CAC ER_" + i + "_" + j + " Daily...";
                        //GenerateCAC_Event("CAC_ER", StockBarDuration.Daily, i, j, "TRAIL|SAR(0,0.0005)|ER(%PERIOD1%,%PERIOD2%,1)", "UpTrend", false);
                        //GenerateCAC_Event("CAC_ER_BREADTH", StockBarDuration.Daily, i, j, "TRAIL|SAR(0,0.0005)|ER(%PERIOD1%,%PERIOD2%,1)", "UpTrend", true);
                    }
                }
                for (int i = 5; i <= 200; i += 5)
                {
                    //StockSplashScreen.ProgressText = "Generating CAC MACD_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_MACD_", StockBarDuration.Daily, i+1, i, "INDICATOR|MACD(%PERIOD1%,%PERIOD2%,2)", "MACDAboveSignal");
                    //GenerateCAC_Event("CAC_MACD_", StockBarDuration.Daily, i, i/2, "INDICATOR|MACD(%PERIOD1%,%PERIOD2%,2)", "MACDAboveSignal");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB, i, "INDICATOR|PUKE(%PERIOD%,3,20,3)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                }
                for (int i = 1; i <= 20; i++)
                {
                    //StockSplashScreen.ProgressText = "Generating CAC TL_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Daily, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_TL_", StockBarDuration.Daily, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
                    //GenerateCAC_Event("CAC_TL_", StockBarDuration.Bar_1_EMA3, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
                    //GenerateCAC_Event("CAC_TL_", StockBarDuration.TLB_EMA3, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
                    //GenerateCAC_Event("CAC_TL_", StockBarDuration.TLB, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                    //GenerateCAC_Event("CAC_PUKE_", StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
                }
                for (int i = 1; i <= 51; i += 10)
                {
                    //StockSplashScreen.ProgressText = "Generating CAC TRAILHL_" + i + " Daily...";
                    //GenerateCAC_Event("CAC_HL_", StockBarDuration.Daily, i, "TRAILSTOP|TRAILHL(%PERIOD%)", "UpTrend", false);
                    //GenerateCAC_Event("CAC_HL_", StockBarDuration.Bar_1_EMA6, i, "TRAILSTOP|TRAILHL(%PERIOD%)", "UpTrend", false);
                }

                string best = string.Empty;
                float max = float.MinValue;
                foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(StockSerie.Groups.INDICES_CALC)))
                {
                    if (stockSerie.Initialise() && stockSerie.Values.Last().CLOSE > max)
                    {
                        max = stockSerie.Values.Last().CLOSE;
                        best = stockSerie.StockName;
                    }
                }
                StockLog.Write("Best index " + best);
                #endregion

            }
            // Deserialize saved orders
            StockSplashScreen.ProgressText = "Reading portofolio data...";
            ReadPortofolios();

            // Initialise dico
            StockSplashScreen.ProgressText = "Initialising menu items...";

            // Create Groups menu items
            CreateGroupMenuItem();

            CreateAgendaMenuItem();

            // Update dynamic menu
            InitialiseBarDurationComboBox();
            CreateRelativeStrengthMenuItem();
            CreateSecondarySerieMenuItem();

            // Update dynamic menu
            InitDataProviderMenuItem();

            // Watchlist menu item
            this.LoadWatchList();

            // 
            InitialiseStockCombo(true);

            //
            InitialiseStrategyCombo();

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
            //if (this.CurrentStockSerie.StockName.StartsWith("INT_"))
            //{
            //    this.barDurationComboBox.SelectedItem = StockBarDuration.TLB_6D;
            //}
            //if (this.CurrentStockSerie.StockName.StartsWith("FUT_"))
            //{
            //    this.barDurationComboBox.SelectedItem = StockBarDuration.TLB_3D_EMA3;
            //}
            //else
            //{
            //    this.barDurationComboBox.SelectedItem = StockBarDuration.Daily;
            //}

            SetDurationForStockGroup(this.CurrentStockSerie.StockGroup);
            this.StockAnalyzerForm_StockSerieChanged(this.CurrentStockSerie, false);

            // Initialise event call backs (because of a bug in the designer)
            this.graphCloseControl.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphCloseControl.GraphControl_MouseClick);
            this.graphScrollerControl.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphScrollerControl.GraphControl_MouseClick);
            this.graphIndicator2Control.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphIndicator2Control.GraphControl_MouseClick);
            this.graphIndicator3Control.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphIndicator3Control.GraphControl_MouseClick);
            this.graphIndicator1Control.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphIndicator1Control.GraphControl_MouseClick);
            this.graphVolumeControl.MouseClick +=
                new System.Windows.Forms.MouseEventHandler(graphVolumeControl.GraphControl_MouseClick);

            // Refreshes intraday every 2 minutes.
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Interval = 120 * 1000;
            refreshTimer.Start();

            #region DailyAlerts

            if (!dailyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(dailyAlertConfig.AlertDefs, dailyAlertConfig.AlertLog);
            }

            if (!weeklyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(weeklyAlertConfig.AlertDefs, weeklyAlertConfig.AlertLog);
            }

            if (!monthlyAlertConfig.AlertLog.IsUpToDate(DateTime.Today.AddDays(-1)))
            {
                GenerateAlert(monthlyAlertConfig.AlertDefs, monthlyAlertConfig.AlertLog);
            }
            #endregion

            if (Settings.Default.GenerateDailyReport)
            {
                // Daily report
                string fileName = Settings.Default.RootFolder + @"\CommentReport\Daily\Report.html";
                if (!File.Exists(fileName) || File.GetLastWriteTime(fileName).Date != DateTime.Today)
                {
                    var durations = new StockBarDuration[]
                         {
                            StockBarDuration.Daily,
                            StockBarDuration.TLB,
                            StockBarDuration.TLB_3D
                         };

                    GenerateReport("Daily Report", durations, dailyAlertConfig.AlertDefs);
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
                        var durations = new StockBarDuration[]
                             {
                            StockBarDuration.Weekly
                             };

                        GenerateReport("Weekly Report", durations, weeklyAlertConfig.AlertDefs);
                    }
                }

                fileName = Settings.Default.RootFolder + @"\CommentReport\Monthly\Report.html";
                lastUpdate = File.GetLastWriteTime(fileName).Date;
                if (!File.Exists(fileName) || lastUpdate.Month != DateTime.Today.Month)
                {
                    var durations = new StockBarDuration[]
                         {
                            StockBarDuration.Monthly
                             };

                    GenerateReport("Montly Report", durations, monthlyAlertConfig.AlertDefs);
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

                string fileName = Path.GetTempPath() + "AlertLog.xml";
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
            allowedTypes.AddRange(this.StockDictionary.Select(p => p.Key.ToUpper()).ToArray());
            searchText.AutoCompleteCustomSource = allowedTypes;
            searchText.AutoCompleteMode = AutoCompleteMode.Suggest;
            searchText.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Ready to start
            StockSplashScreen.CloseForm(true);
            this.Focus();
        }

        private void goBtn_Click(object sender, System.EventArgs e)
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
                System.Windows.Forms.ToolStripItem[] watchListMenuItems =
                   new System.Windows.Forms.ToolStripItem[this.WatchLists.Count()];
                System.Windows.Forms.ToolStripItem[] addToWatchListMenuItems =
                   new System.Windows.Forms.ToolStripItem[this.WatchLists.Count()];
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

        #region TIMER MANAGEMENT


        public static bool busy = false;

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (busy) return;
            busy = true;

            try
            {
                if (this.currentStockSerie != null &&
                    (this.currentStockSerie.StockGroup == StockSerie.Groups.INTRADAY ||
                     this.currentStockSerie.StockGroup == StockSerie.Groups.TURBO ||
                     this.currentStockSerie.StockGroup == StockSerie.Groups.FUTURE))
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, this.currentStockSerie))
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

        private StockAlertConfig intradayAlertConfig = StockAlertConfig.GetConfig("Intraday");
        private StockAlertConfig dailyAlertConfig = StockAlertConfig.GetConfig("Daily");
        private StockAlertConfig weeklyAlertConfig = StockAlertConfig.GetConfig("Weekly");
        private StockAlertConfig monthlyAlertConfig = StockAlertConfig.GetConfig("Monthly");

        private void alertTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday ||
                DateTime.Now.Hour < 8 || DateTime.Now.Hour > 18) return;

            if (this.intradayAlertConfig == null || this.intradayAlertConfig.AlertDefs.Count == 0) return;

            Thread alertThread = new Thread(GenerateIntradayAlert) { Name = "alertTimer" };
            alertThread.Start();
        }
        public void GenerateIntradayAlert()
        {
            if (busy) return;
            busy = true;

            try
            {
                bool newAlert = false;
                string alertString = string.Empty;

                var stockList = this.WatchLists.Find(wl => wl.Name == "Alert").StockList;
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

                DateTime lookBackDate = DateTime.Today.AddDays(-7);

                foreach (string stockName in stockList)
                {
                    if (AlertDetectionProgress != null)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(this.AlertDetectionProgress, stockName);
                        }
                        else
                        {
                            this.AlertDetectionProgress(stockName);
                        }
                    }
                    if (!this.StockDictionary.ContainsKey(stockName)) continue;

                    StockSerie stockSerie = this.StockDictionary[stockName];
                    StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, stockSerie);

                    if (!stockSerie.Initialise()) continue;

                    StockBarDuration previouBarDuration = stockSerie.BarDuration;

                    lock (intradayAlertConfig)
                    {
                        foreach (var alertDef in intradayAlertConfig.AlertDefs)
                        {
                            stockSerie.BarDuration = alertDef.BarDuration;
                            var values = stockSerie.GetValues(alertDef.BarDuration);
                            for (int i = values.Count - 2; i > 0 && values[i].DATE > lookBackDate; i--)
                            {
                                if (stockSerie.MatchEvent(alertDef, i))
                                {
                                    var dailyValue = values.ElementAt(i + 1);
                                    var stockAlert = new StockAlert(alertDef,
                                        dailyValue.DATE,
                                        stockSerie.StockName,
                                        stockSerie.StockGroup.ToString(),
                                        dailyValue.OPEN,
                                        dailyValue.VOLUME);

                                    if (intradayAlertConfig.AlertLog.Alerts.All(a => a != stockAlert))
                                    {
                                        alertString += stockAlert.ToString() + Environment.NewLine;
                                        if (this.InvokeRequired)
                                        {
                                            this.Invoke(new Action(() => intradayAlertConfig.AlertLog.Alerts.Insert(0, stockAlert)));
                                        }
                                        else
                                        {
                                            intradayAlertConfig.AlertLog.Alerts.Insert(0, stockAlert);
                                        }
                                        newAlert = true;
                                    }
                                }
                            }
                        }
                    }
                    stockSerie.BarDuration = previouBarDuration;
                }
                intradayAlertConfig.AlertLog.Save();

                if (newAlert && !string.IsNullOrWhiteSpace(alertString) && !string.IsNullOrWhiteSpace(Settings.Default.UserSMTP) && !string.IsNullOrWhiteSpace(Settings.Default.UserEMail))
                {
                    StockMail.SendEmail("Ultimate Chartist - Intraday Alert", alertString);
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
            }
        }
        public void GenerateAlert_Thread(object param)
        {
            var tuple = (Tuple<List<StockAlertDef>, StockAlertLog>)param;
            if (tuple != null)
            {
                this.GenerateAlert(tuple.Item1, tuple.Item2);
            }
        }
        public void GenerateAlert(List<StockAlertDef> alertDefs, StockAlertLog alertLog)
        {
            if (busy || alertDefs == null) return;
            busy = true;

            try
            {
                string alertString = string.Empty;

                var stockList = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(StockSerie.Groups.CACALL)).ToList();

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

                    if (!stockSerie.Initialise()) continue;

                    StockBarDuration previouBarDuration = stockSerie.BarDuration;

                    lock (alertDefs)
                    {
                        foreach (var alertDef in alertDefs)
                        {
                            stockSerie.BarDuration = alertDef.BarDuration;
                            var values = stockSerie.GetValues(alertDef.BarDuration);

                            int stopIndex = Math.Max(10, stockSerie.LastCompleteIndex - 10);
                            for (int i = stockSerie.LastCompleteIndex; i > stopIndex; i--)
                            {
                                var dailyValue = values.ElementAt(i);
                                if (stockSerie.MatchEvent(alertDef, i))
                                {
                                    StockAlert stockAlert = new StockAlert(alertDef,
                                        dailyValue.DATE,
                                        stockSerie.StockName,
                                        stockSerie.StockGroup.ToString(),
                                        dailyValue.CLOSE,
                                        dailyValue.VOLUME);

                                    if (alertLog.Alerts.All(a => a != stockAlert))
                                    {
                                        alertString += stockAlert.ToString() + Environment.NewLine;
                                        if (this.InvokeRequired)
                                        {
                                            this.Invoke(new Action(() => alertLog.Alerts.Insert(0, stockAlert)));
                                        }
                                        else
                                        {
                                            alertLog.Alerts.Insert(0, stockAlert);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    stockSerie.BarDuration = previouBarDuration;
                }
                alertLog.Save();

                if (!string.IsNullOrWhiteSpace(alertString) && !string.IsNullOrWhiteSpace(Settings.Default.UserSMTP) && !string.IsNullOrWhiteSpace(Settings.Default.UserEMail))
                {
                    StockMail.SendEmail("Ultimate Chartist - " + alertLog.FileName.Replace("AlertLog", "").Replace(".xml", "") + " Alert", alertString);
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
            }
        }
        #endregion

        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.Timer alertTimer;

        private bool CheckLicense()
        {
            return true;
            StockLicense stockLicense = null;

            // Check on local disk in license is found
            string licenseFileName = Settings.Default.RootFolder + @"\license.dat";
            if (File.Exists(licenseFileName))
            {
                string fileName = licenseFileName;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    try
                    {
                        stockLicense = new StockLicense(Settings.Default.UserId, sr.ReadLine());
                    }
                    catch
                    {
                        this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseCorrupted);
                        return false;
                    }
                    if (stockLicense.UserID != Settings.Default.UserId)
                    {
                        this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseInvalidUserId);
                        return false;
                    }
                    if (Settings.Default.MachineID == string.Empty)
                    {
                        Settings.Default.MachineID = StockToolKit.GetMachineUID();
                        Settings.Default.Save();
                    }
                    if (stockLicense.MachineID != Settings.Default.MachineID)
                    {
                        this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseInvalidMachineId);
                        return false;
                    }
                    if (stockLicense.ExpiryDate < DateTime.Today)
                    {
                        this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseExpired);
                        return false;
                    }
                    if (Assembly.GetExecutingAssembly().GetName().Version.Major > stockLicense.MajorVerion)
                    {
                        this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseWrongVersion);
                        return false;
                    }
                }
            }
            else
            {
                this.DeactivateGraphControls(Localisation.UltimateChartistStrings.LicenseNoFile);
            }
            return true;
        }

        private void graphScrollerControl_ZoomChanged(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        #endregion

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
            NbBars = Math.Max(25, NbBars / 2);
            int newIndex = Math.Max(0, endIndex - NbBars);
            if (newIndex != this.startIndex)
            {
                this.ChangeZoom(newIndex, endIndex);
            }
        }

        private void ZoomOut()
        {
            NbBars = Math.Min(this.endIndex, NbBars * 2);
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
            ResetZoom();
        }

        #endregion

        public void OnSelectedStockChanged(string stockName, bool activate)
        {
            if (stockName.EndsWith("_P") || stockName == StockPortofolio.SIMULATION)
            {
                this.CurrentPortofolio = this.StockPortofolioList.First(p => p.Name == stockName);
                if (this.StockDictionary.ContainsKey(stockName))
                {
                    this.StockDictionary.Remove(stockName);
                }
                this.StockDictionary.CreatePortofolioSerie(this.CurrentPortofolio);
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    this.stockNameComboBox.Items.Insert(
                       this.stockNameComboBox.Items.IndexOf(stockName.Replace("_P", "")) + 1, stockName);
                }

                this.barDurationComboBox.SelectedItem = StockBarDuration.Daily;
            }
            if (!this.stockNameComboBox.Items.Contains(stockName))
            {
                this.stockNameComboBox.Items.Add(stockName);
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
        public void OnSelectedStockAndDurationChanged(string stockName, StockBarDuration barDuration, bool activate)
        {
            if (stockName.EndsWith("_P") || stockName == StockPortofolio.SIMULATION)
            {
                this.CurrentPortofolio = this.StockPortofolioList.First(p => p.Name == stockName);
                if (this.StockDictionary.ContainsKey(stockName))
                {
                    this.StockDictionary.Remove(stockName);
                }
                this.StockDictionary.CreatePortofolioSerie(this.CurrentPortofolio);
                if (!this.stockNameComboBox.Items.Contains(stockName))
                {
                    this.stockNameComboBox.Items.Insert(
                       this.stockNameComboBox.Items.IndexOf(stockName.Replace("_P", "")) + 1, stockName);
                }
            }

            this.barDurationComboBox.SelectedItem = barDuration.Duration;
            this.barSmoothingComboBox.SelectedItem = barDuration.Smoothing;
            this.barHeikinAshiCheckBox.CheckBox.Checked = barDuration.HeikinAshi;

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

        private void StockAnalyzerForm_StockSerieChanged(StockSerie newSerie, bool ignoreLinkedTheme)
        {
            //
            if (newSerie == null)
            {
                DeactivateGraphControls("No data to display");
                this.Text = "Ultimate Chartist - " + "No stock selected";
                return;
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

            // TODO Manage COT Series
            if (this.currentStockSerie.StockName.EndsWith("_COT"))
            {
                this.ForceBarDuration(StockBarDuration.Weekly, false);
            }
            else
            {
                var bd = new StockBarDuration((StockAnalyzer.StockClasses.BarDuration)this.barDurationComboBox.SelectedItem, (int)this.barSmoothingComboBox.SelectedItem);
                this.currentStockSerie.BarDuration = bd;
            }
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

            // Set the Check Box UpDownState
            this.followUpCheckBox.CheckBox.Checked = CurrentStockSerie.StockAnalysis.FollowUp;

            // Set the comment button color
            if (CurrentStockSerie.StockAnalysis.Comments.Count == 0)
            {
                this.commentBtn.BackColor = System.Drawing.SystemColors.Control;
            }
            else
            {
                this.commentBtn.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
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
            if (System.IO.File.Exists(watchListsFileName))
            {
                using (FileStream fs = new FileStream(watchListsFileName, FileMode.Open))
                {
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StockWatchList>));
                    this.WatchLists = (List<StockWatchList>)serializer.Deserialize(xmlReader);

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
                if (System.IO.File.Exists(analysisFileName))
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

        private void ReadPortofolios()
        {
            try
            {
                RefreshPortofolioMenu();
                this.CurrentPortofolio = this.StockPortofolioList.First();
            }
            catch (System.Exception exception)
            {
                string message = exception.Message;
                if (exception.InnerException != null)
                {
                    message += "\n\r" + exception.InnerException.Message;
                }
                MessageBox.Show(message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshPortofolioMenu()
        {
            // Create default portofolio if not exist
            if (this.StockPortofolioList.Count == 0)
            {
                StockPortofolio portofolio = new StockPortofolio("Default_P");
                portofolio.TotalDeposit = 10000;

                this.StockPortofolioList.Add(portofolio);
            }

            // Clean existing menus
            this.portofolioDetailsMenuItem.DropDownItems.Clear();
            this.orderListMenuItem.DropDownItems.Clear();

            System.Windows.Forms.ToolStripItem[] portofolioMenuItems =
               new System.Windows.Forms.ToolStripItem[this.StockPortofolioList.Count];
            System.Windows.Forms.ToolStripItem[] portofolioFilterMenuItems =
               new System.Windows.Forms.ToolStripItem[this.StockPortofolioList.Count];
            System.Windows.Forms.ToolStripItem[] orderListMenuItems =
               new System.Windows.Forms.ToolStripItem[this.StockPortofolioList.Count];
            ToolStripMenuItem portofolioDetailsSubMenuItem;
            ToolStripMenuItem portofolioFilterSubMenuItem;
            ToolStripMenuItem orderListSubMenuItem;

            int i = 0;
            foreach (StockPortofolio portofolio in this.StockPortofolioList)
            {
                // Create portofolio menu items
                portofolioDetailsSubMenuItem = new ToolStripMenuItem(portofolio.Name);
                portofolioDetailsSubMenuItem.Click += new EventHandler(this.viewPortogolioMenuItem_Click);
                portofolioMenuItems[i] = portofolioDetailsSubMenuItem;

                // Create portofoglio menu items
                portofolioFilterSubMenuItem = new ToolStripMenuItem(portofolio.Name);
                portofolioFilterSubMenuItem.CheckOnClick = true;
                portofolioFilterMenuItems[i] = portofolioFilterSubMenuItem;

                // create order list menu items
                orderListSubMenuItem = new ToolStripMenuItem(portofolio.Name);
                orderListSubMenuItem.Click += new EventHandler(this.orderListMenuItem_Click);
                orderListMenuItems[i++] = orderListSubMenuItem;
            }
            this.portofolioDetailsMenuItem.DropDownItems.AddRange(portofolioMenuItems);
            this.orderListMenuItem.DropDownItems.AddRange(orderListMenuItems);
        }

        private delegate bool DownloadDataMethod(string destination, ref bool upToDate);

        private void Notifiy_SplashProgressChanged(string text)
        {
            StockSplashScreen.ProgressText = text;
        }

        #region COMMITMENT OF TRADERS

        private static SortedDictionary<string, string> ParseCOTInclude()
        {
            SortedDictionary<string, string> cotIncludeList = new SortedDictionary<string, string>();
            string[] fields;

            string fileName = Settings.Default.RootFolder + @"\COT.cfg";
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        fields = sr.ReadLine().Split(';');
                        cotIncludeList.Add(fields[0], fields[1]);
                    }
                }
            }
            return cotIncludeList;
        }

        //private void ParseFullCotSeries2()
        //{
        //   StockLog.Write("ParseFullCotSeries2");

        //   this.CotDictionary = new SortedDictionary<string, CotSerie>();
        //   string line = string.Empty;
        //   try
        //   {
        //      // Parse COT include list
        //      SortedDictionary<string, string> cotIncludeList = ParseCOTInclude();

        //      // Shall be downloaded from http://www.cftc.gov/MarketReports/files/dea/history/fut_disagg_txt_2010.zip    
        //      // Read new downloaded values
        //      string cotFolder = Settings.Default.RootFolder + COT_SUBFOLDER;
        //      string[] files = System.IO.Directory.GetFiles(cotFolder, "annual_*.txt");

        //      int cotLargeSpeculatorPositionLongIndex = 7;
        //      int cotLargeSpeculatorPositionShortIndex = 8;
        //      int cotCommercialHedgerPositionLongIndex = 10;
        //      int cotCommercialHedgerPositionShortIndex = 11;
        //      int cotSmallSpeculatorPositionLongIndex = 14;
        //      int cotSmallSpeculatorPositionShortIndex = 15;
        //      int cotOpenInterestIndex = 6;

        //      DateTime cotDate;
        //      float cotLargeSpeculatorPositionLong;
        //      float cotLargeSpeculatorPositionShort;
        //      float cotCommercialHedgerPositionLong;
        //      float cotCommercialHedgerPositionShort;
        //      float cotSmallSpeculatorPositionLong;
        //      float cotSmallSpeculatorPositionShort;
        //      float cotOpenInterest;

        //      foreach (string fileName in files)
        //      {
        //         StreamReader sr = new StreamReader(fileName);
        //         CotValue readCotValue = null;
        //         CotSerie cotSerie = null;
        //         int endOfNameIndex = 0;

        //         string cotSerieName = string.Empty;

        //         string[] row;
        //         sr.ReadLine();   // Skip header line
        //         while (!sr.EndOfStream)
        //         {
        //            line = sr.ReadLine().Replace("\"", "");
        //            if (line == string.Empty)
        //            {
        //               continue;
        //            }

        //            endOfNameIndex = line.IndexOf(" ,");
        //            cotSerieName = line.Substring(0, endOfNameIndex - 1);
        //            cotSerieName = cotSerieName.Substring(0, cotSerieName.IndexOf(" - "));

        //            if (!cotIncludeList.Keys.Contains(cotSerieName))
        //            {
        //               continue;
        //            }

        //            row = line.Substring(endOfNameIndex + 2).Split(',');

        //            cotLargeSpeculatorPositionLong = float.Parse(row[cotLargeSpeculatorPositionLongIndex]);
        //            cotLargeSpeculatorPositionShort = float.Parse(row[cotLargeSpeculatorPositionShortIndex]);
        //            cotCommercialHedgerPositionLong = float.Parse(row[cotCommercialHedgerPositionLongIndex]);
        //            cotCommercialHedgerPositionShort = float.Parse(row[cotCommercialHedgerPositionShortIndex]);
        //            cotSmallSpeculatorPositionLong = float.Parse(row[cotSmallSpeculatorPositionLongIndex]);
        //            cotSmallSpeculatorPositionShort = float.Parse(row[cotSmallSpeculatorPositionShortIndex]);
        //            cotOpenInterest = float.Parse(row[cotOpenInterestIndex]);

        //            cotDate = DateTime.Parse(row[1], usCulture);

        //            readCotValue = new CotValue(cotDate, cotLargeSpeculatorPositionLong, cotLargeSpeculatorPositionShort,
        //                cotSmallSpeculatorPositionLong, cotSmallSpeculatorPositionShort,
        //                cotCommercialHedgerPositionLong, cotCommercialHedgerPositionShort, cotOpenInterest);
        //            if (this.CotDictionary.ContainsKey(cotSerieName))
        //            {
        //               cotSerie = this.CotDictionary[cotSerieName];
        //               if (!cotSerie.ContainsKey(readCotValue.Date))
        //               {
        //                  cotSerie.Add(readCotValue.Date, readCotValue);

        //                  // flag as not initialised as values have to be calculated
        //                  cotSerie.IsInitialised = false;
        //               }
        //            }
        //            else
        //            {
        //               cotSerie = new CotSerie(cotSerieName);
        //               this.CotDictionary.Add(cotSerieName, cotSerie);
        //               cotSerie.Add(readCotValue.Date, readCotValue);
        //            }
        //         }
        //         sr.Close();
        //      }
        //      // Match cotserie to stock serie
        //      foreach (KeyValuePair<string, string> pair in cotIncludeList)
        //      {
        //         if (!string.IsNullOrWhiteSpace(pair.Value) && this.StockDictionary.ContainsKey(pair.Value))
        //         {
        //            this.StockDictionary[pair.Key].CotSerie = this.CotDictionary[pair.Value];
        //         }
        //         else
        //         {
        //            StockLog.Write("StockSerie " + pair.Key + " doesn't exist, cannot map a COT");
        //         }
        //      }
        //   }
        //   catch (System.Exception e)
        //   {
        //      MessageBox.Show(e.Message + "\r\r" + line, "Failed to parse COT file");
        //   }
        //}

        #endregion // COT

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

                this.graphCloseControl.Portofolio = CurrentPortofolio;

                // Refresh all components
                RefreshGraph();
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

            // Update simulation dialog
            if (this.strategySimulatorDlg != null && (!this.strategySimulatorDlg.IsDisposed))
            {
                this.strategySimulatorDlg.SelectedStockName = stockNameComboBox.SelectedItem.ToString();
            }
            if (this.filteredStrategySimulatorDlg != null && (!this.filteredStrategySimulatorDlg.IsDisposed))
            {
                this.filteredStrategySimulatorDlg.SelectedStockName = stockNameComboBox.SelectedItem.ToString();
            }
            if (this.riskCalculatorDlg != null && (!this.riskCalculatorDlg.IsDisposed))
            {
                this.riskCalculatorDlg.StockSerie = this.CurrentStockSerie;
            }
        }

        private void downloadBtn_Click(object sender, System.EventArgs e)
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

        private void DownloadStock(bool showSplash)
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

                if (StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, this.currentStockSerie))
                {
                    if (this.currentStockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.CACALL))
                    {
                        try
                        {
                            ABCDataProvider.DownloadFinancial(this.currentStockSerie);
                            ABCDataProvider.DownloadAgenda(this.currentStockSerie);
                        }
                        catch (Exception ex)
                        {
                            StockLog.Write(ex);
                        }
                    }

                    this.currentStockSerie.PaintBarCache = null;
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

        private void DownloadStockGroup()
        {
            if (this.currentStockSerie != null)
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
                    StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, stockSerie);
                    StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " +
                                                     stockSerie.StockName;

                    if (stockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.CACALL))
                    {
                        try
                        {
                            StockSplashScreen.ProgressText = "Downloading Agenda " + stockSerie.StockGroup + " - " +
                                                             stockSerie.StockName;
                            ABCDataProvider.DownloadAgenda(stockSerie);
                            ABCDataProvider.DownloadFinancial(stockSerie);
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
                watchList.StockList.Sort();
                this.SaveWatchList();
            }
        }

        #endregion

        #region DRAWING TOOLBAR HANDLERS

        private void sarLineStripBtn_Click(object sender, EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (sarLineStripBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.AddSAR;
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
            fanLineBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void fanLineBtn_Click(object sender, System.EventArgs e)
        {
            foreach (GraphControl graphControl in this.graphList)
            {
                if (fanLineBtn.Checked)
                {
                    graphControl.DrawingMode = GraphDrawMode.XABCD;
                    graphControl.DrawingStep = GraphDrawingStep.SelectItem;
                }
                else
                {
                    graphControl.DrawingMode = GraphDrawMode.Normal;
                    graphControl.DrawingStep = GraphDrawingStep.Done;
                }
            }
            sarLineStripBtn.Checked = false;
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
            sarLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void deleteLineStripBtn_Click(object sender, System.EventArgs e)
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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
            addHalfLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void drawingStyleStripBtn_Click(object sender, System.EventArgs e)
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

        private void addHalfLineStripBtn_Click(object sender, System.EventArgs e)
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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
            deleteLineStripBtn.Checked = false;
            addSegmentStripBtn.Checked = false;
            cutLineStripBtn.Checked = false;
        }

        private void addSegmentStripBtn_Click(object sender, System.EventArgs e)
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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
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
            sarLineStripBtn.Checked = false;
            copyLineStripBtn.Checked = false;
            drawLineStripBtn.Checked = false;
            fanLineBtn.Checked = false;
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

        private void SaveAnalysis(string analysisFileName)
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

        private void savePortofolioToolStripButton_Click(object sender, EventArgs e)
        {
            // Save stock analysis to XML
            string portofolioFileName = Path.Combine(Settings.Default.RootFolder, Settings.Default.PortofolioFile);
            try
            {
                // Save Portofolios
                SavePortofolios();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Application Error saving Portfolio", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
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

        private void generateChannelStripButton_Click(object sender, EventArgs e)
        {
            //this.CurrentStockSerie.generateAutomaticTrendLines(this.graphCloseControl.StartIndex, this.graphCloseControl.EndIndex, 3, 3, 10);

            OnNeedReinitialise(true);
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

        private void followUpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (stockNameComboBox.SelectedItem != null && stockNameComboBox.SelectedItem.ToString() != string.Empty)
            {
                CurrentStockSerie.StockAnalysis.FollowUp = this.followUpCheckBox.CheckBox.Checked;
            }
        }

        private void commentBtn_Click(object sender, EventArgs e)
        {
            if (this.CurrentStockSerie != null && stockNameComboBox.SelectedItem != null &&
                stockNameComboBox.SelectedItem.ToString() != string.Empty)
            {
                CommentDialog commentDlg = new CommentDialog(this.CurrentStockSerie);
                commentDlg.ShowDialog();
            }
            OnNeedReinitialise(true);
        }

        #endregion

        #region REWIND/FAST FORWARD METHODS

        private void rewindBtn_Click(object sender, System.EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            int step = (this.endIndex - this.startIndex) / 4;
            Rewind(step);
        }

        private void fastForwardBtn_Click(object sender, System.EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            int step = (this.endIndex - this.startIndex) / 4;
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

        private void CreateRelativeStrengthMenuItem()
        {
            // Clean existing menus
            this.indexRelativeStrengthMenuItem.DropDownItems.Clear();

            List<string> validGroups = this.StockDictionary.GetValidGroupNames();
            System.Windows.Forms.ToolStripMenuItem[] groupMenuItems =
               new System.Windows.Forms.ToolStripMenuItem[validGroups.Count];

            int i = 0;
            foreach (string group in validGroups)
            {
                groupMenuItems[i] = new ToolStripMenuItem(group);

                // 
                var groupSeries = StockDictionary.Values.Where(s => s.StockGroup.ToString() == group && !s.StockAnalysis.Excluded);
                if (groupSeries.Count() != 0)
                {
                    System.Windows.Forms.ToolStripMenuItem[] indexRelativeStrengthMenuItems =
                       new System.Windows.Forms.ToolStripMenuItem[groupSeries.Count()];
                    ToolStripMenuItem indexRelativeStrengthMenuSubItem;

                    int n = 0;
                    foreach (StockSerie stockSerie in groupSeries)
                    {
                        // Create indexRelativeStrength menu items
                        indexRelativeStrengthMenuSubItem = new ToolStripMenuItem(stockSerie.StockName);
                        indexRelativeStrengthMenuSubItem.Click +=
                           new EventHandler(indexRelativeStrengthDetailsSubMenuItem_Click);
                        indexRelativeStrengthMenuItems[n++] = indexRelativeStrengthMenuSubItem;
                    }
                    groupMenuItems[i].DropDownItems.AddRange(indexRelativeStrengthMenuItems);
                }

                i++;
            }
            this.indexRelativeStrengthMenuItem.DropDownItems.AddRange(groupMenuItems);
        }

        private void CreateSecondarySerieMenuItem()
        {
            // Clean existing menus
            this.secondarySerieMenuItem.DropDownItems.Clear();
            List<string> validGroups = this.StockDictionary.GetValidGroupNames();
            System.Windows.Forms.ToolStripMenuItem[] groupMenuItems =
               new System.Windows.Forms.ToolStripMenuItem[validGroups.Count];

            int i = 0;
            foreach (string group in validGroups)
            {
                groupMenuItems[i] = new ToolStripMenuItem(group);

                // 
                var groupSeries = StockDictionary.Values.Where(s => s.StockGroup.ToString() == group && !s.StockAnalysis.Excluded);
                if (groupSeries.Count() != 0)
                {
                    System.Windows.Forms.ToolStripMenuItem[] secondarySerieMenuItems = new System.Windows.Forms.ToolStripMenuItem[groupSeries.Count()];
                    ToolStripMenuItem secondarySerieSubMenuItem;

                    int n = 0;
                    foreach (StockSerie stockSerie in groupSeries)
                    {
                        // Create indexRelativeStrength menu items
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
            this.barDurationComboBox.SelectedItem = StockAnalyzer.StockClasses.BarDuration.Daily;

            foreach (int barSmoothing in new List<int> { 1, 3, 6, 9, 12, 20, 50, 100 })
            {
                this.barSmoothingComboBox.Items.Add(barSmoothing);
            }
            this.barSmoothingComboBox.SelectedItem = this.barSmoothingComboBox.Items.OfType<int>().First();
        }

        private void BarDurationChanged(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;

            StockBarDuration barDuration = (BarDuration)barDurationComboBox.SelectedItem;
            barDuration.Smoothing = (int)barSmoothingComboBox.SelectedItem;
            barDuration.HeikinAshi = barHeikinAshiCheckBox.CheckBox.CheckState == CheckState.Checked;
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
                if (endIndex - startIndex < 25)
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

        public void ForceBarDuration(StockBarDuration barDuration, bool triggerEvent)
        {
            if (!triggerEvent)
            {
                this.barDurationComboBox.SelectedIndexChanged -= new System.EventHandler(this.BarDurationChanged);
            }

            this.barDurationComboBox.SelectedItem = barDuration.Duration;
            this.barSmoothingComboBox.SelectedItem = barDuration.Smoothing;
            this.barHeikinAshiCheckBox.CheckBox.Checked = barDuration.HeikinAshi;

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

            OnNeedReinitialise(true);
        }

        private void indexRelativeStrengthDetailsSubMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            StockSerie newSerie =
               this.CurrentStockSerie.GenerateRelativeStrenthStockSerie(StockDictionary[sender.ToString()]);
            if (newSerie == null)
            {
                MessageBox.Show("This operation is not allowed");
                return;
            }
            AddNewSerie(newSerie);
        }

        private void generateWeeklyVariationSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            if (!this.currentStockSerie.Initialise())
            {
                return;
            }
            this.currentStockSerie.BarDuration = StockBarDuration.Daily;
            float[] weeklyData = new float[5];
            int[] occurences = new int[5];
            if (this.currentStockSerie.StockGroup != StockSerie.Groups.BREADTH)
            {
                foreach (StockDailyValue dailyValue in this.currentStockSerie.Values)
                {
                    occurences[(int)dailyValue.DATE.DayOfWeek - 1]++;
                    weeklyData[(int)dailyValue.DATE.DayOfWeek - 1] += dailyValue.VARIATION;
                }
                string report = string.Empty;
                for (int i = 0; i < 5; i++)
                {
                    weeklyData[i] /= (float)occurences[i];
                    report += ((DayOfWeek)(i + 1)).ToString() + ": " + weeklyData[i].ToString("P2") +
                              System.Environment.NewLine;
                }

                MessageBox.Show(report, "Weekly variation for " + this.currentStockSerie.StockName);
            }
        }

        private void overnightSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            StockSerie newSerie = this.CurrentStockSerie.GenerateOvernightStockSerie();
            string currentStockName = this.currentStockSerie.StockName;
            if (newSerie.Initialise())
            {
                AddNewSerie(newSerie);

                // Set current serie as secondary serie
                ToolStripMenuItem currentSerieMenuItem = null;
                foreach (ToolStripMenuItem otherMenuItem in this.secondarySerieMenuItem.DropDownItems)
                {
                    foreach (ToolStripMenuItem subMenuItem in otherMenuItem.DropDownItems)
                    {
                        if (subMenuItem.Text == currentStockName)
                        {
                            currentSerieMenuItem = subMenuItem;
                            break;
                        }
                    }
                    if (currentSerieMenuItem != null)
                    {
                        break;
                    }
                }

                // Display initial serie as secondary serie
                if (currentSerieMenuItem != null)
                {
                    this.secondarySerieMenuItem_Click(currentSerieMenuItem, null);
                }
            }
        }

        private void generateSeasonalitySerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            if (!this.currentStockSerie.Initialise())
            {
                return;
            }
            if (this.currentStockSerie.StockGroup != StockSerie.Groups.BREADTH)
            {
                // Bar duration is set inside the method
                StockSerie seasonalSerie = this.currentStockSerie.CalculateSeasonality();
                if (seasonalSerie.Initialise())
                {
                    int previousSerieCount = this.currentStockSerie.Count;
                    string stockSerieName = this.currentStockSerie.StockName;

                    // 
                    AddNewSerie(seasonalSerie);

                    // Set current serie as secondary serie
                    ToolStripMenuItem currentSerieMenuItem = null;
                    foreach (ToolStripMenuItem otherMenuItem in this.secondarySerieMenuItem.DropDownItems)
                    {
                        foreach (ToolStripMenuItem subMenuItem in otherMenuItem.DropDownItems)
                        {
                            if (subMenuItem.Text == stockSerieName)
                            {
                                currentSerieMenuItem = subMenuItem;
                                break;
                            }
                        }
                        if (currentSerieMenuItem != null)
                        {
                            break;
                        }
                    }

                    // Display initial serie as secondary serie
                    if (currentSerieMenuItem != null)
                    {
                        this.secondarySerieMenuItem_Click(currentSerieMenuItem, null);
                    }

                    // Set appropriate zoom
                    this.ChangeZoom(
                       this.CurrentStockSerie.Count - (this.CurrentStockSerie.Count - previousSerieCount + 200),
                       this.CurrentStockSerie.Count - 1);
                }
            }
        }

        private delegate bool ConditionMatched(int i, StockSerie serie, ref string eventName);

        public bool EMAUp(int i, StockSerie stockSerie)
        {
            FloatSerie ema6 = stockSerie.GetIndicator("EMA(6)").Series[0];
            FloatSerie ema12 = stockSerie.GetIndicator("EMA(12)").Series[0];
            FloatSerie ema50 = stockSerie.GetIndicator("EMA(50)").Series[0];
            FloatSerie longStop = stockSerie.GetTrailStop("TRAILHL(3)").Series[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            return (!float.IsNaN(longStop[i - 1]) && !float.IsNaN(longStop[i]) && closeSerie[i] < ema50[i]);
        }

        public bool NewUpBar(int i, StockSerie stockSerie)
        {
            StockDailyValue previous = stockSerie.ValueArray[i - 1];
            StockDailyValue today = stockSerie.ValueArray[i];
            return today.CLOSE > previous.HIGH;
        }

        public bool BuyMomex(int i, StockSerie stockSerie)
        {
            FloatSerie momexExhaution = stockSerie.GetIndicator("BUYMOMEX(20,True,1.5)").Series[0];

            return momexExhaution[i - 2] < momexExhaution[i - 1] && momexExhaution[i - 1] > momexExhaution[i] &&
                   momexExhaution[i - 1] > 35f;
        }

        public bool TrailBB(int i, StockSerie stockSerie, ref string eventName)
        {
            eventName = "UpBreak_TRAILBB(6,2,-2)";
            BoolSerie upTrend = stockSerie.GetTrailStop("TRAILBB(6,2,-2)").Events[0];

            return !upTrend[i - 1] && upTrend[i];
        }

        public bool TrailHL(int i, StockSerie stockSerie, ref string eventName)
        {
            eventName = "UpBreak_TRAILHL(4)";
            BoolSerie upTrend = stockSerie.GetTrailStop("TRAILHL(1)").Events[0];
            BoolSerie upBar = stockSerie.GetPaintBar("HIGHLOWDAYS(6)").Events[0];

            FloatSerie roc = stockSerie.GetIndicator("ROCEX3(200,100,50,10,20)").Series[0];

            return upTrend[i] && roc[i] > 0 && (upBar[i] && !upBar[i - 1]);
        }

        struct stat
        {
            public int nbr;
            public float var;
        }

        public void statMenu_Click()
        {
            if (this.CurrentStockSerie != null && this.CurrentStockSerie.Initialise())
            {
                var trailStop = this.CurrentStockSerie.GetTrailStop("TRAILEMA(12,1)");
                bool found = false;
                int nb = 0;
                float close = 0f;
                List<stat> stats = new List<stat>();
                int eventIndex = 1;
                // var closeSerie = this.currentStockSerie.GetSerie(StockDataType.CLOSE);
                var closeSerie = this.currentStockSerie.GetExactValues().Select(dv => dv.CLOSE).ToArray();
                for (int i = 1; i < this.currentStockSerie.Count; i++)
                {
                    if (found)
                    {
                        if (float.IsNaN(trailStop.Series[0][i]))
                        {
                            stats.Add(new stat() { nbr = nb, var = (closeSerie[i] - close) / close });
                            found = false;
                        }
                        else
                        {
                            nb++;
                        }
                    }
                    else
                    {
                        if (!float.IsNaN(trailStop.Series[0][i]))
                        {
                            nb = 1;
                            close = closeSerie[i];
                            found = true;
                        }
                    }
                }

                string result = stats.Select(s => s.nbr + " " + s.var.ToString().Replace(".", ",")).Aggregate((i, j) => i + Environment.NewLine + j);
                Clipboard.SetText(result);
            }
        }

        private void statisticsMenuItem_Click(object sender, System.EventArgs e)
        {
            StatisticsDlg statisticsDlg = new StatisticsDlg();
            statisticsDlg.ShowDialog();
            return;

            const int minBar = 50;
            int nbStatBar = 50;

            FloatSerie varSerie = new FloatSerie(nbStatBar);
            FloatSerie percentUpSerie = new FloatSerie(nbStatBar);
            FloatSerie varMaxSerie = new FloatSerie(nbStatBar);
            FloatSerie varMinSerie = new FloatSerie(nbStatBar);

            int eventOccurence = 0;

            ConditionMatched cond = TrailHL;
            string eventName = string.Empty;

            int[] histogram = new int[nbStatBar];

            foreach (
               StockSerie stockSerie in
                  this.StockDictionary.Values.Where(s => s.BelongsToGroup(this.currentStockSerie.StockGroup)))
            {
                if (!stockSerie.Initialise()) continue;
                stockSerie.BarDuration = (StockBarDuration)barDurationComboBox.SelectedItem;

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);


                BoolSerie upTrend = stockSerie.GetTrailStop("TRAILHL(1)").Events[0];

                for (int i = minBar; i < (stockSerie.Count - nbStatBar); i++)
                {
                    if (cond(i, stockSerie, ref eventName))
                    {
                        eventOccurence++;
                        // Event detected
                        int j;
                        for (j = 1; j < nbStatBar; j++)
                        {
                            if (!upTrend[i + j]) break;
                            StockDailyValue dailyValue = stockSerie.ValueArray[i + j + 1];
                            varSerie[j] += dailyValue.VARIATION;
                            percentUpSerie[j] += dailyValue.VARIATION > 0 ? 1 : 0;
                            varMaxSerie[j] = Math.Max(dailyValue.VARIATION, varMaxSerie[j]);
                            varMinSerie[j] = Math.Min(dailyValue.VARIATION, varMinSerie[j]);
                        }
                        histogram[j - 1]++;
                    }
                }
            }
            varSerie /= (float)(eventOccurence);
            percentUpSerie /= (float)(eventOccurence);

            float val = 100f;
            float maxVal = 100f;
            float minVal = 100f;

            // Generate report
            if (!Directory.Exists(Settings.Default.RootFolder + "\\Report"))
                Directory.CreateDirectory(Settings.Default.RootFolder + "\\Report");
            using (
               StreamWriter sr =
                  new StreamWriter(
                     Settings.Default.RootFolder + "\\Report\\" + this.CurrentStockSerie.StockGroup + "_" +
                     barDurationComboBox.SelectedItem.ToString() + "_" +
                     eventName.Replace("(", "_").Replace(")", "_").Replace(".", "_").Replace(";", "_") + "Stat.csv", false)
               )
            {
                sr.WriteLine("Histogram");
                sr.WriteLine("");
                sr.WriteLine("Bar;Occurence");

                for (int i = 0; i < nbStatBar; i++)
                {
                    sr.WriteLine(i + ";" + histogram[i]);
                }

                sr.WriteLine("");
                sr.WriteLine("BarType;" + barDurationComboBox.SelectedItem.ToString());
                sr.WriteLine("Group;" + this.CurrentStockSerie.StockGroup);
                sr.WriteLine("Event;" + eventName);
                sr.WriteLine("NbOccurences;" + eventOccurence);

                sr.WriteLine("");
                sr.WriteLine("Value;Up %;Avg Var;Max Var;Min Var");

                for (int i = 0; i < nbStatBar; i++)
                {
                    val *= 1 + varSerie[i];
                    minVal *= 1 + varMinSerie[i];
                    maxVal *= 1 + varMaxSerie[i];
                    sr.WriteLine(val + ";" + percentUpSerie[i] + ";" + varSerie[i] + ";" + varMaxSerie[i] + ";" +
                                 varMinSerie[i]);
                }
            }
        }

        private void patternRecognitionMenuItem_Click(object sender, System.EventArgs e)
        {
            int before = 10;
            int after = 200;
            StockStatisticsEngine engine = new StockStatisticsEngine(before, after);

            //this.CurrentTheme = "PATTERN";

            //AddNewSerie(engine.GenerateSerie("Test"));

            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "OVERBOUGHTSR(STOKS(30_3_3),75,25)", "LowerHigh"));
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "OVERBOUGHTSR(STOKS(30_3_3),75,25)", "SupportDetected"));
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "TRAILSTOP", "TRAILHL(50)", "TrailedDown"));
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "EMA2Lines(49,50)", "BearishCrossing"));
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "EMA2Lines(49,50)", "BearishCrossing"));
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "ER(60,6,1,0.8)", "Oversold"));
            var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "PAINTBAR", "TRUE(1)", "AllTimeHigh"));
            //var pattern = new StockMatchPattern_Any();
            //var pattern = new StockMatchPattern_StockAlert(new StockAlertDef(StockBarDuration.Daily, "INDICATOR", "HIGHLOWDAYS(200)", "Highest"));
            //var pattern = new StockMatchPattern_ROR(20);

            var series = this.StockDictionary.Values.Where(s => s.BelongsToGroup(this.selectedGroup));
            //var series = this.StockDictionary.Values.Where(s => s.StockName == "Test");
            StockSerie serie = engine.FindPattern(series, BarDuration, pattern);

            var drawingItems = new StockDrawingItems();
            drawingItems.Add(new Line2D(new PointF(before, 0), 0, 1));
            serie.StockAnalysis.DrawingItems.Add(StockBarDuration.Daily, drawingItems);

            AddNewSerie(serie);
        }

        private void logSerieMenuItem_Click(object sender, System.EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            StockSerie newSerie = this.CurrentStockSerie.GenerateLogStockSerie();
            AddNewSerie(newSerie);
        }

        private void inverseSerieMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null || !this.currentStockSerie.Initialise()) return;

            if (this.currentStockSerie.StockName.EndsWith("_INV"))
            {
                stockNameComboBox.SelectedIndex = stockNameComboBox.Items.IndexOf(this.currentStockSerie.StockName.Replace("_INV", ""));
                OnNeedReinitialise(true);
                return;
            }
            if (this.StockDictionary.ContainsKey(this.currentStockSerie.StockName + "_INV"))
            {
                stockNameComboBox.SelectedIndex = stockNameComboBox.Items.IndexOf(this.currentStockSerie.StockName + "_INV");
                OnNeedReinitialise(true);
                return;
            }
            StockSerie newSerie = this.CurrentStockSerie.GenerateInverseStockSerie();
            AddNewSerie(newSerie);
        }
        private void GenerateCACEqualWeight()
        {
            var cacSeries =
               this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW";
            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = 1000f;
            int cacIndex = 0;
            foreach (DateTime date in cacSerie.Keys)
            {
                float var = 0.0f;
                float volume = 0.0f;
                int count = 0;
                foreach (StockSerie serie in cacSeries)
                {
                    if (serie.ContainsKey(date))
                    {
                        count++;
                        StockDailyValue dailyValue = serie[date];
                        var += dailyValue.VARIATION;
                        volume += dailyValue.CLOSE * dailyValue.VOLUME;
                    }
                }
                value += value * (var / count);
                cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)volume, date));
                cacIndex++;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateSRDEqualWeight()
        {
            string serieName = "SRD";
            StockSerie srdSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES, StockDataProvider.Generated);
            /*
            var cacSeries = this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.SRD) && s.Initialise());
      StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = 1000f;
            foreach (DateTime date in cacSerie.Keys)
            {
               float var = 0.0f;
               float volume = 0.0f;
               int count = 0;
               foreach (StockSerie serie in cacSeries)
               {
                  if (serie.ContainsKey(date))
                  {
                     count++;
                     StockDailyValue dailyValue = serie[date];
                     var += dailyValue.VARIATION;
                     volume += dailyValue.CLOSE * dailyValue.VOLUME;
                  }
               }
               if (count > 0)
               {
                  value += value * (var / count);
                  volume /= count;
                  srdSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)volume, date));
               }
            }*/
            StockDictionary.Add(serieName, srdSerie);
        }
        private void GenerateIndexNoDay(string stockName, DayOfWeek dayOfWeek)
        {
            string serieName = stockName + "_NO_" + dayOfWeek;

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC, StockDataProvider.Generated);
            StockSerie stockSerie = this.StockDictionary[stockName];
            stockSerie.Initialise();

            float value = stockSerie.First().Value.OPEN;
            int cacIndex = 0;
            foreach (StockDailyValue dailyValue in stockSerie.Values)
            {
                float volume = 0.0f;
                if (dailyValue.DATE.DayOfWeek != dayOfWeek)
                {
                    value += value * dailyValue.VARIATION;
                }
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateCACEqualWeightNoUpDay()
        {
            //var cacSeries =
            //   this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW_NO_UP";

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = cacSerie.First().Value.OPEN;
            int cacIndex = 0;
            StockDailyValue previousDailyValue = cacSerie.Values.First();

            foreach (StockDailyValue dailyValue in cacSerie.Values)
            {
                float volume = 0.0f;
                if (previousDailyValue.VARIATION < 0)
                {
                    value += value * dailyValue.VARIATION;
                }
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
                previousDailyValue = dailyValue;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateCACEqualWeightNoDownDay()
        {
            //var cacSeries =
            //   this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW_NO_DOWN";

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = cacSerie.First().Value.OPEN;
            int cacIndex = 0;
            StockDailyValue previousDailyValue = cacSerie.Values.First();

            foreach (StockDailyValue dailyValue in cacSerie.Values)
            {
                float volume = 0.0f;
                if (previousDailyValue.VARIATION < 0)
                {
                    value += value * dailyValue.VARIATION;
                }
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
                previousDailyValue = dailyValue;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateCACEqualWeightNoUpDay(DayOfWeek dayOfWeek)
        {
            //var cacSeries =
            //   this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW_NO_UP_" + dayOfWeek;

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = cacSerie.First().Value.OPEN;
            int cacIndex = 0;
            StockDailyValue previousDailyValue = cacSerie.Values.First();

            foreach (StockDailyValue dailyValue in cacSerie.Values)
            {
                float volume = 0.0f;
                if (previousDailyValue.DATE.DayOfWeek == dayOfWeek && previousDailyValue.VARIATION < 0)
                {
                    value += value * dailyValue.VARIATION;
                }
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
                previousDailyValue = dailyValue;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateCACEqualWeightNoDownDay(DayOfWeek dayOfWeek)
        {
            //var cacSeries =
            //   this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW_NODOWN_" + dayOfWeek;

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            float value = cacSerie.First().Value.OPEN;
            int cacIndex = 0;
            StockDailyValue previousDailyValue = cacSerie.Values.First();

            foreach (StockDailyValue dailyValue in cacSerie.Values)
            {
                float volume = 0.0f;
                if (previousDailyValue.DATE.DayOfWeek == dayOfWeek && previousDailyValue.VARIATION > 0)
                {
                    value += value * dailyValue.VARIATION;
                }
                previousDailyValue = dailyValue;
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateIndexOnlyDay(string stockName, DayOfWeek dayOfWeek)
        {
            string serieName = stockName + "_ONLY_" + dayOfWeek;

            StockSplashScreen.ProgressText = "Generating " + serieName + "...";

            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC, StockDataProvider.Generated);
            StockSerie stockSerie = this.StockDictionary[stockName];
            stockSerie.Initialise();

            float value = stockSerie.First().Value.OPEN;
            int cacIndex = 0;
            foreach (StockDailyValue dailyValue in stockSerie.Values)
            {
                float volume = 0.0f;
                if (dailyValue.DATE.DayOfWeek == dayOfWeek)
                {
                    value += value * dailyValue.VARIATION;
                }
                cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)volume, dailyValue.DATE));
                cacIndex++;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }
        private void GenerateCACEqualWeight2()
        {
            var cacSeries =
               this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
            string serieName = "CAC_EW2";
            StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES,
               StockDataProvider.Generated);
            StockSerie cacSerie = this.StockDictionary["CAC40"];
            cacSerie.Initialise();

            IStockEvent CACIndicator = cacSerie.GetTrailStop("TRAILHLS(19,3)") as IStockEvent;

            float value = 1000f;
            int cacIndex = -1;
            foreach (DateTime date in cacSerie.Keys)
            {
                float var = 0.0f;
                float volume = 0.0f;
                int count = 0;
                if (cacIndex >= 0 && CACIndicator.Events[0][cacIndex])
                {
                    foreach (StockSerie serie in cacSeries)
                    {
                        if (serie.ContainsKey(date))
                        {
                            count++;
                            StockDailyValue dailyValue = serie[date];
                            var += dailyValue.VARIATION;
                            volume += dailyValue.CLOSE * dailyValue.VOLUME;
                        }
                    }
                    value += value * (var / count);
                }
                cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)volume, date));
                cacIndex++;
            }
            StockDictionary.Add(serieName, cacEWSerie);
        }

        private void GenerateVixPremium()
        {
            if (!this.StockDictionary.ContainsKey("VIX"))
            {
                return;
            }
            StockSerie vixSerie = this.StockDictionary["VIX"];
            if (!vixSerie.Initialise())
            {
                return;
            }
            StockSerie spSerie = this.StockDictionary["SP500"];
            if (!spSerie.Initialise())
            {
                return;
            }
        }

        #endregion

        private void palmaresMenuItem_Click(object sender, EventArgs e)
        {
            if (palmaresDlg == null)
            {
                palmaresDlg = new PalmaresDlg(StockDictionary, this.WatchLists, this.selectedGroup, this.progressBar);
                palmaresDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                palmaresDlg.SelectedPortofolioChanged += new SelectedPortofolioNameChangedEventHandler(OnCurrentPortofolioNameChanged);
                palmaresDlg.SelectStockGroupChanged += new SelectedStockGroupChangedEventHandler(this.OnSelectedStockGroupChanged);

                palmaresDlg.FormClosing += new FormClosingEventHandler(palmaresDlg_FormClosing);

                if (sender is SimulationParameterControl)
                {
                    SimulationParameterControl simulationParameterControl = (SimulationParameterControl)sender;
                    this.palmaresDlg.StartDate = simulationParameterControl.StartDate;
                    this.palmaresDlg.EndDate = simulationParameterControl.EndDate;
                    this.palmaresDlg.DisplayPortofolio = true;
                    this.palmaresDlg.InitializeListView();
                }
                palmaresDlg.Show();
            }
            else
            {
                if (sender is SimulationParameterControl)
                {
                    SimulationParameterControl simulationParameterControl = (SimulationParameterControl)sender;
                    this.palmaresDlg.StartDate = simulationParameterControl.StartDate;
                    this.palmaresDlg.EndDate = simulationParameterControl.EndDate;
                    this.palmaresDlg.DisplayPortofolio = true;
                }
                this.palmaresDlg.InitializeListView();
                palmaresDlg.Activate();
            }
        }
        private void palmaresDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            palmaresDlg = null;
        }
        private void OnSelectedStockGroupChanged(string stockGroup)
        {
            StockSerie.Groups newGroup = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), stockGroup);
            if (this.selectedGroup != newGroup)
            {
                SetDurationForStockGroup(newGroup);

                this.selectedGroup = newGroup;

                foreach (ToolStripMenuItem groupSubMenuItem in this.stockFilterMenuItem.DropDownItems)
                {
                    groupSubMenuItem.Checked = groupSubMenuItem.Text == stockGroup;
                }

                InitialiseStockCombo(true);
            }
        }

        private void SetDurationForStockGroup(StockSerie.Groups newGroup)
        {
            // In order to speed the intraday display.
            switch (newGroup)
            {
                case StockSerie.Groups.TURBO:
                case StockSerie.Groups.FUTURE:
                case StockSerie.Groups.INTRADAY:
                    this.ForceBarDuration(StockBarDuration.TLB_3D, true);
                    break;
                default:
                    this.ForceBarDuration(StockBarDuration.Daily, true);
                    break;
            }
        }

        private void OnCurrentPortofolioChanged(StockPortofolio portofolio, bool activate)
        {
            if (portofolio != null)
            {
                CurrentPortofolio = portofolio;
                this.StockPortofolioList.Remove(portofolio.Name);
                this.StockPortofolioList.Add(portofolio);
            }

            this.OnNeedReinitialise(true);

            if (activate)
            {
                this.Activate();
            }
        }

        private void OnCurrentPortofolioNameChanged(string portofolioName, bool activate)
        {
            CurrentPortofolio = this.StockPortofolioList.First(p => p.Name == portofolioName);

            this.OnNeedReinitialise(true);

            if (activate)
            {
                this.Activate();
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

        private void showSummaryOrdersMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowSummaryOrders = this.showSummaryOrdersMenuItem.Checked;

            if (this.showSummaryOrdersMenuItem.Checked)
            {
                Settings.Default.ShowOrders = true;
                this.showOrdersMenuItem.Checked = true;
            }
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
        public void importBinckOrders_Click(object sender, EventArgs e)
        {
            ImportBinckOrderDlg importOrderDlg = new ImportBinckOrderDlg();
            if (importOrderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.SavePortofolios();
            }
        }

        public void newOrderMenuItem_Click(object sender, EventArgs e)
        {
            OrderEditionDlg orderCreationDlg = new OrderEditionDlg(this.stockNameComboBox.SelectedItem.ToString(),
               this.StockPortofolioList.GetPortofolioNames(), null);
            if (orderCreationDlg.ShowDialog() == DialogResult.OK)
            {
                StockPortofolio portofolio = this.StockPortofolioList.Get(orderCreationDlg.PortofolioName);
                // Retrieve new sotck order
                portofolio.OrderList.Add(orderCreationDlg.Order);
                portofolio.OrderList.SortByDate();

                // Regenerate the stock serie for the current portoflio
                StockDictionary.CreatePortofolioSerie(portofolio);

                // Save Portofolios
                SavePortofolios();

                // Refresh the screen
                OnNeedReinitialise(false);
            }
        }

        private void OnStockOrderDeleted(StockOrder stockOrder)
        {
            // Save Portofolios
            SavePortofolios();

            //
            OnNeedReinitialise(false);
        }

        public void SavePortofolios()
        {
            // Save portofolio list
            foreach (StockPortofolio portofolio in this.StockPortofolioList)
            {
                portofolio.OrderList.SortByDate();
            }
            XmlSerializer serializer = new XmlSerializer(typeof(StockPortofolioList));
            System.Xml.XmlTextWriter xmlWriter =
               new System.Xml.XmlTextWriter(Path.Combine(Settings.Default.RootFolder, Settings.Default.PortofolioFile),
                  System.Text.Encoding.Default);
            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            serializer.Serialize(xmlWriter, this.StockPortofolioList);
            xmlWriter.Close();
            //
            OnNeedReinitialise(false);
        }

        private void currentPortofolioMenuItem_Click(object sender, EventArgs e)
        {
            if (this.CurrentPortofolio != null)
            {
                this.CurrentPortofolio.Initialize();
                if (portofolioDlg == null)
                {
                    portofolioDlg = new PortofolioDlg(StockDictionary, this.CurrentPortofolio);
                    portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                    portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                    portofolioDlg.Show();
                }

                else
                {
                    portofolioDlg.SetPortofolio(this.CurrentPortofolio);
                    portofolioDlg.Activate();
                }
            }
        }

        private void viewPortogolioMenuItem_Click(object sender, EventArgs e)
        {
            StockPortofolio portofolio = this.StockPortofolioList.Get(sender.ToString());

            if (portofolio != null)
            {
                this.CurrentPortofolio = portofolio;

                portofolio.Initialize();
                if (portofolioDlg == null)
                {

                    portofolioDlg = new PortofolioDlg(StockDictionary, portofolio);
                    portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                    portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                    portofolioDlg.Show();
                }

                else
                {
                    portofolioDlg.SetPortofolio(portofolio);
                    portofolioDlg.Activate();
                }
            }
        }

        private void portofolioDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            portofolioDlg = null;
            OnNeedReinitialise(false);
        }

        private void orderListMenuItem_Click(object sender, EventArgs e)
        {
            StockPortofolio portofolio = this.StockPortofolioList.Get(sender.ToString());
            if (orderListDlg == null)
            {
                orderListDlg = new OrderListDlg(StockDictionary, portofolio.OrderList);
                orderListDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                orderListDlg.StockOrderDeleted += new StockOrderDeletedEventHandler(OnStockOrderDeleted);
                orderListDlg.FormClosing += new FormClosingEventHandler(orderListDlg_FormClosing);
                orderListDlg.Show();
            }
            else
            {
                orderListDlg.Activate();
            }
        }

        private void orderListDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderListDlg = null;
            OnNeedReinitialise(false);
        }

        #endregion

        #region ANALYSIS MENU HANDLERS
        private void strategySimulationMenuItem_Click(object sender, EventArgs e)
        {
            CreateSimulationPortofolio(5000.0f);

            if (strategySimulatorDlg == null || strategySimulatorDlg.IsDisposed)
            {
                strategySimulatorDlg = new StrategySimulatorDlg(StockDictionary, this.StockPortofolioList,
                   this.stockNameComboBox.SelectedItem.ToString());
                strategySimulatorDlg.SimulationCompleted +=
                   new StrategySimulatorDlg.SimulationCompletedEventHandler(strategySimulatorDlg_SimulationCompleted);
                strategySimulatorDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                strategySimulatorDlg.SelectedPortofolioChanged +=
                   new SelectedPortofolioChangedEventHandler(OnCurrentPortofolioChanged);
            }
            else
            {
                strategySimulatorDlg.Activate();
            }
            strategySimulatorDlg.SelectedStockName = this.stockNameComboBox.SelectedItem.ToString();

            strategySimulatorDlg.SelectedPortofolio = CurrentPortofolio;
            strategySimulatorDlg.Show();
        }

        private void filteredStrategySimulationMenuItem_Click(object sender, EventArgs e)
        {
            CreateSimulationPortofolio(5000.0f);

            if (filteredStrategySimulatorDlg == null || filteredStrategySimulatorDlg.IsDisposed)
            {
                filteredStrategySimulatorDlg = new FilteredStrategySimulatorDlg(StockDictionary, this.StockPortofolioList,
                   this.stockNameComboBox.SelectedItem.ToString());
                filteredStrategySimulatorDlg.SimulationCompleted += new FilteredStrategySimulatorDlg.SimulationCompletedEventHandler(filteredStrategySimulatorDlg_SimulationCompleted);
                filteredStrategySimulatorDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                filteredStrategySimulatorDlg.SelectedPortofolioChanged += new SelectedPortofolioChangedEventHandler(OnCurrentPortofolioChanged);
            }
            else
            {
                filteredStrategySimulatorDlg.Activate();
            }
            filteredStrategySimulatorDlg.SelectedStockName = this.stockNameComboBox.SelectedItem.ToString();

            filteredStrategySimulatorDlg.Show();
        }

        private void exportFinancialsMenuItem_Click(object sender, System.EventArgs e)
        {
            bool first = true;
            foreach (var stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.CACALL) && s.Initialise() && s.Financial != null))
            {
                float yield = stockSerie.Financial.Dividend / stockSerie.Last().Value.CLOSE;
                Console.WriteLine(stockSerie.StockGroup + "," + stockSerie.StockName + "," + stockSerie.Financial.Dividend + "," + stockSerie.Last().Value.CLOSE + "," + yield.ToString("P2") + "," + stockSerie.Financial.BookValuePerShare + "," + stockSerie.Financial.TangibleBookValuePerShare);
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            foreach (var stockSerie in this.StockDictionary.Values.Where(s => s.Financial != null && s.Initialise()))
            {
                stockSerie.Financial.Value = stockSerie.Values.Last().CLOSE;
                stockSerie.Financial.CalculateRatios();
                if (stockSerie.Financial.Ratios != null && stockSerie.Financial.Ratios.Count > 0)
                {
                    if (first)
                    {
                        first = false;
                        Console.Write("StockName,");
                        foreach (var ratio in stockSerie.Financial.Ratios)
                        {
                            Console.Write(ratio.First() + ",");
                        }
                        Console.WriteLine();
                    }
                    Console.Write(stockSerie.StockName + ",");
                    foreach (var ratio in stockSerie.Financial.Ratios)
                    {
                        Console.Write(ratio.Last() + ",");
                    }
                    Console.WriteLine();
                }
            }
        }

        private void portofolioSimulationMenuItem_Click(object sender, System.EventArgs e)
        {
            CreateSimulationPortofolio(5000.0f);

            if (portfolioSimulatorDlg == null || portfolioSimulatorDlg.IsDisposed)
            {
                portfolioSimulatorDlg = new PortfolioSimulatorDlg(StockDictionary, this.StockPortofolioList,
                   this.stockNameComboBox.SelectedItem.ToString(), this.WatchLists);
                portfolioSimulatorDlg.SimulationCompleted +=
                   new PortfolioSimulatorDlg.SimulationCompletedEventHandler(portfolioSimulatorDlg_SimulationCompleted);
                portfolioSimulatorDlg.SelectedPortofolioChanged +=
                   new SelectedPortofolioChangedEventHandler(OnCurrentPortofolioChanged);
            }
            else
            {
                portfolioSimulatorDlg.Activate();
            }

            this.CurrentPortofolio = portfolioSimulatorDlg.SelectedPortofolio;
            portfolioSimulatorDlg.Show();

        }

        private void CreateSimulationPortofolio(float portofolioDeposit)
        {
            // Create new simulation portofolio
            if (CurrentPortofolio == null)
            {
                CurrentPortofolio = this.StockPortofolioList.Find(p => p.Name == this.CurrentStockSerie.StockName + "_P");
                if (CurrentPortofolio == null)
                {
                    CurrentPortofolio = new StockPortofolio(this.CurrentStockSerie.StockName + "_P");
                    CurrentPortofolio.IsSimulation = true;
                    CurrentPortofolio.TotalDeposit = portofolioDeposit;
                    this.StockPortofolioList.Add(CurrentPortofolio);
                }
            }
        }

        private void strategySimulatorDlg_SimulationCompleted()
        {
            // Refresh portofolio generated stock
            StockPortofolio portofolio = this.strategySimulatorDlg.SelectedPortofolio;
            portofolio.Initialize();
            StockDictionary.CreatePortofolioSerie(portofolio);

            // Refresh the screen
            OnNeedReinitialise(true);

            //Display portofolio window
            if (portofolioDlg == null)
            {
                portofolioDlg = new PortofolioDlg(StockDictionary, portofolio);
                portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                portofolioDlg.Show();
            }
            else
            {
                portofolioDlg.SetPortofolio(portofolio);
                portofolioDlg.Activate();
            }

            RefreshPortofolioMenu();
        }

        private void filteredStrategySimulatorDlg_SimulationCompleted()
        {
            // Refresh portofolio generated stock
            StockPortofolio portofolio = this.filteredStrategySimulatorDlg.SelectedPortofolio;
            portofolio.Initialize();
            StockDictionary.CreatePortofolioSerie(portofolio);

            // Refresh the screen
            OnNeedReinitialise(true);

            //Display portofolio window
            if (portofolioDlg == null)
            {
                portofolioDlg = new PortofolioDlg(StockDictionary, portofolio);
                portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                portofolioDlg.Show();
            }
            else
            {
                portofolioDlg.SetPortofolio(portofolio);
                portofolioDlg.Activate();
            }

            RefreshPortofolioMenu();

            this.CurrentPortofolio = portofolio;
        }

        private void portfolioSimulatorDlg_SimulationCompleted()
        {
            // Refresh portofolio generated stock
            StockPortofolio portofolio = this.portfolioSimulatorDlg.SelectedPortofolio;
            portofolio.Initialize();
            StockDictionary.CreatePortofolioSerie(portofolio);

            // Refresh the screen
            OnNeedReinitialise(true);

            //Display portofolio window
            if (portofolioDlg == null)
            {
                portofolioDlg = new PortofolioDlg(StockDictionary, portofolio);
                portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                portofolioDlg.Show();
            }
            else
            {
                portofolioDlg.SetPortofolio(portofolio);
                portofolioDlg.Activate();
            }

            RefreshPortofolioMenu();
        }

        private void orderGenerationDlg_SimulationCompleted()
        {
            // Refresh portofolio generated stock
            StockPortofolio portofolio = this.orderGenerationDlg.SelectedPortofolio;
            portofolio.Initialize();
            StockDictionary.CreatePortofolioSerie(portofolio);

            // Refresh the screen
            OnNeedReinitialise(true);

            //Display portofolio window
            if (orderListDlg == null)
            {
                orderListDlg = new OrderListDlg(StockDictionary, portofolio.OrderList);
                orderListDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                orderListDlg.FormClosing += new FormClosingEventHandler(orderListDlg_FormClosing);
                orderListDlg.StockOrderDeleted += new StockOrderDeletedEventHandler(OnStockOrderDeleted);
                orderListDlg.Show();
            }
            else
            {
                orderListDlg.SetOrderList(portofolio.OrderList);
                orderListDlg.Activate();
            }

            RefreshPortofolioMenu();

        }

        private void batchStrategySimulationMenuItem_Click(object sender, EventArgs e)
        {
            if (batchStrategySimulatorDlg == null || batchStrategySimulatorDlg.IsDisposed)
            {
                var barDuration = new StockBarDuration((BarDuration)this.barDurationComboBox.SelectedItem, (int)this.barSmoothingComboBox.SelectedItem);

                batchStrategySimulatorDlg = new BatchStrategySimulatorDlg(StockDictionary, this.StockPortofolioList, this.selectedGroup, barDuration, this.progressBar);
                batchStrategySimulatorDlg.SimulationCompleted += new SimulationCompletedEventHandler(batchStrategySimulatorDlg_SimulationCompleted);

                this.NotifyBarDurationChanged += batchStrategySimulatorDlg.OnBarDurationChanged;
            }
            else
            {
                batchStrategySimulatorDlg.Activate();
            }
            batchStrategySimulatorDlg.Show();
        }

        private void batchStrategySimulatorDlg_SimulationCompleted(SimulationParameterControl simulationParameterControl)
        {
            //
            RefreshPortofolioMenu();

            // 
            OnNeedReinitialise(true);

            // Open Palmares window Initialised to display batch results
            palmaresMenuItem_Click(simulationParameterControl, null);
        }

        private void simulationTuningDlg_SimulationCompleted(StockPortofolio newPortofolio)
        {
            // Refresh portofolio generated stock
            OnCurrentPortofolioChanged(newPortofolio, true);

            StockDictionary.CreatePortofolioSerie(CurrentPortofolio);

            // Refresh the screen
            OnNeedReinitialise(true);

            //Display portofolio window
            if (portofolioDlg == null)
            {
                portofolioDlg = new PortofolioDlg(StockDictionary, CurrentPortofolio);
                portofolioDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                portofolioDlg.FormClosing += new FormClosingEventHandler(portofolioDlg_FormClosing);
                portofolioDlg.Show();
            }
            else
            {
                portofolioDlg.SetPortofolio(CurrentPortofolio);
                portofolioDlg.Activate();
                portofolioDlg.Refresh();
            }

            RefreshPortofolioMenu();
        }

        private void generateOrderMenuItem_Click(object sender, EventArgs e)
        {
            if (orderGenerationDlg == null || orderGenerationDlg.IsDisposed)
            {
                orderGenerationDlg = new OrderGenerationDlg(StockDictionary, this.StockPortofolioList);
                orderGenerationDlg.SimulationCompleted +=
                   new OrderGenerationDlg.SimulationCompletedEventHandler(orderGenerationDlg_SimulationCompleted);
                orderGenerationDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                orderGenerationDlg.SelectedPortofolioChanged +=
                   new SelectedPortofolioChangedEventHandler(OnCurrentPortofolioChanged);
                orderGenerationDlg.SelectedStockName = this.stockNameComboBox.SelectedItem.ToString();
                orderGenerationDlg.Show();
            }
            else
            {
                orderGenerationDlg.Activate();
            }
        }

        #endregion

        #region STATISTICS

        // Statistics implementation
        //private void generateStat1MenuItem_Click(object sender, System.EventArgs e)
        //{
        //    List<StockDataType> dataList = new List<StockDataType>();
        //    dataList.AddRange(new StockDataType[] { StockDataType.EMA3, StockDataType.EMA6, StockDataType.EMA12, StockDataType.EMA20, StockDataType.EMA26, StockDataType.EMA50, StockDataType.EMA100 });

        //    SortedDictionary<int, int> upTrendHistogram = new SortedDictionary<int, int>();
        //    SortedDictionary<int, int> downTrendHistogram = new SortedDictionary<int, int>();

        //    using (StreamWriter sr = new StreamWriter(Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + this.CurrentStockSerie.StockName + "_TrendStat.csv", false))
        //    {
        //        foreach(StockDataType datatype in dataList)
        //        {
        //            downTrendHistogram.Clear();
        //            upTrendHistogram.Clear();

        //            StatToolkit.TrendCountHistogram(this.CurrentStockSerie, datatype, ref downTrendHistogram, ref upTrendHistogram);
        //            int total = 0;
        //            sr.WriteLine("Data\tNb Days\tDT Occurences\tUP Occurences\tTotal Days");
        //            int maxDays = Math.Max(downTrendHistogram.Keys.Last(), upTrendHistogram.Keys.Last());
        //            for (int i = 1; i <= maxDays; i++)
        //            {
        //                sr.Write(datatype.ToString() + "\t" + i + "\t");
        //                if (downTrendHistogram.ContainsKey(i))
        //                {
        //                    sr.Write(downTrendHistogram[i] + "\t");
        //                    total = downTrendHistogram[i];
        //                }
        //                else
        //                {
        //                    sr.Write(0 + "\t");
        //                    total = 0;
        //                }
        //                if (upTrendHistogram.ContainsKey(i))
        //                {
        //                    sr.Write(upTrendHistogram[i] + "\t");
        //                    total += upTrendHistogram[i];
        //                }
        //                else
        //                {
        //                    sr.Write(0 + "\t");
        //                }
        //                sr.Write(total * i + "\t");
        //                sr.WriteLine();
        //            }
        //        }
        //    }
        //}
        //private void generateStat2MenuItem_Click(object sender, System.EventArgs e)
        //{
        //    List<StockDataType> dataList = new List<StockDataType>();
        //    dataList.AddRange(new StockDataType[] { StockDataType.CLOSE, StockDataType.EMA3, StockDataType.EMA6, StockDataType.EMA12, StockDataType.EMA20, StockDataType.EMA26, StockDataType.EMA50, StockDataType.EMA100 });

        //    SortedDictionary<int, int> upTrendHistogram = new SortedDictionary<int, int>();
        //    SortedDictionary<int, int> downTrendHistogram = new SortedDictionary<int, int>();

        //    using (StreamWriter sr = new StreamWriter(Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + this.selectedGroup.ToString() + "_TrendStat.csv", false))
        //    {
        //        using (StreamWriter sr2 = new StreamWriter(Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + this.selectedGroup.ToString() + "_TrendStat2.csv", false))
        //        {
        //            foreach(StockDataType datatype in dataList)
        //            {
        //                downTrendHistogram.Clear();
        //                upTrendHistogram.Clear();

        //                foreach(StockSerie stockSerie in this.StockDictionary.Values)
        //                {
        //                    if (stockSerie.StockGroup != this.selectedGroup)
        //                    {
        //                        continue;
        //                    }
        //                    StatToolkit.TrendCountHistogram(stockSerie, datatype, ref downTrendHistogram, ref upTrendHistogram);
        //                }

        //                int total = 0;
        //                sr.WriteLine("Data\tNb Days\tDT Occurences\tUP Occurences\tOccurences\tTotal Days");
        //                sr2.Write(datatype.ToString() + "\t");
        //                int maxDays = Math.Max(downTrendHistogram.Keys.Last(), upTrendHistogram.Keys.Last());
        //                for (int i = 1; i <= maxDays; i++)
        //                {
        //                    sr.Write(datatype.ToString() + "\t" + i + "\t");
        //                    if (downTrendHistogram.ContainsKey(i))
        //                    {
        //                        sr.Write(downTrendHistogram[i] + "\t");
        //                        total = downTrendHistogram[i];
        //                    }
        //                    else
        //                    {
        //                        sr.Write(0 + "\t");
        //                        total = 0;
        //                    }
        //                    if (upTrendHistogram.ContainsKey(i))
        //                    {
        //                        sr.Write(upTrendHistogram[i] + "\t");
        //                        total += upTrendHistogram[i];
        //                    }
        //                    else
        //                    {
        //                        sr.Write(0 + "\t");
        //                    }
        //                    sr.Write(total + "\t");
        //                    sr.Write(total * i + "\t");
        //                    sr.WriteLine();

        //                    sr2.Write(total + "\t");
        //                }
        //                sr2.WriteLine();
        //            }
        //        }
        //    }
        //}
        //private void stockStatisticsMenuItem_Click(object sender, EventArgs e)
        //{
        //    StatisticsDialog statisticsDlg = new StatisticsDialog(this.graphCloseControl, this.CurrentStockSerie);
        //    statisticsDlg.Show(this);
        //}

        #endregion

        #region

        private static string commentTitleTemplate = "COMMENT_TITLE_TEMPLATE";
        private static string commentTemplate = "COMMENT_TEMPLATE";
        private static string titleTemplate = "TITLE_TEMPLATE";
        private static string eventTemplate = "EVENT_TEMPLATE";
        private static string imageFileCID = "IMAGE_FILE_CID";
        private static string imageFileLink = "IMAGE_FILE_LINK";

        // static private string htmlEventTemplate = "<P style=\"font-size: x-small\">" + eventTemplate + "</P>";
        private static string htmlEventTemplate = "<br />" + eventTemplate;

        private static string htmlTitleTemplate =
           "<br /><hr width=\"50%\"><B><P style=\"text-align: center; font-size: xx-medium\">" + titleTemplate + "</P></B>";

        private static string htmlMailCommentTemplate = "<P STYLE=\"margin-bottom: 0cm\"><B><U>" + commentTitleTemplate +
                                                        "</U></B></P>" +
                                                        "<P STYLE=\"margin-bottom: 0cm; text-decoration: none\">" +
                                                        commentTemplate + "</P>" +
                                                        "<P STYLE=\"margin-bottom: 0cm; text-decoration: none\"><IMG SRC=\"cid:" +
                                                        imageFileCID + "\" ALIGN=LEFT BORDER=1><BR CLEAR=LEFT><BR></P>";

        private static string htmlCommentTemplate = "<P STYLE=\"margin-bottom: 0cm\"><B><U>" + commentTitleTemplate +
                                                    "</U></B></P>" +
                                                    "<P STYLE=\"margin-bottom: 0cm; text-decoration: none\">" +
                                                    commentTemplate + "</P>" +
                                                    "<P STYLE=\"margin-bottom: 0cm; text-decoration: none\"><IMG SRC=\"" +
                                                    imageFileLink + "\" ALIGN=LEFT BORDER=1><BR CLEAR=LEFT><BR></P>";

        private static string htmlAlertTemplate = "<P STYLE=\"margin-bottom: 0cm\"><B><U>" + commentTitleTemplate +
                                                    "</U></B></P>" +
                                                    "<P STYLE=\"margin-bottom: 0cm; text-decoration: none\">" +
                                                    commentTemplate + "</P>";

        //private void GenerateDailyReport()
        //{
        //   CleanImageFolder();

        //   GenerateDailyReport(true);

        //   if (NetworkInterface.GetIsNetworkAvailable())
        //   {
        //      using (MailMessage message = new MailMessage())
        //      {

        //         message.To.Add(Settings.Default.UserEMail);
        //         message.To.Add("david.carbonel@volvo.com");
        //         message.Subject = "Ultimate Chartist Analysis Report - " + DateTime.Now.ToString();
        //         message.From = new MailAddress("noreply@free.fr");
        //         message.IsBodyHtml = true;
        //         SmtpClient smtp = new SmtpClient(Settings.Default.UserSMTP, 25);
        //         try
        //         {
        //            smtp.Send(message);
        //            System.Windows.Forms.MessageBox.Show("Email sent successfully");
        //         }
        //         catch (System.Exception e)
        //         {
        //            System.Windows.Forms.MessageBox.Show(e.Message, "Email error !");
        //         }
        //      }
        //   }
        //}

        private string GenerateEventReport()
        {
            return string.Empty;
        }

        struct RankedSerie
        {
            public float rank;
            public float previousRank;
            public StockSerie stockSerie;
        }
        string MULTITIMEFRAME_HEADER = @"<html>
<head>
    <title>Untitled Page</title>
   <style>
table
{
border-collapse:collapse;
}
table, td, th
{
border:1px solid black;
}
</style>
</head>
<body>
<h1>%TITLE%</h1>";
        string MULTITIMEFRAME_FOOTER = @"
</body>
</html>";
        string MULTITIMEFRAME_TABLE = @"<table>%TABLE%</table>" + Environment.NewLine;

        string CELL_DIR_IMG_TEMPLATE =
           "<td><img alt=\"%DIR%\" src=\"../../img/%DIR%.png\"/></td>" +
           Environment.NewLine;
        string CELL_TEXT_TEMPLATE = "<td>%TEXT%</td>" + Environment.NewLine;
        string ROW_TEMPLATE = "<tr>%ROW%<tr/>" + Environment.NewLine;

        private string GenerateReport(string title, StockBarDuration[] durations, List<StockAlertDef> alertDefs)
        {
            var duration = durations.First();
            string timeFrame = durations.First().ToString();
            string folderName = Settings.Default.RootFolder + @"\CommentReport\" + timeFrame;
            CleanReportFolder(folderName);

            string fileName = folderName + @"\Report.html";


            string mailReport = string.Empty;
            string htmlReport = $"<h1 style=\"text-align: center;\">{title}</h1>";

            string commentTitle = string.Empty;
            string commentBody = string.Empty;
            ImageFormat imageFormat = ImageFormat.Png;
            List<string> cidList = new List<string>();
            List<string> fileNameList = new List<string>();

            //string hostIP = "ftp://ultimatechartist.com";
            //string userName = "ultimatechartist.com|ultimate";
            //string password = "XU5ZWi0Y";

            //StockFTP ftp = new StockFTP(hostIP, userName, password);
            //string[] files = ftp.directoryListDetailed(".");

            #region report multi TimeFrame

            // Generate header
            string rowContent = CELL_TEXT_TEMPLATE.Replace("%TEXT%", "Stock Name");
            string headerRow = string.Empty;
            foreach (StockBarDuration d in durations)
            {
                rowContent += CELL_TEXT_TEMPLATE.Replace("%TEXT%", d.ToString());
            }
            headerRow = ROW_TEMPLATE.Replace("%ROW%", rowContent);

            string tableContent = headerRow;

            var stockList = this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40));

            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = stockList.Count();
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            foreach (StockSerie serie in stockList)
            {
                try
                {
                    rowContent = CELL_TEXT_TEMPLATE.Replace("%TEXT%", serie.StockName);

                    StockSplashScreen.ProgressVal++;
                    StockSplashScreen.ProgressText = "Downloading " + serie.StockGroup + " - " + serie.StockName;

                    foreach (StockBarDuration d in durations)
                    {
                        serie.BarDuration = d;
                        if (serie.Initialise() && serie.Count > 50)
                        {
                            IStockTrailStop trailStop = serie.GetTrailStop("TRAILHL(1)");
                            if (float.IsNaN(trailStop.Series[0].Last))
                            {
                                rowContent += CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN");
                            }
                            else
                            {
                                rowContent += CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP");
                            }
                        }
                        else
                        {
                            rowContent += CELL_TEXT_TEMPLATE.Replace("%TEXT%", "-");
                        }
                    }
                }
                finally
                {
                    // Add Row
                    tableContent += ROW_TEMPLATE.Replace("%ROW%", rowContent);
                }
            }

            StockSplashScreen.CloseForm(true);
            string table = MULTITIMEFRAME_HEADER.Replace("%TITLE%", title) + MULTITIMEFRAME_TABLE.Replace("%TABLE%", tableContent) + MULTITIMEFRAME_FOOTER;

            using (StreamWriter sw = new StreamWriter(folderName + @"\report_table.html"))
            {
                sw.Write(table);
            }
            #endregion

            #region Report leaders

            this.barDurationComboBox.SelectedItem = StockBarDuration.Daily;

            string rankLeaderIndicatorName = "ROR(100,1,6)";
            string rankLoserIndicatorName = "ROD(100,1,6)";
            int nbLeaders = 10;
            string htmlLeaders = GenerateLeaderLoserTable(duration, StockSerie.Groups.CAC40, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_A, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_B, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.EURO_C, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);

            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.COMMODITY, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);
            htmlLeaders += GenerateLeaderLoserTable(duration, StockSerie.Groups.FOREX, rankLeaderIndicatorName, rankLoserIndicatorName, nbLeaders);

            mailReport += htmlLeaders;
            htmlReport += htmlLeaders;

            #endregion

            #region From Report.cfg
            StockSerie previousStockSerie = this.CurrentStockSerie;
            string previousTheme = this.CurrentTheme;
            StockBarDuration previousBarDuration = previousStockSerie.BarDuration;
            #endregion

            #region Generate report from Events
            foreach (StockAlertDef alert in alertDefs)
            {
                string alertMsg = string.Empty;
                commentTitle = "\r\n" + alert.ToString() + "\r\n";

                foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CACALL)))
                {
                    StockSplashScreen.ProgressVal++;
                    StockSplashScreen.ProgressSubText = "Scanning " + stockSerie.StockName;

                    if (!stockSerie.Initialise() || stockSerie.Count < 200 || (stockSerie.Last().Value.VOLUME * stockSerie.Last().Value.CLOSE) > 50000) continue;

                    if (stockSerie.MatchEvent(alert))
                    {
                        var values = stockSerie.GetValues(alert.BarDuration);
                        string alertLine = stockSerie.StockName + ";" + values.ElementAt(values.Count - 2).DATE +
                                           ";" + alert.ToString();

                        alertMsg += "<br>" + alertLine + ";" + stockSerie.GetValues(StockBarDuration.Daily).Last().CLOSE + "</br>";
                    }
                }
                htmlReport += htmlAlertTemplate.Replace(commentTitleTemplate, commentTitle)
                   .Replace(commentTemplate, alertMsg);
            }
            #endregion

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(htmlReport);
            }

            //           Process.Start("http://www.ultimatechartist.com/CommentReport/report.html");
            Process.Start(fileName);
            this.CurrentStockSerie = previousStockSerie;
            this.CurrentTheme = previousTheme;
            this.barDurationComboBox.SelectedItem = previousBarDuration.Duration;
            this.barSmoothingComboBox.SelectedItem = previousBarDuration.Smoothing;
            this.barHeikinAshiCheckBox.CheckBox.Checked = previousBarDuration.HeikinAshi;

            return mailReport;
        }

        private string GenerateLeaderLoserTable(StockBarDuration duration, StockSerie.Groups reportGroup, string rankLeaderIndicatorName, string rankLoserIndicatorName, int nbLeaders)
        {
            string html = "<table><tr><td>";

            List<RankedSerie> leadersDico = new List<RankedSerie>();
            foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(reportGroup)))
            {
                if (stockSerie.Initialise() && stockSerie.Count > 100)
                {
                    stockSerie.BarDuration = duration;
                    IStockIndicator indicator = stockSerie.GetIndicator(rankLeaderIndicatorName);
                    leadersDico.Add(new RankedSerie()
                    {
                        rank = indicator.Series[0].Last,
                        previousRank = indicator.Series[0][indicator.Series[0].Count - 2],
                        stockSerie = stockSerie
                    });
                }
            }

            string rowTemplate = @"
         <tr>
             <td>%COL1%</td>
             <td>%COL2%</td>
             %RANK_DIR_IMG%
             <td>%COL3%</td>
             %CLOSE_DIR_IMG%
             <td>%COL4%</td>
         </tr>";

            html += htmlTitleTemplate.Replace(titleTemplate, "Leaders for " + reportGroup + " - " + rankLeaderIndicatorName);
            html += " <table>";

            var leaders = leadersDico.OrderByDescending(l => l.rank).Take(nbLeaders);
            foreach (RankedSerie pair in leaders)
            {
                var lastValue = pair.stockSerie.ValueArray.Last();
                html += rowTemplate.
                    Replace("%COL1%", pair.stockSerie.StockName).
                    Replace("%COL2%", (pair.rank / 100f).ToString("P2")).
                    Replace("%COL3%", (lastValue.VARIATION).ToString("P2")).
                    Replace("%COL4%", (lastValue.CLOSE).ToString("#.##"));
                if (pair.previousRank <= pair.rank)
                {
                    html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
                }
                else
                {
                    html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
                }
                if (lastValue.VARIATION > 0)
                {
                    html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
                }
                else
                {
                    html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
                }
            }

            html += " </table>";

            html += "</td><td width=100></td><td>";

            leadersDico.Clear();
            foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(reportGroup)))
            {
                if (stockSerie.Initialise() && stockSerie.Count > 100)
                {
                    stockSerie.BarDuration = StockBarDuration.Daily;
                    IStockIndicator indicator = stockSerie.GetIndicator(rankLoserIndicatorName);
                    leadersDico.Add(new RankedSerie() { rank = -indicator.Series[0].Last, previousRank = -indicator.Series[0][indicator.Series[0].Count - 2], stockSerie = stockSerie });
                }
            }

            html += htmlTitleTemplate.Replace(titleTemplate, "Losers for " + reportGroup + " - " + rankLoserIndicatorName);
            html += " <table>";
            leaders = leadersDico.OrderBy(l => l.rank).Take(nbLeaders);
            foreach (RankedSerie pair in leaders)
            {
                var lastValue = pair.stockSerie.ValueArray.Last();
                html += rowTemplate.
                    Replace("%COL1%", pair.stockSerie.StockName).
                    Replace("%COL2%", (pair.rank / 100f).ToString("P2")).
                    Replace("%COL3%", (lastValue.VARIATION).ToString("P2")).
                    Replace("%COL4%", (lastValue.CLOSE).ToString("#.##"));
                if (pair.previousRank <= pair.rank)
                {
                    html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
                }
                else
                {
                    html = html.Replace("%RANK_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
                }
                if (lastValue.VARIATION > 0)
                {
                    html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
                }
                else
                {
                    html = html.Replace("%CLOSE_DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
                }
            }

            html += " </table>";

            html += "</td></tr></table>";

            return html;
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
            if (System.IO.Directory.Exists(folderName))
            {
                foreach (string directory in (System.IO.Directory.EnumerateDirectories(folderName)))
                {
                    System.IO.Directory.Delete(directory, true);
                }
                foreach (string file in (System.IO.Directory.EnumerateFiles(folderName)))
                {
                    System.IO.File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(folderName);
                Directory.CreateDirectory(folderName + "\\img");
            }
        }

        void addToReportStripBtn_Click(object sender, System.EventArgs e)
        {
            using (StreamWriter sw = File.AppendText(Settings.Default.RootFolder + @"\Report.cfg"))
            {
                sw.WriteLine(this.CurrentStockSerie.StockName + ";" + this.CurrentTheme + ";" + this.barDurationComboBox.SelectedItem.ToString() + ";" + (this.endIndex - this.startIndex));
            }
        }
        private void generateDailyReportToolStripBtn_Click(object sender, EventArgs e)
        {
            var durations = new StockBarDuration[]
            {
            StockBarDuration.Daily,
            StockBarDuration.TLB,
            StockBarDuration.TLB_3D,
            };

            GenerateReport("Daily Report", durations, dailyAlertConfig.AlertDefs);
        }
        #endregion
        private void manageWatchlistsMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null || this.WatchLists == null) return;

            WatchListDlg watchlistDlg = new WatchListDlg(this.WatchLists);
            watchlistDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
            if (watchlistDlg.ShowDialog() == DialogResult.OK)
            {
                this.SaveWatchList();
            }
            else
            { this.LoadWatchList(); }
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
        #region Stock Strategy Scanner Dlg
        private StockStrategyScannerDlg stockStrategyScannerDlg = null;
        private void stockStrategyScannerMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            if (stockStrategyScannerDlg == null)
            {
                stockStrategyScannerDlg = new StockStrategyScannerDlg(StockDictionary, this.selectedGroup, (StockBarDuration)this.barDurationComboBox.SelectedItem, this.currentStrategy);
                stockStrategyScannerDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
                stockStrategyScannerDlg.SelectStockGroupChanged += new SelectedStockGroupChangedEventHandler(this.OnSelectedStockGroupChanged);
                stockStrategyScannerDlg.SelectedStrategyChanged += new SelectedStrategyChangedEventHandler(this.StockAnalyzerForm_StrategyChanged);
                stockStrategyScannerDlg.FormClosing += new FormClosingEventHandler(delegate
                {
                    this.NotifyBarDurationChanged -= stockStrategyScannerDlg.OnBarDurationChanged;
                    this.StrategyChanged -= stockStrategyScannerDlg.OnStrategyChanged;
                    this.stockStrategyScannerDlg = null;
                });
            }
            this.NotifyBarDurationChanged += stockStrategyScannerDlg.OnBarDurationChanged;
            this.StrategyChanged += stockStrategyScannerDlg.OnStrategyChanged;

            stockStrategyScannerDlg.Show();
        }

        #endregion

        private void scriptEditorMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentStockSerie == null) return;
            ScriptDlg scriptEditor = new ScriptDlg();
            scriptEditor.Show();
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
                        inverseSerieMenuItem_Click(this, null);
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
                        this.currentPortofolioMenuItem_Click(null, null);
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
                    case Keys.Control | Keys.F2:
                        {
                            this.GenerateTrainingData();
                        }
                        break;
                    case Keys.F2:
                        {
                            this.portfolioRiskManager_Click(null, null);
                        }
                        break;
                    case Keys.F3:
                        {
                            this.ShowMultiTimeFrameDlg();
                        }
                        break;
                    case Keys.F4:
                        {
                            if (marketReplay == null)
                            {
                                marketReplay = new StockMarketReplay();

                                marketReplay.FormClosing += new FormClosingEventHandler(delegate
                                {
                                    this.marketReplay = null;
                                });
                                marketReplay.Show();
                            }
                            else
                            {
                                marketReplay.Activate();
                            }
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
                    case Keys.F6:
                        {
                            this.GenerateHistogram();
                        }
                        break;
                    case Keys.F7:
                        {
                            this.statisticsMenuItem_Click(null, null);
                        }
                        break;
                    case Keys.Control | Keys.F8: // Display Risk Calculator Windows

                        if (riskCalculatorDlg == null)
                        {
                            riskCalculatorDlg = new StockRiskCalculatorDlg();
                            riskCalculatorDlg.StockSerie = this.CurrentStockSerie;

                            riskCalculatorDlg.FormClosing += new FormClosingEventHandler(delegate
                            {
                                this.riskCalculatorDlg = null;
                            });
                            riskCalculatorDlg.Show();
                        }
                        else
                        {
                            riskCalculatorDlg.Activate();
                        }

                        break;
                    case Keys.Control | Keys.Shift | Keys.F8: // Generate multi time frame trend view.
                        {
                            MTFDlg mtfDlg = new MTFDlg();
                            mtfDlg.MtfControl.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                            mtfDlg.Show();
                        }
                        break;
                    case Keys.F9:
                        {
                            this.RunAgentEngine();
                        }
                        break;
                    case Keys.Control | Keys.F9: // Display Risk Calculator Windows
                        {
                            this.RunAgentEngineOnGroup();
                        }
                        break;
                    case Keys.Control | Keys.G: // Historical group view
                        {
                            var mtfDlg = new GroupViewDlg();
                            mtfDlg.groupUserViewControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                            mtfDlg.Disposed += delegate
                            {
                                mtfDlg.groupUserViewControl1.SelectedStockChanged -= OnSelectedStockAndDurationChanged;
                            };
                            mtfDlg.Show();
                        }
                        break;
                    default:
                        return base.ProcessCmdKey(ref msg, keyData);
                }
            }
            return true;
        }

        private void RunAgentEngine()
        {
            StockAgentEngine engine = new StockAgentEngine();
            var stockSeries = new List<StockSerie> { this.CurrentStockSerie };

            engine.GeneticSelection(20, 100, stockSeries, 100);



        }
        private void RunAgentEngineOnGroup()
        {
            var stockSeries = this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.selectedGroup) && s.Initialise());

            StockAgentEngine engine = new StockAgentEngine();
            engine.GeneticSelection(20, 100, stockSeries, 100);
        }

        private void GenerateEMAHistogram()
        {
            SortedDictionary<int, int> histogram = new SortedDictionary<int, int>();
            histogram.Add(0, 0);
            for (int i = 1; i < 100; i++)
            {
                histogram.Add(i, 0);
                histogram.Add(-i, 0);
            }
            int period = 6;
            FloatSerie emaSerie = this.currentStockSerie.GetIndicator("EMA(" + period + ")").Series[0];
            for (int i = period; i < this.currentStockSerie.Count; i++)
            {
                var value = this.currentStockSerie.ValueArray[i];
                float distPercent = (emaSerie[i] - value.CLOSE) / value.CLOSE;
                int rank = (int)Math.Round(distPercent * 100);
                rank = Math.Min(99, rank);
                histogram[rank]++;
            }
            for (int i = -99; i < 100; i++)
            {
                Console.WriteLine(i.ToString() + "," + histogram[i]);
            }
        }
        private void GenerateNbUpDaysHistogram()
        {
            SortedDictionary<int, int> histogram = new SortedDictionary<int, int>();
            histogram.Add(0, 0);
            for (int i = 1; i < 100; i++)
            {
                histogram.Add(-i, 0);
                histogram.Add(i, 0);
            }
            int period = 6;
            FloatSerie emaSerie = this.currentStockSerie.GetIndicator("EMA(" + period + ")").Series[0];
            bool up = true;
            int cumul = 0;
            for (int i = 1; i < this.currentStockSerie.Count; i++)
            {
                var value = this.currentStockSerie.ValueArray[i];
                if (up)
                {
                    if (value.VARIATION > 0) cumul++;
                    else
                    {
                        histogram[cumul]++;
                        cumul = 0;
                        up = false;
                    }
                }
                else
                {
                    if (value.VARIATION < 0) cumul++;
                    else
                    {
                        histogram[-cumul]++;
                        cumul = 0;
                        up = true;
                    }
                }
            }
            for (int i = -99; i < 100; i++)
            {
                Console.WriteLine(i.ToString() + "," + histogram[i]);
            }
        }
        private void GenerateTrainingData()
        {
            var fileName = Path.Combine(Settings.Default.RootFolder, this.selectedGroup.ToString() + "_Train.csv");
            using (var fs = new StreamWriter(fileName, false))
            {
                fs.WriteLine("Name,Index,Close,Var,indic1,indic2,indic3,indic4,indic5,indic6");
                foreach (var stockSerie in this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.selectedGroup) && s.Initialise()).Take(1))
                {
                    stockSerie.BarDuration = this.BarDuration;
                    var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                    var varSerie = stockSerie.GetSerie(StockDataType.VARIATION);
                    var indic1 = (stockSerie.GetIndicator("STOKS(14,3,3)").Series[0] - 50.0f)/50f;
                    var indic2 = (stockSerie.GetIndicator("STOKS(42,3,3)").Series[0] - 50.0f) / 50f;
                    var indic3 = (stockSerie.GetIndicator("STOKS(126, 3, 3)").Series[0] - 50.0f) / 50f;
                    var indic4 = stockSerie.GetIndicator("SPEED(EMA(20),1)").Series[0];
                    var indic5 = stockSerie.GetIndicator("SPEED(EMA(60),1)").Series[0];
                    var indic6 = stockSerie.GetIndicator("SPEED(EMA(120),1)").Series[0];
                    for (int i = 120; i < stockSerie.Count; i++)
                    {
                        fs.WriteLine($"{stockSerie.StockName},{i},{closeSerie[i]},{varSerie[i]},{indic1[i]},{indic2[i]},{indic3[i]},{indic4[i]},{indic5[i]},{indic6[i]}");
                    }
                }
            }

        }
        private void GenerateHistogram()
        {
            SortedDictionary<int, int> histogram = new SortedDictionary<int, int>();
            histogram.Add(0, 0);
            for (int i = 1; i < 100; i++)
            {
                histogram.Add(-i, 0);
                histogram.Add(i, 0);
            }
            int period = 6;
            FloatSerie emaSerie = this.currentStockSerie.GetIndicator("EMA(" + period + ")").Series[0];
            bool up = true;
            int cumul = 0;
            for (int i = 1; i < this.currentStockSerie.Count; i++)
            {
                var value = this.currentStockSerie.ValueArray[i];
                if (up)
                {
                    if (value.VARIATION > 0) cumul++;
                    else
                    {
                        histogram[cumul]++;
                        cumul = 0;
                        up = false;
                    }
                }
                else
                {
                    if (value.VARIATION < 0) cumul++;
                    else
                    {
                        histogram[-cumul]++;
                        cumul = 0;
                        up = true;
                    }
                }
            }
            for (int i = -99; i < 100; i++)
            {
                Console.WriteLine(i.ToString() + "," + histogram[i]);
            }
        }

        private void ShowMultiTimeFrameDlg()
        {
            MultiTimeFrameChartDlg mtg = new MultiTimeFrameChartDlg();
            mtg.Initialize(this.selectedGroup, this.currentStockSerie);
            mtg.WindowState = FormWindowState.Maximized;
            mtg.ShowDialog();
        }

        private PortfolioRiskManagerDlg portfolioRiskManagerDlg = null;
        private void portfolioRiskManager_Click(object sender, EventArgs e)
        {
            if (portfolioRiskManagerDlg != null)
            {
                portfolioRiskManagerDlg.Activate();
                return;
            }
            try
            {
                this.Cursor = Cursors.WaitCursor;
                portfolioRiskManagerDlg = new PortfolioRiskManagerDlg();
                portfolioRiskManagerDlg.FormClosed += PortfolioRiskManagerDlg_FormClosed;
                portfolioRiskManagerDlg.SelectedStockChanged += OnSelectedStockChanged;

                this.Cursor = Cursors.Arrow;
                portfolioRiskManagerDlg.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void PortfolioRiskManagerDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.portfolioRiskManagerDlg.SelectedStockChanged -= OnSelectedStockChanged;
            this.portfolioRiskManagerDlg = null;
        }

        private StockRiskCalculatorDlg riskCalculatorDlg = null;

        private StockMarketReplay marketReplay = null;

        private Point lastMouseLocation = Point.Empty;
        void MouseMoveOverGraphControl(object sender, System.Windows.Forms.MouseEventArgs e)
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
            FloatSerie secondarySerie = this.currentStockSerie.GenerateSecondarySerieFromOtherSerie(this.StockDictionary[sender.ToString()], StockDataType.CLOSE);
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
        private string currentStrategy;
        public string CurrentStrategy
        {
            get { return currentStrategy; }
            set
            {
                if (strategyComboBox.SelectedItem == null || strategyComboBox.SelectedItem.ToString() != value)
                {
                    strategyComboBox.SelectedItem = value;
                }
                if (currentStrategy != value)
                {
                    currentStrategy = value;
                    if (this.StrategyChanged != null)
                    {
                        this.StrategyChanged(value);
                    }
                    this.ApplyTheme();
                }
            }
        }

        public void SetThemeFromIndicator(string fullName)
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

        public void SetThemeFromStrategy(StockFilteredStrategyBase strategy)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strategy.ToTheme())))
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

        #region HORSE RACE DIALOG
        HorseRaceDlg horseRaceDlg = null;
        void showHorseRaceViewMenuItem_Click(object sender, System.EventArgs e)
        {
            if (horseRaceDlg == null)
            {
                horseRaceDlg = new HorseRaceDlg(this.selectedGroup.ToString(), this.BarDuration);
                horseRaceDlg.Disposed += horseRaceDlg_Disposed;
            }
            horseRaceDlg.Show();
        }

        void horseRaceDlg_Disposed(object sender, EventArgs e)
        {
            this.horseRaceDlg = null;
        }
        #endregion
        #region ALERT DIALOG
        AlertDlg alertDlg = null;
        void showAlertDialogMenuItem_Click(object sender, System.EventArgs e)
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

        private void selectDisplayedIndicatorMenuItem_Click(object sender, EventArgs e)
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


        public event NotifySelectedThemeChangedEventHandler NotifyThemeChanged;
        public event NotifyBarDurationChangedEventHandler NotifyBarDurationChanged;
        public event NotifyStrategyChangedEventHandler StrategyChanged;

        void StockAnalyzerForm_StrategyChanged(string newStrategy)
        {
            this.CurrentStrategy = newStrategy;
        }

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
                    if (this.CurrentTheme == null || this.CurrentStockSerie == null) return;
                    if (!this.CurrentStockSerie.IsInitialised)
                    {
                        this.statusLabel.Text = ("Loading data...");
                        this.Refresh();
                    }
                    if (!this.CurrentStockSerie.Initialise() || this.CurrentStockSerie.Count == 0)
                    {
                        this.DeactivateGraphControls("Data for " + this.CurrentStockSerie.StockName +
                                                     " cannot be initialised");
                        return;
                    }
                    if (this.CurrentStockSerie.StockAnalysis.DeleteTransientDrawings() > 0)
                    {
                        this.CurrentStockSerie.PaintBarCache = null;
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
                            themeDictionary[currentTheme]["CloseGraph"].Add("SECONDARY|" +
                                                                            this.graphCloseControl.SecondaryFloatSerie
                                                                                .Name);
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
                                    this.graphCloseControl.Comments = this.CurrentStockSerie.StockAnalysis.Comments;
                                    this.graphCloseControl.Agenda = this.CurrentStockSerie.Agenda;
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
                                            graphControl.BackgroundColor = Color.FromArgb(int.Parse(colorItem[0]),
                                                int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                            colorItem = fields[2].Split(':');
                                            graphControl.TextBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]),
                                                int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                            graphControl.ShowGrid = bool.Parse(fields[3]);
                                            colorItem = fields[4].Split(':');
                                            graphControl.GridColor = Color.FromArgb(int.Parse(colorItem[0]),
                                                int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                            graphControl.ChartMode =
                                                (GraphChartMode)Enum.Parse(typeof(GraphChartMode), fields[5]);
                                            if (entry.ToUpper() == "CLOSEGRAPH")
                                            {
                                                if (fields.Length >= 7)
                                                {
                                                    this.graphCloseControl.SecondaryPen =
                                                        GraphCurveType.PenFromString(fields[6]);
                                                }
                                                else
                                                {
                                                    this.graphCloseControl.SecondaryPen = new Pen(Color.DarkGoldenrod, 1);
                                                }
                                            }
                                            break;
                                        case "SECONDARY":
                                            if (this.currentStockSerie.SecondarySerie != null)
                                            {
                                                CheckSecondarySerieMenu(fields[1]);
                                                this.graphCloseControl.SecondaryFloatSerie =
                                                    this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(
                                                        this.currentStockSerie.SecondarySerie, StockDataType.CLOSE);
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
                                                        this.graphCloseControl.SecondaryFloatSerie =
                                                            this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(
                                                                this.StockDictionary[fields[1]], StockDataType.CLOSE);
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
                                        case "PAINTBAR":
                                            {
                                                IStockPaintBar paintBar =
                                                    (IStockPaintBar)
                                                        StockViewableItemsManager.GetViewableItem(line,
                                                            this.CurrentStockSerie);
                                                curveList.PaintBar = paintBar;
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
                                if (
                                    !this.CurrentStockSerie.StockAnalysis.DrawingItems.ContainsKey(
                                        this.CurrentStockSerie.BarDuration))
                                {
                                    this.CurrentStockSerie.StockAnalysis.DrawingItems.Add(
                                        this.CurrentStockSerie.BarDuration, new StockDrawingItems());
                                }
                                graphControl.Initialize(curveList, horizontalLines, dateSerie,
                                    CurrentStockSerie.StockName,
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
                        this.currentStockSerie.BelongsToGroup(StockSerie.Groups.COT) ||
                        this.currentStockSerie.BelongsToGroup(StockSerie.Groups.INDICATOR) ||
                        this.currentStockSerie.BelongsToGroup(StockSerie.Groups.NONE))
                    {
                        this.CurrentPortofolio = null;

                        if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.BREADTH) && this.currentStockSerie.DataProvider != StockDataProvider.FINRA)
                        {
                            string[] fields = this.currentStockSerie.StockName.Split('.');
                            this.graphCloseControl.SecondaryFloatSerie =
                                this.CurrentStockSerie.GenerateSecondarySerieFromOtherSerie(
                                    this.StockDictionary[fields[1]], StockDataType.CLOSE);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(this.currentStrategy))
                        {
                            CurrentPortofolio =
                                this.StockPortofolioList.Find(p => p.Name == this.currentStockSerie.StockName + "_P");
                            if (CurrentPortofolio == null)
                            {
                                CurrentPortofolio = new StockPortofolio(this.currentStockSerie.StockName + "_P");
                                CurrentPortofolio.IsSimulation = true;
                                CurrentPortofolio.TotalDeposit = 1000;
                                this.StockPortofolioList.Add(CurrentPortofolio);

                                this.RefreshPortofolioMenu();
                            }

                            var selectedStrategy = StrategyManager.CreateStrategy(this.currentStrategy,
                                this.currentStockSerie,
                                null, true);
                            if (selectedStrategy != null)
                            {
                                float amount = this.currentStockSerie.GetMax(StockDataType.CLOSE) * 100f;

                                CurrentPortofolio.TotalDeposit = amount;
                                CurrentPortofolio.Clear();

                                this.currentStockSerie.GenerateSimulation(selectedStrategy,
                                    Settings.Default.StrategyStartDate, this.currentStockSerie.Keys.Last(),
                                    amount, false,
                                    false, Settings.Default.SupportShortSelling, false, 0.0f, false, 0.0f, 0.0f, 0.0f, CurrentPortofolio);
                            }

                            this.graphCloseControl.Portofolio = CurrentPortofolio;

                            if (portofolioDlg != null)
                            {
                                this.CurrentPortofolio.Initialize();
                                portofolioDlg.SetPortofolio(this.CurrentPortofolio);
                                portofolioDlg.Activate();
                            }
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

        void strategyComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.CurrentStrategy = strategyComboBox.SelectedItem.ToString();
        }

        void themeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
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

                    if (this.NotifyThemeChanged != null)
                    {
                        this.NotifyThemeChanged(this.themeDictionary[this.currentTheme]);
                    }
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error loading theme");
            }
        }
        private void InitialiseStrategyCombo()
        {
            // Initialise Combo values
            strategyComboBox.Items.Clear();

            strategyComboBox.Items.Add(string.Empty);
            foreach (string strategy in StrategyManager.GetStrategyList())
            {
                strategyComboBox.Items.Add(strategy);
            }
        }
        private void InitialiseThemeCombo()
        {
            // Initialise Combo values
            themeComboBox.Items.Clear();

            string folderName = Settings.Default.RootFolder + @"\themes";
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
            }

            while (themeComboBox.Items.Count == 0)
            {
                foreach (string themeName in System.IO.Directory.EnumerateFiles(folderName, "*.thm"))
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

        void defaultThemeStripButton_Click(object sender, System.EventArgs e)
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
        void deleteThemeStripButton_Click(object sender, System.EventArgs e)
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

        void aboutMenuItem_Click(object sender, System.EventArgs e)
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
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
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
        private void configDataProviderMenuItem_Click(object sender, System.EventArgs e)
        {
            var configDialog = ((IConfigDialog)((ToolStripMenuItem)sender).Tag);
            if (configDialog.ShowDialog(this.StockDictionary) == System.Windows.Forms.DialogResult.OK)
            {
                var dataProvider = (IStockDataProvider)configDialog;
                dataProvider.InitDictionary(Settings.Default.RootFolder, this.StockDictionary, true);
                this.CreateGroupMenuItem();
                this.CreateSecondarySerieMenuItem();
                this.CreateRelativeStrengthMenuItem();
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

        private void eraseAllDrawingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.graphCloseControl.EraseAllDrawingItems();
            OnNeedReinitialise(true);
        }
        #endregion
        public void ShowFinancials()
        {
            if (this.currentStockSerie != null)
            {
                if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
                {
                    ABCDataProvider.DownloadFinancial(this.currentStockSerie);
                }
                if (this.currentStockSerie.Financial != null)
                {
                    this.currentStockSerie.Financial.Value = this.currentStockSerie.GetSerie(StockDataType.CLOSE).Last;
                    StockFinancialForm financialForm = new StockFinancialForm(this.currentStockSerie);
                    financialForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No financial information for this stock");
                }
            }
        }
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
                    MessageBox.Show("No financial information for this stock");
                }
            }
        }

        internal void OpenInFTMenu()
        {
            if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
            {
                string url = "https://markets.ft.com/data/equities/tearsheet/profile?s=%SYMBOL%:PAR";
                url = url.Replace("%SYMBOL%", this.currentStockSerie.ShortName);
                {
                    Process.Start(url);
                }
            }
        }

        internal void OpenInABCMenu()
        {
            if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.CACALL))
            {
                string url = "https://www.abcbourse.com/graphes/display.aspx?s=%SYMBOL%p";
                url = url.Replace("%SYMBOL%", this.currentStockSerie.ShortName);
                {
                    Process.Start(url);
                }
            }
        }
    }
}