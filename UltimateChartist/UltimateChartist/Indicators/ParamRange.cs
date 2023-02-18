using System;
using System.Globalization;

namespace UltimateChartist.Indicators;

public interface IParamRange
{
    Type GetParamType();
    bool isValidString(string value);
    bool isInRange(object valueString);
    string ValueToString(CultureInfo cultureInfo);

    object MinValue { get; }
    object MaxValue { get; }
    object Value { get; }

}
public abstract class ParamRange : IParamRange
{
    public ParamRange(object minValue, object maxValue, object value)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Value = value;
    }
    public object MinValue { get; protected set; }
    public object MaxValue { get; protected set; }
    public object Value { get; protected set; }
    public abstract bool isValidString(string value);
    public abstract bool isInRange(object valueString);
    virtual public Type GetParamType()
    {
        return Value == null ? null : Value.GetType();
    }

    public abstract string ValueToString(CultureInfo cultureInfo);
}
public class ParamRangeInt : ParamRange
{
    public ParamRangeInt(int minValue, int maxValue, int value)
       : base(minValue, maxValue, value)
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
    public override string ValueToString(CultureInfo cultureInfo)
    {
        return ((int)Value).ToString(cultureInfo);
    }
}
public class ParamRangeDouble : ParamRange
{
    public ParamRangeDouble(double minValue, double maxValue, double value)
       : base(minValue, maxValue, value)
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
    public override string ValueToString(CultureInfo cultureInfo)
    {
        return ((double)Value).ToString(cultureInfo);
    }
}
public class ParamRangeDateTime : ParamRange
{
    public ParamRangeDateTime(DateTime minValue, DateTime maxValue, DateTime value)
       : base(minValue, maxValue, value)
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
    public override string ValueToString(CultureInfo cultureInfo)
    {
        return ((DateTime)Value).ToString(cultureInfo);
    }
}
public class ParamRangeBool : ParamRange
{
    public ParamRangeBool(bool value) : base(false, true, true)
    {
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
    public override string ValueToString(CultureInfo cultureInfo)
    {
        return ((bool)Value).ToString(cultureInfo);
    }
}
public class ParamRangeMA : ParamRange
{
    public ParamRangeMA(EmaType value) : base(EmaType.EMA, EmaType.MA, value)
    {
    }

    public override bool isInRange(object value)
    {
        return value is EmaType;
    }

    public override bool isValidString(string valueString)
    {
        return Enum.TryParse<EmaType>(valueString, out EmaType value);
    }
    public override string ValueToString(CultureInfo cultureInfo)
    {
        return Value.ToString();
    }
}