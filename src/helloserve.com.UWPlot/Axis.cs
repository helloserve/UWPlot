using System;
using System.Collections.Generic;
using System.Linq;
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
        public Transform LabelTransform { get; set; }
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
        internal double MagnitudeMin { get; set; }
        internal double CalculatedMax { get; set; }
        internal double MagnitudeMax { get; set; }
        internal double BaseMagnitude { get; set; }
        internal double CalculatedIncrement { get; set; }
        internal List<double> ScaleValues { get; set; }

        public double CalculateRange => CalculatedMax - CalculatedMin;

        /// <summary>
        /// Measures and divides the axis based on a predetermined number of grid lines.
        /// </summary>
        /// <param name="seriesMaxValue"></param>
        /// <param name="seriesMinValue"></param>
        /// <param name="numberOfLines"></param>
        public void Measure(double seriesMaxValue, double seriesMinValue, int numberOfLines)
        {
            if (Min.HasValue)
            {
                CalculatedMin = Min.Value;
            }
            else
            {
                double magnitude = 0;
                if (seriesMinValue < 0)
                    CalculatedMin = seriesMinValue.CalculateUpperBound(out magnitude);
                else
                    CalculatedMin = seriesMinValue.CalculateLowerBound(out magnitude);
            }

            if (Max.HasValue)
            {
                CalculatedMax = Max.Value;
            }
            else
            {
                double magnitude = 0;
                if (seriesMaxValue < 0)
                    CalculatedMax = seriesMaxValue.CalculateLowerBound(out magnitude);
                else
                    CalculatedMax = seriesMaxValue.CalculateUpperBound(out magnitude);
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
                ScaleValues.Add(Math.Round((CalculatedMax - CalculatedMin) * ((double)i / (double)numberOfLines) + CalculatedMin));
            }
        }

        internal void CalculateSeriesBounds(List<List<SeriesDataPoint>> series)
        {
            int mostDifferenceOccurance = 0;
            double mostDifference = 0;
            int mostMagnitudeOccurance = 0;
            double mostMagnitude = 0;
            List<Dictionary<double, int>> seriesDifferencesRounded = new List<Dictionary<double, int>>();
            List<Dictionary<double, int>> seriesMagnitudes = new List<Dictionary<double, int>>();
            for (int i = 0; i < series.Count; i++)
            {
                seriesDifferencesRounded.Add(new Dictionary<double, int>());
                seriesMagnitudes.Add(new Dictionary<double, int>());
                for (int p = 1; p < series[i].Count - 1; p++)
                {
                    var difference = Math.Abs(series[i][p].Value.GetValueOrDefault() - series[i][p - 1].Value.GetValueOrDefault());
                    var roundedDifference = difference.CalculateUpperBound(out double magnitude);
                    if (seriesDifferencesRounded[i].ContainsKey(roundedDifference))
                        seriesDifferencesRounded[i][roundedDifference]++;
                    else
                        seriesDifferencesRounded[i].Add(roundedDifference, 1);

                    if (seriesMagnitudes[i].ContainsKey(magnitude))
                        seriesMagnitudes[i][magnitude]++;
                    else
                        seriesMagnitudes[i].Add(magnitude, 1);
                }

                foreach (var key in seriesDifferencesRounded[i].Keys)
                {
                    if (seriesDifferencesRounded[i][key] > mostDifferenceOccurance)
                    {
                        mostDifferenceOccurance = seriesDifferencesRounded[i][key];
                        mostDifference = key;
                    }
                }
                foreach (var key in seriesMagnitudes[i].Keys)
                {
                    if (seriesMagnitudes[i][key] > mostMagnitudeOccurance)
                    {
                        mostMagnitudeOccurance = seriesMagnitudes[i][key];
                        mostMagnitude = key;
                    }
                }
            }

            var allValues = series.SelectMany(x => x.Where(s => s.Value.HasValue).Select(s => s.Value.GetValueOrDefault())).ToList();
            double seriesMinValue = allValues.Min();
            double seriesMaxValue = allValues.Max();

            double magnitudeMin = 0;
            double magnitudeMax = 0;
            if (Min.HasValue)
            {
                CalculatedMin = Min.Value;
            }
            else
            {
                if (seriesMinValue < 0)
                    CalculatedMin = seriesMinValue.CalculateUpperBound(out magnitudeMin);
                else
                    CalculatedMin = seriesMinValue.CalculateLowerBound(out magnitudeMin);
            }

            if (Max.HasValue)
            {
                CalculatedMax = Max.Value;
            }
            else
            {
                if (seriesMaxValue < 0)
                    CalculatedMax = seriesMaxValue.CalculateLowerBound(out magnitudeMax);
                else
                    CalculatedMax = seriesMaxValue.CalculateUpperBound(out magnitudeMax);
            }

            if (magnitudeMax > magnitudeMin)
            {
                if (CalculatedMin > 0)
                    CalculatedMin = 0;
                //we can't really increase the magnitude, since the lower bound can't go higher, so best to just make it zero.
            }
            else if (magnitudeMin > magnitudeMax && CalculatedMax != 0)
            {
                if (CalculatedMax < 0 && CalculatedMin < 0)
                    CalculatedMax = 0;
                else
                    CalculatedMax = magnitudeMin * (CalculatedMax / Math.Abs(CalculatedMax));
            }
            MagnitudeMin = magnitudeMin;
            MagnitudeMax = magnitudeMax;
            BaseMagnitude = mostMagnitude;
        }

        /// <summary>
        /// Calculate the optimal amount and increment for grid lines and measures the axis accordingly. Returns the grid lines result as a partial extents object.
        /// </summary>
        /// <param name="seriesMaxValue"></param>
        /// <param name="seriesMinValue"></param>
        /// <param name="height"></param>
        /// <param name="minHeight"></param>
        /// <returns></returns>
        internal CartesianPlotExtents Measure(double height, double minHeight)
        {
            int idealNumberOfLines = (int)(height / minHeight) / 2;
            while (idealNumberOfLines % 2 != 0)
                idealNumberOfLines--;

            int numberOfLines = CalculateNumberOfLines(MagnitudeMin, MagnitudeMax, BaseMagnitude, idealNumberOfLines);

            double heightIncrements = height / numberOfLines;

            ScaleValues = new List<double>();

            for (int i = 0; i <= numberOfLines; i++)
            {
                ScaleValues.Add(CalculatedIncrement * i + CalculatedMin);
            }

            return new CartesianPlotExtents()
            {
                NumberOfScaleLines = numberOfLines,
                ScaleLineIncrements = heightIncrements
            };
        }

        private int CalculateNumberOfLines(double minMagnitude, double maxMagnitude, double mostMagnitude, int idealNumberOfLines)
        {
            CalculatedIncrement = mostMagnitude;
            int numberOfLines = (int)Math.Round((double)Math.Abs(CalculatedMax - CalculatedMin) / CalculatedIncrement);

            if (idealNumberOfLines == 0)
            {
                numberOfLines = 1;
                CalculatedIncrement = CalculatedMax - CalculatedMin;
            }
            else
            {
                while (numberOfLines > (idealNumberOfLines + 1) 
                    && (CalculatedIncrement * 10 < maxMagnitude && CalculatedMax != 0)
                    && (CalculatedIncrement * 10 < minMagnitude && CalculatedMin != 0))
                {
                    CalculatedIncrement *= 10;
                    numberOfLines = (int)Math.Round((double)Math.Abs(CalculatedMax - CalculatedMin) / CalculatedIncrement);
                }
            }

            if (numberOfLines > idealNumberOfLines + 1)
            {
                //now we need to handle cases where it's funny
                var maxPart = CalculatedMax / maxMagnitude;
                var minPart = CalculatedMin / minMagnitude;
                var isMaxEven = maxPart % 2 == 0;
                var isMinEven = minPart % 2 == 0;
                var isDifferenceEven = (maxPart - minPart) % 2 == 0;

                while (!isMaxEven || !isMinEven)
                {
                    if (!isMaxEven)
                    {
                        if (maxPart < 1)
                            maxPart--;
                        else
                            maxPart++;                        
                    }

                    if (!isMinEven)
                    {
                        if (minPart < 0)
                            minPart--;
                        else if (minPart - 1 >= 0)
                            minPart--;
                    }

                    isMaxEven = maxPart % 2 == 0;
                    isMinEven = minPart % 2 == 0;
                    isDifferenceEven = (maxPart - minPart) % 2 == 0;
                }

                CalculatedMin = minPart * minMagnitude;
                CalculatedMax = maxPart * maxMagnitude;
                numberOfLines = idealNumberOfLines;
                CalculatedIncrement = (CalculatedMax - CalculatedMin) / numberOfLines;
                while (CalculatedIncrement % 10 != 0 && numberOfLines > 0)
                {
                    numberOfLines--;
                    CalculatedIncrement = (CalculatedMax - CalculatedMin) / numberOfLines;
                }
            }

            return numberOfLines;
        }

        public enum YAxisType
        {
            Primary,
            Secondary
        }
    }
}
