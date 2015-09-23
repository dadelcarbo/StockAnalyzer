using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.WatchlistDlgs
{
    public partial class WatchListDlg : Form
    {
        private List<StockWatchList> watchLists;

        public event StockAnalyzerForm.StockWatchListsChangedEventHandler StockWatchListsChanged;
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public WatchListDlg(List<StockWatchList> wl)
        {
            InitializeComponent();
            watchLists = wl;
            this.stockWatchListsBindingSource.DataSource = watchLists;
            this.stockWatchListBindingSource.DataSource = watchLists.First().StockList;
        }
        private void watchListComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.watchListComboBox.SelectedItem == null)
            {
                if (watchLists.Count != 0)
                {
                    this.stockWatchListBindingSource.DataSource = watchLists.First().StockList;
                }
                else
                {
                    this.stockWatchListBindingSource.DataSource = null;
                }
            }
            else{
                this.stockWatchListBindingSource.DataSource =
                watchLists.First(wl => wl == this.watchListComboBox.SelectedItem).StockList;
            }
        }
        private void deleteWatchlistBtn_Click(object sender, EventArgs e)
        {
            if (this.watchListComboBox.SelectedItem != null)
            {
                watchLists.Remove((StockWatchList)this.watchListComboBox.SelectedItem);
                this.stockWatchListsBindingSource.DataSource = null;
                this.stockWatchListsBindingSource.DataSource = watchLists;

                if (this.StockWatchListsChanged != null)
                {
                    this.StockWatchListsChanged();
                }
            }
        }
        private void addWatchlistBtn_Click(object sender, EventArgs e)
        {
            if (this.watchListComboBox.Text == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Empty watch list Name","Error");
                return;
            }
            if (watchLists.Find(wl => wl.Name == this.watchListComboBox.Text)!=null)
            {
                System.Windows.Forms.MessageBox.Show("Watchlist " + this.watchListComboBox.Text + " already exists !!!", "Error");
                return;
            }
            StockWatchList newWatchList = new StockWatchList(this.watchListComboBox.Text);
            watchLists.Add(newWatchList);
            this.stockWatchListsBindingSource.DataSource = null;
            this.stockWatchListsBindingSource.DataSource = watchLists;

            if (this.StockWatchListsChanged != null)
            {
                this.StockWatchListsChanged();
            }

            this.watchListComboBox.SelectedItem = newWatchList;
        }
        void stockListBox_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedStockChanged != null && this.stockListBox.SelectedItem!=null)
            {
                this.SelectedStockChanged(this.stockListBox.SelectedItem.ToString(), false);
            }
        }

        private void deleteStockbtn_Click(object sender, EventArgs e)
        {
            StockWatchList watchList = watchLists.Find(wl => wl.Name == this.watchListComboBox.Text);
            if (watchList == null)
            {
                System.Windows.Forms.MessageBox.Show("No watchlist selected", "Error");
                return;
            }
            foreach(string stock in this.stockListBox.SelectedItems)
            {
                watchList.StockList.Remove(stock);
            }
            this.stockWatchListBindingSource.DataSource = null;
            this.stockWatchListBindingSource.DataSource = watchList.StockList;
        }
        private void addStockbtn_Click(object sender, EventArgs e)
        {
            StockScannerDlg stockSelectorDlg = new StockScannerDlg(StockAnalyzerForm.MainFrame.StockDictionary, StockSerie.Groups.ALL, StockSerie.StockBarDuration.Daily,  StockAnalyzerForm.MainFrame.GetCurrentTheme());
            System.Windows.Forms.DialogResult res = stockSelectorDlg.ShowDialog();

            StockWatchList watchList = watchLists.Find(wl => wl.Name == this.watchListComboBox.Text);
            watchList.StockList.AddRange(stockSelectorDlg.SelectedStocks);

            this.stockWatchListBindingSource.DataSource = null;
            this.stockWatchListBindingSource.DataSource = watchLists.First(wl => wl == this.watchListComboBox.SelectedItem).StockList;

        }
    }
}
