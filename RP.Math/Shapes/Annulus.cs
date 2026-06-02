namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) annulus — a flat ring — described by its <see cref="InnerRadius"/> and
    /// <see cref="OuterRadius"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation: the filled region between
    /// two concentric circles. An inner radius of zero makes it a full disc. To place it on a plane in
    /// space, pair it with a position; see <see cref="PlacedAnnulus"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Annulus : IEquatable<Annulus>, IComparable<Annulus>, IFormattable
    {
        #region Fields

        private readonly double innerRadius;
        private readonly double outerRadius;

        #endregion

        #region Constructors

        /// <summary>Construct an annulus from its inner and outer radii.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a radius is negative.</exception>
        /// <exception cref="ArgumentException">Thrown if the inner radius exceeds the outer.</exception>
        public Annulus(double innerRadius, double outerRadius)
        {
            if (innerRadius < 0) throw new ArgumentOutOfRangeException(nameof(innerRadius), innerRadius, NEGATIVE_RADIUS);
            if (outerRadius < 0) throw new ArgumentOutOfRangeException(nameof(outerRadius), outerRadius, NEGATIVE_RADIUS);
            if (innerRadius > outerRadius) throw new ArgumentException(INNER_EXCEEDS_OUTER);

            this.innerRadius = innerRadius;
            this.outerRadius = outerRadius;
        }

        #endregion

        #region Accessors

        /// <summary>The inner radius (the hole).</summary>
        public double InnerRadius { get { return this.innerRadius; } }

        /// <summary>The outer radius (the rim).</summary>
        public double OuterRadius { get { return this.outerRadius; } }

        /// <summary>The radial width of the ring, outer − inner.</summary>
        public double Width { get { return this.outerRadius - this.innerRadius; } }

        /// <summary>The mean radius, halfway between the inner and outer edges.</summary>
        public double MeanRadius { get { return (this.innerRadius + this.outerRadius) / 2.0; } }

        /// <summary>The enclosed area of the ring, π·(outer² − inner²).</summary>
        public double Area
        {
            get { return Math.PI * ((this.outerRadius * this.outerRadius) - (this.innerRadius * this.innerRadius)); }
        }

        /// <summary>The total edge length, the two circumferences: 2·π·(outer + inner).</summary>
        public double Perimeter { get { return 2.0 * Math.PI * (this.outerRadius + this.innerRadius); } }

        #endregion

        #region Comparison (by area)

        /// <summary>Compare with another annulus by <see cref="Area"/>.</summary>
        public int CompareTo(Annulus other) { return this.Area.CompareTo(other.Area); }

        /// <summary>Whether the left annulus's area is less than the right's.</summary>
        public static bool operator <(Annulus a1, Annulus a2) { return a1.Area < a2.Area; }

        /// <summary>Whether the left annulus's area is greater than the right's.</summary>
        public static bool operator >(Annulus a1, Annulus a2) { return a1.Area > a2.Area; }

        /// <summary>Whether the left annulus's area is less than or equal to the right's.</summary>
        public static bool operator <=(Annulus a1, Annulus a2) { return a1.Area <= a2.Area; }

        /// <summary>Whether the left annulus's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Annulus a1, Annulus a2) { return a1.Area >= a2.Area; }

        #endregion

        #region Operators

        /// <summary>Equality of both radii.</summary>
        public static bool operator ==(Annulus a1, Annulus a2)
        {
            return a1.InnerRadius == a2.InnerRadius && a1.OuterRadius == a2.OuterRadius;
        }

        /// <summary>Inequality of either radius.</summary>
        public static bool operator !=(Annulus a1, Annulus a2)
        {
            return !(a1 == a2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Annulus a && this.Equals(a);
        }

        /// <summary>Equality with another annulus (both radii).</summary>
        public bool Equals(Annulus other)
        {
            return this == other;
        }

        /// <summary>Equality with another annulus within an absolute tolerance.</summary>
        public bool Equals(Annulus other, double tolerance)
        {
            return this.innerRadius.AlmostEqualsWithAbsTolerance(other.InnerRadius, tolerance)
                && this.outerRadius.AlmostEqualsWithAbsTolerance(other.OuterRadius, tolerance);
        }

        /// <summary>A hash code derived from the two radii.</summary>
        public override int GetHashCode()
        {
            return this.innerRadius.GetHashCode() ^ this.outerRadius.GetHashCode();
        }

        /// <summary>Deconstruct into the inner and outer radii.</summary>
        public void Deconstruct(out double innerRadius, out double outerRadius)
        {
            innerRadius = this.innerRadius;
            outerRadius = this.outerRadius;
        }

        /// <summary>A string of the form <c>(inner..outer)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(inner..outer)</c> where the radii use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}..{1})",
                this.innerRadius.ToString(format, formatProvider),
                this.outerRadius.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "An annulus radius cannot be negative.";
        private const string INNER_EXCEEDS_OUTER = "An annulus's inner radius cannot exceed its outer radius.";

        #endregion
    }
}
