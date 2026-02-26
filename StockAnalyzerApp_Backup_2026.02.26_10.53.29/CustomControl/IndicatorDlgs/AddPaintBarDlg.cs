using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddPaintBarDlg : Form
    {
        public string PaintBarName => this.paintBarComboBox.SelectedItem.ToString();

        public AddPaintBarDlg()
        {
            InitializeComponent();

            foreach (string indicatorName in StockPaintBarManager.GetPaintBarList())
            {
                this.paintBarComboBox.Items.Add(indicatorName);
            }
            this.paintBarComboBox.SelectedItem = this.paintBarComboBox.Items[0];
        }

        private void paintBarComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockPaintBar ts = StockPaintBarManager.CreatePaintBar(this.PaintBarName);

            this.descriptionTextBox.Text = ts == null ? "Paint Bar not defined !!!" : ts?.Definition;
        }
    }
}
