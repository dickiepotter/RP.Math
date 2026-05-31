namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="BoundingBox"/> type (an upright 3D rectangular block).</summary>
    [TestClass]
    public class BoundingBoxTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_ShouldSortCornersIntoMinMax_Test()
        {
            var b = new BoundingBox(new Vector(4, 5, 6), new Vector(1, 2, 3));
            b.Min.Should().Be(new Vector(1, 2, 3));
            b.Max.Should().Be(new Vector(4, 5, 6));
        }

        [TestMethod, TestCategory("Construction")]
        public void FromCenterSize_ShouldCentreOnThePoint_Test()
        {
            var b = BoundingBox.FromCenterSize(new Vector(0, 0, 0), new Vector(2, 4, 6));
            b.Min.Should().Be(new Vector(-1, -2, -3));
            b.Max.Should().Be(new Vector(1, 2, 3));
            b.Center.Equals(new Vector(0, 0, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void FromPoints_ShouldEncloseAllPoints_Test()
        {
            var b = BoundingBox.FromPoints(new Vector(1, 0, 0), new Vector(-2, 5, 1), new Vector(0, 0, -3));
            b.Min.Should().Be(new Vector(-2, 0, -3));
            b.Max.Should().Be(new Vector(1, 5, 1));
        }

        [TestMethod, TestCategory("Measurement")]
        public void SizeVolumeAndSurfaceArea_ShouldBeCorrect_Test()
        {
            var b = BoundingBox.FromCenterSize(new Vector(0, 0, 0), new Vector(2, 3, 4));
            b.Width.Should().BeApproximately(2, Tol);
            b.Height.Should().BeApproximately(3, Tol);
            b.Depth.Should().BeApproximately(4, Tol);
            b.Volume.Should().BeApproximately(24, Tol);
            b.SurfaceArea.Should().BeApproximately((2 * (6 + 12 + 8)), Tol); // 2(wh+hd+wd)=52
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldIncludeInteriorAndSurface_Test()
        {
            var b = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 2, 2));
            b.Contains(new Vector(1, 1, 1)).Should().BeTrue();
            b.Contains(new Vector(2, 2, 2)).Should().BeTrue();   // surface
            b.Contains(new Vector(3, 1, 1)).Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_AndTryIntersect_ShouldGiveOverlap_Test()
        {
            var a = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 2, 2));
            var c = new BoundingBox(new Vector(1, 1, 1), new Vector(3, 3, 3));
            a.Intersects(c).Should().BeTrue();

            a.TryIntersect(c, out var overlap).Should().BeTrue();
            overlap.Min.Should().Be(new Vector(1, 1, 1));
            overlap.Max.Should().Be(new Vector(2, 2, 2));
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersect_Disjoint_ShouldReturnFalse_Test()
        {
            var a = new BoundingBox(new Vector(0, 0, 0), new Vector(1, 1, 1));
            var c = new BoundingBox(new Vector(5, 5, 5), new Vector(6, 6, 6));
            a.TryIntersect(c, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Combine")]
        public void Union_ShouldEncloseBoth_Test()
        {
            var a = new BoundingBox(new Vector(0, 0, 0), new Vector(1, 1, 1));
            var c = new BoundingBox(new Vector(2, 2, 2), new Vector(3, 3, 3));
            var u = a.Union(c);
            u.Min.Should().Be(new Vector(0, 0, 0));
            u.Max.Should().Be(new Vector(3, 3, 3));
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampToTheBox_Test()
        {
            var b = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 2, 2));
            b.ClosestPoint(new Vector(5, 1, -3)).Should().Be(new Vector(2, 1, 0));
            b.DistanceTo(new Vector(2, 2, 5)).Should().BeApproximately(3, Tol);
            b.DistanceTo(new Vector(1, 1, 1)).Should().BeApproximately(0, Tol); // inside
        }
    }
}
