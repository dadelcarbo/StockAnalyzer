using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses
{
   public class StockPersonality : IComparable, IXmlSerializable
   {
      public float BuyMargin { get; set; }
      public float SellMargin { get; set; }

      [XmlIgnore]
      public Dictionary<StockIndicatorType, StockIndicator> IndicatorDictionary { get; set; }

      static public StockPersonality StockPersonalityClipboard { get; set; }

      public StockPersonality Clone()
      {
         StockPersonality stockPersonality = new StockPersonality();
         stockPersonality.BuyMargin = this.BuyMargin;
         stockPersonality.SellMargin = this.SellMargin;
         foreach (StockIndicator indicator in this.IndicatorDictionary.Values)
         {
            stockPersonality.IndicatorDictionary.Add(indicator.Type, indicator.Clone());
         }
         return stockPersonality;
      }

      public void SetFloatPropertyValue(FloatPropertyRange range, float value)
      {
         if (range.IndicatorType == StockIndicatorType.NONE)
         {
            if (range.Name == "BuyMargin")
            {
               this.BuyMargin = value;
            }
            else
               if (range.Name == "SellMargin")
               {
                  this.SellMargin = value;
               }
               else
               {
                  throw new System.ArgumentException("Property " + range.Name + " doesn't exist in StockPersonality");
               }
         }
         else
         {
            this.IndicatorDictionary[range.IndicatorType].Impact = value;
         }
      }
      public StockPersonality()
      {
         this.IndicatorDictionary = new Dictionary<StockIndicatorType, StockIndicator>();
      }

      static public StockPersonality CreateDefaultPersonality()
      {
         StockPersonality newPersonality = new StockPersonality();

         newPersonality.BuyMargin = 0.035f;
         newPersonality.SellMargin = 0.035f;

         return newPersonality;
      }
      #region IXmlSerializable Members
      public System.Xml.Schema.XmlSchema GetSchema()
      {
         return null;
      }
      public void ReadXml(System.Xml.XmlReader reader)
      {
         this.BuyMargin = float.Parse(reader.GetAttribute("BuyMargin"), StockAnalyzerApp.Global.EnglishCulture);
         this.SellMargin = float.Parse(reader.GetAttribute("SellMargin"), StockAnalyzerApp.Global.EnglishCulture);

         // Deserialize StockIndicator
         XmlSerializer serializer = new XmlSerializer(typeof(StockIndicator));
         reader.ReadStartElement();
         bool hasIndicators = false;
         while (reader.Name == "StockIndicator")
         {
            StockIndicator stockIndicator = (StockIndicator)serializer.Deserialize(reader);
            this.IndicatorDictionary.Add(stockIndicator.Type, stockIndicator);
            hasIndicators = true;
         }
         if (hasIndicators)
         {
            reader.ReadEndElement();
         }
      }
      public void WriteXml(System.Xml.XmlWriter writer)
      {
         writer.WriteAttributeString("BuyMargin", this.BuyMargin.ToString(StockAnalyzerApp.Global.EnglishCulture));
         writer.WriteAttributeString("SellMargin", this.SellMargin.ToString(StockAnalyzerApp.Global.EnglishCulture));

         // Serialize StockIndicator
         XmlSerializer serializer = new XmlSerializer(typeof(StockIndicator));
         foreach (StockIndicator stockIndicator in this.IndicatorDictionary.Values)
         {
            serializer.Serialize(writer, stockIndicator);
         }
      }
      #endregion

      public void ActivateOnly(StockIndicatorType dataType)
      {
         foreach (StockIndicator stockIndicator in this.IndicatorDictionary.Values)
         {
            if (stockIndicator.Type == dataType)
            {
               stockIndicator.IsActive = true;
            }
            else
            {
               stockIndicator.IsActive = false;
            }
         }
      }
      public void ActivateAll(bool active)
      {
         foreach (StockIndicator stockIndicator in this.IndicatorDictionary.Values)
         {
            stockIndicator.IsActive = active;
         }
      }
      public void SetIndicatorImpact(StockIndicatorType dataType, float impact)
      {
         this.IndicatorDictionary[dataType].Impact = impact;
      }

      override public string ToString()
      {
         string personality = string.Empty;
         Type type = typeof(StockPersonality);
         System.Reflection.PropertyInfo[] propInfos = type.GetProperties();
         foreach (System.Reflection.PropertyInfo propInfo in propInfos)
         {
            if (propInfo.PropertyType == typeof(float))
            {
               personality += ((float)propInfo.GetValue(this, null)).ToString("P3") + ";";
            }
         }
         foreach (StockIndicator indicator in this.IndicatorDictionary.Values)
         {
            personality += indicator.Type.ToString() + ";" + indicator.IsActive.ToString() + ";" + indicator.Impact.ToString() + ";" + indicator.SmoothingType.ToString() + ";";
         }
         return personality;
      }
      public string ReportHeaderString()
      {
         string header = string.Empty;
         Type type = typeof(StockPersonality);
         System.Reflection.PropertyInfo[] propInfos = type.GetProperties();
         foreach (System.Reflection.PropertyInfo propInfo in propInfos)
         {
            if (propInfo.PropertyType == typeof(float))
            {
               header += propInfo.Name + ";";
            }
         }
         foreach (StockIndicator indicator in this.IndicatorDictionary.Values)
         {
            header += "Indicator;Active;Impact;Smoothing;";
         }
         return header;
      }

      #region IComparable Members
      public int CompareTo(object obj)
      {
         if (obj == null)
         {
            return -1;
         }
         StockPersonality other = (StockPersonality)obj;
         float accuracy = 0.000001f;
         if ((Math.Abs(this.BuyMargin - other.BuyMargin) < accuracy) &&
              (Math.Abs(this.SellMargin - other.SellMargin) < accuracy))
         {
            return CompareIndicators(other);
         }
         else
         {
            return -1;
         }
      }

      public int CompareIndicators(StockPersonality other)
      {
         if (this.IndicatorDictionary.Values.Count(i => i.IsActive) == other.IndicatorDictionary.Values.Count(i => i.IsActive))
         {
            foreach (StockIndicator indicator in this.IndicatorDictionary.Values)
            {
               if (other.IndicatorDictionary.Keys.Contains(indicator.Type))
               {
                  if (indicator.CompareTo(other.IndicatorDictionary[indicator.Type]) != 0)
                  {
                     return -1;
                  }
               }
               else
               {
                  return -1;
               }
            }
         }
         else
         {
            return -1;
         }
         return 0;
      }
      #endregion
   }
}
