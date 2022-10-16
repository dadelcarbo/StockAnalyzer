using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockPortfolio
{
    public class SaxoOperation
    {
        public const string BUY = "bought";
        public const string SELL = "sold";
        public const string TRANSFER_IN = "transferin";
        public const string TRANSFER_OUT = "transferout";

        public long Id { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public string OperationType { get; set; }
        public int Qty { get; set; }
        public float Value { get; set; }
        public float NetAmount { get; set; }
        public float GrossAmount { get; set; }

        public string GetISIN()
        {
            int index = Instrument.LastIndexOf("ISIN:");
            if (index <= 0)
                return null;
            return Instrument.Substring(index + 6, 12);
        }
        public string GetStockName()
        {
            int index = Instrument.LastIndexOf(" (");
            if (index <= 0)
                return null;
            return Instrument.Substring(0, index);
        }

        public static List<SaxoOperation> LoadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;
            try
            {
                StockLog.Write("----------------------------------------------------------------------------");
                StockLog.Write($"Loading saxo report {fileName}");
                var connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0; data source={fileName};Extended Properties=""Excel 12.0"";";

                var adapter = new OleDbDataAdapter("SELECT * FROM [Opérations$]", connectionString);
                var ds = new DataSet();

                adapter.Fill(ds, "Opérations");

                var data = ds.Tables["Opérations"].AsEnumerable();
                if (data.Count() == 0 || data.First().ItemArray[0].GetType() == typeof(DBNull))
                    return null;

                return data.Select(r => new SaxoOperation
                {
                    Id = (long)r.Field<double>(0),
                    AccountId = r.Field<string>(1),
                    Instrument = r.Field<string>(2),
                    Date = DateTime.Parse(r.Field<string>(3).Replace('-', ' ')),
                    OperationType = r.Field<string>(4),
                    Qty = Math.Abs((int)r.Field<double>(6)),
                    Value = (float)r.Field<double>(7),
                    NetAmount = (float)r.Field<double>(8),
                    GrossAmount = (float)r.Field<double>(10)
                }).OrderBy(o => o.Id).Where(o => !o.Instrument.Contains(" Rights") && !o.Instrument.Contains("Gowex")).ToList();

            }
            catch (Exception e)
            {
                StockLog.Write(e);
                return null;
            }
        }
    }
}
