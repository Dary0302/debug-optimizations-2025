using System;
using System.Linq;
using System.Collections.Generic;

namespace JPEG.Utilities;

static class IEnumerableExtensions
{
    public static T MinOrDefault<T>(this IEnumerable<T> enumerable, Func<T, int> selector)
    {
        return enumerable.MinBy(selector);
    }

    public static IEnumerable<T> Without<T>(this IEnumerable<T> enumerable, params T[] elements)
    {
        var elementsSet = new HashSet<T>(elements);
        return enumerable.Where(x => !elementsSet.Contains(x));
    }

    public static IEnumerable<T> ToEnumerable<T>(this T element)
    {
        yield return element;
    }
}