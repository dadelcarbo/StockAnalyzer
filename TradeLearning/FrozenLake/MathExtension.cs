namespace FrozenLake;

public static class MathExtension
{
    public static int IndexOfMax(this double[] array)
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array is null or empty.");

        int maxIndex = 0;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > array[maxIndex])
            {
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    public static Random GetRandom(bool deterministic)
    {
        return deterministic ? new Random(0) : new Random();
    }

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

}
