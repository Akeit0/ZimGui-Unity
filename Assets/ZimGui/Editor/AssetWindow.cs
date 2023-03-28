using System.IO;
using ZimGui.Core;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ZimGui.Editor {
    public class AssetWindow : EditorWindow
    {
        [MenuItem("Tools/ZimGUIAsset")]
         static void CreateWindow()
        {
            GetWindow<AssetWindow>();
        }
         static TMP_FontAsset _asset;
        void OnGUI() {
           
            _asset=EditorGUILayout.ObjectField(_asset, typeof(TMP_FontAsset),true) as TMP_FontAsset;
            if (_asset!=null&&GUILayout.Button("Save new ZimGUIAsset"))
            {
                var selectedPathName = EditorUtility.SaveFilePanel(title: "Title", directory: "Assets", defaultName: "ZimGUI_"+_asset.name+".bytes", extension: "bytes");
               var bytes= UiMesh.CreateAsset(_asset);
               File.WriteAllBytes(selectedPathName,bytes);
               AssetDatabase.Refresh();
            }
        }
    }
}
#endif