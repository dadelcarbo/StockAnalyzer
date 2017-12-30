using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Newtonsoft.Json;

namespace StockAnalyzer.StockClasses.StockDataProviders
{

    public partial class BarChartJSon
    {
        [JsonProperty("t")]
        public long[] T { get; set; }

        [JsonProperty("c")]
        public float[] C { get; set; }

        [JsonProperty("o")]
        public float[] O { get; set; }

        [JsonProperty("h")]
        public float[] H { get; set; }

        [JsonProperty("l")]
        public float[] L { get; set; }

        [JsonProperty("v")]
        public string[] V { get; set; }

        [JsonProperty("vo")]
        public string[] Vo { get; set; }

        [JsonProperty("s")]
        public string S { get; set; }

        public static BarChartJSon FromJson(string json) => JsonConvert.DeserializeObject<BarChartJSon>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this BarChartJSon self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
