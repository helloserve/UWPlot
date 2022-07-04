using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace helloserve.com.UWPlot
{
    public sealed class SeriesPointToolTip : Control
    {
        private DependencyObject layoutRoot = null;
        public SeriesPointToolTip()
        {
            this.DefaultStyleKey = typeof(SeriesPointToolTip);
            Loaded += SeriesPointToolTip_Loaded;
        }

        private void SeriesPointToolTip_Loaded(object sender, RoutedEventArgs e)
        {
            layoutRoot = VisualTreeHelper.GetChild(this, 0);
        }

        public Size GetContentSize(Size availableSize)
        {
            Size size = new Size(MinWidth, MinHeight);

            if (layoutRoot == null)
            {
                return size;
            }

            if (layoutRoot is Border)
            {
                Border border = layoutRoot as Border;
                border.Measure(availableSize);
                size = border.DesiredSize;
            }

            if (layoutRoot is Panel)
            {
                Panel panel = layoutRoot as Panel;
                panel.Measure(availableSize);
                size = panel.DesiredSize;
            }            

            return size;
        }

        public void SetDebugText(string value)
        {
            if (layoutRoot == null)
            {
                return;
            }

            var debugBlock = (TextBlock)VisualTreeHelper.GetChild(layoutRoot, 0);
            if (debugBlock == null)
            {
                return;
            }

            debugBlock.Text = value;
        }

    }
}
