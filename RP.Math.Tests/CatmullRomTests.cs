namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="CatmullRom"/> spline.</summary>
    [TestClass]
    public class CatmullRomTests
    {
        private const double Tolerance = 1e-9;

        private static CatmullRom Sample()
        {
            return new CatmullRom(
                new Vector(0, 0, 0),
                new Vector(1, 1, 0),
                new Vector(2, 0, 0),
                new Vector(3, 1, 0));
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithFewerThanTwoPoints_Throws_Test()
        {
            Action act = () => new CatmullRom(new Vector(0, 0, 0));
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Accessors")]
        public void SegmentCount_IsOneLessThanWaypointCount_Test()
        {
            Sample().SegmentCount.Should().Be(3);
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_PassesThroughEveryWaypoint_Test()
        {
            CatmullRom spline = Sample();
            int segments = spline.SegmentCount;

            for (int i = 0; i < spline.Count; i++)
            {
                double t = (double)i / segments;
                spline.PointAt(t).Equals(spline[i], Tolerance).Should().BeTrue();
            }
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_AtTheEnds_ReturnsStartAndEnd_Test()
        {
            CatmullRom spline = Sample();
            spline.PointAt(0).Equals(spline.Start, Tolerance).Should().BeTrue();
            spline.PointAt(1).Equals(spline.End, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_ClampsBeyondTheEnds_Test()
        {
            CatmullRom spline = Sample();
            spline.PointAt(-0.5).Equals(spline.Start, Tolerance).Should().BeTrue();
            spline.PointAt(1.5).Equals(spline.End, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Segments")]
        public void Segment_UsesCatmullRomTangents_Test()
        {
            CatmullRom spline = Sample();

            // The tangent at an interior waypoint is half the vector between its neighbours.
            // Segment 1 runs from waypoint 1 to waypoint 2; its start tangent is (P2 - P0)/2.
            Hermite middle = spline.Segment(1);
            Vector expectedStartTangent = (spline[2] - spline[0]) * 0.5;
            middle.StartTangent.Equals(expectedStartTangent, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Segments")]
        public void Segment_OutOfRange_Throws_Test()
        {
            Action act = () => Sample().Segment(3);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
