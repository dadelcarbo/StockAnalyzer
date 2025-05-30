﻿using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using System;
using System.Linq;

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

        virtual public Type GetParamType()
        {
            return MinValue?.GetType();
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
        public ParamRangeFloat()
           : base(float.MinValue, float.MaxValue)
        {
        }
        public ParamRangeFloat(float minValue, float maxValue)
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
    public class ParamRangeDateTime : ParamRange
    {
        public ParamRangeDateTime()
           : base(DateTime.MinValue, DateTime.MaxValue)
        {
        }
        public ParamRangeDateTime(Object minValue, Object maxValue)
           : base(minValue, maxValue)
        {
        }

        public override bool isInRange(Object value)
        {
            return (DateTime)value >= (DateTime)this.MinValue && (DateTime)value <= (DateTime)this.MaxValue;
        }

        public override bool isValidString(string valueString)
        {
            DateTime dateValue;
            if (!DateTime.TryParse(valueString, out dateValue))
            {
                return false;
            }
            return this.isInRange(dateValue);
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
    public class ParamRangeMA : ParamRange
    {
        public ParamRangeMA()
        {
            this.MinValue = String.Empty;
            this.MaxValue = String.Empty;
        }

        public override bool isInRange(Object value)
        {
            return true;
        }

        public override bool isValidString(string valueString)
        {
            return StockIndicatorMovingAvgBase.MaTypes.Contains(valueString.ToUpper());
        }
    }
    public class ParamRangeInput : ParamRange
    {
        public ParamRangeInput()
        {
            this.MinValue = String.Empty;
            this.MaxValue = String.Empty;
        }

        public override bool isInRange(Object value)
        {
            return true;
        }

        public override bool isValidString(string valueString)
        {
            return Enum.TryParse<InputType>(valueString.ToUpper(), out _);
        }
    }

    public class ParamRangeViewableItem : ParamRange
    {
        public ParamRangeViewableItem()
        {
            this.MinValue = String.Empty;
            this.MaxValue = String.Empty;
        }

        public override bool isInRange(Object value)
        {
            return StockViewableItemsManager.Supports(value.ToString().Replace(">", "|"));
        }

        public override bool isValidString(string valueString)
        {
            return StockViewableItemsManager.Supports(valueString.Replace(">", "|"));
        }

        override public Type GetParamType()
        {
            return typeof(string);
        }
    }
    public class ParamRangeIndicator : ParamRange
    {
        public ParamRangeIndicator()
        {
            this.MinValue = String.Empty;
            this.MaxValue = String.Empty;
        }

        public override bool isInRange(Object value)
        {
            return StockIndicatorManager.Supports(value.ToString());
        }

        public override bool isValidString(string valueString)
        {
            return StockIndicatorManager.Supports(valueString);
        }

        override public Type GetParamType()
        {
            return typeof(string);
        }
    }
    public class ParamRangeTrail : ParamRange
    {
        public ParamRangeTrail()
        {
            this.MinValue = String.Empty;
            this.MaxValue = String.Empty;
        }

        public override bool isInRange(Object value)
        {
            return StockTrailManager.Supports(value.ToString());
        }

        public override bool isValidString(string valueString)
        {
            return StockTrailManager.Supports(valueString);
        }

        override public Type GetParamType()
        {
            return typeof(string);
        }
    }
    public class ParamRangeStockName : ParamRange
    {
        public ParamRangeStockName()
        {
            this.MinValue = StockDictionary.Instance.Keys.First();
            this.MaxValue = StockDictionary.Instance.Keys.Last();
        }

        public override bool isInRange(Object value)
        {
            return StockDictionary.Instance.ContainsKey(value.ToString());
        }

        public override bool isValidString(string valueString)
        {
            return StockDictionary.Instance.ContainsKey(valueString);
        }

        override public Type GetParamType()
        {
            return typeof(string);
        }
    }
    public class ParamRangeString : ParamRange
    {
        public ParamRangeString()
        {
            this.MinValue = string.Empty;
            this.MaxValue = string.Empty;
        }
        string[] values;
        public ParamRangeString(string[] values)
        {
            this.values = values;
            this.MinValue = string.Empty;
            this.MaxValue = string.Empty;
        }

        public override bool isInRange(Object value)
        {
            if (value == null)
                return false;
            if (values != null)
            {
                return values.Contains(value.ToString());
            }
            return !string.IsNullOrWhiteSpace(value.ToString());
        }

        public override bool isValidString(string valueString)
        {
            return !string.IsNullOrWhiteSpace(valueString);
        }

        override public Type GetParamType()
        {
            return typeof(string);
        }
    }
}
