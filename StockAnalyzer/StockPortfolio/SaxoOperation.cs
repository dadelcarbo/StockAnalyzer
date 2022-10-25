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
                    Date = ParseSaxoDate( r.Field<string>(3)),
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

        static DateTime ParseSaxoDate(string dateString)
        {
            var fields = dateString.Split('-');
            var year = int.Parse(fields[2]);
            var day = int.Parse(fields[0]);
            int month;
            switch (fields[1])
            {
                case "janv.":
                    month = 1;
                    break;
                case "févr.":
                    month = 2;
                    break;
                case "mars":
                    month = 3;
                    break;
                case "avr.":
                    month = 4;
                    break;
                case "mai":
                    month = 5;
                    break;
                case "juin":
                    month = 6;
                    break;
                case "juil.":
                    month = 7;
                    break;
                case "août":
                    month = 8;
                    break;
                case "sept.":
                    month = 9;
                    break;
                case "oct.":
                    month = 10;
                    break;
                case "nov.":
                    month = 11;
                    break;
                case "déc.":
                    month = 12;
                    break;
                default:
                    throw new ArgumentException($"Month {fields[1]} not supported");
            }

            return new DateTime(year, month, day);

        }
    }
}
