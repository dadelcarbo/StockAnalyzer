﻿using System;

namespace StockAnalyzerApp.CustomControl
{
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute()
        {

        }
        public PropertyAttribute(string propertyGroup, int order)
        {
            this.PropertyGroup = propertyGroup;
            this.Order = order;
        }
        public string PropertyGroup { get; set; }
        public int Order { get; set; }
    }
}
