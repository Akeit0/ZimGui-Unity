using System;
using UnityEngine;

namespace ZimGui {


    public class Window  {
        public string Name;
        public readonly int WindowID;
        static int _lastWindowID;
        public Rect Rect;
        public Vector2 NextPosition;
        public bool Opened;
        public bool Held;
        
        public static void Init() {
            _lastWindowID = 0;
        }

        public Window(string name, Rect rect) {
            WindowID = _lastWindowID++;
            Name = name;
            Rect = rect;
            NextPosition = default;
        }
        public Window() {
            WindowID = _lastWindowID++;
            NextPosition = default;
        }

        public void ResetNext() {
            NextPosition = new Vector2(Rect.xMin, Rect.yMax);
        }

        public void Draw(int frame) {
            var instance = IM.Mesh;
            instance.AddRect(Rect, IMStyle.WindowBackGroundColor);
            var fontSize = IMStyle.FontSize;
            var topLeft = new Vector2(Rect.xMin, Rect.yMax);
            var labelPos = new Vector2(Rect.xMin, Rect.yMax - IMStyle.SpacedTextHeight);
            var gradientRect = new Rect(labelPos, new Vector2(Rect.width, IMStyle.SpacedTextHeight));
            instance.AddHorizontalGradient(gradientRect, IMStyle.WindowLabelLeftColor, IMStyle.WindowLabelRightColor);
            instance.AddText(gradientRect, fontSize, Name, Color.white);
            NextPosition = new Vector2(topLeft.x+fontSize/5, topLeft.y - IMStyle.SpacedTextHeight);
        }

        public void Draw(int frame, ref bool isOpen) {
            if (!isOpen) return;
            var instance = IM.Mesh;
            var fontSize = IMStyle.FontSize;
            var spacedSize = IMStyle.SpacedTextHeight;
            var topLeft = new Vector2(Rect.xMin, Rect.yMax);
            var labelPos = new Vector2(Rect.xMin, Rect.yMax - spacedSize);
            var closeRect = new Rect(Rect.xMax - spacedSize, labelPos.y, spacedSize, spacedSize);
            var closeButtonContainsMouse = closeRect.ContainsActiveMouse();
            if (closeButtonContainsMouse) {
                if (IM.TryConsumeLeftMouseDown()) {
                    isOpen = false;
                    return;
                }
            }

            instance.AddRect(Rect, IMStyle.WindowBackGroundColor);
            var gradientRect = new Rect(labelPos, new Vector2(Rect.width, IMStyle.SpacedTextHeight));
            instance.AddHorizontalGradient(gradientRect, IMStyle.WindowLabelLeftColor, IMStyle.WindowLabelRightColor);
            if (closeButtonContainsMouse) {
                instance.AddRect(closeRect, new UiColor(200,0,0,255));
            }

            instance.AddChar(new Vector2(Rect.xMax - spacedSize/2, Rect.yMax-spacedSize/2), fontSize, 'x',
                Color.white);
            gradientRect.width -= spacedSize;
            instance.AddText(gradientRect, fontSize, Name, Color.white);
            NextPosition = new Vector2(topLeft.x+fontSize/4, topLeft.y - spacedSize);
        }

        public float InnerWidth => Rect.width - IMStyle.FontSize / 2;
        public float InnerYMin => Rect.yMin + 20;

        public void ApplyDrag() {
            if (!IMInput.IsPointerActive) {
                Held = false;
                return;
            }
            if (IMInput.IsPointerDownActive) {
                Held = Rect.ContainsActiveMouse();
                if (Held) IMInput.IsPointerDownActive = false;
            }
            else if (Held) {
                Rect.Move(IM.ScreenDeltaMousePos);
            }

        }


        public bool TryGetNextRect(float height, out Rect rect) {
            var nextY = NextPosition.y - height;
            if (nextY < Rect.yMin) {
                rect = default;
                NextPosition.y = nextY;
                IMStyle.HorizontalDivisionsProgress = 0;
                return false;
            }
            var space = IMStyle.FontSize * IMStyle.IndentLevel;
            var division = IMStyle.HorizontalDivisions;
            if(division==1) {
                rect = new Rect(NextPosition.x + space, nextY, InnerWidth - space, height);
                NextPosition.y -= height * 1.1f;
                return true;
            }
            var ratio = (float) (IMStyle.HorizontalDivisionsProgress) / division;
            var start = NextPosition.x + space;
            var width= InnerWidth - space;
            rect= new Rect(start+ratio*width, nextY,width/division, height);
            if (++IMStyle.HorizontalDivisionsProgress == division) {
                NextPosition.y -= height * 1.1f;
                NextPosition.x = Rect.xMin+IMStyle.FontSize/4;
                IMStyle.HorizontalDivisionsProgress = 0;
            }
            return true;
        }
        
        public bool TryGetNextRect(out Rect rect) {
            return TryGetNextRect(IMStyle.SpacedTextHeight, out rect);
        }
      
        
       public bool TryGetRemainRect(out Rect rect) {
           var innerYMin = InnerYMin;
           if (NextPosition.y <= innerYMin) {
               rect = default;
               return false;
           }
           var height = NextPosition.y - innerYMin;
           var space = IMStyle.FontSize*IMStyle.IndentLevel;
           rect = new Rect(NextPosition.x+space, innerYMin, InnerWidth-space, height);
           NextPosition.y = innerYMin-0.1f;
           return true;
        }
      

        public bool TryGetNextRect(float width, float height, out Rect rect) {
            var nextY = NextPosition.y - height*1.1f;
            if (nextY < Rect.yMin) {
                rect = default;
                return false;
            }

            rect = new Rect(NextPosition.x, NextPosition.y - height, width, height);
            NextPosition.y = nextY;
            return true;
        }


        public Rect GetNextRectHorizontal(float width, float height) {
            var rect = new Rect(NextPosition.x, NextPosition.y, width, height);
            NextPosition.x += width;
            if (Rect.xMax < NextPosition.x) {
                Rect.xMax = NextPosition.x;
            }

            return rect;
        }


    }
}