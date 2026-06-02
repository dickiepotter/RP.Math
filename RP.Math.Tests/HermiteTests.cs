namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Hermite"/> curve segment.</summary>
    [TestClass]
    public class HermiteTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_AtEnds_ReturnsTheEndpoints_Test()
        {
            var h = new Hermite(new Vector(0, 0, 0), new Vector(1, 0, 0), new Vector(2, 2, 0), new Vector(0, 1, 0));
            h.PointAt(0).Equals(new Vector(0, 0, 0), Tolerance).Should().BeTrue();
            h.PointAt(1).Equals(new Vector(2, 2, 0), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void Tangent_AtEnds_ReturnsTheGivenTangents_Test()
        {
            var startTangent = new Vector(1, 0, 0);
            var endTangent = new Vector(0, 1, 0);
            var h = new Hermite(new Vector(0, 0, 0), startTangent, new Vector(2, 2, 0), endTangent);

            h.Tangent(0).Equals(startTangent, Tolerance).Should().BeTrue();
            h.Tangent(1).Equals(endTangent, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_WithTangentsMatchingTheChord_TracesAStraightLine_Test()
        {
            // Tangents equal to (P1 - P0) make the cubic reduce to the straight segment between the points.
            var p0 = new Vector(0, 0, 0);
            var p1 = new Vector(6, 3, 0);
            var chord = p1 - p0;
            var h = new Hermite(p0, chord, p1, chord);

            h.PointAt(0.5).Equals(new Vector(3, 1.5, 0), Tolerance).Should().BeTrue();
        }
    }
}
