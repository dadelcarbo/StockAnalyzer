using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public enum AgendaEntryType
    {
        No = 0,
        CA = 1,
        Result = 2,
        Meeting = 4,
        Dividend = 8,
        Other = 16,
        All = 31
    }
    public class StockAgendaEntry
    {
        public StockAgendaEntry()
        {

        }
        public DateTime Date { get; set; }
        public string Event { get; set; }

        public string Comment { get; set; }
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

                if (Event.ToLower().Contains("dividende"))
                    return AgendaEntryType.Dividend;

                return AgendaEntryType.Other;
            }
        }

        public float? Value { get; set; }

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
        public void Add(DateTime date, string text, string comment)
        {
            var entry = new StockAgendaEntry { Date = date, Event = text, Comment = comment };
            Entries.Add(entry);
            if (entry.EntryType == AgendaEntryType.Dividend)
            {
                var div = comment.Replace("Montant : ", "");
                div = div.Substring(0, div.IndexOf("€"));
                float.TryParse(div, out float value);
                entry.Value = value;
            }
        }
        public void SortDescending()
        {
            this.Entries = this.Entries.OrderByDescending(e => e.Date).ToList();
        }
    }
}
