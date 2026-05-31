namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Sphere"/> type.</summary>
    [TestClass]
    public class SphereTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Sphere(new Vector(0, 0, 0), -1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void VolumeAndSurfaceArea_ShouldMatchFormulae_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            s.Volume.Should().BeApproximately((4.0 / 3.0) * System.Math.PI * 8, Tol);
            s.SurfaceArea.Should().BeApproximately(4 * System.Math.PI * 4, Tol);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldIncludeInteriorAndSurface_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 5);
            s.Contains(new Vector(3, 4, 0)).Should().BeTrue();  // on surface (|.|=5)
            s.Contains(new Vector(1, 1, 1)).Should().BeTrue();
            s.Contains(new Vector(5, 5, 5)).Should().BeFalse();
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestSurfacePoint_ShouldLandOnTheSurface_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 5);
            var p = s.ClosestSurfacePoint(new Vector(20, 0, 0));
            p.Equals(new Vector(5, 0, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Query")]
        public void SignedDistanceTo_ShouldBeNegativeInsidePositiveOutside_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 5);
            s.SignedDistanceTo(new Vector(8, 0, 0)).Should().BeApproximately(3, Tol);
            s.SignedDistanceTo(new Vector(2, 0, 0)).Should().BeApproximately(-3, Tol);
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_ShouldDetectOverlap_Test()
        {
            var a = new Sphere(new Vector(0, 0, 0), 2);
            var b = new Sphere(new Vector(3, 0, 0), 2);   // centres 3 apart, radii sum 4
            var c = new Sphere(new Vector(10, 0, 0), 2);
            a.Intersects(b).Should().BeTrue();
            a.Intersects(c).Should().BeFalse();
        }

        [TestMethod, TestCategory("Shape")]
        public void Centroid_ShouldBeTheCentre_Test()
        {
            var s = new Sphere(new Vector(1, 2, 3), 4);
            s.Centroid.Should().Be(new Vector(1, 2, 3));
        }
    }
}
