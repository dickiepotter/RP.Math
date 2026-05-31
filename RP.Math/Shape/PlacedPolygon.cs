namespace RP.Math
{
    using System;
    using System.Linq;
    using System.Text;

    using Math = System.Math;

    /// <summary>
    /// A filled, planar polygon in 3D space, defined by an ordered list of coplanar corner points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Like <see cref="PlacedTriangle"/>'s corners, a polygon is <i>vertex-defined</i>: its position and
    /// orientation are implied entirely by where its corners sit, so it has no separate conceptual
    /// (unpositioned) form — it is a positioned shape in its own right. The corners are assumed coplanar
    /// and given in order around the boundary (clockwise or anticlockwise); the winding fixes the
    /// direction of the <see cref="Normal"/>.
    /// </para>
    /// <para>
    /// Area, normal and centroid are computed robustly for any simple polygon (convex or not) by summing
    /// edge cross-products. Containment and closest-point treat it as the filled region. At least three
    /// corners are required.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public class PlacedPolygon : IEquatable<PlacedPolygon>
    {
        #region Fields

        private readonly Vector[] vertices;

        #endregion

        #region Constructors

        /// <summary>Construct a polygon from its ordered, coplanar corner points (at least three).</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="vertices"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if fewer than three corners are supplied.</exception>
        public PlacedPolygon(params Vector[] vertices)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (vertices.Length < 3) throw new ArgumentException(TOO_FEW, nameof(vertices));
            this.vertices = (Vector[])vertices.Clone();
        }

        #endregion

        #region Accessors

        /// <summary>The number of corners.</summary>
        public int Count { get { return this.vertices.Length; } }

        /// <summary>The corner at <paramref name="index"/>.</summary>
        public Vector this[int index] { get { return this.vertices[index]; } }

        /// <summary>A copy of the corner points, in order.</summary>
        public Vector[] Vertices { get { return (Vector[])this.vertices.Clone(); } }

        /// <summary>
        /// The vector area of the polygon: <c>½·Σ(Vᵢ × Vᵢ₊₁)</c>. Its magnitude is the area and its
        /// direction is the (un-normalised) face normal — the basis for the other planar quantities.
        /// </summary>
        private Vector AreaVector
        {
            get
            {
                Vector sum = new Vector(0, 0, 0);
                for (int i = 0; i < this.vertices.Length; i++)
                {
                    Vector cur = this.vertices[i];
                    Vector next = this.vertices[(i + 1) % this.vertices.Length];
                    sum += cur.CrossProduct(next);
                }

                return sum * 0.5;
            }
        }

        /// <summary>The unit face normal, following the corner winding (right-hand rule).</summary>
        public Vector Normal { get { return this.AreaVector.NormalizeOrDefault(); } }

        /// <summary>The enclosed area.</summary>
        public double Area { get { return this.AreaVector.Magnitude; } }

        /// <summary>The perimeter: the summed lengths of all edges.</summary>
        public double Perimeter
        {
            get
            {
                double total = 0;
                for (int i = 0; i < this.vertices.Length; i++)
                {
                    total += this.vertices[i].Distance(this.vertices[(i + 1) % this.vertices.Length]);
                }

                return total;
            }
        }

        /// <summary>
        /// The area-weighted centroid (the polygon's balance point), found by triangulating as a fan from
        /// the first corner and averaging the triangle centroids weighted by signed area. Falls back to the
        /// plain corner average for a zero-area polygon.
        /// </summary>
        public Vector Centroid
        {
            get
            {
                Vector normal = this.Normal;
                Vector v0 = this.vertices[0];
                double areaSum = 0;
                Vector weighted = new Vector(0, 0, 0);
                for (int i = 1; i < this.vertices.Length - 1; i++)
                {
                    Vector e1 = this.vertices[i] - v0;
                    Vector e2 = this.vertices[i + 1] - v0;
                    double signedArea = 0.5 * e1.CrossProduct(e2).DotProduct(normal);
                    Vector triCentroid = (v0 + this.vertices[i] + this.vertices[i + 1]) / 3.0;
                    areaSum += signedArea;
                    weighted += triCentroid * signedArea;
                }

                if (Math.Abs(areaSum) < double.Epsilon)
                {
                    return this.VertexAverage();
                }

                return weighted / areaSum;
            }
        }

        /// <summary>The supporting plane the polygon lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.vertices[0], this.Normal); } }

        /// <summary>The edges as world-space <see cref="LineSegment"/>s, in order.</summary>
        public LineSegment[] Edges()
        {
            var edges = new LineSegment[this.vertices.Length];
            for (int i = 0; i < this.vertices.Length; i++)
            {
                edges[i] = new LineSegment(this.vertices[i], this.vertices[(i + 1) % this.vertices.Length]);
            }

            return edges;
        }

        #endregion

        #region Containment and queries

        /// <summary>Whether the polygon is convex within <paramref name="tolerance"/> (all turns the same way).</summary>
        public bool IsConvex(double tolerance)
        {
            Vector normal = this.Normal;
            bool? positive = null;
            int n = this.vertices.Length;
            for (int i = 0; i < n; i++)
            {
                Vector a = this.vertices[i];
                Vector b = this.vertices[(i + 1) % n];
                Vector c = this.vertices[(i + 2) % n];
                double turn = (b - a).CrossProduct(c - b).DotProduct(normal);
                if (turn > tolerance)
                {
                    if (positive == false) return false;
                    positive = true;
                }
                else if (turn < -tolerance)
                {
                    if (positive == true) return false;
                    positive = false;
                }
            }

            return true;
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled polygon (on its plane and inside its edges).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled polygon within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector normal = this.Normal;
            if (Math.Abs((point - this.vertices[0]).DotProduct(normal)) > tolerance)
            {
                return false;
            }

            return this.InPlaneContains(point);
        }

        /// <summary>The point on the filled polygon closest to <paramref name="point"/> (its projection if inside, else the nearest edge point).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector normal = this.Normal;
            Vector projected = point - ((point - this.vertices[0]).DotProduct(normal) * normal);
            if (this.InPlaneContains(projected))
            {
                return projected;
            }

            Vector best = this.vertices[0];
            double bestDist = double.PositiveInfinity;
            foreach (LineSegment edge in this.Edges())
            {
                Vector candidate = edge.ClosestPointTo(point);
                double d = candidate.DistanceSquared(point);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }

            return best;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>Intersect the filled polygon with an infinite <see cref="Line"/>.</summary>
        public bool TryIntersect(Line line, out Vector point)
        {
            if (!this.Plane.TryIntersect(line, out point))
            {
                return false;
            }

            return this.InPlaneContains(point);
        }

        /// <summary>Intersect the filled polygon with a <see cref="Ray"/> (hit must be at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            if (!this.Plane.TryIntersect(ray, out point))
            {
                return false;
            }

            return this.InPlaneContains(point);
        }

        #endregion

        #region Transformation (returns a new polygon)

        /// <summary>A copy of the polygon translated by <paramref name="offset"/>.</summary>
        public PlacedPolygon Translate(Vector offset)
        {
            return new PlacedPolygon(this.vertices.Select(v => v + offset).ToArray());
        }

        /// <summary>A copy of the polygon with every corner placed into world space by <paramref name="pose"/>.</summary>
        public PlacedPolygon Transform(Pose pose)
        {
            return new PlacedPolygon(this.vertices.Select(pose.Apply).ToArray());
        }

        #endregion

        #region Equality and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedPolygon p && this.Equals(p);
        }

        /// <summary>Equality with another polygon (same corners, in the same order).</summary>
        public bool Equals(PlacedPolygon? other)
        {
            if (other is null || other.Count != this.Count)
            {
                return false;
            }

            for (int i = 0; i < this.vertices.Length; i++)
            {
                if (this.vertices[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>A hash code derived from the corners.</summary>
        public override int GetHashCode()
        {
            int hash = this.vertices.Length;
            foreach (Vector v in this.vertices)
            {
                hash ^= v.GetHashCode();
            }

            return hash;
        }

        /// <summary>A string listing the corners.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < this.vertices.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this.vertices[i].ToString());
            }

            return sb.Append(']').ToString();
        }

        #endregion

        #region Helpers

        private Vector VertexAverage()
        {
            Vector sum = new Vector(0, 0, 0);
            foreach (Vector v in this.vertices)
            {
                sum += v;
            }

            return sum / this.vertices.Length;
        }

        /// <summary>
        /// The crossing-number point-in-polygon test, done in the polygon's own plane. The point is assumed
        /// already on (or projected onto) the plane; it is expressed in a 2D in-plane basis and a ray is
        /// cast to count boundary crossings (odd = inside). Works for convex and non-convex simple polygons.
        /// </summary>
        private bool InPlaneContains(Vector point)
        {
            Vector normal = this.Normal;
            Vector u = PerpendicularTo(normal);
            Vector v = normal.CrossProduct(u);
            Vector origin = this.vertices[0];

            double px = (point - origin).DotProduct(u);
            double py = (point - origin).DotProduct(v);

            bool inside = false;
            int n = this.vertices.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = (this.vertices[i] - origin).DotProduct(u);
                double yi = (this.vertices[i] - origin).DotProduct(v);
                double xj = (this.vertices[j] - origin).DotProduct(u);
                double yj = (this.vertices[j] - origin).DotProduct(v);

                bool straddles = (yi > py) != (yj > py);
                if (straddles && px < ((xj - xi) * (py - yi) / (yj - yi)) + xi)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>Any unit vector perpendicular to <paramref name="n"/>.</summary>
        private static Vector PerpendicularTo(Vector n)
        {
            Vector candidate = n.CrossProduct(new Vector(1, 0, 0));
            if (candidate.IsZero())
            {
                candidate = n.CrossProduct(new Vector(0, 1, 0));
            }

            return candidate.NormalizeOrDefault();
        }

        #endregion

        #region Messages

        private const string TOO_FEW = "A polygon needs at least three corners.";

        #endregion
    }
}
