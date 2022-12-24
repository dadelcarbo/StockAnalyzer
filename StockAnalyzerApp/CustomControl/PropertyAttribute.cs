using System;

namespace StockAnalyzerApp.CustomControl
{
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string format = null, string group = null, int order = -1)
        {
            this.Format = format;
            this.Group = group;
            this.Order = order;
        }
        public string PropertyGroup { get; set; }
        public int Order { get; set; }
        public string Format { get; set; }
        public string Group { get; set; }
    }
}
