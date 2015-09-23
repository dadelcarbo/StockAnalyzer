using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockMath;
using StockAnalyzer;
using System.IO;
using System.Globalization;

namespace StockAnalyzerApp.CustomControl
{
    public partial class SarexStrategySimulatorTuningDlg : Form
    {
        private StockDictionary stockDictionary = null;
        private StockPortofolioList stockPortofolioList = null;
        private ToolStripProgressBar progressBar = null;
        private StockSerie.Groups group;

        public StockPortofolio Portofolio { get; set; }
        public delegate void SimulationCompletedEventHandler();
        public event SimulationCompletedEventHandler SimulationCompleted;
        public event StockAnalyzerForm.SimulationCompletedEventHandler BatchSimulationCompleted;
        public event StockAnalyzerForm.SelectedPortofolioChangedEventHandler SelectedPortofolioChanged;
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public string SelectedStrategy
        {
            get { return this.simulationParameterControl.SelectedStrategy; }
            set { this.simulationParameterControl.SelectedStrategy = value; }
        }
        public string SelectedStockName
        {
            get { return this.stockComboBox.SelectedItem.ToString(); }
            set { this.stockComboBox.SelectedIndex = this.stockComboBox.Items.IndexOf(value); }
        }

        public SarexStrategySimulatorTuningDlg(StockDictionary stockDictionary, StockSerie.Groups group, StockPortofolioList stockPortofolioList, ToolStripProgressBar progressBar)
        {
            InitializeComponent();

            this.stockPortofolioList = stockPortofolioList;
            this.stockDictionary = stockDictionary;
            this.group = group;

            // Initialize stock combo
            this.stockComboBox.Enabled = true;
            this.stockComboBox.Items.Clear();
 
            // Count the stock to simulate to initialise the progress bar
            foreach (StockSerie stockSerie in this.stockDictionary.Values)
            {
                if (!stockSerie.StockAnalysis.Excluded && !stockSerie.IsPortofolioSerie && stockSerie.BelongsToGroup(this.group))
                {
                    this.stockComboBox.Items.Add(stockSerie.StockName);
                }
            }
            this.stockComboBox.SelectedItem = this.stockComboBox.Items[0];

            this.progressBar = progressBar;
        }

        #region Button handlers
        private void tuneAllBtn_Click(object sender, System.EventArgs e)
        {
            Console.WriteLine("tuneAllBtn_Click");

            StockSerie stockSerie = null;

            StockPersonality initialPersonality = this.simulationParameterControl.Personality.Clone();
            if (this.allStocksCheckBox.Checked)
            {
                float indicatorSARAccelerationStepStep = float.MaxValue;
                float indicatorSARFastSwingFactorStep = float.MaxValue;
                List<FloatPropertyRange> marginRangeList = SarexParamRangeListFromDialog(ref indicatorSARAccelerationStepStep, ref indicatorSARFastSwingFactorStep);

                this.progressBar.Maximum = this.stockComboBox.Items.Count;
                this.progressBar.Minimum = 0;
                this.progressBar.Value = 0;
                foreach (object item in this.stockComboBox.Items)
                {
                    this.stockComboBox.SelectedItem = item;

                    stockSerie = this.stockDictionary[item.ToString()];
                    this.simulationParameterControl.Personality = initialPersonality.Clone();
                    SarexParamsRangeListToDialog(marginRangeList);

                    TuneAllIndicators(stockSerie);

                    if (SelectedPortofolioChanged!=null) 
                    {
                        SelectedPortofolioChanged(this.Portofolio, false);
                    }

                    this.progressBar.Value++;
                }
                if (BatchSimulationCompleted != null)
                {
                    BatchSimulationCompleted(null);
                }
                this.progressBar.Value = 0;
            }
            else
            {
                stockSerie = this.stockDictionary[this.stockComboBox.SelectedItem.ToString()];

                TuneAllIndicators(stockSerie);
                if (SelectedPortofolioChanged != null)
                {
                    SelectedPortofolioChanged(this.Portofolio, false);
                }

                if (SimulationCompleted != null)
                {
                    SimulationCompleted();
                }
            }
        }
        private void TuneAllIndicators(StockSerie stockSerie)
        {
            float indicatorSARAccelerationStepStep = float.MaxValue;
            float indicatorSARFastSwingFactorStep = float.MaxValue;

            StockPortofolio bestPortofolio = null;
            StockPersonality bestPersonality = null;

            List<FloatPropertyRange> sarexParamRangeList = null;
            FloatPropertyRange accelerationStepRange = null;
            FloatPropertyRange fastSwingFactorRange = null;

            #region Tune only margin with no indicator
            // Generate report header
            if (this.generateReportCheckBox.Checked)
            {
                this.simulationParameterControl.GenerateReportHeader("TuningReport_" + stockSerie.StockName + ".csv", false);
            }
            sarexParamRangeList = SarexParamRangeListFromDialog(ref indicatorSARAccelerationStepStep, ref indicatorSARFastSwingFactorStep);
            accelerationStepRange = sarexParamRangeList.First(r => r.Name == "IndicatorSARAccelerationStep");
            fastSwingFactorRange = sarexParamRangeList.First(r => r.Name == "IndicatorSARFastSwingFactor");

            stockSerie.StockAnalysis.StockPersonality = new StockPersonality();

            TuneSarexParams(stockSerie, sarexParamRangeList, ref bestPortofolio, ref bestPersonality);

            // Update GUI with new parameters
            if (bestPersonality != null)
            {
                stockSerie.StockAnalysis.StockPersonality = bestPersonality.Clone();
                stockSerie.ResetSAREX();
                this.simulationParameterControl.Personality = bestPersonality.Clone();
                this.simulationParameterControl.Refresh();

                this.Portofolio = bestPortofolio;
                if (SimulationCompleted != null)
                {
                    SimulationCompleted();
                }

                // Generate report header
                if (this.generateReportCheckBox.Checked)
                {
                    this.simulationParameterControl.GenerateReportHeader("TuningReport_" + stockSerie.StockName + ".csv", true);
                    this.simulationParameterControl.GenerateReportLine("TuningReport_" + stockSerie.StockName + ".csv", stockSerie, bestPortofolio);
                }
            }
            #endregion

            this.stockPortofolioList.Remove(bestPortofolio.Name);
            this.stockPortofolioList.Add(bestPortofolio);
            this.Portofolio = bestPortofolio;
            stockSerie.StockAnalysis.StockPersonality = bestPersonality;
        }
        #endregion

        private void TuneIndicators(StockSerie stockSerie, List<FloatPropertyRange> propertyRangeList,
            ref StockPortofolio bestPortofolio, ref StockPersonality bestPersonality)
        {
            try
            {
                int nbIterations = 1;
                if (this.allStocksCheckBox.Checked)
                {
                    nbIterations = this.stockComboBox.Items.Count;
                }
                else
                {
                    foreach (FloatPropertyRange range in propertyRangeList)
                    {
                        nbIterations *= range.NbStep();
                    }
                }
                this.progressBar.Maximum = nbIterations;
                this.progressBar.Value = 0;

                TuneIndicatorList(stockSerie, propertyRangeList, true, ref bestPortofolio, ref bestPersonality);

            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Check input parameters");
            }
        }

        private void TuneSarexParams(StockSerie stockSerie, List<FloatPropertyRange> propertyRangeList,
            ref StockPortofolio bestPortofolio, ref StockPersonality bestPersonality)
        {
            try
            {
                int nbIterations = 1;
                if (this.allStocksCheckBox.Checked)
                {
                    nbIterations = this.stockComboBox.Items.Count;
                }
                else
                {
                    foreach (FloatPropertyRange propRange in propertyRangeList)
                    {
                        nbIterations *= propRange.NbStep();
                    }
                }
                this.progressBar.Maximum = nbIterations;
                this.progressBar.Value = 0;

                TuneIndicatorList(stockSerie, propertyRangeList, true, ref bestPortofolio, ref bestPersonality);

            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Check input parameters");
            }
        }

        private void TuneIndicatorList(StockSerie stockSerie, List<FloatPropertyRange> rangeList,
            bool refreshGUI, ref StockPortofolio bestPortofolio, ref StockPersonality bestPersonality)
        {
            int rangeCount = rangeList.Count;
            if (rangeCount == 0)
            {
                return;
            }

            if (rangeCount == 1)
            {
                TuneProperty(stockSerie, rangeList[0], refreshGUI, ref bestPortofolio, ref bestPersonality);
            }
            else
            {
                FloatPropertyRange range = rangeList[0];
                List<FloatPropertyRange> subList = rangeList.GetRange(1, rangeCount - 1);

                float mediumValue = (range.Max + range.Min) / 2.0f;
                float maxWidth = (range.Max - range.Min) / 2.0f;

                stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue);
                TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestPortofolio, ref bestPersonality);

                float precision = range.Step / -100.0f;
                for (float currentWidth = range.Step; (maxWidth - currentWidth) > precision; currentWidth += range.Step)
                {
                    stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue + currentWidth);
                    TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestPortofolio, ref bestPersonality);

                    stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue - currentWidth);
                    TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestPortofolio, ref bestPersonality);
                }
            }
        }
        private void TuneProperty(StockSerie stockSerie, FloatPropertyRange range, bool refreshGUI,
            ref StockPortofolio bestPortofolio, ref StockPersonality bestPersonality)
        {            
            float mediumValue = (range.Max + range.Min) / 2.0f;
            float maxWidth = (range.Max - range.Min) / 2.0f;

            UpdateBestForValue(stockSerie, range, refreshGUI, ref bestPortofolio, ref bestPersonality, mediumValue);

            float precision = range.Step / -100.0f;
            for (float currentWidth = range.Step; (maxWidth - currentWidth) > precision; currentWidth += range.Step)
            {
                UpdateBestForValue(stockSerie, range, refreshGUI, ref bestPortofolio, ref bestPersonality, mediumValue + currentWidth);
                UpdateBestForValue(stockSerie, range, refreshGUI, ref bestPortofolio, ref bestPersonality, mediumValue - currentWidth);
            }
        }

        private void UpdateBestForValue(StockSerie stockSerie, FloatPropertyRange range, bool refreshGUI,
            ref StockPortofolio bestPortofolio, ref StockPersonality bestPersonality, float currentValue)
        {
            stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, currentValue);
            stockSerie.ResetSAREX();

            //Console.WriteLine("Fixed Fee:" + this.simulationParameterControl.fixedFee + " Tax Rate:" + this.simulationParameterControl.taxRate);

            StockPortofolio currentPortofolio = GenerateTradingSimulation(stockSerie, this.simulationParameterControl.StartDate, this.simulationParameterControl.EndDate.AddHours(18),
                this.simulationParameterControl.amount, this.simulationParameterControl.reinvest, 
                this.simulationParameterControl.amendOrders, this.simulationParameterControl.supportShortSelling,
                this.simulationParameterControl.fixedFee, this.simulationParameterControl.taxRate);


            //if (generateReportCheckBox.Checked)
            //{
            //    this.simulationParameterControl.Personality = currentPersonality;
            //    this.simulationParameterControl.GenerateReportLine("TuningReport_"+stockSerie.StockName+".csv", stockSerie, currentPortofolio);
            //}
            if ((bestPortofolio == null || bestPortofolio.TotalAddedValue < currentPortofolio.TotalAddedValue) && currentPortofolio.OrderList.Count != 0)
            {
                bestPersonality = stockSerie.StockAnalysis.StockPersonality.Clone();
                //Console.WriteLine("new best" + bestPersonality.ToString());
                //Console.WriteLine("new best" + currentPortofolio.ToString());

                // Update new best portofolio
                bestPortofolio = currentPortofolio;

                // Refresh GUI
                if (refreshGUI)
                {
                    this.simulationParameterControl.Personality = bestPersonality;
                }
            }
            if (!this.allStocksCheckBox.Checked)
            {
                if (this.progressBar.Value < this.progressBar.Maximum)
                {
                    this.progressBar.Value++;
                }
                else
                {
                    this.progressBar.Value = 0;
                }
            }
        }

        private StockPortofolio GenerateTradingSimulation(StockSerie stockSerie, System.DateTime startDate, System.DateTime endDate, float amount, bool reinvest, bool amendOrders, bool supportShortSelling, float fixedFee, float taxRate)
        {
            // Manage selected Stock and portofolio
            stockPortofolioList.RemoveAll(p => p.Name == (stockSerie.StockName + "_P"));

            StockPortofolio portofolio = new StockPortofolio(stockSerie.StockName + "_P");
            portofolio.TotalDeposit = amount;
            stockPortofolioList.Add(portofolio);

            stockSerie.GenerateSimulation(SelectedStrategy, startDate, endDate, amount, reinvest,
                                    amendOrders, supportShortSelling, fixedFee, taxRate, portofolio);

            // Create Portofoglio serie
            portofolio.Initialize(stockDictionary);

            return portofolio;
        }

        private void SarexParamsRangeListToDialog(List<FloatPropertyRange> rangeList)
        {
            FloatPropertyRange range = rangeList.First(r => r.Name == "IndicatorSARAccelerationStep");
            this.simulationParameterControl.accelerationStepMinTextBox.Text = range.Min.ToString(StockAnalyzerForm.EnglishCulture);
            this.simulationParameterControl.accelerationStepMaxTextBox.Text = range.Max.ToString(StockAnalyzerForm.EnglishCulture);
            this.simulationParameterControl.accelerationStepStepTextBox.Text = range.Step.ToString(StockAnalyzerForm.EnglishCulture);

            range = rangeList.First(r => r.Name == "IndicatorSARFastSwingFactor");
            this.simulationParameterControl.fastSwingFactorMinTextBox.Text = range.Min.ToString(StockAnalyzerForm.EnglishCulture);
            this.simulationParameterControl.fastSwingFactorMaxTextBox.Text = range.Max.ToString(StockAnalyzerForm.EnglishCulture);
            this.simulationParameterControl.fastSwingFactorStepTextBox.Text = range.Step.ToString(StockAnalyzerForm.EnglishCulture);
        }

        private List<FloatPropertyRange> SarexParamRangeListFromDialog(ref float indicatorSARAccelerationStepStep, ref float indicatorSARFastSwingFactorStep)
        {
            #region Validate input Parameters
            // Validate input parameters

            FloatPropertyRange accelerationStepRange = new FloatPropertyRange("IndicatorSARAccelerationStep", float.Parse(this.simulationParameterControl.accelerationStepMinTextBox.Text, StockAnalyzerForm.EnglishCulture),
                float.Parse(this.simulationParameterControl.accelerationStepMaxTextBox.Text, StockAnalyzerForm.EnglishCulture),
                float.Parse(this.simulationParameterControl.accelerationStepStepTextBox.Text, StockAnalyzerForm.EnglishCulture));

            indicatorSARAccelerationStepStep = accelerationStepRange.Step;

            FloatPropertyRange indicatorSARFastSwingFactorRange = new FloatPropertyRange("IndicatorSARFastSwingFactor", float.Parse(this.simulationParameterControl.fastSwingFactorMinTextBox.Text, StockAnalyzerForm.EnglishCulture),
                float.Parse(this.simulationParameterControl.fastSwingFactorMaxTextBox.Text, StockAnalyzerForm.EnglishCulture),
                float.Parse(this.simulationParameterControl.fastSwingFactorStepTextBox.Text, StockAnalyzerForm.EnglishCulture));

            indicatorSARFastSwingFactorStep = indicatorSARFastSwingFactorRange.Step;
            #endregion

            // Build property range list 
            List<FloatPropertyRange> propertyRangeList = new List<FloatPropertyRange>();
            propertyRangeList.Add(accelerationStepRange);
            propertyRangeList.Add(indicatorSARFastSwingFactorRange);
            return propertyRangeList;
        }
        
        private void stockComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (stockComboBox.SelectedItem == null)
            {
                stockComboBox.SelectedItem = stockComboBox.Items[0];
            }
            StockSerie stockSerie = this.stockDictionary[stockComboBox.SelectedItem.ToString()];
            if (stockSerie.StockAnalysis.StockPersonality == null)
            {
                stockSerie.StockAnalysis.StockPersonality = StockPersonality.CreateDefaultPersonality();
            }
            this.simulationParameterControl.Personality = stockSerie.StockAnalysis.StockPersonality;

            if (SelectedStockChanged != null)
            {
                SelectedStockChanged(stockSerie.StockName, false);
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            this.simulationParameterControl.Personality = StockPersonality.CreateDefaultPersonality();
        }
    }
}
