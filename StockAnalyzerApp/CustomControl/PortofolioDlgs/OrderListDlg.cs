using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using System;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
   public delegate void StockOrderDeletedEventHandler(StockOrder stockOrder);

   public partial class OrderListDlg : Form
   {
      private StockOrderList stockOrderList;
      private StockDictionary stockDictionary;
      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
      public event StockOrderDeletedEventHandler StockOrderDeleted;

      public void SetOrderList(StockOrderList stockOrderList)
      {
         this.stockOrderList = stockOrderList;

         // Initialise view
         InitializeListView();
      }
      public OrderListDlg(StockDictionary stockDictionary, StockOrderList stockOrderList)
      {
         InitializeComponent();

         this.stockDictionary = stockDictionary;
         this.stockOrderList = stockOrderList;

         // Initialise view
         InitializeListView();
      }
      private void InitializeListView()
      {
         // Clear existing view
         this.portofolioView.Items.Clear();

         // Group the orders by stock name
         var orders = from order in this.stockOrderList
                      where order is StockOrder
                      orderby order.StockName, order.CreationDate
                      group order by order.StockName;

         //Initialise ListView
         ListViewItem[] viewItems = new ListViewItem[this.stockOrderList.Count];
         string[] subItems = new string[this.portofolioView.Columns.Count];
         int i = 0;
         int nbItems = 0;
         foreach (var stockOrderGroup in orders)
         {
            foreach (var stockOrder in stockOrderGroup)
            {
               i = 0;
               subItems[i++] = stockOrderGroup.Key;
               subItems[i++] = stockOrder.ID.ToString();
               subItems[i++] = stockOrder.Type.ToString();
               subItems[i++] = stockOrder.IsShortOrder.ToString();
               subItems[i++] = stockOrder.State.ToString();
               subItems[i++] = stockOrder.ExecutionDate.ToShortDateString();
               subItems[i++] = stockOrder.Number.ToString();
               subItems[i++] = stockOrder.Value.ToString();
               subItems[i++] = stockOrder.Fee.ToString("0.##");
               subItems[i++] = stockOrder.GapInPoints.ToString("0.##");
               subItems[i++] = stockOrder.UnitCost.ToString("0.###");
               subItems[i++] = stockOrder.TotalCost.ToString("0.##");
               viewItems[nbItems++] = new ListViewItem(subItems);
            }
         }
         ListViewItem[] viewItemsSubArray = new ListViewItem[nbItems];
         System.Array.Copy(viewItems, viewItemsSubArray, nbItems);
         this.portofolioView.Items.AddRange(viewItemsSubArray);
      }
      void portofolioView_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         if (SelectedStockChanged != null)
         {
            SelectedStockChanged(this.portofolioView.SelectedItems[0].Text, true);
         }
      }

      private void editToolStripMenuItem_Click(object sender, EventArgs e)
      {
         StockOrder stockOrder = GetSelectedStockOrder();
         if (stockOrder != null)
         {
            OrderEditionDlg orderEditionDlg = new OrderEditionDlg(stockOrder);
            orderEditionDlg.ShowDialog();
         }
         InitializeListView();
      }

      private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
      {
         StockOrder stockOrder = GetSelectedStockOrder();
         if (stockOrder != null)
         {
            this.stockOrderList.Remove(stockOrder);

            // Send the event
            if (StockOrderDeleted != null)
            {
               StockOrderDeleted(stockOrder);
            }
         }
         InitializeListView();
      }

      private StockOrder GetSelectedStockOrder()
      {
         StockOrder stockOrder = null;
         if (this.portofolioView.SelectedItems.Count != 0)
         {
            ListViewItem selectedItem = this.portofolioView.SelectedItems[0];
            int id = int.Parse(selectedItem.SubItems[1].Text);
            stockOrder = this.stockOrderList.GetOrder(id);
         }
         return stockOrder;
      }

      private void purgeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         StockOrder stockOrder = GetSelectedStockOrder();
         if (stockOrder != null)
         {
            this.stockOrderList.PurgeOrders(stockOrder.StockName);

            // Send the event
            StockOrderDeleted(stockOrder);
         }
         InitializeListView();
      }
   }
}
