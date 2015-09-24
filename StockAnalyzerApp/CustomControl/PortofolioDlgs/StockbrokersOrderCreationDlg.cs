using System;
using System.IO;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
   public partial class StockbrokersOrderCreationDlg : Form
   {
      private StockPortofolio stockPortofolio;
      private StockDictionary stockDictionary;

      public event StockAnalyzerForm.SavePortofolio SavePortofolioNeeded;

      public StockbrokersOrderCreationDlg(StockPortofolio stockPortofolio, StockDictionary stockDictionary)
      {
         InitializeComponent();

         this.stockPortofolio = stockPortofolio;
         this.stockDictionary = stockDictionary;
      }

      private void createOrdersBtn_Click(object sender, EventArgs e)
      {
         string text = this.ordersTextBox.Text.Replace(",", "").Trim();
         text = text.Replace("  ORD ", "|");
         text = text.Replace(" ORD ", "|");
         text = text.Replace("£", "");
         text = text.Replace("   ", "|");
         text = text.Replace("  ", "|");
         text = text.Replace("p|", "|");
         text = text.Replace("\r\n|\r\n", "\r\n");
         text = text.Replace("\r\n|", "\r\n");
         text = text.Replace("\r\n\r\n", "\r\n");
         this.ordersTextBox.Text = text;
         this.Refresh();

         StringReader sr = new StringReader(this.ordersTextBox.Text);
         string line = string.Empty;
         string errorMsg = string.Empty;
         while ((line = sr.ReadLine()) != null)
         {
            StockOrder stockOrder = StockOrder.CreateFromStockBrokers(line, ref errorMsg);
            if (stockOrder != null)
            {
               OrderEditionDlg orderEditionDlg = new OrderEditionDlg(stockOrder.StockName, new string[] { this.stockPortofolio.Name }, null);
               orderEditionDlg.Order = stockOrder;
               orderEditionDlg.ShowDialog();
               if (orderEditionDlg.DialogResult == DialogResult.OK)
               {
                  this.stockPortofolio.OrderList.Add(stockOrder);
               }
            }
            else
            {
               System.Windows.Forms.MessageBox.Show(errorMsg, "Error creating the order");
               break;
            }
         }
      }

      private void closeBtn_Click(object sender, EventArgs e)
      {
         if (SavePortofolioNeeded != null)
         {
            SavePortofolioNeeded();
         }

         this.Close();
      }
   }
}
