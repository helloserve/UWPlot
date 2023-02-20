using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public abstract class Series
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
        /// The description in the legend box of this series.
        /// </summary>
        public string LegendDescription { get; set; }

        /// <summary>
        /// Controls whether the specific values are shown at the data points in the graph.
        /// </summary>
        public bool ShowDataPointValues { get; set; }

        internal abstract SeriesMetaData PrepareData(object dataContext, double fontSize = 12, Transform categoryTransform = null);
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
        public int Count { get; set; }
        public double? ValueMin { get; set; }
        public int ValueMinLength { get; set; }
        public double? ValueMax { get; set; }
        public int ValueMaxLength { get; set; }
        public string LongestCategory { get; set; }
        public double TotalCategoryWidth { get; set; }

        public static SeriesMetaData Empty => new SeriesMetaData();
    }
}
