namespace Saxo.OpenAPI.TradingServices
{
    public class OrderResponse
    {
        public string OrderId { get; set; }
        public OrderResponse[] Orders { get; set; }
    }
}
