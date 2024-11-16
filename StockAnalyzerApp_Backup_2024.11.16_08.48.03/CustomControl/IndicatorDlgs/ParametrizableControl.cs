using StockAnalyzer.StockClasses.StockViewableItems;
using System.Globalization;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class ParametrizableControl : UserControl
    {
        private IStockViewableSeries viewableItem;
        public IStockViewableSeries ViewableItem
        {
            get { return viewableItem; }
            set
            {
                viewableItem = value;
                this.paramListView.Items.Clear();
                if (viewableItem != null)
                {
                    this.paramListView.Items.Clear();

                    ListViewItem[] viewItems = new ListViewItem[viewableItem.ParameterCount];
                    for (int i = 0; i < viewableItem.ParameterCount; i++)
                    {
                        viewItems[i] = new ListViewItem(new string[] { viewableItem.Parameters[i].ToString(), viewableItem.ParameterNames[i], viewableItem.ParameterTypes[i].ToString().Replace("System.", "") });
                    }
                    this.paramListView.Items.AddRange(viewItems);
                }
            }
        }
        public ParametrizableControl()
        {
            InitializeComponent();
        }

        private void paramListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (this.ViewableItem == null || e.Label == null) return;
            ListViewItem item = this.paramListView.Items[e.Item];
            string type = item.SubItems[2].Text;
            switch (type)
            {
                case "Single":
                    float floatResult;
                    if (float.TryParse(e.Label, NumberStyles.Any, CultureInfo.CurrentCulture, out floatResult))
                    {
                        ViewableItem.Parameters[e.Item] = floatResult;
                        e.CancelEdit = false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid entry, expecting a " + type, "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.CancelEdit = true;
                    }
                    break;
                case "Int32":
                    int intResult;
                    if (int.TryParse(e.Label, out intResult))
                    {
                        ViewableItem.Parameters[e.Item] = intResult;
                        e.CancelEdit = false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid entry, expecting a " + type, "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.CancelEdit = true;
                    }
                    break;
                case "Boolean":
                    bool boolResult;
                    if (bool.TryParse(e.Label, out boolResult))
                    {
                        ViewableItem.Parameters[e.Item] = boolResult;
                        e.CancelEdit = false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid entry, expecting a " + type, "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.CancelEdit = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
