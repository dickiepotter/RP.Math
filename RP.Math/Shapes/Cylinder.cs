namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) circular cylinder, described purely by its <see cref="Radius"/> and
    /// <see cref="Height"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation. Its volume, surface area
    /// and the like depend solely on the radius and height, so cylinders can be compared by size on their
    /// own. To place one in space (a centre and an axis to run along), pair it with a position; see
    /// <see cref="PlacedCylinder"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Cylinder : IEquatable<Cylinder>, IComparable<Cylinder>, IFormattable
    {
        #region Fields

        private readonly double radius;
        private readonly double height;

        #endregion

        #region Constructors

        /// <summary>Construct a cylinder from its radius and height.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> or <paramref name="height"/> is negative.</exception>
        public Cylinder(double radius, double height)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), height, NEGATIVE_HEIGHT);

            this.radius = radius;
            this.height = height;
        }

        #endregion

        #region Accessors

        /// <summary>The radius of the cylinder.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The height (length along the axis).</summary>
        public double Height { get { return this.height; } }

        /// <summary>The diameter, twice the radius.</summary>
        public double Diameter { get { return this.radius * 2.0; } }

        /// <summary>The area of one circular cap, π·r².</summary>
        public double BaseArea { get { return Math.PI * this.radius * this.radius; } }

        /// <summary>The enclosed volume, π·r²·h.</summary>
        public double Volume { get { return Math.PI * this.radius * this.radius * this.height; } }

        /// <summary>The curved (side) area, 2·π·r·h, excluding the caps.</summary>
        public double LateralArea { get { return 2.0 * Math.PI * this.radius * this.height; } }

        /// <summary>The total surface area, the curved side plus the two circular caps: 2·π·r·(r + h).</summary>
        public double SurfaceArea { get { return 2.0 * Math.PI * this.radius * (this.radius + this.height); } }

        #endregion

        #region Comparison (by volume)

        /// <summary>
        /// Compare with another cylinder by <see cref="Volume"/> — the natural "which is bigger" ordering,
        /// independent of proportions. Mirrors how <see cref="Vector"/> compares by magnitude.
        /// </summary>
        public int CompareTo(Cylinder other)
        {
            return this.Volume.CompareTo(other.Volume);
        }

        /// <summary>Whether the left cylinder's volume is less than the right's.</summary>
        public static bool operator <(Cylinder c1, Cylinder c2) { return c1.Volume < c2.Volume; }

        /// <summary>Whether the left cylinder's volume is greater than the right's.</summary>
        public static bool operator >(Cylinder c1, Cylinder c2) { return c1.Volume > c2.Volume; }

        /// <summary>Whether the left cylinder's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Cylinder c1, Cylinder c2) { return c1.Volume <= c2.Volume; }

        /// <summary>Whether the left cylinder's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Cylinder c1, Cylinder c2) { return c1.Volume >= c2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of radius and height.</summary>
        public static bool operator ==(Cylinder c1, Cylinder c2)
        {
            return c1.Radius == c2.Radius && c1.Height == c2.Height;
        }

        /// <summary>Inequality of radius or height.</summary>
        public static bool operator !=(Cylinder c1, Cylinder c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Cylinder c && this.Equals(c);
        }

        /// <summary>Equality with another cylinder (radius and height).</summary>
        public bool Equals(Cylinder other)
        {
            return this == other;
        }

        /// <summary>Equality with another cylinder within an absolute tolerance.</summary>
        public bool Equals(Cylinder other, double tolerance)
        {
            return this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance)
                && this.height.AlmostEqualsWithAbsTolerance(other.Height, tolerance);
        }

        /// <summary>A hash code derived from the radius and height.</summary>
        public override int GetHashCode()
        {
            return this.radius.GetHashCode() ^ this.height.GetHashCode();
        }

        /// <summary>Deconstruct into radius and height.</summary>
        public void Deconstruct(out double radius, out double height)
        {
            radius = this.radius;
            height = this.height;
        }

        /// <summary>A string of the form <c>(r=radius, h=height)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(r=radius, h=height)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(r={0}, h={1})",
                this.radius.ToString(format, formatProvider),
                this.height.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A cylinder radius cannot be negative.";
        private const string NEGATIVE_HEIGHT = "A cylinder height cannot be negative.";

        #endregion
    }
}
