namespace RP.Math
{
    using System;

    /// <summary>
    /// A ray: a half-line that starts at an <see cref="Origin"/> point and travels forever in one
    /// <see cref="Direction"/>. Think of a beam of light, or a line of sight — it has a start but no end.
    /// </summary>
    /// <remarks>
    /// A point on the ray is <c>Origin + Direction * t</c> for any distance <c>t</c> greater than or
    /// equal to zero. The direction is stored as a unit (length-1) vector, so <c>t</c> is a real distance.
    /// Immutable; needs only <see cref="Vector"/>.
    /// </remarks>
    public class Ray
    {
        #region Constructors

        /// <summary>
        /// Construct a ray from its start point and a direction. The direction is normalized (scaled to
        /// length 1) so that distances along the ray are measured truly.
        /// </summary>
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
        /// The point at distance <paramref name="distance"/> along the ray from its origin. Negative
        /// distances are behind the origin and are clamped to the origin (a ray only goes forwards).
        /// </summary>
        public Vector PointAt(double distance)
        {
            double forward = distance < 0 ? 0 : distance;
            return Origin + (Direction * forward);
        }

        #endregion

        #region Closest point and distance

        /// <summary>The point on the ray nearest to <paramref name="point"/>.</summary>
        /// <remarks>
        /// We measure how far along the direction the point lies (its projection). If that is behind the
        /// origin we return the origin, since the ray does not extend backwards.
        /// </remarks>
        public Vector ClosestPointTo(Vector point)
        {
            double along = (point - Origin).DotProduct(Direction);
            return PointAt(along);
        }

        /// <summary>The shortest distance from <paramref name="point"/> to the ray.</summary>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>origin -> (direction)</c>.</summary>
        public override string ToString()
        {
            return string.Format("{0} -> ({1})", Origin, Direction);
        }

        #endregion
    }
}
