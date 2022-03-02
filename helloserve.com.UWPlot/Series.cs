using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace helloserve.com.UWPlot
{
    public class Series
    {
        public object ItemsSource { get; set; }
        public string ValueName { get; set; }
        public string DisplayName { get; set; }
        public string CategoryName { get; set; }

        /// <summary>
        /// A format string for the value.
        /// </summary>
        public string ValueFormat { get; set; }

        /// <summary>
        /// A format string of the display property.
        /// </summary>
        public string DisplayFormat { get; set; }

        /// <summary>
        /// A format string for the category value. Only the first series bound to the primary axis needs to set this.
        /// </summary>
        public string CategoryFormat { get; set; }

        /// <summary>
        /// The name of the Y axis that this series is bound to.
        /// </summary>
        public string AxisName { get; set; }

        /// <summary>
        /// The description in the legend box of this series.
        /// </summary>
        public string LegendDescription { get; set; }

        /// <summary>
        /// The size of the bullet drawn at each data point. Default is 1% of the maximum extent. Make 0 to remove.
        /// </summary>
        public double? PointBulletSize { get; set; }

        private Type contextType;
        private PropertyInfo sourceProperty;
        private PropertyInfo valuePropertyInfo;
        private PropertyInfo displayPropertyInfo;
        private PropertyInfo categoryPropertyInfo;

        internal IEnumerable ItemsCollection { get; set; }
        internal List<SeriesDataPoint> ItemsDataPoints { get; set; }
        internal SeriesMetaData MetaData { get; set; }

        internal SeriesMetaData PrepareData(object dataContext)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (string.IsNullOrEmpty(ValueName) || string.IsNullOrEmpty(CategoryName))
            {
                ItemsCollection = null;
                return new SeriesMetaData();
            }

            var type = dataContext.GetType();

            if (contextType is null || type != contextType)
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

            ItemsDataPoints = new List<SeriesDataPoint>();
            var meta = new SeriesMetaData();

            foreach (var item in ItemsCollection)
            {
                var categoryValue = categoryPropertyInfo.GetValue(item);
                var displayValue = string.IsNullOrEmpty(DisplayName) ? string.Empty : displayPropertyInfo.GetValue(item);
                var value = (double?)valuePropertyInfo.GetValue(item);

                var dataPoint = new SeriesDataPoint()
                {
                    Value = value,
                    ValueText = value.FormatObject(ValueFormat),
                    Category = categoryValue.FormatObject(CategoryFormat),
                    Display = displayValue.FormatObject(DisplayFormat)
                };

                if (!meta.ValueMax.HasValue || value > meta.ValueMax.Value)
                {
                    meta.ValueMax = value;
                }

                if (!meta.ValueMin.HasValue || value < meta.ValueMin.Value)
                {
                    meta.ValueMin = value;
                }

                if (string.IsNullOrEmpty(meta.LongestCategory) || dataPoint.Category.Length > meta.LongestCategory.Length)
                {
                    meta.LongestCategory = dataPoint.Category;
                }

                ItemsDataPoints.Add(dataPoint);
            }

            MetaData = meta;
            return meta;
        }
    }

    internal class SeriesDataPoint
    {
        public string Category { get; set; }
        public string Display { get; set; }
        public double? Value { get; set; }
        public string ValueText { get; set; }
    }

    internal class SeriesMetaData
    {
        public double? ValueMin { get; set; }
        public double? ValueMax { get; set; }
        public string LongestCategory { get; set; }
    }
}
