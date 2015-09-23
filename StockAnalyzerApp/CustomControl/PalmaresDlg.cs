using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using System.IO;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl
{
    public partial class PalmaresDlg : Form
    {
        public StockDictionary StockDico {get;set;}
        public List<StockWatchList> WatchLists  {get;set;}

        private ListViewColumnSorter lvwColumnSorter;
        private ToolStripProgressBar progressBar;


        public event StockAnalyzerForm.SelectedStockGroupChangedEventHandler SelectStockGroupChanged;
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedPortofolioNameChangedEventHandler SelectedPortofolioChanged;
        public event StockAnalyzerForm.StockWatchListsChangedEventHandler StockWatchListsChanged;

        public DateTime StartDate {get{return this.fromDateTimePicker.Value;}set{this.fromDateTimePicker.Value=value;}}
        public DateTime EndDate {get{return this.untilDateTimePicker.Value;}set{this.untilDateTimePicker.Value=value;}}
        public bool DisplayPortofolio { get { return this.portofolioCheckBox.Checked; } set { this.portofolioCheckBox.Checked = value; } }

        private DateTime previousFromDate;
        private DateTime previousUntilDate;

        private StockSerie.StockBarDuration barDuration;

        public PalmaresDlg(StockDictionary stockDico, List<StockWatchList> watchLists, StockSerie.Groups selectedGroup, ToolStripProgressBar progressBar, StockSerie.StockBarDuration duration)
        {
            InitializeComponent();

            this.barDuration = barDuration;

            // Initialize dico
            StockDico = stockDico;
            WatchLists = watchLists;

            this.progressBar = progressBar;
            
            this.groupComboBox.Items.Clear();
            this.groupComboBox.Items.AddRange(stockDico.GetValidGroupNames().ToArray());
            this.groupComboBox.SelectedItem = selectedGroup.ToString();

            // Create an instance of a ListView column sorter and assign it to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            this.palmaresView.ListViewItemSorter = (IComparer)lvwColumnSorter;

            // Select default values           
            this.untilCheckBox.Checked = false;
            this.untilDateTimePicker.Enabled = this.untilCheckBox.Checked;
            
            var stockList = stockDico.Values.Where(s => s.BelongsToGroup(selectedGroup));
            if (stockList.Count() > 0 && stockList.ElementAt(0).Initialise())
            { 
                this.fromDateTimePicker.MaxDate = stockList.ElementAt(0).Keys.Last();
                this.fromDateTimePicker.Value = stockList.ElementAt(0).Keys.ElementAt(stockList.ElementAt(0).Keys.Count-2);
                this.untilDateTimePicker.Value = stockList.ElementAt(0).Keys.Last();
            }
            else
            {
                this.fromDateTimePicker.Value = System.DateTime.Today.AddYears(-1);
                this.fromDateTimePicker.MaxDate = System.DateTime.Today.AddDays(-1);
                this.untilDateTimePicker.Value = System.DateTime.Today;
            }
            previousFromDate = fromDateTimePicker.Value;
            previousUntilDate = untilDateTimePicker.Value;

            // 
            InitializeListView();

            // Activate handlers
            this.fromDateTimePicker.CloseUp += new System.EventHandler(this.fromDateTimePicker_ValueChanged);
            this.fromDateTimePicker.LostFocus += new System.EventHandler(this.fromDateTimePicker_ValueChanged);
            this.untilDateTimePicker.CloseUp += new System.EventHandler(this.untilDateTimePicker_ValueChanged);
            this.untilDateTimePicker.LostFocus += new System.EventHandler(this.untilDateTimePicker_ValueChanged);
            this.groupComboBox.SelectedIndexChanged += new System.EventHandler(this.groupComboBox_SelectedIndexChanged);

            this.TopLevel = true;
        }
        public void OnBarDurationChanged(StockSerie.StockBarDuration barDuration)
        {
            this.barDuration = barDuration;
            this.InitializeListView();
        }
        public void InitializeListView()
        {
            if (this.IsDisposed) return;
            Cursor previousCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            StockSplashScreen.ShowSplashScreen();

            // Clear existing view
            this.palmaresView.Items.Clear();

            //Initialise ListView
            ListViewItem[] viewItems = new ListViewItem[StockDico.Values.Count];
            string[] subItems = new string[this.palmaresView.Columns.Count];
            StockDailyValue firstStockValue = null;
            StockDailyValue lastStockValue = null;
            int startIndex, endIndex;
            int i = 0;

            // Initialise progress bar
            var stockSeries = this.StockDico.Values.Where(stockSerie => stockSerie.BelongsToGroup(this.groupComboBox.SelectedItem.ToString()));
            this.progressBar.Maximum = stockSeries.Count();
            this.progressBar.Minimum = 0;
            this.progressBar.Value = 0;

            StockSerie underlyingStockSerie = null;
            foreach (StockSerie stockSerie in stockSeries)
            {
                StockSplashScreen.ProgressText = stockSerie.StockName;
                underlyingStockSerie = null;
                if (this.portofolioCheckBox.Checked)
                {
                    string stockName = stockSerie.StockName;
                    if (stockName.EndsWith("_P"))
                    {
                        underlyingStockSerie = StockDico[stockName.Substring(0, stockName.Length - 2)];
                    }
                }   
                if (!stockSerie.IsPortofolioSerie && portofolioCheckBox.Checked)
                {
                    continue;
                }
                if (stockSerie.IsPortofolioSerie && !portofolioCheckBox.Checked)
                {
                    continue;
                }

                if (!stockSerie.StockAnalysis.Excluded)
                {
                    if (stockSerie.Initialise())
                    {
                        stockSerie.BarDuration = barDuration;
                        if (stockSerie.Values.Last().DATE < this.fromDateTimePicker.Value)
                        {
                            continue;
                        }
                        this.progressBar.Value++;

                        startIndex = stockSerie.IndexOfFirstGreaterOrEquals(this.fromDateTimePicker.Value);
                        //int days = 1;
                        //while (startIndex == -1 && this.fromDateTimePicker.Value.AddDays(days) <= this.untilDateTimePicker.Value)
                        //{
                        //    startIndex = stockSerie.IndexOf(this.fromDateTimePicker.Value.AddDays(days++));
                        //}
                        if (startIndex == -1)
                        {
                            continue;
                        }
                        firstStockValue = stockSerie.ValueArray[startIndex];
                        if (this.untilCheckBox.Checked)
                        {
                            endIndex = stockSerie.IndexOfFirstGreaterOrEquals(this.untilDateTimePicker.Value);
                            if (endIndex == -1)
                            {
                                endIndex = stockSerie.Count - 1;
                            }
                        }
                        else
                        {
                            endIndex = stockSerie.Count - 1;
                        }
                        if (endIndex == -1)
                        {
                            continue;
                        }

                        lastStockValue = stockSerie.ValueArray[endIndex];

                        int k = 0;
                        subItems[k++] = firstStockValue.NAME;
                        subItems[k++] = ((lastStockValue.CLOSE - firstStockValue.CLOSE) / firstStockValue.CLOSE).ToString("P2");
                        subItems[k++] = firstStockValue.OPEN.ToString();
                        subItems[k++] = stockSerie.GetMax(startIndex, endIndex, StockDataType.HIGH).ToString();
                        subItems[k++] = stockSerie.GetMin(startIndex, endIndex, StockDataType.LOW).ToString();
                        subItems[k++] = lastStockValue.CLOSE.ToString();
                        subItems[k++] = stockSerie.GetIndicator(Settings.Default.MomentumIndicator).Series[0][endIndex].ToString();
                        viewItems[i++] = new ListViewItem(subItems);
                    }
                }
            }

            ListViewItem[] viewItemsSubArray = new ListViewItem[i];
            System.Array.Copy(viewItems, viewItemsSubArray, i);
            this.palmaresView.Items.AddRange(viewItemsSubArray);

            this.progressBar.Value = 0;
            this.Refresh();

            StockSplashScreen.CloseForm(true);
            this.Cursor = previousCursor;
        }
        private void palmaresView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.palmaresView.Sort();
        }
        private void PalmaresDlg_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedStockChanged != null)
            {
                if (this.portofolioCheckBox.Checked)
                {
                    string stockPortofolioName = this.palmaresView.SelectedItems[0].Text;
                    if (SelectedPortofolioChanged != null)
                    {
                        SelectedPortofolioChanged(stockPortofolioName, false);
                    }
                    SelectedStockChanged(stockPortofolioName.Substring(0, stockPortofolioName.Length - 2), true);
                }
                else
                {
                    SelectedStockChanged(this.palmaresView.SelectedItems[0].Text, true);
                }
            }
        }
        private void fromDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (fromDateTimePicker.Value != previousFromDate)
            {
                InitializeListView();
                previousFromDate = fromDateTimePicker.Value;
            }
        }
        private void untilDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (untilDateTimePicker.Value != previousUntilDate)
            {
                InitializeListView();
                previousUntilDate = untilDateTimePicker.Value;
            }
        }
        private void groupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectStockGroupChanged != null)
            {
                this.SelectStockGroupChanged(groupComboBox.SelectedItem.ToString());
            }
            InitializeListView();
        }
        private void untilCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.untilDateTimePicker.Enabled = this.untilCheckBox.Checked;
            
            var stockList = this.StockDico.Values.Where(s => s.BelongsToGroup(this.groupComboBox.SelectedItem.ToString()));
            if (stockList.Count() > 0 && stockList.ElementAt(0).Initialise())
            {
                this.untilDateTimePicker.Value = stockList.ElementAt(0).Keys.Last();
            }
            InitializeListView();
        }
        private void portofolioCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            InitializeListView();
        }
        private void addToWinnerWatchListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateWatchList("WINNER");
        }
        private void addToLoserWatchListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateWatchList("LOSER");
        }
        private void CreateWatchList(string postfix)
        {
            if (this.palmaresView.SelectedItems.Count != 0)
            {
                int weekOfTheYear = (this.EndDate.DayOfYear + 4) / 7;

                string watchListName = this.groupComboBox.SelectedItem.ToString() + "_" + postfix + "_" + weekOfTheYear.ToString("00");
                
                StockWatchList swl = WatchLists.Find(wl=>wl.Name == watchListName);
                if (swl == null)
                {
                    swl = new StockWatchList(watchListName);
                    WatchLists.Add(swl);
                }

                foreach (ListViewItem selectedItem in this.palmaresView.SelectedItems)
                {
                    if (!swl.StockList.Contains(selectedItem.ToString()))
                    {
                    swl.StockList.Add(selectedItem.Text);
                    }
                }

                if (this.StockWatchListsChanged!=null)
                {
                    this.StockWatchListsChanged();
                }
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (palmaresView.Items.Count != 0)
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.DefaultExt = "csv";
                fileDialog.Filter = "CSV files (*.csv)|*.csv";
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                fileDialog.FileName = "palamares_" + this.groupComboBox.SelectedItem.ToString() + ".csv";
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    StreamWriter textWriter;
                    try
                    {
                        // Save analysis file
                        using (FileStream fs = new FileStream(fileDialog.FileName, FileMode.Create))
                        {
                            textWriter = new StreamWriter(fs);
                            textWriter.AutoFlush = true;

                            // Write file header
                            textWriter.WriteLine("StartDate,EndDate,Group");
                            textWriter.WriteLine(this.StartDate.ToShortDateString() + "," + this.EndDate.ToShortDateString() + "," + this.groupComboBox.SelectedItem.ToString());
                            textWriter.WriteLine();
                            foreach (ColumnHeader columHeader in this.palmaresView.Columns)
                            {
                                textWriter.Write(columHeader.Text +",");
                            }
                            textWriter.WriteLine();

                            // Write each elements
                            foreach (ListViewItem viewItem in this.palmaresView.Items)
                            {
                                textWriter.WriteLine(viewItem.Text.Replace(',', '.') + "," + 
                                    viewItem.SubItems[1].Text.Replace(',', '.').Replace(" ", "") + "," + 
                                    viewItem.SubItems[2].Text.Replace(',', '.').Replace(" ", "") + "," + 
                                    viewItem.SubItems[3].Text.Replace(',', '.').Replace(" ", "") + "," + 
                                    viewItem.SubItems[4].Text.Replace(',', '.').Replace(" ", "") + "," + 
                                    viewItem.SubItems[5].Text.Replace(',', '.').Replace(" ", "")
                                    );
                            }
                        }
                    }
                    catch (System.Exception exception)
                    {
                        System.Windows.Forms.MessageBox.Show(exception.Message);
                        System.Windows.Forms.MessageBox.Show(exception.StackTrace);
                    }
                }
            }
        }
    }
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult = 0;
            if (this.OrderOfSort == SortOrder.None)
            {
                return compareResult;
            }

            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            switch (ColumnToSort)
            {
                case 0: // Name column
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                    break;
                case 1: // Variation column
                    float a = float.Parse(listviewX.SubItems[ColumnToSort].Text.Replace(" ", "").Replace("%", ""));
                    float b = float.Parse(listviewY.SubItems[ColumnToSort].Text.Replace(" ", "").Replace("%", ""));
                    compareResult = ObjectCompare.Compare(a, b);
                    break;
                case 2: // Open column
                case 3: // High column
                case 4: // Low column
                case 5: // Close column
                case 6: // Custom indcator
                    a = float.Parse(listviewX.SubItems[ColumnToSort].Text.Replace(" ", "").Replace("%", ""));
                    b = float.Parse(listviewY.SubItems[ColumnToSort].Text.Replace(" ", "").Replace("%", ""));
                    compareResult = ObjectCompare.Compare(a, b);
                    break;
                default:
                    compareResult = 0;
                    break;
            }

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else 
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }
    }
}
