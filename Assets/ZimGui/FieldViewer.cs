using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using Str=System.ReadOnlySpan<char>;
namespace ZimGui {

   public   delegate void ReferenceViewer(Rect rect,Str text, ref byte reference,bool isReadOnly);
    public static class FieldView {
         static Dictionary<(Type, string), (short,short)> Fields=new (16);
         public static readonly int BaseOffset = UnsafeUtility.GetFieldOffset(typeof(OffSetObject).GetField("ByteField"));

         static ReferenceViewer[] Actions = 
            {
                (Rect rect,Str name, ref byte b, bool isReadOnly) => {
                    if(isReadOnly)
                        IM.IntField(rect,name,  Unsafe.As<byte, int>(ref b));
                    else 
                        IM.IntField(rect,name, ref Unsafe.As<byte, int>(ref b));
                },
                (Rect rect,Str name, ref byte b, bool isReadOnly) => {
                    if (isReadOnly)
                        IM.FloatField(rect,name,  Unsafe.As<byte, float>(ref b));
                    else 
                        IM.FloatField(rect,name, ref Unsafe.As<byte, float>(ref b));
                    
                },
                (Rect rect,Str name, ref byte b, bool isReadOnly) => {
                    if (isReadOnly)
                        IM.StringField(rect,name,  Unsafe.As<byte, string>(ref b));
                    else 
                        IM.StringField(rect,name, ref Unsafe.As<byte, string>(ref b));
                },
                (Rect rect,Str name, ref byte b, bool isReadOnly) => {
                    IM.BoolField(rect,name, ref Unsafe.As<byte, bool>(ref b));
                },
                
            };

         static Dictionary<Type, short> TypeToAction = new Dictionary<Type, short>(16) {
            {typeof(int), 0},
            {typeof(float), 1},
            {typeof(string), 2},
            {typeof(bool), 3},
        };
         static bool TryGet(Type type, string name,out (short ActionIndex,short Offset) tuple) {
            if (Fields.TryGetValue((type, name), out  tuple)) {
                return true;
            }
            var field = type.GetField(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (field == null) {
                tuple = default;
                return false;
            }

            var fieldType = field.FieldType;
            if (!TypeToAction.TryGetValue(fieldType,out var actionIndex))return false;
            var offset = (short)UnsafeUtility.GetFieldOffset(field);
          
            tuple = (actionIndex, offset);
             Fields.Add((type, name),tuple);
             return true;
        }
       static bool TryGetAutoProperty(Type type, string name,out (short ActionIndex,short Offset) tuple) {
            if (Fields.TryGetValue((type, name), out  tuple)) {
                return true;
            }
            var field = type.GetField("<"+name+">k__BackingField",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (field == null) {
                tuple = default;
                return false;
            }

            var fieldType = field.FieldType;
            if (!TypeToAction.TryGetValue(fieldType,out var actionIndex))return false;
            var offset = (short)UnsafeUtility.GetFieldOffset(field);
          
            tuple = (actionIndex, offset);
             Fields.Add((type, name),tuple);
             return true;
        }
       

        abstract class OffSetObject {
            public byte ByteField;
            public ref byte GetPinnableReference() => ref ByteField;
        }
        public static  ref T GetReference<T>( object o, int offset) {
            return  ref Unsafe.As<byte, T>(ref Unsafe.Add(ref Unsafe.As<OffSetObject>(o).ByteField, offset - BaseOffset));
        }
        public static  ref byte GetReference(this object o, int offset) {
            return  ref Unsafe.Add(ref Unsafe.As<OffSetObject>(o).ByteField, offset - BaseOffset);
        }
      

       
        public static  bool ViewField(this object o, string fieldName,bool isReadOnly=true,bool useRawName=false) {
            if(TryGet(o.GetType(),fieldName,out var tuple)) {
                if (!IM.Current.TryGetNextRect(out var rect)) return false;
                Actions[tuple.Item1](rect,useRawName?fieldName: GetDisplayName(fieldName),ref GetReference(o,tuple.Item2),isReadOnly);
                return true;
            }
            return false;
        }
        public static  bool ViewAutoProperty(this object o, string fieldName,bool isReadOnly=true) {
            if(TryGetAutoProperty(o.GetType(),fieldName,out var tuple)) {
                if (!IM.Current.TryGetNextRect(out var rect)) return false;
                Actions[tuple.Item1](rect,fieldName,ref GetReference(o,tuple.Item2),isReadOnly);
                return true;
            }
            return false;
        }
       
        public static  bool ViewField(this object o, string fieldName,Str displayName,bool isReadOnly=true) {
            if(TryGet(o.GetType(),fieldName,out var tuple)) {
                if (!IM.Current.TryGetNextRect(out var rect)) return false;
                Actions[tuple.Item1](rect,displayName, ref GetReference(o,tuple.Item2),isReadOnly);
                return true;
            }
            return false;
        }

      

        public static void ViewElements(this Span<float> span, Str name, bool isReadOnly = true) {
            if (span.IsEmpty) return;
            if (10 < span.Length) span = span[..10];
            var currentWindow = IM.Current;
            Span<char> label = stackalloc char[name.Length + 2];
            name.CopyTo(label[..name.Length]);
            label[name.Length] = ' ';
            if (isReadOnly) {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.FloatField(rect, label, span[index]);
                }
            }
            else {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.FloatField(rect, label, ref span[index]);
                }
            }
        }
        public static void ViewElements(this Span<int> span, Str name, bool isReadOnly = true) {
            if (span.IsEmpty) return;
            if (10 < span.Length) span = span[..10];
            var currentWindow = IM.Current;
            Span<char> label = stackalloc char[name.Length + 2];
            name.CopyTo(label[..name.Length]);
            label[name.Length] = ' ';
            if (isReadOnly) {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.IntField(rect, label, span[index]);
                }
            }
            else {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.IntField(rect, label, ref span[index]);
                }
            }
        }
        public static void ViewElements(this Span<Vector2> span, Str name, bool isReadOnly = true) {
            if (span.IsEmpty) return;
            if (10 < span.Length) span = span[..10];
            var currentWindow = IM.Current;
            Span<char> label = stackalloc char[name.Length + 2];
            name.CopyTo(label[..name.Length]);
            label[name.Length] = ' ';
            if (isReadOnly) {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.Vector2Field(rect, label, span[index]);
                }
            }
            else {
                for (var index = 0; index < span.Length; index++) {
                    if (!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) break;
                    label[name.Length + 1] = (char) ('0' + index);
                    IM.Vector2Field(rect, label, ref span[index]);
                }
            }
        }
        public static void ViewElements(this ReadOnlySpan<float> span, Str name) {
            var currentWindow = IM.Current;
            foreach (var element in span) {
                if(!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight,out var rect))break;
                IM.FloatField(rect,name, element);
            }
        }
        public static void ViewElements(this ReadOnlySpan<int> span, Str name) {
            var currentWindow = IM.Current;
            foreach (var element in span) {
                if(!currentWindow.TryGetNextRect(IMStyle.SpacedTextHeight,out var rect))break;
                IM.IntField(rect,name, element);
            }
        }
        
        static Str GetDisplayName(string name) {
            var length = name.Length;
            if (length <= 1) return name.AsSpan();
            if (name.StartsWith('_')) {
                return name.AsSpan(1);
            }
            if (name.StartsWith('m') && name[1] == '_') {
                return name.AsSpan(2);
            }
            return name.AsSpan();
        }

    }
}