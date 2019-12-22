using StockAnalyzer.StockPortfolio;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.Portofolio
{
    public class StockOrderList : System.Collections.Generic.List<StockOrder>, IXmlSerializable
    {
        public StockOrderList GetExecutedOrderListSortedByDate(string stockName)
        {
            StockOrderList stockOrderList = new StockOrderList();
            foreach (StockOrder stockOrder in this)
            {
                if (stockOrder.StockName == stockName && stockOrder.State == StockOrder.OrderStatus.Executed)
                {
                    stockOrderList.Add(stockOrder);
                }
            }
            stockOrderList.SortByDate();

            return stockOrderList;
        }
        public StockOrderList GetOrderListSortedByDate(string stockName)
        {
            StockOrderList stockOrderList = new StockOrderList();
            foreach (StockOrder stockOrder in this)
            {
                if (stockOrder.StockName == stockName)
                {
                    stockOrderList.Add(stockOrder);
                }
            }
            stockOrderList.SortByDate();

            return stockOrderList;
        }
        /// <summary>
        /// Returns a StockOrder summarizing the actual order on this stock. If no active order for this stock it return null
        /// </summary>
        /// <param name="stockName"></param>
        /// <returns></returns>
        public StockOrder GetActiveSummaryOrder(string stockName)
        {
            StockOrder cumulOrder = null;
            StockOrderList stockOrderList = GetExecutedOrderListSortedByDate(stockName);

            #region CLEAN UP ALL OBSOLETE SELL ORDER
            // Clean all invalid Sell order if any. An invalid sell order is a sell order before a buy order
            //foreach (StockOrder stockOrder in stockOrderList)
            //{
            //   if (stockOrder.IsBuyOrder())
            //   {
            //      break; // We found the buy order we stop looping
            //   }
            //   else
            //   {
            //      stockOrderList.Remove(stockOrder); // Just ignore it
            //   }
            //}
            #endregion

            // Loop to calculate the summary order list (A couple buy sell is created each time no value for this stock are detained)
            foreach (StockOrder stockOrder in stockOrderList)
            {
                if (cumulOrder == null)
                {
                    cumulOrder = stockOrder;
                }
                else
                {
                    cumulOrder = cumulOrder.Add(stockOrder);
                }
            }

            return cumulOrder;
        }
        /// <summary>
        /// Find a specific order according to its values
        /// </summary>
        /// <param name="stockName"></param>
        /// <param name="type"></param>
        /// <param name="executionDate"></param>
        /// <param name="number"></param>
        /// <param name="value"></param>
        /// <param name="fee"></param>
        /// <returns></returns>
        public StockOrder GetOrder(int id)
        {
            foreach (StockOrder stockOrder in this)
            {
                if (stockOrder.ID == id)
                {
                    return stockOrder;
                }
            }
            return null;
        }
        /// <summary> 
        /// </summary>
        /// <returns>Return a map string to number of active stocks</returns>
        public Dictionary<string, int> GetNbActiveStock()
        {
            Dictionary<string, int> stockCountDico = new Dictionary<string, int>();
            int nbStocks = 0;

            // Group the orders by stock name
            var orders = from order in this
                         where order.State == StockOrder.OrderStatus.Executed
                         orderby order.StockName
                         group order by order.StockName;
            //Initialise ListView
            foreach (var stockOrderGroup in orders)
            {
                nbStocks = 0;
                foreach (var stockOrder in stockOrderGroup)
                {
                    if (stockOrder.IsBuyOrder() ^ stockOrder.IsShortOrder)
                    {
                        nbStocks += stockOrder.Number;
                    }
                    else
                    {
                        nbStocks -= stockOrder.Number;
                    }
                }
                if (nbStocks != 0)
                {
                    stockCountDico.Add(stockOrderGroup.Key, nbStocks);
                }
            }
            return stockCountDico;
        }

        public void PurgeOrders(string stockName)
        {
            this.RemoveAll(delegate(StockOrder order) { return order.StockName == stockName; });

        }

        public void SortByDate()
        {
            this.Sort(delegate(StockOrder stockOrder1, StockOrder stockOrder2)
            {
                if (stockOrder1.ExecutionDate == stockOrder2.ExecutionDate)
                {
                    return stockOrder1.ID.CompareTo(stockOrder2.ID);
                }
                else
                {
                    return stockOrder1.ExecutionDate.CompareTo(stockOrder2.ExecutionDate);
                }
            });
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
            XmlSerializer serializer = new XmlSerializer(typeof(StockOrder));
            while (reader.Name == "StockOrder")
            {
                StockOrder order = (StockOrder)serializer.Deserialize(reader);
                this.Add(order);
            }
            reader.ReadEndElement(); // End StockOrderList
        }
        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            if (this.Count > 0)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StockOrder));
                foreach (StockOrder order in this)
                {
                    serializer.Serialize(writer, order);
                }
            }
        }
        #endregion
    }
}
