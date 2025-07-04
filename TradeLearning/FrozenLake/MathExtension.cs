namespace FrozenLake;

public static class MathExtension
{
    public static int ArgMax<T>(this T[] array) where T : IComparable<T>
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array is null or empty.");

        int maxIndex = 0;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i].CompareTo(array[maxIndex]) > 0)
            {
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    /// <summary>
    /// Nth element in descending order. Starts at n=0
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="rank">Nth element in descresing order. Starts at n=0</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T MaxRank<T>(this T[] array, int rank) where T : IComparable<T>
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array is null or empty.");

        if (array.Length <= rank)
            throw new ArgumentException("rank is small than array size.");

        var rankMax = array.Select((value, index) => new { value, index }).OrderByDescending(x => x.value).ElementAt(rank).value;

        return rankMax;
    }

    public static T Max<T>(this T[] array) where T : IComparable<T>
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array is null or empty.");

        T max = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i].CompareTo(max) > 0)
            {
                max = array[i];
            }
        }
        return max;
    }


    public static Random GetRandom(bool deterministic)
    {
        return deterministic ? new Random(0) : new Random();
    }

    /// <summary>
    /// Normalize a vector. Just ignore value equal to zero. (1,0,0,1) will give (.5,0,0,.5). Sum of values will be 1.
    /// </summary>
    /// <param name="values"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidProgramException"></exception>
    public static void NormalizeNonZero(this double[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Array cannot be null or empty.");

        double sum = 0.0;
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != 0)
            {
                count++;
                sum += values[i];
            }
        }

        for (int i = 0; count > 0 && i < values.Length; i++)
        {
            if (values[i] != 0)
            {
                values[i] /= sum;
            }
        }

        var check = Math.Abs(values.Sum() - 1);
        if (check > 0.001)
            throw new InvalidProgramException("Normalization failed");

    }
    /// <summary>
    /// Pseudo normalize of a vector, not using quadradic formula. Sum of values will be 1.
    /// </summary>
    /// <param name="values"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Normalize(this double[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Array cannot be null or empty.");

        double sum = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        for (int i = 0; i < values.Length; i++)
        {
            values[i] /= sum;
        }
    }

    public static void Softmax(this double[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Array cannot be null or empty.");

        double max = values[0];
        for (int i = 1; i < values.Length; i++)
            if (values[i] > max) max = values[i];

        double sumExp = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Math.Exp(values[i] - max);
            sumExp += values[i];
        }

        for (int i = 0; i < values.Length; i++)
        {
            values[i] /= sumExp;
        }
    }

    private static Random rand = GetRandom(true);
    public static void Shuffle<T>(this T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public static float[] ToFloatArray(this double[] array)
    {
        var res = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
            res[i] = (float)array[i];
        return res;
    }

}
