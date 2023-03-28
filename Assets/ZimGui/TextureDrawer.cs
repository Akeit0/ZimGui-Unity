using System;
using UnityEngine;

namespace ZimGui {
    public class TextureDrawer : MonoBehaviour {
        public Texture Texture1;
        public Texture Texture2;
        public Texture Texture3;
        public Rect Rect1;
        public Rect Rect2;
        public Rect Rect3;

       
        void Update() {
            IM.DrawTexture(Rect1,Texture1);
            IM.DrawTexture(Rect2,Texture2);
            IM.DrawTexture(Rect3,Texture3);
        }
        
        
        
        //
        // void OnDestroy() {
        //     _textureMesh.Dispose();
        // }
    }
}