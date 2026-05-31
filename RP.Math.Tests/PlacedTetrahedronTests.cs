namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the vertex-defined <see cref="PlacedTetrahedron"/> type.</summary>
    [TestClass]
    public class PlacedTetrahedronTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        // The corner tetrahedron at the origin with unit legs along the axes.
        private static PlacedTetrahedron Unit()
        {
            return new PlacedTetrahedron(
                new Vector(0, 0, 0),
                new Vector(1, 0, 0),
                new Vector(0, 1, 0),
                new Vector(0, 0, 1));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Volume_Centroid_SurfaceArea_Test()
        {
            var t = Unit();
            t.Volume.Should().BeApproximately(1.0 / 6.0, Tol);
            t.Centroid.Equals(new Vector(0.25, 0.25, 0.25), Tol).Should().BeTrue();
            t.SurfaceArea.Should().BeApproximately(1.5 + (0.5 * Math.Sqrt(3)), LooseTol); // three legs + the slanted face
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_Test()
        {
            var t = Unit();
            t.Contains(t.Centroid, Tol).Should().BeTrue();
            t.Contains(new Vector(0.1, 0.1, 0.1), Tol).Should().BeTrue();
            t.Contains(new Vector(1, 1, 1), Tol).Should().BeFalse();
            t.Contains(new Vector(-0.1, 0, 0), Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ToVertexRegion_Test()
        {
            Unit().ClosestPoint(new Vector(-1, 0, 0)).Equals(new Vector(0, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersect_ThroughInterior_Test()
        {
            var t = Unit();
            var ray = new Ray(new Vector(0.25, 0.25, 5), new Vector(0, 0, -1));
            t.TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(0.25, 0.25, 0.5), LooseTol).Should().BeTrue(); // enters through the slanted face x+y+z=1

            t.TryIntersect(new Line(new Vector(0.25, 0.25, 0), new Vector(0, 0, 1)), out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(0.25, 0.25, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(0.25, 0.25, 0.5), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Transformation")]
        public void Transform_ShouldPreserveVolume_Test()
        {
            var moved = Unit().Transform(Pose.At(new Vector(10, 10, 10)));
            moved.Volume.Should().BeApproximately(1.0 / 6.0, LooseTol);
            moved.Centroid.Equals(new Vector(10.25, 10.25, 10.25), LooseTol).Should().BeTrue();
        }
    }
}
