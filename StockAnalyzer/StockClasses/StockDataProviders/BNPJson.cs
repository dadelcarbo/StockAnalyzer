using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Newtonsoft.Json;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    /// <summary>
    /// Generated using https://app.quicktype.io/#r=json2csharp
    /// </summary>
    public partial class BNPJSon
    {
        [JsonProperty("previousDayClose")]
        public object PreviousDayClose { get; set; }

        [JsonProperty("series")]
        public Series[] Series { get; set; }

        [JsonProperty("minTickIntervals")]
        public long[] MinTickIntervals { get; set; }

        [JsonProperty("startDate")]
        public object StartDate { get; set; }

        [JsonProperty("endDate")]
        public object EndDate { get; set; }

        [JsonProperty("chartPeriod")]
        public string ChartPeriod { get; set; }
    }

    public partial class Series
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("type")]
        public string PurpleType { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public partial class BNPJSon
    {
        public static BNPJSon FromJson(string json) => JsonConvert.DeserializeObject<BNPJSon>(json, Converter.Settings);
    }

    public static partial class Serialize
    {
        public static string ToJson(this BNPJSon self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}