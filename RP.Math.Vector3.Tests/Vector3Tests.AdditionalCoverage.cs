namespace RP.Math.Tests
{
    using System;
    using System.Globalization;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;

    /// <summary>
    /// Additional coverage for public members that previously had no unit tests:
    /// Interpolate, Min/Max, Distance, MixedProduct, IsBackFace, the component sums,
    /// the indexer / Array accessor, the scalar operators and the string formatters.
    /// </summary>
    public partial class Vector3Tests
    {
        #region Interpolate Tests

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlZero_ShouldReturnTheFirstVector_Test()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(7, 8, 9);

            var result = Vector3.Interpolate(a, b, 0);

            result.Should().Be(a);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlOne_ShouldReturnTheSecondVector_Test()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(7, 8, 9);

            var result = Vector3.Interpolate(a, b, 1);

            result.Should().Be(b);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlHalf_ShouldReturnTheMidpoint_Test()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(4, 8, 12);

            var result = a.Interpolate(b, 0.5);

            result.X.Should().Be(2);
            result.Y.Should().Be(4);
            result.Z.Should().Be(6);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_WithControlAboveOneAndNoExtrapolation_ShouldThrow_Test()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(2, 2, 2);

            Action act = () => Vector3.Interpolate(a, b, 1.5);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_StaticWithControlAboveOneAndExtrapolationAllowed_ShouldExtrapolate_Test()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(2, 2, 2);

            var result = Vector3.Interpolate(a, b, 1.5, true);

            result.X.Should().Be(3, "extrapolating to t = 1.5 along (0,0,0)->(2,2,2) gives (3,3,3)");
            result.Y.Should().Be(3);
            result.Z.Should().Be(3);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_InstanceWithControlAboveOneAndExtrapolationAllowed_ShouldExtrapolate_Test()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(2, 2, 2);

            // The instance overload should honour allowExtrapolation just like the static one.
            var result = a.Interpolate(b, 1.5, true);

            result.X.Should().Be(3);
            result.Y.Should().Be(3);
            result.Z.Should().Be(3);
        }

        #endregion

        #region Min and Max Tests

        [TestMethod, TestCategory("MinMax")]
        public void Max_ShouldReturnTheVectorWithTheGreaterMagnitude_Test()
        {
            var shorter = new Vector3(1, 0, 0);   // magnitude 1
            var longer = new Vector3(3, 4, 0);    // magnitude 5

            Vector3.Max(shorter, longer).Should().Be(longer);
            longer.Max(shorter).Should().Be(longer);
        }

        [TestMethod, TestCategory("MinMax")]
        public void Min_ShouldReturnTheVectorWithTheLesserMagnitude_Test()
        {
            var shorter = new Vector3(1, 0, 0);   // magnitude 1
            var longer = new Vector3(3, 4, 0);    // magnitude 5

            Vector3.Min(shorter, longer).Should().Be(shorter);
            longer.Min(shorter).Should().Be(shorter);
        }

        #endregion

        #region Distance Tests

        [TestMethod, TestCategory("Distance")]
        public void Distance_BetweenTwoVectors_ShouldApplyPythagoras_Test()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(3, 4, 0);

            Vector3.Distance(a, b).Should().Be(5);
            a.Distance(b).Should().Be(5);
        }

        [TestMethod, TestCategory("Distance")]
        public void Distance_FromAVectorToItself_ShouldBeZero_Test()
        {
            var a = new Vector3(7, -2, 5);

            a.Distance(a).Should().Be(0);
        }

        #endregion

        #region Mixed (triple) Product Tests

        [TestMethod, TestCategory("Product")]
        public void MixedProduct_OfTheUnitAxes_ShouldBeOne_Test()
        {
            // X . (Y x Z) == 1 : the signed unit volume of the standard basis.
            Vector3.MixedProduct(Vector3.XAxis, Vector3.YAxis, Vector3.ZAxis).Should().Be(1);
            Vector3.XAxis.MixedProduct(Vector3.YAxis, Vector3.ZAxis).Should().Be(1);
        }

        [TestMethod, TestCategory("Product")]
        public void MixedProduct_OfCoplanarVectors_ShouldBeZero_Test()
        {
            var a = new Vector3(1, 0, 0);
            var b = new Vector3(0, 1, 0);
            var c = new Vector3(1, 1, 0); // lies in the XY plane with a and b

            Vector3.MixedProduct(a, b, c).Should().Be(0, "coplanar vectors span no volume");
        }

        #endregion

        #region IsBackFace Tests

        [TestMethod, TestCategory("BackFace")]
        public void IsBackFace_WhereNormalFacesAwayFromTheLineOfSight_ShouldBeTrue_Test()
        {
            var normal = new Vector3(0, 0, 1);
            var lineOfSight = new Vector3(0, 0, -1); // normal . sight = -1 < 0

            Vector3.IsBackFace(normal, lineOfSight).Should().BeTrue();
            normal.IsBackFace(lineOfSight).Should().BeTrue();
        }

        [TestMethod, TestCategory("BackFace")]
        public void IsBackFace_WhereNormalFacesTowardsTheLineOfSight_ShouldBeFalse_Test()
        {
            var normal = new Vector3(0, 0, 1);
            var lineOfSight = new Vector3(0, 0, 1); // normal . sight = 1, not < 0

            normal.IsBackFace(lineOfSight).Should().BeFalse();
        }

        #endregion

        #region Component Sum Tests

        [TestMethod, TestCategory("Components")]
        public void SumComponents_ShouldAddTheThreeComponents_Test()
        {
            new Vector3(2, 3, 4).SumComponents().Should().Be(9);
        }

        [TestMethod, TestCategory("Components")]
        public void SumComponentSqrs_ShouldAddTheSquaresOfTheComponents_Test()
        {
            new Vector3(2, 3, 4).SumComponentSqrs().Should().Be(29); // 4 + 9 + 16
        }

        #endregion

        #region Indexer and Array Accessor Tests

        [TestMethod, TestCategory("Accessors")]
        public void Indexer_ShouldMapZeroOneTwoToXYZ_Test()
        {
            var vector = new Vector3(10, 20, 30);

            vector[0].Should().Be(10);
            vector[1].Should().Be(20);
            vector[2].Should().Be(30);
        }

        [TestMethod, TestCategory("Accessors")]
        public void Indexer_WithAnOutOfRangeIndex_ShouldThrow_Test()
        {
            var vector = new Vector3(10, 20, 30);

            Action act = () => { var unused = vector[3]; };

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Accessors")]
        public void Array_ShouldReturnTheComponentsInXYZOrder_Test()
        {
            var vector = new Vector3(10, 20, 30);

            vector.Array.Should().Equal(10d, 20d, 30d);
        }

        #endregion

        #region Scalar Operator Tests

        [TestMethod, TestCategory("Operator")]
        public void MultiplyByScalar_ShouldBeCommutativeAndScaleEachComponent_Test()
        {
            var vector = new Vector3(1, 2, 3);

            (vector * 2).Should().Be(new Vector3(2, 4, 6));
            (2 * vector).Should().Be(new Vector3(2, 4, 6), "scalar multiplication is commutative");
        }

        [TestMethod, TestCategory("Operator")]
        public void DivideByScalar_ShouldDivideEachComponent_Test()
        {
            var vector = new Vector3(2, 4, 6);

            var result = vector / 2;

            result.X.Should().Be(1);
            result.Y.Should().Be(2);
            result.Z.Should().Be(3);
        }

        #endregion

        #region ToString Tests

        [TestMethod, TestCategory("Formatting")]
        public void ToString_WithNoFormat_ShouldListTheComponents_Test()
        {
            new Vector3(1, 2, 3).ToString().Should().Be("(1, 2, 3)");
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToString_WithAComponentSelector_ShouldReturnThatComponent_Test()
        {
            var vector = new Vector3(1, 2, 3);

            vector.ToString("x", CultureInfo.InvariantCulture).Should().Be("1");
            vector.ToString("y", CultureInfo.InvariantCulture).Should().Be("2");
            vector.ToString("z", CultureInfo.InvariantCulture).Should().Be("3");
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToVerbString_ShouldDescribeTheComponents_Test()
        {
            var description = new Vector3(1, 2, 3).ToVerbString();

            description.Should().Contain("x=1").And.Contain("y=2").And.Contain("z=3");
        }

        #endregion

        #region Offset (arbitrary line) Rotation Tests

        // These cover the RotateX/Y/Z overloads with NON-ZERO offsets, i.e. rotation about a line
        // parallel to an axis but passing through the offset point (the zero-offset case is already
        // covered in the main Rotation Tests region).

        [TestMethod, TestCategory("Rotation")]
        public void RotateX_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to X through (y=1, z=0).
            var result = Vector3.RotateX(new Vector3(0, 0, 0), 1, 0, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(0);
            Math.Round(result.Y, 6).Should().Be(1);
            Math.Round(result.Z, 6).Should().Be(-1);
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateY_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to Y through (x=1, z=0).
            var result = Vector3.RotateY(new Vector3(0, 0, 0), 1, 0, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(1);
            Math.Round(result.Y, 6).Should().Be(0);
            Math.Round(result.Z, 6).Should().Be(1);
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateZ_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to Z through (x=1, y=0).
            var result = Vector3.RotateZ(new Vector3(0, 0, 0), 1, 0, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(1);
            Math.Round(result.Y, 6).Should().Be(-1);
            Math.Round(result.Z, 6).Should().Be(0);
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateX_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (·, 2, 3) lies on the line parallel to X through (y=2, z=3), so it is unchanged.
            var result = new Vector3(5, 2, 3).RotateX(2, 3, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(5);
            Math.Round(result.Y, 6).Should().Be(2);
            Math.Round(result.Z, 6).Should().Be(3);
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateY_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (5, ·, 3) lies on the line parallel to Y through (x=5, z=3), so it is unchanged.
            var result = new Vector3(5, 2, 3).RotateY(5, 3, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(5);
            Math.Round(result.Y, 6).Should().Be(2);
            Math.Round(result.Z, 6).Should().Be(3);
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateZ_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (3, 4, ·) lies on the line parallel to Z through (x=3, y=4), so it is unchanged.
            var result = new Vector3(3, 4, 7).RotateZ(3, 4, Deg90AsRad);

            Math.Round(result.X, 6).Should().Be(3);
            Math.Round(result.Y, 6).Should().Be(4);
            Math.Round(result.Z, 6).Should().Be(7);
        }

        #endregion
    }
}
