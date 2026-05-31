namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Sphere"/> and the positioned <see cref="PlacedSphere"/>.</summary>
    [TestClass]
    public class SphereTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var s = new Sphere(2);
            s.Volume.Should().BeApproximately((4.0 / 3.0) * Math.PI * 8, Tol);
            s.SurfaceArea.Should().BeApproximately(4 * Math.PI * 4, Tol);
            s.Diameter.Should().Be(4);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Unit_And_NegativeRadius_Test()
        {
            Sphere.Unit.Radius.Should().Be(1);
            Action act = () => new Sphere(-1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByVolume_Test()
        {
            (new Sphere(3) > new Sphere(2)).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            s.Contains(new Vector(1, 0, 0)).Should().BeTrue();
            s.Contains(new Vector(2, 0, 0)).Should().BeTrue();   // on the surface
            s.Contains(new Vector(3, 0, 0)).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestSurfacePoint_And_SignedDistance_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            s.ClosestSurfacePoint(new Vector(10, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
            s.SignedDistanceTo(new Vector(5, 0, 0)).Should().BeApproximately(3, Tol);
            s.SignedDistanceTo(new Vector(1, 0, 0)).Should().BeApproximately(-1, Tol); // inside
        }

        [TestMethod, TestCategory("Placed")]
        public void Intersects_OtherSphere_Test()
        {
            var a = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var near = PlacedSphere.At(new Sphere(2), new Vector(3, 0, 0));
            var far = PlacedSphere.At(new Sphere(2), new Vector(10, 0, 0));
            a.Intersects(near).Should().BeTrue();
            a.Intersects(far).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_FromOutsideAndInside_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            s.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0)), out Vector p).Should().BeTrue();
            p.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();

            s.TryIntersect(new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector exit).Should().BeTrue();
            exit.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();

            s.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(1, 0, 0)), out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_BothCrossings_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            s.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-2, 0, 0), LooseTol).Should().BeTrue();
            far.Equals(new Vector(2, 0, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void Translate_MovesCentre_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0)).Translate(new Vector(1, 2, 3));
            s.Center.Equals(new Vector(1, 2, 3), Tol).Should().BeTrue();
        }
    }
}
