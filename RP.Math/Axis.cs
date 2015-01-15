using System;

namespace RPUtil.Math.Math3D
{

    /// <summary>
    /// An immutable structure containing the volumetric alignments of a Cartesian a
    /// </summary>
    public struct Axis
    {
        #region Struct Variables

        /// <summary>
        /// The alignment of the _x a
        /// </summary>
        private readonly AxisAlignment x;

        /// <summary>
        /// The alignment of the _y a
        /// </summary>
        private readonly AxisAlignment y;

        /// <summary>
        /// The alignment of the _z a
        /// </summary>
        private readonly AxisAlignment z;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for an a structure
        /// </summary>
        /// <param name="x">Alignment for the _x a</param>
        /// <param name="y">Alignment for the _y a</param>
        /// <param name="z">Alignment for the _z a</param>
        /// <exception cref="ArgumentException">
        /// Thrown if two or more a share or complement over an alignment.
        /// </exception>
        public Axis(AxisAlignment x, AxisAlignment y, AxisAlignment z)
        {
            bool ud = false, lr = false, nf = false, fail = false;

            if(x == AxisAlignment.UP || x == AxisAlignment.DOWN) ud = true;
            if(x == AxisAlignment.LEFT || x == AxisAlignment.RIGHT) lr = true;
            if(x == AxisAlignment.NEAR || x == AxisAlignment.FAR) nf = true;
            
            if(y == AxisAlignment.UP || y == AxisAlignment.DOWN)  ud = ud ? (fail = true): true;
            if(y == AxisAlignment.LEFT || y == AxisAlignment.RIGHT) lr = lr ? (fail = true): true;
            if(y == AxisAlignment.NEAR || y == AxisAlignment.FAR) nf = nf ? (fail = true): true;
            
            if(z == AxisAlignment.UP || z == AxisAlignment.DOWN)  if(ud) fail = true;
            if(z == AxisAlignment.LEFT || z == AxisAlignment.RIGHT) if(lr) fail = true;
            if(z == AxisAlignment.NEAR || z == AxisAlignment.FAR) if(nf) fail = true;

            if(fail) throw new ArgumentException(AXIS_DUPLICATION);

            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region Accessors & Mutators

        /// <summary>
        /// Length to access the _x a alignment
        /// </summary>
        public AxisAlignment X { get { return x; } }

        /// <summary>
        /// Length to access the _y a alignment
        /// </summary>
        public AxisAlignment Y { get { return y; } }

        /// <summary>
        /// Length to access the _z a alignment
        /// </summary>
        public AxisAlignment Z { get { return z; } }
            
        /// <summary>
        /// An index accessor 
        /// Mapping index [0] -> X, [1] -> Y and [2] -> Z.
        /// </summary>
        /// <param name="index">The array index referring to a component within the vector (i.e. _x, _y, _z)</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the index is not between 1-3
        /// </exception>
        public AxisAlignment this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: { return X; }
                    case 1: { return Y; }
                    case 2: { return Z; }
                    default: throw new ArgumentException(THREE_COMPONENTS, "index");
                }
            }
        }

        /// <summary>
        /// An index accessor 
        /// Mapping index [0] -> X, [1] -> Y and [2] -> Z.
        /// </summary>
        /// <param name="index">The array index character referring to a component within the vector (i.e. _x, _y, _z)</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the index is not between _x-_z
        /// </exception>
        public AxisAlignment this[char index]
        {
            get
            {
                switch (index)
                {
                    case 'x': { return X; }
                    case 'y': { return Y; }
                    case 'z': { return Z; }
                    default: throw new ArgumentException(THREE_COMPONENTS, "index");
                }
            }
        }

        #endregion

        #region Functions

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

        /// <summary>
        /// Compare two Axis for equality.
        /// Are two Axis equal.
        /// </summary>
        /// <param name="a1">Axis to be compared for equality </param>
        /// <param name="a2">Axis to be compared to </param>
        /// <returns>Boolean decision (truth for equality)</returns>
        /// <implementation>
        /// Checks the equality of each pair of components, all pairs must be equal
        /// </implementation>
        public static bool operator ==(Axis a1, Axis a2)
        {
            return
            (
                a1.X == a2.X &&
                a1.Y == a2.Y &&
                a1.Z == a2.Z
            );
        }

        /// <summary>
        /// Negative comparator of two Axis.
        /// Are two Axis different.
        /// </summary>
        /// <param name="a1">Axis to be compared for in-equality </param>
        /// <param name="a2">Axis to be compared to </param>
        /// <returns>Boolean decision (truth for in-equality)</returns>
        /// <implementation>
        /// Uses the equality operand function for two Axis to prevent code duplication
        /// </implementation>
        public static bool operator !=(Axis a1, Axis a2)
        {
            return !(a1 == a2);
        }

        #endregion

        #region messages

        /// <summary>
        /// Exception message descriptive text 
        /// Used for a failure for an array argument to have three components when three are needed 
        /// </summary>
        private const string THREE_COMPONENTS = "Axis only contain three components , (_x,_y,_z)";

        /// <summary>
        /// Exception message descriptive text 
        /// Used if a share or complement over an alignment
        /// </summary>
        private const string AXIS_DUPLICATION = "Two or more a have the same or inverse alignments";

        /// <summary>
        /// Partial description of a structure for verbose textual output
        /// </summary>
        private const string AXIS_DESCRIPTION = "Cartesian a containing volumetric alignments ";

        #endregion

        #region Constants

        /// <summary>
        /// The default Axis alignments for virtual environments in graphics languages such as OpenGL
        /// </summary>
        public static readonly Axis VE_Axis_Default = new Axis(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.FAR);

        #endregion

        #region Standard Functions

        /// <summary>
        /// Textual description of the a
        /// </summary>
        /// <Implementation>
        /// Uses ToString(string, IFormatProvider) to avoid code duplication
        /// </Implementation>
        /// <returns>Text (String) representing the a</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Textual description of the a
        /// </summary>
        /// <param name="format">Formatting string: '_x','_y','_z','', or 'v' (verbose)</param>
        /// <param name="formatProvider">The culture specific fromatting provider</param>
        /// <returns>Text (String) representing the a</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format != null || format != "")
            {
                char firstChar = Char.ToLower(format[0]);
                switch (firstChar)
                {
                    case 'x': return X.ToString();
                    case 'y': return Y.ToString();
                    case 'z': return Z.ToString();
                    case 'v':
                        return
                            string.Format
                            (
                                "{0}( _x={1}, _y={2}, _z={3} )",
                                AXIS_DESCRIPTION,
                                X, Y, Z
                            );
                }
            }

            return String.Format("({0}, {1}, {2})", X, Y, Z);
        }

        /// <summary>
        /// Get the hashcode
        /// </summary>
        /// <returns>Hashcode for the object instance</returns>
        /// <implementation>
        /// Required in order to implement comparator operations (i.e. ==, !=)
        /// </implementation>
        public override int GetHashCode()
        {
            return ((int)X + (int)Y + (int)Z) % Int32.MaxValue;
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other object (which should be a Axis) to compare to</param>
        /// <returns>Truth if two Axis are equal</returns>
        /// <implementation>
        /// Checks if the object argument is a Axis object 
        /// Uses the equality operator function to avoid code duplication
        /// Required in order to implement comparator operations (i.e. ==, !=)
        /// </implementation>
        public override bool Equals(object other)
        {
            // Check object other is an Axis
            if (other is Axis)
            {
                // Convert object to Axis
                Axis otherAxis = (Axis)other;

                // Check for equality
                return otherAxis == this;
            }
            return false;
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other Axis to compare to</param>
        /// <returns>Truth if two Axis are equal</returns>
        /// <implementation>
        /// Uses the equality operator function to avoid code duplication
        /// </implementation>
        public bool Equals(Axis other)
        {
            return other == this;
        }

        #endregion
    }

    
}
