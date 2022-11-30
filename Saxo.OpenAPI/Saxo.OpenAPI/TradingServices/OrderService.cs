using Saxo.OpenAPI.AuthenticationServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Saxo.OpenAPI.TradingServices
{
    public class OrderService : BaseService
    {
        public bool StartPosition(Instrument instrument, int qty, float buyStop, float stop, float limit)
        {
            var orderReq = new OrderRequest
            {

            };
            return true;
        }
        public long PostOrder(OrderRequest order)
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), "trade/v2/orders");
            try
            {
                var res = Post<OrderResponse>(url, order);
                return long.Parse(res.OrderId);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public dynamic GetOrder(long orderId, string clientKey)
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), $"port/v1/orders/{clientKey}/{orderId}");
            try
            {
                var res = Get<dynamic>(url);
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public dynamic GetOrders(string clientKey, string accountKey)
        {
            Uri url = new Uri(new Uri(LoginHelpers.App.OpenApiBaseUrl), $"port/v1/orders/?ClientKey={clientKey}&AccountKey={accountKey}");
            try
            {
                var res = Get<dynamic>(url);
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }

    public enum OrderType
    {
        Algorithmic, // Algo order.
        DealCapture, // Deal Capture Order. Specify to capture trades, which are already registered on Exchange, into Saxo System. Currently supported for selected partners only.
        GuaranteedStop, //  Order Type currently not supported.
        Limit, //   Limit Order.
        Market, //  Market Order.
        Stop, //    Stop Order.
        StopIfTraded, //    Stop if traded.
        StopLimit, //   Stop Limit Order.
        Switch, //  Switch order, Sell X and Buy Y with one order.
        TrailingStop, //    Trailing stop.
        TrailingStopIfTraded, //    Trailing stop if traded.
        Traspaso, //    Traspaso. Specific type of switch order. Only available on select MutualFunds.
        TraspasoIn, //  TraspasoIn. Specific type of switch order
        TriggerBreakout, // Trigger breakout order. Specific type for trigger orders.
        TriggerLimit, //    Trigger limit order. Specific type for trigger orders.
        TriggerStop, // Trigger stop order. Specific type for trigger orders.
    }

    public class OrderRequest
    {
        public string AccountKey { get; set; }
        public long Uic { get; set; }
        public string AssetType { get; set; }
        public string OrderType { get; set; }
        public string BuySell { get; set; }
        public int Amount { get; set; }
        public OrderDuration OrderDuration { get; set; }
        public string OrderRelation { get; set; }
        public float OrderPrice { get; set; }
        public Order[] Orders { get; set; }
        public bool ManualOrder { get; set; }
    }

    public class Order
    {
        public string AccountKey { get; set; }
        public int Uic { get; set; }
        public string AssetType { get; set; }
        public int Amount { get; set; }
        public string BuySell { get; set; }
        public OrderDuration OrderDuration { get; set; }
        public float OrderPrice { get; set; }
        public string OrderType { get; set; }
    }

    public class OrderResponse
    {
        public string OrderId { get; set; }
        public OrderResponse[] Orders { get; set; }
    }

    public enum OrderDurationType
    {
        AtTheClose, //  At the close of the trading session.
        AtTheOpening, // At the opening of the trading session.
        DayOrder, //   Day Order - Valid for the trading session.
        FillOrKill, // Fill or Kill order.
        GoodForPeriod, // Good for Period.
        GoodTillCancel, // Good till Cancel.
        GoodTillDate, // Good till Date - Expiration Date must also be specified.
        ImmediateOrCancel, // Immediate or Cancel Order.
    }
    public class OrderDuration
    {
        public string DurationType { get; set; }
    }

}
