using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockPortfolioStrategy;

namespace StockAnalyzerApp.CustomControl
{
    public partial class PortfolioSimulatorDlg : Form
    {
        private StockDictionary stockDictionary = null;
        private StockPortofolioList stockPortofolioList = null;

        private List<StockSerie> portfolioStockSeries = null;
        public event StockAnalyzerForm.SelectedPortofolioChangedEventHandler SelectedPortofolioChanged;

        private List<StockWatchList> watchLists;

        public IStockPortfolioStrategy SelectedStrategy { get; set; }

        public StockPortofolio SelectedPortofolio
        {
            get { return this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString()); }
            set { this.portofolioComboBox.SelectedIndex = this.portofolioComboBox.Items.IndexOf(value.Name); }
        }

        public delegate void SimulationCompletedEventHandler();

        public event SimulationCompletedEventHandler SimulationCompleted;

        public PortfolioSimulatorDlg(StockDictionary stockDictionary, StockPortofolioList stockPortofolioList,
            string stockName, List<StockWatchList> watchLists)
        {
            InitializeComponent();

            foreach (var val in Enum.GetValues(typeof(UpdatePeriod)))
            {
                this.frequencyComboBox.Items.Add(val);
            }
            this.frequencyComboBox.SelectedIndex = 2;

            // Initialize portofolio combo
            this.stockDictionary = stockDictionary;
            this.portofolioComboBox.Enabled = true;
            this.stockPortofolioList = stockPortofolioList;
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

            // Initialise input series
            this.watchLists = watchLists;
            this.portfolioStockSeries = new List<StockSerie>();
            foreach (string wlName in this.watchLists.Select(wl => wl.Name))
            {
                this.watchListComboBox.Items.Add(wlName);
            }
            this.watchListComboBox.SelectedIndex = 0;

            // Initialize Strategy combo
            this.strategyComboBox.Enabled = true;
            this.strategyComboBox.Items.Clear();
            foreach (string name in PortfolioStrategyManager.GetStrategyList())
            {
                this.strategyComboBox.Items.Add(name);
            }
            this.strategyComboBox.SelectedIndex = 0;
        }


        private void strategyComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.SelectedStrategy =
                PortfolioStrategyManager.CreateStrategy(this.strategyComboBox.SelectedItem.ToString(),
                    portfolioStockSeries, this.SelectedPortofolio, this.stockDictionary);
        }

        private void simulateTradingBtn_Click(object sender, EventArgs e)
        {
            //  @@@@ this.simulationParameterControl.ValidateInputParameters();

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

            this.SelectedPortofolio.Clear();
            this.SelectedPortofolio.TotalDeposit = 10000;

            // Initialise
            this.SelectedStrategy.Initialise(this.portfolioStockSeries, this.SelectedPortofolio, this.stockDictionary);

            // Initialise Series
            foreach (StockSerie serie in this.portfolioStockSeries)
            {
                if (!serie.Initialise())
                {
                    MessageBox.Show("Failed to initialize " + serie.StockName, "Initialisation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Run simulation
            this.SelectedStrategy.Apply(this.startDatePicker.Value, this.endDatePicker.Value,
                (UpdatePeriod)this.frequencyComboBox.SelectedItem);
        }

        private void portofolioComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SelectedPortofolioChanged != null)
            {
                StockPortofolio portofolio =
                    this.stockPortofolioList.Get(this.portofolioComboBox.SelectedItem.ToString());
                if (portofolio != null)
                {
                    SelectedPortofolioChanged(portofolio, false);
                }
            }
        }

        private void newPortfolioBtn_Click(object sender, EventArgs e)
        {
        }

        private void watchListComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.portfolioStockSeries.Clear();
            foreach (string stockName in this.watchLists.Find(wl => wl.Name == this.watchListComboBox.SelectedItem.ToString()).StockList.Where(stockName => this.stockDictionary.ContainsKey(stockName)))
            {
                this.portfolioStockSeries.Add(this.stockDictionary[stockName]);
            }
        }
    }
}