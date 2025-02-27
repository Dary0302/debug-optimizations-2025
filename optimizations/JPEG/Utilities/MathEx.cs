using System;

namespace JPEG.Utilities;

public static class MathEx
{
    public static double Sum(int from, int to, Func<int, double> function)
    {
        var sum = 0d;
        for (var i = from; i < to; i++)
        {
            sum += function(i);
        }
        return sum;
    }

    public static double SumByTwoVariables(int from1, int to1, int from2, int to2, Func<int, int, double> function)
    {
        var sum = 0d;
        for (var x = from1; x < to1; x++)
        {
            for (var y = from2; y < to2; y++)
            {
                sum += function(x, y);
            }
        }
        return sum;
    }

    public static void LoopByTwoVariables(int from1, int to1, int from2, int to2, Action<int, int> function)
    {
        for (var x = from1; x < to1; x++)
        {
            for (var y = from2; y < to2; y++)
            {
                function(x, y);
            }
        }
    }
}