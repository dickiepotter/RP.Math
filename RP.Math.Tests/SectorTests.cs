namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Sector"/> and the positioned <see cref="PlacedSector"/>.</summary>
    [TestClass]
    public class SectorTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static Sector QuarterCircle()
        {
            return new Sector(2, new Angle(90, AngleUnits.DEG));
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var s = QuarterCircle();
            s.Area.Should().BeApproximately(0.5 * 4 * (Math.PI / 2), Tol); // = pi
            s.ArcLength.Should().BeApproximately(2 * (Math.PI / 2), Tol);  // = pi
            s.ChordLength.Should().BeApproximately(2 * 2 * Math.Sin(Math.PI / 4), Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_WithinRadiusAndSweep_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            s.Contains(new Vector(1, 0.5, 0), LooseTol).Should().BeTrue();   // angle ~27deg, r ~1.1
            s.Contains(new Vector(-1, 0, 0), LooseTol).Should().BeFalse();   // angle 180deg, outside sweep
            s.Contains(new Vector(1, -0.5, 0), LooseTol).Should().BeFalse(); // negative angle, outside sweep
            s.Contains(new Vector(3, 0, 0), LooseTol).Should().BeFalse();    // beyond the radius
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OnArcAndToApex_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            s.ClosestPoint(new Vector(3, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue(); // out along the arc
            s.ClosestPoint(new Vector(-1, 0, 0)).Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue(); // nearest is the apex
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_InsideAndOutsideSweep_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            s.TryIntersect(new Ray(new Vector(1, 0.2, 5), new Vector(0, 0, -1)), out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 0.2, 0), LooseTol).Should().BeTrue();
            s.TryIntersect(new Ray(new Vector(-1, 0, 5), new Vector(0, 0, -1)), out _).Should().BeFalse();
        }
    }
}
