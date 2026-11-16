using Microsoft.Win32;
using StockAnalyzer;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
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
        static void Main(string[] args)
        {
            try
            {
                const string mutexName = "UltimateChartist_SingleInstance";
                using var mutex = new Mutex(true, mutexName, out bool isFirstInstance);

#if !DEBUG
                if (!isFirstInstance)
                {
                    StockLog.Write("Another instance is already running. Forwarding URI to the running instance.");
                    string uri = Array.Find(args, a => a.StartsWith("ultimate-chartist://", StringComparison.OrdinalIgnoreCase));
                    if (uri != null)
                    {
                        StockLog.Write($"Forwarding URI to running instance: {uri}");
                        PipeServer.ForwardUriToRunningInstance(uri);
                    }
                    return; // second instance exits immediately
                }
                else
                {
                    EnsureRegistered();

                    StockLog.Write("This is the first instance. Starting pipe server to listen for URIs.");
                    PipeServer.StartPipeServer();

                    string uri = Array.Find(args, a => a.StartsWith("ultimate-chartist://", StringComparison.OrdinalIgnoreCase));
                    if (uri != null)
                    {
                        Settings.Default.LastInstrument = uri.Split('?')[1].Split('&')[0].Split('=')[1];
                        Settings.Default.SelectedTheme = uri.Split('?')[1].Split('&')[2].Split('=')[1];
                    }
                }
#endif

                var culture = new CultureInfo("en-GB");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new StockAnalyzerForm());

                try
                {
                    if (isFirstInstance)
                        mutex.ReleaseMutex();
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }

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


        /// <summary>
        /// The method to invoke with parameters parsed from the link.
        /// Always runs on the UI thread.
        /// </summary>
        public static void OpenFromLink(Dictionary<string, string> parameters)
        {
            string chart = parameters.TryGetValue("chart", out var c) ? c : "unknown";
            string tf = parameters.TryGetValue("tf", out var t) ? t : "unknown";
            MessageBox.Show($"Open chart {chart} in timeframe {tf}");
            // ... do the real work here
        }


        private const string Scheme = "ultimate-chartist";
        private const string FriendlyName = "URL:Ultimate Chartist Protocol";

        /// <summary>
        /// Ensures the ultimate-chartist:// protocol points to this exe.
        /// Safe to call at every startup. Returns true if the handler is valid.
        /// </summary>
        public static void EnsureRegistered()
        {
            try
            {
                if (IsRegistered())
                    return;
                // Per-user registration: no admin rights required
                using var classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes");
                using var root = classes.CreateSubKey(Scheme);

                root.SetValue(null, FriendlyName);
                root.SetValue("URL Protocol", "");   // required flag

                using var commandKey = root.CreateSubKey(@"shell\open\command");

                string exePath = $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"";

                // Only rewrite if needed (avoids registry churn on every start)
                if ((string)commandKey.GetValue(null) != exePath)
                    commandKey.SetValue(null, exePath);
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
        }

        public static bool IsRegistered()
        {
            using var commandKey = Registry.CurrentUser.OpenSubKey(@$"Software\Classes\{Scheme}\shell\open\command");

            if (commandKey?.GetValue(null) is string current)
            {
                return current.Contains(Process.GetCurrentProcess().MainModule.FileName ?? "");
            }
            return false;
        }
    }
}