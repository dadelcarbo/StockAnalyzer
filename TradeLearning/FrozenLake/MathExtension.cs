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
}
