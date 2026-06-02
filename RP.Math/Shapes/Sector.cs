namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) circular sector — a "pie slice" — described by its <see cref="Radius"/>
    /// and the <see cref="Angle"/> it sweeps.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation: the region bounded by two
    /// radii and the arc between them. A sweep of a full turn is the whole disc. To place it on a plane in
    /// space (anchored at its apex — the circle's centre), pair it with a position; see
    /// <see cref="PlacedSector"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Sector : IEquatable<Sector>, IComparable<Sector>, IFormattable
    {
        #region Fields

        private readonly double radius;
        private readonly Angle angle;

        #endregion

        #region Constructors

        /// <summary>Construct a sector from its radius and the angle it sweeps.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public Sector(double radius, Angle angle)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);

            this.radius = radius;
            this.angle = angle;
        }

        #endregion

        #region Accessors

        /// <summary>The radius of the sector.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The angle the sector sweeps.</summary>
        public Angle Angle { get { return this.angle; } }

        /// <summary>The enclosed area, ½·r²·θ (with θ in radians).</summary>
        public double Area { get { return 0.5 * this.radius * this.radius * this.angle.Rad; } }

        /// <summary>The curved arc length along the rim, r·θ.</summary>
        public double ArcLength { get { return this.radius * this.angle.Rad; } }

        /// <summary>The perimeter: the arc plus the two straight radii, r·θ + 2r.</summary>
        public double Perimeter { get { return this.ArcLength + (2.0 * this.radius); } }

        /// <summary>
        /// The straight-line distance between the two arc ends (the chord across the sector),
        /// <c>2·r·sin(θ/2)</c>.
        /// </summary>
        public double ChordLength { get { return 2.0 * this.radius * Math.Sin(this.angle.Rad / 2.0); } }

        #endregion

        #region Comparison (by area)

        /// <summary>Compare with another sector by <see cref="Area"/>.</summary>
        public int CompareTo(Sector other) { return this.Area.CompareTo(other.Area); }

        /// <summary>Whether the left sector's area is less than the right's.</summary>
        public static bool operator <(Sector s1, Sector s2) { return s1.Area < s2.Area; }

        /// <summary>Whether the left sector's area is greater than the right's.</summary>
        public static bool operator >(Sector s1, Sector s2) { return s1.Area > s2.Area; }

        /// <summary>Whether the left sector's area is less than or equal to the right's.</summary>
        public static bool operator <=(Sector s1, Sector s2) { return s1.Area <= s2.Area; }

        /// <summary>Whether the left sector's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Sector s1, Sector s2) { return s1.Area >= s2.Area; }

        #endregion

        #region Operators

        /// <summary>Equality of radius and swept angle.</summary>
        public static bool operator ==(Sector s1, Sector s2)
        {
            return s1.Radius == s2.Radius && s1.Angle == s2.Angle;
        }

        /// <summary>Inequality of radius or swept angle.</summary>
        public static bool operator !=(Sector s1, Sector s2)
        {
            return !(s1 == s2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Sector s && this.Equals(s);
        }

        /// <summary>Equality with another sector (radius and angle).</summary>
        public bool Equals(Sector other)
        {
            return this == other;
        }

        /// <summary>Equality with another sector within an absolute tolerance (radius and angle in radians).</summary>
        public bool Equals(Sector other, double tolerance)
        {
            return this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance)
                && this.angle.Rad.AlmostEqualsWithAbsTolerance(other.Angle.Rad, tolerance);
        }

        /// <summary>A hash code derived from the radius and angle.</summary>
        public override int GetHashCode()
        {
            return this.radius.GetHashCode() ^ this.angle.GetHashCode();
        }

        /// <summary>Deconstruct into the radius and angle.</summary>
        public void Deconstruct(out double radius, out Angle angle)
        {
            radius = this.radius;
            angle = this.angle;
        }

        /// <summary>A string of the form <c>(r=radius, θ=angle)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(r=radius, θ=angle°)</c> where the radius uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(r={0}, θ={1}°)",
                this.radius.ToString(format, formatProvider),
                this.angle.Deg.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A sector radius cannot be negative.";

        #endregion
    }
}
