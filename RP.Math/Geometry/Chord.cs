namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// A chord: the straight line joining two points that both lie on a circle. It is a
    /// <see cref="LineSegment"/> that also knows the <see cref="Radius"/> of the circle it belongs to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Picture a circle with a straight line drawn across it; both ends of that line touch the circle's
    /// edge. That line is a chord. On its own it is just a line segment — but once you also know the
    /// circle's radius, a surprising amount of circle geometry follows from the chord's
    /// <see cref="LineSegment.Length"/> alone. That extra knowledge is the whole reason this type exists
    /// separately from a plain <see cref="LineSegment"/>.
    /// </para>
    /// <para>The quantities it derives, with <c>c</c> = chord length and <c>r</c> = radius:</para>
    /// <list type="bullet">
    ///   <item><see cref="CentralAngle"/> — the angle the chord subtends at the circle's centre.</item>
    ///   <item><see cref="ArcLength"/> — the curved distance along the edge between the two ends.</item>
    ///   <item><see cref="DistanceFromCentre"/> — the straight distance from the centre to the chord (the apothem).</item>
    ///   <item><see cref="Sagitta"/> — how far the arc bulges out beyond the chord (the arc's height).</item>
    /// </list>
    /// <para>
    /// All four come from a single right-angled triangle: drop a perpendicular from the circle's centre
    /// to the chord. It meets the chord at its midpoint, splitting things into a right triangle whose
    /// hypotenuse is the radius <c>r</c>, one leg is the half-chord <c>c/2</c>, and the other leg is the
    /// distance from the centre to the chord.
    /// </para>
    /// </remarks>
    public class Chord : LineSegment
    {
        #region Constructors

        /// <summary>Construct a chord from its two end points and the radius of the circle they lie on.</summary>
        public Chord(Vector tail, Vector head, double radius)
            : base(tail, head)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            Radius = radius;
        }

        /// <summary>Construct a chord from the six end-point numbers and the circle radius.</summary>
        public Chord(double xt, double yt, double zt, double xh, double yh, double zh, double radius)
            : base(xt, yt, zt, xh, yh, zh)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            Radius = radius;
        }

        /// <summary>Construct a chord from an array of six end-point numbers and the circle radius.</summary>
        public Chord(double[] arr, double radius)
            : base(arr)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            Radius = radius;
        }

        #endregion

        #region Accessors

        /// <summary>The radius of the circle this chord belongs to.</summary>
        public double Radius { get; }

        /// <summary>The diameter (the longest possible chord) of the circle: twice the radius.</summary>
        public double Diameter { get { return Radius * 2.0; } }

        /// <summary>
        /// The angle at the centre of the circle subtended by the chord — the angle between the two radii
        /// drawn to the chord's ends.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maths: <c>θ = 2 · asin( (c / 2) / r )</c>, with <c>c</c> = chord length and <c>r</c> = radius.
        /// </para>
        /// <para>
        /// Why: in the right triangle formed by the centre, the chord's midpoint and one end, the
        /// half-chord <c>c/2</c> is the side opposite half the central angle, and the radius <c>r</c> is
        /// the hypotenuse. So <c>sin(θ/2) = (c/2) / r</c>, giving <c>θ/2 = asin((c/2)/r)</c> and hence
        /// <c>θ = 2·asin((c/2)/r)</c>. The ratio is clamped to the valid range −1..1 so a chord that is
        /// (within rounding) the full diameter resolves to a straight 180° angle rather than producing a
        /// non-number.
        /// </para>
        /// </remarks>
        public Angle CentralAngle
        {
            get
            {
                if (Radius == 0) return new Angle(0);
                double ratio = (Length / 2.0) / Radius;
                ratio = ratio > 1 ? 1 : (ratio < -1 ? -1 : ratio);
                return new Angle(2.0 * Math.Asin(ratio));
            }
        }

        /// <summary>
        /// The curved distance along the circle's edge between the two ends of the chord.
        /// </summary>
        /// <remarks>
        /// Maths: <c>arc = r · θ</c>, where <c>θ</c> is the <see cref="CentralAngle"/> in radians. This is
        /// the definition of arc length: a full turn (<c>θ = 2π</c>) gives the whole circumference
        /// <c>2πr</c>, and any fraction of a turn gives that same fraction of the circumference.
        /// </remarks>
        public double ArcLength { get { return Radius * CentralAngle.Rad; } }

        /// <summary>
        /// The straight distance from the circle's centre to the chord (also called the apothem).
        /// </summary>
        /// <remarks>
        /// Maths: <c>d = sqrt( r² − (c / 2)² )</c>. This is Pythagoras on the same right triangle: the
        /// radius <c>r</c> is the hypotenuse and the half-chord <c>c/2</c> is one leg, so the remaining
        /// leg — the centre-to-chord distance — is <c>sqrt(r² − (c/2)²)</c>. Guarded so a (rounding-error)
        /// negative under the root returns 0 rather than a non-number.
        /// </remarks>
        public double DistanceFromCentre
        {
            get
            {
                double half = Length / 2.0;
                double inside = (Radius * Radius) - (half * half);
                return inside <= 0 ? 0 : Math.Sqrt(inside);
            }
        }

        /// <summary>
        /// The height of the arc above the chord (the sagitta) — how far the curved edge bulges away from
        /// the straight chord, measured at the chord's midpoint.
        /// </summary>
        /// <remarks>
        /// Maths: <c>sagitta = r − d</c>, where <c>d</c> is the <see cref="DistanceFromCentre"/>. Going
        /// from the centre out to the arc is a full radius <c>r</c>; the chord sits <c>d</c> of the way
        /// out; so the gap left between the chord and the arc is <c>r − d</c>. ("Sagitta" is Latin for
        /// arrow — the chord is the bow, this height is the drawn arrow.)
        /// </remarks>
        public double Sagitta { get { return Radius - DistanceFromCentre; } }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>tail -&gt; head (r=radius)</c>.</summary>
        public override string ToString()
        {
            return string.Format("{0} -> {1} (r={2})", Tail, Head, Radius);
        }

        #endregion

        #region Messages

        private const string NEGATIVE_RADIUS = "A circle radius cannot be negative.";

        #endregion
    }
}
