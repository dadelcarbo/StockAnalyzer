namespace StockAnalyzer.StockClasses.StockDataProviders.yCharts
{

    public class ychartJson
    {
        public Chart_Data[][] chart_data { get; set; }
    }

    public class Chart_Data
    {
        public decimal[][] raw_data { get; set; }
    }

}
