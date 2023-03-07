using System;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace helloserve.com.UWPlot
{
    public static class PlotExtensions
    {
        public static string FormatObject(this object obj, string format)
        {
            if (obj is null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(format))
            {
                return obj.ToString();
            }

            return string.Format(format, obj);
        }

        /// <summary>
        /// Measures the size of a string using a textblock.
        /// There is an alternative implementation at https://stackoverflow.com/questions/35969056/how-can-i-measure-the-text-size-in-uwp-apps which is supposedly faster, but fails because it's probably been deprecated or something.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="fontSize">The font size to use.</param>
        /// <param name="limitedToWidth">Optional restriction in width.</param>
        /// <param name="limitedToHeight">Optional restriction in height.</param>
        /// <returns>A desired <see cref="Size"/> struct property.</returns>
        public static Size MeasureTextSize(this string text, double fontSize, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = fontSize;
            textBlock.RenderTransform = transform ?? new TranslateTransform();
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            if (limitedToWidth.HasValue)
            {
                availableSize.Width = limitedToWidth.Value;
            }
            if (limitedToHeight.HasValue)
            {
                availableSize.Height = limitedToHeight.Value;
            }

            textBlock.Measure(availableSize);

            return textBlock.DesiredSize;
        }

        public static void DrawLine(this Canvas layoutRoot, double x1, double y1, double x2, double y2, Brush strokeColor, double strokeThickness)
        {
            var line = new Line();
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            line.Stroke = strokeColor;
            line.StrokeThickness = strokeThickness;

            layoutRoot.Children.Add(line);
        }

        const double radianSegmentStep = (Math.PI * 2) / 128;
        public static Point DrawSlice(this Canvas layoutRoot, double ox, double oy, double radius, double startAngle, double sweepAngle, Brush strokeColor, double strokeTickness, Brush fillColor)
        {
            var points = new PointCollection();

            double avgX = 0;
            double avgY = 0;
            int avgCount = 2;

            var startX = radius * Math.Cos(startAngle);
            var startY = radius * Math.Sin(startAngle);
            avgX += startX;
            avgY += startY;

            points.Add(new Point(ox, oy));
            points.Add(new Point(startX + ox, startY + oy));
            
            double angle = radianSegmentStep;

            while (angle < sweepAngle) 
            {            
                var x = startX * Math.Cos(angle) - startY * Math.Sin(angle);
                var y = startX * Math.Sin(angle) + startY * Math.Cos(angle);

                points.Add(new Point(x + ox, y + oy));

                avgX += x;
                avgY += y;
                avgCount++;

                angle += radianSegmentStep;
            }

            var endX = startX * Math.Cos(sweepAngle) - startY * Math.Sin(sweepAngle);
            var endY = startX * Math.Sin(sweepAngle) + startY * Math.Cos(sweepAngle);
            avgX += endX;
            avgY += endY;

            points.Add(new Point(endX + ox, endY + oy));
            points.Add(new Point(ox, oy));

            Polygon slice = new Polygon();
            slice.Fill = fillColor;
            slice.Stroke = strokeColor;
            slice.StrokeThickness = strokeTickness;
            slice.Points = points;

            layoutRoot.Children.Add(slice);

            return new Point(avgX / avgCount, avgY / avgCount);
        }

        public static void DrawArea(this Canvas layoutRoot, PointCollection points, Brush strokeColor, double strokeTickness, Brush fillColor)
        {
            Polygon area = new Polygon();
            area.Fill = fillColor;
            area.Stroke = null;
            area.StrokeThickness = 0;
            area.Points = points;

            layoutRoot.Children.Add(area);

            double pX = points[0].X;
            double pY = points[0].Y;
            for (int i = 1; i < points.Count - 3; i++)
            {
                layoutRoot.DrawLine(pX, pY, points[i].X, points[i].Y, strokeColor, strokeTickness);
                pX = points[i].X;
                pY = points[i].Y;
            }
        }

        public static Size DrawSliceLabel(this Canvas layoutRoot, string value, double x, double y, double fontSize, Brush backgroundBrush, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            return layoutRoot.DrawString(value, fontSize, s =>
            {
                return new Thickness(x - (s.Width / 2), y - (s.Height / 2), 0, 0);
            }, backgroundBrush, paddingFactor, transform, limitedToWidth, limitedToHeight);
        }

        public static double ToRadians(this double angle)
        {
            return angle / (180 / Math.PI);
        }

        public static void DrawCategoryItem(this Canvas layoutRoot, string category, double x, double y, double fontSize, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            layoutRoot.DrawString(category, fontSize, size => new Thickness(x - (size.Width / 2), y + size.FactorDifference(paddingFactor).Height, 0, 0), paddingFactor, transform);
        }

        /// <summary>
        /// A method that draws a Y-Axis value, placed correctly given the Y-Axis coordinate and type.
        /// </summary>
        /// <param name="layoutRoot"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fontSize"></param>
        /// <param name="limitedToWidth"></param>
        /// <param name="limitedToHeight"></param>
        public static void DrawScaleValueItem(this Canvas layoutRoot, string value, double x, double y, double fontSize, YAxis.YAxisType type, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            layoutRoot.DrawString(value, fontSize, size => new Thickness(type == YAxis.YAxisType.Primary ? x - size.Width * paddingFactor : x + size.FactorDifference(paddingFactor).Width, y - (size.Height / 2), 0, 0), paddingFactor, transform);
        }

        /// <summary>
        /// A method that draws a Y-Axis value, placed correctly given the plot point coordinate and type.
        /// </summary>
        /// <param name="layoutRoot"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fontSize"></param>
        /// <param name="limitedToWidth"></param>
        /// <param name="limitedToHeight"></param>
        public static void DrawPlotValueItem(this Canvas layoutRoot, string value, double x, double y, double fontSize, Rect plotArea, DataPointLocation offset = DataPointLocation.Below, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            layoutRoot.DrawString(value, fontSize, size =>
            {
                double offsetPadding = 0.2D;
                var plotX = x - size.Width / 2 * paddingFactor;
                var plotY = y;
                switch (offset)
                {
                    case DataPointLocation.Above: plotY -= size.Height; break;
                    case DataPointLocation.Below: plotY += size.Height * 0.1D; break;
                    default: break;
                }
                if (plotX < plotArea.X)
                {
                    plotX = plotArea.X + size.Width * offsetPadding;
                }
                if (plotX + size.Width > plotArea.X + plotArea.Width)
                {
                    plotX = plotArea.X + plotArea.Width - size.Width - size.Width * offsetPadding;
                }
                if (plotY < plotArea.Y)
                {
                    plotY = plotArea.Y + size.Height * offsetPadding;
                }
                if (plotY + size.Height > plotArea.Y + plotArea.Height)
                {
                    plotY = plotArea.Y + plotArea.Height - size.Height - size.Height * offsetPadding;
                }
                return new Thickness(plotX, plotY, 0, 0);
            }, paddingFactor, transform);
        }

        /// <summary>
        /// A method that draws the legend description of a series.
        /// </summary>
        /// <param name="layoutRoot"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fontSize"></param>
        /// <param name="color"></param>
        /// <param name="limitedToWidth"></param>
        /// <param name="limitedToHeight"></param>
        /// <returns>A <see cref="Windows.Foundation.Size"/> object of the textblock that was created, so that subsequent legend items can be offset correctly.</returns>
        public static Size DrawLegendItem(this Canvas layoutRoot, string value, double x, double y, double fontSize, double indicatorWidth, Brush color, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            Size descriptionSize = layoutRoot.DrawString(value, fontSize, size => new Thickness(x + 10 + indicatorWidth, y, 0, 0), paddingFactor, transform);

            var points = new PointCollection
            {
                new Point(x, y),
                new Point(x, y + descriptionSize.Height),
                new Point(x + indicatorWidth, y + descriptionSize.Height),
                new Point(x + indicatorWidth, y)
            };

            Polygon indicator = new Polygon();
            indicator.Fill = color;
            indicator.Points = points;

            layoutRoot.Children.Add(indicator);

            return new Size(descriptionSize.Width + indicatorWidth + 40, descriptionSize.Height);
        }

        public static Size DrawLegendValue(this Canvas layoutRoot, string value, double x, double y, double fontSize, TextAlignment textAlignment, double paddingFactor = 1, Transform transform = null, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            return layoutRoot.DrawString(value, fontSize, (s) =>
            {
                if (textAlignment == TextAlignment.Center)
                {
                    return new Thickness(x - (s.Width / 2), y, 0, 0);
                }
                else if (textAlignment == TextAlignment.Right)
                {
                    return new Thickness(x - s.Width, y, 0, 0);
                }
                else
                {
                    return new Thickness(x, y, 0, 0);
                }
            }, paddingFactor, transform, limitedToWidth, limitedToHeight);
        }

        private static Size DrawString(this Canvas layoutRoot, string value, double fontSize, Func<Size, Thickness> positionFunc, double paddingFactor, Transform transform, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            var textBlock = new TextBlock();
            textBlock.Text = value;
            textBlock.FontSize = fontSize;
            if (transform != null)
                textBlock.RenderTransform = transform;

            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            if (limitedToWidth.HasValue)
            {
                availableSize.Width = limitedToWidth.Value;
            }
            if (limitedToHeight.HasValue)
            {
                availableSize.Height = limitedToHeight.Value;
            }

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size desiredSize = textBlock.DesiredSize.WithFactor(paddingFactor);
            textBlock.Margin = positionFunc(desiredSize);

            layoutRoot.Children.Add(textBlock);

            return desiredSize;
        }

        private static Size DrawString(this Canvas layoutRoot, string value, double fontSize, Func<Size, Thickness> positionFunc, Brush backgroundBrush, double paddingFactor, Transform transform, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            var textBlock = new TextBlock();
            textBlock.Text = value;
            textBlock.FontSize = fontSize;
            if (transform != null)
                textBlock.RenderTransform = transform;

            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            if (limitedToWidth.HasValue)
            {
                availableSize.Width = limitedToWidth.Value;
            }
            if (limitedToHeight.HasValue)
            {
                availableSize.Height = limitedToHeight.Value;
            }

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size desiredSize = textBlock.DesiredSize.WithFactor(paddingFactor);
            Thickness margin = positionFunc(desiredSize);

            var canvas = new Grid();
            canvas.Background = backgroundBrush;
            canvas.Margin = new Thickness(
                margin.Left,
                margin.Top,
                margin.Right,
                margin.Bottom);
            canvas.Padding = new Thickness(2, 2, 2, 2);
            canvas.Children.Add(textBlock);

            //layoutRoot.Children.Add(textBlock);
            layoutRoot.Children.Add(canvas);

            return desiredSize;
        }

        public static Size WithFactor(this Size size, double factor)
        {
            return new Size(size.Width * factor, size.Height * factor);
        }

        public static Size FactorDifference(this Size size, double factor)
        {
            var factored = size.WithFactor(factor);
            return new Size(factored.Width - size.Width, factored.Height - size.Height);
        }
    }
}
