namespace StockAnalyzer.StockMath
{
   /// <summary>
   /// Represents a float range with minimum and maximum values.
   /// </summary>
   /// 
   /// <remarks>
   /// <para>The class represents a float range with inclusive limits -
   /// both minimum and maximum values of the range are included into it.
   /// Mathematical notation of such range is <b>[min, max]</b>.</para>
   /// 
   /// <para>Sample usage:</para>
   /// <code>
   /// // create [0.25, 1.5] range
   /// FloatRange range1 = new FloatRange( 0.25, 1.5 );
   /// // create [1.00, 2.25] range
   /// FloatRange range2 = new FloatRange( 1.00, 2.25 );
   /// // check if values is inside of the first range
   /// if ( range1.IsInside( 0.75 ) )
   /// {
   ///     // ...
   /// }
   /// // check if the second range is inside of the first range
   /// if ( range1.IsInside( range2 ) )
   /// {
   ///     // ...
   /// }
   /// // check if two ranges overlap
   /// if ( range1.IsOverlapping( range2 ) )
   /// {
   ///     // ...
   /// }
   /// </code>
   /// </remarks>
   /// 
   public class FloatRange
   {
      private float min, max;

      /// <summary>
      /// Minimum value of the range.
      /// </summary>
      /// 
      /// <remarks><para>The property represents minimum value (left side limit) or the range -
      /// [<b>min</b>, max].</para></remarks>
      /// 
      public float Min
      {
         get { return min; }
         set { min = value; }
      }

      /// <summary>
      /// Maximum value of the range.
      /// </summary>
      /// 
      /// <remarks><para>The property represents maximum value (right side limit) or the range -
      /// [min, <b>max</b>].</para></remarks>
      /// 
      public float Max
      {
         get { return max; }
         set { max = value; }
      }

      /// <summary>
      /// Length of the range (deffirence between maximum and minimum values).
      /// </summary>
      public float Length
      {
         get { return max - min; }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="FloatRange"/> class.
      /// </summary>
      /// 
      /// <param name="min">Minimum value of the range.</param>
      /// <param name="max">Maximum value of the range.</param>
      /// 
      public FloatRange(float min, float max)
      {
         this.min = min;
         this.max = max;
      }

      /// <summary>
      /// Check if the specified value is inside of the range.
      /// </summary>
      /// 
      /// <param name="x">Value to check.</param>
      /// 
      /// <returns><b>True</b> if the specified value is inside of the range or
      /// <b>false</b> otherwise.</returns>
      /// 
      public bool IsInside(float x)
      {
         return ((x >= min) && (x <= max));
      }

      /// <summary>
      /// Check if the specified range is inside of the range.
      /// </summary>
      /// 
      /// <param name="range">Range to check.</param>
      /// 
      /// <returns><b>True</b> if the specified range is inside of the range or
      /// <b>false</b> otherwise.</returns>
      /// 
      public bool IsInside(FloatRange range)
      {
         return ((IsInside(range.min)) && (IsInside(range.max)));
      }

      /// <summary>
      /// Check if the specified range overlaps with the range.
      /// </summary>
      /// 
      /// <param name="range">Range to check for overlapping.</param>
      /// 
      /// <returns><b>True</b> if the specified range overlaps with the range or
      /// <b>false</b> otherwise.</returns>
      /// 
      public bool IsOverlapping(FloatRange range)
      {
         return ((IsInside(range.min)) || (IsInside(range.max)));
      }
   }
}
