namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) torus — a ring (doughnut) — described by its <see cref="MajorRadius"/>
    /// (from the centre to the middle of the tube) and <see cref="MinorRadius"/> (the tube's own radius).
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation. The usual "ring" torus
    /// has the major radius larger than the minor; equal radii give a horn torus and a larger minor gives
    /// a self-intersecting spindle torus (still valid as a value). To place it in space, pair it with a
    /// position; see <see cref="PlacedTorus"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Torus : IEquatable<Torus>, IComparable<Torus>, IFormattable
    {
        #region Fields

        private readonly double majorRadius;
        private readonly double minorRadius;

        #endregion

        #region Constructors

        /// <summary>Construct a torus from its major radius (centre to tube middle) and minor radius (tube).</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a radius is negative.</exception>
        public Torus(double majorRadius, double minorRadius)
        {
            if (majorRadius < 0) throw new ArgumentOutOfRangeException(nameof(majorRadius), majorRadius, NEGATIVE_RADIUS);
            if (minorRadius < 0) throw new ArgumentOutOfRangeException(nameof(minorRadius), minorRadius, NEGATIVE_RADIUS);

            this.majorRadius = majorRadius;
            this.minorRadius = minorRadius;
        }

        #endregion

        #region Accessors

        /// <summary>The major radius: from the centre to the middle of the tube.</summary>
        public double MajorRadius { get { return this.majorRadius; } }

        /// <summary>The minor radius: the radius of the tube itself.</summary>
        public double MinorRadius { get { return this.minorRadius; } }

        /// <summary>The outermost radius, major + minor.</summary>
        public double OuterRadius { get { return this.majorRadius + this.minorRadius; } }

        /// <summary>The radius of the central hole, major − minor (zero or negative for horn/spindle tori).</summary>
        public double InnerRadius { get { return this.majorRadius - this.minorRadius; } }

        /// <summary>The enclosed volume, 2·π²·R·r².</summary>
        public double Volume { get { return 2.0 * Math.PI * Math.PI * this.majorRadius * this.minorRadius * this.minorRadius; } }

        /// <summary>The surface area, 4·π²·R·r.</summary>
        public double SurfaceArea { get { return 4.0 * Math.PI * Math.PI * this.majorRadius * this.minorRadius; } }

        #endregion

        #region Comparison (by volume)

        /// <summary>Compare with another torus by <see cref="Volume"/>.</summary>
        public int CompareTo(Torus other) { return this.Volume.CompareTo(other.Volume); }

        /// <summary>Whether the left torus's volume is less than the right's.</summary>
        public static bool operator <(Torus t1, Torus t2) { return t1.Volume < t2.Volume; }

        /// <summary>Whether the left torus's volume is greater than the right's.</summary>
        public static bool operator >(Torus t1, Torus t2) { return t1.Volume > t2.Volume; }

        /// <summary>Whether the left torus's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Torus t1, Torus t2) { return t1.Volume <= t2.Volume; }

        /// <summary>Whether the left torus's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Torus t1, Torus t2) { return t1.Volume >= t2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of both radii.</summary>
        public static bool operator ==(Torus t1, Torus t2)
        {
            return t1.MajorRadius == t2.MajorRadius && t1.MinorRadius == t2.MinorRadius;
        }

        /// <summary>Inequality of either radius.</summary>
        public static bool operator !=(Torus t1, Torus t2)
        {
            return !(t1 == t2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Torus t && this.Equals(t);
        }

        /// <summary>Equality with another torus (both radii).</summary>
        public bool Equals(Torus other)
        {
            return this == other;
        }

        /// <summary>Equality with another torus within an absolute tolerance.</summary>
        public bool Equals(Torus other, double tolerance)
        {
            return this.majorRadius.AlmostEqualsWithAbsTolerance(other.MajorRadius, tolerance)
                && this.minorRadius.AlmostEqualsWithAbsTolerance(other.MinorRadius, tolerance);
        }

        /// <summary>A hash code derived from the two radii.</summary>
        public override int GetHashCode()
        {
            return this.majorRadius.GetHashCode() ^ this.minorRadius.GetHashCode();
        }

        /// <summary>Deconstruct into the major and minor radii.</summary>
        public void Deconstruct(out double majorRadius, out double minorRadius)
        {
            majorRadius = this.majorRadius;
            minorRadius = this.minorRadius;
        }

        /// <summary>A string of the form <c>(R=major, r=minor)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(R=major, r=minor)</c> where the radii use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(R={0}, r={1})",
                this.majorRadius.ToString(format, formatProvider),
                this.minorRadius.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A torus radius cannot be negative.";

        #endregion
    }
}
