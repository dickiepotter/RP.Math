namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) sphere, described purely by its <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape — its size — with no position. Its volume and surface area
    /// depend solely on the radius, so spheres can be compared by size on their own without any notion of
    /// where they sit. To place one in space, pair it with a position; see <see cref="PlacedSphere"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Sphere : IEquatable<Sphere>, IComparable<Sphere>, IFormattable
    {
        #region Fields

        private readonly double radius;

        #endregion

        #region Constructors

        /// <summary>Construct a sphere from its radius.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public Sphere(double radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            this.radius = radius;
        }

        #endregion

        #region Constants

        /// <summary>The unit sphere: radius 1.</summary>
        public static readonly Sphere Unit = new Sphere(1);

        #endregion

        #region Accessors

        /// <summary>The radius of the sphere.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The diameter of the sphere, twice the radius.</summary>
        public double Diameter { get { return this.radius * 2.0; } }

        /// <summary>The enclosed volume, 4/3·π·r³.</summary>
        public double Volume { get { return (4.0 / 3.0) * Math.PI * this.radius * this.radius * this.radius; } }

        /// <summary>The surface area, 4·π·r².</summary>
        public double SurfaceArea { get { return 4.0 * Math.PI * this.radius * this.radius; } }

        #endregion

        #region Comparison (by volume)

        /// <summary>Compare with another sphere by <see cref="Volume"/> (equivalently by radius).</summary>
        public int CompareTo(Sphere other) { return this.radius.CompareTo(other.Radius); }

        /// <summary>Whether the left sphere's volume is less than the right's.</summary>
        public static bool operator <(Sphere s1, Sphere s2) { return s1.Radius < s2.Radius; }

        /// <summary>Whether the left sphere's volume is greater than the right's.</summary>
        public static bool operator >(Sphere s1, Sphere s2) { return s1.Radius > s2.Radius; }

        /// <summary>Whether the left sphere's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Sphere s1, Sphere s2) { return s1.Radius <= s2.Radius; }

        /// <summary>Whether the left sphere's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Sphere s1, Sphere s2) { return s1.Radius >= s2.Radius; }

        #endregion

        #region Operators

        /// <summary>Equality of radius.</summary>
        public static bool operator ==(Sphere s1, Sphere s2)
        {
            return s1.Radius == s2.Radius;
        }

        /// <summary>Inequality of radius.</summary>
        public static bool operator !=(Sphere s1, Sphere s2)
        {
            return !(s1 == s2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Sphere s && this.Equals(s);
        }

        /// <summary>Equality with another sphere (radius).</summary>
        public bool Equals(Sphere other)
        {
            return this == other;
        }

        /// <summary>Equality with another sphere within an absolute tolerance.</summary>
        public bool Equals(Sphere other, double tolerance)
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

        private const string NEGATIVE_RADIUS = "A sphere radius cannot be negative.";

        #endregion
    }
}
