using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace helloserve.com.UWPlot.Tests
{
    [TestClass]
    public class AxisTests
    {
        [DataTestMethod]
        [DataRow(65780, -5230, 80000, -6000)]
        public void ShouldCalculateBoundsForAxis(double seriesMax, double seriesMin, double boundsMax, double boundsMin)
        {
            YAxis axis = new YAxis();
            axis.Measure(seriesMax, seriesMin, 100);
            Assert.AreEqual(boundsMax, axis.CalculatedMax);
            Assert.AreEqual(boundsMin, axis.CalculatedMin);
        }
    }
}
