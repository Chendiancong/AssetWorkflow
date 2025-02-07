using System;

namespace cdc.AssetWorkflow
{
    public static class Extensions
    {
        public static bool StrEquals(this string str, string other, StringComparison compareType = StringComparison.Ordinal)
        {
            return string.Compare(str, other, compareType) == 0;
        }

        public static bool AlmostEquals(this float num, float other, float EPSILON = 1e-5f)
        {
            return Math.Abs(num - other) <= EPSILON;
        }
    }
}