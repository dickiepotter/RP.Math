namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests pinning the two distinct "absolute value" operations on <see cref="Vector"/>:
    /// <see cref="Vector.Abs()"/> is the magnitude (scalar norm), while
    /// <see cref="Vector.AbsComponents()"/> is the component-wise <c>(|x|, |y|, |z|)</c>.
    /// </summary>
    [TestClass]
    public class VectorAbsTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Abs")]
        public void Abs_ReturnsTheMagnitude_NotComponentwise_Test()
        {
            var v = new Vector(3, -4, 0);
            v.Abs().Should().BeApproximately(5, Tolerance);           // |v|, not a vector
            v.Abs().Should().Be(v.Magnitude);                          // alias for the property
            Vector.Abs(v).Should().BeApproximately(5, Tolerance);      // static form agrees
        }

        [TestMethod, TestCategory("AbsComponents")]
        public void AbsComponents_TakesTheAbsoluteValuePerAxis_Test()
        {
            var v = new Vector(-3, 4, -5);
            v.AbsComponents().Equals(new Vector(3, 4, 5), Tolerance).Should().BeTrue();
            Vector.AbsComponents(v).Equals(new Vector(3, 4, 5), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("AbsComponents")]
        public void AbsComponents_OfAnAlreadyPositiveVector_IsUnchanged_Test()
        {
            var v = new Vector(1, 2, 3);
            v.AbsComponents().Equals(v, Tolerance).Should().BeTrue();
        }
    }
}
