using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace helloserve.com.UWPlot.Tests
{
    [TestClass]
    public class BoundsExtentionsTests
    {
        [DataTestMethod]
        [DataRow(5330D, 6000D)]
        [DataRow(330D, 400D)]
        [DataRow(10330D, 20000D)]
        [DataRow(100330D, 200000D)]
        [DataRow(200330D, 300000D)]
        public void ShouldCalculatedUpperBound(double value, double expected)
        {
            double actual = value.CalculateUpperBound(out double magnitude);
            Assert.AreEqual(expected, actual);
        }
    }
}
