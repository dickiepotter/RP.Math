namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Torus"/> and the positioned <see cref="PlacedTorus"/>.</summary>
    [TestClass]
    public class TorusTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var t = new Torus(3, 1);
            t.OuterRadius.Should().Be(4);
            t.InnerRadius.Should().Be(2);
            t.Volume.Should().BeApproximately(2 * Math.PI * Math.PI * 3 * 1, Tol);
            t.SurfaceArea.Should().BeApproximately(4 * Math.PI * Math.PI * 3 * 1, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_TubeEdgesAndHole_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            t.Contains(new Vector(3, 0, 0), LooseTol).Should().BeTrue();   // on the central circle
            t.Contains(new Vector(4, 0, 0), LooseTol).Should().BeTrue();   // outer edge
            t.Contains(new Vector(2, 0, 0), LooseTol).Should().BeTrue();   // inner edge
            t.Contains(new Vector(3, 0, 1), LooseTol).Should().BeTrue();   // top of the tube
            t.Contains(new Vector(0, 0, 0), LooseTol).Should().BeFalse();  // the central hole
            t.Contains(new Vector(4.1, 0, 0), Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            t.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(4, 0, 0), LooseTol).Should().BeTrue();
            t.ClosestPoint(new Vector(0, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_HitsNearestTubeWall_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            t.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0)), out Vector p).Should().BeTrue();
            p.Equals(new Vector(4, 0, 0), LooseTol).Should().BeTrue(); // nearest of the four crossings
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_FourCrossings_GivesOuterMost_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            t.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-4, 0, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(4, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_DownTheHole_ShouldMiss_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            t.TryIntersect(new Ray(new Vector(0, 0, 10), new Vector(0, 0, -1)), out _).Should().BeFalse();
        }
    }
}
