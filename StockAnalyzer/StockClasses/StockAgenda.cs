using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public enum AgendaEntryType
    {
        No = 0,
        CA = 1,
        Result = 2,
        Meeting = 4,
        Other = 8,
        All = 31
    }
    public class StockAgendaEntry
    {
        public StockAgendaEntry()
        {

        }
        public DateTime Date { get; set; }
        public string Event { get; set; }
        [XmlIgnore]
        public AgendaEntryType EntryType
        {
            get
            {
                if (Event.ToLower().Contains("résultat"))
                    return AgendaEntryType.Result;

                if (Event.ToLower().Contains("affaire"))
                    return AgendaEntryType.CA;

                if (Event.ToLower().Contains("assemblée"))
                    return AgendaEntryType.Meeting;

                return AgendaEntryType.Other;
            }
        }

        public bool IsOfType(AgendaEntryType entryType)
        {
            return (this.EntryType & entryType) > 0;
        }
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
        public StockAgendaEntry this[DateTime date]
        {
            get
            {
                return this.Entries.First(e => e.Date == date);
            }
        }
    }
}
