using System;

namespace RP.Math
{
    /// <summary>
    /// An immutable description of a coordinate-system <b>convention</b>: which Cartesian axis
    /// (x, y, z) plays each role — right, up and forward (far) — and which direction along it is
    /// positive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="OrthogonalAxes"/> is meant to be chosen once, at the start of a project, and then handed
    /// to any maths that needs to know the caller's frame (for example "which way is up?"). It is a
    /// constant label, not something that gets rotated; arbitrary orientations live in
    /// <see cref="Quaternion"/> / <see cref="Pose"/>.
    /// </para>
    /// <para>
    /// Because a convention is built from the three opposed pairs (up/down, left/right, near/far),
    /// and each pair may be used only once, the only frames that can be expressed are genuine
    /// signed permutations of the axes — so it is impossible to construct a skewed, non-unit or
    /// half-defined frame. The signed unit basis (<see cref="Right"/>, <see cref="Up"/>,
    /// <see cref="Forward"/>) and the <see cref="Handedness"/> are derived from the labels, so they
    /// can never disagree with them.
    /// </para>
    /// </remarks>
    public struct OrthogonalAxes
    {
        #region Struct Variables

        /// <summary>The role assigned to the x axis.</summary>
        private readonly AxisAlignment x;

        /// <summary>The role assigned to the y axis.</summary>
        private readonly AxisAlignment y;

        /// <summary>The role assigned to the z axis.</summary>
        private readonly AxisAlignment z;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a convention from the role of each Cartesian axis.
        /// </summary>
        /// <param name="x">The role of the x axis.</param>
        /// <param name="y">The role of the y axis.</param>
        /// <param name="z">The role of the z axis.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if two axes are drawn from the same opposed pair (for example two "up" axes, or
        /// both "up" and "down"). A valid convention uses each of the three pairs exactly once.
        /// </exception>
        public OrthogonalAxes(AxisAlignment x, AxisAlignment y, AxisAlignment z)
        {
            // A valid convention assigns each of the three opposed pairs — up/down, left/right and
            // near/far — to exactly one Cartesian axis. With three axes and three pairs, "no pair
            // used twice" already guarantees "every pair used once" (pigeonhole), so this single
            // duplicate check rejects both contradictions (two "up"s) and incomplete frames.
            AxisPair px = PairOf(x), py = PairOf(y), pz = PairOf(z);
            if (px == py || py == pz || px == pz)
                throw new ArgumentException(AXIS_DUPLICATION);

            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region Accessors

        /// <summary>The role assigned to the x axis.</summary>
        public AxisAlignment X { get { return x; } }

        /// <summary>The role assigned to the y axis.</summary>
        public AxisAlignment Y { get { return y; } }

        /// <summary>The role assigned to the z axis.</summary>
        public AxisAlignment Z { get { return z; } }

        /// <summary>
        /// Index accessor mapping [0] -> X, [1] -> Y and [2] -> Z.
        /// </summary>
        /// <param name="index">The axis index (0 = x, 1 = y, 2 = z).</param>
        /// <exception cref="ArgumentException">Thrown if the index is not 0, 1 or 2.</exception>
        public AxisAlignment this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentException(THREE_COMPONENTS, "index");
                }
            }
        }

        /// <summary>
        /// Index accessor mapping 'x' -> X, 'y' -> Y and 'z' -> Z.
        /// </summary>
        /// <param name="index">The axis character ('x', 'y' or 'z').</param>
        /// <exception cref="ArgumentException">Thrown if the character is not 'x', 'y' or 'z'.</exception>
        public AxisAlignment this[char index]
        {
            get
            {
                switch (index)
                {
                    case 'x': return X;
                    case 'y': return Y;
                    case 'z': return Z;
                    default: throw new ArgumentException(THREE_COMPONENTS, "index");
                }
            }
        }

        #endregion

        #region Derived basis and handedness

        /// <summary>
        /// The unit vector pointing in the convention's <b>right</b> direction.
        /// </summary>
        /// <remarks>If x is <c>RIGHT</c> this is (1,0,0); if x is
        /// <c>LEFT</c> it is (-1,0,0); and likewise for whichever axis carries
        /// the left/right pair.</remarks>
        public Vector Right { get { return DirectionOf(AxisAlignment.RIGHT, AxisAlignment.LEFT); } }

        /// <summary>The unit vector pointing in the convention's <b>up</b> direction.</summary>
        public Vector Up { get { return DirectionOf(AxisAlignment.UP, AxisAlignment.DOWN); } }

        /// <summary>
        /// The unit vector pointing in the convention's <b>forward</b> direction — into the scene,
        /// away from the viewer (the "far" direction).
        /// </summary>
        public Vector Forward { get { return DirectionOf(AxisAlignment.FAR, AxisAlignment.NEAR); } }

        /// <summary>
        /// Whether the convention is left- or right-handed.
        /// </summary>
        /// <remarks>
        /// Point the right hand's fingers along <see cref="Right"/> and curl them towards
        /// <see cref="Up"/>: the thumb gives the right-handed third direction. If that thumb points
        /// <i>away</i> from <see cref="Forward"/> (towards the viewer) the frame is right-handed;
        /// if it points along <see cref="Forward"/> it is left-handed. Equivalently
        /// (Right x Up) . Forward is negative for a right-handed frame.
        /// </remarks>
        public Handedness Handedness { get { return HandednessOf(Right, Up, Forward); } }

        /// <summary>
        /// The signed unit vector for an opposed pair: +axis if that axis carries
        /// <paramref name="positive"/>, -axis if it carries <paramref name="negative"/>.
        /// </summary>
        private Vector DirectionOf(AxisAlignment positive, AxisAlignment negative)
        {
            for (int i = 0; i < 3; i++)
            {
                AxisAlignment a = this[i];
                if (a == positive) return UnitVector(i, 1);
                if (a == negative) return UnitVector(i, -1);
            }

            // Unreachable for any OrthogonalAxes built through the validated constructor (every pair is
            // guaranteed present), but kept explicit rather than returning a silent zero vector.
            throw new InvalidOperationException(
                "OrthogonalAxes is missing the " + positive + "/" + negative + " pair.");
        }

        /// <summary>A signed unit vector along the given Cartesian axis (0 = x, 1 = y, 2 = z).</summary>
        private static Vector UnitVector(int axisIndex, int sign)
        {
            switch (axisIndex)
            {
                case 0: return new Vector(sign, 0, 0);
                case 1: return new Vector(0, sign, 0);
                default: return new Vector(0, 0, sign);
            }
        }

        /// <summary>Which opposed pair an alignment belongs to (used to validate a convention).</summary>
        private static AxisPair PairOf(AxisAlignment a)
        {
            switch (a)
            {
                case AxisAlignment.UP:
                case AxisAlignment.DOWN: return AxisPair.UpDown;
                case AxisAlignment.LEFT:
                case AxisAlignment.RIGHT: return AxisPair.LeftRight;
                case AxisAlignment.NEAR:
                case AxisAlignment.FAR: return AxisPair.NearFar;
                default: throw new ArgumentException("Unknown axis alignment: " + a);
            }
        }

        /// <summary>
        /// Right-handed when (right x up) . forward is negative (the right-hand thumb points away
        /// from forward), left-handed otherwise.
        /// </summary>
        private static Handedness HandednessOf(Vector right, Vector up, Vector forward)
        {
            return right.CrossProduct(up).DotProduct(forward) < 0
                ? Handedness.Right
                : Handedness.Left;
        }

        #endregion

        #region Mapping between this convention and the canonical right / up / far frame

        public void Map
        (
            double x, double y, double z,
            out double right, out double up, out double far,
            out bool iRight, out bool iUp, out bool iFar
        )
        {
            // Preinitialisation
            right = 0.0; up = 0.0; far = 0.0;
            iRight = false; iUp = false; iFar = false;

            switch (X)
            {
                case AxisAlignment.DOWN: up = x; iUp = true; break;
                case AxisAlignment.UP: up = x; break;
                case AxisAlignment.RIGHT: right = x; break;
                case AxisAlignment.LEFT: right = x; iRight = true; break;
                case AxisAlignment.NEAR: far = x; iFar = true; break;
                case AxisAlignment.FAR: far = x; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Y)
            {
                case AxisAlignment.DOWN: up = y; iUp = true; break;
                case AxisAlignment.UP: up = y; break;
                case AxisAlignment.RIGHT: right = y; break;
                case AxisAlignment.LEFT: right = y; iRight = true; break;
                case AxisAlignment.NEAR: far = y; iFar = true; break;
                case AxisAlignment.FAR: far = y; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Z)
            {
                case AxisAlignment.DOWN: up = z; iUp = true; break;
                case AxisAlignment.UP: up = z; break;
                case AxisAlignment.RIGHT: right = z; break;
                case AxisAlignment.LEFT: right = z; iRight = true; break;
                case AxisAlignment.NEAR: far = z; iFar = true; break;
                case AxisAlignment.FAR: far = z; break;
                default: throw new ArgumentException(); // Should never reach this
            }
        }

        public void Map
        (
            out double x, out double y, out double z,
            double right, double up, double far,
            out bool iRight, out bool iUp, out bool iFar
        )
        {
            // Preinitialisation
            x = 0.0; y = 0.0; z = 0.0;
            iRight = false; iUp = false; iFar = false;

            switch (X)
            {
                case AxisAlignment.DOWN: x = up; iUp = true; break;
                case AxisAlignment.UP: x = up; break;
                case AxisAlignment.RIGHT: x = right; break;
                case AxisAlignment.LEFT: x = right; iRight = true; break;
                case AxisAlignment.NEAR: x = far; iFar = true; break;
                case AxisAlignment.FAR: x = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Y)
            {
                case AxisAlignment.DOWN: y = up; iUp = true; break;
                case AxisAlignment.UP: y = up; break;
                case AxisAlignment.RIGHT: y = right; break;
                case AxisAlignment.LEFT: y = right; iRight = true; break;
                case AxisAlignment.NEAR: y = far; iFar = true; break;
                case AxisAlignment.FAR: y = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Z)
            {
                case AxisAlignment.DOWN: z = up; iUp = true; break;
                case AxisAlignment.UP: z = up; break;
                case AxisAlignment.RIGHT: z = right; break;
                case AxisAlignment.LEFT: z = right; iRight = true; break;
                case AxisAlignment.NEAR: z = far; iFar = true; break;
                case AxisAlignment.FAR: z = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }
        }

        public void Map
        (
            float x, float y, float z,
            out float right, out float up, out float far,
            out bool iRight, out bool iUp, out bool iFar
        )
        {
            // Preinitialisation
            right = 0.0f; up = 0.0f; far = 0.0f;
            iRight = false; iUp = false; iFar = false;

            switch (X)
            {
                case AxisAlignment.DOWN: up = x; iUp = true; break;
                case AxisAlignment.UP: up = x; break;
                case AxisAlignment.RIGHT: right = x; break;
                case AxisAlignment.LEFT: right = x; iRight = true; break;
                case AxisAlignment.NEAR: far = x; iFar = true; break;
                case AxisAlignment.FAR: far = x; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Y)
            {
                case AxisAlignment.DOWN: up = y; iUp = true; break;
                case AxisAlignment.UP: up = y; break;
                case AxisAlignment.RIGHT: right = y; break;
                case AxisAlignment.LEFT: right = y; iRight = true; break;
                case AxisAlignment.NEAR: far = y; iFar = true; break;
                case AxisAlignment.FAR: far = y; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Z)
            {
                case AxisAlignment.DOWN: up = z; iUp = true; break;
                case AxisAlignment.UP: up = z; break;
                case AxisAlignment.RIGHT: right = z; break;
                case AxisAlignment.LEFT: right = z; iRight = true; break;
                case AxisAlignment.NEAR: far = z; iFar = true; break;
                case AxisAlignment.FAR: far = z; break;
                default: throw new ArgumentException(); // Should never reach this
            }
        }

        public void Map
        (
            out float x, out float y,
            out float z, float right, float up, float far,
            out bool iRight, out bool iUp, out bool iFar
        )
        {
            // Preinitialisation
            x = 0.0f; y = 0.0f; z = 0.0f;
            iRight = false; iUp = false; iFar = false;

            switch (X)
            {
                case AxisAlignment.DOWN: x = up; iUp = true; break;
                case AxisAlignment.UP: x = up; break;
                case AxisAlignment.RIGHT: x = right; break;
                case AxisAlignment.LEFT: x = right; iRight = true; break;
                case AxisAlignment.NEAR: x = far; iFar = true; break;
                case AxisAlignment.FAR: x = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Y)
            {
                case AxisAlignment.DOWN: y = up; iUp = true; break;
                case AxisAlignment.UP: y = up; break;
                case AxisAlignment.RIGHT: y = right; break;
                case AxisAlignment.LEFT: y = right; iRight = true; break;
                case AxisAlignment.NEAR: y = far; iFar = true; break;
                case AxisAlignment.FAR: y = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }

            switch (Z)
            {
                case AxisAlignment.DOWN: z = up; iUp = true; break;
                case AxisAlignment.UP: z = up; break;
                case AxisAlignment.RIGHT: z = right; break;
                case AxisAlignment.LEFT: z = right; iRight = true; break;
                case AxisAlignment.NEAR: z = far; iFar = true; break;
                case AxisAlignment.FAR: z = far; break;
                default: throw new ArgumentException(); // Should never reach this
            }
        }

        #endregion

        #region Operators

        /// <summary>Two conventions are equal when all three axes carry the same role.</summary>
        public static bool operator ==(OrthogonalAxes a1, OrthogonalAxes a2)
        {
            return a1.X == a2.X && a1.Y == a2.Y && a1.Z == a2.Z;
        }

        /// <summary>Two conventions differ when any axis carries a different role.</summary>
        public static bool operator !=(OrthogonalAxes a1, OrthogonalAxes a2)
        {
            return !(a1 == a2);
        }

        #endregion

        #region Predefined conventions

        // The conventions of the major graphics and maths systems. There is deliberately no
        // "default": a project must choose one explicitly, because the right answer depends on the
        // ecosystem the maths is feeding. Several systems share a frame, so the constants below are
        // intentional aliases (OpenGL == Maya == Godot, and so on).

        /// <summary>
        /// OpenGL: y-up, <b>right-handed</b>. +z points towards the viewer (out of the screen), so
        /// the forward/far direction is -z (z carries <c>NEAR</c>).
        /// </summary>
        public static readonly OrthogonalAxes OpenGL = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.NEAR);

        /// <summary>Maya: y-up, right-handed — the same frame as <see cref="OpenGL"/>.</summary>
        public static readonly OrthogonalAxes Maya = OpenGL;

        /// <summary>Godot: y-up, right-handed — the same frame as <see cref="OpenGL"/>.</summary>
        public static readonly OrthogonalAxes Godot = OpenGL;

        /// <summary>The classic textbook y-up, right-handed maths frame — the same as <see cref="OpenGL"/>.</summary>
        public static readonly OrthogonalAxes MathsYUp = OpenGL;

        /// <summary>
        /// Direct3D / DirectX: y-up, <b>left-handed</b>. +z points into the screen (the forward/far
        /// direction), so z carries <c>FAR</c>.
        /// </summary>
        public static readonly OrthogonalAxes DirectX = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.FAR);

        /// <summary>Direct3D — the same frame as <see cref="DirectX"/>.</summary>
        public static readonly OrthogonalAxes Direct3D = DirectX;

        /// <summary>Unity: y-up, left-handed — the same frame as <see cref="DirectX"/>.</summary>
        public static readonly OrthogonalAxes Unity = DirectX;

        /// <summary>
        /// Blender: z-up, <b>right-handed</b>. +y points into the screen (forward/far) and +z is up.
        /// </summary>
        public static readonly OrthogonalAxes Blender = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.FAR, AxisAlignment.UP);

        /// <summary>3ds Max: z-up, right-handed — the same frame as <see cref="Blender"/>.</summary>
        public static readonly OrthogonalAxes Max3ds = Blender;

        /// <summary>The common z-up, right-handed engineering/maths frame — the same as <see cref="Blender"/>.</summary>
        public static readonly OrthogonalAxes MathsZUp = Blender;

        /// <summary>
        /// Unreal Engine: z-up, <b>left-handed</b>. +x points into the screen (forward/far), +y is
        /// right and +z is up.
        /// </summary>
        public static readonly OrthogonalAxes Unreal = new OrthogonalAxes(AxisAlignment.FAR, AxisAlignment.RIGHT, AxisAlignment.UP);

        #endregion

        #region Messages

        private const string THREE_COMPONENTS = "OrthogonalAxes has only three components (x, y, z).";

        private const string AXIS_DUPLICATION =
            "Two or more axes share an opposed pair; each of up/down, left/right and near/far must be used exactly once.";

        private const string AXIS_DESCRIPTION = "Cartesian axis convention";

        #endregion

        #region Standard Functions

        /// <summary>Textual description of the convention, e.g. "(RIGHT, UP, NEAR)".</summary>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Textual description of the convention.
        /// </summary>
        /// <param name="format">'x', 'y', 'z', or 'v' (verbose); null or empty for the default form.</param>
        /// <param name="formatProvider">The culture-specific formatting provider (unused).</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format))
            {
                switch (Char.ToLower(format[0]))
                {
                    case 'x': return X.ToString();
                    case 'y': return Y.ToString();
                    case 'z': return Z.ToString();
                    case 'v':
                        return string.Format(
                            "{0} ( x={1}, y={2}, z={3}, {4}-handed )",
                            AXIS_DESCRIPTION, X, Y, Z, Handedness);
                }
            }

            return String.Format("({0}, {1}, {2})", X, Y, Z);
        }

        /// <summary>Hashcode, required to support the equality operators.</summary>
        public override int GetHashCode()
        {
            return ((int)X + (int)Y + (int)Z) % Int32.MaxValue;
        }

        /// <summary>Equality against any object.</summary>
        public override bool Equals(object other)
        {
            return other is OrthogonalAxes && (OrthogonalAxes)other == this;
        }

        /// <summary>Equality against another convention.</summary>
        public bool Equals(OrthogonalAxes other)
        {
            return other == this;
        }

        #endregion
    }

    /// <summary>
    /// Names the two opposed directions of each Cartesian axis pair: up/down, left/right and
    /// near/far. "Near" / "far" describe where a thing sits relative to the viewer (near = towards
    /// the viewer, far = into the scene) — static labels, to match the positional up/down and
    /// left/right rather than the motion-flavoured forward/backward.
    /// </summary>
    /// <remarks>
    /// Values are laid out so the low bit is a negation flag: each canonical-positive direction
    /// (FAR, UP, RIGHT) is even, and its opposite (NEAR, DOWN, LEFT) is the same value with the low
    /// bit set. So the opposite of any alignment is <c>value XOR 1</c>, and "is this the negative
    /// direction?" is just <c>(value &amp; 1) == 1</c>.
    /// </remarks>
    public enum AxisAlignment
    {
        FAR = 2,
        NEAR = 3,
        UP = 4,
        DOWN = 5,
        RIGHT = 6,
        LEFT = 7,
    }

    /// <summary>The chirality of a coordinate frame.</summary>
    public enum Handedness
    {
        Left,
        Right,
    }

    /// <summary>The three opposed pairs an <see cref="AxisAlignment"/> can belong to.</summary>
    internal enum AxisPair
    {
        UpDown,
        LeftRight,
        NearFar,
    }
}
