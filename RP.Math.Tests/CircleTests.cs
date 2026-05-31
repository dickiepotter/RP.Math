namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Circle"/> type.</summary>
    [TestClass]
    public class CircleTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_ShouldNormalizeTheNormal_Test()
        {
            var c = new Circle(new Vector(0, 0, 0), new Vector(0, 0, 9), 2);
            c.Normal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Circle(new Vector(0, 0, 0), new Vector(0, 0, 1), -1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void AreaAndPerimeter_ShouldMatchFormulae_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 3);
            c.Area.Should().BeApproximately(System.Math.PI * 9, Tol);
            c.Perimeter.Should().BeApproximately(2 * System.Math.PI * 3, Tol);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_PointOnDisc_ShouldBeTrue_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            c.Contains(new Vector(3, 4, 0), Tol).Should().BeTrue();   // on rim, in plane
            c.Contains(new Vector(1, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_PointOffThePlane_ShouldBeFalse_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            c.Contains(new Vector(1, 1, 2), Tol).Should().BeFalse();  // off the plane
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_PointOutsideRadius_ShouldBeFalse_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            c.Contains(new Vector(6, 0, 0), Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_ShouldContainTheCircleCentre_Test()
        {
            var c = new Circle(new Vector(1, 2, 3), new Vector(0, 1, 0), 4);
            c.Plane.Contains(c.Center, Tol).Should().BeTrue();
            c.Plane.UnitNormal.Equals(new Vector(0, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_OutsidePoint_ShouldClampToRim_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            // A point above and far out: projects to the plane then clamps to the rim.
            c.ClosestPoint(new Vector(20, 0, 7)).Equals(new Vector(5, 0, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Shape")]
        public void ShouldExposeCentroidAndNormal_Test()
        {
            var s = Circle.InXYPlane(new Vector(1, 2, 0), 3);
            s.Centroid.Should().Be(new Vector(1, 2, 0));
            s.Normal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }
    }
}
