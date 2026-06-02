namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) ellipse, described by its two semi-axes <see cref="SemiAxisX"/> and
    /// <see cref="SemiAxisY"/> (the half-widths along its two perpendicular axes).
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape — size and proportion — with no position or orientation.
    /// A circle is the special case where the two semi-axes are equal. To place an ellipse on a plane in
    /// space, pair it with a position; see <see cref="PlacedEllipse"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Ellipse : IEquatable<Ellipse>, IComparable<Ellipse>, IFormattable
    {
        #region Fields

        private readonly double semiAxisX;
        private readonly double semiAxisY;

        #endregion

        #region Constructors

        /// <summary>Construct an ellipse from its two semi-axes (half-widths along the local X and Y axes).</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either semi-axis is negative.</exception>
        public Ellipse(double semiAxisX, double semiAxisY)
        {
            if (semiAxisX < 0) throw new ArgumentOutOfRangeException(nameof(semiAxisX), semiAxisX, NEGATIVE_AXIS);
            if (semiAxisY < 0) throw new ArgumentOutOfRangeException(nameof(semiAxisY), semiAxisY, NEGATIVE_AXIS);

            this.semiAxisX = semiAxisX;
            this.semiAxisY = semiAxisY;
        }

        #endregion

        #region Accessors

        /// <summary>The semi-axis along the local X axis.</summary>
        public double SemiAxisX { get { return this.semiAxisX; } }

        /// <summary>The semi-axis along the local Y axis.</summary>
        public double SemiAxisY { get { return this.semiAxisY; } }

        /// <summary>The longer of the two semi-axes (the semi-major axis).</summary>
        public double SemiMajor { get { return Math.Max(this.semiAxisX, this.semiAxisY); } }

        /// <summary>The shorter of the two semi-axes (the semi-minor axis).</summary>
        public double SemiMinor { get { return Math.Min(this.semiAxisX, this.semiAxisY); } }

        /// <summary>The enclosed area, π·a·b.</summary>
        public double Area { get { return Math.PI * this.semiAxisX * this.semiAxisY; } }

        /// <summary>
        /// The perimeter (circumference), by Ramanujan's approximation.
        /// </summary>
        /// <remarks>An ellipse has no elementary exact perimeter; this uses Ramanujan's well-known close
        /// approximation <c>π·[3(a + b) − √((3a + b)(a + 3b))]</c>.</remarks>
        public double Perimeter
        {
            get
            {
                double a = this.semiAxisX;
                double b = this.semiAxisY;
                return Math.PI * ((3.0 * (a + b)) - Math.Sqrt(((3.0 * a) + b) * (a + (3.0 * b))));
            }
        }

        /// <summary>
        /// The eccentricity, between 0 (a circle) and 1 (a degenerate line segment): <c>√(1 − (b/a)²)</c>
        /// with <c>a</c> the major and <c>b</c> the minor semi-axis.
        /// </summary>
        public double Eccentricity
        {
            get
            {
                double major = this.SemiMajor;
                if (major == 0)
                {
                    return 0;
                }

                double ratio = this.SemiMinor / major;
                return Math.Sqrt(Math.Max(0, 1.0 - (ratio * ratio)));
            }
        }

        /// <summary>The distance from the centre to each focus, <c>√(major² − minor²)</c>.</summary>
        public double FocalDistance
        {
            get { return Math.Sqrt(Math.Max(0, (this.SemiMajor * this.SemiMajor) - (this.SemiMinor * this.SemiMinor))); }
        }

        #endregion

        #region Classification

        /// <summary>Whether the two semi-axes are equal within <paramref name="tolerance"/> (a circle).</summary>
        public bool IsCircle(double tolerance)
        {
            return this.semiAxisX.AlmostEqualsWithAbsTolerance(this.semiAxisY, tolerance);
        }

        #endregion

        #region Comparison (by area)

        /// <summary>Compare with another ellipse by <see cref="Area"/>.</summary>
        public int CompareTo(Ellipse other) { return this.Area.CompareTo(other.Area); }

        /// <summary>Whether the left ellipse's area is less than the right's.</summary>
        public static bool operator <(Ellipse e1, Ellipse e2) { return e1.Area < e2.Area; }

        /// <summary>Whether the left ellipse's area is greater than the right's.</summary>
        public static bool operator >(Ellipse e1, Ellipse e2) { return e1.Area > e2.Area; }

        /// <summary>Whether the left ellipse's area is less than or equal to the right's.</summary>
        public static bool operator <=(Ellipse e1, Ellipse e2) { return e1.Area <= e2.Area; }

        /// <summary>Whether the left ellipse's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Ellipse e1, Ellipse e2) { return e1.Area >= e2.Area; }

        #endregion

        #region Operators

        /// <summary>Equality of both semi-axes.</summary>
        public static bool operator ==(Ellipse e1, Ellipse e2)
        {
            return e1.SemiAxisX == e2.SemiAxisX && e1.SemiAxisY == e2.SemiAxisY;
        }

        /// <summary>Inequality of either semi-axis.</summary>
        public static bool operator !=(Ellipse e1, Ellipse e2)
        {
            return !(e1 == e2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Ellipse e && this.Equals(e);
        }

        /// <summary>Equality with another ellipse (both semi-axes).</summary>
        public bool Equals(Ellipse other)
        {
            return this == other;
        }

        /// <summary>Equality with another ellipse within an absolute tolerance.</summary>
        public bool Equals(Ellipse other, double tolerance)
        {
            return this.semiAxisX.AlmostEqualsWithAbsTolerance(other.SemiAxisX, tolerance)
                && this.semiAxisY.AlmostEqualsWithAbsTolerance(other.SemiAxisY, tolerance);
        }

        /// <summary>A hash code derived from the two semi-axes.</summary>
        public override int GetHashCode()
        {
            return this.semiAxisX.GetHashCode() ^ this.semiAxisY.GetHashCode();
        }

        /// <summary>Deconstruct into the two semi-axes.</summary>
        public void Deconstruct(out double semiAxisX, out double semiAxisY)
        {
            semiAxisX = this.semiAxisX;
            semiAxisY = this.semiAxisY;
        }

        /// <summary>A string of the form <c>(a=semiAxisX, b=semiAxisY)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(a=semiAxisX, b=semiAxisY)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(a={0}, b={1})",
                this.semiAxisX.ToString(format, formatProvider),
                this.semiAxisY.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_AXIS = "An ellipse's semi-axes cannot be negative.";

        #endregion
    }
}
