namespace RP.Math
{
    using System;
    using System.Globalization;

    /// <summary>
    /// An immutable rigid placement in 3D space: a position together with an orientation
    /// ("where" and "which way"). A pose is the human-meaningful form of a rigid transform — a point
    /// plus a rotation — rather than a 4x4 <see cref="Matrix"/>.
    /// </summary>
    /// <remarks>
    /// Orientation is stored internally as a unit <see cref="Quaternion"/> (the robust representation:
    /// compact, no gimbal lock, smoothly interpolatable). For convenience a pose can be constructed from,
    /// and read back as, the friendlier <see cref="Rotation"/> (Euler X/Y/Z) and <see cref="Attitude"/>
    /// (yaw/pitch/roll) types. The stored quaternion is always normalized.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Pose : IEquatable<Pose>, IFormattable
    {
        #region Fields

        private readonly Vector position;
        private readonly Quaternion rotation; // always stored normalized

        #endregion

        #region Constructors

        /// <summary>Construct a pose from a position and an orientation quaternion (normalized on construction).</summary>
        public Pose(Vector position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation.NormalizeOrDefault();
        }

        /// <summary>Construct a pose from a position and a <see cref="Rotation"/> (Euler X/Y/Z).</summary>
        public Pose(Vector position, Rotation rotation)
            : this(position, rotation.ToQuaternion())
        {
        }

        /// <summary>Construct a pose from a position and an <see cref="Attitude"/> (yaw/pitch/roll).</summary>
        public Pose(Vector position, Attitude attitude)
            : this(position, attitude.ToQuaternion())
        {
        }

        /// <summary>Construct a pose at a position with no rotation.</summary>
        public Pose(Vector position)
            : this(position, Quaternion.Identity)
        {
        }

        #endregion

        #region Constants

        /// <summary>The identity pose: at the origin with no rotation.</summary>
        public static readonly Pose Identity = new Pose(new Vector(0, 0, 0), Quaternion.Identity);

        #endregion

        #region Accessors

        /// <summary>The position (translation) of the pose.</summary>
        public Vector Position { get { return this.position; } }

        /// <summary>The orientation of the pose as a unit <see cref="Quaternion"/>.</summary>
        public Quaternion Rotation { get { return this.rotation; } }

        /// <summary>The orientation expressed as a <see cref="Rotation"/> (Euler X/Y/Z).</summary>
        public Rotation RotationAsEuler { get { return RP.Math.Rotation.FromQuaternion(this.rotation); } }

        /// <summary>The orientation expressed as an <see cref="Attitude"/> (yaw/pitch/roll).</summary>
        public Attitude RotationAsAttitude { get { return RP.Math.Rotation.FromQuaternion(this.rotation).ToAttitude(); } }

        #endregion

        #region Factories

        /// <summary>A pose at <paramref name="position"/> with no rotation.</summary>
        public static Pose At(Vector position)
        {
            return new Pose(position, Quaternion.Identity);
        }

        /// <summary>A pose from a position and a rotation about an axis.</summary>
        public static Pose FromAxisAngle(Vector position, Vector axis, Angle angle)
        {
            return new Pose(position, Quaternion.FromAxisAngle(axis, angle));
        }

        #endregion

        #region Transform application

        /// <summary>
        /// Transform a point from this pose's local space into world space: rotate it by the pose's
        /// orientation, then translate by the pose's position.
        /// </summary>
        public Vector Apply(Vector localPoint)
        {
            return this.position + this.rotation.Rotate(localPoint);
        }

        /// <summary>
        /// Transform a direction from local space into world space: rotate it by the pose's orientation
        /// only (no translation).
        /// </summary>
        public Vector ApplyDirection(Vector localDirection)
        {
            return this.rotation.Rotate(localDirection);
        }

        /// <summary>
        /// Transform a point from world space back into this pose's local space (the inverse of
        /// <see cref="Apply"/>).
        /// </summary>
        public Vector ApplyInverse(Vector worldPoint)
        {
            return this.rotation.Conjugate().Rotate(worldPoint - this.position);
        }

        /// <summary>
        /// The 4x4 homogeneous transform matrix equivalent to this pose (rotation then translation),
        /// such that <c>pose.ToMatrix() * p</c> equals <c>pose.Apply(p)</c>. Built directly as the
        /// rotation matrix with the position in the translation column.
        /// </summary>
        public Matrix ToMatrix()
        {
            Matrix r = this.rotation.ToMatrix();
            return new Matrix(
                r.A1_1, r.A1_2, r.A1_3, this.position.X,
                r.A2_1, r.A2_2, r.A2_3, this.position.Y,
                r.A3_1, r.A3_2, r.A3_3, this.position.Z,
                0, 0, 0, 1);
        }

        #endregion

        #region Composition and inverse

        /// <summary>The inverse pose, such that <c>pose.Compose(pose.Inverse())</c> is the identity.</summary>
        public Pose Inverse()
        {
            Quaternion invRotation = this.rotation.Conjugate();
            return new Pose(invRotation.Rotate(-this.position), invRotation);
        }

        /// <summary>
        /// Compose two poses: the result applies <paramref name="inner"/> first, then this pose, i.e.
        /// <c>this.Compose(inner).Apply(p) == this.Apply(inner.Apply(p))</c>.
        /// </summary>
        public Pose Compose(Pose inner)
        {
            return new Pose(this.Apply(inner.Position), this.rotation * inner.Rotation);
        }

        /// <summary>Compose two poses (<paramref name="outer"/> applied after <paramref name="inner"/>).</summary>
        public static Pose operator *(Pose outer, Pose inner)
        {
            return outer.Compose(inner);
        }

        #endregion

        #region Modification (return a new pose)

        /// <summary>A copy of this pose translated by <paramref name="offset"/> (in world space).</summary>
        public Pose Translate(Vector offset)
        {
            return new Pose(this.position + offset, this.rotation);
        }

        /// <summary>A copy of this pose with an additional rotation applied after its current orientation.</summary>
        public Pose RotateBy(Quaternion delta)
        {
            return new Pose(this.position, delta * this.rotation);
        }

        /// <summary>A copy of this pose moved to <paramref name="newPosition"/>, keeping its orientation.</summary>
        public Pose WithPosition(Vector newPosition)
        {
            return new Pose(newPosition, this.rotation);
        }

        /// <summary>A copy of this pose with <paramref name="newRotation"/>, keeping its position.</summary>
        public Pose WithRotation(Quaternion newRotation)
        {
            return new Pose(this.position, newRotation);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Pose p && this.Equals(p);
        }

        /// <summary>Equality with another pose (position and orientation).</summary>
        public bool Equals(Pose other)
        {
            return this.position.Equals(other.Position) && this.rotation.Equals(other.Rotation);
        }

        /// <summary>Equality with another pose within an absolute tolerance (applied to both position and orientation components).</summary>
        public bool Equals(Pose other, double tolerance)
        {
            return this.position.Equals(other.Position, tolerance) && this.rotation.Equals(other.Rotation, tolerance);
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(Pose p1, Pose p2) { return p1.Equals(p2); }

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Pose p1, Pose p2) { return !p1.Equals(p2); }

        /// <summary>A hash code derived from the position and orientation.</summary>
        public override int GetHashCode()
        {
            return this.position.GetHashCode() ^ this.rotation.GetHashCode();
        }

        /// <summary>Deconstruct into position and orientation, enabling <c>var (position, rotation) = pose;</c>.</summary>
        public void Deconstruct(out Vector position, out Quaternion rotation)
        {
            position = this.position;
            rotation = this.rotation;
        }

        /// <summary>A string of the form <c>[position, rotation]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[position, rotation]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0}, {1}]",
                this.position.ToString(format, formatProvider),
                this.rotation.ToString(format, formatProvider));
        }

        #endregion
    }
}
