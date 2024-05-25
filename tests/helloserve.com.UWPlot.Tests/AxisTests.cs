using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace helloserve.com.UWPlot.Tests
{
    [TestClass]
    public class AxisTests
    {
        [DataTestMethod]
        [DataRow(65780, -5230, 70000, -6000, 0.1)]
        [DataRow(980000, 0, 1000000, 0, 0.1)]
        [DataRow(0.05532, 0.05002, 0.06, 0.05, 0.0001)]
        public void ShouldCalculateBoundsForAxis(double seriesMax, double seriesMin, double boundsMax, double boundsMin, double delta)
        {
            YAxis axis = new YAxis();
            axis.Measure(seriesMax, seriesMin, 100);
            Assert.AreEqual(boundsMax, axis.CalculatedMax, delta);
            Assert.AreEqual(boundsMin, axis.CalculatedMin, delta);
        }
    }
}
