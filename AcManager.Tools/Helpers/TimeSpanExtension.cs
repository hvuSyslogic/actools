﻿using System;

namespace AcManager.Tools.Helpers {
    public static class TimeSpanExtension {
        public static string ToProperString(this TimeSpan span) {
            return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }

        public static string ToMillisecondsString(this TimeSpan span) {
            return span.TotalHours > 1d
                    ? $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds:D3}"
                    : $"{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds:D3}";
        }
    }
}
