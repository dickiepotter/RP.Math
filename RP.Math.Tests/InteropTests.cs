namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>
    /// Tests for the cross-type interop: a <see cref="Vector"/> driven by the orientation/transform types,
    /// the <see cref="Matrix"/>↔<see cref="Quaternion"/> round trip, the <see cref="AxisAngle"/> bridges,
    /// and the line-family conversions.
    /// </summary>
    [TestClass]
    public class InteropTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static readonly Vector V = new Vector(1, 2, 3);

        #region Vector rotated by orientation types

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByQuaternion_ShouldMatchQuaternionRotate_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0, 0, 1), new Angle(Math.PI / 2));
            V.Rotate(q).Equals(q.Rotate(V), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByAxisAngle_ShouldMatch_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 0), new Angle(Math.PI / 3));
            V.Rotate(aa).Equals(aa.Rotate(V), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByRotationAndAttitude_ShouldMatch_Test()
        {
            var r = Rotation.AboutZ(new Angle(Math.PI / 2));
            V.Rotate(r).Equals(r.Rotate(V), LooseTol).Should().BeTrue();

            var a = Attitude.FromYaw(new Angle(Math.PI / 4));
            V.Rotate(a).Equals(a.Rotate(V), LooseTol).Should().BeTrue();
        }

        #endregion

        #region Vector transformed by matrix and pose

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_TransformByMatrix_ShouldEqualMatrixTimesVector_Test()
        {
            var m = Rotation.AboutX(new Angle(Math.PI / 2)).ToMatrix();
            V.Transform(m).Equals(m * V, LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_TransformByPose_ShouldEqualPoseApply_Test()
        {
            var pose = new Pose(new Vector(10, 0, 0), Rotation.AboutZ(new Angle(Math.PI / 2)));
            V.Transform(pose).Equals(pose.Apply(V), LooseTol).Should().BeTrue();
        }

        #endregion

        #region Vector angle as an Angle

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_AngleTo_ShouldMatchTheDoubleForm_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            a.AngleTo(b).Rad.Should().BeApproximately(a.Angle(b), Tol);
            a.AngleTo(b).Rad.Should().BeApproximately(Math.PI / 2, Tol);
        }

        #endregion

        #region Matrix <-> Quaternion round trip

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_ShouldInvertQuaternionToMatrix_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(1, 2, 3), new Angle(1.1));
            var back = q.ToMatrix().ToQuaternion();
            back.AngleTo(q).Rad.Should().BeApproximately(0, LooseTol);
        }

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_NearHalfTurn_ShouldStayStable_Test()
        {
            // A ~180° rotation drives the trace negative, exercising the pivot branches.
            var q = Quaternion.FromAxisAngle(new Vector(0, 1, 0), new Angle(Math.PI - 1e-3));
            var back = q.ToMatrix().ToQuaternion();
            back.AngleTo(q).Rad.Should().BeApproximately(0, LooseTol);
        }

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_OfIdentity_ShouldBeIdentity_Test()
        {
            Matrix.Identity.ToQuaternion().Equals(Quaternion.Identity, LooseTol).Should().BeTrue();
        }

        #endregion

        #region AxisAngle bridges

        [TestMethod, TestCategory("Interop")]
        public void Quaternion_FromAxisAngleType_ShouldMatchVectorAngleForm_Test()
        {
            var aa = new AxisAngle(new Vector(0, 0, 1), new Angle(0.7));
            Quaternion.FromAxisAngle(aa).Equals(Quaternion.FromAxisAngle(aa.Axis, aa.Angle), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Interop")]
        public void Pose_FromAxisAngle_ShouldRotateLikeTheAxisAngle_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 0), new Angle(Math.PI / 2));
            var pose = new Pose(new Vector(0, 0, 0), aa);
            pose.Apply(V).Equals(aa.Rotate(V), LooseTol).Should().BeTrue();
        }

        #endregion

        #region Line family conversions

        [TestMethod, TestCategory("LineFamily")]
        public void Line_ToRay_ShouldShareOriginAndDirection_Test()
        {
            var line = new Line(new Vector(1, 2, 3), new Vector(0, 0, 2));
            var ray = line.ToRay();
            ray.Origin.Equals(new Vector(1, 2, 3), Tol).Should().BeTrue();
            ray.Direction.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("LineFamily")]
        public void Ray_ToLine_ShouldShareOriginAndDirection_Test()
        {
            var ray = new Ray(new Vector(1, 2, 3), new Vector(0, 5, 0));
            var line = ray.ToLine();
            line.Point.Equals(new Vector(1, 2, 3), Tol).Should().BeTrue();
            line.Direction.Equals(new Vector(0, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("LineFamily")]
        public void Segment_ToRay_ShouldStartAtTailTowardsHead_Test()
        {
            var seg = new LineSegment(new Vector(0, 0, 0), new Vector(0, 0, 4));
            var ray = seg.ToRay();
            ray.Origin.Equals(new Vector(0, 0, 0), Tol).Should().BeTrue();
            ray.Direction.Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();
        }

        #endregion
    }
}
