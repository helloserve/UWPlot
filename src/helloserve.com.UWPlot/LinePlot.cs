using System;
using Windows.Foundation;
using Windows.UI.Xaml;
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
            }
        }

        internal override void DrawSeries(SeriesDrawDataPoints[] seriesDataPoints)
        {         
            base.DrawSeries(seriesDataPoints);

            double plotWidth = PlotExtents.PlotFrameBottomRight.X - PlotExtents.PlotFrameTopLeft.X;
            double plotHeight = PlotExtents.PlotFrameTopLeft.Y - PlotExtents.PlotFrameTopLeft.Y;

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var series = Series[s];
                var linePlotPoints = seriesDataPoints[s].SeriesDataPoints;

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
                        LayoutRoot.DrawLine(prevX.Value, prevY.Value, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, PlotColors[Series.IndexOf(series)].StrokeBrush, LineThickness);
                    }

                    prevX = linePlotPoints[i].Item1.X;
                    prevY = linePlotPoints[i].Item1.Y;
                }
            }

            for (int s = 0; s < seriesDataPoints.Length; s++)
            {
                var series = Series[s];
                var linePlotPoints = seriesDataPoints[s].SeriesDataPoints;

                for (int i = 0; i < linePlotPoints.Count; i++)
                {
                    if (!linePlotPoints[i].Item2.Value.HasValue)
                    {
                        continue;
                    }

                    if (!series.PointBulletSize.HasValue || series.PointBulletSize.Value > 0)
                    {
                        double pointSize = series.PointBulletSize ?? Math.Max(plotWidth, plotHeight) * 0.01;
                        Ellipse point = new Ellipse();
                        point.Width = pointSize;
                        point.Height = pointSize;
                        point.Fill = PlotColors[Series.IndexOf(series)].StrokeBrush;

                        point.Margin = new Thickness(linePlotPoints[i].Item1.X - (point.Width / 2), linePlotPoints[i].Item1.Y - (point.Height / 2), 0, 0);
                        point.DataContext = linePlotPoints[i].Item2;

                        LayoutRoot.Children.Add(point);
                    }

                    LayoutRoot.DrawPlotValueItem(linePlotPoints[i].Item2.ValueText, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, FontSize, new Rect(PlotExtents.PlotAreaTopLeft, PlotExtents.PlotAreaBottomRight), DataPointLocation.Below);
                }
            }
        }
    }
}
