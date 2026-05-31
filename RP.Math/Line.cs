namespace RP.Math
{
    using System;

    /// <summary>
    /// An infinite straight line in 3D space: a <see cref="Point"/> that the line passes through, and a
    /// <see cref="Direction"/> it runs along. Unlike a <see cref="LineSegment"/> (which has two ends and a
    /// length) or a <see cref="Ray"/> (which has one end), a line stretches forever in <b>both</b>
    /// directions — it has no ends, no length and no midpoint.
    /// </summary>
    /// <remarks>
    /// A point on the line is <c>Point + Direction * t</c> for <b>any</b> number <c>t</c> (positive or
    /// negative). The direction is stored as a unit (length-1) vector, so <c>t</c> is a true distance.
    /// Immutable; needs only <see cref="Vector"/>.
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

        /// <summary>The infinite line that passes through two points.</summary>
        public static Line ThroughPoints(Vector a, Vector b)
        {
            return new Line(a, b - a);
        }

        #endregion

        #region Accessors

        /// <summary>A point the line passes through (any point on the line will do; this is the stored one).</summary>
        public Vector Point { get; }

        /// <summary>The unit (length-1) direction the line runs along (it also runs the opposite way).</summary>
        public Vector Direction { get; }

        #endregion

        #region Points along the line

        /// <summary>
        /// The point at signed distance <paramref name="distance"/> along the line from <see cref="Point"/>.
        /// Positive goes one way, negative the other — both are on the line, because a line is infinite.
        /// </summary>
        public Vector PointAt(double distance)
        {
            return Point + (Direction * distance);
        }

        #endregion

        #region Closest point and distance

        /// <summary>The point on the line nearest to <paramref name="point"/>.</summary>
        /// <remarks>
        /// We measure how far along the direction the point lies (its projection onto the line) and step
        /// that far from the stored point. Unlike a segment, nothing is clamped — the whole line is allowed.
        /// </remarks>
        public Vector ClosestPointTo(Vector point)
        {
            double along = (point - Point).DotProduct(Direction);
            return PointAt(along);
        }

        /// <summary>The shortest distance from <paramref name="point"/> to the line.</summary>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        /// <summary>Whether <paramref name="point"/> lies on the line, within <paramref name="tolerance"/>.</summary>
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
        /// Two lines are parallel when they point the same way (or exactly opposite ways). The cross
        /// product of two parallel directions is the zero vector, so we test that its length is ~0.
        /// </remarks>
        public bool IsParallelTo(Line other, double tolerance)
        {
            return Direction.CrossProduct(other.Direction).Magnitude <= tolerance;
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>point + t(direction)</c>.</summary>
        public override string ToString()
        {
            return string.Format("{0} + t({1})", Point, Direction);
        }

        #endregion
    }
}
