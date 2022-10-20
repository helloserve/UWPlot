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

        internal override void DrawSeries(SeriesDrawDataPoints[] seriesDataPoints)
        {
            base.DrawSeries(seriesDataPoints);

            double actualWidth = ActualWidth;
            double actualHeight = ActualHeight;

            double thickness = barThickness;
            if (thickness == 0)
            {
                thickness = (PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X) * 0.025D;
            }
            double barXDifference = thickness / 2D;
            double barXOffset = seriesDataPoints.Length * thickness / 2D;

            for (int i = 0; i < seriesDataPoints.Length; i++)
            {
                Brush fillColor = PlotColors[i].FillBrush;
                Brush strokeColor = PlotColors[i].StrokeBrush;
                double seriesXOffset = - barXOffset + (i * thickness) + (thickness / 2);

                foreach (var point in seriesDataPoints[i].SeriesDataPoints)
                {
                    var points = new PointCollection();
                    points.Add(new Point(point.Item1.X + seriesXOffset - barXDifference, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset + barXDifference, point.Item1.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset + barXDifference, seriesDataPoints[i].ZeroLine.Y));
                    points.Add(new Point(point.Item1.X + seriesXOffset - barXDifference, seriesDataPoints[i].ZeroLine.Y));

                    Polygon indicator = new Polygon();
                    indicator.Fill = fillColor;
                    indicator.Stroke = strokeColor;
                    indicator.Points = points;

                    LayoutRoot.Children.Add(indicator);
                }
            }

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var linePlotPoints = seriesDataPoints[s].SeriesDataPoints;                
                double seriesXOffset = -barXOffset + (s * thickness) + (thickness / 2);

                for (int i = 0; i < linePlotPoints.Count; i++)
                {
                    if (!linePlotPoints[i].Item2.Value.HasValue)
                    {
                        continue;
                    }

                    LayoutRoot.DrawPlotValueItem(
                        linePlotPoints[i].Item2.ValueText, 
                        linePlotPoints[i].Item1.X + seriesXOffset, 
                        linePlotPoints[i].Item1.Y, 
                        FontSize, 
                        new Rect(PlotExtents.PlotFrameTopLeft, PlotExtents.PlotFrameBottomRight),
                        linePlotPoints[i].Item2.Value < 0 ? DataPointLocation.Below : DataPointLocation.Above);
                }
            }
        }
    }
}
