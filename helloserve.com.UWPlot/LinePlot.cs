using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace helloserve.com.UWPlot
{
    public sealed class LinePlot : Control
    {
        public List<Series> Series { get; set; } = new List<Series>();
        public List<YAxis> YAxis { get; set; } = new List<YAxis>();
        public XAxis XAxis { get; set; } = new XAxis();

        public LinePlot()
        {
            this.DefaultStyleKey = typeof(LinePlot);

            DataContextChanged += LinePlot3_DataContextChanged;
            Loaded += LinePlot3_Loaded;
        }

        private Grid layoutRoot = null;

        //private List<SeriesMetaData> seriesMeta = new List<SeriesMetaData>();
        private int numberOfScaleLines;
        private double scaleLineIncrements;
        private double? valueMin;
        private double? valueMax;
        private string valueMaxString;
        private string longestCategory;

        private Point plotAreaTopLeft;
        private Point plotAreaBottomRight;
        private int verticalGridLineCount;
        private double verticalGridLineSpace;
        private Point legendAreaTopLeft;
        private Point legendAreaBottomRight;
        private double legendItemIndicatorWidth;

        private void LinePlot3_Loaded(object sender, RoutedEventArgs e)
        {
            layoutRoot = (Grid)VisualTreeHelper.GetChild(this, 0);
            Layout();
        }

        private void LinePlot3_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
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

            Invalidate();
        }

        private void ContextNotify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            PrepareData();
            Layout();
            InvalidateArrange();
        }

        private void PrepareData()
        {
            if (DataContext is null)
            {
                return;
            }

            if (Series is null)
            {
                return;
            }

            //seriesMeta = new List<SeriesMetaData>();
            foreach(Series series in Series)
            {
                SeriesMetaData meta = series.PrepareData(DataContext);

                if (!valueMin.HasValue || meta.ValueMin < valueMin)
                {
                    valueMin = meta.ValueMin;
                }

                if (!valueMax.HasValue || meta.ValueMax > valueMax)
                {
                    valueMax = meta.ValueMax;
                    valueMaxString = valueMax.FormatObject(series.ValueFormat);
                }

                if (string.IsNullOrEmpty(longestCategory) || meta.LongestCategory.Length > longestCategory.Length)
                {
                    longestCategory = meta.LongestCategory;
                }

                //seriesMeta.Add(meta);
            }
        }

        private void Layout()
        {
            if (layoutRoot is null)
            {
                return;
            }

            layoutRoot.Children.Clear();

            if (ValidateSeries())
            {
                Plot();
            }
        }

        private bool ValidateSeries()
        {
            string message = null;

            if (Series is null || Series.Count == 0 || Series.Any(x => x.ItemsDataPoints is null) || Series.Any(x => x.ItemsDataPoints.Count == 0))
            {
                message = "No series defined or series is empty.";

                if (Series != null)
                {
                    message = string.Empty;
                    foreach(var series in Series)
                    {
                        if (series.ItemsDataPoints == null || series.ItemsDataPoints.Count == 0)
                        {
                            message += $"{Environment.NewLine}Series #{Series.IndexOf(series)} is not defined or empty.";
                        }
                    }
                }
            }

            //exit there to prevent a slew of null-checks below
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

            layoutRoot.Children.Add(new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = layoutRoot.ActualWidth,
                Y2 = layoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Windows.UI.Colors.Red),
                StrokeThickness = 10
            });

            layoutRoot.Children.Add(new Line()
            {
                X1 = layoutRoot.ActualWidth,
                Y1 = 0,
                X2 = 0,
                Y2 = layoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Windows.UI.Colors.Red),
                StrokeThickness = 10
            });

            layoutRoot.Children.Add(new TextBlock()
            {
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            return false;
        }

        private void Plot()
        {
            if (double.IsNaN(ActualWidth) || double.IsNaN(ActualHeight))
            {
                layoutRoot.Children.Add(new TextBlock()
                {
                    Text = "Plot layout not complete.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });

                return;
            }

            Measure();
            DrawPlotArea();
            DrawSeries();
        }

        private void Measure()
        {
            //legend
            double maxLegendWidth = 0;
            double legendHeight = 0;
            double legendWidth = 0;
            foreach (Series legendSeries in Series)
            {
                if (!string.IsNullOrEmpty(legendSeries.LegendDescription))
                {
                    Size descriptionSize = legendSeries.LegendDescription.MeasureTextSize(FontSize);
                    
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

            legendItemIndicatorWidth = maxLegendWidth * 0.3;
            legendWidth += Series.Count * (40 + legendItemIndicatorWidth);
            legendAreaTopLeft = new Point(ActualWidth * 0.5 - (legendWidth * 0.5), Padding.Top);
            legendAreaBottomRight = new Point(ActualWidth * 0.5 + (legendWidth * 0.5), Padding.Top + legendHeight);
            
            //plot area elements
            Size valueTextMaxSize = valueMaxString.MeasureTextSize(FontSize);                        
            Size categoryTextMaxSize = longestCategory.MeasureTextSize(FontSize);
            
            plotAreaTopLeft = new Point(Padding.Left + Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), Padding.Top + legendHeight);
            plotAreaBottomRight = new Point(ActualWidth - Padding.Right - Math.Max(valueTextMaxSize.Width, categoryTextMaxSize.Width / 2), ActualHeight - Padding.Bottom - categoryTextMaxSize.Height - legendHeight);

            verticalGridLineCount = Series[0].ItemsDataPoints.Count - 2;
            verticalGridLineSpace = (plotAreaBottomRight.X - plotAreaTopLeft.X) / (verticalGridLineCount + 1);

            //from primary YAxis

            YAxis primaryAxis = YAxis.First(a => a.AxisType == UWPlot.YAxis.YAxisType.Primary);

            List<Series> primarySeries = Series.Where(x => x.AxisName == primaryAxis.Name).ToList();
            if (!primarySeries.Any())
            {
                primarySeries = Series;
            }

            double max = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Max(x => x.Value.Value);
            double min = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Min(x => x.Value.Value);

            double height = plotAreaBottomRight.Y - plotAreaTopLeft.Y;
            double minHeight = valueTextMaxSize.Height * 1.2;  //add 20% buffer area
            numberOfScaleLines = (int)(height / minHeight) / 2;
            if (numberOfScaleLines % 10 != 0)
            {
                numberOfScaleLines -= numberOfScaleLines % 10;
            }
            scaleLineIncrements = height / numberOfScaleLines;

            primaryAxis.Measure(max, min, numberOfScaleLines);

            //secondary axis
            foreach (YAxis axis in YAxis.Except(new List<YAxis>() { primaryAxis }))
            {
                primarySeries = Series.Where(x => x.AxisName == axis.Name).ToList();
                if (!primarySeries.Any())
                    primarySeries = Series;

                max = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Max(x => x.Value.Value);
                min = primarySeries.SelectMany(x => x.ItemsDataPoints).Where(x => x.Value.HasValue).Min(x => x.Value.Value);

                axis.Measure(max, min, numberOfScaleLines);
            }
        }

        private void DrawPlotArea()
        {
            SolidColorBrush plotAreaStroke = new SolidColorBrush(Colors.Gray);
            SolidColorBrush plotAreaStrokeDebug = new SolidColorBrush(Colors.Red);
            SolidColorBrush plotAreaStrokeDebug1 = new SolidColorBrush(Colors.Green);
            double plotAreaStrokeThickness = 2;

            double gridLineStrokeThickness = 0.5;

            for (int i = 0; i < numberOfScaleLines; i++)
            {
                double x1 = plotAreaTopLeft.X;
                double y1 = plotAreaBottomRight.Y - (i * scaleLineIncrements);

                double x2 = plotAreaBottomRight.X;
                double y2 = y1;

                layoutRoot.DrawLine(x1, y1, x2, y2, plotAreaStroke, plotAreaStrokeThickness);
            }

            foreach (YAxis axis in YAxis)
            {
                Series series = Series.FirstOrDefault(s => s.AxisName == axis.Name);
                for (int i = 0; i < axis.ScaleValues.Count; i++)
                {
                    double value = (axis.CalculatedIncrement * i) + axis.CalculatedMin;

                    double x1 = plotAreaTopLeft.X;
                    double y1 = plotAreaBottomRight.Y - (i * scaleLineIncrements);

                    double x2 = plotAreaBottomRight.X;
                    double y2 = y1;
                    
                    layoutRoot.DrawScaleValueItem(value.FormatObject(series?.ValueFormat), axis.AxisType == UWPlot.YAxis.YAxisType.Primary ? x1 : x2, y1, FontSize, axis.AxisType);
                }
            }

            double lineStepX = verticalGridLineSpace + plotAreaTopLeft.X;
            for (int i = 0; i < verticalGridLineCount; i++)
            {
                layoutRoot.DrawLine(lineStepX, plotAreaTopLeft.Y, lineStepX, plotAreaBottomRight.Y, plotAreaStroke, gridLineStrokeThickness);
                layoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[1 + i].Category, lineStepX, plotAreaBottomRight.Y, FontSize);
                
                lineStepX += verticalGridLineSpace;
            }
            layoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[0].Category, plotAreaTopLeft.X, plotAreaBottomRight.Y, FontSize);
            layoutRoot.DrawCategoryItem(Series[0].ItemsDataPoints[Series[0].ItemsDataPoints.Count - 1].Category, plotAreaBottomRight.X, plotAreaBottomRight.Y, FontSize);

            layoutRoot.DrawLine(plotAreaTopLeft.X, plotAreaTopLeft.Y, plotAreaTopLeft.X, plotAreaBottomRight.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(plotAreaTopLeft.X, plotAreaBottomRight.Y, plotAreaBottomRight.X, plotAreaBottomRight.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(plotAreaBottomRight.X, plotAreaBottomRight.Y, plotAreaBottomRight.X, plotAreaTopLeft.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(plotAreaBottomRight.X, plotAreaTopLeft.Y, plotAreaTopLeft.X, plotAreaTopLeft.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);

            layoutRoot.DrawLine(legendAreaTopLeft.X, legendAreaTopLeft.Y, legendAreaTopLeft.X, legendAreaBottomRight.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(legendAreaTopLeft.X, legendAreaBottomRight.Y, legendAreaBottomRight.X, legendAreaBottomRight.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(legendAreaBottomRight.X, legendAreaBottomRight.Y, legendAreaBottomRight.X, legendAreaTopLeft.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);
            layoutRoot.DrawLine(legendAreaBottomRight.X, legendAreaTopLeft.Y, legendAreaTopLeft.X, legendAreaTopLeft.Y, plotAreaStroke, plotAreaStrokeThickness * 1.25);

            double legendX = 0;
            foreach (Series series in Series)
            {
                var drawSize = layoutRoot.DrawLegendItem(series.LegendDescription, legendX + legendAreaTopLeft.X, legendAreaTopLeft.Y, FontSize, legendItemIndicatorWidth, Colors.AliceBlue);
                legendX += drawSize.Width;
            }
        }

        private void DrawSeries()
        {
            SolidColorBrush[] plotColors = new SolidColorBrush[]
            {
                new SolidColorBrush(Colors.PowderBlue),
                new SolidColorBrush(Colors.PaleVioletRed),
                new SolidColorBrush(Colors.LightSeaGreen),
            };
            double plotThickness = 4;

            double plotHeight = plotAreaBottomRight.Y - plotAreaTopLeft.Y;
            double? prevX = null;
            double? prevY = null;
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

                    double x = (verticalGridLineSpace * i) + plotAreaTopLeft.X;
                    double y = plotAreaBottomRight.Y - plotValue;

                    linePlotPoints.Add(new Tuple<Point, SeriesDataPoint>(new Point(x, y), dataPoint));
                }

                for (int i = 0; i < linePlotPoints.Count; i++)
                {   
                    if (!linePlotPoints[i].Item2.Value.HasValue)
                    {
                        prevX = null;
                        prevY = null;
                        continue;
                    }

                    if (prevX.HasValue && prevY.HasValue)
                    {
                        layoutRoot.DrawLine(prevX.Value, prevY.Value, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, plotColors[Series.IndexOf(series)], plotThickness);
                    }
                   
                    prevX = linePlotPoints[i].Item1.X;
                    prevY = linePlotPoints[i].Item1.Y;
                }

                for (int i = 0; i < linePlotPoints.Count; i++)
                {
                    if (!linePlotPoints[i].Item2.Value.HasValue)
                    {
                        continue;
                    }

                    if (!series.PointBulletSize.HasValue || series.PointBulletSize.Value > 0)
                    {
                        double pointSize = series.PointBulletSize ?? Math.Max(ActualWidth, ActualHeight) * 0.01;
                        Ellipse point = new Ellipse();
                        point.Width = pointSize;
                        point.Height = pointSize;
                        point.Fill = plotColors[Series.IndexOf(series)];

                        double marginRatioX = linePlotPoints[i].Item1.X / ActualWidth;
                        double marginRatioY = linePlotPoints[i].Item1.Y / ActualHeight;

                        point.Margin = new Thickness((ActualWidth * 2 * marginRatioX) - ActualWidth, (ActualHeight * 2 * marginRatioY) - ActualHeight, 0, 0);

                        layoutRoot.Children.Add(point);
                    }

                    layoutRoot.DrawScaleValueItem(linePlotPoints[i].Item2.ValueText, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, FontSize, UWPlot.YAxis.YAxisType.Primary);
                }

                prevX = null;
                prevY = null;
            }
        }
    }
}
