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
    public partial class SarexSimulatorTuningDlg : Form
    {
        private StockDictionary stockDictionary = null;
        private StockPortofolioList stockPortofolioList = null;
        private ToolStripProgressBar progressBar = null;
        private StockSerie.Groups group;

        public event StockAnalyzerForm.SimulationCompletedEventHandler BatchSimulationCompleted;
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public string SelectedStockName
        {
            get { return this.stockComboBox.SelectedItem.ToString(); }
            set { this.stockComboBox.SelectedIndex = this.stockComboBox.Items.IndexOf(value); }
        }

        public SarexSimulatorTuningDlg(StockDictionary stockDictionary, StockSerie.Groups group, StockPortofolioList stockPortofolioList, ToolStripProgressBar progressBar)
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
            StockSerie stockSerie = null;

            StockPersonality initialPersonality = this.simulationParameterControl.Personality.Clone();
            if (this.generateReportCheckBox.Checked)
            {
                this.GenerateReportHeader(group + "_SARTuningReport.csv", false);
            }

            StockOrder.TaxRate = this.simulationParameterControl.taxRate;
            StockOrder.FixedFee = this.simulationParameterControl.fixedFee;

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
            }
        }
        private void TuneAllIndicators(StockSerie stockSerie)
        {
            float indicatorSARAccelerationStepStep = float.MaxValue;
            float indicatorSARFastSwingFactorStep = float.MaxValue;

            float bestReturn = float.MinValue;
            StockPersonality bestPersonality = null;

            List<FloatPropertyRange> sarexParamRangeList = null;
            FloatPropertyRange accelerationStepRange = null;
            FloatPropertyRange fastSwingFactorRange = null;

            #region Tune only margin with no indicator

            sarexParamRangeList = SarexParamRangeListFromDialog(ref indicatorSARAccelerationStepStep, ref indicatorSARFastSwingFactorStep);
            accelerationStepRange = sarexParamRangeList.First(r => r.Name == "IndicatorSARAccelerationStep");
            fastSwingFactorRange = sarexParamRangeList.First(r => r.Name == "IndicatorSARFastSwingFactor");

            stockSerie.StockAnalysis.StockPersonality = new StockPersonality();

            TuneSarexParams(stockSerie, sarexParamRangeList, ref bestReturn, ref bestPersonality);

            // Update GUI with new parameters
            if (bestPersonality != null)
            {
                stockSerie.StockAnalysis.StockPersonality = bestPersonality.Clone();
                stockSerie.ResetSAREX();
                this.simulationParameterControl.Personality = bestPersonality.Clone();
                this.simulationParameterControl.Refresh();

                if (!this.allStocksCheckBox.Checked)
                {
                    MessageBox.Show("Best return is " + bestReturn.ToString("P2"));
                }
                if (this.generateReportCheckBox.Checked)
                {
                    this.GenerateReportLine(group + "_SARTuningReport.csv", stockSerie, bestReturn);
                }
            }
            #endregion

            stockSerie.StockAnalysis.StockPersonality = bestPersonality;
        }
        #endregion

        private void TuneSarexParams(StockSerie stockSerie, List<FloatPropertyRange> propertyRangeList,
            ref float bestReturn, ref StockPersonality bestPersonality)
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

                TuneIndicatorList(stockSerie, propertyRangeList, true, ref bestReturn, ref bestPersonality);

                this.progressBar.Value = 0;

            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Check input parameters");
            }
        }

        private void TuneIndicatorList(StockSerie stockSerie, List<FloatPropertyRange> rangeList,
            bool refreshGUI, ref float bestReturn, ref StockPersonality bestPersonality)
        {
            int rangeCount = rangeList.Count;
            if (rangeCount == 0)
            {
                return;
            }

            if (rangeCount == 1)
            {
                TuneProperty(stockSerie, rangeList[0], refreshGUI, ref bestReturn, ref bestPersonality);
            }
            else
            {
                FloatPropertyRange range = rangeList[0];
                List<FloatPropertyRange> subList = rangeList.GetRange(1, rangeCount - 1);

                float mediumValue = (range.Max + range.Min) / 2.0f;
                float maxWidth = (range.Max - range.Min) / 2.0f;

                stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue);
                TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestReturn, ref bestPersonality);

                float precision = range.Step / -100.0f;
                for (float currentWidth = range.Step; (maxWidth - currentWidth) > precision; currentWidth += range.Step)
                {
                    stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue + currentWidth);
                    TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestReturn, ref bestPersonality);

                    stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, mediumValue - currentWidth);
                    TuneIndicatorList(stockSerie, subList, refreshGUI, ref bestReturn, ref bestPersonality);
                }
            }
        }
        private void TuneProperty(StockSerie stockSerie, FloatPropertyRange range, bool refreshGUI,
            ref float bestReturn, ref StockPersonality bestPersonality)
        {            
            float mediumValue = (range.Max + range.Min) / 2.0f;
            float maxWidth = (range.Max - range.Min) / 2.0f;

            UpdateBestForValue(stockSerie, range, refreshGUI, ref bestReturn, ref bestPersonality, mediumValue);

            float precision = range.Step / -100.0f;
            for (float currentWidth = range.Step; (maxWidth - currentWidth) > precision; currentWidth += range.Step)
            {
                UpdateBestForValue(stockSerie, range, refreshGUI, ref bestReturn, ref bestPersonality, mediumValue + currentWidth);
                UpdateBestForValue(stockSerie, range, refreshGUI, ref bestReturn, ref bestPersonality, mediumValue - currentWidth);
            }
        }

        private void UpdateBestForValue(StockSerie stockSerie, FloatPropertyRange range, bool refreshGUI,
            ref float bestReturn, ref StockPersonality bestPersonality, float currentValue)
        {
            stockSerie.StockAnalysis.StockPersonality.SetFloatPropertyValue(range, currentValue);
            stockSerie.ResetSAREX();

            float currentReturn = stockSerie.CalculateSarReturn(StockDataType.SAREX_FOLLOWER_CUSTOM);

            if ( currentReturn > bestReturn)
            {
                bestPersonality = stockSerie.StockAnalysis.StockPersonality.Clone();
                bestReturn = currentReturn;

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

        public void GenerateReportHeader(string fileName, bool append)
        {
            // Generate report
            if (!System.IO.Directory.Exists(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report"))
            {
                System.IO.Directory.CreateDirectory(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report");
            }
            using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + fileName, append))
            {
                sr.WriteLine("StockName,IndicatorSARAccelerationStep,IndicatorSARFastSwingFactor,AddedValue(%)");
            }
        }
        public void GenerateReportLine(string fileName, StockSerie stockSerie, float addedValue)
        {
            using (StreamWriter sr = new StreamWriter(StockAnalyzerSettings.Properties.Settings.Default.StockAnalyzerRootFolder + "\\Report\\" + fileName, true))
            {
                sr.WriteLine(stockSerie.StockName + "," + stockSerie.StockAnalysis.StockPersonality.IndicatorSARAccelerationStep.ToString(StockAnalyzerForm.EnglishCulture) + "," + stockSerie.StockAnalysis.StockPersonality.IndicatorSARFastSwingFactor.ToString(StockAnalyzerForm.EnglishCulture) + "," +
                    addedValue.ToString(StockAnalyzerForm.EnglishCulture));
            }
        }
    }
}
