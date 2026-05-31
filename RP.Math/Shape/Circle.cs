namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) circle (a flat disc), described purely by its <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape — its size — with no position or orientation. Its area and
    /// circumference depend solely on the radius, so circles can be compared by size on their own (for
    /// example by area) without any notion of where they sit. To place a disc on a plane in space, pair it
    /// with a position; see <see cref="PlacedCircle"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Circle : IEquatable<Circle>, IComparable<Circle>, IFormattable
    {
        #region Fields

        private readonly double radius;

        #endregion

        #region Constructors

        /// <summary>Construct a circle from its radius.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public Circle(double radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            this.radius = radius;
        }

        #endregion

        #region Constants

        /// <summary>The unit circle: radius 1.</summary>
        public static readonly Circle Unit = new Circle(1);

        #endregion

        #region Accessors

        /// <summary>The radius of the circle.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The diameter of the circle, twice the radius.</summary>
        public double Diameter { get { return this.radius * 2.0; } }

        /// <summary>The enclosed area, π·r².</summary>
        public double Area { get { return Math.PI * this.radius * this.radius; } }

        /// <summary>The circumference (perimeter), 2·π·r.</summary>
        public double Circumference { get { return 2.0 * Math.PI * this.radius; } }

        /// <summary>The perimeter (an alias for <see cref="Circumference"/>), 2·π·r.</summary>
        public double Perimeter { get { return this.Circumference; } }

        #endregion

        #region Comparison (by area)

        /// <summary>Compare with another circle by <see cref="Area"/> (equivalently by radius).</summary>
        public int CompareTo(Circle other) { return this.radius.CompareTo(other.Radius); }

        /// <summary>Whether the left circle's area is less than the right's.</summary>
        public static bool operator <(Circle c1, Circle c2) { return c1.Radius < c2.Radius; }

        /// <summary>Whether the left circle's area is greater than the right's.</summary>
        public static bool operator >(Circle c1, Circle c2) { return c1.Radius > c2.Radius; }

        /// <summary>Whether the left circle's area is less than or equal to the right's.</summary>
        public static bool operator <=(Circle c1, Circle c2) { return c1.Radius <= c2.Radius; }

        /// <summary>Whether the left circle's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Circle c1, Circle c2) { return c1.Radius >= c2.Radius; }

        #endregion

        #region Operators

        /// <summary>Equality of radius.</summary>
        public static bool operator ==(Circle c1, Circle c2)
        {
            return c1.Radius == c2.Radius;
        }

        /// <summary>Inequality of radius.</summary>
        public static bool operator !=(Circle c1, Circle c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Circle c && this.Equals(c);
        }

        /// <summary>Equality with another circle (radius).</summary>
        public bool Equals(Circle other)
        {
            return this == other;
        }

        /// <summary>Equality with another circle within an absolute tolerance.</summary>
        public bool Equals(Circle other, double tolerance)
        {
            return this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance);
        }

        /// <summary>A hash code derived from the radius.</summary>
        public override int GetHashCode()
        {
            return this.radius.GetHashCode();
        }

        /// <summary>Deconstruct into the radius.</summary>
        public void Deconstruct(out double radius)
        {
            radius = this.radius;
        }

        /// <summary>A string of the form <c>(r=radius)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(r=radius)</c> where the radius uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format("(r={0})", this.radius.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A circle radius cannot be negative.";

        #endregion
    }
}
