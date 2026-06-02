namespace RP.Math
{
    using Math = System.Math;

    /// <summary>
    /// An infinite straight line in 3D space: a <see cref="Point"/> the line passes through, and a
    /// <see cref="Direction"/> it runs along.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A line is the "no ends" member of the line family — it stretches forever in <b>both</b>
    /// directions, so it has no length and no midpoint. The whole family differs only in <i>where it is
    /// allowed to stop</i>:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="Line"/> — infinite both ways; no ends (this type).</item>
    ///   <item><see cref="Ray"/> — infinite one way; one end (the origin).</item>
    ///   <item><see cref="LineSegment"/> — finite; two ends, and therefore a length.</item>
    /// </list>
    /// <para>
    /// That difference shows up in "closest point": a line never clamps (any point on it is allowed),
    /// a ray clamps behind its start, and a segment clamps at both ends. Every point on the line is
    /// <c>P(t) = Point + t · Direction</c> for <b>any</b> number <c>t</c>, positive or negative. The
    /// <see cref="Direction"/> is stored as a unit (length-1) vector so <c>t</c> is a true distance.
    /// Immutable; depends only on <see cref="Vector"/>.
    /// </para>
    /// </remarks>
    public class Line
    {
        #region Constructors

        /// <summary>
        /// Construct a line from a point it passes through and a direction it runs along. The direction is
        /// normalized (scaled to length 1).
        /// </summary>
        public Line(Vector point, Vector direction)
        {
            Point = point;
            Direction = direction.NormalizeOrDefault();
        }

        #endregion

        #region Factories

        /// <summary>
        /// The infinite line that passes through two points.
        /// </summary>
        /// <remarks>Maths: a line through <paramref name="a"/> with direction <c>b − a</c> (the
        /// displacement from the first point to the second).</remarks>
        public static Line ThroughPoints(Vector a, Vector b)
        {
            return new Line(a, b - a);
        }

        #endregion

        #region Accessors

        /// <summary>A point the line passes through (any point on the line would do; this is the stored one).</summary>
        public Vector Point { get; }

        /// <summary>The unit (length-1) direction the line runs along (it also runs the exact opposite way).</summary>
        public Vector Direction { get; }

        #endregion

        #region Points along the line

        /// <summary>
        /// The point at signed distance <paramref name="distance"/> along the line from <see cref="Point"/>.
        /// </summary>
        /// <remarks>
        /// Maths: <c>P(t) = Point + t · Direction</c>. A positive <c>t</c> goes one way and a negative
        /// <c>t</c> goes the other — both are on the line, because a line is infinite in both directions.
        /// Nothing is clamped (unlike <see cref="Ray"/> or <see cref="LineSegment"/>).
        /// </remarks>
        public Vector PointAt(double distance)
        {
            return Point + (Direction * distance);
        }

        #endregion

        #region Closest point and distance

        /// <summary>
        /// The point on the line nearest to <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: project the point onto the line's direction. The signed distance along the line is
        /// </para>
        /// <para><c>t = (point − Point) · Direction</c></para>
        /// <para>
        /// (no division, because <c>Direction</c> is unit length), and the nearest point is
        /// <c>Point + t · Direction</c>. Unlike a ray or a segment, <c>t</c> is <b>not</b> clamped —
        /// the whole, endless line is available, so the perpendicular foot is always reachable.
        /// </para>
        /// </remarks>
        public Vector ClosestPointTo(Vector point)
        {
            double along = (point - Point).DotProduct(Direction);
            return PointAt(along);
        }

        /// <summary>
        /// The shortest distance from <paramref name="point"/> to the line.
        /// </summary>
        /// <remarks>Maths: <c>|ClosestPointTo(point) − point|</c> — the length of the perpendicular from
        /// the point to the line.</remarks>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        /// <summary>Whether <paramref name="point"/> lies on the line, within <paramref name="tolerance"/>.</summary>
        /// <remarks>Maths: true when <c>DistanceTo(point) ≤ tolerance</c>. A tolerance is used because
        /// floating-point points rarely sit <i>exactly</i> on a computed line.</remarks>
        public bool Contains(Vector point, double tolerance)
        {
            return DistanceTo(point) <= tolerance;
        }

        #endregion

        #region Relationships between lines

        /// <summary>
        /// Whether this line is parallel to <paramref name="other"/>, within <paramref name="tolerance"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: two lines are parallel when their directions point the same way or exactly opposite ways.
        /// The cross product encodes the angle between two vectors through <c>|a × b| = |a| · |b| · sin θ</c>;
        /// for our unit directions that is just <c>sin θ</c>, which is zero precisely when <c>θ</c> is 0°
        /// or 180°. So we test
        /// </para>
        /// <para><c>|Direction × other.Direction| ≤ tolerance</c>.</para>
        /// </remarks>
        public bool IsParallelTo(Line other, double tolerance)
        {
            return Direction.CrossProduct(other.Direction).Magnitude <= tolerance;
        }

        /// <summary>
        /// The point on this line nearest to <paramref name="other"/> (the foot of the shortest bridge
        /// between the two lines, measured on this line).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: write the two lines as <c>P + t·u</c> (this) and <c>Q + s·v</c> (other), with unit
        /// directions <c>u</c>, <c>v</c>. The shortest connecting segment is perpendicular to <b>both</b>
        /// lines. Let <c>r = P − Q</c>, <c>b = u · v</c>, <c>d = u · r</c>, <c>e = v · r</c>. Imposing the
        /// two perpendicularity conditions and using <c>u · u = v · v = 1</c> gives
        /// </para>
        /// <para><c>t = (b·e − d) / (1 − b²)</c></para>
        /// <para>
        /// and the nearest point is <c>PointAt(t)</c>. The denominator <c>1 − b²</c> is zero exactly when
        /// the lines are parallel (<c>b = ±1</c>); there is then no unique nearest point, so this returns
        /// the line's own anchor <see cref="Point"/> as a representative.
        /// </para>
        /// </remarks>
        public Vector ClosestPointTo(Line other)
        {
            Vector u = Direction;
            Vector v = other.Direction;
            Vector r = Point - other.Point;

            double b = u.DotProduct(v);
            double denominator = 1.0 - (b * b); // u·u = v·v = 1 because the directions are unit length
            if (denominator <= double.Epsilon)
            {
                return Point; // parallel: every point is equally near, so return the anchor
            }

            double d = u.DotProduct(r);
            double e = v.DotProduct(r);
            double t = ((b * e) - d) / denominator;
            return PointAt(t);
        }

        /// <summary>
        /// The shortest distance between this line and <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: for two lines with directions <c>u</c> and <c>v</c>, the vector <c>u × v</c> is
        /// perpendicular to both, so the gap between the lines is the length of <c>r = Q − P</c> measured
        /// along that common perpendicular:
        /// </para>
        /// <para><c>distance = |r · (u × v)| / |u × v|</c></para>
        /// <para>
        /// When the lines are parallel, <c>u × v</c> is zero and this is undefined; the distance is then
        /// just the perpendicular distance from any point of the other line to this one.
        /// </para>
        /// </remarks>
        public double DistanceTo(Line other)
        {
            Vector cross = Direction.CrossProduct(other.Direction);
            double crossMagnitude = cross.Magnitude;
            if (crossMagnitude <= double.Epsilon)
            {
                return DistanceTo(other.Point); // parallel
            }

            Vector r = other.Point - Point;
            return Math.Abs(r.DotProduct(cross)) / crossMagnitude;
        }

        /// <summary>
        /// Whether this line meets <paramref name="other"/> at a single point within
        /// <paramref name="tolerance"/>, returning that point.
        /// </summary>
        /// <remarks>
        /// Maths: in 3D two lines usually pass without touching (they are skew). They cross only when the
        /// shortest distance between them is zero. This rejects parallel lines (no single crossing point),
        /// then checks whether the nearest points on the two lines coincide to within
        /// <paramref name="tolerance"/>; if so, that shared point is the intersection.
        /// </remarks>
        public bool TryIntersect(Line other, double tolerance, out Vector intersection)
        {
            if (IsParallelTo(other, tolerance))
            {
                intersection = Point;
                return false;
            }

            Vector here = ClosestPointTo(other);
            Vector there = other.ClosestPointTo(this);
            intersection = here;
            return here.Distance(there) <= tolerance;
        }

        #endregion

        #region Relationship with a plane

        /// <summary>
        /// Intersect this line with <paramref name="plane"/>. Returns true and the meeting point when they
        /// cross; false (with <paramref name="intersection"/> = this line's <see cref="Point"/>) when the
        /// line runs parallel to the plane.
        /// </summary>
        /// <remarks>The maths lives on <see cref="Plane"/>; this forwards to
        /// <see cref="Plane.TryIntersect(Line, out Vector)"/> so the relationship reads naturally from
        /// either side.</remarks>
        public bool TryIntersect(Plane plane, out Vector intersection)
        {
            return plane.TryIntersect(this, out intersection);
        }

        #endregion

        #region Conversions to other line types

        /// <summary>
        /// A <see cref="Ray"/> starting at this line's <see cref="Point"/> and travelling along its
        /// <see cref="Direction"/> — the forward half of the line.
        /// </summary>
        public Ray ToRay()
        {
            return new Ray(Point, Direction);
        }

        /// <summary>
        /// The forward half of this line as a <see cref="Ray"/> (see <see cref="ToRay"/>). Explicit
        /// because it discards the backward half — it is a narrowing of what the value represents.
        /// </summary>
        public static explicit operator Ray(Line line)
        {
            return line.ToRay();
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>point + t(direction)</c>, echoing the line's equation.</summary>
        public override string ToString()
        {
            return string.Format("{0} + t({1})", Point, Direction);
        }

        #endregion
    }
}
