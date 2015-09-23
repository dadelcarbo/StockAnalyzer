using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddTrailDlg : Form
    {
        public string TrailName { get { return this.trailComboBox.SelectedItem.ToString(); } }

        public AddTrailDlg()
        {
            InitializeComponent();

            foreach (string trailName in StockTrailManager.GetTrailList())
            {
                this.trailComboBox.Items.Add(trailName);
            }
            this.trailComboBox.SelectedItem = this.trailComboBox.Items[0];
        }

        private void trailComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockTrail ts = StockTrailManager.CreateTrail(this.TrailName,null);
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
