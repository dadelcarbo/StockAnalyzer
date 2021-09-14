using System;
using System.Diagnostics;
using System.IO;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockLogging
{
    public class MethodLogger : IDisposable
    {
        private StackFrame sf;
        private Type callerType;
        public MethodLogger(Object caller)
        {
            if (StockLog.Logger.isMethodLoggingEnabled)
            {
                callerType = caller.GetType();
                sf = new StackTrace(1, true).GetFrame(0);
                StockLog.WriteMethodEntry(callerType, sf);
            }
        }

        public MethodLogger(Type callerType)
        {
            if (StockLog.Logger.isMethodLoggingEnabled)
            {
                this.callerType = callerType;
                sf = new StackTrace(1, true).GetFrame(0);
                StockLog.WriteMethodEntry(callerType, sf);
            }
        }

        public void Dispose()
        {
            if (StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.WriteMethodExit(callerType, sf);
            }
        }
    }
    public class StockLog : IDisposable
    {
        public bool isEnabled = false;
        public bool isMethodLoggingEnabled = false;

        public bool isConsoleLogging = false;

        static private StockLog logger = null;
        static public StockLog Logger { get { if (logger == null) { logger = new StockLog(); } return logger; } }

        private StreamWriter sw;

        private StockLog()
        {
            isEnabled = Settings.Default.LoggingEnabled;
            isMethodLoggingEnabled = false; // Settings.Default.LoggingEnabled;
            if (isEnabled)
            {
                if (!Debugger.IsAttached)
                {
                    string logFolder = Settings.Default.RootFolder + @"\log";
                    if (Directory.Exists(logFolder))
                    {
                        try
                        {
                            foreach (string file in Directory.GetFiles(logFolder))
                            {
                                if (File.GetLastWriteTime(file).Date < DateTime.Today)
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    string fileName = logFolder + @"\log_" + DateTime.Now.ToString("yyyMMdd_hhmmss") + ".log";
                    sw = new StreamWriter(fileName, false);
                    sw.AutoFlush = true;
                }
                else
                {
                    sw = new StreamWriter(Console.OpenStandardOutput());
                    sw.AutoFlush = true;
                }
            }
        }

        static public void Write(string logText)
        {
            if (StockLog.Logger.isEnabled)
            {
                StackTrace st = new StackTrace(1, true);
                StackFrame sf = st.GetFrame(0);
                StockLog.Logger.sw.WriteLine("{0}({1},{2}): {3} : {4}", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), sf.GetMethod().Name, logText);
            }
        }
        static public void WriteMethodEntry(StackFrame sf)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.Logger.sw.WriteLine("{0}({1},{2}): {3} : Entry", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), sf.GetMethod().Name);
            }
        }

        static public void WriteMethodEntry(Type type, StackFrame sf)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.Logger.sw.WriteLine("{0}({1},{2}): {3}::{4} : Entry", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), type.ToString(), sf.GetMethod().Name);
            }
        }
        static public void WriteMethodExit(StackFrame sf)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.Logger.sw.WriteLine("{0}({1},{2}): {3} : Exit", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), sf.GetMethod().Name);
            }
        }
        static public void WriteMethodExit(Type type, StackFrame sf)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.Logger.sw.WriteLine("{0}({1},{2}): {3}::{4} : Exit", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), type.ToString(), sf.GetMethod().Name);
            }
        }

        static public void Write(Exception objException)
        {
            if (StockLog.Logger.isEnabled)
            {
                string strException = objException.Message;
                var innerException = objException.InnerException;
                var padding = string.Empty;
                while (innerException != null)
                {
                    strException += Environment.NewLine + padding + innerException.Message;
                    padding += "  ";
                    innerException = innerException.InnerException;
                }
                StreamWriter sw = StockLog.Logger.sw;
                if (objException.Source != null)
                {
                    sw.WriteLine("Source      : " +
                            objException.Source.ToString().Trim());
                }
                if (objException.TargetSite != null)
                {
                    sw.WriteLine("Method      : " +
                            objException.TargetSite.Name.ToString());
                }
                sw.WriteLine("Date        : " +
                        DateTime.Now.ToLongTimeString());
                sw.WriteLine("Time        : " +
                        DateTime.Now.ToShortDateString());
                sw.WriteLine("Error       : " +
                        strException.Trim());
                if (objException.StackTrace != null)
                {
                    sw.WriteLine("Stack Trace : " +
                            objException.StackTrace.ToString().Trim());
                }
                sw.WriteLine("^^-------------------------------------------------------------------^^");
                sw.Flush();
            }
        }

        public void Dispose()
        {
            if (sw != null)
            {
                sw.Close();
            }
        }
    }
}
