using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace Saxo.OpenAPI.TradingServices
{
    public class OrderService : BaseService
    {
        public OrderResponse PatchOrder(Account account, Instrument instrument, string orderId, string orderType, string buySell, int qty, decimal value)
        {
            var patchOrder = new PatchOrder
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = orderType,
                BuySell = buySell,
                Amount = qty,
                OrderPrice = value,
                OrderId = orderId,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() }
            };
            return Patch<OrderResponse>("trade/v2/orders", patchOrder);
        }
        public OrderResponse BuyMarketOrder(Account account, Instrument instrument, int qty, decimal stopValue)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.Market.ToString(),
                BuySell = "Buy",
                Amount = qty,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.DayOrder.ToString() }
            };
            if (stopValue > 0)
            {
                orderRequest.Orders = new OrderRequest[]
                {
                    new OrderRequest
                    {
                        AccountKey = account.AccountKey,
                        Uic = instrument.Identifier,
                        AssetType = instrument.AssetType,
                        Amount = qty,
                        BuySell =   "Sell",
                        OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() },
                        OrderPrice = stopValue,
                        OrderType = SaxoOrderType.StopIfTraded.ToString(),
                        ManualOrder = false
                    }
                };
            }
            return PostOrder(orderRequest);
        }

        public OrderResponse BuyLimitOrder(Account account, Instrument instrument, int qty, decimal limitValue, decimal stopValue)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.Limit.ToString(),
                BuySell = "Buy",
                Amount = qty,
                OrderPrice = limitValue,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() }
            };
            if (stopValue > 0)
            {
                orderRequest.Orders = new OrderRequest[]
                {
                    new OrderRequest
                    {
                        AccountKey = account.AccountKey,
                        Uic = instrument.Identifier,
                        AssetType = instrument.AssetType,
                        Amount = qty,
                        BuySell =   "Sell",
                        OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() },
                        OrderPrice = stopValue,
                        OrderType = SaxoOrderType.StopIfTraded.ToString(),
                        ManualOrder = false
                    }
                };
            }
            return PostOrder(orderRequest);
        }

        public OrderResponse BuyTresholdOrder(Account account, Instrument instrument, int qty, decimal thresholdValue, decimal stopValue)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.StopIfTraded.ToString(),
                BuySell = "Buy",
                Amount = qty,
                OrderPrice = thresholdValue,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() }
            };
            if (stopValue > 0)
            {
                orderRequest.Orders = new OrderRequest[]
                {
                    new OrderRequest
                    {
                        AccountKey = account.AccountKey,
                        Uic = instrument.Identifier,
                        AssetType = instrument.AssetType,
                        Amount = qty,
                        BuySell =   "Sell",
                        OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() },
                        OrderPrice = stopValue,
                        OrderType = SaxoOrderType.StopIfTraded.ToString(),
                        ManualOrder = false
                    }
                };
            }
            return PostOrder(orderRequest);
        }

        public OrderResponse SellMarketOrder(Account account, Instrument instrument, int qty)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.Market.ToString(),
                BuySell = "Sell",
                Amount = qty,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.DayOrder.ToString() }
            };
            return PostOrder(orderRequest);
        }

        public OrderResponse SellLimitOrder(Account account, Instrument instrument, int qty, decimal limitValue)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.Limit.ToString(),
                BuySell = "Sell",
                Amount = qty,
                OrderPrice = limitValue,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() }
            };
            return PostOrder(orderRequest);
        }

        public OrderResponse SellStopOrder(Account account, Instrument instrument, int qty, decimal stopValue)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.StopIfTraded.ToString(),
                BuySell = "Sell",
                Amount = qty,
                OrderPrice = stopValue,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() }
            };
            return PostOrder(orderRequest);
        }

        public bool CancelOrder(Account account, string orderId)
        {
            var res = this.Delete($"trade/v2/orders/{orderId}/?AccountKey={account.AccountKey}");
            return res.Contains(orderId);
        }

        private OrderResponse PostOrder(OrderRequest order)
        {
            return Post<OrderResponse>("trade/v2/orders", order);
        }

        public dynamic GetOrder(long orderId, string clientKey)
        {
            try
            {
                var res = Get<dynamic>($"port/v1/orders/{clientKey}/{orderId}");
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public OpenedOrders GetOpenedOrders(Account account)
        {
            try
            {
                var res = Get<OpenedOrders>($"port/v1/orders/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}");
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
        public ClosedOrders GetClosedOrders(Account account, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (fromDate.Year == 1 || fromDate.Date == toDate.Date)
                    return null;
                var res = Get<ClosedOrders>($"cs/v1/reports/trades/{account.ClientKey}/?$top=1000&AccountKey={account.AccountKey}&FromDate={fromDate.ToString("yyyy-MM-dd")}&ToDate={toDate.ToString("yyyy-MM-dd")}");
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public OrderActivities GetOrderActivities(Account account, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (fromDate.Year == 1 || fromDate.Date == toDate.Date)
                    return null;

                var method = $"cs/v1/audit/orderactivities/?$top={10000}&$skiptoken={0}&ClientKey={account.ClientKey}&AccountKey={account.AccountKey}&FromDateTime={fromDate.ToString("yyyy-MM-dd")}&ToDateTime={toDate.ToString("yyyy-MM-dd")}";

                var res = Get<OrderActivities>(method);
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }

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

    ///////////////////////////////////////////// Opend Orders ////////////////////////////////////////////////////////

    public class OpenedOrders
    {
        public int __count { get; set; }
        public OpenedOrder[] Data { get; set; }
    }

    public class OrderActivities
    {
        public int __count { get; set; }
        public OrderActivity[] Data { get; set; }
    }
    public class OrderActivity
    {
        public DateTime CreationTime { get; set; }
        public DateTime ActivityTime { get; set; }
        public float Amount { get; set; }
        public string AssetType { get; set; }
        public string BuySell { get; set; }
        public Duration Duration { get; set; }
        public long LogId { get; set; }
        public long OrderId { get; set; }
        public string OrderRelation { get; set; }
        public string OrderType { get; set; }
        public List<string> RelatedOrders { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public long Uic { get; set; }
        public float? Price { get; set; }
        public float? AveragePrice { get; set; }
        public float? ExecutionPrice { get; set; }
        public float? FillAmount { get; set; }
        public float? FilledAmount { get; set; }
        public long? PositionId { get; set; }
        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }

        public void CopyFrom(OrderActivity orderActivity)
        {
            this.CreationTime = orderActivity.CreationTime;
            this.ActivityTime = orderActivity.ActivityTime;
            this.Amount = orderActivity.Amount;
            this.AssetType = orderActivity.AssetType;
            this.BuySell = orderActivity.BuySell;
            this.Duration = orderActivity.Duration;
            this.LogId = orderActivity.LogId;
            this.OrderId = orderActivity.OrderId;
            this.OrderRelation = orderActivity.OrderRelation;
            this.OrderType = orderActivity.OrderType;
            this.RelatedOrders = orderActivity.RelatedOrders;
            this.Status = orderActivity.Status;
            this.SubStatus = orderActivity.Status;
            this.Uic = orderActivity.Uic;
            if (orderActivity.Price.HasValue)
            {
                this.Price = orderActivity.Price;
            }
            this.AveragePrice = orderActivity.AveragePrice;
            this.ExecutionPrice = orderActivity.AveragePrice;
            this.FillAmount = orderActivity.FillAmount;
            this.FilledAmount = orderActivity.FillAmount;
            this.PositionId = orderActivity.PositionId;

            this.BarDuration = orderActivity.BarDuration;
            this.EntryComment = orderActivity.EntryComment;
            this.Theme = orderActivity.Theme;
        }



    }


    public class OpenedOrder
    {
        public string AccountId { get; set; }
        public string AccountKey { get; set; }
        public string AdviceNote { get; set; }
        public float Amount { get; set; }
        public float Ask { get; set; }
        public string AssetType { get; set; }
        public float Bid { get; set; }
        public string BuySell { get; set; }
        public string CalculationReliability { get; set; }
        public string ClientId { get; set; }
        public string ClientKey { get; set; }
        public string ClientName { get; set; }
        public string ClientNote { get; set; }
        public string CorrelationKey { get; set; }
        public float CurrentPrice { get; set; }
        public int CurrentPriceDelayMinutes { get; set; }
        public DateTime CurrentPriceLastTraded { get; set; }
        public string CurrentPriceType { get; set; }
        public float DistanceToMarket { get; set; }
        public Duration Duration { get; set; }
        public Exchange Exchange { get; set; }
        public float IpoSubscriptionFee { get; set; }
        public bool IsExtendedHoursEnabled { get; set; }
        public bool IsForceOpen { get; set; }
        public bool IsMarketOpen { get; set; }
        public float MarketPrice { get; set; }
        public string MarketState { get; set; }
        public float MarketValue { get; set; }
        public string NonTradableReason { get; set; }
        public string OpenOrderType { get; set; }
        public string OrderAmountType { get; set; }
        public string OrderId { get; set; }
        public string OrderRelation { get; set; }
        public DateTime OrderTime { get; set; }
        public float Price { get; set; }
        public RelatedOpenOrder[] RelatedOpenOrders { get; set; }
        public string RelatedPositionId { get; set; }
        public string Status { get; set; }
        public string TradingStatus { get; set; }
        public long Uic { get; set; }
    }

    public class Duration
    {
        public string DurationType { get; set; }
    }

    public class Exchange
    {
        public string Description { get; set; }
        public string ExchangeId { get; set; }
        public bool IsOpen { get; set; }
        public string TimeZoneId { get; set; }
    }

    public class RelatedOpenOrder
    {
        public float Amount { get; set; }
        public Duration Duration { get; set; }
        public string OpenOrderType { get; set; }
        public string OrderId { get; set; }
        public float OrderPrice { get; set; }
        public string Status { get; set; }
    }


    public class ClosedOrders
    {
        public int __count { get; set; }
        public ClosedOrder[] Data { get; set; }
    }

    public class ClosedOrder
    {
        public string AccountCurrency { get; set; }
        public int AccountCurrencyDecimals { get; set; }
        public string AccountId { get; set; }
        public string AdjustedTradeDate { get; set; }
        public float Amount { get; set; }
        public string AssetType { get; set; }
        public float Barrier1 { get; set; }
        public float Barrier2 { get; set; }
        public float BookedAmountAccountCurrency { get; set; }
        public float BookedAmountClientCurrency { get; set; }
        public float BookedAmountUSD { get; set; }
        public string ClientCurrency { get; set; }
        public string Direction { get; set; }
        public string ExchangeDescription { get; set; }
        public float FinancingLevel { get; set; }
        public string InstrumentCategoryCode { get; set; }
        public int InstrumentCurrencyDecimal { get; set; }
        public string InstrumentDescription { get; set; }
        public string InstrumentSymbol { get; set; }
        public string IssuerName { get; set; }
        public string OrderId { get; set; }
        public float Price { get; set; }
        public float ResidualValue { get; set; }
        public float SpreadCostAccountCurrency { get; set; }
        public float SpreadCostClientCurrency { get; set; }
        public float SpreadCostUSD { get; set; }
        public float StopLoss { get; set; }
        public float Strike { get; set; }
        public float Strike2 { get; set; }
        public string ToolId { get; set; }
        public string ToOpenOrClose { get; set; }
        public bool TradeBarrierEventStatus { get; set; }
        public string TradeDate { get; set; }
        public float TradedValue { get; set; }
        public string TradeEventType { get; set; }
        public DateTime TradeExecutionTime { get; set; }
        public string TradeId { get; set; }
        public string TradeType { get; set; }
        public long Uic { get; set; }
        public string UnderlyingInstrumentDescription { get; set; }
        public string UnderlyingInstrumentSymbol { get; set; }
        public string ValueDate { get; set; }
        public string Venue { get; set; }
    }

    ////////////////////////// Patch orders  ///////////////////////

    public class PatchOrder
    {
        public string AccountKey { get; set; }
        public long Uic { get; set; }
        public string AssetType { get; set; }
        public int Amount { get; set; }
        public string BuySell { get; set; }
        public OrderDuration OrderDuration { get; set; }
        public decimal OrderPrice { get; set; }
        public string OrderType { get; set; }
        public string OrderId { get; set; }
    }
}
