using System.Windows.Forms;
using StockAnalyzerApp.CustomControl;

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
            this.eraseAllDrawingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showShowStatusBarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideIndicatorsStockMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDrawingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOrdersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.showAgendaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEventMarqueeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCommentMarqueeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIndicatorDivMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIndicatorTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.showHorseRaceViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAlertDialogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.secondarySerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indexRelativeStrengthMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inverseSerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logSerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockScannerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stockStrategyScannerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.palmaresMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.strategySimulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filteredStrategySimulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchStrategySimulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSeasonalitySerieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.portofolioSimulationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.exportFinancialsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.patternRecognitionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portofolioMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentPortofolioMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nameMappingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.saveThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.defaultThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.themeComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.deleteThemeStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.strategyComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.portfolioComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.browseToolStrip = new System.Windows.Forms.ToolStrip();
            this.stockNameComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.barDurationComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.smoothingLabel = new System.Windows.Forms.ToolStripLabel();
            this.barSmoothingComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.barHeikinAshiCheckBox = new  CustomControl.ToolStripCheckedBox();
            this.downloadBtn = new System.Windows.Forms.ToolStripButton();
            this.searchText = new System.Windows.Forms.ToolStripTextBox();
            this.goBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.rewindBtn = new System.Windows.Forms.ToolStripButton();
            this.fastForwardBtn = new System.Windows.Forms.ToolStripButton();
            this.zoomOutBtn = new System.Windows.Forms.ToolStripButton();
            this.zoomInBtn = new System.Windows.Forms.ToolStripButton();
            this.logScaleBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.followUpCheckBox = new  CustomControl.ToolStripCheckedBox();
            this.excludeButton = new System.Windows.Forms.ToolStripButton();
            this.commentBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.drawToolStrip = new System.Windows.Forms.ToolStrip();
            this.magnetStripBtn = new System.Windows.Forms.ToolStripButton();
            this.sarLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.addHalfLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.addSegmentStripBtn = new System.Windows.Forms.ToolStripButton();
            this.fanLineBtn = new System.Windows.Forms.ToolStripButton();
            this.copyLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.cutLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.generateChannelStripButton = new System.Windows.Forms.ToolStripButton();
            this.deleteLineStripBtn = new System.Windows.Forms.ToolStripButton();
            this.drawingStyleStripBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAnalysisToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.snapshotToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.addToReportStripBtn = new System.Windows.Forms.ToolStripButton();
            this.generateDailyReportToolStripBtn = new System.Windows.Forms.ToolStripButton();
            this.AddToWatchListToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.graphCloseControl = new  CustomControl.GraphControls.GraphCloseControl();
            this.graphScrollerControl = new  CustomControl.GraphControls.GraphScrollerControl();
            this.graphIndicator1Control = new  CustomControl.GraphControls.GraphRangedControl();
            this.graphIndicator2Control = new  CustomControl.GraphControls.GraphRangedControl();
            this.graphIndicator3Control = new  CustomControl.GraphControls.GraphRangedControl();
            this.graphVolumeControl = new  CustomControl.GraphControls.GraphVolumeControl();
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
            this.portofolioMenuItem,
            this.watchlistsMenuItem,
            this.helpMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(1120, 24);
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
            this.loadAnalysisFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
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
            this.optionsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
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
            // eraseAllDrawingsToolStripMenuItem
            // 
            this.eraseAllDrawingsToolStripMenuItem.Image = global::StockAnalyzerApp.Properties.Resources.trashcan;
            this.eraseAllDrawingsToolStripMenuItem.Name = "eraseAllDrawingsToolStripMenuItem";
            this.eraseAllDrawingsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
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
            this.toolStripSeparator14,
            this.showAgendaMenuItem,
            this.showEventMarqueeMenuItem,
            this.showCommentMarqueeMenuItem,
            this.showIndicatorDivMenuItem,
            this.showIndicatorTextMenuItem,
            this.toolStripSeparator16,
            this.showHorseRaceViewMenuItem,
            this.showAlertDialogMenuItem,
            this.toolStripSeparator15,
            this.secondarySerieMenuItem,
            this.indexRelativeStrengthMenuItem,
            this.inverseSerieMenuItem,
            this.logSerieMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewMenuItem.Text = "View";
            // 
            // showShowStatusBarMenuItem
            // 
            this.showShowStatusBarMenuItem.CheckOnClick = true;
            this.showShowStatusBarMenuItem.Name = "showShowStatusBarMenuItem";
            this.showShowStatusBarMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showShowStatusBarMenuItem.Text = "Show Status bar";
            this.showShowStatusBarMenuItem.Click += new System.EventHandler(this.showShowStatusBarMenuItem_Click);
            // 
            // hideIndicatorsStockMenuItem
            // 
            this.hideIndicatorsStockMenuItem.CheckOnClick = true;
            this.hideIndicatorsStockMenuItem.Name = "hideIndicatorsStockMenuItem";
            this.hideIndicatorsStockMenuItem.Size = new System.Drawing.Size(215, 22);
            this.hideIndicatorsStockMenuItem.Text = "Hide indicators";
            this.hideIndicatorsStockMenuItem.Click += new System.EventHandler(this.hideIndicatorsStockMenuItem_Click);
            // 
            // showDrawingsMenuItem
            // 
            this.showDrawingsMenuItem.Checked = true;
            this.showDrawingsMenuItem.CheckOnClick = true;
            this.showDrawingsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showDrawingsMenuItem.Name = "showDrawingsMenuItem";
            this.showDrawingsMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showDrawingsMenuItem.Text = "Show Drawings";
            this.showDrawingsMenuItem.Click += new System.EventHandler(this.showDrawingsMenuItem_Click);
            // 
            // showOrdersMenuItem
            // 
            this.showOrdersMenuItem.Checked = true;
            this.showOrdersMenuItem.CheckOnClick = true;
            this.showOrdersMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showOrdersMenuItem.Name = "showOrdersMenuItem";
            this.showOrdersMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showOrdersMenuItem.Text = "Show Orders";
            this.showOrdersMenuItem.Click += new System.EventHandler(this.showOrdersMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(212, 6);
            // 
            // showAgendaMenuItem
            // 
            this.showAgendaMenuItem.Name = "showAgendaMenuItem";
            this.showAgendaMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showAgendaMenuItem.Text = "Show Agenda Entries";
            // 
            // showEventMarqueeMenuItem
            // 
            this.showEventMarqueeMenuItem.Checked = true;
            this.showEventMarqueeMenuItem.CheckOnClick = true;
            this.showEventMarqueeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showEventMarqueeMenuItem.Name = "showEventMarqueeMenuItem";
            this.showEventMarqueeMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showEventMarqueeMenuItem.Text = "Show Event Marquees";
            this.showEventMarqueeMenuItem.Click += new System.EventHandler(this.showEventMarqueeMenuItem_Click);
            // 
            // showCommentMarqueeMenuItem
            // 
            this.showCommentMarqueeMenuItem.Checked = true;
            this.showCommentMarqueeMenuItem.CheckOnClick = true;
            this.showCommentMarqueeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCommentMarqueeMenuItem.Name = "showCommentMarqueeMenuItem";
            this.showCommentMarqueeMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showCommentMarqueeMenuItem.Text = "Show Comment Marquees";
            this.showCommentMarqueeMenuItem.Click += new System.EventHandler(this.showCommentMarqueeMenuItem_Click);
            // 
            // showIndicatorDivMenuItem
            // 
            this.showIndicatorDivMenuItem.Checked = true;
            this.showIndicatorDivMenuItem.CheckOnClick = true;
            this.showIndicatorDivMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showIndicatorDivMenuItem.Name = "showIndicatorDivMenuItem";
            this.showIndicatorDivMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showIndicatorDivMenuItem.Text = "Show Divergences";
            this.showIndicatorDivMenuItem.Click += new System.EventHandler(this.showIndicatorDivMenuItem_Click);
            // 
            // showIndicatorTextMenuItem
            // 
            this.showIndicatorTextMenuItem.Checked = true;
            this.showIndicatorTextMenuItem.CheckOnClick = true;
            this.showIndicatorTextMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showIndicatorTextMenuItem.Name = "showIndicatorTextMenuItem";
            this.showIndicatorTextMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showIndicatorTextMenuItem.Text = "Show Indicator Text";
            this.showIndicatorTextMenuItem.Click += new System.EventHandler(this.showIndicatorTextMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(212, 6);
            // 
            // showHorseRaceViewMenuItem
            // 
            this.showHorseRaceViewMenuItem.Name = "showHorseRaceViewMenuItem";
            this.showHorseRaceViewMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.showHorseRaceViewMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showHorseRaceViewMenuItem.Text = "Horse Race View";
            // 
            // showAlertDialogMenuItem
            // 
            this.showAlertDialogMenuItem.Name = "showAlertDialogMenuItem";
            this.showAlertDialogMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.showAlertDialogMenuItem.Size = new System.Drawing.Size(215, 22);
            this.showAlertDialogMenuItem.Text = "Alert View";
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(212, 6);
            // 
            // secondarySerieMenuItem
            // 
            this.secondarySerieMenuItem.Name = "secondarySerieMenuItem";
            this.secondarySerieMenuItem.Size = new System.Drawing.Size(215, 22);
            this.secondarySerieMenuItem.Text = "Secondary Serie";
            // 
            // indexRelativeStrengthMenuItem
            // 
            this.indexRelativeStrengthMenuItem.Name = "indexRelativeStrengthMenuItem";
            this.indexRelativeStrengthMenuItem.Size = new System.Drawing.Size(215, 22);
            this.indexRelativeStrengthMenuItem.Text = "Index Relative Strength";
            // 
            // inverseSerieMenuItem
            // 
            this.inverseSerieMenuItem.Name = "inverseSerieMenuItem";
            this.inverseSerieMenuItem.Size = new System.Drawing.Size(215, 22);
            this.inverseSerieMenuItem.Text = "Inverse Serie";
            this.inverseSerieMenuItem.Click += new System.EventHandler(this.inverseSerieMenuItem_Click);
            // 
            // logSerieMenuItem
            // 
            this.logSerieMenuItem.Name = "logSerieMenuItem";
            this.logSerieMenuItem.Size = new System.Drawing.Size(215, 22);
            this.logSerieMenuItem.Text = "Log Serie";
            this.logSerieMenuItem.Click += new System.EventHandler(this.logSerieMenuItem_Click);
            // 
            // analysisMenuItem
            // 
            this.analysisMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stockScannerMenuItem,
            this.stockStrategyScannerMenuItem,
            this.palmaresMenuItem,
            this.toolStripSeparator10,
            this.strategySimulationMenuItem,
            this.filteredStrategySimulationMenuItem,
            this.batchStrategySimulationMenuItem,
            this.generateSeasonalitySerieMenuItem,
            this.toolStripSeparator8,
            this.portofolioSimulationMenuItem,
            this.toolStripSeparator20,
            this.exportFinancialsMenuItem,
            this.toolStripSeparator22,
            this.patternRecognitionMenuItem});
            this.analysisMenuItem.Name = "analysisMenuItem";
            this.analysisMenuItem.Size = new System.Drawing.Size(62, 20);
            this.analysisMenuItem.Text = "Analysis";
            // 
            // stockScannerMenuItem
            // 
            this.stockScannerMenuItem.Name = "stockScannerMenuItem";
            this.stockScannerMenuItem.Size = new System.Drawing.Size(219, 22);
            this.stockScannerMenuItem.Text = "Stock Scanner";
            this.stockScannerMenuItem.Click += new System.EventHandler(this.stockScannerMenuItem_Click);
            // 
            // stockStrategyScannerMenuItem
            // 
            this.stockStrategyScannerMenuItem.Name = "stockStrategyScannerMenuItem";
            this.stockStrategyScannerMenuItem.Size = new System.Drawing.Size(219, 22);
            this.stockStrategyScannerMenuItem.Text = "Strategy Scanner";
            this.stockStrategyScannerMenuItem.Click += new System.EventHandler(this.stockStrategyScannerMenuItem_Click);
            // 
            // palmaresMenuItem
            // 
            this.palmaresMenuItem.Name = "palmaresMenuItem";
            this.palmaresMenuItem.Size = new System.Drawing.Size(219, 22);
            this.palmaresMenuItem.Text = "Palmares";
            this.palmaresMenuItem.Click += new System.EventHandler(this.palmaresMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(216, 6);
            // 
            // strategySimulationMenuItem
            // 
            this.strategySimulationMenuItem.Name = "strategySimulationMenuItem";
            this.strategySimulationMenuItem.Size = new System.Drawing.Size(219, 22);
            this.strategySimulationMenuItem.Text = "Strategy Simulation";
            this.strategySimulationMenuItem.Click += new System.EventHandler(this.strategySimulationMenuItem_Click);
            // 
            // filteredStrategySimulationMenuItem
            // 
            this.filteredStrategySimulationMenuItem.Name = "filteredStrategySimulationMenuItem";
            this.filteredStrategySimulationMenuItem.Size = new System.Drawing.Size(219, 22);
            this.filteredStrategySimulationMenuItem.Text = "Filtered Strategy Simulation";
            this.filteredStrategySimulationMenuItem.Click += new System.EventHandler(this.filteredStrategySimulationMenuItem_Click);
            // 
            // batchStrategySimulationMenuItem
            // 
            this.batchStrategySimulationMenuItem.Name = "batchStrategySimulationMenuItem";
            this.batchStrategySimulationMenuItem.Size = new System.Drawing.Size(219, 22);
            this.batchStrategySimulationMenuItem.Text = "Batch Strategy Simulation";
            this.batchStrategySimulationMenuItem.Click += new System.EventHandler(this.batchStrategySimulationMenuItem_Click);
            // 
            // generateSeasonalitySerieMenuItem
            // 
            this.generateSeasonalitySerieMenuItem.Name = "generateSeasonalitySerieMenuItem";
            this.generateSeasonalitySerieMenuItem.Size = new System.Drawing.Size(219, 22);
            this.generateSeasonalitySerieMenuItem.Text = "Generate Seasonality Serie";
            this.generateSeasonalitySerieMenuItem.Click += new System.EventHandler(this.generateSeasonalitySerieMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(216, 6);
            // 
            // portofolioSimulationMenuItem
            // 
            this.portofolioSimulationMenuItem.Name = "portofolioSimulationMenuItem";
            this.portofolioSimulationMenuItem.Size = new System.Drawing.Size(219, 22);
            this.portofolioSimulationMenuItem.Text = "Portofolio Simulation";
            this.portofolioSimulationMenuItem.Click += new System.EventHandler(this.portofolioSimulationMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(216, 6);
            // 
            // exportFinancialsMenuItem
            // 
            this.exportFinancialsMenuItem.Name = "exportFinancialsMenuItem";
            this.exportFinancialsMenuItem.Size = new System.Drawing.Size(219, 22);
            this.exportFinancialsMenuItem.Text = "Export Financials";
            this.exportFinancialsMenuItem.Click += new System.EventHandler(this.exportFinancialsMenuItem_Click);
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(216, 6);
            // 
            // patternRecognitionMenuItem
            // 
            this.patternRecognitionMenuItem.Name = "patternRecognitionMenuItem";
            this.patternRecognitionMenuItem.Size = new System.Drawing.Size(219, 22);
            this.patternRecognitionMenuItem.Text = "Pattern Recognition";
            this.patternRecognitionMenuItem.Click += new System.EventHandler(this.patternRecognitionMenuItem_Click);
            // 
            // portofolioMenuItem
            // 
            this.portofolioMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentPortofolioMenuItem,
            this.nameMappingMenuItem});
            this.portofolioMenuItem.Name = "portofolioMenuItem";
            this.portofolioMenuItem.Size = new System.Drawing.Size(77, 20);
            this.portofolioMenuItem.Text = "Portofolios";
            // 
            // currentPortofolioMenuItem
            // 
            this.currentPortofolioMenuItem.Name = "currentPortofolioMenuItem";
            this.currentPortofolioMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F2)));
            this.currentPortofolioMenuItem.Size = new System.Drawing.Size(244, 22);
            this.currentPortofolioMenuItem.Text = "Show Current Portofolio";
            this.currentPortofolioMenuItem.Click += new System.EventHandler(this.currentPortofolioMenuItem_Click);
            // 
            // nameMappingMenuItem
            // 
            this.nameMappingMenuItem.Name = "nameMappingMenuItem";
            this.nameMappingMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F2)));
            this.nameMappingMenuItem.Size = new System.Drawing.Size(244, 22);
            this.nameMappingMenuItem.Text = "Name Mappings";
            this.nameMappingMenuItem.Click += new System.EventHandler(this.nameMappingMenuItem_Click);
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1120, 554);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1120, 626);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.browseToolStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.drawToolStrip);
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
            this.statusStrip1.Size = new System.Drawing.Size(1120, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // statusLabel
            // 
            this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(1003, 17);
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
            this.indicatorLayoutPanel.Size = new System.Drawing.Size(1120, 554);
            this.indicatorLayoutPanel.TabIndex = 0;
            // 
            // themeToolStrip
            // 
            this.themeToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.themeToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indicatorConfigStripButton,
            this.saveThemeStripButton,
            this.defaultThemeStripButton,
            this.themeComboBox,
            this.deleteThemeStripButton,
            this.toolStripSeparator1,
            this.strategyComboBox,
            this.portfolioComboBox});
            this.themeToolStrip.Location = new System.Drawing.Point(3, 25);
            this.themeToolStrip.Name = "themeToolStrip";
            this.themeToolStrip.Size = new System.Drawing.Size(677, 25);
            this.themeToolStrip.TabIndex = 2;
            // 
            // indicatorConfigStripButton
            // 
            this.indicatorConfigStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.indicatorConfigStripButton.Image = global::StockAnalyzerApp.Properties.Resources.gear;
            this.indicatorConfigStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.indicatorConfigStripButton.Name = "indicatorConfigStripButton";
            this.indicatorConfigStripButton.Size = new System.Drawing.Size(23, 22);
            this.indicatorConfigStripButton.Text = "Configure displayed indicator1Name";
            this.indicatorConfigStripButton.Click += new System.EventHandler(this.selectDisplayedIndicatorMenuItem_Click);
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
            // strategyComboBox
            // 
            this.strategyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.strategyComboBox.Name = "strategyComboBox";
            this.strategyComboBox.Size = new System.Drawing.Size(220, 25);
            this.strategyComboBox.Sorted = true;
            this.strategyComboBox.SelectedIndexChanged += new System.EventHandler(this.strategyComboBox_SelectedIndexChanged);
            // 
            // portfolioComboBox
            // 
            this.portfolioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.portfolioComboBox.Name = "portfolioComboBox";
            this.portfolioComboBox.Size = new System.Drawing.Size(220, 25);
            this.portfolioComboBox.SelectedIndexChanged += new System.EventHandler(this.portfolioComboBox_SelectedIndexChanged);
            // 
            // browseToolStrip
            // 
            this.browseToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.browseToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stockNameComboBox,
            this.barDurationComboBox,
            this.smoothingLabel,
            this.barSmoothingComboBox,
            this.barHeikinAshiCheckBox,
            this.downloadBtn,
            this.searchText,
            this.goBtn,
            this.toolStripSeparator6,
            this.rewindBtn,
            this.fastForwardBtn,
            this.zoomOutBtn,
            this.zoomInBtn,
            this.logScaleBtn,
            this.toolStripSeparator2,
            this.followUpCheckBox,
            this.excludeButton,
            this.commentBtn,
            this.toolStripSeparator5});
            this.browseToolStrip.Location = new System.Drawing.Point(401, 0);
            this.browseToolStrip.Name = "browseToolStrip";
            this.browseToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.browseToolStrip.Size = new System.Drawing.Size(719, 25);
            this.browseToolStrip.TabIndex = 0;
            // 
            // stockNameComboBox
            // 
            this.stockNameComboBox.Name = "stockNameComboBox";
            this.stockNameComboBox.Size = new System.Drawing.Size(250, 25);
            this.stockNameComboBox.SelectedIndexChanged += new System.EventHandler(this.StockNameComboBox_SelectedIndexChanged);
            // 
            // barDurationComboBox
            // 
            this.barDurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.barDurationComboBox.Name = "barDurationComboBox";
            this.barDurationComboBox.Size = new System.Drawing.Size(125, 25);
            this.barDurationComboBox.SelectedIndexChanged += new System.EventHandler(this.BarDurationChanged);
            // 
            // smoothingLabel
            // 
            this.smoothingLabel.Name = "smoothingLabel";
            this.smoothingLabel.Size = new System.Drawing.Size(52, 22);
            this.smoothingLabel.Text = "Smooth:";
            // 
            // barSmoothingComboBox
            // 
            this.barSmoothingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.barSmoothingComboBox.Name = "barSmoothingComboBox";
            this.barSmoothingComboBox.Size = new System.Drawing.Size(75, 25);
            this.barSmoothingComboBox.SelectedIndexChanged += new System.EventHandler(this.BarDurationChanged);
            // 
            // barHeikinAshiCheckBox
            // 
            this.barHeikinAshiCheckBox.Name = "barHeikinAshiCheckBox";
            this.barHeikinAshiCheckBox.Size = new System.Drawing.Size(86, 22);
            this.barHeikinAshiCheckBox.Text = "Heikin Ashi";
            // 
            // downloadBtn
            // 
            this.downloadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.downloadBtn.Image = global::StockAnalyzerApp.Properties.Resources.Reload;
            this.downloadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(23, 22);
            this.downloadBtn.Text = "Download";
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // searchText
            // 
            this.searchText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(200, 23);
            this.searchText.TextChanged += new System.EventHandler(this.goBtn_Click);
            // 
            // goBtn
            // 
            this.goBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.goBtn.Image = global::StockAnalyzerApp.Properties.Resources.search;
            this.goBtn.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(23, 20);
            this.goBtn.Text = "Go to stock";
            this.goBtn.ToolTipText = "Go to stock";
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // rewindBtn
            // 
            this.rewindBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rewindBtn.Image = global::StockAnalyzerApp.Properties.Resources.Backward_icon;
            this.rewindBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rewindBtn.Name = "rewindBtn";
            this.rewindBtn.Size = new System.Drawing.Size(23, 20);
            this.rewindBtn.ToolTipText = "Rewind";
            this.rewindBtn.Click += new System.EventHandler(this.rewindBtn_Click);
            // 
            // fastForwardBtn
            // 
            this.fastForwardBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fastForwardBtn.Image = global::StockAnalyzerApp.Properties.Resources.Forward_icon;
            this.fastForwardBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fastForwardBtn.Name = "fastForwardBtn";
            this.fastForwardBtn.Size = new System.Drawing.Size(23, 20);
            this.fastForwardBtn.ToolTipText = "Forward";
            this.fastForwardBtn.Click += new System.EventHandler(this.fastForwardBtn_Click);
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
            this.logScaleBtn.ToolTipText = "Log scale";
            this.logScaleBtn.Click += new System.EventHandler(this.logScaleBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // followUpCheckBox
            // 
            this.followUpCheckBox.Name = "followUpCheckBox";
            this.followUpCheckBox.Size = new System.Drawing.Size(80, 19);
            this.followUpCheckBox.Text = "Follow-up";
            this.followUpCheckBox.Click += new System.EventHandler(this.followUpCheckBox_CheckedChanged);
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
            // commentBtn
            // 
            this.commentBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.commentBtn.Image = global::StockAnalyzerApp.Properties.Resources.Comment;
            this.commentBtn.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.commentBtn.Name = "commentBtn";
            this.commentBtn.Size = new System.Drawing.Size(23, 20);
            this.commentBtn.Text = "Comment";
            this.commentBtn.ToolTipText = "Edit a comment";
            this.commentBtn.Click += new System.EventHandler(this.commentBtn_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // drawToolStrip
            // 
            this.drawToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.drawToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.magnetStripBtn,
            this.sarLineStripBtn,
            this.drawLineStripBtn,
            this.addHalfLineStripBtn,
            this.addSegmentStripBtn,
            this.fanLineBtn,
            this.copyLineStripBtn,
            this.cutLineStripBtn,
            this.generateChannelStripButton,
            this.deleteLineStripBtn,
            this.drawingStyleStripBtn,
            this.toolStripSeparator13,
            this.saveAnalysisToolStripButton,
            this.snapshotToolStripButton,
            this.toolStripSeparator7,
            this.addToReportStripBtn,
            this.generateDailyReportToolStripBtn,
            this.AddToWatchListToolStripDropDownButton});
            this.drawToolStrip.Location = new System.Drawing.Point(3, 0);
            this.drawToolStrip.Name = "drawToolStrip";
            this.drawToolStrip.Size = new System.Drawing.Size(398, 25);
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
            // sarLineStripBtn
            // 
            this.sarLineStripBtn.CheckOnClick = true;
            this.sarLineStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.sarLineStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddSarex;
            this.sarLineStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sarLineStripBtn.Name = "sarLineStripBtn";
            this.sarLineStripBtn.Size = new System.Drawing.Size(23, 22);
            this.sarLineStripBtn.Text = "Draw SAR stop";
            this.sarLineStripBtn.Click += new System.EventHandler(this.sarLineStripBtn_Click);
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
            // fanLineBtn
            // 
            this.fanLineBtn.CheckOnClick = true;
            this.fanLineBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fanLineBtn.Image = global::StockAnalyzerApp.Properties.Resources.FanLines;
            this.fanLineBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fanLineBtn.Name = "fanLineBtn";
            this.fanLineBtn.Size = new System.Drawing.Size(23, 22);
            this.fanLineBtn.Text = "fanLineBtn";
            this.fanLineBtn.ToolTipText = "Draw multiple lines from the same point";
            this.fanLineBtn.Click += new System.EventHandler(this.fanLineBtn_Click);
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
            // generateChannelStripButton
            // 
            this.generateChannelStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.generateChannelStripButton.Image = global::StockAnalyzerApp.Properties.Resources.Wizard;
            this.generateChannelStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.generateChannelStripButton.Name = "generateChannelStripButton";
            this.generateChannelStripButton.Size = new System.Drawing.Size(23, 22);
            this.generateChannelStripButton.Text = "Generate auto-trend lines";
            this.generateChannelStripButton.Click += new System.EventHandler(this.generateChannelStripButton_Click);
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
            // addToReportStripBtn
            // 
            this.addToReportStripBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addToReportStripBtn.Image = global::StockAnalyzerApp.Properties.Resources.AddToReport;
            this.addToReportStripBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addToReportStripBtn.Name = "addToReportStripBtn";
            this.addToReportStripBtn.Size = new System.Drawing.Size(23, 22);
            this.addToReportStripBtn.Text = "Add to Report";
            this.addToReportStripBtn.Click += new System.EventHandler(this.addToReportStripBtn_Click);
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
            this.graphCloseControl.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphCloseControl.Comments = null;
            this.graphCloseControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphCloseControl.CurveList = null;
            this.graphCloseControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphCloseControl.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphCloseControl.DrawingPen = null;
            this.graphCloseControl.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphCloseControl.EndIndex = 0;
            this.graphCloseControl.GridColor = System.Drawing.Color.Empty;
            this.graphCloseControl.HideIndicators = false;
            this.graphCloseControl.horizontalLines = null;
            this.graphCloseControl.IsLogScale = false;
            this.graphCloseControl.Location = new System.Drawing.Point(0, 24);
            this.graphCloseControl.Magnetism = false;
            this.graphCloseControl.Name = "graphCloseControl";
            this.graphCloseControl.ScaleInvisible = true;
            this.graphCloseControl.SecondaryFloatSerie = null;
            this.graphCloseControl.SecondaryPen = null;
            this.graphCloseControl.ShowGrid = false;
            this.graphCloseControl.ShowVariation = false;
            this.graphCloseControl.Size = new System.Drawing.Size(1120, 626);
            this.graphCloseControl.StartIndex = 0;
            this.graphCloseControl.TabIndex = 0;
            this.graphCloseControl.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphCloseControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphScrollerControl
            // 
            this.graphScrollerControl.AutoSize = true;
            this.graphScrollerControl.BackgroundColor = System.Drawing.Color.White;
            this.graphScrollerControl.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphScrollerControl.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.graphScrollerControl.CurveList = null;
            this.graphScrollerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphScrollerControl.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphScrollerControl.DrawingPen = null;
            this.graphScrollerControl.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphScrollerControl.EndIndex = 0;
            this.graphScrollerControl.GridColor = System.Drawing.Color.Empty;
            this.graphScrollerControl.horizontalLines = null;
            this.graphScrollerControl.IsLogScale = false;
            this.graphScrollerControl.Location = new System.Drawing.Point(0, 24);
            this.graphScrollerControl.Name = "graphScrollerControl";
            this.graphScrollerControl.ScaleInvisible = false;
            this.graphScrollerControl.ShowGrid = false;
            this.graphScrollerControl.ShowVariation = false;
            this.graphScrollerControl.Size = new System.Drawing.Size(1120, 626);
            this.graphScrollerControl.StartIndex = 0;
            this.graphScrollerControl.TabIndex = 2;
            this.graphScrollerControl.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphScrollerControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator1Control
            // 
            this.graphIndicator1Control.AutoSize = true;
            this.graphIndicator1Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator1Control.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator1Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator1Control.CurveList = null;
            this.graphIndicator1Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator1Control.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator1Control.DrawingPen = null;
            this.graphIndicator1Control.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
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
            this.graphIndicator1Control.ShowVariation = false;
            this.graphIndicator1Control.Size = new System.Drawing.Size(1120, 626);
            this.graphIndicator1Control.StartIndex = 0;
            this.graphIndicator1Control.TabIndex = 2;
            this.graphIndicator1Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator1Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator2Control
            // 
            this.graphIndicator2Control.AutoSize = true;
            this.graphIndicator2Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator2Control.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator2Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator2Control.CurveList = null;
            this.graphIndicator2Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator2Control.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator2Control.DrawingPen = null;
            this.graphIndicator2Control.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
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
            this.graphIndicator2Control.ShowVariation = false;
            this.graphIndicator2Control.Size = new System.Drawing.Size(1120, 626);
            this.graphIndicator2Control.StartIndex = 0;
            this.graphIndicator2Control.TabIndex = 1;
            this.graphIndicator2Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator2Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphIndicator3Control
            // 
            this.graphIndicator3Control.AutoSize = true;
            this.graphIndicator3Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator3Control.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphIndicator3Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator3Control.CurveList = null;
            this.graphIndicator3Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator3Control.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphIndicator3Control.DrawingPen = null;
            this.graphIndicator3Control.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
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
            this.graphIndicator3Control.ShowVariation = false;
            this.graphIndicator3Control.Size = new System.Drawing.Size(1120, 626);
            this.graphIndicator3Control.StartIndex = 0;
            this.graphIndicator3Control.TabIndex = 1;
            this.graphIndicator3Control.TextBackgroundColor = System.Drawing.Color.Empty;
            this.graphIndicator3Control.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOverGraphControl);
            // 
            // graphVolumeControl
            // 
            this.graphVolumeControl.AutoSize = true;
            this.graphVolumeControl.BackgroundColor = System.Drawing.Color.White;
            this.graphVolumeControl.ChartMode =  CustomControl.GraphControls.GraphChartMode.Line;
            this.graphVolumeControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphVolumeControl.CurveList = null;
            this.graphVolumeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphVolumeControl.DrawingMode =  CustomControl.GraphControls.GraphDrawMode.Normal;
            this.graphVolumeControl.DrawingPen = null;
            this.graphVolumeControl.DrawingStep =  CustomControl.GraphControls.GraphDrawingStep.SelectItem;
            this.graphVolumeControl.EndIndex = 0;
            this.graphVolumeControl.GridColor = System.Drawing.Color.Empty;
            this.graphVolumeControl.horizontalLines = null;
            this.graphVolumeControl.IsLogScale = false;
            this.graphVolumeControl.Location = new System.Drawing.Point(0, 24);
            this.graphVolumeControl.Name = "graphVolumeControl";
            this.graphVolumeControl.ScaleInvisible = false;
            this.graphVolumeControl.ShowGrid = false;
            this.graphVolumeControl.ShowVariation = false;
            this.graphVolumeControl.Size = new System.Drawing.Size(1120, 626);
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
        private ToolStripComboBox barSmoothingComboBox;
        private ToolStripCheckedBox barHeikinAshiCheckBox;
        private ToolStripMenuItem fileMenuItem;
        private ToolStripMenuItem helpMenuItem;
        private ToolStripMenuItem optionsMenuItem;
        private ToolStripMenuItem configDataProviderMenuItem;
        private ToolStripButton sarLineStripBtn;
        private ToolStripButton drawLineStripBtn;
        private ToolStripButton fanLineBtn;
        private ToolStripButton copyLineStripBtn;
        private ToolStripButton deleteLineStripBtn;
        private ToolStripButton drawingStyleStripBtn;
        private ToolStripButton addSegmentStripBtn;
        private ToolStripButton addHalfLineStripBtn;
        private ToolStripButton saveAnalysisToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton excludeButton;
        private CustomControl.ToolStripCheckedBox followUpCheckBox;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem viewMenuItem;
        private ToolStripButton commentBtn;
        private ToolStripMenuItem portofolioMenuItem;
        private ToolStripButton magnetStripBtn;
        private ToolStripMenuItem palmaresMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private ToolStripMenuItem nameMappingMenuItem;
        private ToolStripMenuItem showOrdersMenuItem;
        private ToolStripMenuItem showDrawingsMenuItem;
        private ToolStripMenuItem showEventMarqueeMenuItem;
        private ToolStripMenuItem showCommentMarqueeMenuItem;
        private ToolStripMenuItem showIndicatorDivMenuItem;
        private ToolStripMenuItem showIndicatorTextMenuItem;
        private ToolStripMenuItem analysisMenuItem;
        private ToolStripMenuItem strategySimulationMenuItem;
        private ToolStripMenuItem filteredStrategySimulationMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem batchStrategySimulationMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton zoomInBtn;
        private ToolStripButton rewindBtn;
        private ToolStripButton logScaleBtn;
        private ToolStripButton fastForwardBtn;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem portofolioSimulationMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripMenuItem exportFinancialsMenuItem;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripMenuItem currentPortofolioMenuItem;
        private ToolStripButton generateChannelStripButton;
        private ToolStripMenuItem showHorseRaceViewMenuItem;
        private ToolStripMenuItem showAlertDialogMenuItem;
        private ToolStripMenuItem hideIndicatorsStockMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem indexRelativeStrengthMenuItem;
        private ToolStripMenuItem logSerieMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripButton addToReportStripBtn;
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
        private ToolStripMenuItem generateSeasonalitySerieMenuItem;
        private ToolStripMenuItem patternRecognitionMenuItem;
        private ToolStripMenuItem inverseSerieMenuItem;
        private ToolStripButton snapshotToolStripButton;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripComboBox themeComboBox;
        private ToolStripComboBox strategyComboBox;
        private ToolStripComboBox portfolioComboBox;
        private ToolStripMenuItem showAgendaMenuItem;
        private ToolStrip themeToolStrip;
        private ToolStripButton defaultThemeStripButton;
        private ToolStripButton deleteThemeStripButton;
        private ToolStripButton saveThemeStripButton;
        private ToolStripButton indicatorConfigStripButton;
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
        private ToolStripMenuItem eraseAllDrawingsToolStripMenuItem;
        private ToolStripMenuItem stockScannerMenuItem;
        private ToolStripMenuItem stockStrategyScannerMenuItem;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem saveThemeMenuItem;
        private ToolStripMenuItem newAnalysisMenuItem;
        private ToolStripTextBox searchText;
        private ToolStripLabel smoothingLabel;
        private ToolStripButton goBtn;
    }
}