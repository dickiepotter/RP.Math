namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Cone"/> and the positioned <see cref="PlacedCone"/>.</summary>
    [TestClass]
    public class ConeTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Cone(3, 4);
            c.SlantHeight.Should().BeApproximately(5, Tol);
            c.BaseArea.Should().BeApproximately(Math.PI * 9, Tol);
            c.LateralArea.Should().BeApproximately(Math.PI * 3 * 5, Tol);
            c.Volume.Should().BeApproximately((1.0 / 3.0) * Math.PI * 9 * 4, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_TaperedProfile_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            c.Contains(new Vector(0, 0, 0), LooseTol).Should().BeTrue();   // base centre
            c.Contains(new Vector(0, 0, 4), LooseTol).Should().BeTrue();   // apex
            c.Contains(new Vector(1, 0, 2), LooseTol).Should().BeTrue();   // on the slant (allowed radius is 1 at z=2)
            c.Contains(new Vector(1.5, 0, 2), LooseTol).Should().BeFalse();
            c.Contains(new Vector(0, 0, 5), LooseTol).Should().BeFalse();  // above the apex
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            c.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();  // base rim
            c.ClosestPoint(new Vector(0, 0, 10)).Equals(new Vector(0, 0, 4), LooseTol).Should().BeTrue(); // apex
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_SideAndAxis_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            c.TryIntersect(new Ray(new Vector(5, 0, 2), new Vector(-1, 0, 0)), out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 0, 2), LooseTol).Should().BeTrue(); // allowed radius is 1 at z=2

            c.TryIntersect(new Ray(new Vector(0, 0, 10), new Vector(0, 0, -1)), out Vector apex).Should().BeTrue();
            apex.Equals(new Vector(0, 0, 4), LooseTol).Should().BeTrue(); // hits the apex first
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_AcrossSide_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            c.TryIntersect(new Line(new Vector(0, 0, 2), new Vector(1, 0, 0)), out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-1, 0, 2), LooseTol).Should().BeTrue();
            far.Equals(new Vector(1, 0, 2), LooseTol).Should().BeTrue();
        }
    }
}
