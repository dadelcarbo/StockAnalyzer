using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class PortofolioDlg : Form
    {
        private StockDictionary stockDictionary;
        private StockPortofolio stockPortofolio;

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public PortofolioDlg(StockDictionary stockDictionary, StockPortofolio stockPortofolio)
        {
            InitializeComponent();

            this.stockDictionary = stockDictionary;
            this.stockPortofolio = stockPortofolio;

            // Initialise list view
            InitializeListView();
        }
        public void SetPortofolio(StockPortofolio portofolio)
        {
            this.stockPortofolio = portofolio;

            // Initialise list view
            InitializeListView();
        }
        private void InitializeListView()
        {
            StockOrder stockOrder;

            // Clear existing view
            this.portofolioView.Items.Clear();
            Dictionary<string, int> nbActiveStocks = this.stockPortofolio.OrderList.GetNbActiveStock();
            //Initialise ListView
            ListViewItem[] viewItems = new ListViewItem[nbActiveStocks.Count + 1];
            string[] subItems = new string[8];
            int nbItems = 0;
            int i = 0;
            StockDailyValue beforeLastStockValue;
            foreach (string stockName in nbActiveStocks.Keys)
            {
                i = 0;
                stockOrder = this.stockPortofolio.OrderList.GetActiveSummaryOrder(stockName);
                if (stockOrder != null)
                {
                    if (this.stockDictionary.ContainsKey(stockName))
                    {
                        StockDailyValue lastStockValue = this.stockDictionary[stockName].Values.Last();
                        beforeLastStockValue = this.stockDictionary[stockName].Values.ElementAt(this.stockDictionary[stockName].Values.Count - 2);
                        float totalValue = stockOrder.Number * lastStockValue.CLOSE;

                        subItems[i++] = stockOrder.StockName;
                        subItems[i++] = stockOrder.Number.ToString();
                        subItems[i++] = stockOrder.UnitCost.ToString("0.00#");
                        subItems[i++] = lastStockValue.CLOSE.ToString();
                        subItems[i++] = ((lastStockValue.CLOSE - stockOrder.UnitCost) / stockOrder.UnitCost).ToString("P2");
                        subItems[i++] =
                           ((lastStockValue.CLOSE - beforeLastStockValue.CLOSE) / beforeLastStockValue.CLOSE).ToString("P2");
                        subItems[i++] = (totalValue - stockOrder.TotalCost).ToString("0.00");
                        subItems[i++] = totalValue.ToString("0.00");
                        viewItems[nbItems++] = new ListViewItem(subItems);
                    }
                    else
                    {
                        float totalValue = Math.Abs(stockOrder.TotalCost);
                        subItems[i++] = stockOrder.StockName;
                        subItems[i++] = stockOrder.Number.ToString();
                        subItems[i++] = stockOrder.UnitCost.ToString("0.00#");
                        subItems[i++] = stockOrder.UnitCost.ToString("0.00#");
                        subItems[i++] = (0).ToString("P2");
                        subItems[i++] = (0).ToString("P2");
                        subItems[i++] = (0).ToString("0.00");
                        subItems[i++] = stockOrder.UnitCost.ToString("0.00");
                        viewItems[nbItems++] = new ListViewItem(subItems);
                    }

                }
            }
            // Add special case for portofoglio
            //if (this.stockDictionary.ContainsKey(this.stockPortofolio.Name))
            //{
            //   this.stockDictionary.Remove(this.stockPortofolio.Name);
            //}
            //if (this.stockPortofolio.OrderList.Count != 0)
            //{
            //   StockSerie refSerie = this.stockDictionary[this.stockPortofolio.OrderList.First().StockName];
            //   this.stockDictionary.Add(this.stockPortofolio.Name, stockPortofolio.GeneratePortfolioStockSerie(this.stockPortofolio.Name, refSerie, refSerie.StockGroup));
            //}
            //else
            //{
            //   StockSerie serie;
            //   string stockName = this.stockPortofolio.Name.Remove(this.stockPortofolio.Name.Length-2);
            //   if (this.stockDictionary.ContainsKey(stockName)){
            //      serie = this.stockDictionary[stockName];
            //   }
            //   else
            //   {
            //      serie = this.stockDictionary["CAC40"];
            //   }
            //   this.stockDictionary.Add(this.stockPortofolio.Name, stockPortofolio.GeneratePortfolioStockSerie(this.stockPortofolio.Name, serie, serie.StockGroup));
            //}
            StockSerie portofoglioSerie = this.stockDictionary[this.stockPortofolio.Name];
            portofoglioSerie.Initialise();
            if (portofoglioSerie.Count > 2)
            {
                StockDailyValue lastStockValue = portofoglioSerie.Values.Last();
                beforeLastStockValue = portofoglioSerie.ValueArray[portofoglioSerie.Values.Count - 2];
                i = 0;
                subItems[i++] = this.stockPortofolio.Name;
                subItems[i++] = "1";
                subItems[i++] = this.stockPortofolio.TotalDeposit.ToString("0.00#");
                subItems[i++] = this.stockPortofolio.TotalPortofolioValue.ToString();
                subItems[i++] = this.stockPortofolio.TotalAddedValue.ToString("P2");
                subItems[i++] = ((lastStockValue.CLOSE - beforeLastStockValue.CLOSE) / beforeLastStockValue.CLOSE).ToString("P2");
                subItems[i++] = (this.stockPortofolio.TotalPortofolioValue - this.stockPortofolio.TotalDeposit).ToString("0.00");
                subItems[i++] = this.stockPortofolio.TotalPortofolioValue.ToString("0.00");
                viewItems[nbItems++] = new ListViewItem(subItems);

                ListViewItem[] viewItemsSubArray = new ListViewItem[nbItems];
                System.Array.Copy(viewItems, viewItemsSubArray, nbItems);
                this.portofolioView.Items.AddRange(viewItemsSubArray);
            }

            // Initialise portofolio params
            this.totalDepositTextBox.Text = stockPortofolio.TotalDeposit.ToString("C2", StockAnalyzerForm.FrenchCulture);
            this.currentValueTextBox.Text = stockPortofolio.TotalPortofolioValue.ToString("C2", StockAnalyzerForm.FrenchCulture);
            this.availableTextBox.Text = stockPortofolio.AvailableLiquitidity.ToString("C2", StockAnalyzerForm.FrenchCulture); ;
            this.addedValueTextBox.Text = stockPortofolio.TotalAddedValue.ToString("P2");
        }
        void portofolioView_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (SelectedStockChanged != null)
            {
                if (this.stockDictionary.ContainsKey(this.stockPortofolio.Name))
                {
                    this.stockDictionary.Remove(this.stockPortofolio.Name);
                }

                //StockSerie referenceStock = this.stockDictionary[this.stockPortofolio.OrderList.First().StockName];
                //this.stockDictionary.Add(this.stockPortofolio.Name, stockPortofolio.GeneratePortfolioStockSerie(this.stockPortofolio.Name, referenceStock, referenceStock.StockGroup));
                if (this.stockDictionary.ContainsKey(this.portofolioView.SelectedItems[0].Text))
                {
                    SelectedStockChanged(this.portofolioView.SelectedItems[0].Text, true);
                }
            }
        }
    }
}
