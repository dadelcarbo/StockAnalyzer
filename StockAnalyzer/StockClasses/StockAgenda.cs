using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public class StockAgendaEntry
    {
        public StockAgendaEntry()
        {

        }
        public DateTime Date { get; set; }
        public string Event { get; set; }
    }

    public class StockAgenda
    {
        public StockAgenda()
        {
            this.Entries = new List<StockAgendaEntry>();
            DownloadDate = DateTime.MinValue;
        }
        public DateTime DownloadDate { get; set; }
        public List<StockAgendaEntry> Entries { get; set; }

        public bool ContainsKey(DateTime date)
        {
            return Entries.Any(e => e.Date == date);
        }
        public void Add(DateTime date, string text)
        {
            Entries.Add(new StockAgendaEntry { Date = date, Event = text });
        }
        public string this[DateTime date]
        {
            get
            {
                return this.Entries.First(e => e.Date == date).Event;
            }
            set
            {
                this.Entries.First(e => e.Date == date).Event = value;
            }
        }
    }
}
