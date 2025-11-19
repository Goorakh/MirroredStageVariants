using System;
using System.Collections;
using System.Collections.Generic;

namespace MirroredStageVariants.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            if (list.Capacity < capacity)
            {
                list.Capacity = capacity;
            }
        }

        public static T Take<T>(this IList<T> list, int index)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            T value = list[index];
            list.RemoveAt(index);
            return value;
        }

        public static object Take(this IList list, int index)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            object value = list[index];
            list.RemoveAt(index);
            return value;
        }
    }
}
