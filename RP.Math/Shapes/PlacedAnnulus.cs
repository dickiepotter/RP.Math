namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An <see cref="Annulus"/> (flat ring) placed in 3D space by a <see cref="Pose"/> — the positioned
    /// partner of the conceptual <see cref="Annulus"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the ring lies in the XY plane centred at the origin; the pose maps that
    /// frame into the world. World queries split a local point into a height (local z) and a radial
    /// distance (local x,y), then test the radius against the inner and outer edges.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedAnnulus : IEquatable<PlacedAnnulus>, IFormattable
    {
        #region Fields

        private readonly Annulus shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed annulus from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedAnnulus(Annulus shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>An annulus lying flat in the world XY plane, centred at <paramref name="center"/>.</summary>
        public static PlacedAnnulus InXYPlane(Annulus shape, Vector center)
        {
            return new PlacedAnnulus(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) annulus.</summary>
        public Annulus Shape { get { return this.shape; } }

        /// <summary>The placement of the annulus.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit normal of the ring's plane (the pose's local +Z).</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The supporting plane the ring lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Center, this.Normal); } }

        /// <summary>The inner radius (from the conceptual shape).</summary>
        public double InnerRadius { get { return this.shape.InnerRadius; } }

        /// <summary>The outer radius (from the conceptual shape).</summary>
        public double OuterRadius { get { return this.shape.OuterRadius; } }

        /// <summary>The enclosed area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the filled ring (on its plane, between the two edges).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled ring within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > tolerance)
            {
                return false;
            }

            double r = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            return r >= this.shape.InnerRadius - tolerance && r <= this.shape.OuterRadius + tolerance;
        }

        /// <summary>
        /// The point on the filled ring closest to <paramref name="point"/> (projected onto the plane,
        /// then pushed in or out to the nearest edge if it falls in the hole or beyond the rim).
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double r = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));

            if (r >= this.shape.InnerRadius && r <= this.shape.OuterRadius)
            {
                return this.pose.Apply(new Vector(local.X, local.Y, 0)); // already over the ring
            }

            if (r == 0)
            {
                return this.pose.Apply(new Vector(this.shape.InnerRadius, 0, 0)); // centre of the hole: any inner-edge point
            }

            double target = r < this.shape.InnerRadius ? this.shape.InnerRadius : this.shape.OuterRadius;
            double scale = target / r;
            return this.pose.Apply(new Vector(local.X * scale, local.Y * scale, 0));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>Intersect the filled ring with an infinite <see cref="Line"/>.</summary>
        public bool TryIntersect(Line line, out Vector point)
        {
            return this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out point);
        }

        /// <summary>Intersect the filled ring with a <see cref="Ray"/> (hit must be at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            return this.TryIntersectLocal(ray.Origin, ray.Direction, requireForward: true, out point);
        }

        private bool TryIntersectLocal(Vector origin, Vector direction, bool requireForward, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(direction);

            if (ld.Z.AlmostEqualsWithAbsTolerance(0, double.Epsilon))
            {
                point = origin;
                return false;
            }

            double t = -lo.Z / ld.Z;
            if (requireForward && t < 0)
            {
                point = origin;
                return false;
            }

            Vector hit = lo + (t * ld);
            double r = Math.Sqrt((hit.X * hit.X) + (hit.Y * hit.Y));
            if (r >= this.shape.InnerRadius && r <= this.shape.OuterRadius)
            {
                point = this.pose.Apply(hit);
                return true;
            }

            point = origin;
            return false;
        }

        #endregion

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedAnnulus Translate(Vector offset)
        {
            return new PlacedAnnulus(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with both radii scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedAnnulus Scale(double factor)
        {
            return new PlacedAnnulus(new Annulus(this.shape.InnerRadius * factor, this.shape.OuterRadius * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedAnnulus Transform(Pose transform)
        {
            return new PlacedAnnulus(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedAnnulus a1, PlacedAnnulus a2)
        {
            return a1.Shape == a2.Shape && a1.Pose == a2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedAnnulus a1, PlacedAnnulus a2)
        {
            return !(a1 == a2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedAnnulus a && this.Equals(a);
        }

        /// <summary>Equality with another placed annulus (conceptual shape and pose).</summary>
        public bool Equals(PlacedAnnulus other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed annulus within an absolute tolerance.</summary>
        public bool Equals(PlacedAnnulus other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Annulus shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(inner..outer) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(inner..outer) @ pose]</c> where components use <paramref name="format"/>.</summary>
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
