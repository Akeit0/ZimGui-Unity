using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ZimGui.Core {
    public static unsafe class NativeCollectionsExtensions {
        public static  NativeArray<T> AsArray<T>(ref this UnsafeList<T> list) where T:unmanaged {
            var array= NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
            return array;
        }
        public static  NativeArray<T> AsArray<T>(ref this UnsafeList<T> list,int startIndex,int length) where T:unmanaged {
            var array= NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(startIndex+list.Ptr,length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
            return array;
        }
         public static  NativeArray<T> CreateNativeArray<T>(T* ptr,int length) where T:unmanaged {
            var array= NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
            return array;
        }
public static  NativeArray<T> AsNativeArray<T>(this IntPtr ptr,int length) where T:unmanaged {
            var array= NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)ptr, length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
            return array;
        }


        public static void Swap(void* ptr,int length1, int length2) {
            var second = (byte*) ptr + length1;
            if (length1 == length2) {
                var temp= UnsafeUtility.Malloc(length1, UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
                UnsafeUtility.MemCpy(temp,ptr,length1);
                UnsafeUtility.MemCpy(ptr, second, length1);
                UnsafeUtility.MemCpy(second,temp, length1);
                UnsafeUtility.Free(temp,Allocator.Temp);
            }
            else if (length1 < length2) {
               var temp= UnsafeUtility.Malloc(length1, UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
               UnsafeUtility.MemCpy(temp,ptr,length1);
               UnsafeUtility.MemMove(ptr, second, length2);
               UnsafeUtility.MemCpy(second,temp, length1);
               UnsafeUtility.Free(temp,Allocator.Temp);
            }
            else {
                var temp= UnsafeUtility.Malloc(length2, UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
                UnsafeUtility.MemCpy(temp,second,length2);
                UnsafeUtility.MemMove((byte*) ptr + length2, ptr, length1);
                UnsafeUtility.MemCpy(ptr,temp, length2);
                UnsafeUtility.Free(temp,Allocator.Temp);
            }
        }public static void Swap<T>(T* ptr,int length1, int length2)where T:unmanaged  {
            var second =  ptr + length1;
            if (length1 == length2) {
                var byteLength1 = sizeof(T) * length1;
                var temp= (T*)UnsafeUtility.Malloc(byteLength1, UnsafeUtility.AlignOf<T>(), Allocator.Temp);
                UnsafeUtility.MemCpy(temp,ptr,byteLength1);
                UnsafeUtility.MemCpy(ptr, second, byteLength1);
                UnsafeUtility.MemCpy(second,temp, byteLength1);
                UnsafeUtility.Free(temp,Allocator.Temp);
            }
            else if (length1 < length2) {
                var byteLength1 = sizeof(T) * length1;
                var byteLength2 = sizeof(T) * length2;
               var temp= (T*)UnsafeUtility.Malloc(byteLength1, UnsafeUtility.AlignOf<T>(), Allocator.Temp);
               UnsafeUtility.MemCpy(temp,ptr,byteLength1);
               UnsafeUtility.MemMove(ptr, second, byteLength2);
               UnsafeUtility.MemCpy(ptr+length2,temp, byteLength1);
               UnsafeUtility.Free(temp,Allocator.Temp);
            }
            else {
                var byteLength1 = sizeof(T) * length1;
                var byteLength2 = sizeof(T) * length2;
                var temp= (T*)UnsafeUtility.Malloc(byteLength2, UnsafeUtility.AlignOf<T>(), Allocator.Temp);
                UnsafeUtility.MemCpy(temp,second,byteLength2);
                UnsafeUtility.MemMove( ptr + length2, ptr, byteLength1);
                UnsafeUtility.MemCpy(ptr,temp, byteLength2);
                UnsafeUtility.Free(temp,Allocator.Temp);
            }
        }
         public static void Insert<T>(ref this UnsafeList<T> list, int index, in T element) where T:unmanaged {
             list.Length += 1;
             var dest = list.Ptr + index;
             UnsafeUtility.MemMove(dest+1,dest,sizeof(T)* (list.Length - index));
             *dest = element;
         }
         public static ref T InsertRef<T>(ref this UnsafeList<T> list, int index) where T:unmanaged {
             list.Length += 1;
             var dest = list.Ptr + index;
             UnsafeUtility.MemMove(dest+1,dest,sizeof(T)* (list.Length-1 - index));
            return ref dest[0];
         }
        public static unsafe Span<T> AsSpan<T>(ref this UnsafeList<T> list) where T:unmanaged {
            return new Span<T>(list.Ptr, list.Length);
        }
        public static unsafe Span<T> AsSpan<T>(ref this NativeArray<T> list) where T:unmanaged {
            return new Span<T>(list.GetUnsafePtr(), list.Length);
        }
        public static ref T GetRef<T>(this NativeArray<T> array, int index)where T:unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe {
                return ref ((T*) array.GetUnsafePtr())[index];
            }
        }
    }
}