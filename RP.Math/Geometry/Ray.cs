namespace RP.Math
{
    /// <summary>
    /// A ray: a half-line that starts at an <see cref="Origin"/> point and travels forever in one
    /// <see cref="Direction"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A ray is the "one end" member of the line family — think of a beam of light or a line of sight: it
    /// has a definite start but no end. It sits between the two other members:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="Line"/> — infinite in <b>both</b> directions (no start, no end).</item>
    ///   <item><see cref="Ray"/> — infinite in <b>one</b> direction; has a start but no end (this type).</item>
    ///   <item><see cref="LineSegment"/> — finite; has both a start and an end.</item>
    /// </list>
    /// <para>
    /// Every point on the ray is <c>P(s) = Origin + s · Direction</c> for a distance <c>s ≥ 0</c>
    /// (negative <c>s</c> would be behind the start, which is not part of the ray). The
    /// <see cref="Direction"/> is stored as a unit (length-1) vector on purpose: it makes <c>s</c> a real
    /// distance and lets the closest-point maths use a plain dot product with no division. Immutable;
    /// depends only on <see cref="Vector"/>.
    /// </para>
    /// </remarks>
    public class Ray
    {
        #region Constructors

        /// <summary>
        /// Construct a ray from its start point and a direction.
        /// </summary>
        /// <remarks>
        /// The direction is normalized (scaled to length 1) so that the distance parameter used elsewhere
        /// is a true distance and the projection maths stays simple. A zero direction yields the zero
        /// vector (a degenerate ray that stays at its origin).
        /// </remarks>
        public Ray(Vector origin, Vector direction)
        {
            Origin = origin;
            Direction = direction.NormalizeOrDefault();
        }

        #endregion

        #region Accessors

        /// <summary>The point the ray starts from.</summary>
        public Vector Origin { get; }

        /// <summary>The unit (length-1) direction the ray travels in.</summary>
        public Vector Direction { get; }

        #endregion

        #region Points along the ray

        /// <summary>
        /// The point at distance <paramref name="distance"/> along the ray from its origin.
        /// </summary>
        /// <remarks>
        /// Maths: <c>P(s) = Origin + s · Direction</c>. Because <see cref="Direction"/> has length 1,
        /// <c>s</c> is the true distance travelled. A ray only goes forwards, so a negative
        /// <paramref name="distance"/> is clamped to 0 (the origin).
        /// </remarks>
        public Vector PointAt(double distance)
        {
            double forward = distance < 0 ? 0 : distance;
            return Origin + (Direction * forward);
        }

        #endregion

        #region Closest point and distance

        /// <summary>
        /// The point on the ray nearest to <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: project the point onto the ray's direction. The distance along the ray is
        /// </para>
        /// <para><c>s = (point − Origin) · Direction</c></para>
        /// <para>
        /// The dot product gives the length of <c>(point − Origin)</c> measured along <c>Direction</c>;
        /// no division is needed because <c>Direction</c> is already unit length. If <c>s</c> is negative
        /// the nearest reachable point is the origin itself (the ray does not extend backwards), which is
        /// handled by the clamp inside <see cref="PointAt(double)"/>.
        /// </para>
        /// </remarks>
        public Vector ClosestPointTo(Vector point)
        {
            double along = (point - Origin).DotProduct(Direction);
            return PointAt(along);
        }

        /// <summary>The shortest distance from <paramref name="point"/> to the ray.</summary>
        /// <remarks>Maths: <c>|ClosestPointTo(point) − point|</c>.</remarks>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        #endregion

        #region Conversions to other line types

        /// <summary>The infinite <see cref="Line"/> this ray lies on (same origin and direction, but extended both ways).</summary>
        public Line ToLine()
        {
            return new Line(Origin, Direction);
        }

        /// <summary>
        /// The infinite <see cref="Line"/> this ray lies on (see <see cref="ToLine"/>). Explicit because
        /// it widens the ray to both directions — the result no longer knows where the ray stopped.
        /// </summary>
        public static explicit operator Line(Ray ray)
        {
            return ray.ToLine();
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>origin -&gt; (direction)</c>.</summary>
        public override string ToString()
        {
            return string.Format("{0} -> ({1})", Origin, Direction);
        }

        #endregion
    }
}
