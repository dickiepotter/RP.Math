namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An immutable sphere in 3D space, defined by its centre and radius (centre-anchored per the
    /// library convention).
    /// </summary>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Sphere : IEquatable<Sphere>, IFormattable
    {
        #region Fields

        private readonly Vector center;
        private readonly double radius;

        #endregion

        #region Constructors

        /// <summary>Construct a sphere from its centre and radius.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public Sphere(Vector center, double radius)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            }

            this.center = center;
            this.radius = radius;
        }

        #endregion

        #region Constants

        /// <summary>The unit sphere: centred at the origin with radius 1.</summary>
        public static readonly Sphere Unit = new Sphere(new Vector(0, 0, 0), 1);

        #endregion

        #region Accessors

        /// <summary>The centre of the sphere.</summary>
        public Vector Center { get { return this.center; } }

        /// <summary>The radius of the sphere.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The diameter of the sphere.</summary>
        public double Diameter { get { return this.radius * 2.0; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.center; } }

        /// <summary>The enclosed volume, 4/3·π·r³.</summary>
        public double Volume { get { return (4.0 / 3.0) * Math.PI * this.radius * this.radius * this.radius; } }

        /// <summary>The surface area, 4·π·r².</summary>
        public double SurfaceArea { get { return 4.0 * Math.PI * this.radius * this.radius; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere.</summary>
        public bool Contains(Vector point)
        {
            return this.center.DistanceSquared(point) <= this.radius * this.radius;
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            double r = this.radius + tolerance;
            return this.center.DistanceSquared(point) <= r * r;
        }

        /// <summary>The point on the sphere's surface closest to <paramref name="point"/>.</summary>
        /// <remarks>For a point at the centre the direction is undefined; the centre is returned.</remarks>
        public Vector ClosestSurfacePoint(Vector point)
        {
            Vector direction = point - this.center;
            if (direction.Magnitude == 0)
            {
                return this.center;
            }

            return this.center + (direction.NormalizeOrDefault() * this.radius);
        }

        /// <summary>The distance from <paramref name="point"/> to the sphere's surface (zero on the surface, negative inside).</summary>
        public double SignedDistanceTo(Vector point)
        {
            return this.center.Distance(point) - this.radius;
        }

        /// <summary>Whether this sphere overlaps <paramref name="other"/> (touching counts as intersecting).</summary>
        public bool Intersects(Sphere other)
        {
            double sumRadii = this.radius + other.Radius;
            return this.center.DistanceSquared(other.Center) <= sumRadii * sumRadii;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the sphere with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/> and <paramref name="far"/>, equal when the line grazes
        /// the sphere); false when the line misses entirely.
        /// </summary>
        /// <remarks>
        /// Maths: substitute the line <c>P + t·D</c> into <c>|X − C|² = r²</c>. With unit direction
        /// <c>D</c> this is a quadratic in <c>t</c> whose discriminant is <c>b² − c</c> for
        /// <c>b = (P − C)·D</c> and <c>c = |P − C|² − r²</c>; a negative discriminant means no real crossing.
        /// </remarks>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector m = line.Point - this.center;
            double b = m.DotProduct(line.Direction);
            double c = m.MagnitudeSquared - (this.radius * this.radius);
            double disc = (b * b) - c;
            if (disc < 0)
            {
                near = far = line.Point;
                return false;
            }

            double s = Math.Sqrt(disc);
            near = line.PointAt(-b - s);
            far = line.PointAt(-b + s);
            return true;
        }

        /// <summary>
        /// Intersect the sphere with a <see cref="Ray"/>. Returns true and the nearest surface point at or
        /// ahead of the ray's origin (if the origin is inside the sphere, the forward exit point); false
        /// when the sphere is missed or lies entirely behind the ray.
        /// </summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector m = ray.Origin - this.center;
            double b = m.DotProduct(ray.Direction);
            double c = m.MagnitudeSquared - (this.radius * this.radius);
            double disc = (b * b) - c;
            if (disc < 0)
            {
                point = ray.Origin;
                return false;
            }

            double s = Math.Sqrt(disc);
            double t = -b - s;
            if (t < 0)
            {
                t = -b + s; // origin is inside the sphere: take the forward exit point
            }

            if (t < 0)
            {
                point = ray.Origin; // the sphere is behind the ray
                return false;
            }

            point = ray.PointAt(t);
            return true;
        }

        #endregion

        #region Transformation (returns a new sphere)

        /// <summary>A copy of the sphere translated by <paramref name="offset"/>.</summary>
        public Sphere Translate(Vector offset)
        {
            return new Sphere(this.center + offset, this.radius);
        }

        /// <summary>A copy of the sphere with its radius scaled by <paramref name="factor"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="factor"/> is negative.</exception>
        public Sphere Scale(double factor)
        {
            return new Sphere(this.center, this.radius * factor);
        }

        #endregion

        #region Operators

        /// <summary>Equality of centre and radius.</summary>
        public static bool operator ==(Sphere s1, Sphere s2)
        {
            return s1.Center == s2.Center && s1.Radius == s2.Radius;
        }

        /// <summary>Inequality of centre and radius.</summary>
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

        /// <summary>Equality with another sphere (centre and radius).</summary>
        public bool Equals(Sphere other)
        {
            return this == other;
        }

        /// <summary>Equality with another sphere within an absolute tolerance.</summary>
        public bool Equals(Sphere other, double tolerance)
        {
            return this.center.Equals(other.Center, tolerance)
                && this.radius.AlmostEqualsWithAbsTolerance(other.Radius, tolerance);
        }

        /// <summary>A hash code derived from the centre and radius.</summary>
        public override int GetHashCode()
        {
            return this.center.GetHashCode() ^ this.radius.GetHashCode();
        }

        /// <summary>Deconstruct into centre and radius, enabling <c>var (center, radius) = sphere;</c>.</summary>
        public void Deconstruct(out Vector center, out double radius)
        {
            center = this.center;
            radius = this.radius;
        }

        /// <summary>A string of the form <c>(center, r=radius)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(center, r=radius)</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, r={1})",
                this.center.ToString(format, formatProvider),
                this.radius.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A sphere radius cannot be negative.";

        #endregion
    }
}
