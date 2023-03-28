using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using ZimGui.Core;
using UnityEngine.Profiling;

namespace ZimGui {
    public class ZimGuiManager :MonoBehaviour{
       public Camera TargetCamera;
        [SerializeField] TMP_FontAsset _fontAsset;
        [SerializeField] TextAsset _data;
        [SerializeField] Texture2D _fontTexture;
        [SerializeField] Material _material;
        [SerializeField] Material _textureMaterial;

        [SerializeField] Texture2D ResizeCursor;
        [SerializeField] Texture2D LRCursor;
        

        void Awake() {
            DontDestroyOnLoad(gameObject);
            if (TargetCamera == null) {
                TargetCamera = Camera.main;
            }

            UiMesh uiMesh;
            if (_fontAsset != null) {
                uiMesh = new UiMesh(_fontAsset, _material);
            }
            else {
                uiMesh = new UiMesh(_data.bytes.AsSpan(), _fontTexture, _material);
            }

            uiMesh.SetUpForTexture(_textureMaterial, 10);
            IM.Init(uiMesh, TargetCamera);
            IM.LRCursor = LRCursor;
            IM.ResizeCursor = ResizeCursor;
            IM.InsertNewFrameToPostEarlyUpdate();
            IM.InsertEndFrameToPrePostLateUpdate();
        }

        void OnDestroy() {
            IM.Dispose();
        }

      
    }
}