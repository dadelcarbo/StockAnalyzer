using System;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockDrawing
{
    public class StockDrawingItems : System.Collections.Generic.List<StockDrawing.DrawingItem>, IXmlSerializable
    {
        public DateTime? RefDate { get; set; }
        public int RefDateIndex { get; set; }
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            // Deserialize Daily Value
            if (reader.Name == ((Object)this).GetType().Name)
            {
                var refDateString = reader.GetAttribute("RefDate");
                if (refDateString != null)
                {
                    this.RefDate = DateTime.Parse(refDateString);
                    this.RefDateIndex = int.Parse(reader.GetAttribute("RefDateIndex"));
                }
                XmlSerializer serializer;
                reader.ReadStartElement(); // Start StockAnalyzer.StockDrawing.StockDrawingItems

                while (reader.Name.Contains("2D"))
                {
                    Type type = Type.GetType("StockAnalyzer.StockDrawing." + reader.Name);
                    if (type == null)
                    {
                        throw new System.Exception("Invalid type found in analysis file: " + reader.Name);
                    }
                    serializer = new XmlSerializer(Type.GetType("StockAnalyzer.StockDrawing." + reader.Name));
                    DrawingItem item = (DrawingItem)serializer.Deserialize(reader);
                    this.Add(item);
                }
                reader.ReadEndElement(); // End StockAnalyzer.StockDrawing.StockDrawingItemsC:\Perso\StockAnalyzer\StockAnalyzer\StockClasses\StockViewableItems\StockPaintBars\StockPaintBar_ADX.cs
            }
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (this.Count > 0)
            {
                if (RefDate != null)
                {
                    writer.WriteAttributeString("RefDate", this.RefDate.ToString());
                    writer.WriteAttributeString("RefDateIndex", this.RefDateIndex.ToString());
                }
                // Serialize Daily Value
                XmlSerializer serializer;
                foreach (StockDrawing.DrawingItem item in this.Where(d => d.IsPersistent))
                {
                    serializer = new XmlSerializer(item.GetType());
                    if (serializer == null)
                    {
                        throw new NotSupportedException(item.GetType().Name + " type is not supported in the serializer");
                    }
                    serializer.Serialize(writer, item);
                }
            }
        }

        public void ApplyDateOffset(DateTime[] dateSerie)
        {
            if (RefDate == null)
                return;
            if (RefDateIndex < dateSerie.Length && dateSerie[RefDateIndex] == RefDate)
                return;

            // Calculate offset
            int offset = Math.Min(RefDateIndex, dateSerie.Length);
            while (--offset > 0 && dateSerie[offset] != RefDate) ;
            if (offset == 0) // Ref date not found, drawing is too old compared to data history, clear drawings.
            {
                this.RefDate = null;
                this.RefDateIndex = 0;
                this.Clear();
                return;
            }
            foreach (var di in this)
            {
                di.ApplyOffset(offset - RefDateIndex);
            }
            RefDateIndex = offset;
        }
        #endregion
    }
}
