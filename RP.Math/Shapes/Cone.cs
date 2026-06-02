namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) right circular cone, described by its <see cref="BaseRadius"/> and
    /// <see cref="Height"/> (the apex sits directly above the centre of the base).
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation. To place it in space
    /// (a base centre and an axis to rise along), pair it with a position; see <see cref="PlacedCone"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Cone : IEquatable<Cone>, IComparable<Cone>, IFormattable
    {
        #region Fields

        private readonly double baseRadius;
        private readonly double height;

        #endregion

        #region Constructors

        /// <summary>Construct a cone from its base radius and height.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="baseRadius"/> or <paramref name="height"/> is negative.</exception>
        public Cone(double baseRadius, double height)
        {
            if (baseRadius < 0) throw new ArgumentOutOfRangeException(nameof(baseRadius), baseRadius, NEGATIVE_RADIUS);
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), height, NEGATIVE_HEIGHT);

            this.baseRadius = baseRadius;
            this.height = height;
        }

        #endregion

        #region Accessors

        /// <summary>The radius of the circular base.</summary>
        public double BaseRadius { get { return this.baseRadius; } }

        /// <summary>The height from the base to the apex.</summary>
        public double Height { get { return this.height; } }

        /// <summary>The slant height from the base rim to the apex, <c>√(r² + h²)</c>.</summary>
        public double SlantHeight { get { return Math.Sqrt((this.baseRadius * this.baseRadius) + (this.height * this.height)); } }

        /// <summary>The half-angle at the apex, <c>atan(r / h)</c>.</summary>
        public Angle HalfAngle { get { return new Angle(Math.Atan2(this.baseRadius, this.height)); } }

        /// <summary>The area of the circular base, π·r².</summary>
        public double BaseArea { get { return Math.PI * this.baseRadius * this.baseRadius; } }

        /// <summary>The curved (side) area, π·r·slant.</summary>
        public double LateralArea { get { return Math.PI * this.baseRadius * this.SlantHeight; } }

        /// <summary>The total surface area, base + curved side.</summary>
        public double SurfaceArea { get { return this.BaseArea + this.LateralArea; } }

        /// <summary>The enclosed volume, ⅓·π·r²·h.</summary>
        public double Volume { get { return (1.0 / 3.0) * Math.PI * this.baseRadius * this.baseRadius * this.height; } }

        #endregion

        #region Comparison (by volume)

        /// <summary>Compare with another cone by <see cref="Volume"/>.</summary>
        public int CompareTo(Cone other) { return this.Volume.CompareTo(other.Volume); }

        /// <summary>Whether the left cone's volume is less than the right's.</summary>
        public static bool operator <(Cone c1, Cone c2) { return c1.Volume < c2.Volume; }

        /// <summary>Whether the left cone's volume is greater than the right's.</summary>
        public static bool operator >(Cone c1, Cone c2) { return c1.Volume > c2.Volume; }

        /// <summary>Whether the left cone's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Cone c1, Cone c2) { return c1.Volume <= c2.Volume; }

        /// <summary>Whether the left cone's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Cone c1, Cone c2) { return c1.Volume >= c2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of base radius and height.</summary>
        public static bool operator ==(Cone c1, Cone c2)
        {
            return c1.BaseRadius == c2.BaseRadius && c1.Height == c2.Height;
        }

        /// <summary>Inequality of base radius or height.</summary>
        public static bool operator !=(Cone c1, Cone c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Cone c && this.Equals(c);
        }

        /// <summary>Equality with another cone (base radius and height).</summary>
        public bool Equals(Cone other)
        {
            return this == other;
        }

        /// <summary>Equality with another cone within an absolute tolerance.</summary>
        public bool Equals(Cone other, double tolerance)
        {
            return this.baseRadius.AlmostEqualsWithAbsTolerance(other.BaseRadius, tolerance)
                && this.height.AlmostEqualsWithAbsTolerance(other.Height, tolerance);
        }

        /// <summary>A hash code derived from the base radius and height.</summary>
        public override int GetHashCode()
        {
            return this.baseRadius.GetHashCode() ^ this.height.GetHashCode();
        }

        /// <summary>Deconstruct into the base radius and height.</summary>
        public void Deconstruct(out double baseRadius, out double height)
        {
            baseRadius = this.baseRadius;
            height = this.height;
        }

        /// <summary>A string of the form <c>(r=baseRadius, h=height)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(r=baseRadius, h=height)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(r={0}, h={1})",
                this.baseRadius.ToString(format, formatProvider),
                this.height.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A cone radius cannot be negative.";
        private const string NEGATIVE_HEIGHT = "A cone height cannot be negative.";

        #endregion
    }
}
