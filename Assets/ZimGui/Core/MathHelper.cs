using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ZimGui.Core {
    public static class MathHelper {
        public static int CeilPow2(int x) {
            var result = 1;
            while (result < x) {
                result <<= 1;
            }

            return result;
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static  Vector2 Perpendicular(this Vector2 v) {
            var x = v.x * v.x + v.y * v.y;
            if(x==0) return default;
            var y = 1f / Mathf.Sqrt(x);
            return new Vector2(-v.y * y, v.x * y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormIntersection(Vector2 a, Vector2 b) {
            var c = a.x * b.x + a.y * b.y+1;
            return new Vector2((a.x + b.x) / c, (a.y + b.y) / c);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastTan(float x) { 
             var xd=x*2d/Math.PI;
             var y = 1 - xd * xd;
             return (float) (xd * (-0.0187111 * y + 0.3158 + 1.2736575 / y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastTan2PI(float x) { 
             var xd=x*4d;
             var y = 1 - xd * xd;
             return (float) (xd * (-0.0187111 * y + 0.3158 + 1.2736575 / y));
        }

        public static Vector2 ConSin(float x) => new Vector2(Mathf.Cos(x), Mathf.Sin(x));



    }
}