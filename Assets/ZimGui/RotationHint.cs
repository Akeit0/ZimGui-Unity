using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ZimGui {

    public  struct EulerHint {
        public Quaternion Rotation {
            get=> _rotation;
            set => GetEulerAngles(value);
        }
        Quaternion _rotation;
        public Vector3 EulerAngles{
            get=> _eulerAngles;
            set { 
                _rotation=Quaternion.Euler(value);
                _eulerAngles = value;
            }
        }
        Vector3 _eulerAngles;
        public EulerHint(Vector3 eulerAngles) {
            _rotation=Quaternion.Euler(eulerAngles);
            _eulerAngles = eulerAngles;
        }
        
        
        
        public  Vector3 GetLocalEulerAngles(Transform t) {
            var q = t.localRotation;
            if (Rotation == q) return _eulerAngles;
            _rotation = q;
            var newEuler =q.eulerAngles;
            newEuler.x = RepeatWorking(newEuler.x - _eulerAngles.x + 180.0F, 360.0F) + _eulerAngles.x - 180.0F;
            newEuler.y = RepeatWorking(newEuler.y - _eulerAngles.y + 180.0F, 360.0F) + _eulerAngles.y - 180.0F;
            newEuler.z = RepeatWorking(newEuler.z - _eulerAngles.z + 180.0F, 360.0F) + _eulerAngles.z - 180.0F;
            _eulerAngles = newEuler;
            return  newEuler;
        }
        public  Vector3 GetEulerAngles(Quaternion q) {
            if (Rotation == q) return _eulerAngles;
            _rotation = q;
            var newEuler =q.eulerAngles;
            newEuler.x = RepeatWorking(newEuler.x - _eulerAngles.x + 180.0F, 360.0F) + _eulerAngles.x - 180.0F;
            newEuler.y = RepeatWorking(newEuler.y - _eulerAngles.y + 180.0F, 360.0F) + _eulerAngles.y - 180.0F;
            newEuler.z = RepeatWorking(newEuler.z - _eulerAngles.z + 180.0F, 360.0F) + _eulerAngles.z - 180.0F;
            _eulerAngles = newEuler;
            return  newEuler;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float RepeatWorking (float t, float length)
        {
            return (t - (Mathf.Floor(t / length) * length));
        }
    }
}