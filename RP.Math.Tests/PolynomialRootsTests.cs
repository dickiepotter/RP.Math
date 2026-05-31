namespace RP.Math.Tests
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="PolynomialRoots"/> real-root solver.</summary>
    [TestClass]
    public class PolynomialRootsTests
    {
        private const double Tol = 1e-7;

        private static void ShouldContainRoots(double[] actual, params double[] expected)
        {
            foreach (double e in expected)
            {
                actual.Any(r => Math.Abs(r - e) < Tol).Should().BeTrue($"root {e} should be found in [{string.Join(", ", actual)}]");
            }

            actual.Length.Should().Be(expected.Length);
        }

        [TestMethod, TestCategory("Quadratic")]
        public void Quadratic_TwoRoots_Test()
        {
            ShouldContainRoots(PolynomialRoots.SolveQuadratic(1, -3, 2), 1, 2);
        }

        [TestMethod, TestCategory("Quadratic")]
        public void Quadratic_NoRealRoots_Test()
        {
            PolynomialRoots.SolveQuadratic(1, 0, 1).Should().BeEmpty();
        }

        [TestMethod, TestCategory("Cubic")]
        public void Cubic_ThreeRealRoots_Test()
        {
            // (x-1)(x-2)(x-3) = x^3 - 6x^2 + 11x - 6
            ShouldContainRoots(PolynomialRoots.SolveCubic(1, -6, 11, -6), 1, 2, 3);
        }

        [TestMethod, TestCategory("Cubic")]
        public void Cubic_OneRealRoot_Test()
        {
            // x^3 + x + 1 = 0 has a single real root near -0.6823
            double[] roots = PolynomialRoots.SolveCubic(1, 0, 1, 1);
            roots.Length.Should().Be(1);
            roots[0].Should().BeApproximately(-0.6823278, 1e-6);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_FourRealRoots_Test()
        {
            // (x-1)(x-2)(x-3)(x-4) = x^4 - 10x^3 + 35x^2 - 50x + 24
            ShouldContainRoots(PolynomialRoots.SolveQuartic(1, -10, 35, -50, 24), 1, 2, 3, 4);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_Biquadratic_Test()
        {
            // (x^2-1)(x^2-4) = x^4 - 5x^2 + 4
            ShouldContainRoots(PolynomialRoots.SolveQuartic(1, 0, -5, 0, 4), -2, -1, 1, 2);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_NoRealRoots_Test()
        {
            // (x^2+1)(x^2+4) = x^4 + 5x^2 + 4
            PolynomialRoots.SolveQuartic(1, 0, 5, 0, 4).Should().BeEmpty();
        }
    }
}
