using System;

namespace UltimateChartist.Indicators
{
    public abstract class ParamRange
    {
        public object MinValue { get; protected set; }
        public object MaxValue { get; protected set; }

        public ParamRange()
        {
        }

        public ParamRange(object minValue, object maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public abstract bool isValidString(string value);
        public abstract bool isInRange(object valueString);

        virtual public Type GetParamType()
        {
            return MinValue == null ? null : MinValue.GetType();
        }
    }
    public class ParamRangeInt : ParamRange
    {
        public ParamRangeInt(object minValue, object maxValue)
           : base(minValue, maxValue)
        {
        }

        public override bool isInRange(object value)
        {
            return (int)value >= (int)MinValue && (int)value <= (int)MaxValue;
        }

        public override bool isValidString(string valueString)
        {
            int intValue;
            if (!int.TryParse(valueString, out intValue))
            {
                return false;
            }
            return isInRange(intValue);
        }
    }
    public class ParamRangeDouble : ParamRange
    {
        public ParamRangeDouble()
           : base(double.MinValue, double.MaxValue)
        {
        }
        public ParamRangeDouble(object minValue, object maxValue)
           : base(minValue, maxValue)
        {
        }

        public override bool isInRange(object value)
        {
            return (double)value >= (double)MinValue && (double)value <= (double)MaxValue;
        }

        public override bool isValidString(string valueString)
        {
            double doubleValue;
            if (!double.TryParse(valueString, out doubleValue))
            {
                return false;
            }
            return isInRange(doubleValue);
        }
    }
    public class ParamRangeDateTime : ParamRange
    {
        public ParamRangeDateTime()
           : base(DateTime.MinValue, DateTime.MaxValue)
        {
        }
        public ParamRangeDateTime(object minValue, object maxValue)
           : base(minValue, maxValue)
        {
        }

        public override bool isInRange(object value)
        {
            return (DateTime)value >= (DateTime)MinValue && (DateTime)value <= (DateTime)MaxValue;
        }

        public override bool isValidString(string valueString)
        {
            DateTime dateValue;
            if (!DateTime.TryParse(valueString, out dateValue))
            {
                return false;
            }
            return isInRange(dateValue);
        }
    }
    public class ParamRangeBool : ParamRange
    {
        public ParamRangeBool()
        {
            MinValue = false;
            MaxValue = true;
        }

        public override bool isInRange(object value)
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
            MinValue = string.Empty;
            MaxValue = string.Empty;
        }

        public override bool isInRange(object value)
        {
            return true;
        }

        public override bool isValidString(string valueString)
        {
            return MovingAverageBase.MaTypes.Contains(valueString.ToUpper());
        }
    }
}