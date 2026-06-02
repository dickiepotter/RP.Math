namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the right-handed (OpenGL-convention) view and projection builders on
    /// <see cref="Matrix"/>: <see cref="Matrix.LookAt"/>, <see cref="Matrix.PerspectiveFieldOfView"/>,
    /// <see cref="Matrix.Orthographic"/>, and the perspective-divide <see cref="Matrix.Transform"/>.
    /// </summary>
    [TestClass]
    public class MatrixViewProjectionTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        #region LookAt

        [TestMethod, TestCategory("LookAt")]
        public void LookAt_PutsTheEyeAtTheOrigin_AndTheTargetDownNegativeZ_Test()
        {
            var eye = new Vector(0, 0, 5);
            var target = new Vector(0, 0, 0);
            Matrix view = Matrix.LookAt(eye, target, new Vector(0, 1, 0));

            view.Transform(eye).Equals(Vector.Origin, LooseTol).Should().BeTrue();
            // Target sits 5 in front of the camera, i.e. down its local -Z (right-handed).
            view.Transform(target).Equals(new Vector(0, 0, -5), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("LookAt")]
        public void LookAt_KeepsWorldRightAsCameraRight_Test()
        {
            var eye = new Vector(0, 0, 5);
            Matrix view = Matrix.LookAt(eye, new Vector(0, 0, 0), new Vector(0, 1, 0));

            // A world point one unit to the +X of the eye stays on camera +X (and up stays up).
            view.Transform(new Vector(1, 0, 5)).Equals(new Vector(1, 0, 0), LooseTol).Should().BeTrue();
            view.Transform(new Vector(0, 1, 5)).Equals(new Vector(0, 1, 0), LooseTol).Should().BeTrue();
        }

        [TestMethod, TestCategory("LookAt")]
        public void LookAt_WithEyeAtTarget_Throws_Test()
        {
            Action act = () => Matrix.LookAt(new Vector(1, 1, 1), new Vector(1, 1, 1), new Vector(0, 1, 0));
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("LookAt")]
        public void LookAt_WithUpParallelToView_Throws_Test()
        {
            // Looking straight down +Y with up also +Y: no way to resolve "right".
            Action act = () => Matrix.LookAt(new Vector(0, 5, 0), new Vector(0, 0, 0), new Vector(0, 1, 0));
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Orthographic

        [TestMethod, TestCategory("Orthographic")]
        public void Orthographic_MapsNearToMinusOne_AndFarToPlusOne_Test()
        {
            Matrix ortho = Matrix.Orthographic(2, 2, 1, 3); // width 2, height 2, near 1, far 3

            // The camera looks down -Z, so the near plane is at z = -near and the far plane at z = -far.
            ortho.Transform(new Vector(0, 0, -1)).Equals(new Vector(0, 0, -1), Tol).Should().BeTrue(); // near -> -1
            ortho.Transform(new Vector(0, 0, -3)).Equals(new Vector(0, 0, 1), Tol).Should().BeTrue();  // far  -> +1
        }

        [TestMethod, TestCategory("Orthographic")]
        public void Orthographic_MapsTheBoxEdgesToTheClipCube_Test()
        {
            Matrix ortho = Matrix.Orthographic(2, 2, 1, 3); // half-width 1, half-height 1

            ortho.Transform(new Vector(1, 1, -1)).Equals(new Vector(1, 1, -1), Tol).Should().BeTrue();
            ortho.Transform(new Vector(-1, -1, -3)).Equals(new Vector(-1, -1, 1), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Orthographic")]
        public void Orthographic_IsAffine_SoTheStarOperatorAlsoApplies_Test()
        {
            Matrix ortho = Matrix.Orthographic(2, 2, 1, 3);
            (ortho * new Vector(0, 0, -3)).Equals(ortho.Transform(new Vector(0, 0, -3)), Tol).Should().BeTrue();
        }

        [TestMethod, TestCategory("Orthographic")]
        public void Orthographic_DegenerateVolume_Throws_Test()
        {
            Action act = () => Matrix.OrthographicOffCenter(-1, 1, -1, 1, 2, 2); // near == far
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region Perspective

        [TestMethod, TestCategory("Perspective")]
        public void Perspective_MapsNearAndFarCentresToTheClipDepths_Test()
        {
            Matrix p = Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 1.0, 1, 3);

            p.Transform(new Vector(0, 0, -1)).Equals(new Vector(0, 0, -1), LooseTol).Should().BeTrue(); // near -> -1
            p.Transform(new Vector(0, 0, -3)).Equals(new Vector(0, 0, 1), LooseTol).Should().BeTrue();  // far  -> +1
        }

        [TestMethod, TestCategory("Perspective")]
        public void Perspective_FillsTheClipWidthAtTheNearPlane_Test()
        {
            // 90° vertical FOV, aspect 1: the near plane half-extent equals the near distance, so a point
            // at (near, 0, -near) sits on the right clip edge (+1).
            Matrix p = Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 1.0, 1, 3);
            p.Transform(new Vector(1, 0, -1)).X.Should().BeApproximately(1, LooseTol);
        }

        [TestMethod, TestCategory("Perspective")]
        public void Perspective_ForeshortensWithDistance_Test()
        {
            // The same world x = 1 appears smaller in clip space the further away it is — the whole point.
            Matrix p = Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 1.0, 1, 3);
            double nearX = p.Transform(new Vector(1, 0, -1)).X; // at the near plane
            double farX = p.Transform(new Vector(1, 0, -3)).X;  // three times further
            farX.Should().BeApproximately(1.0 / 3.0, LooseTol);
            farX.Should().BeLessThan(nearX);
        }

        [TestMethod, TestCategory("Perspective")]
        public void Perspective_InvalidArguments_Throw_Test()
        {
            Action badNear = () => Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 1.0, 0, 3);
            Action badAspect = () => Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 0, 1, 3);
            Action badFov = () => Matrix.PerspectiveFieldOfView(new Angle(180, AngleUnits.DEG), 1.0, 1, 3);

            badNear.Should().Throw<ArgumentOutOfRangeException>();
            badAspect.Should().Throw<ArgumentOutOfRangeException>();
            badFov.Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion

        #region Composition

        [TestMethod, TestCategory("Composition")]
        public void ViewThenProjection_ComposesByMultiplication_Test()
        {
            var eye = new Vector(0, 0, 5);
            Matrix view = Matrix.LookAt(eye, new Vector(0, 0, 0), new Vector(0, 1, 0));
            Matrix proj = Matrix.PerspectiveFieldOfView(new Angle(90, AngleUnits.DEG), 1.0, 1, 10);

            // (proj * view) applied to a world point equals proj applied to the view-space point.
            Matrix viewProj = proj * view;
            var world = new Vector(0, 0, 0); // the target, 5 in front of the eye
            viewProj.Transform(world).Equals(proj.Transform(view.Transform(world)), LooseTol).Should().BeTrue();
        }

        #endregion
    }
}
