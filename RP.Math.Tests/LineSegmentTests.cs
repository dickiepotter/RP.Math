namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the finite <see cref="LineSegment"/> type.</summary>
    [TestClass]
    public class LineSegmentTests
    {
        private const double Tol = 1e-9;

        private static LineSegment ThreeFourFive()
        {
            return new LineSegment(new Vector(0, 0, 0), new Vector(3, 4, 0));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Length_Midpoint_Direction_Test()
        {
            var s = ThreeFourFive();
            s.Length.Should().BeApproximately(5, Tol);
            s.Midpoint.Equals(new Vector(1.5, 2, 0), Tol).Should().BeTrue();
            s.Direction.Equals(new Vector(0.6, 0.8, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Points")]
        public void PointAt_ShouldClampToEnds_Test()
        {
            var s = ThreeFourFive();
            s.PointAt(0.25).Equals(new Vector(0.75, 1, 0), Tol).Should().BeTrue();
            s.PointAt(-1).Equals(new Vector(0, 0, 0), Tol).Should().BeTrue(); // clamped to the tail
            s.PointAt(2).Equals(new Vector(3, 4, 0), Tol).Should().BeTrue();   // clamped to the head
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_ShouldClampToSegment_Test()
        {
            var s = ThreeFourFive();
            s.ClosestPointTo(new Vector(10, 10, 0)).Equals(new Vector(3, 4, 0), Tol).Should().BeTrue(); // beyond the head
            s.ClosestPointTo(new Vector(-5, -5, 0)).Equals(new Vector(0, 0, 0), Tol).Should().BeTrue(); // before the tail
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToLine_And_ToRay_Test()
        {
            var s = ThreeFourFive();
            s.ToLine().Contains(new Vector(6, 8, 0), Tol).Should().BeTrue(); // the unbounded line keeps going
            s.ToRay().Origin.Equals(new Vector(0, 0, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Producing")]
        public void Reversed_And_Translate_Test()
        {
            var s = ThreeFourFive();
            var r = s.Reversed();
            r.Tail.Equals(new Vector(3, 4, 0), Tol).Should().BeTrue();
            r.Head.Equals(new Vector(0, 0, 0), Tol).Should().BeTrue();

            var moved = s.Translate(new Vector(1, 1, 1));
            moved.Tail.Equals(new Vector(1, 1, 1), Tol).Should().BeTrue();
            moved.Head.Equals(new Vector(4, 5, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Producing")]
        public void RotateZ_ShouldPivotAboutTailAndPreserveLength_Test()
        {
            var s = new LineSegment(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var rotated = s.RotateZ(new Angle(90, AngleUnits.DEG));
            rotated.Tail.Equals(new Vector(0, 0, 0), Tol).Should().BeTrue(); // tail is the pivot
            rotated.Length.Should().BeApproximately(1, Tol);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_ShouldBlendEnds_Test()
        {
            var s = ThreeFourFive();
            s.Interpolate(0.5).Equals(new Vector(1.5, 2, 0), Tol).Should().BeTrue();
        }
    }
}
