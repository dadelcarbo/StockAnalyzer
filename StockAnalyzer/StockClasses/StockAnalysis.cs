using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses
{
    [Serializable]
    public class StockAnalysis : IXmlSerializable
    {
        public string Strategy { get; set; }
        public string Theme { get; set; }
        public bool Excluded { get; set; }
        public bool FollowUp { get; set; }
        public Dictionary<DateTime, String> Comments { get; set; }

        public Dictionary<StockSerie.StockBarDuration, StockDrawingItems> DrawingItems { get; set; }

        public StockAnalysis()
        {
            this.Comments = new Dictionary<DateTime, string>();
            this.DrawingItems = new Dictionary<StockSerie.StockBarDuration, StockDrawingItems>();
            this.Excluded = false;
            this.FollowUp = false;
            this.Theme = string.Empty;
        }
        public void Clear()
        {
            this.Comments = new Dictionary<DateTime, string>();
            this.DrawingItems.Clear();
            this.Excluded = false;
            this.FollowUp = false;
            this.Theme = string.Empty;
        }
        public bool IsEmpty()
        {
            return (this.Comments.Count == 0 || (this.Comments.Values.Count(c => !string.IsNullOrWhiteSpace(c)) == 0)) &&
                (this.DrawingItems.Count == 0 || (this.DrawingItems.Values.Count(d => d.Count > 0) == 0)) &&
                this.Excluded == false &&
                this.FollowUp == false &&
                this.Theme == string.Empty;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.Excluded = bool.Parse(reader.GetAttribute("Excluded"));
            this.FollowUp = bool.Parse(reader.GetAttribute("FollowUp"));
            this.Theme = reader.GetAttribute("Theme");

            bool hasDrawings = bool.Parse(reader.GetAttribute("HasDrawings"));
            bool hasComments = bool.Parse(reader.GetAttribute("HasComments"));

            reader.ReadStartElement(); // StockAnalysis 
            
            if (hasDrawings)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StockDrawingItems));
                while (reader.Name == "DrawingItems")
                {
                    StockSerie.StockBarDuration barDuration;
                    if (StockSerie.StockBarDuration.TryParse(reader.GetAttribute("BarDuration"), out barDuration))
                    {
                        reader.ReadStartElement();
                        this.DrawingItems.Add(barDuration, (StockDrawingItems)serializer.Deserialize(reader));
                        reader.ReadEndElement(); // End StockAnalysis
                    }
                    else
                    { // if bar duration name not recognized, just ignore the drawing
                        reader.ReadStartElement();
                        serializer.Deserialize(reader);
                        reader.ReadEndElement(); // End StockAnalysis
                    }
                }
            }

            if (hasComments)
            {
                reader.ReadStartElement(); // Comments
                while (reader.Name == "Comment")
                {

                    DateTime date = DateTime.Parse(reader.GetAttribute("Date"));
                    string comment = reader.GetAttribute("Value");
                    this.Comments.Add(date, comment);

                    reader.ReadStartElement(); // Comment
                }
                reader.ReadEndElement(); // Comments
            }
            if (reader.Name == "StockAnalysis")
            {
                reader.ReadEndElement(); // StockAnalysis
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Serialize Flat Attributes
            writer.WriteAttributeString("Excluded", this.Excluded.ToString());
            writer.WriteAttributeString("FollowUp", this.FollowUp.ToString());
            writer.WriteAttributeString("Theme", this.Theme);

            bool hasDrawings = this.DrawingItems.Count(d => d.Value.Count(item => item.IsPersistent) > 0) > 0;
            writer.WriteAttributeString("HasDrawings", hasDrawings.ToString());

            bool hasComments = this.Comments.Values.Count(c => !string.IsNullOrWhiteSpace(c)) > 0;
            writer.WriteAttributeString("HasComments", hasComments.ToString());

            // Serialize drawing items
            if (hasDrawings)
            {
                foreach (KeyValuePair<StockSerie.StockBarDuration, StockDrawingItems> drawingItems in this.DrawingItems.Where(pair => pair.Value.Count > 0))
                {
                    writer.WriteStartElement("DrawingItems");
                    writer.WriteAttributeString("BarDuration", drawingItems.Key.ToString());

                    XmlSerializer serializer = new XmlSerializer(typeof(StockDrawingItems));
                    serializer.Serialize(writer, drawingItems.Value);

                    writer.WriteEndElement();
                }
            }

            // Serialize Comment
            if (hasComments)
            {
                writer.WriteStartElement("Comments");
                foreach (KeyValuePair<DateTime, string> comment in this.Comments)
                {
                    writer.WriteStartElement("Comment");
                    writer.WriteAttributeString("Date", comment.Key.ToString());
                    writer.WriteAttributeString("Value", comment.Value.ToString());
                    writer.WriteEndElement(); // Comment
                }
                writer.WriteEndElement(); // Comments
            }
        }

        public int DeleteTransientDrawings()
        {
            int count = 0;
            foreach (StockSerie.StockBarDuration barDuration in this.DrawingItems.Keys)
            {
                count = Math.Max(count, this.DrawingItems[barDuration].RemoveAll(d => !d.IsPersistent));
            }
            return count;
        }
    }
}
