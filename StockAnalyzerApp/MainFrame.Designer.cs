using System.Windows.Forms;

namespace StockAnalyzerApp
{
    partial class StockAnalyzerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StockAnalyzerForm));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAnalysisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAnalysisFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAnalysisFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAnalysisFileAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.saveThemeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configDataProviderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.eraseDrawingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eraseAllDrawingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showShowStatusBarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideIndicatorsStockMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDrawingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOrdersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPositionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.showAgendaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEventMarqueeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCommentMarqueeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDividendMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIndicatorDivMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIndicatorTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.showHorseRaceViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.marketReplayViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multipleTimeFrameViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAlertDefDialogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawingDialogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.secondarySerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logSerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockScannerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockSplitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.palmaresMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instrumentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.agentTunningMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portfolioSimulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.tweetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bestTrendMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sectorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expectedValueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statisticsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portfolioMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoTradeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentPortfolioMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portfolioReportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.watchlistsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageWatchlistsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptEditorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.indicatorLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.themeToolStrip = new System.Windows.Forms.ToolStrip();
            this.indicatorConfigStripButton = new System.Windows.Forms.ToolStripButton();
            this.candleStripButton = new System.Windows.Forms.ToolStripButton();
            this.barchartStripButton = new System.Windows.Forms.ToolStripButton();
            this.linechartStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.defaultThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.themeComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.deleteThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.portfolioComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.refreshPortfolioBtn = new System.Windows.Forms.ToolStripButton();
            this.portfolioStatusLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.browseToolStrip = new System.Windows.Forms.ToolStrip();
            this.stockNameComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.barDurationComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.downloadBtn = new System.Windows.Forms.ToolStripButton();
            this.searchCombo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.rewindBtn = new System.Windows.Forms.ToolStripButton();
            this.fastForwardBtn = new System.Windows.Forms.ToolStripButton();
            this.copyIsinBtn = new System.Windows.Forms.ToolStripButton();
            this.zoomOutBtn = new System.Windows.Forms.ToolStripButton();
            this.zoomInBtn = new System.Windows.Forms.ToolStripButton();
            this.logScaleBtn = new System.Windows.Forms.ToolStripButton();
            this.divScaleBtn = new System.Windows.Forms.ToolStripButton();
            this.showVariationBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.excludeButton = new System.Windows.Forms.ToolStripButton();
            this.intradayButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.drawToolStrip = new System.Windows.Forms.ToolStrip();
            this.magnetStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.addHalfLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.addSegmentStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawCupHandleStripBtn = new System.Windows.Forms.ToolStripButton();
            this.copyLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.cutLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawWinRatioStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawBoxStripBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawingStyleStripBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAnalysisToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.snapshotToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.generateDailyReportToolStripBtn = new System.Windows.Forms.ToolStripButton();
            this.AddToWatchListToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.graphCloseControl = new CustomControl.GraphControls.GraphCloseControl();
            this.graphScrollerControl = new CustomControl.GraphControls.GraphScrollerControl();
            this.graphIndicator1Control = new CustomControl.GraphControls.GraphRangedControl();
            this.graphIndicator2Control = new CustomControl.GraphControls.GraphRangedControl();
            this.graphIndicator3Control = new CustomControl.GraphControls.GraphRangedControl();
            this.graphVolumeControl = new CustomControl.GraphControls.GraphVolumeControl();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.mainMenu.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.themeToolStrip.SuspendLayout();
            this.browseToolStrip.SuspendLayout();
            this.drawToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.toolStripMenuItem1,
            this.stockFilterMenuItem,
            this.viewMenuItem,
            this.analysisMenuItem,
            this.portfolioMenuItem,
            this.watchlistsMenuItem,
            this.helpMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(975, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAnalysisMenuItem,
            this.loadAnalysisFileMenuItem,
            this.toolStripSeparator4,
            this.saveAnalysisFileMenuItem,
            this.saveAnalysisFileAsMenuItem,
            this.toolStripSeparator18,
            this.saveThemeMenuItem,
            this.toolStripSeparator9,
            this.optionsMenuItem,
            this.configDataProviderMenuItem,
            this.toolStripSeparator3,
            this.exitToolStripMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "File";
            // 
            // newAnalysisMenuItem
            // 
            this.newAnalysisMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.NewAnalysis;
            this.newAnalysisMenuItem.Name = "newAnalysisMenuItem";
            this.newAnalysisMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newAnalysisMenuItem.Size = new System.Drawing.Size(262, 22);
            this.newAnalysisMenuItem.Text = "New Analysis";
            this.newAnalysisMenuItem.Click += new System.EventHandler(this.newAnalysisMenuItem_Click);
            // 
            // loadAnalysisFileMenuItem
            // 
            this.loadAnalysisFileMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.OpenAnalysis;
            this.loadAnalysisFileMenuItem.Name = "loadAnalysisFileMenuItem";
            //this.loadAnalysisFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.loadAnalysisFileMenuItem.Size = new System.Drawing.Size(262, 22);
            this.loadAnalysisFileMenuItem.Text = "Load Analysis File";
            this.loadAnalysisFileMenuItem.Click += new System.EventHandler(this.loadAnalysisFileMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(259, 6);
            // 
            // saveAnalysisFileMenuItem
            // 
            this.saveAnalysisFileMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.SaveAnalysis;
            this.saveAnalysisFileMenuItem.Name = "saveAnalysisFileMenuItem";
            this.saveAnalysisFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAnalysisFileMenuItem.Size = new System.Drawing.Size(262, 22);
            this.saveAnalysisFileMenuItem.Text = "Save Analysis File";
            this.saveAnalysisFileMenuItem.Click += new System.EventHandler(this.saveAnalysisFileMenuItem_Click);
            // 
            // saveAnalysisFileAsMenuItem
            // 
            this.saveAnalysisFileAsMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.SaveAnalysisAs;
            this.saveAnalysisFileAsMenuItem.Name = "saveAnalysisFileAsMenuItem";
            this.saveAnalysisFileAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
            | System.Windows.Forms.Keys.S)));
            this.saveAnalysisFileAsMenuItem.Size = new System.Drawing.Size(262, 22);
            this.saveAnalysisFileAsMenuItem.Text = "Save Analysis File As...";
            this.saveAnalysisFileAsMenuItem.Click += new System.EventHandler(this.saveAnalysisFileAsMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(259, 6);
            // 
            // saveThemeMenuItem
            // 
            this.saveThemeMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.SaveTheme;
            this.saveThemeMenuItem.Name = "saveThemeMenuItem";
            this.saveThemeMenuItem.Size = new System.Drawing.Size(262, 22);
            this.saveThemeMenuItem.Text = "Save Theme";
            this.saveThemeMenuItem.Click += new System.EventHandler(this.saveThemeMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(259, 6);
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.Options;
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(262, 22);
            this.optionsMenuItem.Text = "Preferences";
            this.optionsMenuItem.Click += new System.EventHandler(this.folderPrefMenuItem_Click);
            // 
            // configDataProviderMenuItem
            // 
            this.configDataProviderMenuItem.Name = "configDataProviderMenuItem";
            this.configDataProviderMenuItem.Size = new System.Drawing.Size(262, 22);
            this.configDataProviderMenuItem.Text = "Configure data providers";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(259, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator17,
            this.eraseDrawingsToolStripMenuItem,
            this.eraseAllDrawingsToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(39, 20);
            this.toolStripMenuItem1.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.undo;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.redo;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(204, 6);
            // 
            // eraseDrawingsToolStripMenuItem
            // 
            this.eraseDrawingsToolStripMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.trashcan;
            this.eraseDrawingsToolStripMenuItem.Name = "eraseDrawingsToolStripMenuItem";
            this.eraseDrawingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.eraseDrawingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.eraseDrawingsToolStripMenuItem.Text = "Erase graph drawings";
            this.eraseDrawingsToolStripMenuItem.Click += new System.EventHandler(this.eraseDrawingsToolStripMenuItem_Click);
            // 
            // eraseAllDrawingsToolStripMenuItem
            // 
            this.eraseAllDrawingsToolStripMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.trashcan;
            this.eraseAllDrawingsToolStripMenuItem.Name = "eraseAllDrawingsToolStripMenuItem";
            this.eraseAllDrawingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.E)));
            this.eraseAllDrawingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.eraseAllDrawingsToolStripMenuItem.Text = "Erase all drawings";
            this.eraseAllDrawingsToolStripMenuItem.Click += new System.EventHandler(this.eraseAllDrawingsToolStripMenuItem_Click);
            // 
            // stockFilterMenuItem
            // 
            this.stockFilterMenuItem.Name = "stockFilterMenuItem";
            this.stockFilterMenuItem.Size = new System.Drawing.Size(45, 20);
            this.stockFilterMenuItem.Text = "Filter";
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showShowStatusBarMenuItem,
            this.hideIndicatorsStockMenuItem,
            this.showDrawingsMenuItem,
            this.showOrdersMenuItem,
            this.showPositionsMenuItem,
            this.toolStripSeparator14,
            this.showAgendaMenuItem,
            this.showDividendMenuItem,
            this.showEventMarqueeMenuItem,
            this.showCommentMarqueeMenuItem,
            this.showIndicatorDivMenuItem,
            this.showIndicatorTextMenuItem,
            this.toolStripSeparator16,
            this.marketReplayViewMenuItem,
            this.showHorseRaceViewMenuItem,
            this.multipleTimeFrameViewMenuItem,
            this.drawingDialogMenuItem,
            this.toolStripSeparator15,
            this.secondarySerieMenuItem,
            this.logSerieMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewMenuItem.Text = "View";
            // 
            // showShowStatusBarMenuItem
            // 
            this.showShowStatusBarMenuItem.CheckOnClick = true;
            this.showShowStatusBarMenuItem.Name = "showShowStatusBarMenuItem";
            this.showShowStatusBarMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showShowStatusBarMenuItem.Text = "Show Status bar";
            this.showShowStatusBarMenuItem.Click += new System.EventHandler(this.showShowStatusBarMenuItem_Click);
            // 
            // hideIndicatorsStockMenuItem
            // 
            this.hideIndicatorsStockMenuItem.CheckOnClick = true;
            this.hideIndicatorsStockMenuItem.Name = "hideIndicatorsStockMenuItem";
            this.hideIndicatorsStockMenuItem.Size = new System.Drawing.Size(233, 22);
            this.hideIndicatorsStockMenuItem.Text = "Hide indicators";
            this.hideIndicatorsStockMenuItem.Click += new System.EventHandler(this.hideIndicatorsStockMenuItem_Click);
            // 
            // showDrawingsMenuItem
            // 
            this.showDrawingsMenuItem.Checked = true;
            this.showDrawingsMenuItem.CheckOnClick = true;
            this.showDrawingsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDrawingsMenuItem.Name = "showDrawingsMenuItem";
            this.showDrawingsMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showDrawingsMenuItem.Text = "Show Drawings";
            this.showDrawingsMenuItem.Click += new System.EventHandler(this.showDrawingsMenuItem_Click);
            // 
            // showOrdersMenuItem
            // 
            this.showOrdersMenuItem.Checked = true;
            this.showOrdersMenuItem.CheckOnClick = true;
            this.showOrdersMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showOrdersMenuItem.Name = "showOrdersMenuItem";
            this.showOrdersMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.showOrdersMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showOrdersMenuItem.Text = "Show Orders";
            this.showOrdersMenuItem.Click += new System.EventHandler(this.showOrdersMenuItem_Click);
            // 
            // showPositionsMenuItem
            // 
            this.showPositionsMenuItem.Checked = true;
            this.showPositionsMenuItem.CheckOnClick = true;
            this.showPositionsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showPositionsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.showPositionsMenuItem.Name = "showPositionsMenuItem";
            this.showPositionsMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showPositionsMenuItem.Text = "Show Positions";
            this.showPositionsMenuItem.Click += new System.EventHandler(this.showPositionsMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(230, 6);
            // 
            // showAgendaMenuItem
            // 
            this.showAgendaMenuItem.Name = "showAgendaMenuItem";
            this.showAgendaMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showAgendaMenuItem.Text = "Show Agenda Entries";
            // 
            // showEventMarqueeMenuItem
            // 
            this.showEventMarqueeMenuItem.Checked = true;
            this.showEventMarqueeMenuItem.CheckOnClick = true;
            this.showEventMarqueeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showEventMarqueeMenuItem.Name = "showEventMarqueeMenuItem";
            this.showEventMarqueeMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showEventMarqueeMenuItem.Text = "Show Event Marquees";
            this.showEventMarqueeMenuItem.Click += new System.EventHandler(this.showEventMarqueeMenuItem_Click);
            // 
            // showCommentMarqueeMenuItem
            // 
            this.showCommentMarqueeMenuItem.Checked = true;
            this.showCommentMarqueeMenuItem.CheckOnClick = true;
            this.showCommentMarqueeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCommentMarqueeMenuItem.Name = "showCommentMarqueeMenuItem";
            this.showCommentMarqueeMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showCommentMarqueeMenuItem.Text = "Show Comment Marquees";
            this.showCommentMarqueeMenuItem.Click += new System.EventHandler(this.showCommentMarqueeMenuItem_Click);
            // 
            // showDividendMenuItem
            // 
            this.showDividendMenuItem.Checked = true;
            this.showDividendMenuItem.CheckOnClick = true;
            this.showDividendMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDividendMenuItem.Name = "showDividendMenuItem";
            this.showDividendMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showDividendMenuItem.Text = "Show Dividends";
            this.showDividendMenuItem.Click += new System.EventHandler(this.showDividendMenuItem_Click);
            // 
            // showIndicatorDivMenuItem
            // 
            this.showIndicatorDivMenuItem.Checked = true;
            this.showIndicatorDivMenuItem.CheckOnClick = true;
            this.showIndicatorDivMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showIndicatorDivMenuItem.Name = "showIndicatorDivMenuItem";
            this.showIndicatorDivMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showIndicatorDivMenuItem.Text = "Show Divergences";
            this.showIndicatorDivMenuItem.Click += new System.EventHandler(this.showIndicatorDivMenuItem_Click);
            // 
            // showIndicatorTextMenuItem
            // 
            this.showIndicatorTextMenuItem.Checked = true;
            this.showIndicatorTextMenuItem.CheckOnClick = true;
            this.showIndicatorTextMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showIndicatorTextMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.showIndicatorTextMenuItem.Name = "showIndicatorTextMenuItem";
            this.showIndicatorTextMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showIndicatorTextMenuItem.Text = "Show Indicator Text";
            this.showIndicatorTextMenuItem.Click += new System.EventHandler(this.showIndicatorTextMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(230, 6);
            // 
            // showHorseRaceViewMenuItem
            // 
            this.showHorseRaceViewMenuItem.Name = "showHorseRaceViewMenuItem";
            this.showHorseRaceViewMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            this.showHorseRaceViewMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showHorseRaceViewMenuItem.Text = "Horse Race View";
            this.showHorseRaceViewMenuItem.Click += showHorseRaceViewMenuItem_Click;
            // 
            // marketReplayViewMenuItem
            // 
            this.marketReplayViewMenuItem.Name = "marketReplayViewMenuItem";
            this.marketReplayViewMenuItem.ShortcutKeys = Keys.F4;
            this.marketReplayViewMenuItem.Size = new System.Drawing.Size(233, 22);
            this.marketReplayViewMenuItem.Text = "Market Replay";
            this.marketReplayViewMenuItem.Click += marketReplayViewMenuItem_Click;
            // 
            // multipleTimeFrameViewMenuItem
            // 
            this.multipleTimeFrameViewMenuItem.Name = "multipleTimeFrameViewMenuItem";
            this.multipleTimeFrameViewMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Shift | Keys.F3;
            this.multipleTimeFrameViewMenuItem.Size = new System.Drawing.Size(233, 22);
            this.multipleTimeFrameViewMenuItem.Text = "Multiple Time Frame View";
            this.multipleTimeFrameViewMenuItem.Click += multipleTimeFrameViewMenuItem_Click;
            // 
            // showAlertDefDialogMenuItem
            // 
            this.showAlertDefDialogMenuItem.Name = "showAlertDefDialogMenuItem";
            this.showAlertDefDialogMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.showAlertDefDialogMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showAlertDefDialogMenuItem.Text = "Alert Definition";
            this.showAlertDefDialogMenuItem.Click += showAlertDefDialogMenuItem_Click;
            // 
            // drawingDialogMenuItem
            // 
            this.drawingDialogMenuItem.Name = "drawingDialogMenuItem";
            this.drawingDialogMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
            this.drawingDialogMenuItem.Size = new System.Drawing.Size(233, 22);
            this.drawingDialogMenuItem.Text = "Drawing View";
            this.drawingDialogMenuItem.Click += drawingDialogMenuItem_Click;
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(230, 6);
            // 
            // secondarySerieMenuItem
            // 
            this.secondarySerieMenuItem.Name = "secondarySerieMenuItem";
            this.secondarySerieMenuItem.Size = new System.Drawing.Size(233, 22);
            this.secondarySerieMenuItem.Text = "Secondary Serie";
            // 
            // logSerieMenuItem
            // 
            this.logSerieMenuItem.Name = "logSerieMenuItem";
            this.logSerieMenuItem.Size = new System.Drawing.Size(233, 22);
            this.logSerieMenuItem.Text = "Log Serie";
            this.logSerieMenuItem.Click += new System.EventHandler(this.logSerieMenuItem_Click);
            // 
            // analysisMenuItem
            // 
            this.analysisMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.stockSplitMenuItem,
                this.toolStripSeparator19,
                this.stockScannerMenuItem,
                this.palmaresMenuItem,
                this.instrumentsMenuItem,
                this.toolStripSeparator10,
                this.agentTunningMenuItem,
                this.portfolioSimulationMenuItem,
                this.toolStripSeparator8,
                this.showAlertDefDialogMenuItem,
                this.tweetMenuItem,
                this.sectorMenuItem,
                this.bestTrendMenuItem,
                this.expectedValueMenuItem,
                this.statisticsMenuItem });
            this.analysisMenuItem.Name = "analysisMenuItem";
            this.analysisMenuItem.Size = new System.Drawing.Size(62, 20);
            this.analysisMenuItem.Text = "Analysis";
            // 
            // stockScannerMenuItem
            // 
            this.stockScannerMenuItem.Name = "stockScannerMenuItem";
            this.stockScannerMenuItem.Size = new System.Drawing.Size(240, 22);
            this.stockScannerMenuItem.Text = "Stock Scanner";
            this.stockScannerMenuItem.Click += new System.EventHandler(this.stockScannerMenuItem_Click);
            // 
            // stockSplitMenuItem
            // 
            this.stockSplitMenuItem.Name = "stockSplitMenuItem";
            this.stockSplitMenuItem.Size = new System.Drawing.Size(240, 22);
            this.stockSplitMenuItem.Text = "Stock Split";
            this.stockSplitMenuItem.Click += new System.EventHandler(this.stockSplitMenuItem_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(237, 6);
            // 
            // palmaresMenuItem
            // 
            this.palmaresMenuItem.Name = "palmaresMenuItem";
            this.palmaresMenuItem.Size = new System.Drawing.Size(240, 22);
            this.palmaresMenuItem.Text = "Palmares";
            this.palmaresMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.palmaresMenuItem.Click += new System.EventHandler(this.palmaresMenuItem_Click);
            // 
            // instrumentsMenuItem
            // 
            this.instrumentsMenuItem.Name = "instrumentsMenuItem";
            this.instrumentsMenuItem.Size = new System.Drawing.Size(240, 22);
            this.instrumentsMenuItem.Text = "Instruments";
            this.instrumentsMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.instrumentsMenuItem.Click += new System.EventHandler(this.instrumentsMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(237, 6);
            // 
            // agentTunningMenuItem
            // 
            this.agentTunningMenuItem.Name = "agentTunningMenuItem";
            this.agentTunningMenuItem.Size = new System.Drawing.Size(240, 22);
            this.agentTunningMenuItem.Text = "Agent Greedy Tunning";
            this.agentTunningMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.agentTunningMenuItem.Click += new System.EventHandler(this.agentTunningMenuItem_Click);
            // 
            // portfolioSimulationMenuItem
            // 
            this.portfolioSimulationMenuItem.Name = "portfolioSimulationMenuItem";
            this.portfolioSimulationMenuItem.Size = new System.Drawing.Size(240, 22);
            this.portfolioSimulationMenuItem.Text = "Portfolio Simulation";
            this.portfolioSimulationMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F9;
            this.portfolioSimulationMenuItem.Click += new System.EventHandler(this.portfolioSimulationMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(237, 6);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(237, 6);
            // 
            // tweetMenuItem
            // 
            this.tweetMenuItem.Name = "tweetMenuItem";
            this.tweetMenuItem.Size = new System.Drawing.Size(240, 22);
            this.tweetMenuItem.Text = "Send a tweet";
            this.tweetMenuItem.Click += new System.EventHandler(this.tweetMenuItem_Click);
            // 
            // bestTrendMenuItem
            // 
            this.bestTrendMenuItem.Name = "bestTrendMenuItem";
            this.bestTrendMenuItem.Size = new System.Drawing.Size(240, 22);
            this.bestTrendMenuItem.Text = "Best Trends";
            this.bestTrendMenuItem.Click += new System.EventHandler(this.bestTrendViewMenuItem_Click);
            // 
            // sectorMenuItem
            // 
            this.sectorMenuItem.Name = "sectorMenuItem";
            this.sectorMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sectorMenuItem.Text = "Sector View";
            this.sectorMenuItem.Click += new System.EventHandler(this.sectorViewMenuItem_Click);
            // 
            // expectedValueMenuItem
            // 
            this.expectedValueMenuItem.Name = "expectedValueMenuItem";
            this.expectedValueMenuItem.Size = new System.Drawing.Size(240, 22);
            this.expectedValueMenuItem.Text = "Expected Value";
            this.expectedValueMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F12)));
            this.expectedValueMenuItem.Click += new System.EventHandler(this.expectedValueMenuItem_Click);
            // 
            // statisticsMenuItem
            // 
            this.statisticsMenuItem.Name = "statisticsMenuItem";
            this.statisticsMenuItem.Size = new System.Drawing.Size(240, 22);
            this.statisticsMenuItem.Text = "Statistics";
            this.statisticsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F12)));
            this.statisticsMenuItem.Click += new System.EventHandler(this.statisticsMenuItem_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(237, 6);
            // 
            // portfolioMenuItem
            // 
            this.portfolioMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoTradeMenuItem,
            this.toolStripSeparator21,
            this.currentPortfolioMenuItem,
            this.portfolioReportMenuItem});
            this.portfolioMenuItem.Name = "portfolioMenuItem";
            this.portfolioMenuItem.Size = new System.Drawing.Size(77, 20);
            this.portfolioMenuItem.Text = "Portfolios";
            // 
            // autoTradeMenuItem
            // 
            this.autoTradeMenuItem.Name = "autoTradeMenuItem";
            this.autoTradeMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(System.Windows.Forms.Keys.F8));
            this.autoTradeMenuItem.Size = new System.Drawing.Size(202, 22);
            this.autoTradeMenuItem.Text = "Auto Trading";
            this.autoTradeMenuItem.Click += new System.EventHandler(this.autoTradeMenuItem_Click);
            // 
            // currentPortfolioMenuItem
            // 
            this.currentPortfolioMenuItem.Name = "currentPortfolioMenuItem";
            this.currentPortfolioMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(System.Windows.Forms.Keys.F2));
            this.currentPortfolioMenuItem.Size = new System.Drawing.Size(202, 22);
            this.currentPortfolioMenuItem.Text = "Show Current Portfolio";
            this.currentPortfolioMenuItem.Click += new System.EventHandler(this.currentPortfolioMenuItem_Click);
            // 
            // portfolioReportMenuItem
            // 
            this.portfolioReportMenuItem.Name = "portfolioReportMenuItem";
            this.portfolioReportMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.portfolioReportMenuItem.Size = new System.Drawing.Size(202, 22);
            this.portfolioReportMenuItem.Text = "Report";
            this.portfolioReportMenuItem.Click += new System.EventHandler(this.portfolioReportMenuItem_Click);
            // 
            // watchlistsMenuItem
            // 
            this.watchlistsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageWatchlistsMenuItem,
            this.scriptEditorMenuItem});
            this.watchlistsMenuItem.Name = "watchlistsMenuItem";
            this.watchlistsMenuItem.Size = new System.Drawing.Size(73, 20);
            this.watchlistsMenuItem.Text = "Watchlists";
            // 
            // manageWatchlistsMenuItem
            // 
            this.manageWatchlistsMenuItem.Name = "manageWatchlistsMenuItem";
            this.manageWatchlistsMenuItem.Size = new System.Drawing.Size(138, 22);
            this.manageWatchlistsMenuItem.Text = "Manage";
            this.manageWatchlistsMenuItem.Click += new System.EventHandler(this.manageWatchlistsMenuItem_Click);
            // 
            // scriptEditorMenuItem
            // 
            this.scriptEditorMenuItem.Name = "scriptEditorMenuItem";
            this.scriptEditorMenuItem.Size = new System.Drawing.Size(138, 22);
            this.scriptEditorMenuItem.Text = "Script Editor";
            this.scriptEditorMenuItem.Click += new System.EventHandler(this.scriptEditorMenuItem_Click);
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpMenuItem.Text = "Help";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutMenuItem.Text = "About";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.indicatorLayoutPanel);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(975, 509);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(975, 583);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.drawToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.browseToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.themeToolStrip);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(975, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // statusLabel
            // 
            this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(858, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "statusLabel";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Step = 1;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // indicatorLayoutPanel
            // 
            this.indicatorLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.indicatorLayoutPanel.ColumnCount = 1;
            this.indicatorLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.indicatorLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indicatorLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.indicatorLayoutPanel.Name = "indicatorLayoutPanel";
            this.indicatorLayoutPanel.RowCount = 6;
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.indicatorLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.5F));
            this.indicatorLayoutPanel.Size = new System.Drawing.Size(975, 509);
            this.indicatorLayoutPanel.TabIndex = 0;
            // 
            // themeToolStrip
            // 
            this.themeToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.themeToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indicatorConfigStripButton,
            this.candleStripButton,
            this.barchartStripButton,
            this.linechartStripButton,
            this.defaultThemeStripButton,
            this.themeComboBox,
            this.deleteThemeStripButton,
            this.saveThemeStripButton,
            this.toolStripSeparator1,
            this.portfolioComboBox,
            this.portfolioStatusLbl,
            this.refreshPortfolioBtn});
            this.themeToolStrip.Location = new System.Drawing.Point(3, 0);
            this.themeToolStrip.Name = "themeToolStrip";
            this.themeToolStrip.Size = new System.Drawing.Size(455, 25);
            this.themeToolStrip.TabIndex = 2;
            // 
            // indicatorConfigStripButton
            // 
            this.indicatorConfigStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.indicatorConfigStripButton.Image = global::StockAnalyzerApp.Properties.Resources.gear;
            this.indicatorConfigStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.indicatorConfigStripButton.Name = "indicatorConfigStripButton";
            this.indicatorConfigStripButton.Size = new System.Drawing.Size(23, 22);
            this.indicatorConfigStripButton.Text = "Configure displayed indicator";
            this.indicatorConfigStripButton.Click += new System.EventHandler(this.selectDisplayedIndicatorMenuItem_Click);
            // 
            // candleStripButton
            // 
            this.candleStripButton.CheckOnClick = true;
            this.candleStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.candleStripButton.Image = global::StockAnalyzerApp.Properties.Resources.Candle;
            this.candleStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.candleStripButton.Name = "candleStripButton";
            this.candleStripButton.Size = new System.Drawing.Size(23, 22);
            this.candleStripButton.Text = "Set candle chart";
            this.candleStripButton.Click += CandleStripButton_Click;
            // 
            // barchartStripButton
            // 
            this.barchartStripButton.CheckOnClick = true;
            this.barchartStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.barchartStripButton.Image = global::StockAnalyzerApp.Properties.Resources.BarChart;
            this.barchartStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.barchartStripButton.Name = "barchartStripButton";
            this.barchartStripButton.Size = new System.Drawing.Size(23, 22);
            this.barchartStripButton.Text = "Set bar chart";
            this.barchartStripButton.Click += BarchartStripButton_Click;
            // 
            // linechartStripButton
            // 
            this.linechartStripButton.CheckOnClick = true;
            this.linechartStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.linechartStripButton.Image = global::StockAnalyzerApp.Properties.Resources.LineChart;
            this.linechartStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.linechartStripButton.Name = "linechartStripButton";
            this.linechartStripButton.Size = new System.Drawing.Size(23, 22);
            this.linechartStripButton.Text = "Set line chart";
            this.linechartStripButton.Click += LinechartStripButton_Click;
            // 
            // saveThemeStripButton
            // 
            this.saveThemeStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveThemeStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveThemeStripButton.Image")));
            this.saveThemeStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveThemeStripButton.Name = "saveThemeStripButton";
            this.saveThemeStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveThemeStripButton.Text = "Save current theme";
            this.saveThemeStripButton.Click += new System.EventHandler(this.saveThemeMenuItem_Click);
            // 
            // defaultThemeStripButton
            // 
            this.defaultThemeStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.defaultThemeStripButton.Image = global::StockAnalyzerApp.Properties.Resources.SetDefaultTheme;
            this.defaultThemeStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.defaultThemeStripButton.Name = "defaultThemeStripButton";
            this.defaultThemeStripButton.Size = new System.Drawing.Size(23, 22);
            this.defaultThemeStripButton.Text = "Set as default theme for this stock";
            this.defaultThemeStripButton.Click += new System.EventHandler(this.defaultThemeStripButton_Click);
            // 
            // themeComboBox
            // 
            this.themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeComboBox.Name = "themeComboBox";
            this.themeComboBox.Size = new System.Drawing.Size(121, 25);
            this.themeComboBox.Sorted = true;
            this.themeComboBox.SelectedIndexChanged += new System.EventHandler(this.themeComboBox_SelectedIndexChanged);
            // 
            // deleteThemeStripButton
            // 
            this.deleteThemeStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteThemeStripButton.Image = global::StockAnalyzerApp.Properties.Resources.DeleteTheme;
            this.deleteThemeStripButton.Name = "deleteThemeStripButton";
            this.deleteThemeStripButton.Size = new System.Drawing.Size(23, 22);
            this.deleteThemeStripButton.Text = "Delete selected theme";
            this.deleteThemeStripButton.Click += new System.EventHandler(this.deleteThemeStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // portfolioComboBox
            // 
            this.portfolioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.portfolioComboBox.Name = "portfolioComboBox";
            this.portfolioComboBox.Size = new System.Drawing.Size(220, 25);
            this.portfolioComboBox.SelectedIndexChanged += new System.EventHandler(this.portfolioComboBox_SelectedIndexChanged);
            // 
            // refreshPortfolioBtn
            // 
            this.refreshPortfolioBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshPortfolioBtn.Image = global::StockAnalyzerApp.Properties.Resources.Reload;
            this.refreshPortfolioBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshPortfolioBtn.Name = "refreshPortfolioBtn";
            this.refreshPortfolioBtn.Size = new System.Drawing.Size(23, 20);
            this.refreshPortfolioBtn.Text = "Refresh Portfolio";
            this.refreshPortfolioBtn.ToolTipText = "Refresh Portfolio";
            this.refreshPortfolioBtn.Click += new System.EventHandler(this.refreshPortfolioBtn_Click);
            // 
            // portfolioStatusLbl
            // 
            this.portfolioStatusLbl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.portfolioStatusLbl.Image = global::StockAnalyzerApp.Properties.Resources.RedIcon;
            this.portfolioStatusLbl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.portfolioStatusLbl.Name = "portfolioStatusLbl";
            this.portfolioStatusLbl.Size = new System.Drawing.Size(23, 20);
            this.portfolioStatusLbl.Text = "Portfolio";
            this.portfolioStatusLbl.ToolTipText = "Not Connected";
            // 
            // browseToolStrip
            // 
            this.browseToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.browseToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rewindBtn,
            this.fastForwardBtn,
            this.copyIsinBtn,
            this.stockNameComboBox,
            this.barDurationComboBox,
            this.downloadBtn,
            this.searchCombo,
            this.toolStripSeparator6,
            this.zoomOutBtn,
            this.zoomInBtn,
            this.showVariationBtn,
            this.logScaleBtn,
            this.divScaleBtn,
            this.toolStripSeparator2,
            this.excludeButton,
            this.intradayButton,
            this.toolStripSeparator5});
            this.browseToolStrip.Location = new System.Drawing.Point(458, 0);
            this.browseToolStrip.Name = "browseToolStrip";
            this.browseToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.browseToolStrip.Size = new System.Drawing.Size(517, 27);
            this.browseToolStrip.TabIndex = 0;
            // 
            // stockNameComboBox
            // 
            this.stockNameComboBox.Name = "stockNameComboBox";
            this.stockNameComboBox.Size = new System.Drawing.Size(250, 27);
            this.stockNameComboBox.SelectedIndexChanged += new System.EventHandler(this.StockNameComboBox_SelectedIndexChanged);
            // 
            // barDurationComboBox
            // 
            this.barDurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.barDurationComboBox.Name = "barDurationComboBox";
            this.barDurationComboBox.Size = new System.Drawing.Size(125, 27);
            this.barDurationComboBox.SelectedIndexChanged += new System.EventHandler(this.BarDurationChanged);
            // 
            // downloadBtn
            // 
            this.downloadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.downloadBtn.Image = global::StockAnalyzerApp.Properties.Resources.Reload;
            this.downloadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(23, 20);
            this.downloadBtn.Text = "Download";
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // searchCombo
            // 
            this.searchCombo.Name = "searchCombo";
            this.searchCombo.Size = new System.Drawing.Size(250, 23);
            this.searchCombo.SelectedIndexChanged += new System.EventHandler(this.goToStock);
            this.searchCombo.TextChanged += SearchCombo_TextChanged;
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // rewindBtn
            // 
            this.rewindBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rewindBtn.Image = global::StockAnalyzerApp.Properties.Resources.Backward_icon;
            this.rewindBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rewindBtn.Name = "rewindBtn";
            this.rewindBtn.Size = new System.Drawing.Size(23, 20);
            this.rewindBtn.ToolTipText = "Back";
            this.rewindBtn.Click += new System.EventHandler(this.rewindBtn_Click);
            // 
            // fastForwardBtn
            // 
            this.fastForwardBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fastForwardBtn.Image = global::StockAnalyzerApp.Properties.Resources.Forward_icon;
            this.fastForwardBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fastForwardBtn.Name = "fastForwardBtn";
            this.fastForwardBtn.Size = new System.Drawing.Size(23, 20);
            this.fastForwardBtn.ToolTipText = "Next";
            this.fastForwardBtn.Click += new System.EventHandler(this.fastForwardBtn_Click);
            // 
            // copyIsinBtn
            // 
            this.copyIsinBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyIsinBtn.Image = global::StockAnalyzerApp.Properties.Resources.Copy;
            this.copyIsinBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyIsinBtn.Name = "copyIsinBtn";
            this.copyIsinBtn.Size = new System.Drawing.Size(23, 20);
            this.copyIsinBtn.ToolTipText = "Copy ISIN";
            this.copyIsinBtn.Click += new System.EventHandler(this.copyIsinBtn_Click);
            // 
            // zoomOutBtn
            // 
            this.zoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutBtn.Image = global::StockAnalyzerApp.Properties.Resources.ZoomOut;
            this.zoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomOutBtn.Name = "zoomOutBtn";
            this.zoomOutBtn.Size = new System.Drawing.Size(23, 20);
            this.zoomOutBtn.ToolTipText = "Zoom Out";
            this.zoomOutBtn.Click += new System.EventHandler(this.ZoomOutBtn_Click);
            // 
            // zoomInBtn
            // 
            this.zoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInBtn.Image = global::StockAnalyzerApp.Properties.Resources.ZoomIn;
            this.zoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomInBtn.Name = "zoomInBtn";
            this.zoomInBtn.Size = new System.Drawing.Size(23, 20);
            this.zoomInBtn.ToolTipText = "Zoon In";
            this.zoomInBtn.Click += new System.EventHandler(this.ZoomInBtn_Click);
            // 
            // logScaleBtn
            // 
            this.logScaleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.logScaleBtn.Image = global::StockAnalyzerApp.Properties.Resources.Log;
            this.logScaleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.logScaleBtn.Name = "logScaleBtn";
            this.logScaleBtn.Size = new System.Drawing.Size(23, 20);
            this.logScaleBtn.ToolTipText = "Log scale CTRL+L";
            this.logScaleBtn.Click += new System.EventHandler(this.logScaleBtn_Click);
            // 
            // divScaleBtn
            // 
            this.divScaleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.divScaleBtn.Image = global::StockAnalyzerApp.Properties.Resources.Div;
            this.divScaleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.divScaleBtn.Name = "divScaleBtn";
            this.divScaleBtn.Size = new System.Drawing.Size(23, 20);
            this.divScaleBtn.ToolTipText = "div scale CTRL+L";
            this.divScaleBtn.Click += new System.EventHandler(this.divScaleBtn_Click);
            // 
            // showVariationBtn
            // 
            this.showVariationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.showVariationBtn.Name = "showVariationBtn";
            this.showVariationBtn.Text = "%";
            this.showVariationBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.showVariationBtn.Size = new System.Drawing.Size(23, 20);
            this.showVariationBtn.ToolTipText = "Show bar variation CTRL+V";
            this.showVariationBtn.Click += new System.EventHandler(this.showVariationBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // excludeButton
            // 
            this.excludeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.excludeButton.Image = global::StockAnalyzerApp.Properties.Resources.Delete;
            this.excludeButton.Name = "excludeButton";
            this.excludeButton.Size = new System.Drawing.Size(23, 20);
            this.excludeButton.Text = "Exclude";
            this.excludeButton.ToolTipText = "Exclude value from list";
            this.excludeButton.Click += new System.EventHandler(this.excludeButton_Click);
            // 
            // intradayButton
            // 
            this.intradayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.intradayButton.Image = global::StockAnalyzerApp.Properties.Resources.Binoculars;
            this.intradayButton.Name = "intradayButton";
            this.intradayButton.Size = new System.Drawing.Size(23, 20);
            this.intradayButton.Text = "Intraday";
            this.intradayButton.ToolTipText = "Add or remove from intraday list";
            this.intradayButton.Click += new System.EventHandler(this.intradayButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // drawToolStrip
            // 
            this.drawToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.drawToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.magnetStripBtn,
            this.drawCupHandleStripBtn,
            this.drawWinRatioStripBtn,
            this.drawBoxStripBtn,
            this.drawLineStripBtn,
            this.addHalfLineStripBtn,
            this.addSegmentStripBtn,
            this.copyLineStripBtn,
            this.cutLineStripBtn,
            this.deleteLineStripBtn,
            this.drawingStyleStripBtn,
            this.toolStripSeparator13,
            this.saveAnalysisToolStripButton,
            this.snapshotToolStripButton,
            this.toolStripSeparator7,
            this.generateDailyReportToolStripBtn,
            this.AddToWatchListToolStripDropDownButton});
            this.drawToolStrip.Location = new System.Drawing.Point(3, 27);
            this.drawToolStrip.Name = "drawToolStrip";
            this.drawToolStrip.Size = new System.Drawing.Size(421, 25);
            this.drawToolStrip.TabIndex = 1;
            // 
            // magnetStripBtn
            // 
            this.magnetStripBtn.CheckOnClick = true;
            this.magnetStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.magnetStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.magnet_pencil;
            this.magnetStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.magnetStripBtn.Name = "magnetStripBtn";
            this.magnetStripBtn.Size = new System.Drawing.Size(23, 22);
            this.magnetStripBtn.Text = "Magnet to higher high and lower lows";
            this.magnetStripBtn.Click += new System.EventHandler(this.magnetStripBtn_Click);
            // 
            // drawLineStripBtn
            // 
            this.drawLineStripBtn.CheckOnClick = true;
            this.drawLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawLineStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddLine;
            this.drawLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawLineStripBtn.Name = "drawLineStripBtn";
            this.drawLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.drawLineStripBtn.Text = "Add a line";
            this.drawLineStripBtn.Click += new System.EventHandler(this.drawLineStripBtn_Click);
            // 
            // drawWinRatioStripBtn
            // 
            this.drawWinRatioStripBtn.CheckOnClick = true;
            this.drawWinRatioStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawWinRatioStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddWinRatio;
            this.drawWinRatioStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawWinRatioStripBtn.Name = "drawWinRatioStripBtn";
            this.drawWinRatioStripBtn.Size = new System.Drawing.Size(23, 22);
            this.drawWinRatioStripBtn.Text = "Draw win ratio";
            this.drawWinRatioStripBtn.Click += new System.EventHandler(this.drawWinRatioStripBtn_Click);
            // 
            // drawBoxStripBtn
            // 
            this.drawBoxStripBtn.CheckOnClick = true;
            this.drawBoxStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawBoxStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddBox;
            this.drawBoxStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawBoxStripBtn.Name = "drawBoxStripBtn";
            this.drawBoxStripBtn.Size = new System.Drawing.Size(23, 22);
            this.drawBoxStripBtn.Text = "Draw Box";
            this.drawBoxStripBtn.Click += new System.EventHandler(this.drawBoxStripBtn_Click);
            // 
            // addHalfLineStripBtn
            // 
            this.addHalfLineStripBtn.CheckOnClick = true;
            this.addHalfLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addHalfLineStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddSemiLine;
            this.addHalfLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addHalfLineStripBtn.Name = "addHalfLineStripBtn";
            this.addHalfLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.addHalfLineStripBtn.Text = "Add a half line";
            this.addHalfLineStripBtn.Click += new System.EventHandler(this.addHalfLineStripBtn_Click);
            // 
            // addSegmentStripBtn
            // 
            this.addSegmentStripBtn.CheckOnClick = true;
            this.addSegmentStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addSegmentStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddSegment;
            this.addSegmentStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addSegmentStripBtn.Name = "addSegmentStripBtn";
            this.addSegmentStripBtn.Size = new System.Drawing.Size(23, 22);
            this.addSegmentStripBtn.Text = "Add a segment";
            this.addSegmentStripBtn.Click += new System.EventHandler(this.addSegmentStripBtn_Click);
            // 
            // cupHandleBtn
            // 
            this.drawCupHandleStripBtn.CheckOnClick = true;
            this.drawCupHandleStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawCupHandleStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.CupHandle;
            this.drawCupHandleStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawCupHandleStripBtn.Name = "cupHandleBtn";
            this.drawCupHandleStripBtn.Size = new System.Drawing.Size(23, 22);
            this.drawCupHandleStripBtn.Text = "cupHandleBtn";
            this.drawCupHandleStripBtn.ToolTipText = "Draw cup and handle";
            this.drawCupHandleStripBtn.Click += new System.EventHandler(this.cupHandleBtn_Click);
            // 
            // copyLineStripBtn
            // 
            this.copyLineStripBtn.CheckOnClick = true;
            this.copyLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyLineStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.CopyLine;
            this.copyLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyLineStripBtn.Name = "copyLineStripBtn";
            this.copyLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.copyLineStripBtn.Text = "Copy a line";
            this.copyLineStripBtn.Click += new System.EventHandler(this.copyLineStripBtn_Click);
            // 
            // cutLineStripBtn
            // 
            this.cutLineStripBtn.CheckOnClick = true;
            this.cutLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cutLineStripBtn.Image = ((System.Drawing.Image)(resources.GetObject("cutLineStripBtn.Image")));
            this.cutLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutLineStripBtn.Name = "cutLineStripBtn";
            this.cutLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.cutLineStripBtn.Text = "Cut a line";
            this.cutLineStripBtn.ToolTipText = "Cut the right part of a line, press CTRL to cut the left part";
            this.cutLineStripBtn.Click += new System.EventHandler(this.cutLineStripBtn_Click);
            // 
            // deleteLineStripBtn
            // 
            this.deleteLineStripBtn.CheckOnClick = true;
            this.deleteLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteLineStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.trashcan;
            this.deleteLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteLineStripBtn.Name = "deleteLineStripBtn";
            this.deleteLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.deleteLineStripBtn.Text = "Delete selected item";
            this.deleteLineStripBtn.Click += new System.EventHandler(this.deleteLineStripBtn_Click);
            // 
            // drawingStyleStripBtn
            // 
            this.drawingStyleStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawingStyleStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.Paint;
            this.drawingStyleStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawingStyleStripBtn.Name = "drawingStyleStripBtn";
            this.drawingStyleStripBtn.Size = new System.Drawing.Size(23, 22);
            this.drawingStyleStripBtn.Text = "Define drawing style";
            this.drawingStyleStripBtn.Click += new System.EventHandler(this.drawingStyleStripBtn_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 25);
            // 
            // saveAnalysisToolStripButton
            // 
            this.saveAnalysisToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAnalysisToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveAnalysisToolStripButton.Image")));
            this.saveAnalysisToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAnalysisToolStripButton.Name = "saveAnalysisToolStripButton";
            this.saveAnalysisToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveAnalysisToolStripButton.Text = "&Save analysis";
            this.saveAnalysisToolStripButton.Click += new System.EventHandler(this.saveAnalysisToolStripButton_Click);
            // 
            // snapshotToolStripButton
            // 
            this.snapshotToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.snapshotToolStripButton.Image = global::StockAnalyzerApp.Properties.Resources.Camera;
            this.snapshotToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.snapshotToolStripButton.Name = "snapshotToolStripButton";
            this.snapshotToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.snapshotToolStripButton.Text = "Snapshot";
            this.snapshotToolStripButton.Click += new System.EventHandler(this.snapshotToolStripButton_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // generateDailyReportToolStripBtn
            // 
            this.generateDailyReportToolStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.generateDailyReportToolStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.EMail;
            this.generateDailyReportToolStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.generateDailyReportToolStripBtn.Name = "generateDailyReportToolStripBtn";
            this.generateDailyReportToolStripBtn.Size = new System.Drawing.Size(23, 22);
            this.generateDailyReportToolStripBtn.Text = "Generate Daily Report";
            this.generateDailyReportToolStripBtn.Click += new System.EventHandler(this.generateDailyReportToolStripBtn_Click);
            // 
            // AddToWatchListToolStripDropDownButton
            // 
            this.AddToWatchListToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddToWatchListToolStripDropDownButton.Image = global::StockAnalyzerApp.Properties.Resources.AddFolder;
            this.AddToWatchListToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddToWatchListToolStripDropDownButton.Name = "AddToWatchListToolStripDropDownButton";
            this.AddToWatchListToolStripDropDownButton.Size = new System.Drawing.Size(29, 22);
            this.AddToWatchListToolStripDropDownButton.Text = "Add to watch list";
            // 
            // graphCloseControl
            // 
            this.graphCloseControl.Agenda = null;
            this.graphCloseControl.BackgroundColor = System.Drawing.Color.White;
            this.graphCloseControl.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphCloseControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphCloseControl.CurveList = null;
            this.graphCloseControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphCloseControl.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphCloseControl.DrawingPen = null;
            this.graphCloseControl.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphCloseControl.EndIndex = 0;
            this.graphCloseControl.GridColor = System.Drawing.Color.Empty;
            this.graphCloseControl.HideIndicators = false;
            this.graphCloseControl.horizontalLines = null;
            this.graphCloseControl.IsLogScale = false;
            this.graphCloseControl.Location = new System.Drawing.Point(0, 24);
            this.graphCloseControl.Magnetism = false;
            this.graphCloseControl.Name = "graphCloseControl";
            this.graphCloseControl.ScaleInvisible = false;
            this.graphCloseControl.SecondaryFloatSerie = null;
            this.graphCloseControl.SecondaryPen = null;
            this.graphCloseControl.ShowGrid = false;
            this.graphCloseControl.Size = new System.Drawing.Size(975, 583);
            this.graphCloseControl.StartIndex = 0;
            this.graphCloseControl.TabIndex = 0;
            this.graphCloseControl.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphCloseControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphScrollerControl
            // 
            this.graphScrollerControl.AutoSize = true;
            this.graphScrollerControl.BackgroundColor = System.Drawing.Color.White;
            this.graphScrollerControl.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphScrollerControl.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.graphScrollerControl.CurveList = null;
            this.graphScrollerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphScrollerControl.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphScrollerControl.DrawingPen = null;
            this.graphScrollerControl.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphScrollerControl.EndIndex = 0;
            this.graphScrollerControl.GridColor = System.Drawing.Color.Empty;
            this.graphScrollerControl.horizontalLines = null;
            this.graphScrollerControl.IsLogScale = false;
            this.graphScrollerControl.Location = new System.Drawing.Point(0, 24);
            this.graphScrollerControl.Name = "graphScrollerControl";
            this.graphScrollerControl.ScaleInvisible = false;
            this.graphScrollerControl.ShowGrid = false;
            this.graphScrollerControl.Size = new System.Drawing.Size(975, 583);
            this.graphScrollerControl.StartIndex = 0;
            this.graphScrollerControl.TabIndex = 2;
            this.graphScrollerControl.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphScrollerControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator1Control
            // 
            this.graphIndicator1Control.AutoSize = true;
            this.graphIndicator1Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator1Control.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator1Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator1Control.CurveList = null;
            this.graphIndicator1Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator1Control.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator1Control.DrawingPen = null;
            this.graphIndicator1Control.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphIndicator1Control.EndIndex = 0;
            this.graphIndicator1Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator1Control.horizontalLines = null;
            this.graphIndicator1Control.IsLogScale = false;
            this.graphIndicator1Control.Location = new System.Drawing.Point(0, 24);
            this.graphIndicator1Control.Name = "graphIndicator1Control";
            this.graphIndicator1Control.RangeMax = 0F;
            this.graphIndicator1Control.RangeMin = 0F;
            this.graphIndicator1Control.ScaleInvisible = false;
            this.graphIndicator1Control.ShowGrid = false;
            this.graphIndicator1Control.Size = new System.Drawing.Size(975, 583);
            this.graphIndicator1Control.StartIndex = 0;
            this.graphIndicator1Control.TabIndex = 2;
            this.graphIndicator1Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator1Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator2Control
            // 
            this.graphIndicator2Control.AutoSize = true;
            this.graphIndicator2Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator2Control.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator2Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator2Control.CurveList = null;
            this.graphIndicator2Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator2Control.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator2Control.DrawingPen = null;
            this.graphIndicator2Control.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphIndicator2Control.EndIndex = 0;
            this.graphIndicator2Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator2Control.horizontalLines = null;
            this.graphIndicator2Control.IsLogScale = false;
            this.graphIndicator2Control.Location = new System.Drawing.Point(0, 24);
            this.graphIndicator2Control.Name = "graphIndicator2Control";
            this.graphIndicator2Control.RangeMax = 0F;
            this.graphIndicator2Control.RangeMin = 0F;
            this.graphIndicator2Control.ScaleInvisible = false;
            this.graphIndicator2Control.ShowGrid = false;
            this.graphIndicator2Control.Size = new System.Drawing.Size(975, 583);
            this.graphIndicator2Control.StartIndex = 0;
            this.graphIndicator2Control.TabIndex = 1;
            this.graphIndicator2Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator2Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator3Control
            // 
            this.graphIndicator3Control.AutoSize = true;
            this.graphIndicator3Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator3Control.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator3Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator3Control.CurveList = null;
            this.graphIndicator3Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator3Control.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator3Control.DrawingPen = null;
            this.graphIndicator3Control.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphIndicator3Control.EndIndex = 0;
            this.graphIndicator3Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator3Control.horizontalLines = null;
            this.graphIndicator3Control.IsLogScale = false;
            this.graphIndicator3Control.Location = new System.Drawing.Point(0, 24);
            this.graphIndicator3Control.Name = "graphIndicator3Control";
            this.graphIndicator3Control.RangeMax = 0F;
            this.graphIndicator3Control.RangeMin = 0F;
            this.graphIndicator3Control.ScaleInvisible = false;
            this.graphIndicator3Control.ShowGrid = false;
            this.graphIndicator3Control.Size = new System.Drawing.Size(975, 583);
            this.graphIndicator3Control.StartIndex = 0;
            this.graphIndicator3Control.TabIndex = 1;
            this.graphIndicator3Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator3Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphVolumeControl
            // 
            this.graphVolumeControl.AutoSize = true;
            this.graphVolumeControl.BackgroundColor = System.Drawing.Color.White;
            this.graphVolumeControl.ChartMode = CustomControl.GraphControls.GraphChartMode.Line;
            this.graphVolumeControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphVolumeControl.CurveList = null;
            this.graphVolumeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphVolumeControl.DrawingMode = CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphVolumeControl.DrawingPen = null;
            this.graphVolumeControl.DrawingStep = CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphVolumeControl.EndIndex = 0;
            this.graphVolumeControl.GridColor = System.Drawing.Color.Empty;
            this.graphVolumeControl.horizontalLines = null;
            this.graphVolumeControl.IsLogScale = false;
            this.graphVolumeControl.Location = new System.Drawing.Point(0, 24);
            this.graphVolumeControl.Name = "graphVolumeControl";
            this.graphVolumeControl.ScaleInvisible = false;
            this.graphVolumeControl.ShowGrid = false;
            this.graphVolumeControl.Size = new System.Drawing.Size(975, 583);
            this.graphVolumeControl.StartIndex = 0;
            this.graphVolumeControl.TabIndex = 2;
            this.graphVolumeControl.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphVolumeControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 25);
            // 
            // StockAnalyzerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 650);
            this.Controls.Add(this.graphCloseControl);
            this.Controls.Add(this.graphScrollerControl);
            this.Controls.Add(this.graphIndicator1Control);
            this.Controls.Add(this.graphIndicator2Control);
            this.Controls.Add(this.graphIndicator3Control);
            this.Controls.Add(this.graphVolumeControl);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.mainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Name = "StockAnalyzerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ultimate Chartist";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.themeToolStrip.ResumeLayout(false);
            this.themeToolStrip.PerformLayout();
            this.browseToolStrip.ResumeLayout(false);
            this.browseToolStrip.PerformLayout();
            this.drawToolStrip.ResumeLayout(false);
            this.drawToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip browseToolStrip;
        private System.Windows.Forms.ToolStrip drawToolStrip;
        private CustomControl.GraphControls.GraphCloseControl graphCloseControl;
        private CustomControl.GraphControls.GraphScrollerControl graphScrollerControl;
        private CustomControl.GraphControls.GraphRangedControl graphIndicator1Control;
        private CustomControl.GraphControls.GraphRangedControl graphIndicator2Control;
        private CustomControl.GraphControls.GraphRangedControl graphIndicator3Control;
        private CustomControl.GraphControls.GraphVolumeControl graphVolumeControl;
        private ToolStripComboBox stockNameComboBox;
        private ToolStripComboBox barDurationComboBox;
        private ToolStripMenuItem fileMenuItem;
        private ToolStripMenuItem helpMenuItem;
        private ToolStripMenuItem optionsMenuItem;
        private ToolStripMenuItem configDataProviderMenuItem;
        private ToolStripButton drawLineStripBtn;
        private ToolStripButton drawWinRatioStripBtn;
        private ToolStripButton drawBoxStripBtn;
        private ToolStripButton drawCupHandleStripBtn;
        private ToolStripButton copyLineStripBtn;
        private ToolStripButton deleteLineStripBtn;
        private ToolStripButton drawingStyleStripBtn;
        private ToolStripButton addSegmentStripBtn;
        private ToolStripButton addHalfLineStripBtn;
        private ToolStripButton saveAnalysisToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton excludeButton;
        private ToolStripButton intradayButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem viewMenuItem;
        private ToolStripMenuItem portfolioMenuItem;
        private ToolStripButton magnetStripBtn;
        private ToolStripMenuItem palmaresMenuItem;
        private ToolStripMenuItem instrumentsMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private ToolStripMenuItem portfolioReportMenuItem;
        private ToolStripMenuItem showOrdersMenuItem;
        private ToolStripMenuItem showPositionsMenuItem;
        private ToolStripMenuItem showDrawingsMenuItem;
        private ToolStripMenuItem showEventMarqueeMenuItem;
        private ToolStripMenuItem showCommentMarqueeMenuItem;
        private ToolStripMenuItem showDividendMenuItem;
        private ToolStripMenuItem showIndicatorDivMenuItem;
        private ToolStripMenuItem showIndicatorTextMenuItem;
        private ToolStripMenuItem analysisMenuItem;
        private ToolStripMenuItem agentTunningMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem portfolioSimulationMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton zoomInBtn;
        private ToolStripButton rewindBtn;
        private ToolStripButton showVariationBtn;
        private ToolStripButton logScaleBtn;
        private ToolStripButton divScaleBtn;
        private ToolStripButton fastForwardBtn;
        private ToolStripButton copyIsinBtn;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripMenuItem currentPortfolioMenuItem;
        private ToolStripMenuItem autoTradeMenuItem;
        private ToolStripMenuItem showHorseRaceViewMenuItem;
        private ToolStripMenuItem marketReplayViewMenuItem;
        private ToolStripMenuItem multipleTimeFrameViewMenuItem;
        private ToolStripMenuItem showAlertDefDialogMenuItem;
        private ToolStripMenuItem drawingDialogMenuItem;
        private ToolStripMenuItem hideIndicatorsStockMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem logSerieMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripButton generateDailyReportToolStripBtn;
        private ToolStripDropDownButton AddToWatchListToolStripDropDownButton;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem watchlistsMenuItem;
        private ToolStripMenuItem manageWatchlistsMenuItem;
        private ToolStripMenuItem scriptEditorMenuItem;
        private TableLayoutPanel indicatorLayoutPanel;
        private ToolStripMenuItem stockFilterMenuItem;
        private ToolStripMenuItem secondarySerieMenuItem;
        private ToolStripMenuItem bestTrendMenuItem;
        private ToolStripMenuItem tweetMenuItem;
        private ToolStripMenuItem sectorMenuItem;
        private ToolStripMenuItem expectedValueMenuItem;
        private ToolStripMenuItem statisticsMenuItem;
        private ToolStripButton snapshotToolStripButton;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripComboBox themeComboBox;
        private ToolStripComboBox portfolioComboBox;
        private ToolStripButton refreshPortfolioBtn;
        private ToolStripStatusLabel portfolioStatusLbl;
        private ToolStripMenuItem showAgendaMenuItem;
        private ToolStrip themeToolStrip;
        private ToolStripButton defaultThemeStripButton;
        private ToolStripButton deleteThemeStripButton;
        private ToolStripButton saveThemeStripButton;
        private ToolStripButton indicatorConfigStripButton;
        private ToolStripButton candleStripButton;
        private ToolStripButton barchartStripButton;
        private ToolStripButton linechartStripButton;
        private ToolStripButton zoomOutBtn;
        private ToolStripMenuItem showShowStatusBarMenuItem;
        private ToolStripButton cutLineStripBtn;
        private ToolStripButton downloadBtn;
        private ToolStripMenuItem aboutMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem loadAnalysisFileMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem saveAnalysisFileMenuItem;
        private ToolStripMenuItem saveAnalysisFileAsMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripMenuItem eraseDrawingsToolStripMenuItem;
        private ToolStripMenuItem eraseAllDrawingsToolStripMenuItem;
        private ToolStripMenuItem stockScannerMenuItem;
        private ToolStripMenuItem stockSplitMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem saveThemeMenuItem;
        private ToolStripMenuItem newAnalysisMenuItem;
        private ToolStripComboBox searchCombo;
    }
}