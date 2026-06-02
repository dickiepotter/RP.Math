namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedRectangle"/> type.</summary>
    [TestClass]
    public class PlacedRectangleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static PlacedRectangle Flat()
        {
            return PlacedRectangle.InXYPlane(new Rectangle(4, 6), new Vector(0, 0, 0));
        }

        [TestMethod, TestCategory("Placement")]
        public void InXYPlane_ShouldExposeAxesAndNormal_Test()
        {
            var r = Flat();
            r.Center.Should().Be(new Vector(0, 0, 0));
            r.AxisU.Equals(new Vector(1, 0, 0), Tol).Should().BeTrue();
            r.AxisV.Equals(new Vector(0, 1, 0), Tol).Should().BeTrue();
            r.Normal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldDelegateToShape_Test()
        {
            var r = Flat();
            r.Area.Should().Be(24);
            r.Perimeter.Should().Be(20);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectExtentsAndPlane_Test()
        {
            var r = Flat();
            r.Contains(new Vector(1, 2, 0), Tol).Should().BeTrue();
            r.Contains(new Vector(3, 0, 0), Tol).Should().BeFalse(); // half-width is 2
            r.Contains(new Vector(0, 0, 0.5), Tol).Should().BeFalse(); // off the plane
            r.Contains(new Vector(0, 0, 0.5), 1).Should().BeTrue();    // within tolerance of the plane
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampToExtents_Test()
        {
            var r = Flat();
            r.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), Tol).Should().BeTrue();
            r.ClosestPoint(new Vector(0, 10, 7)).Equals(new Vector(0, 3, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_ThroughFace_ShouldHit_Test()
        {
            var r = Flat();
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));
            r.TryIntersect(line, out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(1, 1, 0), LooseTol).Should().BeTrue();
            far.Equals(near, LooseTol).Should().BeTrue(); // a flat rectangle has a single crossing

        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_OutsideExtent_ShouldMiss_Test()
        {
            var r = Flat();
            var line = new Line(new Vector(3, 0, 5), new Vector(0, 0, -1)); // hits plane at x = 3 > half-width 2
            r.TryIntersect(line, out _, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var r = Flat();
            var ray = new Ray(new Vector(1, 1, 5), new Vector(0, 0, 1)); // away from the plane
            r.TryIntersect(ray, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placement")]
        public void Rotated_Containment_ShouldRoundTripThroughPose_Test()
        {
            var pose = Pose.FromAxisAngle(new Vector(1, 2, 3), new Vector(1, 0, 0), new Angle(90, AngleUnits.DEG));
            var r = new PlacedRectangle(new Rectangle(4, 6), pose);

            // A point known to be inside in local coordinates must be contained once placed by the pose.
            r.Contains(pose.Apply(new Vector(1, 2, 0)), LooseTol).Should().BeTrue();
            r.Contains(pose.Apply(new Vector(3, 0, 0)), LooseTol).Should().BeFalse(); // beyond half-width
            r.Center.Equals(new Vector(1, 2, 3), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Modification")]
        public void Translate_And_Scale_Test()
        {
            var r = Flat().Translate(new Vector(0, 0, 5));
            r.Center.Equals(new Vector(0, 0, 5), Tol).Should().BeTrue();

            var bigger = Flat().Scale(2);
            bigger.Area.Should().Be(24 * 4); // both sides doubled
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareShapeAndPose_Test()
        {
            (Flat() == Flat()).Should().BeTrue();
            (Flat() != Flat().Translate(new Vector(1, 0, 0))).Should().BeTrue();
        }
    }
}
