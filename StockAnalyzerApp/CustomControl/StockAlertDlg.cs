using System;
using System.Collections;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class StockAlertDlg : Form
    {
        public StockAlertDlg()
        {
            InitializeComponent();
        }

        public void AddAlert(string stockName, string group, string alertName)
        {
            string[] items = new string[] { stockName, group, alertName };
            this.alertListView.Items.Add(new ListViewItem(items));
        }

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        void alertListView_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            SelectedStockChanged(this.alertListView.SelectedItems[0].Text, true);
        }

        void alertListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            // Set the ListViewItemSorter property to a new ListViewItemComparer
            // object.
            this.alertListView.ListViewItemSorter = new ListViewItemComparer(e.Column);
            // Call the sort method to manually sort.
            this.alertListView.Sort();
        }

        // Implements the manual sorting of items by column.
        class ListViewItemComparer : IComparer
        {
            private readonly int col;
            public ListViewItemComparer()
            {
                col = 0;
            }
            public ListViewItemComparer(int column)
            {
                col = column;
            }
            public int Compare(object x, object y)
            {
                int returnVal = -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text);
                return returnVal;
            }
        }
    }
}
