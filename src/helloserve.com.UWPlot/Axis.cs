using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public abstract class Axis
    {
        /// <summary>
        /// The name of the axis. Use this name to reference the axis in a series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The axis description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A transform that will be applied to each increment label on the axis
        /// </summary>
        public Transform LabelTransform { get; set; } = new TranslateTransform();
    }

    public class XAxis : Axis
    {

    }

    public class YAxis : Axis
    {
        /// <summary>
        /// The type of the axis scale.
        /// </summary>
        public ScaleType ScaleType { get; set; } = ScaleType.Auto;

        /// <summary>
        /// If specified, defines the increments on the scale.
        /// </summary>
        public double? Increment { get; set; }

        /// <summary>
        /// If specified, anchors the minimum value on the axis scale.
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        /// If specified, anchors the maximum value on the axis scale.
        /// </summary>
        public double? Max { get; set; }

        /// <summary>
        /// Indicates if the axis is a Primary axis (drawn on the left of the chart) or a Secondary axis (drawn to the right). The default is primary.
        /// </summary>
        public YAxisType AxisType { get; set; } = YAxisType.Primary;

        /// <summary>
        /// If specified, the axis scale will round to nearest magnitude, e.g. nearest 1, nearest 10, nearest 100 etc. This value is overridden by the <see cref="Increment"/> property.
        /// </summary>
        public int? NearestRoundingMagnitude { get; set; }

        internal double CalculatedMin { get; set; }
        internal double CalculatedMax { get; set; }
        internal double CalculatedIncrement { get; set; }
        internal List<double> ScaleValues { get; set; }

        public double CalculateRange => CalculatedMax - CalculatedMin;

        public void Measure(double seriesMaxValue, double seriesMinValue, int numberOfLines)
        {
            if (Min.HasValue)
            {
                CalculatedMin = Min.Value;
            }
            else
            {
                if (seriesMinValue < 0)
                    CalculatedMin = CalculateUpperBound(seriesMinValue);
                else
                    CalculatedMin = CalculateLowerBound(seriesMinValue);
            }

            if (Max.HasValue)
            {
                CalculatedMax = Max.Value;
            }
            else
            {
                if (seriesMaxValue < 0)
                    CalculatedMax = CalculateLowerBound(seriesMaxValue);
                else
                    CalculatedMax = CalculateUpperBound(seriesMaxValue);
            }

            if (Increment.HasValue)
            {
                CalculatedIncrement = Increment.Value;
            }
            else
            {
                CalculatedIncrement = Math.Round((CalculatedMax - CalculatedMin) / numberOfLines);
            }

            ScaleValues = new List<double>();

            for (int i = 0; i < numberOfLines + 1; i++)
            {
                ScaleValues.Add(CalculatedMin + (i * CalculatedIncrement));
            }
        }

        private double CalculateUpperBound(double value)
        {
            var whole = Math.Abs((int)value);
            var magnitude = Math.Pow(10, Math.Max(1, whole.ToString().Length));

            var bound = Math.Truncate(value / magnitude) * magnitude;

            while (Math.Abs(bound) < Math.Abs(value))
            {
                magnitude /= 10;
                bound = Math.Round(value / magnitude) * magnitude;
            }

            return bound;
        }

        private double CalculateLowerBound(double val)
        {
            var whole = Math.Abs((int)val);
            var magnitude = Math.Pow(10, Math.Max(1, whole.ToString().Length - 1));

            var bound = Math.Round(val / magnitude) * magnitude;

            while (Math.Abs(bound) > Math.Abs(val))
            {
                magnitude /= 10;
                bound = Math.Round(val / magnitude) * magnitude;
                if (Math.Abs(bound) < 100 && Math.Abs(bound) > -100)
                {
                    bound = 0;
                }
            }

            return bound;
        }

        public enum YAxisType
        {
            Primary,
            Secondary
        }
    }
}
