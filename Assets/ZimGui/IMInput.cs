using UnityEngine;

namespace ZimGui {
    public enum FocusState {
        NotFocus,
        NewFocus,
        Focus,
    }

    public static class IMInput {
        public struct ModalWindowData {
            public bool IsActive;
            public bool Activated;
            public Vector2 BasePoint;
            public int BaseID;
        }

        public static ModalWindowData ModalWindow;
        static RayCaster _rayCaster = new RayCaster(16);
        public static void Add(int id, Rect rect) => _rayCaster.Add(id, rect);
        public static void Add(Rect rect) => _rayCaster.Add(CurrentID, rect);

        public static void Init() {
            ModalWindow = default;
            _rayCaster.Clear();
        }

        public static string InputString {
            get {
                if (_inputStringIsCalled) return _inputString;
                _inputString = Input.inputString;
                _inputStringIsCalled = false;
                return _inputString;
            }
        }

        static string _inputString;
        static bool _inputStringIsCalled = false;
        public static bool CanPush() => IsPointerDownActive | !IsPointerOn;
        static bool _canPush;
        public static void NewFrame(Vector2 pointerPos, bool pointerOn, bool pointerDown) {
            CurrentID = 0;
            FocusActivated = false;
            _inputStringIsCalled = false;
            IsPointerDownActive = IsPointerDown = pointerDown;
            IsPointerActive = IsPointerOn = pointerOn;
            _canPush = IsPointerDown | !IsPointerOn;
            ModalWindow.Activated = false;
            PointerPosition = pointerPos;
            if (ModalWindow.IsActive) {
                TargetID = -1;
            }
            else {
                if (RetainFocus(pointerPos, pointerOn, pointerDown)) {
                    TargetID = FocusID;
                }
                else {
                    FocusID = -10;
                    TargetID = _rayCaster.Raycast(pointerPos);
                }

                _rayCaster.Clear();
            }
        }

        public static void EndFrame() {
            ModalWindow.IsActive &= ModalWindow.Activated;
            if (0 <= FocusID && !FocusActivated) FocusID = -1;
        }

        static bool RetainFocus(Vector2 pointerPos, bool pointerOn, bool pointerDown) {
            if (FocusID < -1) return false;
            if (FocusNeedMouse) {
                if (!pointerOn) {
                    FocusID = -10;
                    return false;
                }

                return true;
            }

            if (pointerDown && !FocusRect.Contains(pointerPos)) {
                FocusID = -10;
                return false;
            }

            return true;
        }

        public static FocusState GetModalWindowState(this Rect rect) {
            if (ModalWindow.IsActive) {
                if (ModalWindow.BaseID != CurrentID) return FocusState.NotFocus;
                if (!rect.Contains(ModalWindow.BasePoint)) {
                    return FocusState.NotFocus;
                }

                ModalWindow.Activated = true;
                return FocusState.Focus;
            }
            else {
                if (!IsPointerDownActive || !ContainsActiveMouseDown(rect)) return FocusState.NotFocus;
                IsPointerDownActive = false;
                IsPointerActive = false;
                ModalWindow.Activated = true;
                ModalWindow.IsActive = true;
                ModalWindow.BasePoint = rect.center;
                ModalWindow.BaseID = CurrentID;
                TargetID = -1;
                return FocusState.NewFocus;
            }
        }

        public static void CloseModalWindow() {
            ModalWindow = default;
            FocusID = -10;
        }

        public static bool IsFocusActive(this Rect rect, bool focusNeedMouse = true) {
            if (TargetID != CurrentID || FocusActivated) return false;
            if (focusNeedMouse && !(IsPointerDownActive || IsPointerActive)) return false;
            if (FocusID == CurrentID) {
                if (!rect.Contains(FocusPosition)) return false;
                IsPointerDownActive = false;
                IsPointerActive = false;
                FocusPosition = rect.center;
                if (!focusNeedMouse) FocusRect = rect;
                FocusActivated = true;
                return true;
            }

            if (-1 <= FocusID || !ContainsActiveMouseDown(rect)) return false;
            IsPointerDownActive = false;
            IsPointerActive = false;
            FocusPosition = rect.center;
            if (!focusNeedMouse) FocusRect = rect;
            FocusNeedMouse = focusNeedMouse;
            FocusActivated = true;
            FocusID = CurrentID;
            return true;
        }

        public static FocusState GetFocusState(this Rect rect, bool focusNeedMouse = true) {
            if (TargetID != CurrentID || FocusActivated) return FocusState.NotFocus;
            if (focusNeedMouse && !(IsPointerDownActive || IsPointerActive)) return FocusState.NotFocus;
            if (FocusID == CurrentID) {
                if (!rect.Contains(FocusPosition)) return FocusState.NotFocus;
                IsPointerDownActive = false;
                IsPointerActive = false;
                FocusPosition = rect.center;
                FocusRect = rect;
                FocusActivated = true;
                return FocusState.Focus;
            }

            if (-1 <= FocusID || !ContainsActiveMouseDown(rect)) return FocusState.NotFocus;
            IsPointerDownActive = false;
            IsPointerActive = false;
            FocusPosition = rect.center;
            FocusRect = rect;
            FocusNeedMouse = focusNeedMouse;
            FocusActivated = true;
            FocusID = CurrentID;
            IsPointerActive = false;
            return FocusState.NewFocus;
        }

        public static void LoseFocus() {
            FocusID = -10;
        }

        public static bool IsFocusActive(Vector2 center, float radius, bool focusNeedMouse = true) {
            if (TargetID != CurrentID || FocusActivated) return false;
            if (focusNeedMouse && !(IsPointerDownActive || IsPointerActive)) return false;
            if (FocusID == CurrentID) {
                if (radius * radius < (FocusPosition - center).sqrMagnitude) return false;
                IsPointerDownActive = false;
                IsPointerActive = false;
                FocusPosition = center;
                if (!focusNeedMouse) FocusRect = new Rect(center.x - radius, center.y - radius, 2 * radius, 2 * radius);
                FocusActivated = true;
                return true;
            }

            if (-1 <= FocusID || !CircleContainsActiveMouseDown(center, radius)) return false;
            IsPointerDownActive = false;
            IsPointerActive = false;
            FocusPosition = center;
            if (!focusNeedMouse) FocusRect = new Rect(center.x - radius, center.y - radius, 2 * radius, 2 * radius);
            FocusNeedMouse = focusNeedMouse;
            FocusActivated = true;
            FocusID = CurrentID;
            return true;
        }

        public static bool ContainsActiveMouse(this Rect rect) {
            return TargetID == CurrentID && rect.Contains(PointerPosition);
        }
        
        public static bool CanPush(this Rect rect) {
            return _canPush&& TargetID == CurrentID && rect.Contains(PointerPosition);
        }

        public static bool ContainsActiveMouseDown(this Rect rect) {
            return IsPointerDownActive && TargetID == CurrentID && rect.Contains(PointerPosition);
        }

        public static bool ContainsMouse(this Rect rect) {
            return rect.Contains(PointerPosition);
        }

        public static bool CircleContainsActiveMouse(Vector2 center, float radius) {
            if (!IsPointerActive || TargetID != CurrentID) return false;
            return ((PointerPosition - center).sqrMagnitude < radius * radius);
        }

        public static bool CircleContainsActiveMouseDown(Vector2 center, float radius) {
            return IsPointerDownActive && TargetID == CurrentID &&
                   ((PointerPosition - center).sqrMagnitude < radius * radius);
        }

        public static int TargetID;
        public static int CurrentID;
        public static Vector2 FocusPosition;
        public static Rect FocusRect;
        public static Vector2 PointerPosition;
        public static bool FocusNeedMouse;
        public static int FocusID;
        public static bool FocusActivated;
        public static bool IsPointerDown;
        public static bool IsPointerOn;
        public static bool IsPointerActive;
        public static bool IsPointerDownActive;
    }
}