using System;
using System.Xml.Serialization;

using StockAnalyzer.StockMath;


namespace StockAnalyzer.StockClasses
{
    public class StockIndicator: IComparable
    {
        public StockIndicatorType Type { get; set; }
        public bool IsActive { get; set; }
        public float Impact { get; set; }
        public StockMathToolkit.SmoothingType SmoothingType
        {
            get { return smoothingType; }
            set { smoothingType = value; SmoothingFunction = StockMathToolkit.GetSmoothingFunction(value); }
        }

        [XmlIgnore]
        public FloatPropertyRange Range { get; set; }
        [XmlIgnore]
        public StockMathToolkit.SmoothingFunction SmoothingFunction {get;private set;}

        private StockMathToolkit.SmoothingType smoothingType;

        public StockIndicator()
        {
        }
        public StockIndicator(StockIndicatorType type, bool active, float impact, StockMathToolkit.SmoothingType smoothingType)
        {
            this.Type = type;
            this.IsActive = active;
            this.Impact = impact;
            this.SmoothingType = smoothingType;
            this.Range = null;
        }
        public StockIndicator(StockIndicatorType type, bool active, float impact, StockMathToolkit.SmoothingType smoothingType, float min, float max, float step)
        {
            this.Type = type;
            this.IsActive = active;
            this.Impact = impact;
            this.SmoothingType = smoothingType;
            this.Range = new FloatPropertyRange(this.Type, min, max, step);
        }
        public StockIndicator Clone()
        {
            StockIndicator stockIndicator = (StockIndicator)this.MemberwiseClone();
            if (this.Range == null)
            {
                stockIndicator.Range = null;
            }
            else
            {
                stockIndicator.Range = this.Range.Clone();
            }
            return stockIndicator;
        }

        #region IComparable Members
        public int CompareTo(object obj)
        {
            float accuracy = 0.0001f;
            if (obj is StockIndicator)
            {
                StockIndicator other = (StockIndicator)obj;
                if (this.Type != other.Type || (this.IsActive ^ other.IsActive) ||this.SmoothingFunction != other.SmoothingFunction || Math.Abs(this.Impact - other.Impact) > accuracy)
                {
                    return -1;
                }
            }
            else
            {
                throw new System.ArgumentException("Cannot compare StockPersonality with objects of type " + obj.GetType().ToString());
            }
            return 0;
        }
        #endregion

        public override string ToString()
        {
            return Type.ToString() + " " + Impact + " " + this.SmoothingFunction.Method.Name;
        }
    }
}
