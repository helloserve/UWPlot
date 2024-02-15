using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace helloserve.com.UWPlot
{
    public sealed class AreaPlot : CartesianPlot
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

                Point? firstPoint = null;
                Point lastPoint;
                var pointsArea = new PointCollection();
                PlotColorItem seriesColor;

                for (int i = 0; i < linePlotPoints.Count; i++)
                {
                    if (!linePlotPoints[i].Item2.Value.HasValue)
                    {
                        if (pointsArea.Count > 0)
                        {
                            //close the area and start over
                            pointsArea.Add(new Point(lastPoint.X, PlotExtents.PlotAreaBottomRight.Y));
                            pointsArea.Add(new Point(firstPoint.Value.X, PlotExtents.PlotAreaBottomRight.Y));
                            pointsArea.Add(firstPoint.Value);

                            seriesColor = GetSeriesColor(Series.IndexOf(series));

                            LayoutRoot.DrawArea(pointsArea, seriesColor.StrokeBrush, LineThickness, seriesColor.FillBrush);

                            pointsArea = new PointCollection();
                        }

                        continue;
                    }

                    lastPoint = new Point(linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y);
                    
                    if (firstPoint == null)
                        firstPoint = lastPoint;
                    
                    pointsArea.Add(lastPoint);
                }

                pointsArea.Add(new Point(lastPoint.X, PlotExtents.PlotAreaBottomRight.Y));
                pointsArea.Add(new Point(firstPoint.Value.X, PlotExtents.PlotAreaBottomRight.Y));
                pointsArea.Add(firstPoint.Value);

                seriesColor = GetSeriesColor(Series.IndexOf(series));
                LayoutRoot.DrawArea(pointsArea, seriesColor.StrokeBrush, LineThickness, seriesColor.FillBrush);
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
                        point.Fill = GetSeriesColor(Series.IndexOf(series)).StrokeBrush;

                        point.Margin = new Thickness(linePlotPoints[i].Item1.X - (point.Width / 2), linePlotPoints[i].Item1.Y - (point.Height / 2), 0, 0);
                        point.DataContext = linePlotPoints[i].Item2;

                        LayoutRoot.Children.Add(point);
                    }

                    if (series.ShowDataPointValues)
                    {
                        LayoutRoot.DrawPlotValueItem(linePlotPoints[i].Item2.ValueText, linePlotPoints[i].Item1.X, linePlotPoints[i].Item1.Y, FontSize, new Rect(PlotExtents.PlotAreaTopLeft, PlotExtents.PlotAreaBottomRight), DataPointLocation.Below);
                    }
                }
            }
        }
    }
}
