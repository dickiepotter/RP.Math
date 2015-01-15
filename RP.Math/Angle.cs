#region Imports

using System;

#endregion

namespace RPUtil.Math
{
    /// <summary>
    /// Double precision angle
    /// </summary>
    /// <author>Richard Potter BSc(Hons)</author>
    /// <created>Jun-07</created>
    /// <modified>Jun-07</modified>
    /// <version>1.00</version>
    /// <Changes></Changes>
    /// <remarks>
    /// Immutable and always reduces multiple full angles
    /// </remarks>
    [Serializable]
    public struct Angle
        : IComparable, IComparable<Angle>, IEquatable<Angle>, IFormattable
    {
        #region Struct Variables

        /// <summary>
        /// The angle value as a radian
        /// If positive the angle is taken clockwise, negative is counter-clockwise
        /// </summary>
        private readonly double v;

        /// <summary>
        /// Tolerance for comparison operations
        /// </summary>
        private static double t = DefaultTolerence;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the angle
        /// </summary>
        /// <param name="rad">angle in radians</param>
        public Angle(double rad)
        {
            v = ToAngleValue(rad);
        }

        /// <summary>
        /// Constructor for the angle accepting various unit types
        /// </summary>
        /// <param name="value">The angle value in specified units</param>
        /// <param name="units">The unit type being provided</param>
        public Angle(double value, AngleUnits units)
        {
            switch (units)
            {
                case AngleUnits.DEG:
                    v = ToAngleValue(DegToRad(value));
                    break;
                case AngleUnits.RAD:
                    v = ToAngleValue(value);
                    break;
                case AngleUnits.GRAD:
                    v = ToAngleValue(GradToRad(value));
                    break;
                default:
                    // Should have thrown an error
                    v = 0.0;
                    break;

            }
        }

        #endregion

        #region Accessors & Mutators

        /// <summary>
        /// Length to set the comparison operation tolerance for ALL angles
        /// </summary>
        public static double Tolerance
        {
            get { return t; }
            set { t = value; }
        }

        /// <summary>
        /// Length to access the angle value as a double precision radian
        /// </summary>
        public double Rad { get { return v; } }

        /// <summary>
        /// Length to access the angle value as a double precision degree
        /// </summary>
        public double Deg { get { return RadToDeg(v); } }

        /// <summary>
        /// Length to access the angle value as a double precision gradian
        /// </summary>
        public double Grad { get { return RadToGrad(v); } }

        /// <summary>
        /// TraverseLeafs to access the angle value as specified units
        /// </summary>
        /// <param name="units">Units to provide angle value in</param>
        /// <returns>The angle value in specified units</returns>
        public double Value(AngleUnits units) { return units == AngleUnits.DEG ? Deg : Rad; }

        #endregion

        #region Operators

        /// <summary>
        /// Addition of two angles (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="a1">Angle to be added to</param>
        /// <param name="a2">Angle to be added</param>
        /// <returns>The clockwise sum of the two angles as an angle</returns>
        public static Angle operator +(Angle a1, Angle a2)
        {
            return new Angle((+a1).Rad + (+a2).Rad);
        }

        /// <summary>
        /// Addition of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be added to</param>
        /// <param name="a2">Angle to be added</param>
        /// <returns>The clockwise sum of the two angles as an angle</returns>
        public static Angle operator +(double d, Angle a2)
        {
            return (Angle)d + a2;
        }

        /// <summary>
        /// Addition of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be added</param>
        /// <param name="a1">Angle to be added to</param>
        /// <returns>The clockwise sum of the two angles as an angle</returns>
        public static Angle operator +(Angle a1, double d)
        {
            return a1 + (Angle)d;
        }

        /// <summary>
        /// Subtraction of two angles (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="a1">Angle to be subtracted from</param>
        /// <param name="a2">Angle to be subtracted</param>
        /// <returns>The clockwise difference of the two angles as an angle</returns>
        public static Angle operator -(Angle a1, Angle a2)
        {
            return new Angle((+a1).Rad - (+a2).Rad);
        }

        /// <summary>
        /// Subtraction of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be subtracted from</param>
        /// <param name="a2">Angle to be subtracted</param>
        /// <returns>The clockwise difference of the two angles as an angle</returns>
        public static Angle operator -(double d, Angle a2)
        {
            return (Angle)d - a2;
        }

        /// <summary>
        /// Subtraction of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="a1">Angle to be subtracted from</param>
        /// <param name="d">Radian to be subtracted</param>
        /// <returns>The clockwise difference of the two angles as an angle</returns>
        public static Angle operator -(Angle a1, double d)
        {
            return a1 - (Angle)d;
        }

        /// <summary>
        /// Product of two angles (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="a1">Angle to be multiplied</param>
        /// <param name="a2">Angle to be multiplied by</param>
        /// <returns>The clockwise product of the two angles as an angle</returns>
        public static Angle operator *(Angle a1, Angle a2)
        {
            return new Angle((+a1).Rad * (+a2).Rad);
        }

        /// <summary>
        /// Product of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be multiplied</param>
        /// <param name="a2">Angle to be multiplied by</param>
        /// <returns>The clockwise product of the two angles as an angle</returns>
        public static Angle operator *(double d, Angle a2)
        {
            return (Angle)d * a2;
        }

        /// <summary>
        /// Product of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be multiplied by</param>
        /// <param name="a1">Angle to be multiplied</param>
        /// <returns>The clockwise product of the two angles as an angle</returns>
        public static Angle operator *(Angle a1, double d)
        {
            return a1 * (Angle)d;
        }

        /// <summary>
        /// Division of two angles (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="a1">Angle to be divided</param>
        /// <param name="a2">Angle to divide by</param>
        /// <returns>The clockwise division of the two angles as an angle</returns>
        public static Angle operator /(Angle a1, Angle a2)
        {
            return new Angle((+a1).Rad / (+a2).Rad);
        }

        /// <summary>
        /// Division of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be divided</param>
        /// <param name="a2">Angle to be divided by</param>
        /// <returns>The clockwise division of the two angles as an angle</returns>
        public static Angle operator /(double d, Angle a2)
        {
            return (Angle)d / a2;
        }

        /// <summary>
        /// Division of a double precision radian and an angle (clockwise)
        /// </summary>
        /// <remarks>
        /// Even if the angles are specified as counter-clockwise 
        /// this operation uses and returns clockwise angles.
        /// </remarks>
        /// <param name="d">Radian to be divided by</param>
        /// <param name="a1">Angle to be divided</param>
        /// <returns>The clockwise division of the two angles as an angle</returns>
        public static Angle operator /(Angle a1, double d)
        {
            return a1 / (Angle)d;
        }

        /// <summary>
        /// Negation of an angle producing the counter-clockwise angle as a negative value
        /// e.g. 90 degree angle ==> -270 degree counter-clockwise 
        /// </summary>
        /// <param name="a1">Angle to be negated</param>
        /// <returns>Counter-clockwise angle as a negative value</returns>
        public static Angle operator -(Angle a1)
        {
            return
                a1.IsClockwise() ?
                    new Angle(-(Full_Angle_Rad - a1.Rad)) :
                    a1;
        }

        /// <summary>
        /// Reinforcement of an angle producing the clockwise angle as a positive angle
        /// e.g. -90 degree angle ==> 270 degree clockwise
        /// </summary>
        /// <param name="a1">Angle to be reinforced</param>
        /// <returns>Clockwise angle</returns>
        public static Angle operator +(Angle a1)
        {
            return
                a1.IsClockwise() ?
                    a1 :
                    new Angle(Full_Angle_Rad + a1.Rad);
        }

        /// <summary>
        /// Inverse of an angle producing the clockwise angle for a counter-clockwise angle and vice-versa.
        /// Toggles negation
        /// </summary>
        /// <param name="a1">Angle to be inverted</param>
        /// <returns>Inverted angle</returns>
        public static Angle operator !(Angle a1)
        {
            return a1.IsClockwise() ? -a1 : +a1;
        }

        /// <summary>
        /// Compare the clockwise values of two angles (less than)
        /// </summary>
        /// <param name="a1">Angle to be compared </param>
        /// <param name="a2">Angle to be compared with</param>
        /// <returns>True if a1 less than a2</returns>
        public static bool operator <(Angle a1, Angle a2)
        {
            return (+a1).Rad < (+a2).Rad;
        }

        /// <summary>
        /// Compare the clockwise values of two angles (greater than)
        /// </summary>
        /// <param name="a1">Angle to be compared </param>
        /// <param name="a2">Angle to be compared with</param>
        /// <returns>True if a1 is more than a2</returns>
        public static bool operator >(Angle a1, Angle a2)
        {
            return (+a1).Rad > (+a2).Rad;
        }

        /// <summary>
        /// Compare the clockwise values of two angles (less than or equal to)
        /// </summary>
        /// <param name="a1">Angle to be compared </param>
        /// <param name="a2">Angle to be compared with</param>
        /// <returns>True if a1 is less than or equal to a2</returns>
        public static bool operator <=(Angle a1, Angle a2)
        {
            return (+a1).Rad <= (+a2).Rad;
        }

        /// <summary>
        /// Compare the clockwise values of two angles (greater than or equal to)
        /// </summary>
        /// <param name="a1">Angle to be compared </param>
        /// <param name="a2">Angle to be compared with</param>
        /// <returns>True if a1 is greater than or equal to a2</returns>
        public static bool operator >=(Angle a1, Angle a2)
        {
            return (+a1).Rad >= (+a2).Rad;
        }

        /// <summary>
        /// Compare for congruent angles.
        /// Compare clockwise value of two angles for equality.
        /// </summary>
        /// <param name="a1">Angle to be compared for equality</param>
        /// <param name="a2">Angle to be compared to</param>
        /// <returns>Boolean decision (truth for equality)</returns>
        /// <implementation>
        /// A tolerence to the equality operator is applied
        /// </implementation>
        public static bool operator ==(Angle a1, Angle a2)
        {
            return System.Math.Abs((+a1).Rad - (+a2).Rad) <= Tolerance;
        }

        /// <summary>
        /// Compare for congruent angles.
        /// Compare clockwise value of a double precision radian and an angle for equality.
        /// </summary>
        /// <param name="d">Radian to be compared for equality</param>
        /// <param name="a2">Angle to be compared to</param>
        /// <returns>Boolean decision (truth for equality)</returns>
        /// <implementation>
        /// A tolerence to the equality operator is applied
        /// </implementation>
        public static bool operator ==(double d, Angle a2)
        {
            return a2 == (Angle)d;
        }

        /// <summary>
        /// Compare for congruent angles.
        /// Compare clockwise value of a double precision radian and an angle for equality.
        /// </summary>
        /// <param name="a1">Angle to be compared for equality</param>
        /// <param name="d">Radian to be compared to</param>
        /// <returns>Boolean decision (truth for equality)</returns>
        /// <implementation>
        /// A tolerence to the equality operator is applied
        /// </implementation>
        public static bool operator ==(Angle a1, double d)
        {
            return a1 == (Angle)d;
        }

        /// <summary>
        /// Compare clockwise value of two angles for inequality.
        /// </summary>
        /// <param name="a1">Angle to be compared for inequality</param>
        /// <param name="a2">Angle to be compared to</param>
        /// <returns>Boolean decision (truth for inequality)</returns>
        /// <implementation>
        /// A tolerence to the inequality operator is applied
        /// </implementation>
        public static bool operator !=(Angle a1, Angle a2)
        {
            return !(a1 == a2);
        }

        /// <summary>
        /// Compare clockwise value of two angles for inequality.
        /// </summary>
        /// <param name="d">Radian to be compared for inequality</param>
        /// <param name="a2">Angle to be compared to</param>
        /// <returns>Boolean decision (truth for inequality)</returns>
        /// <implementation>
        /// A tolerence to the inequality operator is applied
        /// </implementation>
        public static bool operator !=(double d, Angle a2)
        {
            return !(d == a2);
        }

        /// <summary>
        /// Compare clockwise value of two angles for inequality.
        /// </summary>
        /// <param name="a1">Angle to be compared for inequality</param>
        /// <param name="d">Radian to be compared to</param>
        /// <returns>Boolean decision (truth for inequality)</returns>
        /// <implementation>
        /// A tolerence to the inequality operator is applied
        /// </implementation>
        public static bool operator !=(Angle a1, double d)
        {
            return !(a1 == d);
        }

        #endregion

        #region Conversions

        /// <summary>
        /// Convert double precision degree value to radians
        /// </summary>
        /// <param name="deg">Degrees value to convert</param>
        /// <returns>Converted radians</returns>
        public static double DegToRad(double deg)
        {
            return DEG_TO_RAD_CONVERSION_FACTOR * deg;
        }

        /// <summary>
        /// Convert double precision radian value to degrees
        /// </summary>
        /// <param name="rad">Radian value to convert</param>
        /// <returns>Converted degrees</returns>
        public static double RadToDeg(double rad)
        {
            return RAD_TO_DEG_CONVERSION_FACTOR * rad;
        }

        /// <summary>
        /// Convert double precision gradian value to radians
        /// </summary>
        /// <param name="grad">gradian value to convert</param>
        /// <returns>Converted radians</returns>
        public static double GradToRad(double grad)
        {
            return GRAD_TO_RAD_CONVERSION_FACTOR * grad;
        }

        /// <summary>
        /// Convert double precision radian value to gradian
        /// </summary>
        /// <param name="rad">Radian value to convert</param>
        /// <returns>Converted gradian</returns>
        public static double RadToGrad(double rad)
        {
            return RAD_TO_GRAD_CONVERSION_FACTOR * rad;
        }

        /// <summary>
        /// Reduces radian angles greater than a full circle to a valid angle
        /// e.g. 3*PI (540 degree) angle ==> PI (180 degree)
        /// </summary>
        /// <param name="rad">To convert</param>
        /// <returns>Double precision radian value reduced to a valid angle</returns>
        public static double ToAngleValue(double rad)
        {
            return
                rad > Full_Angle_Rad ?
                System.Math.IEEERemainder(rad, Full_Angle_Rad) :
                rad;
        }

        /// <summary>
        /// Implicit convertor from double precision radian to angle
        /// </summary>
        /// <param name="r">double precision radian angle</param>
        /// <returns>Converted Angle structure</returns>
        public static implicit operator Angle(double r)
        {
            return new Angle(r);
        }

        /// <summary>
        /// Implicit convertor from angle to double precision radian
        /// </summary>
        /// <param name="a">Angle to convert to double precision radians</param>
        /// <returns>angle value as a radian</returns>
        public static implicit operator double(Angle a)
        {
            return a.Rad;
        }

        /// <summary>
        /// Implicit convertor from float radian to angle
        /// </summary>
        /// <param name="r">float radian to convert from</param>
        /// <returns>Converted Angle structure</returns>
        public static implicit operator Angle(float r)
        {
            return new Angle(r);
        }

        /// <summary>
        /// Explicit convertor from angle to floating point precision radian
        /// </summary>
        /// <param name="a">Angle to convert to radians</param>
        /// <returns>angle value as a radian</returns>
        /// <remarks>Possible loss of precision</remarks>
        public static explicit operator float(Angle a)
        {
            return (float)a.Rad;
        }

        /// <summary>
        /// Implicit convertor from integer radian to angle
        /// </summary>
        /// <param name="r">int radian to convert from</param>
        /// <returns>Converted Angle structure</returns>
        public static implicit operator Angle(int r)
        {
            return new Angle(r);
        }

        /// <summary>
        /// Explicit convertor from angle to integer precision radian
        /// </summary>
        /// <param name="a">Angle to convert to radians</param>
        /// <returns>angle value as a radian</returns>
        /// <remarks>Possible loss of precision</remarks>
        public static explicit operator int(Angle a)
        {
            return (int)a.Rad;
        }

        #endregion

        #region Functions

        public static Angle Clockwise(Angle a1) { return +a1; }

        public Angle Clockwise() { return +this; }

        public static Angle CounterClockwise(Angle a1) { return -a1; }

        public Angle CounterClockwise() { return -this; }

        public static Angle SmallAngle(Angle a1) { return a1.IsReflex() ? (!a1) : a1; }

        public Angle SmallAngle() { return SmallAngle(this); }

        public static Angle Reflex(Angle a1) { return a1.IsReflex() ? a1 : (!a1); }

        public Angle Reflex() { return Reflex(this); }

        public static Angle Abs(Angle a1) { return new Angle(System.Math.Abs(a1.Rad)); }

        public Angle Abs() { return Abs(this); }

        public static Angle Complement(Angle a1)
        {
            if (!a1.IsAcute()) throw new InvalidOperationException(NOT_ACUTE);
            return new Angle(Right_Angle_Rad - a1.Rad);
        }

        public Angle Complement() { return Complement(this); }

        public static Angle Supplement(Angle a1)
        {
            if (a1.IsAcute() || a1.IsObtuse()) return new Angle(Strait_Angle_Rad - a1.Rad);
            throw new InvalidOperationException(NOT_ACUTE_OR_OBTUSE);
        }

        public Angle Supplement() { return Supplement(this); }

        #endregion

        #region Sin, cos, Tan

        public static double Sin(Angle a1) { return System.Math.Sin(a1.Rad); }
        public static double Sinh(Angle a1) { return System.Math.Sinh(a1.Rad); }
        public static Angle Asin(double sin) { return new Angle(System.Math.Asin(sin)); }
        public double Sin() { return Sin(this); }
        public double Sinh() { return Sinh(this); }

        public static double Cos(Angle a1) { return System.Math.Cos(a1.Rad); }
        public static double Cosh(Angle a1) { return System.Math.Cosh(a1.Rad); }
        public static Angle Acos(double cos) { return new Angle(System.Math.Acos(cos)); }
        public double Cos() { return Cos(this); }
        public double Cosh() { return Cosh(this); }

        public static double Tan(Angle a1) { return System.Math.Tan(a1.Rad); }
        public static double Tanh(Angle a1) { return System.Math.Tanh(a1.Rad); }
        public static Angle Atan(double tan) { return new Angle(System.Math.Atan(tan)); }
        public double Tan() { return Tan(this); }
        public double Tanh() { return Tanh(this); }

        #endregion

        #region Decisions

        public static bool IsClockwise(Angle a1) { return a1.Rad < 0 ? false : true; }
        public bool IsClockwise() { return IsClockwise(this); }

        public static bool IsReflex(Angle a1) { return a1.Abs().Rad > Strait_Angle_Rad ? true : false; }
        public bool IsReflex() { return IsReflex(this); }

        public static bool IsAcute(Angle a1) { return a1.Abs().Rad < Right_Angle_Rad ? true : false; }
        public bool IsAcute() { return IsAcute(this); }

        public static bool IsObtuse(Angle a1) { return a1.Abs().Rad > Right_Angle_Rad && (!a1.IsReflex()) ? true : false; }
        public bool IsObtuse() { return IsObtuse(this); }

        public static bool IsStraitAngle(Angle a1) { return System.Math.Abs(a1.Abs().Rad - Strait_Angle_Rad) <= Tolerance; }
        public bool IsStraitAngle() { return IsStraitAngle(this); }

        public static bool IsRightAngle(Angle a1) { return System.Math.Abs(a1.Abs().Rad - Right_Angle_Rad) <= Tolerance; }
        public bool IsRightAngle() { return IsRightAngle(this); }

        public static bool IsFullOrZeroAngle(Angle a1)
        {
            return
                System.Math.Abs(a1.Abs().Rad - Full_Angle_Rad) <= Tolerance ||
                System.Math.Abs(a1.Rad) <= Tolerance;
        }
        public bool IsFullOrZeroAngle() { return IsFullOrZeroAngle(this); }


        public static bool IsOblique(Angle a1)
        {
            if
            (
                System.Math.Abs(a1.Abs().Rad - Full_Angle_Rad) <= Tolerance ||
                System.Math.Abs(a1.Abs().Rad - Right_Angle_Rad) <= Tolerance ||
                System.Math.Abs(a1.Abs().Rad - Strait_Angle_Rad) <= Tolerance ||
                System.Math.Abs(a1.Abs().Rad - Three_Quater_Circle_Rad) <= Tolerance ||
                System.Math.Abs(a1.Rad) <= Tolerance
            )
                return true;
            return false;
        }
        public bool IsOblique() { return IsOblique(this); }

        public static bool IsComplementOf(Angle a1, Angle a2) { return (a1 + a2).IsRightAngle(); }
        public bool IsComplementOf(Angle other) { return IsComplementOf(this, other); } //90

        public static bool IsSupplementOf(Angle a1, Angle a2) { return (a1 + a2).IsStraitAngle(); }
        public bool IsSupplementOf(Angle other) { return IsSupplementOf(this, other); } //180

        public static bool IsExplementOf(Angle a1, Angle a2) { return (a1 + a2).IsFullOrZeroAngle(); }
        public bool IsExplementOf(Angle other) { return IsExplementOf(this, other); } //360

        #endregion

        #region Standard Functions

        /// <summary>
        /// Textual description of the Angle
        /// </summary>
        /// <Implementation>
        /// Uses ToString(string, IFormatProvider) to avoid code duplication
        /// </Implementation>
        /// <returns>Text (String) representing the angle</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Textual description of the Anlge
        /// </summary>
        /// <param name="format">
        /// Formatting string: 
        /// 'd' (degrees),
        /// 'g' (gradians),
        /// 'r' (radians),
        /// '', or 
        /// 'v' (verbose) 
        /// followed by standard numeric format string characters valid for a double precision floating point</param>
        /// <param name="formatProvider">The culture specific fromatting provider</param>
        /// <returns>Text (String) representing the vector</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            // If no format is passed
            if (string.IsNullOrEmpty(format)) return v.ToString();

            char firstChar = Char.ToLower(format[0]);
            string remainder = null;

            if (format.Length > 1)
                remainder = format.Substring(1);

            switch (firstChar)
            {
                case 'd': return Deg.ToString(remainder, formatProvider);
                case 'r': return Rad.ToString(remainder, formatProvider);
                case 'g': return Grad.ToString(remainder, formatProvider);
                case 'v':
                    return
                        string.Format
                        (
                            "{0} {1} angle of {2} radians ( {3} degrees, {4} gradians )",
                            IsClockwise() ? "Clockwise" : "Counter-clockwise",
                            IsAcute() ?
                            "acute" : IsRightAngle() ?
                            "right" : IsObtuse() ?
                            "obtuse" : IsStraitAngle() ?
                            "strait" : IsReflex() ?
                            "reflex" : IsFullOrZeroAngle() ?
                            "full (or zero)" : "Unknown",
                            Rad.ToString(remainder, formatProvider),
                            Deg.ToString(remainder, formatProvider),
                            Grad.ToString(remainder, formatProvider)
                        );
                default:
                    return v.ToString(format, formatProvider);
            }
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
            return v.GetHashCode() % Int32.MaxValue;
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other object (which should be an Angle) to compare to</param>
        /// <returns>Truth if two angles are equal within a tolerence</returns>
        public override bool Equals(object other)
        {
            if (other is Angle)
            {
                Angle otherAngle = (Angle)other;
                return otherAngle == this;
            }
            return false;
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other Angle to compare to</param>
        /// <returns>Truth if two clockwise angles are equal within a tolerence</returns>
        public bool Equals(Angle other)
        {
            return other == this;
        }

        /// <summary>
        /// Compares the clockwise value of two angles
        /// </summary>
        /// <param name="other">The angle to compare this instance with</param>
        /// <returns>
        /// -1: This instance's angle is less than the others angle
        /// 0: This instance's angle is equal to the others angle
        /// 1: This instance's angle is greater than the others angle
        /// </returns>
        /// <implementation>
        /// Implemented to fulfil the IComparable interface
        /// </implementation>
        public int CompareTo(Angle other) { return this < other ? -1 : this > other ? 1 : 0; }


        /// <summary>
        /// Compares the clockwise value of two angles
        /// </summary>
        /// <param name="other">The angle to compare this instance with</param>
        /// <returns>
        /// -1: This instance's angle is less than the others angle
        /// 0: This instance's angle is equal to the others angle
        /// 1: This instance's angle is greater than the others angle
        /// </returns>
        /// <implementation>
        /// Implemented to fulfil the IComparable interface
        /// </implementation>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the type of object to be compared is not known to this class
        /// </exception>
        public int CompareTo(object other)
        {
            if (other is Angle) return CompareTo((Angle)other);

            // Error condition: other is not a angle object
            throw new ArgumentException
                (
                // Error message includes information about the actual type of the argument
                NON_ANGLE_COMPARISON + "\n" + ARGUMENT_TYPE + other.GetType(),
                "other"
                );
        }

        #endregion

        #region Identities

        public const double Right_Angle_Rad = PI / 2;
        public static readonly Angle Right_Angle = new Angle(Right_Angle_Rad);

        public const double Full_Angle_Rad = 2 * PI;
        public static readonly Angle Full_Angle = new Angle(Full_Angle_Rad);

        public const double Strait_Angle_Rad = PI;
        public static readonly Angle Strait_Angle = new Angle(Strait_Angle_Rad);

        public const double Three_Quater_Circle_Rad = (3 * PI) / 2;
        public static readonly Angle Three_Quater_Circle = new Angle(Three_Quater_Circle_Rad);

        public static readonly Angle Zero_Angle = new Angle(0.0);

        #endregion

        #region Constants


        public const double PI = System.Math.PI;

        private const double DEG_TO_RAD_CONVERSION_FACTOR = PI / 180;
        private const double RAD_TO_DEG_CONVERSION_FACTOR = 180 / PI;
        private const double GRAD_TO_RAD_CONVERSION_FACTOR = PI / 200;
        private const double RAD_TO_GRAD_CONVERSION_FACTOR = 200 / PI;
        /// <summary>
        /// The default tolerence used when comparing two a 
        /// </summary>
        public const double DefaultTolerence = Double.Epsilon;

        #endregion

        #region messages

        private const string NOT_ACUTE = "This operation can only be performed on acute angles";
        private const string NOT_ACUTE_OR_OBTUSE = "This operation can only be performed on acute and obtuse angles (not reflex angles)";

        /// <summary>
        /// Exception message descriptive text 
        /// Used when attempting to compare an angle to an object which is not a type of Angle 
        /// </summary>
        private const string NON_ANGLE_COMPARISON = "Cannot compare an Angle type to a non-angle";

        /// <summary>
        /// Exception message additional information text 
        /// Used when adding type information of the given argument into an error message 
        /// </summary>
        private const string ARGUMENT_TYPE = "The argument provided is a type of ";

        #endregion
    }

    public enum AngleUnits
    {
        DEGREE = 1,
        RADIAN = 0,
        GRADIENT = 2,
        DEG = 1,
        RAD = 0,
        GRAD = 2
    }
}