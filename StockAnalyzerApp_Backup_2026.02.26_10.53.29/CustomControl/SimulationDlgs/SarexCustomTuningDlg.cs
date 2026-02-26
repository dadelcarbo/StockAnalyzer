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
using System.IO;
using System.Globalization;

namespace StockAnalyzerApp.CustomControl
{
    public partial class SarexCustomTuningDlg : Form
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

        public SarexCustomTuningDlg(StockDictionary stockDictionary, StockSerie.Groups group, StockPortofolioList stockPortofolioList, ToolStripProgressBar progressBar)
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
        private void tuneOneByOneButton_Click(object sender, System.EventArgs e)
        {
            StockSerie stockSerie = null;

            // Build indicator to tune list
            List<StockIndicator> indicatorsToTune = new List<StockIndicator>();
            foreach (StockIndicator indicator in this.simulationParameterControl.Personality.IndicatorDictionary.Values)
            {
                if (indicator.IsActive && indicator.Type != StockIndicatorType.BUY_SELL_RATE && indicator.Type != StockIndicatorType.NONE)
                {
                    indicatorsToTune.Add(indicator.Clone());
                }
            }
            if (indicatorsToTune.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Please select the indicators and ranges to tune");
                return;
            }

            if (this.allStocksCheckBox.Checked)
            {
                StockPersonality initialPersonality = this.simulationParameterControl.Personality.Clone();

                this.progressBar.Maximum = this.stockComboBox.Items.Count;
                this.progressBar.Minimum = 0;
                this.progressBar.Value = 0;
                foreach (object item in this.stockComboBox.Items)
                {
                    this.stockComboBox.SelectedItem = item;

                    stockSerie = this.stockDictionary[item.ToString()];
                    this.simulationParameterControl.Personality = initialPersonality.Clone();

                    TuneOneByOneIndicators(stockSerie, indicatorsToTune);

                    if (SelectedPortofolioChanged!=null) 
                    {
                        SelectedPortofolioChanged(this.Portofolio, false);
                    }

                    this.progressBar.Value++;
                }
                if (BatchSimulationCompleted != null)
                {
                    BatchSimulationCompleted(this.simulationParameterControl);
                }
                this.progressBar.Value = 0;
            }
            else
            {
                stockSerie = this.stockDictionary[this.stockComboBox.SelectedItem.ToString()];

                TuneOneByOneIndicators(stockSerie, indicatorsToTune);
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
        private void tuneAllBtn_Click(object sender, System.EventArgs e)
        {
            StockSerie stockSerie = null;

            // Build indicator to tune list
            List<StockIndicator> indicatorsToTune = new List<StockIndicator>();
            foreach (StockIndicator indicator in this.simulationParameterControl.Personality.IndicatorDictionary.Values)
            {
                if (indicator.IsActive && indicator.Type != StockIndicatorType.BUY_SELL_RATE && indicator.Type != StockIndicatorType.NONE)
                {
                    indicatorsToTune.Add(indicator.Clone());
                }
            }
            if (indicatorsToTune.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Please select the indicators and ranges to tune");
                return;
            }

            StockPersonality initialPersonality = this.simulationParameterControl.Personality.Clone();
            if (this.allStocksCheckBox.Checked)
            {
                this.progressBar.Maximum = this.stockComboBox.Items.Count;
                this.progressBar.Minimum = 0;
                this.progressBar.Value = 0;
                foreach (object item in this.stockComboBox.Items)
                {
                    this.stockComboBox.SelectedItem = item;

                    stockSerie = this.stockDictionary[item.ToString()];
                    this.simulationParameterControl.Personality = initialPersonality.Clone();

                    TuneAllIndicators(stockSerie, indicatorsToTune);

                    if (SelectedPortofolioChanged!=null) 
                    {
                        SelectedPortofolioChanged(this.Portofolio, false);
                    }

                    this.progressBar.Value++;
                }
                if (BatchSimulationCompleted != null)
                {
                    BatchSimulationCompleted(this.simulationParameterControl);
                }
                this.progressBar.Value = 0;
            }
            else
            {
                stockSerie = this.stockDictionary[this.stockComboBox.SelectedItem.ToString()];

                TuneAllIndicators(stockSerie, indicatorsToTune);
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
        private void TuneOneByOneIndicators(StockSerie stockSerie, List<StockIndicator> indicatorsToTune)
        {
            StockPersonality bestPersonality = null;
            float bestReturn = 0;
            StockPersonality newPersonality = new StockPersonality();
            foreach (StockIndicator indicator in indicatorsToTune)
            {
                List<StockIndicator> indicatorList = new List<StockIndicator>();
                indicatorList.Add(indicator);
                this.GAIndicatorList(indicatorList, stockSerie, 3, 10, ref bestPersonality, ref bestReturn);

                Console.WriteLine("Best return = " + bestReturn.ToString() + " best personality = " + bestPersonality.ToString());
                newPersonality.IndicatorDictionary.Add(indicator.Type, indicator.Clone());
            }

            stockSerie.StockAnalysis.StockPersonality = newPersonality;
        }

        private SortedDictionary<float, StockPersonality> EvalPersonalityList(StockSerie stockSerie, List<StockPersonality> personalityList)
        {
            SortedDictionary<float, StockPersonality> returnDico = new SortedDictionary<float, StockPersonality>();

            float returnValue;
            foreach (StockPersonality stockPersonality in personalityList)
            {
                stockSerie.StockAnalysis.StockPersonality = stockPersonality;
                stockSerie.AddSerie(StockDataType.SAREX_FOLLOWER_CUSTOM, null);
                stockSerie.AddSerie(StockIndicatorType.BUY_SELL_RATE, null);

                returnValue = stockSerie.CalculateSarReturn(StockDataType.SAREX_FOLLOWER_CUSTOM);
                if (returnValue >0 && !returnDico.ContainsKey(returnValue))
                {
                    returnDico.Add(returnValue, stockPersonality);
                }
            }

            return returnDico;
        }

        private List<StockPersonality> GenerateNewGeneration(List<StockPersonality> personalityList, int populationSize)
        {
            List<StockPersonality> newGenPersonalityList = new List<StockPersonality>();
            StockPersonality newPersonality = null;
            foreach (StockPersonality personality in personalityList)
            {
                for (int i = 0; i < populationSize; i++)
                {
                    newPersonality = personality.Clone();
                    foreach (StockIndicator indicator in newPersonality.IndicatorDictionary.Values)
                    {
                        float stdev = (indicator.Range.Max - indicator.Range.Min) / 4.0f;

                        indicator.Impact = FloatRandom.NextUniform(indicator.Range.Min, indicator.Range.Max);
                        indicator.Range.Max = indicator.Impact + stdev;
                        indicator.Range.Min = indicator.Impact - stdev;
                    }
                    newGenPersonalityList.Add(newPersonality);
                }
            }

            return newGenPersonalityList;
        }
        private void GAIndicatorList(List<StockIndicator> indicatorList, StockSerie stockSerie, int maxGeneration, int populationSize, ref StockPersonality bestPersonality, ref float bestReturn)
        {
            int nbFittest = 5;

            float impact, range, step, bestResult = float.MinValue;

            List<StockPersonality> personalityList = new List<StockPersonality>();
            List<StockPersonality> tmpPersonalityList = null;
            List<StockPersonality> personalityNewGenList = new List<StockPersonality>();
            StockPersonality personality = null;
            StockPersonality newPersonality = null;
            StockIndicator newIndicator = null;

            int nbTryPerIndicator = 10;

            // Generate initial population
            foreach (StockIndicator indicator in indicatorList)
            {
                if (personalityList.Count == 0)
                {
                    impact = indicator.Range.Min;
                    step = (indicator.Range.Max - indicator.Range.Min) / (float)(nbTryPerIndicator -1);
                    for (int i = 0; i < nbTryPerIndicator; i++)
                    {
                        newIndicator = new StockIndicator(indicator.Type, true, impact, indicator.SmoothingType, impact - step, impact + step, step / 10.0f);
                        newPersonality = new StockPersonality();
                        newPersonality.IndicatorDictionary.Add(indicator.Type, newIndicator);
                        personalityList.Add(newPersonality);
                        impact += step;
                    }
                }
                else
                {
                    tmpPersonalityList = new List<StockPersonality>();
                    foreach (StockPersonality currentPersonality in personalityList)
                    {
                        impact = indicator.Range.Min;
                        step = (indicator.Range.Max - indicator.Range.Min) / (float)(nbTryPerIndicator-1);
                        for (int i = 0; i < nbTryPerIndicator; i++)
                        {
                            newIndicator = new StockIndicator(indicator.Type, true, impact, indicator.SmoothingType, impact - step, impact + step, step / 10.0f);
                            newPersonality = currentPersonality.Clone();
                            newPersonality.IndicatorDictionary.Add(indicator.Type, newIndicator);
                            tmpPersonalityList.Add(newPersonality);
                            impact += step;
                        }
                    }
                    personalityList = tmpPersonalityList;
                }
            }

            // Start GA
            SortedDictionary<float, StockPersonality> returnDico = null;
            for (int n = 0; n < maxGeneration; n++)
            {
                // Calculate fittest personalities
                returnDico = this.EvalPersonalityList(stockSerie, personalityList);

                // Select the best personalities
                personalityList.Clear();
                for (int i = 0; i < Math.Min(nbFittest, returnDico.Count); i++)
                {
                    personality = returnDico.Values.ElementAt(returnDico.Count - i - 1);

                    // Calculate new indicator min/max values
                    foreach (StockIndicator stockIndicator in personality.IndicatorDictionary.Values)
                    {
                        range = (stockIndicator.Range.Max - stockIndicator.Range.Min) / 4.0f;
                        stockIndicator.Range.Max = stockIndicator.Impact + range;
                        stockIndicator.Range.Min = stockIndicator.Impact - range;
                    }
                    personalityList.Add(personality);
                } // at this point the nbFittest are stored in the array.

                // Generate new generation
                personalityNewGenList = GenerateNewGeneration(personalityList, populationSize);
                personalityList.AddRange(personalityNewGenList);

                bestPersonality = returnDico.Values.Last();
                bestReturn = returnDico.Keys.Last();
                Console.WriteLine("Best return = " + bestReturn.ToString() + " best personality = " + bestPersonality.ToString());

                if (bestReturn <= bestResult)
                {
                    break;
                }
                else
                {
                    bestResult = bestReturn;
                }
            }
        }

        private void TuneAllIndicators(StockSerie stockSerie, List<StockIndicator> indicatorList)
        {
            StockPersonality bestPersonality = null;
            float bestReturn = 0;

            this.GAIndicatorList(indicatorList, stockSerie, 8, 20, ref bestPersonality, ref bestReturn);

            Console.WriteLine("Best return = " + bestReturn.ToString() + " best personality = " + bestPersonality.ToString());
            stockSerie.StockAnalysis.StockPersonality = bestPersonality;

            this.simulationParameterControl.Personality = bestPersonality;
        }
        #endregion
        private List<FloatPropertyRange> IndicatorRangeListFromDialog()
        {
            // Build property range list 
            List<FloatPropertyRange> propertyRangeList = new List<FloatPropertyRange>();
            foreach (StockIndicator indicator in this.simulationParameterControl.Personality.IndicatorDictionary.Values)
            {
                if (indicator.IsActive)
                {
                    propertyRangeList.Add(indicator.Range);
                }
            }
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

        private void simulationParameterControl_Load(object sender, EventArgs e)
        {

        }
    }
}
