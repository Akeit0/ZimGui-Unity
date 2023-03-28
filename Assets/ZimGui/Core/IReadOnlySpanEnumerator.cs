using System;
using System.Collections.Generic;

namespace ZimGui.Core {
    public interface IReadOnlySpanEnumerator<T> {
        public ReadOnlySpan<T> Current { get; }
        public bool MoveNext();
    }

    public struct TextListEnumerator : IReadOnlySpanEnumerator<char> {
        public List<string>.Enumerator Enumerator;
        public ReadOnlySpan<char> Current => Enumerator.Current;
        public bool MoveNext() => Enumerator.MoveNext();
    } 
}