namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Cone"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Cone"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the cone stands with its base disc on the <c>z = 0</c> plane (centred at the
    /// origin) and its apex at local <c>(0, 0, height)</c>; the radius tapers linearly from
    /// <see cref="Cone.BaseRadius"/> at the base to zero at the apex. The pose maps that frame into the
    /// world, so the pose position is the <b>base centre</b>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedCone : IEquatable<PlacedCone>, IFormattable
    {
        #region Fields

        private readonly Cone shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed cone from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedCone(Cone shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A cone standing on the world XY plane (apex pointing along +Z), base centred at <paramref name="baseCenter"/>.</summary>
        public static PlacedCone Upright(Cone shape, Vector baseCenter)
        {
            return new PlacedCone(shape, Pose.At(baseCenter));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) cone.</summary>
        public Cone Shape { get { return this.shape; } }

        /// <summary>The placement of the cone.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre of the base disc in world space.</summary>
        public Vector BaseCenter { get { return this.pose.Position; } }

        /// <summary>The apex in world space.</summary>
        public Vector Apex { get { return this.pose.Apply(new Vector(0, 0, this.shape.Height)); } }

        /// <summary>The unit axis from base to apex (the pose's local +Z).</summary>
        public Vector Axis { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The centroid in world space (one quarter of the height up from the base).</summary>
        public Vector Centroid { get { return this.pose.Apply(new Vector(0, 0, this.shape.Height / 4.0)); } }

        /// <summary>The base radius (from the conceptual shape).</summary>
        public double BaseRadius { get { return this.shape.BaseRadius; } }

        /// <summary>The height (from the conceptual shape).</summary>
        public double Height { get { return this.shape.Height; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The total surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the cone.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the cone, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            double h = this.shape.Height;
            if (h == 0 || local.Z < -tolerance || local.Z > h + tolerance)
            {
                return false;
            }

            double zc = local.Z < 0 ? 0 : (local.Z > h ? h : local.Z);
            double allowed = this.shape.BaseRadius * (h - zc) / h;
            double radial = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            return radial <= allowed + tolerance;
        }

        /// <summary>
        /// The point on or within the cone closest to <paramref name="point"/>. Solved in the cone's
        /// axial–radial half-plane, where the solid cone is the triangle (base centre, base rim, apex);
        /// the nearest point there is mapped back out at the query's own azimuth.
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double radial = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            double h = this.shape.Height;
            double r = this.shape.BaseRadius;

            // (z, ρ) half-plane: closest point of the query to the cone's cross-section triangle.
            Vector nearest = ClosestPointOnTriangle(
                new Vector(0, 0, 0),     // base centre  (z = 0, ρ = 0)
                new Vector(0, r, 0),     // base rim      (z = 0, ρ = r)
                new Vector(h, 0, 0),     // apex          (z = h, ρ = 0)
                new Vector(local.Z, radial, 0));

            double z = nearest.X;
            double rho = nearest.Y;
            double dirX = radial > 0 ? local.X / radial : 1.0;
            double dirY = radial > 0 ? local.Y / radial : 0.0;
            return this.pose.Apply(new Vector(dirX * rho, dirY * rho, z));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the cone with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/>, <paramref name="far"/>); false when the line misses.
        /// </summary>
        /// <remarks>
        /// Maths, in local space: the curved side satisfies <c>x² + y² = k²·(h − z)²</c> with <c>k = r/h</c>
        /// — a quadratic in <c>t</c> whose roots are kept only where <c>z</c> lies between base and apex —
        /// while the base cap is the <c>z = 0</c> plane kept within the radius. Nearest and farthest
        /// survivors are the entry and exit.
        /// </remarks>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector lo = this.pose.ApplyInverse(line.Point);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(line.Direction);
            List<double> hits = this.SurfaceParameters(lo, ld);
            if (hits.Count == 0)
            {
                near = far = line.Point;
                return false;
            }

            double tMin = double.PositiveInfinity, tMax = double.NegativeInfinity;
            foreach (double t in hits)
            {
                if (t < tMin) tMin = t;
                if (t > tMax) tMax = t;
            }

            near = this.pose.Apply(lo + (tMin * ld));
            far = this.pose.Apply(lo + (tMax * ld));
            return true;
        }

        /// <summary>Intersect the cone with a <see cref="Ray"/> (nearest hit at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(ray.Origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(ray.Direction);
            List<double> hits = this.SurfaceParameters(lo, ld);

            double best = double.PositiveInfinity;
            foreach (double t in hits)
            {
                if (t >= 0 && t < best)
                {
                    best = t;
                }
            }

            if (double.IsPositiveInfinity(best))
            {
                point = ray.Origin;
                return false;
            }

            point = this.pose.Apply(lo + (best * ld));
            return true;
        }

        private List<double> SurfaceParameters(Vector lo, Vector ld)
        {
            var hits = new List<double>(3);
            const double eps = 1e-9;
            double h = this.shape.Height;
            double r = this.shape.BaseRadius;
            if (h == 0)
            {
                return hits;
            }

            double k = r / h;
            double w0 = h - lo.Z; // (h − z) at t = 0

            // Curved side: x² + y² = k²·(h − z)².
            double a = (ld.X * ld.X) + (ld.Y * ld.Y) - (k * k * ld.Z * ld.Z);
            double b = (2.0 * ((lo.X * ld.X) + (lo.Y * ld.Y))) + (2.0 * k * k * w0 * ld.Z);
            double c = (lo.X * lo.X) + (lo.Y * lo.Y) - (k * k * w0 * w0);
            foreach (double t in PolynomialRoots.SolveQuadratic(a, b, c))
            {
                double z = lo.Z + (t * ld.Z);
                if (z >= -eps && z <= h + eps)
                {
                    hits.Add(t);
                }
            }

            // Base cap: z = 0, kept within the radius.
            if (Math.Abs(ld.Z) > eps)
            {
                double t = -lo.Z / ld.Z;
                double x = lo.X + (t * ld.X);
                double y = lo.Y + (t * ld.Y);
                if ((x * x) + (y * y) <= (r * r) + eps)
                {
                    hits.Add(t);
                }
            }

            return hits;
        }

        #endregion

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedCone Translate(Vector offset)
        {
            return new PlacedCone(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the base radius and height scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedCone Scale(double factor)
        {
            return new PlacedCone(new Cone(this.shape.BaseRadius * factor, this.shape.Height * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedCone Transform(Pose transform)
        {
            return new PlacedCone(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedCone c1, PlacedCone c2)
        {
            return c1.Shape == c2.Shape && c1.Pose == c2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedCone c1, PlacedCone c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedCone c && this.Equals(c);
        }

        /// <summary>Equality with another placed cone (conceptual shape and pose).</summary>
        public bool Equals(PlacedCone other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed cone within an absolute tolerance.</summary>
        public bool Equals(PlacedCone other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Cone shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(r, h) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(r, h) @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>The point on triangle (a, b, c) closest to <paramref name="p"/> (Ericson's Voronoi-region method).</summary>
        private static Vector ClosestPointOnTriangle(Vector a, Vector b, Vector c, Vector p)
        {
            Vector ab = b - a;
            Vector ac = c - a;
            Vector ap = p - a;

            double d1 = ab.DotProduct(ap);
            double d2 = ac.DotProduct(ap);
            if (d1 <= 0 && d2 <= 0) return a;

            Vector bp = p - b;
            double d3 = ab.DotProduct(bp);
            double d4 = ac.DotProduct(bp);
            if (d3 >= 0 && d4 <= d3) return b;

            double vc = (d1 * d4) - (d3 * d2);
            if (vc <= 0 && d1 >= 0 && d3 <= 0)
            {
                return a + ((d1 / (d1 - d3)) * ab);
            }

            Vector cp = p - c;
            double d5 = ab.DotProduct(cp);
            double d6 = ac.DotProduct(cp);
            if (d6 >= 0 && d5 <= d6) return c;

            double vb = (d5 * d2) - (d1 * d6);
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                return a + ((d2 / (d2 - d6)) * ac);
            }

            double va = (d3 * d6) - (d5 * d4);
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                return b + (((d4 - d3) / ((d4 - d3) + (d5 - d6))) * (c - b));
            }

            double denom = 1.0 / (va + vb + vc);
            return a + (ab * (vb * denom)) + (ac * (vc * denom));
        }

        #endregion
    }
}
