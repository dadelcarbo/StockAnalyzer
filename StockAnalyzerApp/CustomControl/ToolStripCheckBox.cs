using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public class ToolStripCheckedBox : ToolStripControlHost
    {
        public ToolStripCheckedBox()
           : base(new CheckBox())
        {
        }

        public CheckBox CheckBox => (CheckBox)this.Control;
    }
}
