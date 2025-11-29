using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StockAnalyzer.StockHelpers
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        static ColorJsonConverter instance = new ColorJsonConverter();
        public static ColorJsonConverter Instance => instance;

        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Color.FromName(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Name);
        }
    }
    public class PenJsonConverter : JsonConverter<Pen>
    {
        public override Pen Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var color = JsonSerializer.Deserialize<Color>(root.GetProperty("Color").GetRawText(), options);
                var width = root.GetProperty("Width").GetSingle();
                var dashStyle = (DashStyle)root.GetProperty("DashStyle").GetInt32();

                return new Pen(color, width) { DashStyle = dashStyle };
            }
        }

        public override void Write(Utf8JsonWriter writer, Pen value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Color");
            JsonSerializer.Serialize(writer, value.Color, options);
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("DashStyle", (int)value.DashStyle);
            writer.WriteEndObject();
        }
    }

}
