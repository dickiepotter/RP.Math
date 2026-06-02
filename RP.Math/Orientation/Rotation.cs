namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An immutable Euler rotation expressed as three angles about the X, Y and Z axes.
    /// </summary>
    /// <remarks>
    /// This is the human-friendly "front door" for orientation. The mathematically robust form is a
    /// <see cref="Quaternion"/>; <see cref="ToQuaternion"/> / <see cref="FromQuaternion"/> bridge the two,
    /// and types that store orientation (e.g. <see cref="Pose"/>) keep a quaternion internally while
    /// accepting a <see cref="Rotation"/>.
    /// <para>
    /// Convention: the component rotations are applied in the order X, then Y, then Z, about the world
    /// axes — equivalently the rotation matrix <c>Rz * Ry * Rx</c>. Component arithmetic (<c>+</c>,
    /// <c>-</c>) is <em>component-wise on the angles</em> (useful for nudging a rotation); it is not the
    /// same as composing two rotations — for true composition convert to <see cref="Quaternion"/> and
    /// multiply.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Rotation : IEquatable<Rotation>, IFormattable
    {
        #region Fields

        private readonly Angle x;
        private readonly Angle y;
        private readonly Angle z;

        #endregion

        #region Constructors

        /// <summary>Construct a rotation from angles about the X, Y and Z axes.</summary>
        public Rotation(Angle x, Angle y, Angle z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Construct a rotation that applies the same angle about the selected axes only.
        /// </summary>
        public Rotation(Angle angle, bool x, bool y, bool z)
        {
            this.x = x ? angle : new Angle(0);
            this.y = y ? angle : new Angle(0);
            this.z = z ? angle : new Angle(0);
        }

        #endregion

        #region Constants

        /// <summary>The identity rotation (no rotation about any axis).</summary>
        public static readonly Rotation Zero = new Rotation(new Angle(0), new Angle(0), new Angle(0));

        #endregion

        #region Accessors

        /// <summary>The rotation about the X axis.</summary>
        public Angle X { get { return this.x; } }

        /// <summary>The rotation about the Y axis.</summary>
        public Angle Y { get { return this.y; } }

        /// <summary>The rotation about the Z axis.</summary>
        public Angle Z { get { return this.z; } }

        #endregion

        #region Factories

        /// <summary>A rotation about the X axis only.</summary>
        public static Rotation AboutX(Angle angle) { return new Rotation(angle, new Angle(0), new Angle(0)); }

        /// <summary>A rotation about the Y axis only.</summary>
        public static Rotation AboutY(Angle angle) { return new Rotation(new Angle(0), angle, new Angle(0)); }

        /// <summary>A rotation about the Z axis only.</summary>
        public static Rotation AboutZ(Angle angle) { return new Rotation(new Angle(0), new Angle(0), angle); }

        #endregion

        #region Operators

        /// <summary>Component-wise addition of the angles of two rotations (not rotation composition).</summary>
        public static Rotation operator +(Rotation r1, Rotation r2)
        {
            return new Rotation(r1.X + r2.X, r1.Y + r2.Y, r1.Z + r2.Z);
        }

        /// <summary>Component-wise subtraction of the angles of two rotations.</summary>
        public static Rotation operator -(Rotation r1, Rotation r2)
        {
            return new Rotation(r1.X - r2.X, r1.Y - r2.Y, r1.Z - r2.Z);
        }

        /// <summary>Negate each component angle. This is the inverse Euler rotation only for single-axis rotations.</summary>
        public static Rotation operator -(Rotation r1)
        {
            return new Rotation(-r1.X, -r1.Y, -r1.Z);
        }

        /// <summary>Component-wise equality of the three angles.</summary>
        public static bool operator ==(Rotation r1, Rotation r2)
        {
            return r1.X == r2.X && r1.Y == r2.Y && r1.Z == r2.Z;
        }

        /// <summary>Component-wise inequality of the three angles.</summary>
        public static bool operator !=(Rotation r1, Rotation r2)
        {
            return !(r1 == r2);
        }

        #endregion

        #region Conversion

        /// <summary>
        /// The equivalent unit <see cref="Quaternion"/>. The component rotations are composed in the
        /// order X, then Y, then Z (matrix <c>Rz * Ry * Rx</c>).
        /// </summary>
        public Quaternion ToQuaternion()
        {
            Quaternion qx = Quaternion.FromAxisAngle(new Vector(1, 0, 0), this.x);
            Quaternion qy = Quaternion.FromAxisAngle(new Vector(0, 1, 0), this.y);
            Quaternion qz = Quaternion.FromAxisAngle(new Vector(0, 0, 1), this.z);
            return qz * qy * qx;
        }

        /// <summary>The equivalent 4x4 homogeneous rotation matrix.</summary>
        public Matrix ToMatrix()
        {
            return this.ToQuaternion().ToMatrix();
        }

        /// <summary>
        /// Extract the X, Y, Z Euler angles from a quaternion, consistent with <see cref="ToQuaternion"/>
        /// (the <c>Rz * Ry * Rx</c> convention). At the gimbal-lock poles (Y = ±90°) the X rotation is
        /// folded into Z.
        /// </summary>
        /// <remarks>
        /// Uses the standard closed-form quaternion→Euler conversion for the body Z-Y-X sequence
        /// (equivalently the matrix <c>Rz * Ry * Rx</c>), so it inverts <see cref="ToQuaternion"/> exactly.
        /// </remarks>
        public static Rotation FromQuaternion(Quaternion q)
        {
            Quaternion u = q.NormalizeOrDefault();
            double qx = u.X, qy = u.Y, qz = u.Z, qw = u.W;

            // Y (pitch about the Y axis): asin(2(w*y - z*x)), clamped for safety.
            double sinY = 2.0 * ((qw * qy) - (qz * qx));
            sinY = Math.Max(-1.0, Math.Min(1.0, sinY));
            double yAngle = Math.Asin(sinY);

            double xAngle, zAngle;
            if (Math.Abs(sinY) < 1.0 - 1e-9)
            {
                // X (roll about X): atan2(2(w*x + y*z), 1 - 2(x^2 + y^2))
                xAngle = Math.Atan2(2.0 * ((qw * qx) + (qy * qz)), 1.0 - (2.0 * ((qx * qx) + (qy * qy))));

                // Z (yaw about Z): atan2(2(w*z + x*y), 1 - 2(y^2 + z^2))
                zAngle = Math.Atan2(2.0 * ((qw * qz) + (qx * qy)), 1.0 - (2.0 * ((qy * qy) + (qz * qz))));
            }
            else
            {
                // Gimbal lock: fold the X rotation into Z.
                xAngle = 0;
                zAngle = 2.0 * Math.Atan2(qz, qw) * Math.Sign(sinY);
            }

            return new Rotation(new Angle(xAngle), new Angle(yAngle), new Angle(zAngle));
        }

        /// <summary>The inverse rotation (applies the opposite rotation), via the quaternion form.</summary>
        public Rotation Inverse()
        {
            return FromQuaternion(this.ToQuaternion().Conjugate());
        }

        #endregion

        #region Conversion operators

        // Re-encodings into the other orientation types. Explicit (a representation change, not the same
        // value) and lossless; each is a thin alias over an existing conversion method.

        /// <summary>The equivalent unit <see cref="Quaternion"/> (see <see cref="ToQuaternion"/>).</summary>
        public static explicit operator Quaternion(Rotation r) { return r.ToQuaternion(); }

        /// <summary>The equivalent <see cref="AxisAngle"/> (see <see cref="AxisAngle.FromRotation"/>).</summary>
        public static explicit operator AxisAngle(Rotation r) { return AxisAngle.FromRotation(r); }

        /// <summary>The equivalent 4x4 rotation <see cref="Matrix"/> (see <see cref="ToMatrix"/>).</summary>
        public static explicit operator Matrix(Rotation r) { return r.ToMatrix(); }

        #endregion

        #region Apply

        /// <summary>Rotate a vector by this rotation.</summary>
        public Vector Rotate(Vector v)
        {
            return this.ToQuaternion().Rotate(v);
        }

        /// <summary>Rotate a vector by a rotation.</summary>
        public static Vector Rotate(Rotation r, Vector v)
        {
            return r.Rotate(v);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Component-wise equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Rotation r && this.Equals(r);
        }

        /// <summary>Component-wise equality with another rotation.</summary>
        public bool Equals(Rotation other)
        {
            return this == other;
        }

        /// <summary>Component-wise equality with another rotation within an absolute angular tolerance (radians).</summary>
        public bool Equals(Rotation other, double toleranceRadians)
        {
            return this.X.Rad.AlmostEqualsWithAbsTolerance(other.X.Rad, toleranceRadians)
                && this.Y.Rad.AlmostEqualsWithAbsTolerance(other.Y.Rad, toleranceRadians)
                && this.Z.Rad.AlmostEqualsWithAbsTolerance(other.Z.Rad, toleranceRadians);
        }

        /// <summary>A hash code derived from the three angles.</summary>
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
        }

        /// <summary>Deconstruct into the three component angles, enabling <c>var (x, y, z) = rotation;</c>.</summary>
        public void Deconstruct(out Angle x, out Angle y, out Angle z)
        {
            x = this.x;
            y = this.y;
            z = this.z;
        }

        /// <summary>A string of the form <c>(x, y, z)</c> in radians.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(x, y, z)</c> where each angle (in radians) uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2})",
                this.x.Rad.ToString(format, formatProvider),
                this.y.Rad.ToString(format, formatProvider),
                this.z.Rad.ToString(format, formatProvider));
        }

        #endregion
    }

    /// <summary>
    /// An immutable orientation in the navigation naming of yaw, pitch and roll.
    /// </summary>
    /// <remarks>
    /// A companion to <see cref="Rotation"/> using aviation terms. Yaw, pitch and roll are only meaningful
    /// once a coordinate convention says which way is up, right and forward, so turning an attitude into a
    /// concrete rotation requires an <see cref="OrthogonalAxes"/> (see <see cref="ToQuaternion(OrthogonalAxes)"/>):
    /// yaw is about <see cref="OrthogonalAxes.Up"/>, pitch about <see cref="OrthogonalAxes.Right"/>, roll about
    /// <see cref="OrthogonalAxes.Forward"/>. Like <see cref="Rotation"/>, the robust form is a <see cref="Quaternion"/>.
    /// </remarks>
    [Serializable]
    public struct Attitude : IEquatable<Attitude>, IFormattable
    {
        #region Fields

        private readonly Angle yaw;
        private readonly Angle pitch;
        private readonly Angle roll;

        #endregion

        #region Constructors

        /// <summary>Construct an attitude from yaw, pitch and roll angles.</summary>
        public Attitude(Angle yaw, Angle pitch, Angle roll)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
        }

        /// <summary>Construct an attitude that applies the same angle to the selected components only.</summary>
        public Attitude(Angle angle, bool yaw, bool pitch, bool roll)
        {
            this.yaw = yaw ? angle : new Angle(0);
            this.pitch = pitch ? angle : new Angle(0);
            this.roll = roll ? angle : new Angle(0);
        }

        #endregion

        #region Constants

        /// <summary>The identity attitude (level, facing forward).</summary>
        public static readonly Attitude Zero = new Attitude(new Angle(0), new Angle(0), new Angle(0));

        #endregion

        #region Accessors

        /// <summary>The yaw (heading) angle — rotation about the convention's <see cref="OrthogonalAxes.Up"/>.</summary>
        public Angle Yaw { get { return this.yaw; } }

        /// <summary>The pitch (elevation) angle — rotation about the convention's <see cref="OrthogonalAxes.Right"/>.</summary>
        public Angle Pitch { get { return this.pitch; } }

        /// <summary>The roll (bank) angle — rotation about the convention's <see cref="OrthogonalAxes.Forward"/>.</summary>
        public Angle Roll { get { return this.roll; } }

        #endregion

        #region Factories

        /// <summary>An attitude with yaw only.</summary>
        public static Attitude FromYaw(Angle angle) { return new Attitude(angle, new Angle(0), new Angle(0)); }

        /// <summary>An attitude with pitch only.</summary>
        public static Attitude FromPitch(Angle angle) { return new Attitude(new Angle(0), angle, new Angle(0)); }

        /// <summary>An attitude with roll only.</summary>
        public static Attitude FromRoll(Angle angle) { return new Attitude(new Angle(0), new Angle(0), angle); }

        #endregion

        #region Conversion

        /// <summary>
        /// The equivalent unit <see cref="Quaternion"/> with the yaw / pitch / roll angles interpreted in
        /// the given coordinate convention: yaw about <see cref="OrthogonalAxes.Up"/>, pitch about
        /// <see cref="OrthogonalAxes.Right"/>, roll about <see cref="OrthogonalAxes.Forward"/>.
        /// </summary>
        public Quaternion ToQuaternion(OrthogonalAxes axes)
        {
            return Quaternion.FromYawPitchRoll(this.yaw, this.pitch, this.roll, axes);
        }

        /// <summary>Rotate a vector by this attitude, interpreting yaw / pitch / roll in the given convention.</summary>
        public Vector Rotate(Vector v, OrthogonalAxes axes)
        {
            return this.ToQuaternion(axes).Rotate(v);
        }

        /// <summary>
        /// Recover the yaw / pitch / roll of a rotation, read in the given coordinate convention. This is
        /// the inverse of <see cref="ToQuaternion(OrthogonalAxes)"/>: yaw is about <see cref="OrthogonalAxes.Up"/>,
        /// pitch about <see cref="OrthogonalAxes.Right"/>, roll about <see cref="OrthogonalAxes.Forward"/>.
        /// At the pitch singularity (±90°) the roll is folded into the yaw.
        /// </summary>
        /// <remarks>
        /// The three rotation axes are orthonormal, so changing into the convention's own basis
        /// (<c>P = [Right Up Forward]</c>) turns the Up–Right–Forward sequence into the standard
        /// <c>R_y(A)·R_x(B)·R_z(C)</c>, which has a known closed-form extraction. The basis sign
        /// <c>s = ±1</c> (negative for a right-handed convention) carries the rotation sense back out, so
        /// the result follows that convention's handedness — matching <see cref="ToQuaternion(OrthogonalAxes)"/>.
        /// </remarks>
        public static Attitude FromQuaternion(Quaternion q, OrthogonalAxes axes)
        {
            Quaternion qn = q.NormalizeOrDefault();
            Vector right = axes.Right, up = axes.Up, fwd = axes.Forward;

            // s = sign of the basis triple product: negative ⇒ right-handed convention.
            double s = right.CrossProduct(up).DotProduct(fwd) < 0 ? -1.0 : 1.0;

            // Columns of M (the rotation matrix) in the convention's basis: M(basis) = q.Rotate(basis).
            Vector mRight = qn.Rotate(right);
            Vector mUp = qn.Rotate(up);
            Vector mFwd = qn.Rotate(fwd);

            // N = P^T M P entries, where N = R_y(A)·R_x(B)·R_z(C) with A = s·yaw, B = s·pitch, C = s·roll.
            double sinB = Math.Max(-1.0, Math.Min(1.0, -up.DotProduct(mFwd))); // sin B = -N12
            double bAngle = Math.Asin(sinB);

            double aAngle, cAngle;
            if (Math.Abs(sinB) < 1.0 - 1e-9)
            {
                aAngle = Math.Atan2(right.DotProduct(mFwd), fwd.DotProduct(mFwd)); // atan2(N02, N22)
                cAngle = Math.Atan2(up.DotProduct(mRight), up.DotProduct(mUp));    // atan2(N10, N11)
            }
            else
            {
                // Pitch ±90°: fold the roll into the yaw.
                double n00 = right.DotProduct(mRight);
                double n01 = right.DotProduct(mUp);
                aAngle = sinB > 0 ? Math.Atan2(n01, n00) : Math.Atan2(-n01, n00);
                cAngle = 0.0;
            }

            return new Attitude(new Angle(s * aAngle), new Angle(s * bAngle), new Angle(s * cAngle));
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Component-wise equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Attitude a && this.Equals(a);
        }

        /// <summary>Component-wise equality with another attitude.</summary>
        public bool Equals(Attitude other)
        {
            return this.yaw == other.Yaw && this.pitch == other.Pitch && this.roll == other.Roll;
        }

        /// <summary>Component-wise equality within an absolute angular tolerance (radians).</summary>
        public bool Equals(Attitude other, double toleranceRadians)
        {
            return this.yaw.Rad.AlmostEqualsWithAbsTolerance(other.Yaw.Rad, toleranceRadians)
                && this.pitch.Rad.AlmostEqualsWithAbsTolerance(other.Pitch.Rad, toleranceRadians)
                && this.roll.Rad.AlmostEqualsWithAbsTolerance(other.Roll.Rad, toleranceRadians);
        }

        /// <summary>A hash code derived from the three angles.</summary>
        public override int GetHashCode()
        {
            return this.yaw.GetHashCode() ^ this.pitch.GetHashCode() ^ this.roll.GetHashCode();
        }

        /// <summary>Component-wise equality operator.</summary>
        public static bool operator ==(Attitude a1, Attitude a2) { return a1.Equals(a2); }

        /// <summary>Component-wise inequality operator.</summary>
        public static bool operator !=(Attitude a1, Attitude a2) { return !a1.Equals(a2); }

        /// <summary>Deconstruct into yaw, pitch and roll.</summary>
        public void Deconstruct(out Angle yaw, out Angle pitch, out Angle roll)
        {
            yaw = this.yaw;
            pitch = this.pitch;
            roll = this.roll;
        }

        /// <summary>A string of the form <c>(yaw, pitch, roll)</c> in radians.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(yaw, pitch, roll)</c> where each angle (radians) uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2})",
                this.yaw.Rad.ToString(format, formatProvider),
                this.pitch.Rad.ToString(format, formatProvider),
                this.roll.Rad.ToString(format, formatProvider));
        }

        #endregion
    }
}
