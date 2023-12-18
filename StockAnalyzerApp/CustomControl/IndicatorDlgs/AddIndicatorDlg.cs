using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddIndicatorDlg : Form
    {
        public string IndicatorName => this.indicatorComboBox.SelectedItem.ToString();

        public AddIndicatorDlg(bool priceIndicator)
        {
            InitializeComponent();

            foreach (string indicatorName in StockIndicatorManager.GetIndicatorList(priceIndicator))
            {
                this.indicatorComboBox.Items.Add(indicatorName);
            }
            this.indicatorComboBox.SelectedItem = this.indicatorComboBox.Items[0];
        }

        private void indicatorComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockIndicator ts = StockIndicatorManager.CreateIndicator(this.IndicatorName);
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
