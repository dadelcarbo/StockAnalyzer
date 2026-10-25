using StockAnalyzer;
using StockAnalyzer.StockLogging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp
{
    public static class PipeServer
    {
        private const string PipeName = "UltimateChartistPipe";
        public static void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            string uri = reader.ReadLine();
                            if (!string.IsNullOrEmpty(uri))
                            {
                                ProcessUri(uri);
                            }
                        }
                    }
                }
            });
        }

        public static void ForwardUriToRunningInstance(string uri)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(2000); // 2s timeout
                using var writer = new StreamWriter(client) { AutoFlush = true };
                writer.Write(uri);
            }
            catch { /* running instance busy or gone: silently exit */ }
        }

        private static void ProcessUri(string uri)
        {
            try
            {
                // Extract parameter from URI if needed, for example: ultimate-chartist://open?instrument=FR0014007ND6p&duration=Daily&theme=_Palmares
                uri = uri.Split('?')[1];
                var parameters = uri.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                var dict = new System.Collections.Generic.Dictionary<string, string>();

                foreach (var param in parameters)
                {
                    var parts = param.Split('=');
                    if (parts.Length == 2)
                    {
                        dict[parts[0]] = parts[1];
                    }
                }

                // Example: bring the main window to the foreground
                var mainWindow = Application.OpenForms[0] as StockAnalyzerForm;
                if (mainWindow != null)
                {
                    mainWindow.Invoke((MethodInvoker)(() =>
                    {
                        mainWindow.HandleUriParameters(dict);
                    }));
                }
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
        }
    }
}