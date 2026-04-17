using StockAnalyzer.StockLogging;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
                var culture = new CultureInfo("en-GB");
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StockAnalyzerForm());
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                MessageBox.Show(ex.Message, "Internal Software Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message, "Internal Software Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }
    }
}