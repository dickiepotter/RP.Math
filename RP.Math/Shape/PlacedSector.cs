namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Sector"/> (pie slice) placed in 3D space by a <see cref="Pose"/> — the positioned
    /// partner of the conceptual <see cref="Sector"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the sector lies in the XY plane with its <b>apex at the origin</b> (the pose
    /// position is the apex, not the area centroid — the apex is the meaningful reference for a sector),
    /// sweeping from local +X anticlockwise through the shape's angle. The pose maps that frame into the
    /// world.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedSector : IEquatable<PlacedSector>, IFormattable
    {
        #region Fields

        private readonly Sector shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed sector from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedSector(Sector shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A sector lying flat in the world XY plane with its apex at <paramref name="apex"/>, sweeping from +X.</summary>
        public static PlacedSector InXYPlane(Sector shape, Vector apex)
        {
            return new PlacedSector(shape, Pose.At(apex));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) sector.</summary>
        public Sector Shape { get { return this.shape; } }

        /// <summary>The placement of the sector.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The apex (the circle's centre) in world space.</summary>
        public Vector Apex { get { return this.pose.Position; } }

        /// <summary>The unit normal of the sector's plane (the pose's local +Z).</summary>
        public Vector Normal { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The supporting plane the sector lies in.</summary>
        public Plane Plane { get { return Plane.FromPointNormal(this.Apex, this.Normal); } }

        /// <summary>The two arc-end points in world space (at angle 0 and at the swept angle).</summary>
        public Vector ArcStart { get { return this.pose.Apply(new Vector(this.shape.Radius, 0, 0)); } }

        /// <summary>The far arc-end point in world space (at the swept angle).</summary>
        public Vector ArcEnd
        {
            get
            {
                double theta = this.shape.Angle.Rad;
                return this.pose.Apply(new Vector(this.shape.Radius * Math.Cos(theta), this.shape.Radius * Math.Sin(theta), 0));
            }
        }

        /// <summary>
        /// The area centroid in world space — along the bisector at distance
        /// <c>(2/3)·r·sin(θ/2)/(θ/2)</c> from the apex.
        /// </summary>
        public Vector Centroid
        {
            get
            {
                double half = this.shape.Angle.Rad / 2.0;
                double distance = half == 0 ? 0 : (2.0 / 3.0) * this.shape.Radius * Math.Sin(half) / half;
                return this.pose.Apply(new Vector(distance * Math.Cos(half), distance * Math.Sin(half), 0));
            }
        }

        /// <summary>The enclosed area (from the conceptual shape).</summary>
        public double Area { get { return this.shape.Area; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on the filled sector (on its plane, within the radius and the swept angle).</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on the filled sector within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            if (Math.Abs(local.Z) > tolerance)
            {
                return false;
            }

            return this.WithinSector(local.X, local.Y, tolerance);
        }

        /// <summary>
        /// The point on the filled sector closest to <paramref name="point"/> — the nearest of its
        /// interior, its arc and its two straight radius edges.
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            Vector flat = new Vector(local.X, local.Y, 0);
            if (this.WithinSector(local.X, local.Y, 0))
            {
                return this.pose.Apply(flat); // projection is already inside
            }

            double theta = this.shape.Angle.Rad;
            double r = this.shape.Radius;
            Vector apex = new Vector(0, 0, 0);
            Vector edge0End = new Vector(r, 0, 0);
            Vector edge1End = new Vector(r * Math.Cos(theta), r * Math.Sin(theta), 0);

            // Candidates: the two straight edges, and the arc point at the query's angle (if within the sweep).
            Vector best = new LineSegment(apex, edge0End).ClosestPointTo(flat);
            double bestDist = best.DistanceSquared(flat);

            Consider(new LineSegment(apex, edge1End).ClosestPointTo(flat), flat, ref best, ref bestDist);

            double ang = Math.Atan2(local.Y, local.X);
            if (ang < 0)
            {
                ang += 2.0 * Math.PI;
            }

            if (ang <= theta)
            {
                Consider(new Vector(r * Math.Cos(ang), r * Math.Sin(ang), 0), flat, ref best, ref bestDist);
            }

            return this.pose.Apply(best);
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>Intersect the filled sector with an infinite <see cref="Line"/>.</summary>
        public bool TryIntersect(Line line, out Vector point)
        {
            return this.TryIntersectLocal(line.Point, line.Direction, requireForward: false, out point);
        }

        /// <summary>Intersect the filled sector with a <see cref="Ray"/> (hit must be at or ahead of the origin).</summary>
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
            if (this.WithinSector(hit.X, hit.Y, 0))
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
        public PlacedSector Translate(Vector offset)
        {
            return new PlacedSector(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with the radius scaled by <paramref name="factor"/> (angle and placement unchanged).</summary>
        public PlacedSector Scale(double factor)
        {
            return new PlacedSector(new Sector(this.shape.Radius * factor, this.shape.Angle), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedSector Transform(Pose transform)
        {
            return new PlacedSector(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedSector s1, PlacedSector s2)
        {
            return s1.Shape == s2.Shape && s1.Pose == s2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedSector s1, PlacedSector s2)
        {
            return !(s1 == s2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedSector s && this.Equals(s);
        }

        /// <summary>Equality with another placed sector (conceptual shape and pose).</summary>
        public bool Equals(PlacedSector other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed sector within an absolute tolerance.</summary>
        public bool Equals(PlacedSector other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Sector shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(r, θ) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(r, θ) @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion

        #region Helpers

        /// <summary>Whether the local in-plane point (x, y) is within the radius and the swept angle (with slack).</summary>
        private bool WithinSector(double x, double y, double tolerance)
        {
            double r = Math.Sqrt((x * x) + (y * y));
            if (r > this.shape.Radius + tolerance)
            {
                return false;
            }

            if (r <= tolerance)
            {
                return true; // at the apex
            }

            double ang = Math.Atan2(y, x);
            if (ang < 0)
            {
                ang += 2.0 * Math.PI;
            }

            double angleTolerance = r > 0 ? tolerance / r : 0; // arc-length slack converted to an angle
            return ang <= this.shape.Angle.Rad + angleTolerance;
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
