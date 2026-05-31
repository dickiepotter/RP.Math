namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Cuboid"/> type.</summary>
    [TestClass]
    public class CuboidTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSizes_Test()
        {
            var c = new Cuboid(2, 3, 4);
            c.Width.Should().Be(2);
            c.Height.Should().Be(3);
            c.Depth.Should().Be(4);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeSize_ShouldThrow_Test()
        {
            Action act = () => new Cuboid(2, -3, 4);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var c = new Cuboid(2, 3, 4);
            c.Volume.Should().Be(24);
            c.SurfaceArea.Should().Be(52);                       // 2(6 + 12 + 8)
            c.SpaceDiagonal.Should().BeApproximately(System.Math.Sqrt(29), Tol);
            c.Size.Should().Be(new Vector(2, 3, 4));
        }

        [TestMethod, TestCategory("Classification")]
        public void IsCube_ShouldDetectEqualEdges_Test()
        {
            Cuboid.Cube(3).IsCube(Tol).Should().BeTrue();
            new Cuboid(2, 3, 4).IsCube(Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByVolume_Test()
        {
            var small = Cuboid.Cube(2); // 8
            var large = new Cuboid(2, 3, 4); // 24
            (large > small).Should().BeTrue();
            large.CompareTo(small).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSizes_Test()
        {
            (new Cuboid(2, 3, 4) == new Cuboid(2, 3, 4)).Should().BeTrue();
            (new Cuboid(2, 3, 4) != new Cuboid(2, 3, 5)).Should().BeTrue();
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSizes_Test()
        {
            var (w, h, d) = new Cuboid(2, 3, 4);
            w.Should().Be(2);
            h.Should().Be(3);
            d.Should().Be(4);
        }
    }
}
