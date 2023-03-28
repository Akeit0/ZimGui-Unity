using System;
using ZimGui.Core;
using UnityEngine;
namespace ZimGui {
    
    public interface IParserFormatter<T>:IFormatter<T> {
        public bool TryParse(ReadOnlySpan<char> text, out T value);
        public T SetDefault(Span<char> text, out int charsWritten);
    }
    
    public struct Int32ParserFormatter:IParserFormatter<int> {
        public bool TryParse(ReadOnlySpan<char> text, out int value) => int.TryParse(text, out value);
        public bool TryFormat(int value, Span<char> text, out int charsWritten) => value.TryFormat(text, out charsWritten);
        public int SetDefault(Span<char> text, out int charsWritten) {
            text[0] = '0';
            charsWritten = 1;
            return 0;
        }
    }
    public struct SingleParserFormatter:IParserFormatter<float> {
        public bool TryParse(ReadOnlySpan<char> text, out float value) => float.TryParse(text, out value);
        public bool TryFormat(float value, Span<char> text, out int charsWritten) => value.TryFormat(text, out charsWritten,IMStyle.FloatFormat);
        public float SetDefault(Span<char> text, out int charsWritten) {
            text[0] = '0';
            charsWritten = 1;
            return 0;
        }
    }
    public interface IElementParserFormatter<T> {
        public bool TryParse(ReadOnlySpan<char> text,char separator,out int charsRead, out T value);
        public bool TryFormat(T value,Span<char> text, out int charsWritten);
        public T SetDefault(Span<char> text, out int charsWritten);
    }
   
   
    public interface IOverWritingParserFormatter<in T> {
        public bool TryParse(ReadOnlySpan<char> text,  T target);
        public bool TryFormat(T target,Span<char> text, out int charsWritten);
        public void SetDefault(Span<char> text,T target, out int charsWritten);
        public void SetDefault(T value);
    }
  
}