namespace RP.Math
{
    using System;
    using System.Collections.Generic;

    using Math = System.Math;

    /// <summary>
    /// Finds the <b>real</b> roots of low-degree polynomials — quadratic, cubic and quartic. Used by the
    /// shape intersection maths (notably <see cref="PlacedTorus"/>, whose line/ray intersection is a
    /// quartic) where a closed-form answer is wanted rather than a general numerical solver.
    /// </summary>
    /// <remarks>
    /// The cubic uses Cardano's method (with the trigonometric form for the three-real-roots case); the
    /// quartic uses Ferrari's method, reducing to a resolvent cubic. Complex roots are discarded — only
    /// real solutions are returned, unsorted. Leading coefficients near zero degrade gracefully to the
    /// next lower degree.
    /// </remarks>
    public static class PolynomialRoots
    {
        private const double Epsilon = 1e-12;

        /// <summary>The real roots of <c>a·x² + b·x + c = 0</c>.</summary>
        public static double[] SolveQuadratic(double a, double b, double c)
        {
            if (Math.Abs(a) < Epsilon)
            {
                return Math.Abs(b) < Epsilon ? Array.Empty<double>() : new[] { -c / b };
            }

            double disc = (b * b) - (4.0 * a * c);
            if (disc < -Epsilon)
            {
                return Array.Empty<double>();
            }

            if (disc <= Epsilon)
            {
                return new[] { -b / (2.0 * a) };
            }

            // Numerically stable form (Numerical Recipes): the naive (-b ± √disc)/2a subtracts two
            // nearly-equal numbers for one of the roots when |b| ≫ √disc, destroying precision. Forming
            // q with the sign of b keeps that subtraction away from cancellation, then the second root
            // comes from the product relation x₁·x₂ = c/a. q is never zero here because disc > Epsilon.
            double s = Math.Sqrt(disc);
            double q = -0.5 * (b + (b < 0 ? -s : s));
            double r1 = q / a;
            double r2 = c / q;
            return r1 <= r2 ? new[] { r1, r2 } : new[] { r2, r1 };
        }

        /// <summary>The real roots of <c>a·x³ + b·x² + c·x + d = 0</c>.</summary>
        public static double[] SolveCubic(double a, double b, double c, double d)
        {
            if (Math.Abs(a) < Epsilon)
            {
                return SolveQuadratic(b, c, d);
            }

            // Normalise to x³ + bx² + cx + d.
            b /= a; c /= a; d /= a;

            // Depress to t³ + p·t + q with x = t − b/3.
            double p = c - (b * b / 3.0);
            double q = (2.0 * b * b * b / 27.0) - (b * c / 3.0) + d;
            double shift = -b / 3.0;
            double disc = (q * q / 4.0) + (p * p * p / 27.0);

            if (disc > Epsilon)
            {
                double sqrtDisc = Math.Sqrt(disc);
                double u = Cbrt((-q / 2.0) + sqrtDisc);
                double v = Cbrt((-q / 2.0) - sqrtDisc);
                return new[] { u + v + shift };
            }

            if (disc < -Epsilon)
            {
                // Three distinct real roots (trigonometric form).
                double m = 2.0 * Math.Sqrt(-p / 3.0);
                double theta = Math.Acos(Clamp((3.0 * q) / (p * m), -1.0, 1.0)) / 3.0;
                return new[]
                {
                    (m * Math.Cos(theta)) + shift,
                    (m * Math.Cos(theta - (2.0 * Math.PI / 3.0))) + shift,
                    (m * Math.Cos(theta - (4.0 * Math.PI / 3.0))) + shift,
                };
            }

            // disc ≈ 0: a repeated root.
            if (Math.Abs(p) < Epsilon)
            {
                return new[] { shift }; // triple root
            }

            double single = (3.0 * q) / p;
            double doubled = -(3.0 * q) / (2.0 * p);
            return new[] { single + shift, doubled + shift };
        }

        /// <summary>The real roots of <c>a·x⁴ + b·x³ + c·x² + d·x + e = 0</c>.</summary>
        public static double[] SolveQuartic(double a, double b, double c, double d, double e)
        {
            if (Math.Abs(a) < Epsilon)
            {
                return SolveCubic(b, c, d, e);
            }

            // Normalise to x⁴ + bx³ + cx² + dx + e.
            b /= a; c /= a; d /= a; e /= a;

            // Depress to y⁴ + p·y² + q·y + r with x = y − b/4.
            double b4 = b / 4.0;
            double p = c - (3.0 * b * b / 8.0);
            double q = d - (b * c / 2.0) + (b * b * b / 8.0);
            double r = e - (b * d / 4.0) + (b * b * c / 16.0) - (3.0 * b * b * b * b / 256.0);

            var roots = new List<double>(4);

            if (Math.Abs(q) < Epsilon)
            {
                // Biquadratic: y⁴ + p·y² + r = 0, solved as a quadratic in y².
                foreach (double z in SolveQuadratic(1.0, p, r))
                {
                    if (z >= 0)
                    {
                        double sq = Math.Sqrt(z);
                        roots.Add(sq - b4);
                        if (sq > Epsilon)
                        {
                            roots.Add(-sq - b4);
                        }
                    }
                }

                return roots.ToArray();
            }

            // Ferrari: find a real root m of the resolvent cubic, with 2m > 0.
            double[] resolvent = SolveCubic(8.0, 8.0 * p, (2.0 * p * p) - (8.0 * r), -(q * q));
            double m = double.NegativeInfinity;
            foreach (double root in resolvent)
            {
                if (root > m)
                {
                    m = root;
                }
            }

            if (m <= Epsilon)
            {
                return roots.ToArray(); // no usable resolvent root
            }

            double bigR = Math.Sqrt(2.0 * m);
            double baseTerm = (p / 2.0) + m;
            double offset = q / (2.0 * bigR);

            // y⁴ + p·y² + q·y + r = (y² − R·y + (base + offset))·(y² + R·y + (base − offset)).
            foreach (double y in SolveQuadratic(1.0, -bigR, baseTerm + offset))
            {
                roots.Add(y - b4);
            }

            foreach (double y in SolveQuadratic(1.0, bigR, baseTerm - offset))
            {
                roots.Add(y - b4);
            }

            return roots.ToArray();
        }

        /// <summary>The real cube root, defined for negative inputs (unlike <see cref="Math.Pow(double, double)"/>).</summary>
        private static double Cbrt(double value)
        {
            return value < 0 ? -Math.Pow(-value, 1.0 / 3.0) : Math.Pow(value, 1.0 / 3.0);
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
