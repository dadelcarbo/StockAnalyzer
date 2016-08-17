using StockAnalyzer;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockSecurity;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzer.StockWeb;
using StockAnalyzerApp.CustomControl;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.CustomControl.HorseRaceDlgs;
using StockAnalyzerApp.CustomControl.IndicatorDlgs;
using StockAnalyzerApp.CustomControl.PortofolioDlgs;
using StockAnalyzerApp.CustomControl.SimulationDlgs;
using StockAnalyzerApp.CustomControl.WatchlistDlgs;
using StockAnalyzerApp.Localisation;
using StockAnalyzerApp.StockScripting;
using StockAnalyzerSettings.Properties;
using StockNeuralNetwork;
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
using System.Xml.Serialization;
using StockAnalyzerApp.CustomControl.AlertDialog;
using StockAnalyzerApp.CustomControl.MultiTimeFrameDlg;
using StockAnalyzerApp.CustomControl.StatisticsDlg;

namespace StockAnalyzerApp
{
   public partial class StockAnalyzerForm : Form
   {
      public delegate void SelectedStockChangedEventHandler(string stockName, bool ativateMainWindow);
      public delegate void SelectedStockAndDurationChangedEventHandler(string stockName, StockSerie.StockBarDuration barDuration, bool ativateMainWindow);

      public delegate void SelectedStockGroupChangedEventHandler(string stockgroup);

      public delegate void SelectedStrategyChangedEventHandler(string strategyName);

      public delegate void NotifySelectedThemeChangedEventHandler(Dictionary<string, List<string>> theme);

      public delegate void NotifyBarDurationChangedEventHandler(StockSerie.StockBarDuration barDuration);

      public delegate void NotifyStrategyChangedEventHandler(string newStrategy);

      public delegate void SelectedPortofolioChangedEventHandler(StockPortofolio portofolio, bool ativateMainWindow);

      public delegate void SelectedPortofolioNameChangedEventHandler(string portofolioName, bool ativateMainWindow);

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
      public StockPortofolioList StockPortofolioList { get; private set; }

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
      private string currentWatchList = null;


      public StockSerie.StockBarDuration BarDuration
      {
         get { return (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem; }
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

      private static int nbBars;

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
         this.StockDictionary.ReportProgress +=
            new StockAnalyzer.StockClasses.StockDictionary.ReportProgressHandler(StockDictionary_ReportProgress);

         nbBars = Settings.Default.DefaultBarNumber;

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
      }

      private void Form1_Load(object sender, EventArgs e)
      {
         Thread.CurrentThread.CurrentUICulture = EnglishCulture;
         Thread.CurrentThread.CurrentCulture = EnglishCulture;
         // Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");

         // Graphical initialisation
         StockSplashScreen.ProgressText = "Checking license";
         StockSplashScreen.ProgressVal = 0;
         StockSplashScreen.ProgressMax = 100;
         StockSplashScreen.ProgressMin = 0;
         StockSplashScreen.ShowSplashScreen();

         StockLog.Write("GetFolderPath: " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

         // This is the first time the user runs the application.
         Settings.Default.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                       @"\UltimateChartistRoot";

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

         if (this.StockDictionary.ContainsKey("SP500"))
         {
            StockSerie cashSerie = this.StockDictionary["SP500"].GenerateCashStockSerie();
            this.StockDictionary.Add(cashSerie.StockName, cashSerie);
         }

#if DEBUG
         bool fastStart = true;
#else
         bool fastStart = false;
#endif
         if (!fastStart)
         {
            if (Settings.Default.SupportIntraday)
            {
               //
               StockSplashScreen.ProgressText = "Parsing intraday data...";
               StockSplashScreen.ProgressVal = 50;
               ParseIntraday();
            }

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

            // Generate position indicator1Name
            List<StockSerie.Groups> groups = new List<StockSerie.Groups>()
            {
               StockSerie.Groups.CAC40,
               StockSerie.Groups.COMMODITY,
               StockSerie.Groups.COUNTRY,
               StockSerie.Groups.CURRENCY
            };

            this.GroupReference = new SortedDictionary<StockSerie.Groups, StockSerie>();
            this.GroupReference.Add(StockSerie.Groups.CAC40, this.StockDictionary["CAC40"]);
            this.GroupReference.Add(StockSerie.Groups.COMMODITY, this.StockDictionary["GOLD"]);
            this.GroupReference.Add(StockSerie.Groups.COUNTRY, this.StockDictionary["SP500"]);
            this.GroupReference.Add(StockSerie.Groups.CURRENCY, this.StockDictionary["SP500"]);

            //GeneratePosition(groups);

            // Generate Vix Premiu
            StockSplashScreen.ProgressText = "Generating VIX Premium data...";
            //GenerateVixPremium();

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

            for (int i = 2; i < 40; i+=1)
            {
               //GenerateIndex_Event("CAC40", "EMA_", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("CAC40", "EMA_INV", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("CAC40", "HLAVG_", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("CAC40", "HLAVG_INV", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("CAC40", "EMA_", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("CAC40", "EMA_INV", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("CAC40", "HLAVG_", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("CAC40", "HLAVG_INV", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
             
               //GenerateIndex_Event("SP500", "EMA_", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("SP500", "EMA_INV", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("SP500", "HLAVG_", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("SP500", "HLAVG_INV", StockSerie.StockBarDuration.Daily, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("SP500", "EMA_", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("SP500", "EMA_INV", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILEMA(%PERIOD1%)", "DownTrend");
               //GenerateIndex_Event("SP500", "HLAVG_", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "UpTrend");
               //GenerateIndex_Event("SP500", "HLAVG_INV", StockSerie.StockBarDuration.TLB_EMA3, i, 0, "TRAILSTOP|TRAILHLAVG(%PERIOD1%)", "DownTrend");
            }
            StockSplashScreen.ProgressText = "Generating CAC SAR...";
            //for (float i = 0.0001f; i <= 0.001f; i += 0.0001f)
            //{
            //   GenerateCAC_SAR(true, i);
            //}

            StockSplashScreen.ProgressText = "Generating CAC Random...";
            //GenerateCAC_Random();

            StockSplashScreen.ProgressText = "Generating CAC RANK_" + 3 + " Daily...";
            //GenerateCAC_Event("CAC_RANK_", StockSerie.StockBarDuration.Daily, 3, "INDICATOR|RANK(%PERIOD%,10,20,0)", "Overbought");

            for (int i = 10; i <= 500; i += 5)
            {
               StockSplashScreen.ProgressText = "Generating CAC CCIEX_" + i + " Daily...";
               //GenerateCAC_Event("CAC40", StockSerie.StockBarDuration.Daily, i, "INDICATOR|STOKF(%PERIOD%,1,61.8,38.2)", "Overbought",false);
            }
            for (int i = 15; i <= 40; i += 1)
            {
               for (int j = 40; j <= 60; j++)
               {
                  StockSplashScreen.ProgressText = "Generating SRD STOCK" + i + " Daily...";
                  //GenerateIndex_Event("CAC40", "STOCK", StockSerie.StockBarDuration.Daily, i, j, "INDICATOR|STOKF(%PERIOD1%,1,%PERIOD2%,38.2)", "Overbought");
               }
            }
            for (int i = 2; i <= 50; i += 1)
            {
               //StockSplashScreen.ProgressText = "Generating CAC EMA_" + i + " Daily...";
               //GenerateCAC_Event("CAC_TRAILHMA_UP_", StockSerie.StockBarDuration.Daily, i, "TRAILSTOP|TRAILHMA(%PERIOD%,1)", "UpTrend", false);
               //StockSplashScreen.ProgressText = "Generating CAC EMA_" + i + " Daily...";
               //GenerateCAC_Event("CAC_TRAILHMA_DOWN_", StockSerie.StockBarDuration.Daily, i, "TRAILSTOP|TRAILHMA(%PERIOD%,1)", "DownTrend", false);
            }
            int period = 11;
            int smoothing = 7;
            for (int i = 1; i <= period; i++)
            {
               for (int j = 1; j <= smoothing; j++, j++)
               {
                  //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily...";
                  //GenerateCAC_Event("CAC_TRAILHLS_UP_", StockSerie.StockBarDuration.Daily, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                  //GenerateCAC_Event("CAC_TRAILHLS_DOWN_", StockSerie.StockBarDuration.Daily, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "DownTrend", false);
                  //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_3...";
                  //GenerateCAC_Event("CAC_TRAILHLS_", StockSerie.StockBarDuration.Daily_EMA3, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                  //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_6...";
                  //GenerateCAC_Event("CAC_TRAILHLS_", StockSerie.StockBarDuration.Daily_EMA6, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                  //StockSplashScreen.ProgressText = "Generating CAC TRAILHLS_" + i + "," + j + " Daily_9...";
                  //GenerateCAC_Event("CAC_TRAILHLS_", StockSerie.StockBarDuration.Daily_EMA9, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
                  //StockSplashScreen.ProgressText = "Generating CAC TRAILEMA_" + i + "," + j + " TLB...";
                  //GenerateCAC_Event("CAC_TRAILHLS_", StockSerie.StockBarDuration.TLB, i, j,
                  //   "TRAILSTOP|TRAILHLS(%PERIOD1%,%PERIOD2%)", "UpTrend", false);
               }
            }
            //for (int i = 90; i <= 100; i += 10)
            //{
            //   StockSplashScreen.ProgressText = "Generating CAC TRAILEMA_" + i + " Daily...";
            //   GenerateCAC2_Event("CAC_TRAILEMA_TLB_", StockSerie.StockBarDuration.Daily, i, "TRAILSTOP|TRAILEMA(%PERIOD%,%PERIOD%)", "UpTrend", true);
            //} 
            //for (int i = 10; i <= 500; i += 5)
            //{
            //   StockSplashScreen.ProgressText = "Generating CAC HMA_" + i + " Daily...";
            //   GenerateCAC_Event("CAC_HMA_", StockSerie.StockBarDuration.Daily, i, "INDICATOR|HMA(%PERIOD%)", "PriceAbove");
            //   GenerateCAC_Event("CAC_HMA_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|HMA(%PERIOD%)", "PriceAbove");
            //}
            for (int i = 1; i <= 400; i++)
            {
               //StockSplashScreen.ProgressText = "Generating CAC PUKE_" + i + " Daily...";
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Daily, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB, i, "INDICATOR|PUKE(%PERIOD%,3,20,3)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
            }
            for (int i = 30; i <= 230; i += 5)
            {
               for (int j = 1; j < 20; j += 2)
               {
                  //StockSplashScreen.ProgressText = "Generating CAC RSI_" + i + "_" + j + " Daily...";
                  //GenerateCAC_Event("CAC_RSI", StockSerie.StockBarDuration.Daily, i, j, "INDICATOR|RSI(%PERIOD1%,50,50,%PERIOD2%)", "Overbought", true);
               }
            }
            for (int i = 10; i <= 60; i += 10)
            {
               for (int j = 2; j < 6; j += 1)
               {
                  //StockSplashScreen.ProgressText = "Generating CAC ER_" + i + "_" + j + " Daily...";
                  //GenerateCAC_Event("CAC_ER", StockSerie.StockBarDuration.Daily, i, j, "TRAIL|SAR(0,0.0005)|ER(%PERIOD1%,%PERIOD2%,1)", "UpTrend", false);
                  //GenerateCAC_Event("CAC_ER_BREADTH", StockSerie.StockBarDuration.Daily, i, j, "TRAIL|SAR(0,0.0005)|ER(%PERIOD1%,%PERIOD2%,1)", "UpTrend", true);
               }
            }
            for (int i = 5; i <= 200; i += 5)
            {
               //StockSplashScreen.ProgressText = "Generating CAC MACD_" + i + " Daily...";
               //GenerateCAC_Event("CAC_MACD_", StockSerie.StockBarDuration.Daily, i+1, i, "INDICATOR|MACD(%PERIOD1%,%PERIOD2%,2)", "MACDAboveSignal");
               //GenerateCAC_Event("CAC_MACD_", StockSerie.StockBarDuration.Daily, i, i/2, "INDICATOR|MACD(%PERIOD1%,%PERIOD2%,2)", "MACDAboveSignal");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB, i, "INDICATOR|PUKE(%PERIOD%,3,20,3)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
            }
            for (int i = 1; i <= 20; i++)
            {
               //StockSplashScreen.ProgressText = "Generating CAC TL_" + i + " Daily...";
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Daily, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.Bar_1_EMA9, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_TL_", StockSerie.StockBarDuration.Daily, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
               //GenerateCAC_Event("CAC_TL_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
               //GenerateCAC_Event("CAC_TL_", StockSerie.StockBarDuration.TLB_EMA3, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
               //GenerateCAC_Event("CAC_TL_", StockSerie.StockBarDuration.TLB, i, "PAINTBAR|TRENDLINEHL(%PERIOD%,10)", "UpTrend");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA3, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
               //GenerateCAC_Event("CAC_PUKE_", StockSerie.StockBarDuration.TLB_EMA6, i, "INDICATOR|PUKE(%PERIOD%,3,0,10)", "Bullish");
            }
            for (int i = 1; i <= 51; i += 10)
            {
               //StockSplashScreen.ProgressText = "Generating CAC TRAILHL_" + i + " Daily...";
               //GenerateCAC_Event("CAC_HL_", StockSerie.StockBarDuration.Daily, i, "TRAILSTOP|TRAILHL(%PERIOD%)", "UpTrend", false);
               //GenerateCAC_Event("CAC_HL_", StockSerie.StockBarDuration.Bar_1_EMA6, i, "TRAILSTOP|TRAILHL(%PERIOD%)", "UpTrend", false);
            }
            //for (int i = 25; i <= 500; i+=25)
            //{
            //   StockSplashScreen.ProgressText = "Generating CAC RSI_" + i + " Daily...";
            //   GenerateCAC_Event("CAC_RSI_", StockSerie.StockBarDuration.Daily, i, "INDICATOR|RSI(%PERIOD%,50,50)", "Overbought");
            //   GenerateCAC_Event("CAC_RSI_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|RSI(%PERIOD%,50,50)", "Overbought");
            //} 
            for (int i = 25; i <= 500; i += 25)
            {
               //StockSplashScreen.ProgressText = "Generating CAC RANK_" + i + " Daily...";
               //GenerateCAC_Event("CAC_RANK_", StockSerie.StockBarDuration.Daily, i, "INDICATOR|RSI(%PERIOD%,50,50)", "Overbought");
               //GenerateCAC_Event("CAC_RANK_", StockSerie.StockBarDuration.Bar_1_EMA3, i, "INDICATOR|RSI(%PERIOD%,50,50)", "Overbought");
            }
         }

         string best = string.Empty;
         float max = float.MinValue;
         foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.INDICES_CALC)))
         {
            if (stockSerie.Initialise() && stockSerie.Values.Last().CLOSE > max)
            {
               max = stockSerie.Values.Last().CLOSE;
               best = stockSerie.StockName;
            }
         }
         StockLog.Write("Best index " + best);
         //MessageBox.Show("Best index " + best);

         // Deserialize saved orders
         StockSplashScreen.ProgressText = "Reading portofolio data...";
         ReadPortofolios();

         // Initialise dico
         StockSplashScreen.ProgressText = "Initialising menu items...";

         // Create Groups menu items
         CreateGroupMenuItem();

         // Update dynamic menu
         InitialiseBarDurationComboBox();
         CreateRelativeStrengthMenuItem();
         CreateSecondarySerieMenuItem();

         // Update dynamic menu
         InitDataProviderMenuItem();

         // Watchlist menu item
         this.LoadWatchList();
         InitialiseWatchListComboBox();

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
         
         // 
         InitialiseStockCombo();

         //
         InitialiseStrategyCombo();

         this.Show();
         this.progressBar.Value = 0;
         this.showShowStatusBarMenuItem.Checked = Settings.Default.ShowStatusBar;
         this.statusStrip1.Visible = Settings.Default.ShowStatusBar;
         this.showDrawingsMenuItem.Checked = Settings.Default.ShowDrawings;
         this.showEventMarqueeMenuItem.Checked = Settings.Default.ShowEventMarquee;
         this.showIndicatorDivMenuItem.Checked = Settings.Default.ShowIndicatorDiv;

         this.StockSerieChanged += new OnStockSerieChangedHandler(StockAnalyzerForm_StockSerieChanged);
         this.ThemeChanged += new OnThemeChangedHandler(StockAnalyzerForm_ThemeChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(graphScrollerControl_ZoomChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphCloseControl.OnZoomChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator2Control.OnZoomChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator3Control.OnZoomChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator1Control.OnZoomChanged);
         this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphVolumeControl.OnZoomChanged);

         StockSplashScreen.ProgressText = "Loading "+this.CurrentStockSerie.StockName+" data...";
         if (this.CurrentStockSerie.StockName.StartsWith("INT_"))
         {
            this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.TLB_6D;
         }
         else
         {
            this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.Daily;
         }
         this.StockAnalyzerForm_StockSerieChanged(this.CurrentStockSerie, false);

         StockSplashScreen.CloseForm(true);

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

         this.Focus();

         // Refreshes intrady every 2 minutes.
         refreshTimer = new System.Windows.Forms.Timer();
         refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
         refreshTimer.Interval = 120 * 1000;
         refreshTimer.Start();

         if (Settings.Default.GenerateDailyReport)
         {
            string fileName = Settings.Default.RootFolder + @"\CommentReport\report.html";
            if (!File.Exists(fileName) || File.GetLastWriteTime(fileName).Date != DateTime.Today)
            {
               GenerateDailyReport();
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
      }


      #region TIMER MANAGEMENT

      private void refreshTimer_Tick(object sender, EventArgs e)
      {
         if (this.currentStockSerie != null && (this.currentStockSerie.StockGroup == StockSerie.Groups.INTRADAY ||  this.currentStockSerie.StockGroup == StockSerie.Groups.TURBO))
         {
            this.Cursor = Cursors.WaitCursor;

            try
            {
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
            finally
            {
               this.Cursor = Cursors.Arrow;
            }
         }
      }

      private StockAlertDef cciEx = new StockAlertDef(StockSerie.StockBarDuration.TLB_9D_EMA3, "DECORATOR", "DIVWAIT(1.5,1)|CCIEX(50,12,20,0.0195,75,-75)", "ExhaustionBottom");
      private StockAlertDef barAbove = new StockAlertDef(StockSerie.StockBarDuration.TLB_27D_EMA3, "INDICATOR", "HMA(30)", "FirstBarAbove");
      private StockAlertDef barBelow = new StockAlertDef(StockSerie.StockBarDuration.TLB_27D_EMA3, "INDICATOR", "HMA(30)", "FirstBarBelow");
      private StockAlertDef ResistanceBroken = new StockAlertDef(StockSerie.StockBarDuration.TLB_9D_EMA3, "PAINTBAR", "TRENDLINEHL(1,10)", "ResistanceBroken");
      private StockAlertDef trailHL = new StockAlertDef(StockSerie.StockBarDuration.TLB_9D_EMA3, "TRAILSTOP", "TRAILHLS(2,3)", "BrokenUp");
      private StockAlertDef trailHLSR = new StockAlertDef(StockSerie.StockBarDuration.TLB_9D, "INDICATOR", "TRAILHLSR(5)", "ResistanceBroken");

      private List<StockAlertDef> alertDefs = new List<StockAlertDef>();

      private void alertTimer_Tick(object sender, EventArgs e)
      {
         if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday ||
             DateTime.Now.Hour < 8 || DateTime.Now.Hour > 18) return;


         Thread alertThread = new Thread(GenerateAlert);
         alertThread.Start();
      }

      public void GenerateAlert()
      {
         alertDefs.Clear();
         alertDefs.Add(cciEx);
         alertDefs.Add(barAbove);
         alertDefs.Add(barBelow);
         alertDefs.Add(trailHL);
         alertDefs.Add(ResistanceBroken);
         alertDefs.Add(trailHLSR);

         var stockList = this.WatchLists.Find(wl => wl.Name == "Alert").StockList;
         if (AlertDetectionStarted!=null)
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
         
         StockAlertLog stockAlertLog = StockAlertLog.Instance;

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

            StockSerie.StockBarDuration previouBarDuration = stockSerie.BarDuration;

            foreach (StockAlertDef alertDef in alertDefs)
            {
               stockSerie.BarDuration = alertDef.BarDuration;
               var values = stockSerie.GetValues(alertDef.BarDuration);
               for (int i = values.Count - 2; i > 0 && values[i].DATE > lookBackDate; i--)
               {
                  var dailyValue = values.ElementAt(i);
                  if (stockSerie.MatchEvent(alertDef,i))
                  {
                     StockAlert stockAlert = new StockAlert(alertDef,
                        dailyValue.DATE,
                        stockSerie.StockName,
                        dailyValue.CLOSE,
                        stockSerie.GetValues(StockSerie.StockBarDuration.Daily).Last().CLOSE);

                     if (!stockAlertLog.Alerts.Any(a => a == stockAlert))
                     {
                        if (this.InvokeRequired)
                        {
                           this.Invoke(new Action(() => stockAlertLog.Alerts.Insert(0,stockAlert)));
                        }
                        else
                        {
                           stockAlertLog.Alerts.Insert(0,stockAlert);
                        }
                     }
                  }
               }
            }
            stockSerie.BarDuration = previouBarDuration;
         }
         stockAlertLog.Save();

         if (this.AlertDetected != null)
         {
            this.Invoke(this.AlertDetected);
         }

         StockSplashScreen.CloseForm(true);
      }

      #endregion

      private System.Windows.Forms.Timer refreshTimer;
      private System.Windows.Forms.Timer alertTimer;

      private bool CheckLicense()
      {
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
         nbBars = endIndex - startIndex;
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
         nbBars = Math.Max(25, nbBars / 2);
         int newIndex = Math.Max(0, endIndex - nbBars);
         if (newIndex != this.startIndex)
         {
            this.ChangeZoom(newIndex, endIndex);
         }
      }

      private void ZoomOut()
      {
         nbBars = Math.Min(this.endIndex, nbBars * 2);
         int newIndex = endIndex - nbBars;
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
         if (stockName.EndsWith("_P") || stockName == "Default")
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

            this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.Daily;
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
      public void OnSelectedStockAndDurationChanged(string stockName, StockSerie.StockBarDuration barDuration ,bool activate)
      {
         if (stockName.EndsWith("_P") || stockName == "Default")
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
         
         this.barDurationComboBox.SelectedItem = barDuration;

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
         if (!newSerie.Initialise())
         {
            DeactivateGraphControls("No data to display");
            this.Text = "Ultimate Chartist - " + "Failure Loading data selected";
            return;
         }

         // TODO Manage COT Series
         if (this.currentStockSerie.StockName.EndsWith("_COT"))
         {
            this.ForceBarDuration(StockSerie.StockBarDuration.Weekly, false);
         }
         else
         {
            this.currentStockSerie.BarDuration = (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem;
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

      private void ParseIntraday()
      {
         // Parse Intraday data
         string stockName = null;
         string shortName = null;
         string previousStockName = null;

         StockBarSerie tickBarSerie1 = null;
         StockBarSerie intradayBarSerie = null;
         StockBarSerie rangeBarSerie1 = null;

         if (Directory.Exists(Settings.Default.RootFolder + INTRADAY_SUBFOLDER))
         {
            foreach (
               string fileFullName in
                  Directory.GetFiles(Settings.Default.RootFolder + INTRADAY_SUBFOLDER).OrderBy(s => s))
            {
               StockTick[] ticks = StockTick.ParseEuronextFile(fileFullName, -1);
               stockName = fileFullName.Split('_')[1];
               shortName = fileFullName.Split('_')[0];

               if (stockName != previousStockName)
               {
                  StockSplashScreen.ProgressText = "Parsing intraday data " + stockName;
                  if (previousStockName != null)
                  {
                     this.StockDictionary.Add(tickBarSerie1.Name, new StockSerie(tickBarSerie1, StockSerie.Groups.TICK));
                     this.StockDictionary.Add(intradayBarSerie.Name,
                        new StockSerie(intradayBarSerie, StockSerie.Groups.INTRADAY));
                     this.StockDictionary.Add(rangeBarSerie1.Name,
                        new StockSerie(rangeBarSerie1, StockSerie.Groups.RANGE));
                  }
                  previousStockName = stockName;
                  tickBarSerie1 = StockBarSerie.CreateTickBarSerie(stockName + "_TICK1", shortName,
                     Math.Max(ticks.Length / 720, 5), ticks);
                  intradayBarSerie = StockBarSerie.CreateIntradayBarSerie(stockName + "_5MIN", shortName, 5, ticks);
                  rangeBarSerie1 = StockBarSerie.CreateRangeBarSerie(stockName + "_range1", shortName,
                     ticks.Last().Value / 200, ticks);
               }
               else
               {
                  tickBarSerie1.Append(ticks);
                  intradayBarSerie.Append(ticks);
                  rangeBarSerie1.Append(ticks);
               }
            }
            if (tickBarSerie1 != null)
            {
               this.StockDictionary.Add(tickBarSerie1.Name, new StockSerie(tickBarSerie1, StockSerie.Groups.TICK));
               this.StockDictionary.Add(intradayBarSerie.Name,
                  new StockSerie(intradayBarSerie, StockSerie.Groups.INTRADAY));
               this.StockDictionary.Add(rangeBarSerie1.Name, new StockSerie(rangeBarSerie1, StockSerie.Groups.RANGE));
            }
         }
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
         if (string.IsNullOrEmpty(Settings.Default.PortofolioFile))
         {
            Settings.Default.PortofolioFile = "Portfolio.xml";
         }
         // Read Stock Values from XML
         string orderFileName = Path.Combine(Settings.Default.RootFolder, Settings.Default.PortofolioFile);
         try
         {
            // Parsing portofolios
            if (System.IO.File.Exists(orderFileName))
            {
               XmlSerializer serializer = new XmlSerializer(typeof(StockPortofolioList));
               using (FileStream fs = new FileStream(orderFileName, FileMode.Open))
               {
                  System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                  settings.IgnoreWhitespace = true;
                  System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);

                  this.StockPortofolioList = (StockPortofolioList)serializer.Deserialize(xmlReader);
               }
            }
            else
            {
               this.StockPortofolioList = new StockPortofolioList();
               this.StockPortofolioList.Add(new StockPortofolio("BinckPEA_P", 10000));
               this.StockPortofolioList.Add(new StockPortofolio("BinckTitre_P", 10000));
               this.SavePortofolios();
            }

            // Generate Portfolio Series
            foreach (var portfolio in this.StockPortofolioList)
            {
               portfolio.Initialize(this.StockDictionary);
               StockSerie portfolioSerie = new StockSerie(portfolio.Name, portfolio.Name, StockSerie.Groups.Portfolio, StockDataProvider.Portofolio);
               this.StockDictionary.Add(portfolio.Name, portfolioSerie);
            }
            
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
         this.portofolioFilterMenuItem.DropDownItems.Clear();

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
            // Create portofoglio menu items
            portofolioDetailsSubMenuItem = new ToolStripMenuItem(portofolio.Name);
            portofolioDetailsSubMenuItem.Click += new EventHandler(this.viewPortogolioMenuItem_Click);
            portofolioMenuItems[i] = portofolioDetailsSubMenuItem;

            // Create portofoglio menu items
            portofolioFilterSubMenuItem = new ToolStripMenuItem(portofolio.Name);
            portofolioFilterSubMenuItem.CheckOnClick = true;
            portofolioFilterSubMenuItem.Click += new EventHandler(portofolioFilterSubMenuItem_Click);
            portofolioFilterMenuItems[i] = portofolioFilterSubMenuItem;

            // create order list menu items
            orderListSubMenuItem = new ToolStripMenuItem(portofolio.Name);
            orderListSubMenuItem.Click += new EventHandler(this.orderListMenuItem_Click);
            orderListMenuItems[i++] = orderListSubMenuItem;
         }
         this.portofolioDetailsMenuItem.DropDownItems.AddRange(portofolioMenuItems);
         this.orderListMenuItem.DropDownItems.AddRange(orderListMenuItems);
         this.portofolioFilterMenuItem.DropDownItems.AddRange(portofolioFilterMenuItems);
      }

      private void portofolioFilterSubMenuItem_Click(object sender, EventArgs e)
      {
         foreach (ToolStripMenuItem subItem in this.portofolioFilterMenuItem.DropDownItems)
         {
            if (subItem != (ToolStripMenuItem)sender)
            {
               subItem.Checked = false;
            }
         }

         if (((ToolStripMenuItem)sender).Checked)
         {
            this.CurrentPortofolio = this.StockPortofolioList.Get(sender.ToString());
            this.currentWatchList = null;
            foreach (ToolStripMenuItem menuItem in this.watchListMenuItem.DropDownItems)
            {
               menuItem.Checked = false;
            }
         }
         else
         {
            this.CurrentPortofolio = null;
         }

         this.InitialiseStockCombo();
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

      private void InitialiseStockCombo()
      {
         // Initialise Combo values
         stockNameComboBox.Items.Clear();
         stockNameComboBox.SelectedItem = string.Empty;

         List<string> stocks;

         List<String> stockList = new List<String>();
         var series = StockDictionary.Values.Where(s => s.BelongsToGroup(this.selectedGroup));
         foreach (StockSerie stockSerie in series)
         {
            if ((!showOnlyEventMenuItem.Checked) || stockSerie.HasEvents)
            {
               stockList.Add(stockSerie.StockName);
            }
         }
         stocks = stockList;
         foreach (string stockName in stocks)
         {
            if (StockDictionary.Keys.Contains(stockName))
            {
               StockSerie stockSerie = StockDictionary[stockName];

               // Check if in exclusion list
               if ((!showOnlyEventMenuItem.Checked) || stockSerie.HasEvents)
               {
                  stockNameComboBox.Items.Add(stockName);
               }
            }
         }
         // 
         if (stockNameComboBox.Items.Count != 0)
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
         if (Control.ModifierKeys == Keys.Control)
         {
            DownloadStockGroup();
         }
         else
         {
            DownloadStock();
         }
      }

      private void DownloadStock()
      {
         if (this.currentStockSerie != null)
         {
            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressText = "Downloading " + this.currentStockSerie.StockGroup + " - " +
                                             this.currentStockSerie.StockName;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            if (StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, this.currentStockSerie))
            {
               if (this.currentStockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.SRD) || currentStockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.EURONEXT))
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

            StockSplashScreen.CloseForm(true);
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

               if (stockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.SRD) || stockSerie.BelongsToGroup(StockAnalyzer.StockClasses.StockSerie.Groups.EURONEXT))
               {
                  try
                  {
                     StockSplashScreen.ProgressText = "Downloading Agenda " + stockSerie.StockGroup + " - " +
                                                      stockSerie.StockName;
                     ABCDataProvider.DownloadAgenda(stockSerie);
                  }
                  catch (Exception ex)
                  {
                     StockLog.Write(ex);
                  }
               }

               StockSplashScreen.ProgressVal++;
            }
            
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

      private void showOnlyEventMenuItem_Click(object sender, EventArgs e)
      {
         showOnlyEventMenuItem.Checked = !showOnlyEventMenuItem.Checked;
         if (showOnlyEventMenuItem.Checked)
         {
            this.progressBar.Value = 0;
            this.progressBar.Maximum = StockDictionary.Count;
            StockDictionary.DetectEvents(new StockDictionary.OnSerieEventDetectionDone(OnSerieEventProcessed),
               this.StockPortofolioList, Settings.Default.SelectedEvents);
            this.progressBar.Value = 0;
         }
         InitialiseStockCombo();
      }

      private void selectEventsMenuItem_Click(object sender, EventArgs e)
      {
         ResetDrawingButtons();

         StockEventSelectorDlg stockEventSelectorDlg = new StockEventSelectorDlg(Settings.Default.SelectedEvents,
            (StockEvent.EventFilterMode)
               Enum.Parse(typeof(StockEvent.EventFilterMode), Settings.Default.EventFilterMode));
         if (stockEventSelectorDlg.ShowDialog() == DialogResult.OK)
         {
            Settings.Default.SelectedEvents = stockEventSelectorDlg.SelectedEvents;
            Settings.Default.EventFilterMode = stockEventSelectorDlg.EventFilterMode.ToString();

            // Detect new indicators
            this.statusLabel.Text = "Detecting Events";
            this.Refresh();
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = StockDictionary.Count;
            this.progressBar.Value = 0;
            if (this.showOnlyEventMenuItem.Checked == true)
            {
               StockDictionary.DetectEvents(new StockDictionary.OnSerieEventDetectionDone(OnSerieEventProcessed),
                  this.StockPortofolioList, Settings.Default.SelectedEvents);
               this.progressBar.Value = 0;
            }
            this.statusLabel.Text = string.Empty;
            this.progressBar.Value = 0;

            Settings.Default.Save();
         }
         // Reinit combo
         InitialiseStockCombo();
      }

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

      private void InitialiseWatchListComboBox()
      {
         // Clean existing menus
         this.watchListMenuItem.DropDownItems.Clear();

         if (this.WatchLists != null)
         {
            // 
            System.Windows.Forms.ToolStripItem[] watchListMenuItems =
               new System.Windows.Forms.ToolStripItem[this.WatchLists.Count()];
            ToolStripMenuItem watchListSubMenuItem;
            System.Windows.Forms.ToolStripItem[] addToWatchListMenuItems =
               new System.Windows.Forms.ToolStripItem[this.WatchLists.Count()];
            ToolStripMenuItem addToWatchListSubMenuItem;

            int i = 0;
            foreach (StockWatchList watchList in WatchLists)
            {
               // Create menu items
               watchListSubMenuItem = new ToolStripMenuItem(watchList.Name);
               watchListSubMenuItem.CheckOnClick = true;
               watchListSubMenuItem.Click += new EventHandler(watchListMenuItem_Click);
               watchListMenuItems[i] = watchListSubMenuItem;

               // Create add to wath list menu items
               addToWatchListSubMenuItem = new ToolStripMenuItem(watchList.Name);
               addToWatchListSubMenuItem.Click += new EventHandler(addToWatchListSubMenuItem_Click);
               addToWatchListMenuItems[i++] = addToWatchListSubMenuItem;
            }
            this.watchListMenuItem.DropDownItems.Clear();
            this.watchListMenuItem.DropDownItems.AddRange(watchListMenuItems);
            this.AddToWatchListToolStripDropDownButton.DropDownItems.Clear();
            this.AddToWatchListToolStripDropDownButton.DropDownItems.AddRange(addToWatchListMenuItems);
         }
         // No list is selected so far
         currentWatchList = null;
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

      private void watchListMenuItem_Click(object sender, System.EventArgs e)
      {
         foreach (ToolStripMenuItem subItem in this.watchListMenuItem.DropDownItems)
         {
            if (subItem != (ToolStripMenuItem)sender)
            {
               subItem.Checked = false;
            }
         }

         if (((ToolStripMenuItem)sender).Checked)
         {
            this.currentWatchList = sender.ToString();
            this.CurrentPortofolio = null;
            foreach (ToolStripMenuItem menuItem in this.portofolioFilterMenuItem.DropDownItems)
            {
               menuItem.Checked = false;
            }
         }
         else
         {
            this.currentWatchList = null;
         }

         // Refresh Stock Combo list
         InitialiseStockCombo();
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
               graphControl.DrawingMode = GraphDrawMode.AndrewPitchFork;
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
         this.groupMenuItem.DropDownItems.Clear();

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

         this.groupMenuItem.DropDownItems.AddRange(groupMenuItems.ToArray());
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
            var groupSeries =
               StockDictionary.Values.Where(s => s.StockGroup.ToString() == group && !s.StockAnalysis.Excluded);
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
            var groupSeries =
               StockDictionary.Values.Where(s => s.StockGroup.ToString() == group && !s.StockAnalysis.Excluded);
            if (groupSeries.Count() != 0)
            {
               System.Windows.Forms.ToolStripMenuItem[] secondarySerieMenuItems =
                  new System.Windows.Forms.ToolStripMenuItem[groupSeries.Count()];
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
         foreach (StockSerie.StockBarDuration barDuration in Enum.GetValues(typeof(StockSerie.StockBarDuration)))
         {
            this.barDurationComboBox.Items.Add(barDuration);
         }
      }

      private void barDurationComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (this.currentStockSerie == null) return;

         StockSerie.StockBarDuration barDuration = (StockSerie.StockBarDuration)barDurationComboBox.SelectedItem;
         if (this.CurrentStockSerie.BarDuration != barDuration)
         {
            int previousBarCount = this.CurrentStockSerie.Count;
            this.CurrentStockSerie.BarDuration = barDuration;

            if (previousBarCount != this.CurrentStockSerie.Count)
            {
               nbBars = Settings.Default.DefaultBarNumber;
            }
            this.endIndex = this.CurrentStockSerie.Count - 1;
            this.startIndex = Math.Max(0, this.endIndex - nbBars);

            OnNeedReinitialise(true);
            this.ApplyTheme();

            if (NotifyBarDurationChanged != null)
            {
               this.NotifyBarDurationChanged(barDuration);
            }
         }
      }

      public void ForceBarDuration(StockSerie.StockBarDuration barDuration, bool triggerEvent)
      {
         if (!triggerEvent)
         {
            this.barDurationComboBox.SelectedIndexChanged -=
               new System.EventHandler(this.barDurationComboBox_SelectedIndexChanged);
         }
         this.barDurationComboBox.SelectedItem = barDuration;
         if (!triggerEvent)
         {
            this.barDurationComboBox.SelectedIndexChanged +=
               new System.EventHandler(this.barDurationComboBox_SelectedIndexChanged);
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
         this.currentStockSerie.BarDuration = StockSerie.StockBarDuration.Daily;
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

      //        string data = @"uri:/instrument/1.0/aapl/chartdata;type=quote;range=1d/csv/
      //ticker:aapl
      //unit:MIN
      //timezone:EDT
      //currency:USD
      //gmtoffset:-14400
      //previous_close:490.6400
      //Timestamp:1379943000,1379966400
      //labels:1379944800,1379948400,1379952000,1379955600,1379959200,1379962800,1379966400
      //values:Timestamp,close,high,low,open,volume
      //close:482.8200,494.5000
      //high:483.5000,496.8300
      //low:482.6200,494.4500
      //open:482.8400,496.1900
      //volume:100,3994000
      //1379943059,494.1400,496.8300,494.1400,496.1900,3994000
      //1379943119,494.3800,495.2950,493.5600,494.2600,419900
      //1379943179,492.9000,494.2800,492.8300,494.2800,387800
      //1379943239,492.0300,493.2600,491.5200,492.8800,395300
      //1379943299,492.4200,492.7100,492.1200,492.1300,295600
      //1379943344,491.9300,492.6700,491.2000,492.5100,195600
      //1379943418,492.2990,492.4700,490.9700,492.2600,292500
      //1379943478,491.7500,492.4400,491.4800,492.3500,162500
      //1379943539,492.8500,492.8500,491.3300,491.4300,219200
      //1379943599,493.7959,493.8500,492.4630,492.8400,283300";

      // using (StringReader sr = new StringReader(data))
      // {
      //     string line = sr.ReadLine();
      //     while (!line.StartsWith("Timestamp")) line = sr.ReadLine();

      //     long timeStamp1 = long.Parse(line.Split(':')[1].Split(',')[0]);
      //     long timeStamp2 = long.Parse(line.Split(':')[1].Split(',')[1]);

      //     DateTime refDate = new DateTime(1970, 1, 1);

      //     DateTime date1 = refDate.AddTicks(timeStamp1 * 10000000L);
      //     DateTime date2 = refDate.AddTicks(timeStamp2 * 10000000L);

      //     StockLog.Write("Date 1: " + date1.ToLongDateString() + " " + date1.ToLongTimeString());
      //     StockLog.Write("Date 2: " + date2.ToLongDateString() + " " + date2.ToLongTimeString());

      //     while (!line.StartsWith("volume")) line = sr.ReadLine();

      //     while ((line = sr.ReadLine()) != null)
      //     {
      //         timeStamp1 = long.Parse(line.Split(',')[0]);
      //         DateTime date = refDate.AddTicks(timeStamp1 * 10000000L);
      //         StockLog.Write("Date " + date.ToShortDateString() + " Time " + date.ToShortTimeString());
      //     }
      //} 

      private void statisticsMenuItem_Click(object sender, System.EventArgs e)
      {
         StatisticsDlg statisticsDlg = new StatisticsDlg();
         statisticsDlg.ShowDialog();
         return;

         int minBar = 50;
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
            stockSerie.BarDuration = (StockSerie.StockBarDuration)barDurationComboBox.SelectedItem;

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

      private void logSerieMenuItem_Click(object sender, System.EventArgs e)
      {
         if (this.currentStockSerie == null) return;
         StockSerie newSerie = this.CurrentStockSerie.GenerateLogStockSerie();
         AddNewSerie(newSerie);
      }

      private void uniformRandomWalkMenuItem_Click(object sender, System.EventArgs e)
      {
         if (this.currentStockSerie == null) return;
         StockSerie newSerie = this.CurrentStockSerie.GenerateRandomSerie();
         AddNewSerie(newSerie);
      }

      private void normalRandomWalkMenuItem_Click(object sender, System.EventArgs e)
      {
         if (this.currentStockSerie == null) return;
         StockSerie newSerie = this.CurrentStockSerie.GenerateNormalRandomSerie();
         AddNewSerie(newSerie);
      }

      private void gauchyRandomWalkMenuItem_Click(object sender, System.EventArgs e)
      {
         if (this.currentStockSerie == null) return;
         StockSerie newSerie = this.CurrentStockSerie.GenerateGauchyRandomSerie();
         AddNewSerie(newSerie);
      }

      private void inverseSerieMenuItem_Click(object sender, EventArgs e)
      {
         if (this.currentStockSerie == null) return;
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

         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,StockDataProvider.Generated);
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
            if (previousDailyValue.DATE.DayOfWeek == dayOfWeek && previousDailyValue.VARIATION >0 )
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
            float var = 0.0f;
            float volume = 0.0f;
            int count = 0;
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

      private void GenerateCAC2_Event(string indexName, StockSerie.StockBarDuration barDuration, int period, string eventPattern, string eventName, bool stopOnLowBreadth)
      {
         var cacSeries = this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();
         int seriesCount = cacSeries.Count;
         string serieName = indexName + "2_" + period + "_" + barDuration;
         string ieventName = eventPattern.Replace("%PERIOD%", period.ToString());
         int eventIndex =
            ((IStockEvent)StockViewableItemsManager.GetViewableItem(ieventName)).EventNames.ToList().IndexOf(eventName);

         foreach (StockSerie serie in cacSeries)
         {
            serie.BarDuration = barDuration;
         }
         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();

         float cash = cacSeries.Count * 1000;
         float value = cash;

         List<Position> openedPositions = new List<Position>();
         List<Position> closedPositions = new List<Position>();
         foreach (DateTime date in cacSerie.Keys.Where(d => d.Year > 1991))
         {
            // Try to sell
            foreach (Position pos in openedPositions)
            {
               StockSerie serie = cacSeries.First(s => s.StockName == pos.Name);
               int index = serie.IndexOf(date) - 1;
               if (index > 0)
               {
                  IStockEvent events = (IStockEvent)serie.GetViewableItem(ieventName);
                  if (!events.Events[eventIndex][index])
                  {
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily).ElementAt(index + 1);
                     cash += pos.EndPosition(dailyValue.OPEN);

                     closedPositions.Add(pos);
                  }
               }
            }

            // Try to buy     
            int openPositionCount = openedPositions.Count(p => p.IsOpened);
            float invested = 0.0f;
            foreach (StockSerie serie in cacSeries.Where(s => !openedPositions.Any(p => p.Name == s.StockName)))
            {
               int index = serie.IndexOf(date) - 1;
               if (index > 0)
               {
                  IStockEvent events = (IStockEvent)serie.GetViewableItem(ieventName);
                  if (events.Events[eventIndex][index])
                  {
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily).ElementAt(index + 1);
                     int qty = (int)(cash / ((seriesCount - openPositionCount) * dailyValue.OPEN));

                     openedPositions.Add(new Position(serie.StockName, dailyValue.OPEN, qty));

                     invested += qty * dailyValue.OPEN;
                  }
               }
            }
            cash -= invested;

            // Clean opened positions
            openedPositions.RemoveAll(p => !p.IsOpened);

            int count = openedPositions.Count;

            value = cash;
            foreach (Position pos in openedPositions)
            {
               StockSerie serie = cacSeries.First(s => s.StockName == pos.Name);
               DateTime posDate = date;
               StockDailyValue dailyValue = null;
               do
               {
                  dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily).FirstOrDefault(v => v.DATE == posDate);
                  posDate = posDate.AddDays(-1);
               } while (dailyValue == null);

               value += pos.Number * dailyValue.CLOSE;
            }

            value = value / (float)cacSeries.Count;

            cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)count, date));
         }
         foreach (StockSerie serie in cacSeries)
         {
            serie.BarDuration = StockSerie.StockBarDuration.Daily;
         }
         StockDictionary.Add(serieName, cacEWSerie);

         StockLog.Write(serieName + ";" + period + ";" + cacEWSerie.Values.Last().CLOSE);
      }

      private void GenerateCAC_Event(string indexName, StockSerie.StockBarDuration barDuration, int period,
         string eventPattern, string eventName, bool stopOnLowBreadth)
      {
         var cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();
         string serieName = indexName + period + "_" + barDuration;
         string ieventName = eventPattern.Replace("%PERIOD%", period.ToString());
         int eventIndex =
            ((IStockEvent)StockViewableItemsManager.GetViewableItem(ieventName)).EventNames.ToList().IndexOf(eventName);

         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         StockSerie BX4Serie = this.StockDictionary["BX4"];
         BX4Serie.Initialise();
         cacSeries.Add(BX4Serie);

         float value = 1000f;
         int previousCount = 0;
         int previousNbActive = 0;
         foreach (DateTime date in cacSerie.Keys.Where(d => d.Year > 2005))
         {
            float var = 0.0f;
            int count = 0;
            int nbActive = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date))
               {
                  count++;
                  serie.BarDuration = barDuration;
                  IStockEvent events = (IStockEvent)serie.GetViewableItem(ieventName);
                  int index = serie.IndexOf(date) - 1;
                  if (index >= 0 && events.Events[eventIndex][index])
                  {
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily)
                        .ElementAt(index + 1);
                     var += dailyValue.VARIATION;
                     nbActive++;
                  }
               }
            }
            if (stopOnLowBreadth && previousNbActive < count / 3)
            {
               cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)0, date));
            }
            else
            {
               if (count != 0) value += value * (var / count);
               cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)nbActive, date));
            }
            previousNbActive = nbActive;
            previousCount = count;
         }
         foreach (StockSerie serie in cacSeries)
         {
            serie.BarDuration = StockSerie.StockBarDuration.Daily;
         }
         StockDictionary.Add(serieName, cacEWSerie);

         StockLog.Write(serieName + ";" + period + ";" + cacEWSerie.Values.Last().CLOSE);
      }
      private void GenerateIndex_Event(string stockName, string indexName, StockSerie.StockBarDuration barDuration, int period1, int period2, string eventPattern, string eventName)
      {
         string serieName = stockName +"_" + indexName + period1 + "_" + period2 + "_" + barDuration;
         StockSplashScreen.ProgressText = "Generating " + serieName + "...";

         string ieventName = eventPattern.Replace("%PERIOD1%", period1.ToString());
         ieventName = ieventName.Replace("%PERIOD2%", period2.ToString());
         int eventIndex = ((IStockEvent)StockViewableItemsManager.GetViewableItem(ieventName)).EventNames.ToList().IndexOf(eventName);

         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC, StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary[stockName];
         cacSerie.Initialise();
         cacSerie.BarDuration = barDuration;

         float value = 1000f;// cacSerie.Values.First().CLOSE;
         IStockEvent events = (IStockEvent)cacSerie.GetViewableItem(ieventName);
         int index = 2;
         foreach (StockDailyValue dailyValue in cacSerie.Values.Skip(2))
         {
            if (events.Events[eventIndex][index - 1])
            {
               if (events.Events[eventIndex][index - 2])
               {
                  value += value * dailyValue.VARIATION;
               }
               else
               {
                  value += value * (dailyValue.CLOSE - dailyValue.OPEN) / dailyValue.OPEN;
               }
            }

            cacEWSerie.Add(dailyValue.DATE, new StockDailyValue(serieName, value, value, value, value, (long)0, dailyValue.DATE));
            index++;
         }
         StockDictionary.Add(serieName, cacEWSerie);
         StockLog.Write(serieName + ";" + period1 + ";" + period2 + ";" + cacEWSerie.Values.Last().CLOSE);
      }

      private void GenerateCAC_Event(string indexName, StockSerie.StockBarDuration barDuration, int period1, int period2,
         string eventPattern, string eventName, bool stopOnLowBreadth)
      {
         var cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();
         string serieName = indexName + period1 + "_" + period2 + "_" + barDuration;
         string ieventName = eventPattern.Replace("%PERIOD1%", period1.ToString());
         ieventName = ieventName.Replace("%PERIOD2%", period2.ToString());
         int eventIndex =
            ((IStockEvent)StockViewableItemsManager.GetViewableItem(ieventName)).EventNames.ToList().IndexOf(eventName);

         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         //StockSerie BX4Serie = this.StockDictionary["BX4"];
         //BX4Serie.Initialise();
         //cacSeries.Add(BX4Serie);

         foreach (StockSerie serie in cacSeries)
         {
            if (serie.BarDuration != barDuration) serie.BarDuration = barDuration;
         }
         float value = 1000f;
         int previousCount = 0;
         int previousNbActive = 0;
         foreach (DateTime date in cacSerie.Keys.Where(d => d.Year>2005))
         {
            float var = 0.0f;
            int count = 0;
            int nbActive = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date) && serie.Count > 200)
               {
                  count++;
                  IStockEvent events = (IStockEvent)serie.GetViewableItem(ieventName);
                  int index = serie.IndexOf(date) - 1;
                  if (index >= 0 && events.Events[eventIndex][index])
                  {
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily)
                        .ElementAt(index + 1);
                     var += dailyValue.VARIATION;
                     nbActive++;
                  }
               }
            }
            if (stopOnLowBreadth && previousNbActive < count / 2)
            {
               cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)0, date));
            }
            else
            {
               if (count != 0) value += value * (var / count);
               cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)nbActive, date));
            }
            previousNbActive = nbActive;
            previousCount = count;
         }
         foreach (StockSerie serie in cacSeries)
         {
            if (serie.BarDuration != StockSerie.StockBarDuration.Daily) serie.BarDuration = StockSerie.StockBarDuration.Daily;
         }
         StockDictionary.Add(serieName, cacEWSerie);
         StockLog.Write(serieName + ";" + period1 + ";" + period2 + ";" + cacEWSerie.Values.Last().CLOSE);
      }

      private void GenerateCAC_SAR(bool stopOnLowBreadth, float speed)
      {
         string serieName = "CAC_SAR_" + speed.ToString("#.######");
         List<StockSerie> cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();
         StockSerie newIndexSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         //StockSerie BX4Serie = this.StockDictionary["BX4"];
         //BX4Serie.Initialise();
         //cacSeries.Add(BX4Serie);

         float value = 1000f;
         int previousCount = 0;
         int previousNbActive = 0;
         foreach (DateTime date in cacSerie.Keys)
         {
            float var = 0.0f;
            int count = 0;
            int nbActive = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date))
               {
                  IStockIndicator indicator = serie.GetIndicator("SAR(0," + speed + ",0.2)");

                  count++;

                  int index = serie.IndexOf(date) - 1;
                  if (index >= 0 && indicator.Events[8][index])
                  {
                     StockDailyValue dailyValue = serie[date];
                     var += dailyValue.VARIATION;
                     nbActive++;
                  }
               }
            }
            //if (count < 5 && previousCount >= count && BX4Serie.ContainsKey(date))
            //{
            //   count++;
            //   StockDailyValue dailyValue = BX4Serie[date];
            //   var += dailyValue.VARIATION;
            //}
            if (stopOnLowBreadth && previousNbActive < count / 2)
            {
               newIndexSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)0, date));
            }
            else
            {
               if (count != 0) value += value * (var / count);
               newIndexSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)nbActive, date));
            }
            previousNbActive = nbActive;
            previousCount = count;
         }
         StockDictionary.Add(serieName, newIndexSerie);
      }

      private void GenerateCAC_Random()
      {
         List<StockSerie> cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();
         StockSerie cacEWSerie = new StockSerie("CAC_RANDOM", "CAC_RANDOM", StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         StockSerie BX4Serie = this.StockDictionary["BX4"];
         BX4Serie.Initialise();
         cacSeries.Add(BX4Serie);

         Random rnd = new Random();

         float value = 1000f;
         int previousCount = 0;
         foreach (DateTime date in cacSerie.Keys)
         {
            float var = 0.0f;
            int count = 0;
            int nbActive = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date))
               {
                  count++;
                  if (rnd.Next(0, 11) > 5)
                  {
                     StockDailyValue dailyValue = serie[date];
                     var += dailyValue.VARIATION;
                     nbActive++;
                  }
               }
            }
            //if (count < 5 && previousCount >= count && BX4Serie.ContainsKey(date))
            //{
            //   count++;
            //   StockDailyValue dailyValue = BX4Serie[date];
            //   var += dailyValue.VARIATION;
            //}
            if (count != 0) value += value * (var / count);
            cacEWSerie.Add(date, new StockDailyValue("CAC_RANDOM", value, value, value, value, (long)nbActive, date));

            previousCount = count;
         }
         StockDictionary.Add("CAC_RANDOM", cacEWSerie);

      }

      private void GenerateCAC_TL(StockSerie.StockBarDuration barDuration, int period)
      {
         var cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
         string serieName = "CAC_TL" + period + "_" + barDuration;
         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         StockSerie BX4Serie = this.StockDictionary["BX4"];
         BX4Serie.Initialise();

         float value = 1000f;
         int previousCount = 0;
         foreach (DateTime date in cacSerie.Keys)
         {
            float var = 0.0f;
            int count = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date))
               {
                  serie.BarDuration = barDuration;
                  IStockPaintBar trendLine = serie.GetPaintBar("TRENDLINEHL(" + period + ",10)");
                  int index = serie.IndexOf(date) - 1;
                  if (index >= 0 && trendLine.Events[5][index])
                  {
                     count++;
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily)
                        .ElementAt(index + 1);
                     var += dailyValue.VARIATION;
                  }
               }
            }
            //if (count < 5 && previousCount >= count && BX4Serie.ContainsKey(date))
            //{
            //   count++;
            //   StockDailyValue dailyValue = BX4Serie[date];
            //   var += dailyValue.VARIATION;
            //}
            if (count != 0) value += value * (var / count);
            cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)count, date));

            previousCount = count;
         }
         foreach (StockSerie serie in cacSeries)
         {
            serie.BarDuration = StockSerie.StockBarDuration.Daily;
         }
         StockDictionary.Add(serieName, cacEWSerie);

      }

      private void GenerateCAC_STOKS(StockSerie.StockBarDuration barDuration, int period)
      {
         var cacSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise());
         string serieName = "CAC_STOKS" + period + "_" + barDuration;
         StockSerie cacEWSerie = new StockSerie(serieName, serieName, StockSerie.Groups.INDICES_CALC,
            StockDataProvider.Generated);
         StockSerie cacSerie = this.StockDictionary["CAC40"];
         cacSerie.Initialise();
         StockSerie BX4Serie = this.StockDictionary["BX4"];
         BX4Serie.Initialise();

         float value = 1000f;
         int previousCount = 0;
         foreach (DateTime date in cacSerie.Keys)
         {
            float var = 0.0f;
            int count = 0;
            foreach (StockSerie serie in cacSeries)
            {
               if (serie.ContainsKey(date))
               {
                  count++;
                  serie.BarDuration = barDuration;
                  IStockEvent events = serie.GetIndicator("STOKS(" + period + ",3,3,50,50)");
                  int index = serie.IndexOf(date) - 1;
                  if (index >= 0 && events.Events[2][index])
                  {
                     StockDailyValue dailyValue = serie.GetValues(StockSerie.StockBarDuration.Daily)
                        .ElementAt(index + 1);
                     var += dailyValue.VARIATION;
                  }
               }
            }
            //if (count < 5 && previousCount >= count && BX4Serie.ContainsKey(date))
            //{
            //   count++;
            //   StockDailyValue dailyValue = BX4Serie[date];
            //   var += dailyValue.VARIATION;
            //}
            if (count != 0) value += value * (var / count);
            cacEWSerie.Add(date, new StockDailyValue(serieName, value, value, value, value, (long)count, date));

            previousCount = count;
         }
         foreach (StockSerie serie in cacSeries)
         {
            serie.BarDuration = StockSerie.StockBarDuration.Daily;
         }
         StockDictionary.Add(serieName, cacEWSerie);

         StockLog.Write(serieName + ";" + cacEWSerie.Values.Last().CLOSE);

      }

      private void GeneratePosition(List<StockSerie.Groups> groups)
      {
         string folderName = Settings.Default.RootFolder + POSITION_SUBFOLDER;
         if (!System.IO.Directory.Exists(folderName))
         {
            System.IO.Directory.CreateDirectory(folderName);
         }

         Settings.Default.MomentumIndicator = "ROR(100,6,1)";

         foreach (StockSerie.Groups group in groups)
         {

            StockSplashScreen.ProgressText = "Generating position data for " + group;
            var groupSeries =
               StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(group) && s.Initialise());

            // Find last date
            DateTime lastDate = DateTime.MinValue;
            StockSplashScreen.ProgressSubText = "Initialising series";
            foreach (StockSerie serie in groupSeries)
            {
               serie.BarDuration = StockSerie.StockBarDuration.Daily;
               if (lastDate < serie.Keys.Last())
               {
                  lastDate = serie.Keys.Last();
               }
            }

            // Parse Variation cache
            string fileName = folderName + @"\" + group + ".csv";
            StockSplashScreen.ProgressSubText = "Loading cache data";
            DateTime lastCacheDate = DateTime.MinValue;
            if (File.Exists(fileName))
            {
               using (StreamReader sr = new StreamReader(fileName))
               {
                  DateTime date = DateTime.MinValue;
                  do
                  {
                     string line = sr.ReadLine();
                     string[] fields = line.Split(',');
                     date = DateTime.Parse(fields[0]);

                     for (int i = 1; i < fields.Count(); i++)
                     {
                        string[] values = fields[i].Split(':');
                        this.StockDictionary[values[0]][date].POSITION = float.Parse(values[1], usCulture);
                     }
                  } while (!sr.EndOfStream);
                  lastCacheDate = date;
               }
            }

            if (lastDate == lastCacheDate)
            {
               foreach (StockSerie serie in groupSeries)
               {
                  serie.PreInitialise();
               }
               continue;
            }

            // Find reference stock in group (first date)
            StockSerie stockSerie = this.GroupReference[group];
            stockSerie.Initialise();
            DateTime firstDate = stockSerie.Keys.First();

            firstDate = lastCacheDate > firstDate ? lastCacheDate : firstDate;

            // Calculate positions for each stocks in group
            SortedDictionary<DateTime, List<MomentumSerie>> positions =
               new SortedDictionary<DateTime, List<MomentumSerie>>();

            foreach (var date in stockSerie.Keys.Where(d => d > firstDate && d.Date == d))
            {
               StockSplashScreen.ProgressSubText = "Calculating positions " + date.ToShortDateString();
               List<MomentumSerie> momSeries = new List<MomentumSerie>();
               int dateIndex = -1;
               foreach (StockSerie serie in groupSeries)
               {
                  if ((dateIndex = serie.IndexOf(date)) != -1)
                  {
                     momSeries.Add(new MomentumSerie()
                     {
                        MomentumSlow = serie.GetIndicator(Settings.Default.MomentumIndicator).Series[0][dateIndex],
                        MomentumFast = serie.GetIndicator(Settings.Default.MomentumIndicator).Series[0][dateIndex],
                        StockSerie = serie
                     });
                  }
               }
               if (momSeries.Count > 1)
               {
                  var moms = momSeries.OrderBy(ms => ms.MomentumSlow);
                  int count = moms.Count();
                  int i = 0;
                  foreach (var m in moms)
                  {
                     m.PositionSlow = -100f + 200f * (float)(i++) / (count - 1);
                  }

                  moms = momSeries.OrderBy(ms => ms.MomentumFast);
                  i = 0;
                  foreach (var m in moms)
                  {
                     m.PositionFast = -100f + 200f * (float)(i++) / (count - 1);
                     m.StockSerie[date].POSITION = m.Position;
                  }
                  positions.Add(date, momSeries);
               }
            }

            StockSplashScreen.ProgressSubText = "PreInitialising";
            // In order to get Variation attribute set in DailyValues
            foreach (StockSerie serie in groupSeries)
            {
               serie.PreInitialise();
            }

            // Serialise positions
            StockSplashScreen.ProgressSubText = "Saving position cache";
            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
               foreach (var pair in positions)
               {
                  sw.Write(pair.Key + ",");
                  int count = pair.Value.Count;
                  int i = 0;
                  foreach (var m in pair.Value)
                  {
                     sw.Write(m.StockSerie.StockName + ":" + m.Position.ToString(usCulture));
                     if (++i < count) sw.Write(",");
                  }
                  sw.WriteLine();
               }
            }
         }
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
         StockSerie vixPremium = new StockSerie("VIX_PREMIUM", "VIX_PREMIUM", StockSerie.Groups.INDICATOR,
            StockDataProvider.Generated);

         // Generate the VIX Premium
         FloatSerie spVolatilitySerie = spSerie.GetSerie(StockIndicatorType.VOLATILITY_STDEV);
         float spVolatility = 0;
         int index = 0;
         foreach (StockDailyValue vixValue in vixSerie.Values)
         {
            index = spSerie.IndexOf(vixValue.DATE);
            if (index != -1)
            {
               spVolatility = spVolatilitySerie[index];
               StockDailyValue dailyValue = new StockDailyValue(vixPremium.StockName, vixValue.OPEN / spVolatility,
                  vixValue.HIGH / spVolatility, vixValue.LOW / spVolatility, vixValue.CLOSE / spVolatility, 0, vixValue.DATE);
               vixPremium.Add(dailyValue.DATE, dailyValue);
               dailyValue.Serie = vixPremium;
            }
         }
         vixPremium.Initialise();
         this.StockDictionary.Add(vixPremium.StockName, vixPremium);
      }

      #endregion

      private void palmaresMenuItem_Click(object sender, EventArgs e)
      {
         if (palmaresDlg == null)
         {
            palmaresDlg = new PalmaresDlg(StockDictionary, this.WatchLists, this.selectedGroup, this.progressBar);
            palmaresDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
            palmaresDlg.SelectedPortofolioChanged +=
               new SelectedPortofolioNameChangedEventHandler(OnCurrentPortofolioNameChanged);
            palmaresDlg.SelectStockGroupChanged +=
               new SelectedStockGroupChangedEventHandler(this.OnSelectedStockGroupChanged);

            palmaresDlg.FormClosing += new FormClosingEventHandler(palmaresDlg_FormClosing);
            palmaresDlg.StockWatchListsChanged += new StockWatchListsChangedEventHandler(OnWatchListsChanged);

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
            // In order to speed the intraday display.
            switch (newGroup)
            {
               case StockSerie.Groups.TURBO:
                  this.ForceBarDuration(StockSerie.StockBarDuration.TLB_3D, true);
                  break;
               case StockSerie.Groups.INTRADAY:
                  this.ForceBarDuration(StockSerie.StockBarDuration.TLB_6D, true);
                  break;
               default:
                  this.ForceBarDuration(StockSerie.StockBarDuration.Daily, true);
                  break;
            }

            this.selectedGroup = newGroup;

            foreach (ToolStripMenuItem groupSubMenuItem in this.groupMenuItem.DropDownItems)
            {
               groupSubMenuItem.Checked = groupSubMenuItem.Text == stockGroup;
            }

            InitialiseStockCombo();
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

      private void OnWatchListsChanged()
      {
         InitialiseWatchListComboBox();
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
            this.CurrentPortofolio.Initialize(StockDictionary);
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

            portofolio.Initialize(StockDictionary);
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

      private void addOrdersMenuItem_Click(object sender, System.EventArgs e)
      {
         StockbrokersOrderCreationDlg orderCreationDialog =
            new StockbrokersOrderCreationDlg(this.StockPortofolioList.Find(p => p.Name == "STOCKBROKERS"),
               StockDictionary);
         orderCreationDialog.SavePortofolioNeeded += new SavePortofolio(this.SavePortofolios);
         orderCreationDialog.ShowDialog();

         OnNeedReinitialise(false);
      }

      private void addStockCAPCAMenuItem_Click(object sender, EventArgs e)
      {
         CAPCAOrderCreationDlg orderCreationDialog =
            new CAPCAOrderCreationDlg(this.StockPortofolioList.Find(p => p.Name == "CA-CPA"), StockDictionary);
         orderCreationDialog.SavePortofolioNeeded += new SavePortofolio(this.SavePortofolios);
         orderCreationDialog.ShowDialog();

         OnNeedReinitialise(false);
      }

      #endregion

      #region ANALYSIS MENU HANDLERS

      #region Best Return Strategy

      private void bestReturnStrategySimulationMenuItem_Click(object sender, EventArgs e)
      {

         int maxPositions = 2;
         float maxPositionValue = 10000f;

         string portofolioName = "BestReturn";

         float portofolioValue = 10000;
         float cashValue = portofolioValue;

         #region CreatePortofolio

         // Create new simulation portofolio
         if (CurrentPortofolio == null)
         {
            CurrentPortofolio = this.StockPortofolioList.Find(p => p.Name == portofolioName);
            if (CurrentPortofolio == null)
            {
               CurrentPortofolio = new StockPortofolio(portofolioName);
               CurrentPortofolio.IsSimulation = true;
               CurrentPortofolio.TotalDeposit = portofolioValue;
               this.StockPortofolioList.Add(CurrentPortofolio);
            }
         }

         #endregion

         StockSerie referenceSerie = this.StockDictionary["CAC40"];
         referenceSerie.Initialise();
         referenceSerie.BarDuration = StockSerie.StockBarDuration.Monthly;
         List<StockSerie> stockSeries =
            this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40) && s.Initialise()).ToList();

         // Switch to monthly values
         foreach (StockSerie serie in stockSeries)
         {
            serie.BarDuration = StockSerie.StockBarDuration.Monthly;
         }

         DateTime startDate = referenceSerie.Keys.First();
         List<StockDailyValue> values = new List<StockDailyValue>();
         List<Position> portofolio = new List<Position>();

         DateTime lastDate = referenceSerie.Keys.Last();
         foreach (DateTime date in referenceSerie.Keys)
         {
            StockLog.Write(date.ToShortDateString());

            values.Clear();

            // Find matching values
            foreach (StockSerie serie in stockSeries)
            {
               int index = serie.IndexOf(date);
               if (index > 0)
               {
                  values.Add(serie[date]);
               }
            }

            // Select positives + order by return
            List<StockDailyValue> selectedValues =
               values.Where(s => s.VARIATION > 0).OrderByDescending(s => s.VARIATION).ToList();

            // Close not listed names.
            foreach (Position pos in portofolio.Where(p => float.IsNaN(p.Close)))
            {
               if (!selectedValues.Any(s => s.NAME == pos.Name))
               {
                  cashValue += pos.EndPosition(stockSeries.First(s => s.StockName == pos.Name)[date].OPEN);
                  portofolioValue += pos.Gain;

                  StockLog.Write("Selling: " + pos.ToString() + " gain: " + pos.Gain.ToString());
               }
               else
               {
                  selectedValues.RemoveAll(s => s.NAME == pos.Name);
               }
            }

            // Open new positions
            int openPositionsCount = portofolio.Count(p => p.IsOpened);
            int candidateCount = selectedValues.Count();

            int nbPositionsToOpen = maxPositions - openPositionsCount;

            float consumedCash = 0;
            for (int i = 0; i < nbPositionsToOpen && i < candidateCount; i++)
            {
               StockDailyValue value = selectedValues[i];
               int size = (int)((Math.Min(cashValue / nbPositionsToOpen, maxPositionValue)) / value.CLOSE);
               Position pos = new Position(value.NAME, value.CLOSE, size);
               portofolio.Add(pos);
               consumedCash = value.CLOSE * size;

               StockLog.Write("Buying: " + pos.ToString() + " at: " + pos.Open.ToString());
            }
            cashValue -= consumedCash;

            if (date == lastDate)
            {
               foreach (Position pos in portofolio.Where(p => p.IsOpened))
               {
                  cashValue += pos.EndPosition(stockSeries.First(s => s.StockName == pos.Name)[date].CLOSE);
                  portofolioValue += pos.Gain;

                  StockLog.Write("Selling: " + pos.ToString() + " gain: " + pos.Gain.ToString());
               }
            }

            // Display Open Positions
            StockLog.Write("Portofolio Value =" + portofolioValue);
            //foreach (Variation pos in portofolio.Where(p => p.IsOpened))
            //{
            //    StockLog.Write(pos.ToString());
            //}
         }
         StockLog.Write("Cash Value: " + cashValue);
      }

      private class Position
      {
         public string Name { get; set; }
         public float Open { get; set; }
         public float Close { get; set; }
         public int Number { get; set; }

         public bool IsOpened
         {
            get { return float.IsNaN(this.Close); }
         }

         public float Gain
         {
            get { return (this.Close - this.Open) * this.Number; }
         }

         public Position(string name, float open, int number)
         {
            this.Name = name;
            this.Open = open;
            this.Number = number;
            this.Close = float.NaN;
            if (number <= 0) throw new ArgumentException("Cannot open a position with negative values");
         }

         public float EndPosition(float close)
         {
            this.Close = close;
            return close * this.Number;
         }

         public override string ToString()
         {
            return this.Name + ": " + this.Number.ToString();
         }
      }

      #endregion

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
            filteredStrategySimulatorDlg.SimulationCompleted +=
               new FilteredStrategySimulatorDlg.SimulationCompletedEventHandler(
                  filteredStrategySimulatorDlg_SimulationCompleted);
            filteredStrategySimulatorDlg.SelectedStockChanged +=
               new SelectedStockChangedEventHandler(OnSelectedStockChanged);
            filteredStrategySimulatorDlg.SelectedPortofolioChanged +=
               new SelectedPortofolioChangedEventHandler(OnCurrentPortofolioChanged);
         }
         else
         {
            filteredStrategySimulatorDlg.Activate();
         }
         filteredStrategySimulatorDlg.SelectedStockName = this.stockNameComboBox.SelectedItem.ToString();

         filteredStrategySimulatorDlg.SelectedPortofolio = CurrentPortofolio;
         filteredStrategySimulatorDlg.Show();
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
         portofolio.Initialize(StockDictionary);
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
         portofolio.Initialize(StockDictionary);
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
         portofolio.Initialize(StockDictionary);
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
         portofolio.Initialize(StockDictionary);
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
            batchStrategySimulatorDlg = new BatchStrategySimulatorDlg(StockDictionary, this.StockPortofolioList,
               this.selectedGroup, (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem, this.progressBar);
            batchStrategySimulatorDlg.SimulationCompleted +=
               new SimulationCompletedEventHandler(batchStrategySimulatorDlg_SimulationCompleted);

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
         "<br /><hr width=\"50%\"><B><P style=\"text-align: center; font-size: xx-large\">" + titleTemplate + "</P></B>";

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

      private string GenerateDailyReport()
      {
         CleanImageFolder();

         string mailReport = string.Empty;
         string htmlReport = string.Empty;

         string commentTitle = string.Empty;
         string commentBody = string.Empty;
         int imageCount = 0;
         ImageFormat imageFormat = ImageFormat.Png;
         List<string> cidList = new List<string>();
         List<string> fileNameList = new List<string>();

         //string hostIP = "ftp://ultimatechartist.com";
         //string userName = "ultimatechartist.com|ultimate";
         //string password = "XU5ZWi0Y";

         //StockFTP ftp = new StockFTP(hostIP, userName, password);
         //string[] files = ftp.directoryListDetailed(".");

         #region report multi TimeFrame

         StockSerie.StockBarDuration[] durations = new StockSerie.StockBarDuration[]
         {
            StockSerie.StockBarDuration.Daily,
            StockSerie.StockBarDuration.Daily_EMA3,
            StockSerie.StockBarDuration.TLB,
            StockSerie.StockBarDuration.TLB_EMA3,
            StockSerie.StockBarDuration.TLB_3D,
            StockSerie.StockBarDuration.TLB_3D_EMA3
         };


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
<body>";
         string MULTITIMEFRAME_FOOTER = @"
</body>
</html>";
         string MULTITIMEFRAME_TABLE = @"<table>%TABLE%</table>" + Environment.NewLine;

         string CELL_DIR_IMG_TEMPLATE =
            "<td><img alt=\"%DIR%\" src=\"../img/%DIR%.png\"/></td>" +
            Environment.NewLine;
         string CELL_TEXT_TEMPLATE = "<td>%TEXT%</td>" + Environment.NewLine;
         string ROW_TEMPLATE = "<tr>%ROW%<tr/>" + Environment.NewLine;

         // Generate header
         string rowContent = CELL_TEXT_TEMPLATE.Replace("%TEXT%", "Stock Name");
         string headerRow = string.Empty;
         foreach (StockSerie.StockBarDuration duration in durations)
         {
            rowContent += CELL_TEXT_TEMPLATE.Replace("%TEXT%", duration.ToString());
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

               //StockDataProviderBase.DownloadSerieData(Settings.Default.RootFolder, serie);

               foreach (StockSerie.StockBarDuration duration in durations)
               {
                  serie.BarDuration = duration;
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
         string table = MULTITIMEFRAME_HEADER + MULTITIMEFRAME_TABLE.Replace("%TABLE%", tableContent) +
                        MULTITIMEFRAME_FOOTER;

         if (!Directory.Exists(Settings.Default.RootFolder + "\\CommentReport"))
         {
            Directory.CreateDirectory(Settings.Default.RootFolder + "\\CommentReport");
            Directory.CreateDirectory(Settings.Default.RootFolder + "\\CommentReport\\img");
         }
         using (StreamWriter sw = new StreamWriter(Settings.Default.RootFolder + @"\CommentReport\report_table.html"))
         {
            sw.Write(table);
         }
         #endregion

         #region Report leaders

         this.CurrentTheme = "ReportTheme";
         this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.TLB;
         string rankIndicatorName = "ROR(100,1,6)";
         int nbLeaders = 15;
         List<RankedSerie> leadersDico = new List<RankedSerie>();
         foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.SRD)))
         {
            if (stockSerie.Initialise() && stockSerie.Count > 100)
            {
               stockSerie.BarDuration = StockSerie.StockBarDuration.Daily;
               IStockIndicator indicator = stockSerie.GetIndicator(rankIndicatorName);
               leadersDico.Add(new RankedSerie() { rank = indicator.Series[0].Last, previousRank = indicator.Series[0][indicator.Series[0].Count - 2], stockSerie = stockSerie });
            }
         }

         string htmlLeaders = string.Empty;
         string htmlLosers = string.Empty;

         string rowTemplate = @"
         <tr>
             <td>%COL1%</td>
             <td>%COL2%</td>
             %DIR_IMG%
         </tr>";

         htmlLeaders = htmlTitleTemplate.Replace(titleTemplate, "Leaders for " + rankIndicatorName);
         htmlLeaders += " <table>";

         var leaders = leadersDico.OrderByDescending(l => l.rank).Take(nbLeaders);
         foreach (RankedSerie pair in leaders)
         {
            htmlLeaders += rowTemplate.Replace("%COL1%", pair.stockSerie.StockName).Replace("%COL2%", (pair.rank).ToString("#.##"));
            if (pair.previousRank <= pair.rank)
            {
               htmlLeaders = htmlLeaders.Replace("%DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
            }
            else
            {
               htmlLeaders = htmlLeaders.Replace("%DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
            }
         }

         htmlLeaders += " </table>";

         mailReport += htmlLeaders;
         htmlReport += htmlLeaders;

         rankIndicatorName = "ROD(100,1,6)";
         leadersDico.Clear();
         foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.SRD)))
         {
            if (stockSerie.Initialise() && stockSerie.Count > 100)
            {
               stockSerie.BarDuration = StockSerie.StockBarDuration.Daily;
               IStockIndicator indicator = stockSerie.GetIndicator(rankIndicatorName);
               leadersDico.Add(new RankedSerie() { rank = -indicator.Series[0].Last, previousRank = -indicator.Series[0][indicator.Series[0].Count - 2], stockSerie = stockSerie });
            }
         }

         htmlLeaders = htmlTitleTemplate.Replace(titleTemplate, "Losers for " + rankIndicatorName);
         htmlLeaders += " <table>";
         leaders = leadersDico.OrderBy(l => l.rank).Take(nbLeaders);
         foreach (RankedSerie pair in leaders)
         {
            htmlLeaders += rowTemplate.Replace("%COL1%", pair.stockSerie.StockName).Replace("%COL2%", (pair.rank).ToString("#.##"));
            if (pair.previousRank <= pair.rank)
            {
               htmlLeaders = htmlLeaders.Replace("%DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "UP"));
            }
            else
            {
               htmlLeaders = htmlLeaders.Replace("%DIR_IMG%", CELL_DIR_IMG_TEMPLATE.Replace("%DIR%", "DOWN"));
            }
         }

         htmlLeaders += " </table>";

         mailReport += htmlLeaders;
         htmlReport += htmlLeaders;


         #endregion

         StockSerie previousStockSerie = this.CurrentStockSerie;
         string previousTheme = this.CurrentTheme;
         StockSerie.StockBarDuration previousBarDuration =
            (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem;

         using (StreamReader sr = new StreamReader(Settings.Default.RootFolder + @"\report.cfg"))
         {
            #region Report from config file

            while (!sr.EndOfStream)
            {
               string line = sr.ReadLine();
               //continue;
               if (string.IsNullOrWhiteSpace(line))
               {
                  continue;
               }

               string[] fields = line.Split(';');
               if (fields[0].StartsWith("#Title", StringComparison.InvariantCultureIgnoreCase))
               {
                  mailReport += htmlTitleTemplate.Replace(titleTemplate, fields[1]);
                  htmlReport += htmlTitleTemplate.Replace(titleTemplate, fields[1]);
               }

               if (fields.Length == 0 || !this.StockDictionary.ContainsKey(fields[0]))
               {
                  continue;
               }


               this.barDurationComboBox.SelectedItem =
                  (StockSerie.StockBarDuration)Enum.Parse(typeof(StockSerie.StockBarDuration), fields[2]);
               this.CurrentStockSerie = this.StockDictionary[fields[0]];
               this.CurrentTheme = fields[1];

               if (fields.Length >= 4)
               {
                  int nbBars = int.Parse(fields[3]);
                  this.ChangeZoom(Math.Max(0, CurrentStockSerie.Count - 1 - nbBars), CurrentStockSerie.Count - 1);
               }

               StockSerie stockSerie = this.CurrentStockSerie;

               // Extract indicators
               string eventTypeString = ExtractEventsForReport();

               this.snapshotToolStripButton_Click(null, null);
               Image bitmap = Clipboard.GetImage();

               string fileName = GetFileName(DateTime.Now, stockSerie, imageFormat.ToString().ToLower());
               if (!System.IO.File.Exists(fileName))
               {
                  bitmap.Save(fileName, imageFormat);
                  fileNameList.Add(fileName);

                  // Get image CID
                  string cid = "Image_" + imageCount++;
                  cidList.Add(cid);

                  commentTitle = "\r\n" + stockSerie.StockName + "( " + fields[2] + ") - " + fields[1] + " - " +
                                 stockSerie.Keys.Last().ToShortDateString() + "\r\n\r\n";

                  // Build report from html template
                  mailReport += htmlMailCommentTemplate.Replace(commentTitleTemplate, commentTitle)
                     .Replace(commentTemplate, commentBody)
                     .Replace(imageFileCID, cid);
                  mailReport += eventTypeString;
                  htmlReport += htmlCommentTemplate.Replace(commentTitleTemplate, commentTitle)
                     .Replace(commentTemplate, commentBody)
                     .Replace(imageFileLink,
                        fileName.Replace(
                           StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\", "./")
                           .Replace(@"\", "/"));
                  htmlReport += eventTypeString;
               }
            }

            #endregion
         }

         #region Generate report from Portfolio

         //List<String> stockNames = this.StockPortofolioList.GetStockNames();
         //foreach (StockSerie stockSerie in this.StockDictionary.Values.Where(s => stockNames.Contains(s.StockName)))
         //{
         //   this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.Bar_3;
         //   this.CurrentTheme = "TrendLine";
         //   this.CurrentStockSerie = stockSerie;

         //   string eventTypeString = ExtractEventsForReport();

         //   if (string.IsNullOrWhiteSpace(eventTypeString))
         //   {
         //      continue;
         //   }

         //   this.snapshotToolStripButton_Click(null, null);
         //   Image bitmap = Clipboard.GetImage();

         //   string fileName = GetFileName(DateTime.Today, stockSerie, imageFormat.ToString().ToLower());
         //   if (!System.IO.File.Exists(fileName))
         //   {
         //      bitmap.Save(fileName, imageFormat);
         //      fileNameList.Add(fileName);

         //      // Get image CID
         //      string cid = "Image_" + imageCount++;
         //      cidList.Add(cid);

         //      commentTitle = "\r\n" + stockSerie.StockName + "( " + this.barDurationComboBox.SelectedItem + ") - " +
         //                     " - " + stockSerie.Keys.Last().ToShortDateString() + "\r\n\r\n";

         //      // Build report from html template
         //      mailReport += htmlMailCommentTemplate.Replace(commentTitleTemplate, commentTitle)
         //         .Replace(commentTemplate, commentBody)
         //         .Replace(imageFileCID, cid);
         //      mailReport += eventTypeString;
         //      htmlReport += htmlCommentTemplate.Replace(commentTitleTemplate, commentTitle)
         //         .Replace(commentTemplate, commentBody)
         //         .Replace(imageFileLink,
         //            fileName.Replace(
         //               StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\", "./")
         //               .Replace(@"\", "/"));
         //      htmlReport += eventTypeString;
         //   }
         //}

         #endregion

         #region Generate report from Events

         StockAlertDef cciEx = new StockAlertDef(StockSerie.StockBarDuration.Daily, "DECORATOR", "DIVWAIT(1.5,1)|CCIEX(50,12,20,0.0195,75,-75)", "ExhaustionBottom");
         StockAlertDef barAbove = new StockAlertDef(StockSerie.StockBarDuration.Daily, "INDICATOR", "HMA(30)", "FirstBarAbove");
         StockAlertDef barBelow = new StockAlertDef(StockSerie.StockBarDuration.Daily, "INDICATOR", "HMA(30)", "FirstBarBelow");
         StockAlertDef ResistanceBroken = new StockAlertDef(StockSerie.StockBarDuration.Daily, "PAINTBAR", "TRENDLINEHL(1,10)", "ResistanceBroken");
         StockAlertDef trailHL = new StockAlertDef(StockSerie.StockBarDuration.Daily, "TRAILSTOP", "TRAILHLS(2,3)", "BrokenUp");
         StockAlertDef trailHLSR = new StockAlertDef(StockSerie.StockBarDuration.Daily, "INDICATOR", "TRAILHLSR(5)", "ResistanceBroken");
         List<StockAlertDef> alerts = new List<StockAlertDef>();

         alerts.Clear();
         alerts.Add(cciEx);
         alerts.Add(barAbove);
         alerts.Add(barBelow);
         alerts.Add(trailHL);
         alerts.Add(ResistanceBroken);
         alerts.Add(trailHLSR);

         foreach (
            StockSerie stockSerie in this.StockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.CAC40)))
         {
            //this.barDurationComboBox.SelectedItem = StockSerie.StockBarDuration.Daily;
            //this.CurrentTheme = "Empty";
            //this.CurrentStockSerie = stockSerie;

            StockSplashScreen.ProgressVal++;
            StockSplashScreen.ProgressSubText = "Scanning " + stockSerie.StockName;

            if (!stockSerie.Initialise() || stockSerie.Count < 200) continue;

            string alertMsg = string.Empty;
            foreach (StockAlertDef alert in alerts)
            {
               if (stockSerie.MatchEvent(alert))
               {
                  var values = stockSerie.GetValues(alert.BarDuration);
                  string alertLine = stockSerie.StockName + ";" + values.ElementAt(values.Count - 2).DATE.TimeOfDay +
                                     ";" + alert.ToString();

                  alertMsg += "<br>" + alertLine + ";" + stockSerie.GetValues(StockSerie.StockBarDuration.Daily).Last().CLOSE + "</br>";
               }
            }
            if (!string.IsNullOrEmpty(alertMsg))
            {
               commentTitle = "\r\n" + stockSerie.StockName + " - " + stockSerie.Keys.Last().ToShortDateString() + "\r\n";

               // Build report from html template
               //mailReport += htmlMailCommentTemplate.Replace(commentTitleTemplate, commentTitle)
               //   .Replace(commentTemplate, commentBody)
               //   //.Replace(imageFileCID, cid);
               //mailReport += eventTypeString;
               htmlReport += htmlAlertTemplate.Replace(commentTitleTemplate, commentTitle)
                  .Replace(commentTemplate, alertMsg);
               //.Replace(imageFileLink,
               //   fileName.Replace(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\",
               //      "./").Replace(@"\", "/"));
               //htmlReport += alertMsg;
            }
         }

         //this.snapshotToolStripButton_Click(null, null);
         //Image bitmap = Clipboard.GetImage();

         //string fileName = GetFileName(DateTime.Today, stockSerie, imageFormat.ToString().ToLower());
         //if (!System.IO.File.Exists(fileName))
         //{
         //   bitmap.Save(fileName, imageFormat);
         //   fileNameList.Add(fileName);

         //   // Get image CID
         //   string cid = "Image_" + imageCount++;
         //   cidList.Add(cid);

         //   commentTitle = "\r\n" + stockSerie.StockName + "( " + this.barDurationComboBox.SelectedItem + ") - " + " - " +
         //                  stockSerie.Keys.Last().ToShortDateString() + "\r\n\r\n";

         //   // Build report from html template
         //   mailReport += htmlMailCommentTemplate.Replace(commentTitleTemplate, commentTitle)
         //      .Replace(commentTemplate, commentBody)
         //      .Replace(imageFileCID, cid);
         //   mailReport += eventTypeString;
         //   htmlReport += htmlCommentTemplate.Replace(commentTitleTemplate, commentTitle)
         //      .Replace(commentTemplate, commentBody)
         //      .Replace(imageFileLink,
         //         fileName.Replace(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\",
         //            "./").Replace(@"\", "/"));
         //   htmlReport += eventTypeString;
         //}

         #endregion


         //AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailReport, null, "text/html");
         //int index = 0;
         //foreach (string cid in cidList)
         //{
         //    LinkedResource imagelink = new LinkedResource(fileNameList[index++], "image/" + imageFormat.ToString());
         //    imagelink.ContentId = cid;
         //    imagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
         //    htmlView.LinkedResources.Add(imagelink);
         //}
         //email.AlternateViews.Add(htmlView);

         using (StreamWriter sw = new StreamWriter(Settings.Default.RootFolder + @"\CommentReport\report.html"))
         {
            sw.Write(htmlReport);
         }
         // ftp.uploadDirectory("www/CommentReport", Settings.Default.RootFolder + @"\CommentReport");

         //           Process.Start("http://www.ultimatechartist.com/CommentReport/report.html");
         Process.Start(Settings.Default.RootFolder + @"\CommentReport\report.html");
         this.CurrentStockSerie = previousStockSerie;
         this.CurrentTheme = previousTheme;
         this.barDurationComboBox.SelectedItem = previousBarDuration;


         return mailReport;
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

      private static int imageID = 0;
      private static string GetFileName(DateTime readDate, StockSerie serie, string extension)
      {
         imageID++;
         string directoryName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\";
         if (!System.IO.Directory.Exists(directoryName))
         {
            System.IO.Directory.CreateDirectory(directoryName);
         }
         directoryName += readDate.ToString("dd_MM_yyyy");
         if (!System.IO.Directory.Exists(directoryName))
         {
            System.IO.Directory.CreateDirectory(directoryName);
         }
         string fileName = directoryName + @"\" + serie.StockName.Replace("/", "_") + "_" + imageID + "." + extension;
         return fileName;
      }
      private static void CleanImageFolder()
      {
         string directoryName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\";

         if (System.IO.Directory.Exists(directoryName))
         {
            foreach (string directory in (System.IO.Directory.EnumerateDirectories(directoryName)))
            {
               System.IO.Directory.Delete(directory, true);
            }
            foreach (string file in (System.IO.Directory.EnumerateFiles(directoryName)))
            {
               System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete(directoryName, true);
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
         this.GenerateDailyReport();
      }
      #endregion
      private void selectEventForMarqueeMenuItem_Click(object sender, EventArgs e)
      {
         if (this.currentStockSerie == null) return;
         StockEventSelectorDlg stockEventSelectorDlg = new StockEventSelectorDlg(Settings.Default.EventMarquees, (StockEvent.EventFilterMode)Enum.Parse(typeof(StockEvent.EventFilterMode), Settings.Default.EventMarqueeMode));
         if (stockEventSelectorDlg.ShowDialog() == DialogResult.OK)
         {
            Settings.Default.EventMarquees = stockEventSelectorDlg.SelectedEvents;
            Settings.Default.EventMarqueeMode = stockEventSelectorDlg.EventFilterMode.ToString();

            Settings.Default.Save();
            OnNeedReinitialise(false);
         }
      }
      private void manageWatchlistsMenuItem_Click(object sender, EventArgs e)
      {
         if (this.currentStockSerie == null || this.WatchLists == null) return;

         WatchListDlg watchlistDlg = new WatchListDlg(this.WatchLists);
         watchlistDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
         watchlistDlg.StockWatchListsChanged += new StockWatchListsChangedEventHandler(this.OnWatchListsChanged);
         if (watchlistDlg.ShowDialog() == DialogResult.OK)
         {
            this.SaveWatchList();
            this.InitialiseWatchListComboBox();
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
                (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem,
                this.themeDictionary[this.currentTheme]);
            stockScannerDlg.SelectedStockChanged += new SelectedStockChangedEventHandler(OnSelectedStockChanged);
            stockScannerDlg.SelectStockGroupChanged +=
                new SelectedStockGroupChangedEventHandler(this.OnSelectedStockGroupChanged);
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
            stockStrategyScannerDlg = new StockStrategyScannerDlg(StockDictionary, this.selectedGroup, (StockSerie.StockBarDuration)this.barDurationComboBox.SelectedItem, this.currentStrategy);
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
                     this.DownloadStock();
                  }
                  break;
               case Keys.Control | Keys.F5:
                  {
                     this.DownloadStockGroup();
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
                     List<string> indicators = new List<string> { "TRAILSTOP|TRAILHLS(2,1)"};
                     List<StockSerie.StockBarDuration> durations = new List<StockSerie.StockBarDuration>
                            {
                                StockSerie.StockBarDuration.Daily,
                                StockSerie.StockBarDuration.Daily_EMA6,
                                StockSerie.StockBarDuration.TLB,
                                StockSerie.StockBarDuration.TLB_EMA6,
                                StockSerie.StockBarDuration.TLB_3D,
                                StockSerie.StockBarDuration.TLB_3D_EMA6,
                            };
                     //MultiTimeFrameGrid grid = new MultiTimeFrameGrid();

                     //this.Cursor = Cursors.WaitCursor;

                     //grid.LoadData(durations, indicators, this.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.currentStockSerie.StockGroup)).ToList());

                     //this.Cursor = Cursors.Arrow;

                     //grid.Show(this);

                     MTFDlg mtfDlg = new MTFDlg();
                     mtfDlg.MtfControl.SelectedStockChanged += OnSelectedStockAndDurationChanged;
                     mtfDlg.Show();
                  }
                  break;
               default:
                  return base.ProcessCmdKey(ref msg, keyData);
            }
         }
         return true;
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
      public void teachNetworkMenuItem_Click(object sender, EventArgs e)
      {
         NeuralNetwork network = null;

         // string fileName = Settings.Default.StockAnalyzerRootFolder + @"\StockNeuralNetwork\" + this.StockName + ".nntwk";
         string fileName = Settings.Default.RootFolder + @"\StockNeuralNetwork\ALL_STOCKS.nntwk";
         if (File.Exists(fileName))
         {
            network = NeuralNetwork.load(fileName);
         }

         this.progressBar.Value = 0;
         this.progressBar.Maximum = this.StockDictionary.Values.Count;
         foreach (StockSerie stockSerie in this.StockDictionary.Values)
         {
            if (!stockSerie.IsPortofolioSerie && stockSerie.Values.Count > 50)
            {
               stockSerie.Initialise();
               // #### stockSerie.CalculateBuySellRateWithNN2(ref network, true);
            }
            else
            {
               StockLog.Write("Skipping portofofolio serie: " + stockSerie.StockName);
            }
            this.progressBar.Value++;
         }
         this.progressBar.Value = 0;

         if (!Directory.Exists(Settings.Default.RootFolder + @"\StockNeuralNetwork"))
         {
            Directory.CreateDirectory(Settings.Default.RootFolder + @"\StockNeuralNetwork");
         }
         network.save(fileName);
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
      void showAlertViewMenuItem_Click(object sender, System.EventArgs e)
      {
         if (alertDlg == null)
         {
            alertDlg = new AlertDlg();
            alertDlg.alertControl1.SelectedStockChanged += OnSelectedStockAndDurationChanged;
            alertDlg.Disposed += alertDlg_Disposed;
            alertDlg.Show();
         }
         else
         {
            alertDlg.Activate();
         }
      }

      void alertDlg_Disposed(object sender, EventArgs e)
      {
         alertDlg.alertControl1.SelectedStockChanged -= OnSelectedStockAndDurationChanged;
         this.alertDlg = null;
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
               this.CurrentStockSerie.StockAnalysis.DeleteTransientDrawings();

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
                        StockLog.Write("Exception londing theme: " + this.currentTheme);
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

                  if (this.currentStockSerie.BelongsToGroup(StockSerie.Groups.BREADTH))
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
                        this.CurrentPortofolio.Initialize(this.StockDictionary);
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
      #region BAR TYPE MENU HANDLERS
      private void dailyMenuItem_Click(object sender, EventArgs e)
      {
         if (!dailyMenuItem.Checked)
         {
            dailyMenuItem.Checked = true;
            tickMenuItem.Checked = false;
            rangeMenuItem.Checked = false;
            intradayMenuItem.Checked = false;
            Settings.Default.BarType = (int)StockBar.StockBarType.Daily;
         }
         OnNeedReinitialise(false);
      }

      private void rangeMenuItem_Click(object sender, EventArgs e)
      {
         if (!rangeMenuItem.Checked)
         {
            dailyMenuItem.Checked = false;
            tickMenuItem.Checked = false;
            rangeMenuItem.Checked = true;
            intradayMenuItem.Checked = false;
            Settings.Default.BarType = (int)StockBar.StockBarType.Range;
         }
         OnNeedReinitialise(false);
      }

      private void tickMenuItem_Click(object sender, EventArgs e)
      {
         if (!tickMenuItem.Checked)
         {
            dailyMenuItem.Checked = false;
            tickMenuItem.Checked = true;
            rangeMenuItem.Checked = false;
            intradayMenuItem.Checked = false;
            Settings.Default.BarType = (int)StockBar.StockBarType.Tick;
         }
         OnNeedReinitialise(false);
      }

      private void intradayMenuItem_Click(object sender, EventArgs e)
      {
         if (!intradayMenuItem.Checked)
         {
            dailyMenuItem.Checked = false;
            tickMenuItem.Checked = false;
            rangeMenuItem.Checked = false;
            intradayMenuItem.Checked = true;
            Settings.Default.BarType = (int)StockBar.StockBarType.Intraday;
         }
         OnNeedReinitialise(false);
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
         if (((IConfigDialog)((ToolStripMenuItem)sender).Tag).ShowDialog(this.StockDictionary) == System.Windows.Forms.DialogResult.OK)
         {
            StockDataProviderBase.InitStockDictionary(Settings.Default.RootFolder, this.StockDictionary, Settings.Default.DownloadData && NetworkInterface.GetIsNetworkAvailable(), new DownloadingStockEventHandler(Notifiy_SplashProgressChanged));
            this.CreateGroupMenuItem();
            this.CreateSecondarySerieMenuItem();
            this.CreateRelativeStrengthMenuItem();
            this.InitialiseStockCombo();
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

   }
}