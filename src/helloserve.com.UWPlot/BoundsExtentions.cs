using System;

namespace helloserve.com.UWPlot
{
    internal static class BoundsExtentions
    {
        public static double CalculateUpperBound(this double value, out double magnitude)
        {
            if (value < 0)
                return CalculateUpperBound(Math.Abs(value), out magnitude) * -1;

            if (value < 1000)
                return value.CalculateUpperBoundWithMagnitude(100, out magnitude);

            if (value < 10000)
                return value.CalculateUpperBoundWithMagnitude(1000, out magnitude);

            if (value < 100000)
                return value.CalculateUpperBoundWithMagnitude(10000, out magnitude);

            var whole = (int)value;
            magnitude = Math.Pow(10, Math.Max(1, whole.ToString().Length - 1));

            return value.CalculateUpperBoundWithMagnitude(magnitude, out magnitude);
        }

        private static double CalculateUpperBoundWithMagnitude(this double value, double magnitude, out double appliedMagnitude)
        {
            appliedMagnitude = magnitude;
            return (Math.Truncate(value / magnitude) + 1) * magnitude;
        }

        public static double CalculateLowerBound(this double value, out double magnitude)
        {
            if (value < 0)
                return CalculateLowerBound(Math.Abs(value), out magnitude) * -1;

            var whole = (int)value;
            magnitude = Math.Pow(10, Math.Max(1, whole.ToString().Length - 1));

            var bound = Math.Round(value / magnitude) * magnitude;

            while (bound > value)
            {
                magnitude /= 10;
                bound = Math.Round(value / magnitude) * magnitude;
                if (bound < 100)
                {
                    bound = 0;
                }
            }

            return bound;
        }
    }
}
