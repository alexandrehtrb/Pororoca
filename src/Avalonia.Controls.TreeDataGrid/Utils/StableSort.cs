// This source file is adapted from the dotnet runtime project.
// (https://github.com/dotnet/runtime)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Avalonia.Controls.Utils
{
    internal class StableSort
    {
        public static List<int> SortedMap<T>(IReadOnlyList<T> elements, Comparison<int> compare)
        {
            var map = new List<int>(elements.Count);
            for (var i = 0; i < elements.Count; i++)
            {
                map.Add(i);
            }

            var span = CollectionsMarshal.AsSpan(map);
            SortHelper<int>.Sort(span, compare);
            return map;
        }
    }
}
