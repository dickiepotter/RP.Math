namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Triangle"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Triangle"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conceptual <see cref="Shape"/> supplies the three side lengths and all size-only maths (area,
    /// angles, classification); the <see cref="Pose"/> supplies the placement. In the triangle's own local
    /// frame it lies in the XY plane with its <b>centroid at the origin</b>, the A→B edge running along
    /// local +X and corner C on the local +Y side; the pose maps that frame into the world. Because the
    /// canonical centroid is the origin, the world <see cref="Centroid"/> is simply the pose's position.
    /// </para>
    /// <para>
    /// Construct one directly from a <see cref="Triangle"/> and a <see cref="Pose"/>, or — the common case
    /// — from three world corners with <see cref="FromVertices"/>, which recovers both the side lengths and
    /// the pose that places them.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedTriangle : IEquatable<PlacedTriangle>, IFormattable
    {
        #region Fields

        private readonly Triangle shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed triangle from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedTriangle(Triangle shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>
        /// A placed triangle from its three world corners. The side lengths become the conceptual
        /// <see cref="Triangle"/> and the <see cref="Pose"/> is the rigid placement (centroid position and
        /// orientation) such that <see cref="A"/>, <see cref="B"/> and <see cref="C"/> reproduce the inputs.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the three corners are collinear (no triangle exists).</exception>
        public static PlacedTriangle FromVertices(Vector a, Vector b, Vector c)
        {
            // Conceptual side lengths (each opposite its corner). Constructing the Triangle validates them.
            double sideA = (b - c).Magnitude;
            double sideB = (c - a).Magnitude;
            double sideC = (a - b).Magnitude;
            var shape = new Triangle(sideA, sideB, sideC);

            // The world frame: A→B is local +X, the face normal is local +Z.
            Vector e1 = (b - a).NormalizeOrDefault();
            Vector normal = (b - a).CrossProduct(c - a).NormalizeOrDefault();

            // Build the rotation local→world by composing two turns: first send local +Z onto the normal,
            // then spin about the normal to send the (rotated) local +X onto the A→B edge direction.
            Quaternion toNormal = RotationBetween(new Vector(0, 0, 1), normal);
            Vector rotatedX = toNormal.Rotate(new Vector(1, 0, 0));
            double spin = Math.Atan2(rotatedX.CrossProduct(e1).DotProduct(normal), rotatedX.DotProduct(e1));
            Quaternion aboutNormal = Quaternion.FromAxisAngle(normal, new Angle(spin));
            Quaternion rotation = aboutNormal * toNormal;

            return new PlacedTriangle(shape, new Pose((a + b + c) / 3.0, rotation));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) triangle.</summary>
        public Triangle Shape { get { return this.shape; } }

        /// <summary>The placement (position and orientation) of the triangle.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The world position of corner A.</summary>
        public Vector A { get { return this.pose.Apply(this.LocalA()); } }

        /// <summary>The world position of corner B.</summary>
        public Vector B { get { return this.pose.Apply(this.LocalB()); } }

        /// <summary>The world position of corner C.</summary>
        public Vector C { get { return this.pose.Apply(this.LocalC()); } }

        /// <summary>The centroid in world space (the pose's position, since the canonical centroid is the origin).</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit face normal in world space (the pose's local +Z), following the A→B→C winding.</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The supporting plane the triangle lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Centroid, this.Normal); } }

        /// <summary>The length of side A (from the conceptual shape).</summary>
        public double SideA { get { return this.shape.SideA; } }

        /// <summary>The length of side B (from the conceptual shape).</summary>
        public double SideB { get { return this.shape.SideB; } }

        /// <summary>The length of side C (from the conceptual shape).</summary>
        public double SideC { get { return this.shape.SideC; } }

        /// <summary>The interior angle at corner A (from the conceptual shape).</summary>
        public Angle AngleA { get { return this.shape.AngleA; } }

        /// <summary>The interior angle at corner B (from the conceptual shape).</summary>
        public Angle AngleB { get { return this.shape.AngleB; } }

        /// <summary>The interior angle at corner C (from the conceptual shape).</summary>
        public Angle AngleC { get { return this.shape.AngleC; } }

        /// <summary>The area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        /// <summary>The perimeter (from the conceptual shape).</summary>
        public double Perimeter { get { return this.shape.Perimeter; } }

        /// <summary>The edge from corner A to corner B as a world-space <see cref="LineSegment"/>.</summary>
        public LineSegment EdgeAB { get { return new LineSegment(this.A, this.B); } }

        /// <summary>The edge from corner B to corner C as a world-space <see cref="LineSegment"/>.</summary>
        public LineSegment EdgeBC { get { return new LineSegment(this.B, this.C); } }

        /// <summary>The edge from corner C to corner A as a world-space <see cref="LineSegment"/>.</summary>
        public LineSegment EdgeCA { get { return new LineSegment(this.C, this.A); } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the filled triangle (on its plane and inside its edges).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled triangle within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > tolerance)
            {
                return false;
            }

            Vector bary = Barycentric(this.LocalA(), this.LocalB(), this.LocalC(), local);
            return !bary.IsNaN() && bary.X >= -tolerance && bary.Y >= -tolerance && bary.Z >= -tolerance;
        }

        /// <summary>
        /// The barycentric coordinates <c>(u, v, w)</c> of <paramref name="point"/> with respect to the
        /// corners <c>(A, B, C)</c>, packed into a <see cref="Vector"/>. They sum to 1, and the point is
        /// inside the triangle exactly when all three are non-negative. Computed in the triangle's local
        /// frame (so the point is implicitly projected onto the triangle's plane).
        /// </summary>
        public Vector Barycentric(Vector point)
        {
            return Barycentric(this.LocalA(), this.LocalB(), this.LocalC(), this.pose.ApplyInverse(point));
        }

        /// <summary>
        /// The point on the filled triangle closest to <paramref name="point"/> (the nearest of its
        /// interior, edges and corners), found in the triangle's local frame.
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = ClosestPointOnTriangle(this.LocalA(), this.LocalB(), this.LocalC(), this.pose.ApplyInverse(point));
            return this.pose.Apply(local);
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the filled triangle with an infinite <see cref="Line"/>. A line meets the flat triangle
        /// at most once, so on a hit <paramref name="near"/> and <paramref name="far"/> are the same crossing
        /// point; false when it crosses the triangle's plane outside the edges or runs parallel to it.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            bool hit = this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out Vector point);
            near = far = point;
            return hit;
        }

        /// <summary>
        /// Intersect the filled triangle with a <see cref="Ray"/>. As
        /// <see cref="TryIntersect(Line, out Vector, out Vector)"/>, but the hit must lie at or ahead of the ray's origin.
        /// </summary>
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
                point = origin; // parallel to the plane
                return false;
            }

            double t = -lo.Z / ld.Z;
            if (requireForward && t < 0)
            {
                point = origin;
                return false;
            }

            Vector hit = lo + (t * ld);
            Vector bary = Barycentric(this.LocalA(), this.LocalB(), this.LocalC(), hit);
            if (!bary.IsNaN() && bary.X >= 0 && bary.Y >= 0 && bary.Z >= 0)
            {
                point = this.pose.Apply(hit);
                return true;
            }

            point = origin;
            return false;
        }

        #endregion

        #region Modification (returns a new placed triangle)

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedTriangle Translate(Vector offset)
        {
            return new PlacedTriangle(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the conceptual shape's sides scaled by <paramref name="factor"/> about the centroid (placement unchanged).</summary>
        public PlacedTriangle Scale(double factor)
        {
            return new PlacedTriangle(
                new Triangle(this.shape.SideA * factor, this.shape.SideB * factor, this.shape.SideC * factor),
                this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedTriangle Transform(Pose transform)
        {
            return new PlacedTriangle(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedTriangle t1, PlacedTriangle t2)
        {
            return t1.Shape == t2.Shape && t1.Pose == t2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedTriangle t1, PlacedTriangle t2)
        {
            return !(t1 == t2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedTriangle t && this.Equals(t);
        }

        /// <summary>Equality with another placed triangle (conceptual shape and pose).</summary>
        public bool Equals(PlacedTriangle other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed triangle within an absolute tolerance.</summary>
        public bool Equals(PlacedTriangle other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Triangle shape, out Pose pose)
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

        #region Local layout

        /// <summary>
        /// The triangle's three corners in its local frame: centroid at the origin, the A→B edge along
        /// local +X and corner C on the local +Y side, all in the z = 0 plane. Derived from the side
        /// lengths via the law of cosines.
        /// </summary>
        private void LocalVertices(out Vector a, out Vector b, out Vector c)
        {
            double sideA = this.shape.SideA; // |B−C|
            double sideB = this.shape.SideB; // |C−A|
            double sideC = this.shape.SideC; // |A−B|

            // Lay A at the origin and B at (sideC, 0); solve C from the two distances it must satisfy.
            double cx = ((sideC * sideC) + (sideB * sideB) - (sideA * sideA)) / (2.0 * sideC);
            double cyInside = (sideB * sideB) - (cx * cx);
            double cy = cyInside <= 0 ? 0 : Math.Sqrt(cyInside);

            // Shift everything so the centroid sits at the origin.
            double gx = (0 + sideC + cx) / 3.0;
            double gy = (0 + 0 + cy) / 3.0;

            a = new Vector(0 - gx, 0 - gy, 0);
            b = new Vector(sideC - gx, 0 - gy, 0);
            c = new Vector(cx - gx, cy - gy, 0);
        }

        private Vector LocalA()
        {
            this.LocalVertices(out Vector a, out _, out _);
            return a;
        }

        private Vector LocalB()
        {
            this.LocalVertices(out _, out Vector b, out _);
            return b;
        }

        private Vector LocalC()
        {
            this.LocalVertices(out _, out _, out Vector c);
            return c;
        }

        #endregion

        #region Helpers

        /// <summary>The barycentric coordinates of <paramref name="p"/> with respect to triangle (a, b, c).</summary>
        private static Vector Barycentric(Vector a, Vector b, Vector c, Vector p)
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
                return Vector.NaN;
            }

            double v = ((d22 * dp1) - (d12 * dp2)) / denom;
            double w = ((d11 * dp2) - (d12 * dp1)) / denom;
            return new Vector(1.0 - v - w, v, w);
        }

        /// <summary>The point on the filled triangle (a, b, c) closest to <paramref name="p"/> (Ericson's Voronoi-region method).</summary>
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
                double v = d1 / (d1 - d3);
                return a + (v * ab);
            }

            Vector cp = p - c;
            double d5 = ab.DotProduct(cp);
            double d6 = ac.DotProduct(cp);
            if (d6 >= 0 && d5 <= d6) return c;

            double vb = (d5 * d2) - (d1 * d6);
            if (vb <= 0 && d2 >= 0 && d6 <= 0)
            {
                double w = d2 / (d2 - d6);
                return a + (w * ac);
            }

            double va = (d3 * d6) - (d5 * d4);
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
            {
                double w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + (w * (c - b));
            }

            double denom = 1.0 / (va + vb + vc);
            double vv = vb * denom;
            double ww = vc * denom;
            return a + (ab * vv) + (ac * ww);
        }

        /// <summary>
        /// A quaternion rotating the unit vector <paramref name="from"/> onto the unit vector
        /// <paramref name="to"/> — the shortest-arc turn. Delegates to <see cref="Quaternion.FromToRotation"/>.
        /// </summary>
        private static Quaternion RotationBetween(Vector from, Vector to)
        {
            return Quaternion.FromToRotation(from, to);
        }

        #endregion
    }
}
