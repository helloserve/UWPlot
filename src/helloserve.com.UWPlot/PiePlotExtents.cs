using Windows.Foundation;

namespace helloserve.com.UWPlot
{
    public class PiePlotExtents : PlotExtents
    {
        public Point Origin { get; set; }
        public double Radius { get; set; }
        public PiePlotOrientation Orientation { get; set; }
        public int LegendColumns { get; set; } = 1;
        public double LegendColumnWidth { get; set; }
        public double LegendItemHeight { get; set; }
    }

    public enum PiePlotOrientation
    {
        Portrait,
        Landscape,
    }
}
