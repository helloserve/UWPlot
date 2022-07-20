using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace helloserve.com.UWPlot
{
    public sealed class BarPlot : Plot
    {
        private double barThickness;
        public double BarThickness
        {
            get { return barThickness; }
            set
            {
                barThickness = value;
                Invalidate();
            }
        }

        protected override void OnMeasurePlotFrame()
        {
            base.OnMeasurePlotFrame();
            double xFactor = (PlotExtents.PlotFrameBottomRight.X - PlotExtents.PlotFrameTopLeft.X) * 0.05;
            PlotExtents.PlotAreaPadding = new Thickness(xFactor, 0, xFactor, 0);
        }

        protected override void DrawPlotArea()
        {
            base.DrawPlotArea();

            LayoutRoot.DrawLine(PlotExtents.PlotAreaTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotAreaTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, GridLineStrokeThickness);
            LayoutRoot.DrawLine(PlotExtents.PlotAreaBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotAreaBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, GridLineStrokeThickness);
        }

        internal override void DrawSeries(List<Tuple<Point, SeriesDataPoint>>[] seriesDataPoints)
        {
            base.DrawSeries(seriesDataPoints);

            double thickness = barThickness;
            if (thickness == 0)
            {
                thickness = (PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X) * 0.025D;
            }
            double barXDifference = thickness / 2D;
            double barXOffset = seriesDataPoints.Length * thickness / 2D;

            for (int i = 0; i < seriesDataPoints.Length; i++)
            {
                SolidColorBrush fillColor = PlotColors[i];
                double seriesXOffset = - barXOffset + (i * thickness) + (thickness / 2);

                foreach (var point in seriesDataPoints[i])
                {
                    var points = new PointCollection();
                    points.Add(new Point(point.Item1.X + seriesXOffset - barXDifference, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset + barXDifference, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset + barXDifference, PlotExtents.PlotAreaBottomRight.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset - barXDifference, PlotExtents.PlotAreaBottomRight.Y));

                    Polygon indicator = new Polygon();
                    indicator.Fill = fillColor;
                    indicator.Points = points;

                    LayoutRoot.Children.Add(indicator);
                }
            }

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var series = Series[s];
                var linePlotPoints = seriesDataPoints[s];
                double seriesXOffset = -barXOffset + (s * thickness) + (thickness / 2);

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
                        point.Fill = PlotColors[s];

                        double marginRatioX = (linePlotPoints[i].Item1.X + seriesXOffset) / ActualWidth;
                        double marginRatioY = linePlotPoints[i].Item1.Y / ActualHeight;

                        point.Margin = new Thickness((ActualWidth * 2 * marginRatioX) - ActualWidth, (ActualHeight * 2 * marginRatioY) - ActualHeight, 0, 0);
                        point.DataContext = linePlotPoints[i].Item2;

                        LayoutRoot.Children.Add(point);
                    }

                    LayoutRoot.DrawPlotValueItem(linePlotPoints[i].Item2.ValueText, linePlotPoints[i].Item1.X + seriesXOffset, linePlotPoints[i].Item1.Y, FontSize, new Rect(PlotExtents.PlotFrameTopLeft, PlotExtents.PlotFrameBottomRight), DataPointLocation.Above);
                }
            }
        }
    }
}
