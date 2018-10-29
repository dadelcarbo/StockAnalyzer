using System;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockMath
{
   public class FloatPropertyRange
   {
      public float Min { get; set; }
      public float Max { get; set; }
      public float Step { get; set; }
      public string Name { get; private set; }

      public FloatPropertyRange(string name, float min, float max, float step)
      {
         this.Min = min;
         this.Max = max;
         this.Step = step;
         this.Name = name;

         if (max < min)
         {
            throw new System.Exception("Max is lower than min, please check your input");
         }
      }
      public FloatPropertyRange(float min, float max)
      {
         this.Min = min;
         this.Max = max;
         this.Step = float.MaxValue;
      }

      public int NbStep()
      {
         return 1 + (int)Math.Ceiling((Max - Min) / Step);
      }
      public FloatPropertyRange Clone()
      {
         return (FloatPropertyRange)this.MemberwiseClone();
      }

   }
}
