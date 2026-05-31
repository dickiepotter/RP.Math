namespace RP.Math
{
    using System;
    using System.Globalization;

    using RP.Math.Exceptions;

    using Math = System.Math;

    /// <summary>
    /// An immutable quaternion (x, y, z, w) — a four-component number used primarily to represent
    /// rotations in three dimensions without the gimbal-lock and interpolation problems of Euler angles.
    /// </summary>
    /// <remarks>
    /// Follows the same design as <see cref="Vector"/>: an immutable value type providing both static
    /// and instance forms of each operation, tolerance-aware equality, safe (<c>…OrDefault</c>) and
    /// strict variants of normalization, and formatting.
    /// The scalar (real) part is <see cref="W"/>; the vector (imaginary) part is (<see cref="X"/>,
    /// <see cref="Y"/>, <see cref="Z"/>). Multiplication uses the Hamilton convention.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>, IFormattable
    {
        #region Fields

        private readonly double x;
        private readonly double y;
        private readonly double z;
        private readonly double w;

        #endregion

        #region Constructors

        /// <summary>Construct a quaternion from its four components.</summary>
        public Quaternion(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>Construct a quaternion from a vector (imaginary) part and a scalar (real) part.</summary>
        public Quaternion(Vector vectorPart, double scalarPart)
        {
            this.x = vectorPart.X;
            this.y = vectorPart.Y;
            this.z = vectorPart.Z;
            this.w = scalarPart;
        }

        /// <summary>Construct a quaternion from an array of exactly four components (x, y, z, w).</summary>
        /// <exception cref="ArgumentException">Thrown if the array does not contain exactly four components.</exception>
        public Quaternion(double[] xyzw)
        {
            if (xyzw == null) throw new ArgumentNullException(nameof(xyzw));
            if (xyzw.Length != 4) throw new ArgumentException(FOUR_COMPONENTS, nameof(xyzw));
            this.x = xyzw[0];
            this.y = xyzw[1];
            this.z = xyzw[2];
            this.w = xyzw[3];
        }

        /// <summary>Copy constructor.</summary>
        public Quaternion(Quaternion q)
        {
            this.x = q.X;
            this.y = q.Y;
            this.z = q.Z;
            this.w = q.W;
        }

        #endregion

        #region Constants

        /// <summary>The identity quaternion (0, 0, 0, 1) representing no rotation.</summary>
        public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);

        /// <summary>The zero quaternion (0, 0, 0, 0).</summary>
        public static readonly Quaternion Zero = new Quaternion(0, 0, 0, 0);

        /// <summary>A quaternion whose components are all <see cref="double.NaN"/>.</summary>
        public static readonly Quaternion NaN = new Quaternion(double.NaN, double.NaN, double.NaN, double.NaN);

        #endregion

        #region Accessors

        /// <summary>The x component of the vector (imaginary) part.</summary>
        public double X { get { return this.x; } }

        /// <summary>The y component of the vector (imaginary) part.</summary>
        public double Y { get { return this.y; } }

        /// <summary>The z component of the vector (imaginary) part.</summary>
        public double Z { get { return this.z; } }

        /// <summary>The scalar (real) component.</summary>
        public double W { get { return this.w; } }

        /// <summary>The vector (imaginary) part as a <see cref="Vector"/>.</summary>
        public Vector Xyz { get { return new Vector(this.x, this.y, this.z); } }

        /// <summary>The scalar (real) part. Alias for <see cref="W"/>.</summary>
        public double Scalar { get { return this.w; } }

        /// <summary>The magnitude (norm / length) of the quaternion.</summary>
        public double Magnitude { get { return Math.Sqrt(this.MagnitudeSquared); } }

        /// <summary>The squared magnitude (norm squared) of the quaternion. Cheaper than <see cref="Magnitude"/>.</summary>
        public double MagnitudeSquared { get { return (this.x * this.x) + (this.y * this.y) + (this.z * this.z) + (this.w * this.w); } }

        /// <summary>The quaternion components as an array (x, y, z, w).</summary>
        public double[] Array { get { return new[] { this.x, this.y, this.z, this.w }; } }

        /// <summary>Index accessor: [0]-&gt;X, [1]-&gt;Y, [2]-&gt;Z, [3]-&gt;W.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not 0, 1, 2 or 3.</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.x;
                    case 1: return this.y;
                    case 2: return this.z;
                    case 3: return this.w;
                    default: throw new ArgumentOutOfRangeException(nameof(index), index, FOUR_COMPONENTS);
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>Component-wise addition of two quaternions.</summary>
        public static Quaternion operator +(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z, q1.W + q2.W);
        }

        /// <summary>Component-wise subtraction of two quaternions.</summary>
        public static Quaternion operator -(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z, q1.W - q2.W);
        }

        /// <summary>Multiply a quaternion by a scalar.</summary>
        public static Quaternion operator *(Quaternion q1, double s2)
        {
            return new Quaternion(q1.X * s2, q1.Y * s2, q1.Z * s2, q1.W * s2);
        }

        /// <summary>Multiply a scalar by a quaternion.</summary>
        public static Quaternion operator *(double s1, Quaternion q2)
        {
            return q2 * s1;
        }

        /// <summary>Divide a quaternion by a scalar.</summary>
        public static Quaternion operator /(Quaternion q1, double s2)
        {
            return new Quaternion(q1.X / s2, q1.Y / s2, q1.Z / s2, q1.W / s2);
        }

        /// <summary>
        /// The Hamilton product of two quaternions. Represents the composition of the two rotations
        /// (apply <paramref name="q2"/> first, then <paramref name="q1"/>). Not commutative.
        /// </summary>
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(
                (q1.W * q2.X) + (q1.X * q2.W) + (q1.Y * q2.Z) - (q1.Z * q2.Y),
                (q1.W * q2.Y) - (q1.X * q2.Z) + (q1.Y * q2.W) + (q1.Z * q2.X),
                (q1.W * q2.Z) + (q1.X * q2.Y) - (q1.Y * q2.X) + (q1.Z * q2.W),
                (q1.W * q2.W) - (q1.X * q2.X) - (q1.Y * q2.Y) - (q1.Z * q2.Z));
        }

        /// <summary>Negate a quaternion (component-wise). Represents the same rotation as the original.</summary>
        public static Quaternion operator -(Quaternion q1)
        {
            return new Quaternion(-q1.X, -q1.Y, -q1.Z, -q1.W);
        }

        /// <summary>Unary plus (returns the quaternion unchanged).</summary>
        public static Quaternion operator +(Quaternion q1)
        {
            return new Quaternion(+q1.X, +q1.Y, +q1.Z, +q1.W);
        }

        /// <summary>Component-wise equality.</summary>
        public static bool operator ==(Quaternion q1, Quaternion q2)
        {
            return q1.X == q2.X && q1.Y == q2.Y && q1.Z == q2.Z && q1.W == q2.W;
        }

        /// <summary>Component-wise inequality.</summary>
        public static bool operator !=(Quaternion q1, Quaternion q2)
        {
            return !(q1 == q2);
        }

        #endregion

        #region Products

        /// <summary>The dot product of two quaternions (treated as 4-vectors).</summary>
        public static double DotProduct(Quaternion q1, Quaternion q2)
        {
            return (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
        }

        /// <summary>The dot product of this quaternion and another.</summary>
        public double DotProduct(Quaternion other)
        {
            return DotProduct(this, other);
        }

        /// <summary>The Hamilton product of two quaternions.</summary>
        /// <implementation><see cref="op_Multiply(Quaternion, Quaternion)"/></implementation>
        public static Quaternion Multiply(Quaternion q1, Quaternion q2)
        {
            return q1 * q2;
        }

        /// <summary>The Hamilton product of this quaternion and another (this applied after other).</summary>
        public Quaternion Multiply(Quaternion other)
        {
            return this * other;
        }

        #endregion

        #region Conjugate, Inverse and Normalize

        /// <summary>The conjugate of a quaternion (negates the vector part). For a unit quaternion this is its inverse.</summary>
        public static Quaternion Conjugate(Quaternion q1)
        {
            return new Quaternion(-q1.X, -q1.Y, -q1.Z, q1.W);
        }

        /// <summary>The conjugate of this quaternion.</summary>
        public Quaternion Conjugate()
        {
            return Conjugate(this);
        }

        /// <summary>The multiplicative inverse of a quaternion (conjugate divided by magnitude squared).</summary>
        /// <exception cref="NormalizeQuaternionException">Thrown if the quaternion has zero magnitude.</exception>
        public static Quaternion Inverse(Quaternion q1)
        {
            double normSq = q1.MagnitudeSquared;
            if (normSq == 0) throw new NormalizeQuaternionException(INVERSE_0);
            return Conjugate(q1) / normSq;
        }

        /// <summary>The multiplicative inverse of this quaternion.</summary>
        public Quaternion Inverse()
        {
            return Inverse(this);
        }

        /// <summary>
        /// The normalized (unit) quaternion. A unit quaternion represents a pure rotation.
        /// </summary>
        /// <exception cref="NormalizeQuaternionException">Thrown if the quaternion has zero magnitude or a non-finite component.</exception>
        public static Quaternion Normalize(Quaternion q1)
        {
            if (q1.IsNaN()) throw new NormalizeQuaternionException(NORMALIZE_NaN);

            double magnitude = q1.Magnitude;
            if (magnitude == 0) throw new NormalizeQuaternionException(NORMALIZE_0);
            if (double.IsInfinity(magnitude)) throw new NormalizeQuaternionException(NORMALIZE_Inf);

            return q1 / magnitude;
        }

        /// <summary>The normalized (unit) quaternion.</summary>
        public Quaternion Normalize()
        {
            return Normalize(this);
        }

        /// <summary>
        /// The normalized (unit) quaternion, or <see cref="Identity"/> if it cannot be normalized
        /// (zero magnitude or a non-finite component). Never throws.
        /// </summary>
        public static Quaternion NormalizeOrDefault(Quaternion q1)
        {
            double magnitude = q1.Magnitude;
            if (q1.IsNaN() || magnitude == 0 || double.IsInfinity(magnitude))
            {
                return Identity;
            }

            return q1 / magnitude;
        }

        /// <summary>The normalized (unit) quaternion, or <see cref="Identity"/> if it cannot be normalized.</summary>
        public Quaternion NormalizeOrDefault()
        {
            return NormalizeOrDefault(this);
        }

        #endregion

        #region Rotation construction and conversion

        /// <summary>
        /// Create a unit quaternion representing a rotation of <paramref name="angle"/> about <paramref name="axis"/>.
        /// A <see cref="double"/> (radians) may be passed directly thanks to <see cref="Angle"/>'s implicit conversion.
        /// </summary>
        /// <param name="axis">The axis of rotation (need not be unit length; it is normalized).</param>
        /// <param name="angle">The rotation angle.</param>
        public static Quaternion FromAxisAngle(Vector axis, Angle angle)
        {
            Vector unit = axis.NormalizeOrDefault();
            double half = angle.Rad / 2.0;
            double s = Math.Sin(half);
            return new Quaternion(unit.X * s, unit.Y * s, unit.Z * s, Math.Cos(half));
        }

        /// <summary>Create a unit quaternion from an <see cref="AxisAngle"/> rotation.</summary>
        public static Quaternion FromAxisAngle(AxisAngle axisAngle)
        {
            return FromAxisAngle(axisAngle.Axis, axisAngle.Angle);
        }

        /// <summary>
        /// Decompose this (assumed unit) quaternion into the axis and angle it represents.
        /// </summary>
        /// <param name="axis">The unit axis of rotation (defaults to the X axis when the angle is zero).</param>
        /// <param name="angle">The rotation angle.</param>
        public void ToAxisAngle(out Vector axis, out Angle angle)
        {
            Quaternion q = NormalizeOrDefault(this);
            double wClamped = Math.Max(-1.0, Math.Min(1.0, q.W));
            angle = new Angle(2.0 * Math.Acos(wClamped));

            double s = Math.Sqrt(1.0 - (wClamped * wClamped));
            axis = s < 1e-9 ? new Vector(1, 0, 0) : new Vector(q.X / s, q.Y / s, q.Z / s);
        }

        /// <summary>
        /// Create a unit quaternion from yaw, pitch and roll angles.
        /// Convention: yaw about the Y axis, pitch about the X axis, roll about the Z axis,
        /// composed as <c>yaw * pitch * roll</c>.
        /// </summary>
        public static Quaternion FromYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
        {
            Quaternion qYaw = FromAxisAngle(new Vector(0, 1, 0), yaw);
            Quaternion qPitch = FromAxisAngle(new Vector(1, 0, 0), pitch);
            Quaternion qRoll = FromAxisAngle(new Vector(0, 0, 1), roll);
            return qYaw * qPitch * qRoll;
        }

        /// <summary>
        /// Convert this (assumed unit) quaternion to an equivalent 4x4 homogeneous rotation matrix,
        /// such that <c>q.ToMatrix() * v</c> equals <c>q.Rotate(v)</c>.
        /// </summary>
        public Matrix ToMatrix()
        {
            Quaternion q = NormalizeOrDefault(this);
            double xx = q.X * q.X, yy = q.Y * q.Y, zz = q.Z * q.Z;
            double xy = q.X * q.Y, xz = q.X * q.Z, yz = q.Y * q.Z;
            double wx = q.W * q.X, wy = q.W * q.Y, wz = q.W * q.Z;

            return new Matrix(
                1 - (2 * (yy + zz)), 2 * (xy - wz),       2 * (xz + wy),       0,
                2 * (xy + wz),       1 - (2 * (xx + zz)), 2 * (yz - wx),       0,
                2 * (xz - wy),       2 * (yz + wx),       1 - (2 * (xx + yy)), 0,
                0,                   0,                   0,                   1);
        }

        /// <summary>
        /// Rotate a vector by this quaternion. The quaternion is normalized first, so a non-unit
        /// quaternion still produces a pure rotation.
        /// </summary>
        public Vector Rotate(Vector v)
        {
            Quaternion q = NormalizeOrDefault(this);
            Vector u = q.Xyz;
            Vector t = 2.0 * u.CrossProduct(v);
            return v + (q.W * t) + u.CrossProduct(t);
        }

        /// <summary>Rotate a vector by a quaternion.</summary>
        public static Vector Rotate(Quaternion q1, Vector v)
        {
            return q1.Rotate(v);
        }

        #endregion

        #region Interpolation

        /// <summary>
        /// Spherical linear interpolation between two (assumed unit) quaternions. <paramref name="control"/>
        /// of 0 returns <paramref name="q1"/>, 1 returns <paramref name="q2"/>; the result is a unit quaternion.
        /// </summary>
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, double control)
        {
            Quaternion a = NormalizeOrDefault(q1);
            Quaternion b = NormalizeOrDefault(q2);

            double dot = DotProduct(a, b);

            // Take the shorter arc.
            if (dot < 0)
            {
                b = -b;
                dot = -dot;
            }

            // If very close, fall back to normalized linear interpolation to avoid division by ~0.
            if (dot > 0.9995)
            {
                return NormalizeOrDefault(a + (control * (b - a)));
            }

            double theta0 = Math.Acos(dot);
            double theta = theta0 * control;
            double sinTheta0 = Math.Sin(theta0);

            double s1 = Math.Sin(theta0 - theta) / sinTheta0;
            double s2 = Math.Sin(theta) / sinTheta0;

            return (a * s1) + (b * s2);
        }

        /// <summary>Spherical linear interpolation from this quaternion toward another.</summary>
        public Quaternion Slerp(Quaternion other, double control)
        {
            return Slerp(this, other, control);
        }

        /// <summary>
        /// Normalized linear interpolation between two quaternions (cheaper than <see cref="Slerp(Quaternion, Quaternion, double)"/>,
        /// but with non-constant angular velocity). The result is a unit quaternion.
        /// </summary>
        public static Quaternion Lerp(Quaternion q1, Quaternion q2, double control)
        {
            Quaternion b = DotProduct(q1, q2) < 0 ? -q2 : q2;
            return NormalizeOrDefault(q1 + (control * (b - q1)));
        }

        /// <summary>Normalized linear interpolation from this quaternion toward another.</summary>
        public Quaternion Lerp(Quaternion other, double control)
        {
            return Lerp(this, other, control);
        }

        /// <summary>The angle of the relative rotation between two (assumed unit) quaternions.</summary>
        public static Angle AngleBetween(Quaternion q1, Quaternion q2)
        {
            double dot = DotProduct(NormalizeOrDefault(q1), NormalizeOrDefault(q2));
            dot = Math.Max(-1.0, Math.Min(1.0, Math.Abs(dot)));
            return new Angle(2.0 * Math.Acos(dot));
        }

        /// <summary>The angle of the relative rotation between this quaternion and another.</summary>
        public Angle AngleTo(Quaternion other)
        {
            return AngleBetween(this, other);
        }

        #endregion

        #region Predicates

        /// <summary>Whether this quaternion is a unit quaternion within the given tolerance.</summary>
        public bool IsUnit(double tolerance)
        {
            return this.MagnitudeSquared.AlmostEqualsWithAbsTolerance(1, tolerance);
        }

        /// <summary>Whether this quaternion is the identity rotation within the given tolerance.</summary>
        public bool IsIdentity(double tolerance)
        {
            return this.Equals(Identity, tolerance);
        }

        /// <summary>Whether all components are zero.</summary>
        public bool IsZero()
        {
            return this.x == 0 && this.y == 0 && this.z == 0 && this.w == 0;
        }

        /// <summary>Whether any component is <see cref="double.NaN"/>.</summary>
        public bool IsNaN()
        {
            return double.IsNaN(this.x) || double.IsNaN(this.y) || double.IsNaN(this.z) || double.IsNaN(this.w);
        }

        #endregion

        #region Equality, deconstruction and conversion

        /// <summary>Component-wise equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Quaternion q && this.Equals(q);
        }

        /// <summary>Component-wise equality with another quaternion.</summary>
        public bool Equals(Quaternion other)
        {
            return this == other;
        }

        /// <summary>Component-wise equality with another quaternion within an absolute tolerance.</summary>
        public bool Equals(Quaternion other, double tolerance)
        {
            return this.X.AlmostEqualsWithAbsTolerance(other.X, tolerance)
                && this.Y.AlmostEqualsWithAbsTolerance(other.Y, tolerance)
                && this.Z.AlmostEqualsWithAbsTolerance(other.Z, tolerance)
                && this.W.AlmostEqualsWithAbsTolerance(other.W, tolerance);
        }

        /// <summary>A hash code derived from the four components.</summary>
        public override int GetHashCode()
        {
            return this.x.GetHashCode()
                ^ this.y.GetHashCode()
                ^ this.z.GetHashCode()
                ^ this.w.GetHashCode();
        }

        /// <summary>Deconstruct the quaternion into its components, enabling <c>var (x, y, z, w) = q;</c>.</summary>
        public void Deconstruct(out double x, out double y, out double z, out double w)
        {
            x = this.x;
            y = this.y;
            z = this.z;
            w = this.w;
        }

        /// <summary>Implicitly construct a quaternion from an (x, y, z, w) tuple.</summary>
        public static implicit operator Quaternion((double x, double y, double z, double w) components)
        {
            return new Quaternion(components.x, components.y, components.z, components.w);
        }

        /// <summary>Implicitly convert a quaternion to an (X, Y, Z, W) tuple.</summary>
        public static implicit operator (double X, double Y, double Z, double W)(Quaternion q)
        {
            return (q.X, q.Y, q.Z, q.W);
        }

        #endregion

        #region ToString

        /// <summary>A string of the form <c>(x, y, z, w)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(x, y, z, w)</c> where each component uses <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2}, {3})",
                this.x.ToString(format, formatProvider),
                this.y.ToString(format, formatProvider),
                this.z.ToString(format, formatProvider),
                this.w.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string FOUR_COMPONENTS = "Array must contain exactly four components (x, y, z, w)";
        private const string NORMALIZE_0 = "Cannot normalize a quaternion with zero magnitude";
        private const string NORMALIZE_Inf = "Cannot normalize a quaternion with infinite magnitude";
        private const string NORMALIZE_NaN = "Cannot normalize a quaternion with a NaN component";
        private const string INVERSE_0 = "Cannot invert a quaternion with zero magnitude";

        #endregion
    }
}
