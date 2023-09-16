using System;

namespace StockAnalyzer.StockMath
{
    public class StockMathToolkit
    {
        public enum SmoothingType
        {
            Identity,
            Sigmoid,
            Sigmoid2,
            Sigmoid3,
            Sigmoid4
        }
        public static SmoothingFunction GetSmoothingFunction(SmoothingType smoothingType)
        {
            switch (smoothingType)
            {
                case SmoothingType.Identity:
                    return new SmoothingFunction(Identity);
                case SmoothingType.Sigmoid:
                    return new SmoothingFunction(Sigmoid);
                case SmoothingType.Sigmoid2:
                    return new SmoothingFunction(Sigmoid2);
                case SmoothingType.Sigmoid3:
                    return new SmoothingFunction(Sigmoid3);
                case SmoothingType.Sigmoid4:
                    return new SmoothingFunction(Sigmoid4);
                default:
                    throw new System.Exception("Smoothing function not supported: " + smoothingType.ToString());
            }
        }
        public static SmoothingFunction GetSmoothingFunction(string smoothingTypeName)
        {
            SmoothingType smoothingType = (SmoothingType)Enum.Parse(typeof(SmoothingType), smoothingTypeName);
            return GetSmoothingFunction(smoothingType);
        }

        public delegate float SmoothingFunction(float val, float width, float scale);
        static public float Sigmoid(float val, float width, float scale)
        {
            float sigmoidCoef = 10.0f / width;
            float sigmoidValue = ((2.0f / (1.0f + (float)Math.Exp(-val * sigmoidCoef))) - 1.0f);
            if (sigmoidValue < -1.0f || sigmoidValue > 1.0f)
            {
                throw new System.ArithmeticException("Sigmoid value exceeds range:" + sigmoidValue.ToString());
            }
            return scale * sigmoidValue;
        }
        static public float Sigmoid2(float val, float width, float scale)
        {
            float sigmoidCoef = (float)Math.PI / width;
            float x = val * sigmoidCoef;
            float sigmoidValue = 0.0f;
            if (x >= 0)
            {
                sigmoidValue = (float)Math.Sqrt(Math.Sin(x));
            }
            else
            {
                sigmoidValue = -(float)Math.Sqrt(Math.Sin(-x));
            }
            if (sigmoidValue < -1.0f || sigmoidValue > 1.0f)
            {
                throw new System.ArithmeticException("Sigmoid value exceeds range:" + sigmoidValue.ToString());
            }
            return scale * sigmoidValue;
        }
        static public float Sigmoid3(float val, float width, float scale)
        {
            float sigmoidCoef = (float)Math.PI / width;
            float x = val * sigmoidCoef;
            float sigmoidValue = 0.0f;
            if (x < 0)
            {
                sigmoidValue = -(float)(Math.Pow(Math.Sin(-x), 1.5));
            }
            else
            {
                sigmoidValue = (float)(Math.Pow(Math.Sin(x), 1.5));
            }
            if (sigmoidValue < -1.0f || sigmoidValue > 1.0f)
            {
                throw new System.ArithmeticException("Sigmoid value exceeds range:" + sigmoidValue.ToString());
            }
            return scale * sigmoidValue;
        }
        static public float Sigmoid4(float val, float width, float scale)
        {
            float sigmoidCoef = (float)Math.PI / width;
            float x = val * sigmoidCoef;
            float sigmoidValue = 0.0f;

            sigmoidValue = (float)Math.Abs(scale * (Math.Cos(x)));

            return sigmoidValue;
        }
        static public float Identity(float val, float width, float scale)
        {
            return val * scale;
        }
    }
}
