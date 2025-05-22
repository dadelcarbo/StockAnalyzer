using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddTrailStopDlg : Form
    {
        public string TrailStopName => this.trailStopComboBox.SelectedItem.ToString();

        public AddTrailStopDlg()
        {
            InitializeComponent();

            foreach (string indicatorName in StockTrailStopManager.GetTrailStopList())
            {
                this.trailStopComboBox.Items.Add(indicatorName);
            }
            this.trailStopComboBox.SelectedItem = this.trailStopComboBox.Items[0];
        }

        private void trailStopComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockTrailStop ts = StockTrailStopManager.CreateTrailStop(this.trailStopComboBox.SelectedItem.ToString());
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
