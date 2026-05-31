namespace RP.Math
{
    using System;

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
