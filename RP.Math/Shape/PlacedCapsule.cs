namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Capsule"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Capsule"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the capsule's central segment runs along local +Z from
    /// <c>(0, 0, −h/2)</c> to <c>(0, 0, +h/2)</c> (with <c>h</c> the cylinder height), and the solid is
    /// every point within the radius of that segment. The pose maps that frame into the world.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedCapsule : IEquatable<PlacedCapsule>, IFormattable
    {
        #region Fields

        private readonly Capsule shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed capsule from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedCapsule(Capsule shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A capsule standing along the world +Z axis, centred at <paramref name="center"/>.</summary>
        public static PlacedCapsule AlongZ(Capsule shape, Vector center)
        {
            return new PlacedCapsule(shape, Pose.At(center));
        }

        /// <summary>
        /// A capsule whose central segment runs between the two hemisphere centres
        /// <paramref name="segmentStart"/> and <paramref name="segmentEnd"/>.
        /// </summary>
        public static PlacedCapsule FromSegment(Vector segmentStart, Vector segmentEnd, double radius)
        {
            Vector axis = segmentEnd - segmentStart;
            var shape = new Capsule(radius, axis.Magnitude);
            return new PlacedCapsule(shape, new Pose((segmentStart + segmentEnd) / 2.0, RotationToAxis(axis)));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) capsule.</summary>
        public Capsule Shape { get { return this.shape; } }

        /// <summary>The placement of the capsule.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The unit axis the capsule runs along (the pose's local +Z).</summary>
        public Vector Axis { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>One hemisphere centre (the end of the central segment in the −Z direction).</summary>
        public Vector SegmentStart { get { return this.pose.Apply(new Vector(0, 0, -this.shape.CylinderHeight / 2.0)); } }

        /// <summary>The other hemisphere centre (the end of the central segment in the +Z direction).</summary>
        public Vector SegmentEnd { get { return this.pose.Apply(new Vector(0, 0, this.shape.CylinderHeight / 2.0)); } }

        /// <summary>The radius (from the conceptual shape).</summary>
        public double Radius { get { return this.shape.Radius; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the capsule.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the capsule, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            double half = this.shape.CylinderHeight / 2.0;
            double zc = local.Z < -half ? -half : (local.Z > half ? half : local.Z);
            double dz = local.Z - zc;
            double distSquared = (local.X * local.X) + (local.Y * local.Y) + (dz * dz);
            double r = this.shape.Radius + tolerance;
            return distSquared <= r * r;
        }

        /// <summary>The point on or within the capsule closest to <paramref name="point"/> (a swept-segment projection).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double half = this.shape.CylinderHeight / 2.0;
            double zc = local.Z < -half ? -half : (local.Z > half ? half : local.Z);
            Vector axisPoint = new Vector(0, 0, zc);
            Vector offset = local - axisPoint;
            double dist = offset.Magnitude;

            if (dist <= this.shape.Radius)
            {
                return this.pose.Apply(local); // already inside
            }

            return this.pose.Apply(axisPoint + (offset * (this.shape.Radius / dist)));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the capsule with an infinite <see cref="Line"/>. Returns true and the two surface
        /// crossing points (<paramref name="near"/>, <paramref name="far"/>); false when the line misses.
        /// </summary>
        /// <remarks>
        /// Maths, in local space: the straight middle is the cylinder <c>x² + y² = r²</c> within the
        /// segment, and each end is a sphere of radius <c>r</c> at the segment end — kept only on its outer
        /// hemisphere (beyond the segment). Nearest and farthest survivors are the entry and exit.
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

        /// <summary>Intersect the capsule with a <see cref="Ray"/> (nearest hit at or ahead of the origin).</summary>
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
            var hits = new List<double>(4);
            const double eps = 1e-9;
            double half = this.shape.CylinderHeight / 2.0;
            double r = this.shape.Radius;

            // Cylindrical middle: x² + y² = r², kept within the segment.
            double a = (ld.X * ld.X) + (ld.Y * ld.Y);
            if (a > eps)
            {
                double b = 2.0 * ((lo.X * ld.X) + (lo.Y * ld.Y));
                double c = (lo.X * lo.X) + (lo.Y * lo.Y) - (r * r);
                foreach (double t in PolynomialRoots.SolveQuadratic(a, b, c))
                {
                    double z = lo.Z + (t * ld.Z);
                    if (z >= -half - eps && z <= half + eps)
                    {
                        hits.Add(t);
                    }
                }
            }

            // End hemispheres: spheres at z = ±half, each kept only on its outer side.
            AddHemisphere(hits, lo, ld, -half, isTop: false, r, eps);
            AddHemisphere(hits, lo, ld, half, isTop: true, r, eps);
            return hits;
        }

        private static void AddHemisphere(List<double> hits, Vector lo, Vector ld, double capZ, bool isTop, double r, double eps)
        {
            // |(lo + t·ld) − (0,0,capZ)|² = r², with |ld| = 1.
            Vector m = new Vector(lo.X, lo.Y, lo.Z - capZ);
            double b = m.DotProduct(ld);
            double c = m.MagnitudeSquared - (r * r);
            double disc = (b * b) - c;
            if (disc < 0)
            {
                return;
            }

            double s = Math.Sqrt(disc);
            foreach (double t in new[] { -b - s, -b + s })
            {
                double z = lo.Z + (t * ld.Z);
                bool onOuterHalf = isTop ? z >= capZ - eps : z <= capZ + eps;
                if (onOuterHalf)
                {
                    hits.Add(t);
                }
            }
        }

        #endregion

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedCapsule Translate(Vector offset)
        {
            return new PlacedCapsule(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the radius and cylinder height scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedCapsule Scale(double factor)
        {
            return new PlacedCapsule(new Capsule(this.shape.Radius * factor, this.shape.CylinderHeight * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedCapsule Transform(Pose transform)
        {
            return new PlacedCapsule(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedCapsule c1, PlacedCapsule c2)
        {
            return c1.Shape == c2.Shape && c1.Pose == c2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedCapsule c1, PlacedCapsule c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedCapsule c && this.Equals(c);
        }

        /// <summary>Equality with another placed capsule (conceptual shape and pose).</summary>
        public bool Equals(PlacedCapsule other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed capsule within an absolute tolerance.</summary>
        public bool Equals(PlacedCapsule other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Capsule shape, out Pose pose)
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

        /// <summary>A rotation turning local +Z to point along <paramref name="axis"/>.</summary>
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
