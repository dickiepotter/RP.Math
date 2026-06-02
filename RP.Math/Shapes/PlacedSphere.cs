namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Sphere"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Sphere"/>.
    /// </summary>
    /// <remarks>
    /// A sphere is fully symmetric, so only the pose's position matters (its orientation is irrelevant);
    /// the world-space maths works directly from the <see cref="Center"/> and radius.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedSphere : IEquatable<PlacedSphere>, IFormattable
    {
        #region Fields

        private readonly Sphere shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed sphere from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedSphere(Sphere shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A sphere centred at <paramref name="center"/>.</summary>
        public static PlacedSphere At(Sphere shape, Vector center)
        {
            return new PlacedSphere(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) sphere.</summary>
        public Sphere Shape { get { return this.shape; } }

        /// <summary>The placement of the sphere (only its position is meaningful).</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The radius (from the conceptual shape).</summary>
        public double Radius { get { return this.shape.Radius; } }

        /// <summary>The diameter (from the conceptual shape).</summary>
        public double Diameter { get { return this.shape.Diameter; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere.</summary>
        public bool Contains(Vector point)
        {
            return this.Center.DistanceSquared(point) <= this.Radius * this.Radius;
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            double r = this.Radius + tolerance;
            return this.Center.DistanceSquared(point) <= r * r;
        }

        /// <summary>The point on the sphere's surface closest to <paramref name="point"/> (the centre for a point at the centre).</summary>
        public Vector ClosestSurfacePoint(Vector point)
        {
            Vector direction = point - this.Center;
            if (direction.Magnitude == 0)
            {
                return this.Center;
            }

            return this.Center + (direction.NormalizeOrDefault() * this.Radius);
        }

        /// <summary>The distance from <paramref name="point"/> to the surface (zero on it, negative inside).</summary>
        public double SignedDistanceTo(Vector point)
        {
            return this.Center.Distance(point) - this.Radius;
        }

        /// <summary>Whether this sphere overlaps <paramref name="other"/> (touching counts as intersecting).</summary>
        public bool Intersects(PlacedSphere other)
        {
            double sumRadii = this.Radius + other.Radius;
            return this.Center.DistanceSquared(other.Center) <= sumRadii * sumRadii;
        }

        /// <summary>
        /// The distance from <paramref name="point"/> to the sphere (zero when it lies on or within it).
        /// For the signed form (negative inside) use <see cref="SignedDistanceTo"/>.
        /// </summary>
        public double DistanceTo(Vector point)
        {
            return Math.Max(0, this.SignedDistanceTo(point));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the sphere with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/>, <paramref name="far"/>, equal when the line grazes);
        /// false when the line misses.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector m = line.Point - this.Center;
            double b = m.DotProduct(line.Direction);
            double c = m.MagnitudeSquared - (this.Radius * this.Radius);
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
        /// ahead of the origin (the forward exit if the origin is inside); false when missed or behind.
        /// </summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector m = ray.Origin - this.Center;
            double b = m.DotProduct(ray.Direction);
            double c = m.MagnitudeSquared - (this.Radius * this.Radius);
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
                t = -b + s; // origin inside: take the forward exit
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

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedSphere Translate(Vector offset)
        {
            return new PlacedSphere(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the radius scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedSphere Scale(double factor)
        {
            return new PlacedSphere(new Sphere(this.shape.Radius * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedSphere Transform(Pose transform)
        {
            return new PlacedSphere(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedSphere s1, PlacedSphere s2)
        {
            return s1.Shape == s2.Shape && s1.Pose == s2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedSphere s1, PlacedSphere s2)
        {
            return !(s1 == s2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedSphere s && this.Equals(s);
        }

        /// <summary>Equality with another placed sphere (conceptual shape and pose).</summary>
        public bool Equals(PlacedSphere other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed sphere within an absolute tolerance.</summary>
        public bool Equals(PlacedSphere other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Sphere shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(r) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(r) @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion
    }
}
