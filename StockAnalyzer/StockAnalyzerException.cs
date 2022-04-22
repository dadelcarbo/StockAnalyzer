using System;
using System.Windows.Forms;

namespace StockAnalyzer
{
    public class StockAnalyzerException : Exception
    {
        public StockAnalyzerException(string p)
           : base(p)
        {
        }
        public StockAnalyzerException(string p, Exception e)
           : base(p, e)
        {
        }

        public static void MessageBox(Exception ex)
        {
            string msg = ex.Message;
            string padding = System.Environment.NewLine + " ";
            Exception e = ex.InnerException;

            while (e != null)
            {
                msg += padding + "=>" + e.Message;
                padding += "  ";
                e = e.InnerException;
            }
            msg += Environment.NewLine;
            msg += ex.StackTrace;
            System.Windows.Forms.MessageBox.Show(msg, "Application error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
