namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>
    /// Unit tests for the <see cref="AxisAngle"/> type.
    /// </summary>
    [TestClass]
    public class AxisAngleTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        #region Construction

        [TestMethod, TestCategory("Construction")]
        public void Construction_ShouldNormalizeTheAxis_Test()
        {
            var aa = new AxisAngle(new Vector(0, 0, 5), new Angle(1));
            aa.Axis.Magnitude.Should().BeApproximately(1, Tolerance);
            aa.Axis.Equals(new Vector(0, 0, 1), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Construction")]
        public void Identity_ShouldHaveZeroAngle_Test()
        {
            AxisAngle.Identity.Angle.Rad.Should().BeApproximately(0, Tolerance);
        }

        #endregion

        #region Rotation of a vector (Rodrigues)

        [TestMethod, TestCategory("Rotate")]
        public void Rotate_QuarterTurnAboutZ_ShouldTakeXToY_Test()
        {
            var aa = new AxisAngle(new Vector(0, 0, 1), new Angle(Math.PI / 2));
            aa.Rotate(new Vector(1, 0, 0)).Equals(new Vector(0, 1, 0), LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotate")]
        public void Rotate_ShouldMatchTheEquivalentQuaternion_Test()
        {
            var aa = new AxisAngle(new Vector(1, 2, 3), new Angle(0.9));
            var v = new Vector(4, -1, 2);

            aa.Rotate(v).Equals(aa.ToQuaternion().Rotate(v), LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotate")]
        public void Rotate_AboutTheAxis_ShouldLeaveTheAxisUnchanged_Test()
        {
            var axis = new Vector(1, 2, 3);
            var aa = new AxisAngle(axis, new Angle(1.234));
            aa.Rotate(axis).Equals(axis, LooseTolerance).Should().BeTrue();
        }

        #endregion

        #region Inverse

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_ThenRotate_ShouldReturnTheOriginalVector_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 1), new Angle(0.7));
            var v = new Vector(3, 4, 5);

            aa.Inverse().Rotate(aa.Rotate(v)).Equals(v, LooseTolerance).Should().BeTrue();
        }

        #endregion

        #region Conversions round-trip

        [TestMethod, TestCategory("Conversion")]
        public void FromQuaternion_ThenToQuaternion_ShouldRoundTrip_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(1, -2, 0.5), new Angle(1.1));
            var aa = AxisAngle.FromQuaternion(q);

            // Same represented rotation (quaternion double-cover means the components may differ in sign).
            aa.ToQuaternion().AngleTo(q).Rad.Should().BeApproximately(0, LooseTolerance);
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToRotation_ShouldAgreeWithRotatingDirectly_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 0), new Angle(Math.PI / 3));
            var v = new Vector(1, 2, 3);

            aa.ToRotation().Rotate(v).Equals(aa.Rotate(v), LooseTolerance).Should().BeTrue();
        }

        #endregion

        #region Equality

        [TestMethod, TestCategory("Equality")]
        public void EqualsWithTolerance_ShouldTreatNegatedAxisAndAngleAsTheSameRotation_Test()
        {
            var aa = new AxisAngle(new Vector(0, 0, 1), new Angle(0.5));
            var flipped = new AxisAngle(new Vector(0, 0, -1), new Angle(-0.5));

            // Same physical rotation expressed two ways.
            aa.Equals(flipped, LooseTolerance).Should().BeTrue();

            // ...but the raw fields differ, so plain Equals does not.
            aa.Equals(flipped).Should().BeFalse();
        }

        #endregion
    }
}
