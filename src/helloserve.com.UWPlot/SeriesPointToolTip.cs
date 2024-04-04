using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace helloserve.com.UWPlot
{
    public sealed class SeriesPointToolTip : Control
    {
        private DependencyObject _layoutRoot = null;
        public DependencyObject LayoutRoot
        {
            get
            {
                if (_layoutRoot == null)
                {
                    _layoutRoot = VisualTreeHelper.GetChild(this, 0);
                }
                return _layoutRoot;
            }
        }

        public SeriesPointToolTip()
        {
            this.DefaultStyleKey = typeof(SeriesPointToolTip);
            Loaded += SeriesPointToolTip_Loaded;
        }

        private void SeriesPointToolTip_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public Size GetContentSize(Size availableSize)
        {
            Size size = new Size(MinWidth, MinHeight);

            if (LayoutRoot == null)
            {
                return size;
            }

            if (LayoutRoot is Border)
            {
                Border border = LayoutRoot as Border;
                border.Measure(availableSize);
                size = border.DesiredSize;
            }

            if (LayoutRoot is Panel)
            {
                Panel panel = LayoutRoot as Panel;
                panel.Measure(availableSize);
                size = panel.DesiredSize;
            }            

            return size;
        }

        public void SetDebugText(string value)
        {
            if (LayoutRoot == null)
            {
                return;
            }

            var debugBlock = (TextBlock)VisualTreeHelper.GetChild(LayoutRoot, 0);
            if (debugBlock == null)
            {
                return;
            }

            debugBlock.Text = value;
        }

    }
}
