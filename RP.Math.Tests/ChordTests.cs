namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the <see cref="Chord"/> type (a line segment that knows its circle's radius).</summary>
    [TestClass]
    public class ChordTests
    {
        private const double Tol = 1e-9;

        // A chord of length 6 across a circle of radius 5.
        private static Chord SixAcrossFive()
        {
            return new Chord(new Vector(-3, 0, 0), new Vector(3, 0, 0), 5);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Chord(new Vector(0, 0, 0), new Vector(1, 0, 0), -1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Measurement")]
        public void Length_And_Diameter_Test()
        {
            var c = SixAcrossFive();
            c.Length.Should().BeApproximately(6, Tol);
            c.Diameter.Should().Be(10);
        }

        [TestMethod, TestCategory("Geometry")]
        public void DistanceFromCentre_And_Sagitta_Test()
        {
            var c = SixAcrossFive();
            c.DistanceFromCentre.Should().BeApproximately(4, Tol); // sqrt(25 - 9)
            c.Sagitta.Should().BeApproximately(1, Tol);            // 5 - 4
        }

        [TestMethod, TestCategory("Geometry")]
        public void CentralAngle_And_ArcLength_Test()
        {
            var c = SixAcrossFive();
            double expectedAngle = 2 * Math.Asin(3.0 / 5.0); // 2·asin((c/2)/r)
            c.CentralAngle.Rad.Should().BeApproximately(expectedAngle, Tol);
            c.ArcLength.Should().BeApproximately(5 * expectedAngle, Tol);
        }

        [TestMethod, TestCategory("Geometry")]
        public void Diameter_Chord_ShouldSubtendStraightAngle_Test()
        {
            var c = new Chord(new Vector(-5, 0, 0), new Vector(5, 0, 0), 5); // a full diameter
            c.CentralAngle.Rad.Should().BeApproximately(Math.PI, Tol);
            c.Sagitta.Should().BeApproximately(5, Tol); // the arc bulges a full radius
        }
    }
}
