using System;
using System.Collections.Generic;

namespace ZimGui.Text {
    public class RefDataMap<TValue> {
        public struct Entry {
            public int hashCode;
            public int next;
            public TValue value;
        }
        
        int[] buckets;
        Entry[] entries;
        int count;

        public int Count => count ;

        public TValue Default;

        public ReadOnlySpan<Entry> ReadEntries() => entries.AsSpan()[..count];

        public RefDataMap() {
            
        }
        public RefDataMap(int capacity) {
            Initialize(capacity);
        }

        void Initialize(int capacity) {
            int size = 1;
            while (size < capacity) {
                size <<= 1;
            }

            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
        }

        void Resize() {
            var newSize = count * 2;
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            for (int i = 0; i < count; i++) {
                int bucket = newEntries[i].hashCode & (newSize - 1);
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            buckets = newBuckets;
            entries = newEntries;
        }



        public void Clear() {
            if (count > 0) {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                Array.Clear(entries, 0, count);
                count = 0;
              
            }
        }

        public ref TValue this[string key] {
            get {
                if (key == null) {
                    throw new Exception();
                }
                if (buckets == null) Initialize(4);
                int hashCode = key.GetExHashCode();
                int targetBucket = hashCode & (buckets.Length - 1);
                var es = entries;
                for (int i = buckets[targetBucket]; i >= 0;) {
                    ref var entry = ref es[i];
                    if (entry.hashCode == hashCode) {
                        return ref entry.value;
                    }
                    i =entry.next;
                }
                if (count >= entries.Length*0.8f) {
                    Resize();
                    targetBucket = hashCode & (buckets.Length - 1);
                }
                var index = count;
                count++;
                
                entries[index].hashCode = hashCode;
                entries[index].next = buckets[targetBucket];
                entries[index].value = Default;
                buckets[targetBucket] = index;
                return ref  entries[index].value;
            }
        }
        
    }
}