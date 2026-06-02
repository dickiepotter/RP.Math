namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An <see cref="Ellipsoid"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of
    /// the conceptual <see cref="Ellipsoid"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the ellipsoid is axis-aligned and centred at the origin, with semi-axes
    /// along local X, Y and Z. The pose maps that frame into the world. Line/ray intersection is exact:
    /// scaling each local axis by its reciprocal semi-axis turns the ellipsoid into a unit sphere, so the
    /// crossing reduces to a quadratic in the same line parameter.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedEllipsoid : IEquatable<PlacedEllipsoid>, IFormattable
    {
        #region Fields

        private readonly Ellipsoid shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed ellipsoid from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedEllipsoid(Ellipsoid shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>An axis-aligned ellipsoid (no rotation) centred at <paramref name="center"/>.</summary>
        public static PlacedEllipsoid AxisAligned(Ellipsoid shape, Vector center)
        {
            return new PlacedEllipsoid(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) ellipsoid.</summary>
        public Ellipsoid Shape { get { return this.shape; } }

        /// <summary>The placement of the ellipsoid.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the ellipsoid.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the ellipsoid, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            double a = this.shape.SemiAxisX + tolerance;
            double b = this.shape.SemiAxisY + tolerance;
            double c = this.shape.SemiAxisZ + tolerance;
            if (a <= 0 || b <= 0 || c <= 0)
            {
                return false;
            }

            return ((local.X * local.X) / (a * a))
                 + ((local.Y * local.Y) / (b * b))
                 + ((local.Z * local.Z) / (c * c)) <= 1.0;
        }

        /// <summary>
        /// The point on or within the ellipsoid closest to <paramref name="point"/>. An interior point
        /// returns itself; an exterior point's nearest boundary point is found by solving the
        /// Lagrange-multiplier condition <c>x_i = e_i²·q_i / (t + e_i²)</c> for the multiplier <c>t</c>
        /// (a monotonic equation solved by bisection).
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double a = this.shape.SemiAxisX, b = this.shape.SemiAxisY, c = this.shape.SemiAxisZ;
            if (a <= 0 || b <= 0 || c <= 0)
            {
                return this.Center; // degenerate
            }

            double inside = ((local.X * local.X) / (a * a)) + ((local.Y * local.Y) / (b * b)) + ((local.Z * local.Z) / (c * c));
            if (inside <= 1.0)
            {
                return this.pose.Apply(local); // already inside the filled ellipsoid
            }

            double a2 = a * a, b2 = b * b, c2 = c * c;
            double qx = local.X, qy = local.Y, qz = local.Z;

            // Solve F(t) = Σ (e_i·q_i / (t + e_i²))² − 1 = 0 for t ≥ 0 (F is decreasing there).
            double lo = 0;
            double hi = Math.Sqrt((a * qx * a * qx) + (b * qy * b * qy) + (c * qz * c * qz)); // F(hi) ≤ 0
            for (int i = 0; i < 80; i++)
            {
                double t = (lo + hi) / 2.0;
                double fx = (a * qx) / (t + a2);
                double fy = (b * qy) / (t + b2);
                double fz = (c * qz) / (t + c2);
                double f = (fx * fx) + (fy * fy) + (fz * fz) - 1.0;
                if (f > 0)
                {
                    lo = t;
                }
                else
                {
                    hi = t;
                }
            }

            double tt = (lo + hi) / 2.0;
            return this.pose.Apply(new Vector(
                (a2 * qx) / (tt + a2),
                (b2 * qy) / (tt + b2),
                (c2 * qz) / (tt + c2)));
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the ellipsoid with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/>, <paramref name="far"/>); false when the line misses.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector lo = this.pose.ApplyInverse(line.Point);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(line.Direction);
            if (!this.SolveSphereSpace(lo, ld, out double tNear, out double tFar))
            {
                near = far = line.Point;
                return false;
            }

            near = this.pose.Apply(lo + (tNear * ld));
            far = this.pose.Apply(lo + (tFar * ld));
            return true;
        }

        /// <summary>Intersect the ellipsoid with a <see cref="Ray"/> (nearest hit at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(ray.Origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(ray.Direction);
            if (!this.SolveSphereSpace(lo, ld, out double tNear, out double tFar) || tFar < 0)
            {
                point = ray.Origin;
                return false;
            }

            double t = tNear >= 0 ? tNear : tFar;
            point = this.pose.Apply(lo + (t * ld));
            return true;
        }

        /// <summary>
        /// Solve the crossing parameters by scaling local space so the ellipsoid becomes a unit sphere
        /// (divide each axis by its semi-axis), then intersecting that sphere. The parameter <c>t</c> is
        /// unchanged by the scaling, so it indexes the original line directly.
        /// </summary>
        private bool SolveSphereSpace(Vector lo, Vector ld, out double tNear, out double tFar)
        {
            tNear = tFar = 0;
            double a = this.shape.SemiAxisX, b = this.shape.SemiAxisY, c = this.shape.SemiAxisZ;
            if (a <= 0 || b <= 0 || c <= 0)
            {
                return false;
            }

            Vector o = new Vector(lo.X / a, lo.Y / b, lo.Z / c);
            Vector d = new Vector(ld.X / a, ld.Y / b, ld.Z / c);

            double qa = d.DotProduct(d);
            double qb = 2.0 * o.DotProduct(d);
            double qc = o.DotProduct(o) - 1.0;
            double disc = (qb * qb) - (4.0 * qa * qc);
            if (disc < 0 || qa == 0)
            {
                return false;
            }

            double s = Math.Sqrt(disc);
            tNear = (-qb - s) / (2.0 * qa);
            tFar = (-qb + s) / (2.0 * qa);
            return true;
        }

        #endregion

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedEllipsoid Translate(Vector offset)
        {
            return new PlacedEllipsoid(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with all three semi-axes scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedEllipsoid Scale(double factor)
        {
            return new PlacedEllipsoid(
                new Ellipsoid(this.shape.SemiAxisX * factor, this.shape.SemiAxisY * factor, this.shape.SemiAxisZ * factor),
                this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedEllipsoid Transform(Pose transform)
        {
            return new PlacedEllipsoid(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedEllipsoid e1, PlacedEllipsoid e2)
        {
            return e1.Shape == e2.Shape && e1.Pose == e2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedEllipsoid e1, PlacedEllipsoid e2)
        {
            return !(e1 == e2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedEllipsoid e && this.Equals(e);
        }

        /// <summary>Equality with another placed ellipsoid (conceptual shape and pose).</summary>
        public bool Equals(PlacedEllipsoid other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed ellipsoid within an absolute tolerance.</summary>
        public bool Equals(PlacedEllipsoid other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Ellipsoid shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(a, b, c) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(a, b, c) @ pose]</c> where components use <paramref name="format"/>.</summary>
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
