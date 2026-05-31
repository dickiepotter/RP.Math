namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedCylinder"/> type.</summary>
    [TestClass]
    public class PlacedCylinderTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static PlacedCylinder AlongZ()
        {
            return PlacedCylinder.AlongZ(new Cylinder(2, 4), new Vector(0, 0, 0)); // radius 2, half-height 2
        }

        [TestMethod, TestCategory("Placement")]
        public void AlongZ_ShouldExposeAxisAndCaps_Test()
        {
            var c = AlongZ();
            c.Axis.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
            c.BaseCenter.Equals(new Vector(0, 0, -2), Tol).Should().BeTrue();
            c.TopCenter.Equals(new Vector(0, 0, 2), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectRadiusAndHeight_Test()
        {
            var c = AlongZ();
            c.Contains(new Vector(1, 0, 0), Tol).Should().BeTrue();
            c.Contains(new Vector(0, 0, 2), Tol).Should().BeTrue();   // on a cap
            c.Contains(new Vector(0, 0, 2.5), Tol).Should().BeFalse(); // beyond a cap
            c.Contains(new Vector(2.5, 0, 0), Tol).Should().BeFalse(); // beyond the radius
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampRadiusAndHeight_Test()
        {
            AlongZ().ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), Tol).Should().BeTrue();
            AlongZ().ClosestPoint(new Vector(0, 0, 5)).Equals(new Vector(0, 0, 2), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_HitsCurvedSide_Test()
        {
            var ray = new Ray(new Vector(5, 0, 0), new Vector(-1, 0, 0));
            AlongZ().TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_HitsCap_Test()
        {
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1));
            AlongZ().TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(0, 0, 2), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_AcrossSide_ShouldGiveBoth_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            AlongZ().TryIntersect(line, out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-2, 0, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_FromInside_ShouldHitExit_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            AlongZ().TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Factory")]
        public void FromEndPoints_ShouldOrientAlongAxis_Test()
        {
            var c = PlacedCylinder.FromEndPoints(new Vector(0, 0, 0), new Vector(4, 0, 0), 2);
            c.Axis.Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();
            c.BaseCenter.Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
            c.TopCenter.Equals(new Vector(4, 0, 0), LooseTol).Should().BeTrue();
            c.Height.Should().BeApproximately(4, LooseTol);
        }
    }
}
