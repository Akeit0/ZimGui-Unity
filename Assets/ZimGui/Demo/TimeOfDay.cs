using System;
using UnityEngine;

namespace ZimGui.Demo {
    public readonly struct TimeOfDay {
        public readonly int Ticks;
    
        public TimeOfDay(DateTime dateTime) {
            var timeOfDay = dateTime.TimeOfDay;
            Ticks = (int)(timeOfDay.Ticks/10000);
        }

        public int Hours => Ticks / 3600000;
        public int FormatHoursToMilliseconds(Span<char> span) {
            var hours = Ticks / 3600000;
            span[0] = (char)('0' + hours / 10);
            span[1] =(char) ('0' + (hours - 10*(hours / 10)));
            span[2] =':';
            var minutesTicks = Ticks - hours * 3600000;
            var minutes = minutesTicks/ 60000;
            span[3] = (char)('0' + minutes / 10);
            span[4] =(char) ('0' + (minutes - 10*(minutes / 10)));
            span[5] =':';
            var secondsTicks = minutesTicks - minutes * 60000;
            var seconds=(secondsTicks) / 1000;
            span[6] = (char)('0' + seconds / 10);
            span[7] =(char) ('0' + (seconds - 10*(seconds / 10)));
            span[8] =':';
            var milliSecondsTicks = secondsTicks - seconds * 1000;
            var mill100 = milliSecondsTicks / 100;
            span[9] = (char)('0' + mill100);
            var mill10 = (milliSecondsTicks - mill100 * 100)/10;
            span[10] =(char) ('0' +mill10);
            var mill1 = milliSecondsTicks-mill100*100-mill10*10;
            span[11] =(char) ('0' + mill1);
            return 12;
        }
        public int FormatHoursToSeconds(Span<char> span) {
            var hours = Ticks / 3600000;
            span[0] = (char)('0' + hours / 10);
            span[1] =(char) ('0' + (hours - 10*(hours / 10)));
            span[2] =':';
            var minutesTicks = Ticks - hours * 3600000;
            var minutes = minutesTicks/ 60000;
            span[3] = (char)('0' + minutes / 10);
            span[4] =(char) ('0' + (minutes - 10*(minutes / 10)));
            span[5] =':';
            var secondsTicks = minutesTicks - minutes * 60000;
            var seconds=(secondsTicks) / 1000;
            span[6] = (char)('0' + seconds / 10);
            span[7] =(char) ('0' + (seconds - 10*(seconds / 10)));
            return 8;
        }
    }
}