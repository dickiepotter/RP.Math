namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the new cross-type intersection maths between the solid/planar shapes
    /// (<see cref="Sphere"/>, <see cref="Circle"/>) and the line family.
    /// </summary>
    [TestClass]
    public class ShapeIntersectionTests
    {
        private const double LooseTol = 1e-6;

        #region Sphere

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_PointingAtCentre_ShouldHitNearSide_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            var ray = new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0));
            s.TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            var ray = new Ray(new Vector(10, 0, 0), new Vector(1, 0, 0));
            s.TryIntersect(ray, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_FromInside_ShouldHitTheExit_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            s.TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectLine_ShouldGiveBothCrossings_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            s.TryIntersect(line, out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-2, 0, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectLine_Missing_ShouldBeFalse_Test()
        {
            var s = new Sphere(new Vector(0, 0, 0), 2);
            var line = new Line(new Vector(0, 5, 0), new Vector(1, 0, 0)); // passes 5 above, misses r = 2
            s.TryIntersect(line, out _, out _).Should().BeFalse();
        }

        #endregion

        #region Circle

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectLine_ThroughDisc_ShouldHit_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1)); // straight down through the disc
            c.TryIntersect(line, out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 1, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectLine_CrossingPlaneOutsideRadius_ShouldMiss_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            var line = new Line(new Vector(20, 0, 5), new Vector(0, 0, -1)); // hits the plane at x = 20, outside r = 5
            c.TryIntersect(line, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var c = Circle.InXYPlane(new Vector(0, 0, 0), 5);
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, 1)); // away from the plane
            c.TryIntersect(ray, out _).Should().BeFalse();
        }

        #endregion
    }
}
