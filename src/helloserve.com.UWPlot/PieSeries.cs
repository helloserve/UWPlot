using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public class PieSeries : Series
    {
        private Type contextType;
        private PropertyInfo sourceProperty;
        private PropertyInfo valuePropertyInfo;
        private PropertyInfo displayPropertyInfo;
        private PropertyInfo categoryPropertyInfo;

        internal IEnumerable ItemsCollection { get; set; }
        internal List<PieSeriesDataPoint> ItemsDataPoints { get; set; }

        internal override SeriesMetaData PrepareData(object dataContext, double fontSize = 12, Transform categoryTransform = null)
        {
            var type = dataContext.GetType();

            if (contextType is null || type != contextType || sourceProperty is null)
            {
                contextType = type;

                var sourceBinding = ItemsSource as Windows.UI.Xaml.Data.Binding;
                sourceProperty = type.GetProperty(sourceBinding.Path?.Path);
                if (sourceProperty == null)
                {
                    throw new ArgumentNullException($"ItemsSource is not a property of {type.Name}.");
                }

                var sourceType = sourceProperty.PropertyType;

                if (sourceType.GetInterface(nameof(IEnumerable)) is null)
                {
                    throw new ArgumentException($"ItemsSource is configured with {sourceBinding.Path.Path}, but it doesn't implement IEnumerable.");
                }

                if (sourceType.GenericTypeArguments is null || sourceType.GenericTypeArguments.Length == 0)
                {
                    throw new ArgumentException($"Unable to determine generic type argument of collection at {sourceBinding.Path.Path}");
                }

                var sourceGenericType = sourceType.GenericTypeArguments[0];

                valuePropertyInfo = sourceGenericType.GetProperty(ValueName);
                categoryPropertyInfo = sourceGenericType.GetProperty(CategoryName);
                if (!string.IsNullOrEmpty(DisplayName))
                {
                    displayPropertyInfo = sourceGenericType.GetProperty(DisplayName);
                }
            }

            ItemsCollection = sourceProperty.GetValue(dataContext) as IEnumerable;

            ItemsDataPoints = new List<PieSeriesDataPoint>();
            var meta = new SeriesMetaData();
            var total = 0D;
            
            foreach (var item in ItemsCollection)
            {
                var categoryValue = categoryPropertyInfo.GetValue(item);
                var displayValue = string.IsNullOrEmpty(DisplayName) ? string.Empty : displayPropertyInfo.GetValue(item);
                var value = (double?)valuePropertyInfo.GetValue(item);
                if (value == double.NaN)
                    value = 0;

                var dataPoint = new PieSeriesDataPoint()
                {
                    Value = value,
                    ValueText = value.FormatObject(ValueFormat),
                    Category = categoryValue.FormatObject(CategoryFormat),
                    Display = displayValue.FormatObject(DisplayFormat)
                };

                if (!meta.ValueMax.HasValue || value > meta.ValueMax.Value)
                {
                    meta.ValueMax = value;
                    meta.ValueMaxLength = dataPoint.ValueText.Length;
                }

                if (!meta.ValueMin.HasValue || value < meta.ValueMin.Value)
                {
                    meta.ValueMin = value;
                    meta.ValueMinLength = dataPoint.ValueText.Length;
                }

                if (string.IsNullOrEmpty(meta.LongestCategory) || dataPoint.Category.Length > meta.LongestCategory.Length)
                {
                    meta.LongestCategory = dataPoint.Category;
                }

                var categorySize = dataPoint.Category.MeasureTextSize(fontSize, categoryTransform);
                meta.TotalCategoryWidth += categorySize.Width;

                total += value.GetValueOrDefault();

                ItemsDataPoints.Add(dataPoint);
            }

            var factor = total == 0 ? 0 : 1 / total;

            foreach (var item in ItemsDataPoints)
            {
                item.NormalizedValue = item.Value.GetValueOrDefault() * factor;
            }

            return meta;
        }
    }

    internal class PieSeriesDataPoint : SeriesDataPoint
    {
        public double NormalizedValue { get; set; }
    }
}
