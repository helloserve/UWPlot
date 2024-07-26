using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
            var range = Math.Abs(seriesMaxValue - seriesMinValue);

            if (Min.HasValue)
            {
                CalculatedMin = Min.Value;
            }
            else
            {
                double magnitude = 0;
                if (seriesMinValue < 0)
                    CalculatedMin = seriesMinValue.CalculateUpperBound(range, out magnitude);
                else
                    CalculatedMin = seriesMinValue.CalculateLowerBound(range, out magnitude);
            }

            if (Max.HasValue)
            {
                CalculatedMax = Max.Value;
            }
            else
            {
                double magnitude = 0;
                if (seriesMaxValue < 0)
                    CalculatedMax = seriesMaxValue.CalculateLowerBound(range, out magnitude);
                else
                    CalculatedMax = seriesMaxValue.CalculateUpperBound(range, out magnitude);
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

        internal void CalculateSeriesBoundsV2(List<List<SeriesDataPoint>> series)
        {
            double overallMin = series.Min(x => x.Where(p => p.Value.HasValue).Min(p => p.Value.Value));
            double overallMax = series.Max(x => x.Where(p => p.Value.HasValue).Max(p => p.Value.Value));

            double minSign = overallMin / Math.Abs(overallMin);
            double maxSign = overallMax / Math.Abs(overallMax);

            double overallDifference = overallMax - overallMin;
            double marginRatio = overallDifference * 0.1D;

            double lower = overallMin - marginRatio;
            double upper = overallMax + marginRatio;

            if (overallMin > 0 && lower < 0)
                lower = 0;

            if (overallMax < 0 && upper > 0)
                upper = 0;

            lower = NearestRound(lower, minSign);
            upper = NearestRound(upper, maxSign);

            CalculatedMin = lower;
            CalculatedMax = upper;
            if (Min.HasValue)
            {
                CalculatedMin = Min.Value;
            }

            if (Max.HasValue)
            {
                CalculatedMax = Max.Value;
            }

            double NearestRound(double value, double sign)
            {
                double abs = Math.Abs(value);
                if (value < 100)
                {
                    if (overallDifference < 10)
                    {
                        value = Math.Ceiling(value);
                    }
                    else if (overallDifference < 100)
                    {
                        value = Math.Truncate(value / 10) * 10;
                    }
                }
                else if (value < 1000)
                {
                    value = Math.Truncate(value / 100) * 100;
                }
                else if (value < 10000)
                {
                    value = Math.Truncate(value / 1000) * 1000;
                }
                else if (value < 100000)
                {
                    value = Math.Truncate(value / 10000) * 10000;
                }
                else if (value < 1000000)
                {
                    value = Math.Truncate(value / 100000) * 100000;
                }
                else
                {
                    value = Math.Truncate(value / 1000000) * 1000000;
                }

                return value * sign;
            }
        }

        internal void CalculateSeriesBounds(List<List<SeriesDataPoint>> series)
        {
            int mostDifferenceOccurance = 0;
            double mostDifference = 0;
            int mostMagnitudeOccurance = 0;
            double mostMagnitude = 0;
            double overallMax = double.MinValue;
            double overallMin = double.MaxValue;
            List<Dictionary<double, int>> seriesDifferencesRounded = new List<Dictionary<double, int>>();
            List<Dictionary<double, int>> seriesMagnitudes = new List<Dictionary<double, int>>();

            if (series[0].Count == 1)
            {
                overallMax = BoundsExtentions.MaxOf(series.Select(x => x[0].Value.GetValueOrDefault()).ToArray());
                overallMin = BoundsExtentions.MinOf(series.Select(x => x[0].Value.GetValueOrDefault()).ToArray());

                if (overallMax > 0 && overallMin > 0)
                {
                    overallMin = 0;
                }
                if (overallMax < 0 && overallMin < 0)
                {
                    overallMax = 0;
                }

                BaseMagnitude = BoundsExtentions.MaxOf(series.Select(x => Math.Abs(x[0].Value.GetValueOrDefault())).ToArray()).GetMagnitude();
                MagnitudeMin = BoundsExtentions.MaxOf(series.Select(x => Math.Abs(x[0].Value.GetValueOrDefault())).ToArray()).GetMagnitude();
                MagnitudeMax = BaseMagnitude;
                CalculatedMax = overallMax.CalculateUpperBound(BaseMagnitude);
                CalculatedMin = overallMin.CalculateLowerBound(BaseMagnitude);                
            }
            else
            {
                for (int i = 0; i < series.Count; i++)
                {
                    seriesDifferencesRounded.Add(new Dictionary<double, int>());
                    seriesMagnitudes.Add(new Dictionary<double, int>());

                    if (series[i].Count > 0 && series[i][0].Value.HasValue)
                    {
                        if (series[i][0].Value > overallMax)
                            overallMax = series[i][0].Value.Value;
                        if (series[i][0].Value < overallMin)
                            overallMin = series[i][0].Value.Value;
                    }

                    for (int p = 1; p < series[i].Count; p++)
                    {
                        var indexValue = series[i][p].Value;
                        if (indexValue.HasValue)
                        {
                            if (indexValue > overallMax)
                                overallMax = indexValue.Value;
                            if (indexValue < overallMin)
                                overallMin = indexValue.Value;
                        }

                        var difference = Math.Abs(indexValue.GetValueOrDefault() - series[i][p - 1].Value.GetValueOrDefault());
                        var roundedDifference = difference.CalculateUpperBound(difference, out double magnitude);
                        if (roundedDifference > 0)
                        {
                            if (seriesDifferencesRounded[i].ContainsKey(roundedDifference))
                                seriesDifferencesRounded[i][roundedDifference]++;
                            else
                                seriesDifferencesRounded[i].Add(roundedDifference, 1);
                        }

                        if (magnitude > 0)
                        {
                            if (seriesMagnitudes[i].ContainsKey(magnitude))
                                seriesMagnitudes[i][magnitude]++;
                            else
                                seriesMagnitudes[i].Add(magnitude, 1);
                        }

                        //compare with other series
                        for (int s = 0; s < series.Count; s++)
                        {
                            if (s == i)
                                continue;

                            difference = Math.Abs(indexValue.GetValueOrDefault() - series[s][p].Value.GetValueOrDefault());
                            roundedDifference = difference.CalculateUpperBound(difference, out magnitude);
                            if (roundedDifference > 0)
                            {
                                if (seriesDifferencesRounded[i].ContainsKey(roundedDifference))
                                    seriesDifferencesRounded[i][roundedDifference]++;
                                else
                                    seriesDifferencesRounded[i].Add(roundedDifference, 1);
                            }

                            if (magnitude > 0)
                            {
                                if (seriesMagnitudes[i].ContainsKey(magnitude))
                                    seriesMagnitudes[i][magnitude]++;
                                else
                                    seriesMagnitudes[i].Add(magnitude, 1);
                            }
                        }
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

                double overallDifference = overallMax - overallMin;

                double magnitudeMin = 0;
                double magnitudeMax = 0;

                bool allNegative = overallMin <= 0 && overallMax <= 0;
                bool allPositive = overallMin >= 0 && overallMax >= 0;
                bool split = overallMin <= 0 && overallMax >= 0;

                if (Min.HasValue)
                {
                    CalculatedMin = Min.Value;
                }
                else
                {
                    if (allNegative)
                        CalculatedMin = overallMin.CalculateUpperBound(mostMagnitude);
                    else if (split)
                        CalculatedMin = overallMin.CalculateUpperBound(mostMagnitude);
                    else
                        CalculatedMin = overallMin.CalculateLowerBound(mostMagnitude);
                }

                if (Max.HasValue)
                {
                    CalculatedMax = Max.Value;
                }
                else
                {
                    if (allNegative)
                        CalculatedMax = overallMax.CalculateLowerBound(mostMagnitude);
                    else
                        CalculatedMax = overallMax.CalculateUpperBound(mostMagnitude);
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
                MagnitudeMin = mostMagnitude;
                MagnitudeMax = mostMagnitude;
                BaseMagnitude = mostMagnitude;
            }
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

            double calculatedMin = CalculatedMin;
            double calculatedMax = CalculatedMax;
            double calculatedIncrement = CalculatedIncrement;

            int numberOfLines = CalculateNumberOfLines(ref calculatedMin, ref calculatedMax, ref calculatedIncrement, MagnitudeMin, MagnitudeMax, BaseMagnitude, idealNumberOfLines);

            CalculatedMin = calculatedMin;
            CalculatedMax = calculatedMax;
            CalculatedIncrement = calculatedIncrement;

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

        private static int CalculateNumberOfLines(ref double calculatedMin, ref double calculatedMax, ref double calculatedIncrement, double minMagnitude, double maxMagnitude, double mostMagnitude, int idealNumberOfLines)
        {
            if (calculatedMax > 0 && calculatedMin < 0)
            {
                return CalculateNumberOfLinesWithZero(ref calculatedMin, ref calculatedMax, ref calculatedIncrement, minMagnitude, maxMagnitude, mostMagnitude, idealNumberOfLines);
            }

            calculatedIncrement = mostMagnitude;
            int numberOfLines = (int)Math.Round((double)Math.Abs(calculatedMax - calculatedMin) / calculatedIncrement);

            if (idealNumberOfLines == 0)
            {
                numberOfLines = 1;
                calculatedIncrement = calculatedMax - calculatedMin;
            }
            else
            {
                double differenceMagnitude = (calculatedMax - calculatedMin).GetMagnitude();

                while (numberOfLines > (idealNumberOfLines + 1) 
                    && (calculatedIncrement * 10 < differenceMagnitude))
                {
                    calculatedIncrement *= 10;
                    numberOfLines = (int)Math.Round((double)Math.Abs(calculatedMax - calculatedMin) / calculatedIncrement);
                }
            }

            if (numberOfLines > idealNumberOfLines + 1)
            {
                //now we need to handle cases where it's funny
                var maxPart = Math.Ceiling(calculatedMax / maxMagnitude);
                var minPart = Math.Floor(calculatedMin / minMagnitude);
                var isMaxEven = maxPart % 2 == 0;
                var isMinEven = minPart % 2 == 0;

                while (!isMaxEven || !isMinEven)
                {
                    if (!isMaxEven)
                    {
                        if (maxPart > 0)
                            maxPart++;
                        else if (maxPart + 1 <= 0)
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
                }

                calculatedMin = minPart * minMagnitude;
                calculatedMax = maxPart * maxMagnitude;
                numberOfLines = idealNumberOfLines;
                calculatedIncrement = (calculatedMax - calculatedMin) / numberOfLines;
                while (calculatedIncrement % 10 != 0 && numberOfLines > 2)
                {
                    numberOfLines--;
                    calculatedIncrement = (calculatedMax - calculatedMin) / numberOfLines;
                }
            }

            return numberOfLines;
        }

        private static int CalculateNumberOfLinesWithZero(ref double calculatedMin, ref double calculatedMax, ref double calculatedIncrement, double minMagnitude, double maxMagnitude, double mostMagnitude, int idealNumberOfLines)
        {
            double overallDiff = calculatedMax - calculatedMin;
            double positivePortion = calculatedMax / overallDiff;
            double negativePortion = Math.Abs(calculatedMin) / overallDiff;
            int idealNumberOfPositiveLines = (int)Math.Round(idealNumberOfLines * positivePortion);
            int idealNumberOfNegativeLines = (int)Math.Round(idealNumberOfLines * negativePortion);            

            double positiveMax = calculatedMax;
            double positiveMin = 0;
            double positiveIncrement = calculatedIncrement;
            int positiveLines = CalculateNumberOfLines(ref positiveMin, ref positiveMax, ref positiveIncrement, mostMagnitude, mostMagnitude, mostMagnitude, idealNumberOfPositiveLines);

            double negativeMax = 0;
            double negativeMin = calculatedMin;
            double negativeIncrement = calculatedIncrement;
            int negativeLines = CalculateNumberOfLines(ref negativeMin, ref negativeMax, ref negativeIncrement, mostMagnitude, mostMagnitude, mostMagnitude, idealNumberOfNegativeLines);

            calculatedIncrement = positivePortion >= negativePortion ? positiveIncrement : negativeIncrement;
            calculatedMin = -1 * negativeLines * calculatedIncrement;
            calculatedMax = positiveLines * calculatedIncrement;
            
            return positiveLines + negativeLines;
        }

        public enum YAxisType
        {
            Primary,
            Secondary
        }
    }
}
