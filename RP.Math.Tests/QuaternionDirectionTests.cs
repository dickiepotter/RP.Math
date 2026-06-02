namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="Quaternion.FromToRotation"/> and <see cref="Quaternion.LookRotation(Vector, Vector, OrthogonalAxes)"/>.
    /// </summary>
    [TestClass]
    public class QuaternionDirectionTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        #region FromToRotation

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_TurnsFromOntoTo_Test()
        {
            Quaternion q = Quaternion.FromToRotation(Vector.XAxis, Vector.YAxis);
            q.Rotate(Vector.XAxis).Equals(Vector.YAxis, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfAlignedVectors_IsIdentity_Test()
        {
            Quaternion q = Quaternion.FromToRotation(new Vector(2, 0, 0), new Vector(5, 0, 0));
            q.IsIdentity(Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfOppositeVectors_IsAHalfTurn_Test()
        {
            Quaternion q = Quaternion.FromToRotation(Vector.XAxis, new Vector(-1, 0, 0));
            q.Rotate(Vector.XAxis).Equals(new Vector(-1, 0, 0), LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_NormalizesInputs_Test()
        {
            // Non-unit inputs should behave like their directions.
            Quaternion q = Quaternion.FromToRotation(new Vector(0, 3, 0), new Vector(0, 0, 7));
            q.Rotate(Vector.YAxis).Equals(Vector.ZAxis, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfZeroVector_IsIdentity_Test()
        {
            Quaternion.FromToRotation(Vector.Zero, Vector.XAxis).IsIdentity(Tolerance).Should().BeTrue();
        }

        #endregion

        #region LookRotation

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_AlongTheConventionForward_IsIdentity_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            Quaternion q = Quaternion.LookRotation(axes.Forward, axes.Up, axes);
            q.IsIdentity(LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_MapsTheConventionForwardOntoTheTarget_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var target = new Vector(1, 2, 3).NormalizeOrDefault();

            Quaternion q = Quaternion.LookRotation(target, axes.Up, axes);

            q.Rotate(axes.Forward).Equals(target, LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_KeepsUpPerpendicularToForwardAndOnTheUpSide_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var forward = new Vector(1, 0, 1);
            var up = new Vector(0, 1, 0);

            Quaternion q = Quaternion.LookRotation(forward, up, axes);
            Vector resultUp = q.Rotate(axes.Up);

            // The resulting up is perpendicular to the forward direction...
            resultUp.DotProduct(forward.NormalizeOrDefault()).Should().BeApproximately(0, LooseTolerance);
            // ...and points to the same side as the requested up (positive component along it).
            resultUp.DotProduct(up).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_WithUpParallelToForward_StillFacesTheTarget_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var forward = new Vector(0, 1, 0);

            // up parallel to forward: the roll is unspecified, but it must still look along forward.
            Quaternion q = Quaternion.LookRotation(forward, new Vector(0, 2, 0), axes);

            q.Rotate(axes.Forward).Equals(forward.NormalizeOrDefault(), LooseTolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_SingleArgument_UsesTheConventionUp_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.DirectX;
            var forward = new Vector(1, 1, 1);

            Quaternion two = Quaternion.LookRotation(forward, axes.Up, axes);
            Quaternion one = Quaternion.LookRotation(forward, axes);

            one.Equals(two, LooseTolerance).Should().BeTrue();
        }

        #endregion
    }
}
