namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) capsule — a cylinder with a hemispherical cap at each end — described
    /// by its <see cref="Radius"/> and <see cref="CylinderHeight"/> (the length of the straight middle
    /// section, between the two hemisphere centres).
    /// </summary>
    /// <remarks>
    /// A capsule is the set of all points within <see cref="Radius"/> of a central line segment, so it is
    /// equivalently a "swept sphere". This type holds only the intrinsic shape; to place it in space, pair
    /// it with a position; see <see cref="PlacedCapsule"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Capsule : IEquatable<Capsule>, IComparable<Capsule>, IFormattable
    {
        #region Fields

        private readonly double radius;
        private readonly double cylinderHeight;

        #endregion

        #region Constructors

        /// <summary>Construct a capsule from its radius and the height of its straight cylindrical middle.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> or <paramref name="cylinderHeight"/> is negative.</exception>
        public Capsule(double radius, double cylinderHeight)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            if (cylinderHeight < 0) throw new ArgumentOutOfRangeException(nameof(cylinderHeight), cylinderHeight, NEGATIVE_HEIGHT);

            this.radius = radius;
            this.cylinderHeight = cylinderHeight;
        }

        #endregion

        #region Accessors

        /// <summary>The radius (of both the cylindrical middle and the hemispherical caps).</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The height of the straight cylindrical section (between the two hemisphere centres).</summary>
        public double CylinderHeight { get { return this.cylinderHeight; } }

        /// <summary>The overall length end to end, cylinder height plus the two cap radii.</summary>
        public double TotalHeight { get { return this.cylinderHeight + (2.0 * this.radius); } }

        /// <summary>
        /// The enclosed volume: the cylindrical middle plus the two hemispheres (a whole sphere),
        /// <c>π·r²·h + (4/3)·π·r³</c>.
        /// </summary>
        public double Volume
        {
            get
            {
                double cylinder = Math.PI * this.radius * this.radius * this.cylinderHeight;
                double sphere = (4.0 / 3.0) * Math.PI * this.radius * this.radius * this.radius;
                return cylinder + sphere;
            }
        }

        /// <summary>
        /// The surface area: the curved side of the middle plus the two hemispheres (a whole sphere),
        /// <c>2·π·r·h + 4·π·r²</c>.
        /// </summary>
        public double SurfaceArea
        {
            get { return (2.0 * Math.PI * this.radius * this.cylinderHeight) + (4.0 * Math.PI * this.radius * this.radius); }
        }

        #endregion

        #region Comparison (by volume)

        /// <summary>Compare with another capsule by <see cref="Volume"/>.</summary>
        public int CompareTo(Capsule other) { return this.Volume.CompareTo(other.Volume); }

        /// <summary>Whether the left capsule's volume is less than the right's.</summary>
        public static bool operator <(Capsule c1, Capsule c2) { return c1.Volume < c2.Volume; }

        /// <summary>Whether the left capsule's volume is greater than the right's.</summary>
        public static bool operator >(Capsule c1, Capsule c2) { return c1.Volume > c2.Volume; }

        /// <summary>Whether the left capsule's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Capsule c1, Capsule c2) { return c1.Volume <= c2.Volume; }

        /// <summary>Whether the left capsule's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Capsule c1, Capsule c2) { return c1.Volume >= c2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of radius and cylinder height.</summary>
        public static bool operator ==(Capsule c1, Capsule c2)
        {
            return c1.Radius == c2.Radius && c1.CylinderHeight == c2.CylinderHeight;
        }

        /// <summary>Inequality of radius or cylinder height.</summary>
        public static bool operator !=(Capsule c1, Capsule c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Capsule c && this.Equals(c);
        }

        /// <summary>Equality with another capsule (radius and cylinder height).</summary>
        public bool Equals(Capsule other)
        {
            return this == other;
        }

        /// <summary>Equality with another capsule within an absolute tolerance.</summary>
        public bool Equals(Capsule other, double tolerance)
        {
            return this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance)
                && this.cylinderHeight.AlmostEqualsWithAbsTolerance(other.CylinderHeight, tolerance);
        }

        /// <summary>A hash code derived from the radius and cylinder height.</summary>
        public override int GetHashCode()
        {
            return this.radius.GetHashCode() ^ this.cylinderHeight.GetHashCode();
        }

        /// <summary>Deconstruct into the radius and cylinder height.</summary>
        public void Deconstruct(out double radius, out double cylinderHeight)
        {
            radius = this.radius;
            cylinderHeight = this.cylinderHeight;
        }

        /// <summary>A string of the form <c>(r=radius, h=cylinderHeight)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(r=radius, h=cylinderHeight)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "(r={0}, h={1})",
                this.radius.ToString(format, formatProvider),
                this.cylinderHeight.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A capsule radius cannot be negative.";
        private const string NEGATIVE_HEIGHT = "A capsule cylinder height cannot be negative.";

        #endregion
    }
}
