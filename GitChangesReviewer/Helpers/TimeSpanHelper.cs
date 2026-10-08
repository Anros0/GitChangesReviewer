namespace GitChangesReviewer.Helpers
{
    internal static class TimeSpanHelper
    {
        public static TimeSpan FromNanoseconds(long value)
        {
            return TimeSpan.FromTicks(value / 100);
        }
    }
}
