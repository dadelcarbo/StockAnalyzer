using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using System;
using System.Net.Http;

namespace StockAnalyzer.Saxo.OpenAPI.TradingServices
{
    public class ChartService : BaseService
    {
        private readonly InstrumentService instrumentService = new InstrumentService();
        private const int MAX_BAR_COUNT = 1200;
        public OHLC[] GetData(long uic, int horizon, DateTime? from = null)
        {
            if (!LoginService.IsConnected)
                return null;

            var saxoInstrument = instrumentService.GetInstrumentById(uic);
            if (saxoInstrument == null)
                return null;

            string method;
            if (from == null)
            {
                method = $"chart/v1/charts/?AssetType={saxoInstrument.AssetType}&Horizon={horizon}&Uic={saxoInstrument.Identifier}";
            }
            else
            {
                method = $"chart/v1/charts/?AssetType={saxoInstrument.AssetType}&Horizon={horizon}&Mode=From&Time={from}&Uic={saxoInstrument.Identifier}";
            }
            try
            {
                var data = Get<ChartData>(method);
                return data?.Data;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
        }
    }

    internal class ChartData
    {
        public OHLC[] Data { get; set; }
        public int DataVersion { get; set; }
    }
    public class OHLC
    {
        public float Close { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Open { get; set; }
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
    }
}
