using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ZimGui.Core {
    public unsafe struct FixedCharMap<T>:IDisposable where T:unmanaged{
       
        [NativeDisableUnsafePtrRestriction]
        T* _values;
        public T* GetUnsafeValuesPtr => _values;
        [NativeDisableUnsafePtrRestriction]
        (ushort Key,short Next)* _entries;
        public (ushort Key,short Next)* GetUnsafeEntriesPtr => _entries;
        [NativeDisableUnsafePtrRestriction]
        short* _buckets;
        public short* GetUnsafeBucketsPtr => _buckets;
        public readonly int Count;
        public readonly int BucketsLength;
        public short DefaultIndex;
        short _count;
        public T Default => _values[DefaultIndex];
        public FixedCharMap(int count) {
            Count = count;
            BucketsLength = MathHelper.CeilPow2(count);
            _values = (T* )UnsafeUtility.Malloc(sizeof(T) * Count, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            _entries = ((ushort ,short )* )UnsafeUtility.Malloc(sizeof((ushort ,short )) * Count, UnsafeUtility.AlignOf<(ushort ,short )>(), Allocator.Persistent);
            (ushort ,short ) defaultEntry = (0, -1);
            UnsafeUtility.MemCpyReplicate(_entries, &defaultEntry, sizeof((ushort ,short )), Count);
            _buckets=(short* )UnsafeUtility.Malloc(sizeof(short) * BucketsLength, UnsafeUtility.AlignOf<short>(), Allocator.Persistent);
            short defaultTarget = -1;
            UnsafeUtility.MemCpyReplicate(_buckets, &defaultTarget, sizeof(short), BucketsLength);
            _count = 0;
            DefaultIndex = default;
        }

        public int ByteLength => 10 + Count * sizeof(T) + Count * 4 + 4 * BucketsLength;
        public void Serialize(Span<byte> span) {
            var offset = 0;
            span = span[..ByteLength];
            span.Write(ref offset,Count);
            span.Write(ref offset,BucketsLength);
            span.Write(ref offset,DefaultIndex);
            span.Write(ref offset,_values,Count);
            span.Write(ref offset,_entries,Count);
            span.Write(ref offset,_buckets,BucketsLength);
        }
        public void Serialize(Span<byte> span,ref int offset) { 
            span.Write(ref offset,Count);
            span.Write(ref offset,BucketsLength);
            span.Write(ref offset,DefaultIndex);
            span.Write(ref offset,_values,Count);
            span.Write(ref offset,_entries,Count);
            span.Write(ref offset,_buckets,BucketsLength);
        }
         public  FixedCharMap(ReadOnlySpan<byte> span) {
             var offset = 0;
             Count = span.ReadInt32(ref offset);
             BucketsLength = span.ReadInt32(ref offset);
             DefaultIndex = span.ReadInt16(ref offset);
             _values = (T* )UnsafeUtility.Malloc(sizeof(T) * Count, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
             span.WriteTo(ref offset,_values,Count);
             _entries = ((ushort ,short )* )UnsafeUtility.Malloc(sizeof((ushort ,short )) * Count, UnsafeUtility.AlignOf<(ushort ,short )>(), Allocator.Persistent);
             span.WriteTo(ref offset,_entries,Count);
             _buckets=(short* )UnsafeUtility.Malloc(sizeof(short) * BucketsLength, UnsafeUtility.AlignOf<short>(), Allocator.Persistent);
             span.WriteTo(ref offset,_buckets,BucketsLength);
             _count = (short)Count;
         }
        
        
        public bool IsCreated => _entries != null;
        public void Dispose() {
            UnsafeUtility.Free(_values,Allocator.Persistent);
            _values = null;
            UnsafeUtility.Free(_entries,Allocator.Persistent);
            _entries = null;
            UnsafeUtility.Free(_buckets,Allocator.Persistent);
            _buckets = null;
        }
       
        
        void Insert(ushort key, T value) {
            var targetBucket = key& (BucketsLength - 1);
            var i = _buckets[targetBucket];
            if(0<=i) {
                var current = _entries[i];
                if (current.Key == key) {
                    return;
                }
                while (0<= current.Next) {
                    current = _entries[current.Next];
                    if (current.Key == key) {
                        return;
                    }
                }
            }
            var index = _count;
            if (Count <= index) throw new Exception();
            _count++;
            if(0<=i)
                _entries[index].Next = i ;
            _entries[index].Key = key;
            _values[index] = value;
            _buckets[targetBucket] = index;
        }
        public void Add(ushort key, T value) {
            Insert(key, value);
        }
        public T this[ushort key] {
            get {
                var index = _buckets[key& (BucketsLength - 1)];
                var entries = _entries;
                if (index <0) return  _values[DefaultIndex];
                var current =   entries[index];
                if (current.Key == key) return _values[index];
                index = current.Next;
                while (0<= index) {
                    current =  entries[index];
                    if (current.Key == key) return  _values[index];
                    index = current.Next;
                }
                return  _values[DefaultIndex];
            }
            set => Insert(key, value);
        }

        public ref readonly T GetRef(ushort key) {
            var index = _buckets[key& (BucketsLength - 1)];
            var entries = _entries;
            if (index <0) return  ref _values[DefaultIndex];
            var current =  entries[index];
            if (current.Key == key) return ref _values[index];
             index = current.Next;
            while (0<= index) {
                current =  entries[index];
                if (current.Key == key) return ref _values[index];
                index = current.Next;
            }
            return ref _values[DefaultIndex];
        }
        public bool TryGetValue(ushort key, out T value) {
            var index = _buckets[key& (BucketsLength - 1)];
            if (index <0) {
                value = default;
                return false;
            }
            var entries = _entries;
            var current = entries[index];
            if (current.Key == key) {
                value= _values[index];
                return true;
            }
            index = current.Next;
            while (0<= index) {
                current =  entries[index];
                if (current.Key == key) {
                    value= _values[index];
                    return true;
                }
                index = current.Next;
            }
            value = default;
            return false;
        }
        public short GetEntryIndex(ushort key) {
            var index = _buckets[key& (BucketsLength - 1)];
            if (index <0) return DefaultIndex;
            var entries = _entries;
            var current = entries[index];
            if (current.Key == key) return index;
            index = current.Next;
            while (0<= index) {
                current = entries[index];
                if (current.Key == key) return index;
                index = current.Next;
            }
            return DefaultIndex;
        }
        public ref readonly T GetFromEntryIndex(int index)=>ref _values[index];
    }
}