using System;
using System.Windows.Forms;
using System.IO;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
    public partial class CAPCAOrderCreationDlg : Form
    {
        private StockPortofolio stockPortofolio;
        private StockDictionary stockDictionary;

        public event StockAnalyzerForm.SavePortofolio SavePortofolioNeeded; 

        public CAPCAOrderCreationDlg(StockPortofolio stockPortofolio, StockDictionary stockDictionary)
        {
            InitializeComponent();

            this.stockPortofolio = stockPortofolio;
            this.stockDictionary = stockDictionary;
        }

        private void createOrdersBtn_Click(object sender, EventArgs e)
        {
            string text = this.ordersTextBox.Text.Replace(",", ".").Trim();
            text = text.Replace("\r\n    ", "\r\n");
            text = text.Replace(" étranger", "");
            text = text.Replace(" Achat  ", "|Buy|");
            text = text.Replace(" Vente  ", "|Sell|");
            text = text.Replace("    ", "|");
            text = text.Replace("||", "|");
            text = text.Replace(" Exécuté à ", "|");
            text = text.Replace(" €", "");
            text = text.Replace(" \r\n", "\r\n");
            text = text.Replace("...", "");

            this.ordersTextBox.Text = text;
            this.Refresh();

            StringReader sr = new StringReader(this.ordersTextBox.Text);
            string line = string.Empty;
            string errorMsg = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                StockOrder stockOrder = StockOrder.CreateFromCAPCA(line, ref errorMsg);
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
