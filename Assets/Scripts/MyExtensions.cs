using System.Collections.Generic;

public static class MyExtensions
{
    public static void SetRangeValues<T>(this IList<T> source, int start, int end, T value)
    {
        if (start > 0 && end < source.Count)
        {
            for (int i = start; i <= end; i++)
            {
                source[i] = value;
            }
        }
    }

    public static T Pop<T>(this IList<T> source, int idx)
    {
        var ret = source[idx];
        source.RemoveAt(idx);
        return ret;
    }
}