namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) rectangle, described purely by its <see cref="Width"/> and
    /// <see cref="Height"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape — size and proportions — with no position or orientation.
    /// Its area, perimeter, diagonal and aspect ratio depend solely on the two lengths, so rectangles can
    /// be compared by size without any notion of where they sit. To place one on a plane in space, pair
    /// it with a position; see <see cref="PlacedRectangle"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Rectangle : IEquatable<Rectangle>, IComparable<Rectangle>, IFormattable
    {
        #region Fields

        private readonly double width;
        private readonly double height;

        #endregion

        #region Constructors

        /// <summary>Construct a rectangle from its width and height.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either size is negative.</exception>
        public Rectangle(double width, double height)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width), width, NEGATIVE_SIZE);
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), height, NEGATIVE_SIZE);

            this.width = width;
            this.height = height;
        }

        #endregion

        #region Factories

        /// <summary>A square (equal width and height) of the given <paramref name="side"/>.</summary>
        public static Rectangle Square(double side)
        {
            return new Rectangle(side, side);
        }

        #endregion

        #region Accessors

        /// <summary>The width.</summary>
        public double Width { get { return this.width; } }

        /// <summary>The height.</summary>
        public double Height { get { return this.height; } }

        /// <summary>The enclosed area, width × height.</summary>
        public double Area { get { return this.width * this.height; } }

        /// <summary>The perimeter, 2·(width + height).</summary>
        public double Perimeter { get { return 2.0 * (this.width + this.height); } }

        /// <summary>The length of the diagonal, <c>√(width² + height²)</c> (Pythagoras).</summary>
        public double DiagonalLength { get { return Math.Sqrt((this.width * this.width) + (this.height * this.height)); } }

        /// <summary>The aspect ratio, width ÷ height (not-a-number for a zero height).</summary>
        public double AspectRatio { get { return this.width / this.height; } }

        #endregion

        #region Classification

        /// <summary>Whether the width and height are equal within <paramref name="tolerance"/> (a square).</summary>
        public bool IsSquare(double tolerance)
        {
            return this.width.AlmostEqualsWithAbsTolerance(this.height, tolerance);
        }

        #endregion

        #region Comparison (by area)

        /// <summary>
        /// Compare with another rectangle by <see cref="Area"/> — the natural "which is bigger" ordering,
        /// independent of proportions. Mirrors how <see cref="Vector"/> compares by magnitude.
        /// </summary>
        public int CompareTo(Rectangle other)
        {
            return this.Area.CompareTo(other.Area);
        }

        /// <summary>Whether the left rectangle's area is less than the right's.</summary>
        public static bool operator <(Rectangle r1, Rectangle r2) { return r1.Area < r2.Area; }

        /// <summary>Whether the left rectangle's area is greater than the right's.</summary>
        public static bool operator >(Rectangle r1, Rectangle r2) { return r1.Area > r2.Area; }

        /// <summary>Whether the left rectangle's area is less than or equal to the right's.</summary>
        public static bool operator <=(Rectangle r1, Rectangle r2) { return r1.Area <= r2.Area; }

        /// <summary>Whether the left rectangle's area is greater than or equal to the right's.</summary>
        public static bool operator >=(Rectangle r1, Rectangle r2) { return r1.Area >= r2.Area; }

        #endregion

        #region Operators

        /// <summary>Equality of width and height.</summary>
        public static bool operator ==(Rectangle r1, Rectangle r2)
        {
            return r1.Width == r2.Width && r1.Height == r2.Height;
        }

        /// <summary>Inequality of width or height.</summary>
        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !(r1 == r2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Rectangle r && this.Equals(r);
        }

        /// <summary>Equality with another rectangle (width and height).</summary>
        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        /// <summary>Equality with another rectangle within an absolute tolerance.</summary>
        public bool Equals(Rectangle other, double tolerance)
        {
            return this.width.AlmostEqualsWithAbsTolerance(other.Width, tolerance)
                && this.height.AlmostEqualsWithAbsTolerance(other.Height, tolerance);
        }

        /// <summary>A hash code derived from the width and height.</summary>
        public override int GetHashCode()
        {
            return this.width.GetHashCode() ^ this.height.GetHashCode();
        }

        /// <summary>Deconstruct into width and height, enabling <c>var (w, h) = rectangle;</c>.</summary>
        public void Deconstruct(out double width, out double height)
        {
            width = this.width;
            height = this.height;
        }

        /// <summary>A string of the form <c>w×h</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>w×h</c> where the sizes use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "{0}×{1}",
                this.width.ToString(format, formatProvider),
                this.height.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_SIZE = "A rectangle's width and height cannot be negative.";

        #endregion
    }
}
