using System;
using ZimGui.Core;
using UnityEngine;
using UnityEngine.Profiling;

namespace ZimGui {
   
    public interface IFormatter<in T> {
        public bool TryFormat(T value,Span<char> text, out int charsWritten);
    }

    public struct Vector2Formatter : IFormatter<Vector2> {
        public bool TryFormat(Vector2 value, Span<char> text, out int charsWritten) {
            try {
                text[0] = '(';
                var offset = 1;
                value.x.Format(ref offset,text);
                text[offset++] = ',';
                value.y.Format(ref offset,text);
                text[offset++] = ')';
                charsWritten = offset;
                return true;
            }
            catch (Exception) { 
                charsWritten = 0;
                return false;
            }
        }
    }
    public struct Vector3Formatter : IFormatter<Vector3> {
        public bool TryFormat(Vector3 value, Span<char> text, out int charsWritten) {
            try {
                text[0] = '(';
                var offset = 1;
                value.x.Format(ref offset,text);
                text[offset++] = ',';
                value.y.Format(ref offset,text);
                text[offset++] = ',';
                value.z.Format(ref offset,text);
                text[offset++] = ')';
                charsWritten = offset;
                return true;
            }
            catch (Exception) { 
                charsWritten = 0;
                return false;
            }
        }
    }
}