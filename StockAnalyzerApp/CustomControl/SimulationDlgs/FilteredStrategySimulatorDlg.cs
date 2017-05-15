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
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;

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
                this.filterIndicatorTextBox.Text = "TRUE(1)";
                this.filterIndicatorTextBox_TextChanged(null, null);
                this.buyFilterComboBox.SelectedItem = "True";
                this.shortFilterComboBox.SelectedItem = "True";
                return;
            }

            if (stopTriggerIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(stopLongTriggerComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(stopShortTriggerComboBox.SelectedItem.ToString()))
            {
                this.stopTriggerIndicatorTextBox.Text = "TRUE(1)";
                this.stopTriggerIndicatorTextBox_TextChanged(null, null);
                this.stopLongTriggerComboBox.SelectedItem = "False";
                this.stopShortTriggerComboBox.SelectedItem = "False";
                return;
            }

            if (entryTriggerIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(buyTriggerComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortTriggerComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid entry trigger: " + entryTriggerIndicatorTextBox.Text);
                return;
            }

            if (exitTriggerIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(sellTriggerComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(coverTriggerComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid exit trigger: " + exitTriggerIndicatorTextBox.Text);
                return;
            }

            if (stopTriggerIndicatorTextBox.Text == string.Empty || string.IsNullOrEmpty(stopLongTriggerComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(stopShortTriggerComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid stop trigger: " + stopTriggerIndicatorTextBox.Text);
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

            StockFilteredStrategyBase filteredStrategy = new StockFilteredStrategyBase(
                filterEvents, buyFilterComboBox.Text, shortFilterComboBox.Text,
                entryTriggerEvents, buyTriggerComboBox.Text, shortTriggerComboBox.Text,
                exitTriggerEvents, sellTriggerComboBox.Text, coverTriggerComboBox.Text,
                stopTriggerEvents, stopLongTriggerComboBox.Text, stopShortTriggerComboBox.Text);

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
                        else if (this.filterIndicatorTextBox.Text.StartsWith("TRAIL|") && StockTrailManager.Supports(this.filterIndicatorTextBox.Text.Replace("TRAIL|", "")))
                        {
                            string[] fields = this.filterIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 3)
                            {
                                filterIndicator = StockTrailManager.CreateTrail(fields[1], fields[2]);
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
                MessageBox.Show("Cannot create filter indicator " + this.filterIndicatorTextBox.Text + " , check syntax please");
            }
        }

        private string previousEntryTriggerIndicator = string.Empty;
        private IStockEvent entryTriggerEvents = null;
        private void entryTriggerIndicatorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (entryTriggerIndicatorTextBox.Text == string.Empty) return;
            if (previousEntryTriggerIndicator == entryTriggerIndicatorTextBox.Text) return;
            previousEntryTriggerIndicator = entryTriggerIndicatorTextBox.Text;

            IStockViewableSeries triggerIndicator = null;
            if (StockIndicatorManager.Supports(this.entryTriggerIndicatorTextBox.Text))
            {
                triggerIndicator = StockIndicatorManager.CreateIndicator(this.entryTriggerIndicatorTextBox.Text);
            }
            else
            {
                if (StockTrailStopManager.Supports(this.entryTriggerIndicatorTextBox.Text))
                {
                    triggerIndicator = StockTrailStopManager.CreateTrailStop(this.entryTriggerIndicatorTextBox.Text);
                }
                else
                {
                    if (StockPaintBarManager.Supports(this.entryTriggerIndicatorTextBox.Text))
                    {
                        triggerIndicator = StockPaintBarManager.CreatePaintBar(this.entryTriggerIndicatorTextBox.Text);
                    }
                    else
                    {
                        if (StockDecoratorManager.Supports(this.entryTriggerIndicatorTextBox.Text))
                        {
                            string[] fields = this.entryTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 2)
                            {
                                triggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                            }
                        }
                        else if (this.entryTriggerIndicatorTextBox.Text.StartsWith("TRAIL|") && StockTrailManager.Supports(this.entryTriggerIndicatorTextBox.Text.Replace("TRAIL|", "")))
                        {
                            string[] fields = this.entryTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 3)
                            {
                                triggerIndicator = StockTrailManager.CreateTrail(fields[1], fields[2]);
                            }
                        }
                    }
                }
            }
            if (triggerIndicator != null)
            {
                entryTriggerEvents = triggerIndicator as IStockEvent;

                buyTriggerComboBox.Items.Clear();
                buyTriggerComboBox.Items.AddRange(entryTriggerEvents.EventNames.Cast<object>().ToArray());
                if (buyTriggerComboBox.Items.Contains(buyTriggerComboBox.Text))
                {
                    buyTriggerComboBox.SelectedItem = buyTriggerComboBox.Text;
                }
                else
                {
                    buyTriggerComboBox.SelectedIndex = 0;
                }

                shortTriggerComboBox.Items.Clear();
                shortTriggerComboBox.Items.AddRange(entryTriggerEvents.EventNames.Cast<object>().ToArray());
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
                MessageBox.Show("Cannot create trigger indicator " + this.entryTriggerIndicatorTextBox.Text + " , check syntax please");
            }
        }


        private string previousExitTriggerIndicator = string.Empty;
        private IStockEvent exitTriggerEvents = null;
        private void exitTriggerIndicatorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (exitTriggerIndicatorTextBox.Text == string.Empty) return;
            if (previousExitTriggerIndicator == exitTriggerIndicatorTextBox.Text) return;
            previousExitTriggerIndicator = exitTriggerIndicatorTextBox.Text;

            IStockViewableSeries triggerIndicator = null;
            if (StockIndicatorManager.Supports(this.exitTriggerIndicatorTextBox.Text))
            {
                triggerIndicator = StockIndicatorManager.CreateIndicator(this.exitTriggerIndicatorTextBox.Text);
            }
            else
            {
                if (StockTrailStopManager.Supports(this.exitTriggerIndicatorTextBox.Text))
                {
                    triggerIndicator = StockTrailStopManager.CreateTrailStop(this.exitTriggerIndicatorTextBox.Text);
                }
                else
                {
                    if (StockPaintBarManager.Supports(this.exitTriggerIndicatorTextBox.Text))
                    {
                        triggerIndicator = StockPaintBarManager.CreatePaintBar(this.exitTriggerIndicatorTextBox.Text);
                    }
                    else
                    {
                        if (StockDecoratorManager.Supports(this.exitTriggerIndicatorTextBox.Text))
                        {
                            string[] fields = this.exitTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 2)
                            {
                                triggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                            }
                        }
                        else if (this.exitTriggerIndicatorTextBox.Text.StartsWith("TRAIL|") && StockTrailManager.Supports(this.exitTriggerIndicatorTextBox.Text.Replace("TRAIL|", "")))
                        {
                            string[] fields = this.exitTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 3)
                            {
                                triggerIndicator = StockTrailManager.CreateTrail(fields[1], fields[2]);
                            }
                        }
                    }
                }
            }
            if (triggerIndicator != null)
            {
                exitTriggerEvents = triggerIndicator as IStockEvent;

                sellTriggerComboBox.Items.Clear();
                sellTriggerComboBox.Items.AddRange(exitTriggerEvents.EventNames.Cast<object>().ToArray());
                if (sellTriggerComboBox.Items.Contains(sellTriggerComboBox.Text))
                {
                    sellTriggerComboBox.SelectedItem = sellTriggerComboBox.Text;
                }
                else
                {
                    sellTriggerComboBox.SelectedIndex = 0;
                }

                coverTriggerComboBox.Items.Clear();
                coverTriggerComboBox.Items.AddRange(exitTriggerEvents.EventNames.Cast<object>().ToArray());
                if (coverTriggerComboBox.Items.Contains(coverTriggerComboBox.Text))
                {
                    coverTriggerComboBox.SelectedItem = coverTriggerComboBox.Text;
                }
                else
                {
                    coverTriggerComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("Cannot create exit trigger indicator " + this.exitTriggerIndicatorTextBox.Text + " , check syntax please");
            }
        }

        private string previousStopTriggerIndicator = string.Empty;
        private IStockEvent stopTriggerEvents = null;
        private void stopTriggerIndicatorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (stopTriggerIndicatorTextBox.Text == string.Empty) return;
            if (previousStopTriggerIndicator == stopTriggerIndicatorTextBox.Text) return;
            previousStopTriggerIndicator = stopTriggerIndicatorTextBox.Text;

            IStockViewableSeries triggerIndicator = null;
            if (StockIndicatorManager.Supports(this.stopTriggerIndicatorTextBox.Text))
            {
                triggerIndicator = StockIndicatorManager.CreateIndicator(this.stopTriggerIndicatorTextBox.Text);
            }
            else
            {
                if (StockTrailStopManager.Supports(this.stopTriggerIndicatorTextBox.Text))
                {
                    triggerIndicator = StockTrailStopManager.CreateTrailStop(this.stopTriggerIndicatorTextBox.Text);
                }
                else
                {
                    if (StockPaintBarManager.Supports(this.stopTriggerIndicatorTextBox.Text))
                    {
                        triggerIndicator = StockPaintBarManager.CreatePaintBar(this.stopTriggerIndicatorTextBox.Text);
                    }
                    else
                    {
                        if (StockDecoratorManager.Supports(this.stopTriggerIndicatorTextBox.Text))
                        {
                            string[] fields = this.stopTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 2)
                            {
                                triggerIndicator = StockDecoratorManager.CreateDecorator(fields[0], fields[1]);
                            }
                        }
                        else if (this.stopTriggerIndicatorTextBox.Text.StartsWith("TRAIL|") && StockTrailManager.Supports(this.stopTriggerIndicatorTextBox.Text.Replace("TRAIL|", "")))
                        {
                            string[] fields = this.stopTriggerIndicatorTextBox.Text.Split('|');
                            if (fields.Length == 3)
                            {
                                triggerIndicator = StockTrailManager.CreateTrail(fields[1], fields[2]);
                            }
                        }
                    }
                }
            }
            if (triggerIndicator != null)
            {
                stopTriggerEvents = triggerIndicator as IStockEvent;

                stopLongTriggerComboBox.Items.Clear();
                stopLongTriggerComboBox.Items.AddRange(stopTriggerEvents.EventNames.Cast<object>().ToArray());
                if (stopLongTriggerComboBox.Items.Contains(stopLongTriggerComboBox.Text))
                {
                    stopLongTriggerComboBox.SelectedItem = stopLongTriggerComboBox.Text;
                }
                else
                {
                    stopLongTriggerComboBox.SelectedIndex = 0;
                }

                stopShortTriggerComboBox.Items.Clear();
                stopShortTriggerComboBox.Items.AddRange(stopTriggerEvents.EventNames.Cast<object>().ToArray());
                if (stopShortTriggerComboBox.Items.Contains(stopShortTriggerComboBox.Text))
                {
                    stopShortTriggerComboBox.SelectedItem = stopShortTriggerComboBox.Text;
                }
                else
                {
                    stopShortTriggerComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("Cannot create stop trigger indicator " + this.stopTriggerIndicatorTextBox.Text + " , check syntax please");
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

            if (entryTriggerIndicatorTextBox.Text == string.Empty ||
                string.IsNullOrEmpty(buyFilterComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(shortFilterComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid entry trigger: " + entryTriggerIndicatorTextBox.Text);
                return;
            }
            if (exitTriggerIndicatorTextBox.Text == string.Empty ||
                string.IsNullOrEmpty(sellTriggerComboBox.SelectedItem.ToString()) ||
                string.IsNullOrEmpty(coverTriggerComboBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Invalid exit trigger: " + exitTriggerIndicatorTextBox.Text);
                return;
            }

            StockFilteredStrategyBase filteredStrategy = new StockFilteredStrategyBase(
                filterEvents, buyFilterComboBox.Text, shortFilterComboBox.Text,
                entryTriggerEvents, buyTriggerComboBox.Text, shortTriggerComboBox.Text,
                exitTriggerEvents, sellTriggerComboBox.Text, coverTriggerComboBox.Text,
                stopTriggerEvents, stopLongTriggerComboBox.Text, stopShortTriggerComboBox.Text);

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

                this.entryTriggerIndicatorTextBox.Text = string.Empty;
                previousEntryTriggerIndicator = string.Empty;
                this.buyTriggerComboBox.Items.Clear();
                this.buyTriggerComboBox.Text = string.Empty;
                this.shortTriggerComboBox.Items.Clear();
                this.shortTriggerComboBox.Text = string.Empty;

                this.stopTriggerIndicatorTextBox.Text = string.Empty;
                previousStopTriggerIndicator = string.Empty;
                this.stopLongTriggerComboBox.Items.Clear();
                this.stopLongTriggerComboBox.Text = string.Empty;
                this.stopShortTriggerComboBox.Items.Clear();
                this.stopShortTriggerComboBox.Text = string.Empty;

                return;
            }
            StockFilteredStrategyBase strategy = StrategyManager.CreateFilteredStrategy(strategyName);

            this.filterIndicatorTextBox.Text = strategy.FilterName;
            filterIndicatorTextBox_TextChanged(null, null);

            this.buyFilterComboBox.SelectedItem = strategy.OkToBuyFilterEventName;
            this.shortFilterComboBox.SelectedItem = strategy.OkToShortFilterEventName;

            this.entryTriggerIndicatorTextBox.Text = strategy.EntryTriggerName;
            entryTriggerIndicatorTextBox_TextChanged(null, null);

            this.buyTriggerComboBox.SelectedItem = strategy.BuyTriggerEventName;
            this.shortTriggerComboBox.SelectedItem = strategy.ShortTriggerEventName;

            this.exitTriggerIndicatorTextBox.Text = strategy.ExitTriggerName;
            exitTriggerIndicatorTextBox_TextChanged(null, null);

            this.sellTriggerComboBox.SelectedItem = strategy.SellTriggerEventName;
            this.coverTriggerComboBox.SelectedItem = strategy.CoverTriggerEventName;

            this.stopTriggerIndicatorTextBox.Text = strategy.StopTriggerName;
            stopTriggerIndicatorTextBox_TextChanged(null, null);

            this.stopLongTriggerComboBox.SelectedItem = strategy.StopLongTriggerEventName;
            this.stopShortTriggerComboBox.SelectedItem = strategy.StopShortTriggerEventName;
        }

    }
}
