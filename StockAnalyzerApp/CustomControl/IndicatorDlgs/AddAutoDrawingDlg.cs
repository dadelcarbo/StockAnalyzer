using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddAutoDrawingDlg : Form
    {
        public string AutoDrawingName { get { return this.autoDrawingComboBox.SelectedItem.ToString(); } }

        public AddAutoDrawingDlg()
        {
            InitializeComponent();

            foreach (string indicatorName in StockAutoDrawingManager.GetAutoDrawingList())
            {
                this.autoDrawingComboBox.Items.Add(indicatorName);
            }
            this.autoDrawingComboBox.SelectedItem = this.autoDrawingComboBox.Items[0];
        }

        private void autoDrawingComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockAutoDrawing ts = StockAutoDrawingManager.CreateAutoDrawing(this.AutoDrawingName);
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
