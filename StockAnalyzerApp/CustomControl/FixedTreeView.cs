using System;
using System.Windows.Forms;
namespace StockAnalyzerApp.CustomControl
{
   public class FixedTreeView : TreeView
   {
      protected override void WndProc(ref Message m)
      {
         if (m.Msg == 0x203) // identified double click
            m.Result = IntPtr.Zero;
         else
            base.WndProc(ref m);
      }
   }
}