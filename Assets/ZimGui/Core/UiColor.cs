using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ZimGui {
    
    public struct UiColor : IEquatable<UiColor> {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
        public UiColor(uint rgba) {
            R = (byte)(rgba >> 24);
            G = (byte)((rgba >> 16)&0xFF);
            B = (byte)((rgba >> 8)&0xFF);
            A = (byte)(rgba&0xFF);
        }
        public UiColor(uint rgb,byte a) {
            R = (byte)((rgb >> 16)&0xFF);
            G = (byte)((rgb >> 8)&0xFF);
            B = (byte)(rgb&0xFF);
            A = a;
        }
        

        public UiColor(byte r, byte g, byte b, byte a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public UiColor(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public static implicit operator UiColor(Color color) => new UiColor((byte) (color.r * 255), (byte) (color.g * 255), (byte) (color.b * 255), (byte) (color.a * 255));

        public static implicit operator Color(UiColor color) =>
            new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

        public static implicit operator UiColor(Color32 color) =>UnsafeUtility.As<Color32,UiColor>(ref color);
        public static implicit operator Color32(UiColor color) => new Color32(color.R, color.G, color.B, color.A);
        
        public static implicit operator UiColor(uint a) =>UnsafeUtility.As<uint,UiColor>(ref a);

        /// <summary>
        /// #FFFFFFFF
        /// </summary>
        public static UiColor White => new UiColor(255, 255, 255);

        /// <summary>
        /// #000000FF
        /// </summary>
        public static UiColor Black => new UiColor(0, 0, 0);

        /// <summary>
        /// #7F7F7FFF
        /// </summary>
        public static UiColor Gray => new UiColor(127, 127, 127);

        /// <summary>
        /// #7F7F7FFF
        /// </summary>
        public static UiColor Grey => new UiColor(127, 127, 127);

        /// <summary>
        /// #00000000
        /// </summary>
        public static UiColor Clear => default;

        /// <summary>
        /// #FF000000
        /// </summary>
        public static UiColor Red => new UiColor(255, 0, 0);

        /// <summary>
        /// #00FF00FF
        /// </summary>
        public static UiColor Green => new UiColor(0, 255, 0);

        /// <summary>
        /// #0000FFFF
        /// </summary>
        public static UiColor Blue => new UiColor(0, 0, 255);

        /// <summary>
        /// #EA0400FF
        /// </summary>
        public static UiColor Yellow => new UiColor(255,234, 4);

        /// <summary>
        /// #00FFFFFF
        /// </summary>
        public static UiColor Cyan => new UiColor(0, 255, 255);

        /// <summary>
        /// #FF00FFFF
        /// </summary>
        public static UiColor Magenta => new UiColor(255, 0, 255);

        /// <summary>
        /// #F97306FF
        /// </summary>
        public static UiColor Orange => new UiColor(0xf9, 0x73, 0x06);

        public static UiColor AlphaFade(UiColor color, byte fadeAlpha) =>
            new UiColor(color.R, color.G, color.B, (byte) (color.A - fadeAlpha));

        public static UiColor AlphaDiv(UiColor color, byte fadeAlpha) =>
            new UiColor(color.R, color.G, color.B, (byte) (color.A / fadeAlpha));
        public static UiColor WithAlpha(UiColor color, byte alpha) =>
            new UiColor(color.R, color.G, color.B, alpha);

       public  UiColor WithAlpha( byte alpha) =>
            new UiColor(R, G, B, alpha);

       public static UiColor Lerp(UiColor a, UiColor b, float t) {
          return  new UiColor((byte)(a.R + (b.R - a.R) * t),(byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t),(byte)(a.A + (b.A - a.A) * t));
       }

        [MethodImpl((MethodImplOptions) 256)]
        public static UiColor operator *(UiColor a, float b) =>
            new UiColor((byte) (a.R * b), (byte) (a.G * b), (byte) (a.B * b), (byte) (a.A * b));

        [MethodImpl((MethodImplOptions) 256)]
        public static UiColor operator *(float b, UiColor a) =>
            new UiColor((byte) (a.R * b), (byte) (a.G * b), (byte) (a.B * b), (byte) (a.A * b));

        [MethodImpl((MethodImplOptions) 256)]
        public static UiColor operator /(UiColor a, float b) =>
            new UiColor((byte) (a.R / b), (byte) (a.G / b), (byte) (a.B / b), (byte) (a.A / b));

        [MethodImpl((MethodImplOptions) 256)]
        public static UiColor operator /(UiColor a, int b) =>
            new UiColor((byte) (a.R / b), (byte) (a.G / b), (byte) (a.B / b), (byte) (a.A / b));

        [MethodImpl((MethodImplOptions) 256)]
        public static bool operator ==(UiColor lhs, UiColor rhs) =>
            UnsafeUtility.As<UiColor,int>(ref lhs)==UnsafeUtility.As<UiColor,int>(ref rhs);

        [MethodImpl((MethodImplOptions) 256)]
        public static bool operator !=(UiColor lhs, UiColor rhs) => !(lhs == rhs);

        public bool Equals(UiColor other) {
            return UnsafeUtility.As<UiColor,int>(ref this)==UnsafeUtility.As<UiColor,int>(ref other);
        }

        public override bool Equals(object obj) {
            return obj is UiColor other && Equals(other);
        }

        public override int GetHashCode() {
            return UnsafeUtility.As<UiColor,int>(ref this);
        }

        public override string ToString() {
            return "("+R+", "+G+", "+B+", "+A+")";
        }
    }
}