using System;
using System.Collections.Generic;
using Windows.Foundation;
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

        internal override void DrawSeries(List<Tuple<Point, SeriesDataPoint>>[] seriesDataPoints)
        {
            base.DrawSeries(seriesDataPoints);

            double thickness = barThickness;
            if (thickness == 0)
            {
                thickness = (PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X) * 0.025D;
            }
            double barXDifference = thickness / 2D;
            double barXOffset = -(seriesDataPoints.Length * thickness) / 2D;

            for (int i = 0; i < seriesDataPoints.Length; i++)
            {
                SolidColorBrush fillColor = PlotColors[i];
                double seriesXOffset = barXOffset + (i + 1) * thickness;

                foreach (var point in seriesDataPoints[i])
                {
                    var points = new PointCollection();
                    points.Add(new Point(point.Item1.X - barXDifference + seriesXOffset, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + barXDifference + seriesXOffset, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + barXDifference + seriesXOffset, PlotExtents.PlotAreaBottomRight.Y));
                    points.Add(new Point(point.Item1.X - barXDifference + seriesXOffset, PlotExtents.PlotAreaBottomRight.Y));

                    Polygon indicator = new Polygon();
                    indicator.Fill = fillColor;
                    indicator.Points = points;

                    LayoutRoot.Children.Add(indicator);
                }
            }
        }
    }
}
