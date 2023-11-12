using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal static class CollectionExtensions
    {
        public static readonly NotifyCollectionChangedEventArgs ResetEvent =
            new(NotifyCollectionChangedAction.Reset);

        public static int BinarySearch<TRow, TModel>(
            this IReadOnlyList<TRow> items,
            TModel model,
            Comparison<TModel> comparison,
            int from = 0,
            int to = -1)
                where TRow : IRow<TModel>
        {
            to = to == -1 ? items.Count - 1 : to;

            var lo = from;
            var hi = to;

            while (lo <= hi)
            {
                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint.
                //       Saves one subtraction per loop compared to
                //       `int i = lo + ((hi - lo) >> 1);`
                var i = (int)(((uint)hi + (uint)lo) >> 1);
                var c = comparison(model, items[i].Model);
                if (c == 0)
                    return i;
                else if (c > 0)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            // If none found, then a negative number that is the bitwise complement
            // of the index of the next element that is larger than or, if there is
            // no larger element, the bitwise complement of `length`, which
            // is `lo` at this point.
            return ~lo;
        }

        public static void InsertMany<T>(this List<T> list, int index, T item, int count)
        {
            var repeat = FastRepeat<T>.Instance;
            repeat.Count = count;
            repeat.Item = item;
            list.InsertRange(index, FastRepeat<T>.Instance);
            repeat.Item = default;
        }

        public static T[] Slice<T>(this List<T> list, int index, int count)
        {
            var result = new T[count];
            list.CopyTo(index, result, 0, count);
            return result;
        }

        private class FastRepeat<T> : ICollection<T>
        {
            public static readonly FastRepeat<T> Instance = new();
            public int Count { get; set; }
            public bool IsReadOnly => true;
            [AllowNull] public T Item { get; set; }
            public void Add(T item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(T item) => throw new NotImplementedException();
            public bool Remove(T item) => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

            public void CopyTo(T[] array, int arrayIndex)
            {
                var end = arrayIndex + Count;

                for (var i = arrayIndex; i < end; ++i)
                {
                    array[i] = Item;
                }
            }
        }
    }
}
