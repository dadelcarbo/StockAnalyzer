using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace UltimateChartist.Helpers;

public class MethodLogger : IDisposable
{
    private StackFrame sf;
    private Type callerType;
    private bool isActive = false;
    public MethodLogger(object caller, bool activated = false, string text = "")
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

    static public void Write(string logText, bool isActive = true)
    {
        if (isActive && Logger.isEnabled)
        {
            StackTrace st = new StackTrace(1, true);
            StackFrame sf = st.GetFrame(0);
            var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {sf.GetMethod().Name}";
            Logger.sw.WriteLine($"{prefix}: {logText}");
        }
    }
    static public void WriteMethodEntry(Type type, StackFrame sf, string text)
    {
        if (Logger.isEnabled && Logger.isMethodLoggingEnabled)
        {
            var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {type.Name}::{sf.GetMethod().Name}";
            Logger.sw.WriteLine($"{prefix}: Entry {text}");
        }
    }
    static public void WriteMethodExit(Type type, StackFrame sf)
    {
        if (Logger.isEnabled && Logger.isMethodLoggingEnabled)
        {
            var prefix = $"{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}): {DateTime.Now.TimeOfDay} (Thread:{Thread.CurrentThread.ManagedThreadId}) {type.Name}::{sf.GetMethod().Name}";
            Logger.sw.WriteLine($"{prefix}: Exit");
        }
    }

    static public void Write(Exception objException)
    {
        if (Logger.isEnabled)
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
            StreamWriter sw = Logger.sw;
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
