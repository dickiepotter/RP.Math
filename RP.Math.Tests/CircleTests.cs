namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Circle"/> and the positioned <see cref="PlacedCircle"/>.</summary>
    [TestClass]
    public class CircleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Circle(3);
            c.Area.Should().BeApproximately(Math.PI * 9, Tol);
            c.Circumference.Should().BeApproximately(2 * Math.PI * 3, Tol);
            c.Perimeter.Should().BeApproximately(2 * Math.PI * 3, Tol);
            c.Diameter.Should().Be(6);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Circle(-1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByArea_Test()
        {
            (new Circle(3) > new Circle(2)).Should().BeTrue();
            new Circle(3).CompareTo(new Circle(2)).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("Placed")]
        public void Placement_ExposesCentreAndNormal_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(3), new Vector(1, 2, 0));
            c.Center.Should().Be(new Vector(1, 2, 0));
            c.Centroid.Should().Be(new Vector(1, 2, 0));
            c.Normal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
            c.Area.Should().BeApproximately(Math.PI * 9, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void FromCenterNormal_ShouldOrientPlane_Test()
        {
            var c = PlacedCircle.FromCenterNormal(new Circle(4), new Vector(1, 2, 3), new Vector(0, 1, 0));
            c.Normal.Equals(new Vector(0, 1, 0), LooseTol).Should().BeTrue();
            c.Plane.Contains(c.Center, LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_OnDisc_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            c.Contains(new Vector(3, 4, 0), Tol).Should().BeTrue();  // on the rim
            c.Contains(new Vector(1, 1, 0), Tol).Should().BeTrue();
            c.Contains(new Vector(6, 0, 0), Tol).Should().BeFalse(); // beyond the radius
            c.Contains(new Vector(1, 1, 2), Tol).Should().BeFalse(); // off the plane
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OutsidePoint_ShouldClampToRim_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            c.ClosestPoint(new Vector(20, 0, 7)).Equals(new Vector(5, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_ThroughDisc_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));
            c.TryIntersect(line, out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(1, 1, 0), LooseTol).Should().BeTrue();
            far.Equals(near, LooseTol).Should().BeTrue(); // a flat disc has a single crossing

        }
    }
}
