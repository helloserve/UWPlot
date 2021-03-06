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

namespace helloserve.com.UWPlot
{
    public abstract class Plot : Control
    {
        public List<Series> Series { get; set; } = new List<Series>();
        public List<YAxis> YAxis { get; set; } = new List<YAxis>();
        public XAxis XAxis { get; set; } = new XAxis();
        public Style ToolTipStyle { get; set; }
        public double PaddingFactor { get; set; } = 1;

        public List<SolidColorBrush> PlotColors = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Colors.PowderBlue),
            new SolidColorBrush(Colors.PaleVioletRed),
            new SolidColorBrush(Colors.LightSeaGreen)
        };

        private Brush plotAreaStrokeBrush = new SolidColorBrush(Colors.Gray);
        public Brush PlotAreaStrokeBrush
        {
            get { return plotAreaStrokeBrush; }
            set
            {
                plotAreaStrokeBrush = value;
                Invalidate();
            }
        }

        private double plotAreaStrokeThickness = 2;
        public double PlotAreaStrokeThickness
        {
            get { return plotAreaStrokeThickness; }
            set 
            { 
                plotAreaStrokeThickness = value;
                Invalidate();
            }
        }

        private double gridLineStrokeThickness = 0.5;
        public double GridLineStrokeThickness
        {
            get { return gridLineStrokeThickness; }
            set
            {
                gridLineStrokeThickness = value;
                Invalidate();
            }
        }

        protected Grid LayoutRoot = null;
        protected SeriesPointToolTip ToolTip { get; set; } = new SeriesPointToolTip();

        internal DataExtents DataExtents = new DataExtents();
        internal PlotExtents PlotExtents = new PlotExtents();

        private List<Tuple<Point, SeriesDataPoint>>[] SeriesDataPoints = null;

        public Plot()
        {
            this.DefaultStyleKey = typeof(Plot);

            DataContextChanged += Plot_DataContextChanged;
            Loaded += Plot_Loaded;
            SizeChanged += Plot_SizeChanged;

            IsHitTestVisible = true;
        }

        protected virtual void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Invalidate();
        }

        protected virtual void Plot_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutRoot = (Grid)VisualTreeHelper.GetChild(this, 0);
            LayoutRoot.PointerMoved += LayoutRoot_PointerMoved;
            LayoutRoot.PointerExited += LayoutRoot_PointerExited;
            ToolTip.Visibility = Visibility.Collapsed;
            ToolTip.Style = ToolTipStyle;

            Layout();
        }

        private void LayoutRoot_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ToolTip.Visibility = Visibility.Collapsed;
        }

        private void LayoutRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
            double x = Math.Max(0, pointerPosition.X - Window.Current.Bounds.X);
            double y = Math.Max(0, pointerPosition.Y - Window.Current.Bounds.Y);

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
            List<Tuple<Point, SeriesDataPoint>> seriesPoints = SeriesDataPoints[0];
            int closestX = int.MaxValue;
            int index = -1;
            for (int i = 0; i < seriesPoints.Count; i++)
            {
                var point = seriesPoints[i];
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
                Series series = Series[i];
                List<Tuple<Point, SeriesDataPoint>> points = SeriesDataPoints[i];
                _ = stringBuilder
                    .Append(series.LegendDescription)
                    .Append(": ")
                    .Append(points[index].Item2.ValueText);

                if (i < SeriesDataPoints.Length - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            ToolTip.SetDebugText(stringBuilder.ToString());
            //toolTip.SetDebugText($"Desired Size: {size.Width}x{size.Height}\r\nMargin: ({Math.Round(toolTip.Margin.Left)},{Math.Round(toolTip.Margin.Top)})x({Math.Round(toolTip.Margin.Right)},{Math.Round(toolTip.Margin.Bottom)})\r\nPlotArea: ({Math.Round(plotAreaTopLeft.X)},{Math.Round(plotAreaTopLeft.Y)})x({Math.Round(plotAreaBottomRight.X)},{Math.Round(plotAreaBottomRight.Y)})");

            Invalidate();
        }

        protected virtual void Plot_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            INotifyPropertyChanged contextNotify = DataContext as INotifyPropertyChanged;
            if (contextNotify != null)
            {
                contextNotify.PropertyChanged -= ContextNotify_PropertyChanged;
            }

            contextNotify = args.NewValue as INotifyPropertyChanged;
            if (contextNotify != null)
            {
                contextNotify.PropertyChanged += ContextNotify_PropertyChanged;
            }

            ContextNotify_PropertyChanged(this, new PropertyChangedEventArgs(nameof(DataContext)));
        }

        private void ContextNotify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PrepareData(DataExtents);
            Invalidate();
        }

        protected virtual void Invalidate()
        {
            SeriesDataPoints = null;
            Layout();
            InvalidateArrange();
        }

        private void Layout()
        {
            if (LayoutRoot is null)
            {
                return;
            }

            LayoutRoot.Children.Clear();

            if (ValidateSeries())
            {
                Draw();
            }
        }

        internal virtual void PrepareData(DataExtents extents)
        {
            if (DataContext is null)
            {
                return;
            }

            if (Series is null)
            {
                return;
            }

            foreach (Series series in Series)
            {
                SeriesMetaData meta = series.PrepareData(DataContext);

                if (!extents.ValueMin.HasValue || meta.ValueMin < extents.ValueMin)
                {
                    extents.ValueMin = meta.ValueMin;
                }

                if (!extents.ValueMax.HasValue || meta.ValueMax > extents.ValueMax)
                {
                    extents.ValueMax = meta.ValueMax;
                    extents.ValueMaxString = extents.ValueMax.FormatObject(series.ValueFormat);
                }

                if (string.IsNullOrEmpty(extents.LongestCategory) || meta.LongestCategory.Length > extents.LongestCategory.Length)
                {
                    extents.LongestCategory = meta.LongestCategory;
                }
            }
        }

        protected virtual bool ValidateSeries()
        {
            string message = null;

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

            LayoutRoot.Children.Add(new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = LayoutRoot.ActualWidth,
                Y2 = LayoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 10
            });

            LayoutRoot.Children.Add(new Line()
            {
                X1 = LayoutRoot.ActualWidth,
                Y1 = 0,
                X2 = 0,
                Y2 = LayoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 10
            });

            LayoutRoot.Children.Add(new TextBlock()
            {
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            return false;
        }

        protected void Draw()
        {
            if (double.IsNaN(ActualWidth) || double.IsNaN(ActualHeight))
            {
                LayoutRoot.Children.Add(new TextBlock()
                {
                    Text = "Plot layout not complete.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });

                return;
            }

            Measure();
            DrawPlotArea();
            DrawSeriesInternal();

            LayoutRoot.Children.Add(ToolTip);
        }

        protected virtual void Measure()
        {
            //legend
            double maxLegendWidth = 0;
            double legendHeight = 0;
            double legendWidth = 0;
            foreach (Series legendSeries in Series)
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
            PlotExtents.LegendAreaTopLeft = new Point(ActualWidth * 0.5 - (legendWidth * 0.5), Padding.Top);
            PlotExtents.LegendAreaBottomRight = new Point(ActualWidth * 0.5 + (legendWidth * 0.5), Padding.Top + legendHeight);

            OnMeasureLegend();

            //plot area elements
            Size valueTextMaxSize = DataExtents.ValueMaxString.MeasureTextSize(FontSize).WithFactor(PaddingFactor);
            Size categoryTextMaxSize = DataExtents.LongestCategory.MeasureTextSize(FontSize, transform: XAxis.LabelTransform).WithFactor(PaddingFactor);

            PlotExtents.PlotFrameTopLeft = new Point(Padding.Left + Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), Padding.Top + legendHeight);
            PlotExtents.PlotFrameBottomRight = new Point(ActualWidth - Padding.Right - Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), ActualHeight - Padding.Bottom - categoryTextMaxSize.Height - legendHeight);

            OnMeasurePlotFrame();

            PlotExtents.PlotAreaTopLeft = new Point(PlotExtents.PlotFrameTopLeft.X + PlotExtents.PlotAreaPadding.Left, PlotExtents.PlotFrameTopLeft.Y + PlotExtents.PlotAreaPadding.Top);
            PlotExtents.PlotAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X - PlotExtents.PlotAreaPadding.Right, PlotExtents.PlotFrameBottomRight.Y - PlotExtents.PlotAreaPadding.Bottom);

            OnMeasurePlotArea();

            PlotExtents.VerticalGridLineCount = Series[0].ItemsDataPoints.Count - 2;
            PlotExtents.VerticalGridLineSpace = (PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X) / (PlotExtents.VerticalGridLineCount + 1);

            OnMeasureVerticalGridLines();

            //from primary YAxis

            YAxis primaryAxis = YAxis.First(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);

            List<Series> primarySeries = Series.Where(x => x.AxisName == primaryAxis.Name).ToList();
            if (!primarySeries.Any())
            {
                primarySeries = Series;
            }

            double max = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Max(x => x.Value.Value);
            double min = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Min(x => x.Value.Value);

            double height = PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y;
            double minHeight = valueTextMaxSize.Height * 1.2;  //add 20% buffer area
            PlotExtents.NumberOfScaleLines = (int)(height / minHeight) / 2;
            if (PlotExtents.NumberOfScaleLines % 10 != 0)
            {
                PlotExtents.NumberOfScaleLines -= PlotExtents.NumberOfScaleLines % 10;
            }
            PlotExtents.ScaleLineIncrements = height / PlotExtents.NumberOfScaleLines;

            primaryAxis.Measure(max, min, PlotExtents.NumberOfScaleLines);

            OnMeasurePrimaryYAxis();

            //secondary axis
            foreach (YAxis axis in YAxis.Except(new List<YAxis>() { primaryAxis }))
            {
                primarySeries = Series.Where(x => x.AxisName == axis.Name).ToList();
                if (!primarySeries.Any())
                    primarySeries = Series;

                max = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Max(x => x.Value.Value);
                min = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Min(x => x.Value.Value);

                axis.Measure(max, min, PlotExtents.NumberOfScaleLines);
            }

            OnMeasureSecondaryYAxis();
        }

        protected virtual void OnMeasureLegend() { }
        protected virtual void OnMeasurePlotFrame() { }
        protected virtual void OnMeasurePlotArea() { }
        protected virtual void OnMeasureVerticalGridLines() { }
        protected virtual void OnMeasurePrimaryYAxis() { }
        protected virtual void OnMeasureSecondaryYAxis() { }

        protected virtual void DrawPlotArea()
        {
            LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);

            LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);

            for (int i = 0; i < PlotExtents.NumberOfScaleLines; i++)
            {
                double x1 = PlotExtents.PlotFrameTopLeft.X;
                double y1 = PlotExtents.PlotAreaBottomRight.Y - (i * PlotExtents.ScaleLineIncrements);

                double x2 = PlotExtents.PlotFrameBottomRight.X;
                double y2 = y1;

                LayoutRoot.DrawLine(x1, y1, x2, y2, PlotAreaStrokeBrush, PlotAreaStrokeThickness);
            }

            foreach (YAxis axis in YAxis)
            {
                Series series = Series.FirstOrDefault(s => s.AxisName == axis.Name);
                for (int i = 0; i < axis.ScaleValues.Count; i++)
                {
                    double value = (axis.CalculatedIncrement * i) + axis.CalculatedMin;

                    double x1 = PlotExtents.PlotFrameTopLeft.X;
                    double y1 = PlotExtents.PlotAreaBottomRight.Y - (i * PlotExtents.ScaleLineIncrements);

                    double x2 = PlotExtents.PlotFrameBottomRight.X;
                    double y2 = y1;

                    LayoutRoot.DrawScaleValueItem(value.FormatObject(series?.ValueFormat), axis.AxisType == UWPlot.YAxis.YAxisType.Primary ? x1 : x2, y1, FontSize, axis.AxisType, paddingFactor: PaddingFactor, transform: axis.LabelTransform);
                }
            }

            double lineStepX = PlotExtents.VerticalGridLineSpace + PlotExtents.PlotAreaTopLeft.X;
            for (int i = 0; i < PlotExtents.VerticalGridLineCount; i++)
            {
                LayoutRoot.DrawLine(lineStepX, PlotExtents.PlotFrameTopLeft.Y, lineStepX, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, GridLineStrokeThickness);
                LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[1 + i].Category, lineStepX, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);

                lineStepX += PlotExtents.VerticalGridLineSpace;
            }
            LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[0].Category, PlotExtents.PlotAreaTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);
            LayoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[Series[0].ItemsDataPoints.Count - 1].Category, PlotExtents.PlotAreaBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, FontSize, paddingFactor: PaddingFactor, transform: XAxis.LabelTransform);

            double legendX = (PlotExtents.LegendAreaBottomRight.X - PlotExtents.LegendAreaTopLeft.X) * 0.08D;
            foreach (Series series in Series)
            {
                var drawSize = LayoutRoot.DrawLegendItem(series.LegendDescription, legendX + PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, FontSize, PlotExtents.LegendItemIndicatorWidth, PlotColors[Series.IndexOf(series)]);
                legendX += drawSize.Width;
            }
        }

        private void DrawSeriesInternal()
        {
            if (SeriesDataPoints == null)
            {
                SeriesDataPoints = CalculateSeriesDataPoints();
            }

            DrawSeries(SeriesDataPoints);
        }

        internal virtual List<Tuple<Point, SeriesDataPoint>>[] CalculateSeriesDataPoints()
        {
            var seriesDataPoints = new List<Tuple<Point, SeriesDataPoint>>[Series.Count];

            double plotHeight = PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y;

            foreach (Series series in Series)
            {
                YAxis axis = YAxis.SingleOrDefault(a => a.Name == series.AxisName);
                axis = axis ?? YAxis.SingleOrDefault(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);

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

                seriesDataPoints[Series.IndexOf(series)] = linePlotPoints;
            }

            return seriesDataPoints;
        }


        internal virtual void DrawSeries(List<Tuple<Point, SeriesDataPoint>>[] seriesDataPoints)
        {

        }
    }
}
