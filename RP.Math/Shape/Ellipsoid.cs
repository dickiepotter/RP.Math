namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) ellipsoid, described by its three semi-axes <see cref="SemiAxisX"/>,
    /// <see cref="SemiAxisY"/> and <see cref="SemiAxisZ"/> (the half-widths along its three axes).
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape with no position or orientation; a sphere is the special
    /// case where all three semi-axes are equal. To place it in space, pair it with a position; see
    /// <see cref="PlacedEllipsoid"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Ellipsoid : IEquatable<Ellipsoid>, IComparable<Ellipsoid>, IFormattable
    {
        #region Fields

        private readonly double semiAxisX;
        private readonly double semiAxisY;
        private readonly double semiAxisZ;

        #endregion

        #region Constructors

        /// <summary>Construct an ellipsoid from its three semi-axes.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any semi-axis is negative.</exception>
        public Ellipsoid(double semiAxisX, double semiAxisY, double semiAxisZ)
        {
            if (semiAxisX < 0) throw new ArgumentOutOfRangeException(nameof(semiAxisX), semiAxisX, NEGATIVE_AXIS);
            if (semiAxisY < 0) throw new ArgumentOutOfRangeException(nameof(semiAxisY), semiAxisY, NEGATIVE_AXIS);
            if (semiAxisZ < 0) throw new ArgumentOutOfRangeException(nameof(semiAxisZ), semiAxisZ, NEGATIVE_AXIS);

            this.semiAxisX = semiAxisX;
            this.semiAxisY = semiAxisY;
            this.semiAxisZ = semiAxisZ;
        }

        #endregion

        #region Accessors

        /// <summary>The semi-axis along the local X axis.</summary>
        public double SemiAxisX { get { return this.semiAxisX; } }

        /// <summary>The semi-axis along the local Y axis.</summary>
        public double SemiAxisY { get { return this.semiAxisY; } }

        /// <summary>The semi-axis along the local Z axis.</summary>
        public double SemiAxisZ { get { return this.semiAxisZ; } }

        /// <summary>The three semi-axes packed as a vector.</summary>
        public Vector SemiAxes { get { return new Vector(this.semiAxisX, this.semiAxisY, this.semiAxisZ); } }

        /// <summary>The enclosed volume, (4/3)·π·a·b·c.</summary>
        public double Volume { get { return (4.0 / 3.0) * Math.PI * this.semiAxisX * this.semiAxisY * this.semiAxisZ; } }

        /// <summary>
        /// The surface area, by Knud Thomsen's approximation.
        /// </summary>
        /// <remarks>An ellipsoid has no elementary exact surface area; this uses the Thomsen approximation
        /// <c>4π·((aᵖbᵖ + aᵖcᵖ + bᵖcᵖ) / 3)^(1/p)</c> with <c>p ≈ 1.6075</c> (accurate to about 1%).</remarks>
        public double SurfaceArea
        {
            get
            {
                const double p = 1.6075;
                double a = Math.Pow(this.semiAxisX, p);
                double b = Math.Pow(this.semiAxisY, p);
                double c = Math.Pow(this.semiAxisZ, p);
                return 4.0 * Math.PI * Math.Pow(((a * b) + (a * c) + (b * c)) / 3.0, 1.0 / p);
            }
        }

        #endregion

        #region Classification

        /// <summary>Whether all three semi-axes are equal within <paramref name="tolerance"/> (a sphere).</summary>
        public bool IsSphere(double tolerance)
        {
            return this.semiAxisX.AlmostEqualsWithAbsTolerance(this.semiAxisY, tolerance)
                && this.semiAxisY.AlmostEqualsWithAbsTolerance(this.semiAxisZ, tolerance);
        }

        #endregion

        #region Comparison (by volume)

        /// <summary>Compare with another ellipsoid by <see cref="Volume"/>.</summary>
        public int CompareTo(Ellipsoid other) { return this.Volume.CompareTo(other.Volume); }

        /// <summary>Whether the left ellipsoid's volume is less than the right's.</summary>
        public static bool operator <(Ellipsoid e1, Ellipsoid e2) { return e1.Volume < e2.Volume; }

        /// <summary>Whether the left ellipsoid's volume is greater than the right's.</summary>
        public static bool operator >(Ellipsoid e1, Ellipsoid e2) { return e1.Volume > e2.Volume; }

        /// <summary>Whether the left ellipsoid's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Ellipsoid e1, Ellipsoid e2) { return e1.Volume <= e2.Volume; }

        /// <summary>Whether the left ellipsoid's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Ellipsoid e1, Ellipsoid e2) { return e1.Volume >= e2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of all three semi-axes.</summary>
        public static bool operator ==(Ellipsoid e1, Ellipsoid e2)
        {
            return e1.SemiAxisX == e2.SemiAxisX && e1.SemiAxisY == e2.SemiAxisY && e1.SemiAxisZ == e2.SemiAxisZ;
        }

        /// <summary>Inequality of any semi-axis.</summary>
        public static bool operator !=(Ellipsoid e1, Ellipsoid e2)
        {
            return !(e1 == e2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Ellipsoid e && this.Equals(e);
        }

        /// <summary>Equality with another ellipsoid (all three semi-axes).</summary>
        public bool Equals(Ellipsoid other)
        {
            return this == other;
        }

        /// <summary>Equality with another ellipsoid within an absolute tolerance.</summary>
        public bool Equals(Ellipsoid other, double tolerance)
        {
            return this.semiAxisX.AlmostEqualsWithAbsTolerance(other.SemiAxisX, tolerance)
                && this.semiAxisY.AlmostEqualsWithAbsTolerance(other.SemiAxisY, tolerance)
                && this.semiAxisZ.AlmostEqualsWithAbsTolerance(other.SemiAxisZ, tolerance);
        }

        /// <summary>A hash code derived from the three semi-axes.</summary>
        public override int GetHashCode()
        {
            return this.semiAxisX.GetHashCode() ^ this.semiAxisY.GetHashCode() ^ this.semiAxisZ.GetHashCode();
        }

        /// <summary>Deconstruct into the three semi-axes.</summary>
        public void Deconstruct(out double semiAxisX, out double semiAxisY, out double semiAxisZ)
        {
            semiAxisX = this.semiAxisX;
            semiAxisY = this.semiAxisY;
            semiAxisZ = this.semiAxisZ;
        }

        /// <summary>A string of the form <c>(a, b, c)</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>(a, b, c)</c> where the semi-axes use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "({0}, {1}, {2})",
                this.semiAxisX.ToString(format, formatProvider),
                this.semiAxisY.ToString(format, formatProvider),
                this.semiAxisZ.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_AXIS = "An ellipsoid's semi-axes cannot be negative.";

        #endregion
    }
}
