using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    [Bindable]
    [ContentProperty(Name = "Items")]
    public class PlotColorsCollection : DependencyObject
    {
        private List<PlotColorItem> items = new List<PlotColorItem>();
        public List<PlotColorItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        public PlotColorItem this[int index] => items[index];

        public PlotColorsCollection()
        {

        }
    }

    [Bindable]
    public class PlotColorItem
    {
        public PlotColorItem() 
        {
        }

        private Brush strokeBrush;
        public Brush StrokeBrush
        {
            get { return strokeBrush; }
            set { strokeBrush = value; }
        }

        private Brush fillBrush;
        public Brush FillBrush
        {
            get { return fillBrush; }
            set { fillBrush = value; }
        }
    }
}
