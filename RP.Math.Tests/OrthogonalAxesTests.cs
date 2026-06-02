using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RP.Math.Tests
{
    /// <summary>
    /// Covers the merged <see cref="OrthogonalAxes"/> convention type: derived basis vectors, derived
    /// handedness, validation, the predefined system conventions, and formatting.
    /// </summary>
    [TestClass]
    public class OrthogonalAxesTests
    {
        // ---- Constructor validation -------------------------------------------------

        [TestMethod]
        public void Constructor_RejectsTwoAxesFromTheSamePair()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.UP, AxisAlignment.DOWN, AxisAlignment.RIGHT);
            act.Should().Throw<System.ArgumentException>();
        }

        [TestMethod]
        public void Constructor_RejectsRepeatedRole()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.RIGHT, AxisAlignment.UP);
            act.Should().Throw<System.ArgumentException>();
        }

        [TestMethod]
        public void Constructor_AcceptsAGenuineConvention()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.FAR, AxisAlignment.RIGHT, AxisAlignment.UP);
            act.Should().NotThrow();
        }

        // ---- Derived basis vectors --------------------------------------------------

        [TestMethod]
        public void Right_Up_Forward_FollowTheLabelsAndSigns()
        {
            // Unreal: +x forward (far), +y right, +z up.
            var a = new OrthogonalAxes(AxisAlignment.FAR, AxisAlignment.RIGHT, AxisAlignment.UP);

            a.Forward.Should().Be(new Vector(1, 0, 0));
            a.Right.Should().Be(new Vector(0, 1, 0));
            a.Up.Should().Be(new Vector(0, 0, 1));
        }

        [TestMethod]
        public void NegativeLabels_FlipTheBasisVector()
        {
            // z carries NEAR, so the forward/far direction is -z.
            var a = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.NEAR);

            a.Forward.Should().Be(new Vector(0, 0, -1));
        }

        // ---- Derived handedness -----------------------------------------------------

        [TestMethod]
        public void Handedness_IsRightHandedForOpenGlStyleFrames()
        {
            OrthogonalAxes.OpenGL.Handedness.Should().Be(Handedness.Right);
            OrthogonalAxes.Blender.Handedness.Should().Be(Handedness.Right);
        }

        [TestMethod]
        public void Handedness_IsLeftHandedForDirectXStyleFrames()
        {
            OrthogonalAxes.DirectX.Handedness.Should().Be(Handedness.Left);
            OrthogonalAxes.Unreal.Handedness.Should().Be(Handedness.Left);
        }

        // ---- Predefined conventions -------------------------------------------------

        [TestMethod]
        public void OpenGlAndDirectX_DifferOnlyInTheDepthAxis()
        {
            // The whole right- vs left-handed distinction is exactly NEAR vs FAR on z.
            OrthogonalAxes.OpenGL.Z.Should().Be(AxisAlignment.NEAR);
            OrthogonalAxes.DirectX.Z.Should().Be(AxisAlignment.FAR);
            OrthogonalAxes.OpenGL.X.Should().Be(OrthogonalAxes.DirectX.X);
            OrthogonalAxes.OpenGL.Y.Should().Be(OrthogonalAxes.DirectX.Y);
        }

        [TestMethod]
        public void AliasedConventions_AreEqual()
        {
            OrthogonalAxes.Maya.Should().Be(OrthogonalAxes.OpenGL);
            OrthogonalAxes.Godot.Should().Be(OrthogonalAxes.OpenGL);
            OrthogonalAxes.MathsYUp.Should().Be(OrthogonalAxes.OpenGL);
            OrthogonalAxes.Unity.Should().Be(OrthogonalAxes.DirectX);
            OrthogonalAxes.Max3ds.Should().Be(OrthogonalAxes.Blender);
            OrthogonalAxes.MathsZUp.Should().Be(OrthogonalAxes.Blender);
        }

        // ---- Formatting (was BUG 6) -------------------------------------------------

        [TestMethod]
        public void ToString_DefaultAndVerbose_DoNotThrow()
        {
            var a = OrthogonalAxes.OpenGL;

            a.ToString().Should().Be("(RIGHT, UP, NEAR)");
            a.ToString("v", null).Should().Contain("Right-handed");
        }
    }
}
