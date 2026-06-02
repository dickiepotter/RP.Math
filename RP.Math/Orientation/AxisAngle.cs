namespace RP.Math
{
    using System;
    using System.Globalization;

    /// <summary>
    /// An immutable rotation expressed as an <see cref="Axis"/> to turn about and an <see cref="Angle"/>
    /// to turn through — the most direct, geometric way to say "spin this much around that line".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the fifth member of the orientation family, alongside <see cref="Quaternion"/>,
    /// <see cref="Rotation"/> (Euler X/Y/Z), <see cref="Attitude"/> (yaw/pitch/roll) and the rotation
    /// <see cref="Matrix"/>. They all describe the same thing — a turn in 3D — and interconvert freely:
    /// <see cref="ToQuaternion"/> / <see cref="FromQuaternion(Quaternion)"/> are the bridge the others
    /// already use, so axis-angle slots straight in.
    /// </para>
    /// <para>
    /// The maths is Euler's rotation theorem: <i>every</i> rotation in 3D, however complicated, is
    /// equivalent to a single rotation by some angle about some fixed axis. That axis and angle are
    /// exactly what this type stores. The <see cref="Axis"/> is kept as a unit (length-1) vector so it
    /// names a pure direction; a zero axis means "no rotation" (the <see cref="Identity"/>).
    /// </para>
    /// <para>
    /// Two axis-angles can describe the same physical rotation while differing in their raw fields —
    /// turning +θ about <c>k</c> is the same as turning −θ about <c>−k</c>, and the angle wraps every
    /// full turn. Plain <see cref="Equals(AxisAngle)"/> compares the stored fields; the tolerance overload
    /// <see cref="Equals(AxisAngle, double)"/> compares the <em>rotation they represent</em>, so those
    /// equivalent forms compare equal. Depends on <see cref="Vector"/>, <see cref="Angle"/> and
    /// <see cref="Quaternion"/>.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct AxisAngle : IEquatable<AxisAngle>, IFormattable
    {
        #region Fields

        private readonly Vector axis;  // unit length, or the zero vector for the identity rotation
        private readonly Angle angle;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a rotation of <paramref name="angle"/> about <paramref name="axis"/>. The axis is
        /// normalized (scaled to length 1); a zero axis yields the identity rotation.
        /// </summary>
        public AxisAngle(Vector axis, Angle angle)
        {
            this.axis = axis.NormalizeOrDefault();
            this.angle = angle;
        }

        #endregion

        #region Constants

        /// <summary>The identity rotation: no turn (zero angle about the X axis by convention).</summary>
        public static readonly AxisAngle Identity = new AxisAngle(new Vector(1, 0, 0), new Angle(0));

        #endregion

        #region Accessors

        /// <summary>The unit axis turned about (the zero vector for the identity rotation).</summary>
        public Vector Axis { get { return this.axis; } }

        /// <summary>The angle turned through about the <see cref="Axis"/>.</summary>
        public Angle Angle { get { return this.angle; } }

        #endregion

        #region Factories

        /// <summary>The axis-angle equivalent of a <see cref="Quaternion"/>.</summary>
        /// <remarks>Maths: a unit quaternion already <i>is</i> an axis-angle in disguise —
        /// <c>(sin(θ/2)·k, cos(θ/2))</c> — so this just reads the axis and angle back out via
        /// <see cref="Quaternion.ToAxisAngle(out Vector, out Angle)"/>.</remarks>
        public static AxisAngle FromQuaternion(Quaternion q)
        {
            q.ToAxisAngle(out Vector a, out Angle ang);
            return new AxisAngle(a, ang);
        }

        /// <summary>The axis-angle equivalent of a <see cref="Rotation"/> (Euler X/Y/Z).</summary>
        public static AxisAngle FromRotation(Rotation rotation)
        {
            return FromQuaternion(rotation.ToQuaternion());
        }

        /// <summary>
        /// The axis-angle equivalent of an <see cref="Attitude"/> (yaw/pitch/roll), interpreting the yaw /
        /// pitch / roll in the given coordinate convention.
        /// </summary>
        public static AxisAngle FromAttitude(Attitude attitude, OrthogonalAxes axes)
        {
            return FromQuaternion(attitude.ToQuaternion(axes));
        }

        #endregion

        #region Conversion

        /// <summary>
        /// The equivalent unit <see cref="Quaternion"/>. Maths: <c>(sin(θ/2)·k, cos(θ/2))</c> for unit
        /// axis <c>k</c> and angle <c>θ</c> — the canonical axis-angle to quaternion formula.
        /// </summary>
        public Quaternion ToQuaternion()
        {
            return Quaternion.FromAxisAngle(this.axis, this.angle);
        }

        /// <summary>The equivalent 4x4 homogeneous rotation matrix.</summary>
        public Matrix ToMatrix()
        {
            return this.ToQuaternion().ToMatrix();
        }

        /// <summary>Express this rotation in the Euler X/Y/Z naming of <see cref="Rotation"/>.</summary>
        public Rotation ToRotation()
        {
            return Rotation.FromQuaternion(this.ToQuaternion());
        }

        #endregion

        #region Conversion operators

        // The orientation types are different encodings of the same turn, so they interconvert exactly
        // but are NOT the same value — hence these are explicit (you ask for the re-encoding), each a
        // thin alias over the To*/From* methods above. Attitude is excluded: it needs an OrthogonalAxes
        // convention, which a parameterless cast cannot supply.

        /// <summary>The equivalent unit <see cref="Quaternion"/> (see <see cref="ToQuaternion"/>).</summary>
        public static explicit operator Quaternion(AxisAngle a) { return a.ToQuaternion(); }

        /// <summary>The equivalent Euler <see cref="Rotation"/> (see <see cref="ToRotation"/>).</summary>
        public static explicit operator Rotation(AxisAngle a) { return a.ToRotation(); }

        /// <summary>The equivalent 4x4 rotation <see cref="Matrix"/> (see <see cref="ToMatrix"/>).</summary>
        public static explicit operator Matrix(AxisAngle a) { return a.ToMatrix(); }

        #endregion

        #region Tuple conversion

        // A Deconstruct(out Vector, out Angle) already lives in the Standard functions region below.

        /// <summary>
        /// Build from an (axis, angle) tuple. Explicit because, as with the constructor, the axis is
        /// normalized — a non-unit axis in the tuple does not round-trip unchanged, so the conversion
        /// is not a pure repack.
        /// </summary>
        public static explicit operator AxisAngle((Vector axis, Angle angle) value)
        {
            return new AxisAngle(value.axis, value.angle);
        }

        /// <summary>Read out the (unit axis, angle) pair.</summary>
        public static implicit operator (Vector Axis, Angle Angle)(AxisAngle a)
        {
            return (a.Axis, a.Angle);
        }

        #endregion

        #region Inverse and apply

        /// <summary>
        /// The inverse rotation: the same axis turned through the opposite angle, which undoes this one.
        /// </summary>
        public AxisAngle Inverse()
        {
            return new AxisAngle(this.axis, new Angle(-this.angle.Rad));
        }

        /// <summary>
        /// Rotate a vector by this rotation, using Rodrigues' rotation formula directly.
        /// </summary>
        /// <remarks>
        /// <para>Maths (Rodrigues' formula), for unit axis <c>k</c>, angle <c>θ</c> and vector <c>v</c>:</para>
        /// <para><c>v' = v·cos θ + (k × v)·sin θ + k·(k · v)·(1 − cos θ)</c></para>
        /// <para>
        /// The idea is to split <c>v</c> into the part along the axis (which the turn leaves untouched)
        /// and the part perpendicular to it (which spins in the plane of rotation). The first term keeps
        /// the perpendicular part's projection, the second swings it round by the cross product, and the
        /// last term restores the unchanged along-axis component. A zero axis leaves <c>v</c> unchanged.
        /// </para>
        /// </remarks>
        public Vector Rotate(Vector v)
        {
            Vector k = this.axis;
            double cos = this.angle.Cos();
            double sin = this.angle.Sin();

            return (v * cos)
                + (k.CrossProduct(v) * sin)
                + (k * (k.DotProduct(v) * (1.0 - cos)));
        }

        /// <summary>Rotate a vector by an axis-angle rotation.</summary>
        public static Vector Rotate(AxisAngle rotation, Vector v)
        {
            return rotation.Rotate(v);
        }

        #endregion

        #region Predicates

        /// <summary>Whether this represents the identity (no-op) rotation within an angular tolerance (radians).</summary>
        public bool IsIdentity(double tolerance)
        {
            return this.Equals(Identity, tolerance);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object by stored fields.</summary>
        public override bool Equals(object? other)
        {
            return other is AxisAngle a && this.Equals(a);
        }

        /// <summary>Equality with another axis-angle by stored fields (axis and angle), not by represented rotation.</summary>
        public bool Equals(AxisAngle other)
        {
            return this.axis.Equals(other.Axis) && this.angle.Equals(other.Angle);
        }

        /// <summary>
        /// Whether this and <paramref name="other"/> represent the same rotation within
        /// <paramref name="tolerance"/> (an angular tolerance in radians). This compares the rotations
        /// themselves, so the equivalent forms (±θ about ∓axis, and full-turn wraps) compare equal.
        /// </summary>
        /// <remarks>Maths: convert both to quaternions and measure the angle of the relative rotation
        /// between them (see <see cref="Quaternion.AngleTo(Quaternion)"/>); equal rotations differ by 0.</remarks>
        public bool Equals(AxisAngle other, double tolerance)
        {
            return this.ToQuaternion().AngleTo(other.ToQuaternion()).Rad <= tolerance;
        }

        /// <summary>Equality operator (by stored fields).</summary>
        public static bool operator ==(AxisAngle a1, AxisAngle a2) { return a1.Equals(a2); }

        /// <summary>Inequality operator (by stored fields).</summary>
        public static bool operator !=(AxisAngle a1, AxisAngle a2) { return !a1.Equals(a2); }

        /// <summary>A hash code derived from the axis and angle.</summary>
        public override int GetHashCode()
        {
            return this.axis.GetHashCode() ^ this.angle.GetHashCode();
        }

        /// <summary>Deconstruct into the axis and angle, enabling <c>var (axis, angle) = axisAngle;</c>.</summary>
        public void Deconstruct(out Vector axis, out Angle angle)
        {
            axis = this.axis;
            angle = this.angle;
        }

        /// <summary>A string of the form <c>angle rad about (axis)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>angle rad about (axis)</c> where the angle (radians) and axis components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "{0} rad about {1}",
                this.angle.Rad.ToString(format, formatProvider),
                this.axis.ToString(format, formatProvider));
        }

        #endregion
    }
}
