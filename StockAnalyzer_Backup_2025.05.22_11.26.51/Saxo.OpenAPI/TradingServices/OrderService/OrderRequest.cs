using System;

namespace Saxo.OpenAPI.TradingServices
{
    public enum SaxoOrderType
    {
        Algorithmic,            // Algo order.
        DealCapture,            // Deal Capture Order. Specify to capture trades, which are already registered on Exchange, into Saxo System. Currently supported for selected partners only.
        GuaranteedStop,         // Order Type currently not supported.
        Limit,                  // Limit Order.
        Market,                 // Market Order.
        Stop,                   // Stop Order.
        StopIfTraded,           // Stop if traded.
        StopLimit,              // Stop Limit Order.
        Switch,                 // Switch order, Sell X and Buy Y with one order.
        TrailingStop,           // Trailing stop.
        TrailingStopIfTraded,   // Trailing stop if traded.
        Traspaso,               // Traspaso. Specific type of switch order. Only available on select MutualFunds.
        TraspasoIn,             // TraspasoIn. Specific type of switch order
        TriggerBreakout,        // Trigger breakout order. Specific type for trigger orders.
        TriggerLimit,           // Trigger limit order. Specific type for trigger orders.
        TriggerStop,            // Trigger stop order. Specific type for trigger orders.
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
        public DateTime? ExpirationDateTime { get; set; }
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
        public decimal OrderPrice { get; set; }
        public OrderRequest[] Orders { get; set; }
        public bool ManualOrder { get; set; }
        public bool CancelOrders { get; set; }
    }

    public class SlaveOrderRequest
    {
        //public string OrderId { get; set; }
        public OrderRequest[] Orders { get; set; }
    }
}
