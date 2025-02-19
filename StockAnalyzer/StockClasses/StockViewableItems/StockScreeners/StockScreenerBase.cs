﻿using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Globalization;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public abstract class StockScreenerBase : IStockScreener
    {
        public StockScreenerBase()
        {
            if (this.ParameterDefaultValues != null)
            {
                this.parameters = (Object[])this.ParameterDefaultValues.Clone();
            }
        }
        public virtual string Name
        {
            get
            {
                string name = ShortName + "(";

                if (this.parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (this.parameters[i] is float)
                        {
                            name += ((float)this.parameters[i]).ToString(CultureInfo.GetCultureInfo("en-GB"));
                        }
                        else
                        {
                            name += this.parameters[i].ToString();
                        }
                        if (i + 1 < parameters.Length)
                        {
                            name += ",";
                        }
                    }
                }
                name += ")";
                return name;
            }
        }
        //abstract public string Name { get; }
        public virtual string Definition
        {
            get
            {
                string name = ShortName + "(";

                if (this.ParameterNames != null)
                {
                    for (int i = 0; i < this.ParameterNames.Length; i++)
                    {
                        name += this.ParameterDefaultValues[i].GetType().ToString().Replace("System.", "") + " " + this.ParameterNames[i];
                        if (i + 1 < parameters.Length)
                        {
                            name += ",";
                        }
                    }
                }
                name += ")";
                return name;
            }
        }
        public string ShortName => this.GetType().Name.Split('_')[1];
        public int ParameterCount => ParameterNames.Length;
        abstract public string[] ParameterNames { get; }


        private Type[] parameterTypes;
        public Type[] ParameterTypes
        {
            get
            {
                if (parameterTypes == null)
                {
                    parameterTypes = new Type[this.ParameterRanges.Length];
                    int i = 0;
                    foreach (var param in this.ParameterRanges)
                    {
                        parameterTypes[i++] = param.GetParamType();
                    }
                }
                return parameterTypes;
            }
        }

        abstract public Object[] ParameterDefaultValues { get; }
        protected Object[] parameters;
        virtual public Object[] Parameters { get { return parameters; } protected set { parameters = value; } }
        abstract public ParamRange[] ParameterRanges { get; }

        public void ParseInputParameters(string[] parameters)
        {
            using MethodLogger ml = new MethodLogger(this);
            // Parse input parameters
            if (parameters.Length < this.ParameterCount)
            {
                StockLog.Write("Invalid input parameter number: " + parameters.Length + " expected: " + this.ParameterCount);
                StockLog.Write("Using default parameters");
            }
            for (int i = 0; i < Math.Min(parameters.Length, this.ParameterTypes.Length); i++)
            {
                switch (this.ParameterTypes[i].Name)
                {
                    case "Int32":
                        int intParam;
                        if (int.TryParse(parameters[i], out intParam))
                        {
                            this.parameters[i] = intParam;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid input parameter: " + ParameterNames[i] + " type: " + this.ParameterTypes[i].ToString() + " expected");
                        }
                        break;
                    case "DateTime":
                        DateTime dateParam = DateTime.MinValue;
                        if (DateTime.TryParse(parameters[i], out dateParam))
                        {
                            this.parameters[i] = dateParam;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid input parameter: " + ParameterNames[i] + " type: " + this.ParameterTypes[i].ToString() + " expected");
                        }
                        break;
                    case "Single":
                        float floatParam;
                        if (float.TryParse(parameters[i], NumberStyles.Float, CultureInfo.GetCultureInfo("en-US"), out floatParam))
                        {
                            this.parameters[i] = floatParam;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid input parameter: " + ParameterNames[i] + " value: " + parameters[i] + " type: " + this.ParameterTypes[i].ToString() + " expected");
                        }
                        break;
                    case "Boolean":
                        bool boolParam;
                        if (bool.TryParse(parameters[i], out boolParam))
                        {
                            this.parameters[i] = boolParam;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid input parameter: " + ParameterNames[i] + " type: " + this.ParameterTypes[i].ToString() + " expected");
                        }
                        break;
                    case "String":
                        this.parameters[i] = parameters[i];
                        break;
                    case "StockSerie":
                        this.parameters[i] = parameters[i];
                        break;
                    default:
                        throw new NotImplementedException("This type is not yet implemented: " + this.ParameterTypes[i].ToString());
                }
            }
        }
        protected void CheckInputParameters(Object[] parameters)
        {
            // Parse input parameters
            if (parameters.Length != this.ParameterCount)
            {
                throw new ArgumentException("Invalid input parameter number: " + parameters.Length + " expected: " + this.ParameterCount);
            }
            for (int i = 0; i < this.ParameterCount; i++)
            {
                if (this.ParameterTypes[i] != parameters[i].GetType())
                {
                    throw new ArgumentException("Invalid input parameter: " + ParameterNames[i] + " type: " + this.ParameterTypes[i].ToString() + " expected");
                }
            }
        }

        protected void CreateEventSeries(int count)
        {
            this.Match = new BoolSerie(count, "Match");
        }

        public virtual bool RequiresVolumeData => false;


        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }

        abstract public void ApplyTo(StockSerie stockSerie);


        #region IStockScreener implementation
        public BoolSerie Match { get; set; }
        #endregion
    }
}
