namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Cylinder"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Cylinder"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conceptual <see cref="Shape"/> supplies the radius and height and all size-only maths; the
    /// <see cref="Pose"/> supplies the placement. In the cylinder's own local frame it runs along local
    /// +Z, centred at the origin, so the two caps sit at local <c>z = ±height/2</c>. Because a cylinder is
    /// rotationally symmetric about its axis, any spin of the pose about local +Z leaves it unchanged.
    /// </para>
    /// <para>
    /// World queries map the world point into that local frame with <see cref="Pose.ApplyInverse"/>, where
    /// the axis is simply +Z and a point splits cleanly into a <i>height</i> coordinate (local z) and a
    /// <i>radial</i> distance (local x,y), then map any returned point forward again.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedCylinder : IEquatable<PlacedCylinder>, IFormattable
    {
        #region Fields

        private readonly Cylinder shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed cylinder from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedCylinder(Cylinder shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A cylinder standing along the world +Z axis, centred at <paramref name="center"/>.</summary>
        public static PlacedCylinder AlongZ(Cylinder shape, Vector center)
        {
            return new PlacedCylinder(shape, Pose.At(center));
        }

        /// <summary>
        /// A cylinder spanning the two cap centres <paramref name="baseCenter"/> and
        /// <paramref name="topCenter"/>: its axis runs between them and its height is their distance apart.
        /// </summary>
        public static PlacedCylinder FromEndPoints(Vector baseCenter, Vector topCenter, double radius)
        {
            Vector axis = topCenter - baseCenter;
            var shape = new Cylinder(radius, axis.Magnitude);
            Vector center = (baseCenter + topCenter) / 2.0;
            return new PlacedCylinder(shape, new Pose(center, RotationToAxis(axis)));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) cylinder.</summary>
        public Cylinder Shape { get { return this.shape; } }

        /// <summary>The placement (position and orientation) of the cylinder.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre of the cylinder in world space (midway along the axis).</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit axis the cylinder runs along (the pose's local +Z).</summary>
        public Vector Axis { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The radius (from the conceptual shape).</summary>
        public double Radius { get { return this.shape.Radius; } }

        /// <summary>The height (from the conceptual shape).</summary>
        public double Height { get { return this.shape.Height; } }

        /// <summary>The centre of the base cap (half the height back along the axis).</summary>
        public Vector BaseCenter { get { return this.pose.Apply(new Vector(0, 0, -this.shape.Height / 2.0)); } }

        /// <summary>The centre of the top cap (half the height forward along the axis).</summary>
        public Vector TopCenter { get { return this.pose.Apply(new Vector(0, 0, this.shape.Height / 2.0)); } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The curved (side) area (from the conceptual shape).</summary>
        public double LateralArea { get { return this.shape.LateralArea; } }

        /// <summary>The total surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the cylinder.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the cylinder, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > (this.shape.Height / 2.0) + tolerance)
            {
                return false;
            }

            double radialSquared = (local.X * local.X) + (local.Y * local.Y);
            double r = this.shape.Radius + tolerance;
            return radialSquared <= r * r;
        }

        /// <summary>
        /// The point on or within the cylinder closest to <paramref name="point"/> (the height coordinate
        /// clamped to half the height, and the radial offset clamped to the radius — the cylinder's
        /// symmetry lets the two be clamped independently).
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double half = this.shape.Height / 2.0;
            double z = local.Z < -half ? -half : (local.Z > half ? half : local.Z);

            double radialDist = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            double x = local.X;
            double y = local.Y;
            if (radialDist > this.shape.Radius)
            {
                double scale = this.shape.Radius / radialDist;
                x *= scale;
                y *= scale;
            }

            return this.pose.Apply(new Vector(x, y, z));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the cylinder with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/> entering, <paramref name="far"/> leaving; equal when
        /// the line grazes the cylinder); false when the line misses it entirely.
        /// </summary>
        /// <remarks>
        /// Maths, in local space: a line can cross the curved side or a flat cap. The curved-side crossings
        /// come from substituting the line into <c>x² + y² = r²</c> (a quadratic in <c>t</c>) and keeping
        /// only roots whose local z lands between the caps; the cap crossings come from reaching each cap
        /// plane (<c>z = ±h/2</c>) and keeping it only within the radius. The nearest and farthest
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

        /// <summary>
        /// Intersect the cylinder with a <see cref="Ray"/>. Returns true and the nearest surface point at
        /// or ahead of the ray's origin (if the origin is inside, the forward exit point); false when the
        /// cylinder is missed or lies entirely behind the ray.
        /// </summary>
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

        /// <summary>
        /// The parameters <c>t</c> at which the local line <c>lo + t·ld</c> crosses the local cylinder's
        /// surface — its curved side (within the height) and its two caps (within the radius).
        /// </summary>
        private List<double> SurfaceParameters(Vector lo, Vector ld)
        {
            var hits = new List<double>(4);
            const double eps = 1e-9;
            double half = this.shape.Height / 2.0;
            double r = this.shape.Radius;

            // Curved side: x² + y² = r² in local space.
            double a = (ld.X * ld.X) + (ld.Y * ld.Y);
            if (a > eps)
            {
                double b = 2.0 * ((lo.X * ld.X) + (lo.Y * ld.Y));
                double c = (lo.X * lo.X) + (lo.Y * lo.Y) - (r * r);
                double disc = (b * b) - (4.0 * a * c);
                if (disc >= 0)
                {
                    double s = Math.Sqrt(disc);
                    foreach (double t in new[] { (-b - s) / (2.0 * a), (-b + s) / (2.0 * a) })
                    {
                        double z = lo.Z + (t * ld.Z);
                        if (Math.Abs(z) <= half + eps)
                        {
                            hits.Add(t);
                        }
                    }
                }
            }

            // Caps: the planes z = ±half, kept only where the hit lands within the radius.
            if (Math.Abs(ld.Z) > eps)
            {
                foreach (double end in new[] { -half, half })
                {
                    double t = (end - lo.Z) / ld.Z;
                    double x = lo.X + (t * ld.X);
                    double y = lo.Y + (t * ld.Y);
                    if ((x * x) + (y * y) <= (r * r) + eps)
                    {
                        hits.Add(t);
                    }
                }
            }

            return hits;
        }

        #endregion

        #region Modification (returns a new placed cylinder)

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedCylinder Translate(Vector offset)
        {
            return new PlacedCylinder(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the conceptual shape's radius and height scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedCylinder Scale(double factor)
        {
            return new PlacedCylinder(new Cylinder(this.shape.Radius * factor, this.shape.Height * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedCylinder Transform(Pose transform)
        {
            return new PlacedCylinder(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedCylinder c1, PlacedCylinder c2)
        {
            return c1.Shape == c2.Shape && c1.Pose == c2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedCylinder c1, PlacedCylinder c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedCylinder c && this.Equals(c);
        }

        /// <summary>Equality with another placed cylinder (conceptual shape and pose).</summary>
        public bool Equals(PlacedCylinder other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed cylinder within an absolute tolerance.</summary>
        public bool Equals(PlacedCylinder other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Cylinder shape, out Pose pose)
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

        /// <summary>
        /// A rotation that turns the local +Z axis to point along <paramref name="axis"/> (used to build a
        /// pose from two cap centres). Identity when already +Z; a half turn about +X when exactly −Z.
        /// </summary>
        private static Quaternion RotationToAxis(Vector axis)
        {
            Vector target = axis.NormalizeOrDefault();
            if (target.IsZero())
            {
                return Quaternion.Identity;
            }

            Vector z = new Vector(0, 0, 1);
            double dot = z.DotProduct(target);
            if (dot >= 1.0 - 1e-12)
            {
                return Quaternion.Identity; // already along +Z
            }

            if (dot <= -1.0 + 1e-12)
            {
                return Quaternion.FromAxisAngle(new Vector(1, 0, 0), new Angle(Math.PI)); // opposite: half turn
            }

            Vector rotationAxis = z.CrossProduct(target);
            return Quaternion.FromAxisAngle(rotationAxis, new Angle(Math.Acos(dot)));
        }

        #endregion
    }
}
