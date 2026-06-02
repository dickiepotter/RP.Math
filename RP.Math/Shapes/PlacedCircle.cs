namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Circle"/> (a flat disc) placed in 3D space by a <see cref="Pose"/> — the positioned
    /// partner of the conceptual <see cref="Circle"/>.
    /// </summary>
    /// <remarks>
    /// In the disc's own local frame it lies in the XY plane centred at the origin, with its normal along
    /// local +Z; the pose maps that frame into the world. Because a disc is rotationally symmetric about
    /// its normal, any spin of the pose about local +Z leaves it unchanged — so, like the older positioned
    /// circle, a centre and a normal fully describe it.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedCircle : IEquatable<PlacedCircle>, IFormattable
    {
        #region Fields

        private readonly Circle shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed circle from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedCircle(Circle shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A disc lying flat in the world XY plane (normal +Z), centred at <paramref name="center"/>.</summary>
        public static PlacedCircle InXYPlane(Circle shape, Vector center)
        {
            return new PlacedCircle(shape, Pose.At(center));
        }

        /// <summary>A disc at <paramref name="center"/> whose plane has the given <paramref name="normal"/>.</summary>
        public static PlacedCircle FromCenterNormal(Circle shape, Vector center, Vector normal)
        {
            return new PlacedCircle(shape, new Pose(center, RotationToNormal(normal)));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) circle.</summary>
        public Circle Shape { get { return this.shape; } }

        /// <summary>The placement of the disc.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit normal of the disc's plane (the pose's local +Z).</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The supporting plane the disc lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Center, this.Normal); } }

        /// <summary>The radius (from the conceptual shape).</summary>
        public double Radius { get { return this.shape.Radius; } }

        /// <summary>The diameter (from the conceptual shape).</summary>
        public double Diameter { get { return this.shape.Diameter; } }

        /// <summary>The enclosed area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        /// <summary>The circumference (from the conceptual shape).</summary>
        public double Perimeter { get { return this.shape.Circumference; } }

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
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > tolerance)
            {
                return false;
            }

            double r = this.shape.Radius + tolerance;
            return (local.X * local.X) + (local.Y * local.Y) <= r * r;
        }

        /// <summary>The point on the disc closest to <paramref name="point"/> (projected onto the plane, then clamped to the radius).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double distance = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            if (distance <= this.shape.Radius || distance == 0)
            {
                return this.pose.Apply(new Vector(local.X, local.Y, 0));
            }

            double scale = this.shape.Radius / distance;
            return this.pose.Apply(new Vector(local.X * scale, local.Y * scale, 0));
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the disc with an infinite <see cref="Line"/>. A line meets the flat disc at most once,
        /// so on a hit <paramref name="near"/> and <paramref name="far"/> are the same crossing point; false
        /// when the line crosses the plane outside the radius or runs parallel.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            bool hit = this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out Vector point);
            near = far = point;
            return hit;
        }

        /// <summary>Intersect the disc with a <see cref="Ray"/> (hit must be at or ahead of the origin).</summary>
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
            if ((hit.X * hit.X) + (hit.Y * hit.Y) <= this.shape.Radius * this.shape.Radius)
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
        public PlacedCircle Translate(Vector offset)
        {
            return new PlacedCircle(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the radius scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedCircle Scale(double factor)
        {
            return new PlacedCircle(new Circle(this.shape.Radius * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedCircle Transform(Pose transform)
        {
            return new PlacedCircle(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedCircle c1, PlacedCircle c2)
        {
            return c1.Shape == c2.Shape && c1.Pose == c2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedCircle c1, PlacedCircle c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedCircle c && this.Equals(c);
        }

        /// <summary>Equality with another placed circle (conceptual shape and pose).</summary>
        public bool Equals(PlacedCircle other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed circle within an absolute tolerance.</summary>
        public bool Equals(PlacedCircle other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Circle shape, out Pose pose)
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

        #region Helpers

        /// <summary>A rotation turning local +Z to point along <paramref name="normal"/> (defaults to +Z for a zero normal).</summary>
        private static Quaternion RotationToNormal(Vector normal)
        {
            Vector target = normal.NormalizeOrDefault();
            if (target.IsZero())
            {
                return Quaternion.Identity;
            }

            Vector z = new Vector(0, 0, 1);
            double dot = z.DotProduct(target);
            if (dot >= 1.0 - 1e-12)
            {
                return Quaternion.Identity;
            }

            if (dot <= -1.0 + 1e-12)
            {
                return Quaternion.FromAxisAngle(new Vector(1, 0, 0), new Angle(Math.PI));
            }

            return Quaternion.FromAxisAngle(z.CrossProduct(target), new Angle(Math.Acos(dot)));
        }

        #endregion
    }
}
