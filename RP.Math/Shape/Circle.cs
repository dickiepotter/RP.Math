namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An immutable circle (a flat disc) in 3D space, defined by its centre, the unit normal of the
    /// plane it lies in, and its radius.
    /// </summary>
    /// <remarks>
    /// The normal supplied to the constructor is normalized; a zero normal defaults to +Z (the circle
    /// then lies in the XY plane).
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Circle : IEquatable<Circle>, IFormattable
    {
        #region Fields

        private readonly Vector center;
        private readonly Vector normal; // stored unit length
        private readonly double radius;

        #endregion

        #region Constructors

        /// <summary>Construct a circle from its centre, plane normal and radius.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public Circle(Vector center, Vector normal, double radius)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            }

            this.center = center;
            Vector unit = normal.NormalizeOrDefault();
            this.normal = unit.IsZero() ? new Vector(0, 0, 1) : unit;
            this.radius = radius;
        }

        #endregion

        #region Factories

        /// <summary>A circle lying in the XY plane (normal +Z) at the given centre and radius.</summary>
        public static Circle InXYPlane(Vector center, double radius)
        {
            return new Circle(center, new Vector(0, 0, 1), radius);
        }

        #endregion

        #region Accessors

        /// <summary>The centre of the circle.</summary>
        public Vector Center { get { return this.center; } }

        /// <summary>The radius of the circle.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The diameter of the circle.</summary>
        public double Diameter { get { return this.radius * 2.0; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.center; } }

        /// <summary>The unit normal of the plane the circle lies in.</summary>
        public Vector Normal { get { return this.normal; } }

        /// <summary>The supporting plane the circle lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.center, this.normal); } }

        /// <summary>The enclosed area, π·r².</summary>
        public double Area { get { return Math.PI * this.radius * this.radius; } }

        /// <summary>The circumference, 2·π·r.</summary>
        public double Perimeter { get { return 2.0 * Math.PI * this.radius; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the disc (on its plane and within its radius).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the disc within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector offset = point - this.center;
            double outOfPlane = Math.Abs(offset.DotProduct(this.normal));
            if (outOfPlane > tolerance)
            {
                return false;
            }

            double r = this.radius + tolerance;
            return offset.MagnitudeSquared <= r * r;
        }

        /// <summary>The point on the disc closest to <paramref name="point"/> (projected onto the plane, then clamped to the radius).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector offset = point - this.center;
            Vector inPlane = offset - (offset.DotProduct(this.normal) * this.normal);
            double distance = inPlane.Magnitude;
            if (distance <= this.radius || distance == 0)
            {
                return this.center + inPlane;
            }

            return this.center + (inPlane * (this.radius / distance));
        }

        #endregion

        #region Transformation (returns a new circle)

        /// <summary>A copy of the circle translated by <paramref name="offset"/>.</summary>
        public Circle Translate(Vector offset)
        {
            return new Circle(this.center + offset, this.normal, this.radius);
        }

        /// <summary>A copy of the circle with its radius scaled by <paramref name="factor"/>.</summary>
        public Circle Scale(double factor)
        {
            return new Circle(this.center, this.normal, this.radius * factor);
        }

        #endregion

        #region Operators

        /// <summary>Equality of centre, normal and radius.</summary>
        public static bool operator ==(Circle c1, Circle c2)
        {
            return c1.Center == c2.Center && c1.Normal == c2.Normal && c1.Radius == c2.Radius;
        }

        /// <summary>Inequality of centre, normal and radius.</summary>
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

        /// <summary>Equality with another circle (centre, normal and radius).</summary>
        public bool Equals(Circle other)
        {
            return this == other;
        }

        /// <summary>Equality with another circle within an absolute tolerance.</summary>
        public bool Equals(Circle other, double tolerance)
        {
            return this.center.Equals(other.Center, tolerance)
                && this.normal.Equals(other.Normal, tolerance)
                && this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance);
        }

        /// <summary>A hash code derived from the centre, normal and radius.</summary>
        public override int GetHashCode()
        {
            return this.center.GetHashCode() ^ this.normal.GetHashCode() ^ this.radius.GetHashCode();
        }

        /// <summary>Deconstruct into centre, normal and radius.</summary>
        public void Deconstruct(out Vector center, out Vector normal, out double radius)
        {
            center = this.center;
            normal = this.normal;
            radius = this.radius;
        }

        /// <summary>A string of the form <c>(center, n=normal, r=radius)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(center, n=normal, r=radius)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, n={1}, r={2})",
                this.center.ToString(format, formatProvider),
                this.normal.ToString(format, formatProvider),
                this.radius.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A circle radius cannot be negative.";

        #endregion
    }
}
