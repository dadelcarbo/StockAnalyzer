using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class AddDecoratorDlg : Form
    {
        public string DecoratorName => this.decoratorComboBox.SelectedItem.ToString();

        public AddDecoratorDlg()
        {
            InitializeComponent();

            foreach (string decoratorName in StockDecoratorManager.GetDecoratorList())
            {
                this.decoratorComboBox.Items.Add(decoratorName);
            }
            this.decoratorComboBox.SelectedItem = this.decoratorComboBox.Items[0];
        }

        private void decoratorComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            IStockDecorator ts = StockDecoratorManager.CreateDecorator(this.DecoratorName, null);
            this.descriptionTextBox.Text = ts.Definition;
        }
    }
}
