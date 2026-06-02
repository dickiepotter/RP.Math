namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An <see cref="Ellipse"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Ellipse"/>.
    /// </summary>
    /// <remarks>
    /// In the ellipse's own local frame it lies in the XY plane centred at the origin, with
    /// <see cref="Ellipse.SemiAxisX"/> along local +X and <see cref="Ellipse.SemiAxisY"/> along local +Y;
    /// the pose maps that frame into the world. World queries map the point back into the local frame,
    /// where the ellipse is axis-aligned, and map any returned point forward again.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedEllipse : IEquatable<PlacedEllipse>, IFormattable
    {
        #region Fields

        private readonly Ellipse shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed ellipse from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedEllipse(Ellipse shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>An ellipse lying flat in the world XY plane, centred at <paramref name="center"/>.</summary>
        public static PlacedEllipse InXYPlane(Ellipse shape, Vector center)
        {
            return new PlacedEllipse(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) ellipse.</summary>
        public Ellipse Shape { get { return this.shape; } }

        /// <summary>The placement of the ellipse.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit axis carrying <see cref="Ellipse.SemiAxisX"/> (the pose's local +X).</summary>
        public Vector AxisU { get { return this.pose.ApplyDirection(new Vector(1, 0, 0)); } }

        /// <summary>The unit axis carrying <see cref="Ellipse.SemiAxisY"/> (the pose's local +Y).</summary>
        public Vector AxisV { get { return this.pose.ApplyDirection(new Vector(0, 1, 0)); } }

        /// <summary>The unit normal of the ellipse's plane (the pose's local +Z).</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The supporting plane the ellipse lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Center, this.Normal); } }

        /// <summary>The enclosed area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        /// <summary>The perimeter (from the conceptual shape).</summary>
        public double Perimeter { get { return this.shape.Perimeter; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the filled ellipse (on its plane and within its boundary).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled ellipse within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > tolerance)
            {
                return false;
            }

            double a = this.shape.SemiAxisX + tolerance;
            double b = this.shape.SemiAxisY + tolerance;
            if (a <= 0 || b <= 0)
            {
                return false;
            }

            return ((local.X * local.X) / (a * a)) + ((local.Y * local.Y) / (b * b)) <= 1.0;
        }

        /// <summary>
        /// The point on the filled ellipse closest to <paramref name="point"/>. If the projection onto the
        /// plane already lies inside the boundary that projection is returned; otherwise the nearest point
        /// on the boundary curve is found by Eberly's robust closest-point-on-ellipse method.
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double a = this.shape.SemiAxisX;
            double b = this.shape.SemiAxisY;

            bool insideBoundary = a > 0 && b > 0
                && ((local.X * local.X) / (a * a)) + ((local.Y * local.Y) / (b * b)) <= 1.0;
            if (insideBoundary)
            {
                return this.pose.Apply(new Vector(local.X, local.Y, 0)); // projection onto the plane
            }

            (double ex, double ey) = ClosestPointOnEllipse(a, b, local.X, local.Y);
            return this.pose.Apply(new Vector(ex, ey, 0));
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the filled ellipse with an infinite <see cref="Line"/>. A line meets the flat ellipse at
        /// most once, so on a hit <paramref name="near"/> and <paramref name="far"/> are the same crossing
        /// point; false otherwise.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            bool hit = this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out Vector point);
            near = far = point;
            return hit;
        }

        /// <summary>Intersect the filled ellipse with a <see cref="Ray"/> (hit must be at or ahead of the origin).</summary>
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
            double a = this.shape.SemiAxisX;
            double b = this.shape.SemiAxisY;
            if (a > 0 && b > 0 && ((hit.X * hit.X) / (a * a)) + ((hit.Y * hit.Y) / (b * b)) <= 1.0)
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
        public PlacedEllipse Translate(Vector offset)
        {
            return new PlacedEllipse(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with both semi-axes scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedEllipse Scale(double factor)
        {
            return new PlacedEllipse(new Ellipse(this.shape.SemiAxisX * factor, this.shape.SemiAxisY * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedEllipse Transform(Pose transform)
        {
            return new PlacedEllipse(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedEllipse e1, PlacedEllipse e2)
        {
            return e1.Shape == e2.Shape && e1.Pose == e2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedEllipse e1, PlacedEllipse e2)
        {
            return !(e1 == e2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedEllipse e && this.Equals(e);
        }

        /// <summary>Equality with another placed ellipse (conceptual shape and pose).</summary>
        public bool Equals(PlacedEllipse other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed ellipse within an absolute tolerance.</summary>
        public bool Equals(PlacedEllipse other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Ellipse shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(a, b) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(a, b) @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// The point on the ellipse boundary (semi-axes <paramref name="a"/>, <paramref name="b"/>) closest
        /// to the local point (<paramref name="px"/>, <paramref name="py"/>), by Eberly's method. Handles
        /// the query in any quadrant and either axis ordering by reflecting and swapping.
        /// </summary>
        private static (double, double) ClosestPointOnEllipse(double a, double b, double px, double py)
        {
            if (a <= 0 || b <= 0)
            {
                return (0, 0); // degenerate ellipse
            }

            double sx = px < 0 ? -1 : 1;
            double sy = py < 0 ? -1 : 1;
            double qx = Math.Abs(px);
            double qy = Math.Abs(py);

            bool swap = a < b;
            double e0 = swap ? b : a;
            double e1 = swap ? a : b;
            double y0 = swap ? qy : qx;
            double y1 = swap ? qx : qy;

            (double r0, double r1) = ClosestInFirstQuadrant(e0, e1, y0, y1);

            double resultX = swap ? r1 : r0;
            double resultY = swap ? r0 : r1;
            return (sx * resultX, sy * resultY);
        }

        /// <summary>Eberly's first-quadrant solver, requiring <c>e0 ≥ e1 &gt; 0</c> and <c>y0, y1 ≥ 0</c>.</summary>
        private static (double, double) ClosestInFirstQuadrant(double e0, double e1, double y0, double y1)
        {
            if (y1 > 0)
            {
                if (y0 > 0)
                {
                    double z0 = y0 / e0;
                    double z1 = y1 / e1;
                    double g = (z0 * z0) + (z1 * z1) - 1.0;
                    if (g != 0)
                    {
                        double r0 = (e0 / e1) * (e0 / e1);
                        double sbar = GetRoot(r0, z0, z1, g);
                        return ((r0 * y0) / (sbar + r0), y1 / (sbar + 1.0));
                    }

                    return (y0, y1); // already on the ellipse
                }

                return (0, e1); // on the minor axis
            }

            double numer0 = e0 * y0;
            double denom0 = (e0 * e0) - (e1 * e1);
            if (numer0 < denom0)
            {
                double xde0 = numer0 / denom0;
                return (e0 * xde0, e1 * Math.Sqrt(Math.Max(0, 1.0 - (xde0 * xde0))));
            }

            return (e0, 0); // closest is the major-axis vertex
        }

        /// <summary>Bisection for the parameter in Eberly's method.</summary>
        private static double GetRoot(double r0, double z0, double z1, double g)
        {
            double n0 = r0 * z0;
            double s0 = z1 - 1.0;
            double s1 = g < 0 ? 0 : (Math.Sqrt((n0 * n0) + (z1 * z1)) - 1.0);
            double s = 0;
            for (int i = 0; i < 64; i++)
            {
                s = (s0 + s1) / 2.0;
                if (s == s0 || s == s1)
                {
                    break;
                }

                double ratio0 = n0 / (s + r0);
                double ratio1 = z1 / (s + 1.0);
                g = (ratio0 * ratio0) + (ratio1 * ratio1) - 1.0;
                if (g > 0)
                {
                    s0 = s;
                }
                else if (g < 0)
                {
                    s1 = s;
                }
                else
                {
                    break;
                }
            }

            return s;
        }

        #endregion
    }
}
