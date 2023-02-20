using Windows.Foundation;
using Windows.UI.Xaml;

namespace helloserve.com.UWPlot
{
    public abstract class PlotExtents
    {
        public Point LegendAreaTopLeft { get; set; }
        public Point LegendAreaBottomRight { get; set; }
        public double LegendItemIndicatorWidth { get; set; }

        public Point PlotFrameTopLeft { get; set; }
        public Point PlotFrameBottomRight { get; set; }
        public Point PlotAreaTopLeft { get; set; }
        public Point PlotAreaBottomRight { get; set; }
        public Thickness PlotAreaPadding { get; internal set; }

        public double AreaWidth => PlotAreaBottomRight.X - PlotAreaTopLeft.X;
        public double FrameWidth => PlotFrameBottomRight.X - PlotFrameTopLeft.X;

    }
}
