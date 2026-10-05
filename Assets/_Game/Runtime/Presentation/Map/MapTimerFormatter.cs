using System;

namespace Gravivore.Presentation.Map
{
    public static class MapTimerFormatter
    {
        public static string FormatRounded(float seconds)
        {
            var wholeSeconds = ClampToWholeSeconds(seconds);
            if (wholeSeconds <= 0) return "готово";
            if (wholeSeconds < 60) return "<1 мин";

            var totalMinutes = Math.Max(1, wholeSeconds / 60);
            if (totalMinutes < 60) return $"{totalMinutes} мин";

            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return minutes == 0 ? $"{hours} ч" : $"{hours} ч {minutes} мин";
        }

        public static string FormatExact(float seconds)
        {
            var wholeSeconds = ClampToWholeSeconds(seconds);
            var hours = wholeSeconds / 3600;
            var minutes = (wholeSeconds % 3600) / 60;
            var remainder = wholeSeconds % 60;
            return hours > 0
                ? $"{hours}:{minutes:00}:{remainder:00}"
                : $"{minutes:00}:{remainder:00}";
        }

        public static int RoundedBucket(float seconds)
        {
            var wholeSeconds = ClampToWholeSeconds(seconds);
            if (wholeSeconds <= 0) return 0;
            if (wholeSeconds < 60) return 1;
            return 2 + Math.Max(1, wholeSeconds / 60);
        }

        public static int ExactSecondBucket(float seconds) => ClampToWholeSeconds(seconds);

        private static int ClampToWholeSeconds(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0f) return 0;
            return (int)Math.Ceiling(seconds);
        }
    }
}
