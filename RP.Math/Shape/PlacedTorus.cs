namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <see cref="Torus"/> placed in 3D space by a <see cref="Pose"/> — the positioned partner of the
    /// conceptual <see cref="Torus"/>.
    /// </summary>
    /// <remarks>
    /// In its own local frame the torus lies in the XY plane, centred at the origin, with its axis along
    /// local +Z; the tube's central circle has the major radius and the tube itself the minor radius. The
    /// pose maps that frame into the world. Line/ray intersection is exact: substituting the line into the
    /// torus's implicit equation yields a quartic in the line parameter, solved by
    /// <see cref="PolynomialRoots.SolveQuartic"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct PlacedTorus : IEquatable<PlacedTorus>, IFormattable
    {
        #region Fields

        private readonly Torus shape;
        private readonly Pose pose;

        #endregion

        #region Constructors

        /// <summary>Construct a placed torus from a conceptual <paramref name="shape"/> and a <paramref name="pose"/>.</summary>
        public PlacedTorus(Torus shape, Pose pose)
        {
            this.shape = shape;
            this.pose = pose;
        }

        #endregion

        #region Factories

        /// <summary>A torus lying flat in the world XY plane (axis +Z), centred at <paramref name="center"/>.</summary>
        public static PlacedTorus InXYPlane(Torus shape, Vector center)
        {
            return new PlacedTorus(shape, Pose.At(center));
        }

        #endregion

        #region Accessors

        /// <summary>The conceptual (unpositioned) torus.</summary>
        public Torus Shape { get { return this.shape; } }

        /// <summary>The placement of the torus.</summary>
        public Pose Pose { get { return this.pose; } }

        /// <summary>The centre in world space.</summary>
        public Vector Center { get { return this.pose.Position; } }

        /// <summary>The geometric centre (canonical origin) of the shape.</summary>
        public Vector Centroid { get { return this.pose.Position; } }

        /// <summary>The axis through the hole (the pose's local +Z).</summary>
        public Vector Axis { get { return this.pose.ApplyDirection(new Vector(0, 0, 1)); } }

        /// <summary>The major radius (from the conceptual shape).</summary>
        public double MajorRadius { get { return this.shape.MajorRadius; } }

        /// <summary>The minor radius (from the conceptual shape).</summary>
        public double MinorRadius { get { return this.shape.MinorRadius; } }

        /// <summary>The enclosed volume (from the conceptual shape).</summary>
        public double Volume { get { return this.shape.Volume; } }

        /// <summary>The surface area (from the conceptual shape).</summary>
        public double SurfaceArea { get { return this.shape.SurfaceArea; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the torus's tube.</summary>
        public bool Contains(Vector point)
        {
            return this.Contains(point, 0);
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the tube, within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            Vector local = this.pose.ApplyInverse(point);
            double inPlane = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));
            double q = inPlane - this.shape.MajorRadius;
            double distToTubeCircle = Math.Sqrt((q * q) + (local.Z * local.Z));
            return distToTubeCircle <= this.shape.MinorRadius + tolerance;
        }

        /// <summary>
        /// The point on or within the torus closest to <paramref name="point"/>: project onto the central
        /// circle, then clamp the offset to the tube radius.
        /// </summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector local = this.pose.ApplyInverse(point);
            double inPlane = Math.Sqrt((local.X * local.X) + (local.Y * local.Y));

            Vector circlePoint = inPlane > 0
                ? new Vector(local.X * (this.shape.MajorRadius / inPlane), local.Y * (this.shape.MajorRadius / inPlane), 0)
                : new Vector(this.shape.MajorRadius, 0, 0); // on the axis: any tube-circle point will do

            Vector offset = local - circlePoint;
            double dist = offset.Magnitude;
            if (dist <= this.shape.MinorRadius)
            {
                return this.pose.Apply(local); // inside the tube
            }

            return this.pose.Apply(circlePoint + (offset * (this.shape.MinorRadius / dist)));
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the torus with an infinite <see cref="Line"/>. Returns true and the nearest and
        /// farthest of up to four crossing points (<paramref name="near"/>, <paramref name="far"/>); false
        /// when the line misses the tube.
        /// </summary>
        /// <remarks>
        /// Maths, in local space: the torus is <c>(x² + y² + z² + R² − r²)² = 4R²(x² + y²)</c>. Substituting
        /// the line <c>O + t·D</c> gives a quartic in <c>t</c>; its real roots are the surface crossings
        /// (a line can pierce a torus up to four times). The smallest and largest are reported.
        /// </remarks>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            Vector lo = this.pose.ApplyInverse(line.Point);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(line.Direction);
            double[] roots = this.SurfaceParameters(lo, ld);
            if (roots.Length == 0)
            {
                near = far = line.Point;
                return false;
            }

            double tMin = double.PositiveInfinity, tMax = double.NegativeInfinity;
            foreach (double t in roots)
            {
                if (t < tMin) tMin = t;
                if (t > tMax) tMax = t;
            }

            near = this.pose.Apply(lo + (tMin * ld));
            far = this.pose.Apply(lo + (tMax * ld));
            return true;
        }

        /// <summary>Intersect the torus with a <see cref="Ray"/> (nearest hit at or ahead of the origin).</summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            Vector lo = this.pose.ApplyInverse(ray.Origin);
            Vector ld = this.pose.Rotation.Conjugate().Rotate(ray.Direction);
            double[] roots = this.SurfaceParameters(lo, ld);

            double best = double.PositiveInfinity;
            foreach (double t in roots)
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

        /// <summary>The real roots of the torus quartic for the local line <c>lo + t·ld</c> (with <c>ld</c> unit length).</summary>
        private double[] SurfaceParameters(Vector lo, Vector ld)
        {
            double r2 = this.shape.MajorRadius * this.shape.MajorRadius;
            double k = r2 - (this.shape.MinorRadius * this.shape.MinorRadius); // R² − r²

            double g = lo.DotProduct(lo);                       // |O|²
            double h = lo.DotProduct(ld);                       // O·D   (|D| = 1)
            double pPlane = (ld.X * ld.X) + (ld.Y * ld.Y);       // Dx² + Dy²
            double qPlane = (lo.X * ld.X) + (lo.Y * ld.Y);       // Ox·Dx + Oy·Dy
            double sPlane = (lo.X * lo.X) + (lo.Y * lo.Y);       // Ox² + Oy²

            double a4 = 1.0;
            double a3 = 4.0 * h;
            double a2 = (2.0 * g) + (4.0 * h * h) + (2.0 * k) - (4.0 * r2 * pPlane);
            double a1 = (4.0 * g * h) + (4.0 * k * h) - (8.0 * r2 * qPlane);
            double a0 = (g * g) + (2.0 * k * g) + (k * k) - (4.0 * r2 * sPlane);

            return PolynomialRoots.SolveQuartic(a4, a3, a2, a1, a0);
        }

        #endregion

        #region Modification

        /// <summary>A copy translated by <paramref name="offset"/> in world space.</summary>
        public PlacedTorus Translate(Vector offset)
        {
            return new PlacedTorus(this.shape, this.pose.Translate(offset));
        }

        /// <summary>A copy with both radii scaled by <paramref name="factor"/> (placement unchanged).</summary>
        public PlacedTorus Scale(double factor)
        {
            return new PlacedTorus(new Torus(this.shape.MajorRadius * factor, this.shape.MinorRadius * factor), this.pose);
        }

        /// <summary>A copy whose placement is composed with <paramref name="transform"/> (applied after the current pose).</summary>
        public PlacedTorus Transform(Pose transform)
        {
            return new PlacedTorus(this.shape, transform * this.pose);
        }

        #endregion

        #region Operators

        /// <summary>Equality of conceptual shape and pose.</summary>
        public static bool operator ==(PlacedTorus t1, PlacedTorus t2)
        {
            return t1.Shape == t2.Shape && t1.Pose == t2.Pose;
        }

        /// <summary>Inequality of conceptual shape or pose.</summary>
        public static bool operator !=(PlacedTorus t1, PlacedTorus t2)
        {
            return !(t1 == t2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is PlacedTorus t && this.Equals(t);
        }

        /// <summary>Equality with another placed torus (conceptual shape and pose).</summary>
        public bool Equals(PlacedTorus other)
        {
            return this == other;
        }

        /// <summary>Equality with another placed torus within an absolute tolerance.</summary>
        public bool Equals(PlacedTorus other, double tolerance)
        {
            return this.shape.Equals(other.Shape, tolerance) && this.pose.Equals(other.Pose, tolerance);
        }

        /// <summary>A hash code derived from the conceptual shape and pose.</summary>
        public override int GetHashCode()
        {
            return this.shape.GetHashCode() ^ this.pose.GetHashCode();
        }

        /// <summary>Deconstruct into the conceptual shape and pose.</summary>
        public void Deconstruct(out Torus shape, out Pose pose)
        {
            shape = this.shape;
            pose = this.pose;
        }

        /// <summary>A string of the form <c>[(R, r) @ pose]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[(R, r) @ pose]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} @ {1}]",
                this.shape.ToString(format, formatProvider),
                this.pose.ToString(format, formatProvider));
        }

        #endregion
    }
}
