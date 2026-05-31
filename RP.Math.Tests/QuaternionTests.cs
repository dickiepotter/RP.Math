namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math.Exceptions;

    /// <summary>
    /// Unit tests for the <see cref="Quaternion"/> type.
    /// </summary>
    [TestClass]
    public class QuaternionTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        #region Construction

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithComponents_ShouldExposeThem_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            q.X.Should().Be(1);
            q.Y.Should().Be(2);
            q.Z.Should().Be(3);
            q.W.Should().Be(4);
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromVectorAndScalar_ShouldSplitTheParts_Test()
        {
            var q = new Quaternion(new Vector(1, 2, 3), 4);
            q.Xyz.Should().Be(new Vector(1, 2, 3));
            q.Scalar.Should().Be(4);
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromArrayOfWrongLength_ShouldThrow_Test()
        {
            Action act = () => new Quaternion(new double[] { 1, 2, 3 });
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Construction")]
        public void Identity_ShouldBeZeroVectorWithScalarOne_Test()
        {
            Quaternion.Identity.Should().Be(new Quaternion(0, 0, 0, 1));
        }

        #endregion

        #region Magnitude

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_OfIdentity_ShouldBeOne_Test()
        {
            Quaternion.Identity.Magnitude.Should().BeApproximately(1, Tolerance);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void MagnitudeSquared_ShouldBeSumOfSquares_Test()
        {
            new Quaternion(1, 2, 3, 4).MagnitudeSquared.Should().BeApproximately(30, Tolerance);
        }

        #endregion

        #region Operators and products

        [TestMethod, TestCategory("Operator")]
        public void Addition_ShouldBeComponentWise_Test()
        {
            var sum = new Quaternion(1, 2, 3, 4) + new Quaternion(5, 6, 7, 8);
            sum.Should().Be(new Quaternion(6, 8, 10, 12));
        }

        [TestMethod, TestCategory("Operator")]
        public void ScalarMultiplication_ShouldScaleEachComponent_Test()
        {
            (new Quaternion(1, 2, 3, 4) * 2).Should().Be(new Quaternion(2, 4, 6, 8));
        }

        [TestMethod, TestCategory("Product")]
        public void IdentityProduct_ShouldLeaveQuaternionUnchanged_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            (q * Quaternion.Identity).Equals(q, Tolerance).Should().BeTrue();
            (Quaternion.Identity * q).Equals(q, Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Product")]
        public void HamiltonProduct_ShouldFollowBasisRules_Test()
        {
            // i * j = k : (1,0,0,0) * (0,1,0,0) = (0,0,1,0)
            var i = new Quaternion(1, 0, 0, 0);
            var j = new Quaternion(0, 1, 0, 0);
            (i * j).Equals(new Quaternion(0, 0, 1, 0), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_ShouldBeSumOfComponentProducts_Test()
        {
            Quaternion.DotProduct(new Quaternion(1, 2, 3, 4), new Quaternion(5, 6, 7, 8))
                .Should().BeApproximately(70, Tolerance);
        }

        #endregion

        #region Conjugate, inverse, normalize

        [TestMethod, TestCategory("Conjugate")]
        public void Conjugate_ShouldNegateTheVectorPart_Test()
        {
            new Quaternion(1, 2, 3, 4).Conjugate().Should().Be(new Quaternion(-1, -2, -3, 4));
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_TimesOriginal_ShouldBeIdentity_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            (q * q.Inverse()).Equals(Quaternion.Identity, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_ShouldProduceUnitQuaternion_Test()
        {
            new Quaternion(1, 2, 3, 4).Normalize().IsUnit(Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_OfZero_ShouldThrow_Test()
        {
            Action act = () => Quaternion.Zero.Normalize();
            act.Should().Throw<NormalizeQuaternionException>();
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_OfZero_ShouldReturnIdentity_Test()
        {
            Quaternion.Zero.NormalizeOrDefault().Should().Be(Quaternion.Identity);
        }

        #endregion

        #region Rotation

        [TestMethod, TestCategory("Rotation")]
        public void FromAxisAngle_ShouldProduceUnitQuaternion_Test()
        {
            Quaternion.FromAxisAngle(new Vector(0, 0, 1), System.Math.PI / 2)
                .IsUnit(Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void Rotate_NinetyDegreesAboutZ_ShouldTakeXToY_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0, 0, 1), System.Math.PI / 2);
            var rotated = q.Rotate(new Vector(1, 0, 0));
            rotated.Equals(new Vector(0, 1, 0), LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void Rotate_AboutAxis_ShouldPreserveMagnitude_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(1, 1, 1), 1.234);
            var v = new Vector(3, -2, 5);
            q.Rotate(v).Magnitude.Should().BeApproximately(v.Magnitude, LooseTolerance);
        }

        [TestMethod, TestCategory("Rotation")]
        public void ToMatrix_TimesVector_ShouldMatchRotate_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0.3, 0.5, 0.8), 0.9);
            var v = new Vector(1, 2, 3);

            var viaMatrix = q.ToMatrix() * v;
            var viaRotate = q.Rotate(v);

            viaMatrix.Equals(viaRotate, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void ToAxisAngle_ShouldRoundTripFromAxisAngle_Test()
        {
            var axis = new Vector(0, 0, 1);
            var q = Quaternion.FromAxisAngle(axis, System.Math.PI / 3);

            q.ToAxisAngle(out var outAxis, out var outAngle);

            outAngle.Rad.Should().BeApproximately(System.Math.PI / 3, LooseTolerance);
            outAxis.Equals(axis, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void ComposedRotations_ShouldEqualSingleRotationOfSumOfAngles_Test()
        {
            var z = new Vector(0, 0, 1);
            var q45 = Quaternion.FromAxisAngle(z, System.Math.PI / 4);
            var q90 = Quaternion.FromAxisAngle(z, System.Math.PI / 2);

            (q45 * q45).Equals(q90, LooseTolerance).Should().BeTrue();
        }

        #endregion

        #region Interpolation

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_AtZero_ShouldReturnTheFirstQuaternion_Test()
        {
            var a = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 0.2);
            var b = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 1.5);
            Quaternion.Slerp(a, b, 0).Equals(a, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_AtOne_ShouldReturnTheSecondQuaternion_Test()
        {
            var a = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 0.2);
            var b = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 1.5);
            Quaternion.Slerp(a, b, 1).Equals(b, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_Midpoint_ShouldBeHalfwayRotation_Test()
        {
            var z = new Vector(0, 0, 1);
            var a = Quaternion.FromAxisAngle(z, 0);
            var b = Quaternion.FromAxisAngle(z, System.Math.PI / 2);

            var mid = Quaternion.Slerp(a, b, 0.5);
            var expected = Quaternion.FromAxisAngle(z, System.Math.PI / 4);

            mid.Equals(expected, LooseTolerance).Should().BeTrue();
        }

        #endregion

        #region Equality, formatting, deconstruction

        [TestMethod, TestCategory("Equality")]
        public void Equality_WithTolerance_ShouldIgnoreTinyDifferences_Test()
        {
            new Quaternion(1, 2, 3, 4)
                .Equals(new Quaternion(1 + 1e-12, 2, 3, 4), Tolerance)
                .Should().BeTrue();
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToString_ShouldListFourComponents_Test()
        {
            new Quaternion(1, 2, 3, 4)
                .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                .Should().Be("(1.0, 2.0, 3.0, 4.0)");
        }

        [TestMethod, TestCategory("Conversion")]
        public void Deconstruct_ShouldYieldComponents_Test()
        {
            var (x, y, z, w) = new Quaternion(1, 2, 3, 4);
            x.Should().Be(1);
            y.Should().Be(2);
            z.Should().Be(3);
            w.Should().Be(4);
        }

        [TestMethod, TestCategory("Conversion")]
        public void ImplicitTupleConversion_ShouldRoundTrip_Test()
        {
            Quaternion q = (1.0, 2.0, 3.0, 4.0);
            q.Should().Be(new Quaternion(1, 2, 3, 4));
        }

        #endregion
    }
}
