using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses.StockDataProviders.CNN
{

    public class FearAndGreedData
    {
        public Fear_And_Greed fear_and_greed { get; set; }
        public Fear_And_Greed_Historical fear_and_greed_historical { get; set; }
        public Market_Momentum_Sp500 market_momentum_sp500 { get; set; }
        public Market_Momentum_Sp125 market_momentum_sp125 { get; set; }
        public Stock_Price_Strength stock_price_strength { get; set; }
        public Stock_Price_Breadth stock_price_breadth { get; set; }
        public Put_Call_Options put_call_options { get; set; }
        public Market_Volatility_Vix market_volatility_vix { get; set; }
        public Market_Volatility_Vix_50 market_volatility_vix_50 { get; set; }
        public Junk_Bond_Demand junk_bond_demand { get; set; }
        public Safe_Haven_Demand safe_haven_demand { get; set; }
    }

    public class Fear_And_Greed
    {
        public float score { get; set; }
        public string rating { get; set; }
        public DateTime timestamp { get; set; }
        public float previous_close { get; set; }
        public float previous_1_week { get; set; }
        public float previous_1_month { get; set; }
        public float previous_1_year { get; set; }
    }

    public class Fear_And_Greed_Historical
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public long x { get; set; }
        public float y { get; set; }
        public string rating { get; set; }
    }

    public class Market_Momentum_Sp500
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Market_Momentum_Sp125
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Stock_Price_Strength
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Stock_Price_Breadth
    {
        public long timestamp { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Put_Call_Options
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Market_Volatility_Vix
    {
        public long timestamp { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Market_Volatility_Vix_50
    {
        public long timestamp { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Junk_Bond_Demand
    {
        public long timestamp { get; set; }
        public float score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }

    public class Safe_Haven_Demand
    {
        public long timestamp { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public Datum[] data { get; set; }
    }
}
