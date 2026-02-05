using Saxo.OpenAPI.AuthenticationServices;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace SaxoTokenRefresh
{
    /*
    sc create SaxoTokenRefresh binPath="C:\src\Repos\StockAnalyzer\SaxoTokenRefresh\bin\Release\SaxoTokenRefresh.exe"
    sc description SaxoTokenRefresh "Saxo Token Refresh"
    sc config SaxoTokenRefresh start=auto
     */
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Set up a timer to run your logic periodically
            _timer = new System.Timers.Timer(10 * 60 * 1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();

            Settings.Default.LoggingEnabled = true;
            StockLog.Write($"Service Started at {DateTime.Now.ToLongTimeString()}{Environment.NewLine}", true);
        }

        const string saxoPath = @"c:\ProgramData\UltimateChartist\Saxo";

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Settings.Default.LoggingEnabled = true;
            using (var ml = new MethodLogger(this, true))
            {
                try
                {
                    // Check if process is already running
                    var process = Process.GetProcessesByName("Ultimate Chartist").FirstOrDefault();
                    if (process != null)
                    {
                        StockLog.Write("Ultimate Chartist is running. Nothing to do", true);
                        return;
                    }

                    foreach (var file in Directory.GetFiles(saxoPath, "1*.json"))
                    {
                        var clientId = Path.GetFileNameWithoutExtension(file);
                        var session = LoginService.SilentLogin(clientId, saxoPath, false);
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }
            }

            StockLog.Write($"OnTimerElapsed out", true);
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
