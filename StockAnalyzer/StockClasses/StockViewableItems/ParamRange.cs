using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
   public abstract class ParamRange
   {
      public Object MinValue { get; protected set; }
      public Object MaxValue { get; protected set; }

      public ParamRange()
      {
      }
      public ParamRange(Object minValue, Object maxValue)
      {
         this.MinValue = minValue;
         this.MaxValue = maxValue;
      }
      public abstract bool isValidString(string value);
      public abstract bool isInRange(Object valueString);

      public Type GetParamType()
      {
         return MinValue == null ? null : MinValue.GetType();
      }
   }

   public class ParamRangeInt : ParamRange
   {
      public ParamRangeInt(Object minValue, Object maxValue)
         : base(minValue, maxValue)
      {
      }
      public override bool isInRange(Object value)
      {
         return (int)value >= (int)this.MinValue && (int)value <= (int)this.MaxValue;
      }
      public override bool isValidString(string valueString)
      {
         int intValue;
         if (!int.TryParse(valueString, out intValue))
         {
            return false;
         }
         return this.isInRange(intValue);
      }
   }
   public class ParamRangeFloat : ParamRange
   {
      public ParamRangeFloat(Object minValue, Object maxValue)
         : base(minValue, maxValue)
      {
      }
      public override bool isInRange(Object value)
      {
         return (float)value >= (float)this.MinValue && (float)value <= (float)this.MaxValue;
      }
      public override bool isValidString(string valueString)
      {
         float floatValue;
         if (!float.TryParse(valueString, out floatValue))
         {
            return false;
         }
         return this.isInRange(floatValue);
      }
   }
   public class ParamRangeBool : ParamRange
   {
      public ParamRangeBool()
      {
         this.MinValue = false;
         this.MaxValue = true;
      }
      public override bool isInRange(Object value)
      {
         return true;
      }
      public override bool isValidString(string valueString)
      {
         bool boolValue;
         return bool.TryParse(valueString, out boolValue);
      }
   }
   public class ParamRangeStringList : ParamRange
   {
      private List<string> stringList;
      public ParamRangeStringList(List<string> list)
      {
         this.MinValue = String.Empty;
         this.MaxValue = String.Empty;
         this.stringList = list;
      }
      public override bool isInRange(Object value)
      {
         return true;
      }
      public override bool isValidString(string valueString)
      {
         return stringList.Contains(valueString.ToUpper());
      }
   }
}
