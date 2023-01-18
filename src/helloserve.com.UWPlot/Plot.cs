using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public abstract class Plot : Control
    {
        public static readonly DependencyProperty PlotColorsProperty = DependencyProperty.Register("PlotColors", typeof(PlotColorsCollection), typeof(Plot), new PropertyMetadata(null, OnPlotColorsChanged));
        private static void OnPlotColorsChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            Plot plot = target as Plot;
            plot.plotColors = args.NewValue as PlotColorsCollection;
        }

        private PlotColorsCollection plotColors = new PlotColorsCollection()
        {
            Items = new List<PlotColorItem>()
            {
                new PlotColorItem()
                {
                    StrokeBrush = new SolidColorBrush(Colors.PowderBlue),
                    FillBrush = new SolidColorBrush(Colors.PowderBlue)
                },
                new PlotColorItem()
                {
                    StrokeBrush = new SolidColorBrush(Colors.PaleVioletRed),
                    FillBrush = new SolidColorBrush(Colors.PaleVioletRed)
                },
                new PlotColorItem()
                {
                    StrokeBrush = new SolidColorBrush(Colors.LightSeaGreen),
                    FillBrush = new SolidColorBrush(Colors.LightSeaGreen)
                }
            }
        };

        public PlotColorsCollection PlotColors
        {
            get => plotColors;
            set => SetValue(PlotColorsProperty, value);
        }

        protected Canvas LayoutRoot = null;

        internal DataExtents DataExtents = new DataExtents();

        public Plot()
        {
            this.DefaultStyleKey = typeof(Plot);

            DataContextChanged += Plot_DataContextChanged;
            Loaded += Plot_Loaded;
            SizeChanged += Plot_SizeChanged;

            IsHitTestVisible = true;
        }

        private bool hasDrawn = false;

        private void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"{Name} Plot Size Changed");
#endif
            hasDrawn = false;
            InvalidateMeasure();
        }

        private void Plot_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"{Name} Loaded");
#endif            
            LayoutRoot = (Canvas)VisualTreeHelper.GetChild(this, 0);

            HandlePlotLoaded(sender, e);

            hasDrawn = true;
            InvalidateMeasure();
        }

        protected virtual void HandlePlotLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void Plot_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            INotifyPropertyChanged contextNotify = DataContext as INotifyPropertyChanged;
            if (contextNotify != null)
            {
                contextNotify.PropertyChanged -= ContextNotify_PropertyChanged;
            }

            contextNotify = args.NewValue as INotifyPropertyChanged;
            if (contextNotify != null)
            {
                contextNotify.PropertyChanged += ContextNotify_PropertyChanged;
            }

            ContextNotify_PropertyChanged(this, new PropertyChangedEventArgs(nameof(DataContext)));
        }

        private void ContextNotify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine($"{Name} Data Changed");
#endif
            DataExtents.IsPrepared = false;

            hasDrawn = false;
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var sw = Stopwatch.StartNew();

            try
            {

                if (LayoutRoot == null)
                    return availableSize;

#if DEBUG
                Debug.WriteLine($"{Name} Measure Override");
#endif

                if (!DataExtents.IsPrepared)
                {
                    PrepareData(DataExtents);
                }

                Size measuredSize = MeasurePlot(availableSize);

                return measuredSize;
            }
            catch
            {
#if DEBUG
                Debug.WriteLine($"{Name} Error measuring");
#endif
                throw;
            }
            finally
            {
#if DEBUG
                Debug.WriteLine($"{Name} Measure took {sw.ElapsedMilliseconds}ms");
#endif
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                if (LayoutRoot == null)
                    return finalSize;

                if (!DataExtents.IsPrepared)
                    return finalSize;

#if DEBUG
                Debug.WriteLine($"{Name} Arrange Override");
#endif

                if (!hasDrawn || LayoutRoot.Children.Count == 0)
                {
                    Draw();
                }
                else
                {
#if DEBUG
                    Debug.WriteLine($"{Name} Not drawing, already busy");
#endif
                }

#if DEBUG
                Debug.WriteLine($"{Name} Layout Children Count: {LayoutRoot.Children.Count}");
#endif

                return base.ArrangeOverride(finalSize);
            }
            finally
            {
#if DEBUG
                Debug.WriteLine($"{Name} Arrange took {sw.ElapsedMilliseconds}ms");
#endif
            }
        }

        protected virtual void ClearLayout()
        {
#if DEBUG
            Debug.WriteLine($"{Name} Clear Layout");
#endif            
            LayoutRoot.Children.Clear();
        }

        internal abstract void PrepareData(DataExtents extents);
        protected abstract bool ValidateSeries();
        protected abstract Size MeasurePlot(Size size);
        protected virtual void Draw()
        {
            hasDrawn = true;
        }
    }
}
