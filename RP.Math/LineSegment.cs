namespace RP.Math
{
    using System;

    /// <summary>
    /// A straight line between two points in 3D space: a <see cref="Tail"/> (start) and a
    /// <see cref="Head"/> (end). Unlike an infinite line, a segment has a definite length and two ends.
    /// </summary>
    /// <remarks>
    /// Immutable: the two end points are fixed once constructed; operations that "change" a segment return
    /// a new one. The maths here only needs <see cref="Vector"/> and <see cref="Angle"/>.
    /// </remarks>
    public class LineSegment
    {
        #region Constructors

        /// <summary>Construct a segment from its tail (start) and head (end) points.</summary>
        public LineSegment(Vector tail, Vector head)
        {
            Tail = tail;
            Head = head;
        }

        /// <summary>Construct a segment from the six numbers (xt, yt, zt) -> (xh, yh, zh).</summary>
        public LineSegment(double xt, double yt, double zt, double xh, double yh, double zh)
        {
            Tail = new Vector(xt, yt, zt);
            Head = new Vector(xh, yh, zh);
        }

        /// <summary>Construct a segment from an array of exactly six numbers (xt, yt, zt, xh, yh, zh).</summary>
        public LineSegment(double[] arr)
        {
            if (arr == null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length != 6) throw new ArgumentException(SIX_COMPONENTS, nameof(arr));
            Tail = new Vector(arr[0], arr[1], arr[2]);
            Head = new Vector(arr[3], arr[4], arr[5]);
        }

        #endregion

        #region Accessors

        /// <summary>The start point of the segment.</summary>
        public Vector Tail { get; }

        /// <summary>The end point of the segment.</summary>
        public Vector Head { get; }

        /// <summary>The straight-line length of the segment (the distance from tail to head).</summary>
        public double Length { get { return (Head - Tail).Magnitude; } }

        /// <summary>The point exactly halfway between the tail and the head.</summary>
        public Vector Midpoint { get { return (Tail + Head) / 2.0; } }

        /// <summary>The unit (length-1) direction pointing from the tail towards the head.</summary>
        public Vector Direction { get { return (Head - Tail).NormalizeOrDefault(); } }

        #endregion

        #region Points along the segment

        /// <summary>
        /// The point a fraction <paramref name="t"/> of the way from the tail (t = 0) to the head (t = 1).
        /// Values outside 0..1 are clamped back onto the segment.
        /// </summary>
        public Vector PointAt(double t)
        {
            double clamped = t < 0 ? 0 : (t > 1 ? 1 : t);
            return Tail + ((Head - Tail) * clamped);
        }

        #endregion

        #region Closest point and distance

        /// <summary>
        /// The point on this segment that is nearest to <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// We measure how far along the segment the point projects (a fraction of the way from tail to
        /// head), keep that fraction within the segment's ends (0..1), and return that point. A zero-length
        /// segment simply returns its tail.
        /// </remarks>
        public Vector ClosestPointTo(Vector point)
        {
            Vector tailToHead = Head - Tail;
            double lengthSquared = tailToHead.MagnitudeSquared;
            if (lengthSquared == 0)
            {
                return Tail; // the segment is a single point
            }

            double t = (point - Tail).DotProduct(tailToHead) / lengthSquared;
            return PointAt(t);
        }

        /// <summary>The shortest distance from <paramref name="point"/> to this segment.</summary>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        #endregion

        #region Bridges to other line types

        /// <summary>
        /// The infinite <see cref="Line"/> this segment lies on (same direction, passing through the tail).
        /// Useful when you want to ignore the segment's ends and treat it as endless.
        /// </summary>
        public Line ToLine()
        {
            return new Line(Tail, Head - Tail);
        }

        #endregion

        #region Producing new segments

        /// <summary>A copy of the segment with its tail and head swapped (pointing the other way).</summary>
        public LineSegment Reversed()
        {
            return new LineSegment(Head, Tail);
        }

        /// <summary>A copy of the segment translated (slid) by <paramref name="offset"/>.</summary>
        public LineSegment Translate(Vector offset)
        {
            return new LineSegment(Tail + offset, Head + offset);
        }

        /// <summary>A copy of the segment with its head rotated about its tail, around the world X axis.</summary>
        public LineSegment RotateX(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateX(angle));
        }

        /// <summary>A copy of the segment with its head rotated about its tail, around the world Y axis.</summary>
        public LineSegment RotateY(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateY(angle));
        }

        /// <summary>A copy of the segment with its head rotated about its tail, around the world Z axis.</summary>
        public LineSegment RotateZ(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateZ(angle));
        }

        #endregion

        #region Interpolate (point between the ends)

        /// <summary>The point a fraction <paramref name="control"/> of the way along the segment (0 = tail, 1 = head).</summary>
        public static Vector Interpolate(LineSegment segment, double control)
        {
            return Vector.Interpolate(segment.Tail, segment.Head, control);
        }

        /// <summary>The point a fraction <paramref name="control"/> of the way along this segment (0 = tail, 1 = head).</summary>
        public Vector Interpolate(double control)
        {
            return Interpolate(this, control);
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>tail -> head</c>.</summary>
        public override string ToString()
        {
            return string.Format("{0} -> {1}", Tail, Head);
        }

        #endregion

        #region Messages

        private const string SIX_COMPONENTS = "Array must contain exactly six components, (xt, yt, zt, xh, yh, zh).";

        #endregion
    }
}
