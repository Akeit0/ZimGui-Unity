using System;
using System.Collections.Generic;
using UnityEngine;
using Str=System.ReadOnlySpan<char>;
namespace ZimGui.Demo {
    public enum LogTimeType {
        None,
        Seconds,
        MilliSeconds
    }
    public static class SimpleConsole {
        static object _lock = new object();
        static RingBuffer<(TimeOfDay Time ,string Text ,UiColor Color)> _elements;


        public static int Capacity {
            get {
                lock (_lock) {
                    return _elements.Capacity;
                }
            }
        }
        public static void Init(int capacity=32,bool receiveLog=true) {
            _elements=new (capacity);
              if(receiveLog)    Application.logMessageReceivedThreaded += ReceivedLog;
        }
        static void ReceivedLog(string logString, string stackTrace, LogType logType) {
            switch (logType) {
                case LogType.Log:Log(logString);
                    return;
                case LogType.Warning:
                case LogType.Assert:
                    Log(logString,UiColor.Yellow);
                    return;
                case LogType.Error:
                case LogType.Exception:
                    Log(logString,UiColor.Red);
                    return;
                default: return;
            }
        }
        
        public static void Clear() {
            _elements.Clear();
            Application.logMessageReceivedThreaded -= ReceivedLog;
        }
        public static void Log(string text) {
            if (_inLock) return;
            lock (_lock) {
                _elements.Add((new TimeOfDay(DateTime.Now),text,IMStyle.FontColor));
            }
            
        }
         public static void Log(string text,UiColor color) {
             if (_inLock) return;
             lock (_lock) {
                 _elements.Add((new TimeOfDay(DateTime.Now), text, color));
             }
         }
         static float scrollValue=0;
         static bool _inLock=false;
         
      
        public static bool Draw() {
            lock (_lock) {
                _inLock = true;
                try {
                    var count = _elements.Count;
                    if (IM.Button("Clear")) {
                        _elements.Clear();
                        return true;
                    }

                    if (count == 0) return true;
                    if (!IM.Current.TryGetRemainRect(out var rect)) return true;
                    var height = rect.height;
                    var elementHeight = IMStyle.SpacedTextHeight * 1.1f;
                    var elementsHeight = elementHeight * count;
                    var elementRect = new Rect(rect.xMin, rect.yMax - elementHeight, rect.width,
                        IMStyle.SpacedTextHeight);
                    if (height < IMStyle.SpacedTextHeight) {
                        return true;
                    }

                    if (elementsHeight < height) {
                        scrollValue = 0;
                        Span<char> timeLabel = stackalloc char[11];
                        timeLabel[0] = '[';
                        timeLabel[9] = ']';
                        timeLabel[10] = ' ';
                        var timeRange = timeLabel[1..9];
                        foreach (var element in _elements) {
                            element.Time.FormatHoursToSeconds(timeRange);
                            IM.Label(elementRect, timeLabel, element.Text, element.Color);
                            elementRect.MoveY(-elementHeight);
                        }

                        return true;
                    }
                    else {
                        var barRect = rect;
                        barRect.xMin += rect.width - 10f;
                        IM.VerticalScroll(barRect, ref scrollValue, height / elementsHeight, out var top,
                            out var bottom);
                        elementRect.xMax -= 10f;
                        var topIndex = (int) (Mathf.Ceil(count * top));
                        topIndex = Mathf.Clamp(topIndex, 0, count - 1);
                        elementRect.MoveY((count * top - topIndex) * elementHeight);
                        Span<char> timeLabel = stackalloc char[11];
                        timeLabel[0] = '[';
                        timeLabel[9] = ']';
                        timeLabel[10] = ' ';
                        var timeRange = timeLabel[1..9];
                        foreach (var element in _elements.Slice(topIndex, (int) (height / elementHeight))) {
                            element.Time.FormatHoursToSeconds(timeRange);
                            IM.Label(elementRect, timeLabel, element.Text, element.Color);
                            elementRect.MoveY(-elementHeight);
                        }


                        return true;
                    }
                }
                finally {
                    _inLock = false;
                }
                
               
            }
        }
    }
}