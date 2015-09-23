using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StockAnalyzerApp
{
    static class StockAnalyzerApp
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           try
           {
              Application.EnableVisualStyles();
              Application.SetCompatibleTextRenderingDefault(false);
              Application.Run(new StockAnalyzerForm());
           }
           catch (System.Exception ex)
           {
              MessageBox.Show(ex.Message, "Internal Software Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
              if (ex.InnerException != null)
              {
              MessageBox.Show(ex.InnerException.Message, "Internal Software Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
              }
           }
        }
    }
}