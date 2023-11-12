// This source file is adapted from the dotnet runtime project.
// (https://github.com/dotnet/runtime)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.Utils
{
    internal static class SortHelper<T>
    {
        public static void Sort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null, "Check the arguments in the caller!");

            // Add a try block here to detect bogus comparisons
            try
            {
                IntrospectiveSort(keys, comparer);
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException("Unable to sort because the IComparer.Compare() method returns inconsistent results." +
                    $"Either a value does not compare equal to itself, or one value repeatedly compared to another value yields different results.IComparer: '{comparer}'.");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to compare two elements in the array.", e);
            }
        }

        public static int BinarySearch(IReadOnlyList<T> items, T value, Comparison<T>? compare = null)
        {
            return BinarySearch(items, 0, items.Count, value, compare);
        }

        public static int BinarySearch(IReadOnlyList<T> items, int index, int length, T value, Comparison<T>? compare = null)
        {
            try
            {
                compare ??= Comparer<T>.Default.Compare;
                return InternalBinarySearch(items, index, length, value, compare);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to compare two elements in the array.", e);
            }
        }

        private static int InternalBinarySearch(IReadOnlyList<T> items, int index, int length, T value, Comparison<T> compare)
        {
            Debug.Assert(items != null, "Check the arguments in the caller!");
            Debug.Assert(index >= 0 && length >= 0 && (items.Count - index >= length), "Check the arguments in the caller!");

            var lo = index;
            var hi = index + length - 1;
            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);
                var order = compare(items[i], value);

                if (order == 0) return i;
                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static void SwapIfGreater(Span<T> keys, Comparison<T> comparer, int i, int j)
        {
            Debug.Assert(i != j);

            if (comparer(keys[i], keys[j]) > 0)
            {
                (keys[j], keys[i]) = (keys[i], keys[j]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<T> a, int i, int j)
        {
            Debug.Assert(i != j);

            (a[j], a[i]) = (a[i], a[j]);
        }

        private static void IntrospectiveSort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);

            if (keys.Length > 1)
            {
                IntroSort(keys, 2 * (BitOperations.Log2((uint)keys.Length) + 1), comparer);
            }
        }

        private static void IntroSort(Span<T> keys, int depthLimit, Comparison<T> comparer)
        {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(depthLimit >= 0);
            Debug.Assert(comparer != null);

            var partitionSize = keys.Length;
            while (partitionSize > 1)
            {
                if (partitionSize <= 16)
                {

                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        SwapIfGreater(keys, comparer, 0, 2);
                        SwapIfGreater(keys, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                var p = PickPivotAndPartition(keys.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys[(p + 1)..partitionSize], depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static int PickPivotAndPartition(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(keys.Length >= 16);
            Debug.Assert(comparer != null);

            var hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            var middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, 0, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            var pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer(keys[++left], pivot) < 0) ;
                while (comparer(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1)
            {
                Swap(keys, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!keys.IsEmpty);

            var n = keys.Length;
            for (var i = n >> 1; i >= 1; i--)
            {
                DownHeap(keys, i, n, comparer);
            }

            for (var i = n; i > 1; i--)
            {
                Swap(keys, 0, i - 1);
                DownHeap(keys, 1, i - 1, comparer);
            }
        }

        private static void DownHeap(Span<T> keys, int i, int n, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);

            var d = keys[i - 1];
            while (i <= n >> 1)
            {
                var child = 2 * i;
                if (child < n && comparer(keys[child - 1], keys[child]) < 0)
                {
                    child++;
                }

                if (!(comparer(d, keys[child - 1]) < 0))
                    break;

                keys[i - 1] = keys[child - 1];
                i = child;
            }

            keys[i - 1] = d;
        }

        private static void InsertionSort(Span<T> keys, Comparison<T> comparer)
        {
            for (var i = 0; i < keys.Length - 1; i++)
            {
                var t = keys[i + 1];

                var j = i;
                while (j >= 0 && comparer(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }

                keys[j + 1] = t;
            }
        }
    }
}
