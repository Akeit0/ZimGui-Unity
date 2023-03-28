using System;


namespace ZimGui.Demo {
    public class RingBuffer <T>{
        T[] _array;
        int _startIndex;

        public int Count { get; private set; }
        public T this[int index] => _array[( _startIndex + index ) % _array.Length];

        public int Capacity=> _array.Length;
        
        public RingBufferSlice Slice(int startIndex,int length) {
            return new RingBufferSlice(this,startIndex, length);
        }
        

        public RingBuffer( int capacity )
        {
            _array = new T[capacity];
        }

        public void Clear() {
            _array.AsSpan(0,Count).Clear();
            Count = 0;
            _startIndex = 0;
        }
        
        public void Add( T value )
        {
            if( Count < _array.Length )
                _array[Count++] = value;
            else
            {
                _array[_startIndex] = value;
                if( ++_startIndex >= _array.Length )
                    _startIndex = 0;
            }
        }
         public Enumerator GetEnumerator() => new Enumerator(this);
        
        public ref struct Enumerator {
            public readonly ReadOnlySpan<T> Span;
            public readonly int StartIndex;
             int _count;
             int _index;

           
            public Enumerator(RingBuffer <T> ringBuffer) {
                Span = ringBuffer._array.AsSpan(0,ringBuffer.Count);
                StartIndex = ringBuffer._startIndex;
                _count = 0;
                _index = 0;
            }
            public bool MoveNext() {
                if ((uint)_count < (uint)Span.Length) {
                    var i = (StartIndex + _count);
                    _index=i<Span.Length?i:i-Span.Length;                    
                    _count++;
                    return true;
                }
                return false;     
            }
            public T Current => Span[_index];
        }

        public readonly ref struct RingBufferSlice {
            public readonly ReadOnlySpan<T> Span;
            public readonly int StartIndex;
            public readonly int Length;

            public RingBufferSlice(RingBuffer <T> ringBuffer, int  start,int length) {
                Span = ringBuffer._array.AsSpan(0,ringBuffer.Count);
                var  i= ringBuffer._startIndex + start;
                StartIndex = i<Span.Length?i:i-Span.Length;   
                Length = length;
            }

            public SliceEnumerator GetEnumerator() => new SliceEnumerator(this);
            public ref struct SliceEnumerator {
                public readonly ReadOnlySpan<T> Span;
                public readonly int StartIndex;
                public readonly int Length;
                int _count;
                int _index;

                public SliceEnumerator(RingBufferSlice slice) {
                    Span = slice.Span;
                    StartIndex = slice.StartIndex;
                    Length = slice.Length;
                    _count = 0;
                    _index = 0;
                }
                public bool MoveNext() {
                    if (_count < Length) {
                        var i = (StartIndex + _count);
                        _index=i<Span.Length?i:i-Span.Length;        
                        _count++;
                        return true;
                    }
                    return false;     
                }
                public T Current => Span[_index];
            }
        }
    }
}