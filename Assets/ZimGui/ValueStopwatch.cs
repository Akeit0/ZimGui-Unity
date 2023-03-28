using System;
using System.Diagnostics;
using System.Globalization;

namespace ZimGui {
    public readonly struct ValueStopwatch
    {
        static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        readonly long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        public TimeSpan GetElapsedTime()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
            }

            var end = Stopwatch.GetTimestamp();
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }
    public readonly struct ElapsedTimeLogger :IDisposable{
        public static Action<string> Logger = UnityEngine.Debug.Log;
        readonly ValueStopwatch _stopwatch;
        public readonly string Label;
        ElapsedTimeLogger(ValueStopwatch stopwatch) {
            _stopwatch = stopwatch;
            Label = null;
        }
        ElapsedTimeLogger(string label,ValueStopwatch stopwatch) {
            _stopwatch = stopwatch;
            Label = label;
        }
        public static ElapsedTimeLogger StartNew() =>new (ValueStopwatch.StartNew());
        public static ElapsedTimeLogger StartNew(string label) =>new (label,ValueStopwatch.StartNew());

        public void Dispose() {
            Log();
        }
        public void Log() {
            if (Label != null)
                Logger(Label + " : " + _stopwatch.GetElapsedTime().TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
            else  Logger(_stopwatch.GetElapsedTime().TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        }
    }
}