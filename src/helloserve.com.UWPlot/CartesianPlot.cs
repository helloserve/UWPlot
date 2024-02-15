using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Markup;
using System.ServiceModel.Channels;

#if DEBUG
using System.Diagnostics;
#endif

namespace helloserve.com.UWPlot
{
    [ContentProperty(Name = "Series")]
    public abstract class CartesianPlot : Plot
    {
        public List<CartesianSeries> Series { get; set; } = new List<CartesianSeries>();
        public List<YAxis> YAxis { get; set; } = new List<YAxis>();
        public XAxis XAxis { get; set; } = new XAxis();
        public Style ToolTipStyle { get; set; }
        public double PaddingFactor { get; set; } = 1.05;

        private double gridLineStrokeThickness = 0.5;
        public double GridLineStrokeThickness
        {
            get { return gridLineStrokeThickness; }
            set
            {
                gridLineStrokeThickness = value;
            }
        }

        protected SeriesPointToolTip ToolTip { get; set; } = new SeriesPointToolTip();

        internal CartesianPlotExtents PlotExtents = new CartesianPlotExtents();

        private SeriesDrawDataPoints[] SeriesDataPoints = null;

        protected override void ClearLayout()
        {
            SeriesDataPoints = null;
            base.ClearLayout();
        }

        protected override void HandlePlotLoaded(object sender, RoutedEventArgs e)
        {
            if (LayoutRoot == null)
                return;

            LayoutRoot.PointerMoved += LayoutRoot_PointerMoved;
            LayoutRoot.PointerExited += LayoutRoot_PointerExited;
            ToolTip.Visibility = Visibility.Collapsed;
            ToolTip.Style = ToolTipStyle;
        }

        private void LayoutRoot_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ToolTip.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine("Hiding Tooltip");
        }

        private void LayoutRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (dataPrepException != null || !string.IsNullOrEmpty(dataValidationErrorMessage))
                return;

            var pointerPosition = e.GetCurrentPoint(this);
            double x = Math.Max(0, pointerPosition.Position.X);
            double y = Math.Max(0, pointerPosition.Position.Y);
            if (x < PlotExtents.PlotAreaTopLeft.X || x > PlotExtents.PlotAreaBottomRight.X || y < PlotExtents.PlotAreaTopLeft.Y || y > PlotExtents.PlotAreaBottomRight.Y)
            {
                return;
            }

            Size size = ToolTip.GetContentSize(new Size(PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X, PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y));

            double xDisplay = x + 32;
            double yDisplay = y + 32;

            if (xDisplay + size.Width > PlotExtents.PlotAreaBottomRight.X)
            {
                xDisplay = x - 32 - size.Width;
            }
            if (yDisplay + size.Height > PlotExtents.PlotAreaBottomRight.Y)
            {
                yDisplay = y - 32 - size.Height;
            }

            double xDisplay1 = PlotExtents.PlotAreaBottomRight.X + PlotExtents.PlotAreaTopLeft.X - (xDisplay + size.Width);
            double yDisplay1 = PlotExtents.PlotAreaBottomRight.Y + PlotExtents.PlotAreaTopLeft.Y - (yDisplay + size.Height);

            ToolTip.Margin = new Thickness(xDisplay, yDisplay, xDisplay1, yDisplay1);
            ToolTip.Visibility = Visibility.Visible;

            //find the point closest to x
            SeriesDrawDataPoints seriesPoints = SeriesDataPoints[0];
            int closestX = int.MaxValue;
            int index = -1;
            for (int i = 0; i < seriesPoints.SeriesDataPoints.Count; i++)
            {
                var point = seriesPoints.SeriesDataPoints[i];
                int diffX = Math.Abs((int)Math.Round(point.Item1.X - x));
                if (diffX < closestX)
                {
                    index = i;
                    closestX = diffX;
                }
                if (diffX > closestX)
                    break;  //if we start to diverge again, we're moving further away
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < SeriesDataPoints.Length; i++)
            {
                CartesianSeries series = Series[i];
                SeriesDrawDataPoints points = SeriesDataPoints[i];
                _ = stringBuilder
                    .Append(series.LegendDescription)
                    .Append(": ")
                    .Append(points.SeriesDataPoints[index].Item2.ValueText);

                if (i < SeriesDataPoints.Length - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            ToolTip.SetDebugText(stringBuilder.ToString());
            //ToolTip.SetDebugText($"Desired Size: {size.Width}x{size.Height}\r\nMargin: ({Math.Round(ToolTip.Margin.Left)},{Math.Round(ToolTip.Margin.Top)})x({Math.Round(ToolTip.Margin.Right)},{Math.Round(ToolTip.Margin.Bottom)})\r\nPlotArea: ({Math.Round(PlotExtents.PlotAreaTopLeft.X)},{Math.Round(PlotExtents.PlotAreaTopLeft.Y)})x({Math.Round(PlotExtents.PlotAreaBottomRight.X)},{Math.Round(PlotExtents.PlotAreaBottomRight.Y)})");

            System.Diagnostics.Debug.WriteLine($"Desired Size: {size.Width}x{size.Height}\r\nMargin: ({Math.Round(ToolTip.Margin.Left)},{Math.Round(ToolTip.Margin.Top)})x({Math.Round(ToolTip.Margin.Right)},{Math.Round(ToolTip.Margin.Bottom)})\r\nPlotArea: ({Math.Round(PlotExtents.PlotAreaTopLeft.X)},{Math.Round(PlotExtents.PlotAreaTopLeft.Y)})x({Math.Round(PlotExtents.PlotAreaBottomRight.X)},{Math.Round(PlotExtents.PlotAreaBottomRight.Y)})");

            InvalidateArrange();
        }

        internal override void PrepareData(DataExtents extents)
        {
            if (DataContext is null)
            {
                return;
            }

            if (Series is null)
            {
                return;
            }

            dataPrepException = null;

            try
            {
                bool hasData = false;

                foreach (CartesianSeries series in Series)
                {
                    SeriesMetaData meta = series.PrepareData(DataContext, FontSize, YAxis[0].LabelTransform);
                    hasData |= meta.Count > 0;

                    if (string.IsNullOrEmpty(extents.LongestCategory) || meta.LongestCategory?.Length > extents.LongestCategory.Length)
                    {
                        extents.LongestCategory = meta.LongestCategory;
                    }

                    extents.TotalCategoryWidth = meta.TotalCategoryWidth;
                }

                if (hasData)
                {
                    YAxis primaryAxis = YAxis.First(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);
                    List<CartesianSeries> primarySeries = Series.Where(x => x.AxisName == primaryAxis.Name).ToList();
                    if (!primarySeries.Any())
                    {
                        primarySeries = Series;
                    }

                    primaryAxis.CalculateSeriesBounds(primarySeries.Select(x => x.ItemsDataPoints).ToList());

                    extents.ValueMin = primaryAxis.CalculatedMin;
                    extents.ValueMinString = extents.ValueMin.FormatObject(primarySeries.First().ValueFormat);

                    extents.ValueMax = primaryAxis.CalculatedMax;
                    extents.ValueMaxString = extents.ValueMax.FormatObject(primarySeries.First().ValueFormat);
                }

                extents.IsPrepared = hasData;
            }
            catch (Exception ex)
            {
                dataPrepException = ex;
            }
        }

        protected override bool ValidateSeries()
        {
            var sw = Stopwatch.StartNew();
            string message = null;
            try
            {               
                if (dataPrepException != null)
                {
                    message = $"{dataPrepException.Message}{Environment.NewLine}{dataPrepException.StackTrace}";
                }
                else
                {

                    if (Series is null || Series.Count == 0 || Series.Any(x => x.ItemsDataPoints is null) || Series.Any(x => x.ItemsDataPoints.Count == 0))
                    {
                        message = "No series defined or series is empty.";

                        if (Series != null)
                        {
                            message = string.Empty;
                            foreach (var series in Series)
                            {
                                if (series.ItemsDataPoints is null || series.ItemsDataPoints.Count == 0)
                                {
                                    message += $"{Environment.NewLine}Series #{Series.IndexOf(series)} is not defined or empty.";
                                }
                            }
                        }
                    }

                    //exit here to prevent a slew of null-checks below
                    if (!string.IsNullOrEmpty(message))
                    {
                        return false;
                    }

                    if (Series.Select(x => x.ItemsDataPoints.Count).Distinct().Count() > 1)
                    {
                        message = "There are different number of data points in the different series. These have to match. Provide null values for the catogories that are missing.";
                    }

                    for (int i = 0; i < Series[0].ItemsDataPoints.Count; i++)
                    {
                        string category = Series[0].ItemsDataPoints[i].Category;
                        foreach (var series in Series)
                        {
                            if (!series.ItemsDataPoints[i].Category.Equals(category))
                            {
                                message += $"{Environment.NewLine}Series #{Series.IndexOf(series)} has category {series.ItemsDataPoints[i].Category} which does not match the first or primary series in position {i}.";
                            }
                        }
                    }

                    if (YAxis.Count == 0)
                    {
                        YAxis.Add(new YAxis());
                    }

                    if (YAxis.Count(x => x.AxisType == UWPlot.YAxis.YAxisType.Primary) > 1)
                    {
                        if (YAxis.Count == 2)
                        {
                            YAxis[0].AxisType = UWPlot.YAxis.YAxisType.Primary;
                            YAxis[1].AxisType = UWPlot.YAxis.YAxisType.Secondary;
                        }
                        else
                        {
                            message = "More than one primary y axis is defined. A graph can have only one primary y-axis, and an optional secondary y-axis";
                        }
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
#if DEBUG
                Debug.WriteLine($"{Name} ValidateSeries took {sw.ElapsedMilliseconds}ms");
#endif
                dataValidationErrorMessage = message;
            }
        }

        protected override void Draw()
        {
#if DEBUG
            Debug.WriteLine($"{Name} Draw");
#endif

            if (LayoutRoot == null)
                return;

            if (!ValidateSeries())
                return;

            var sw = Stopwatch.StartNew();

            try
            {

                ClearLayout();
                DrawPlotArea();
                DrawSeriesInternal();

#if DEBUG
                Debug.WriteLine($"{Name} Draw Complete");
#endif

                LayoutRoot.Children.Add(ToolTip);

#if DEBUG
                Debug.WriteLine($"{Name} ToolTip Added");
#endif
            }
            finally
            {
#if DEBUG
                Debug.WriteLine($"{Name} Draw took {sw.ElapsedMilliseconds}ms");
#endif
            }
        }

        protected override Size MeasurePlot(Size availableSize)
        {
            if (!DataExtents.IsPrepared)
                return availableSize;

            double actualWidth = availableSize.Width;
            double actualHeight = availableSize.Height;

            //legend
            double maxLegendWidth = 0;
            double legendHeight = 0;
            double legendWidth = 0;
            foreach (CartesianSeries legendSeries in Series)
            {
                if (!string.IsNullOrEmpty(legendSeries.LegendDescription))
                {
                    Size descriptionSize = legendSeries.LegendDescription.MeasureTextSize(FontSize).WithFactor(PaddingFactor);

                    legendWidth += descriptionSize.Width;

                    if (descriptionSize.Width > maxLegendWidth)
                    {
                        maxLegendWidth = descriptionSize.Width;
                    }

                    if (descriptionSize.Height > legendHeight)
                    {
                        legendHeight = descriptionSize.Height;
                    }
                }
            }

            PlotExtents.LegendItemIndicatorWidth = legendHeight;
            legendWidth += Series.Count * (40 + PlotExtents.LegendItemIndicatorWidth);
            PlotExtents.LegendAreaTopLeft = new Point(actualWidth * 0.5 - (legendWidth * 0.5), Padding.Top - 5);
            PlotExtents.LegendAreaBottomRight = new Point(actualWidth * 0.5 + (legendWidth * 0.5), Padding.Top - 5 + legendHeight);

            OnMeasureLegend();

            //plot area elements
            Size valueTextMaxSize = DataExtents.LongestValueString.MeasureTextSize(FontSize).WithFactor(PaddingFactor);
            Size categoryTextMaxSize = DataExtents.LongestCategory.MeasureTextSize(FontSize, transform: XAxis.LabelTransform).WithFactor(PaddingFactor);

            PlotExtents.PlotFrameTopLeft = new Point(Padding.Left + Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), Padding.Top + legendHeight);
            PlotExtents.PlotFrameBottomRight = new Point(actualWidth - Padding.Right - Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), actualHeight - Padding.Bottom - categoryTextMaxSize.Height - legendHeight);

            OnMeasurePlotFrame();

            PlotExtents.PlotAreaTopLeft = new Point(PlotExtents.PlotFrameTopLeft.X + PlotExtents.PlotAreaPadding.Left, PlotExtents.PlotFrameTopLeft.Y + PlotExtents.PlotAreaPadding.Top);
            PlotExtents.PlotAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X - PlotExtents.PlotAreaPadding.Right, PlotExtents.PlotFrameBottomRight.Y - PlotExtents.PlotAreaPadding.Bottom);

            OnMeasurePlotArea();

            PlotExtents.VerticalGridLineCount = Series[0].ItemsDataPoints.Count - 2;
            if (PlotExtents.VerticalGridLineCount + 1 == 0)
                PlotExtents.VerticalGridLineCount = 1;
            else
                PlotExtents.VerticalGridLineSpace = (PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X) / (PlotExtents.VerticalGridLineCount + 1);

            OnMeasureVerticalGridLines();

            //from primary YAxis            
            double height = PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y;
            double minHeight = valueTextMaxSize.Height * 1.2;  //add 20% buffer area

            YAxis primaryAxis = YAxis.First(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);
            var extents = primaryAxis.Measure(height, minHeight);
            PlotExtents.NumberOfScaleLines = extents.NumberOfScaleLines;
            PlotExtents.ScaleLineIncrements = extents.ScaleLineIncrements;

            OnMeasurePrimaryYAxis();

            //secondary axis
            bool hasSecondaryY = false;
            foreach (YAxis axis in YAxis.Except(new List<YAxis>() { primaryAxis }))
            {
                hasSecondaryY = true;
                var secondarySeries = Series.Where(x => x.AxisName == axis.Name).ToList();
                if (!secondarySeries.Any())
                    secondarySeries = Series;

                var max = secondarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Max(x => x.Value.Value);
                var min = secondarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Min(x => x.Value.Value);

                axis.Measure(max, min, PlotExtents.NumberOfScaleLines);
            }

            OnMeasureSecondaryYAxis();

            Size measuredSize = new Size(
                Math.Max(0, (PlotExtents.PlotFrameBottomRight.X + (hasSecondaryY ? valueTextMaxSize.Width : 0)) - (PlotExtents.PlotFrameTopLeft.X - valueTextMaxSize.Width)),
                Math.Max(0, (PlotExtents.LegendAreaBottomRight.Y - PlotExtents.LegendAreaTopLeft.Y) + (PlotExtents.PlotFrameBottomRight.Y - PlotExtents.PlotFrameTopLeft.Y) + categoryTextMaxSize.Height));

            return new Size(Math.Max(0, Math.Max(availableSize.Width, measuredSize.Width)), Math.Max(0, Math.Max(availableSize.Height, measuredSize.Height)));
        }

        protected virtual void OnMeasureLegend() { }
        protected virtual void OnMeasurePlotFrame() { }
        protected virtual void OnMeasurePlotArea() { }
        protected virtual void OnMeasureVerticalGridLines() { }
        protected virtual void OnMeasurePrimaryYAxis() { }
        protected virtual void OnMeasureSecondaryYAxis() { }

        protected virtual void DrawPlotArea()
        {
            if (!DataExtents.IsPrepared)
                return;

#if DEBUG
            Debug.WriteLine($"{Name} DrawPlotArea");
#endif
            double actualWidth = ActualWidth;
            double actualHeight = ActualHeight;

            LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);

            //LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);

#if DEBUG
            Debug.WriteLine($"{Name} PlotExtents.NumberOfScaleLines: {PlotExtents.NumberOfScaleLines}");
#endif

            for (int i = 0; i < PlotExtents.NumberOfScaleLines; i++)
            {
                double x1 = PlotExtents.PlotFrameTopLeft.X;
                double y1 = PlotExtents.PlotAreaBottomRight.Y - (i * PlotExtents.ScaleLineIncrements);

                double x2 = PlotExtents.PlotFrameBottomRight.X;
                double y2 = y1;

#if DEBUG
                Debug.WriteLine($"{Name} ScaleLine: ({x1},{y1}) - ({x2},{y2})");
#endif
                LayoutRoot.DrawLine(x1, y1, x2, y2, PlotAreaStrokeBrush, GridLineStrokeThickness);
            }

            foreach (YAxis axis in YAxis)
            {
                CartesianSeries series = Series.FirstOrDefault(s => s.AxisName == axis.Name);
                for (int i = 0; i < axis.ScaleValues.Count; i++)
                {
                    double value = axis.ScaleValues[i];

                    double x1 = PlotExtents.PlotFrameTopLeft.X;
                    double y1 = PlotExtents.PlotAreaBottomRight.Y - (i * PlotExtents.ScaleLineIncrements);

                    double x2 = PlotExtents.PlotFrameBottomRight.X;
                    double y2 = y1;

                    LayoutRoot.DrawScaleValueItem(value.FormatObject(series?.ValueFormat), axis.AxisType == UWPlot.YAxis.YAxisType.Primary ? x1 : x2, y1, FontSize, axis.AxisType, paddingFactor: PaddingFactor, transform: axis.LabelTransform);
                }
            }

            double lineStepX = PlotExtents.VerticalGridLineSpace + PlotExtents.PlotAreaTopLeft.X;
            double categoryStep = Math.Ceiling(Math.Max(1D, DataExtents.TotalCategoryWidth / PlotExtents.AreaWidth));
            for (int i = 0; i < PlotExtents.VerticalGridLineCount; i++)
            {
                LayoutRoot.DrawLine(lineStepX, PlotExtents.PlotFrameTopLeft.Y, lineStepX, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, GridLineStrokeThickness);
                
                if (PlotExtents.VerticalGridLineCount >= 1 && (i + 1) % categoryStep == 0)
                {
                    if (i == PlotExtents.VerticalGridLineCount - 1 && categoryStep > 1)
                        continue;

                    LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[1 + i].Category, lineStepX, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);
                }                

                lineStepX += PlotExtents.VerticalGridLineSpace;
            }
            LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[0].Category, PlotExtents.PlotAreaTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);
            LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[Series[0].ItemsDataPoints.Count - 1].Category, PlotExtents.PlotAreaBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);

            double legendX = (PlotExtents.LegendAreaBottomRight.X - PlotExtents.LegendAreaTopLeft.X) * 0.08D;
            foreach (CartesianSeries series in Series)
            {
                var drawSize = LayoutRoot.DrawLegendItem(series.LegendDescription, legendX + PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, FontSize, PlotExtents.LegendItemIndicatorWidth, GetSeriesColor(Series.IndexOf(series)).StrokeBrush);
                legendX += drawSize.Width;
            }
        }

        private void DrawSeriesInternal()
        {
#if DEBUG
            Debug.WriteLine($"{Name} DrawSeriesInternal");
#endif

            if (SeriesDataPoints == null)
            {
                SeriesDataPoints = CalculateSeriesDataPoints();
            }

            DrawSeries(SeriesDataPoints);
        }

        internal virtual SeriesDrawDataPoints[] CalculateSeriesDataPoints()
        {
            var seriesDataPoints = new SeriesDrawDataPoints[Series.Count];

            double plotHeight = PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y;

            foreach (CartesianSeries series in Series)
            {
                YAxis axis = YAxis.SingleOrDefault(a => a.Name == series.AxisName);
                axis = axis ?? YAxis.SingleOrDefault(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);

                Point axisZeroLine = new Point(0, PlotExtents.PlotAreaBottomRight.Y - (-axis.CalculatedMin) / axis.CalculateRange * plotHeight);

                List<Tuple<Point, SeriesDataPoint>> linePlotPoints = new List<Tuple<Point, SeriesDataPoint>>();

                for (int i = 0; i < series.ItemsDataPoints.Count; i++)
                {
                    SeriesDataPoint dataPoint = series.ItemsDataPoints[i];

                    if (!dataPoint.Value.HasValue)
                    {
                        linePlotPoints.Add(new Tuple<Point, SeriesDataPoint>(new Point(), dataPoint));
                        continue;
                    }

                    double plotValue = (dataPoint.Value.Value - axis.CalculatedMin) / axis.CalculateRange * plotHeight;

                    double x = (PlotExtents.VerticalGridLineSpace * i) + PlotExtents.PlotAreaTopLeft.X;
                    double y = PlotExtents.PlotAreaBottomRight.Y - plotValue;

                    linePlotPoints.Add(new Tuple<Point, SeriesDataPoint>(new Point(x, y), dataPoint));
                }

                seriesDataPoints[Series.IndexOf(series)] = new SeriesDrawDataPoints()
                {
                    ZeroLine = axisZeroLine,
                    DrawValueItem = series.ShowDataPointValues,
                    SeriesDataPoints = linePlotPoints
                };
            }

            return seriesDataPoints;
        }


        internal virtual void DrawSeries(SeriesDrawDataPoints[] seriesDataPoints)
        {

        }

        protected virtual PlotColorItem GetSeriesColor(int index)
        {
            var ratio = PlotColors.Count / Series.Count;

            int colorIndex = 0;
            if (ratio <= 0)
            {
                colorIndex = index % PlotColors.Count;
            }
            else
            {
                colorIndex = index * ratio;
            }

            return PlotColors[colorIndex];
        }
    }

    internal class SeriesDrawDataPoints
    {
        public Point ZeroLine { get; set; }
        public bool DrawValueItem { get; set; }
        public List<Tuple<Point, SeriesDataPoint>> SeriesDataPoints { get; set; }

    }
}
