using System;

namespace helloserve.com.UWPlot
{
    internal static class BoundsExtentions
    {
        public static double CalculateUpperBound(this double value, double? range, out double magnitude)
        {
            double bound = value.CalculateUpperBoundFromValue(range, out magnitude);
            
            return bound;
        }

        private static double CalculateUpperBoundFromValue(this double value, double? range, out double magnitude)
        {
            if (value < 0)
                return CalculateUpperBound(Math.Abs(value), range, out magnitude) * -1;

            if (value < 100)
            {
                if (range.HasValue && range < 0.1)
                    return value.CalculateUpperBoundWithMagnitude(0.01, out magnitude);

                if (range.HasValue && range < 1)
                    return value.CalculateUpperBoundWithMagnitude(0.1, out magnitude);

                if (range.HasValue && range < 10)
                    return value.CalculateUpperBoundWithMagnitude(1, out magnitude);
                
                return value.CalculateUpperBoundWithMagnitude(10, out magnitude);
            }

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
            if (magnitude < 1)
            {
                double whole = 0;
                while (whole < value)
                {
                    whole += magnitude;
                }
            }

            if (magnitude == 1)
                return Math.Ceiling(value);

            return (Math.Truncate(value / magnitude) + 1) * magnitude;
        }

        public static double CalculateLowerBound(this double value, double? range, out double magnitude)
        {
            if (value < 0)
                return CalculateLowerBound(Math.Abs(value), range, out magnitude) * -1;

            double whole = (int)value;
            
            if (range < 1)
            {
                int count = 0;
                var c = value;
                while (c < 1)
                {
                    c = c * 10;
                    count++;
                }

                magnitude = 1D / Math.Pow(10, count);

                whole = 1;
                while (whole > value)
                {
                    whole -= magnitude;
                }

                return whole;
            }

            if (range.HasValue && range < 10)
            {
                magnitude = 1;
                return (double)whole;
            }

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
