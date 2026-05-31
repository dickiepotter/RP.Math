namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Cylinder"/> type.</summary>
    [TestClass]
    public class CylinderTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresRadiusAndHeight_Test()
        {
            var c = new Cylinder(2, 5);
            c.Radius.Should().Be(2);
            c.Height.Should().Be(5);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Cylinder(-2, 5);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var c = new Cylinder(2, 5);
            c.Diameter.Should().Be(4);
            c.BaseArea.Should().BeApproximately(Math.PI * 4, Tol);
            c.Volume.Should().BeApproximately(Math.PI * 4 * 5, Tol);
            c.LateralArea.Should().BeApproximately(2 * Math.PI * 2 * 5, Tol);
            c.SurfaceArea.Should().BeApproximately(2 * Math.PI * 2 * (2 + 5), Tol);
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByVolume_Test()
        {
            var small = new Cylinder(1, 1);
            var large = new Cylinder(2, 5);
            (large > small).Should().BeTrue();
            large.CompareTo(small).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareRadiusAndHeight_Test()
        {
            (new Cylinder(2, 5) == new Cylinder(2, 5)).Should().BeTrue();
            (new Cylinder(2, 5) != new Cylinder(2, 6)).Should().BeTrue();
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldRadiusAndHeight_Test()
        {
            var (r, h) = new Cylinder(2, 5);
            r.Should().Be(2);
            h.Should().Be(5);
        }
    }
}
