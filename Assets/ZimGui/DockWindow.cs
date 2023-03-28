using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ZimGui {
    public class DockWindow {
        List<string> _names = new (16);
        List<WObject> _activeWObjects = new (16);
        public int Index;
        public bool Opened = true;
        public void Add(WObject wObject) {
            _activeWObjects.Add(wObject);
        }
        public void Clear() {
            Index = -1;
            _activeWObjects.Clear();
            _names.Clear();
        }

        public static KeyCode OpenKey=KeyCode.Escape;
        public void Draw() {
            var lockPosition = false;
            if (Input.GetKeyDown(OpenKey)) {
                Opened = true;
                lockPosition = true;
            }
            foreach (var _ in IM.BeginWindow("Dock",new Rect(0,0,100,100),ref Opened,!lockPosition,Coordinate.TopLeft)) {
                for (var i = 0; i < _activeWObjects.Count; i++) {
                    var o = _activeWObjects[i];
                    var remove = false;
                    if (!IM.Current.TryGetNextRect(out var rect)) {
                        return;
                    };
                    var foldWidth = rect.width - IMStyle.SpacedTextHeight;
                    var  foldRect = new Rect(rect.xMin, rect.yMin,foldWidth, rect.height);
                    var buttonRect = new Rect(rect.xMin + foldWidth, rect.yMin, IMStyle.SpacedTextHeight, rect.height);
                    if (IM.Button(buttonRect, "-")) {
                        _activeWObjects.RemoveAt(i);
                        IM.WObjects.Add(o);
                        o.Opened = false;
                        break;
                    }
                    if (!IM.Foldout(foldRect, o.Name, o.Opened)) {
                        o.Opened=false;
                        continue;
                    }
                    using (IM.Indent()) {
                        if (! o.DrawAsElement()) {
                                remove = true;
                        }
                    }
                    if (remove) {
                        _activeWObjects.RemoveAtSwapBack(Index);
                        break;
                    }
                    o.Opened=true;
                }
                _names.Clear();
                foreach (var w in IM.WObjects) {
                    _names.Add(w.Name);
                    
                }
                if (IM.InputDropdownButton("Add", _names, ref Index)) {
                    _activeWObjects.Add( IM.WObjects[Index]);
                    IM.WObjects.RemoveAt(Index);
                }
                if (IM.InputDropdownButton("Open", _names, ref Index)) {
                    var w = IM.WObjects[Index];
                   w.Opened = true;
                   var mousePos = IMInput.PointerPosition;
                   w.Rect = new Rect(mousePos.x-50, mousePos.y-100+IMStyle.SpacedTextHeight/2, 100, 100);
                }
                
                
            }
        }
    }
}