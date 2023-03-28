using System;
using UnityEngine;

namespace ZimGui {
    public struct RayCaster {
        public Rect[] Rects;
        public int[] TargetID;
        public int Count;

        public RayCaster(int capacity) {
            Rects = new Rect[capacity];
            TargetID = new int[capacity];
            Count = 0;
        }

        public void Add(int id, Rect rect) {
            if (Count == Rects.Length) {
                Array.Resize(ref Rects ,Count*2);
                Array.Resize(ref TargetID ,Count*2);
            }
            Rects[Count] = rect;
            TargetID[Count] = id;
            Count++;
        } 
        public void Add(RayCaster child) {
            for (int i = 0; i < child.Count; i++) {
                Add(child.TargetID[i],child.Rects[i]);
            }
        }
        public void Clear() {
            Rects.AsSpan(0,Count).Clear();
            TargetID.AsSpan(0,Count).Clear();
            Count = 0;
        }
        public int Raycast(Vector2 v) {
            var rects = Rects;
            for (int i = Count-1; 0<=i; --i) {
                if (rects[i].Contains(v)) {
                    return TargetID[i];
                }
            }
            return -1;
        }
    }
}