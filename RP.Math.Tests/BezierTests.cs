namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Bezier"/> curve.</summary>
    [TestClass]
    public class BezierTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithFewerThanTwoPoints_Throws_Test()
        {
            Action act = () => new Bezier(new Vector(0, 0, 0));
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Accessors")]
        public void Degree_IsOneLessThanControlPointCount_Test()
        {
            Bezier.Cubic(Vector.Zero, Vector.XAxis, Vector.YAxis, Vector.ZAxis).Degree.Should().Be(3);
            Bezier.Quadratic(Vector.Zero, Vector.XAxis, Vector.YAxis).Degree.Should().Be(2);
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_AtEnds_ReturnsStartAndEnd_Test()
        {
            var b = Bezier.Cubic(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(3, 2, 0), new Vector(4, 0, 0));
            b.PointAt(0).Equals(b.Start, Tolerance).Should().BeTrue();
            b.PointAt(1).Equals(b.End, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_OnLinearBezier_IsTheMidpoint_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(4, 0, 0));
            b.PointAt(0.5).Equals(new Vector(2, 0, 0), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_OnSymmetricQuadratic_HitsTheExpectedHeight_Test()
        {
            // The midpoint of a quadratic is the average of the midpoint of the chord and the control point.
            var b = Bezier.Quadratic(new Vector(0, 0, 0), new Vector(2, 4, 0), new Vector(4, 0, 0));
            b.PointAt(0.5).Equals(new Vector(2, 2, 0), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void Tangent_OnLinearBezier_PointsAlongTheChord_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(4, 0, 0));
            // Derivative of a linear Bézier is constant: (P1 - P0) once (degree 1).
            b.Tangent(0.3).Equals(new Vector(4, 0, 0), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Length")]
        public void Length_OfLinearBezier_IsTheChordLength_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(3, 4, 0));
            b.Length(16).Should().BeApproximately(5, LooseTolerance);
        }

        [TestMethod, TestCategory("Length")]
        public void Length_WithZeroSegments_Throws_Test()
        {
            Action act = () => new Bezier(Vector.Zero, Vector.XAxis).Length(0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
