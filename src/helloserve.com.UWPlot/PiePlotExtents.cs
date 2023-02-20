using Windows.Foundation;

namespace helloserve.com.UWPlot
{
    public class PiePlotExtents : PlotExtents
    {
        public Point Origin { get; set; }
        public double Radius { get; set; }

        public PiePlotOrientation Orientation { get; set; }
    }

    public enum PiePlotOrientation
    {
        Portrait,
        Landscape,
    }
}
