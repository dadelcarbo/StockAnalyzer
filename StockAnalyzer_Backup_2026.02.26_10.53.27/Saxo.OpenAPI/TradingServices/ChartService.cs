using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Linq;
using System.Net.Http;

namespace StockAnalyzer.Saxo.OpenAPI.TradingServices
{
    public class ChartService : BaseService
    {
        private readonly InstrumentService instrumentService = new InstrumentService();
        private const int MAX_BAR_COUNT = 1200;
        public StockDailyValue[] GetData(long uic, BarDuration duration, DateTime? from = null)
        {
            if (!LoginService.IsConnected)
                return null;

            var instrument = instrumentService.GetInstrumentById(uic);
            if (instrument == null)
                return null;

            int horizon;
            switch (duration)
            {
                case BarDuration.Daily:
                    horizon = 1440;
                    break;
                case BarDuration.Weekly:
                    horizon = 10080;
                    break;
                case BarDuration.Monthly:
                    horizon = 43200;
                    break;
                case BarDuration.M_5:
                    horizon = 5;
                    break;
                case BarDuration.M_10:
                    horizon = 10;
                    break;
                case BarDuration.M_15:
                    horizon = 15;
                    break;
                case BarDuration.M_30:
                    horizon = 30;
                    break;
                case BarDuration.H_1:
                    horizon = 60;
                    break;
                case BarDuration.H_2:
                    horizon = 120;
                    break;
                case BarDuration.H_4:
                    horizon = 240;
                    break;
                default:
                    StockLog.Write($"Duration: {duration} is not supported in Saxo OpenAPI");
                    return null;
            }
            string method;
            if (from == null)
            {
                method = $"chart/v1/charts/?AssetType={instrument.AssetType}&Horizon={horizon}&Uic={instrument.Identifier}";
            }
            else
            {
                method = $"chart/v1/charts/?AssetType={instrument.AssetType}&Horizon={horizon}&Mode=From&Time={from}&Uic={instrument.Identifier}";
            }
            try
            {
                var data = Get<ChartData>(method);
                if (data?.Data != null && data?.Data.Length > 0)
                {
                    return data.Data.Select(ohlc => new StockDailyValue(ohlc.Open, ohlc.High, ohlc.Low, ohlc.Close, (long)ohlc.Volume, ohlc.Time)).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Error requesting data from the OpenApi: " + ex.Message, ex);
            }
            return null;
        }
    }

    internal class ChartData
    {
        public OHLC[] Data { get; set; }
        public int DataVersion { get; set; }
    }
    internal class OHLC
    {
        public float Close { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Open { get; set; }
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
    }
}
