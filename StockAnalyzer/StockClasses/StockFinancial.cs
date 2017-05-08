using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public class StockFinancial
    {
        public DateTime DownloadDate { get; set; }
        public string Indices { get; set; }
        public string Sector { get; set; }
        [XmlIgnore]
        public long MarketCap { get { return (long)(this.ShareNumber * this.Value); } }
        public long ShareNumber { get; set; }
        [XmlIgnore]
        public float Value { get; set; }
        public string SRD { get; set; }
        public string PEA { get; set; }
        public string Coupon { get; set; }
        public float Dividend { get; set; }
        public float Yield { get { return this.Dividend / this.Value; } }

        public string Activity { get; set; }

        public List<List<string>> IncomeStatement { get; set; }
        [XmlIgnore]
        public DataTable IncomeStatementTable
        {
            get
            {
                DataTable table = new DataTable("IncomeStatementTable");
                TableFromStrings(table, IncomeStatement);
                return table;
            }
        }

        public List<List<string>> BalanceSheet { get; set; }
        [XmlIgnore]
        public DataTable BalanceSheetTable
        {
            get
            {
                DataTable table = new DataTable("BalanceSheetTable");
                TableFromStrings(table, BalanceSheet);
                return table;
            }
        }

        public List<List<string>> Quaterly { get; set; }
        [XmlIgnore]
        public DataTable QuaterlyTable
        {
            get
            {
                DataTable table = new DataTable("QuaterlyTable");
                TableFromStrings(table, Quaterly);
                return table;
            }
        }

        [XmlIgnore]
        public List<List<string>> Ratios { get; set; }
        [XmlIgnore]
        public DataTable RatiosTable
        {
            get
            {
                DataTable table = new DataTable("RatiosTable");
                TableFromStrings(table, Ratios);
                return table;
            }
        }

        private void TableFromStrings(DataTable table, List<List<string>> strings)
        {
            if (strings != null && strings.Count > 0)
            {
                // Create Column
                foreach (string header in strings[0])
                {
                    if (string.IsNullOrWhiteSpace(header))
                    {
                        table.Columns.Add("-");
                    }
                    else
                    {
                        table.Columns.Add(header.Replace(".", "-"));
                    }
                }
                foreach (var values in strings.Skip(1))
                {
                    var row = table.NewRow();
                    row.ItemArray = values.ToArray();
                    table.Rows.Add(row);
                }
            }
        }

        public void CalculateRatios()
        {
            if (this.IncomeStatement.Count <= 0) return;
            this.Ratios = new List<List<string>>();
            List<String> header = new List<string>() { "Ratios" };
            for (int i = 1; i < this.IncomeStatement[0].Count; i++)
            {
                header.Add(this.IncomeStatement[0][i]);
            }
            this.Ratios.Add(header);

            var resNet = this.IncomeStatement.FirstOrDefault(i => i.First() == "Résultat net");
            if (resNet != null)
            {
                List<String> resOpString = new List<string>();
                List<String> distribRatioString = new List<string>();
                List<String> PERString = new List<string>();
                this.Ratios.Add(resOpString);
                this.Ratios.Add(distribRatioString);
                this.Ratios.Add(PERString);
                resOpString.Add("EPS");
                distribRatioString.Add("Distrib Ratio");
                PERString.Add("PER");
                for (int i = 1; i < resNet.Count; i++)
                {
                    long resultat = resNet[i] == null ? 0 : long.Parse(resNet[i].Replace(" ", ""));
                    float eps = (1000.0f * resultat / (float)this.ShareNumber);
                    resOpString.Add(eps.ToString("0.##"));
                    float per = this.Value / eps;
                    PERString.Add(per.ToString("0.##"));
                    float distribRatio = this.Dividend / eps;
                    distribRatioString.Add(distribRatio.ToString("0.##"));
                }
            }
            var resOpLine = this.IncomeStatement.FirstOrDefault(i => i.First() == "Résultat opérationnel");
            if (resOpLine != null)
            {
                List<String> resOpCAString = new List<string>();
                this.Ratios.Add(resOpCAString);
                resOpCAString.Add("Cap/RO");
                for (int i = 1; i < resOpLine.Count; i++)
                {
                    long ca = resOpLine[i] == null ? 0 : long.Parse(resOpLine[i].Replace(" ", ""));
                    float caCap = (float)this.MarketCap / (1000.0f * ca);
                    resOpCAString.Add(caCap.ToString("0.##"));
                }
            }

            var caLine = this.IncomeStatement.FirstOrDefault(i => i.First() == "Chiffre d'affaires");
            if (caLine != null)
            {
                List<String> capCAString = new List<string>();
                this.Ratios.Add(capCAString);
                capCAString.Add("Cap/CA");
                for (int i = 1; i < caLine.Count; i++)
                {
                    long ca = caLine[i] == null ? 0 : long.Parse(caLine[i].Replace(" ", ""));
                    float caCap = (float)this.MarketCap / (1000.0f * ca);
                    capCAString.Add(caCap.ToString("0.##"));
                }
            }

            var actif = this.BalanceSheet.FirstOrDefault(i => i.First() == "Total actif");
            if (actif != null)
            {
                List<String> capActifString = new List<string>();
                this.Ratios.Add(capActifString);
                capActifString.Add("Cap/Actif");
                for (int i = 1; i < resNet.Count; i++)
                {
                    long totalActif = actif[i] == null ? 0 : long.Parse(actif[i].Replace(" ", ""));
                    float capActif = ((float)this.MarketCap / (float)(totalActif * 1000f));
                    capActifString.Add(capActif.ToString("0.##"));
                }
            }

            var cash = this.BalanceSheet.FirstOrDefault(i => i.First() == "Trésorerie et équivalents de trésorerie");
            if (cash != null)
            {
                List<String> capCashString = new List<string>();
                this.Ratios.Add(capCashString);
                capCashString.Add("Cap/Cash");
                for (int i = 1; i < resNet.Count; i++)
                {
                    long totalCash = cash[i] == null ? 0 : long.Parse(cash[i].Replace(" ", ""));
                    float capCash = ((float)this.MarketCap / (float)(totalCash * 1000f));
                    capCashString.Add(capCash.ToString("0.##"));
                }
            }
        }
    }
}
