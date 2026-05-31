namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Rectangle"/> type.</summary>
    [TestClass]
    public class RectangleTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSizes_Test()
        {
            var r = new Rectangle(3, 4);
            r.Width.Should().Be(3);
            r.Height.Should().Be(4);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeSize_ShouldThrow_Test()
        {
            Action act = () => new Rectangle(-1, 2);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var r = new Rectangle(3, 4);
            r.Area.Should().Be(12);
            r.Perimeter.Should().Be(14);
            r.DiagonalLength.Should().BeApproximately(5, Tol);
            r.AspectRatio.Should().BeApproximately(0.75, Tol);
        }

        [TestMethod, TestCategory("Classification")]
        public void IsSquare_ShouldDetectEqualSides_Test()
        {
            Rectangle.Square(5).IsSquare(Tol).Should().BeTrue();
            new Rectangle(3, 4).IsSquare(Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByArea_Test()
        {
            var small = new Rectangle(2, 2); // area 4
            var large = new Rectangle(3, 4); // area 12
            (large > small).Should().BeTrue();
            (small <= large).Should().BeTrue();
            large.CompareTo(small).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSizes_Test()
        {
            (new Rectangle(3, 4) == new Rectangle(3, 4)).Should().BeTrue();
            (new Rectangle(3, 4) != new Rectangle(4, 3)).Should().BeTrue();
            new Rectangle(3, 4).Equals(new Rectangle(3.0000001, 4), 1e-3).Should().BeTrue();
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSizes_Test()
        {
            var (w, h) = new Rectangle(3, 4);
            w.Should().Be(3);
            h.Should().Be(4);
        }
    }
}
