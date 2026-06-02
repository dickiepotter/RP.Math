namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for <see cref="Plane.TryIntersect(Plane, out Line)"/>.</summary>
    [TestClass]
    public class PlanePlaneIntersectionTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        /// <summary>Assert that an infinite line lies in a plane (two of its points are on the plane).</summary>
        private static void LineShouldLieIn(Plane plane, Line line)
        {
            plane.Contains(line.Point, LooseTol).Should().BeTrue();
            plane.Contains(line.PointAt(5), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_XYWithXZ_GivesTheXAxis_Test()
        {
            Plane.XY.TryIntersect(Plane.XZ, out Line line).Should().BeTrue();

            // The line of intersection lies in both planes...
            LineShouldLieIn(Plane.XY, line);
            LineShouldLieIn(Plane.XZ, line);

            // ...and runs along the X axis (parallel to it, either sense).
            line.Direction.CrossProduct(Vector.XAxis).Magnitude.Should().BeApproximately(0, LooseTol);
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_OffsetAxisPlanes_GivesTheExpectedLine_Test()
        {
            var x2 = Plane.FromPointNormal(new Vector(2, 0, 0), new Vector(1, 0, 0)); // x = 2
            var y3 = Plane.FromPointNormal(new Vector(0, 3, 0), new Vector(0, 1, 0)); // y = 3

            x2.TryIntersect(y3, out Line line).Should().BeTrue();

            LineShouldLieIn(x2, line);
            LineShouldLieIn(y3, line);

            // The line is { x = 2, y = 3, z free }: parallel to Z and passing through (2, 3, 0).
            line.Direction.CrossProduct(Vector.ZAxis).Magnitude.Should().BeApproximately(0, LooseTol);
            line.Contains(new Vector(2, 3, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_ObliquePlanes_LineLiesInBoth_Test()
        {
            var p1 = Plane.FromPointNormal(new Vector(1, 0, 0), new Vector(1, 1, 0));
            var p2 = Plane.FromPointNormal(new Vector(0, 0, 2), new Vector(0, 1, 2));

            p1.TryIntersect(p2, out Line line).Should().BeTrue();

            LineShouldLieIn(p1, line);
            LineShouldLieIn(p2, line);

            // Direction is perpendicular to both normals.
            line.Direction.DotProduct(p1.Normal).Should().BeApproximately(0, LooseTol);
            line.Direction.DotProduct(p2.Normal).Should().BeApproximately(0, LooseTol);
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_ParallelPlanes_ReturnsFalse_Test()
        {
            var z5 = Plane.FromPointNormal(new Vector(0, 0, 5), new Vector(0, 0, 1));
            Plane.XY.TryIntersect(z5, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_CoincidentPlanes_ReturnsFalse_Test()
        {
            // Same surface, differently scaled coefficients — still parallel, so no single line.
            Plane.XY.TryIntersect(new Plane(0, 0, 5, 0), out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_OppositelyOrientedSamePlane_ReturnsFalse_Test()
        {
            Plane.XY.TryIntersect(-Plane.XY, out _).Should().BeFalse();
        }
    }
}
