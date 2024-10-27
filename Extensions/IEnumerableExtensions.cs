using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Lib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var result = source.SelectMany(selector);
            if (!result.Any())
            {
                return result;
            }
            return result.Concat(result.SelectManyRecursive(selector));
        }

        public static string Join(this IEnumerable<string> source, string separator) => string.Join(separator, source);

        public static string Join(this IEnumerable<string> source, string separator, int startIndex, int count) => string.Join(separator, source.ToArray(), startIndex, count);

        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
                action(i);
        }

        public static string ToJsonString(this IEnumerable<string> list)
        {
            return "{" + string.Join(", ", list.Select(kvp => "\"" + kvp + "\"")) + "}";
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> arr)
        {
            if (arr.Count() > 1)
            {
                Random rnd = new Random(arr.Count() - 1);
                IEnumerable<T> randomizedOrder = arr.OrderBy(x => rnd.Next());

                return randomizedOrder;
            }

            return arr;
        }
    }
}
