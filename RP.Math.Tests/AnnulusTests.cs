namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Annulus"/> and the positioned <see cref="PlacedAnnulus"/>.</summary>
    [TestClass]
    public class AnnulusTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var a = new Annulus(2, 5);
            a.Width.Should().Be(3);
            a.MeanRadius.Should().Be(3.5);
            a.Area.Should().BeApproximately(Math.PI * (25 - 4), Tol);
            a.Perimeter.Should().BeApproximately(2 * Math.PI * 7, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void InnerExceedingOuter_ShouldThrow_Test()
        {
            Action act = () => new Annulus(5, 2);
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_RingHoleAndRim_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            a.Contains(new Vector(3, 0, 0), Tol).Should().BeTrue();
            a.Contains(new Vector(1, 0, 0), Tol).Should().BeFalse(); // in the hole
            a.Contains(new Vector(6, 0, 0), Tol).Should().BeFalse(); // beyond the rim
            a.Contains(new Vector(2, 0, 0), Tol).Should().BeTrue();  // on the inner edge
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_PushesToNearestEdge_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            a.ClosestPoint(new Vector(1, 0, 0)).Equals(new Vector(2, 0, 0), Tol).Should().BeTrue();  // out to inner edge
            a.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(5, 0, 0), Tol).Should().BeTrue(); // in to outer edge
            a.ClosestPoint(new Vector(3, 0, 0)).Equals(new Vector(3, 0, 0), Tol).Should().BeTrue();  // already over the ring
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_OnRingAndThroughHole_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            a.TryIntersect(new Ray(new Vector(3, 0, 5), new Vector(0, 0, -1)), out Vector p).Should().BeTrue();
            p.Equals(new Vector(3, 0, 0), 1e-6).Should().BeTrue();
            a.TryIntersect(new Ray(new Vector(1, 0, 5), new Vector(0, 0, -1)), out _).Should().BeFalse(); // through the hole
        }
    }
}
