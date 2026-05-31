namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the <see cref="Angle"/>-typed rotation overloads added to <see cref="Vector"/>.
    /// These extend the original radian-based API so the type speaks the richer Angle vocabulary.
    /// </summary>
    [TestClass]
    public class VectorAngleRotationTests
    {
        private const double Tol = 1e-12;

        private static readonly Vector Sample = new Vector(2, 3, 5);
        private static readonly Angle Quarter = new Angle(90, AngleUnits.DEG);

        #region Angle overload equals radian form

        [TestMethod, TestCategory("Rotation")]
        public void Yaw_AngleOverload_ShouldEqualRadianForm_Test()
        {
            Sample.Yaw(Quarter).Equals(Sample.Yaw(Quarter.Rad), Tol).Should().BeTrue();
            Vector.Yaw(Sample, Quarter).Equals(Vector.Yaw(Sample, Quarter.Rad), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void Pitch_AngleOverload_ShouldEqualRadianForm_Test()
        {
            Sample.Pitch(Quarter).Equals(Sample.Pitch(Quarter.Rad), Tol).Should().BeTrue();
            Vector.Pitch(Sample, Quarter).Equals(Vector.Pitch(Sample, Quarter.Rad), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void Roll_AngleOverload_ShouldEqualRadianForm_Test()
        {
            Sample.Roll(Quarter).Equals(Sample.Roll(Quarter.Rad), Tol).Should().BeTrue();
            Vector.Roll(Sample, Quarter).Equals(Vector.Roll(Sample, Quarter.Rad), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateXYZ_AngleOverloads_ShouldEqualRadianForms_Test()
        {
            Sample.RotateX(Quarter).Equals(Sample.RotateX(Quarter.Rad), Tol).Should().BeTrue();
            Sample.RotateY(Quarter).Equals(Sample.RotateY(Quarter.Rad), Tol).Should().BeTrue();
            Sample.RotateZ(Quarter).Equals(Sample.RotateZ(Quarter.Rad), Tol).Should().BeTrue();

            Vector.RotateX(Sample, Quarter).Equals(Vector.RotateX(Sample, Quarter.Rad), Tol).Should().BeTrue();
            Vector.RotateY(Sample, Quarter).Equals(Vector.RotateY(Sample, Quarter.Rad), Tol).Should().BeTrue();
            Vector.RotateZ(Sample, Quarter).Equals(Vector.RotateZ(Sample, Quarter.Rad), Tol).Should().BeTrue();
        }

        #endregion

        #region Behaviour

        [TestMethod, TestCategory("Rotation")]
        public void Roll_NinetyDegrees_ShouldTakeXAxisToYAxis_Test()
        {
            new Vector(1, 0, 0).Roll(new Angle(90, AngleUnits.DEG))
                .Equals(new Vector(0, 1, 0), 1e-9).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void Yaw_FullTurn_ShouldReturnToStart_Test()
        {
            Sample.Yaw(new Angle(360, AngleUnits.DEG)).Equals(Sample, 1e-9).Should().BeTrue();
        }

        #endregion

        #region Overload resolution is unambiguous

        [TestMethod, TestCategory("Rotation")]
        public void DoubleArgument_ShouldStillBindToRadianOverload_Test()
        {
            // A double binds to the radian overload exactly; behaviour is unchanged from before.
            double rad = System.Math.PI / 2;
            Sample.Roll(rad).Equals(Vector.Roll(Sample, rad), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Rotation")]
        public void IntegerArgument_ShouldBindToRadianOverload_Test()
        {
            // An int prefers the built-in int->double over the user-defined int->Angle, so this is the
            // radian form (1 radian), not 1 degree.
            Sample.Roll(1).Equals(Vector.Roll(Sample, 1.0), Tol).Should().BeTrue();
        }

        #endregion
    }
}
