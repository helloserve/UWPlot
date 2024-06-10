using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace helloserve.com.UWPlot.Tests
{
    [TestClass]
    public class BoundsExtentionsTests
    {
        [DataTestMethod]
        [DataRow(5330D, 6000D)]
        [DataRow(330D, 400D)]
        [DataRow(10330D, 12000D)]
        [DataRow(100330D, 120000D)]
        [DataRow(200330D, 220000D)]
        public void ShouldCalculatedUpperBound(double value, double expected)
        {
            double actual = value.CalculateUpperBound(value, out double magnitude);
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow(0.0053D, 0.001D)]
        [DataRow(0.053D, 0.01D)]
        [DataRow(0.53D, 0.1D)]
        [DataRow(5.3D, 1D)]
        [DataRow(53D, 10D)]
        [DataRow(533D, 100D)]
        [DataRow(5330D, 1000D)]
        [DataRow(53333D, 10000D)]
        [DataRow(533333D, 100000D)]
        [DataRow(5333333D, 1000000D)]
        public void ShouldCalculateMagnitude(double value, double expected)
        {
            double actual = value.GetMagnitude();
            Assert.AreEqual(expected, actual);
        }
    }    
}
