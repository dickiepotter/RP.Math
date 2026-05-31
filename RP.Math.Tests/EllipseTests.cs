namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Ellipse"/> and the positioned <see cref="PlacedEllipse"/>.</summary>
    [TestClass]
    public class EllipseTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var e = new Ellipse(3, 2);
            e.Area.Should().BeApproximately(Math.PI * 6, Tol);
            e.SemiMajor.Should().Be(3);
            e.SemiMinor.Should().Be(2);
            e.FocalDistance.Should().BeApproximately(Math.Sqrt(5), Tol);
            e.Eccentricity.Should().BeApproximately(Math.Sqrt(5) / 3.0, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Circle_Case_Test()
        {
            var e = new Ellipse(2, 2);
            e.IsCircle(Tol).Should().BeTrue();
            e.Eccentricity.Should().BeApproximately(0, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void NegativeAxis_ShouldThrow_Test()
        {
            Action act = () => new Ellipse(-1, 2);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByArea_Test()
        {
            (new Ellipse(3, 2) > new Ellipse(1, 1)).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_OnAndBeyondBoundary_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            e.Contains(new Vector(3, 0, 0), LooseTol).Should().BeTrue();
            e.Contains(new Vector(0, 2, 0), LooseTol).Should().BeTrue();
            e.Contains(new Vector(3.1, 0, 0), Tol).Should().BeFalse();
            e.Contains(new Vector(0, 0, 0.5), Tol).Should().BeFalse(); // off the plane
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OnAxes_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            e.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(3, 0, 0), LooseTol).Should().BeTrue();
            e.ClosestPoint(new Vector(0, 10, 0)).Equals(new Vector(0, 2, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_GenericExterior_ShouldLandOnBoundary_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            Vector p = e.ClosestPoint(new Vector(10, 10, 0));
            // The returned point must satisfy the ellipse equation (x/3)^2 + (y/2)^2 = 1.
            double onBoundary = ((p.X * p.X) / 9.0) + ((p.Y * p.Y) / 4.0);
            onBoundary.Should().BeApproximately(1, LooseTol);
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_ThroughDisc_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(1, 0.5, 5), new Vector(0, 0, -1));
            e.TryIntersect(ray, out Vector p).Should().BeTrue();
            p.Equals(new Vector(1, 0.5, 0), LooseTol).Should().BeTrue();
        }
    }
}
