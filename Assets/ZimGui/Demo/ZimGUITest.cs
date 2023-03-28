using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZimGui.Demo {
    public class ZimGUITest : MonoBehaviour {
        void Start() {
            var meshRenderers = FindObjectsOfType<Renderer>();
            _transforms = meshRenderers.Select(t => t.GetComponent<Transform>()).Prepend(null).ToArray();
            _names = new string[_transforms.Length];
            _names[0] = "None";
            for (int i = 1; i < _transforms.Length; i++) {
                _names[i] = _transforms[i].name;
            }
            selectedIndex = Array.IndexOf(_transforms, Target);
            IMStyle.WindowLabelLeftColor = LeftColor;
            IMStyle.WindowLabelRightColor = RightColor;
            IMStyle.FontSize = FontSize;
            SimpleConsole.Init(50);
            IMStyle.FloatFormat = null;
            IM.Add("Settings", DrawSettings);
            IM.Add("Demo", DrawDemo);
            IM.Add("SimpleLog",SimpleConsole.Draw);
        }
        static int _keyIndex = 0;
        static string[] _keyNames = {"Escape", "Tab", "F4"};
        static KeyCode[] _keyCodes = {KeyCode.Escape, KeyCode.Tab, KeyCode.F4};
        public int FontSize;
        public Color LeftColor = Color.clear;
        public Color RightColor = Color.red;

        public float MoveSpeed = 10;
        int selectedIndex;
        string[] _names;
        Transform[] _transforms;
        public Transform Target;


        public void Log() {
            SimpleConsole.Log(LogText, Color.HSVToRGB(Random.Range(0f, 1), 0.6f, 1));
        }
        public void DebugLog() {
          Debug.Log(LogText);
        }

        bool DrawSettings() {
            if (this == null) return false;
            IMStyle.FontColor = 0xFFFFFFFF;
            IM.IntField("FontSize ", ref FontSize, 9, 100);
            if (IM.Button("Apply FontSize"))
                IMStyle.FontSize = FontSize;
            if (IM.DropDownField("ResetKey",_keyNames, ref _keyIndex)) {
                DockWindow.OpenKey = _keyCodes[_keyIndex];
            }

           

            return true;
        }

        bool isMoveSpeedReadonly = false;
        [SerializeField]Vector2[] _points;
        
         bool _logFoldout = false;
         bool _pointsFoldout = false;
         bool _moveFoldout = false;
         bool _fillFoldOut = false;

        public string LogText;
        EulerHint _hint = default;
        bool DrawDemo() {
            if (this == null) return false;
            if (IM.Foldout("Logger", ref _logFoldout)) {
                using (IM.Indent()) {
                    IM.StringField("LogText", ref LogText);
                    using (IM.HorizontalDivide(2)) {
                        this.MethodButton(nameof(DebugLog));
                        this.MethodButton(nameof(Log)); 
                    }
                }
            }
            if (IM.Foldout("Points",ref  _pointsFoldout)) {
                using (IM.Indent()) {
                    IMStyle.DragNumberScale = 10;
                    _points.AsSpan().ViewElements(ReadOnlySpan<char>.Empty, false);
                    IMStyle.DragNumberScale = 1;
                }
                IM.Mesh.AddLinesLoop( _points.AsSpan(),10,UiColor.Cyan);
            }
            if (IM.Foldout("Move", ref _moveFoldout)) {
                using (IM.Indent()) {
                    if (IM.InputDropdownField(_names, ref selectedIndex)) {
                        Target = _transforms[selectedIndex];
                         if(Target) _hint.EulerAngles = Target.localEulerAngles;
                    }
                    if (Target != null) {
                        IM.Slider(ref MoveSpeed, -30, 30);
                        IM.BoolField("ReadOnly", ref isMoveSpeedReadonly);
                        this.ViewField("MoveSpeed", isMoveSpeedReadonly);
                        var pos = Target.localPosition;
                        if (IM.Vector3Field("Position", ref  pos)) {
                            Target.localPosition = pos;
                        }
                        if (IM.Vector3Field("Rotation",_hint.GetLocalEulerAngles(Target ),out var   euler)) {
                            _hint.EulerAngles=euler;
                            Target.localRotation = _hint.Rotation;
                        }
                        if (IM.Vector3Field("Scale",Target.localScale, out var    scale)) {
                            Target.localScale = scale;
                        }
                        Vector3 screenPoint;
                        Vector2 targetScreenPos;
                         if (IM.SlidePad(20, out var v2)) {
                             Target.localPosition = pos += (Vector3) (v2 * (Time.deltaTime * MoveSpeed));
                             screenPoint = IM.Camera.WorldToScreenPoint(pos);
                             targetScreenPos = new Vector2(screenPoint.x, screenPoint.y + 50);
                             IM.Circle(IM.ScreenMousePos, 12, UiColor.Yellow);
                            IM.Line(targetScreenPos, IM.ScreenMousePos, 8, UiColor.Green, UiColor.Yellow);
                        }else {
                             screenPoint = IM.Camera.WorldToScreenPoint(pos);
                             targetScreenPos = new Vector2(screenPoint.x, screenPoint.y + 50);
                        }
                        IM.Circle(targetScreenPos, 20, UiColor.Green);
                      
                    }
                }
            }
            if (IM.Foldout("Circle",ref  _fillFoldOut)) {
                using (IM.Indent()) {
                    var fillAmount = (Time.time / 3) % 1;
                    IM.IntField("FillAmount", (int) (100 * fillAmount));
                    if (IM.Current.TryGetNextRect(80, out var rect)) {
                        IM.Mesh.AddRadialFilledCircle(new Vector2(rect.xMin + 40, rect.yMin + 40), 40, UiColor.White,
                            fillAmount);
                    }
                }
            }
            return true;
        }

        void OnDestroy() {
            SimpleConsole.Clear();
        }
    }
}