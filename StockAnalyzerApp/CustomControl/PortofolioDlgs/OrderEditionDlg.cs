using System;
using System.Drawing;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
   public partial class OrderEditionDlg : Form
   {
      public string PortofolioName
      {
         get
         {
            return this.portofolioComboBox.SelectedItem.ToString();
         }
      }

      private StockOrder stockOrder;
      public StockOrder Order
      {
         get
         {
            return stockOrder;
         }
         set
         {
            stockOrder = value;
            InitFields();
         }
      }
      public OrderEditionDlg(string currentStockName, string[] portofolioList, StockOrder stockOrderTemplate)
      {
         InitializeComponent();

         //Initialise stock name combo list
         StockDictionary stockDictionary = StockAnalyzerForm.MainFrame.StockDictionary;
         foreach (string stockName in stockDictionary.Keys)
         {
            this.stockNameCombo.Items.Add(stockName);
         }

         // Order Type change combo box
         string[] orderTypeNames = Enum.GetNames(typeof(StockOrder.OrderType));
         this.orderTypeComboBox.Enabled = true;
         this.orderTypeComboBox.Items.Clear();
         foreach (string name in orderTypeNames)
         {
            this.orderTypeComboBox.Items.Add(name);
         }
         this.orderTypeComboBox.SelectedIndex = 0;

         // UpDownState combo box
         string[] stateNames = Enum.GetNames(typeof(StockOrder.OrderStatus));
         this.stateComboBox.Enabled = true;
         this.stateComboBox.Items.Clear();
         foreach (string name in stateNames)
         {
            this.stateComboBox.Items.Add(name);
         }
         this.stateComboBox.SelectedIndex = 0;

         // Portofolio change
         if (portofolioList == null)
         {
            portofolioList = StockAnalyzerForm.MainFrame.StockPortofolioList.GetPortofolioNames();
         }
         this.portofolioComboBox.Enabled = true;
         this.portofolioComboBox.Items.Clear();
         foreach (string name in portofolioList)
         {
            this.portofolioComboBox.Items.Add(name);
         }
         this.portofolioComboBox.SelectedItem = this.portofolioComboBox.Items[0];

         // Create default order
         if (stockOrderTemplate == null)
         {
            this.Order = StockOrder.CreateExecutedOrder(currentStockName, StockOrder.OrderType.BuyAtLimit, false, DateTime.Today, DateTime.Today, 0, 0.0f, 0.0f);
         }
         else
         {
            this.Order = stockOrderTemplate;
         }
      }
      public OrderEditionDlg(StockOrder stockOrder)
      {
         InitializeComponent();

         //Initialise stock name combo list
         this.stockNameCombo.Items.Add(stockOrder.StockName);
         this.stockNameCombo.SelectedItem = stockOrder.StockName;

         this.Order = stockOrder;

         // Disable the portofolio change
         this.portofolioComboBox.Enabled = false;

         this.parseOrderBtn.Enabled = false;
         this.orderText.Enabled = false;

         this.amountToInvestTextBox.Text = "1000";
      }
      private void InitFields()
      {
         // StockName
         //Initialise stock name combo list
         string bestMatch = string.Empty;
         string orderStockName = this.Order.StockName.ToUpper();
         foreach (string stockName in this.stockNameCombo.Items)
         {
            if (stockName.CompareTo(orderStockName) <= 0)
            {
               bestMatch = stockName;
            }
         }
         this.stockNameCombo.SelectedIndex = this.stockNameCombo.Items.IndexOf(bestMatch);
         if (bestMatch != orderStockName)
         {
            this.stockNameCombo.BackColor = Color.Red;
         }
         else
         {
            this.stockNameCombo.BackColor = Color.White;
         }

         // Order type
         string[] orderTypeNames = Enum.GetNames(typeof(StockOrder.OrderType));
         this.orderTypeComboBox.Enabled = true;
         this.orderTypeComboBox.Items.Clear();
         foreach (string name in orderTypeNames)
         {
            this.orderTypeComboBox.Items.Add(name);
         }
         this.orderTypeComboBox.SelectedIndex = this.orderTypeComboBox.Items.IndexOf(this.Order.Type.ToString());

         // Short orders
         this.shortCheckBox.Checked = this.Order.IsShortOrder;

         // Order state
         string[] stateNames = Enum.GetNames(typeof(StockOrder.OrderStatus));
         this.stateComboBox.Enabled = true;
         this.stateComboBox.Items.Clear();
         foreach (string name in stateNames)
         {
            this.stateComboBox.Items.Add(name);
         }
         this.stateComboBox.SelectedIndex = this.stateComboBox.Items.IndexOf(this.Order.State.ToString());


         // Initialise fields with stockOrder values
         this.executionDateTimePicker.Value = Order.ExecutionDate;
         this.creationDateTimePicker.Value = Order.CreationDate;

         this.benchmarkTextBox.Text = this.Order.Benchmark.ToString();
         this.gapTextBox.Text = this.Order.GapInPoints.ToString();
         this.limitTextBox.Text = this.Order.Limit.ToString();
         this.thresoldTextBox.Text = this.Order.Threshold.ToString();

         this.nbShareTextBox.Text = Order.Number.ToString();
         this.valueTextBox.Text = Order.Value.ToString();
         this.feeTextBox.Text = Order.Fee.ToString();
         this.amountToInvestTextBox.Text = this.Order.AmountToInvest.ToString();
      }
      // Private members
      private void okBtn_Click(object sender, EventArgs e)
      {
         try
         {
            this.Order.StockName = this.stockNameCombo.SelectedItem.ToString();
            this.Order.Number = int.Parse(this.nbShareTextBox.Text);
            this.Order.Value = float.Parse(this.valueTextBox.Text);
            this.Order.Fee = float.Parse(this.feeTextBox.Text);
            this.Order.Benchmark = float.Parse(this.benchmarkTextBox.Text);
            this.Order.AmountToInvest = float.Parse(this.amountToInvestTextBox.Text);
            this.Order.GapInPoints = float.Parse(this.gapTextBox.Text);
            this.Order.Limit = float.Parse(this.limitTextBox.Text);
            this.Order.Threshold = float.Parse(this.thresoldTextBox.Text);
            this.Order.CreationDate = this.creationDateTimePicker.Value;
            this.Order.ExecutionDate = this.executionDateTimePicker.Value;
            this.Order.IsShortOrder = this.shortCheckBox.Checked;
            this.Order.Type = (StockOrder.OrderType)Enum.Parse(typeof(StockOrder.OrderType), this.orderTypeComboBox.Text);
            this.Order.State = (StockOrder.OrderStatus)Enum.Parse(typeof(StockOrder.OrderStatus), this.stateComboBox.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
         }
         catch (System.Exception exception)
         {
            System.Windows.Forms.MessageBox.Show(exception.Message, "Invalid input data");
         }
      }
      private void parseOrderBtn_Click(object sender, EventArgs e)
      {
         string text = string.Empty;
         int index = 0;
         if (this.PortofolioName == "STOCKBROKERS")
         {
            this.orderText.Text.Replace(",", "").Trim();
            text = text.Replace("  ORD ", "|");
            text = text.Replace(" ORD ", "|");
            text = text.Replace("£", "");
            text = text.Replace("   ", "|");
            text = text.Replace("  ", "|");
            text = text.Replace("p|", "|");
            text = text.Replace("\r\n|\r\n", "\r\n");
            text = text.Replace("\r\n|", "\r\n");
            text = text.Replace("\r\n\r\n", "\r\n");
            this.orderText.Text = text;

            string errorMsg = string.Empty;
            StockOrder stockOrder = StockOrder.CreateFromStockBrokers(this.orderText.Text, ref errorMsg);

            // Update GUI
            InitFields();
         }
         else
         {
            text = this.orderText.Text.Replace("\t", "").Trim();
            if (!string.IsNullOrEmpty(text))
            {
               try
               {
                  string header;

                  // Find Stock Name
                  header = "Titre:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf("\r\n");
                  string stockName = text.Substring(0, index).Trim().ToUpper();
                  //if (!StockDictionary.StockDictionarySingleton.ContainsKey(stockName))
                  //{
                  //   MessageBox.Show(stockName + " Not found !");
                  //   return;
                  //}
                  this.Order.StockName = stockName;
                  this.stockNameCombo.SelectedItem = stockName;
                  this.stockNameCombo.Text = stockName;

                  // Find order type
                  header = "Type d'ordre:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf("\r\n");
                  string orderString = text.Substring(0, index).Trim().ToUpper();
                  if (orderString.StartsWith("ACHAT"))
                  {
                     this.Order.Type = StockOrder.OrderType.BuyAtLimit;
                  }
                  else if (orderString.StartsWith("VENTE"))
                  {
                     this.Order.Type = StockOrder.OrderType.SellAtLimit;
                  }
                  else
                  {
                     MessageBox.Show("Invalid Order Type: " + orderString);
                     return;
                  }
                  this.orderTypeComboBox.SelectedItem = Order.Type.ToString();

                  // Find number of stocks
                  header = "Quantité:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf("\r\n");
                  this.nbShareTextBox.Text = text.Substring(0, index).Trim();
                  this.Order.Number = int.Parse(this.nbShareTextBox.Text);

                  // Find stock value
                  header = "Prix unitaire:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf(" EUR");
                  string orderValueString = text.Substring(0, index).Trim();
                  this.Order.Value = float.Parse(orderValueString, StockAnalyzerForm.FrenchCulture);
                  this.valueTextBox.Text = this.Order.Value.ToString(".##");

                  // Find order fee
                  header = "Montant final:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  header = "Dt ";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf(" EUR");
                  string totalAmount = text.Substring(0, index).Replace(".","");

                  this.Order.Fee = float.Parse(totalAmount, StockAnalyzerForm.FrenchCulture) -
                                   (this.Order.Value*this.Order.Number);
                  this.feeTextBox.Text = this.Order.Fee.ToString(".##");
                  
                  // Find order date
                  header = "Date d'ordre:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf("\r\n");
                  DateTime orderDateTime = DateTime.Parse(text.Substring(0, index).Trim(), StockAnalyzerForm.FrenchCulture);

                  header = "Heure:";
                  index = text.IndexOf(header);
                  text = text.Substring(index + header.Length).Trim();
                  index = text.IndexOf("\r\n");

                  TimeSpan orderTime = TimeSpan.Parse(text.Substring(0, index).Trim(), StockAnalyzerForm.FrenchCulture);

                  this.Order.ExecutionDate = orderDateTime + orderTime;
                  this.executionDateTimePicker.Value = this.Order.ExecutionDate;
                  this.creationDateTimePicker.Value = this.Order.ExecutionDate;

                  InitFields();

               }
               catch (Exception exception)
               {
                  this.orderText.Text = exception.Message;
               }
               this.Refresh();
            }
         }
      }

      private void amountToInvestTextBox_TextChanged(object sender, EventArgs e)
      {
         double amount;
         if (!double.TryParse(this.amountToInvestTextBox.Text, out amount)) return;
         this.nbShareTextBox.Text = Math.Floor(amount / double.Parse(this.valueTextBox.Text)).ToString();
      }
   }
}