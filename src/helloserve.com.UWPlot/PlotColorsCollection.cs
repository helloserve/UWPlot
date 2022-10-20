using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public class PlotColorsCollection
    {
        private List<PlotColorItem> items;
        public List<PlotColorItem> Items
        {
            get { return items; }
            internal set { items = value; }
        }

        public PlotColorItem this[int index] => items[index];
    }

    public class PlotColorItem
    {
        private Brush strokeBrush;
        public Brush StrokeBrush
        {
            get { return strokeBrush; }
            internal set { strokeBrush = value; }
        }

        private Brush fillBrush;
        public Brush FillBrush
        {
            get { return fillBrush; }
            set { fillBrush = value; }
        }
    }
}
