using System;
using System.Collections.Generic;
using Unity.Collections;
namespace ZimGui.Text {
   
    public static class TextQuery {
        public static void QueryStartsWith(this List<string> inputs,ReadOnlySpan<char> text,List<string> result) {
            result.Clear();
            
            var textLength = text.Length;
            foreach (var t in inputs) {
                if (textLength<t.Length&&t.AsSpan().StartsWith(text)) {
                    result.Add(t);
                }
            }
        }
        public static void QueryStartsWith(this List<string> inputs,ReadOnlySpan<char> text,List<string> result,StringComparison comparison) {
            result.Clear();
            var textLength = text.Length;
            foreach (var t in inputs) {
                if (textLength<t.Length&&t.AsSpan().StartsWith(text,comparison)) {
                    result.Add(t);
                }
            }
        }
        public static void QueryContains(this List<string> inputs,ReadOnlySpan<char> text,List<string> result,StringComparison comparison) {
            result.Clear();
            var textLength = text.Length;
            foreach (var t in inputs) {
                if (textLength<t.Length&&t.AsSpan().Contains(text,comparison)) {
                    result.Add(t);
                }
            }
        }
        public static void QueryContains<T>(this T inputs,ReadOnlySpan<char> text,List<(int,string)> result,StringComparison comparison)where T:IList<string>{
            result.Clear();
            var textLength = text.Length;
            for (var index = 0; index < inputs.Count; index++) {
                var t = inputs[index];
                if (textLength <= t.Length && t.AsSpan().Contains(text, comparison)) {
                    result.Add((index,t));
                }
            }
        }
        
        public static void QueryNextChar(this List<string> inputs,char c,int index,List<string> result) {
            result.Clear();
            foreach (var t in inputs) {
                if (index<t.Length&&t[index]==c) {
                    result.Add(t);
                }
            }
        }
    }
}