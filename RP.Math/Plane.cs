namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An immutable plane in 3D space, described by the equation <c>A·x + B·y + C·z + D = 0</c> where
    /// (A, B, C) is the plane's <see cref="Normal"/> and <c>D</c> is the signed offset such that the
    /// distance from the origin to the plane is <c>-D / |Normal|</c>.
    /// </summary>
    /// <remarks>
    /// Follows the <see cref="Vector"/> design: an immutable value type with static and instance forms,
    /// tolerance-aware equality, safe handling of the degenerate (zero-normal) plane, and formatting.
    /// Factories that build from geometry (<see cref="FromPointNormal"/>, <see cref="FromThreePoints"/>)
    /// produce a plane with a unit normal; the raw component constructor stores whatever is supplied
    /// (call <see cref="Normalize()"/> to get the normalized form).
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Plane : IEquatable<Plane>, IFormattable
    {
        #region Fields

        private readonly double a;
        private readonly double b;
        private readonly double c;
        private readonly double d;

        #endregion

        #region Constructors

        /// <summary>Construct a plane from the coefficients of <c>A·x + B·y + C·z + D = 0</c>.</summary>
        public Plane(double a, double b, double c, double d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        /// <summary>Construct a plane from a normal vector and the <c>D</c> offset coefficient.</summary>
        public Plane(Vector normal, double d)
        {
            this.a = normal.X;
            this.b = normal.Y;
            this.c = normal.Z;
            this.d = d;
        }

        #endregion

        #region Constants

        /// <summary>The plane z = 0 (the XY plane), with unit normal +Z.</summary>
        public static readonly Plane XY = new Plane(0, 0, 1, 0);

        /// <summary>The plane y = 0 (the XZ plane), with unit normal +Y.</summary>
        public static readonly Plane XZ = new Plane(0, 1, 0, 0);

        /// <summary>The plane x = 0 (the YZ plane), with unit normal +X.</summary>
        public static readonly Plane YZ = new Plane(1, 0, 0, 0);

        #endregion

        #region Accessors

        /// <summary>The A coefficient (x component of the normal).</summary>
        public double A { get { return this.a; } }

        /// <summary>The B coefficient (y component of the normal).</summary>
        public double B { get { return this.b; } }

        /// <summary>The C coefficient (z component of the normal).</summary>
        public double C { get { return this.c; } }

        /// <summary>The D coefficient (signed offset).</summary>
        public double D { get { return this.d; } }

        /// <summary>The plane's normal vector (A, B, C); not necessarily unit length.</summary>
        public Vector Normal { get { return new Vector(this.a, this.b, this.c); } }

        /// <summary>The unit normal of the plane, or the zero vector for a degenerate plane.</summary>
        public Vector UnitNormal { get { return this.Normal.NormalizeOrDefault(); } }

        /// <summary>The point on the plane closest to the origin.</summary>
        public Vector Origin { get { return this.ClosestPoint(new Vector(0, 0, 0)); } }

        #endregion

        #region Factories

        /// <summary>Construct a plane from a point on it and its normal. The stored normal is unit length.</summary>
        public static Plane FromPointNormal(Vector point, Vector normal)
        {
            Vector n = normal.NormalizeOrDefault();
            double d = -n.DotProduct(point);
            return new Plane(n, d);
        }

        /// <summary>
        /// Construct a plane through three points. The normal follows the right-hand rule for the order
        /// p1 → p2 → p3 and is unit length. Throws if the points are collinear (no unique plane).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the three points are collinear.</exception>
        public static Plane FromThreePoints(Vector p1, Vector p2, Vector p3)
        {
            Vector normal = (p2 - p1).CrossProduct(p3 - p1);
            if (normal.Magnitude == 0)
            {
                throw new ArgumentException(COLLINEAR_POINTS);
            }

            return FromPointNormal(p1, normal);
        }

        #endregion

        #region Normalize

        /// <summary>
        /// The plane scaled so its normal is unit length (the geometric plane is unchanged). Returns the
        /// plane unchanged if its normal has zero magnitude.
        /// </summary>
        public static Plane Normalize(Plane p)
        {
            double magnitude = p.Normal.Magnitude;
            if (magnitude == 0)
            {
                return p;
            }

            return new Plane(p.A / magnitude, p.B / magnitude, p.C / magnitude, p.D / magnitude);
        }

        /// <summary>The plane scaled so its normal is unit length.</summary>
        public Plane Normalize()
        {
            return Normalize(this);
        }

        #endregion

        #region Distance, side and closest point

        /// <summary>
        /// The signed distance from <paramref name="point"/> to the plane: positive on the side the
        /// normal points to, negative on the other, zero on the plane. Uses the unit normal, so the raw
        /// component scale does not matter.
        /// </summary>
        public double SignedDistanceTo(Vector point)
        {
            double magnitude = this.Normal.Magnitude;
            if (magnitude == 0)
            {
                return double.NaN;
            }

            return (this.Normal.DotProduct(point) + this.d) / magnitude;
        }

        /// <summary>The unsigned (absolute) distance from <paramref name="point"/> to the plane.</summary>
        public double DistanceTo(Vector point)
        {
            return Math.Abs(this.SignedDistanceTo(point));
        }

        /// <summary>
        /// Which side of the plane <paramref name="point"/> lies on: +1 on the normal's side, -1 on the
        /// far side, 0 when on the plane (within <paramref name="tolerance"/>).
        /// </summary>
        public int SideOf(Vector point, double tolerance)
        {
            double s = this.SignedDistanceTo(point);
            if (s > tolerance) return 1;
            if (s < -tolerance) return -1;
            return 0;
        }

        /// <summary>Whether <paramref name="point"/> lies on the plane within <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            return this.DistanceTo(point) <= tolerance;
        }

        /// <summary>The point on the plane closest to <paramref name="point"/> (its orthogonal projection).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector unit = this.UnitNormal;
            return point - (this.SignedDistanceTo(point) * unit);
        }

        /// <summary>Reflect <paramref name="point"/> through the plane to the opposite side.</summary>
        public Vector Reflect(Vector point)
        {
            Vector unit = this.UnitNormal;
            return point - (2.0 * this.SignedDistanceTo(point) * unit);
        }

        #endregion

        #region Line intersection

        /// <summary>
        /// Intersect this plane with the infinite line through <paramref name="linePoint"/> in direction
        /// <paramref name="lineDirection"/>. Returns true and the intersection point when they meet at a
        /// single point; false (with <paramref name="intersection"/> = the line point) when the line is
        /// parallel to the plane.
        /// </summary>
        public bool TryIntersectLine(Vector linePoint, Vector lineDirection, out Vector intersection)
        {
            double denominator = this.Normal.DotProduct(lineDirection);
            if (denominator.AlmostEqualsWithAbsTolerance(0, double.Epsilon) || denominator == 0)
            {
                intersection = linePoint;
                return false;
            }

            double t = -(this.Normal.DotProduct(linePoint) + this.d) / denominator;
            intersection = linePoint + (t * lineDirection);
            return true;
        }

        /// <summary>
        /// The parameter <c>t</c> along the line <c>linePoint + t·lineDirection</c> at which it crosses
        /// the plane, or <see cref="double.NaN"/> if the line is parallel to the plane.
        /// </summary>
        public double IntersectLineParameter(Vector linePoint, Vector lineDirection)
        {
            double denominator = this.Normal.DotProduct(lineDirection);
            if (denominator == 0)
            {
                return double.NaN;
            }

            return -(this.Normal.DotProduct(linePoint) + this.d) / denominator;
        }

        #endregion

        #region Predicates

        /// <summary>Whether the plane is degenerate (its normal has zero magnitude).</summary>
        public bool IsDegenerate()
        {
            return this.Normal.Magnitude == 0;
        }

        /// <summary>Whether this plane is parallel to <paramref name="other"/> within <paramref name="tolerance"/> (ignoring offset and normal direction).</summary>
        public bool IsParallelTo(Plane other, double tolerance)
        {
            return this.UnitNormal.CrossProduct(other.UnitNormal).Magnitude <= tolerance;
        }

        #endregion

        #region Operators

        /// <summary>Flip the plane's orientation (negate the normal and offset); the geometric plane is unchanged.</summary>
        public static Plane operator -(Plane p)
        {
            return new Plane(-p.A, -p.B, -p.C, -p.D);
        }

        /// <summary>Component-wise equality of the four coefficients.</summary>
        public static bool operator ==(Plane p1, Plane p2)
        {
            return p1.A == p2.A && p1.B == p2.B && p1.C == p2.C && p1.D == p2.D;
        }

        /// <summary>Component-wise inequality of the four coefficients.</summary>
        public static bool operator !=(Plane p1, Plane p2)
        {
            return !(p1 == p2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Component-wise equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Plane p && this.Equals(p);
        }

        /// <summary>Component-wise equality with another plane (by raw coefficients).</summary>
        public bool Equals(Plane other)
        {
            return this == other;
        }

        /// <summary>
        /// Whether this plane and <paramref name="other"/> describe the same geometric plane within
        /// <paramref name="tolerance"/> — i.e. the same set of points, regardless of coefficient scale
        /// or normal direction. Compares both planes in normalized form (and their negations).
        /// </summary>
        public bool Equals(Plane other, double tolerance)
        {
            Plane p = this.Normalize();
            Plane q = other.Normalize();

            bool same =
                p.A.AlmostEqualsWithAbsTolerance(q.A, tolerance) &&
                p.B.AlmostEqualsWithAbsTolerance(q.B, tolerance) &&
                p.C.AlmostEqualsWithAbsTolerance(q.C, tolerance) &&
                p.D.AlmostEqualsWithAbsTolerance(q.D, tolerance);

            bool opposite =
                p.A.AlmostEqualsWithAbsTolerance(-q.A, tolerance) &&
                p.B.AlmostEqualsWithAbsTolerance(-q.B, tolerance) &&
                p.C.AlmostEqualsWithAbsTolerance(-q.C, tolerance) &&
                p.D.AlmostEqualsWithAbsTolerance(-q.D, tolerance);

            return same || opposite;
        }

        /// <summary>A hash code derived from the four coefficients.</summary>
        public override int GetHashCode()
        {
            return this.a.GetHashCode() ^ this.b.GetHashCode() ^ this.c.GetHashCode() ^ this.d.GetHashCode();
        }

        /// <summary>Deconstruct into the four coefficients, enabling <c>var (a, b, c, d) = plane;</c>.</summary>
        public void Deconstruct(out double a, out double b, out double c, out double d)
        {
            a = this.a;
            b = this.b;
            c = this.c;
            d = this.d;
        }

        /// <summary>A string of the form <c>(A·x + B·y + C·z + D = 0)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(A·x + B·y + C·z + D = 0)</c> where each coefficient uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}x + {1}y + {2}z + {3} = 0)",
                this.a.ToString(format, formatProvider),
                this.b.ToString(format, formatProvider),
                this.c.ToString(format, formatProvider),
                this.d.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string COLLINEAR_POINTS = "The three points are collinear and do not define a unique plane.";

        #endregion
    }
}
