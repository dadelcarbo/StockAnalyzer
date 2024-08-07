﻿using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace StockAnalyzer.StockLogging
{
    public class MethodLogger : IDisposable
    {
        private readonly StackFrame sf;
        private readonly Type callerType;
        private readonly bool isActive = false;
        public MethodLogger(Object caller, bool activated = false, string text = "")
        {
            isActive = activated;
            if (activated && StockLog.Logger.isMethodLoggingEnabled)
            {
                callerType = caller.GetType();
                sf = new StackTrace(1, true).GetFrame(0);
                StockLog.WriteMethodEntry(callerType, sf, text);
            }
        }

        public MethodLogger(Type callerType, bool activated = false, string text = "")
        {
            isActive = activated;
            if (activated && StockLog.Logger.isMethodLoggingEnabled)
            {
                this.callerType = callerType;
                sf = new StackTrace(1, true).GetFrame(0);
                StockLog.WriteMethodEntry(callerType, sf, text);
            }
        }

        public void Dispose()
        {
            if (isActive && StockLog.Logger.isMethodLoggingEnabled)
            {
                StockLog.WriteMethodExit(callerType, sf);
            }
        }
    }

    internal class StockLogStream : IDisposable
    {
        private readonly StreamWriter sw;
        public StockLogStream(StreamWriter streamWriter)
        {
            sw = streamWriter;
        }

        public void Dispose()
        {
            if (sw != null)
                sw.Close();
        }

        public void WriteLine(string text)
        {
            if (sw != null)
            {
                sw.WriteLine(text);
            }
            else
            {
                Debug.WriteLine(text);
            }
        }
    }

    public class StockLog : IDisposable
    {
        public bool isEnabled = false;
        public bool isMethodLoggingEnabled = false;

        public bool isConsoleLogging = false;

        static private StockLog logger = null;
        static public StockLog Logger { get { logger ??= new StockLog(); return logger; } }

        private readonly StockLogStream sw;

        private StockLog()
        {
            isEnabled = Settings.Default.LoggingEnabled;
            isMethodLoggingEnabled = Settings.Default.LoggingEnabled;
            if (isEnabled)
            {
                if (!Debugger.IsAttached)
                {
                    string logFolder = Folders.Log;
                    if (Directory.Exists(logFolder))
                    {
                        try
                        {
                            foreach (string file in Directory.GetFiles(logFolder, "*.log"))
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
                    sw = new StockLogStream(new StreamWriter(fileName, false) { AutoFlush = true });
                }
                else
                {
                    sw = new StockLogStream(null);
                }
            }
        }

        static public void Write(string logText, bool isActive = true)
        {
            if (isActive && StockLog.Logger.isEnabled)
            {
                StackTrace st = new StackTrace(1, true);
                StackFrame sf = st.GetFrame(0);
                var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {sf.GetMethod().Name}";
                StockLog.Logger.sw.WriteLine($"{prefix}: {logText}");
            }
        }
        static public void WriteMethodEntry(Type type, StackFrame sf, string text)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {type.Name}::{sf.GetMethod().Name}";
                StockLog.Logger.sw.WriteLine($"{prefix}: Entry {text}");
            }
        }
        static public void WriteMethodExit(Type type, StackFrame sf)
        {
            if (StockLog.Logger.isEnabled && StockLog.Logger.isMethodLoggingEnabled)
            {
                var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {type.Name}::{sf.GetMethod().Name}";
                StockLog.Logger.sw.WriteLine($"{prefix}: Exit");
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
                var sw = StockLog.Logger.sw;
                if (objException.Source != null)
                {
                    sw.WriteLine("Source      : " + objException.Source.ToString().Trim());
                }
                if (objException.TargetSite != null)
                {
                    sw.WriteLine("Method      : " + objException.TargetSite.Name.ToString());
                }
                sw.WriteLine("Date        : " + DateTime.Now.ToLongTimeString());
                sw.WriteLine("Time        : " + DateTime.Now.ToShortDateString());
                sw.WriteLine("Thread        : " + Thread.CurrentThread.ManagedThreadId);
                sw.WriteLine("Error       : " + strException.Trim());
                if (objException.StackTrace != null)
                {
                    sw.WriteLine("Stack Trace : " +
                            objException.StackTrace.ToString().Trim());
                }
                sw.WriteLine("^^-------------------------------------------------------------------^^");
            }
        }

        public void Dispose()
        {
            sw.Dispose();
        }
    }
}
