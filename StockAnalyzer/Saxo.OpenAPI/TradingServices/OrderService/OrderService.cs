using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace Saxo.OpenAPI.TradingServices
{
    public class OrderService : BaseService
    {
        public OrderResponse PatchOrder(Account account, Instrument instrument, long orderId, string orderType, string buySell, int qty, decimal value)
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

        public OrderResponse BuyLimitOrder(Account account, Instrument instrument, int qty, decimal limitValue, decimal stopValue, bool T1)
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
            if (stopValue == 0)
            {
                return PostOrder(orderRequest);
            }

            if (!T1)
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
                return PostOrder(orderRequest);
            }
            else
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
                    },
                    new OrderRequest
                    {
                        AccountKey = account.AccountKey,
                        Uic = instrument.Identifier,
                        AssetType = instrument.AssetType,
                        Amount = qty,
                        BuySell =   "Sell",
                        OrderDuration = new OrderDuration { DurationType = OrderDurationType.GoodTillCancel.ToString() },
                        OrderPrice = limitValue + limitValue -  stopValue,
                        OrderType = SaxoOrderType.Limit.ToString(),
                        ManualOrder = false
                    }
                };
                return PostOrder(orderRequest);
            }
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

        public OrderResponse SellMarketOrder(Account account, Instrument instrument, int qty, bool cancelOrders)
        {
            var orderRequest = new OrderRequest
            {
                AccountKey = account.AccountKey,
                Uic = instrument.Identifier,
                AssetType = instrument.AssetType,
                OrderType = SaxoOrderType.Market.ToString(),
                BuySell = "Sell",
                Amount = qty,
                OrderDuration = new OrderDuration { DurationType = OrderDurationType.DayOrder.ToString() },
                CancelOrders = cancelOrders
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

        public bool CancelOrder(Account account, long orderId)
        {
            var res = this.Delete($"trade/v2/orders/{orderId}/?AccountKey={account.AccountKey}");
            return res.Contains(orderId.ToString());
        }

        private OrderResponse PostOrder(OrderRequest order)
        {
            return Post<OrderResponse>("trade/v2/orders", order);
        }
        private OrderResponse PostSlaveOrder(SlaveOrderRequest order)
        {
            return Post<OrderResponse>("trade/v2/orders", order);
        }

        /// <summary>
        /// Return all orders for account, or only for specified UIC
        /// </summary>
        /// <param name="account"></param>
        /// <param name="uic"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public OpenedOrder[] GetOpenedOrders(Account account, long uic = 0)
        {
            try
            {
                var res = Get<OpenedOrders>($"port/v1/orders/?ClientKey={account.ClientKey}&AccountKey={account.AccountKey}");
                if (uic == 0)
                {
                    return res?.Data?.Where(o => o.Uic == uic).ToArray();
                }
                else
                {
                    return res?.Data;
                }
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public List<OrderActivity> GetOrderActivities(Account account, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (fromDate.Year == 1 || fromDate.Date == toDate.Date)
                    return null;

                List<OrderActivity> orderActivities = new List<OrderActivity>();
                var timeSpan = new TimeSpan(60, 0, 0, 0);

                var startDate = fromDate;
                var upToDate = startDate + timeSpan;
                upToDate = upToDate < toDate ? upToDate : toDate;
                while (startDate < upToDate)
                {
                    var method = $"cs/v1/audit/orderactivities/?$top={10000}&$skiptoken={0}&ClientKey={account.ClientKey}&AccountKey={account.AccountKey}&FromDateTime={startDate.ToString("yyyy-MM-dd")}&ToDateTime={upToDate.ToString("yyyy-MM-dd")}";

                    var res = Get<OrderActivities>(method);
                    orderActivities.AddRange(res.Data);

                    startDate += timeSpan;
                    upToDate = startDate + timeSpan;
                    upToDate = upToDate < toDate ? upToDate : toDate;
                }
                return orderActivities;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }

        public OrderActivities GetOrderActivities(Account account, long orderId)
        {
            try
            {
                var method = $"cs/v1/audit/orderactivities/?$top={10000}&$skiptoken={0}&ClientKey={account.ClientKey}&AccountKey={account.AccountKey}&OrderId={orderId}";

                return Get<OrderActivities>(method);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
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
        public DateTime ActivityTime { get; set; }
        public float Amount { get; set; }
        public string AssetType { get; set; }
        public string BuySell { get; set; }
        public Duration Duration { get; set; }
        public long LogId { get; set; }
        public long OrderId { get; set; }
        public string OrderRelation { get; set; }
        public string OrderType { get; set; }
        public List<long> RelatedOrders { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public long Uic { get; set; }
        public float? Price { get; set; }
        public float? AveragePrice { get; set; }
        public float? ExecutionPrice { get; set; }
        public float? FillAmount { get; set; }
        public float? FilledAmount { get; set; }
        public long? PositionId { get; set; }
    }
    public class SaxoOrder
    {
        public DateTime CreationTime { get; set; }
        public DateTime ActivityTime { get; set; }
        public int Qty { get; set; }
        public string AssetType { get; set; }
        public string BuySell { get; set; }
        public Duration Duration { get; set; }
        public long LogId { get; set; }
        public long OrderId { get; set; }
        public string OrderRelation { get; set; }
        public string OrderType { get; set; }
        public List<long> RelatedOrders { get; set; }
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
        public string StockName { get; set; }
        public string Isin { get; set; }

        [JsonIgnore]
        public bool IsActive => !IsExecuted && (this.Status == "Working" || this.Status == "Placed" || this.Status == "DoneForDay") && this.SubStatus != "Rejected";

        [JsonIgnore]
        public bool IsExecuted => (this.Status == "FinalFill") && this.SubStatus != "Rejected";

        public SaxoOrder()
        {

        }
        public SaxoOrder(OrderActivity orderActivity)
        {
            this.CreationTime = orderActivity.ActivityTime;
            this.CopyFrom(orderActivity);
        }

        public SaxoOrder(OpenedOrder o)
        {
            this.CreationTime = o.OrderTime;
            this.ActivityTime = o.OrderTime;
            this.Qty = (int)o.Amount;
            this.AssetType = o.AssetType;
            this.BuySell = o.BuySell;
            this.Duration = o.Duration;
            this.OrderId = o.OrderId;
            this.OrderRelation = o.OrderRelation;
            this.OrderType = o.OpenOrderType;
            //this.RelatedOrders = o.RelatedOpenOrders;
            this.Status = o.Status;
            this.SubStatus = o.Status;
            this.Uic = o.Uic;
            this.Price = o.Price;
            this.PositionId = o.RelatedPositionId;
        }

        public void CopyFrom(OrderActivity orderActivity)
        {
            this.ActivityTime = orderActivity.ActivityTime;
            this.Qty = (int)orderActivity.Amount;
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
        public long OrderId { get; set; }
        public string OrderRelation { get; set; }
        public DateTime OrderTime { get; set; }
        public float Price { get; set; }
        public RelatedOpenOrder[] RelatedOpenOrders { get; set; }
        public long? RelatedPositionId { get; set; }
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
        public long OrderId { get; set; }
    }
}
