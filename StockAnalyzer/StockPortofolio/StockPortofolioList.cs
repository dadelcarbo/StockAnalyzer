using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Xml.Serialization;

namespace StockAnalyzer.Portofolio
{
   public class StockPortofolioList : System.Collections.Generic.List<StockPortofolio>, IXmlSerializable
   {
      public static StockPortofolioList Instance { get; set; }

      public StockPortofolioList()
      {
         Instance = this;
      }

      public List<string> GetStockNames()
      {
         List<string> stockNames = null;
         List<string> stockNames2 = null;
         foreach (StockPortofolio portofolio in this)
         {
            if (stockNames == null)
            {
               stockNames = portofolio.GetStockNames();
            }
            else
            {
               stockNames2 = portofolio.GetStockNames();
               foreach (string stockName in stockNames2)
               {
                  if (!stockNames.Contains(stockName))
                  {
                     stockNames.Add(stockName);
                  }
               }
            }
         }
         return stockNames;
      }

      public StockOrderList GetSortedByDateOrderList(string stockName)
      {
         StockOrderList stockOrderList = new StockOrderList();
         foreach (StockPortofolio portofolio in this)
         {
            stockOrderList.AddRange(portofolio.OrderList.GetExecutedOrderListSortedByDate(stockName));
         }
         stockOrderList.SortByDate();
         return stockOrderList;
      }
      public string[] GetPortofolioNames()
      {
         string[] names = new string[this.Count];
         int i = 0;
         foreach (StockPortofolio portofolio in this)
         {
            names[i++] = portofolio.Name;
         }
         return names;
      }
      public StockPortofolio Get(string name)
      {
         return this.Find(delegate(StockPortofolio p) { return p.Name == name; });
      }
      public void Remove(string name)
      {
         this.RemoveAll(delegate(StockPortofolio p) { return p.Name == name; });
      }

      #region IXmlSerializable Members
      System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
      {
         return null;
      }
      void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
      {
         // Deserialize Daily Value
         reader.ReadStartElement();
         XmlSerializer serializer = new XmlSerializer(typeof(StockPortofolio));
         while (reader.Name == "StockPortofolio")
         {
            StockPortofolio portofolio = (StockPortofolio)serializer.Deserialize(reader);
            if (!portofolio.IsSimulation)
            {
               this.Add(portofolio);
            }
         }
         if (!reader.EOF)
         {
            reader.ReadEndElement(); // End StockPortofolioList
         }
      }
      void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
      {
         if (this.Count > 0)
         {
            XmlSerializer serializer = new XmlSerializer(typeof(StockPortofolio));
            foreach (StockPortofolio portofolio in this)
            {
               if (!portofolio.IsSimulation)
               {
                  serializer.Serialize(writer, portofolio);
               }
            }
         }
      }
      #endregion
   }
}
