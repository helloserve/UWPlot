using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Security.Isolation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

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

        private Brush plotAreaStrokeBrush = new SolidColorBrush(Colors.Gray);
        public Brush PlotAreaStrokeBrush
        {
            get { return plotAreaStrokeBrush; }
            set
            {
                plotAreaStrokeBrush = value;
            }
        }

        private double plotAreaStrokeThickness = 2;
        public double PlotAreaStrokeThickness
        {
            get { return plotAreaStrokeThickness; }
            set
            {
                plotAreaStrokeThickness = value;
            }
        }

        protected Canvas LayoutRoot = null;

        internal DataExtents DataExtents = new DataExtents();

        protected Exception dataPrepException;
        protected string dataValidationErrorMessage;

        public Plot()
        {
            this.DefaultStyleKey = typeof(Plot);

            DataContextChanged += Plot_DataContextChanged;
            Loaded += Plot_Loaded;
            SizeChanged += Plot_SizeChanged;

            IsHitTestVisible = true;

            if (Background == null || Background is SolidColorBrush)
            {
                if (Background == null || (Background as SolidColorBrush).Color == Colors.Transparent)
                {
                    Background = (SolidColorBrush)Application.Current.Resources["SystemControlPageBackgroundChromeLowBrush"];
                }
            }
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

            hasDrawn = false;
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
            dataPrepException = null;
            dataValidationErrorMessage = null;

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

                if (!(DataExtents.IsPrepared || string.IsNullOrEmpty(dataValidationErrorMessage)))
                    return finalSize;

#if DEBUG
                Debug.WriteLine($"{Name} Arrange Override");
#endif

                if (!hasDrawn || LayoutRoot.Children.Count == 0)
                {
                    DrawSelf();
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

        private void DrawSelf()
        {
            if (LayoutRoot == null)
                return;

            if (hasDrawn)
                return;

            ClearLayout();

            if (!string.IsNullOrEmpty(dataValidationErrorMessage) || dataPrepException != null)
            {
                DrawPlotError();
            }
            else
            {
                try
                {
                    Draw();
                }
                catch (Exception ex)
                {
                    dataValidationErrorMessage = ex.Message;
                    DrawSelf();
                }
            }

            hasDrawn = true;
        }

        protected virtual void DrawPlotError()
        {
            LayoutRoot.Children.Add(new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = LayoutRoot.ActualWidth,
                Y2 = LayoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Maroon),
                StrokeThickness = 2
            });

            LayoutRoot.Children.Add(new Line()
            {
                X1 = LayoutRoot.ActualWidth,
                Y1 = 0,
                X2 = 0,
                Y2 = LayoutRoot.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Maroon),
                StrokeThickness = 2
            });

            LayoutRoot.Children.Add(new TextBlock()
            {
                Text = dataValidationErrorMessage ?? dataPrepException?.Message ?? "Data Error",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        internal abstract void PrepareData(DataExtents extents);
        protected abstract bool ValidateSeries();
        protected abstract Size MeasurePlot(Size availableSize);
        protected abstract void Draw();
    }
}
