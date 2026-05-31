namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) triangle, described purely by its three side lengths
    /// <see cref="SideA"/>, <see cref="SideB"/> and <see cref="SideC"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type holds only the <i>intrinsic</i> shape of a triangle — its size and proportions — with no
    /// position or orientation. Everything it computes (area, angles, classification) depends solely on
    /// the side lengths, so two triangles can be compared by size without any notion of where they sit in
    /// space. To place a triangle in the world, pair it with a position; see
    /// <see cref="PlacedTriangle"/>.
    /// </para>
    /// <para>
    /// The naming follows the usual trigonometric convention: <see cref="SideA"/> is the side opposite
    /// corner A (and so faces <see cref="AngleA"/>), and likewise for B and C. The three lengths must be
    /// positive and satisfy the <b>triangle inequality</b> — each side strictly shorter than the sum of
    /// the other two — or the constructor throws, because no triangle exists otherwise.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Triangle : IEquatable<Triangle>, IComparable<Triangle>, IFormattable
    {
        #region Fields

        private readonly double sideA;
        private readonly double sideB;
        private readonly double sideC;

        #endregion

        #region Constructors

        /// <summary>Construct a triangle from its three side lengths (A opposite corner A, and so on).</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any side is not positive.</exception>
        /// <exception cref="ArgumentException">Thrown if the sides break the triangle inequality (no such triangle exists).</exception>
        public Triangle(double sideA, double sideB, double sideC)
        {
            if (sideA <= 0) throw new ArgumentOutOfRangeException(nameof(sideA), sideA, NON_POSITIVE_SIDE);
            if (sideB <= 0) throw new ArgumentOutOfRangeException(nameof(sideB), sideB, NON_POSITIVE_SIDE);
            if (sideC <= 0) throw new ArgumentOutOfRangeException(nameof(sideC), sideC, NON_POSITIVE_SIDE);
            if (sideA + sideB <= sideC || sideB + sideC <= sideA || sideC + sideA <= sideB)
            {
                throw new ArgumentException(TRIANGLE_INEQUALITY);
            }

            this.sideA = sideA;
            this.sideB = sideB;
            this.sideC = sideC;
        }

        #endregion

        #region Factories

        /// <summary>An equilateral triangle (all three sides equal to <paramref name="side"/>).</summary>
        public static Triangle Equilateral(double side)
        {
            return new Triangle(side, side, side);
        }

        /// <summary>
        /// A right-angled triangle from its two legs; the hypotenuse (and so <see cref="SideC"/>) is
        /// derived by Pythagoras as <c>√(legA² + legB²)</c>.
        /// </summary>
        public static Triangle RightAngled(double legA, double legB)
        {
            if (legA <= 0) throw new ArgumentOutOfRangeException(nameof(legA), legA, NON_POSITIVE_SIDE);
            if (legB <= 0) throw new ArgumentOutOfRangeException(nameof(legB), legB, NON_POSITIVE_SIDE);
            return new Triangle(legA, legB, Math.Sqrt((legA * legA) + (legB * legB)));
        }

        #endregion

        #region Accessors

        /// <summary>The length of side A (opposite corner A).</summary>
        public double SideA { get { return this.sideA; } }

        /// <summary>The length of side B (opposite corner B).</summary>
        public double SideB { get { return this.sideB; } }

        /// <summary>The length of side C (opposite corner C).</summary>
        public double SideC { get { return this.sideC; } }

        /// <summary>The perimeter: the sum of the three sides.</summary>
        public double Perimeter { get { return this.sideA + this.sideB + this.sideC; } }

        /// <summary>The semiperimeter: half the perimeter (the <c>s</c> used in Heron's formula).</summary>
        public double Semiperimeter { get { return this.Perimeter / 2.0; } }

        /// <summary>
        /// The area, by Heron's formula.
        /// </summary>
        /// <remarks>Maths: <c>√(s·(s−a)·(s−b)·(s−c))</c>, where <c>s</c> is the <see cref="Semiperimeter"/>.
        /// Heron's formula gives the area from the three sides alone — no height or position needed.</remarks>
        public double Area
        {
            get
            {
                double s = this.Semiperimeter;
                double inside = s * (s - this.sideA) * (s - this.sideB) * (s - this.sideC);
                return inside <= 0 ? 0 : Math.Sqrt(inside);
            }
        }

        /// <summary>The interior angle at corner A (opposite <see cref="SideA"/>), by the law of cosines.</summary>
        public Angle AngleA { get { return AngleOpposite(this.sideA, this.sideB, this.sideC); } }

        /// <summary>The interior angle at corner B (opposite <see cref="SideB"/>), by the law of cosines.</summary>
        public Angle AngleB { get { return AngleOpposite(this.sideB, this.sideC, this.sideA); } }

        /// <summary>The interior angle at corner C (opposite <see cref="SideC"/>), by the law of cosines.</summary>
        public Angle AngleC { get { return AngleOpposite(this.sideC, this.sideA, this.sideB); } }

        #endregion

        #region Classification

        /// <summary>Whether all three sides are equal within <paramref name="tolerance"/>.</summary>
        public bool IsEquilateral(double tolerance)
        {
            return this.sideA.AlmostEqualsWithAbsTolerance(this.sideB, tolerance)
                && this.sideB.AlmostEqualsWithAbsTolerance(this.sideC, tolerance);
        }

        /// <summary>Whether at least two sides are equal within <paramref name="tolerance"/>.</summary>
        public bool IsIsosceles(double tolerance)
        {
            return this.sideA.AlmostEqualsWithAbsTolerance(this.sideB, tolerance)
                || this.sideB.AlmostEqualsWithAbsTolerance(this.sideC, tolerance)
                || this.sideC.AlmostEqualsWithAbsTolerance(this.sideA, tolerance);
        }

        /// <summary>Whether all three sides differ (no two equal within <paramref name="tolerance"/>).</summary>
        public bool IsScalene(double tolerance)
        {
            return !this.IsIsosceles(tolerance);
        }

        /// <summary>
        /// Whether the triangle is right-angled within <paramref name="tolerance"/> — its longest side
        /// squared equals the sum of the other two squared (Pythagoras).
        /// </summary>
        public bool IsRightAngled(double tolerance)
        {
            double max = Math.Max(this.sideA, Math.Max(this.sideB, this.sideC));
            double sumSquares = (this.sideA * this.sideA) + (this.sideB * this.sideB) + (this.sideC * this.sideC);
            double hypotenuseSquares = 2.0 * max * max; // sum of all squares = 2·max² when max² = the other two squares
            return sumSquares.AlmostEqualsWithAbsTolerance(hypotenuseSquares, tolerance);
        }

        #endregion

        #region Comparison (by area)

        /// <summary>
        /// Compare with another triangle by <see cref="Area"/> — the natural "which is bigger" ordering,
        /// independent of side proportions. Mirrors how <see cref="Vector"/> compares by magnitude.
        /// </summary>
        public int CompareTo(Triangle other)
        {
            return this.Area.CompareTo(other.Area);
        }

        /// <summary>Whether the left triangle's area is less than the right's.</summary>
        public static bool operator <(Triangle t1, Triangle t2) { return t1.Area < t2.Area; }

        /// <summary>Whether the left triangle's area is greater than the right's.</summary>
        public static bool operator >(Triangle t1, Triangle t2) { return t1.Area > t2.Area; }

        /// <summary>Whether the left triangle's area is less than or equal to the right's.</summary>
        public static bool operator <=(Triangle t1, Triangle t2) { return t1.Area <= t2.Area; }

        /// <summary>Whether the left triangle's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Triangle t1, Triangle t2) { return t1.Area >= t2.Area; }

        #endregion

        #region Operators

        /// <summary>Equality of all three side lengths (in order).</summary>
        public static bool operator ==(Triangle t1, Triangle t2)
        {
            return t1.SideA == t2.SideA && t1.SideB == t2.SideB && t1.SideC == t2.SideC;
        }

        /// <summary>Inequality of side lengths.</summary>
        public static bool operator !=(Triangle t1, Triangle t2)
        {
            return !(t1 == t2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Triangle t && this.Equals(t);
        }

        /// <summary>Equality with another triangle (all three sides, in order).</summary>
        public bool Equals(Triangle other)
        {
            return this == other;
        }

        /// <summary>Equality with another triangle within an absolute tolerance (side by side, in order).</summary>
        public bool Equals(Triangle other, double tolerance)
        {
            return this.sideA.AlmostEqualsWithAbsTolerance(other.SideA, tolerance)
                && this.sideB.AlmostEqualsWithAbsTolerance(other.SideB, tolerance)
                && this.sideC.AlmostEqualsWithAbsTolerance(other.SideC, tolerance);
        }

        /// <summary>A hash code derived from the three side lengths.</summary>
        public override int GetHashCode()
        {
            return this.sideA.GetHashCode() ^ this.sideB.GetHashCode() ^ this.sideC.GetHashCode();
        }

        /// <summary>Deconstruct into the three side lengths, enabling <c>var (a, b, c) = triangle;</c>.</summary>
        public void Deconstruct(out double sideA, out double sideB, out double sideC)
        {
            sideA = this.sideA;
            sideB = this.sideB;
            sideC = this.sideC;
        }

        /// <summary>A string of the form <c>(a, b, c)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(a, b, c)</c> where the sides use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2})",
                this.sideA.ToString(format, formatProvider),
                this.sideB.ToString(format, formatProvider),
                this.sideC.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// The angle opposite side <paramref name="opposite"/> in a triangle whose other two sides are
        /// <paramref name="adjacent1"/> and <paramref name="adjacent2"/>, by the law of cosines.
        /// </summary>
        /// <remarks>Maths: <c>cos θ = (p² + q² − o²) / (2·p·q)</c> for the side <c>o</c> opposite the angle
        /// and adjacent sides <c>p</c>, <c>q</c>. The ratio is clamped to −1..1 so rounding cannot push it
        /// outside the valid range of arccos.</remarks>
        private static Angle AngleOpposite(double opposite, double adjacent1, double adjacent2)
        {
            double cos = ((adjacent1 * adjacent1) + (adjacent2 * adjacent2) - (opposite * opposite))
                / (2.0 * adjacent1 * adjacent2);
            cos = cos > 1 ? 1 : (cos < -1 ? -1 : cos);
            return new Angle(Math.Acos(cos));
        }

        #endregion

        #region Messages

        private const string NON_POSITIVE_SIDE = "A triangle's side lengths must be positive.";
        private const string TRIANGLE_INEQUALITY = "The side lengths break the triangle inequality: each side must be shorter than the sum of the other two.";

        #endregion
    }
}
