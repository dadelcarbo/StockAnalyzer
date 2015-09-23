using System;
using System.Collections.Generic;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockLogging;
using System.IO;

namespace StockAnalyzer.StockClasses
{
    public class CotSerie : SortedDictionary<DateTime, CotValue>
    {
        public string CotSerieName { get; set; }
        public FloatSerie[] ValueSeries { get; set; }
        public bool IsInitialised { get; set; }

        public CotSerie(string cotSerieName)
        {
            this.CotSerieName = cotSerieName;
            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(CotValue.CotValueType)).Length];
            this.IsInitialised = false;
        }

        public FloatSerie GetSerie(CotValue.CotValueType cotType)
        {
            if (ValueSeries[(int)cotType] == null)
            {
                this.Initialise();
            }
            return ValueSeries[(int)cotType];
        }

        public bool Initialise()
        {
            if (this.IsInitialised)
            {
                return true;
            }
            try
            {
                foreach (CotValue.CotValueType cotType in Enum.GetValues(typeof(CotValue.CotValueType)))
                {
                    System.Reflection.PropertyInfo pi = typeof(CotValue).GetProperty(cotType.ToString());
                    FloatSerie cotSerie = new FloatSerie(this.Values.Count);
                    int i = 0;
                    foreach (CotValue cotValue in this.Values)
                    {
                        cotSerie[i] = (float)pi.GetValue(cotValue, null);
                        i++;
                    }
                    cotSerie.Name = cotType.ToString();
                    ValueSeries[(int)cotType] = cotSerie;
                }
                this.IsInitialised = true;
            }
            catch (System.Exception e)
            {
                StockLog.Write("Exception occured initialising the COTSerie: " + this.CotSerieName + " " + e.Message);
            }
            return this.IsInitialised;
        }
        public bool SaveToFile(string fileName)
        {
            bool res = false;
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
                {
                    sw.WriteLine(CotValue.StringFormat());
                    foreach (CotValue value in this.Values)
                    {
                        sw.WriteLine(value.ToString());
                    }
                    res = true;
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Exception occured saving the COTSerie: " + this.CotSerieName + " " + e.Message);
            }
            return res;
        }

        public static CotSerie ReadFromFile(string fileName, string serieName)
        {
            CotSerie cotSerie = new CotSerie(serieName);
            try
            {
                CotValue value;
                using (StreamReader sr = new StreamReader(fileName, false))
                {
                    sr.ReadLine(); // Skip header line
                    while (!sr.EndOfStream)
                    {
                        value = CotValue.Parse(sr.ReadLine());
                        cotSerie.Add(value.Date, value);
                    }
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Exception occured parsing the COTSerie: " + serieName + " " + e.Message);
            }
            return cotSerie;
        }
    }
}
