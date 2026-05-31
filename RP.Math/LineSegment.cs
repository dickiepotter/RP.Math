namespace RP.Math
{
    using System;

    /// <summary>
    /// A straight line between two fixed points in 3D space: a <see cref="Tail"/> (start) and a
    /// <see cref="Head"/> (end).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A segment is the <i>finite</i> member of the line family. It has two ends, so it has a definite
    /// <see cref="Length"/> and a <see cref="Midpoint"/>, and every "closest point" question is answered
    /// <b>on the segment</b> — the answer can never fall beyond the tail or the head. Compare:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="Line"/> — infinite in both directions (no ends, no length).</item>
    ///   <item><see cref="Ray"/> — infinite in one direction (one end).</item>
    ///   <item><see cref="LineSegment"/> — finite, two ends (this type).</item>
    /// </list>
    /// <para>
    /// The maths is built on one idea: every point on the segment can be written as
    /// <c>P(t) = Tail + t · (Head − Tail)</c>, where the parameter <c>t</c> runs from 0 (the tail) to
    /// 1 (the head). Most methods are just this formula with <c>t</c> chosen or clamped appropriately.
    /// Immutable, and depends only on <see cref="Vector"/> and <see cref="Angle"/>.
    /// </para>
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

        /// <summary>Construct a segment from the six numbers (xt, yt, zt) -&gt; (xh, yh, zh).</summary>
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

        /// <summary>
        /// The straight-line length of the segment.
        /// </summary>
        /// <remarks>
        /// Maths: <c>Length = |Head − Tail|</c>. The segment, written as a vector, is the displacement
        /// from the tail to the head (<c>Head − Tail</c>); its length is the magnitude of that vector.
        /// </remarks>
        public double Length { get { return (Head - Tail).Magnitude; } }

        /// <summary>
        /// The point exactly halfway between the tail and the head.
        /// </summary>
        /// <remarks>Maths: <c>Midpoint = (Tail + Head) / 2</c> — the average of the two end points.</remarks>
        public Vector Midpoint { get { return (Tail + Head) / 2.0; } }

        /// <summary>
        /// The unit (length-1) direction pointing from the tail towards the head.
        /// </summary>
        /// <remarks>
        /// Maths: <c>Direction = (Head − Tail) / |Head − Tail|</c>. We take the tail-to-head displacement
        /// and divide by its length so the result has length 1 — a pure direction with the size removed.
        /// A zero-length segment has no direction and yields the zero vector.
        /// </remarks>
        public Vector Direction { get { return (Head - Tail).NormalizeOrDefault(); } }

        #endregion

        #region Points along the segment

        /// <summary>
        /// The point a fraction <paramref name="t"/> of the way from the tail (t = 0) to the head (t = 1).
        /// </summary>
        /// <remarks>
        /// Maths: <c>P(t) = Tail + t · (Head − Tail)</c>. At <c>t = 0</c> the term vanishes and we are at
        /// the tail; at <c>t = 1</c> we have added the whole tail-to-head displacement and are at the head.
        /// Because a segment stops at its ends, <paramref name="t"/> is clamped to the range 0..1.
        /// </remarks>
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
        /// <para>
        /// Maths: the nearest point on the <i>infinite</i> line is found by projecting the point onto the
        /// segment's direction. Writing the segment as <c>d = Head − Tail</c>, the parameter is
        /// </para>
        /// <para><c>t = ((point − Tail) · d) / (d · d)</c></para>
        /// <para>
        /// This comes from minimising the squared distance <c>|Tail + t·d − point|²</c>: differentiating
        /// and setting it to zero gives <c>(Tail + t·d − point) · d = 0</c>, which rearranges to the
        /// formula above. The dot product measures "how far along <c>d</c>" the point lies; dividing by
        /// <c>d · d</c> (the squared length) turns that into the fraction <c>t</c>. We then clamp <c>t</c>
        /// to 0..1 so the answer stays on the segment, and evaluate <c>P(t)</c>. A zero-length segment
        /// returns its tail.
        /// </para>
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

        /// <summary>
        /// The shortest distance from <paramref name="point"/> to this segment.
        /// </summary>
        /// <remarks>Maths: the distance from the point to its nearest point on the segment,
        /// <c>|ClosestPointTo(point) − point|</c>.</remarks>
        public double DistanceTo(Vector point)
        {
            return ClosestPointTo(point).Distance(point);
        }

        #endregion

        #region Bridges to other line types

        /// <summary>
        /// The infinite <see cref="Line"/> this segment lies on (same direction, passing through the tail).
        /// </summary>
        /// <remarks>
        /// Useful when you want to ignore the segment's ends and treat it as endless — for example to ask
        /// for the closest point without clamping to the tail or head. Maths: a line through <c>Tail</c>
        /// with direction <c>Head − Tail</c>.
        /// </remarks>
        public Line ToLine()
        {
            return new Line(Tail, Head - Tail);
        }

        /// <summary>
        /// A <see cref="Ray"/> from this segment's <see cref="Tail"/> towards its <see cref="Head"/>,
        /// extended forever past the head.
        /// </summary>
        public Ray ToRay()
        {
            return new Ray(Tail, Head - Tail);
        }

        #endregion

        #region Producing new segments

        /// <summary>A copy of the segment with its tail and head swapped (pointing the other way).</summary>
        public LineSegment Reversed()
        {
            return new LineSegment(Head, Tail);
        }

        /// <summary>
        /// A copy of the segment translated (slid) by <paramref name="offset"/>.
        /// </summary>
        /// <remarks>Maths: add the same offset to both ends — <c>(Tail + offset, Head + offset)</c> —
        /// so the segment keeps its length and direction and only its position changes.</remarks>
        public LineSegment Translate(Vector offset)
        {
            return new LineSegment(Tail + offset, Head + offset);
        }

        /// <summary>
        /// A copy of the segment with its head rotated about its tail, around the world X axis.
        /// </summary>
        /// <remarks>
        /// Maths: we keep the tail fixed, rotate the tail-to-head displacement <c>(Head − Tail)</c> by the
        /// angle, then re-attach it to the tail: <c>new Head = Tail + rotate(Head − Tail)</c>. Rotating the
        /// displacement rather than the head itself is what makes the tail the pivot.
        /// </remarks>
        public LineSegment RotateX(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateX(angle));
        }

        /// <summary>A copy of the segment with its head rotated about its tail, around the world Y axis.</summary>
        /// <remarks>See <see cref="RotateX(Angle)"/> for the method; this rotates about Y instead.</remarks>
        public LineSegment RotateY(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateY(angle));
        }

        /// <summary>A copy of the segment with its head rotated about its tail, around the world Z axis.</summary>
        /// <remarks>See <see cref="RotateX(Angle)"/> for the method; this rotates about Z instead.</remarks>
        public LineSegment RotateZ(Angle angle)
        {
            return new LineSegment(Tail, Tail + (Head - Tail).RotateZ(angle));
        }

        #endregion

        #region Interpolate (point between the ends)

        /// <summary>The point a fraction <paramref name="control"/> of the way along the segment (0 = tail, 1 = head).</summary>
        /// <remarks>Maths: the same straight-line blend as <see cref="PointAt(double)"/>,
        /// <c>Tail · (1 − control) + Head · control</c>, delegated to <see cref="Vector.Interpolate(Vector, Vector, double)"/>.</remarks>
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

        /// <summary>A string of the form <c>tail -&gt; head</c>.</summary>
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
