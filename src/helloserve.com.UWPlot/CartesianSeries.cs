using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml.Media;
using Windows.Gaming.Input.Custom;
using Windows.Media.Streaming.Adaptive;



#if DEBUG
using System.Diagnostics;    
#endif

namespace helloserve.com.UWPlot
{
    public class CartesianSeries : Series
    {

        /// <summary>
        /// The name of the Y axis that this series is bound to.
        /// </summary>
        public string AxisName { get; set; }

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

        internal override SeriesMetaData PrepareData(object dataContext, double fontSize = 12, Transform categoryTransform = null)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (string.IsNullOrEmpty(ValueName) || string.IsNullOrEmpty(CategoryName))
            {
                ItemsCollection = null;
                return SeriesMetaData.Empty;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                var type = dataContext.GetType();
                Type sourceType = null;

                if (contextType is null || type != contextType || sourceProperty is null)
                {
                    contextType = type;

                    var sourceBinding = ItemsSource as Windows.UI.Xaml.Data.Binding;
                    if (sourceBinding != null)
                    {
                        sourceProperty = type.GetProperty(sourceBinding.Path?.Path);
                        if (sourceProperty == null)
                        {
                            throw new ArgumentNullException($"ItemsSource is not a property of {type.Name}.");
                        }

                        ItemsCollection = sourceProperty.GetValue(dataContext) as IEnumerable;

                        sourceType = sourceProperty.PropertyType;

                        if (sourceType.GetInterface(nameof(IEnumerable)) is null)
                        {
                            throw new ArgumentException($"ItemsSource is configured with {sourceBinding.Path.Path}, but it doesn't implement IEnumerable.");
                        }
                    }
                    else
                    {
                        if (ItemsSource == null)
                            return SeriesMetaData.Empty;

                        var collection = ItemsSource as IEnumerable;

                        if (collection == null)
                        {
                            throw new ArgumentException($"ItemsSource is of type {ItemsSource.GetType().Name} and doesn't implement IEnumerable.");
                        }

                        ItemsCollection = collection;

                        sourceType = collection.GetType();
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

                ItemsDataPoints = new List<SeriesDataPoint>();
                var meta = new SeriesMetaData();

                foreach (var item in ItemsCollection)
                {
                    var categoryValue = categoryPropertyInfo.GetValue(item);
                    var displayValue = string.IsNullOrEmpty(DisplayName) ? string.Empty : displayPropertyInfo.GetValue(item);
                    var value = (double?)valuePropertyInfo.GetValue(item);
                    if (value == double.NaN)
                        value = 0;

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

                    ItemsDataPoints.Add(dataPoint);
                }

                meta.Count = ItemsDataPoints.Count;
                MetaData = meta;
                return meta;
            }
            finally
            {
#if DEBUG
                Debug.WriteLine($"Series PrepareData took {sw.ElapsedMilliseconds}ms");
#endif
            }
        }
    }

}
