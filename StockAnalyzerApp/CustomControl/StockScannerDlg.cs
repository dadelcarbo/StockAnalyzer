using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class StockScannerDlg : Form
    {
        private readonly StockDictionary stockDictionary;
        private StockBarDuration barDuration;

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockGroupChangedEventHandler SelectStockGroupChanged;

        public IEnumerable<string> SelectedStocks => this.selectedStockListBox.Items.Cast<string>();

        public StockScannerDlg(StockDictionary stockDictionary, StockSerie.Groups stockGroup, StockBarDuration barDuration, Dictionary<string, List<string>> theme)
        {
            InitializeComponent();

            this.stockDictionary = stockDictionary;
            this.barDuration = barDuration;

            // Initialise group combo box
            groupComboBox.Items.AddRange(this.stockDictionary.GetValidGroupNames().ToArray());
            groupComboBox.SelectedItem = stockGroup.ToString();
            groupComboBox.SelectedValueChanged += new EventHandler(groupComboBox_SelectedValueChanged);

            periodComboBox.SelectedIndex = 0;
            completeBarCheckBox.Checked = true;

            OnThemeChanged(theme);
            OnBarDurationChanged(barDuration);

            oneRadioButton.Checked = true;
        }
        public void OnThemeChanged(Dictionary<string, List<string>> theme)
        {
            if (this.IsDisposed) return;
            // Initialise treeview
            eventTreeView.Nodes.Clear();
            selectedStockListBox.Items.Clear();

            foreach (string entry in theme.Keys)
            {
                try
                {
                    foreach (string line in theme[entry])
                    {
                        string[] fields = line.Split('|');
                        switch (fields[0].ToUpper())
                        {
                            case "PAINTBAR":
                            case "AUTODRAWING":
                            case "CLOUD":
                            case "TRAILSTOP":
                            case "INDICATOR":
                                {
                                    IStockViewableSeries viewableSerie = StockViewableItemsManager.GetViewableItem(line);
                                    IStockEvent stockEvent = viewableSerie as IStockEvent;
                                    if (stockEvent != null)
                                    {
                                        bool nodeAlreadyExists = false;
                                        foreach (TreeNode node in eventTreeView.Nodes)
                                        {
                                            if (node.Text == viewableSerie.Name)
                                            {
                                                nodeAlreadyExists = true;
                                                break;
                                            }
                                        }
                                        if (nodeAlreadyExists) break;

                                        if (stockEvent.EventCount != 0)
                                        {
                                            TreeNode treeNode = new TreeNode(fields[1]);
                                            eventTreeView.Nodes.Add(treeNode);
                                            for (int i = 0; i < stockEvent.EventCount; i++)
                                            {
                                                treeNode.Nodes.Add(new TreeNode(stockEvent.EventNames[i]) { Tag = stockEvent });
                                                eventTreeView.Refresh();
                                            }
                                        }
                                    }
                                }
                                break;
                            case "DECORATOR":
                                {
                                    IStockViewableSeries viewableSerie = StockViewableItemsManager.GetViewableItem(line);
                                    if (viewableSerie is IStockEvent)
                                    {
                                        // Search for parent node
                                        IStockDecorator decorator = viewableSerie as IStockDecorator;
                                        TreeNode parentNode = null;
                                        foreach (TreeNode node in eventTreeView.Nodes)
                                        {
                                            if (node.Text == decorator.DecoratedItem)
                                            {
                                                parentNode = node;
                                                break;
                                            }
                                        }
                                        if (parentNode == null)
                                        {
                                            break;
                                        }

                                        // Create sub nodes
                                        if (decorator.SeriesCount != 0)
                                        {
                                            TreeNode treeNode = new TreeNode(fields[1]);
                                            parentNode.Nodes.Add(treeNode);
                                            for (int i = 0; i < decorator.EventCount; i++)
                                            {
                                                treeNode.Nodes.Add(new TreeNode(decorator.EventNames[i]) { Tag = decorator });
                                                eventTreeView.Refresh();
                                            }
                                        }
                                    }
                                }
                                break;
                            case "TRAIL":
                                {
                                    IStockViewableSeries viewableSerie = StockViewableItemsManager.GetViewableItem(line);
                                    if (viewableSerie is IStockEvent)
                                    {
                                        // Search for parent node
                                        IStockTrail trail = viewableSerie as IStockTrail;
                                        TreeNode parentNode = null;
                                        foreach (TreeNode node in eventTreeView.Nodes)
                                        {
                                            if (node.Text == trail.TrailedItem)
                                            {
                                                parentNode = node;
                                                break;
                                            }
                                        }
                                        if (parentNode == null)
                                        {
                                            break;
                                        }

                                        // Create sub nodes
                                        if (trail.SeriesCount != 0)
                                        {
                                            TreeNode treeNode = new TreeNode(fields[1]);
                                            parentNode.Nodes.Add(treeNode);

                                            for (int i = 0; i < trail.EventCount; i++)
                                            {
                                                treeNode.Nodes.Add(new TreeNode(trail.EventNames[i]) { Tag = trail });
                                                eventTreeView.Refresh();
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                continue;
                        }
                    }
                }
                catch { }
            }

            eventTreeView.ExpandAll();
        }
        public void OnBarDurationChanged(StockBarDuration barDuration)
        {
            this.barDuration = barDuration;
        }

        void groupComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (groupComboBox.SelectedItem.ToString() == "INTRADAY")
            {
                this.refreshDataCheckBox.CheckState = CheckState.Checked;
            }
            else
            {
                this.refreshDataCheckBox.CheckState = CheckState.Unchecked;
            }
            if (SelectStockGroupChanged != null)
            {
                SelectStockGroupChanged(groupComboBox.SelectedItem.ToString());
            }
        }
        /// <summary>
        /// Returns a value indicating whether the specified TreeNode has checked child nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool HasCheckedChildNodes(TreeNode node)
        {
            if (node.Nodes.Count == 0) return false;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked) return true;
                // Recursively check the children of the current child node.
                if (HasCheckedChildNodes(childNode)) return true;
            }
            return false;
        } /// <summary>
          /// Returns a value indicating whether the specified TreeNode has checked child nodes.
          /// </summary>
          /// <param name="node"></param>
          /// <returns></returns>
        private List<TreeNode> GetCheckedChildNodes(TreeNode node)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            if (node.Nodes.Count == 0) return nodes;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked && childNode.Tag != null) nodes.Add(childNode);
                // Recursively check the children of the current child node.
                nodes.AddRange(GetCheckedChildNodes(childNode));
            }
            return nodes;
        }

        enum ProgressStatus
        {
            NeedDownload,
            Downloading,
            Downloaded,
            Initilised,
            Calculated
        };
        private readonly Dictionary<StockSerie, ProgressStatus> progress = new Dictionary<StockSerie, ProgressStatus>();

        private System.Windows.Forms.Timer downloadTimer;
        private System.Windows.Forms.Timer processTimer;

        Cursor cursor;
        private void selectButton_Click(object sender, EventArgs e)
        {
            startDate = DateTime.Now;
            //    ForceCheckNodes();
            //    DumpCheckNodes();
            eventTreeView.Refresh();

            var stockInGroupList = stockDictionary.Values.Where(s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString()) && !s.IsPortfolioSerie);

            selectedStockListBox.Items.Clear();
            selectedStockListBox.Refresh();

            if (progressBar != null)
            {
                progressBar.Minimum = 0;
                progressBar.Maximum = stockInGroupList.Count();
                progressBar.Value = 0;
            }
            allCriteria = allRadioButton.Checked;

            // Build EventMatchList
            eventMatches = new List<StockSerie.EventMatch>();

            foreach (TreeNode treeNode in eventTreeView.Nodes)
            {
                List<TreeNode> checkedNodes = GetCheckedChildNodes(treeNode);

                if (checkedNodes.Count == 0)
                {
                    continue;
                }

                foreach (TreeNode childNode in checkedNodes)
                {
                    IStockEvent eventSerie = (IStockEvent)childNode.Tag;
                    eventMatches.Add(new StockSerie.EventMatch()
                    {
                        ViewableSerie = (IStockViewableSeries)eventSerie,
                        EventIndex = eventSerie.EventNames.ToList().IndexOf(childNode.Text)
                    });
                }
            }

            if (eventMatches.Count == 0) return;

            cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            this.Enabled = false;
            progress.Clear();
            if (this.refreshDataCheckBox.Checked)
            {
                foreach (StockSerie stockSerie in stockInGroupList)
                {
                    stockSerie.IsInitialised = false;
                    progress.Add(stockSerie, ProgressStatus.NeedDownload);
                }

                // Refreshes intraday every 2 minutes.
                downloadTimer = new System.Windows.Forms.Timer();
                downloadTimer.Tick += new EventHandler(this.DownloadSeries);
                downloadTimer.Interval = 100;
                downloadTimer.Start();
            }
            else
            {
                foreach (StockSerie stockSerie in stockInGroupList)
                {
                    progress.Add(stockSerie, ProgressStatus.Downloaded);
                }
            }

            processTimer = new System.Windows.Forms.Timer();
            processTimer.Tick += new EventHandler(this.ProcessScan);
            processTimer.Interval = 10;
            processTimer.Start();
        }

        private bool allCriteria;
        private List<StockSerie.EventMatch> eventMatches;

        private void ProcessScan(object sender, EventArgs e)
        {
            try
            {
                int calculatedCount = 0;
                lock (progress)
                {
                    calculatedCount = progress.Count(p => p.Value == ProgressStatus.Calculated);
                }
                if (calculatedCount < progress.Count)
                {
                    StockSerie stockSerie;
                    lock (progress)
                    {
                        stockSerie = progress.FirstOrDefault(p => p.Value == ProgressStatus.Downloaded).Key;


                        if (stockSerie == null)
                        {
                            StockLog.Write("Nothing to process");
                            Thread.Sleep(100);
                            return;
                        }

                        StockLog.Write("Processing " + stockSerie.StockName);
                        progress[stockSerie] = ProgressStatus.Calculated;
                    }
                    progressLabel.Text = stockSerie.StockName;

                    if (!stockSerie.Initialise() || stockSerie.Count < 100)
                    {
                        return;
                    }

                    stockSerie.BarDuration = barDuration;
                    int lastIndex = completeBarCheckBox.Checked ? stockSerie.LastCompleteIndex : stockSerie.LastIndex;

                    int firstIndex = lastIndex + 1 - (int)periodComboBox.SelectedItem;

                    // Check event matching
                    bool selected = false;
                    for (int i = lastIndex; i >= firstIndex && !selected; i--)
                    {
                        selected |= allCriteria
                           ? stockSerie.MatchEventsAnd(i, eventMatches)
                           : stockSerie.MatchEventsOr(i, eventMatches);
                    }
                    if (selected)
                    {
                        selectedStockListBox.Items.Add(stockSerie.StockName);
                    }
                    if (progressBar != null)
                    {
                        progressBar.Value++;
                    }
                }
                else
                {
                    if (progressBar != null)
                    {
                        progressBar.Value = 0;
                        progressLabel.Text = selectedStockListBox.Items.Count + "/" + progress.Count(s => s.Key.IsInitialised);
                    }
                    processTimer.Stop();
                    processTimer.Dispose();

                    this.Enabled = true;
                    Cursor = cursor;
                    this.Activate();

                    StockLog.Write((DateTime.Now - startDate).ToString());
                }
            }
            catch
            {
            }
        }

        public void DownloadSeries(object sender, EventArgs e)
        {
            try
            {
                int neededCount;
                lock (progress)
                {
                    neededCount = progress.Count(p => p.Value == ProgressStatus.NeedDownload);
                }
                if (neededCount > 0)
                {
                    int downloadingCount;
                    lock (progress)
                    {
                        downloadingCount = progress.Count(p => p.Value == ProgressStatus.Downloading);
                    }
                    if (downloadingCount < 6)
                    {
                        // Find next to download
                        StockSerie stockSerie;
                        lock (progress)
                        {
                            stockSerie = progress.FirstOrDefault(p => p.Value == ProgressStatus.NeedDownload).Key;
                        }
                        if (stockSerie != null)
                        {
                            // StockLog.Write("Downloading " + stockSerie.StockName);
                            lock (progress)
                            {
                                progress[stockSerie] = ProgressStatus.Downloading;
                            }
                            stockSerie.IsInitialised = false;

                            Thread thread = new Thread(DownloadSerie);
                            thread.Name = "Scanner Dnld " + stockSerie.Symbol;
                            thread.Start(stockSerie);
                        }
                    }
                    else
                    {
                        // StockLog.Write("Download queue is full ");
                    }
                }
                else
                {
                    downloadTimer.Stop();
                    downloadTimer.Dispose();
                }
            }
            catch { }
        }

        private void DownloadSerie(object data)
        {
            StockSerie stockSerie = data as StockSerie;
            try
            {
                StockLog.Write("StockName:" + stockSerie.StockName + "ThreadID:" + Thread.CurrentThread.ManagedThreadId);

                using (new StockSerieLocker(stockSerie))
                {
                    StockDataProviderBase.DownloadSerieData(stockSerie);
                }
                lock (progress)
                {
                    progress[stockSerie] = ProgressStatus.Downloaded;
                }
            }
            catch
            {
                lock (progress)
                {
                    // Set as calculated in case of error
                    progress[stockSerie] = ProgressStatus.Calculated;
                }
            }
            // StockLog.Write(stockSerie.StockName +" Downloaded");
        }

        DateTime startDate = DateTime.Now;
        private void selectButton_Click2(object sender, EventArgs e)
        {
            startDate = DateTime.Now;
            //    ForceCheckNodes();
            //    DumpCheckNodes();
            eventTreeView.Refresh();

            Cursor cursor = Cursor;
            Cursor = Cursors.WaitCursor;

            var stockInGroupList = stockDictionary.Values.Where(s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString()) && !s.IsPortfolioSerie);
            try
            {
                selectedStockListBox.Items.Clear();
                selectedStockListBox.Refresh();

                if (progressBar != null)
                {
                    progressBar.Minimum = 0;
                    progressBar.Maximum = stockInGroupList.Count();
                    progressBar.Value = 0;
                }
                bool allCriteria = allRadioButton.Checked;

                // Build EventMatchList
                List<StockSerie.EventMatch> eventMatches = new List<StockSerie.EventMatch>();

                foreach (TreeNode treeNode in eventTreeView.Nodes)
                {
                    List<TreeNode> checkedNodes = GetCheckedChildNodes(treeNode);

                    if (checkedNodes.Count == 0)
                    {
                        continue;
                    }

                    foreach (TreeNode childNode in checkedNodes)
                    {
                        IStockEvent eventSerie = (IStockEvent)childNode.Tag;
                        eventMatches.Add(new StockSerie.EventMatch()
                        {
                            ViewableSerie = (IStockViewableSeries)eventSerie,
                            EventIndex = eventSerie.EventNames.ToList().IndexOf(childNode.Text)
                        });
                    }
                }

                if (eventMatches.Count == 0) return;
                foreach (StockSerie stockSerie in stockInGroupList)
                {
                    progressLabel.Text = stockSerie.StockName;
                    progressLabel.Refresh();


                    using (new StockSerieLocker(stockSerie))
                    {
                        if (this.refreshDataCheckBox.Checked)
                        {
                            stockSerie.IsInitialised = false;
                            StockDataProviderBase.DownloadSerieData(stockSerie);
                        }

                        if (!stockSerie.Initialise())
                        {
                            continue;
                        }

                        stockSerie.BarDuration = barDuration;
                        int lastIndex = completeBarCheckBox.Checked ? stockSerie.LastCompleteIndex : stockSerie.LastIndex;
                        int firstIndex = lastIndex + 1 - (int)periodComboBox.SelectedItem;

                        // Check event matching
                        bool selected =
                           false;
                        for (int i = lastIndex; i >= firstIndex && !selected; i--)
                        {
                            selected |= allCriteria
                               ? stockSerie.MatchEventsAnd(i, eventMatches)
                               : stockSerie.MatchEventsOr(i, eventMatches);
                        }
                        if (selected)
                        {
                            selectedStockListBox.Items.Add(stockSerie.StockName);
                            selectedStockListBox.Refresh();
                        }
                        if (progressBar != null)
                        {
                            progressBar.Value++;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Script Error !!!");
            }
            finally
            {
                if (progressBar != null)
                {
                    progressBar.Value = 0;
                    progressLabel.Text = selectedStockListBox.Items.Count + "/" + stockInGroupList.Count();
                }
                Cursor = cursor;
            }
            StockLog.Write((DateTime.Now - startDate).ToString());
        }

        void clearButton_Click(object sender, EventArgs e)
        {
            selectedStockListBox.Items.Clear();

            //ForceCheckNodes();
            eventTreeView.Refresh();
        }

        void selectedStockListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (SelectedStockChanged != null && selectedStockListBox.SelectedItem != null)
            {
                SelectedStockChanged(selectedStockListBox.SelectedItem.ToString(), true);
            }
            Focus();
        }

        private void eventTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                CheckChildNodes(e.Node);
                eventTreeView.Refresh();
            }
        }
        void DumpCheckNodes()
        {
            // StockLog.Write();
            foreach (var node in GetCheckedChildNodes(eventTreeView.TopNode))
            {
                // StockLog.Write(node.FullPath);
            }
        }

        //private void ForceCheckNodes()
        //{
        //    foreach (TreeNode childNode in eventTreeView.Nodes)
        //    {
        //        ForceCheckNodes(childNode);
        //    }
        //}
        //private void ForceCheckNodes(TreeNode node)
        //{
        //    bool isChecked = node.Checked;
        //    node.Checked = !isChecked;
        //    node.Checked = isChecked;

        //    eventTreeView.Refresh();
        //    if (node.Nodes == null) return;
        //    foreach (TreeNode childNode in node.Nodes)
        //    {
        //        ForceCheckNodes(childNode);
        //    }
        //}

        private void CheckChildNodes(TreeNode node)
        {
            if (node.Nodes == null) return;
            foreach (TreeNode childNode in node.Nodes)
            {
                childNode.Checked = node.Checked;
                eventTreeView.Refresh();
                CheckChildNodes(childNode);
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                StockSplashScreen.FadeInOutSpeed = 0.25;
                StockSplashScreen.ProgressVal = 0;
                StockSplashScreen.ProgressMax = 100;
                StockSplashScreen.ProgressMin = 0;
                StockSplashScreen.ShowSplashScreen();

                var stockInGroupList = stockDictionary.Values.Where(s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString()) && !s.IsPortfolioSerie);
                foreach (StockSerie stockSerie in stockInGroupList)
                {
                    stockSerie.IsInitialised = false;
                    StockSplashScreen.ProgressText = "Downloading " + stockSerie.StockGroup + " - " + stockSerie.StockName;
                    StockDataProviderBase.DownloadSerieData(stockSerie);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to download selected stock data...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StockSplashScreen.CloseForm(true);
            }
        }
    }
}
