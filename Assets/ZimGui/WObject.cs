using System;
using UnityEngine;

namespace ZimGui {
    public class WObject:Window {
        public Func<bool> DrawFunction;

        public WObject(string name, Rect rect, Func<bool> func) : base(name, rect) {
            DrawFunction = func;
        }
        public WObject(string name, Func<bool> func) : base(name, new Rect(0,0,200,300)) {
            DrawFunction = func;
        }
        public bool DrawAsElement() {
            if (!Opened) return true;
          return  DrawFunction();
        }
        public bool DrawAsWindow() {
            if (!Opened) return true;
            IM.Current = this;
            IMInput.CurrentID = WindowID;
            if (Held) {
                IMInput.TargetID = IMInput.CurrentID;
            }
            DrawBackGround();
            bool movable = !IM.RectResize(ref Rect, new Vector2(30, IMStyle.SpacedTextHeight + 20));
            if (!DrawFunction()) {
                DrawFunction = null;
                IM.EndWindowWithOutDrag(false);
                return false;
            }
            if (movable) {
                if (!IMInput.IsPointerActive) {
                    Held = false;
                }
                else if (IMInput.IsPointerDownActive) {
                    Held = Rect.ContainsActiveMouse();
                    if (Held) IMInput.IsPointerDownActive = false;
                }
                else if (Held) {
                    Rect.Move(IM.ScreenDeltaMousePos);
                }
            }
            IM.EndWindowWithOutDrag(true);
            return true;
        }

        void DrawBackGround() {
            var instance = IM.Mesh;
            var fontSize = IMStyle.FontSize;
            var spacedSize = IMStyle.SpacedTextHeight;
            var topLeft = new Vector2(Rect.xMin, Rect.yMax);
            var labelPos = new Vector2(Rect.xMin, Rect.yMax - spacedSize);
            var closeRect = new Rect(Rect.xMax - spacedSize, labelPos.y, spacedSize, spacedSize);
            var closeButtonContainsMouse = closeRect.ContainsActiveMouse();
            if (closeButtonContainsMouse) {
                if (IM.TryConsumeLeftMouseDown()) {
                    Opened = false;
                    return;
                }
            }

            instance.AddRect(Rect, IMStyle.WindowBackGroundColor);
            var gradientRect = new Rect(labelPos, new Vector2(Rect.width, IMStyle.SpacedTextHeight));
            instance.AddHorizontalGradient(gradientRect, IMStyle.WindowLabelLeftColor, IMStyle.WindowLabelRightColor);
            if (closeButtonContainsMouse) {
                instance.AddRect(closeRect, new UiColor(200,0,0,255));
            }

            instance.AddChar(new Vector2(Rect.xMax -spacedSize/2,  Rect.yMax-spacedSize/2), fontSize, 'x',
                Color.white);
            gradientRect.width -= spacedSize;
            instance.AddText(gradientRect, fontSize, Name, Color.white);
            NextPosition = new Vector2(topLeft.x+fontSize/4, topLeft.y - spacedSize);
        }

    }
}