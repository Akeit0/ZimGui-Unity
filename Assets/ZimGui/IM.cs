using System;
using System.Collections.Generic;
using System.Reflection;
using ZimGui.Core;
using ZimGui.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using Str = System.ReadOnlySpan<char>;

namespace ZimGui {
    public static class IMStyle {
        public static UiColor BasicColor = UiColor.White;
        public static UiColor WindowLabelLeftColor = UiColor.Black;
        public static UiColor WindowLabelRightColor = UiColor.Cyan;
        public static UiColor WindowBackGroundColor = new UiColor(56, 56, 56, 180);

        public static UiColor WindowLabelColor {
            set {
                WindowLabelLeftColor = value;
                WindowLabelRightColor = value;
            }
        }

        public static UiColor FontColor = UiColor.White;
        public static UiColor ButtonColor = new UiColor(30, 30, 30, 150);
        public static UiColor FieldColor = new UiColor(30, 30, 30, 255);
        public static UiColor HoveredButtonColor = new UiColor(150, 150, 150, 150);
        public static UiColor UnHoveredButtonColor = new UiColor(100, 100, 100, 150);
        public static float FontSize = 20;
        public static float LabelWidth => FontSize * 7.5f;
        public static float LineSpaceRatio = 1.4f;

        public static string FloatFormat = "F2";
        public static float SpacedTextHeight => FontSize * LineSpaceRatio;

        public static float DragNumberScale = 1f;
        public static int IndentLevel = 0;


        public static int HorizontalDivisions=1;
        public static int HorizontalDivisionsProgress=0;
    }

    public struct NewFrame {
    }

    public struct EndFrame {
    }

    public static class IM {
        static Window _screenWindow;
        public static readonly Dictionary<string, Window> WindowDict = new Dictionary<string, Window>();
        public static Window Current;
        public static Rect ScreenRect;
        public static Vector2 ScreenDeltaMousePos = new Vector2(0, 0);
        public static Vector2 ScreenMousePos = new Vector2(0, 0);
        public static bool GetLeftArrowKey => Input.GetKey(KeyCode.LeftArrow);
        public static bool GetLeftArrowKeyDown => Input.GetKeyDown(KeyCode.LeftArrow);
        public static bool GetRightArrowKey => Input.GetKey(KeyCode.RightArrow);
        public static bool GetRightArrowKeyDown => Input.GetKeyDown(KeyCode.RightArrow);

        public static UiMesh Mesh;

        public static Camera Camera;

        static Range _popUpRange;

        public static bool IsInModalWindow;


        public readonly struct ModalWindowArea : IDisposable {
            public readonly int StartIndex;
            public readonly int WindowID;

            ModalWindowArea(int startIndex) {
                StartIndex = startIndex;
                IsInModalWindow = true;
                WindowID = IMInput.CurrentID;
                IMInput.CurrentID = -1;
            }

            public static ModalWindowArea New() {
                if (IsInModalWindow) throw new Exception();
                return new ModalWindowArea(Mesh.Length);
            }

            void IDisposable.Dispose() {
                IMInput.CurrentID = WindowID;
                _popUpRange = StartIndex..Mesh.Length;
                IsInModalWindow = false;
            }
        }

        public static Rect GetPadded(ref this Rect rect, float padding) =>
            new(rect.x - padding, rect.y - padding, rect.width + 2 * padding, rect.height + 2 * padding);

        public static void Move(ref this Rect rect, Vector2 delta) {
            ref var min = ref UnsafeUtility.As<Rect, Vector2>(ref rect);
            min += delta;
        }

        public static void MoveX(ref this Rect rect, float delta) {
            ref var min = ref UnsafeUtility.As<Rect, float>(ref rect);
            min += delta;
        }

        public static void MoveY(ref this Rect rect, float delta) {
            ref var min = ref UnsafeUtility.As<Rect, Vector2>(ref rect);
            min.y += delta;
        }

        public static bool DivideToLabelAndValue(ref this Rect rect, out Rect labelRect, out Rect valueRect) {
            var labelWidth = IMStyle.LabelWidth;
            if (rect.width < labelWidth) {
                labelRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
                valueRect = default;
                return false;
            }

            labelRect = new Rect(rect.xMin, rect.yMin, labelWidth, rect.height);
            valueRect = new Rect(rect.xMin + labelWidth, rect.yMin, rect.width - labelWidth, rect.height);
            return true;
        }

        public static bool TryDivide(ref this Rect rect, float labelWidth, out Rect labelRect,
            out Rect valueRect) {
            if (rect.width < labelWidth) {
                labelRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
                valueRect = default;
                return false;
            }

            labelRect = new Rect(rect.xMin, rect.yMin, labelWidth, rect.height);
            valueRect = new Rect(rect.xMin + labelWidth, rect.yMin, rect.width - labelWidth, rect.height);
            return true;
        }
       

        public static bool TryConsumeLeftMouseDown() {
            if (IMInput.IsPointerDownActive) {
                IMInput.IsPointerDownActive = false;
                IMInput.IsPointerActive = false;
                return true;
            }

            return false;
        }


        public static char[] ScratchBuffer = new char[1024];
        static int frame;

        public static void Init(UiMesh mesh,
            Camera camera) {
            Window.Init();
            _screenWindow = new Window();
            Mesh = mesh;
            Camera = camera;
            IMInput.Init();
            _integratedWindow.Opened = true;
        }


        public static void Dispose() {
            WindowDict.Clear();
            WObjects.Clear();
            _integratedWindow.Clear();
            OnNewFrame = null;
            OnEndFrame = null;
            Mesh?.Dispose();
            Mesh = null;
            PlayerLoopInserter.RemoveRunner(typeof(NewFrame), typeof(EndFrame));
        }

        public static void InsertNewFrameToPostEarlyUpdate() {
            PlayerLoopInserter.InsertSystem(typeof(NewFrame), typeof(EarlyUpdate),
                InsertType.After, NewFrame);
        }

        public static void InsertEndFrameToPrePostLateUpdate() {
            PlayerLoopInserter.InsertSystem(typeof(EndFrame), typeof(PostLateUpdate),
                InsertType.Before, EndFrame);
        }

        public static bool TryGetPaste(out string text) {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.V)) {
                text = GUIUtility.systemCopyBuffer;
                return true;
            }

            text = "";
            return false;
        }

        public static event Action OnNewFrame;
        public static event Action OnEndFrame;
        public static void RequestResizeCursor() => _resizeCursorNeeded = true;

        public static Texture2D ResizeCursor;
        static bool _useResizeCursor;
        static bool _resizeCursorNeeded;
        public static Texture2D LRCursor;
        static bool _useLRCursor;
        static bool _LRCursorNeeded;

        static DockWindow _integratedWindow = new DockWindow();
        public static void Add(string name, Func<bool> func) => WObjects.Add(new WObject(name, func));

        public static void NewFrame() {
            if (Mesh == null) return;
            var currentFrame = Time.frameCount;
            if (frame != currentFrame) {
                frame = currentFrame;
                IMInput.NewFrame(Input.mousePosition, Input.GetMouseButton(0), Input.GetMouseButtonDown(0));
                var pos = Input.mousePosition;
                ScreenRect = new Rect(0, 0, Screen.width, Screen.height);
                _screenWindow.Rect = ScreenRect;
                _screenWindow.ResetNext();
                Current = _screenWindow;
                var newScreenMousePos = new Vector2(pos.x, pos.y);

                ScreenDeltaMousePos = newScreenMousePos - ScreenMousePos;
                ScreenMousePos = newScreenMousePos;
                _popUpRange = default;
                _resizeCursorNeeded = false;
                _LRCursorNeeded = false;
                try {
                    OnNewFrame?.Invoke();
                    _integratedWindow.Draw();
                    _wObjectsCopy.Clear();
                    foreach (var w in WObjects) {
                        if(w.Opened)
                            _wObjectsCopy.Add(w);
                    }
                    foreach (var w in _wObjectsCopy) {
                        if (! w.DrawAsWindow()) {
                            WObjects.Remove(w);
                        }
                    }
                    
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
        public static List<WObject> WObjects = new (16);
        static List<WObject> _wObjectsCopy = new (16);
       
        public static void EndFrame() {
            try {
                OnEndFrame?.Invoke();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }

            IMInput.EndFrame();
            if (_resizeCursorNeeded && !_useResizeCursor) {
                Cursor.SetCursor(ResizeCursor, new Vector2(8, 8), CursorMode.Auto);
                _useResizeCursor = true;
            }
            else if (_LRCursorNeeded && !_useLRCursor) {
                Cursor.SetCursor(LRCursor, new Vector2(8, 8), CursorMode.Auto);
                _useLRCursor = true;
            }
            else if ((_useResizeCursor && !_resizeCursorNeeded) || _useLRCursor && !_LRCursorNeeded) {
                _useResizeCursor = false;
                _useLRCursor = false;
                Cursor.SetCursor(null, new Vector2(8, 8), CursorMode.Auto);
            }

            var mesh = Mesh;
            if (_popUpRange.End.Value != 0) {
                mesh.SetDeferred(_popUpRange);
            }

            if (mesh != null) {
                if (mesh.Build()) {
                    mesh.Draw();
                }

                mesh.Clear();
            }
        }

        public static Rect TopLeftToBottomLeftCoordinate(this Rect rect) {
            var screenHeight = ScreenRect.height;
            return new Rect(rect.xMin, screenHeight - rect.yMax, rect.width, rect.height);
        }

        public static WindowState BeginWindow(string name, Rect rect, bool firstTimeOnly = true,
            Coordinate coordinate = Coordinate.BottomLeft) {
            if (coordinate == Coordinate.TopLeft) {
                rect = rect.TopLeftToBottomLeftCoordinate();
            }
            if (!WindowDict.TryGetValue(name, out Current)) {
                Current = new Window(name, rect);
                WindowDict.Add(name, Current);
            }
            else if (!firstTimeOnly) {
                Current.Rect = rect;
            }
            IMInput.CurrentID = Current.WindowID;
            if (Current.Held) {
                IMInput.TargetID = IMInput.CurrentID;
            }
            Current.Draw(Time.frameCount);
            
            if (firstTimeOnly) {
                if (RectResize(ref Current.Rect, new Vector2(30, IMStyle.SpacedTextHeight + 20))) {
                    Current.Held = false;
                }
            }
            return new WindowState(true);
        }

        public static WindowState BeginWindow(string name, Rect rect, ref bool isOpen, bool firstTimeOnly = true,
            Coordinate coordinate = Coordinate.BottomLeft) {
            if (!isOpen) return default;
            if (coordinate == Coordinate.TopLeft) {
                rect = rect.TopLeftToBottomLeftCoordinate();
            }

            if (!WindowDict.TryGetValue(name, out Current)) {
                Current = new Window(name, rect);
                WindowDict.Add(name, Current);
            }
            else if (!firstTimeOnly) {
                Current.Rect = rect;
            }

            IMInput.CurrentID = Current.WindowID;
            if (Current.Held) {
                IMInput.TargetID = IMInput.CurrentID;
            }

            Current.Draw(Time.frameCount, ref isOpen);
            if (!isOpen) return default;
            if (firstTimeOnly) {
                if (RectResize(ref Current.Rect, new Vector2(30, IMStyle.SpacedTextHeight + 20))) {
                    Current.Held = false;
                }
            }

            return new WindowState(true);
        }


        public static void EndWindow() {
           
            Current.ApplyDrag();
            

            IMInput.Add(Current.Rect);
            IMInput.CurrentID = 0;
            Current = _screenWindow;
        }
        public static void EndWindowWithOutDrag(bool alive) {
          if(alive)  IMInput.Add(Current.Rect);
            IMInput.CurrentID = 0;
            Current = _screenWindow;
        }

        public static DisposableIndent Indent() {
            IMStyle.IndentLevel++;
            return default;
        }
        public static DisposableDivision HorizontalDivide(int numOfDivision) {
            var lastDivision = IMStyle.HorizontalDivisions;
            IMStyle.HorizontalDivisions=numOfDivision;
            return new DisposableDivision(lastDivision);
        }

        public static void DrawTexture(Rect rect, Texture texture) {
            Mesh.AddTexture(rect, texture);
        }

        public static void DrawTexture(float height, Texture texture) {
            var width = texture.width * height / texture.height;
            if (Current.TryGetNextRect(width, height, out var rect)) {
                Mesh.AddTexture(rect, texture);
            }
        }

        public static void Line(Vector2 start, Vector2 end, float width, UiColor color) {
            Mesh.AddLine(start, end, width, color);
        }

        public static void Line(Vector2 start, Vector2 end, float width, UiColor startColor, UiColor endColor) {
            Mesh.AddLine(start, end, width, startColor, endColor);
        }

        public static void Circle(Vector2 center, float radius, UiColor color) {
            Mesh.AddCircle(center, radius, color);
        }

        public static void Circle(Vector2 center, float radius1, UiColor color1, float radius2, UiColor color2) {
            Mesh.AddCircle(center, radius1, color1, radius2, color2);
        }


        public static bool DropDownField(IList<string> list, ref int selected) {
            var lineHeight = IMStyle.SpacedTextHeight;
            var fontSize = IMStyle.FontSize;
            var mesh = Mesh;
            if (!Current.TryGetNextRect(lineHeight, out var rect)) return false;
            var focusState = DropDownFocusButton(rect, selected < 0 ? Str.Empty : list[selected]);
            if (focusState == FocusState.NotFocus) return false;
            if (focusState == FocusState.NewFocus) {
                return false;
            }

            using var p = ModalWindowArea.New();
            rect.MoveY(-lineHeight);
            var alreadySelected = false;
            for (var index = 0; index < list.Count; index++) {
                if (!alreadySelected && rect.ContainsMouse()) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                    alreadySelected = true;
                    if (TryConsumeLeftMouseDown()) {
                        selected = index;
                        IMInput.CloseModalWindow();
                        return true;
                    }
                }
                else {
                    mesh.AddRect(rect, new UiColor(40, 40, 40, 255));
                }

                mesh.AddText(rect, fontSize, list[index], IMStyle.FontColor);
                rect.MoveY(-lineHeight);
                if (rect.yMax < 0) break;
            }

            if (IMInput.IsPointerDownActive) {
                IMInput.CloseModalWindow();
            }

            return false;
        }public static bool DropDownField(Str label,IList<string> list, ref int selected) {
            var lineHeight = IMStyle.SpacedTextHeight;
            var fontSize = IMStyle.FontSize;
            var mesh = Mesh;
            if (!Current.TryGetNextRect(lineHeight, out var rect)) return false;
            if (!DivideToLabelAndValue(ref rect, out var labelRect, out var valueRect)) {
                Label(labelRect, label);
            }
            Label(labelRect, label);
            var focusState = DropDownFocusButton(valueRect, selected < 0 ? Str.Empty : list[selected]);
            if (focusState == FocusState.NotFocus) return false;
            if (focusState == FocusState.NewFocus) {
                return false;
            }
            using var p = ModalWindowArea.New();
            valueRect.MoveY(-lineHeight);
            var alreadySelected = false;
            for (var index = 0; index < list.Count; index++) {
                if (!alreadySelected && valueRect.ContainsMouse()) {
                    mesh.AddRect(valueRect, new UiColor(40, 156, 200, 255));
                    alreadySelected = true;
                    if (TryConsumeLeftMouseDown()) {
                        selected = index;
                        IMInput.CloseModalWindow();
                        return true;
                    }
                }
                else {
                    mesh.AddRect(valueRect, new UiColor(40, 40, 40, 255));
                }

                mesh.AddText(valueRect, fontSize, list[index], IMStyle.FontColor);
                valueRect.MoveY(-lineHeight);
                if (valueRect.yMax < 0) break;
            }

            if (IMInput.IsPointerDownActive) {
                IMInput.CloseModalWindow();
            }

            return false;
        }

        public static bool DropDownArea(Rect rect, IList<string> list, out int selected, bool enter = false) {
            var lineHeight = IMStyle.SpacedTextHeight;
            var fontSize = IMStyle.FontSize;
            var mesh = Mesh;
            rect.MoveY(-lineHeight);
            selected = -1;
            var alreadySelected = false;
            for (var index = 0; index < list.Count; index++) {
                if (!alreadySelected && rect.ContainsMouse()) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                    alreadySelected = true;
                    _tempSelected = index;
                    if (enter || TryConsumeLeftMouseDown()) {
                        selected = index;
                        return true;
                    }
                }
                else if (index == _tempSelected) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                    if (enter) {
                        selected = index;
                        return true;
                    }
                }
                else {
                    mesh.AddRect(rect, new UiColor(40, 40, 40, 255));
                }

                mesh.AddText(rect, fontSize, list[index], IMStyle.FontColor);
                rect.MoveY(-lineHeight);
                if (rect.yMax < 0) break;
            }

            return false;
        }

        public static bool DropDownArea(Rect rect, List<(int, string)> list, out int selected, bool enter = false) {
            var lineHeight = IMStyle.SpacedTextHeight;
            var fontSize = IMStyle.FontSize;
            var mesh = Mesh;
            rect.MoveY(-lineHeight);
            var alreadySelected = false;
            selected = -1;

            for (var index = 0; index < list.Count; index++) {
                if (!alreadySelected && rect.ContainsMouse()) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                    alreadySelected = true;
                    _tempSelected = index;
                    if (enter || TryConsumeLeftMouseDown()) {
                        selected = list[index].Item1;
                        return true;
                    }
                }
                else if (index == _tempSelected) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                    if (enter) {
                        selected = list[index].Item1;
                        return true;
                    }
                }
                else {
                    mesh.AddRect(rect, new UiColor(40, 40, 40, 255));
                }

                mesh.AddText(rect, fontSize, list[index].Item2, IMStyle.FontColor);
                rect.MoveY(-lineHeight);
                if (rect.yMax < 0) break;
            }

            return false;
        }

        static List<(int, string)> _queried = new();
        static int _tempSelected;

        public static bool InputDropdownField<TList>(TList list, ref int selected, bool forceUpdate = false)
            where TList : IList<string> {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) {
                return false;
            }
            var focusState = DropDownFocusButton(rect, selected < 0 ? Str.Empty : list[selected]);
            if (focusState == FocusState.NotFocus) return false;
            if (focusState == FocusState.NewFocus) {
                _inputFieldData = default;
                _tempSelected = 0;
                return false;
            }

            using var p = ModalWindowArea.New();
            var lineHeight = IMStyle.SpacedTextHeight;
            rect.MoveY(-lineHeight);

            var inputState = DrawAndGetInputState(rect);
            var input = _inputBuffer.AsSpan()[.._inputFieldData.Length];
            var enter = false;
            if ((forceUpdate && inputState == InputState.NoChange) || inputState == InputState.Change) {
                _tempSelected = 0;
                list.QueryContains(input, _queried, StringComparison.OrdinalIgnoreCase);
            }
            else if (inputState == InputState.Enter) {
                enter = true;
                if (!input.IsEmpty) list.QueryContains(input, _queried, StringComparison.OrdinalIgnoreCase);
            }

            if (!input.IsEmpty) {
                if (DropDownArea(rect, _queried, out selected, enter)) {
                    IMInput.CloseModalWindow();
                    return true;
                }

                if (IMInput.IsPointerDownActive) {
                    IMInput.CloseModalWindow();
                }

                return false;
            }

            if (DropDownArea(rect, list, out selected, enter)) {
                IMInput.CloseModalWindow();
                return true;
            }

            if (IMInput.IsPointerDownActive) {
                IMInput.CloseModalWindow();
            }

            return false;
        } 
        public static bool InputDropdownButton<TList>(Str label,TList list, ref int selected, bool forceUpdate = false)
            where TList : IList<string> {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) {
                return false;
            }

            selected = Mathf.Clamp(selected,0, list.Count);
            var focusState = FocusButton(rect, label);
            if (focusState == FocusState.NotFocus) return false;
            if (focusState == FocusState.NewFocus) {
                _inputFieldData = default;
                _tempSelected = 0;
                return false;
            }

            using var p = ModalWindowArea.New();
            var lineHeight = IMStyle.SpacedTextHeight;
            rect.MoveY(-lineHeight);

            var inputState = DrawAndGetInputState(rect);
            var input = _inputBuffer.AsSpan()[.._inputFieldData.Length];
            var enter = false;
            if ((forceUpdate && inputState == InputState.NoChange) || inputState == InputState.Change) {
                _tempSelected = 0;
                list.QueryContains(input, _queried, StringComparison.OrdinalIgnoreCase);
            }
            else if (inputState == InputState.Enter) {
                enter = true;
                if (!input.IsEmpty) list.QueryContains(input, _queried, StringComparison.OrdinalIgnoreCase);
            }

            if (!input.IsEmpty) {
                if (DropDownArea(rect, _queried, out selected, enter)) {
                    IMInput.CloseModalWindow();
                    return true;
                }

                if (IMInput.IsPointerDownActive) {
                    IMInput.CloseModalWindow();
                }

                return false;
            }

            if (DropDownArea(rect, list, out selected, enter)) {
                IMInput.CloseModalWindow();
                return true;
            }

            if (IMInput.IsPointerDownActive) {
                IMInput.CloseModalWindow();
            }

            return false;
        }


        public static bool VerticalTabs(IList<string> list, ref int selected) {
            var lineHeight = IMStyle.SpacedTextHeight;
            var fontSize = IMStyle.FontSize;
            var mesh = Mesh;
            var alreadySelected = false;
            for (var index = 0; index < list.Count; index++) {
                if (!Current.TryGetNextRect(lineHeight, out var rect)) break;
                if (!alreadySelected && selected == index) {
                    mesh.AddRect(rect, new UiColor(40, 156, 200, 255));
                }
                else if (!alreadySelected && rect.ContainsActiveMouse()) {
                    mesh.AddRect(rect, new UiColor(20, 20, 20, 255));
                    if (TryConsumeLeftMouseDown()) {
                        selected = index;
                        alreadySelected = true;
                    }
                }
                else {
                    mesh.AddRect(rect, new UiColor(40, 40, 40, 255));
                }

                mesh.AddText(rect, fontSize, list[index], IMStyle.FontColor);
            }

            return alreadySelected;
        }


        public static void Label(Str label) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            Mesh.AddText(rect, IMStyle.FontSize, label, IMStyle.FontColor);
        }

        public static void Label(Str label1, Str label2) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            Span<char> label = stackalloc char[label1.Length + label2.Length];
            label1.CopyTo(label[..label1.Length]);
            label2.CopyTo(label[label1.Length..]);
            Mesh.AddText(rect, IMStyle.FontSize, label, IMStyle.FontColor);
        }

        public static bool Label(Str label1, Str label2, UiColor color) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            Mesh.AddText(rect, IMStyle.FontSize, label1, label2, color);
            return true;
        }

        public static void Label(Rect rect, Str label1, Str label2, UiColor color) {
            Mesh.AddText(rect, IMStyle.FontSize, label1, label2, color);
        }

        public static void Label(Rect rect, Str label) {
            Mesh.AddText(rect, IMStyle.FontSize, label, IMStyle.FontColor);
        }


        public static void Label(Str label, UiColor color) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            Mesh.AddText(rect, IMStyle.FontSize, label, color);
        }

        public static void Label(Str label, ReadOnlySpan<(int length, UiColor color)> colors) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            Mesh.AddText(rect, IMStyle.FontSize, label, colors);
        }


        public static void Label(Str label, int value) {
            var span = ScratchBuffer.AsSpan();
            label.CopyTo(span[..label.Length]);
            var remain = span[label.Length..];
            if (value.TryFormat(remain, out var count)) {
                span = span[..(label.Length + count)];
                Label(span);
            }
        }

        public static void Label(Str label, int value, UiColor labelColor, UiColor valueColor) {
            var span = ScratchBuffer.AsSpan();
            label.CopyTo(span[..label.Length]);
            var remain = span[label.Length..];
            if (value.TryFormat(remain, out var charsWritten)) {
                ReadOnlySpan<(int, UiColor)> colors = stackalloc (int, UiColor)[2]
                    {(label.Length, labelColor), (charsWritten, valueColor)};
                span = span[..(label.Length + charsWritten)];
                Label(span, colors);
            }
        }

        public static void Label<T, TValue>(Str label, TValue value)
            where T : struct, IFormatter<TValue> {
            var span = ScratchBuffer.AsSpan();
            label.CopyTo(span[..label.Length]);
            var remain = span[label.Length..];
            if (default(T).TryFormat(value, remain, out var charsWritten)) {
                span = span[..(label.Length + charsWritten)];
                if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
                Mesh.AddText(rect, IMStyle.FontSize, span, IMStyle.FontColor);
            }
        }

        public static void Label<T, TValue>(Str label, TValue value, UiColor labelColor,
            UiColor valueColor)
            where T : struct, IFormatter<TValue> {
            var span = ScratchBuffer.AsSpan();
            label.CopyTo(span[..label.Length]);
            var remain = span[label.Length..];
            if (default(T).TryFormat(value, remain, out var charsWritten)) {
                ReadOnlySpan<(int, UiColor)> colors = stackalloc (int, UiColor)[2]
                    {(label.Length, labelColor), (charsWritten, valueColor)};
                span = span[..(label.Length + charsWritten)];
                Label(span, colors);
            }
        }

        public static void Label<T, TValue>(TValue value) where T : struct, IFormatter<TValue> {
            var span = ScratchBuffer.AsSpan();
            if (default(T).TryFormat(value, span, out var charsWritten)) {
                Label(span[..charsWritten]);
            }
        }

        public static void Label(Str label, float value) {
            var span = ScratchBuffer.AsSpan();
            label.CopyTo(span.Slice(0, label.Length));
            var remain = span.Slice(label.Length);
            if (value.TryFormat(remain, out var count)) {
                span = span.Slice(0, label.Length + count);
                Label(span);
            }
        }

        public static void Label(float value, Str format = default,
            IFormatProvider formatProvider = default) {
            var span = ScratchBuffer.AsSpan();
            if (value.TryFormat(span, out var count, format, formatProvider)) {
                span = span.Slice(0, count);
                Label(span);
                span.Clear();
            }
        }

        public static bool MethodButton(this object o, string name) {
            if (Button(name)) {
                var method = o.GetType().GetMethod(name,
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                    BindingFlags.Static);
                if (method == null) {
                    Debug.LogAssertion(name + "is null");
                    return false;
                }
                method.Invoke(o, null);
                return true;
            }
            return false;
        }
      

        public static bool MethodButton(Type type, string name) {
            if (Button(name)) {
                var method = type.GetMethod(name,
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null) {
                    Debug.LogAssertion(name + "is null");
                    return false;
                }

                method.Invoke(null, null);
                return true;
            }

            return false;
        }

        public static bool Button(Str label) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            return Button(rect, label);
        }

        public static bool RadioButton(ref bool on) => RadioButton(ref on, IMStyle.SpacedTextHeight / 2);

        public static bool RadioButton(ref bool on, float radius) {
            if (!Current.TryGetNextRect(2 * radius, out var rect)) return false;
            rect.width = 2 * radius;
            var contains = IMInput.CircleContainsActiveMouse(rect.center, radius);
            var pushed = contains && TryConsumeLeftMouseDown();
            on ^= pushed;
            var instance = Mesh;
            var center = rect.center;
            if (contains)
                instance.AddCircle(center, radius + 1, new UiColor(255, 255, 255, 255), radius,
                    IMStyle.FieldColor);
            else instance.AddCircle(center, radius, IMStyle.FieldColor);
            if (on) {
                instance.AddCircle(center, radius / 2,
                    new UiColor(200, 200, 200, 255));
            }

            return pushed;
        }

        public static bool CheckBox(bool on) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return on;
            var size = IMStyle.FontSize;
            rect = new Rect(rect.xMin + size / 10, rect.yMin, size, size);
            var contains = rect.ContainsActiveMouse();
            var pushed = contains && TryConsumeLeftMouseDown();
            on ^= pushed;
            var instance = Mesh;
            if (contains)
                instance.AddRect(rect, 1, new UiColor(255, 255, 255, 255), IMStyle.FieldColor);
            else instance.AddRect(rect, IMStyle.FieldColor);
            if (on) {
                var center = rect.center;
                ReadOnlySpan<Vector2> lines = stackalloc[] {
                    new Vector2(center.x - size / 3, center.y),
                    new Vector2(center.x - size / 8, center.y - size * (5f / 24)),
                    new Vector2(center.x + size / 3, center.y + size / 4)
                };
                instance.AddLines(lines, size / 10, IMStyle.FontColor);
            }

            return on;
        }

        public static bool BoolField(Str label, ref bool on) {
            if (!Current.TryGetNextRect(out var rect)) return false;
            return BoolField(rect, label, ref on);
        }

        public static bool BoolField(Rect rect, Str label, ref bool on) {
            var success = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            if (!success) {
                return false;
            }

            var size = IMStyle.FontSize;
            valueRect = new Rect(valueRect.xMin + size / 10, valueRect.yMin + size / 10, size, size);
            var contains = valueRect.ContainsActiveMouse();
            var pushed = contains && TryConsumeLeftMouseDown();
            on ^= pushed;
            var instance = Mesh;
            if (contains)
                instance.AddRect(valueRect, 1, new UiColor(255, 255, 255, 255), IMStyle.FieldColor);
            else instance.AddRect(valueRect, IMStyle.FieldColor);
            if (on) {
                var center = valueRect.center;
                ReadOnlySpan<Vector2> lines = stackalloc[] {
                    new Vector2(center.x - size / 3, center.y),
                    new Vector2(center.x - size / 8, center.y - size * (5f / 24)),
                    new Vector2(center.x + size / 3, center.y + size / 4)
                };
                instance.AddLines(lines, size / 10, IMStyle.FontColor);
            }

            return pushed;
        }

        public static bool CheckBox(ref bool on) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return on;
            var size = IMStyle.FontSize;
            rect = new Rect(rect.xMin + size / 10, rect.yMin, size, size);
            var contains = rect.ContainsActiveMouse();
            var pushed = contains && TryConsumeLeftMouseDown();
            on ^= pushed;
            var instance = Mesh;
            if (contains)
                instance.AddRect(rect, 1, new UiColor(255, 255, 255, 255), IMStyle.FieldColor);
            else instance.AddRect(rect, IMStyle.FieldColor);
            if (on) {
                var center = rect.center;
                ReadOnlySpan<Vector2> lines = stackalloc[] {
                    new Vector2(center.x - size / 3, center.y),
                    new Vector2(center.x - size / 8, center.y - size * (5f / 24)),
                    new Vector2(center.x + size / 3, center.y + size / 4)
                };
                instance.AddLines(lines, size / 10, IMStyle.FontColor);
            }

            return pushed;
        }

        public static void HorizontalGradient(UiColor left, UiColor right) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            Mesh.AddHorizontalGradient(rect, left, right);
        }

        public static void HorizontalGradient(Rect rect, UiColor left, UiColor right) {
            Mesh.AddHorizontalGradient(rect, left, right);
        }

        public static void Gradient(float height, UiColor topLeftColor, UiColor topRightColor, UiColor bottomLeftColor,
            UiColor bottomRightColor) {
            if (!Current.TryGetNextRect(height, out var rect)) return;
            Mesh.AddGradient(rect, topLeftColor, topRightColor, bottomLeftColor, bottomRightColor);
        }

        public static void Gradient(Rect rect, UiColor topLeftColor, UiColor topRightColor, UiColor bottomLeftColor,
            UiColor bottomRightColor) {
            Mesh.AddGradient(rect, topLeftColor, topRightColor, bottomLeftColor, bottomRightColor);
        }


        public static bool Slider(ref float value, float min, float max) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            return Slider(rect, ref value, min, max);
        }

        public static bool Slider(ref int value, int min, int max) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            return Slider(ref value, min, max, rect);
        }

        public static void Quad(Rect rect) {
            Mesh.AddRect(rect, IMStyle.BasicColor);
        }

        public static void Quad(Rect rect, UiColor topLeftColor, UiColor topRightColor, UiColor bottomLeftColor,
            UiColor bottomRightColor) {
            Mesh.AddRect(rect, IMStyle.BasicColor);
        }


        public static bool Button(Rect rect, Str label) {
            var canPush = rect.CanPush();
            var pushed = canPush && TryConsumeLeftMouseDown();
            var instance = Mesh;
            var width = Mathf.Max(rect.height / 20, 1);
            instance.AddRect(rect, width, IMStyle.ButtonColor, canPush ? IMStyle.HoveredButtonColor : IMStyle.UnHoveredButtonColor);
            instance.AddCenterText(rect.center,rect.width-2*width, IMStyle.FontSize, label, IMStyle.FontColor);
            return pushed;
        }

        public static bool Foldout(Str label, bool foldout) {
            if (!Current.TryGetNextRect(out var rect)) return false;
            return Foldout(rect, label, foldout);
        }
        public static bool Foldout(Str label,ref  bool foldout) {
            if (!Current.TryGetNextRect(out var rect)) return false;
            return foldout=Foldout(rect, label, foldout);
        }

        public static bool Foldout(Rect rect, Str label, bool foldout) {
            var canPush = rect.CanPush();
            var pushed = canPush && TryConsumeLeftMouseDown();
            var result = foldout ^ pushed;
            var instance = Mesh;
            var width = Mathf.Max(rect.height / 20, 1);
            instance.AddRect(rect, width, IMStyle.ButtonColor, canPush ?IMStyle.HoveredButtonColor : IMStyle.UnHoveredButtonColor);

            var fontSize = IMStyle.FontSize;
            var fontColor = IMStyle.FontColor;
            if (!label.IsEmpty) {
                var textRect = new Rect(rect.x + width + fontSize * 1.4f, rect.y + width,
                    rect.width - 2 * width - fontSize * 1.4f,
                    rect.height - 2 * width);
                instance.AddText(textRect, fontSize, label, fontColor);
            }

            if (result)
                instance.AddInvertedTriangle(new Vector2(rect.xMin + fontSize * 0.7f, rect.yMin + rect.height / 2),
                    fontSize * 0.5f, fontColor);
            else
                instance.AddRightPointingTriangle(new Vector2(rect.xMin + fontSize * 0.7f, rect.yMin + rect.height / 2),
                    fontSize * 0.5f, fontColor);
            return result;
        }

        public static FocusState DropDownFocusButton(Rect rect, Str label) {
            var focusState = rect.GetModalWindowState();
            var canPush = rect.CanPush();
            var instance = Mesh;
            var width = Mathf.Max(rect.height / 20, 1);
            instance.AddRect(rect, width, IMStyle.ButtonColor, canPush ? IMStyle.HoveredButtonColor : IMStyle.UnHoveredButtonColor);

            var fontSize = IMStyle.FontSize;
            var fontColor = IMStyle.FontColor;
            if (!label.IsEmpty) {
                var textRect = new Rect(rect.x + width, rect.y + width, rect.width - 2 * width - fontSize,
                    rect.height - 2 * width);
                instance.AddText(textRect, fontSize, label, fontColor);
            }

            instance.AddInvertedTriangle(new Vector2(rect.xMax - fontSize * 0.7f, rect.yMin + rect.height / 2),
                fontSize * 0.5f, fontColor);
            return focusState;
        }
        public static FocusState FocusButton(Rect rect, Str label) {
            var focusState = rect.GetModalWindowState();
            var canPush = rect.CanPush();
            var instance = Mesh;
            var width = Mathf.Max(rect.height / 20, 1);
            instance.AddRect(rect, width, IMStyle.ButtonColor, canPush ? IMStyle.HoveredButtonColor : IMStyle.UnHoveredButtonColor);
            var fontSize = IMStyle.FontSize;
            var fontColor = IMStyle.FontColor;
            if (!label.IsEmpty) {
               
                instance.AddCenterText(rect.center,rect.width-2*width, fontSize, label, fontColor);
            }

            return focusState;
        }


        public enum InputState {
            InputIsNull,
            Change,
            NoChange,
            Enter,
        }


        public static InputState DrawAndGetInputState(Rect rect) {
            ref var data = ref _inputFieldData;
            var buffer = _inputBuffer.AsSpan();
            data.FocusTime += Time.unscaledDeltaTime;
            var instance = Mesh;
            instance.AddRect(rect, UiColor.Black);
            var inputString = IMInput.InputString;
            if (!string.IsNullOrEmpty(inputString) | TryGetPaste(out var pasteText)) {
                var preCaretLength = data.CaretPosition;
                var postCaretLength = data.Length - preCaretLength;
                var postCaret = ScratchBuffer.AsSpan(0, postCaretLength);
                if (postCaretLength != 0) buffer.Slice(data.CaretPosition, postCaretLength).CopyTo(postCaret);
                var enter = false;
                if (pasteText != null) {
                    foreach (var c in pasteText) {
                        if (c is '\n' or '\r') {
                            IMInput.LoseFocus();
                            enter = true;
                            break;
                        }

                        if (preCaretLength < buffer.Length) {
                            buffer[preCaretLength++] = c;
                        }
                    }
                }

                if (inputString != null)
                    foreach (var c in inputString) {
                        if (c == '\b') {
                            if (0 < preCaretLength) {
                                --preCaretLength;
                            }
                        }
                        else if (c is '\n' or '\r') {
                            IMInput.LoseFocus();
                            enter = true;
                            break;
                        }
                        else if (preCaretLength < buffer.Length) {
                            buffer[preCaretLength++] = c;
                        }
                    }

                if (preCaretLength + postCaretLength == 0) {
                    data.Length = 0;
                    data.CaretPosition = 0;

                    return enter ? InputState.Enter : InputState.InputIsNull;
                }

                float caretPos;
                if (postCaretLength != 0) {
                    caretPos = instance.CalculateLength(buffer[..preCaretLength], IMStyle.FontSize);
                    postCaret.CopyTo(buffer.Slice(preCaretLength, postCaretLength));
                    instance.AddText(rect,IMStyle.FontSize, buffer[..(preCaretLength + postCaretLength)], IMStyle.FontColor);
                }
                else {
                    caretPos = instance.AddText(rect,IMStyle.FontSize, buffer[..preCaretLength], IMStyle.FontColor);
                }

                if (1.5f < (data.FocusTime * 3) % 3) {
                    instance.AddRect(new Rect(rect.xMin + caretPos, rect.yMin, 2, IMStyle.SpacedTextHeight),
                        IMStyle.FontColor);
                }

                data.Length = preCaretLength + postCaretLength;
                data.CaretPosition = preCaretLength;
                return enter ? InputState.Enter : InputState.Change;
            }

            if (data.Length == 0) {
                if (1.5f < (data.FocusTime * 3) % 3) {
                    instance.AddRect(new Rect(rect.xMin, rect.yMin, 2, IMStyle.SpacedTextHeight),
                        IMStyle.FontColor);
                }

                return InputState.InputIsNull;
            }

            if (GetLeftArrowKeyDown) {
                data.FocusTime = 0.6f;
                --data.CaretPosition;
            }

            if (GetRightArrowKeyDown) {
                data.FocusTime = 0.6f;
                ++data.CaretPosition;
            }

            data.CaretPosition = Mathf.Clamp(data.CaretPosition, 0, data.Length);
            var drawCaret = 0.5f < (data.FocusTime) % 1;
            if (drawCaret) {
                float caretPos;
                if (data.CaretPosition != data.Length) {
                    caretPos = instance.AddTextNoMask(rect, IMStyle.FontSize, buffer[..data.CaretPosition],
                        IMStyle.FontColor);
                    instance.AddTextNoMask(new Rect(rect.xMin + caretPos, rect.yMin, 2, IMStyle.SpacedTextHeight),
                        IMStyle.FontSize,
                        buffer[data.CaretPosition..data.Length], IMStyle.FontColor);
                }
                else {
                    caretPos = instance.AddTextNoMask(rect, IMStyle.FontSize, buffer[..data.Length],
                        IMStyle.FontColor);
                }

                instance.AddRect(new Rect(rect.xMin + caretPos, rect.yMin, 2, IMStyle.SpacedTextHeight),
                    IMStyle.FontColor);
            }
            else instance.AddTextNoMask(rect, IMStyle.FontSize, buffer[..data.Length], IMStyle.FontColor);

            return InputState.NoChange;
        }


        public static double HoldingValue;
        public static float ValueHoldingPosX;
        public static float ValueHoldingPosY;
        static InputFieldData _inputFieldData;
        static char[] _inputBuffer = new char[1024];


        public static bool IntField(Str label, ref int value, int min = int.MinValue,
            int max = int.MaxValue) {
            if (!Current.TryGetNextRect(out var rect)) return false;
            return IntField(rect, label, ref value, min, max);
        }

        public static bool IntField(Rect rect, Str label, ref int value, int min = int.MinValue,
            int max = int.MaxValue) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            var contains = labelRect.ContainsActiveMouse();
            _LRCursorNeeded |= contains;
            var focusState = labelRect.GetFocusState();
            if (focusState == FocusState.NewFocus) {
                HoldingValue = value;
                ValueHoldingPosX = ScreenMousePos.x;
                var mesh = Mesh;
                mesh.AddRect(valueRect, UiColor.Black);
                var buffer = ScratchBuffer.AsSpan();
                value.TryFormat(buffer, out var charsWritten);
                mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }
            else if (focusState == FocusState.Focus) {
                _LRCursorNeeded = true;
                var changed = false;
                var deltaX = ScreenMousePos.x - ValueHoldingPosX;
                var v = (int) (HoldingValue + deltaX / IMStyle.FontSize*IMStyle.DragNumberScale);
                v = Mathf.Clamp(v, min, max);
                if (v != value) {
                    value = v;
                    changed = true;
                }

                if (showValue) {
                    var mesh = Mesh;
                    mesh.AddRect(valueRect, UiColor.Black);
                    var buffer = ScratchBuffer.AsSpan();
                    value.TryFormat(buffer, out var charsWritten);
                    mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
                    return changed;
                }
            }

            if (!showValue) return false;
            return InputField<Int32ParserFormatter, int>(valueRect, ref value);
        }

        public static void IntField(Str label, int value) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            IntField(rect, label, value);
        }

        public static void IntField(Rect rect, Str label, int value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            var mesh = Mesh;
            mesh.AddText(labelRect, IMStyle.FontSize, label, IMStyle.FontColor);
            if (showValue) {
                var buffer = ScratchBuffer.AsSpan();
                value.TryFormat(buffer, out var charsWritten);
                mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }
        }

        public static void FloatField(Str label, float value) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            FloatField(rect, label, value);
        }

        public static void FloatField(Rect rect, Str label, float value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            var mesh = Mesh;
            mesh.AddText(labelRect, IMStyle.FontSize, label, IMStyle.FontColor);
            if (showValue) {
                var buffer = ScratchBuffer.AsSpan();
                value.TryFormat(buffer, out var charsWritten);
                mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }
        }
        public static void FloatField(Rect rect,float labelWidth, Str label, float value) {
            var showValue = rect.TryDivide(labelWidth,out var labelRect, out var valueRect);
            var mesh = Mesh;
            mesh.AddText(labelRect, IMStyle.FontSize, label, IMStyle.FontColor);
            if (showValue) {
                var buffer = ScratchBuffer.AsSpan();
                value.TryFormat(buffer, out var charsWritten);
                mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }
        }

        public static void StringField(Rect rect, Str label, string value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            var mesh = Mesh;
            mesh.AddText(labelRect, IMStyle.FontSize, label, IMStyle.FontColor);
            if (showValue) {
                var buffer = ScratchBuffer.AsSpan(..value.Length);
                value.AsSpan().CopyTo(buffer);
                mesh.AddText(valueRect, IMStyle.FontSize, buffer, IMStyle.FontColor);
            }
        }

        public static void StringField(Str label, string value) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return;
            StringField(rect, label, value);
        }

        public static void ValueField<TFormatter, TValue>(Rect rect, Str label, TValue value)
            where TFormatter : struct, IFormatter<TValue> {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            var mesh = Mesh;
            mesh.AddText(labelRect, IMStyle.FontSize, label, IMStyle.FontColor);
            if (!showValue) return;
            var span = ScratchBuffer.AsSpan();
            if (default(TFormatter).TryFormat(value, span, out var charsWritten)) {
                mesh.AddText(valueRect, IMStyle.FontSize, span[.. charsWritten], IMStyle.FontColor);
            }
        }

        public static bool FloatField(Rect rect, Str label, ref float value, float min = float.MinValue,
            float max = float.MaxValue) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            var contains = labelRect.ContainsActiveMouse();
            _LRCursorNeeded |= contains;
            var focusState = labelRect.GetFocusState();
            if (focusState == FocusState.NewFocus) {
                HoldingValue = value;
                ValueHoldingPosX = ScreenMousePos.x;
                if (showValue) {
                    var mesh = Mesh;
                    mesh.AddRect(valueRect, UiColor.Black);
                    var buffer = ScratchBuffer.AsSpan();
                    value.TryFormat(buffer, out var charsWritten);
                    mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
                }
            }
            else if (focusState == FocusState.Focus) {
                _LRCursorNeeded = true;
                var changed = false;
                var deltaX = ScreenMousePos.x - ValueHoldingPosX;
                var v = (float) (HoldingValue + deltaX / IMStyle.FontSize*IMStyle.DragNumberScale);
                v = Mathf.Clamp(v, min, max);
                if (v != value) {
                    value = Mathf.Floor(v * 100) / 100;
                    changed = true;
                }

                if (showValue) {
                    var mesh = Mesh;
                    mesh.AddRect(valueRect, UiColor.Black);
                    var buffer = ScratchBuffer.AsSpan();
                    value.TryFormat(buffer, out var charsWritten);
                    mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
                }

                return changed;
            }

            if (!showValue) return false;
            return InputField<SingleParserFormatter, float>(valueRect, ref value);
        }

        public static bool FloatField(Rect rect, float labelWidth, Str label, ref float value) {
            var showValue = rect.TryDivide(labelWidth, out var labelRect, out var valueRect);
            Label(labelRect, label);
            var contains = labelRect.ContainsActiveMouse();
            _LRCursorNeeded |= contains;
            var focusState = labelRect.GetFocusState();
            if (focusState == FocusState.NewFocus) {
                HoldingValue = value;
                ValueHoldingPosX = ScreenMousePos.x;
                if (showValue) {
                    var mesh = Mesh;
                    mesh.AddRect(valueRect, UiColor.Black);
                    var buffer = ScratchBuffer.AsSpan();
                    value.TryFormat(buffer, out var charsWritten);
                    mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
                }
            }
            else if (focusState == FocusState.Focus) {
                _LRCursorNeeded = true;
                var changed = false;
                var deltaX = ScreenMousePos.x - ValueHoldingPosX;
                var v = (float) (HoldingValue + deltaX / IMStyle.FontSize*IMStyle.DragNumberScale);
                v = Mathf.Floor(v * 100) / 100;
                if (v != value) {
                    value = v;
                    changed = true;
                }

                if (showValue) {
                    var mesh = Mesh;
                    mesh.AddRect(valueRect, UiColor.Black);
                    var buffer = ScratchBuffer.AsSpan();
                    value.TryFormat(buffer, out var charsWritten);
                    mesh.AddText(valueRect, IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
                }

                return changed;
            }

            if (!showValue) return false;
            return InputField<SingleParserFormatter, float>(valueRect, ref value);
        }

        public static bool FloatField(Str label, ref float value, float min = float.MinValue,
            float max = float.MaxValue) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;

            return FloatField(rect, label, ref value, min, max);
        }

        public static bool Vector2Field(Rect rect, Str label, ref Vector2 value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            if (!showValue) return false;
            var xRect = valueRect;
            xRect.width = valueRect.width / 2;
            var change = FloatField(xRect, IMStyle.FontSize*1.2f, "X", ref value.x);
           
            var yRect = new Rect(xRect.xMax, xRect.yMin, xRect.width , xRect.height);
            if (0 < yRect.width) change |= FloatField(yRect, IMStyle.FontSize*1.2f, "Y", ref value.y);
            return change;
        }
      
        public static void Vector2Field(Rect rect, Str label,  Vector2 value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            if (!showValue) return ;
            var xRect = valueRect;
            var padding = IMStyle.FontSize / 4f;
            xRect.width = valueRect.width / 2-padding;
            FloatField(xRect, IMStyle.FontSize, "X",  value.x);
            var yRect = new Rect(xRect.xMax+ padding, xRect.yMin, xRect.width , xRect.height);
            if (0 < yRect.width)  FloatField(yRect, IMStyle.FontSize, "Y",  value.y);
        }

        public static bool Vector2Field(Str label, ref Vector2 value) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            return Vector2Field(rect, label, ref value);
        }
        public static bool Vector2Field(Str label,  Vector2 value,out Vector2 newValue) {
            if (Vector2Field(label,ref value)) {
                newValue = value;
                return true;
            }
            newValue = value;
            return false;
        }
        public static bool Vector3Field(Rect rect, Str label, ref Vector3 value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            if (!showValue) return false;
            var xRect = valueRect;
            xRect.width = valueRect.width / 3;
            var change = FloatField(xRect, IMStyle.FontSize*1.2f, "X", ref value.x);
            var yRect = new Rect(xRect.xMax, xRect.yMin, xRect.width , xRect.height);
            change |= FloatField(yRect, IMStyle.FontSize * 1.2f, "Y", ref value.y);
            var zRect = new Rect(yRect.xMax, yRect.yMin, yRect.width , yRect.height);
            change |= FloatField(zRect, IMStyle.FontSize * 1.2f, "Z", ref value.z);
            return change;
        }
        
        public static bool Vector3Field(Str label, ref Vector3 value) {
            if (!Current.TryGetNextRect(IMStyle.SpacedTextHeight, out var rect)) return false;
            return Vector3Field(rect, label, ref value);
        }
        public static (FocusState state, bool containsMouse) GetFocusState(Rect rect, bool needMouse = true) {
            var containsMouse = rect.ContainsActiveMouse();
            return (rect.GetFocusState(), containsMouse);
        }
        public static bool Vector3Field(Str label,  Vector3 value,out Vector3 newValue) {
            if (Vector3Field(label,ref value)) {
                newValue = value;
                return true;
            }
            newValue = value;
            return false;
        }
        public static bool InputField<T, TValue>(Rect rect, ref TValue value)
            where T : struct, IParserFormatter<TValue> {
            var focusState = rect.GetFocusState(false);
            if (focusState == FocusState.Focus) {
                var state = DrawAndGetInputState(rect);
                var buffer = _inputBuffer.AsSpan();
                switch (state) {
                    case InputState.InputIsNull: {
                        value = default;
                        return true;
                    }
                    case InputState.Change: {
                        default(T).TryParse(buffer[.._inputFieldData.Length], out value);
                        return true;
                    }
                    case InputState.NoChange: return false;
                    case InputState.Enter: return false;
                    default: throw new NotImplementedException();
                }
            }

            var mesh = Mesh;
            mesh.AddRect(rect, UiColor.Black);
            if (focusState == FocusState.NewFocus) {
                var buffer = _inputBuffer.AsSpan();
                default(T).TryFormat(value, buffer, out var charsWritten);
                _inputFieldData.Length = charsWritten;
                _inputFieldData.CaretPosition = charsWritten;
                mesh.AddText(rect,IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }
            else {
                var buffer = ScratchBuffer.AsSpan();
                default(T).TryFormat(value, buffer, out var charsWritten);
                Mesh.AddText(rect,IMStyle.FontSize, buffer[..charsWritten], IMStyle.FontColor);
            }

            return false;
        }

        public static bool StringField(Str label, ref string value) {
            if (!Current.TryGetNextRect(out var rect)) return false;
            return StringField(rect, label, ref value);
        }

        public static bool StringField(Rect rect, Str label, ref string value) {
            var showValue = rect.DivideToLabelAndValue(out var labelRect, out var valueRect);
            Label(labelRect, label);
            if (!showValue) return false;
            return InputField(valueRect, ref value);
        }

        public static bool InputField(Rect rect, ref string value) {
            var focusState = rect.GetFocusState(false);
            if (focusState == FocusState.Focus) {
                var state = DrawAndGetInputState(rect);
                var buffer = _inputBuffer.AsSpan();
                switch (state) {
                    case InputState.InputIsNull: {
                        value = default;
                        return true;
                    }
                    case InputState.Change: {
                        value = buffer[.._inputFieldData.Length].ToString();
                        return true;
                    }
                    case InputState.NoChange: return false;
                    case InputState.Enter: return false;
                    default: throw new NotImplementedException();
                }
            }

            var mesh = Mesh;
            mesh.AddRect(rect, UiColor.Black);
            if (focusState == FocusState.NewFocus) {
                if (string.IsNullOrEmpty(value)) {
                    _inputFieldData.Length = 0;
                    _inputFieldData.CaretPosition = 0;
                    return false;
                }

                var buffer = _inputBuffer.AsSpan();
                var length = value.Length;
                value.AsSpan().CopyTo(buffer[..length]);
                _inputFieldData.Length = length;
                _inputFieldData.CaretPosition = length;
                mesh.AddText(rect,IMStyle.FontSize, value, IMStyle.FontColor);
            }
            else {
                Mesh.AddText(rect,IMStyle.FontSize, value, IMStyle.FontColor);
            }

            return false;
        }


        public static void VerticalScroll(Rect rect, ref float value, float size, out float top, out float bottom) {
            var instance = Mesh;
            var barHeight = rect.height * size;
            var barWidth = rect.width * 0.95f;
            var remainHeight = rect.height * (1 - size);
            instance.AddRect(rect,  UiColor.Black);
            var focusState = rect.GetFocusState();
            if (focusState == FocusState.NewFocus) {
                ValueHoldingPosY = ScreenMousePos.y;
                var currentY = ValueHoldingPosY;
                var barYMin = rect.yMin + (1 - value) * remainHeight;
                if (currentY < barYMin) {
                    value = 1 + (rect.yMin - currentY) / remainHeight;
                }
                else if (barYMin + barHeight < currentY) {
                    value = (rect.yMax - currentY) / remainHeight;
                }

                HoldingValue = value;
            }
            else if (focusState == FocusState.Focus) {
                value = (float) HoldingValue - (ScreenMousePos.y - ValueHoldingPosY) / remainHeight;
            }

            {
                value = Mathf.Clamp(value, 0, 1);
                var barRect = new Rect(rect.xMin + rect.width * 0.025f,
                    rect.yMin + (1 - value) * rect.height * (1 - size),
                    barWidth,
                    barHeight);
                instance.AddRect(barRect, IMStyle.BasicColor);
                top = value * (1 - size);
                bottom = 1 - (1 - value) * (1 - size);
            }
        }

        public static bool Slider(Rect rect, ref float value, float min, float max) {
            var instance = Mesh;
            var currentValue = value = Mathf.Clamp(value, min, max);
            var ratio = (currentValue - min) / (max - min);
            var halfWidth = 3f;
            var newRectXMin = rect.xMin + halfWidth;
            var newRectXMax = rect.xMax - halfWidth;
            instance.AddRect(newRectXMin, newRectXMax, rect.yMin + rect.height / 3f, rect.yMin + rect.height / 1.5f,
                UiColor.Black);
            var rectWidth = newRectXMax - newRectXMin;
            var sliderCenterX = Mathf.Lerp(newRectXMin, newRectXMax, ratio);
            var sliderRect = new Rect(sliderCenterX - halfWidth, rect.yMin, halfWidth * 2,
                rect.height);
            var focus = rect.IsFocusActive();
            if (focus) {
                var currentX = ScreenMousePos.x;
                currentX = Mathf.Clamp(currentX, newRectXMin, newRectXMax);
                sliderRect.MoveX(currentX - sliderCenterX);
                if (currentX == newRectXMax) value = max;
                else if (currentX == newRectXMin) value = min;
                else value = min + (max - min) * ((currentX - newRectXMin) / rectWidth);
                instance.AddRect(sliderRect, IMStyle.BasicColor);
                return true;
            }

            instance.AddRect(sliderRect, IMStyle.BasicColor);
            return false;
        }

        public static bool Slider(ref int value, int min, int max, Rect rect) {
            var instance = Mesh;
            var currentValue = value = Mathf.Clamp(value, min, max);
            var ratio = (float) (currentValue - min) / (max - min);
            var halfWidth = 3f;
            var newRectXMin = rect.xMin + halfWidth;
            var newRectXMax = rect.xMax - halfWidth;
            instance.AddRect(newRectXMin, newRectXMax, rect.yMin + rect.height / 3f, rect.yMin + rect.height / 1.5f,
                UiColor.Black);
            var rectWidth = newRectXMax - newRectXMin;

            var sliderCenterX = Mathf.Lerp(newRectXMin, newRectXMax, ratio);
            var sliderRect = new Rect(sliderCenterX - halfWidth, rect.yMin, halfWidth * 2,
                rect.height);
            var focus = rect.IsFocusActive();
            if (focus) {
                var currentX = ScreenMousePos.x;
                currentX = Mathf.Clamp(currentX, newRectXMin, newRectXMax);
                var range = max - min;
                value = min + Mathf.RoundToInt(range * ((currentX - newRectXMin) / rectWidth));
                currentX = (value - min) * rectWidth / range + newRectXMin;
                sliderRect.MoveX(currentX - sliderCenterX);
                instance.AddRect(sliderRect, IMStyle.BasicColor);
                return true;
            }

            instance.AddRect(sliderRect, IMStyle.BasicColor);
            return false;
        }

        public static bool SlidePad(float radius, out Vector2 value) {
            if (!Current.TryGetNextRect(radius * 2, out var rect)) {
                value = default;
                return false;
            }

            return SlidePad(rect, out value);
        }

        public static bool RectResize(ref Rect rect, Vector2 minimumSize) {
            var instance = Mesh;
            var bottomLeft = new Vector2(rect.xMax, rect.yMin);
            var leftX = bottomLeft.x - 20;
            var holdRect = new Rect(leftX, bottomLeft.y, 20, 20);
            instance.AddBottomRightTriangle(holdRect, UiColor.Blue);
            var contains = holdRect.ContainsActiveMouse();
            if (contains) {
                RequestResizeCursor() ;
            }
            var focus = holdRect.GetFocusState();
            if (focus == FocusState.Focus) {
                RequestResizeCursor() ;
                var delta = ScreenDeltaMousePos;
                var newWidth = Mathf.Max(minimumSize.x, rect.width + delta.x);
                var newHeight = Mathf.Max(minimumSize.y, rect.height - delta.y);
                rect = new Rect(rect.xMin, rect.yMax - newHeight, newWidth, newHeight);
                IMInput.FocusPosition = holdRect.center + delta;
                IMInput.FocusRect = new Rect(holdRect.xMin + delta.x, holdRect.yMin + delta.y, 20, 20);
                IMInput.IsPointerActive = false;
                return true;
            }

            return false;
        }

        public static bool SlidePad(Rect rect, out Vector2 value) {
            var slideRadius = Mathf.Min(rect.width, rect.height) / 2;
            var slidePadRadius = slideRadius / 2;
            var center = new Vector2(rect.xMin + slideRadius, rect.yMin + slideRadius);
            var contains = IMInput.CircleContainsActiveMouse(center, slideRadius);
            var hold = IMInput.IsFocusActive(center, slideRadius);
            var instance = Mesh;
            if (hold || contains)
                instance.AddCircle(center, slideRadius + 2, UiColor.White, slideRadius, new UiColor(32, 32, 32, 255));
            else instance.AddCircle(center, slideRadius, new UiColor(32, 32, 32, 255));
            Vector2 delta;
            if (hold) {
                IMInput.FocusPosition = center;
                var moveRadius = slideRadius - slidePadRadius;
                var temp = (ScreenMousePos - center) / moveRadius;
                var sqrMagnitude = temp.sqrMagnitude;
                value = sqrMagnitude <= 1 ? temp : temp / Mathf.Sqrt(sqrMagnitude);
                delta = value * moveRadius;
            }
            else {
                value = delta = default;
            }

            instance.AddCircle(center + delta, slidePadRadius, IMStyle.UnHoveredButtonColor);
            return hold;
        }
    }
}