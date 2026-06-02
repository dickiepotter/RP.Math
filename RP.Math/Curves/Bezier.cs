namespace RP.Math
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An immutable Bézier curve defined by an ordered list of <em>control points</em>. The curve passes
    /// through the first and last control points and is pulled toward the interior ones without touching
    /// them. A curve with two control points is a straight line, three a quadratic, four a cubic; any
    /// higher degree is allowed too.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The curve is evaluated by <b>de Casteljau's algorithm</b>: to find the point at parameter
    /// <c>t</c> (running 0→1 from start to end), repeatedly replace each adjacent pair of points with the
    /// point a fraction <c>t</c> along the segment between them. Each pass produces one fewer point; the
    /// single point left when only one remains is the point on the curve. It is a little slower than
    /// evaluating the algebraic (Bernstein) polynomial, but it is numerically stable and — far more useful
    /// here — it makes plain <i>why</i> the curve bends the way it does: it is just repeated straight-line
    /// interpolation (<see cref="Vector.Interpolate(Vector, double)"/>) all the way down.
    /// </para>
    /// <para>
    /// Built on <see cref="Vector"/> and following the library's design: an immutable value with both
    /// static and instance forms of the main operations.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public class Bezier : IEquatable<Bezier>
    {
        #region Fields

        private readonly Vector[] controlPoints;

        #endregion

        #region Constructors

        /// <summary>Construct a Bézier curve from its ordered control points (at least two).</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="controlPoints"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if fewer than two control points are supplied.</exception>
        public Bezier(params Vector[] controlPoints)
        {
            if (controlPoints == null) throw new ArgumentNullException(nameof(controlPoints));
            if (controlPoints.Length < 2) throw new ArgumentException(TOO_FEW, nameof(controlPoints));
            this.controlPoints = (Vector[])controlPoints.Clone();
        }

        #endregion

        #region Factories

        /// <summary>A quadratic (degree-2) Bézier curve through a start, one control point, and an end.</summary>
        public static Bezier Quadratic(Vector start, Vector control, Vector end)
        {
            return new Bezier(start, control, end);
        }

        /// <summary>A cubic (degree-3) Bézier curve — the everyday case — with two interior control points.</summary>
        public static Bezier Cubic(Vector start, Vector control1, Vector control2, Vector end)
        {
            return new Bezier(start, control1, control2, end);
        }

        #endregion

        #region Accessors

        /// <summary>The number of control points.</summary>
        public int Count { get { return this.controlPoints.Length; } }

        /// <summary>The degree of the curve (one less than the number of control points).</summary>
        public int Degree { get { return this.controlPoints.Length - 1; } }

        /// <summary>The control point at <paramref name="index"/>.</summary>
        public Vector this[int index] { get { return this.controlPoints[index]; } }

        /// <summary>A copy of the control points, in order.</summary>
        public Vector[] ControlPoints { get { return (Vector[])this.controlPoints.Clone(); } }

        /// <summary>The start of the curve — the first control point, reached at <c>t = 0</c>.</summary>
        public Vector Start { get { return this.controlPoints[0]; } }

        /// <summary>The end of the curve — the last control point, reached at <c>t = 1</c>.</summary>
        public Vector End { get { return this.controlPoints[this.controlPoints.Length - 1]; } }

        #endregion

        #region Evaluation

        /// <summary>
        /// The point on the curve at parameter <paramref name="t"/> (0 at the <see cref="Start"/>, 1 at the
        /// <see cref="End"/>), evaluated by de Casteljau's algorithm.
        /// </summary>
        public Vector PointAt(double t)
        {
            return DeCasteljau(this.controlPoints, t);
        }

        /// <summary>
        /// The tangent (the curve's velocity, <c>dP/dt</c>) at parameter <paramref name="t"/>. Its direction
        /// is the way the curve is heading; its length is the speed of the parameterisation, which is not
        /// constant. Normalize it for a pure direction.
        /// </summary>
        /// <remarks>
        /// Maths: the derivative of a degree-<c>n</c> Bézier is itself a degree-<c>(n−1)</c> Bézier whose
        /// control points are <c>n·(Pᵢ₊₁ − Pᵢ)</c> — the scaled gaps between neighbouring control points —
        /// so the same de Casteljau evaluation gives the tangent.
        /// </remarks>
        public Vector Tangent(double t)
        {
            int n = this.Degree;
            if (n == 0) return Vector.Zero;

            var hodograph = new Vector[n];
            for (int i = 0; i < n; i++)
            {
                hodograph[i] = n * (this.controlPoints[i + 1] - this.controlPoints[i]);
            }

            return DeCasteljau(hodograph, t);
        }

        /// <summary>
        /// An approximation of the curve's arc length, taken as the summed lengths of
        /// <paramref name="segments"/> straight chords sampled evenly in <c>t</c>. More segments give a
        /// closer (always slightly under-) estimate, since a Bézier has no simple closed-form length.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="segments"/> is less than one.</exception>
        public double Length(int segments)
        {
            if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), segments, SEGMENTS_POSITIVE);

            double total = 0;
            Vector previous = this.PointAt(0);
            for (int i = 1; i <= segments; i++)
            {
                Vector current = this.PointAt((double)i / segments);
                total += previous.Distance(current);
                previous = current;
            }

            return total;
        }

        /// <summary>An approximation of the curve's arc length using a sensible default sample count.</summary>
        public double Length()
        {
            return this.Length(64);
        }

        /// <summary>
        /// One step of de Casteljau: collapse the control points down to the single point at parameter
        /// <paramref name="t"/> by repeated pairwise interpolation.
        /// </summary>
        private static Vector DeCasteljau(Vector[] points, double t)
        {
            // Work on a copy so the curve's own control points are never mutated.
            var working = (Vector[])points.Clone();
            int count = working.Length;

            for (int round = 1; round < count; round++)
            {
                for (int i = 0; i < count - round; i++)
                {
                    // Lerp neighbouring points; reading [i] and [i+1] before [i] is overwritten keeps this
                    // valid in place (allow extrapolation so t outside 0..1 still evaluates the polynomial).
                    working[i] = working[i].Interpolate(working[i + 1], t, allowExtrapolation: true);
                }
            }

            return working[0];
        }

        #endregion

        #region Equality and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Bezier b && this.Equals(b);
        }

        /// <summary>Equality with another Bézier curve (same control points, in order).</summary>
        public bool Equals(Bezier? other)
        {
            return other != null && this.controlPoints.SequenceEqual(other.controlPoints);
        }

        /// <summary>A hash code derived from the control points.</summary>
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (Vector p in this.controlPoints)
            {
                hash = (hash * 31) ^ p.GetHashCode();
            }

            return hash;
        }

        /// <summary>A string of the form <c>Bezier[(p0); (p1); …]</c>.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder("Bezier[");
            for (int i = 0; i < this.controlPoints.Length; i++)
            {
                if (i > 0) sb.Append("; ");
                sb.Append(this.controlPoints[i].ToString());
            }

            return sb.Append(']').ToString();
        }

        #endregion

        #region Messages

        private const string TOO_FEW = "A Bézier curve needs at least two control points (a start and an end).";
        private const string SEGMENTS_POSITIVE = "The number of segments must be at least one.";

        #endregion
    }
}
