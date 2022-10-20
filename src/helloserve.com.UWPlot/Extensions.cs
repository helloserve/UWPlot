using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace helloserve.com.UWPlot
{
    public static class Extensions
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
                switch(offset) 
                {
                    case DataPointLocation.Above: plotY -= size.Height;break;
                    case DataPointLocation.Below: plotY += size.Height * 0.1D;break;
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
            Size descriptionSize = layoutRoot.DrawString(value, fontSize, size => new Thickness(x, y, 0, 0), paddingFactor, transform);

            var points = new PointCollection();
            points.Add(new Point(x + descriptionSize.Width + 10, y));
            points.Add(new Point(x + descriptionSize.Width + 10, y + descriptionSize.Height));
            points.Add(new Point(x + descriptionSize.Width + 10 + indicatorWidth, y + descriptionSize.Height));
            points.Add(new Point(x + descriptionSize.Width + 10 + indicatorWidth, y));

            Polygon indicator = new Polygon();
            indicator.Fill = color;
            indicator.Points = points;

            layoutRoot.Children.Add(indicator);

            return new Size(descriptionSize.Width + indicatorWidth + 40, descriptionSize.Height);
        }

        private static Size DrawString(this Canvas layoutRoot, string value, double fontSize, Func<Size, Thickness> positionFunc, double paddingFactor, Transform transform, double? limitedToWidth = null, double? limitedToHeight = null)
        {
            var textBlock = new TextBlock();
            textBlock.Text = value;
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

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size desiredSize = textBlock.DesiredSize.WithFactor(paddingFactor);
            textBlock.Margin = positionFunc(desiredSize);

            layoutRoot.Children.Add(textBlock);

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
