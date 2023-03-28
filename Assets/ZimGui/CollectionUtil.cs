using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZimGui {
    public static class CollectionUtil {
        abstract class Dummy<T> {
            public T[] Items;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list) {
            return Unsafe.As<Dummy<T>>(list).Items.AsSpan(0, list.Count);
        }
    }

    

}