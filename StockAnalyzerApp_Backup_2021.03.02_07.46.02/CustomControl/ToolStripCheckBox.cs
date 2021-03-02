using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
   public class ToolStripCheckedBox : ToolStripControlHost
   {
      public ToolStripCheckedBox()
         : base(new CheckBox())
      {
      }

      public CheckBox CheckBox
      {
         get { return (CheckBox)this.Control; }
      }
   }
}
