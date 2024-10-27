using Common.Lib.HelperClasses;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Lib.Extensions
{
    /// <summary>Static extensions class that adds extension methods for <see cref="Array"/> type objects</summary>
    public static class ArrayExtensions
    {
        /// <summary>Performs the specified action on each element of the <see cref="Array"/></summary>
        /// <param name="array"></param>
        /// <param name="action">The <see cref="Action{T1, T2}"/> delegate to perform on each element of the <seealso cref="Array"/></param>
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }

        public static int CalculateLeastCommonMultiple(this IEnumerable<int> array)
        {
            return Tools.LeastCommonMultipleOfArray(array);
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
