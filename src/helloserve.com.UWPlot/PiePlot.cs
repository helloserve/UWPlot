using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace helloserve.com.UWPlot
{
    public class PiePlot : Plot
    {

        /// <summary>
        /// Set this property to have to pie chart use this collection of colors when there are 10 or less items in the plot.
        /// </summary>
        public PlotColorsCollection PlotColors10
        {
            get { return (PlotColorsCollection)GetValue(PlotColors10Property); }
            set { SetValue(PlotColors10Property, value); }
        }

        // Using a DependencyProperty as the backing store for PlotColors10.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotColors10Property =
            DependencyProperty.Register("PlotColors10", typeof(PlotColorsCollection), typeof(PiePlot), new PropertyMetadata(null));


        /// <summary>
        /// Set this property to have the pie chart use this collection of colors when there are 15 or less items in the plot.
        /// </summary>
        public PlotColorsCollection PlotColors15
        {
            get { return (PlotColorsCollection)GetValue(PlotColors15Property); }
            set { SetValue(PlotColors15Property, value); }
        }

        // Using a DependencyProperty as the backing store for PlotColors15.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotColors15Property =
            DependencyProperty.Register("PlotColors15", typeof(PlotColorsCollection), typeof(PiePlot), new PropertyMetadata(null));


        /// <summary>
        /// Set this property to have the pie chart use this collection of colors when there are 20 or less items in the plot.
        /// </summary>
        public PlotColorsCollection PlotColors20
        {
            get { return (PlotColorsCollection)GetValue(PlotColors20Property); }
            set { SetValue(PlotColors20Property, value); }
        }

        // Using a DependencyProperty as the backing store for PlotColors20.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotColors20Property =
            DependencyProperty.Register("PlotColors20", typeof(PlotColorsCollection), typeof(PiePlot), new PropertyMetadata(null));


        /// <summary>
        /// Set this property to have the pie chart use this collection of colors when there are 25 or less items in the plot.
        /// </summary>
        public PlotColorsCollection PlotColors25
        {
            get { return (PlotColorsCollection)GetValue(PlotColors25Property); }
            set { SetValue(PlotColors25Property, value); }
        }

        // Using a DependencyProperty as the backing store for PlotColors25.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotColors25Property =
            DependencyProperty.Register("PlotColors25", typeof(PlotColorsCollection), typeof(PiePlot), new PropertyMetadata(null));




        public Brush EmptyStrokeBrush
        {
            get { return (Brush)GetValue(EmptyStrokeBrushProperty); }
            set { SetValue(EmptyStrokeBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EmptyStrokeBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmptyStrokeBrushProperty =
            DependencyProperty.Register("EmptyStrokeBrush", typeof(Brush), typeof(PiePlot), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));



        private Brush plotLegendStrokeBrush = new SolidColorBrush(Colors.Pink);
        public Brush PlotLegendStrokeBrush
        {
            get { return plotLegendStrokeBrush; }
            set
            {
                plotLegendStrokeBrush = value;
            }
        }

        public PieSeries Series { get; set; }
        public double PaddingFactor { get; set; } = 1.05;
        public bool DrawLabels { get; set; }
        public bool DrawZeroLabels { get; set; }
        public PiePlotExtents PlotExtents { get; set; } = new PiePlotExtents();

        private PlotColorsCollection colorCollection;

        public PiePlot() : base()
        {
            colorCollection = PlotColors;
        }

        protected override Size MeasurePlot(Size availableSize)
        {
            double actualWidth = availableSize.Width;
            double actualHeight = availableSize.Height;

            PlotExtents.PlotFrameTopLeft = new Point(Padding.Left, Padding.Top);
            PlotExtents.PlotFrameBottomRight = new Point(actualWidth - Padding.Right, actualHeight - Padding.Bottom);

            //orientation
            if (actualWidth > actualHeight * 1.3)
                PlotExtents.Orientation = PiePlotOrientation.Landscape;
            else
                PlotExtents.Orientation = PiePlotOrientation.Portrait;

            if (!DataExtents.IsPrepared)
                return availableSize;

            //legend
            double maxLegendDescriptionWidth = 0;
            double maxLegendValueWidth = 0;
            double maxLegendWidth = 0;
            double maxLegendHeight = 0;
            double legendHeight = 0;

            foreach (PieSeriesDataPoint dataPoint in Series.ItemsDataPoints)
            {
                if (!string.IsNullOrEmpty(dataPoint.Category))
                {
                    Size descriptionSize = dataPoint.Category.MeasureTextSize(FontSize).WithFactor(PaddingFactor);
                    Size valueSize = dataPoint.ValueText.MeasureTextSize(FontSize).WithFactor(PaddingFactor);

                    legendHeight += descriptionSize.Height * 1.2;

                    if (descriptionSize.Width > maxLegendDescriptionWidth)
                    {
                        maxLegendDescriptionWidth = descriptionSize.Width;
                    }

                    if (valueSize.Width > maxLegendValueWidth)
                    {
                        maxLegendValueWidth = valueSize.Width;
                    }

                    double totalWidth = (maxLegendDescriptionWidth + maxLegendValueWidth) * 1.5;
                    if (totalWidth > maxLegendWidth)
                    {
                        maxLegendWidth = totalWidth;
                    }

                    if (descriptionSize.Height > maxLegendHeight)
                    {
                        maxLegendHeight = descriptionSize.Height;
                    }
                }
            }

            PlotExtents.LegendColumns = 1;
            while (legendHeight > (PlotExtents.PlotFrameBottomRight.Y - PlotExtents.PlotFrameTopLeft.Y) * 0.8)
            {
                PlotExtents.LegendColumns += 1;
                PlotExtents.Orientation = PiePlotOrientation.Landscape;                
                legendHeight /= 2;
            }

            PlotExtents.LegendItemIndicatorWidth = maxLegendHeight;
            double legendWidth = (maxLegendWidth + maxLegendHeight) * PlotExtents.LegendColumns;

            PlotExtents.LegendColumnWidth = maxLegendWidth + maxLegendHeight;
            PlotExtents.LegendItemHeight = maxLegendHeight;

            if (PlotExtents.Orientation == PiePlotOrientation.Portrait)
            {
                PlotExtents.PlotAreaTopLeft = PlotExtents.PlotAreaTopLeft;
                PlotExtents.PlotAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y - legendHeight);

                double widthRemaining = PlotExtents.PlotFrameBottomRight.X - PlotExtents.PlotFrameTopLeft.X - legendWidth;
                PlotExtents.LegendAreaTopLeft = new Point(PlotExtents.PlotFrameTopLeft.X + widthRemaining / 2, PlotExtents.PlotFrameBottomRight.Y - legendHeight);
                PlotExtents.LegendAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X - widthRemaining / 2, PlotExtents.PlotFrameBottomRight.Y);
            }
            else
            {
                PlotExtents.PlotAreaTopLeft = PlotExtents.PlotFrameTopLeft;
                PlotExtents.PlotAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X - legendWidth, PlotExtents.PlotFrameBottomRight.Y);

                double heightRemaining = PlotExtents.PlotFrameBottomRight.Y - PlotExtents.PlotFrameTopLeft.Y - legendHeight;
                PlotExtents.LegendAreaTopLeft = new Point(PlotExtents.PlotFrameBottomRight.X - legendWidth, PlotExtents.PlotFrameTopLeft.Y + heightRemaining / 2);
                PlotExtents.LegendAreaBottomRight = new Point(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y - heightRemaining / 2);
            }

            if (PlotExtents.PlotAreaTopLeft.X < PlotExtents.PlotFrameTopLeft.X || PlotExtents.PlotAreaTopLeft.Y < PlotExtents.PlotFrameTopLeft.Y)
                PlotExtents.PlotAreaTopLeft = PlotExtents.PlotFrameTopLeft;

            if (PlotExtents.PlotAreaBottomRight.X > PlotExtents.PlotFrameBottomRight.X || PlotExtents.PlotAreaBottomRight.Y > PlotExtents.PlotFrameBottomRight.Y)
                PlotExtents.PlotAreaBottomRight = PlotExtents.PlotFrameBottomRight;
            
            double width = Math.Max(PlotExtents.FrameWidth * 0.2, Math.Abs(PlotExtents.PlotAreaBottomRight.X - PlotExtents.PlotAreaTopLeft.X));
            double height = Math.Max(PlotExtents.FrameWidth * 0.2, Math.Abs(PlotExtents.PlotAreaBottomRight.Y - PlotExtents.PlotAreaTopLeft.Y));
            PlotExtents.Origin = new Point(PlotExtents.PlotAreaTopLeft.X + width * 0.5, PlotExtents.PlotAreaTopLeft.Y + height * 0.5);
            PlotExtents.Radius = Math.Min(width * 0.5, height * 0.5) * 0.9;

            return availableSize;
        }
    
        protected override bool ValidateSeries()
        {
            if (dataPrepException != null)
            {
                dataValidationErrorMessage = $"{dataPrepException.Message}{Environment.NewLine}{dataPrepException.StackTrace}";
                return false;
            }

            string message = null;

            if (Series is null || Series.ItemsDataPoints is null)
            {
                message = "No series defined or set";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dataValidationErrorMessage = message;
                return false;
            }

            var total = Math.Round(Series.ItemsDataPoints.Sum(x => x.NormalizedValue),4);
            if (total > 1)
            {
                message = "Cannot plot as pie chart with items totalling greater than 100%";
            }

            if (string.IsNullOrEmpty(message))
            {
                return true;
            }

            dataValidationErrorMessage = message;
            return string.IsNullOrEmpty(message);
        }

        internal override void PrepareData(DataExtents extents)
        {
            if (DataContext is null)
            {
                return;
            }
            
            if (Series is null)
            {
                return;
            }

            dataPrepException = null;
            dataValidationErrorMessage = null;

            try
            {
                SeriesMetaData meta = Series.PrepareData(DataContext);
                extents.LongestCategory = meta.LongestCategory;
                extents.IsPrepared = true;

                if (Series.ItemsDataPoints.Count <= 10 && PlotColors10 != null)
                    colorCollection = PlotColors10;
                else if (Series.ItemsDataPoints.Count <= 15 && PlotColors15 != null)
                    colorCollection = PlotColors15;
                else if (Series.ItemsDataPoints.Count <= 20 && PlotColors20 != null)
                    colorCollection = PlotColors20;
                else if (Series.ItemsDataPoints.Count <= 25 && PlotColors25 != null)
                    colorCollection = PlotColors25;
                else
                {
                    colorCollection = PlotColors;

                    if (PlotColors.Items.Count < Series.ItemsDataPoints.Count)
                    {
                        Random rnd = new Random();

                        while (PlotColors.Items.Count < Series.ItemsDataPoints.Count)
                        {
                            byte r = (byte)rnd.Next(0, 255);
                            byte g = (byte)rnd.Next(0, 255);
                            byte b = (byte)rnd.Next(0, 255);
                            PlotColors.Items.Add(new PlotColorItem()
                            {
                                FillBrush = new SolidColorBrush(new Color() { A = 255, R = r, G = g, B = b }),
                                StrokeBrush = new SolidColorBrush(new Color() { A = 255, R = r, G = g, B = b })
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dataPrepException = ex;
            }
        }

        protected override void Draw()
        {
#if DEBUG
            Debug.WriteLine($"{Name} Draw");
#endif

            if (!ValidateSeries())
                return;

            ClearLayout();
            
            DrawPlotArea();
            DrawSeriesInternal();
        }

        protected virtual void DrawPlotArea()
        {
            if (!DataExtents.IsPrepared)
                return;

            //LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameBottomRight.Y, PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.PlotFrameBottomRight.X, PlotExtents.PlotFrameTopLeft.Y, PlotExtents.PlotFrameTopLeft.X, PlotExtents.PlotFrameTopLeft.Y, PlotAreaStrokeBrush, PlotAreaStrokeThickness * 1.25);

            //LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotLegendStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotLegendStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaBottomRight.Y, PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotLegendStrokeBrush, PlotAreaStrokeThickness * 1.25);
            //LayoutRoot.DrawLine(PlotExtents.LegendAreaBottomRight.X, PlotExtents.LegendAreaTopLeft.Y, PlotExtents.LegendAreaTopLeft.X, PlotExtents.LegendAreaTopLeft.Y, PlotLegendStrokeBrush, PlotAreaStrokeThickness * 1.25);

            var colorStep = (double)colorCollection.Items.Count / (double)Series.ItemsDataPoints.Count;

            double height = 0;
            int sIndex = 0;
            int columnCount = (int)Math.Round((PlotExtents.LegendAreaBottomRight.Y - PlotExtents.LegendAreaTopLeft.Y) / PlotExtents.LegendItemHeight);
            int sCount = 0;
            for (int i = 0; i < PlotExtents.LegendColumns; i++)
            {
                height = 0;
                sCount = 0;
                while (sIndex < Series.ItemsDataPoints.Count)
                {
                    var item = Series.ItemsDataPoints[sIndex];
                    int colorIndex = (int)Math.Round(colorStep * sIndex);
                    double columnX = PlotExtents.LegendAreaTopLeft.X + (i * PlotExtents.LegendColumnWidth);
                    LayoutRoot.DrawLegendItem(
                        item.Category, 
                        columnX, 
                        PlotExtents.LegendAreaTopLeft.Y + height, 
                        FontSize, 
                        PlotExtents.LegendItemIndicatorWidth, 
                        colorCollection.Items[colorIndex].FillBrush, 
                        PaddingFactor);
                    LayoutRoot.DrawLegendValue(
                        item.ValueText, 
                        columnX + PlotExtents.LegendColumnWidth - PlotExtents.LegendItemHeight, 
                        PlotExtents.LegendAreaTopLeft.Y + height, 
                        FontSize, 
                        TextAlignment.Right);

                    sCount++;
                    height += PlotExtents.LegendItemHeight;

                    if (sCount == columnCount)
                        break;

                    sIndex++;
                }
            }
        }

        protected virtual void DrawSeriesInternal()
        {
            Point[] labelPoints = new Point[Series.ItemsDataPoints.Count];

            var colorStep = (double)colorCollection.Items.Count / (double)Series.ItemsDataPoints.Count;

            double totalAreaCovered = Series.ItemsDataPoints.Sum(x => x.NormalizedValue);
            if (totalAreaCovered == 0)
            {
                DrawEmptyPlot();
                return;
            }


            double offset = 0;
            for (int i = 0; i < Series.ItemsDataPoints.Count; i++)
            {
                var item = Series.ItemsDataPoints[i];

                if (item.NormalizedValue == 0 && !DrawZeroLabels)
                    continue;

                int colorIndex = (int)Math.Round(colorStep * i);

                Point sliceAvg = LayoutRoot.DrawSlice(
                    PlotExtents.Origin.X, 
                    PlotExtents.Origin.Y, 
                    PlotExtents.Radius, 
                    offset * Math.PI * 2, 
                    item.NormalizedValue * Math.PI * 2,
                    colorCollection.Items[colorIndex].StrokeBrush, 
                    1, 
                    colorCollection.Items[colorIndex].FillBrush);

                labelPoints[i] = sliceAvg;
                offset += item.NormalizedValue;
            }

            if (DrawLabels)
            {
                for (int i = 0; i < Series.ItemsDataPoints.Count; i++)
                {
                    var item = Series.ItemsDataPoints[i];

                    if (item.NormalizedValue == 0 && !DrawZeroLabels)
                        continue;

                    LayoutRoot.DrawSliceLabel(
                        item.ValueText, 
                        labelPoints[i].X + PlotExtents.Origin.X, 
                        labelPoints[i].Y + PlotExtents.Origin.Y, 
                        FontSize, 
                        new SolidColorBrush(Colors.DarkGray));
                }
            }
        }

        protected virtual void DrawEmptyPlot()
        {
            LayoutRoot.DrawSlice(
                PlotExtents.Origin.X,
                PlotExtents.Origin.Y,
                PlotExtents.Radius,
                0,
                Math.PI * 2,
                EmptyStrokeBrush,
                1,
                new SolidColorBrush(Colors.Transparent));
        }
    }
}
