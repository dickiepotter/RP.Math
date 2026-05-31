namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A filled tetrahedron in 3D space, defined by its four corner points <see cref="A"/>,
    /// <see cref="B"/>, <see cref="C"/> and <see cref="D"/>.
    /// </summary>
    /// <remarks>
    /// Like <see cref="PlacedPolygon"/>, a tetrahedron is <i>vertex-defined</i>: its placement is implied by its
    /// corners, so it has no separate conceptual (unpositioned) form. It is the simplest solid — four
    /// triangular faces — and is treated as the filled region they bound. A tetrahedron whose four corners
    /// are coplanar has zero <see cref="Volume"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedTetrahedron : IEquatable<PlacedTetrahedron>, IFormattable
    {
        #region Fields

        private readonly Vector a;
        private readonly Vector b;
        private readonly Vector c;
        private readonly Vector d;

        #endregion

        #region Constructors

        /// <summary>Construct a tetrahedron from its four corner points.</summary>
        public PlacedTetrahedron(Vector a, Vector b, Vector c, Vector d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        #endregion

        #region Accessors

        /// <summary>The first corner.</summary>
        public Vector A { get { return this.a; } }

        /// <summary>The second corner.</summary>
        public Vector B { get { return this.b; } }

        /// <summary>The third corner.</summary>
        public Vector C { get { return this.c; } }

        /// <summary>The fourth corner.</summary>
        public Vector D { get { return this.d; } }

        /// <summary>The centroid: the average of the four corners.</summary>
        public Vector Centroid { get { return (this.a + this.b + this.c + this.d) / 4.0; } }

        /// <summary>
        /// The enclosed volume.
        /// </summary>
        /// <remarks>Maths: <c>|(B − A) · ((C − A) × (D − A))| / 6</c> — one sixth of the absolute scalar
        /// triple product of the three edges leaving <c>A</c> (the signed volume of the parallelepiped they
        /// span, divided by six).</remarks>
        public double Volume
        {
            get { return Math.Abs((this.b - this.a).DotProduct((this.c - this.a).CrossProduct(this.d - this.a))) / 6.0; }
        }

        /// <summary>The total surface area: the summed areas of the four triangular faces.</summary>
        public double SurfaceArea
        {
            get
            {
                return TriangleArea(this.a, this.b, this.c)
                     + TriangleArea(this.a, this.b, this.d)
                     + TriangleArea(this.a, this.c, this.d)
                     + TriangleArea(this.b, this.c, this.d);
            }
        }

        /// <summary>Whether the four corners are (within <paramref name="tolerance"/>) coplanar, giving zero volume.</summary>
        public bool IsDegenerate(double tolerance)
        {
            return this.Volume <= tolerance;
        }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the tetrahedron.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>
        /// Whether <paramref name="point"/> lies on or within the tetrahedron, within
        /// <paramref name="tolerance"/> (a small slack on the barycentric coordinates).
        /// </summary>
        /// <remarks>Maths: the point's four barycentric coordinates — the signed volumes of the four
        /// sub-tetrahedra formed with each face, divided by the whole volume — are all non-negative exactly
        /// when the point is inside.</remarks>
        public bool Contains(Vector point, double tolerance)
        {
            double whole = SignedVolume(this.a, this.b, this.c, this.d);
            if (whole == 0)
            {
                return false; // degenerate (flat) tetrahedron
            }

            double u = SignedVolume(point, this.b, this.c, this.d) / whole;
            double v = SignedVolume(this.a, point, this.c, this.d) / whole;
            double w = SignedVolume(this.a, this.b, point, this.d) / whole;
            double x = SignedVolume(this.a, this.b, this.c, point) / whole;

            return u >= -tolerance && v >= -tolerance && w >= -tolerance && x >= -tolerance;
        }

        /// <summary>The point on or within the tetrahedron closest to <paramref name="point"/>.</summary>
        /// <remarks>An interior point returns itself; otherwise the nearest point over the four triangular
        /// faces is returned.</remarks>
        public Vector ClosestPoint(Vector point)
        {
            if (this.Contains(point, 0))
            {
                return point;
            }

            Vector best = ClosestPointOnTriangle(this.a, this.b, this.c, point);
            double bestDist = best.DistanceSquared(point);
            Consider(ClosestPointOnTriangle(this.a, this.b, this.d, point), point, ref best, ref bestDist);
            Consider(ClosestPointOnTriangle(this.a, this.c, this.d, point), point, ref best, ref bestDist);
            Consider(ClosestPointOnTriangle(this.b, this.c, this.d, point), point, ref best, ref bestDist);
            return best;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the tetrahedron with an infinite <see cref="Line"/>. Returns true and the entry and
        /// exit points (<paramref name="near"/>, <paramref name="far"/>); false when the line misses.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            List<double> hits = this.FaceParameters(line.Point, line.Direction);
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

            near = line.Point + (tMin * line.Direction);
            far = line.Point + (tMax * line.Direction);
            return true;
        }

        /// <summary>Intersect the tetrahedron with a <see cref="Ray"/> (nearest hit at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            List<double> hits = this.FaceParameters(ray.Origin, ray.Direction);
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

            point = ray.Origin + (best * ray.Direction);
            return true;
        }

        /// <summary>The line parameters at which <c>origin + t·direction</c> crosses the four triangular faces.</summary>
        private List<double> FaceParameters(Vector origin, Vector direction)
        {
            var hits = new List<double>(4);
            AddFaceHit(hits, this.a, this.b, this.c, origin, direction);
            AddFaceHit(hits, this.a, this.b, this.d, origin, direction);
            AddFaceHit(hits, this.a, this.c, this.d, origin, direction);
            AddFaceHit(hits, this.b, this.c, this.d, origin, direction);
            return hits;
        }

        private static void AddFaceHit(List<double> hits, Vector f0, Vector f1, Vector f2, Vector origin, Vector direction)
        {
            Vector normal = (f1 - f0).CrossProduct(f2 - f0);
            double denom = normal.DotProduct(direction);
            if (denom.AlmostEqualsWithAbsTolerance(0, double.Epsilon))
            {
                return; // parallel to the face
            }

            double t = (f0 - origin).DotProduct(normal) / denom;
            Vector hit = origin + (t * direction);
            if (PointInTriangle(f0, f1, f2, hit))
            {
                hits.Add(t);
            }
        }

        #endregion

        #region Transformation (returns a new tetrahedron)

        /// <summary>A copy translated by <paramref name="offset"/>.</summary>
        public PlacedTetrahedron Translate(Vector offset)
        {
            return new PlacedTetrahedron(this.a + offset, this.b + offset, this.c + offset, this.d + offset);
        }

        /// <summary>A copy with every corner placed into world space by <paramref name="pose"/>.</summary>
        public PlacedTetrahedron Transform(Pose pose)
        {
            return new PlacedTetrahedron(pose.Apply(this.a), pose.Apply(this.b), pose.Apply(this.c), pose.Apply(this.d));
        }

        #endregion

        #region Operators

        /// <summary>Equality of all four corners (in order).</summary>
        public static bool operator ==(PlacedTetrahedron t1, PlacedTetrahedron t2)
        {
            return t1.A == t2.A && t1.B == t2.B && t1.C == t2.C && t1.D == t2.D;
        }

        /// <summary>Inequality of corners.</summary>
        public static bool operator !=(PlacedTetrahedron t1, PlacedTetrahedron t2)
        {
            return !(t1 == t2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedTetrahedron t && this.Equals(t);
        }

        /// <summary>Equality with another tetrahedron (all four corners, in order).</summary>
        public bool Equals(PlacedTetrahedron other)
        {
            return this == other;
        }

        /// <summary>Equality with another tetrahedron within an absolute tolerance (corner by corner).</summary>
        public bool Equals(PlacedTetrahedron other, double tolerance)
        {
            return this.a.Equals(other.A, tolerance)
                && this.b.Equals(other.B, tolerance)
                && this.c.Equals(other.C, tolerance)
                && this.d.Equals(other.D, tolerance);
        }

        /// <summary>A hash code derived from the four corners.</summary>
        public override int GetHashCode()
        {
            return this.a.GetHashCode() ^ this.b.GetHashCode() ^ this.c.GetHashCode() ^ this.d.GetHashCode();
        }

        /// <summary>Deconstruct into the four corners.</summary>
        public void Deconstruct(out Vector a, out Vector b, out Vector c, out Vector d)
        {
            a = this.a;
            b = this.b;
            c = this.c;
            d = this.d;
        }

        /// <summary>A string of the form <c>(A, B, C, D)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(A, B, C, D)</c> where corners use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2}, {3})",
                this.a.ToString(format, formatProvider),
                this.b.ToString(format, formatProvider),
                this.c.ToString(format, formatProvider),
                this.d.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>Six times the signed volume of the tetrahedron (p, q, r, s).</summary>
        private static double SignedVolume(Vector p, Vector q, Vector r, Vector s)
        {
            return (q - p).DotProduct((r - p).CrossProduct(s - p));
        }

        /// <summary>The area of triangle (p, q, r).</summary>
        private static double TriangleArea(Vector p, Vector q, Vector r)
        {
            return 0.5 * (q - p).CrossProduct(r - p).Magnitude;
        }

        /// <summary>Whether <paramref name="p"/> (assumed on the triangle's plane) lies inside triangle (a, b, c).</summary>
        private static bool PointInTriangle(Vector a, Vector b, Vector c, Vector p)
        {
            Vector e1 = b - a;
            Vector e2 = c - a;
            Vector pa = p - a;
            double d11 = e1.DotProduct(e1);
            double d12 = e1.DotProduct(e2);
            double d22 = e2.DotProduct(e2);
            double dp1 = pa.DotProduct(e1);
            double dp2 = pa.DotProduct(e2);
            double denom = (d11 * d22) - (d12 * d12);
            if (denom == 0)
            {
                return false;
            }

            double v = ((d22 * dp1) - (d12 * dp2)) / denom;
            double w = ((d11 * dp2) - (d12 * dp1)) / denom;
            return v >= 0 && w >= 0 && v + w <= 1;
        }

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

        private static void Consider(Vector candidate, Vector target, ref Vector best, ref double bestDist)
        {
            double d = candidate.DistanceSquared(target);
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }

        #endregion
    }
}
