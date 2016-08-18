using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

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


        static private string COT_ARCHIVE_SUBFOLDER = @"\data\archive\weekly\cot";
        public bool Initialise()
        {
            if (this.IsInitialised)
            {
                return true;
            }
            try
            {
                if (this.Count == 0)
                {
                    if (!this.LoadFromFile(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + COT_ARCHIVE_SUBFOLDER + @"\" + this.CotSerieName + ".csv"))
                    {
                        return false;
                    }
                }
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
            cotSerie.LoadFromFile(fileName);
            return cotSerie;
        }

        private bool LoadFromFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) return false;
                CotValue value;
                using (StreamReader sr = new StreamReader(fileName, false))
                {
                    sr.ReadLine(); // Skip header line
                    while (!sr.EndOfStream)
                    {
                        value = CotValue.Parse(sr.ReadLine());
                        this.Add(value.Date, value);
                    }
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Exception occured parsing the COTSerie: " + this.CotSerieName + " " + e.Message);
                return false;
            }
            return true;
        }
        public FloatSerie GetSerie(CotValue.CotValueType cotValueType, DateTime[] dateList)
        {
            System.Reflection.PropertyInfo pi = typeof(CotValue).GetProperty(cotValueType.ToString());

            this.Initialise();
            List<float> serie = new List<float>();
            int i = 0, j = 0;
            DateTime[] cotDates = this.Keys.ToArray();
            while (i < dateList.Length && dateList[i] < this.Keys.First())
            {
                serie.Add(0f);
                i++;
            }
            while (j < cotDates.Length && cotDates[j] < dateList[0])
            {
                j++;
            }
            float previousCOT = j > 0 ? (float)pi.GetValue(this.Values.ElementAt(j - 1), null) : (float)pi.GetValue(this.Values.First(), null);
            for (; i < dateList.Length; i++)
            {
                if (dateList[i] >= cotDates[j])
                {
                    CotValue cotValue = this[cotDates[j]];

                    serie.Add(previousCOT = (float)pi.GetValue(cotValue, null));
                    if (j < cotDates.Length - 1) j++;
                }
                else
                {
                    serie.Add(previousCOT);
                }
            }
            return new FloatSerie(serie.ToArray(), cotValueType.ToString());
        }
    }
}
