using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace ZimGui.Core {
    public static class FormatEx {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Format(this float f,ref int offset, Span<char> span) {
            f.TryFormat(span[offset..], out var charsWritten,IMStyle.FloatFormat);
            offset += charsWritten;
        }
        public static void Format(this int i,ref int offset, Span<char> span) {
            i.TryFormat(span[offset..], out var charsWritten);
            offset += charsWritten;
        }
        public static void Format(this byte i,ref int offset, Span<char> span) {
            if (99 < i) {
                var hundredsPlace = i / 100;
                var tensPlace = (i - hundredsPlace * 100)/10;
                var onesPlace = (i - hundredsPlace * 100 - tensPlace * 10);
                span[offset++] = (char)('0' + hundredsPlace);
                span[offset++] = (char)('0' + tensPlace);
                span[offset++] = (char)('0' + onesPlace);
            }
            else if(9 < i) {
                var tensPlace = i/10;
                var onesPlace = i -  tensPlace * 10;
                span[offset++] = (char)('0' + tensPlace);
                span[offset++] = (char)('0' + onesPlace);
            }
            else span[offset++] = (char)('0' + i);
        }public static int Format(this byte i, Span<char> span) {
            if (99 < i) {
                var hundredsPlace = i / 100;
                var tensPlace = (i - hundredsPlace * 100)/10;
                var onesPlace = (i - hundredsPlace * 100 - tensPlace * 10);
                span[0] = (char)('0' + hundredsPlace);
                span[1] = (char)('0' + tensPlace);
                span[2] = (char)('0' + onesPlace);
                return 3;
            }
            if(9 < i) {
                var tensPlace = i/10;
                var onesPlace = i -  tensPlace * 10;
                span[0] = (char)('0' + tensPlace);
                span[1] = (char)('0' + onesPlace);
                return 2;
            }
            span[0] = (char)('0' + i);
            return 1;

        }
        
        public static void Write(this Span<byte> span,ref int offset, ushort value) {
            UnsafeUtility.As<byte, ushort>(ref span[offset])=value;
            offset += 2;
        }
        public static void Write(this Span<byte> span,ref int offset, short value) {
            UnsafeUtility.As<byte, short>(ref span[offset])=value;
            offset += 2;
        }
        public static  void Write(this Span<byte> span,ref int offset, int value) {
            UnsafeUtility.As<byte, int>(ref span[offset])=value;
            offset += 4;
        }
        public static void Write(this Span<byte> span,ref int offset, float value) {
            UnsafeUtility.As<byte, float>(ref span[offset])=value;
            offset += 4;
        }
        
        public static void Write(this Span<byte> span,ref int offset, ReadOnlySpan<byte> value) {
            value.CopyTo(span.Slice(offset,value.Length));
            offset += value.Length;
        }
        public static  void Write<T>(this Span<byte> span,ref int offset, ReadOnlySpan<T> value) where T:unmanaged {
            var valueAsBytes = MemoryMarshal.Cast<T, byte>(value);
            valueAsBytes.CopyTo(span.Slice(offset,valueAsBytes.Length));
            offset += valueAsBytes.Length;
        }
        public static unsafe void Write<T>(this Span<byte> span,ref int offset, T* ptr,int length) where T:unmanaged {
            var valueAsBytes = new ReadOnlySpan<byte>((byte*)ptr,length*sizeof(T));
            valueAsBytes.CopyTo(span.Slice(offset,valueAsBytes.Length));
            offset += valueAsBytes.Length;
        }
        public static ushort ReadUInt16(this ReadOnlySpan<byte> span,ref int offset) {
            return MemoryMarshal.Cast<byte, ushort>(span[offset..(offset += 4)])[0];
        }
        public static short ReadInt16(this ReadOnlySpan<byte> span,ref int offset) {
            return MemoryMarshal.Cast<byte, short>(span[offset..(offset += 2)])[0];
        }
        public static float ReadSingle(this ReadOnlySpan<byte> span,ref int offset) {
            return MemoryMarshal.Cast<byte, float>(span[offset..(offset += 4)])[0];
        }
        public static int ReadInt32(this ReadOnlySpan<byte> span,ref int offset) {
            return MemoryMarshal.Cast<byte, int>(span[offset..(offset += 4)])[0];
        }
        public static unsafe void WriteTo<T>(this ReadOnlySpan<byte> span,ref int offset, T* ptr,int length) where T:unmanaged {
            var valueAsBytes = new Span<byte>((byte*)ptr,length*sizeof(T));
            span.Slice(offset,valueAsBytes.Length).CopyTo( valueAsBytes);
            offset += valueAsBytes.Length;
        }
        
    }
}