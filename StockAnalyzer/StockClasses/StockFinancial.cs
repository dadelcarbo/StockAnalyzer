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
        public string Dividend { get; set; }
        public string Indices { get; set; }
        [XmlIgnore]
        public long MarketCap { get { return (long)(this.ShareNumber * this.Value); } }
        public string PEA { get; set; }
        public string Sector { get; set; }
        public long ShareNumber { get; set; }
        [XmlIgnore]
        public float Value { get; set; }
        public string SRD { get; set; }
        public float Yield { get; set; }

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
    }
}
