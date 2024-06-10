using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace helloserve.com.UWPlot.Tests
{
    [TestClass]
    public class AxisTests
    {
        [DataTestMethod]
        [DataRow(65780, -5230, 70000, -6000, 0.1)]
        [DataRow(980000, 0, 1000000, 0, 0.1)]
        [DataRow(1090000, 0, 1200000, 0, 0.1)]
        [DataRow(18289, 0, 20000, 0, 0.1)]
        [DataRow(0.05532, 0.05002, 0.06, 0.05, 0.0001)]
        public void ShouldCalculateBoundsForAxis(double seriesMax, double seriesMin, double boundsMax, double boundsMin, double delta)
        {
            YAxis axis = new YAxis();
            axis.Measure(seriesMax, seriesMin, 6);
            Assert.AreEqual(boundsMax, axis.CalculatedMax, delta);
            Assert.AreEqual(boundsMin, axis.CalculatedMin, delta);
        }

        [DataTestMethod]
        [DataRow(-30000, 10000, 1200000, 1000000, 10000, -200000, 1200000)]
        public void ShouldMeasureLineCount(double calculatedMin, double minMagnitude, double calculatedMax, double maxMagnitude, double baseMagnitude, double finalCalculatedMin, double finalCalculatedMax)
        {
            YAxis axis = new YAxis();
            axis.CalculatedMin = calculatedMin;
            axis.MagnitudeMin = minMagnitude;
            axis.CalculatedMax = calculatedMax;
            axis.MagnitudeMax = maxMagnitude;
            axis.BaseMagnitude = baseMagnitude;

            var result = axis.Measure(395, 25);

            Assert.AreEqual(7, result.NumberOfScaleLines);
            Assert.AreEqual(finalCalculatedMax, axis.CalculatedMax);
            Assert.AreEqual(finalCalculatedMin, axis.CalculatedMin);
        }
    }
}
