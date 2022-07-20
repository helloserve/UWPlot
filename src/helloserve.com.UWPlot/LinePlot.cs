using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace helloserve.com.UWPlot
{
    public sealed class LinePlot : Plot
    {
        private double lineThickness = 4D;
        public double LineThickness
        {
            get { return lineThickness; }
            set
            {
                lineThickness = value;
                Invalidate();
            }
        }

        internal override void DrawSeries(List<Tuple<Point, SeriesDataPoint>>[] seriesDataPoints)
        {
            base.DrawSeries(seriesDataPoints);

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var series = Series[s];
                var linePlotPoints = seriesDataPoints[s];

                double? prevX = null;
                double? prevY = null;

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
                        LayoutRoot.DrawLine(prevX.Value, prevY.Value, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, PlotColors[Series.IndexOf(series)], LineThickness);
                    }

                    prevX = linePlotPoints[i].Item1.X;
                    prevY = linePlotPoints[i].Item1.Y;
                }
            }

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var series = Series[s];
                var linePlotPoints = seriesDataPoints[s];

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
                        point.Fill = PlotColors[Series.IndexOf(series)];

                        double marginRatioX = linePlotPoints[i].Item1.X / ActualWidth;
                        double marginRatioY = linePlotPoints[i].Item1.Y / ActualHeight;

                        point.Margin = new Thickness((ActualWidth * 2 * marginRatioX) - ActualWidth, (ActualHeight * 2 * marginRatioY) - ActualHeight, 0, 0);
                        point.DataContext = linePlotPoints[i].Item2;

                        LayoutRoot.Children.Add(point);
                    }

                    LayoutRoot.DrawPlotValueItem(linePlotPoints[i].Item2.ValueText, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, FontSize, new Rect(PlotExtents.PlotAreaTopLeft, PlotExtents.PlotAreaBottomRight), DataPointLocation.Below);
                }
            }
        }
    }
}
