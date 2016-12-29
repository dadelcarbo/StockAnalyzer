using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockStrategyClasses;
using StockAnalyzerSettings.Properties;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl
{
    public partial class FilteredStrategySimulatorDlg : Form
    {
        private StockDictionary stockDictionary = null;
        private StockPortofolioList stockPortofolioList = null;

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedPortofolioChangedEventHandler SelectedPortofolioChanged;

        public string SelectedStockName
        {
            get { return this.stockComboBox.SelectedItem.ToString(); }
            set
            {
                if (value != SelectedPortofolio.Name)
                {
                    this.stockComboBox.SelectedIndex = Math.Max(this.stockComboBox.Items.IndexOf(value), 0);
                    if (this.portofolioComboBox.Items.Contains(value + "_P"))
                    {
                        this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(value + "_P");
                    }
                }
            }
        }

        public StockPortofolio SelectedPortofolio
        {
            get { return this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString()); }
            set { this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(value.Name); }
        }

        public delegate void SimulationCompletedEventHandler();
        public event SimulationCompletedEventHandler SimulationCompleted;

        public FilteredStrategySimulatorDlg(StockDictionary stockDictionary, StockPortofolioList stockPortofolioList, string stockName)
        {
            InitializeComponent();

            // Initialize portofolio combo
            this.stockDictionary = stockDictionary;
            this.portofolioComboBox.Enabled = true;
            this.portofolioComboBox.Items.Clear();
            if (stockPortofolioList.Count == 0)
            {
                StockPortofolio portofolio = new StockPortofolio("SIMULATION");
                stockPortofolioList.Add(portofolio);
            }
            foreach (string name in stockPortofolioList.GetPortofolioNames())
            {
                this.portofolioComboBox.Items.Add(name);
            }
            this.portofolioComboBox.SelectedItem = this.portofolioComboBox.Items[0];

            // Initialize stock combo
            this.stockPortofolioList = stockPortofolioList;
            this.stockComboBox.Enabled = true;
            this.stockComboBox.Items.Clear();
            foreach (StockSerie stockSerie in stockDictionary.Values)
            {
                this.stockComboBox.Items.Add(stockSerie.StockName);
            }
            this.stockComboBox.SelectedItem = this.stockComboBox.Items[0];
        }

        private void simulateTradingBtn_Click(object sender, EventArgs e)
        {
            this.simulationParameterControl.ValidateInputParameters();

            if (filterIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(buyFilterComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortFilterComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid filter: " + filterIndicatorTextBox.Text);
                return;
            }

            if (triggerIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(buyFilterComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortFilterComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid trigger: " + triggerIndicatorTextBox.Text);
                return;
            }
            //
            GenerateSimulation();

            // Send event
            if (SimulationCompleted != null)
            {
                SimulationCompleted();
            }
        }

        private void GenerateSimulation()
        {
            // Manage selected Stock and portofolio
            // Get selected portofolio
            StockPortofolio portofolio = this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
            portofolio.OrderList.Clear();

            // Get selected Stock
            StockSerie stockSerie = this.stockDictionary[this.SelectedStockName];
            if (!stockSerie.Initialise()) { return; }

            StockFilteredStrategyBase filteredStrategy = new StockFilteredStrategyBase(filterEvents, triggerEvents,
                buyTriggerComboBox.Text, shortTriggerComboBox.Text,
                buyFilterComboBox.Text, shortFilterComboBox.Text);


            StockAnalyzerForm.MainFrame.SetThemeFromStrategy(filteredStrategy);

            StockOrder lastOrder = stockSerie.GenerateSimulation(filteredStrategy,
                simulationParameterControl.StartDate, simulationParameterControl.EndDate.AddHours(18),
                simulationParameterControl.amount, simulationParameterControl.reinvest,
                simulationParameterControl.amendOrders, simulationParameterControl.supportShortSelling,
                this.simulationParameterControl.takeProfit, this.simulationParameterControl.profitRate,
                this.simulationParameterControl.stopLoss, this.simulationParameterControl.stopLossRate,
                simulationParameterControl.fixedFee, simulationParameterControl.taxRate, portofolio);

            // Do a bit of cleanup
            if (lastOrder != null && this.simulationParameterControl.removePendingOrders)
            {
                if (lastOrder.IsBuyOrder())
                {
                    lastOrder = null;
                }
                else
                {
                    if (lastOrder.State != StockOrder.OrderStatus.Executed)
                    {
                        portofolio.OrderList.Remove(portofolio.OrderList.Last());
                        lastOrder = null;
                    }
                }
            }

            if (this.simulationParameterControl.displayPendingOrders && lastOrder != null && lastOrder.State == StockOrder.OrderStatus.Pending)
            {
                OrderEditionDlg orderEditionDlg = new OrderEditionDlg(lastOrder);
                orderEditionDlg.StartPosition = FormStartPosition.Manual;
                orderEditionDlg.Location = new Point(0, 0);
                orderEditionDlg.ShowDialog();
            }
        }
        private void stockComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedStockChanged != null)
            {
                SelectedStockChanged(SelectedStockName, false);
            }
        }
        void portofolioComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SelectedPortofolioChanged != null)
            {
                StockPortofolio portofolio = this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
                if (portofolio != null)
                {
                    SelectedPortofolioChanged(portofolio, false);
                }
            }
        }

        private string previousFilterIndicator = string.Empty;
        private IStockEvent filterEvents = null;
        private void filterIndicatorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (filterIndicatorTextBox.Text == string.Empty) return;
            if (previousFilterIndicator == filterIndicatorTextBox.Text) return;
            previousFilterIndicator = filterIndicatorTextBox.Text;
            IStockViewableSeries filterIndicator = null;

            if (StockIndicatorManager.Supports(this.filterIndicatorTextBox.Text))
            {
                filterIndicator = StockIndicatorManager.CreateIndicator(this.filterIndicatorTextBox.Text);
            }
            else
            {
                if (StockTrailStopManager.Supports(this.filterIndicatorTextBox.Text))
                {
                    filterIndicator = StockTrailStopManager.CreateTrailStop(this.filterIndicatorTextBox.Text);
                }
                else
                {
                    if (StockPaintBarManager.Supports(this.filterIndicatorTextBox.Text))
                    {
                        filterIndicator = StockPaintBarManager.CreatePaintBar(this.filterIndicatorTextBox.Text);
                    }
                    else
                    {
                        if (StockDecoratorManager.Supports(this.filterIndicatorTextBox.Text))
                        {
                            string[] fields = this.filterIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 2)
                            {
                                filterIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                            }
                        }
                    }
                }
            }

            if (filterIndicator != null)
            {
                filterEvents = filterIndicator as IStockEvent;

                buyFilterComboBox.Items.Clear();
                buyFilterComboBox.Items.AddRange(filterEvents.EventNames.Cast<object>().ToArray());
                if (buyFilterComboBox.Items.Contains(buyFilterComboBox.Text))
                {
                    buyFilterComboBox.SelectedItem = buyFilterComboBox.Text;
                }
                else
                {
                    buyFilterComboBox.SelectedIndex = 0;
                }

                shortFilterComboBox.Items.Clear();
                shortFilterComboBox.Items.AddRange(filterEvents.EventNames.Cast<object>().ToArray());
                if (shortFilterComboBox.Items.Contains(shortFilterComboBox.Text))
                {
                    shortFilterComboBox.SelectedItem = shortFilterComboBox.Text;
                }
                else
                {
                    shortFilterComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("Cannot create indicator1Name " + this.filterIndicatorTextBox.Text + " , check syntax please");
            }
        }

        private string previousTriggerIndicator = string.Empty;
        private IStockEvent triggerEvents = null;
        private void triggerIndicatorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (triggerIndicatorTextBox.Text == string.Empty) return;
            if (previousTriggerIndicator == triggerIndicatorTextBox.Text) return;
            previousTriggerIndicator = triggerIndicatorTextBox.Text;

            IStockViewableSeries triggerIndicator = null;
            if (StockIndicatorManager.Supports(this.triggerIndicatorTextBox.Text))
            {
                triggerIndicator = StockIndicatorManager.CreateIndicator(this.triggerIndicatorTextBox.Text);
            }
            else
            {
                if (StockTrailStopManager.Supports(this.triggerIndicatorTextBox.Text))
                {
                    triggerIndicator = StockTrailStopManager.CreateTrailStop(this.triggerIndicatorTextBox.Text);
                }
                else
                {
                    if (StockPaintBarManager.Supports(this.triggerIndicatorTextBox.Text))
                    {
                        triggerIndicator = StockPaintBarManager.CreatePaintBar(this.triggerIndicatorTextBox.Text);
                    }
                    else
                    {
                        if (StockDecoratorManager.Supports(this.triggerIndicatorTextBox.Text))
                        {
                            string[] fields = this.triggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 2)
                            {
                                triggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                            }
                        }
                    }
                }
            }
            if (triggerIndicator != null)
            {
                triggerEvents = triggerIndicator as IStockEvent;

                buyTriggerComboBox.Items.Clear();
                buyTriggerComboBox.Items.AddRange(triggerEvents.EventNames.Cast<object>().ToArray());
                if (buyTriggerComboBox.Items.Contains(buyTriggerComboBox.Text))
                {
                    buyTriggerComboBox.SelectedItem = buyTriggerComboBox.Text;
                }
                else
                {
                    buyTriggerComboBox.SelectedIndex = 0;
                }

                shortTriggerComboBox.Items.Clear();
                shortTriggerComboBox.Items.AddRange(triggerEvents.EventNames.Cast<object>().ToArray());
                if (shortTriggerComboBox.Items.Contains(shortTriggerComboBox.Text))
                {
                    shortTriggerComboBox.SelectedItem = shortTriggerComboBox.Text;
                }
                else
                {
                    shortTriggerComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("Cannot create indicator1Name, check syntax please");
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.strategyNameTextBox.Text))
            {
                MessageBox.Show("Provide a strategy name");
                return;
            }

            if (filterIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(buyFilterComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortFilterComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid filter: " + filterIndicatorTextBox.Text);
                return;
            }

            if (triggerIndicatorTextBox.Text == string.Empty ||
                string.IsNullOrEmpty(buyFilterComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortFilterComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid trigger: " + triggerIndicatorTextBox.Text);
                return;
            }

            StockFilteredStrategyBase filteredStrategy = new StockFilteredStrategyBase(filterEvents, triggerEvents,
           buyTriggerComboBox.Text, shortTriggerComboBox.Text,
           buyFilterComboBox.Text, shortFilterComboBox.Text);

            filteredStrategy.Name = this.strategyNameTextBox.Text;

            filteredStrategy.Save(Settings.Default.RootFolder);
            this.simulationParameterControl.LoadStrategies(filteredStrategy.Name);

            StrategyManager.ResetStrategyList();
        }


        void simulationParameterControl_SelectedStrategyChanged(string strategyName)
        {
            if (strategyName == string.Empty)
            {
                this.filterIndicatorTextBox.Text = string.Empty;
                previousFilterIndicator = string.Empty;
                this.buyFilterComboBox.Items.Clear();
                this.buyFilterComboBox.Text = string.Empty;
                this.shortFilterComboBox.Items.Clear();
                this.shortFilterComboBox.Text = string.Empty;

                this.triggerIndicatorTextBox.Text = string.Empty;
                previousTriggerIndicator = string.Empty;
                this.buyTriggerComboBox.Items.Clear();
                this.buyTriggerComboBox.Text = string.Empty;
                this.shortTriggerComboBox.Items.Clear();
                this.shortTriggerComboBox.Text = string.Empty;

                return;
            }
            StockFilteredStrategyBase strategy = StrategyManager.CreateFilteredStrategy(strategyName);

            this.filterIndicatorTextBox.Text = strategy.FilterName;
            filterIndicatorTextBox_TextChanged(null, null);

            this.buyFilterComboBox.SelectedItem = strategy.OkToBuyFilterEventName;
            this.shortFilterComboBox.SelectedItem = strategy.OkToShortFilterEventName;

            this.triggerIndicatorTextBox.Text = strategy.TriggerName;
            triggerIndicatorTextBox_TextChanged(null, null);

            this.buyTriggerComboBox.SelectedItem = strategy.BuyTriggerEventName;
            this.shortTriggerComboBox.SelectedItem = strategy.ShortTriggerEventName;
        }

    }
}
