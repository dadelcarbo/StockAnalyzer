namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    internal class SaxoProduct
    {
        public string ISIN { get; set; }
        public string StockName { get; set; }
        public string Type { get; set; }
        public double? Bid { get; set; }
        public double? Ask { get; set; }
        public double Leverage { get; set; }
        public double Ratio { get; set; }
        public double? Spread
        {
            get
            {
                if (Bid != null & Ask != null)
                {
                    return (Ask - Bid) / Bid;
                }
                return null;
            }
        }
    }
}