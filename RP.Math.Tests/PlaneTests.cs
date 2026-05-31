namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Plane"/> type.
    /// </summary>
    [TestClass]
    public class PlaneTests
    {
        private const double Tol = 1e-9;

        #region Construction and factories

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromCoefficients_ShouldExposeThem_Test()
        {
            var p = new Plane(1, 2, 3, 4);
            p.A.Should().Be(1);
            p.B.Should().Be(2);
            p.C.Should().Be(3);
            p.D.Should().Be(4);
            p.Normal.Should().Be(new Vector(1, 2, 3));
        }

        [TestMethod, TestCategory("Construction")]
        public void FromPointNormal_ShouldHaveUnitNormalAndContainThePoint_Test()
        {
            var point = new Vector(0, 0, 5);
            var p = Plane.FromPointNormal(point, new Vector(0, 0, 10));

            p.Normal.Magnitude.Should().BeApproximately(1, Tol);
            p.Contains(point, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void FromThreePoints_ShouldContainAllThree_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(1, 0, 0);
            var c = new Vector(0, 1, 0);
            var p = Plane.FromThreePoints(a, b, c);

            p.Contains(a, Tol).Should().BeTrue();
            p.Contains(b, Tol).Should().BeTrue();
            p.Contains(c, Tol).Should().BeTrue();
            // XY plane: normal should be ±Z.
            p.UnitNormal.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void FromThreePoints_Collinear_ShouldThrow_Test()
        {
            Action act = () => Plane.FromThreePoints(new Vector(0, 0, 0), new Vector(1, 0, 0), new Vector(2, 0, 0));
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Distance, side, closest point

        [TestMethod, TestCategory("Distance")]
        public void SignedDistanceTo_ShouldBePositiveOnTheNormalSide_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            p.SignedDistanceTo(new Vector(0, 0, 5)).Should().BeApproximately(5, Tol);
            p.SignedDistanceTo(new Vector(0, 0, -5)).Should().BeApproximately(-5, Tol);
        }

        [TestMethod, TestCategory("Distance")]
        public void DistanceTo_ShouldBeUnsigned_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            p.DistanceTo(new Vector(3, 4, -7)).Should().BeApproximately(7, Tol);
        }

        [TestMethod, TestCategory("Distance")]
        public void DistanceTo_ShouldIgnoreRawNormalScale_Test()
        {
            // Same geometric plane z = 0 but with a non-unit normal.
            var p = new Plane(0, 0, 10, 0);
            p.DistanceTo(new Vector(0, 0, 4)).Should().BeApproximately(4, Tol);
        }

        [TestMethod, TestCategory("Side")]
        public void SideOf_ShouldClassifyPoints_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            p.SideOf(new Vector(0, 0, 1), Tol).Should().Be(1);
            p.SideOf(new Vector(0, 0, -1), Tol).Should().Be(-1);
            p.SideOf(new Vector(5, 5, 0), Tol).Should().Be(0);
        }

        [TestMethod, TestCategory("ClosestPoint")]
        public void ClosestPoint_ShouldDropOntoThePlane_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var closest = p.ClosestPoint(new Vector(3, 4, 9));
            closest.Equals(new Vector(3, 4, 0), Tol).Should().BeTrue();
            p.Contains(closest, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Reflect")]
        public void Reflect_ShouldMirrorThroughThePlane_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            p.Reflect(new Vector(2, 3, 5)).Equals(new Vector(2, 3, -5), Tol).Should().BeTrue();
        }

        #endregion

        #region Line intersection

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersectLine_ShouldFindCrossingPoint_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var ok = p.TryIntersectLine(new Vector(1, 2, 5), new Vector(0, 0, -1), out var hit);
            ok.Should().BeTrue();
            hit.Equals(new Vector(1, 2, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersectLine_ParallelLine_ShouldReturnFalse_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var ok = p.TryIntersectLine(new Vector(0, 0, 5), new Vector(1, 0, 0), out _);
            ok.Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersection")]
        public void IntersectLineParameter_ShouldGiveTAlongTheLine_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            // From z=5 heading -z, the plane is reached at t = 5.
            p.IntersectLineParameter(new Vector(0, 0, 5), new Vector(0, 0, -1)).Should().BeApproximately(5, Tol);
        }

        #endregion

        #region Predicates and equality

        [TestMethod, TestCategory("Predicate")]
        public void IsParallelTo_ShouldDetectParallelPlanes_Test()
        {
            var p = new Plane(0, 0, 1, 0);
            var q = new Plane(0, 0, 1, -5);    // z = 5, parallel
            p.IsParallelTo(q, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Predicate")]
        public void IsDegenerate_ShouldDetectZeroNormal_Test()
        {
            new Plane(0, 0, 0, 1).IsDegenerate().Should().BeTrue();
            new Plane(0, 0, 1, 0).IsDegenerate().Should().BeFalse();
        }

        [TestMethod, TestCategory("Equality")]
        public void GeometricEquality_ShouldIgnoreScaleAndDirection_Test()
        {
            var p = new Plane(0, 0, 1, 0);        // z = 0
            var scaled = new Plane(0, 0, 5, 0);   // same plane, scaled normal
            var flipped = new Plane(0, 0, -1, 0); // same plane, opposite normal

            p.Equals(scaled, Tol).Should().BeTrue();
            p.Equals(flipped, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Equality")]
        public void GeometricEquality_ShouldRejectDifferentPlanes_Test()
        {
            var p = new Plane(0, 0, 1, 0);    // z = 0
            var q = new Plane(0, 0, 1, -5);   // z = 5
            p.Equals(q, Tol).Should().BeFalse();
        }

        [TestMethod, TestCategory("Operator")]
        public void Negation_ShouldFlipCoefficientsButKeepTheGeometricPlane_Test()
        {
            var p = new Plane(0, 0, 1, -5);
            var n = -p;
            n.Should().Be(new Plane(0, 0, -1, 5));
            p.Equals(n, Tol).Should().BeTrue();
        }

        #endregion
    }
}
