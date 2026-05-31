namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Pose"/> type (position + orientation, quaternion-backed).
    /// </summary>
    [TestClass]
    public class PoseTests
    {
        private const double Tol = 1e-6;

        private static Angle Deg(double d) => new Angle(d, AngleUnits.DEG);

        #region Construction

        [TestMethod, TestCategory("Construction")]
        public void Construction_ShouldStoreNormalizedRotation_Test()
        {
            var p = new Pose(new Vector(1, 2, 3), new Quaternion(0, 0, 0, 5));
            p.Rotation.IsUnit(Tol).Should().BeTrue();
            p.Position.Should().Be(new Vector(1, 2, 3));
        }

        [TestMethod, TestCategory("Construction")]
        public void Identity_ShouldBeOriginWithNoRotation_Test()
        {
            Pose.Identity.Position.Should().Be(new Vector(0, 0, 0));
            Pose.Identity.Rotation.Equals(Quaternion.Identity, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromRotation_ShouldMatchQuaternionForm_Test()
        {
            var rot = Rotation.AboutZ(Deg(90));
            var fromRotation = new Pose(new Vector(1, 0, 0), rot);
            var fromQuaternion = new Pose(new Vector(1, 0, 0), rot.ToQuaternion());
            fromRotation.Equals(fromQuaternion, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromAttitude_ShouldMatchQuaternionForm_Test()
        {
            var att = new Attitude(Deg(30), Deg(20), Deg(10));
            var fromAttitude = new Pose(new Vector(2, 1, 0), att);
            var fromQuaternion = new Pose(new Vector(2, 1, 0), att.ToQuaternion());
            fromAttitude.Equals(fromQuaternion, Tol).Should().BeTrue();
        }

        #endregion

        #region Transform application

        [TestMethod, TestCategory("Apply")]
        public void Apply_WithIdentity_ShouldReturnThePointUnchanged_Test()
        {
            Pose.Identity.Apply(new Vector(3, 4, 5)).Equals(new Vector(3, 4, 5), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Apply")]
        public void Apply_ShouldRotateThenTranslate_Test()
        {
            // 90 deg about Z takes local X (1,0,0) to (0,1,0), then translate by (10,0,0) -> (10,1,0).
            var pose = new Pose(new Vector(10, 0, 0), Rotation.AboutZ(Deg(90)));
            pose.Apply(new Vector(1, 0, 0)).Equals(new Vector(10, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Apply")]
        public void ApplyDirection_ShouldRotateButNotTranslate_Test()
        {
            var pose = new Pose(new Vector(10, 20, 30), Rotation.AboutZ(Deg(90)));
            pose.ApplyDirection(new Vector(1, 0, 0)).Equals(new Vector(0, 1, 0), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Apply")]
        public void ApplyInverse_ShouldUndoApply_Test()
        {
            var pose = new Pose(new Vector(10, -5, 2), Rotation.AboutZ(Deg(37)));
            var p = new Vector(1, 2, 3);
            pose.ApplyInverse(pose.Apply(p)).Equals(p, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Apply")]
        public void ToMatrix_TimesPoint_ShouldMatchApply_Test()
        {
            var pose = new Pose(new Vector(4, -3, 7), new Attitude(Deg(25), Deg(15), Deg(40)));
            var p = new Vector(1, 2, 3);
            (pose.ToMatrix() * p).Equals(pose.Apply(p), Tol).Should().BeTrue();
        }

        #endregion

        #region Composition and inverse

        [TestMethod, TestCategory("Compose")]
        public void Compose_ShouldEqualNestedApply_Test()
        {
            var outer = new Pose(new Vector(1, 2, 3), Rotation.AboutZ(Deg(30)));
            var inner = new Pose(new Vector(-2, 5, 1), Rotation.AboutX(Deg(50)));
            var p = new Vector(2, -1, 4);

            outer.Compose(inner).Apply(p).Equals(outer.Apply(inner.Apply(p)), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Compose")]
        public void PoseTimesInverse_ShouldBeIdentity_Test()
        {
            var pose = new Pose(new Vector(10, -5, 2), new Attitude(Deg(20), Deg(35), Deg(50)));
            var p = new Vector(3, 4, 5);
            (pose * pose.Inverse()).Apply(p).Equals(p, Tol).Should().BeTrue();
        }

        #endregion

        #region Round-trip orientation views

        [TestMethod, TestCategory("Conversion")]
        public void RotationAsEuler_ShouldRoundTrip_Test()
        {
            var euler = new Rotation(Deg(15), Deg(40), Deg(20));
            var pose = new Pose(new Vector(0, 0, 0), euler);
            pose.RotationAsEuler.Equals(euler, Tol).Should().BeTrue();
        }

        #endregion

        #region Modification and equality

        [TestMethod, TestCategory("Modification")]
        public void Translate_ShouldMovePositionButKeepRotation_Test()
        {
            var pose = new Pose(new Vector(1, 1, 1), Rotation.AboutZ(Deg(90)));
            var moved = pose.Translate(new Vector(2, 0, 0));
            moved.Position.Should().Be(new Vector(3, 1, 1));
            moved.Rotation.Equals(pose.Rotation, Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Conversion")]
        public void Deconstruct_ShouldYieldPositionAndRotation_Test()
        {
            var pose = new Pose(new Vector(1, 2, 3), Quaternion.Identity);
            var (position, rotation) = pose;
            position.Should().Be(new Vector(1, 2, 3));
            rotation.Equals(Quaternion.Identity, Tol).Should().BeTrue();
        }

        #endregion
    }
}
