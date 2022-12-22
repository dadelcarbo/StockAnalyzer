using System;

namespace StockAnalyzerApp.CustomControl
{
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string format = null)
        {
            this.Format = format;
        }
        public PropertyAttribute(string propertyGroup, int order)
        {
            this.PropertyGroup = propertyGroup;
            this.Order = order;
        }
        public string PropertyGroup { get; set; }
        public int Order { get; set; }
        public string Format { get; set; }
    }
}
