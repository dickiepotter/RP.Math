namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Cuboid"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Cuboid"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conceptual <see cref="Shape"/> supplies the three edge lengths and all size-only maths; the
    /// <see cref="Pose"/> supplies the placement. In the cuboid's own local frame it is an axis-aligned
    /// box centred at the origin, with width, height and depth along local X, Y and Z; the pose maps that
    /// frame into the world.
    /// </para>
    /// <para>
    /// World queries map the world point back into that local frame with <see cref="Pose.ApplyInverse"/>,
    /// where the box is axis-aligned and the maths is at its simplest, then map any returned point forward
    /// again with <see cref="Pose.Apply"/>.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedCuboid : IEquatable<PlacedCuboid>, IFormattable
    {
        #region Fields

        private readonly Cuboid shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed cuboid from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedCuboid(Cuboid shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>An axis-aligned cuboid (no rotation) centred at <paramref name="center"/>.</summary>
        public static PlacedCuboid AxisAligned(Cuboid shape, Vector center)
        {
            return new PlacedCuboid(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) cuboid.</summary>
        public Cuboid Shape { get { return this.shape; } }

        /// <summary>The placement (position and orientation) of the cuboid.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre of the cuboid in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit axis along which the width is measured (the pose's local +X).</summary>
        public Vector AxisU { get { return this.pose.ApplyDirection(new Vector(1, 0, 0)); } }

        /// <summary>The unit axis along which the height is measured (the pose's local +Y).</summary>
        public Vector AxisV { get { return this.pose.ApplyDirection(new Vector(0, 1, 0)); } }

        /// <summary>The unit axis along which the depth is measured (the pose's local +Z).</summary>
        public Vector AxisW { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The width (from the conceptual shape).</summary>
        public double Width { get { return this.shape.Width; } }

        /// <summary>The height (from the conceptual shape).</summary>
        public double Height { get { return this.shape.Height; } }

        /// <summary>The depth (from the conceptual shape).</summary>
        public double Depth { get { return this.shape.Depth; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        /// <summary>The eight world-space corners (all eight sign combinations of the half-extents).</summary>
        public Vector[] Corners()
        {
            double hw = this.shape.Width / 2.0;
            double hh = this.shape.Height / 2.0;
            double hd = this.shape.Depth / 2.0;
            return new[]
            {
                this.pose.Apply(new Vector(-hw, -hh, -hd)),
                this.pose.Apply(new Vector(hw, -hh, -hd)),
                this.pose.Apply(new Vector(hw, hh, -hd)),
                this.pose.Apply(new Vector(-hw, hh, -hd)),
                this.pose.Apply(new Vector(-hw, -hh, hd)),
                this.pose.Apply(new Vector(hw, -hh, hd)),
                this.pose.Apply(new Vector(hw, hh, hd)),
                this.pose.Apply(new Vector(-hw, hh, hd)),
            };
        }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the cuboid.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the cuboid, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            return Math.Abs(local.X) <= (this.shape.Width / 2.0) + tolerance
                && Math.Abs(local.Y) <= (this.shape.Height / 2.0) + tolerance
                && Math.Abs(local.Z) <= (this.shape.Depth / 2.0) + tolerance;
        }

        /// <summary>
        /// The point on or within the cuboid closest to <paramref name="point"/> (each local coordinate
        /// clamped into its extent).
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            return this.pose.Apply(new Vector(
                Clamp(local.X, this.shape.Width / 2.0),
                Clamp(local.Y, this.shape.Height / 2.0),
                Clamp(local.Z, this.shape.Depth / 2.0)));
        }

        /// <summary>The distance from <paramref name="point"/> to the shape (zero when it lies on or within it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the cuboid with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/> entering, <paramref name="far"/> leaving; equal when
        /// the line grazes an edge or corner); false when the line misses the box entirely.
        /// </summary>
        /// <remarks>
        /// Maths: the slab test, done in local space where the box is axis-aligned. The box is the overlap
        /// of three slabs (one per axis); each slab limits the line's parameter <c>t</c> to an interval,
        /// and the line is inside only where all three overlap. Tracking the running overlap
        /// <c>[tNear, tFar]</c> and rejecting when it empties gives the entry and exit.
        /// </remarks>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector lo = this.pose.ApplyInverse(line.Point);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(line.Direction);
            if (!this.Slab(lo, ld, out double tNear, out double tFar))
            {
                near = far = line.Point;
                return false;
            }

            near = this.pose.Apply(lo + (tNear * ld));
            far = this.pose.Apply(lo + (tFar * ld));
            return true;
        }

        /// <summary>
        /// Intersect the cuboid with a <see cref="Ray"/>. Returns true and the nearest surface point at or
        /// ahead of the ray's origin (if the origin is inside, the forward exit point); false when the box
        /// is missed or lies entirely behind the ray.
        /// </summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(ray.Origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(ray.Direction);
            if (!this.Slab(lo, ld, out double tNear, out double tFar) || tFar < 0)
            {
                point = ray.Origin;
                return false;
            }

            double t = tNear >= 0 ? tNear : tFar; // origin inside the box: take the forward exit
            point = this.pose.Apply(lo + (t * ld));
            return true;
        }

        /// <summary>The slab test in the box's local (axis-aligned) frame; false when the overlap interval empties.</summary>
        private bool Slab(Vector origin, Vector direction, out double tNear, out double tFar)
        {
            tNear = double.NegativeInfinity;
            tFar = double.PositiveInfinity;

            double[] o = { origin.X, origin.Y, origin.Z };
            double[] d = { direction.X, direction.Y, direction.Z };
            double[] halves = { this.shape.Width / 2.0, this.shape.Height / 2.0, this.shape.Depth / 2.0 };

            for (int i = 0; i < 3; i++)
            {
                double h = halves[i];
                if (d[i].AlmostEqualsWithAbsTolerance(0, double.Epsilon))
                {
                    if (o[i] < -h || o[i] > h)
                    {
                        return false; // parallel to this slab and outside it
                    }

                    continue; // parallel but inside: this slab adds no constraint
                }

                double t1 = (-h - o[i]) / d[i];
                double t2 = (h - o[i]) / d[i];
                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                }

                if (t1 > tNear) tNear = t1;
                if (t2 < tFar) tFar = t2;
                if (tNear > tFar)
                {
                    return false; // the slabs' intervals no longer overlap
                }
            }

            return true;
        }

        #endregion

        #region Modification (returns a new placed cuboid)

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedCuboid Translate(Vector offset)
        {
            return new PlacedCuboid(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the conceptual shape's sizes scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedCuboid Scale(double factor)
        {
            return new PlacedCuboid(
                new Cuboid(this.shape.Width * factor, this.shape.Height * factor, this.shape.Depth * factor),
                this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedCuboid Transform(Pose transform)
        {
            return new PlacedCuboid(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedCuboid c1, PlacedCuboid c2)
        {
            return c1.Shape == c2.Shape && c1.Pose == c2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedCuboid c1, PlacedCuboid c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedCuboid c && this.Equals(c);
        }

        /// <summary>Equality with another placed cuboid (conceptual shape and pose).</summary>
        public bool Equals(PlacedCuboid other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed cuboid within an absolute tolerance.</summary>
        public bool Equals(PlacedCuboid other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Cuboid shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[w×h×d @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[w×h×d @ pose]</c> where components use <paramref name="format"/>.</summary>
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
