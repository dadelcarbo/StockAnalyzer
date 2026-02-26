using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    [Serializable]
    public class StockAnalysis : IXmlSerializable
    {
        public string Theme { get; set; }
        public bool Excluded { get; set; }
        public Dictionary<BarDuration, StockDrawingItems> DrawingItems { get; set; }

        public StockAnalysis()
        {
            this.DrawingItems = new Dictionary<BarDuration, StockDrawingItems>();
            this.Excluded = false;
            this.Theme = string.Empty;
        }
        public void Clear()
        {
            this.DrawingItems.Clear();
            this.Excluded = false;
            this.Theme = string.Empty;
        }
        public bool IsEmpty()
        {
            return this.Excluded == false
                && this.Theme == string.Empty
                && this.DrawingItems.Values.SelectMany(d => d, (d, dd) => dd).Where(dd => dd.IsPersistent).Count() == 0;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.Excluded = bool.Parse(reader.GetAttribute("Excluded"));
            this.Theme = reader.GetAttribute("Theme");

            bool hasDrawings = bool.Parse(reader.GetAttribute("HasDrawings"));

            reader.ReadStartElement(); // StockAnalysis 

            if (hasDrawings)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StockDrawingItems));
                while (reader.Name == "DrawingItems")
                {
                    BarDuration barDuration;
                    if (BarDuration.TryParse(reader.GetAttribute("BarDuration"), out barDuration))
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

            if (reader.Name == "StockAnalysis")
            {
                reader.ReadEndElement(); // StockAnalysis
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // Serialize Flat Attributes
            writer.WriteAttributeString("Excluded", this.Excluded.ToString());
            writer.WriteAttributeString("Theme", this.Theme);

            bool hasDrawings = this.DrawingItems.Count(d => d.Value.Count(item => item.IsPersistent) > 0) > 0;
            writer.WriteAttributeString("HasDrawings", hasDrawings.ToString());

            // Serialize drawing items
            if (hasDrawings)
            {
                foreach (KeyValuePair<BarDuration, StockDrawingItems> drawingItems in this.DrawingItems.Where(pair => pair.Value.Count(item => item.IsPersistent) > 0))
                {
                    writer.WriteStartElement("DrawingItems");
                    writer.WriteAttributeString("BarDuration", drawingItems.Key.ToString());

                    XmlSerializer serializer = new XmlSerializer(typeof(StockDrawingItems));
                    serializer.Serialize(writer, drawingItems.Value);

                    writer.WriteEndElement();
                }
            }
        }

        public int DeleteTransientDrawings()
        {
            int count = 0;
            if (!DrawingItem.KeepTransient)
            {
                foreach (var values in this.DrawingItems.Values)
                {
                    count = Math.Max(count, values.RemoveAll(d => !d.IsPersistent));

                }
            }
            return count;
        }
    }
}
