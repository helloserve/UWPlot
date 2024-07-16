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

        public static double CalculateUpperBound(this double value, double magnitude)
        {
            if (value < 0)
                return Math.Abs(value).CalculateUpperBound(magnitude) * -1;

            double bound = Math.Ceiling(value / magnitude) * magnitude;

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

            if (value < 1000000)
                return value.CalculateUpperBoundWithMagnitude(100000, out magnitude);

            magnitude = value.GetMagnitude();

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

                return whole;
            }

            if (magnitude == 1)
                return Math.Ceiling(value);

            double bound = (Math.Truncate(value / magnitude) + 1) * magnitude;

            if ((bound % value) / value > 0.25)
            {
                double step = magnitude * 0.1;
                double rounded = Math.Truncate(value / magnitude) * magnitude;
                double margin = (Math.Ceiling((value % rounded) / step) + 1) * step;

                bound = rounded + margin;
            }

            return bound;
        }

        public static double CalculateLowerBound(this double value, double? range, out double magnitude)
        {
            if (value < 0)
                return CalculateLowerBound(Math.Abs(value), range, out magnitude) * -1;

            double whole = (int)value;
            
            if (range < 1)
            {
                magnitude = GetMagnitude(value);
                return Math.Ceiling((1 % value) / magnitude) * magnitude;
            }

            if (range.HasValue && range < 10)
            {
                magnitude = 1;
                return (double)whole;
            }

            magnitude = whole.GetMagnitude();

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

        public static double CalculateLowerBound(this double value, double magnitude)
        {
            if (value < 0)
                return Math.Abs(value).CalculateLowerBound(magnitude) * -1;

            double bound = Math.Floor(value / magnitude) * magnitude;

            return bound;
        }

        public static double GetMagnitude(this double value)
        {
            if (value < 0)
                return GetMagnitude(Math.Abs(value));

            var magnitude = Math.Pow(10, Math.Ceiling(Math.Log10(value)) - 1);

            if (magnitude == 0)
                return 1;

            return magnitude;
        }

        public static double MaxOf(params double[] values)
        {
            double max = double.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }
            }

            return max;
        }

        public static double MinOf(params double[] values)
        {
            double min = double.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                {
                    min = values[i];
                }
            }

            return min;
        }
    }
}
