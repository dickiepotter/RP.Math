namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Covers the cast operators added across the library: the explicit re-encoding mesh between the
    /// convention-free orientation types (Quaternion / AxisAngle / Rotation / Matrix), the implicit
    /// aggregate-tuple casts on AxisAngle and Pose, and the explicit line-family widenings.
    /// </summary>
    [TestClass]
    public class CastingTests
    {
        private const double Tol = 1e-9;

        private static Quaternion SampleRotation()
        {
            // A representative non-trivial rotation (non-axis-aligned, well away from the gimbal poles).
            return Quaternion.FromAxisAngle(new Vector(1, 2, 3), new Angle(0.7));
        }

        // ---- Rotation-representation explicit mesh ---------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_AgreeWithTheirNamedMethods()
        {
            var q = SampleRotation();
            var v = new Vector(1, 2, 3);

            ((AxisAngle)q).Equals(AxisAngle.FromQuaternion(q), Tol).Should().BeTrue();
            ((Rotation)q).Equals(Rotation.FromQuaternion(q), Tol).Should().BeTrue();
            (((Matrix)q) * v).Equals(q.ToMatrix() * v, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_PreserveTheRotation_ThroughEveryType()
        {
            var q = SampleRotation();
            var v = new Vector(2, -1, 0.5);
            var expected = q.Rotate(v);

            // Round-trip out to each representation and back to a quaternion; the action must be identical.
            ((Quaternion)(AxisAngle)q).Rotate(v).Equals(expected, Tol).Should().BeTrue();
            ((Quaternion)(Rotation)q).Rotate(v).Equals(expected, Tol).Should().BeTrue();
            ((Quaternion)(Matrix)q).Rotate(v).Equals(expected, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_ChainAcrossEveryType()
        {
            var q = SampleRotation();
            var v = new Vector(0, 3, -2);
            var expected = q.Rotate(v);

            // q -> AxisAngle -> Rotation -> Matrix -> Quaternion, all via casts.
            var aa = (AxisAngle)q;
            var rot = (Rotation)aa;
            var mat = (Matrix)rot;
            var back = (Quaternion)mat;

            back.Rotate(v).Equals(expected, Tol).Should().BeTrue();
        }

        // ---- Aggregate tuple casts -------------------------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void AxisAngle_TupleCast_In_NormalisesTheAxis()
        {
            var rawAxis = new Vector(0, 0, 5); // deliberately non-unit
            var angle = new Angle(1.1);

            var a = (AxisAngle)(rawAxis, angle); // explicit in (normalises the axis)

            a.Axis.Equals(rawAxis.NormalizeOrDefault(), Tol).Should().BeTrue("the axis is normalised on construction");
            a.Angle.Rad.Should().BeApproximately(1.1, Tol);
        }

        [TestMethod, TestCategory("Casting")]
        public void AxisAngle_TupleCast_Out_ReadsTheFields()
        {
            var a = new AxisAngle(new Vector(0, 1, 0), new Angle(0.4));

            (Vector Axis, Angle Angle) t = a; // implicit out

            t.Axis.Equals(a.Axis, Tol).Should().BeTrue();
            t.Angle.Rad.Should().BeApproximately(a.Angle.Rad, Tol);
        }

        [TestMethod, TestCategory("Casting")]
        public void Pose_TupleCast_RoundTrips()
        {
            var pos = new Vector(1, 2, 3);
            var q = SampleRotation();
            var probe = new Vector(0.5, -2, 1);

            var p = (Pose)(pos, q); // explicit in (normalises the orientation)
            p.Position.Should().Be(pos);
            p.Rotation.Rotate(probe).Equals(q.Rotate(probe), Tol).Should().BeTrue();

            (Vector Position, Quaternion Orientation) t = p; // implicit out
            t.Position.Should().Be(pos);
            t.Orientation.Rotate(probe).Equals(q.Rotate(probe), Tol).Should().BeTrue();
        }

        // ---- Line-family explicit casts --------------------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void LineSegment_CastsToLineAndRay_MatchTheNamedMethods()
        {
            var seg = new LineSegment(new Vector(1, 1, 1), new Vector(4, 1, 1));

            var line = (Line)seg;
            line.Point.Should().Be(seg.ToLine().Point);
            line.Direction.Should().Be(seg.ToLine().Direction);

            var ray = (Ray)seg;
            ray.Origin.Should().Be(seg.ToRay().Origin);
            ray.Direction.Should().Be(seg.ToRay().Direction);
        }

        [TestMethod, TestCategory("Casting")]
        public void Line_And_Ray_CastBetweenEachOther()
        {
            var line = new Line(new Vector(2, 0, 0), new Vector(1, 0, 0));
            var ray = new Ray(new Vector(2, 0, 0), new Vector(1, 0, 0));

            var asRay = (Ray)line;
            asRay.Origin.Should().Be(line.Point);
            asRay.Direction.Should().Be(line.Direction);

            var asLine = (Line)ray;
            asLine.Point.Should().Be(ray.Origin);
            asLine.Direction.Should().Be(ray.Direction);
        }
    }
}
