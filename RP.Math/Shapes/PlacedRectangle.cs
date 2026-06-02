namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Rectangle"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of
    /// the conceptual <see cref="Rectangle"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conceptual <see cref="Shape"/> supplies the size (width and height) and all size-only maths;
    /// the <see cref="Pose"/> supplies the placement. In the rectangle's own local frame it lies in the
    /// XY plane centred at the origin, with width along local +X and height along local +Y; the pose maps
    /// that frame into the world, so the rectangle's <see cref="Normal"/> is the pose's local +Z.
    /// </para>
    /// <para>
    /// Every world-space query works by mapping the world point back into that local frame with
    /// <see cref="Pose.ApplyInverse"/>, doing the simple axis-aligned maths there, and (where a point is
    /// returned) mapping the result forward again with <see cref="Pose.Apply"/>. Writing the geometry once
    /// in the canonical frame is the whole reason the conceptual shape and its placement are kept apart.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedRectangle : IEquatable<PlacedRectangle>, IFormattable
    {
        #region Fields

        private readonly Rectangle shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed rectangle from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedRectangle(Rectangle shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A rectangle lying flat in the world XY plane, centred at <paramref name="center"/>.</summary>
        public static PlacedRectangle InXYPlane(Rectangle shape, Vector center)
        {
            return new PlacedRectangle(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) rectangle.</summary>
        public Rectangle Shape { get { return this.shape; } }

        /// <summary>The placement (position and orientation) of the rectangle.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre of the rectangle in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit in-plane axis along which the width is measured (the pose's local +X).</summary>
        public Vector AxisU { get { return this.pose.ApplyDirection(new Vector(1, 0, 0)); } }

        /// <summary>The unit in-plane axis along which the height is measured (the pose's local +Y).</summary>
        public Vector AxisV { get { return this.pose.ApplyDirection(new Vector(0, 1, 0)); } }

        /// <summary>The unit normal of the rectangle's plane (the pose's local +Z).</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The width (from the conceptual shape).</summary>
        public double Width { get { return this.shape.Width; } }

        /// <summary>The height (from the conceptual shape).</summary>
        public double Height { get { return this.shape.Height; } }

        /// <summary>The enclosed area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        /// <summary>The perimeter (from the conceptual shape).</summary>
        public double Perimeter { get { return this.shape.Perimeter; } }

        /// <summary>The supporting plane the rectangle lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Center, this.Normal); } }

        /// <summary>
        /// The four world-space corners, in order: <c>(−U, −V)</c>, <c>(+U, −V)</c>, <c>(+U, +V)</c>,
        /// <c>(−U, +V)</c> relative to the centre — a consistent winding around the rectangle.
        /// </summary>
        public Vector[] Corners()
        {
            double hw = this.shape.Width / 2.0;
            double hh = this.shape.Height / 2.0;
            return new[]
            {
                this.pose.Apply(new Vector(-hw, -hh, 0)),
                this.pose.Apply(new Vector(hw, -hh, 0)),
                this.pose.Apply(new Vector(hw, hh, 0)),
                this.pose.Apply(new Vector(-hw, hh, 0)),
            };
        }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the filled rectangle (on its plane and within its edges).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled rectangle within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            return Math.Abs(local.Z) <= tolerance
                && Math.Abs(local.X) <= (this.shape.Width / 2.0) + tolerance
                && Math.Abs(local.Y) <= (this.shape.Height / 2.0) + tolerance;
        }

        /// <summary>
        /// The point on the filled rectangle closest to <paramref name="point"/> (projected onto the
        /// plane in local space, then clamped into the width and height extents).
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double x = Clamp(local.X, this.shape.Width / 2.0);
            double y = Clamp(local.Y, this.shape.Height / 2.0);
            return this.pose.Apply(new Vector(x, y, 0));
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the filled rectangle with an infinite <see cref="Line"/>. A line meets the flat
        /// rectangle at most once, so on a hit <paramref name="near"/> and <paramref name="far"/> are the
        /// same crossing point; false when it crosses the plane outside the edges or runs parallel to it.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            bool hit = this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out Vector point);
            near = far = point;
            return hit;
        }

        /// <summary>
        /// Intersect the filled rectangle with a <see cref="Ray"/>. As
        /// <see cref="TryIntersect(Line, out Vector, out Vector)"/>, but the hit must lie at or ahead of the ray's origin.
        /// </summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            return this.TryIntersectLocal(ray.Origin, ray.Direction, requireForward: true, out point);
        }

        /// <summary>
        /// Cross the line/ray with the rectangle's local XY plane, then keep the hit only if it lands
        /// within the width and height. Working in local space turns the plane into <c>z = 0</c>, so the
        /// crossing is simply where the local z-coordinate reaches zero.
        /// </summary>
        private bool TryIntersectLocal(Vector origin, Vector direction, bool requireForward, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(direction); // direction into local space (rotation only)

            if (ld.Z.AlmostEqualsWithAbsTolerance(0, double.Epsilon))
            {
                point = origin; // parallel to the plane
                return false;
            }

            double t = -lo.Z / ld.Z;
            if (requireForward && t < 0)
            {
                point = origin; // the plane crossing is behind the ray's origin
                return false;
            }

            Vector hit = lo + (t * ld);
            if (Math.Abs(hit.X) <= this.shape.Width / 2.0 && Math.Abs(hit.Y) <= this.shape.Height / 2.0)
            {
                point = this.pose.Apply(hit);
                return true;
            }

            point = origin;
            return false;
        }

        #endregion

        #region Modification (returns a new placed rectangle)

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedRectangle Translate(Vector offset)
        {
            return new PlacedRectangle(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the conceptual shape's width and height scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedRectangle Scale(double factor)
        {
            return new PlacedRectangle(new Rectangle(this.shape.Width * factor, this.shape.Height * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedRectangle Transform(Pose transform)
        {
            return new PlacedRectangle(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedRectangle r1, PlacedRectangle r2)
        {
            return r1.Shape == r2.Shape && r1.Pose == r2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedRectangle r1, PlacedRectangle r2)
        {
            return !(r1 == r2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedRectangle r && this.Equals(r);
        }

        /// <summary>Equality with another placed rectangle (conceptual shape and pose).</summary>
        public bool Equals(PlacedRectangle other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed rectangle within an absolute tolerance.</summary>
        public bool Equals(PlacedRectangle other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Rectangle shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[w×h @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[w×h @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>Clamp <paramref name="value"/> into the symmetric range [−half, +half].</summary>
        private static double Clamp(double value, double half)
        {
            return value < -half ? -half : (value > half ? half : value);
        }

        #endregion
    }
}
