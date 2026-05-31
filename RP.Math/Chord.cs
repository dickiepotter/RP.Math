namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// A chord: the straight line joining two points that both lie on a circle. It is a
    /// <see cref="LineSegment"/> that also knows the <see cref="Radius"/> of the circle it belongs to,
    /// which unlocks the circle maths around it (the angle it spans, the arc above it, and so on).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A picture helps: draw a circle, then a straight line cutting across it. That line is the chord.
    /// The two ends sit on the circle's edge. From just the chord's <see cref="LineSegment.Length"/> and
    /// the circle's <see cref="Radius"/> you can work out several things:
    /// </para>
    /// <list type="bullet">
    ///   <item><b>Central angle</b> — the angle at the centre of the circle between the two ends.</item>
    ///   <item><b>Arc length</b> — the curved distance along the circle's edge between the two ends.</item>
    ///   <item><b>Sagitta</b> — how far the arc bulges away from the chord (the "height" of the arc).</item>
    ///   <item><b>Apothem</b> — the straight distance from the circle's centre to the chord.</item>
    /// </list>
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

        /// <summary>The diameter (widest possible chord) of the circle: twice the radius.</summary>
        public double Diameter { get { return Radius * 2.0; } }

        /// <summary>
        /// The angle at the centre of the circle subtended by the chord (the angle between the lines from
        /// the centre out to each end of the chord).
        /// </summary>
        /// <remarks>
        /// Worked out from <c>2 · asin( (length / 2) / radius )</c>. A chord cannot be longer than the
        /// diameter; if the supplied length somehow exceeds it the ratio is clamped so the result stays
        /// the straight (180°) angle rather than producing a non-number.
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

        /// <summary>The curved distance along the circle's edge between the two ends of the chord.</summary>
        /// <remarks>Arc length = radius × central angle (with the angle measured in radians).</remarks>
        public double ArcLength { get { return Radius * CentralAngle.Rad; } }

        /// <summary>
        /// The straight distance from the circle's centre to the chord (also called the apothem).
        /// </summary>
        /// <remarks>Found with Pythagoras: <c>sqrt( radius² − (length / 2)² )</c>.</remarks>
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
        /// the straight chord at its middle.
        /// </summary>
        /// <remarks>Sagitta = radius − distance-from-centre.</remarks>
        public double Sagitta { get { return Radius - DistanceFromCentre; } }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>tail -> head (r=radius)</c>.</summary>
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
