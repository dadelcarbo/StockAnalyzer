using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.CloudDlgs
{
    public partial class AddCloudDlg : Form
    {
        public string CloudName { get { return this.cloudComboBox.SelectedItem.ToString(); } }

        public AddCloudDlg()
        {
            InitializeComponent();

            foreach (string cloudName in StockCloudManager.GetList())
            {
                this.cloudComboBox.Items.Add(cloudName);
            }
            this.cloudComboBox.SelectedItem = this.cloudComboBox.Items[0];
        }

        private void cloudComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockCloud ts = StockCloudManager.CreateCloud(this.CloudName);
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
