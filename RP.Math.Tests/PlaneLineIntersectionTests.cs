namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the intersection and closest-approach maths that ties the line family
    /// (<see cref="Line"/>, <see cref="Ray"/>, <see cref="LineSegment"/>) to <see cref="Plane"/> and to
    /// itself.
    /// </summary>
    [TestClass]
    public class PlaneLineIntersectionTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        #region Plane and Line

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectLine_Crossing_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY; // z = 0
            var line = new Line(new Vector(2, 3, 5), new Vector(0, 0, -1)); // straight down onto z = 0

            plane.TryIntersect(line, out Vector hit).Should().BeTrue();
            hit.Equals(new Vector(2, 3, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectLine_Parallel_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var line = new Line(new Vector(0, 0, 4), new Vector(1, 0, 0)); // runs parallel to z = 0

            plane.TryIntersect(line, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Line")]
        public void Line_TryIntersectPlane_ShouldAgreeWithThePlane_Test()
        {
            var plane = Plane.XY;
            var line = new Line(new Vector(2, 3, 5), new Vector(0, 0, -1));

            line.TryIntersect(plane, out Vector hit).Should().BeTrue();
            hit.Equals(new Vector(2, 3, 0), LooseTol).Should().BeTrue();
        }

        #endregion

        #region Plane and Ray

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectRay_PointingAtThePlane_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY;
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1)); // aimed at z = 0

            plane.TryIntersect(ray, out Vector hit).Should().BeTrue();
            hit.Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectRay_PointingAway_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, 1)); // crossing is behind the origin

            plane.TryIntersect(ray, out _).Should().BeFalse();
        }

        #endregion

        #region Plane and LineSegment

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectSegment_Crossing_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY;
            var segment = new LineSegment(new Vector(0, 0, 2), new Vector(0, 0, -2)); // crosses z = 0 at the middle

            plane.TryIntersect(segment, out Vector hit).Should().BeTrue();
            hit.Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectSegment_NotReaching_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var segment = new LineSegment(new Vector(0, 0, 2), new Vector(0, 0, 1)); // both ends above z = 0

            plane.TryIntersect(segment, out _).Should().BeFalse();
        }

        #endregion

        #region Line and Line

        [TestMethod, TestCategory("LineLine")]
        public void Line_TryIntersectLine_Crossing_ShouldReturnThePoint_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)); // the X axis
            var y = new Line(new Vector(0, 0, 0), new Vector(0, 1, 0)); // the Y axis

            x.TryIntersect(y, Tol, out Vector hit).Should().BeTrue();
            hit.Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_TryIntersectLine_Skew_ShouldReturnFalse_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));      // along X at z = 0
            var skew = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0));   // along Y at z = 1 (never meets)

            x.TryIntersect(skew, Tol, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_DistanceTo_SkewLines_ShouldBeTheGap_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));    // along X at z = 0
            var skew = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0)); // along Y at z = 1

            x.DistanceTo(skew).Should().BeApproximately(1, LooseTol);
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_DistanceTo_ParallelLines_ShouldBeThePerpendicularGap_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 3, 0), new Vector(1, 0, 0)); // parallel, 3 apart in Y

            a.DistanceTo(b).Should().BeApproximately(3, LooseTol);
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_ClosestPointTo_OtherLine_ShouldBeTheFootOfTheCommonPerpendicular_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));      // along X at z = 0
            var skew = new Line(new Vector(2, 0, 1), new Vector(0, 1, 0));   // along Y through x = 2 at z = 1

            // Nearest point on the X axis is directly under the crossing at x = 2.
            x.ClosestPointTo(skew).Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        #endregion
    }
}
