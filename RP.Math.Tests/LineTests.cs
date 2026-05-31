namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the infinite <see cref="Line"/> type.</summary>
    [TestClass]
    public class LineTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_ShouldNormalizeDirection_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(0, 0, 5));
            line.Direction.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void ThroughPoints_ShouldRunBetweenThem_Test()
        {
            var line = Line.ThroughPoints(new Vector(0, 0, 0), new Vector(2, 2, 0));
            line.Contains(new Vector(1, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Points")]
        public void PointAt_ShouldTravelBothWays_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            line.PointAt(3).Equals(new Vector(3, 0, 0), Tol).Should().BeTrue();
            line.PointAt(-3).Equals(new Vector(-3, 0, 0), Tol).Should().BeTrue(); // a line is infinite both ways
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_And_DistanceTo_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 1, 0));
            line.ClosestPointTo(new Vector(0, 2, 0)).Equals(new Vector(1, 1, 0), Tol).Should().BeTrue();
            line.DistanceTo(new Vector(0, 2, 0)).Should().BeApproximately(System.Math.Sqrt(2), Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void Parallel_Lines_ShouldReportDistance_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 5, 0), new Vector(1, 0, 0));
            a.IsParallelTo(b, Tol).Should().BeTrue();
            a.DistanceTo(b).Should().BeApproximately(5, Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void Skew_Lines_ShouldMeasureGap_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0)); // passes 1 above, perpendicular
            a.DistanceTo(b).Should().BeApproximately(1, Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void TryIntersect_CrossingLines_ShouldReturnPoint_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(1, -1, 0), new Vector(0, 1, 0));
            a.TryIntersect(b, Tol, out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 0, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToRay_ShouldShareOriginAndDirection_Test()
        {
            var line = new Line(new Vector(1, 2, 3), new Vector(0, 0, 1));
            var ray = line.ToRay();
            ray.Origin.Equals(new Vector(1, 2, 3), Tol).Should().BeTrue();
            ray.Direction.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }
    }
}
