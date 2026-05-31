namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Triangle"/> type.</summary>
    [TestClass]
    public class TriangleTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSides_Test()
        {
            var t = new Triangle(3, 4, 5);
            t.SideA.Should().Be(3);
            t.SideB.Should().Be(4);
            t.SideC.Should().Be(5);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NonPositiveSide_ShouldThrow_Test()
        {
            Action act = () => new Triangle(0, 1, 1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_BreaksTriangleInequality_ShouldThrow_Test()
        {
            Action act = () => new Triangle(1, 1, 5);
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Factory")]
        public void RightAngled_DerivesHypotenuse_Test()
        {
            var t = Triangle.RightAngled(3, 4);
            t.SideC.Should().BeApproximately(5, Tol);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Perimeter_And_Area_ShouldMatchFormulae_Test()
        {
            var t = new Triangle(3, 4, 5);
            t.Perimeter.Should().Be(12);
            t.Semiperimeter.Should().Be(6);
            t.Area.Should().BeApproximately(6, Tol); // Heron: sqrt(6*3*2*1)
        }

        [TestMethod, TestCategory("Measurement")]
        public void Equilateral_Area_ShouldMatchFormula_Test()
        {
            var t = Triangle.Equilateral(2);
            t.Area.Should().BeApproximately(System.Math.Sqrt(3), Tol); // (sqrt3/4)*side^2
        }

        [TestMethod, TestCategory("Angles")]
        public void Angles_OfRightTriangle_ShouldSumAndIncludeRightAngle_Test()
        {
            var t = new Triangle(3, 4, 5);
            t.AngleC.Deg.Should().BeApproximately(90, 1e-6);            // opposite the longest side
            (t.AngleA.Deg + t.AngleB.Deg + t.AngleC.Deg).Should().BeApproximately(180, 1e-6);
        }

        [TestMethod, TestCategory("Classification")]
        public void Classification_ShouldIdentifyKinds_Test()
        {
            Triangle.Equilateral(2).IsEquilateral(Tol).Should().BeTrue();
            new Triangle(2, 2, 3).IsIsosceles(Tol).Should().BeTrue();
            new Triangle(3, 4, 5).IsScalene(Tol).Should().BeTrue();
            new Triangle(3, 4, 5).IsRightAngled(1e-6).Should().BeTrue();
            Triangle.Equilateral(2).IsRightAngled(1e-6).Should().BeFalse();
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByArea_Test()
        {
            var small = Triangle.Equilateral(2);   // area ~1.73
            var large = new Triangle(3, 4, 5);      // area 6
            (large > small).Should().BeTrue();
            (small < large).Should().BeTrue();
            large.CompareTo(small).Should().BeGreaterThan(0);
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSides_Test()
        {
            new Triangle(3, 4, 5).Equals(new Triangle(3, 4, 5)).Should().BeTrue();
            (new Triangle(3, 4, 5) == new Triangle(3, 4, 5)).Should().BeTrue();
            (new Triangle(3, 4, 5) != new Triangle(4, 4, 5)).Should().BeTrue();
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSides_Test()
        {
            var (a, b, c) = new Triangle(3, 4, 5);
            a.Should().Be(3);
            b.Should().Be(4);
            c.Should().Be(5);
        }
    }
}
