using System;
using System.Xml.Serialization;

namespace RPUtil.Math.Math3D
{
    /*public class VectorPair
        : IComparable, IComparable<VectorPair>, IEquatable<VectorPair>, IFormattable
    {
        #region Variables

        private readonly Vector vd;

        #endregion

        #region Constructors

        public VectorPair(Vector tail, Vector head)
        {
            Tail = tail;
            Head = head;
        }

        public VectorPair
        (
            double xt,
            double yt,
            double zt,
            double xh,
            double yh,
            double zh
        )
        {
            Tail = new Vector(xt, yt, zt);
            Head = new Vector(xh, yh, zh);

            vd = Tail - Head;
        }

        public VectorPair(double[] arr)
        {
            if (arr.Length != 6) throw new ArgumentException(SIX_COMPONENTS, "arr");
            Tail = new Vector(arr[0], arr[1], arr[2]);
            Head = new Vector(arr[3], arr[4], arr[5]);

            vd = Tail - Head;
        }

        public VectorPair(double[,] arr)
        {
            if (arr.GetLength(0) != 3 || arr.GetLength(1) != 2)
                throw new ArgumentException(THREE_TWO_DIMENSIONS, "arr");

            Tail = new Vector(arr[0, 0], arr[0, 1], arr[0, 2]);
            Head = new Vector(arr[1, 0], arr[1, 1], arr[1, 2]);

            vd = Tail - Head;
        }

        #endregion

        #region Accessors

        public Vector Tail { get; private set; }
        public Vector Head { get; private set; }

        /// <summary>
        /// The x axis value of the vector.
        /// </summary>
        public double X
        {
            get { return vd.X; }
        }

        /// <summary>
        /// The y axis value of the vector.
        /// </summary>
        public double Y
        {
            get { return vd.Y; }
        }

        /// <summary>
        /// The z axis value of the vector.
        /// </summary>
        public double Z
        {
            get { return vd.Z; }
        }

        /// <summary>
        /// The magnitude (aka. length or absolute value) of the vector.
        /// </summary>
        public double Magnitude
        {
            get { return vd.Magnitude; }
        }

        /// <summary>
        /// The axis components of the vector as an array.
        /// </summary>
        [XmlIgnore]
        public double[] Array
        {
            get { return vd.Array; }
        }

        /// <summary>
        /// An index accessor mapping index [0]->X, [1]->Y and [2]->Z.
        /// </summary>
        /// <param name="index">The array index referring to a component within the vector (i.e. x, y, z).</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the array argument is not 0, 1 or 2.
        /// </exception>
        public double this[int index]
        {
            get { return vd.Array[index]; }
        }

        /// <summary>
        /// An index accessor mapping index [x]->X, [y]->Y and [z]->Z and an optional h(head) or t(tail) vector index identifier.
        /// </summary>
        /// <param name="index">The array index string referring to a component within the vector, head or tail (i.e. x, y, z).</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the index is not a recognised string.
        /// </exception>
        public double this[string index]
        {
            get
            {
                switch (index)
                {
                    case "x": { return X; }
                    case "y": { return Y; }
                    case "z": { return Z; }
                    case "xh": { return Head.X; }
                    case "yh": { return Head.Y; }
                    case "zh": { return Head.Z; }
                    case "xt": { return Tail.X; }
                    case "yt": { return Tail.Y; }
                    case "zt": { return Tail.Z; }
                    default: throw new ArgumentException("index");
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Addition of two vector pairs.
        /// </summary>
        /// <param name="v1">Vector pair to be added to.</param>
        /// <param name="v2">Vector pair to be added.</param>
        /// <returns>VectorPair representing the sum of two vector pairs.</returns>
        public static VectorPair operator +(VectorPair v1, VectorPair v2)
        {
            return
            (
                new VectorPair
                    (
                        v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                        v1.Head.X + v2.Head.X,
                        v1.Head.Y + v2.Head.Y,
                        v1.Head.Z + v2.Head.Z
                    )
            );
        }

        /// <summary>
        /// Subtraction of two vector pairs.
        /// </summary>
        /// <param name="v1">Vector pair to be subtracted from.</param>
        /// <param name="v2">Vector pair to be subtracted.</param>
        /// <returns>Vector pair representing the difference of two vector pairs.</returns>
        public static VectorPair operator -(VectorPair v1, VectorPair v2)
        {
            return
            (
                new VectorPair
                    (
                        v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                        v1.Head.X - v2.Head.X,
                        v1.Head.Y - v2.Head.Y,
                        v1.Head.Z - v2.Head.Z
                    )
            );
        }

        /// <summary>
        /// Product of a vector pair and a scalar value.
        /// </summary>
        /// <param name="v1">Vector pair to be multiplied.</param>
        /// <param name="s2">Scalar value to be multiplied by.</param>
        /// <returns>Vector pair representing the product of the vector pair and scalar.</returns>
        public static VectorPair operator *(VectorPair v1, double s2)
        {
            return
            (
                new VectorPair
                (
                    v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                    v1.Head.X * s2,
                    v1.Head.Y * s2,
                    v1.Head.Z * s2
                )
            );
        }

        /// <summary>
        /// Product of a scalar value and a vector pair.
        /// </summary>
        /// <param name="s1">Scalar value to be multiplied.</param>
        /// <param name="v2">Vector pair to be multiplied by.</param>
        /// <returns>Vector pair representing the product of the scalar and vector pair.</returns>
        public static VectorPair operator *(double s1, VectorPair v2)
        {
            return v2 * s1;
        }

        /// <summary>
        /// Division of a vector pair and a scalar value.
        /// </summary>
        /// <param name="v1">Vector pair to be divided.</param>
        /// <param name="s2">Scalar value to be divided by.</param>
        /// <returns>Vector pair representing the division of the vector pair and scalar.</returns>
        public static VectorPair operator /(VectorPair v1, double s2)
        {
            return
            (
                new VectorPair
                    (
                        v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                        v1.Head.X / s2,
                        v1.Head.Y / s2,
                        v1.Head.Z / s2
                    )
            );
        }

        /// <summary>
        /// Negation of a vector pair. This inverts the direction of the vector pair.
        /// </summary>
        /// <param name="v1">Vector pair to be negated.</param>
        /// <returns>Negated Vector pair.</returns>
        public static VectorPair operator -(VectorPair v1)
        {
            return
            (
                new VectorPair
                    (
                        v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                        -v1.Head.X,
                        -v1.Head.Y,
                        -v1.Head.Z
                    )
            );
        }

        /// <summary>
        /// Reinforcement of a Vector pair. This makes the vector pair positive (+VectorPair);
        /// </summary>
        /// <param name="v1">Vector pair to be reinforced.</param>
        /// <returns>Reinforced vector pair.</returns>
        public static VectorPair operator +(VectorPair v1)
        {
            return
            (
                new VectorPair
                    (
                        v1.Tail.X, v1.Tail.Y, v1.Tail.Z,
                        +v1.Head.X,
                        +v1.Head.Y,
                        +v1.Head.Z
                    )
            );
        }

        /// <summary>
        /// Compare the magnitude of two vector pairs (less than).
        /// </summary>
        /// <param name="v1">Vector pair to be compared.</param>
        /// <param name="v2">Vector pair to be compared with.</param>
        /// <returns>True if v1 less than v2.</returns>
        public static bool operator <(VectorPair v1, VectorPair v2)
        {
            return v1.SumComponentSqrs() < v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two vector pairs (greater than).
        /// </summary>
        /// <param name="v1">Vector pair to be compared.</param>
        /// <param name="v2">Vector pair to be compared with.</param>
        /// <returns>True if v1 greater than v2.</returns>
        public static bool operator >(VectorPair v1, VectorPair v2)
        {
            return v1.SumComponentSqrs() > v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two vector pairs (less than or equal to).
        /// </summary>
        /// <param name="v1">Vector pair to be compared.</param>
        /// <param name="v2">Vector pair to be compared with.</param>
        /// <returns>True if v1 less than or equal to v2.</returns>
        public static bool operator <=(VectorPair v1, VectorPair v2)
        {
            return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two vector pairs (greater than or equal to).
        /// </summary>
        /// <param name="v1">Vector pair to be compared.</param>
        /// <param name="v2">Vector pair to be compared with.</param>
        /// <returns>True if v1 greater than or equal to v2.</returns>
        public static bool operator >=(VectorPair v1, VectorPair v2)
        {
            return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare two vector pairs for equality.
        /// </summary>
        /// <param name="v1">Vector pair to be compared for equality.</param>
        /// <param name="v2">Vector pair to be compared to.</param>
        /// <returns>Boolean decision (truth for equality).</returns>
        /// <implementation>
        /// Checks the equality of each pair of components, all pairs must be equal.
        /// </implementation>
        public static bool operator ==(VectorPair v1, VectorPair v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        /// <summary>
        /// Negative comparator of two VectorPairs testing if two vector pairs different.
        /// </summary>
        /// <param name="v1">Vector pair to be compared for in-equality.</param>
        /// <param name="v2">Vector pair to be compared to.</param>
        /// <returns>Boolean decision (truth for in-equality).</returns>>
        public static bool operator !=(VectorPair v1, VectorPair v2)
        {
            return !(v1 == v2);
        }

        public static explicit operator Vector(VectorPair v1)
        {
            return new Vector(v1.X, v1.Y, v1.Z);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Determine the cross product (or vector product) of two vector pairs. This is the normal to the vector pair (vector 90° to the plane).
        /// </summary>
        /// <param name="v1">The vector pair to multiply.</param>
        /// <param name="v2">The vector pair to multiply by.</param>
        /// <returns>Vector pair representing the cross product of the two vector pairs.</returns>
        /// <implementation>
        /// Cross products are non commutable.
        /// </implementation>
        public static Vector CrossProduct(VectorPair v1, VectorPair v2)
        {
            return Vector.CrossProduct((Vector) v1, (Vector) v2);
        }

        /// <summary>
        /// Determine the cross product (or vector product) of two vector pairs. This is the normal to the vector pair (vector 90° to the plane).
        /// </summary>
        /// <param name="other">The vector pair to multiply by.</param>
        /// <returns>Vector representing the cross product of the two vector pairs.</returns>
        /// <implementation>
        public Vector CrossProduct(VectorPair other)
        {
            return CrossProduct(this, other);
        }

        /// <summary>
        /// Determine the dot product of two Vectors.
        /// </summary>
        /// <param name="v1">The vector to multiply</param>
        /// <param name="v2">The vector to multiply by</param>
        /// <returns>Scalar representing the dot product of the two vectors</returns>
        /// <implementation>
        /// </implementation>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static double DotProduct(Vector v1, Vector v2)
        {
            return
            (
                v1.X * v2.X +
                v1.Y * v2.Y +
                v1.Z * v2.Z
            );
        }

        /// <summary>
        /// Determine the dot product of this Vector and another
        /// </summary>
        /// <param name="other">The vector to multiply by</param>
        /// <returns>Scalar representing the dot product of the two vectors</returns>
        /// <implementation>
        /// <see cref="DotProduct(Vector)"/>
        /// </implementation>
        public double DotProduct(VectorPair other)
        {
            return DotProduct(this, other);
        }

        /// <summary>
        /// Determine the mixed product of three Vectors
        /// Determine volume (with sign precision) of parallelepiped spanned on given vectors
        /// Determine the scalar triple product of three vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <param name="v3">The third vector</param>
        /// <returns>Scalar representing the mixed product of the three vectors</returns>
        /// <implementation>
        /// Mixed products are non commutable
        /// <see cref="CrossProduct(Vector, Vector)"/>
        /// <see cref="DotProduct(Vector, Vector)"/>
        /// </implementation>
        /// <Acknowledgement>This code was provided by Michał Bryłka</Acknowledgement>
        public static double MixedProduct(Vector v1, Vector v2, Vector v3)
        {
            return DotProduct(CrossProduct(v1, v2), v3);
        }

        /// <summary>
        /// Determine the mixed product of three Vectors
        /// Determine volume (with sign precision) of parallelepiped spanned on given vectors
        /// Determine the scalar triple product of three vectors
        /// </summary>
        /// <param name="other_v1">The second vector</param>
        /// <param name="other_v2">The third vector</param>
        /// <returns>Scalar representing the mixed product of the three vectors</returns>
        /// <implementation>
        /// Mixed products are non commutable
        /// <see cref="MixedProduct(Vector, Vector, Vector)"/>
        /// Uses MixedProduct(Vector, Vector, Vector) to avoid code duplication
        /// </implementation>
        public double MixedProduct(Vector other_v1, Vector other_v2)
        {
            return DotProduct(CrossProduct(this, other_v1), other_v2);
        }

        /// <summary>
        /// Get the normalized vector
        /// Get the unit vector
        /// Scale the Vector so that the magnitude is 1
        /// </summary>
        /// <param name="v1">The vector to be normalized</param>
        /// <returns>The normalized Vector</returns>
        /// <implementation>
        /// Uses the Magnitude function to avoid code duplication 
        /// </implementation>
        /// <exception cref="System.DivideByZeroException">
        /// Thrown when the normalisation of a zero magnitude vector is attempted
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector Normalize(Vector v1)
        {
            // Check for divide by zero errors
            if (v1.Magnitude == 0)
                throw new DivideByZeroException(NORMALIZE_0);

            // find the inverse of the vectors magnitude
            double inverse = 1 / v1.Magnitude;
            return
                (
                    new Vector
                        (
                // multiply each component by the inverse of the magnitude
                        v1.X * inverse,
                        v1.Y * inverse,
                        v1.Z * inverse
                        )
                );
        }

        /// <summary>
        /// Get the normalized vector
        /// Get the unit vector
        /// Scale the Vector so that the magnitude is 1
        /// </summary>
        /// <returns>The normalized Vector</returns>
        /// <implementation>
        /// Uses the Magnitude and Normalize function to avoid code duplication 
        /// </implementation>
        /// <exception cref="System.DivideByZeroException">
        /// Thrown when the normalisation of a zero magnitude vector is attempted
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public Vector Normalize()
        {
            return Normalize(this);
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors or an extrapolated value if allowed
        /// </summary>
        /// <param name="v1">The Vector to interpolate from (where control ==0)</param>
        /// <param name="v2">The Vector to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1), or an extrapolated point if allowed</param>
        /// <param name="allowExtrapolation">True if the control may represent a point not on the vertex between v1 and v2</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors or an extrapolated point on the extended virtex</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1 and extrapolation is not allowed
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector Interpolate(Vector v1, Vector v2, double control, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (control > 1 || control < 0))
                // Error message includes information about the actual value of the argument
                throw new ArgumentOutOfRangeException
                        (
                            "control",
                            control,
                            INTERPOLATION_RANGE + "\n" + ARGUMENT_VALUE + control
                        );

            return
                (
                    new Vector
                        (
                        v1.X * (1 - control) + v2.X * control,
                        v1.Y * (1 - control) + v2.Y * control,
                        v1.Z * (1 - control) + v2.Z * control
                        )
                );
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors
        /// </summary>
        /// <param name="v1">The Vector to interpolate from (where control ==0)</param>
        /// <param name="v2">The Vector to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1)</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector, Vector, double, bool)"/>
        /// Uses the Interpolate(Vector,Vector,double,bool) method to avoid code duplication
        /// </implementation>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector Interpolate(Vector v1, Vector v2, double control)
        {
            return Interpolate(v1, v2, control, false);
        }


        /// <summary>
        /// Take an interpolated value from between two Vectors
        /// </summary>
        /// <param name="other">The Vector to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1)</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector, Vector, double)"/>
        /// Overload for Interpolate method, finds an interpolated value between this Vector and another
        /// Uses the Interpolate(Vector,Vector,double) method to avoid code duplication
        /// </implementation>
        public Vector Interpolate(Vector other, double control)
        {
            return Interpolate(this, other, control);
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors or an extrapolated value if allowed
        /// </summary>
        /// <param name="other">The Vector to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1), or an extrapolated point if allowed</param>
        /// <param name="allowExtrapolation">True if the control may represent a point not on the vertex between v1 and v2</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors or an extrapolated point on the extended virtex</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector, Vector, double, bool)"/>
        /// Uses the Interpolate(Vector,Vector,double,bool) method to avoid code duplication
        /// </implementation>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1 and extrapolation is not allowed
        /// </exception>
        public Vector Interpolate(Vector other, double control, bool allowExtrapolation)
        {
            return Interpolate(this, other, control);
        }

        /// <summary>
        /// FindPlugins the distance between two Vectors
        /// Pythagoras theorem on two Vectors
        /// </summary>
        /// <param name="v1">The Vector to find the distance from </param>
        /// <param name="v2">The Vector to find the distance to </param>
        /// <returns>The distance between two Vectors</returns>
        /// <implementation>
        /// </implementation>
        public static double Distance(Vector v1, Vector v2)
        {
            return
            (
                System.Math.Sqrt
                (
                    (v1.X - v2.X) * (v1.X - v2.X) +
                    (v1.Y - v2.Y) * (v1.Y - v2.Y) +
                    (v1.Z - v2.Z) * (v1.Z - v2.Z)
                )
            );
        }

        /// <summary>
        /// Finds the distance between the heads of two Vectors
        /// Pythagoras theorem on two Vectors
        /// </summary>
        /// <param name="other">The Vector to find the distance to </param>
        /// <returns>The distance between two Vectors</returns>
        /// <implementation>
        /// <see cref="Distance(Vector, Vector)"/>
        /// Overload for Distance method, finds distance between this Vector and another
        /// Uses the Distance(Vector,Vector) method to avoid code duplication
        /// </implementation>
        public double Distance(Vector other)
        {
            return Distance(this, other);
        }

        /// <summary>
        /// Finds the distance between the heads of two Vectors
        /// </summary>
        /// <param name="v1">The Vector to discern the angle from </param>
        /// <param name="v2">The Vector to discern the angle to</param>
        /// <returns>The angle between two positional Vectors</returns>
        /// <implementation>
        /// </implementation>
        /// <Acknowledgement>F.Hill, 2001, Computer Graphics using OpenGL, 2ed </Acknowledgement>
        public static Angle Angle(Vector v1, Vector v2)
        {
            return new Angle(System.Math.Acos(Normalize(v1).DotProduct(Normalize(v2))));
        }

        /// <summary>
        /// FindPlugins the angle between this Vector and another
        /// </summary>
        /// <param name="other">The Vector to discern the angle to</param>
        /// <returns>The angle between two positional Vectors</returns>
        /// <implementation>
        /// <see cref="Angle(Vector, Vector)"/>
        /// Uses the Angle(Vector,Vector) method to avoid code duplication
        /// </implementation>
        public Angle Angle(Vector other)
        {
            return Angle(this, other);
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the greater Vector
        /// </summary>
        /// <param name="v1">The vector to compare</param>
        /// <param name="v2">The vector to compare with</param>
        /// <returns>
        /// The greater of the two Vectors (based on magnitude)
        /// </returns>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector Max(Vector v1, Vector v2)
        {
            if (v1 >= v2) { return v1; }
            return v2;
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the greater Vector
        /// </summary>
        /// <param name="other">The vector to compare with</param>
        /// <returns>
        /// The greater of the two Vectors (based on magnitude)
        /// </returns>
        /// <implementation>
        /// <see cref="Max(Vector, Vector)"/>
        /// Uses function Max(Vector, Vector) to avoid code duplication
        /// </implementation>
        public Vector Max(Vector other)
        {
            return Max(this, other);
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the lesser Vector
        /// </summary>
        /// <param name="v1">The vector to compare</param>
        /// <param name="v2">The vector to compare with</param>
        /// <returns>
        /// The lesser of the two Vectors (based on magnitude)
        /// </returns>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector Min(Vector v1, Vector v2)
        {
            if (v1 <= v2) { return v1; }
            return v2;
        }

        /// <summary>
        /// Compares the magnitude of two Vectors and returns the greater Vector
        /// </summary>
        /// <param name="other">The vector to compare with</param>
        /// <returns>
        /// The lesser of the two Vectors (based on magnitude)
        /// </returns>
        /// <implementation>
        /// <see cref="Min(Vector, Vector)"/>
        /// Uses function Min(Vector, Vector) to avoid code duplication
        /// </implementation>
        public Vector Min(Vector other)
        {
            return Min(this, other);
        }

        /// <summary>
        /// FindPlugins the absolute value of a Vector
        /// FindPlugins the magnitude of a Vector
        /// </summary>
        /// <returns>A Vector representing the absolute values of the vector</returns>
        /// <implementation>
        /// An alternative interface to the magnitude property
        /// </implementation>
        public static Double Abs(Vector v1)
        {
            return v1.Magnitude;
        }

        /// <summary>
        /// FindPlugins the absolute value of a Vector
        /// FindPlugins the magnitude of a Vector
        /// </summary>
        /// <returns>A Vector representing the absolute values of the vector</returns>
        /// <implementation>
        /// An alternative interface to the magnitude property
        /// </implementation>
        public double Abs()
        {
            return Magnitude;
        }

        #endregion

        /*#region Interpolate

        public static Vector Interpolate(LineSegment l1, double control, bool allowExtrapolation)
        {
            return Vector.Interpolate(l1.Tail, l1.Head, control, allowExtrapolation);
        }

        public static Vector Interpolate(LineSegment l1, double control)
        {
            return Interpolate(l1, control, false);
        }

        public Vector Interpolate(double control)
        {
            return Interpolate(this, control);
        }

        public Vector Interpolate(double control, bool allowExtrapolation)
        {
            return Interpolate(this, control);
        }

        #endregion

        #region Rotation

        public static LineSegment Rotate(LineSegment l1, Angle angle, bool x, bool y, bool z)
        {
            if (x) l1 = RotateX(l1, angle);
            if (y) l1 = RotateY(l1, angle);
            if (z) l1 = RotateZ(l1, angle);
            return l1;
        }

        public LineSegment Rotate(Angle angle, bool x, bool y, bool z)
        {
            return Rotate(this, angle, x, y, z);
        }

        public static LineSegment RotateX(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateX(angle));
        }

        public LineSegment RotateX(Angle angle)
        {
            return RotateX(this, angle);
        }

        public static LineSegment RotateY(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateY(angle));
        }

        public LineSegment RotateY(Angle angle)
        {
            return RotateY(this, angle);
        }

        public static LineSegment RotateZ(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateZ(angle));
        }

        public LineSegment RotateZ(Angle angle)
        {
            return RotateZ(this, angle);
        }

        #endregion

        #region Axis based functions

        public static LineSegment Yaw(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Yaw(angle, axis));
        }

        public LineSegment Yaw(Angle angle, Axis axis)
        {
            return Yaw(this, angle, axis);
        }

        public static LineSegment Pitch(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Pitch(angle, axis));
        }

        public LineSegment Pitch(Angle angle, Axis axis)
        {
            return Pitch(this, angle, axis);
        }

        public static LineSegment Roll(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Roll(angle, axis));
        }

        public LineSegment Roll(Angle angle, Axis axis)
        {
            return Roll(this, angle, axis);
        }

        #endregion

        

        #region messages

        private const string SIX_COMPONENTS = "Array must contain exactly six components , (x1,y1,z1)(x2,y2,z2)";
        private const string THREE_TWO_DIMENSIONS = "Array must have dimensions of [3,2]";

        #endregion
    }*/
}