namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Capsule"/> and the positioned <see cref="PlacedCapsule"/>.</summary>
    [TestClass]
    public class CapsuleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Capsule(1, 4);
            c.TotalHeight.Should().Be(6);
            c.Volume.Should().BeApproximately((Math.PI * 1 * 4) + ((4.0 / 3.0) * Math.PI), Tol);
            c.SurfaceArea.Should().BeApproximately((2 * Math.PI * 1 * 4) + (4 * Math.PI), Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_BodyAndCaps_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0)); // segment z in [-2, 2]
            c.Contains(new Vector(1, 0, 0), LooseTol).Should().BeTrue();   // cylinder side
            c.Contains(new Vector(0, 0, 3), LooseTol).Should().BeTrue();   // tip of the top cap
            c.Contains(new Vector(0, 0, 3.1), LooseTol).Should().BeFalse();
            c.Contains(new Vector(1.1, 0, 0), LooseTol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            c.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();
            c.ClosestPoint(new Vector(0, 0, 5)).Equals(new Vector(0, 0, 3), LooseTol).Should().BeTrue(); // out along the cap
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_SideAndCap_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            c.TryIntersect(new Ray(new Vector(5, 0, 0), new Vector(-1, 0, 0)), out Vector side).Should().BeTrue();
            side.Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();

            c.TryIntersect(new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1)), out Vector cap).Should().BeTrue();
            cap.Equals(new Vector(0, 0, 3), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_AcrossBody_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            c.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-1, 0, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void FromSegment_ShouldOrientAlongSegment_Test()
        {
            var c = PlacedCapsule.FromSegment(new Vector(0, 0, 0), new Vector(4, 0, 0), 1);
            c.Axis.Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();
            c.SegmentStart.Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
            c.SegmentEnd.Equals(new Vector(4, 0, 0), LooseTol).Should().BeTrue();
        }
    }
}
