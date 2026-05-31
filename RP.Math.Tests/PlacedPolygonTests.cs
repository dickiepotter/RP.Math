namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the vertex-defined <see cref="PlacedPolygon"/> type.</summary>
    [TestClass]
    public class PlacedPolygonTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        // A 2x2 square in the XY plane.
        private static PlacedPolygon Square()
        {
            return new PlacedPolygon(
                new Vector(0, 0, 0),
                new Vector(2, 0, 0),
                new Vector(2, 2, 0),
                new Vector(0, 2, 0));
        }

        [TestMethod, TestCategory("Construction")]
        public void TooFewVertices_ShouldThrow_Test()
        {
            Action act = () => new PlacedPolygon(new Vector(0, 0, 0), new Vector(1, 0, 0));
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Area_Perimeter_Normal_Centroid_Test()
        {
            var s = Square();
            s.Area.Should().BeApproximately(4, Tol);
            s.Perimeter.Should().BeApproximately(8, Tol);
            s.Normal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
            s.Centroid.Equals(new Vector(1, 1, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_Test()
        {
            var s = Square();
            s.Contains(new Vector(1, 1, 0), Tol).Should().BeTrue();
            s.Contains(new Vector(3, 1, 0), Tol).Should().BeFalse();
            s.Contains(new Vector(1, 1, 0.5), Tol).Should().BeFalse(); // off the plane
        }

        [TestMethod, TestCategory("Containment")]
        public void Concave_Convexity_And_Containment_Test()
        {
            // A square with a triangular notch cut into the top edge down to (2, 2).
            var concave = new PlacedPolygon(
                new Vector(0, 0, 0),
                new Vector(4, 0, 0),
                new Vector(4, 4, 0),
                new Vector(2, 2, 0),
                new Vector(0, 4, 0));
            concave.IsConvex(Tol).Should().BeFalse();
            concave.Contains(new Vector(2, 1, 0), Tol).Should().BeTrue();  // below the notch
            concave.Contains(new Vector(2, 3, 0), Tol).Should().BeFalse(); // inside the notch (outside the polygon)
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_OnEdge_Test()
        {
            Square().ClosestPoint(new Vector(3, 1, 0)).Equals(new Vector(2, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersect_ThroughFace_Test()
        {
            var ray = new Ray(new Vector(1, 1, 5), new Vector(0, 0, -1));
            Square().TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 1, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Transformation")]
        public void Transform_ShouldMoveVertices_Test()
        {
            var moved = Square().Translate(new Vector(0, 0, 5));
            moved[0].Equals(new Vector(0, 0, 5), Tol).Should().BeTrue();
            moved.Area.Should().BeApproximately(4, Tol);
        }
    }
}
