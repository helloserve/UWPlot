using Windows.Foundation;
using Windows.UI.Xaml;

namespace helloserve.com.UWPlot
{
    public class CartesianPlotExtents
    {
        public int NumberOfScaleLines { get; set; }
        public double ScaleLineIncrements { get; set; }
        public int VerticalGridLineCount { get; set; }

        public Point LegendAreaTopLeft { get; set; }
        public Point LegendAreaBottomRight { get; set; }
        public double LegendItemIndicatorWidth { get; set; }

        public Point PlotFrameTopLeft { get; set; }
        public Point PlotFrameBottomRight { get; set; }
        public Point PlotAreaTopLeft { get; set; }
        public Point PlotAreaBottomRight { get; set; }
        public Thickness PlotAreaPadding { get; internal set; }

        public double VerticalGridLineSpace { get; set; }
    }
}
